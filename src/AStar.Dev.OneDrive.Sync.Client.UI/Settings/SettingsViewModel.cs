using System.Collections.ObjectModel;
using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Contracts;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Repositories;
using AStar.Dev.OneDrive.Sync.Client.UI.Common;
using ReactiveUI;
using static AStar.Dev.OneDrive.Sync.Client.UI.ThemeManager.ThemeManager;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Settings;

public class SettingsViewModel : ViewModelBase
{
    private readonly SqliteDatabaseMigrator _migrator;
    private readonly SqliteSettingsRepository _settingsRepository;

    public SettingsViewModel(string? databasePath = null)
    {
        _migrator = new SqliteDatabaseMigrator(databasePath);
        _settingsRepository = new SqliteSettingsRepository(databasePath);
    }

    private string _userName = "User";
    public string UserName
    {
        get => _userName;
        set => this.RaiseAndSetIfChanged(ref _userName, value);
    }

    public ObservableCollection<string> AvailableLanguages { get; } = ["en-GB"];

    private string _selectedLanguage = "en-GB";
    public string SelectedLanguage
    {
        get => _selectedLanguage;
        set => this.RaiseAndSetIfChanged(ref _selectedLanguage, value);
    }

    public ObservableCollection<string> AvailableThemes { get; } =
        ["Light", "Dark", "Auto", "Colorful", "Professional", "Hacker", "HighContrast"];

    private string _selectedTheme = "Light";
    public string SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if(_selectedTheme == value)
            {
                return;
            }

            _ = this.RaiseAndSetIfChanged(ref _selectedTheme, value);
            ApplyTheme(value);
            ThemeChanged?.Invoke(this, value);
        }
    }

    public event EventHandler<string>? ThemeChanged;
    public event EventHandler<string>? LayoutChanged;

    public ObservableCollection<string> AvailableLayouts { get; } = ["Explorer", "Dashboard", "Terminal"];

    private string _selectedLayout = "Explorer";
    public string SelectedLayout
    {
        get => _selectedLayout;
        set
        {
            if(_selectedLayout == value)
            {
                return;
            }

            _ = this.RaiseAndSetIfChanged(ref _selectedLayout, value);
            LayoutChanged?.Invoke(this, value);
        }
    }

    public Task<Result<bool, Exception>> SaveSettingsAsync(CancellationToken cancellationToken = default)
        => Try.RunAsync(async () =>
        {
            await _migrator.EnsureMigratedAsync(cancellationToken);
            await _settingsRepository.SaveAsync(
                new SettingsState(SelectedTheme, SelectedLanguage, SelectedLayout, UserName),
                cancellationToken);
            return true;
        });

    public Task<Result<bool, Exception>> LoadSettingsAsync(CancellationToken cancellationToken = default)
        => Try.RunAsync(async () =>
        {
            await _migrator.EnsureMigratedAsync(cancellationToken);
            SettingsState? state = await _settingsRepository.LoadAsync(cancellationToken);
            if(state is null)
            {
                return true;
            }

            ApplySettings(state);
            return true;
        });

    private void ApplySettings(SettingsState state)
    {
        _selectedTheme = state.SelectedTheme;
        _ = this.RaiseAndSetIfChanged(ref _selectedTheme, SelectedTheme);

        _selectedLanguage = state.SelectedLanguage;
        _ = this.RaiseAndSetIfChanged(ref _selectedLanguage, SelectedLanguage);

        _selectedLayout = state.SelectedLayout;
        _ = this.RaiseAndSetIfChanged(ref _selectedLayout, SelectedLayout);

        _userName = state.UserName;
        _ = this.RaiseAndSetIfChanged(ref _userName, UserName);
    }
}
