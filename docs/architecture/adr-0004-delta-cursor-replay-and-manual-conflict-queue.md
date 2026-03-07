# ADR-0004: Delta Cursor Checkpointing, Replay Semantics, and Manual Conflict Queue

- Status: Accepted
- Date: 2026-03-07
- Owners: Architecture + Application + UX
- Related: [Implementation Plan Part 2](../implementation-plan-part2.md), [ADR-0002](adr-0002-sync-pipeline-boundaries.md)

## Context
Part 2 requires incremental synchronization from OneDrive delta endpoints, restart-safe processing, and user-safe conflict handling. Current implementation does not persist or process delta cursors and has no conflict decision workflow.

User decisions captured:
- Conflict default policy: manual queue for user decision

Key constraints:
- Must avoid missed remote changes after restarts/interruption
- Must avoid silent overwrite in ambiguous conflict scenarios
- Must support multi-account isolation

## Decision
Adopt per-account scoped delta checkpoints with at-least-once replay semantics and a manual conflict queue by default.

Checkpoint decisions:
- Maintain delta cursor/checkpoint per account and sync root scope
- Commit checkpoint after successful processing of each delta page/batch
- On restart, replay from last committed checkpoint (at-least-once)

Replay/idempotency decisions:
- Assign deterministic operation identity to prevent duplicate side effects
- Make upload/download apply handlers idempotent where feasible
- Preserve queue state durably to resume incomplete operations

Conflict decisions:
- Default behavior: move conflicting operations/items to manual conflict queue
- Require explicit user resolution choice per conflict or batch action
- Continue applying non-conflicting operations while conflicts remain queued
- Record conflict classification and chosen resolution for auditability

## Alternatives Considered
1. Global single cursor
   - Pros: simpler model
   - Cons: cross-account contamination risk and poor isolation

2. Commit cursor only at end of full sync run
   - Pros: reduced write frequency
   - Cons: higher replay blast radius after interruption

3. Automatic remote-wins or local-wins defaults
   - Pros: less user interaction
   - Cons: elevated risk of silent data loss and user distrust

4. Block all apply operations until all conflicts are resolved
  - Pros: strict consistency gate
  - Cons: reduced throughput and higher user friction for unrelated items

## Trade-offs and Consequences
- Gains:
  - Better correctness under interruption and retries
  - Safer user-facing conflict outcomes
  - Strong account-level isolation
- Costs:
  - More queue/checkpoint state to manage
  - Additional UX flow for conflict resolution

Cross-cutting implications:
- Reliability: replay-safe, crash-resilient sync progression
- Security: conflict logs must avoid sensitive file content
- Performance: more frequent checkpoint writes; mitigate with batched commits per page
- Observability: metrics for cursor age, replay count, conflict backlog
- Testability: deterministic replay and conflict-classification test suites

## Risks and Mitigations
- Risk: duplicate processing due to at-least-once replay
  - Mitigation: idempotent handlers and operation identity checks

- Risk: conflict queue growth overwhelms users
  - Mitigation: conflict grouping, filtering, and batch resolution actions

- Risk: checkpoint bug causes stalled delta progression
  - Mitigation: cursor freshness monitoring and alerting on stagnation

## Validation Strategy
- Integration tests for interruption/restart at each stage of delta processing
- Property-based or scenario tests for idempotent replay behavior
- UX integration tests for manual conflict queue workflows
- Multi-account tests validating cursor/queue isolation

## Rollout / Rollback
Rollout:
1. Introduce checkpoint persistence schema and repository adapters
2. Implement delta page processing and commit cycle
3. Add replay-safe queue execution on startup
4. Enable manual conflict queue UI and resolution actions

Rollback:
1. Freeze new delta commits while keeping existing local state intact
2. Disable bidirectional apply stage and switch to read-only reconciliation mode
3. Retain conflict queue data for later recovery

## Consequences
- Incremental sync progress is resilient across restarts and failures
- Conflicts become explicit user decisions instead of silent policy side effects
- Multi-account correctness improves through scoped checkpoint/queue storage
