using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Repositories;
using AStar.Dev.Utilities;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Tests.Data;

public sealed class SqliteRemoteDeltaApplierShould
{
    [Fact]
    public async Task UpsertAndDeleteSyncFilesFromRemoteDeltaItems()
    {
        var databasePath = Path.GetTempPath().CombinePath($"astar-delta-applier-tests-{Guid.NewGuid():N}", "astar-onedrive.db");
        var migrator = new SqliteDatabaseMigrator(databasePath);
        await migrator.EnsureMigratedAsync(TestContext.Current.CancellationToken);
        var sut = new SqliteRemoteDeltaApplier(databasePath);
        var accountId = "acct-delta";
        IReadOnlyList<RemoteDeltaItem> initialChanges =
        [
            new RemoteDeltaItem("file-1", "/Docs/a.txt", RemoteDeltaChangeKind.Created),
            new RemoteDeltaItem("file-2", "/Docs/b.txt", RemoteDeltaChangeKind.Updated)
        ];

        Result<Unit, string> firstApply = await sut.ApplyAsync(accountId, "drive-root", initialChanges, TestContext.Current.CancellationToken);
        firstApply.ShouldBeOfType<Result<Unit, string>.Ok>();

        await using(AstarOneDriveDbContextModel context = AstarOneDriveDbContextFactory.Create(databasePath))
        {
            context.SyncFiles.Count(x => x.AccountId == accountId).ShouldBe(2);
            context.SyncFiles.Single(x => x.AccountId == accountId && x.Id == "file-1").RemotePath.ShouldBe("/Docs/a.txt");
        }

        IReadOnlyList<RemoteDeltaItem> secondChanges =
        [
            new RemoteDeltaItem("file-1", "/Docs/a-renamed.txt", RemoteDeltaChangeKind.Updated),
            new RemoteDeltaItem("file-2", "/Docs/b.txt", RemoteDeltaChangeKind.Deleted)
        ];

        Result<Unit, string> secondApply = await sut.ApplyAsync(accountId, "drive-root", secondChanges, TestContext.Current.CancellationToken);
        secondApply.ShouldBeOfType<Result<Unit, string>.Ok>();

        await using(AstarOneDriveDbContextModel context = AstarOneDriveDbContextFactory.Create(databasePath))
        {
            context.SyncFiles.Count(x => x.AccountId == accountId).ShouldBe(1);
            context.SyncFiles.Single(x => x.AccountId == accountId && x.Id == "file-1").RemotePath.ShouldBe("/Docs/a-renamed.txt");
        }
    }
}