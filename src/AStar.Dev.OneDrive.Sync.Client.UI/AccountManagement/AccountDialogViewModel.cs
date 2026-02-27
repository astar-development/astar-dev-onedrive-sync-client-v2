using System.Collections.ObjectModel;
using System.Net.Mail;
using System.Windows.Input;
using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Contracts;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Repositories;
using AStar.Dev.OneDrive.Sync.Client.UI.Common;
using ReactiveUI;

namespace AStar.Dev.OneDrive.Sync.Client.UI.AccountManagement;

/// <summary>
/// ViewModel for creating and editing account information in a dialog workflow.
/// </summary>
public class AccountDialogViewModel : ViewModelBase
{
    private readonly SqliteDatabaseMigrator _migrator;
    private readonly SqliteAccountsRepository _accountsRepository;
    private readonly string? _existingAccountId;

    /// <summary>
    /// Initializes a new dialog for creating a new account.
    /// </summary>
    /// <param name="databasePath">Optional SQLite database file path.</param>
    public AccountDialogViewModel(string? databasePath = null)
    {
        _migrator = new SqliteDatabaseMigrator(databasePath);
        _accountsRepository = new SqliteAccountsRepository(databasePath);
        SaveCommand = new RelayCommand(_ => Save());
        CancelCommand = new RelayCommand(_ => Cancel());
        LoginCommand = new RelayCommand(_ => TriggerLogin());
    }

    /// <summary>
    /// Initializes a dialog for editing an existing account.
    /// </summary>
    /// <param name="account">The account being edited.</param>
    /// <param name="databasePath">Optional SQLite database file path.</param>
    public AccountDialogViewModel(AccountInfo account, string? databasePath = null)
        : this(databasePath)
    {
        _existingAccountId = account.Id;
        Email = account.Email;
        QuotaBytes = account.QuotaBytes;
        UsedBytes = account.UsedBytes;
    }

    /// <summary>
    /// Gets a value indicating whether the dialog is editing an existing account.
    /// </summary>
    public bool IsEditMode => !string.IsNullOrWhiteSpace(_existingAccountId);

    /// <summary>
    /// Gets or sets the account email address.
    /// </summary>
    public string Email
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = string.Empty;

    /// <summary>
    /// Gets or sets the account quota in bytes.
    /// </summary>
    public long QuotaBytes
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Gets or sets used storage in bytes.
    /// </summary>
    public long UsedBytes
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Gets the login providers available in the stub flow.
    /// </summary>
    public ObservableCollection<string> LoginProviders { get; } = ["OneDrive"];

    /// <summary>
    /// Gets or sets the validation error message.
    /// </summary>
    public string ValidationError
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether save was completed successfully.
    /// </summary>
    public bool IsSaved
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Gets a value indicating whether cancel was requested.
    /// </summary>
    public bool IsCancelled
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Gets a value indicating whether login was triggered.
    /// </summary>
    public bool LoginTriggered
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Gets the command that validates and persists the account.
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// Gets the command that closes the dialog without saving.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Gets the command that triggers the authentication stub.
    /// </summary>
    public ICommand LoginCommand { get; }

    /// <summary>
    /// Raised when the dialog should close.
    /// </summary>
    public event EventHandler<bool>? CloseRequested;

    private async void Save()
    {
        await ValidateEmail(Email)
            .MapFailure(message => new InvalidOperationException(message))
            .BindAsync(_ => SaveAccountAsync())
            .MatchAsync(
                _ =>
                {
                    ValidationError = string.Empty;
                    IsSaved = true;
                    IsCancelled = false;
                    CloseRequested?.Invoke(this, true);
                    return Task.CompletedTask;
                },
                error =>
                {
                    ValidationError = error.Message;
                    return Task.CompletedTask;
                });
    }

    private async Task<Result<AccountInfo, Exception>> SaveAccountAsync(CancellationToken cancellationToken = default)
        => await Try.RunAsync(async () =>
        {
            await _migrator.EnsureMigratedAsync(cancellationToken);
            var accounts = (await _accountsRepository.LoadAsync(cancellationToken)).ToList();

            var id = _existingAccountId ?? Guid.NewGuid().ToString("N");
            var updated = new AccountState(id, Email, QuotaBytes, UsedBytes);
            var existingIndex = accounts.FindIndex(x => x.Id == id);
            if(existingIndex >= 0)
            {
                accounts[existingIndex] = updated;
            }
            else
            {
                accounts.Add(updated);
            }

            await _accountsRepository.SaveAsync(accounts, cancellationToken);
            return new AccountInfo(updated.Id, updated.Email, updated.QuotaBytes, updated.UsedBytes);
        });

    private void Cancel()
    {
        IsCancelled = true;
        IsSaved = false;
        CloseRequested?.Invoke(this, false);
    }

    private void TriggerLogin() => LoginTriggered = true;

    private static Result<bool, string> ValidateEmail(string email)
        => IsValidEmail(email)
            ? true
            : "Invalid email format.";

    private static bool IsValidEmail(string email)
    {
        if(string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            _ = new MailAddress(email);
            return true;
        }
        catch(FormatException)
        {
            return false;
        }
    }
}