using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Repositories;

/// <summary>
/// Applies remote delta changes into the local SQLite sync file index.
/// </summary>
public sealed class SqliteRemoteDeltaApplier(string? databasePath = null) : IRemoteDeltaApplier
{
    /// <inheritdoc />
    public async Task<Result<Unit, string>> ApplyAsync(string accountId, string scopeId, IReadOnlyList<RemoteDeltaItem> changes, CancellationToken cancellationToken = default)
        => await Try.RunAsync(async () =>
        {
            await using AstarOneDriveDbContextModel context = AstarOneDriveDbContextFactory.Create(databasePath);
            await EnsureAccountAsync(context, accountId, cancellationToken);

            foreach(RemoteDeltaItem change in changes)
            {
                if(change.ChangeKind == RemoteDeltaChangeKind.Deleted)
                {
                    await DeleteAsync(context, accountId, change, cancellationToken);
                    continue;
                }

                await UpsertAsync(context, accountId, change, cancellationToken);
            }

            _ = await context.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }).MapFailureAsync(error => error.Message);

    private static async Task DeleteAsync(AstarOneDriveDbContextModel context, string accountId, RemoteDeltaItem change, CancellationToken cancellationToken)
    {
        SyncFileEntity? existing = await context.SyncFiles
            .SingleOrDefaultAsync(x => x.AccountId == accountId && x.Id == change.Id, cancellationToken);
        if(existing is null)
        {
            return;
        }

        _ = context.SyncFiles.Remove(existing);
    }

    private static async Task UpsertAsync(AstarOneDriveDbContextModel context, string accountId, RemoteDeltaItem change, CancellationToken cancellationToken)
    {
        var remotePath = NormalizeRemotePath(change.Path, change.Id);
        SyncFileEntity? existing = await context.SyncFiles
            .SingleOrDefaultAsync(x => x.AccountId == accountId && x.Id == change.Id, cancellationToken);
        if(existing is null)
        {
            _ = context.SyncFiles.Add(new SyncFileEntity
            {
                Id = change.Id,
                AccountId = accountId,
                ParentId = null,
                Name = ExtractName(remotePath, change.Id),
                LocalPath = ToLocalPath(remotePath),
                RemotePath = remotePath,
                ItemType = "File",
                IsSelected = true,
                IsExpanded = false,
                SortOrder = 0,
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            });
            return;
        }

        existing.Name = ExtractName(remotePath, change.Id);
        existing.LocalPath = ToLocalPath(remotePath);
        existing.RemotePath = remotePath;
        existing.UpdatedUtc = DateTime.UtcNow;
    }

    private static async Task EnsureAccountAsync(AstarOneDriveDbContextModel context, string accountId, CancellationToken cancellationToken)
    {
        AccountEntity? account = await context.Accounts.SingleOrDefaultAsync(x => x.Id == accountId, cancellationToken);
        if(account is not null)
        {
            return;
        }

        _ = context.Accounts.Add(new AccountEntity
        {
            Id = accountId,
            Email = $"{accountId}@delta.local",
            QuotaBytes = 0,
            UsedBytes = 0,
            IsActive = true,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        });
    }

    private static string NormalizeRemotePath(string path, string id)
    {
        var normalized = string.IsNullOrWhiteSpace(path) ? $"/{id}" : path;
        return normalized.StartsWith("/", StringComparison.Ordinal) ? normalized : $"/{normalized}";
    }

    private static string ToLocalPath(string remotePath)
        => $"/local{remotePath}";

    private static string ExtractName(string remotePath, string fallback)
    {
        var trimmed = remotePath.TrimEnd('/');
        if(string.IsNullOrWhiteSpace(trimmed) || string.Equals(trimmed, "/", StringComparison.Ordinal))
        {
            return fallback;
        }

        var separator = trimmed.LastIndexOf('/');
        return separator >= 0 && separator < trimmed.Length - 1 ? trimmed[(separator + 1)..] : fallback;
    }
}