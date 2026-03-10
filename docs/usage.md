# AStar OneDrive Sync Client Usage Guide

## Scope
This guide targets technical and power users working directly with the current desktop client build. It focuses on practical app behavior, configuration, and layout-specific capabilities.

## Startup And Persistence
1. Launch the UI project.
2. On startup the app applies database migrations, loads persisted settings, then applies language and theme.
3. Settings persisted in the app are restored on next launch:
- Selected theme
- Selected language
- Selected layout
- User name

## Core Workflow
1. Add or manage one or more accounts from the account list.
2. Select an account to scope folder tree operations.
3. Use folder tree controls to mark selection and expansion state.
4. Trigger sync from `Sync Status` or Terminal status bar controls.
5. Monitor progress and status (`Idle`, `Syncing...`, `Paused`, `Error`).
6. Review activity and logs for diagnostics.

## Accounts
- Add account:
Uses `Add Account` to open the account dialog and start account onboarding/session flow.
- Manage account:
Opens the account dialog for the selected account.
- Logout/remove account:
Unlinks the selected account session and removes it from the local account list.
- Persistence:
Accounts are loaded from and saved to SQLite-backed state.

## Folder Tree
- Account-scoped tree state:
Folder tree load/save is scoped by active account ID.
- Node controls:
Each node supports expand, collapse, and selection toggle.
- Persistence:
Selection/expanded state and hierarchy order are persisted and restored.
- No account selected:
Tree is cleared when no account is selected.

## Sync Controls And Status
- Commands:
`Sync Now`, `Pause`, `Resume`
- Status model:
`Status`, `ProgressPercentage`, and `SyncError` drive UI and layout metrics.
- Activity stream:
Status changes and conflict warnings are appended as activity entries.
- Current sync behavior:
Sync status view issues download queue operations from available sync files and records conflict outcomes in activity.

## Logs And Diagnostics
- Logs tab:
Displays aggregated log text in monospaced format.
- Activity forwarding:
Recent sync activity is appended into logs (severity-tagged messages).
- Debug log window shortcut:
`Ctrl+Shift+L` toggles the debug log window.

## Themes
Supported themes:
- `Light`
- `Dark`
- `Colorful`
- `Professional`
- `Hacker`
- `HighContrast`

Theme changes apply immediately and persist through settings.

## Layouts
Supported layouts:
- `Explorer`
- `Dashboard`
- `Terminal`

You can switch layouts from menu commands and from Settings (`SelectedLayout`).

### Explorer Layout
Intended use:
Focused operational view for account + folder + sync work.

What is available:
- Top menu for settings and layout switching.
- Left pane folder tree (selection and expand/collapse).
- Right pane account list and sync status controls.
- Bottom status bar showing current sync status and current theme.

What should be possible:
- Manage accounts.
- Select and prepare folders.
- Execute and monitor sync operations.
- Switch to other layouts without leaving context.

### Dashboard Layout
Intended use:
At-a-glance metrics and overview-style operations.

What is available:
- App bar with theme picker.
- Drawer with navigation-style buttons and settings shortcuts.
- Account card list.
- Sync status panel.
- Metrics card for linked accounts, selected account email, sync progress.

What should be possible:
- Monitor sync and account health quickly.
- Change theme quickly from top bar.
- Open settings and switch layouts.

Current implementation note:
- The `+` add-account card button is present visually; account creation is reliably initiated from the account list `Add Account` action.

### Terminal Layout
Intended use:
Power-user operations with combined control and diagnostics.

What is available:
- Command/menu bar with settings and layout switching.
- Bottom operational status bar and direct `Sync`, `Pause`, `Resume` controls.
- Left column account list + folder tree.
- Right tabbed workspace:
`Status`, `Logs`, `Settings`

What should be possible:
- Drive sync operations while observing status in real time.
- Inspect logs without changing layouts.
- Tune theme/layout/language settings in-place.

## Settings Behavior
- `OK` and `Apply` persist current settings.
- `Cancel` restores last committed settings in-memory.
- Changing theme from settings applies immediately.
- Changing layout from settings switches the active layout.

## Practical Notes
- If theme switching appears stale, force a layout switch or resize; visuals are invalidated on apply, but heavy UI states can lag briefly on some systems.
- For troubleshooting sessions, keep Terminal layout active to combine controls, status, and logs in one workspace.