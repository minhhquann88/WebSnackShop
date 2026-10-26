using SnackShop.Domain.Common;

namespace SnackShop.Domain.Catalog;

public static class CatalogErrors
{
    public static readonly Error InvalidSlug = Error.Validation(
        "Catalog.InvalidSlug", "Slug must contain only lowercase letters, digits and single hyphens (max 120).");

    public static readonly Error InvalidSku = Error.Validation(
        "Catalog.InvalidSku", "SKU must be 3-50 characters of uppercase letters, digits or hyphens.");

    public static readonly Error CategoryNotFound = Error.NotFound("Catalog.CategoryNotFound", "The category was not found.");

    public static readonly Error ProductNotFound = Error.NotFound("Catalog.ProductNotFound", "The product was not found.");

    public static readonly Error VariantNotFound = Error.NotFound("Catalog.VariantNotFound", "The product variant was not found.");

    public static readonly Error ImageNotFound = Error.NotFound("Catalog.ImageNotFound", "The product image was not found.");

    public static readonly Error DuplicateCategorySlug = Error.Conflict("Catalog.DuplicateCategorySlug", "Another category already uses this slug.");

    public static readonly Error DuplicateCategoryName = Error.Conflict("Catalog.DuplicateCategoryName", "Another category already uses this name.");

    public static readonly Error DuplicateProductSlug = Error.Conflict("Catalog.DuplicateProductSlug", "Another product already uses this slug.");

    public static readonly Error DuplicateSku = Error.Conflict("Catalog.DuplicateSku", "Another variant already uses this SKU.");

    public static readonly Error CategoryHasProducts = Error.Conflict(
        "Catalog.CategoryHasProducts", "The category still contains products. Move or delete them first.");

    public static readonly Error CannotRemoveLastVariant = Error.Conflict(
        "Catalog.CannotRemoveLastVariant", "A product must keep at least one variant.");

    public static readonly Error CannotActivateWithoutActiveVariant = Error.Conflict(
        "Catalog.CannotActivateWithoutActiveVariant", "A product needs at least one active variant before it can be shown.");

    public static readonly Error CannotDeactivateLastActiveVariant = Error.Conflict(
        "Catalog.CannotDeactivateLastActiveVariant", "An active product must keep at least one active variant. Hide the product first.");

    public static readonly Error TooManyImages = Error.Validation(
        "Catalog.TooManyImages", $"A product can have at most {Product.MaxImages} images.");

    public static readonly Error TooManyVariants = Error.Validation(
        "Catalog.TooManyVariants", $"A product can have at most {Product.MaxVariants} variants.");

    public static readonly Error InvalidPrice = Error.Validation("Catalog.InvalidPrice", "Price must be greater than zero.");

    public static readonly Error InvalidCompareAtPrice = Error.Validation(
        "Catalog.InvalidCompareAtPrice", "Compare-at price must be greater than the selling price.");

    public static readonly Error ImageOrderMismatch = Error.Validation(
        "Catalog.ImageOrderMismatch", "The new image order must list every image of the product exactly once.");

    public static readonly Error UnsupportedImage = Error.Validation(
        "Catalog.UnsupportedImage", "Only JPEG, PNG or WebP images up to 5 MB are accepted.");
}
