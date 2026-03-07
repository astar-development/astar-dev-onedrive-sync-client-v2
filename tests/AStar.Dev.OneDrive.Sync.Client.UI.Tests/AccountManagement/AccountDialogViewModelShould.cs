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
    public void RejectInvalidEmailWhenSaveCommandExecutes()
    {
        var viewModel = new AccountDialogViewModel
        {
            Email = "invalid-email"
        };

        viewModel.SaveCommand.Execute(null);

        viewModel.ValidationError.ShouldNotBeEmpty();
        viewModel.IsSaved.ShouldBeFalse();
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
    public async Task LinkAccountWhenLoginCommandExecutes()
    {
        var service = new TestAccountSessionService(new AccountSessionState(
            new OneDriveAccountProfile("acct-1", "linked@example.com", 8192, 1024),
            new AccountSessionMetadata("acct-1", DateTime.UtcNow.AddMinutes(30), DateTime.UtcNow, null, false)));
        var viewModel = new AccountDialogViewModel(accountSessionService: service)
        {
            Email = "linked@example.com"
        };

        viewModel.LoginCommand.Execute(null);
        await WaitForConditionAsync(() => viewModel.LoginTriggered);

        viewModel.LoginTriggered.ShouldBeTrue();
        viewModel.Email.ShouldBe("linked@example.com");
        viewModel.QuotaBytes.ShouldBe(8192);
        viewModel.UsedBytes.ShouldBe(1024);
        service.LinkCalls.ShouldBe(1);
    }

    [Fact]
    public async Task PersistAccountWhenSaveCommandExecutesWithValidEmail()
    {
        var databasePath = CreateDatabasePath();
        var viewModel = new AccountDialogViewModel(databasePath: databasePath)
        {
            Email = "saved@example.com",
            QuotaBytes = 5000,
            UsedBytes = 400
        };

        viewModel.SaveCommand.Execute(null);
        var listViewModel = new AccountListViewModel(databasePath);
        _ = await listViewModel.LoadAccountsAsync(TestContext.Current.CancellationToken);

        viewModel.IsSaved.ShouldBeTrue();
        listViewModel.Accounts.Count.ShouldBe(1);
        listViewModel.Accounts[0].Email.ShouldBe("saved@example.com");
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

    private sealed class TestAccountSessionService(AccountSessionState state) : IAccountSessionService
    {
        public int LinkCalls { get; private set; }

        public Task<Result<AccountSessionState, string>> LinkAccountAsync(string emailHint, CancellationToken cancellationToken = default)
        {
            LinkCalls++;
            return Task.FromResult<Result<AccountSessionState, string>>(state);
        }

        public Task<Result<AccountSessionState, string>> ReauthenticateAsync(string accountId, string emailHint, CancellationToken cancellationToken = default)
            => Task.FromResult<Result<AccountSessionState, string>>(state);

        public Task<Result<AccountSessionState, string>> GetValidSessionAsync(string accountId, CancellationToken cancellationToken = default)
            => Task.FromResult<Result<AccountSessionState, string>>(state);

        public Task<Result<Unit, string>> UnlinkAccountAsync(string accountId, CancellationToken cancellationToken = default)
            => Task.FromResult<Result<Unit, string>>(Unit.Value);
    }
}
