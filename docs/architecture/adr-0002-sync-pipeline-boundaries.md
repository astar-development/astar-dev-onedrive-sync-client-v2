# ADR-0002: Application-Orchestrated Bidirectional Sync Pipeline Boundaries

- Status: Accepted
- Date: 2026-03-07
- Owners: Architecture + Application + Infrastructure
- Related: [Implementation Plan Part 2](../implementation-plan-part2.md), [Implementation Plan](../implementation-plan.md)

## Context
Part 2 requires real OneDrive synchronization with upload/download, delta processing, queueing, pause/resume, and conflict handling. The current codebase has a stubbed sync repository and UI-driven orchestration behavior, which is insufficient for reliable bidirectional synchronization.

User decisions captured:
- Sync mode from first production rollout: bidirectional
- Conflict default: manual queue for user decision (see ADR-0004)

Key constraints:
- Preserve Onion Architecture dependency direction
- Keep sync behavior testable and restart-safe
- Avoid placing transport/storage details in UI

## Decision
Adopt an Application-layer sync orchestrator with explicit ports and bounded infrastructure adapters.

Preferred boundaries:
- UI:
  - Triggers sync lifecycle actions (start/pause/resume/cancel)
  - Displays state/progress/conflicts/logs
  - Does not call OneDrive HTTP/storage directly
- Application:
  - Owns orchestration state machine and scheduling order (scan, delta, upload, download)
  - Owns queue semantics and retry policies
  - Exposes typed results/errors to UI
- Infrastructure:
  - Implements OneDrive API client, local filesystem adapter, queue/checkpoint persistence adapters
  - Handles network, IO, and provider-specific concerns
- Domain:
  - Holds sync concepts and invariants (operation identity, item states, conflict categories)

Operational decisions:
- Use durable queue/checkpoint persistence in SQLite
- Use bounded concurrency workers for upload/download operations
- Use idempotent operation identifiers for replay safety

## Alternatives Considered
1. UI-driven orchestration with direct infrastructure calls
   - Pros: fastest short-term delivery
   - Cons: boundary violations, lower testability, higher regression risk

2. Infrastructure-heavy orchestration
   - Pros: centralized provider implementation
   - Cons: business workflow leaks into provider layer, weaker portability

3. Single-threaded sequential pipeline only
   - Pros: simpler correctness model
   - Cons: poor performance and responsiveness for larger sync sets

## Trade-offs and Consequences
- Gains:
  - Clear ownership and maintainability
  - Better unit/integration testability
  - Reliable restart and replay behavior
- Costs:
  - More upfront interface design
  - Additional orchestration complexity

Cross-cutting implications:
- Security: keep secrets/tokens outside Application and UI
- Performance: bounded concurrency and batch processing
- Reliability: durable queue + checkpointing + idempotency
- Observability: stage-level logs and metrics for orchestration transitions
- Testability: deterministic orchestration tests via adapter interfaces

## Risks and Mitigations
- Risk: queue/checkpoint corruption causes repeated or skipped operations
  - Mitigation: transactional writes and idempotent operation keys

- Risk: orchestration complexity introduces state bugs
  - Mitigation: explicit state machine tests and transition invariants

- Risk: throughput bottlenecks with conservative worker limits
  - Mitigation: tune concurrency via configuration and telemetry feedback

## Validation Strategy
- Unit tests for orchestration transitions and failure branches
- Integration tests for queue persistence and restart recovery
- Boundary tests to prevent UI -> Infrastructure direct coupling for sync internals
- Soak tests for repeated bidirectional cycles

## Rollout / Rollback
Rollout:
1. Introduce new Application sync interfaces and orchestrator behind current UI commands
2. Add durable queue/checkpoint persistence
3. Switch UI sync actions to orchestrator-backed flows
4. Enable bounded concurrency with conservative defaults

Rollback:
1. Disable orchestrator execution via feature flag/safe switch if introduced
2. Fall back to read-only status mode while preserving persisted queue/checkpoints
3. Keep schema backward-compatible until rollout is stable

## Consequences
- Bidirectional sync becomes a first-class, layered workflow
- Future provider changes remain localized to Infrastructure adapters
- UI remains focused on user workflow/state presentation
