using System.Collections.ObjectModel;
using System.Windows.Input;
using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Authentication;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Contracts;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Repositories;
using AStar.Dev.OneDrive.Sync.Client.UI.Common;
using AStar.Dev.Utilities;
using ReactiveUI;

namespace AStar.Dev.OneDrive.Sync.Client.UI.AccountManagement;

/// <summary>
/// ViewModel for creating and editing account information in a dialog workflow.
/// </summary>
public class AccountDialogViewModel : ViewModelBase
{
    private readonly RelayCommand _cancelCommand;
    private readonly SqliteDatabaseMigrator _migrator;
    private readonly SqliteAccountsRepository _accountsRepository;
    private readonly IAccountSessionService _accountSessionService;
    private string? _existingAccountId;

    /// <summary>
    /// Initializes a new dialog for creating a new account.
    /// </summary>
    /// <param name="databasePath">Optional SQLite database file path.</param>
    public AccountDialogViewModel(string? databasePath = null, IAccountSessionService? accountSessionService = null)
    {
        _migrator = new SqliteDatabaseMigrator(databasePath);
        _accountsRepository = new SqliteAccountsRepository(databasePath);
        _accountSessionService = accountSessionService ?? CreateAccountSessionService(databasePath);
        _cancelCommand = new RelayCommand(_ => Cancel(), _ => !IsSaved);
        CancelCommand = _cancelCommand;
        LoginCommand = new RelayCommand(async _ => await TriggerLoginAsync());
    }

    /// <summary>
    /// Initializes a dialog for editing an existing account.
    /// </summary>
    /// <param name="account">The account being edited.</param>
    /// <param name="databasePath">Optional SQLite database file path.</param>
    public AccountDialogViewModel(AccountInfo account, string? databasePath = null, IAccountSessionService? accountSessionService = null)
        : this(databasePath, accountSessionService)
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
    /// Gets a value indicating whether persistence was completed successfully.
    /// </summary>
    public bool IsSaved
    {
        get;
        private set
        {
            _ = this.RaiseAndSetIfChanged(ref field, value);
            _cancelCommand.RaiseCanExecuteChanged();
        }
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
    /// Gets the command that closes the dialog without saving.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Gets the command that performs account link or reauthentication.
    /// </summary>
    public ICommand LoginCommand { get; }

    /// <summary>
    /// Raised when the dialog should close.
    /// </summary>
    public event EventHandler<bool>? CloseRequested;

    private async Task<Result<AccountInfo, Exception>> SaveAccountAsync(string? accountId = null, CancellationToken cancellationToken = default)
        => await Try.RunAsync(async () =>
        {
            await _migrator.EnsureMigratedAsync(cancellationToken);
            var accounts = (await _accountsRepository.LoadAsync(cancellationToken)).ToList();

            var normalizedEmail = Email.Trim();
            var id = accountId ?? _existingAccountId ?? Guid.NewGuid().ToString("N");
            var updated = new AccountState(id, normalizedEmail, QuotaBytes, UsedBytes);
            _ = accounts.RemoveAll(x => x.Id == updated.Id || string.Equals(x.Email, updated.Email, StringComparison.OrdinalIgnoreCase));
            accounts.Add(updated);

            await _accountsRepository.SaveAsync(accounts, cancellationToken);
            return new AccountInfo(updated.Id, updated.Email, updated.QuotaBytes, updated.UsedBytes);
        });

    private void Cancel()
    {
        IsCancelled = true;
        IsSaved = false;
        CloseRequested?.Invoke(this, false);
    }

    private async Task TriggerLoginAsync()
    {
        var emailHint = Email;
        Result<AccountSessionState, string> result = IsEditMode && !string.IsNullOrWhiteSpace(_existingAccountId)
            ? await _accountSessionService.ReauthenticateAsync(_existingAccountId, emailHint)
            : await _accountSessionService.LinkAccountAsync(emailHint);

        Result<AccountInfo, Exception> persistResult = await result
            .MapFailure(message => (Exception)new InvalidOperationException(message))
            .BindAsync(async session =>
            {
                _existingAccountId = session.Profile.AccountId;
                Email = session.Profile.Email;
                QuotaBytes = session.Profile.QuotaBytes;
                UsedBytes = session.Profile.UsedBytes;
                return await SaveAccountAsync(session.Profile.AccountId);
            });

        _ = persistResult.Match(
            _ =>
            {
                ValidationError = string.Empty;
                LoginTriggered = true;
                IsSaved = true;
                IsCancelled = false;
                CloseRequested?.Invoke(this, true);
                return true;
            },
            error =>
            {
                ValidationError = error.Message;
                LoginTriggered = false;
                IsSaved = false;
                return false;
            });
    }

    private static IAccountSessionService CreateAccountSessionService(string? databasePath)
    {
        var path = Path.GetDirectoryName(DatabasePathResolver.ResolveDatabasePath())!;
        var tokenStorePath = path.CombinePath("secure-store");

        return new OneDriveAccountSessionService(
            new OneDriveAuthenticationAdapter(),
            new FileBackedSecureAccountTokenStore(tokenStorePath),
            new SqliteAccountSessionMetadataRepository(databasePath));
    }
}
