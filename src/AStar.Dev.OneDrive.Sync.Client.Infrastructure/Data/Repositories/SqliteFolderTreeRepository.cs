using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Contracts;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Repositories;

/// <summary>
/// SQLite repository for persisting and retrieving folder tree structures.
/// </summary>
public sealed class SqliteFolderTreeRepository(string? databasePath = null)
{
    private const string DefaultAccountId = "local-folder-tree-account";
    private const string DefaultEmail = "folder-tree@local.astar";

    /// <summary>
    /// Saves a collection of folder nodes to the database, replacing all existing nodes.
    /// </summary>
    /// <param name="nodes">The folder nodes to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task SaveAsync(IReadOnlyList<FolderNodeState> nodes, CancellationToken cancellationToken = default)
        => await SaveAsync(DefaultAccountId, nodes, cancellationToken);

    /// <summary>
    /// Saves a collection of folder nodes for a specific account, replacing existing nodes for that account.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="nodes">The folder nodes to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task SaveAsync(string accountId, IReadOnlyList<FolderNodeState> nodes, CancellationToken cancellationToken = default)
    {
        var normalizedAccountId = NormalizeAccountId(accountId);
        await using AstarOneDriveDbContextModel context = AstarOneDriveDbContextFactory.Create(databasePath);
        await EnsureAccountAsync(context, normalizedAccountId, cancellationToken);

        List<SyncFileEntity> existing = await context.SyncFiles.Where(x => x.AccountId == normalizedAccountId).ToListAsync(cancellationToken);
        context.SyncFiles.RemoveRange(existing);

        DateTime now = DateTime.UtcNow;
        foreach(FolderNodeState node in nodes)
        {
            _ = context.SyncFiles.Add(new SyncFileEntity
            {
                Id = node.Id,
                AccountId = normalizedAccountId,
                ParentId = node.ParentId,
                Name = node.Name,
                LocalPath = BuildPath(nodes, node.Id, isRemote: false),
                RemotePath = BuildPath(nodes, node.Id, isRemote: true),
                ItemType = "Folder",
                IsSelected = node.IsSelected,
                IsExpanded = node.IsExpanded,
                SortOrder = node.SortOrder,
                CreatedUtc = now,
                UpdatedUtc = now
            });
        }

        _ = await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Loads all folder nodes from the database, ordered by sort order.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of folder node states.</returns>
    public async Task<IReadOnlyList<FolderNodeState>> LoadAsync(CancellationToken cancellationToken = default)
        => await LoadAsync(DefaultAccountId, cancellationToken);

    /// <summary>
    /// Loads all folder nodes for a specific account, ordered by sort order.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of folder node states.</returns>
    public async Task<IReadOnlyList<FolderNodeState>> LoadAsync(string accountId, CancellationToken cancellationToken = default)
    {
        var normalizedAccountId = NormalizeAccountId(accountId);
        await using AstarOneDriveDbContextModel context = AstarOneDriveDbContextFactory.Create(databasePath);
        List<FolderNodeState> items = await context.SyncFiles
            .AsNoTracking()
            .Where(x => x.AccountId == normalizedAccountId)
            .OrderBy(x => x.SortOrder)
            .Select(x => new FolderNodeState(
                x.Id,
                x.ParentId,
                x.Name,
                x.IsSelected,
                x.IsExpanded,
                x.SortOrder))
            .ToListAsync(cancellationToken);

        return items;
    }

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
            Email = BuildEmail(accountId),
            QuotaBytes = 0,
            UsedBytes = 0,
            IsActive = true,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        });
        _ = await context.SaveChangesAsync(cancellationToken);
    }

    private static string NormalizeAccountId(string accountId)
        => string.IsNullOrWhiteSpace(accountId) ? DefaultAccountId : accountId;

    private static string BuildEmail(string accountId)
        => string.Equals(accountId, DefaultAccountId, StringComparison.Ordinal)
            ? DefaultEmail
            : $"{accountId}@folder-tree.local";

    private static string BuildPath(IReadOnlyList<FolderNodeState> nodes, string nodeId, bool isRemote)
    {
        var lookup = nodes.ToDictionary(x => x.Id, x => x);
        var parts = new List<string>();
        var currentId = nodeId;

        while(lookup.TryGetValue(currentId, out FolderNodeState? current))
        {
            parts.Add(current.Name);
            if(string.IsNullOrWhiteSpace(current.ParentId))
            {
                break;
            }

            currentId = current.ParentId;
        }

        var path = string.Join('/', parts);
        return isRemote ? $"/{path}" : $"/local/{path}";
    }
}
