using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;

namespace AStar.Dev.OneDrive.Sync.Client.UI.AccountManagement;

[ExcludeFromCodeCoverage]
public partial class AccountListView : UserControl
{
    private AccountListViewModel? _viewModel;

    public AccountListView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    public AccountListViewModel? ViewModel => DataContext as AccountListViewModel;

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if(_viewModel is not null)
        {
            _viewModel.AccountDialogRequested -= OnAccountDialogRequested;
        }

        _viewModel = DataContext as AccountListViewModel;
        if(_viewModel is not null)
        {
            _viewModel.AccountDialogRequested += OnAccountDialogRequested;
        }
    }

    private async void OnAccountDialogRequested(object? sender, AccountDialogViewModel dialogViewModel)
    {
        var dialog = new AccountDialogView { DataContext = dialogViewModel };
        if(TopLevel.GetTopLevel(this) is Window owner)
        {
            _ = await dialog.ShowDialog<bool>(owner);
            return;
        }

        dialog.Show();
    }
}
