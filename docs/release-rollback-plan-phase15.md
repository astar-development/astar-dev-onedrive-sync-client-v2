# Phase 15 Rollback Plan

## Scope
This plan covers rollback for Part 2 staged rollout behavior across schema-dependent and client-only paths.

## Schema rollback
1. Stop background sync scheduling for all staged cohorts.
2. Export affected SQLite state files for audit and recovery.
3. Revert to the previous schema migration baseline.
4. Validate repository startup and read paths with the last known-good build.
5. Re-enable clients only after migration parity checks pass.

## Client behavior rollback
1. Toggle rollout gate to disabled for all environments.
2. Ship previous stable client package to staged cohort channels.
3. Keep diagnostics collection enabled for failed-sync evidence.
4. Verify startup, manual sync, and conflict queue behavior against baseline.
5. Close rollback once error rate and sync completion metrics recover.

## Verification checklist
- Confirm no active sync run remains in a partial destructive transition.
- Confirm persisted run state can be resumed by the stable client.
- Confirm support desk has release and rollback identifiers for incident correlation.