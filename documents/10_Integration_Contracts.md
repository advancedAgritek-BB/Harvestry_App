# Integration Contracts

## Slack
- OAuth install; Events API; Interactivity; slash commands (/cl); mapping tables for idempotent mirroring.
- Modes: mirror or notify-only; outbox pattern with reconciliation on outages.
- SLO: Slack reply mirrored to app in ≤2s p95.

## QuickBooks Online (QBO)
- OAuth2; encrypted tokens; mapping for items/customers/vendors/accounts/tax/location/class.
- Modes: Item-level (POs, Bills, Adjustments, Invoices, Payments) + GL-summary JE.
- Idempotency (Request-ID); adaptive throttling; webhook HMAC verification; DLQ & reconciliation report per period.

## METRC/BioTrack
- Rate-aware sync queue; idempotent payloads; retries with backoff; error surfacing & remediation.
- Exportable audit trails; per-site credentials; realtime vs scheduled modes.
