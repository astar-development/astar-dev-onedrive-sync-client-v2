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
    {
        await using AstarOneDriveDbContextModel context = AstarOneDriveDbContextFactory.Create(databasePath);
        await EnsureDefaultAccountAsync(context, cancellationToken);

        List<SyncFileEntity> existing = await context.SyncFiles.Where(x => x.AccountId == DefaultAccountId).ToListAsync(cancellationToken);
        context.SyncFiles.RemoveRange(existing);

        DateTime now = DateTime.UtcNow;
        foreach (FolderNodeState node in nodes)
        {
            _ = context.SyncFiles.Add(new SyncFileEntity
            {
                Id = node.Id,
                AccountId = DefaultAccountId,
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
    {
        await using AstarOneDriveDbContextModel context = AstarOneDriveDbContextFactory.Create(databasePath);
        List<FolderNodeState> items = await context.SyncFiles
            .AsNoTracking()
            .Where(x => x.AccountId == DefaultAccountId)
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

    private static async Task EnsureDefaultAccountAsync(AstarOneDriveDbContextModel context, CancellationToken cancellationToken)
    {
        AccountEntity? existing = await context.Accounts.SingleOrDefaultAsync(x => x.Id == DefaultAccountId, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        _ = context.Accounts.Add(new AccountEntity
        {
            Id = DefaultAccountId,
            Email = DefaultEmail,
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

        while (lookup.TryGetValue(currentId, out FolderNodeState? current))
        {
            parts.Add(current.Name);
            if (string.IsNullOrWhiteSpace(current.ParentId))
            {
                break;
            }

            currentId = current.ParentId;
        }

        var path = string.Join('/', parts);
        return isRemote ? $"/{path}" : $"/local/{path}";
    }
}
