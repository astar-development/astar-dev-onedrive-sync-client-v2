using Microsoft.EntityFrameworkCore;

namespace AstarOneDrive.Infrastructure.Data;

/// <summary>
/// Applies EF Core migrations for the SQLite database on startup.
/// </summary>
public sealed class SqliteDatabaseMigrator(string? databasePath = null)
{
    private readonly string _databasePath = string.IsNullOrWhiteSpace(databasePath)
        ? DatabasePathResolver.ResolveDatabasePath()
        : databasePath;

    /// <summary>
    /// Ensures all pending database migrations have been applied asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task EnsureMigratedAsync(CancellationToken cancellationToken = default)
    {
        await using var context = AstarOneDriveDbContextFactory.Create(_databasePath);
        await context.Database.MigrateAsync(cancellationToken);
    }

    /// <summary>
    /// Ensures all pending database migrations have been applied synchronously.
    /// </summary>
    public void EnsureMigrated()
    {
        using var context = AstarOneDriveDbContextFactory.Create(_databasePath);
        context.Database.Migrate();
    }
}
