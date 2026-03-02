namespace AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;

/// <summary>
/// Defines the contract for applying database migrations on application startup.
/// </summary>
public interface IMigrationService
{
    /// <summary>
    /// Ensures all pending database migrations have been applied.
    /// </summary>
    void EnsureMigrated();
}
