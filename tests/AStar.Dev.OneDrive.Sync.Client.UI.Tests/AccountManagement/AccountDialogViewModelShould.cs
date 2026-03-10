using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using AStar.Dev.OneDrive.Sync.Client.UI.AccountManagement;
using AStar.Dev.Utilities;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.AccountManagement;

public sealed class AccountDialogViewModelShould
{
    [Fact]
    public void InitializeWithEmptyFieldsForNewAccount()
    {
        var viewModel = new AccountDialogViewModel();

        viewModel.Email.ShouldBeEmpty();
        viewModel.QuotaBytes.ShouldBe(0);
        viewModel.UsedBytes.ShouldBe(0);
        viewModel.IsEditMode.ShouldBeFalse();
    }

    [Fact]
    public void InitializeWithPopulatedFieldsForEditAccount()
    {
        var account = new AccountInfo("acct-1", "existing@example.com", 1200, 250);
        var viewModel = new AccountDialogViewModel(account);

        viewModel.Email.ShouldBe("existing@example.com");
        viewModel.QuotaBytes.ShouldBe(1200);
        viewModel.UsedBytes.ShouldBe(250);
        viewModel.IsEditMode.ShouldBeTrue();
    }

    [Fact]
    public void CloseWithoutSavingWhenCancelCommandExecutes()
    {
        var viewModel = new AccountDialogViewModel
        {
            Email = "user@example.com"
        };

        viewModel.CancelCommand.Execute(null);

        viewModel.IsCancelled.ShouldBeTrue();
        viewModel.IsSaved.ShouldBeFalse();
    }

    [Fact]
    public async Task LinkAccountAndPersistWhenLoginCommandExecutes()
    {
        var databasePath = CreateDatabasePath();
        var service = new TestAccountSessionService(new AccountSessionState(
            new OneDriveAccountProfile("acct-1", "linked@example.com", 8192, 1024),
            new AccountSessionMetadata("acct-1", DateTime.UtcNow.AddMinutes(30), DateTime.UtcNow, null, false)));
        var closeRequested = false;
        var viewModel = new AccountDialogViewModel(databasePath, service)
        {
            Email = "linked@example.com"
        };
        viewModel.CloseRequested += (_, saved) => closeRequested = saved;

        viewModel.LoginCommand.Execute(null);
        await WaitForConditionAsync(() => viewModel.IsSaved);
        var listViewModel = new AccountListViewModel(databasePath);
        _ = await listViewModel.LoadAccountsAsync(TestContext.Current.CancellationToken);

        viewModel.LoginTriggered.ShouldBeTrue();
        viewModel.IsSaved.ShouldBeTrue();
        closeRequested.ShouldBeTrue();
        viewModel.Email.ShouldBe("linked@example.com");
        viewModel.QuotaBytes.ShouldBe(8192);
        viewModel.UsedBytes.ShouldBe(1024);
        listViewModel.Accounts.Count.ShouldBe(1);
        listViewModel.Accounts[0].Id.ShouldBe("acct-1");
        service.LinkCalls.ShouldBe(1);
    }

    [Fact]
    public async Task DisableCancelWhenLoginSuccessfullyPersistsAccount()
    {
        var service = new TestAccountSessionService(new AccountSessionState(
            new OneDriveAccountProfile("acct-2", "cancel-lock@example.com", 1024, 256),
            new AccountSessionMetadata("acct-2", DateTime.UtcNow.AddMinutes(30), DateTime.UtcNow, null, false)));
        var viewModel = new AccountDialogViewModel(accountSessionService: service)
        {
            Email = "cancel-lock@example.com"
        };

        viewModel.LoginCommand.Execute(null);
        await WaitForConditionAsync(() => viewModel.IsSaved);

        viewModel.IsSaved.ShouldBeTrue();
        viewModel.CancelCommand.CanExecute(null).ShouldBeFalse();
    }

    [Fact]
    public async Task ShowValidationErrorAndKeepDialogOpenWhenLoginFails()
    {
        var service = new TestAccountSessionService(error: "duplicate key");
        var closeRequested = false;
        var viewModel = new AccountDialogViewModel(accountSessionService: service)
        {
            Email = "broken@example.com"
        };
        viewModel.CloseRequested += (_, saved) => closeRequested = saved;

        viewModel.LoginCommand.Execute(null);
        await WaitForConditionAsync(() => !string.IsNullOrWhiteSpace(viewModel.ValidationError));

        viewModel.IsSaved.ShouldBeFalse();
        viewModel.ValidationError.ShouldContain("duplicate key");
        closeRequested.ShouldBeFalse();
    }

    [Fact]
    public async Task PersistConfiguredLocalSyncRootWhenLoginCommandExecutes()
    {
        var databasePath = CreateDatabasePath();
        var service = new TestAccountSessionService(new AccountSessionState(
            new OneDriveAccountProfile("acct-root-1", "rooted@example.com", 4096, 64),
            new AccountSessionMetadata("acct-root-1", DateTime.UtcNow.AddMinutes(30), DateTime.UtcNow, null, false)));
        var viewModel = new AccountDialogViewModel(databasePath, service)
        {
            Email = "rooted@example.com",
            LocalSyncRootPath = "/tmp/astar-sync/rooted"
        };

        viewModel.LoginCommand.Execute(null);
        await WaitForConditionAsync(() => viewModel.IsSaved);

        var loaded = new AccountListViewModel(databasePath);
        _ = await loaded.LoadAccountsAsync(TestContext.Current.CancellationToken);

        loaded.Accounts.Count.ShouldBe(1);
        loaded.Accounts[0].LocalSyncRootPath.ShouldBe("/tmp/astar-sync/rooted");
    }

    [Fact]
    public async Task RejectOverlappingLocalSyncRootWhenLoginCommandExecutes()
    {
        var databasePath = CreateDatabasePath();
        var seed = new AccountListViewModel(databasePath);
        seed.Accounts.Add(new AccountInfo("acct-a", "alpha@example.com", 1000, 100, "/tmp/astar-sync/alpha"));
        Result<bool, Exception> seedSave = await seed.SaveAccountsAsync(TestContext.Current.CancellationToken);
        Pattern.IsSuccess(seedSave).ShouldBeTrue();

        var service = new TestAccountSessionService(new AccountSessionState(
            new OneDriveAccountProfile("acct-b", "beta@example.com", 2000, 200),
            new AccountSessionMetadata("acct-b", DateTime.UtcNow.AddMinutes(30), DateTime.UtcNow, null, false)));
        var viewModel = new AccountDialogViewModel(databasePath, service)
        {
            Email = "beta@example.com",
            LocalSyncRootPath = "/tmp/astar-sync/alpha/sub"
        };

        viewModel.LoginCommand.Execute(null);
        await WaitForConditionAsync(() => !string.IsNullOrWhiteSpace(viewModel.ValidationError));

        viewModel.IsSaved.ShouldBeFalse();
        viewModel.ValidationError.ToLowerInvariant().ShouldContain("overlap");
    }

    private static string CreateDatabasePath()
        => Path.GetTempPath().CombinePath($"astar-ui-account-dialog-tests-{Guid.NewGuid():N}", "astar-onedrive.db");

    private static async Task WaitForConditionAsync(Func<bool> condition)
    {
        for(var attempt = 0; attempt < 50 && !condition(); attempt++)
        {
            if(!condition())
            {
                await Task.Delay(10, TestContext.Current.CancellationToken);
            }
        }
    }

    private sealed class TestAccountSessionService : IAccountSessionService
    {
        private readonly AccountSessionState? _state;
        private readonly string? _error;

        public TestAccountSessionService(AccountSessionState state)
            => _state = state;

        public TestAccountSessionService(string error)
            => _error = error;

        public int LinkCalls { get; private set; }

        public Task<Result<AccountSessionState, string>> LinkAccountAsync(string emailHint, CancellationToken cancellationToken = default)
        {
            LinkCalls++;
            return _error is null
                ? Task.FromResult<Result<AccountSessionState, string>>(_state!)
                : Task.FromResult<Result<AccountSessionState, string>>(_error);
        }

        public Task<Result<AccountSessionState, string>> ReauthenticateAsync(string accountId, string emailHint, CancellationToken cancellationToken = default)
            => _error is null
                ? Task.FromResult<Result<AccountSessionState, string>>(_state!)
                : Task.FromResult<Result<AccountSessionState, string>>(_error);

        public Task<Result<AccountSessionState, string>> GetValidSessionAsync(string accountId, CancellationToken cancellationToken = default)
            => _error is null
                ? Task.FromResult<Result<AccountSessionState, string>>(_state!)
                : Task.FromResult<Result<AccountSessionState, string>>(_error);

        public Task<Result<Unit, string>> UnlinkAccountAsync(string accountId, CancellationToken cancellationToken = default)
            => Task.FromResult<Result<Unit, string>>(Unit.Value);
    }
}
