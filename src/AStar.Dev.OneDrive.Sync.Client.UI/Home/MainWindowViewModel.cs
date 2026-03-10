using System.Windows.Input;
using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.UI.AccountManagement;
using AStar.Dev.OneDrive.Sync.Client.UI.Common;
using AStar.Dev.OneDrive.Sync.Client.UI.Composition;
using AStar.Dev.OneDrive.Sync.Client.UI.FolderTrees;
using AStar.Dev.OneDrive.Sync.Client.UI.Layouts;
using AStar.Dev.OneDrive.Sync.Client.UI.Logs;
using AStar.Dev.OneDrive.Sync.Client.UI.Settings;
using AStar.Dev.OneDrive.Sync.Client.UI.SyncStatus;
using Avalonia.Threading;
using ReactiveUI;
using System.ComponentModel;
using System.Collections.Specialized;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Home;

/// <summary>
/// ViewModel for the main application window, coordinating all major feature areas.
/// </summary>
public class MainWindowViewModel : ViewModelBase
{
    private const string DefaultScopeId = "drive-root";
    private static readonly string DefaultRootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".astar-sync", "default");

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
            if(field == value)
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
    /// Gets the linked account count used by dashboard metrics.
    /// </summary>
    public int LinkedAccountCount
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Gets the selected account email used by dashboard metrics.
    /// </summary>
    public string SelectedAccountEmail
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    } = "No account selected";

    /// <summary>
    /// Gets the current sync progress percentage used by dashboard metrics.
    /// </summary>
    public int SyncProgressMetric
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Gets the terminal operational status text.
    /// </summary>
    public string TerminalOperationalStatus
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    } = "Idle";

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
    /// </summary>
    /// <param name="initializeLayoutView">Whether to initialize the layout view immediately.</param>
    public MainWindowViewModel(string? databasePath = null, bool initializeLayoutView = true)
    {
        Accounts = new AccountListViewModel(databasePath);
        FolderTree = new FolderTreeViewModel(databasePath);
        Sync = new SyncStatusViewModel(orchestratorService: ResolveOrchestratorService());
        Logs = new LogsViewModel();
        Settings = new SettingsViewModel();
        Accounts.PropertyChanged += OnAccountsPropertyChanged;
        Accounts.Accounts.CollectionChanged += OnAccountsCollectionChanged;
        Sync.PropertyChanged += OnSyncPropertyChanged;
        Sync.RecentActivity.CollectionChanged += OnSyncRecentActivityChanged;
        Settings.ThemeChanged += (_, themeName) => ThemeManager.ThemeManager.ApplyTheme(themeName);

        SwitchToExplorerCommand = new RelayCommand(_ => CurrentLayout = LayoutType.Explorer);
        SwitchToDashboardCommand = new RelayCommand(_ => CurrentLayout = LayoutType.Dashboard);
        SwitchToTerminalCommand = new RelayCommand(_ => SetTerminalLayout(0));
        OpenUserSettingsCommand = new RelayCommand(_ => SetTerminalLayout(2));
        OpenAppSettingsCommand = new RelayCommand(_ => SetTerminalLayout(2));

        if(initializeLayoutView)
        {
            ApplyLayout(LayoutType.Explorer);
        }

        RefreshDashboardMetrics();
        RefreshTerminalOperationalStatus();
        UpdateSyncRunContext();
        if(initializeLayoutView && Accounts.SelectedAccount is not null)
        {
            _ = ReloadFolderTreeForSelectedAccountAsync();
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
        if(Dispatcher.UIThread is null || !Dispatcher.UIThread.CheckAccess())
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

    private void OnAccountsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(!string.Equals(e.PropertyName, nameof(AccountListViewModel.SelectedAccount), StringComparison.Ordinal))
        {
            return;
        }

        UpdateSyncRunContext();
        _ = ReloadFolderTreeForSelectedAccountAsync();
        RefreshDashboardMetrics();
    }

    private void OnAccountsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => RefreshDashboardMetrics();

    private void OnSyncPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(string.Equals(e.PropertyName, nameof(SyncStatusViewModel.Status), StringComparison.Ordinal)
            || string.Equals(e.PropertyName, nameof(SyncStatusViewModel.ProgressPercentage), StringComparison.Ordinal)
            || string.Equals(e.PropertyName, nameof(SyncStatusViewModel.SyncError), StringComparison.Ordinal))
        {
            RefreshDashboardMetrics();
            RefreshTerminalOperationalStatus();
        }
    }

    private void OnSyncRecentActivityChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if(e.Action is not (NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Replace))
        {
            return;
        }

        if(Sync.RecentActivity.Count == 0)
        {
            return;
        }

        SyncActivityEntry latest = Sync.RecentActivity[^1];
        Logs.Append($"[{latest.Level}] {latest.Message}");
    }

    private void RefreshDashboardMetrics()
    {
        LinkedAccountCount = Accounts.Accounts.Count;
        SelectedAccountEmail = Accounts.SelectedAccount?.Email ?? "No account selected";
        SyncProgressMetric = Sync.ProgressPercentage;
    }

    private void RefreshTerminalOperationalStatus()
        => TerminalOperationalStatus = string.IsNullOrWhiteSpace(Sync.SyncError)
            ? $"{Sync.Status} ({Sync.ProgressPercentage}%)"
            : $"{Sync.Status}: {Sync.SyncError}";

    private async Task ReloadFolderTreeForSelectedAccountAsync()
    {
        AccountInfo? selectedAccount = Accounts.SelectedAccount;
        if(selectedAccount is null)
        {
            FolderTree.ClearTree();
            return;
        }

        _ = await FolderTree.LoadTreeAsync(selectedAccount.Id)
            .MatchAsync(
                _ => Task.CompletedTask,
                _ => Task.CompletedTask);
    }

    private void UpdateSyncRunContext()
    {
        AccountInfo? selectedAccount = Accounts.SelectedAccount;
        if(selectedAccount is null)
        {
            Sync.ClearRunContext();
            return;
        }

        var rootPath = string.IsNullOrWhiteSpace(selectedAccount.LocalSyncRootPath)
            ? DefaultRootPath
            : selectedAccount.LocalSyncRootPath;
        Sync.SetRunContext(selectedAccount.Id, DefaultScopeId, rootPath, useStartupScan: true);
    }

    private static ISyncOrchestratorService? ResolveOrchestratorService()
    {
        try
        {
            return CompositionRoot.Resolve<ISyncOrchestratorService>();
        }
        catch(InvalidOperationException)
        {
            return null;
        }
    }
}
