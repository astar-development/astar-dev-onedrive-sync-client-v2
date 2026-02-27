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
    public void TriggerAuthenticationStubWhenLoginCommandExecutes()
    {
        var viewModel = new AccountDialogViewModel();

        viewModel.LoginCommand.Execute(null);

        viewModel.LoginTriggered.ShouldBeTrue();
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
}