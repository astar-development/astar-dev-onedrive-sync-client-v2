using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Graph;

/// <summary>
/// Placeholder remote delta source that returns an empty terminal page.
/// </summary>
public sealed class NoOpRemoteDeltaSource : IRemoteDeltaSource
{
    /// <inheritdoc />
    public Task<Result<RemoteDeltaPage, string>> GetDeltaPageAsync(string accountId, string scopeId, string? cursor, CancellationToken cancellationToken = default)
        => Task.FromResult<Result<RemoteDeltaPage, string>>(new RemoteDeltaPage([], null, cursor ?? string.Empty));
}