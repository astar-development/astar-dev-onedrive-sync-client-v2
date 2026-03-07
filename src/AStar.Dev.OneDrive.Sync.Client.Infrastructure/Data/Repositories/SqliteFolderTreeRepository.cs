using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Contracts;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Repositories;

/// <summary>
/// SQLite repository for persisting and retrieving folder tree structures.
/// </summary>
public sealed class SqliteFolderTreeRepository(string? databasePath = null)
{
    public const string DefaultAccountId = "local-folder-tree-account";
    private const string DefaultEmail = "folder-tree@local.astar";
    private const char StorageSeparator = ':';

    /// <summary>
    /// Saves a collection of folder nodes to the database, replacing all existing nodes.
    /// </summary>
    /// <param name="nodes">The folder nodes to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task SaveAsync(IReadOnlyList<FolderNodeState> nodes, CancellationToken cancellationToken = default)
        => await SaveAsync(DefaultAccountId, nodes, cancellationToken);

    /// <summary>
    /// Saves a collection of folder nodes for a specific account, replacing existing account nodes.
    /// </summary>
    /// <param name="accountId">The owning account identifier.</param>
    /// <param name="nodes">The folder nodes to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task SaveAsync(string accountId, IReadOnlyList<FolderNodeState> nodes, CancellationToken cancellationToken = default)
    {
        var scopedAccountId = ResolveAccountId(accountId);
        await using AstarOneDriveDbContextModel context = AstarOneDriveDbContextFactory.Create(databasePath);
        await EnsureAccountAsync(context, scopedAccountId, cancellationToken);

        List<SyncFileEntity> existing = await context.SyncFiles.Where(x => x.AccountId == scopedAccountId).ToListAsync(cancellationToken);
        context.SyncFiles.RemoveRange(existing);

        DateTime now = DateTime.UtcNow;
        foreach(FolderNodeState node in nodes)
        {
            _ = context.SyncFiles.Add(new SyncFileEntity
            {
                Id = ToStoredNodeId(scopedAccountId, node.Id),
                AccountId = scopedAccountId,
                ParentId = ToStoredParentId(scopedAccountId, node.ParentId),
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
    /// Loads folder nodes for a specific account, ordered by sort order.
    /// </summary>
    /// <param name="accountId">The owning account identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of folder node states.</returns>
    public async Task<IReadOnlyList<FolderNodeState>> LoadAsync(string accountId, CancellationToken cancellationToken = default)
    {
        var scopedAccountId = ResolveAccountId(accountId);
        await using AstarOneDriveDbContextModel context = AstarOneDriveDbContextFactory.Create(databasePath);
        List<FolderNodeState> items = await context.SyncFiles
            .AsNoTracking()
            .Where(x => x.AccountId == scopedAccountId)
            .OrderBy(x => x.SortOrder)
            .Select(x => new FolderNodeState(
                FromStoredNodeId(scopedAccountId, x.Id),
                FromStoredParentId(scopedAccountId, x.ParentId),
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

        var email = accountId == DefaultAccountId
            ? DefaultEmail
            : $"{accountId}@folder-tree.local";

        _ = context.Accounts.Add(new AccountEntity
        {
            Id = accountId,
            Email = email,
            QuotaBytes = 0,
            UsedBytes = 0,
            IsActive = true,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        });
        _ = await context.SaveChangesAsync(cancellationToken);
    }

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

    private static string ResolveAccountId(string? accountId)
        => string.IsNullOrWhiteSpace(accountId) ? DefaultAccountId : accountId.Trim();

    private static string ToStoredNodeId(string accountId, string nodeId)
        => $"{accountId}{StorageSeparator}{nodeId}";

    private static string? ToStoredParentId(string accountId, string? parentId)
        => string.IsNullOrWhiteSpace(parentId) ? null : ToStoredNodeId(accountId, parentId);

    private static string FromStoredNodeId(string accountId, string storedNodeId)
    {
        var prefix = $"{accountId}{StorageSeparator}";
        return storedNodeId.StartsWith(prefix, StringComparison.Ordinal)
            ? storedNodeId[prefix.Length..]
            : storedNodeId;
    }

    private static string? FromStoredParentId(string accountId, string? storedParentId)
        => string.IsNullOrWhiteSpace(storedParentId)
            ? null
            : FromStoredNodeId(accountId, storedParentId);
}
