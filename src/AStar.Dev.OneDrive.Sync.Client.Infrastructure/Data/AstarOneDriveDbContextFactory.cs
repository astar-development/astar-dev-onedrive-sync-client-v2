using Microsoft.EntityFrameworkCore;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data;

public static class ClientDbContextFactory
{
    public static ClientDbContext Create(string? databasePath = null)
    {
        var resolvedPath = string.IsNullOrWhiteSpace(databasePath)
            ? DatabasePathResolver.ResolveDatabasePath()
            : databasePath;

        var dbDirectory = Path.GetDirectoryName(resolvedPath);
        if(!string.IsNullOrWhiteSpace(dbDirectory))
        {
            _ = Directory.CreateDirectory(dbDirectory);
        }

        var optionsBuilder = new DbContextOptionsBuilder<ClientDbContext>();
        _ = optionsBuilder.UseSqlite($"Data Source={resolvedPath}");

        return new ClientDbContext(optionsBuilder.Options);
    }
}
