using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Services;

/// <summary>
/// Coordinates local inventory scan and persistence operations.
/// </summary>
public sealed class LocalInventoryService(ILocalFileScanner scanner, ILocalInventoryStore store) : ILocalInventoryService
{
    /// <inheritdoc />
    public Task<Result<IReadOnlyList<LocalInventoryItem>, string>> RunStartupScanAsync(string accountId, string rootPath, CancellationToken cancellationToken = default)
        => ScanAndPersistAsync(accountId, rootPath, cancellationToken);

    /// <inheritdoc />
    public Task<Result<IReadOnlyList<LocalInventoryItem>, string>> RunManualScanAsync(string accountId, string rootPath, CancellationToken cancellationToken = default)
        => ScanAndPersistAsync(accountId, rootPath, cancellationToken);

    /// <inheritdoc />
    public Task<Result<IReadOnlyList<LocalInventoryItem>, string>> LoadSnapshotAsync(string accountId, CancellationToken cancellationToken = default)
        => store.LoadAsync(accountId, cancellationToken);

    private async Task<Result<IReadOnlyList<LocalInventoryItem>, string>> ScanAndPersistAsync(string accountId, string rootPath, CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<LocalInventoryItem>, string> scanResult = await scanner.ScanAsync(rootPath, cancellationToken);
        if(scanResult is Result<IReadOnlyList<LocalInventoryItem>, string>.Error scanError)
        {
            return scanError.Reason;
        }

        IReadOnlyList<LocalInventoryItem> snapshot = ((Result<IReadOnlyList<LocalInventoryItem>, string>.Ok)scanResult).Value;
        Result<Unit, string> saveResult = await store.SaveAsync(accountId, snapshot, cancellationToken);
        return saveResult switch
        {
            Result<Unit, string>.Ok => scanResult,
            Result<Unit, string>.Error saveError => saveError.Reason,
            _ => "local inventory persistence failed"
        };
    }
}