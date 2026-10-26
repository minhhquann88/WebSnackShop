using SnackShop.Domain.Common;

namespace SnackShop.Domain.Catalog;

/// <summary>
/// A snack sold by the shop. Aggregate root owning its variants (price, SKU) and images.
/// Invariants: at least one variant; an active product has at least one active variant; at most one primary image.
/// </summary>
public sealed class Product : AggregateRoot, IAuditable, ISoftDeletable
{
    public const int MaxNameLength = 200;
    public const int MaxShortDescriptionLength = 300;
    public const int MaxDescriptionLength = 5000;
    public const int MaxInfoFieldLength = 500;
    public const int MaxImages = 10;
    public const int MaxVariants = 20;

    private readonly List<ProductVariant> _variants = [];
    private readonly List<ProductImage> _images = [];

    private Product()
    {
    }

    private Product(Guid id, Guid categoryId, ProductDetails details, Slug slug)
        : base(id)
    {
        CategoryId = categoryId;
        Slug = slug;
        ApplyDetails(details);
        IsActive = true;
    }

    public Guid CategoryId { get; private set; }

    public string Name { get; private set; } = default!;

    public Slug Slug { get; private set; } = default!;

    public string? ShortDescription { get; private set; }

    public string? Description { get; private set; }

    public string? Ingredients { get; private set; }

    public string? Origin { get; private set; }

    public string? Brand { get; private set; }

    public int? ShelfLifeDays { get; private set; }

    /// <summary>Lowercase, diacritic-free name + brand for accent-insensitive search.</summary>
    public string SearchText { get; private set; } = default!;

    public bool IsActive { get; private set; }

    public bool IsFeatured { get; private set; }

    /// <summary>Units delivered to customers. Increased when an order is delivered; used for "best selling" sort.</summary>
    public int SoldCount { get; private set; }

    public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();

    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>Creates a product with its first variant (every product needs one).</summary>
    public static Result<Product> Create(Guid categoryId, ProductDetails details, Slug slug, VariantSpec firstVariant)
    {
        ArgumentNullException.ThrowIfNull(details);
        ArgumentNullException.ThrowIfNull(firstVariant);

        var product = new Product(Guid.CreateVersion7(), categoryId, details, slug);
        var variantResult = product.AddVariant(firstVariant);

        return variantResult.IsFailure ? Result.Failure<Product>(variantResult.Error) : product;
    }

    public void UpdateDetails(Guid categoryId, ProductDetails details, Slug slug)
    {
        ArgumentNullException.ThrowIfNull(details);
        CategoryId = categoryId;
        Slug = slug;
        ApplyDetails(details);
    }

    public Result SetVisibility(bool isActive, bool isFeatured)
    {
        if (isActive && !_variants.Any(v => v.IsActive))
        {
            return Result.Failure(CatalogErrors.CannotActivateWithoutActiveVariant);
        }

        IsActive = isActive;
        IsFeatured = isFeatured;
        return Result.Success();
    }

    public Result<ProductVariant> AddVariant(VariantSpec spec)
    {
        ArgumentNullException.ThrowIfNull(spec);

        if (_variants.Count >= MaxVariants)
        {
            return Result.Failure<ProductVariant>(CatalogErrors.TooManyVariants);
        }

        if (_variants.Any(v => v.Sku == spec.Sku))
        {
            return Result.Failure<ProductVariant>(CatalogErrors.DuplicateSku);
        }

        var pricing = ProductVariant.ValidatePricing(spec.Price, spec.CompareAtPrice);
        if (pricing.IsFailure)
        {
            return Result.Failure<ProductVariant>(pricing.Error);
        }

        var variant = new ProductVariant(Id, NormalizeVariantName(spec.Name), spec.Sku, spec.Price, spec.CompareAtPrice, spec.WeightGrams, spec.SortOrder);
        _variants.Add(variant);
        return variant;
    }

    public Result UpdateVariant(Guid variantId, VariantSpec spec, bool isActive)
    {
        ArgumentNullException.ThrowIfNull(spec);

        var variant = _variants.Find(v => v.Id == variantId);
        if (variant is null)
        {
            return Result.Failure(CatalogErrors.VariantNotFound);
        }

        if (_variants.Any(v => v.Id != variantId && v.Sku == spec.Sku))
        {
            return Result.Failure(CatalogErrors.DuplicateSku);
        }

        var pricing = ProductVariant.ValidatePricing(spec.Price, spec.CompareAtPrice);
        if (pricing.IsFailure)
        {
            return pricing;
        }

        if (!isActive && IsActive && !_variants.Any(v => v.Id != variantId && v.IsActive))
        {
            return Result.Failure(CatalogErrors.CannotDeactivateLastActiveVariant);
        }

        variant.Update(NormalizeVariantName(spec.Name), spec.Sku, spec.Price, spec.CompareAtPrice, spec.WeightGrams, spec.SortOrder);
        variant.SetActive(isActive);
        return Result.Success();
    }

    public Result RemoveVariant(Guid variantId)
    {
        var variant = _variants.Find(v => v.Id == variantId);
        if (variant is null)
        {
            return Result.Failure(CatalogErrors.VariantNotFound);
        }

        if (_variants.Count == 1)
        {
            return Result.Failure(CatalogErrors.CannotRemoveLastVariant);
        }

        if (IsActive && variant.IsActive && !_variants.Any(v => v.Id != variantId && v.IsActive))
        {
            return Result.Failure(CatalogErrors.CannotDeactivateLastActiveVariant);
        }

        _variants.Remove(variant);
        return Result.Success();
    }

    public Result<ProductImage> AddImage(string url, string? altText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        if (_images.Count >= MaxImages)
        {
            return Result.Failure<ProductImage>(CatalogErrors.TooManyImages);
        }

        var sortOrder = _images.Count == 0 ? 0 : _images.Max(i => i.SortOrder) + 1;
        var image = new ProductImage(Id, url, string.IsNullOrWhiteSpace(altText) ? null : altText.Trim(), sortOrder, isPrimary: _images.Count == 0);
        _images.Add(image);
        return image;
    }

    /// <summary>Removes an image and returns its URL so the caller can delete the stored file.</summary>
    public Result<string> RemoveImage(Guid imageId)
    {
        var image = _images.Find(i => i.Id == imageId);
        if (image is null)
        {
            return Result.Failure<string>(CatalogErrors.ImageNotFound);
        }

        _images.Remove(image);

        if (image.IsPrimary && _images.Count > 0)
        {
            _images.OrderBy(i => i.SortOrder).First().SetPrimary(true);
        }

        return image.Url;
    }

    public Result SetPrimaryImage(Guid imageId)
    {
        if (!_images.Exists(i => i.Id == imageId))
        {
            return Result.Failure(CatalogErrors.ImageNotFound);
        }

        foreach (var image in _images)
        {
            image.SetPrimary(image.Id == imageId);
        }

        return Result.Success();
    }

    public Result ReorderImages(IReadOnlyList<Guid> orderedImageIds)
    {
        ArgumentNullException.ThrowIfNull(orderedImageIds);

        var sameSet = orderedImageIds.Count == _images.Count
            && orderedImageIds.Distinct().Count() == orderedImageIds.Count
            && orderedImageIds.All(id => _images.Exists(i => i.Id == id));

        if (!sameSet)
        {
            return Result.Failure(CatalogErrors.ImageOrderMismatch);
        }

        for (var i = 0; i < orderedImageIds.Count; i++)
        {
            _images.First(img => img.Id == orderedImageIds[i]).SetSortOrder(i);
        }

        return Result.Success();
    }

    public void RecordSale(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        SoldCount += quantity;
    }

    private void ApplyDetails(ProductDetails details)
    {
        Name = details.Name.Trim();
        ShortDescription = Clean(details.ShortDescription);
        Description = Clean(details.Description);
        Ingredients = Clean(details.Ingredients);
        Origin = Clean(details.Origin);
        Brand = Clean(details.Brand);
        ShelfLifeDays = details.ShelfLifeDays;
        SearchText = TextNormalizer.ForSearch($"{Name} {Brand}");
    }

    private static string NormalizeVariantName(string name) =>
        string.IsNullOrWhiteSpace(name) ? ProductVariant.DefaultName : name.Trim();

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

/// <summary>Descriptive fields of a product, grouped to keep factory/update signatures short.</summary>
public sealed record ProductDetails(
    string Name,
    string? ShortDescription,
    string? Description,
    string? Ingredients,
    string? Origin,
    string? Brand,
    int? ShelfLifeDays);

/// <summary>Input for creating or updating a variant.</summary>
public sealed record VariantSpec(
    string Name,
    Sku Sku,
    Money Price,
    Money? CompareAtPrice,
    int? WeightGrams,
    int SortOrder);
