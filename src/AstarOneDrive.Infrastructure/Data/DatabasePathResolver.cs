namespace AstarOneDrive.Infrastructure.Data;

public static class DatabasePathResolver
{
    private const string AppFolderName = "AstarOneDrive";
    private const string DatabaseName = "astar-onedrive.db";

    public static string ResolveDatabasePath()
    {
        var basePath = ResolvePlatformDataDirectory();
        var appPath = Path.Combine(basePath, AppFolderName);
        Directory.CreateDirectory(appPath);
        return Path.Combine(appPath, DatabaseName);
    }

    private static string ResolvePlatformDataDirectory()
    {
        if (OperatingSystem.IsWindows())
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        if (OperatingSystem.IsMacOS())
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(userProfile, "Library", "Application Support");
        }

        var xdgDataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
        if (!string.IsNullOrWhiteSpace(xdgDataHome))
        {
            return xdgDataHome;
        }

        var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(homePath, ".local", "share");
    }
}
