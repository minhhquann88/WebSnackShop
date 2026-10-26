using SnackShop.Domain.Common;

namespace SnackShop.Domain.Catalog;

/// <summary>A group of products shown in the storefront menu (single level at MVP).</summary>
public sealed class Category : AggregateRoot, IAuditable, ISoftDeletable
{
    public const int MaxNameLength = 100;
    public const int MaxDescriptionLength = 500;
    public const int MaxImageUrlLength = 500;

    private Category()
    {
    }

    private Category(Guid id, string name, Slug slug, string? description, string? imageUrl, int sortOrder)
        : base(id)
    {
        Name = name;
        Slug = slug;
        Description = description;
        ImageUrl = imageUrl;
        SortOrder = sortOrder;
        IsActive = true;
    }

    public string Name { get; private set; } = default!;

    public Slug Slug { get; private set; } = default!;

    public string? Description { get; private set; }

    public string? ImageUrl { get; private set; }

    public int SortOrder { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public static Category Create(string name, Slug slug, string? description, string? imageUrl, int sortOrder) =>
        new(Guid.CreateVersion7(), name.Trim(), slug, Clean(description), Clean(imageUrl), sortOrder);

    public void Update(string name, Slug slug, string? description, string? imageUrl, int sortOrder, bool isActive)
    {
        Name = name.Trim();
        Slug = slug;
        Description = Clean(description);
        ImageUrl = Clean(imageUrl);
        SortOrder = sortOrder;
        IsActive = isActive;
    }

    public void MoveTo(int sortOrder) => SortOrder = sortOrder;

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
