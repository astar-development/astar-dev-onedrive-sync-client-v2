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

/// <summary>
/// ViewModel for the main application window, coordinating all major feature areas.
/// </summary>
public class MainWindowViewModel : ViewModelBase
{
    /// <summary>
    /// Gets the account management view model.
    /// </summary>
    public AccountListViewModel Accounts { get; }
    
    /// <summary>
    /// Gets the folder tree view model.
    /// </summary>
    public FolderTreeViewModel FolderTree { get; }
    
    /// <summary>
    /// Gets the synchronization status view model.
    /// </summary>
    public SyncStatusViewModel Sync { get; }
    
    /// <summary>
    /// Gets the logs view model.
    /// </summary>
    public LogsViewModel Logs { get; }
    
    /// <summary>
    /// Gets the settings view model.
    /// </summary>
    public SettingsViewModel Settings { get; }

    /// <summary>
    /// Gets or sets the current layout view instance.
    /// </summary>
    public object? CurrentLayoutView
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Gets or sets the current layout type.
    /// </summary>
    public LayoutType CurrentLayout
    {
        get;
        set
        {
            if (field == value)
                return;
            _ = this.RaiseAndSetIfChanged(ref field, value);
            ApplyLayout(value);
        }
    } = LayoutType.Explorer;

    /// <summary>
    /// Gets the command to switch to the Explorer layout.
    /// </summary>
    public ICommand SwitchToExplorerCommand { get; }
    
    /// <summary>
    /// Gets the command to switch to the Dashboard layout.
    /// </summary>
    public ICommand SwitchToDashboardCommand { get; }
    
    /// <summary>
    /// Gets the command to switch to the Terminal layout.
    /// </summary>
    public ICommand SwitchToTerminalCommand { get; }
    
    /// <summary>
    /// Gets the command to open user settings.
    /// </summary>
    public ICommand OpenUserSettingsCommand { get; }
    
    /// <summary>
    /// Gets the command to open application settings.
    /// </summary>
    public ICommand OpenAppSettingsCommand { get; }

    /// <summary>
    /// Gets or sets the selected tab index in the Terminal layout.
    /// </summary>
    public int TerminalSelectedTabIndex
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
    /// </summary>
    /// <param name="initializeLayoutView">Whether to initialize the layout view immediately.</param>
    public MainWindowViewModel(bool initializeLayoutView = true)
    {
        Accounts = new AccountListViewModel();
        FolderTree = new FolderTreeViewModel();
        Sync = new SyncStatusViewModel();
        Logs = new LogsViewModel();
        Settings = new SettingsViewModel();
        Settings.ThemeChanged += (_, themeName) => ThemeManager.ThemeManager.ApplyTheme(themeName);

        SwitchToExplorerCommand = new RelayCommand(_ => CurrentLayout = LayoutType.Explorer);
        SwitchToDashboardCommand = new RelayCommand(_ => CurrentLayout = LayoutType.Dashboard);
        SwitchToTerminalCommand = new RelayCommand(_ => SetTerminalLayout(0));
        OpenUserSettingsCommand = new RelayCommand(_ => SetTerminalLayout(2));
        OpenAppSettingsCommand = new RelayCommand(_ => SetTerminalLayout(2));

        if (initializeLayoutView)
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
        if (Dispatcher.UIThread is null || !Dispatcher.UIThread.CheckAccess())
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
