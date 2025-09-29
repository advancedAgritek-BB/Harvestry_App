# Security, Privacy & Governance

- **Isolation:** Row-Level Security (org+site scope) on all data; ABAC overlays for high-risk actions (destruction, overrides, closed-loop enable).
- **Audit:** Tamper-evident hash chain (prev_hash, row_hash); nightly verification; anchor to WORM; optional public anchor.
- **Secrets:** KMS/Vault; token rotation jobs; verified webhooks.
- **Privacy:** DPIA/PIA for features touching PII; minimize PII by default.
- **Access:** Just-in-time elevation for sensitive ops; least privilege.
- **Compliance:** Exportable regulator-ready audit by plant/lot/package/facility.
