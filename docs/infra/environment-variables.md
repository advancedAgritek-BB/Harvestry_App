# Environment Variables Reference

This document centralizes environment variables required to operate FRP-04 (Tasks, Messaging & Slack) and FRP-05 (Telemetry Ingest & Rollups). Existing services should merge these definitions into their deployment manifests prior to feature rollout.

---

## FRP-04 Tasks, Messaging & Slack

| Variable | Required | Description | Default | Owner |
|----------|----------|-------------|---------|-------|
| `TASKS_SLACK_FEATURE_FLAG` | Yes | Toggles outbound Slack notifications. Set to `false` until workspace credentials are provisioned. | `false` | Workflow & Messaging |
| `TASKS_SLACK_WORKSPACE_ID` | Yes | Slack workspace identifier used for outbound notifications. Managed by DevOps secrets store. | _none_ | DevOps |
| `TASKS_SLACK_BOT_TOKEN` | Yes | Encrypted Slack bot token (xoxb). Store in secrets manager; inject via environment provider at runtime. | _none_ | DevOps |
| `TASKS_SLACK_REFRESH_TOKEN` | Yes | Slack bot refresh token used to obtain new access tokens (token rotation enabled). | _none_ | Workflow & Messaging |
| `TASKS_SLACK_SIGNING_SECRET` | No | Reserved for future interactive workflows; not required for FRP-04 MVP. | _none_ | Workflow & Messaging |
| `TASKS_QUEUE_MAX_RETRY` | Yes | Maximum retry attempts before a notification is dead-lettered. | `5` | Workflow & Messaging |
| `TASKS_QUEUE_BACKOFF_SECONDS` | Yes | Base exponential backoff interval for Slack retry worker. | `15` | Workflow & Messaging |
| `TASKS_DEFAULT_TIMEZONE` | No | Optional override for task due-date normalization (defaults to site configuration). | Site TZ | Product |
| `SLACK_CLIENT_ID` | Yes | Slack app client ID used for OAuth exchanges (mirrors manifest). | _none_ | Workflow & Messaging |
| `SLACK_CLIENT_SECRET` | Yes | Slack app client secret used for OAuth exchanges; rotate after each install. | _none_ | DevOps |

## FRP-05 Telemetry Ingest & Rollups

| Variable | Required | Description | Default | Owner |
|----------|----------|-------------|---------|-------|
| `CONNECTIONSTRINGS__TELEMETRYDB` | Yes | TimescaleDB connection string; must enable extensions, compression, and logical replication. | _none_ | DevOps |
| `TELEMETRY__MQTT__ENABLED` | Yes | Set to `true` to activate MQTT ingest worker. Leave `false` for local dev or air-gapped installs. | `false` | Sensors Team |
| `TELEMETRY__MQTT__HOST` | Yes | MQTT broker host or IP address. | _none_ | Sensors Team |
| `TELEMETRY__MQTT__PORT` | Yes | MQTT broker port. | `1883` | Sensors Team |
| `TELEMETRY__MQTT__USERNAME` | Conditional | Username for MQTT authentication (if required). | _none_ | Sensors Team |
| `TELEMETRY__MQTT__PASSWORD` | Conditional | Password or token for MQTT authentication. Store in secrets manager. | _none_ | Sensors Team |
| `TELEMETRY__MQTT__CLIENTID` | No | Optional override for MQTT client identifier. | auto-generated | Telemetry & Controls |
| `TELEMETRY__MQTT__TOPICFILTER` | No | Topic filter subscribed by ingest worker. | `site/+/equipment/+/telemetry/#` | Telemetry & Controls |
| `TELEMETRY__MQTT__RECONNECTINTERVALSECONDS` | No | Delay (seconds) before reconnect attempts. | `5` | Telemetry & Controls |
| `TELEMETRY__MQTT__USETLS` | Conditional | Enables TLS when connecting to the broker. | `false` | DevOps |
| `TELEMETRY__WALREPLICATION__ENABLED` | Yes | Enables WAL fan-out listener when logical replication infrastructure is ready. | `false` | DevOps |
| `TELEMETRY__WALREPLICATION__CONNECTIONSTRING` | No | Optional replication connection string override (defaults to TelemetryDb + `Replication=database`). | inherited | DevOps |
| `TELEMETRY__WALREPLICATION__SLOTNAME` | Yes | Logical replication slot consumed by fan-out worker. | `telemetry_slot` | DevOps |
| `TELEMETRY__WALREPLICATION__PUBLICATIONNAME` | Yes | Publication exposing telemetry inserts/updates. | `telemetry_publication` | DevOps |
| `TELEMETRY__WALREPLICATION__STATUSINTERVALSECONDS` | No | Interval between feedback messages to PostgreSQL. | `10` | DevOps |
| `TELEMETRY__WALREPLICATION__RETRYDELAYSECONDS` | No | Initial reconnect delay (seconds) after WAL stream interruption. | `5` | DevOps |
| `TELEMETRY__WALREPLICATION__MAXRETRYDELAYSECONDS` | No | Maximum backoff (seconds) for WAL reconnect attempts. | `60` | DevOps |
| `TELEMETRY__SUBSCRIPTIONS__ENABLED` | Yes | Enables background monitoring/pruning of SignalR subscriptions. | `false` | Telemetry & Controls |
| `TELEMETRY__SUBSCRIPTIONS__MONITORINTERVALSECONDS` | No | Interval (seconds) between subscription health snapshots. | `30` | Telemetry & Controls |
| `TELEMETRY__SUBSCRIPTIONS__STALECONNECTIONSECONDS` | No | Idle threshold (seconds) before connections are pruned. | `120` | Telemetry & Controls |
| `TELEMETRY__SUBSCRIPTIONS__TOPSTREAMSTOLOG` | No | Number of busiest streams included in debug logs. | `5` | Telemetry & Controls |
| `TELEMETRY_HTTP_API_KEY` | No | API key for HTTP ingest endpoints. Optional until external partners onboard. | _none_ | Telemetry & Controls |
| `TELEMETRY_MAX_BATCH_SIZE` | Yes | Maximum number of readings accepted per ingest batch. | `5000` | Telemetry & Controls |
| `TELEMETRY_COPY_BATCH_BYTES` | Yes | Target COPY batch size (bytes) for TimescaleDB bulk ingest. | `1048576` (1 MiB) | Telemetry & Controls |
| `TELEMETRY_ALERT_EVALUATION_INTERVAL` | Yes | Interval (seconds) for alert evaluation worker cadence. | `30` | Telemetry & Controls |
| `TELEMETRY_ALERT_COOLDOWN_MINUTES` | Yes | Default cooldown between consecutive alert firings. | `15` | Telemetry & Controls |
| `TELEMETRY_SIGNALR_ALLOWED_ORIGINS` | Yes | Required when SignalR hub is exposed to browser clients. Specify comma-separated origin URLs (e.g., https://app.example.com,https://admin.example.com). Never use wildcard (*) in production. | _none_ | DevOps |

> **Security Note:** Missing/incorrect CORS configuration can expose the SignalR hub to unauthorized cross-origin requests. Always enforce explicit origins and avoid wildcards in production environments.

---

## Action Items

1. DevOps to create secrets for Slack bot token, workspace ID, and MQTT credentials.
2. Telemetry & Controls to document broker topics and payload schema for device adapters.
3. Database team to confirm TimescaleDB privileges and replication slot naming conventions.
4. Update deployment manifests (`k8s`, Helm, Terraform) to include the new variables prior to promoting FRP-04/05 to staging.
