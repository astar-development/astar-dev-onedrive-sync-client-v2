# AstarOneDrive Multi‑Layout, Multi‑Theme Application Architecture Guide

This document outlines the cross‑platform AvaloniaUI application built with **Onion Architecture** that supports:

    Multiple layouts (Explorer, Dashboard, Terminal)
    Multiple themes (Light, Dark, Auto, Colorful, Professional, Hacker, High Contrast)
    Localization
    Multiple OneDrive accounts
    Folder tree selection
    User & Application settings

The solution follows strict dependency rules: UI → Application → Domain, with Infrastructure supporting both Application and Domain.

## 1. Project Structure

### Solution Organization (Onion Architecture)

```
AstarOneDrive.slnx
├── src/
│   ├── AstarOneDrive.Domain/                 (Core entities, interfaces)
│   │   ├── Entities/
│   │   │   └── SyncFile.cs
│   │   └── Interfaces/
│   │       └── ISyncFileRepository.cs
│   │
│   ├── AstarOneDrive.Application/            (Business logic, services)
│   │   ├── Interfaces/
│   │   │   └── ISyncService.cs
│   │   └── Services/
│   │       └── SyncService.cs
│   │
│   ├── AstarOneDrive.Infrastructure/         (Data access, external services)
│   │   └── Repositories/
│   │       └── OneDriveSyncFileRepository.cs
│   │
│   └── AstarOneDrive.UI/                     (Presentation layer)
│       ├── App.axaml
│       ├── App.axaml.cs
│       ├── Program.cs
│       ├── ViewLocator.cs
│       │
│       ├── Home/
│       │   ├── MainWindow.axaml
│       │   ├── MainWindow.axaml.cs
│       │   └── MainWindowViewModel.cs
│       │
│       ├── Layouts/                         (Swappable layout panels)
│       │   ├── ExplorerLayoutView.axaml
│       │   ├── ExplorerLayoutView.axaml.cs
│       │   ├── ExplorerLayoutViewModel.cs
│       │   ├── DashboardLayoutView.axaml
│       │   ├── DashboardLayoutView.axaml.cs
│       │   ├── DashboardLayoutViewModel.cs
│       │   ├── TerminalLayoutView.axaml
│       │   ├── TerminalLayoutView.axaml.cs
│       │   └── TerminalLayoutViewModel.cs
│       │
│       ├── AccountManagement/
│       │   ├── AccountListView.axaml
│       │   ├── AccountListView.axaml.cs
│       │   └── AccountListViewModel.cs
│       │
│       ├── FolderTrees/                     (Hierarchical folder browser)
│       │   ├── FolderTreeView.axaml
│       │   ├── FolderTreeView.axaml.cs
│       │   └── FolderTreeViewModel.cs
│       │
│       ├── SyncStatus/                      (Real-time sync feedback)
│       │   ├── SyncStatusView.axaml
│       │   ├── SyncStatusView.axaml.cs
│       │   └── SyncStatusViewModel.cs
│       │
│       ├── Logs/                            (Sync activity logs)
│       │   ├── LogsView.axaml
│       │   ├── LogsView.axaml.cs
│       │   └── LogsViewModel.cs
│       │
│       ├── Settings/                        (User & app settings)
│       │   ├── SettingsView.axaml
│       │   ├── SettingsView.axaml.cs
│       │   └── SettingsViewModel.cs
│       │
│       ├── Themes/                          (Theme resource dictionaries)
│       │   ├── Light.axaml
│       │   ├── Dark.axaml
│       │   ├── Auto.axaml
│       │   ├── Colorful.axaml
│       │   ├── Professional.axaml
│       │   ├── Hacker.axaml
│       │   └── HighContrast.axaml
│       │
│       ├── Locales/                         (Localization dictionaries)
│       │   └── en-US.axaml
│       │
│       ├── ThemeManager/
│       │   └── ThemeManager.cs
│       │
│       ├── Common/
│       │   ├── ViewModelBase.cs             (Reactive base for all VMs)
│       │   ├── RelayCommand.cs              (ICommand implementation)
│       │   └── LayoutType.cs                (Enum for layout switching)
│       │
│       └── Assets/                           (Images, icons, resources)
│
└── tests/
    ├── AstarOneDrive.Domain.Tests/
    ├── AstarOneDrive.Application.Tests/
    └── AstarOneDrive.UI.Tests/
        ├── ViewModels/
        └── Layouts/
```

## 2. Namespaces & Architecture Layers

### Domain Layer (`AstarOneDrive.Domain`)
No external dependencies. Contains:
- Core entities: `SyncFile`
- Repository interfaces: `ISyncFileRepository`

**Usage**: Referenced by Application and Infrastructure only.

### Application Layer (`AstarOneDrive.Application`)
Depends only on Domain. Contains:
- Business logic: `SyncService`
- Service interfaces: `ISyncService`

**Usage**: Referenced by Infrastructure and UI.

### Infrastructure Layer (`AstarOneDrive.Infrastructure`)
Depends on Domain and Application. Contains:
- Repository implementations: `OneDriveSyncFileRepository`
- External service integrations (OneDrive API client, database access, etc.)

**Usage**: Wired up in the UI layer's composition root.

### UI Layer (`AstarOneDrive.UI`)
Depends only on Application (never Infrastructure directly).
Namespaces:
- `AstarOneDrive.UI.Home` — Main window, host layout container
- `AstarOneDrive.UI.Layouts` — Three swappable layout implementations
- `AstarOneDrive.UI.AccountManagement` — Account list view and VM
- `AstarOneDrive.UI.FolderTrees` — Folder tree view and VM
- `AstarOneDrive.UI.SyncStatus` — Sync status view and VM
- `AstarOneDrive.UI.Logs` — Activity log view and VM
- `AstarOneDrive.UI.Settings` — Settings view and VM
- `AstarOneDrive.UI.Common` — Base classes, helpers, enums
- `AstarOneDrive.UI.ThemeManager` — Theme switching logic

## 3. Core UI Architecture: Layout Switching

### MainWindow (`AstarOneDrive.UI.Home.MainWindow`)
Top-level `ReactiveWindow<MainWindowViewModel>` that hosts all layouts.

**XAML**:
```xml
<ReactiveWindow xmlns="https://github.com/avaloniaui"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:vm="using:AstarOneDrive.UI.Home"
                x:Class="AstarOneDrive.UI.Home.MainWindow"
                x:DataType="vm:MainWindowViewModel">

    <ContentControl Content="{Binding CurrentLayoutView}" />
</ReactiveWindow>
```

### MainWindowViewModel (`AstarOneDrive.UI.Home.MainWindowViewModel`)
Orchestrates layout switching and holds shared view models.

```csharp
public class MainWindowViewModel : ViewModelBase
{
    public AccountListViewModel Accounts { get; }
    public FolderTreeViewModel FolderTree { get; }
    public SyncStatusViewModel Sync { get; }
    public LogsViewModel Logs { get; }
    public SettingsViewModel Settings { get; }

    private object? _currentLayoutView;
    public object? CurrentLayoutView
    {
        get => _currentLayoutView;
        set { _currentLayoutView = value; RaisePropertyChanged(); }
    }

    private LayoutType _currentLayout = LayoutType.Explorer;
    public LayoutType CurrentLayout
    {
        get => _currentLayout;
        set
        {
            _currentLayout = value;
            RaisePropertyChanged();
            ApplyLayout(value);
        }
    }

    public ICommand SwitchToExplorerCommand { get; }
    public ICommand SwitchToDashboardCommand { get; }
    public ICommand SwitchToTerminalCommand { get; }

    private void ApplyLayout(LayoutType layout)
    {
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
    }
}
```

**Key Points**:
- All shared view models are instantiated here and passed to child layouts via `DataContext`
- Each layout view receives `MainWindowViewModel` as its `DataContext`, making all shared VMs accessible
- Layout switching is command-driven and reactive
- This gives you zero duplication across layouts
## 4. Shared ViewModels (Single Instances Across All Layouts)

These view models contain all logic and state. All three layouts bind to the **same instances**.

### `AccountListViewModel` (`AstarOneDrive.UI.AccountManagement`)
- Add/remove OneDrive accounts
- Store account metadata
- Expose folder trees per account
- Trigger sync operations

### `FolderTreeViewModel` (`AstarOneDrive.UI.FolderTrees`)
- Represents hierarchical folder structure
- Supports checkboxes for selective sync
- Implements expand/collapse logic
- Tracks selection state

### `SyncStatusViewModel` (`AstarOneDrive.UI.SyncStatus`)
- Tracks current sync state (Idle, Syncing, Paused, Error)
- Exposes sync progress
- Provides Sync Now / Pause controls
- Aggregates sync statistics

### `LogsViewModel` (`AstarOneDrive.UI.Logs`)
- Real-time sync activity log
- Filters by status (Info, Warning, Error)
- Scrolls to latest entries automatically
- Clears log functionality

### `SettingsViewModel` (`AstarOneDrive.UI.Settings`)
- User settings (folder locations, sync frequency, etc.)
- Application settings (theme, language, window state, etc.)
- Persists settings to disk
- Raises events on theme/language changes

**Usage Pattern**:
```csharp
// In MainWindowViewModel constructor
Accounts = new AccountListViewModel();
FolderTree = new FolderTreeViewModel();
Sync = new SyncStatusViewModel();
Logs = new LogsViewModel();
Settings = new SettingsViewModel();

// Each layout receives MainWindowViewModel as DataContext
CurrentLayoutView = new ExplorerLayoutView { DataContext = this };

// Inside ExplorerLayoutView, all shared VMs are accessible:
// {Binding Accounts.Items} — account list
// {Binding FolderTree.Nodes} — folder tree
// {Binding Sync.Status} — sync status
// etc.
```
4. Stage 3 — Build Shared Components

Each layout reuses these components:
AccountListView

    Displays accounts

    “Add Account” button

FolderTreeView

    TreeView with checkboxes

    Expandable folders

SyncStatusView

    Current sync state

    Buttons: Sync Now, Pause

LogsView

    Real‑time sync logs

SettingsView

    User settings

    Application settings

These components are layout‑agnostic.
## 6. The Three Layouts (`AstarOneDrive.UI.Layouts`)

Each layout uses the same shared components, arranged differently. All inherit `ReactiveUserControl<TViewModel>`.

### 6.1 `ExplorerLayoutView`
Classic Windows Explorer–style desktop layout.

**Structure**:
```
┌─ Menu Bar (File, Layouts, Help) ─────────────────────┐
├──────────────────────────────────────────────────────┤
│                    Main Content Area                  │
│  ┌─────────────────┬──────────────────────────────┐  │
│  │   Folder Tree   │  Account List                │  │
│  │   (Left)        │  (Top Right)                 │  │
│  │                 │  ─────────────────────────   │  │
│  │                 │  Sync Status                 │  │
│  │                 │  (Bottom Right)              │  │
│  │                 │                              │  │
│  └─────────────────┴──────────────────────────────┘  │
├──────────────────────────────────────────────────────┤
│ Status Bar: "Idle" | "System" Theme                  │
└──────────────────────────────────────────────────────┘
```

**Components**:
- Top: Menu bar for File, Layouts, Help
- Left: `FolderTreeView` (full height)
- Right (top): `AccountListView`
- Right (bottom): `SyncStatusView`
- Bottom: Status bar with sync summary and theme name

### 6.2 `DashboardLayoutView`
Modern card-based layout with app bar.

**Structure**:
```
┌─ App Bar (Theme, Settings, Layout Menu) ────────────┐
├──────────────────────────────────────────────────────┤
│  ┌─ Left Drawer ───┐   ┌─ Main Content ───────┐    │
│  │ Accounts         │   │ Account Cards Grid   │    │
│  │ Settings         │   │ (Folder Tree below)  │    │
│  └──────────────────┘   └──────────────────────┘    │
│                    ┌─ Floating Panel ─┐             │
│                    │ Sync Status      │             │
│                    │ (Bottom Right)   │             │
│                    └──────────────────┘             │
└──────────────────────────────────────────────────────┘
```

**Components**:
- Top: App bar with theme dropdown, settings link, layout menu
- Left (drawer): `AccountListView` + `SettingsView` link
- Center: `FolderTreeView` with sync details
- Floating (bottom right): `SyncStatusView` (card style)

### 6.3 `TerminalLayoutView`
Technical split-pane layout for advanced users.

**Structure**:
```
┌─ Menu Bar ──────────────────────────────────────────┐
├──────────────────────────────────────────────────────┤
│  ┌────────────────┬──────────────────────────────┐  │
│  │ Account List   │   Tabs:                      │  │
│  │ (Top Left)     │  [Status] [Logs] [Settings] │  │
│  ├────────────────┤  ├─────────────────────────┤ │  │
│  │ Folder Tree    │  │  Tab Content            │ │  │
│  │ (Bottom Left)  │  │  (Sync/Logs/Settings)   │ │  │
│  │                │  │                         │ │  │
│  └────────────────┴──────────────────────────────┘  │
├──────────────────────────────────────────────────────┤
│ Console Bar: [Sync Now] [Pause] Status: "Idle"      │
└──────────────────────────────────────────────────────┘
```

**Components**:
- Top Left: `AccountListView` (stacked above folder tree)
- Bottom Left: `FolderTreeView`
- Right: Tabbed pane
  - **Status Tab**: `SyncStatusView`
  - **Logs Tab**: `LogsView`
  - **Settings Tab**: `SettingsView`
- Bottom: Console-style status bar with Sync/Pause buttons

**Layout Switching**:
Users can switch layouts via:
- Menu → Layouts → [Explorer | Dashboard | Terminal]
- Settings dialog
- Command in future versions

Layout preference is persisted to disk and restored on app restart.
## 7. Theme Support (`AstarOneDrive.UI.Themes` + `AstarOneDrive.UI.ThemeManager`)

Themes are resource dictionaries that apply globally to all layouts.

### Available Themes
```
Themes/
├── Light.axaml         — Bright, professional
├── Dark.axaml          — Dark mode
├── Auto.axaml          — Follow OS system preference
├── Colorful.axaml      — Vibrant accent colors
├── Professional.axaml  — Corporate blue/gray
├── Hacker.axaml        — Green-on-black terminal style
└── HighContrast.axaml  — Maximum accessibility
```

### `ThemeManager.cs` (`AstarOneDrive.UI.ThemeManager`)

```csharp
public static class ThemeManager
{
    public static void ApplyTheme(string themeName)
    {
        var app = Application.Current;
        app.Styles.Clear();

        app.Styles.Add(new StyleInclude(new Uri("avares://AstarOneDrive.UI/"))
        {
            Source = new Uri($"avares://AstarOneDrive.UI/Themes/{themeName}.axaml")
        });
    }
}
```

**Usage**:
```csharp
// Apply theme on app startup
ThemeManager.ApplyTheme("Dark");

// Theme change on settings update
Settings.ThemeChanged += (_, themeName) => ThemeManager.ApplyTheme(themeName);
```

**Theme Application**:
- Themes apply automatically to all layouts without restart
- Selected theme is persisted in `SettingsViewModel`
- All XAML views automatically inherit theme colors/styles
## 8. Localization (`AstarOneDrive.UI.Locales`)

Localization strings are stored in XAML resource dictionaries, keyed by culture.

### Locale Files
```
Locales/
└── en-US.axaml         — English (US) strings
    (Future: fr-FR.axaml, de-DE.axaml, etc.)
```

### Example Locale Dictionary (`en-US.axaml`)
```xml
<ResourceDictionary xmlns="https://github.com/avaloniaui">
    <!-- Menu Items -->
    <System:String x:Key="Menu_File">File</System:String>
    <System:String x:Key="Menu_Layouts">Layouts</System:String>
    <System:String x:Key="Menu_Help">Help</System:String>
    
    <!-- Buttons -->
    <System:String x:Key="Btn_AddAccount">Add Account</System:String>
    <System:String x:Key="Btn_SyncNow">Sync Now</System:String>
    <System:String x:Key="Btn_Pause">Pause</System:String>
    
    <!-- Status Messages -->
    <System:String x:Key="Status_Idle">Idle</System:String>
    <System:String x:Key="Status_Syncing">Syncing...</System:String>
    <System:String x:Key="Status_Error">Error</System:String>
</ResourceDictionary>
```

### Usage in XAML
```xml
<TextBlock Text="{DynamicResource Menu_File}" />
<Button Content="{DynamicResource Btn_SyncNow}" />
```

### Future Language Addition
1. Create `Locales/fr-FR.axaml` with French translations
2. Add language selector to `SettingsView`
3. Implement `LocalizationManager.SetLanguage(culture)` to swap dictionaries
4. Persist language choice in `SettingsViewModel`

## 9. Menu & Settings UI

### Menu Structure (All Layouts)
```xml
<Menu DockPanel.Dock="Top">
    <MenuItem Header="{DynamicResource Menu_File}">
        <MenuItem Header="{DynamicResource Menu_UserSettings}" />
        <MenuItem Header="{DynamicResource Menu_AppSettings}" />
    </MenuItem>

    <MenuItem Header="{DynamicResource Menu_Layouts}">
        <MenuItem Header="Explorer" Command="{Binding SwitchToExplorerCommand}" />
        <MenuItem Header="Dashboard" Command="{Binding SwitchToDashboardCommand}" />
        <MenuItem Header="Terminal" Command="{Binding SwitchToTerminalCommand}" />
    </MenuItem>
</Menu>
```

### Settings Access
Settings can be opened via:
- Menu → File → User/Application Settings
- Settings tab in Terminal layout
- Dashboard left drawer

All routes open `SettingsView` bound to `SettingsViewModel`.

### SettingsViewModel
Manages:
- **User Settings**: Folder sync paths, sync frequency, notification preferences
- **Application Settings**: Theme, language, window state, auto-launch on startup
- Persistence to disk (JSON or similar)
- Event notifications for theme/language/layout changes


## 10. OneDrive Integration (Business Logic Layer)

OneDrive integration lives in **Application** and **Infrastructure** layers, independent of UI.

### Application Layer Contracts
- `ISyncService` — Business logic for sync operations
- Models for sync state, progress, errors

### Infrastructure Layer Implementation
- `OneDriveSyncFileRepository` — Calls OneDrive API
- OAuth login & token refresh
- Folder structure fetching
- File diff computation
- Sync conflict resolution

### Data Flow
```
UI View → ViewModel
        ↓ (ICommand)
        MainWindowViewModel.SyncStatusViewModel
        ↓ (Property binding)
        SyncService (ISyncService)
        ↓ (Calls)
        OneDriveSyncFileRepository
        ↓ (HTTP)
        OneDrive API
```

### Key Components
- **OAuth Flow**: Handle login, refresh tokens, store securely
- **Folder Sync**: Build tree, compute diffs, track changes
- **Conflict Resolution**: User-defined rules (keep newer, replace, skip)
- **Logging**: Feed events to `LogsViewModel` for display

UI never references Infrastructure directly—all service calls go through Application layer interfaces.

## 11. Testing Strategy

### Test Projects
- `AstarOneDrive.Domain.Tests` — Entity and interface contract tests
- `AstarOneDrive.Application.Tests` — Service logic tests (mocked Infrastructure)
- `AstarOneDrive.UI.Tests` — ViewModel and control type tests

### Testing Stack
- **Framework**: xUnit.net v3
- **Assertions**: Shouldly (fluent API)
- **Mocking**: NSubstitute (for service/repository mocks)

### TDD Workflow (Mandatory)
1. Write failing test first (Red)
2. Commit test
3. Write minimal production code to pass (Green)
4. Refactor while keeping tests green

All PR commits must demonstrate this Red → Green → Refactor flow.

## 12. Future Enhancements

- **Plugin System**: Dynamic layout/theme loading
- **Additional Layouts**: Tree view, timeline, grid
- **Additional Themes**: System integration by OS
- **Cloud Integration**: Sync OneDrive account settings
- **Mobile Support**: iOS/Android via Avalonia
- **Web Support**: WebAssembly deployment
- **Advanced Search**: Full-text indexing of synced files
- **Selective Sync UI**: Fine-grained control per account/folder
- **Bandwidth Throttling**: User-configurable sync speeds
