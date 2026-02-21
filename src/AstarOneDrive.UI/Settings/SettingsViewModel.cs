using System.Collections.ObjectModel;
using System.Text.Json;
using AStar.Dev.Functional.Extensions;
using AstarOneDrive.UI.Common;
using static AstarOneDrive.UI.ThemeManager.ThemeManager;
using ReactiveUI;

namespace AstarOneDrive.UI.Settings;

file record SettingsData(string SelectedTheme, string SelectedLanguage, string SelectedLayout, string UserName);

public class SettingsViewModel : ViewModelBase
{
    // User settings
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

    // Application settings
    public ObservableCollection<string> AvailableThemes { get; } =
        ["Light", "Dark", "Auto", "Colorful", "Professional", "Hacker", "HighContrast"];

    private string _selectedTheme = "Light";
    public string SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (_selectedTheme == value) return;
            this.RaiseAndSetIfChanged(ref _selectedTheme, value);
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
            if (_selectedLayout == value) return;
            this.RaiseAndSetIfChanged(ref _selectedLayout, value);
            LayoutChanged?.Invoke(this, value);
        }
    }

    private static string GetSettingsFilePath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "AstarOneDrive");
        Directory.CreateDirectory(appFolder);
        return Path.Combine(appFolder, "settings.json");
    }

    public Task<Result<bool, Exception>> SaveSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = new SettingsData(SelectedTheme, SelectedLanguage, SelectedLayout, UserName);
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        return Try.RunAsync(async () =>
        {
            await File.WriteAllTextAsync(GetSettingsFilePath(), json, cancellationToken);
            return true;
        });
    }

    public Task<Result<bool, Exception>> LoadSettingsAsync(CancellationToken cancellationToken = default) =>
        Try.RunAsync(async () =>
        {
            var filePath = GetSettingsFilePath();
            if (!File.Exists(filePath))
            {
                return true;
            }

            var json = await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            ApplySettingsFromJson(json);

            return true;
        });


    private void ApplySettingsFromJson(string json)
    {
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.TryGetProperty("SelectedTheme", out var theme))
        {
            _selectedTheme = theme.GetString() ?? _selectedTheme;
            this.RaiseAndSetIfChanged(ref _selectedTheme, SelectedTheme);
        }

        if (root.TryGetProperty("SelectedLanguage", out var language))
        {
            _selectedLanguage = language.GetString() ?? _selectedLanguage;
            this.RaiseAndSetIfChanged(ref _selectedLanguage, SelectedLanguage);
        }

        if (root.TryGetProperty("SelectedLayout", out var layout))
        {
            _selectedLayout = layout.GetString() ?? _selectedLayout;
            this.RaiseAndSetIfChanged(ref _selectedLayout,  SelectedLayout);
        }

        if (root.TryGetProperty("UserName", out var userName))
        {
            _userName = userName.GetString() ?? _userName;
            this.RaiseAndSetIfChanged(ref _userName, UserName);
        }
    }

}
