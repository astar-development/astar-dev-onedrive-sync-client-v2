using Microsoft.EntityFrameworkCore;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data;

public static class AstarOneDriveDbContextFactory
{
    public static AstarOneDriveDbContextModel Create(string? databasePath = null)
    {
        var resolvedPath = string.IsNullOrWhiteSpace(databasePath)
            ? DatabasePathResolver.ResolveDatabasePath()
            : databasePath;

        var dbDirectory = Path.GetDirectoryName(resolvedPath);
        if(!string.IsNullOrWhiteSpace(dbDirectory))
        {
            _ = Directory.CreateDirectory(dbDirectory);
        }

        var optionsBuilder = new DbContextOptionsBuilder<AstarOneDriveDbContextModel>();
        _ = optionsBuilder.UseSqlite($"Data Source={resolvedPath}");

        return new AstarOneDriveDbContextModel(optionsBuilder.Options);
    }
}
