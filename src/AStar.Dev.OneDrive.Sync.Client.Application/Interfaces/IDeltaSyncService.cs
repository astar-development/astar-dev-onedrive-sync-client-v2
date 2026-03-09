using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;

/// <summary>
/// Defines remote delta pull operations with checkpoint progression.
/// </summary>
public interface IDeltaSyncService
{
    /// <summary>
    /// Pulls remote delta pages for an account/scope and commits checkpoint on successful completion.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="scopeId">The sync scope identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The pull summary.</returns>
    Task<Result<DeltaPullSummary, string>> PullAsync(string accountId, string scopeId, CancellationToken cancellationToken = default);
}