using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Repositories;

/// <summary>
/// Persists account-scoped local inventory snapshots in SQLite.
/// </summary>
public sealed class SqliteLocalInventoryStore(string? databasePath = null) : ILocalInventoryStore
{
    private const string InventoryPrefix = "/__local_inventory__/";

    /// <inheritdoc />
    public Task<Result<Unit, string>> SaveAsync(string accountId, IReadOnlyList<LocalInventoryItem> items, CancellationToken cancellationToken = default)
        => Try.RunAsync(async () =>
        {
            var normalizedAccountId = NormalizeAccountId(accountId);
            await using AstarOneDriveDbContextModel context = AstarOneDriveDbContextFactory.Create(databasePath);
            await EnsureAccountAsync(context, normalizedAccountId, cancellationToken);

            List<SyncFileEntity> existing = await context.SyncFiles
                .Where(x => x.AccountId == normalizedAccountId && x.RemotePath.StartsWith(InventoryPrefix))
                .ToListAsync(cancellationToken);
            context.SyncFiles.RemoveRange(existing);

            DateTime now = DateTime.UtcNow;
            foreach(LocalInventoryItem item in items)
            {
                _ = context.SyncFiles.Add(new SyncFileEntity
                {
                    Id = BuildId(item.RelativePath),
                    AccountId = normalizedAccountId,
                    ParentId = null,
                    Name = Path.GetFileName(item.LocalPath),
                    LocalPath = item.LocalPath,
                    RemotePath = $"{InventoryPrefix}{item.RelativePath}",
                    ItemType = "File",
                    IsSelected = false,
                    IsExpanded = false,
                    LastSyncUtc = item.LastWriteUtc,
                    CTag = item.Fingerprint,
                    ETag = item.FingerprintPolicy,
                    SizeBytes = item.SizeBytes,
                    SortOrder = 0,
                    CreatedUtc = now,
                    UpdatedUtc = now
                });
            }

            _ = await context.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }).MapFailureAsync(error => error.Message);

    /// <inheritdoc />
    public Task<Result<IReadOnlyList<LocalInventoryItem>, string>> LoadAsync(string accountId, CancellationToken cancellationToken = default)
        => Try.RunAsync(async () =>
        {
            var normalizedAccountId = NormalizeAccountId(accountId);
            await using AstarOneDriveDbContextModel context = AstarOneDriveDbContextFactory.Create(databasePath);

            List<LocalInventoryItem> items = await context.SyncFiles
                .AsNoTracking()
                .Where(x => x.AccountId == normalizedAccountId && x.RemotePath.StartsWith(InventoryPrefix))
                .OrderBy(x => x.LocalPath)
                .Select(x => new LocalInventoryItem(
                    x.LocalPath,
                    x.RemotePath.Substring(InventoryPrefix.Length),
                    x.SizeBytes ?? 0,
                    x.LastSyncUtc ?? DateTime.UnixEpoch,
                    x.CTag ?? string.Empty,
                    x.ETag ?? string.Empty))
                .ToListAsync(cancellationToken);

            return (IReadOnlyList<LocalInventoryItem>)items;
        }).MapFailureAsync(error => error.Message);

    private static async Task EnsureAccountAsync(AstarOneDriveDbContextModel context, string accountId, CancellationToken cancellationToken)
    {
        AccountEntity? existing = await context.Accounts.SingleOrDefaultAsync(x => x.Id == accountId, cancellationToken);
        if(existing is not null)
        {
            return;
        }

        _ = context.Accounts.Add(new AccountEntity
        {
            Id = accountId,
            Email = $"{accountId}@local.inventory",
            QuotaBytes = 0,
            UsedBytes = 0,
            IsActive = true,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        });
        _ = await context.SaveChangesAsync(cancellationToken);
    }

    private static string NormalizeAccountId(string accountId)
        => string.IsNullOrWhiteSpace(accountId) ? "local-inventory-account" : accountId;

    private static string BuildId(string relativePath)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(relativePath));
        return Convert.ToHexString(bytes[..16]).ToLowerInvariant();
    }
}