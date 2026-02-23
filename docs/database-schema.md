# Database Schema (SQLite + EF Core)

## Purpose
This document defines the initial SQLite schema for configuration, accounts, folder/file state, and sync activity.
It is the source of truth for Phase 7a in the implementation plan.

Architecture decision reference: [docs/architecture/adr-0001-sqlite-ef-core-persistence.md](architecture/adr-0001-sqlite-ef-core-persistence.md).

## Scope and Constraints
- Persistence technology: SQLite using latest EF Core packages.
- Migrations location: `src/AstarOneDrive.Infrastructure/Data/Migrations`.
- Mapping style: `IEntityTypeConfiguration<T>` for every entity.
- Null handling: columns are required by default; nullable only when business-justified.
- String lengths: explicit max lengths on all text columns.
- Runtime behavior: pending migrations are applied at startup before repository use.

## Runtime Database Location
Use a platform-specific user data directory and place the DB at:
- `<app-data>/AstarOneDrive/astar-onedrive.db`

Resolution rules:
- Windows: `%LocalAppData%/AstarOneDrive/astar-onedrive.db`
- Linux: `$XDG_DATA_HOME/AstarOneDrive/astar-onedrive.db` (fallback: `~/.local/share/AstarOneDrive/astar-onedrive.db`)
- macOS: `~/Library/Application Support/AstarOneDrive/astar-onedrive.db`

Implementation note: centralize path resolution in Infrastructure so UI/Application never build file paths.

## Logical Model

### Settings
Single-row logical configuration (stored as one or more rows to keep migration flexibility).

| Column | Type | Required | Max Length | Notes |
|---|---|---:|---:|---|
| Id | TEXT (Guid) | Yes | 36 | Primary key |
| Key | TEXT | Yes | 100 | Unique key (e.g., `SelectedTheme`) |
| Value | TEXT | Yes | 1000 | Setting value |
| UpdatedUtc | TEXT (ISO-8601) | Yes | 40 | Last update time |

Indexes:
- `UX_Settings_Key` unique on `Key`

### Accounts
One row per OneDrive account profile shown in UI.

| Column | Type | Required | Max Length | Notes |
|---|---|---:|---:|---|
| Id | TEXT | Yes | 64 | Domain/account identifier |
| Email | TEXT | Yes | 320 | RFC-compatible practical max |
| DisplayName | TEXT | No | 200 | Optional user-facing label |
| QuotaBytes | INTEGER | Yes | - | Total quota |
| UsedBytes | INTEGER | Yes | - | Used quota |
| IsActive | INTEGER (bool) | Yes | - | Active flag |
| CreatedUtc | TEXT (ISO-8601) | Yes | 40 | Creation time |
| UpdatedUtc | TEXT (ISO-8601) | Yes | 40 | Last update |

Indexes:
- `UX_Accounts_Email` unique on `Email`
- `IX_Accounts_IsActive` on `IsActive`

### SyncFiles
Normalized file/folder state used by folder tree and sync pipeline.

| Column | Type | Required | Max Length | Notes |
|---|---|---:|---:|---|
| Id | TEXT (Guid) | Yes | 36 | Primary key |
| AccountId | TEXT | Yes | 64 | FK to Accounts(Id) |
| ParentId | TEXT (Guid) | No | 36 | Self-reference for hierarchy |
| Name | TEXT | Yes | 260 | File/folder name |
| LocalPath | TEXT | Yes | 1024 | Full local path |
| RemotePath | TEXT | Yes | 1024 | Full OneDrive path |
| ItemType | TEXT | Yes | 20 | `File` or `Folder` |
| IsSelected | INTEGER (bool) | Yes | - | UI selection state |
| IsExpanded | INTEGER (bool) | Yes | - | UI expansion state |
| LastSyncUtc | TEXT (ISO-8601) | No | 40 | Last successful sync time |
| CTag | TEXT | No | 200 | Remote CTag |
| ETag | TEXT | No | 200 | Remote version marker |
| SizeBytes | INTEGER | No | - | Optional for folders |
| CreatedUtc | TEXT (ISO-8601) | Yes | 40 | Creation time |
| UpdatedUtc | TEXT (ISO-8601) | Yes | 40 | Last update |

Indexes:
- `IX_SyncFiles_AccountId`
- `IX_SyncFiles_ParentId`
- `UX_SyncFiles_Account_LocalPath` unique on `(AccountId, LocalPath)`
- `UX_SyncFiles_Account_RemotePath` unique on `(AccountId, RemotePath)`

Foreign keys:
- `FK_SyncFiles_Accounts_AccountId` (`SyncFiles.AccountId` -> `Accounts.Id`) ON DELETE CASCADE
- `FK_SyncFiles_SyncFiles_ParentId` (`SyncFiles.ParentId` -> `SyncFiles.Id`) ON DELETE CASCADE

### SyncActivity (optional but recommended)
Stores recent sync activity entries for diagnostics and UI history.

| Column | Type | Required | Max Length | Notes |
|---|---|---:|---:|---|
| Id | TEXT (Guid) | Yes | 36 | Primary key |
| AccountId | TEXT | No | 64 | Nullable for global events |
| Level | TEXT | Yes | 20 | Info/Warning/Error |
| Message | TEXT | Yes | 2000 | Activity message |
| CreatedUtc | TEXT (ISO-8601) | Yes | 40 | Event timestamp |

Indexes:
- `IX_SyncActivity_CreatedUtc`
- `IX_SyncActivity_AccountId`

## EF Core Configuration Rules
Create one configuration class per entity under Infrastructure Data configuration folder, for example:
- `SettingsConfiguration : IEntityTypeConfiguration<SettingEntity>`
- `AccountConfiguration : IEntityTypeConfiguration<AccountEntity>`
- `SyncFileConfiguration : IEntityTypeConfiguration<SyncFileEntity>`
- `SyncActivityConfiguration : IEntityTypeConfiguration<SyncActivityEntity>`

Each configuration must define:
- Table name
- Primary key
- Required vs optional fields (`IsRequired`)
- String max lengths (`HasMaxLength`)
- Indexes and uniqueness
- Foreign keys and delete behavior

## Migration and Startup Contract
1. Generate migrations into `src/AstarOneDrive.Infrastructure/Data/Migrations`.
2. Ensure startup calls `Database.Migrate()` before repositories are resolved/used.
3. If migration fails, log failure and return a functional error (`Result<T, TError>`), do not silently continue.

## JSON-to-DB Transition Mapping
Legacy JSON payloads map to tables as follows:
- `settings.json` -> `Settings`
- `accounts.json` -> `Accounts`
- `folder-tree.json` -> `SyncFiles` (folder/file hierarchy + selection/expansion state)

Transition guidance:
- Keep read compatibility only during migration window if needed.
- New writes must go only to database tables.
- Remove JSON write paths after migration validation is complete.

## Test Expectations (TDD)
Minimum failing tests to add before implementation:
- Context creates SQLite DB in platform-specific path.
- Migrations are discovered from Infrastructure Migrations folder.
- Runtime startup applies pending migration.
- Insert/query for Settings, Accounts, SyncFiles succeeds.
- Constraint tests for required fields and max lengths fail/pass as expected.
- Foreign key and unique index behavior is validated.
