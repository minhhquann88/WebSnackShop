using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SnackShop.Application.Abstractions.Services;

namespace SnackShop.Infrastructure.Storage;

/// <summary>
/// Stores files on local disk under a random name (never the client's file name). Swap for an S3-compatible
/// implementation when uploads outgrow the VPS (docs/08 §8).
/// </summary>
public sealed class LocalFileStorage(IOptions<StorageOptions> options, IHostEnvironment environment) : IFileStorage
{
    private readonly StorageOptions _options = options.Value;

    public string RootPath => Path.GetFullPath(Path.IsPathRooted(_options.LocalPath)
        ? _options.LocalPath
        : Path.Combine(environment.ContentRootPath, _options.LocalPath));

    public async Task<string> SaveAsync(Stream content, string extension, string folder, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(content);
        EnsureSafeSegment(folder);
        EnsureSafeExtension(extension);

        var directory = Path.Combine(RootPath, folder);
        Directory.CreateDirectory(directory);

        var fileName = $"{Guid.CreateVersion7():N}{extension}";
        var fullPath = Path.Combine(directory, fileName);

        await using (var file = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
        {
            await content.CopyToAsync(file, cancellationToken);
        }

        return $"{_options.PublicBaseUrl.TrimEnd('/')}/{folder}/{fileName}";
    }

    public Task DeleteAsync(string url, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(url);

        var prefix = _options.PublicBaseUrl.TrimEnd('/') + "/";
        if (!url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        var relative = url[prefix.Length..].Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(RootPath, relative));

        // Never delete outside the uploads root (defence against crafted URLs).
        if (fullPath.StartsWith(RootPath + Path.DirectorySeparatorChar, StringComparison.Ordinal) && File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private static void EnsureSafeSegment(string segment)
    {
        if (string.IsNullOrWhiteSpace(segment) || segment.Any(c => !char.IsAsciiLetterOrDigit(c) && c != '-'))
        {
            throw new ArgumentException("Folder must be a simple name.", nameof(segment));
        }
    }

    private static void EnsureSafeExtension(string extension)
    {
        if (extension is not (".jpg" or ".png" or ".webp"))
        {
            throw new ArgumentException("Unsupported extension.", nameof(extension));
        }
    }
}
