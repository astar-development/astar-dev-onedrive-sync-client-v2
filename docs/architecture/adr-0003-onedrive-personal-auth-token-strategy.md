# ADR-0003: OneDrive Personal Account Authentication and Token Lifecycle Strategy

- Status: Accepted
- Date: 2026-03-07
- Owners: Architecture + Security + Infrastructure
- Related: [Implementation Plan Part 2](../implementation-plan-part2.md), [ADR-0002](adr-0002-sync-pipeline-boundaries.md)

## Context
The current account login flow is a stub and does not support real OneDrive authentication. Part 2 requires production-ready account linking and session continuity.

User decisions captured:
- Account scope for initial rollout: Personal Microsoft accounts only

Key constraints:
- Desktop UX must be practical and secure
- Token handling must avoid leakage into logs and non-secure storage
- Reauth flow must minimize user friction

## Decision
Adopt a OneDrive Personal-only authentication strategy with secure token lifecycle management.

Scope decisions:
- Support Personal Microsoft accounts only in initial rollout
- Exclude Work/School (Entra ID) from Phase 1-2 delivery of auth

Auth/session decisions:
- Use interactive sign-in for first account link
- Use system browser with loopback callback for interactive desktop sign-in
- Use silent token refresh whenever possible
- Fall back to interactive reauthentication when refresh fails or consent changes

Storage decisions:
- Prefer OS-protected secure storage (keyring/libsecret on Linux)
- Allow encrypted local fallback storage if secure store is unavailable
- Persist non-secret account/session metadata in local SQLite
- Never persist secrets in plain text settings tables

Boundary decisions:
- UI triggers account link/unlink/reauth workflows
- Application owns account session use-cases and failure mapping
- Infrastructure owns provider-specific auth, token acquisition, refresh, and secure secret storage

## Alternatives Considered
1. Support Personal + Work/School from start
   - Pros: broader compatibility
   - Cons: higher complexity and longer delivery timeline

2. Device code as primary flow
   - Pros: simple callback setup
   - Cons: less polished desktop UX and slower user completion

3. Store tokens in SQLite with app-level encryption only
   - Pros: unified storage model
   - Cons: weaker protection than OS-backed credential stores

4. Require OS secure storage and block sync when unavailable
  - Pros: strongest default secret storage posture
  - Cons: reduced usability in environments without configured keyring support

## Trade-offs and Consequences
- Gains:
  - Faster, lower-risk delivery by narrowing identity scope
  - Better user experience via silent refresh-first behavior
  - Stronger secret handling with OS-protected storage
- Costs:
  - Work/School accounts deferred
  - Additional platform-specific secret store handling

Cross-cutting implications:
- Security: strict no-secrets-in-logs policy; token redaction by default
- Reliability: graceful degraded behavior when credentials expire
- Observability: auth outcome metrics and error categories without sensitive payloads
- Testability: adapter interfaces for token store and auth client allow deterministic tests

## Risks and Mitigations
- Risk: secure storage API differences across OS targets
  - Mitigation: wrap storage behind infrastructure interface and test per platform

- Risk: consent revocation breaks unattended sync
  - Mitigation: detect token refresh failures and surface actionable reauth prompt

- Risk: insufficient visibility into auth failures
  - Mitigation: standardized error codes and telemetry tags per failure class

## Validation Strategy
- Integration tests for first sign-in, refresh success, refresh failure, reauth fallback
- Security tests to assert no token content is logged
- Tests for secure-store unavailable path and encrypted fallback behavior
- Manual cross-platform sign-in validation for supported desktop targets

## Rollout / Rollback
Rollout:
1. Implement Personal account auth provider and secure token storage adapter
2. Replace account login stub with real flow
3. Enable silent refresh on startup/sync trigger
4. Gate unsupported account types with clear user messaging

Rollback:
1. Disable login path via feature switch if auth defects are blocking
2. Preserve linked account metadata while invalidating broken sessions safely
3. Revert to non-sync mode with explicit user notification

## Consequences
- Real account linking is available for Personal accounts in initial rollout
- Session continuity supports background and repeated sync runs
- Enterprise account support remains an explicit future extension
