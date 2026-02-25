# ADR-0001: Adopt SQLite + EF Core for Local Persistence

- Status: Accepted
- Date: 2026-02-23
- Owners: Architecture + Infrastructure
- Related: [Implementation Plan](../implementation-plan.md), [Database Schema](../database-schema.md)

## Context
The application currently persists user and app state using JSON files (settings, accounts, folder tree), which creates schema drift risk and fragmented persistence behavior. The implementation plan introduces a dedicated persistence phase requiring:
- SQLite as the storage engine
- EF Core with migrations in Infrastructure
- Runtime migration application on startup
- Platform-specific database location
- Strong schema rules via `IEntityTypeConfiguration<T>`
- Explicit nullability and string length constraints

Key constraints:
- Must preserve Onion Architecture dependency direction
- Must support incremental rollout from existing JSON storage
- Must remain testable under TDD (red-green-refactor)

## Decision
Adopt SQLite with EF Core in Infrastructure as the default local persistence mechanism.

Preferred option:
- Use EF Core DbContext in Infrastructure
- Define mappings with one `IEntityTypeConfiguration<T>` per entity
- Store migrations under `src/AStar.Dev.OneDrive.Sync.Client.Infrastructure/Data/Migrations`
- Apply migrations at runtime before repositories are used
- Resolve DB path from platform-specific app data directories
- Replace JSON write paths with repository-based DB writes

## Alternatives Considered
1. Keep JSON files only
   - Pros: simple, no migration tooling
   - Cons: weak schema guarantees, difficult evolution, duplicate file I/O logic

2. Raw SQLite via ADO.NET/Dapper
   - Pros: lightweight runtime
   - Cons: manual schema/version management, weaker consistency in constraints and model evolution

3. Full remote/hosted database
   - Pros: central data management
   - Cons: unnecessary operational complexity for local desktop persistence requirements

## Trade-offs
- Gains: schema versioning, safer constraints, stronger data integrity, better testability
- Costs: EF Core package footprint, migration lifecycle management, startup migration handling

## Cross-Cutting Implications
- Security: store DB in user-scoped application data path; avoid world-writable paths
- Performance: index common lookup fields; avoid loading entire trees where unnecessary
- Reliability: startup migration gate ensures schema compatibility
- Observability: log migration start/success/failure and DB path resolution
- Testability: integration tests validate migration and constraints against SQLite

## Risks and Mitigations
- Risk: migration failure blocks startup
  - Mitigation: fail fast with explicit error result/logging; provide actionable message

- Risk: data loss during JSON-to-DB transition
  - Mitigation: one-time import path with verification tests before removing JSON writes

- Risk: invalid constraints impacting existing data
  - Mitigation: define conservative initial limits and add migration tests for edge values

## Validation Strategy
- Unit tests for configuration classes and path resolution
- Integration tests for DbContext + migration execution + constraint enforcement
- Composition tests to ensure repositories resolve and are used by UI/Application flows
- Manual acceptance in implementation plan Phase 15

## Rollout / Migration Steps
1. Add EF Core packages and DbContext in Infrastructure
2. Add entity configurations and initial migration
3. Wire runtime `Database.Migrate()` at startup
4. Introduce repository abstractions for settings/accounts/folder-file state
5. Read legacy JSON only for transition if required; write to DB only
6. Remove JSON write paths after migration validation

## Consequences
- Persistence behavior becomes schema-driven and versioned
- Future model changes are managed through migrations instead of ad-hoc file changes
- Infrastructure owns persistence concerns; UI/Application consume abstractions
