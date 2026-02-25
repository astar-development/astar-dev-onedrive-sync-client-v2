using Microsoft.EntityFrameworkCore;

namespace AstarOneDrive.Infrastructure.Data;

public static class AstarOneDriveDbContextFactory
{
    public static AstarOneDriveDbContext Create(string? databasePath = null)
    {
        var resolvedPath = string.IsNullOrWhiteSpace(databasePath)
            ? DatabasePathResolver.ResolveDatabasePath()
            : databasePath;

        var dbDirectory = Path.GetDirectoryName(resolvedPath);
        if (!string.IsNullOrWhiteSpace(dbDirectory))
        {
            Directory.CreateDirectory(dbDirectory);
        }

        var optionsBuilder = new DbContextOptionsBuilder<AstarOneDriveDbContext>();
        optionsBuilder.UseSqlite($"Data Source={resolvedPath}");

        return new AstarOneDriveDbContext(optionsBuilder.Options);
    }
}
