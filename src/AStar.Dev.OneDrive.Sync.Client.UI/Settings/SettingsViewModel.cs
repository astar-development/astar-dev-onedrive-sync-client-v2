using System.Collections.ObjectModel;
using System.Windows.Input;
using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Contracts;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Repositories;
using AStar.Dev.OneDrive.Sync.Client.UI.Common;
using AStar.Dev.OneDrive.Sync.Client.UI.Localization;
using ReactiveUI;
using static AStar.Dev.OneDrive.Sync.Client.UI.ThemeManager.ThemeManager;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Settings;

public class SettingsViewModel : ViewModelBase
{
    private readonly SqliteDatabaseMigrator _migrator;
    private readonly SqliteSettingsRepository _settingsRepository;
    private SettingsState _lastCommittedState;

    public SettingsViewModel(string? databasePath = null)
    {
        _migrator = new SqliteDatabaseMigrator(databasePath);
        _settingsRepository = new SqliteSettingsRepository(databasePath);
        OkCommand = new RelayCommand(_ => SaveSettings());
        ApplyCommand = new RelayCommand(_ => SaveSettings());
        CancelCommand = new RelayCommand(_ => RevertToCommittedState());
        _lastCommittedState = CreateState();
    }

    public string UserName
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = "User";

    public ObservableCollection<string> AvailableLanguages { get; } = ["en-GB", "en-US"];

    public string SelectedLanguage
    {
        get;
        set
        {
            if(field == value)
            {
                return;
            }

            _ = this.RaiseAndSetIfChanged(ref field, value);
            LocalizationManager.SetLanguage(value);
        }
    } = "en-GB";

    public ObservableCollection<string> AvailableThemes { get; } =
        ["Light", "Dark", "Colorful", "Professional", "Hacker", "HighContrast"];

    public string SelectedTheme
    {
        get;
        set
        {
            if(field == value)
            {
                return;
            }

            _ = this.RaiseAndSetIfChanged(ref field, value);
            ApplyTheme(value);
            ThemeChanged?.Invoke(this, value);
        }
    } = "Light";

    public event EventHandler<string>? ThemeChanged;
    public event EventHandler<string>? LayoutChanged;

    public ObservableCollection<string> AvailableLayouts { get; } = ["Explorer", "Dashboard", "Terminal"];

    public string SelectedLayout
    {
        get;
        set
        {
            if(field == value)
            {
                return;
            }

            _ = this.RaiseAndSetIfChanged(ref field, value);
            LayoutChanged?.Invoke(this, value);
        }
    } = "Explorer";

    /// <summary>
    /// Saves settings and closes the dialog.
    /// </summary>
    public ICommand OkCommand { get; }

    /// <summary>
    /// Saves settings without closing the dialog.
    /// </summary>
    public ICommand ApplyCommand { get; }

    /// <summary>
    /// Reverts unsaved changes.
    /// </summary>
    public ICommand CancelCommand { get; }

    public Task<Result<bool, Exception>> SaveSettingsAsync(CancellationToken cancellationToken = default)
        => Try.RunAsync(async () =>
        {
            await _migrator.EnsureMigratedAsync(cancellationToken);
            SettingsState state = CreateState();
            await _settingsRepository.SaveAsync(state, cancellationToken);
            _lastCommittedState = state;
            return true;
        });

    public Task<Result<bool, Exception>> LoadSettingsAsync(CancellationToken cancellationToken = default)
        => Try.RunAsync(async () =>
        {
            await _migrator.EnsureMigratedAsync(cancellationToken);
            SettingsState? state = await _settingsRepository.LoadAsync(cancellationToken);
            if(state is null)
            {
                _lastCommittedState = CreateState();
                return true;
            }

            ApplySettings(state);
            _lastCommittedState = state;
            return true;
        });

    private void ApplySettings(SettingsState state)
    {
        SelectedTheme = state.SelectedTheme;
        SelectedLanguage = state.SelectedLanguage;
        SelectedLayout = state.SelectedLayout;
        UserName = state.UserName;
    }

    private SettingsState CreateState()
        => new(SelectedTheme, SelectedLanguage, SelectedLayout, UserName);

    private void RevertToCommittedState()
    {
        SettingsState state = _lastCommittedState;
        SelectedTheme = state.SelectedTheme;
        SelectedLanguage = state.SelectedLanguage;
        SelectedLayout = state.SelectedLayout;
        UserName = state.UserName;
    }

    private void SaveSettings()
        => SaveSettingsAsync().GetAwaiter().GetResult();
}
