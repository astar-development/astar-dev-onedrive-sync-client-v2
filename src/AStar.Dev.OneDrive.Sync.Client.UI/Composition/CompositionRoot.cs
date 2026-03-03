using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Services;
using AStar.Dev.OneDrive.Sync.Client.Domain.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Repositories;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Repositories;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Composition;

/// <summary>
/// Provides a simple service locator for wiring Application and Infrastructure dependencies.
/// </summary>
public static class CompositionRoot
{
    private static readonly Dictionary<Type, Func<object>> Registrations = [];
    private static bool _isInitialized;

    /// <summary>
    /// Initializes service registrations.
    /// </summary>
    /// <param name="databasePath">Optional custom SQLite database path.</param>
    public static void Initialize(string? databasePath = null)
    {
        Registrations.Clear();

        Registrations[typeof(ISyncFileRepository)] = () => new OneDriveSyncFileRepository();
        Registrations[typeof(ISyncService)] = () => new SyncService(Resolve<ISyncFileRepository>());
        Registrations[typeof(AstarOneDriveDbContextModel)] = () => AstarOneDriveDbContextFactory.Create(databasePath);
        Registrations[typeof(SqliteSettingsRepository)] = () => new SqliteSettingsRepository(databasePath);
        Registrations[typeof(SqliteAccountsRepository)] = () => new SqliteAccountsRepository(databasePath);
        Registrations[typeof(SqliteFolderTreeRepository)] = () => new SqliteFolderTreeRepository(databasePath);

        _isInitialized = true;
    }

    /// <summary>
    /// Resolves a service instance of the requested type.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <returns>The resolved service instance.</returns>
    public static T Resolve<T>()
        where T : notnull
    {
        EnsureInitialized();
        return Registrations.TryGetValue(typeof(T), out Func<object>? factory)
            ? (T)factory()
            : throw new InvalidOperationException($"No registration found for type {typeof(T).FullName}.");
    }

    private static void EnsureInitialized()
    {
        if (!_isInitialized)
        {
            Initialize();
        }
    }
}