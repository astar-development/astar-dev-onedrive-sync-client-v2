using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Repositories;

/// <summary>
/// Placeholder delta applier implementation.
/// </summary>
public sealed class NoOpRemoteDeltaApplier : IRemoteDeltaApplier
{
    /// <inheritdoc />
    public Task<Result<Unit, string>> ApplyAsync(string accountId, string scopeId, IReadOnlyList<RemoteDeltaItem> changes, CancellationToken cancellationToken = default)
        => Task.FromResult<Result<Unit, string>>(Unit.Value);
}