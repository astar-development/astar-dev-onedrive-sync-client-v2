using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Repositories;
using AStar.Dev.Utilities;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Tests.Data;

public sealed class LocalInventoryPersistenceShould
{
    [Fact]
    public async Task PersistAndRestoreInventoryItemsPerAccount()
    {
        var databasePath = Path.GetTempPath().CombinePath($"astar-local-inventory-tests-{Guid.NewGuid():N}", "astar-onedrive.db");
        var migrator = new SqliteDatabaseMigrator(databasePath);
        await migrator.EnsureMigratedAsync(TestContext.Current.CancellationToken);
        var repository = new SqliteLocalInventoryStore(databasePath);
        var accountA = "acct-a";
        var accountB = "acct-b";
        IReadOnlyList<LocalInventoryItem> itemsA =
        [
            new("/sync/a.txt", "a.txt", 11, DateTime.UnixEpoch, "fpa", "metadata-sha256-v1")
        ];
        IReadOnlyList<LocalInventoryItem> itemsB =
        [
            new("/sync/b.txt", "b.txt", 22, DateTime.UnixEpoch.AddMinutes(1), "fpb", "metadata-sha256-v1")
        ];

        Result<Unit, string> saveA = await repository.SaveAsync(accountA, itemsA, TestContext.Current.CancellationToken);
        Result<Unit, string> saveB = await repository.SaveAsync(accountB, itemsB, TestContext.Current.CancellationToken);
        Result<IReadOnlyList<LocalInventoryItem>, string> loadA = await repository.LoadAsync(accountA, TestContext.Current.CancellationToken);
        Result<IReadOnlyList<LocalInventoryItem>, string> loadB = await repository.LoadAsync(accountB, TestContext.Current.CancellationToken);

        saveA.ShouldBeOfType<Result<Unit, string>.Ok>();
        saveB.ShouldBeOfType<Result<Unit, string>.Ok>();
        loadA.ShouldBeOfType<Result<IReadOnlyList<LocalInventoryItem>, string>.Ok>().Value.Single().RelativePath.ShouldBe("a.txt");
        loadB.ShouldBeOfType<Result<IReadOnlyList<LocalInventoryItem>, string>.Ok>().Value.Single().RelativePath.ShouldBe("b.txt");
    }
}