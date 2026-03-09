using AStar.Dev.Functional.Extensions;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;

/// <summary>
/// Defines orchestration and scheduling operations for end-to-end sync runs.
/// </summary>
public interface ISyncOrchestratorService
{
    Task<Result<Unit, string>> RunOnceAsync(string accountId, string scopeId, string rootPath, bool useStartupScan, CancellationToken cancellationToken = default);
    Task<Result<Unit, string>> PauseAsync(CancellationToken cancellationToken = default);
    Task<Result<Unit, string>> ResumeAsync(CancellationToken cancellationToken = default);
    Task<Result<Unit, string>> CancelAsync(CancellationToken cancellationToken = default);
    Task<Result<Unit, string>> StartBackgroundSchedulingAsync(string accountId, string scopeId, string rootPath, TimeSpan interval, CancellationToken cancellationToken = default);
    Task<Result<Unit, string>> StopBackgroundSchedulingAsync(CancellationToken cancellationToken = default);
}
