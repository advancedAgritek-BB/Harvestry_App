# Runbook — Slack Outage Reconciliation
- Switch bridge to 'notify_only' mode; queue updates in outbox.
- On restore: reconcile edits/deletes with idempotency keys; verify no dupes.
