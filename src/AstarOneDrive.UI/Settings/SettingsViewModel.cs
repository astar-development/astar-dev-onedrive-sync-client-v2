using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using AstarOneDrive.UI.Common;
using static AstarOneDrive.UI.ThemeManager.ThemeManager;

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

    public ObservableCollection<string> AvailableLanguages { get; } = ["en-US"];

    private string _selectedLanguage = "en-US";
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

    public async Task SaveSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = new SettingsData(SelectedTheme, SelectedLanguage, SelectedLayout, UserName);
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(GetSettingsFilePath(), json, cancellationToken);
    }

    public async Task LoadSettingsAsync(CancellationToken cancellationToken = default)
    {
        var filePath = GetSettingsFilePath();
        if (!File.Exists(filePath))
        {
            return;
        }

        var json = await File.ReadAllTextAsync(filePath, cancellationToken);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.TryGetProperty("SelectedTheme", out var theme))
        {
            SelectedTheme = theme.GetString() ?? "Light";
        }

        if (root.TryGetProperty("SelectedLanguage", out var language))
        {
            SelectedLanguage = language.GetString() ?? "en-US";
        }

        if (root.TryGetProperty("SelectedLayout", out var layout))
        {
            SelectedLayout = layout.GetString() ?? "Explorer";
        }

        if (root.TryGetProperty("UserName", out var userName))
        {
            UserName = userName.GetString() ?? "User";
        }
    }

}
