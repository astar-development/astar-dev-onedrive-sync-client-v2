using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Services;
using AStar.Dev.OneDrive.Sync.Client.Domain.Entities;
using AStar.Dev.OneDrive.Sync.Client.Domain.Interfaces;
using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Repositories;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Repositories;
using AStar.Dev.OneDrive.Sync.Client.UI.Composition;
using AStar.Dev.Utilities;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.Composition;

public sealed class CompositionRootShould
{
    private string _databasePath = string.Empty;

    public CompositionRootShould() => _databasePath = CreateDatabasePath();

    [Fact]
    public void ResolveSyncServiceAsRealImplementationWhenCompositionIsInitialized()
    {
        try
        {
            CompositionRoot.Initialize(_databasePath);
    
            ISyncService service = CompositionRoot.Resolve<ISyncService>();
    
            _ = service.ShouldBeOfType<SyncService>();
        }
        finally
        {
            Directory.Delete(_databasePath);
        }
    }

    [Fact]
    public async Task ResolveSyncFileRepositoryAsOneDriveStubWhenCompositionIsInitialized()
    {
        try
        {
            CompositionRoot.Initialize(_databasePath);
    
            ISyncFileRepository repository = CompositionRoot.Resolve<ISyncFileRepository>();
            Result<IReadOnlyList<SyncFile>, string> result = await repository.GetAllAsync(TestContext.Current.CancellationToken);
    
            _ = repository.ShouldBeOfType<OneDriveSyncFileRepository>();
            result.ShouldBeOfType<Result<IReadOnlyList<SyncFile>, string>.Ok>()
                .Value.Count.ShouldBe(0);
        }
        finally
        {
            Directory.Delete(_databasePath);
        }
    }

    [Fact]
    public void ResolveDbContextAndSqliteRepositoriesWhenCompositionIsInitialized()
    {
        try
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
        finally
        {
            Directory.Delete(_databasePath);
        }
    }

    private static string CreateDatabasePath()
    {
        var dbPath = Path.GetTempPath().CombinePath("AStar.Dev.OneDrive.Sync.Client.UI.Tests", $"testing-{Guid.NewGuid():N}.db");

        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        return dbPath;
    }
}
