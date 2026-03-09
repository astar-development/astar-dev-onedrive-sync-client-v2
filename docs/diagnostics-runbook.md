## Diagnostics Runbook

Date: 2026-03-09
Scope: Phase 13 observability and diagnostics hardening.

### Structured Lifecycle Logs

Sync pipeline operations emit structured lifecycle events via `ISyncDiagnosticsSink`.

Event fields:
- `EventName`: operation lifecycle marker, for example `upload.enqueued`, `upload.completed`, `download.failed`, `conflict.detected`
- `Operation`: logical operation category (`upload`, `download`, `conflict`)
- `CorrelationId`: propagated per-operation correlation ID
- `Outcome`: `ok`, `error`, `cancelled`, or `warning`
- `Message`: diagnostic message (redacted before sink persistence)
- `OccurredUtc`: UTC timestamp

### Metrics

Sync diagnostics emit these metrics:
- Queue depth: `queue.depth.upload`, `queue.depth.download`
- Duration: `duration.upload.ms`, `duration.download.ms`
- Throughput: `throughput.upload`, `throughput.download`
- Retries: `retry.count`
- Conflicts: `conflict.count`

In UI, metrics are mapped to OpenTelemetry counters in `Metrics.Record(...)`.

### Correlation ID Propagation

Correlation IDs are normalized in `SyncService` for both upload and download queue items.
- If a queue item has a correlation ID, it is preserved.
- If missing, the queue item ID is used as fallback.

The normalized ID is propagated in both:
- structured diagnostic events
- emitted metric points

### Redaction Policy

Sensitive values are redacted by `InMemoryLogSink.RedactSensitiveData(...)` before in-memory retention.

Current redaction rules:
- Email addresses -> `[REDACTED_EMAIL]`
- Token key/value fragments (`token=...` / `token: ...`) -> `[REDACTED_TOKEN]`

### Troubleshooting Workflow

1. Open Terminal layout and inspect logs/status.
2. Locate correlation ID from lifecycle logs.
3. Filter related metrics/logs by `correlation_id`.
4. For failures:
- verify `*.failed` lifecycle event reason
- verify retry metrics (`retry.count`)
- verify conflict metrics (`conflict.count`) for conflict paths

### Operational Notes

- Lifecycle events and metrics are emitted best-effort and do not block sync execution.
- If diagnostics sink is unavailable, a no-op sink keeps the sync pipeline functional.
