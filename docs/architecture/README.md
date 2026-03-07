# Architecture Decision Records (ADRs)

This index tracks architecture decisions for AStar.Dev.OneDrive.Sync.Client.

## ADR Index

| ADR | Status | Date | Title |
|---|---|---|---|
| [ADR-0001](adr-0001-sqlite-ef-core-persistence.md) | Accepted | 2026-02-23 | Adopt SQLite + EF Core for Local Persistence |
| [ADR-0002](adr-0002-sync-pipeline-boundaries.md) | Accepted | 2026-03-07 | Application-Orchestrated Bidirectional Sync Pipeline Boundaries |
| [ADR-0003](adr-0003-onedrive-personal-auth-token-strategy.md) | Accepted | 2026-03-07 | OneDrive Personal Account Authentication and Token Lifecycle Strategy |
| [ADR-0004](adr-0004-delta-cursor-replay-and-manual-conflict-queue.md) | Accepted | 2026-03-07 | Delta Cursor Checkpointing, Replay Semantics, and Manual Conflict Queue |

## How to add a new ADR

1. Create a new file in this folder using the next sequence number: `adr-0002-...md`.
2. Include context, decision, alternatives, trade-offs, risks, validation, and rollout.
3. Add the ADR to the table above.
4. Link related docs (implementation plan, schema, or design docs).
