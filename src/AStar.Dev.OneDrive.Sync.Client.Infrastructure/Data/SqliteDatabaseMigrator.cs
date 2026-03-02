using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data;

/// <summary>
/// Applies EF Core migrations for the SQLite database on startup.
/// </summary>
public sealed class SqliteDatabaseMigrator(string? databasePath = null) : IMigrationService
{
    private readonly string _databasePath = string.IsNullOrWhiteSpace(databasePath)
        ? DatabasePathResolver.ResolveDatabasePath()
        : databasePath;

    public async Task EnsureMigratedAsync(CancellationToken cancellationToken = default)
    {
        await using AstarOneDriveDbContextModel context = AstarOneDriveDbContextFactory.Create(_databasePath);
        await context.Database.MigrateAsync(cancellationToken);
    }

    public void EnsureMigrated()
    {
        using AstarOneDriveDbContextModel context = AstarOneDriveDbContextFactory.Create(_databasePath);
        context.Database.Migrate();
    }
}
