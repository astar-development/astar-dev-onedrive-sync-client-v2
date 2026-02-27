using AStar.Dev.Functional.Extensions;
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

    private static string CreateDatabasePath()
        => Path.GetTempPath().CombinePath($"astar-ui-accounts-tests-{Guid.NewGuid():N}", "astar-onedrive.db");
}
