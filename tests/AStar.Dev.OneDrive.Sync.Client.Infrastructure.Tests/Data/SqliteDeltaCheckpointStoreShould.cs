using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Repositories;
using AStar.Dev.Utilities;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Tests.Data;

public sealed class SqliteDeltaCheckpointStoreShould
{
    [Fact]
    public async Task PersistCheckpointPerAccountAndScope()
    {
        var databasePath = Path.GetTempPath().CombinePath($"astar-delta-checkpoints-tests-{Guid.NewGuid():N}", "astar-onedrive.db");
        var migrator = new SqliteDatabaseMigrator(databasePath);
        await migrator.EnsureMigratedAsync(TestContext.Current.CancellationToken);
        var repository = new SqliteDeltaCheckpointStore(databasePath);
        var accountA = "acct-a";
        var accountB = "acct-b";
        var scope = "drive-root";

        Result<Unit, string> saveA = await repository.SaveAsync(new SyncCheckpoint(accountA, scope, "cursor-a", DateTime.UtcNow), TestContext.Current.CancellationToken);
        Result<Unit, string> saveB = await repository.SaveAsync(new SyncCheckpoint(accountB, scope, "cursor-b", DateTime.UtcNow), TestContext.Current.CancellationToken);
        Result<Option<SyncCheckpoint>, string> loadA = await repository.LoadAsync(accountA, scope, TestContext.Current.CancellationToken);
        Result<Option<SyncCheckpoint>, string> loadB = await repository.LoadAsync(accountB, scope, TestContext.Current.CancellationToken);

        saveA.ShouldBeOfType<Result<Unit, string>.Ok>();
        saveB.ShouldBeOfType<Result<Unit, string>.Ok>();
        loadA.ShouldBeOfType<Result<Option<SyncCheckpoint>, string>.Ok>().Value.ShouldBeOfType<Option<SyncCheckpoint>.Some>().Value.DeltaToken.ShouldBe("cursor-a");
        loadB.ShouldBeOfType<Result<Option<SyncCheckpoint>, string>.Ok>().Value.ShouldBeOfType<Option<SyncCheckpoint>.Some>().Value.DeltaToken.ShouldBe("cursor-b");
    }
}