using System.Collections.ObjectModel;
using System.Windows.Input;
using AstarOneDrive.UI.Common;

namespace AstarOneDrive.UI.AccountManagement;

public class AccountsViewModel : ViewModelBase
{
    public ObservableCollection<AccountViewModel> Accounts { get; } = new();

    public ICommand AddAccountCommand { get; }
    public ICommand ManageAccountCommand { get; }

    public AccountsViewModel()
    {
        AddAccountCommand = new RelayCommand(_ => AddAccount());
        ManageAccountCommand = new RelayCommand(account => ManageAccount(account as AccountViewModel));
    }

    private void AddAccount()
    {
        // Placeholder logic
        Accounts.Add(new AccountViewModel
        {
            Email = "new.account@example.com",
            StorageUsed = "0 GB used"
        });
    }

    private void ManageAccount(AccountViewModel? account)
    {
        if (account == null) return;

        // TODO: Open account management dialog
    }
}

public class AccountViewModel : ViewModelBase
{
    private string _email = "";
    public string Email
    {
        get => _email;
        set { _email = value; RaisePropertyChanged(); }
    }

    private string _storageUsed = "";
    public string StorageUsed
    {
        get => _storageUsed;
        set { _storageUsed = value; RaisePropertyChanged(); }
    }
}
