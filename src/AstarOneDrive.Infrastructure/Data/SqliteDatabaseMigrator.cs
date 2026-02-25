using Microsoft.EntityFrameworkCore;

namespace AstarOneDrive.Infrastructure.Data;

public sealed class SqliteDatabaseMigrator(string? databasePath = null)
{
    private readonly string _databasePath = string.IsNullOrWhiteSpace(databasePath)
        ? DatabasePathResolver.ResolveDatabasePath()
        : databasePath;

    public async Task EnsureMigratedAsync(CancellationToken cancellationToken = default)
    {
        await using var context = AstarOneDriveDbContextFactory.Create(_databasePath);
        await context.Database.MigrateAsync(cancellationToken);
    }

    public void EnsureMigrated()
    {
        using var context = AstarOneDriveDbContextFactory.Create(_databasePath);
        context.Database.Migrate();
    }
}
