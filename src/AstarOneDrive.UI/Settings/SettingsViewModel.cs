using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using AstarOneDrive.UI.Common;
using AstarOneDrive.UI.ThemeManager;

namespace AstarOneDrive.UI.Settings;

public class SettingsViewModel : ViewModelBase
{
    // User settings
    private string _userName = "User";
    public string UserName
    {
        get => _userName;
        set { _userName = value; RaisePropertyChanged(); }
    }

    public ObservableCollection<string> AvailableLanguages { get; } =
        new() { "en-US" };

    private string _selectedLanguage = "en-US";
    public string SelectedLanguage
    {
        get => _selectedLanguage;
        set { _selectedLanguage = value; RaisePropertyChanged(); }
    }

    // Application settings
    public ObservableCollection<string> AvailableThemes { get; } =
        new() { "Light", "Dark", "Auto", "Colorful", "Professional", "Hacker", "HighContrast" };

    private string _selectedTheme = "Light";
public string SelectedTheme
{
    get => _selectedTheme;
    set
    {
        if (_selectedTheme != value)
        {
            _selectedTheme = value;
            RaisePropertyChanged();
            ThemeManager.ThemeManager.ApplyTheme(value);
            ThemeChanged?.Invoke(this, value);
        }
    }
}

public event EventHandler<string>? ThemeChanged;
public event EventHandler<string>? LayoutChanged;

    public ObservableCollection<string> AvailableLayouts { get; } =
        new() { "Explorer", "Dashboard", "Terminal" };

    private string _selectedLayout = "Explorer";
public string SelectedLayout
{
    get => _selectedLayout;
    set
    {
        if (_selectedLayout != value)
        {
            _selectedLayout = value;
            RaisePropertyChanged();
            LayoutChanged?.Invoke(this, value);
        }
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
        var settings = new
        {
            SelectedTheme,
            SelectedLanguage,
            SelectedLayout,
            UserName
        };

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
