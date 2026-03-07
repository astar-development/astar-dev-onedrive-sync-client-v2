using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using AStar.Dev.OneDrive.Sync.Client.UI.AccountManagement;
using AStar.Dev.Utilities;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.AccountManagement;

public sealed class AccountListViewModelShould
{
    [Fact]
    public void Constructor_InitializesEmptyAccountsCollection()
    {
        var viewModel = new AccountListViewModel();

        viewModel.Accounts.ShouldBeEmpty();
    }

    [Fact]
    public void AddAccountCommand_RaisesDialogRequested()
    {
        var viewModel = new AccountListViewModel();
        var raised = false;
        viewModel.AccountDialogRequested += (_, _) => raised = true;

        viewModel.AddAccountCommand.Execute(null);

        raised.ShouldBeTrue();
    }

    [Fact]
    public void AddAccountCommand_CanExecute_ReturnsTrue()
    {
        var viewModel = new AccountListViewModel();

        viewModel.AddAccountCommand.CanExecute(null).ShouldBeTrue();
    }

    [Fact]
    public void RemoveAccountCommand_CanExecute_ReturnsTrue_WhenSelectedAccountExists()
    {
        var viewModel = new AccountListViewModel();
        var account = new AccountInfo("id", "user@example.com", 1000, 100);
        viewModel.Accounts.Add(account);
        viewModel.SelectedAccount = account;

        viewModel.RemoveAccountCommand.CanExecute(null).ShouldBeTrue();
    }

    [Fact]
    public void ManageAccountCommand_CanExecute_ReturnsTrue_WhenSelectedAccountExists()
    {
        var viewModel = new AccountListViewModel();
        var account = new AccountInfo("id", "user@example.com", 1000, 100);
        viewModel.Accounts.Add(account);
        viewModel.SelectedAccount = account;

        viewModel.ManageAccountCommand.CanExecute(account).ShouldBeTrue();
    }

    [Fact]
    public void SelectedAccount_Set_RaisesPropertyChanged()
    {
        var viewModel = new AccountListViewModel();
        var raised = false;
        viewModel.PropertyChanged += (_, args) => raised |= args.PropertyName == nameof(AccountListViewModel.SelectedAccount);
        viewModel.SelectedAccount = new AccountInfo("id", "user@example.com", 1000, 100);

        raised.ShouldBeTrue();
    }

    [Fact]
    public async Task SaveAndLoadAccounts_PersistsAndRestoresDataFromDatabase()
    {
        var databasePath = CreateDatabasePath();
        var viewModel = new AccountListViewModel(databasePath);
        viewModel.Accounts.Add(new AccountInfo("acct-1", "persisted@example.com", 2000, 250));
        Result<bool, Exception> saveResult = await viewModel.SaveAccountsAsync(TestContext.Current.CancellationToken);
        Pattern.IsSuccess(saveResult).ShouldBeTrue();

        var loadedViewModel = new AccountListViewModel(databasePath);
        Result<bool, Exception> loadResult = await loadedViewModel.LoadAccountsAsync(TestContext.Current.CancellationToken);
        Pattern.IsSuccess(loadResult).ShouldBeTrue();
        loadedViewModel.Accounts.Count.ShouldBe(1);
        loadedViewModel.Accounts[0].Email.ShouldBe("persisted@example.com");
    }

    [Fact]
    public async Task RemoveAccountCommand_UnlinksSessionAndRemovesAccount()
    {
        var accountSessionService = new TrackingAccountSessionService();
        var viewModel = new AccountListViewModel(accountSessionService: accountSessionService);
        var account = new AccountInfo("acct-9", "unlink@example.com", 100, 25);
        viewModel.Accounts.Add(account);
        viewModel.SelectedAccount = account;

        viewModel.RemoveAccountCommand.Execute(null);
        await WaitForConditionAsync(() => accountSessionService.UnlinkedAccountIds.Count == 1);

        viewModel.Accounts.ShouldBeEmpty();
        viewModel.SelectedAccount.ShouldBeNull();
        accountSessionService.UnlinkedAccountIds.ShouldContain("acct-9");
    }

    private static string CreateDatabasePath()
        => Path.GetTempPath().CombinePath($"astar-ui-accounts-tests-{Guid.NewGuid():N}", "astar-onedrive.db");

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

    private sealed class TrackingAccountSessionService : IAccountSessionService
    {
        public List<string> UnlinkedAccountIds { get; } = [];

        public Task<Result<AccountSessionState, string>> LinkAccountAsync(string emailHint, CancellationToken cancellationToken = default)
            => Task.FromResult<Result<AccountSessionState, string>>(CreateState("acct-link", emailHint));

        public Task<Result<AccountSessionState, string>> ReauthenticateAsync(string accountId, string emailHint, CancellationToken cancellationToken = default)
            => Task.FromResult<Result<AccountSessionState, string>>(CreateState(accountId, emailHint));

        public Task<Result<AccountSessionState, string>> GetValidSessionAsync(string accountId, CancellationToken cancellationToken = default)
            => Task.FromResult<Result<AccountSessionState, string>>(CreateState(accountId, "user@example.com"));

        public Task<Result<Unit, string>> UnlinkAccountAsync(string accountId, CancellationToken cancellationToken = default)
        {
            UnlinkedAccountIds.Add(accountId);
            return Task.FromResult<Result<Unit, string>>(Unit.Value);
        }

        private static AccountSessionState CreateState(string accountId, string email)
            => new(
                new OneDriveAccountProfile(accountId, email, 0, 0),
                new AccountSessionMetadata(accountId, DateTime.UtcNow.AddMinutes(30), DateTime.UtcNow, null, false));
    }
}
