using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;

namespace AStar.Dev.OneDrive.Sync.Client.UI.SyncStatus;

/// <summary>
/// In-memory run state store used by UI composition for orchestrated sync flows.
/// </summary>
public sealed class InMemorySyncRunStateStore : ISyncRunStateStore
{
    private readonly Lock _gate = new();
    private readonly Dictionary<string, SyncRunState> _states = [];

    /// <inheritdoc />
    public Task<Result<Option<SyncRunState>, string>> LoadAsync(string accountId, string scopeId, CancellationToken cancellationToken = default)
    {
        lock(_gate)
        {
            return Task.FromResult<Result<Option<SyncRunState>, string>>(
                _states.TryGetValue(Key(accountId, scopeId), out SyncRunState? state)
                    ? new Option<SyncRunState>.Some(state)
                    : Option.None<SyncRunState>());
        }
    }

    /// <inheritdoc />
    public Task<Result<Unit, string>> SaveAsync(SyncRunState state, CancellationToken cancellationToken = default)
    {
        lock(_gate)
        {
            _states[Key(state.AccountId, state.ScopeId)] = state;
            return Task.FromResult<Result<Unit, string>>(Unit.Value);
        }
    }

    /// <inheritdoc />
    public Task<Result<Unit, string>> ClearAsync(string accountId, string scopeId, CancellationToken cancellationToken = default)
    {
        lock(_gate)
        {
            _ = _states.Remove(Key(accountId, scopeId));
            return Task.FromResult<Result<Unit, string>>(Unit.Value);
        }
    }

    private static string Key(string accountId, string scopeId)
        => $"{accountId}:{scopeId}";
}