using System.Windows.Input;
using AstarOneDrive.UI.AccountManagement;
using AstarOneDrive.UI.Common;
using AstarOneDrive.UI.FolderTrees;
using AstarOneDrive.UI.Layouts;
using AstarOneDrive.UI.Logs;
using AstarOneDrive.UI.Settings;
using AstarOneDrive.UI.SyncStatus;

namespace AstarOneDrive.UI.Home;

public class MainWindowViewModel : ViewModelBase
{
    // Shared ViewModels used by all layouts
    public AccountListViewModel Accounts { get; }
    public FolderTreeViewModel FolderTree { get; }
    public SyncStatusViewModel Sync { get; }
    public LogsViewModel Logs { get; }
    public SettingsViewModel Settings { get; }

    // Current layout view (bound to MainWindow ContentControl)
    private object? _currentLayoutView;
    public object? CurrentLayoutView
    {
        get => _currentLayoutView;
        set => this.RaiseAndSetIfChanged(ref _currentLayoutView, value);
    }

    // Current layout type
    private LayoutType _currentLayout = LayoutType.Explorer;
    public LayoutType CurrentLayout
    {
        get => _currentLayout;
        set
        {
            if (_currentLayout == value) return;
            this.RaiseAndSetIfChanged(ref _currentLayout, value);
            ApplyLayout(value);
        }
    }

    // Commands for switching layouts
    public ICommand SwitchToExplorerCommand { get; }
    public ICommand SwitchToDashboardCommand { get; }
    public ICommand SwitchToTerminalCommand { get; }

    public MainWindowViewModel()
    {
        // Instantiate shared ViewModels
        Accounts = new AccountListViewModel();
        FolderTree = new FolderTreeViewModel();
        Sync = new SyncStatusViewModel();
        Logs = new LogsViewModel();Settings = new SettingsViewModel();
Settings.ThemeChanged += (_, themeName) => ThemeManager.ThemeManager.ApplyTheme(themeName);

        // Commands
        SwitchToExplorerCommand = new RelayCommand(_ => CurrentLayout = LayoutType.Explorer);
        SwitchToDashboardCommand = new RelayCommand(_ => CurrentLayout = LayoutType.Dashboard);
        SwitchToTerminalCommand = new RelayCommand(_ => CurrentLayout = LayoutType.Terminal);

        // Default layout
        ApplyLayout(LayoutType.Explorer);
        Settings.LayoutChanged += (_, layoutName) =>
{
    switch (layoutName)
    {
        case "Explorer":
            CurrentLayout = LayoutType.Explorer;
            break;

        case "Dashboard":
            CurrentLayout = LayoutType.Dashboard;
            break;

        case "Terminal":
            CurrentLayout = LayoutType.Terminal;
            break;
    }
};

    }

    private void ApplyLayout(LayoutType layout)
    {
        // Each layout view receives this MainViewModel as its DataContext
        // so all shared ViewModels are available to child components.

        switch (layout)
        {
            case LayoutType.Explorer:
                CurrentLayoutView = new ExplorerLayoutView { DataContext = this };
                break;

            case LayoutType.Dashboard:
                CurrentLayoutView = new DashboardLayoutView { DataContext = this };
                break;

            case LayoutType.Terminal:
                CurrentLayoutView = new TerminalLayoutView { DataContext = this };
                break;
        }
        
        Settings.SelectedLayout = layout.ToString();
    }
}
