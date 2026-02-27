using System.Collections.ObjectModel;
using System.Windows.Input;
using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Contracts;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Repositories;
using AStar.Dev.OneDrive.Sync.Client.UI.Common;
using ReactiveUI;

namespace AStar.Dev.OneDrive.Sync.Client.UI.AccountManagement;

/// <summary>
/// ViewModel for managing a list of OneDrive accounts, including adding, removing, and managing individual accounts.
/// </summary>
public class AccountListViewModel : ViewModelBase
{
    private readonly RelayCommand _removeAccountCommand;
    private readonly RelayCommand _manageAccountCommand;
    private readonly SqliteDatabaseMigrator _migrator;
    private readonly SqliteAccountsRepository _accountsRepository;

    public ObservableCollection<AccountInfo> Accounts { get; } = [];

    /// <summary>
    /// The currently selected account in the UI. Setting this property will raise PropertyChanged and update command states for Remove and Manage actions.
    /// </summary>
    /// <remarks>
    /// This property is used to determine which account is being acted upon when the user clicks "Remove" or "Manage" buttons. It is also used to enable or disable those buttons based on whether an account is selected.
    /// </remarks>
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

    /// <summary>
    /// Command to add a new account. This will create a new AccountInfo with default values and add it to the Accounts collection.
    /// </summary>
    /// <remarks>
    /// The AddAccountCommand does not require a selected account to execute, so it is always enabled. When executed, it adds a new account with a unique ID and placeholder email to the Accounts collection.
    /// </remarks>
    public ICommand AddAccountCommand { get; }

    /// <summary>
    /// Command to remove the currently selected account. This command is only enabled when an account is selected. When executed, it removes the selected account from the Accounts collection and clears the selection.
    /// </summary>
    public ICommand RemoveAccountCommand => _removeAccountCommand;

    /// <summary>
    /// Command to manage the currently selected account. This command is only enabled when an account is selected. When executed, it will trigger logic to manage the selected account (e.g., open a details view). The actual management logic is not implemented in this ViewModel and should be handled by the view or a higher-level coordinator.
    /// </summary>
    public ICommand ManageAccountCommand { get; }

    /// <summary>
    /// Initializes a new instance of the AccountListViewModel class. This constructor sets up the database migrator and accounts repository, and initializes the commands for adding, removing, and managing accounts. The database path can be optionally provided; if not, it will use a default location.
    /// </summary>
    /// <param name="databasePath">The optional path to the database file. If not provided, a default location will be used.</param>
    public AccountListViewModel(string? databasePath = null)
    {
        _migrator = new SqliteDatabaseMigrator(databasePath);
        _accountsRepository = new SqliteAccountsRepository(databasePath);

        AddAccountCommand = new RelayCommand(_ => AddAccount());
        _removeAccountCommand = new RelayCommand(_ => RemoveSelectedAccount(), _ => SelectedAccount is not null);
        _manageAccountCommand = new RelayCommand(account => ManageAccount(account as AccountInfo), _ => SelectedAccount is not null);
        ManageAccountCommand = _manageAccountCommand;
    }

    /// <summary>
    /// Saves the current list of accounts to the database. This method will first ensure that any necessary database migrations are applied, then it will convert the Accounts collection into a list of AccountState objects and save them using the accounts repository. The result of the operation is returned as a Result<bool, Exception>, indicating success or failure of the save operation.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous save operation. The task result contains a Result<bool, Exception> indicating success or failure.</returns>
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

    /// <summary>
    /// Loads the list of accounts from the database. This method will first ensure that any necessary database migrations are applied, then it will load the account states from the accounts repository and populate the Accounts collection. The result of the operation is returned as a Result<bool, Exception>, indicating success or failure of the load operation.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous load operation. The task result contains a Result<bool, Exception> indicating success or failure.</returns>
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
}
