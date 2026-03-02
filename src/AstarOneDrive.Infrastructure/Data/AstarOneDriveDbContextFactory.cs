using Microsoft.EntityFrameworkCore;

namespace AstarOneDrive.Infrastructure.Data;

/// <summary>
/// Factory for creating configured instances of the OneDrive database context.
/// </summary>
public static class AstarOneDriveDbContextFactory
{
    /// <summary>
    /// Creates a new database context instance configured for SQLite.
    /// </summary>
    /// <param name="databasePath">Optional custom database path. If null, uses the default platform-specific path.</param>
    /// <returns>A configured database context instance.</returns>
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
