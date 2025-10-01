# Environment Variables Reference

This document captures the runtime configuration required across Track B services. Values marked *(secret)* should be managed via the platform secret store (e.g., SSM, Vault, Helm secrets) and never committed to source control.

---

## Database Connectivity

| Variable | Scope | Description | Default | Notes |
|----------|-------|-------------|---------|-------|
| `IDENTITY_DB_CONNECTION` *(secret)* | Core Platform – Identity | Connection string for the shared identity Postgres cluster. | — | Required by FRP-01 services and acts as the fallback for spatial when a dedicated database is not provisioned. |
| `SPATIAL_DB_CONNECTION` *(secret, optional)* | Core Platform – Spatial | Overrides the default connection string for spatial services. | If unset, spatial reuses `IDENTITY_DB_CONNECTION`. | Supply this when spatial workloads need their own Postgres instance (e.g., compliance isolation, high-volume telemetry). |

### Diagnostics

- Both variables must point to databases with identical schema migrations (baseline + FRP-02). 
- If `SPATIAL_DB_CONNECTION` is provided, ensure the `appsettings.{Environment}.json` files and Kubernetes/Helm manifests map it to the `Spatial` connection name consumed by `SpatialDataSourceFactory`.
- Failing to provide either variable results in a startup exception with the message `Connection string 'Spatial' was not found.`

---

## Observability

| Variable | Scope | Description |
|----------|-------|-------------|
| `OTEL_EXPORTER_OTLP_ENDPOINT` | All services | Endpoint for OpenTelemetry traces/metrics. |
| `OTEL_SERVICE_NAME` | All services | Logical service identifier in telemetry backends. |

---

## Authentication & Authorization

| Variable | Scope | Description |
|----------|-------|-------------|
| `JWT_AUTHORITY` | Public APIs | Base URL for JWT authority (Azure AD, Auth0, etc.). |
| `JWT_AUDIENCE` | Public APIs | Expected audience claim for incoming tokens. |

---

## Deployment Checklist

1. Populate `IDENTITY_DB_CONNECTION` in every environment.
2. Decide whether spatial shares the identity database or needs a dedicated cluster. If dedicated, set `SPATIAL_DB_CONNECTION`; otherwise omit it to inherit `IDENTITY_DB_CONNECTION`.
3. Verify Helm/Terraform manifests surface the correct variables to pods and CI test pipelines.
4. Confirm secrets are present before running migrations or starting services: `scripts/test/run-with-local-postgres.sh` reads `SPATIAL_DB_CONNECTION` when defined.

---

_Last updated: 2025-09-30_
