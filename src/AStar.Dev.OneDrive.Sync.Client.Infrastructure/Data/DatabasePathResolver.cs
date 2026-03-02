using AStar.Dev.Utilities;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data;

/// <summary>
/// Resolves the platform-specific database file path for the application.
/// </summary>
public static class DatabasePathResolver
{
    private const string AppFolderName = "AStar.Dev.OneDrive.Sync.Client";
    private const string DatabaseName = "astar-onedrive.db";
    private const string XdgDataHomeEnvironmentVariable = "XDG_DATA_HOME";

    /// <summary>
    /// Resolves the full database file path, creating the directory if it doesn't exist.
    /// </summary>
    /// <returns>The absolute path to the database file.</returns>
    public static string ResolveDatabasePath()
    {
        var applicationDataBasePath = ResolvePlatformSpecificDataDirectory().CombinePath(AppFolderName);
        _ = Directory.CreateDirectory(applicationDataBasePath);
        
        return applicationDataBasePath.CombinePath(DatabaseName);
    }

    private static string ResolvePlatformSpecificDataDirectory()
    {
        if (OperatingSystem.IsWindows())
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        if (OperatingSystem.IsMacOS())
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).CombinePath("Library", "Application Support");
        }

        var xdgDataHome = Environment.GetEnvironmentVariable(XdgDataHomeEnvironmentVariable);

        return !string.IsNullOrWhiteSpace(xdgDataHome)
            ? xdgDataHome
            : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).CombinePath(".local", "share");
    }
}
