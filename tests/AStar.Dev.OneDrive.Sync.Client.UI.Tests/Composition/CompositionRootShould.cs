using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Services;
using AStar.Dev.OneDrive.Sync.Client.Domain.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Repositories;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Repositories;
using AStar.Dev.OneDrive.Sync.Client.UI.Composition;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.Composition;

public sealed class CompositionRootShould
{
    [Fact]
    public void ResolveSyncServiceAsRealImplementation()
    {
        ISyncService service = CompositionRoot.Resolve<ISyncService>();

        _ = service.ShouldBeOfType<SyncService>();
    }

    [Fact]
    public void ResolveSyncFileRepositoryAsOneDriveStub()
    {
        ISyncFileRepository repository = CompositionRoot.Resolve<ISyncFileRepository>();

        _ = repository.ShouldBeOfType<OneDriveSyncFileRepository>();
    }

    [Fact]
    public void ResolveDbContextAndSqliteRepositories()
    {
        using AstarOneDriveDbContextModel dbContext = CompositionRoot.Resolve<AstarOneDriveDbContextModel>();
        SqliteSettingsRepository settings = CompositionRoot.Resolve<SqliteSettingsRepository>();
        SqliteAccountsRepository accounts = CompositionRoot.Resolve<SqliteAccountsRepository>();
        SqliteFolderTreeRepository folderTree = CompositionRoot.Resolve<SqliteFolderTreeRepository>();

        dbContext.ShouldNotBeNull();
        settings.ShouldNotBeNull();
        accounts.ShouldNotBeNull();
        folderTree.ShouldNotBeNull();
    }
}