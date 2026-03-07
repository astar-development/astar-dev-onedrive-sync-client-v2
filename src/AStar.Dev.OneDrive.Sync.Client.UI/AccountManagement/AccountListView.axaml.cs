using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;

namespace AStar.Dev.OneDrive.Sync.Client.UI.AccountManagement;

[ExcludeFromCodeCoverage]
public partial class AccountListView : UserControl
{
    private static readonly object DialogHostSync = new();
    private static AccountListView? _dialogHost;
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
            TryRegisterDialogHost();
        }
    }

    private async void OnAccountDialogRequested(object? sender, AccountDialogViewModel dialogViewModel)
    {
        if(!IsDialogHost())
        {
            return;
        }

        await ShowAccountDialogAsync(dialogViewModel);
    }

    /// <summary>
    /// Shows the account dialog for the given view model.
    /// </summary>
    /// <param name="dialogViewModel">The dialog view model.</param>
    protected virtual async Task ShowAccountDialogAsync(AccountDialogViewModel dialogViewModel)
    {
        var dialog = new AccountDialogView { DataContext = dialogViewModel };
        if(TopLevel.GetTopLevel(this) is Window owner)
        {
            _ = await dialog.ShowDialog<bool>(owner);
            return;
        }

        dialog.Show();
    }

    private void TryRegisterDialogHost()
    {
        lock(DialogHostSync)
        {
            _dialogHost ??= this;
        }
    }

    private bool IsDialogHost()
    {
        lock(DialogHostSync)
        {
            _dialogHost ??= this;

            return ReferenceEquals(_dialogHost, this);
        }
    }
}
