using System.Collections.ObjectModel;
using System.Windows.Input;
using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Contracts;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Repositories;
using AStar.Dev.OneDrive.Sync.Client.UI.Common;
using ReactiveUI;

namespace AStar.Dev.OneDrive.Sync.Client.UI.AccountManagement;

public class AccountListViewModel : ViewModelBase
{
    private readonly RelayCommand _removeAccountCommand;
    private readonly RelayCommand _manageAccountCommand;
    private readonly SqliteDatabaseMigrator _migrator;
    private readonly SqliteAccountsRepository _accountsRepository;

    public ObservableCollection<AccountInfo> Accounts { get; } = new();

    public AccountInfo? SelectedAccount
    {
        get;
        set
        {
            _ = this.RaiseAndSetIfChanged(ref field, value);
            _removeAccountCommand.RaiseCanExecuteChanged();
            _manageAccountCommand.RaiseCanExecuteChanged();
        }
    }

    public ICommand AddAccountCommand { get; }
    public ICommand RemoveAccountCommand => _removeAccountCommand;
    public ICommand ManageAccountCommand { get; }

    public AccountListViewModel(string? databasePath = null)
    {
        _migrator = new SqliteDatabaseMigrator(databasePath);
        _accountsRepository = new SqliteAccountsRepository(databasePath);

        AddAccountCommand = new RelayCommand(_ => AddAccount());
        _removeAccountCommand = new RelayCommand(_ => RemoveSelectedAccount(), _ => SelectedAccount is not null);
        _manageAccountCommand = new RelayCommand(account => ManageAccount(account as AccountInfo), _ => SelectedAccount is not null);
        ManageAccountCommand = _manageAccountCommand;
    }

    private void AddAccount()
        => Accounts.Add(new AccountInfo(Guid.NewGuid().ToString("N"), "new.account@example.com", 0, 0));

    private void RemoveSelectedAccount()
    {
        if(SelectedAccount is null)
        {
            return;
        }

        _ = Accounts.Remove(SelectedAccount);
        SelectedAccount = null;
    }

    private void ManageAccount(AccountInfo? account)
    {
        if(account is null)
        {
            return;
        }

        SelectedAccount = account;
    }

    public Task<Result<bool, Exception>> SaveAccountsAsync(CancellationToken cancellationToken = default)
        => Try.RunAsync(async () =>
        {
            await _migrator.EnsureMigratedAsync(cancellationToken);
            var state = Accounts
                .Select(x => new AccountState(x.Id, x.Email, x.QuotaBytes, x.UsedBytes))
                .ToList();
            await _accountsRepository.SaveAsync(state, cancellationToken);
            return true;
        });

    public Task<Result<bool, Exception>> LoadAccountsAsync(CancellationToken cancellationToken = default)
        => Try.RunAsync(async () =>
        {
            await _migrator.EnsureMigratedAsync(cancellationToken);
            IReadOnlyList<AccountState> state = await _accountsRepository.LoadAsync(cancellationToken);
            Accounts.Clear();
            foreach(AccountState account in state)
            {
                Accounts.Add(new AccountInfo(account.Id, account.Email, account.QuotaBytes, account.UsedBytes));
            }

            return true;
        });
}
