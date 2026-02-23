using AStar.Dev.Functional.Extensions;
using AstarOneDrive.UI.AccountManagement;
using Shouldly;

namespace AstarOneDrive.UI.Tests.ViewModels.AccountManagement;

public sealed class AccountListViewModelTests
{
    private static string AccountsFilePath
    {
        get
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var astarPath = Path.Combine(appDataPath, "AstarOneDrive");
            return Path.Combine(astarPath, "accounts.json");
        }
    }

    [Fact]
    public void Constructor_InitializesEmptyAccountsCollection()
    {
        var viewModel = new AccountListViewModel();

        viewModel.Accounts.ShouldBeEmpty();
    }

    [Fact]
    public void Accounts_NotifiesOnItemAdded()
    {
        var viewModel = new AccountListViewModel();
        var changed = false;
        viewModel.Accounts.CollectionChanged += (_, _) => changed = true;

        viewModel.AddAccountCommand.Execute(null);

        changed.ShouldBeTrue();
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
    public async Task SaveAndLoadAccounts_PersistsAndRestoresData()
    {
        DeleteAccountsFileIfExists();
        var viewModel = new AccountListViewModel();
        viewModel.Accounts.Add(new AccountInfo("acct-1", "persisted@example.com", 2000, 250));

        var saveResult = await viewModel.SaveAccountsAsync(TestContext.Current.CancellationToken);
        Pattern.IsSuccess(saveResult).ShouldBeTrue();

        var loadedViewModel = new AccountListViewModel();
        var loadResult = await loadedViewModel.LoadAccountsAsync(TestContext.Current.CancellationToken);
        Pattern.IsSuccess(loadResult).ShouldBeTrue();
        loadedViewModel.Accounts.Count.ShouldBe(1);
        loadedViewModel.Accounts[0].Email.ShouldBe("persisted@example.com");
    }

    private static void DeleteAccountsFileIfExists()
    {
        if (File.Exists(AccountsFilePath))
        {
            File.Delete(AccountsFilePath);
        }
    }
}