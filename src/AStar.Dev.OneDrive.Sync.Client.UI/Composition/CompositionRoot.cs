using System.Net.Http;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Services;
using AStar.Dev.OneDrive.Sync.Client.Domain.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Authentication;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Repositories;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Graph;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Repositories;
using AStar.Dev.OneDrive.Sync.Client.UI.SyncStatus;

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

        var secureStorePath = string.IsNullOrWhiteSpace(databasePath)
            ? null
            : Path.Combine(Path.GetDirectoryName(databasePath) ?? string.Empty, "secure-store");

        Registrations[typeof(ISyncFileRepository)] = () => new OneDriveSyncFileRepository();
        Registrations[typeof(ISyncDiagnosticsSink)] = () => new SyncDiagnosticsSink();
        Registrations[typeof(ISyncService)] = () => new SyncService(Resolve<ISyncFileRepository>(), diagnosticsSink: Resolve<ISyncDiagnosticsSink>());
        Registrations[typeof(ILocalFileScanner)] = () => new FileSystemLocalFileScanner();
        Registrations[typeof(ILocalInventoryStore)] = () => new SqliteLocalInventoryStore(databasePath);
        Registrations[typeof(ILocalInventoryService)] = () => new LocalInventoryService(Resolve<ILocalFileScanner>(), Resolve<ILocalInventoryStore>());
        Registrations[typeof(IDeltaCheckpointStore)] = () => new SqliteDeltaCheckpointStore(databasePath);
        Registrations[typeof(IOneDriveGraphClient)] = () =>
        {
            var options = new OneDriveGraphClientOptions();
            var httpClient = new HttpClient { BaseAddress = options.BaseUri, Timeout = options.RequestTimeout };
            return new OneDriveGraphClient(httpClient, new NoOpOneDriveGraphTelemetry());
        };
        Registrations[typeof(ISecureAccountTokenStore)] = () => new FileBackedSecureAccountTokenStore(secureStorePath);
        Registrations[typeof(SqliteAccountSessionMetadataRepository)] = () => new SqliteAccountSessionMetadataRepository(databasePath);
        Registrations[typeof(IAccountSessionService)] = () => new OneDriveAccountSessionService(
            new OneDriveAuthenticationAdapter(),
            Resolve<ISecureAccountTokenStore>(),
            Resolve<SqliteAccountSessionMetadataRepository>());
        Registrations[typeof(IRemoteDeltaSource)] = () => new OneDriveRemoteDeltaSource(
            Resolve<IOneDriveGraphClient>(),
            Resolve<IAccountSessionService>(),
            Resolve<ISecureAccountTokenStore>());
        Registrations[typeof(IRemoteDeltaApplier)] = () => new SqliteRemoteDeltaApplier(databasePath);
        Registrations[typeof(IDeltaSyncService)] = () => new DeltaSyncService(Resolve<IRemoteDeltaSource>(), Resolve<IDeltaCheckpointStore>(), Resolve<IRemoteDeltaApplier>());
        Registrations[typeof(ISyncRunStateStore)] = () => new InMemorySyncRunStateStore();
        Registrations[typeof(ISyncOrchestratorService)] = () => new SyncOrchestratorService(
            Resolve<ILocalInventoryService>(),
            Resolve<IDeltaSyncService>(),
            Resolve<ISyncService>(),
            Resolve<ISyncRunStateStore>(),
            Resolve<ISyncDiagnosticsSink>());
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
        if(!_isInitialized)
        {
            Initialize();
        }
    }
}
