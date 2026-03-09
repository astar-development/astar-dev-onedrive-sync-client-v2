# Local Inventory Fingerprint Policy

Date: 2026-03-09
Scope: Phase 6 local inventory baseline

## Decision

Use a deterministic metadata fingerprint policy named `metadata-sha256-v1`.

Fingerprint input:

`{relativePath}|{sizeBytes}|{lastWriteUtcTicks}`

Fingerprint output:

- SHA-256 hex string (lowercase)

## Rationale

- Fast to compute for startup/manual scans.
- Deterministic across runs when file metadata is unchanged.
- Sufficient for baseline local-vs-previous snapshot comparison.

## Notes

- This policy is metadata-based and does not hash full file content.
- False negatives are unlikely for ordinary edits but remain possible in edge cases where metadata is preserved.
- A future phase can add an opt-in content hash policy for higher assurance.