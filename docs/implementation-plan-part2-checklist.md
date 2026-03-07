## Implementation Plan Part 2 Checklist

Date: 2026-03-07
Related plan: `docs/implementation-plan-part2.md`

Use this file to track execution progress phase-by-phase in PRs.

Legend:
- `[ ]` not started
- `[-]` in progress
- `[x]` complete

## Global Gates (apply to every phase)
- [ ] Scope aligns with `docs/implementation-plan-part2.md`
- [ ] Onion boundaries respected (UI -> Application -> Domain; Infrastructure -> Application -> Domain)
- [ ] Tests authored and executed per repository TDD policy (Red/Green/Refactor)
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
- [ ] Implement real OneDrive auth flow (replace login stub)
- [ ] Implement token lifecycle (acquire/refresh/expire handling)
- [ ] Persist account session metadata securely
- [ ] Implement link/unlink/reauth UI workflow
- [ ] Add integration tests for auth/session behavior

### Phase 4: OneDrive Graph client foundation (2 days)
- [ ] Implement Infrastructure Graph client abstraction
- [ ] Add retry/backoff/throttling handling
- [ ] Map HTTP/API failures to typed sync errors
- [ ] Add request latency/error class telemetry
- [ ] Add integration tests for transient and terminal failures

### Phase 5: Remote folder tree and account-scoped selection (2 days)
- [ ] Load remote folder tree per selected account
- [ ] Persist account-scoped selected/expanded node state
- [ ] Support account switch and tree reload without stale state
- [ ] Ensure tree bindings support operational select/expand/collapse flows
- [ ] Add tests for per-account state restoration

### Phase 6: Local file inventory and change detection baseline (2 days)
- [ ] Implement local scanner/inventory service
- [ ] Persist file fingerprints/metadata required for comparison
- [ ] Add startup/manual scan entry points
- [ ] Add deterministic tests for inventory output
- [ ] Document hash/fingerprint policy decision

### Phase 7: Delta pull from OneDrive (2 days)
- [ ] Implement delta pagination and checkpoint commit flow
- [ ] Persist delta cursor per account/scope
- [ ] Implement interrupted-run safe replay behavior
- [ ] Add integration tests for incremental runs
- [ ] Verify no full refresh on unchanged runs

### Phase 8: Download pipeline (remote -> local) (3 days)
- [ ] Build queued download runner with bounded concurrency
- [ ] Use temp-file + atomic finalize strategy
- [ ] Wire progress/cancel/resume status to UI sync view
- [ ] Add path/disk validation
- [ ] Add failure recovery tests for partial/interrupted downloads

### Phase 9: Upload pipeline (local -> remote) (3 days)
- [ ] Build queued upload runner for create/update/delete
- [ ] Implement idempotent retries
- [ ] Add chunked upload path for large files
- [ ] Add operation correlation IDs
- [ ] Add integration tests for retry/restart safety

### Phase 10: Conflict detection and resolution policy (2 days)
- [ ] Implement conflict classifier (etag/timestamp/rename/delete)
- [ ] Implement policy engine (remote wins/local wins/rename/manual)
- [ ] Expose conflict outcomes in UI/logs
- [ ] Add tests for deterministic conflict outcomes
- [ ] Validate no silent overwrite/data loss paths

### Phase 11: Sync orchestrator + scheduling (2 days)
- [ ] Implement orchestration state machine for scan/delta/upload/download
- [ ] Add pause/resume/cancel behavior end-to-end
- [ ] Add startup/background scheduling
- [ ] Persist queue/checkpoints for crash recovery
- [ ] Add integration tests for interrupted-run resume

### Phase 12: Layout and workflow completion (2 days)
- [ ] Explorer layout uses real account/tree/sync actions
- [ ] Dashboard shows real sync/account metrics
- [ ] Terminal integrates operational logs/status/actions
- [ ] Remove placeholder layout-only commands/VM behavior
- [ ] Add UI integration tests for key user journeys

### Phase 13: Observability and diagnostics hardening (2 days)
- [ ] Add structured logs for operation lifecycle and failures
- [ ] Add metrics: queue depth, duration, throughput, retries, conflicts
- [ ] Propagate correlation IDs across layers
- [ ] Validate log redaction for sensitive data
- [ ] Add diagnostics documentation/runbook updates

### Phase 14: Reliability, performance, and resilience tests (2 days)
- [ ] Add fault-injection tests (network failures, throttling, restarts)
- [ ] Add soak tests for repeated sync cycles
- [ ] Capture baseline performance for scan/delta/upload/download
- [ ] Validate restart safety and partial-transfer recovery
- [ ] Track and resolve blocking reliability defects

### Phase 15: Release readiness and controlled rollout (1 day)
- [ ] Confirm feature flags/safe defaults
- [ ] Publish rollback plan (schema/client behavior)
- [ ] Complete release checklist and support notes
- [ ] Final architecture + QA sign-off
- [ ] Mark Part 2 ready for staged rollout

## Optional Tracking Fields Per Phase
- Owner:
- Branch:
- PR:
- Start date:
- End date:
- Risks found:
- Follow-ups:
