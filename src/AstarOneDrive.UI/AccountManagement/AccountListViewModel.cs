using System.Collections.ObjectModel;
using System.Windows.Input;
using AstarOneDrive.UI.Common;
using ReactiveUI;

namespace AstarOneDrive.UI.AccountManagement;

public class AccountListViewModel : ViewModelBase
{
    public ObservableCollection<AccountViewModel> Accounts { get; } = new();

    public ICommand AddAccountCommand { get; }
    public ICommand ManageAccountCommand { get; }

    public AccountListViewModel()
    {
        AddAccountCommand = new RelayCommand(_ => AddAccount());
        ManageAccountCommand = new RelayCommand(account => ManageAccount(account as AccountViewModel));
    }

    private void AddAccount() =>
        // Placeholder logic
        Accounts.Add(new AccountViewModel
        {
            Email = "new.account@example.com",
            StorageUsed = "0 GB used"
        });

    private void ManageAccount(AccountViewModel? account)
    {
        if (account == null) return;

        // TODO: Open account management dialog
    }
}

public class AccountViewModel : ViewModelBase
{
    public string Email
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = "";
    public string StorageUsed
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = "";
}
