namespace SnackShop.Application.Abstractions.Services;

/// <summary>Stores uploaded files and returns their public URL. Local disk at MVP, S3-compatible later.</summary>
public interface IFileStorage
{
    /// <param name="content">File bytes; read to the end, not disposed.</param>
    /// <param name="extension">Lowercase extension including the dot, e.g. ".webp".</param>
    /// <param name="folder">Logical folder, e.g. "products".</param>
    /// <param name="cancellationToken">Cancellation.</param>
    Task<string> SaveAsync(Stream content, string extension, string folder, CancellationToken cancellationToken);

    /// <summary>Deletes a file previously returned by <see cref="SaveAsync"/>. Unknown URLs are ignored.</summary>
    Task DeleteAsync(string url, CancellationToken cancellationToken);
}
