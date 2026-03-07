using AStar.Dev.OneDrive.Sync.Client.UI.AccountManagement;
using AStar.Dev.OneDrive.Sync.Client.UI.Tests.ThemeManager;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.AccountManagement;

[Collection(ThemeManagerTestCollection.Name)]
public sealed class AccountListViewShould
{
    [Fact]
    public async Task OpenSingleDialogWhenMultipleViewsShareSameViewModel()
    {
        var viewModel = new AccountListViewModel();
        var firstView = new TestAccountListView();
        var secondView = new TestAccountListView();

        firstView.DataContext = viewModel;
        secondView.DataContext = viewModel;

        viewModel.AddAccountCommand.Execute(null);
        await WaitForConditionAsync(() => firstView.OpenCount + secondView.OpenCount > 0);

        (firstView.OpenCount + secondView.OpenCount).ShouldBe(1);
    }

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

    private sealed class TestAccountListView : AccountListView
    {
        public int OpenCount { get; private set; }

        protected override Task ShowAccountDialogAsync(AccountDialogViewModel dialogViewModel)
        {
            OpenCount++;
            return Task.CompletedTask;
        }
    }
}
