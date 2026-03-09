using System.Security.Cryptography;
using System.Text;
using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Repositories;

/// <summary>
/// Scans local files and emits deterministic metadata-based inventory items.
/// </summary>
public sealed class FileSystemLocalFileScanner : ILocalFileScanner
{
    private const string FingerprintPolicy = "metadata-sha256-v1";

    /// <inheritdoc />
    public Task<Result<IReadOnlyList<LocalInventoryItem>, string>> ScanAsync(string rootPath, CancellationToken cancellationToken = default)
        => Try.RunAsync(() => ScanInternal(rootPath, cancellationToken))
            .MapFailureAsync(error => error.Message);

    private static Task<IReadOnlyList<LocalInventoryItem>> ScanInternal(string rootPath, CancellationToken cancellationToken)
    {
        if(string.IsNullOrWhiteSpace(rootPath))
        {
            return Task.FromResult<IReadOnlyList<LocalInventoryItem>>([]);
        }

        if(!Directory.Exists(rootPath))
        {
            return Task.FromResult<IReadOnlyList<LocalInventoryItem>>([]);
        }

        var basePath = Path.GetFullPath(rootPath);
        var items = Directory
            .EnumerateFiles(basePath, "*", SearchOption.AllDirectories)
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(path => BuildItem(basePath, path))
            .ToList();
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<LocalInventoryItem>>(items);
    }

    private static LocalInventoryItem BuildItem(string basePath, string filePath)
    {
        var info = new FileInfo(filePath);
        var relativePath = Path.GetRelativePath(basePath, info.FullName).Replace('\\', '/');
        var metadata = $"{relativePath}|{info.Length}|{info.LastWriteTimeUtc.Ticks}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(metadata));
        var fingerprint = Convert.ToHexString(bytes).ToLowerInvariant();
        return new LocalInventoryItem(info.FullName, relativePath, info.Length, info.LastWriteTimeUtc, fingerprint, FingerprintPolicy);
    }
}