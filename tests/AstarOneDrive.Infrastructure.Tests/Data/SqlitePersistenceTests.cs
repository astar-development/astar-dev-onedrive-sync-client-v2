using AStar.Dev.Utilities;
using AstarOneDrive.Infrastructure.Data;
using AstarOneDrive.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AstarOneDrive.Infrastructure.Tests.Data;

public sealed class SqlitePersistenceTests
{
    [Fact]
    public void ResolveDatabasePath_UsesPlatformSpecificLocation()
    {
        var path = DatabasePathResolver.ResolveDatabasePath();

        path.ShouldEndWith("AstarOneDrive".CombinePath("astar-onedrive.db"));
        Path.IsPathRooted(path).ShouldBeTrue();
    }

    [Fact]
    public async Task EnsureMigratedAsync_AppliesPendingMigrations()
    {
        var databasePath = Path.GetTempPath().CombinePath($"astar-onedrive-tests-{Guid.NewGuid():N}", "astar-onedrive.db");
        var migrator = new SqliteDatabaseMigrator(databasePath);

        await migrator.EnsureMigratedAsync(TestContext.Current.CancellationToken);

        await using var context = AstarOneDriveDbContextFactory.Create(databasePath);
        var pending = await context.Database.GetPendingMigrationsAsync(TestContext.Current.CancellationToken);
        pending.ShouldBeEmpty();
    }

    [Fact]
    public async Task Context_AllowsInsertAndQuery_ForSettingsAccountsAndSyncFiles()
    {
        var databasePath = Path.GetTempPath().CombinePath($"astar-onedrive-tests-{Guid.NewGuid():N}", "astar-onedrive.db");
        var migrator = new SqliteDatabaseMigrator(databasePath);
        await migrator.EnsureMigratedAsync(TestContext.Current.CancellationToken);

        await using var context = AstarOneDriveDbContextFactory.Create(databasePath);
        var account = new AccountEntity
        {
            Id = "acct-1",
            Email = "user@example.com",
            QuotaBytes = 1024,
            UsedBytes = 128,
            IsActive = true,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        };
        context.Accounts.Add(account);
        context.Settings.Add(new SettingEntity
        {
            Id = Guid.NewGuid().ToString(),
            Key = "SelectedTheme",
            Value = "Dark",
            UpdatedUtc = DateTime.UtcNow
        });
        context.SyncFiles.Add(new SyncFileEntity
        {
            Id = Guid.NewGuid().ToString(),
            AccountId = account.Id,
            Name = "root",
            LocalPath = "/tmp/root",
            RemotePath = "/root",
            ItemType = "Folder",
            IsSelected = true,
            IsExpanded = true,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        });

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        context.Settings.Count().ShouldBe(1);
        context.Accounts.Count().ShouldBe(1);
        context.SyncFiles.Count().ShouldBe(1);
    }

    [Fact]
    public async Task Context_RejectsNullForRequiredFields()
    {
        var databasePath = Path.GetTempPath().CombinePath($"astar-onedrive-tests-{Guid.NewGuid():N}", "astar-onedrive.db");
        var migrator = new SqliteDatabaseMigrator(databasePath);
        await migrator.EnsureMigratedAsync(TestContext.Current.CancellationToken);

        await using var context = AstarOneDriveDbContextFactory.Create(databasePath);
        context.Accounts.Add(new AccountEntity
        {
            Id = "acct-null",
            Email = null!,
            QuotaBytes = 1,
            UsedBytes = 0,
            IsActive = true,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        });

        await Should.ThrowAsync<DbUpdateException>(() => context.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Context_EnforcesConfiguredMaxLength()
    {
        var databasePath = Path.GetTempPath().CombinePath($"astar-onedrive-tests-{Guid.NewGuid():N}", "astar-onedrive.db");
        var migrator = new SqliteDatabaseMigrator(databasePath);
        await migrator.EnsureMigratedAsync(TestContext.Current.CancellationToken);

        await using var context = AstarOneDriveDbContextFactory.Create(databasePath);
        var account = new AccountEntity
        {
            Id = "acct-2",
            Email = "user2@example.com",
            QuotaBytes = 2048,
            UsedBytes = 0,
            IsActive = true,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        };
        context.Accounts.Add(account);
        context.SyncFiles.Add(new SyncFileEntity
        {
            Id = Guid.NewGuid().ToString(),
            AccountId = account.Id,
            Name = new string('x', 261),
            LocalPath = "/tmp/too-long",
            RemotePath = "/too-long",
            ItemType = "File",
            IsSelected = false,
            IsExpanded = false,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        });

        await Should.ThrowAsync<DbUpdateException>(() => context.SaveChangesAsync(TestContext.Current.CancellationToken));
    }
}