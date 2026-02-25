using Microsoft.EntityFrameworkCore;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data;

public sealed class SqliteDatabaseMigrator(string? databasePath = null)
{
    private readonly string _databasePath = string.IsNullOrWhiteSpace(databasePath)
        ? DatabasePathResolver.ResolveDatabasePath()
        : databasePath;

    public async Task EnsureMigratedAsync(CancellationToken cancellationToken = default)
    {
        await using AStar.Dev.OneDrive.Sync.ClientDbContext context = AStar.Dev.OneDrive.Sync.ClientDbContextFactory.Create(_databasePath);
        await context.Database.MigrateAsync(cancellationToken);
    }

    public void EnsureMigrated()
    {
        using AStar.Dev.OneDrive.Sync.ClientDbContext context = AStar.Dev.OneDrive.Sync.ClientDbContextFactory.Create(_databasePath);
        context.Database.Migrate();
    }
}
