using System.ComponentModel.DataAnnotations;

namespace SnackShop.Infrastructure.Storage;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>Directory where uploads are written. Relative paths resolve against the content root.</summary>
    [Required]
    public string LocalPath { get; init; } = "uploads";

    /// <summary>Public URL prefix that serves <see cref="LocalPath"/>, e.g. https://api.snackshop.vn/uploads or /uploads.</summary>
    [Required]
    public string PublicBaseUrl { get; init; } = "/uploads";

    /// <summary>Request path the API serves uploads from (static files middleware).</summary>
    public const string RequestPath = "/uploads";
}
