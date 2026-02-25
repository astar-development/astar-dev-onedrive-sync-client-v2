## Plan: Complete UI Implementation (TDD)

**TL;DR:** 16 phases over ~3-4 weeks. Start with resource files (locales), then implement & test each ViewModel in order: AccountList → FolderTree → SyncStatus → Settings → LayoutVMs → SQLite/EF Core persistence → Integration. Final phases add OneDrive stub integration and error handling. Each phase has a failing test first, minimal implementation, then verification.

**Steps**

### **Phase 1: Localization Infrastructure (Red) ✅**
Create minimal localization system and test it.

1. ✅ **Test First** — tests/AStar.Dev.OneDrive.Sync.Client.UI.Tests/LocalizationTests.cs
   - Test: `LocalizationManager` can load en-GB dictionary
   - Test: `DynamicResource` keys resolve from dictionary
   - Test: Fallback behavior when key missing

2. ✅ **Create Locale Files**
   - src/AStar.Dev.OneDrive.Sync.Client.UI/Locales/en-GB.axaml — All UI strings (Menu, Buttons, Status, etc.)

3. ✅ **Implement Manager** — src/AStar.Dev.OneDrive.Sync.Client.UI/Localization/LocalizationManager.cs
   - `SetLanguage(string culture)` — Load XAML dictionary
   - `GetString(key)` — Retrieve localized string

4. ✅ **Verify**
   - Localization tests pass (1 test class)
   - All XAML views updated to use `{DynamicResource}` instead of hard-coded strings
   - Build succeeds

---

### **Phase 2: SettingsViewModel Implementation & Tests ✅**
Most foundational—settings affect theme, language, layout memory.

1. ✅ **Test First** — tests/AStar.Dev.OneDrive.Sync.Client.UI.Tests/ViewModels/Settings/SettingsViewModelTests.cs
   - Test: Constructor initializes with defaults (Theme=Light, Language=en-GB, SelectedLayout=Explorer)
   - Test: Setting `SelectedTheme` fires `PropertyChanged`
   - Test: Setting `SelectedTheme` calls `ThemeManager.ApplyTheme()`
   - Test: `SaveSettings()` persists to disk (mock file system)
   - Test: `LoadSettings()` restores from disk
   - Test: Available themes list is non-empty
   - Test: `ThemeChanged` event fires on theme change

2. ✅ **Implement** — Update SettingsViewModel.cs
   - Add `SelectedTheme` property with event
   - Add `SelectedLanguage` property
   - Add `SaveSettings()` / `LoadSettings()` (JSON to disk)
   - Trigger `ThemeManager.ApplyTheme()` on theme change
   - Add `AvailableThemes` list (hardcoded: Light, Dark, Professional, Colorful, Hacker, HighContrast)

3. ✅ **Verify**
   - 7 tests pass in SettingsViewModelTests
   - `dotnet build` succeeds
   - Launch app, switch theme in Settings → app theme changes immediately

---

### **Phase 3: ThemeManager & System Integration ✅**
Wire theme switching through the app.

1. ✅ **Test First** — tests/AStar.Dev.OneDrive.Sync.Client.UI.Tests/ThemeManager/ThemeManagerTests.cs
   - Test: `ApplyTheme("Dark")` loads Dark.axaml without error
   - Test: `ApplyTheme()` clears old theme before applying new
   - Test: Invalid theme name throws `InvalidOperationException`

2. ✅ **Implement** — Update ThemeManager.cs
   - Add error handling for missing theme files
   - Add logging for theme switches
   - Test that `Application.Current.Styles` is updated

3. ✅ **Wire in App.axaml.cs**
   - Remove duplicate theme assignments (line 14 & 26 conflict)
   - Load initial theme from `SettingsViewModel.SelectedTheme` on startup

4. ✅ **Verify**
   - 3 tests pass in ThemeManagerTests
   - App starts with saved theme
   - Theme switching works without crashes
   - All theme files exist and load

---

### **Phase 4: AccountListViewModel Implementation & Tests ✅**
First main ViewModel—represents account list data.

1. ✅ **Test First** — tests/AStar.Dev.OneDrive.Sync.Client.UI.Tests/ViewModels/AccountManagement/AccountListViewModelTests.cs
   - Test: Constructor initializes empty `Accounts` collection
   - Test: `Accounts` observable collection notifies on items added
   - Test: `AddAccountCommand` can be executed
   - Test: `RemoveAccountCommand` can be executed (when account selected)
   - Test: `ManageAccountCommand` can be executed
   - Test: `SelectedAccount` property works
   - Test: Accounts list can be saved/loaded from disk (mock file system)

2. ✅ **Implement** — Update AccountListViewModel.cs
   - Add `ObservableCollection<AccountInfo>` for accounts
   - Implement `ICommand` properties: `AddAccountCommand`, `RemoveAccountCommand`, `ManageAccountCommand`
   - Add `SelectedAccount` property with `PropertyChanged`
   - Implement save/load persistence
   - Create placeholder `AccountInfo` model class (id, email, quota)

3. ✅ **Create Model** — src/AStar.Dev.OneDrive.Sync.Client.UI/AccountManagement/AccountInfo.cs
   ```csharp
   public record AccountInfo(string Id, string Email, long QuotaBytes, long UsedBytes);
   ```

4. ✅ **Verify**
   - 7 tests pass
   - Build succeeds
   - Launch app, add/remove accounts in UI (command executions visible via debug breakpoints)
   - Accounts persist after restart

---

### **Phase 5: FolderTreeViewModel Implementation & Tests ✅**
Handle folder hierarchy data.

1. ✅ **Test First** — tests/AStar.Dev.OneDrive.Sync.Client.UI.Tests/ViewModels/FolderTrees/FolderTreeViewModelTests.cs
   - Test: Constructor initializes with empty node collection
   - Test: `Nodes` (ObservableCollection) notifies on add/remove
   - Test: `SelectedNode` property works
   - Test: `ToggleNodeSelection` command toggles checkbox
   - Test: `ExpandNode` / `CollapseNode` work
   - Test: Folder hierarchy can be persisted/restored

2. ✅ **Implement** — Update FolderTreeViewModel.cs
   - Add `ObservableCollection<FolderNode>` for tree nodes
   - Add `SelectedNode` property
   - Implement `ToggleNodeSelection`, `ExpandNode`, `CollapseNode` commands
   - Remove placeholder hard-coded data; load from saved state on init
   - Add persistence logic

3. ✅ **Create Model** — src/AStar.Dev.OneDrive.Sync.Client.UI/FolderTrees/FolderNode.cs
   ```csharp
   public record FolderNode(string Id, string Name, bool IsSelected, bool IsExpanded, ObservableCollection<FolderNode> Children);
   ```

4. ✅ **Verify**
   - 6 tests pass
   - Build succeeds
   - Folder tree renders in UI without hard-coded data
   - Selection state persists

---

### **Phase 6: SyncStatusViewModel Implementation & Tests ✅**
Manage sync state and status display.

1. ✅ **Test First** — tests/AStar.Dev.OneDrive.Sync.Client.UI.Tests/ViewModels/SyncStatus/SyncStatusViewModelTests.cs
   - Test: Constructor initializes `Status` to "Idle"
   - Test: `StartSyncCommand` sets status to "Syncing..."
   - Test: `PauseSyncCommand` sets status to "Paused"
   - Test: `ProgressPercentage` property updates
   - Test: `RecentActivity` logs entries
   - Test: `SyncError` property stores errors
   - Test: `PropertyChanged` fires on state changes

2. ✅ **Implement** — Update SyncStatusViewModel.cs
   - Replace TODO placeholders with real properties
   - Add `Status` property (Idle, Syncing, Paused, Error)
   - Add `ProgressPercentage` property
   - Add `RecentActivity` ObservableCollection
   - Implement `StartSyncCommand`, `PauseSyncCommand` commands
   - Add event-based activity logging

3. ✅ **Create Model** — src/AStar.Dev.OneDrive.Sync.Client.UI/SyncStatus/SyncActivityEntry.cs
   ```csharp
   public record SyncActivityEntry(DateTime Timestamp, string Level, string Message);
   ```

4. ✅ **Verify**
   - 7 tests pass
   - Build succeeds
   - UI shows sync status, starts/pauses sync (command works)
   - Activity log appears in real time

---

### **Phase 7: LayoutViewModel Tests & Implementation ✅**
Ensure all three layout ViewModels are functional.

1. ✅ **Test First** — Create tests/AStar.Dev.OneDrive.Sync.Client.UI.Tests/ViewModels/Layouts/LayoutViewModelTests.cs
   - Test: Each layout ViewModel (Explorer, Dashboard, Terminal) can be instantiated
   - Test: Each receives `MainWindowViewModel` as context and can access shared VMs
   - Test: Layout-specific commands execute

2. ✅ **Implement** — Create/update individual layout VMs if needed
   - src/AStar.Dev.OneDrive.Sync.Client.UI/Layouts/ExplorerLayoutViewModel.cs — Refactor from placeholder
   - DashboardLayoutViewModel.cs
   - src/AStar.Dev.OneDrive.Sync.Client.UI/Layouts/TerminalLayoutViewModel.cs

3. ✅ **Verify**
   - 3 tests pass (one per layout)
   - All layouts instantiate without error
   - Switching layouts in UI works

---

### **Phase 7a: SQLite + EF Core Persistence Foundation ✅**
Introduce a database-backed persistence layer for configuration, accounts, and file metadata.

Reference: see [docs/database-schema.md](database-schema.md) for table definitions, constraints, indexes, and migration/runtime contract.

1. ✅ **Test First** — tests/AStar.Dev.OneDrive.Sync.Client.Infrastructure.Tests/Data/SqlitePersistenceTests.cs
   - Test: `AstarOneDriveDbContextModel` can create/connect to SQLite database using platform-specific app data location
   - Test: Runtime startup applies pending migrations automatically
   - Test: `Settings`, `Accounts`, and `SyncFiles` entities can be inserted/queried
   - Test: Required fields reject nulls; optional fields allow null only where explicitly configured
   - Test: Max length constraints are enforced for configured text columns

2. ✅ **Implement Infrastructure** — src/AStar.Dev.OneDrive.Sync.Client.Infrastructure/Data
   - Add latest EF Core SQLite packages (`Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Design`, `Microsoft.EntityFrameworkCore.Tools`)
   - Create `AstarOneDriveDbContextModel` with `DbSet` definitions for configuration, account, and file/sync metadata
   - Add per-entity `IEntityTypeConfiguration<T>` classes to configure schema, keys, indexes, max lengths, and nullability
   - Store migrations in `src/AStar.Dev.OneDrive.Sync.Client.Infrastructure/Data/Migrations`
   - Configure DB path from platform-specific application data directory (per OS conventions)

3. ✅ **Add Runtime Migration Application**
   - Update startup/composition root so `Database.Migrate()` runs on app startup before repositories/services execute
   - Add guard/logging for migration failures and return `Result<T, TError>` from persistence bootstrap where applicable

4. ✅ **Refactor Persistence Abstractions**
   - Replace file/JSON persistence interfaces used by UI/Application with repository abstractions backed by EF Core
   - Map existing persistence use-cases to tables: settings → `Settings`, accounts → `Accounts`, folder/file state → `SyncFiles` (and related tables if split)

5. ✅ **Verify**
   - Migration files generated under `src/AStar.Dev.OneDrive.Sync.Client.Infrastructure/Data/Migrations`
   - App startup auto-applies migrations with no manual step
   - Persistence integration tests pass against SQLite
   - Existing UI flows (settings/accounts/folder state) read/write via database

---

### **Phase 8: Layout Switching Integration Test**
Test that `MainWindowViewModel` correctly orchestrates layout swaps.

1. **Test** — tests/AStar.Dev.OneDrive.Sync.Client.UI.Tests/ViewModels/Home/MainWindowViewModelTests.cs (expand existing)
   - Test: `CurrentLayout` property starts as Explorer
   - Test: `SwitchToExplorerCommand` sets layout to Explorer view
   - Test: `SwitchToDashboardCommand` sets layout to Dashboard view
   - Test: `SwitchToTerminalCommand` sets layout to Terminal view
   - Test: Shared VMs (Accounts, FolderTree, Sync, Logs, Settings) are same instance across swaps
   - Test: Layout preference persists via `Settings` table (`SettingsViewModel.SelectedLayout`)

2. **Implement** — MainWindowViewModel.cs
   - Ensure all layout commands work
   - Persist selected layout to `Settings` table via persistence service
   - Load layout preference from database on startup

3. **Verify**
   - 6 tests pass
   - Launch app, switch between layouts → works
   - Close app & reopen → last layout is restored from database

---

### **Phase 9: Settings Dialog Full Integration**
Connect Settings view to theme/language/layout switching.

1. **Test First** — tests/AStar.Dev.OneDrive.Sync.Client.UI.Tests/Integration/SettingsIntegrationTests.cs
   - Test: Changing theme in dialog updates app theme
   - Test: Changing language in dialog updates UI labels
   - Test: OK button saves all settings
   - Test: Cancel button discards changes

2. **Implement** — SettingsView.axaml & code-behind
   - Add theme dropdown bound to `SettingsViewModel.AvailableThemes`
   - Add language dropdown (English for now)
   - Add OK / Cancel / Apply buttons
   - Wire up save/load to `Settings` table on dialog open/close

3. **Verify**
   - 4 tests pass
   - Open Settings in app → all controls populated
   - Change theme → app theme updates (live)
   - Cancel → original settings restored
   - OK → settings saved, dialog closes

---

### **Phase 10: Account Management Dialog**
Build add/edit account dialog (stub—real OAuth comes later).

1. **Test First** — tests/AStar.Dev.OneDrive.Sync.Client.UI.Tests/ViewModels/AccountManagement/AccountDialogViewModelTests.cs
   - Test: Dialog opens with empty fields for new account
   - Test: Dialog opens with populated fields for edit
   - Test: `SaveCommand` validates email format
   - Test: `CancelCommand` closes without saving
   - Test: `LoginCommand` triggers authentication (stub for now)

2. **Implement** — src/AStar.Dev.OneDrive.Sync.Client.UI/AccountManagement/AccountDialogViewModel.cs & View
   - Create dialog VM with email, storage info properties
   - Implement Save/Cancel/Login commands
   - Persist account changes to `Accounts` table via repository
   - Create dialog XAML (TextBox for email, buttons)

3. **Verify**
   - 5 tests pass
   - Click "Add Account" in app → dialog opens
   - Enter email & click Login → stub behavior (no crash)
   - Cancel returns to main

---

### **Phase 11: Folder Tree UI Binding**
Remove hard-coded data, bind to FolderTreeViewModel.

1. **Test** — tests/AStar.Dev.OneDrive.Sync.Client.UI.Tests/Layouts/ExplorerLayoutIntegrationTest.cs
   - Test: FolderTreeView renders nodes from ViewModel
   - Test: Checkboxes reflect `IsSelected` state
   - Test: Expand/collapse works

2. **Implement** — FolderTreeView.axaml
   - Replace hard-coded TreeView with binding to `{Binding FolderTree.Nodes}`
   - Load nodes from database-backed folder/file state tables
   - Bind checkbox to `IsSelected`
   - Bind expand/collapse to commands

3. **Verify**
   - 3 tests pass
   - Launch app → folder tree is empty (no hard-coded data)
   - Add stub root node via code → appears in tree

---

### **Phase 12: Error Handling & User Feedback**
Add error dialog/notification system.

1. **Test First** — tests/AStar.Dev.OneDrive.Sync.Client.UI.Tests/Common/ErrorHandlerTests.cs
   - Test: `ShowErrorDialog(title, message)` works
   - Test: Errors are logged
   - Test: User can dismiss dialog

2. **Implement** — src/AStar.Dev.OneDrive.Sync.Client.UI/Common/ErrorHandler.cs + ErrorDialog.axaml
   - Create error dialog window
   - Create `ErrorHandler` static utility
   - Hook exception events in App.axaml.cs to show dialog

3. **Verify**
   - 3 tests pass
   - Trigger an error (e.g., invalid theme) → error dialog appears

---

### **Phase 13: Infrastructure Wire-Up (Stub)**
Set up dependency injection for Application/Infrastructure layers.

1. **Test First** — tests/AStar.Dev.OneDrive.Sync.Client.UI.Tests/Composition/CompositionRootTests.cs
   - Test: `ISyncService` can be resolved (returns real impl)
   - Test: `ISyncFileRepository` can be resolved (returns mock OneDrive stub)
   - Test: `AstarOneDriveDbContextModel` and persistence repositories can be resolved

2. **Implement** — src/AStar.Dev.OneDrive.Sync.Client.UI/Composition/CompositionRoot.cs
   - Wire up `ISyncService` → `SyncService`
   - Wire up `ISyncFileRepository` → `OneDriveSyncFileRepository` (stub)
   - Wire up EF Core DbContext + repositories for `Settings`, `Accounts`, and `SyncFiles`
   - Use simple service locator pattern (no external DI container yet)

3. **Verify**
   - 2 tests pass
   - No runtime `NullReferenceException` when accessing services

---

### **Phase 14: Sync Feature Integration (Stub)**
Connect Sync button to stub sync service.

1. **Test First** — tests/AStar.Dev.OneDrive.Sync.Client.UI.Tests/Integration/SyncIntegrationTests.cs
   - Test: Click "Sync Now" → `SyncStatusViewModel.Status` → "Syncing..."
   - Test: Sync completes → Status → "Idle"
   - Test: Sync error → Status → "Error" + message shown

2. **Implement** — Update `SyncStatusViewModel` to call `ISyncService.SyncAsync()`
   - Call `OneDriveSyncFileRepository.GetFilesAsync()` (returns empty for now)
   - Update UI status accordingly

3. **Verify**
   - 3 tests pass
   - Click Sync → status changes, completes without crash

---

### **Phase 15: Build & Acceptance Test**
Full-stack sanity check.

1. **Test** — Manual explorer:
   - ✓ App launches, loads saved theme/layout from database
   - ✓ Switch layouts → no crashes
   - ✓ Change theme → app theme updates
   - ✓ Add/remove account → list updates
   - ✓ Expand folder tree → works
   - ✓ Click Sync → status message changes
   - ✓ Settings dialog open/save/cancel works
   - ✓ Close & relaunch → state restored from database

2. **Final Build**
   ```bash
   dotnet build
   dotnet test
   ```

3. **Verify**
   - 0 build warnings
   - 30+ UI tests pass
   - App runs without crashes

---

**Decisions**
- **ADR Index**: [docs/architecture/README.md](architecture/README.md)
- **ADR**: [docs/architecture/adr-0001-sqlite-ef-core-persistence.md](architecture/adr-0001-sqlite-ef-core-persistence.md)
- **Localization**: English-only for now (infrastructure ready for future languages)
- **Test Coverage**: Pragmatic—every ViewModel tested for core properties/commands, but not UI scaffolding
- **DI**: Simple service locator in CompositionRoot (upgrade to full container later if needed)
- **OneDrive**: Stubbed (Phase 13 returns empty results; real API comes after UI is solid)
- **Storage**: SQLite (EF Core) in platform-specific app data location; schema managed by EF Core migrations
- **Schema Source of Truth**: [docs/database-schema.md](database-schema.md)
