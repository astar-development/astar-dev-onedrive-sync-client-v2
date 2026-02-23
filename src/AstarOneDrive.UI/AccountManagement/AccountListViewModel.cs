using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;
using AStar.Dev.Functional.Extensions;
using AstarOneDrive.UI.Common;
using ReactiveUI;

namespace AstarOneDrive.UI.AccountManagement;

public class AccountListViewModel : ViewModelBase
{
    private readonly RelayCommand _removeAccountCommand;
    private readonly RelayCommand _manageAccountCommand;

    public ObservableCollection<AccountInfo> Accounts { get; } = new();

    public AccountInfo? SelectedAccount
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            _removeAccountCommand.RaiseCanExecuteChanged();
            _manageAccountCommand.RaiseCanExecuteChanged();
        }
    }

    public ICommand AddAccountCommand { get; }
    public ICommand RemoveAccountCommand => _removeAccountCommand;
    public ICommand ManageAccountCommand { get; }

    public AccountListViewModel()
    {
        AddAccountCommand = new RelayCommand(_ => AddAccount());
        _removeAccountCommand = new RelayCommand(_ => RemoveSelectedAccount(), _ => SelectedAccount is not null);
        _manageAccountCommand = new RelayCommand(account => ManageAccount(account as AccountInfo), _ => SelectedAccount is not null);
        ManageAccountCommand = _manageAccountCommand;
    }

    private void AddAccount() =>
        Accounts.Add(new AccountInfo(Guid.NewGuid().ToString("N"), "new.account@example.com", 0, 0));

    private void RemoveSelectedAccount()
    {
        if (SelectedAccount is null)
        {
            return;
        }

        Accounts.Remove(SelectedAccount);
        SelectedAccount = null;
    }

    private void ManageAccount(AccountInfo? account)
    {
        if (account is null)
        {
            return;
        }

        SelectedAccount = account;
    }

    public Task<Result<bool, Exception>> SaveAccountsAsync(CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(Accounts.ToList(), new JsonSerializerOptions { WriteIndented = true });
        return Try.RunAsync(async () =>
        {
            await File.WriteAllTextAsync(GetAccountsFilePath(), json, cancellationToken);
            return true;
        });
    }

    public Task<Result<bool, Exception>> LoadAccountsAsync(CancellationToken cancellationToken = default) =>
        Try.RunAsync(async () =>
        {
            var filePath = GetAccountsFilePath();
            if (!File.Exists(filePath))
            {
                return true;
            }

            var json = await File.ReadAllTextAsync(filePath, cancellationToken);
            var restored = JsonSerializer.Deserialize<List<AccountInfo>>(json) ?? [];

            Accounts.Clear();
            foreach (var account in restored)
            {
                Accounts.Add(account);
            }

            return true;
        });

    private static string GetAccountsFilePath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "AstarOneDrive");
        Directory.CreateDirectory(appFolder);
        return Path.Combine(appFolder, "accounts.json");
    }
}
