## Implementation Plan Part 2 (Remaining Work)

Date: 2026-03-07

Execution checklist: `docs/implementation-plan-part2-checklist.md`

### Why this document exists
This plan captures work that is still required based on the current codebase, regardless of what `docs/implementation-plan.md` marks as complete.

### Current baseline (verified in code)
- UI shells for Explorer, Dashboard, and Terminal layouts exist.
- SQLite persistence exists for settings, accounts, and folder-tree-like records.
- Sync flow is stub-level: `ISyncService` only returns `GetSyncFilesAsync`, and `OneDriveSyncFileRepository` returns an empty list.
- Account login is currently a stub (`LoginCommand` toggles state only).
- No implemented OneDrive Graph auth/session/token management.
- No implemented delta cursor lifecycle, upload engine, download engine, or conflict resolver.
- Tree data is persisted locally, but not hydrated from real OneDrive account content.

### Scope for Part 2
- Complete end-to-end OneDrive sync capabilities (auth, delta sync, upload/download, conflict handling).
- Finish functional behavior behind layouts/tree/actions so UI is operational, not just scaffolded.
- Improve architecture boundaries, reliability, observability, and rollout safety.

### Constraints and assumptions
- Onion architecture dependency direction remains the source of truth.
- TDD Red-Green-Refactor workflow is mandatory per repository policy.
- Phase size target: 1-3 days each.
- Desktop-first Avalonia client with local SQLite store remains the persistence baseline.
- Multi-account behavior is required, not optional.

### Architecture approach
Preferred option:
- Build a sync pipeline in Application layer using explicit ports for remote (OneDrive Graph) and local filesystem operations.
- Keep OneDrive-specific HTTP/auth/token logic in Infrastructure.
- Keep UI focused on orchestration/state presentation via Application interfaces.
- Introduce account-scoped sync state, delta tokens, and operation queue semantics.

Alternative A:
- Keep most orchestration in UI ViewModels and call Infrastructure directly.
- Trade-off: faster short-term changes, but violates boundaries and will increase regression risk.

Alternative B:
- Replace custom sync orchestration with a third-party sync SDK.
- Trade-off: potentially faster initial feature completion, but less control over conflict rules, retry semantics, and telemetry.

Decision:
- Use the preferred option for maintainability, testability, and incremental rollout.

### Work breakdown (1-3 day phases)

#### Phase 1 (1 day): Gap lock + ADR set
Goals:
- Freeze remaining feature scope and define acceptance criteria for Part 2.
- Record architecture decisions for sync pipeline and account/session model.

Deliverables:
- `docs/architecture/adr-xxxx-sync-pipeline-boundaries.md`
- `docs/architecture/adr-xxxx-onedrive-auth-token-strategy.md`
- `docs/architecture/adr-xxxx-delta-cursor-and-replay-model.md`
- Updated feature checklist linking each missing feature to a phase.

Exit criteria:
- Team agrees on scope and ADRs before implementation begins.

#### Phase 2 (2 days): Application contracts for real sync
Goals:
- Expand Application interfaces beyond `GetSyncFilesAsync`.
- Introduce explicit operations: enumerate tree, run delta pull, schedule upload/download, pause/resume/cancel.

Deliverables:
- New Application interfaces and result/error contracts.
- Domain/Application models for sync item state, operation queue item, conflict type, and sync checkpoint.

Exit criteria:
- Unit tests define expected orchestration behavior and failure handling.

#### Phase 3 (2 days): OneDrive authentication and account session
Goals:
- Replace login stub with real OneDrive auth flow.
- Persist account session metadata securely.

Deliverables:
- Infrastructure auth adapter (token acquisition/refresh lifecycle).
- Account link/unlink/reauth flow for UI.
- Secure token storage strategy and expiration handling.

Exit criteria:
- User can add a real account and re-open app without immediate relogin when token is valid.

#### Phase 4 (2 days): OneDrive Graph client foundation
Goals:
- Implement resilient HTTP layer for OneDrive APIs.
- Add retry/backoff/rate-limit handling with typed failures.

Deliverables:
- Graph client abstraction in Infrastructure.
- Centralized error mapping (auth/network/throttle/conflict/not-found).
- Basic telemetry hooks around request latency and error classes.

Exit criteria:
- Integration tests validate retry behavior and mapped failures.

#### Phase 5 (2 days): Remote folder tree and account-scoped selection
Goals:
- Load real remote folder tree per selected account.
- Persist account-scoped selections and expanded nodes.

Deliverables:
- Remote tree fetch + local cache hydration.
- Tree ViewModel updates to support account switching and reload.
- Tree UI bindings for select/expand/collapse against mutable VM state (not immutable-record binding dead ends).

Exit criteria:
- Tree view reflects real account folders and state is restored per account.

#### Phase 6 (2 days): Local file inventory and change detection baseline
Goals:
- Build local scanner/index model to compare local files with remote metadata.
- Persist minimal local fingerprint metadata.

Deliverables:
- Local inventory service (path, size, timestamps, hash policy).
- Scan scheduling entry points for manual and startup sync.

Exit criteria:
- Deterministic local snapshot is produced and stored for sync comparison.

#### Phase 7 (2 days): Delta pull from OneDrive
Goals:
- Implement delta-token lifecycle and remote change ingestion.
- Persist cursor/checkpoint per account and root scope.

Deliverables:
- Delta fetch service with pagination and checkpoint commits.
- Restart-safe replay behavior when interrupted.

Exit criteria:
- Re-running sync uses checkpoint and only processes incremental remote changes.

#### Phase 8 (3 days): Download pipeline (remote -> local)
Goals:
- Implement queued downloads with progress, cancellation, and resume-safe behavior.

Deliverables:
- Download operation runner with temp files + atomic finalize.
- Mapping of progress/events to SyncStatus and logs.
- Validation for disk space and path constraints.

Exit criteria:
- Selected remote changes are downloaded reliably with visible progress and recoverable interruption.

#### Phase 9 (3 days): Upload pipeline (local -> remote)
Goals:
- Implement queued uploads with idempotency and retry policy.

Deliverables:
- Upload runner for create/update/delete actions.
- Chunked upload strategy for larger files.
- Local-to-remote operation correlation IDs for troubleshooting.

Exit criteria:
- Local changes are uploaded reliably and reflected on OneDrive.

#### Phase 10 (2 days): Conflict detection and resolution policy
Goals:
- Detect conflict scenarios and apply deterministic policy.
- Support user-visible conflict outcomes.

Deliverables:
- Conflict classifier (timestamp drift, etag mismatch, rename/delete cases).
- Configurable resolution policies (remote wins, local wins, rename copy, manual queue).
- Conflict UI status and actionable log entries.

Exit criteria:
- Conflict cases are handled without data loss and with explicit user-visible outcome.

#### Phase 11 (2 days): Sync orchestrator + scheduling
Goals:
- Coordinate scan, delta pull, upload, and download in one state machine.
- Add pause/resume/cancel and startup/background scheduling.

Deliverables:
- Orchestrator service with lifecycle states and guardrails.
- Sync queue persistence for crash recovery.

Exit criteria:
- End-to-end sync run executes in stable order and resumes after restart.

#### Phase 12 (2 days): Layout and workflow completion
Goals:
- Complete user workflows across Explorer/Dashboard/Terminal beyond static shell behavior.

Deliverables:
- Explorer: operational folder selection, account context, sync actions.
- Dashboard: meaningful metrics/cards fed by real sync data.
- Terminal: logs, status, and command actions tied to orchestrator.
- Remove placeholder-only layout view models and dead commands.

Exit criteria:
- All three layouts can execute real sync workflows with consistent state.

#### Phase 13 (2 days): Observability and diagnostics hardening
Goals:
- Make sync behavior diagnosable in production-like environments.

Deliverables:
- Structured logs for operation lifecycle and failures.
- Metrics for queue depth, sync duration, throughput, retry counts, conflict counts.
- Correlation IDs propagated UI -> Application -> Infrastructure.

Exit criteria:
- Failure triage can be done from logs/metrics without reproducing locally.

#### Phase 14 (2 days): Reliability, performance, and resilience tests
Goals:
- Validate behavior under network faults, throttling, app restarts, and larger trees.

Deliverables:
- Soak and fault-injection test set.
- Performance baselines for scan, delta, upload, and download paths.
- Recovery test cases for interrupted sync and partial transfer.

Exit criteria:
- Stability criteria met for restart safety and sustained sync cycles.

#### Phase 15 (1 day): Release readiness and controlled rollout
Goals:
- Prepare incremental rollout and rollback safety.

Deliverables:
- Feature flags (if needed) and safe defaults.
- Rollback steps for schema/client regressions.
- Release checklist and support runbook updates.

Exit criteria:
- Part 2 features can be enabled gradually with clear rollback path.

### Cross-cutting concerns by design
Security:
- Use secure token handling and avoid logging secrets.
- Validate all external inputs and OneDrive payload assumptions.

Performance:
- Use bounded concurrency for transfers.
- Cache remote metadata and avoid full tree refresh on each sync.

Reliability:
- Persist checkpoints/queue state before destructive transitions.
- Ensure idempotent retries for both upload and download operations.

Testability:
- Maintain thin adapters around filesystem/network/time for deterministic tests.
- Keep orchestration logic isolated from UI to maximize unit/integration coverage.

Observability:
- Structured logs and metrics for each sync stage and account scope.
- Correlation IDs for every sync run and transfer operation.

### Risks and mitigations
- Risk: OneDrive API limits/throttling reduce sync throughput.
- Mitigation: adaptive backoff, bounded worker count, telemetry-driven tuning.

- Risk: Conflict rules cause unexpected user outcomes.
- Mitigation: explicit policies, dry-run tests, conflict audit trail in UI/logs.

- Risk: Architecture drift (UI coupling directly to Infrastructure) increases defects.
- Mitigation: enforce Application interfaces and add boundary tests.

- Risk: Restart interruptions corrupt partial files.
- Mitigation: temp-file + atomic rename, queue checkpointing, resume markers.

- Risk: Multi-account cross-contamination of state.
- Mitigation: strict account-scoped keys for tokens, cursors, tree state, and queue items.

### Suggested implementation order
1. Phases 1-4 (contracts/auth/client).
2. Phases 5-7 (tree + local inventory + delta).
3. Phases 8-11 (transfer engines + orchestrator).
4. Phases 12-15 (UX completion, hardening, rollout).

This sequence delivers incremental value early while reducing rework risk in transfer and conflict logic.
