## Implementation Plan Part 2 Checklist

Date: 2026-03-07
Related plan: `docs/implementation-plan-part2.md`

Use this file to track execution progress phase-by-phase in PRs.

Legend:
- `[ ]` not started
- `[-]` in progress
- `[x]` complete

## Global Gates (apply to every phase)
- [x] Scope aligns with `docs/implementation-plan-part2.md`
- [ ] Onion boundaries respected (UI -> Application -> Domain; Infrastructure -> Application -> Domain)
- [-] Tests authored and executed per repository TDD policy (Red/Green/Refactor)
- [ ] Logging/metrics impact reviewed
- [ ] Docs/ADR updates included when architecture changes
- [ ] Rollback impact assessed

## Phase Checklist

### Phase 1: Gap lock + ADR set (1 day)
- [x] Create ADR for sync pipeline boundaries
- [x] Create ADR for OneDrive auth/token strategy
- [x] Create ADR for delta cursor/checkpoint model
- [x] Finalize missing-feature acceptance criteria
- [x] Get architecture review sign-off

### Phase 2: Application contracts for real sync (2 days)
- [x] Expand Application interfaces beyond `GetSyncFilesAsync`
- [x] Add contracts for tree enumerate, delta pull, upload/download scheduling
- [x] Add pause/resume/cancel semantics
- [x] Add domain/application models for queue item, conflict type, checkpoint
- [x] Add/update unit tests for orchestration and failures

### Phase 3: OneDrive authentication and account session (2 days)
- [x] Implement real OneDrive auth flow (replace login stub)
- [x] Implement token lifecycle (acquire/refresh/expire handling)
- [x] Persist account session metadata securely
- [x] Implement link/unlink/reauth UI workflow
- [x] Add integration tests for auth/session behavior

### Phase 4: OneDrive Graph client foundation (2 days)
- [x] Implement Infrastructure Graph client abstraction
- [x] Add retry/backoff/throttling handling
- [x] Map HTTP/API failures to typed sync errors
- [x] Add request latency/error class telemetry
- [x] Add integration tests for transient and terminal failures

### Phase 5: Remote folder tree and account-scoped selection (2 days)
- [x] Load remote folder tree per selected account
- [x] Persist account-scoped selected/expanded node state
- [x] Support account switch and tree reload without stale state
- [x] Ensure tree bindings support operational select/expand/collapse flows
- [x] Add tests for per-account state restoration

### Phase 6: Local file inventory and change detection baseline (2 days)
- [x] Implement local scanner/inventory service
- [x] Persist file fingerprints/metadata required for comparison
- [x] Add startup/manual scan entry points
- [x] Add deterministic tests for inventory output
- [x] Document hash/fingerprint policy decision

### Phase 7: Delta pull from OneDrive (2 days)
- [x] Implement delta pagination and checkpoint commit flow
- [x] Persist delta cursor per account/scope
- [x] Implement interrupted-run safe replay behavior
- [x] Add integration tests for incremental runs
- [x] Verify no full refresh on unchanged runs

### Phase 8: Download pipeline (remote -> local) (3 days)
- [x] Build queued download runner with bounded concurrency
- [x] Use temp-file + atomic finalize strategy
- [x] Wire progress/cancel/resume status to UI sync view
- [x] Add path/disk validation
- [x] Add failure recovery tests for partial/interrupted downloads

### Phase 9: Upload pipeline (local -> remote) (3 days)
- [x] Build queued upload runner for create/update/delete
- [x] Implement idempotent retries
- [x] Add chunked upload path for large files
- [x] Add operation correlation IDs
- [x] Add integration tests for retry/restart safety

### Phase 10: Conflict detection and resolution policy (2 days)
- [x] Implement conflict classifier (etag/timestamp/rename/delete)
- [x] Implement policy engine (remote wins/local wins/rename/manual)
- [x] Expose conflict outcomes in UI/logs
- [x] Add tests for deterministic conflict outcomes
- [x] Validate no silent overwrite/data loss paths

### Phase 11: Sync orchestrator + scheduling (2 days)
- [x] Implement orchestration state machine for scan/delta/upload/download
- [x] Add pause/resume/cancel behavior end-to-end
- [x] Add startup/background scheduling
- [x] Persist queue/checkpoints for crash recovery
- [x] Add integration tests for interrupted-run resume

### Phase 12: Layout and workflow completion (2 days)
- [x] Explorer layout uses real account/tree/sync actions
- [x] Dashboard shows real sync/account metrics
- [x] Terminal integrates operational logs/status/actions
- [x] Remove placeholder layout-only commands/VM behavior
- [x] Add UI integration tests for key user journeys

### Phase 13: Observability and diagnostics hardening (2 days)
- [x] Add structured logs for operation lifecycle and failures
- [x] Add metrics: queue depth, duration, throughput, retries, conflicts
- [x] Propagate correlation IDs across layers
- [x] Validate log redaction for sensitive data
- [x] Add diagnostics documentation/runbook updates

### Phase 14: Reliability, performance, and resilience tests (2 days)
- [x] Add fault-injection tests (network failures, throttling, restarts)
- [x] Add soak tests for repeated sync cycles
- [x] Capture baseline performance for scan/delta/upload/download
- [x] Validate restart safety and partial-transfer recovery
- [x] Track and resolve blocking reliability defects

### Phase 15: Release readiness and controlled rollout (1 day)
- [x] Confirm feature flags/safe defaults
- [x] Publish rollback plan (schema/client behavior)
- [x] Complete release checklist and support notes
- [x] Final architecture + QA sign-off
- [x] Mark Part 2 ready for staged rollout

## Optional Tracking Fields Per Phase
- Owner:
- Branch:
- PR:
- Start date:
- End date:
- Risks found:
- Follow-ups:
