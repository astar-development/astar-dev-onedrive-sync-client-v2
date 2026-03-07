using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Avalonia.Controls;

namespace AStar.Dev.OneDrive.Sync.Client.UI.AccountManagement;

[ExcludeFromCodeCoverage]
public partial class AccountListView : UserControl
{
    private static readonly Lock DialogHostSync = new();
    private static readonly ConditionalWeakTable<AccountListViewModel, DialogHostRegistration> DialogHosts = [];
    private AccountListViewModel? _viewModel;

    public AccountListView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    public AccountListViewModel? ViewModel => DataContext as AccountListViewModel;

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        _viewModel?.AccountDialogRequested -= OnAccountDialogRequested;

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
        if(_viewModel is null)
        {
            return;
        }

        lock(DialogHostSync)
        {
            if(!DialogHosts.TryGetValue(_viewModel, out DialogHostRegistration? registration))
            {
                DialogHosts.Add(_viewModel, new DialogHostRegistration(this));
                return;
            }

            if(!registration.TryGetHost(out _))
            {
                registration.SetHost(this);
            }
        }
    }

    private bool IsDialogHost()
    {
        if(_viewModel is null)
        {
            return false;
        }

        lock(DialogHostSync)
        {
            if(!DialogHosts.TryGetValue(_viewModel, out DialogHostRegistration? registration))
            {
                DialogHosts.Add(_viewModel, new DialogHostRegistration(this));
                return true;
            }

            if(!registration.TryGetHost(out AccountListView? host))
            {
                registration.SetHost(this);
                return true;
            }

            return ReferenceEquals(host, this);
        }
    }

    private sealed class DialogHostRegistration(AccountListView host)
    {
        private WeakReference<AccountListView> _host = new(host);

        public bool TryGetHost(out AccountListView? host)
            => _host.TryGetTarget(out host);

        public void SetHost(AccountListView host)
            => _host = new WeakReference<AccountListView>(host);
    }
}
