using System.Collections.ObjectModel;
using AstarOneDrive.UI.Common;

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
            ThemeChanged?.Invoke(this, value);
        }
    }
}

public event EventHandler<string>? ThemeChanged;
    
    public ObservableCollection<string> AvailableLayouts { get; } =
        new() { "Explorer", "Dashboard", "Terminal" };

    private string _selectedLayout = "Explorer";
    public string SelectedLayout
    {
        get => _selectedLayout;
        set { _selectedLayout = value; RaisePropertyChanged(); }
    }
}
