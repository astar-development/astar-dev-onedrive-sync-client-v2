using System.Windows.Input;
using AStar.Dev.OneDrive.Sync.Client.UI.AccountManagement;
using AStar.Dev.OneDrive.Sync.Client.UI.Common;
using AStar.Dev.OneDrive.Sync.Client.UI.FolderTrees;
using AStar.Dev.OneDrive.Sync.Client.UI.Layouts;
using AStar.Dev.OneDrive.Sync.Client.UI.Logs;
using AStar.Dev.OneDrive.Sync.Client.UI.Settings;
using AStar.Dev.OneDrive.Sync.Client.UI.SyncStatus;
using Avalonia.Threading;
using ReactiveUI;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Home;

public class MainWindowViewModel : ViewModelBase
{
    // Shared ViewModels used by all layouts
    public AccountListViewModel Accounts { get; }
    public FolderTreeViewModel FolderTree { get; }
    public SyncStatusViewModel Sync { get; }
    public LogsViewModel Logs { get; }
    public SettingsViewModel Settings { get; }

    // Current layout view (bound to MainWindow ContentControl)
    public object? CurrentLayoutView
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    // Current layout type
    public LayoutType CurrentLayout
    {
        get;
        set
        {
            if(field == value)
                return;
            _ = this.RaiseAndSetIfChanged(ref field, value);
            ApplyLayout(value);
        }
    } = LayoutType.Explorer;

    // Commands for switching layouts
    public ICommand SwitchToExplorerCommand { get; }
    public ICommand SwitchToDashboardCommand { get; }
    public ICommand SwitchToTerminalCommand { get; }
    public ICommand OpenUserSettingsCommand { get; }
    public ICommand OpenAppSettingsCommand { get; }

    public int TerminalSelectedTabIndex
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public MainWindowViewModel(bool initializeLayoutView = true)
    {
        // Instantiate shared ViewModels
        Accounts = new AccountListViewModel();
        FolderTree = new FolderTreeViewModel();
        Sync = new SyncStatusViewModel();
        Logs = new LogsViewModel();
        Settings = new SettingsViewModel();
        Settings.ThemeChanged += (_, themeName) => ThemeManager.ThemeManager.ApplyTheme(themeName);

        // Commands
        SwitchToExplorerCommand = new RelayCommand(_ => CurrentLayout = LayoutType.Explorer);
        SwitchToDashboardCommand = new RelayCommand(_ => CurrentLayout = LayoutType.Dashboard);
        SwitchToTerminalCommand = new RelayCommand(_ => SetTerminalLayout(0));
        OpenUserSettingsCommand = new RelayCommand(_ => SetTerminalLayout(2));
        OpenAppSettingsCommand = new RelayCommand(_ => SetTerminalLayout(2));

        if(initializeLayoutView)
        {
            ApplyLayout(LayoutType.Explorer);
        }

        Settings.LayoutChanged += (_, layoutName) =>
                    {
                        switch(layoutName)
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

        if(!Dispatcher.UIThread.CheckAccess())
        {
            Settings.SelectedLayout = layout.ToString();
            return;
        }

        switch(layout)
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

    private void SetTerminalLayout(int tabIndex)
    {
        TerminalSelectedTabIndex = tabIndex;
        CurrentLayout = LayoutType.Terminal;
    }
}
