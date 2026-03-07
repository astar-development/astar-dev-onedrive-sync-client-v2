using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Services;
using AStar.Dev.OneDrive.Sync.Client.Domain.Entities;
using AStar.Dev.OneDrive.Sync.Client.Domain.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Repositories;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Repositories;
using AStar.Dev.OneDrive.Sync.Client.UI.Composition;
using AStar.Dev.Utilities;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.Composition;

public sealed class CompositionRootShould : IDisposable
{
    private readonly string _databasePath = string.Empty;

    public CompositionRootShould() => _databasePath = CreateDatabasePath();

    [Fact]
    public void ResolveSyncServiceAsRealImplementationWhenCompositionIsInitialized()
    {
        CompositionRoot.Initialize(_databasePath);

        ISyncService service = CompositionRoot.Resolve<ISyncService>();

        _ = service.ShouldBeOfType<SyncService>();
    }

    [Fact]
    public async Task ResolveSyncFileRepositoryAsOneDriveStubWhenCompositionIsInitialized()
    {
        CompositionRoot.Initialize(_databasePath);

        ISyncFileRepository repository = CompositionRoot.Resolve<ISyncFileRepository>();
        Result<IReadOnlyList<SyncFile>, string> result = await repository.GetAllAsync(TestContext.Current.CancellationToken);

        _ = repository.ShouldBeOfType<OneDriveSyncFileRepository>();
        result.ShouldBeOfType<Result<IReadOnlyList<SyncFile>, string>.Ok>()
            .Value.Count.ShouldBe(0);
    }

    [Fact]
    public void ResolveDbContextAndSqliteRepositoriesWhenCompositionIsInitialized()
    {
        CompositionRoot.Initialize(_databasePath);

        using AstarOneDriveDbContextModel dbContext = CompositionRoot.Resolve<AstarOneDriveDbContextModel>();
        SqliteSettingsRepository settings = CompositionRoot.Resolve<SqliteSettingsRepository>();
        SqliteAccountsRepository accounts = CompositionRoot.Resolve<SqliteAccountsRepository>();
        SqliteFolderTreeRepository folderTree = CompositionRoot.Resolve<SqliteFolderTreeRepository>();

        dbContext.ShouldNotBeNull();
        settings.ShouldNotBeNull();
        accounts.ShouldNotBeNull();
        folderTree.ShouldNotBeNull();
    }

    private static string CreateDatabasePath()
    {
        var databasePath = DatabasePathResolver.ResolveDatabasePath(Guid.CreateVersion7());
        var dbDirectory = Path.GetDirectoryName(databasePath) ?? string.Empty;
        var dbFileName = Path.GetFileName(databasePath);
        Directory.CreateDirectory(dbDirectory);
        
        var dbPath = dbDirectory.CombinePath(dbFileName);

        return dbPath;
    }

    public void Dispose()
    {
        if (!string.IsNullOrEmpty(_databasePath))
        {
            Directory.Delete(Path.GetDirectoryName(_databasePath) ?? string.Empty, true);
        }
    }
}
