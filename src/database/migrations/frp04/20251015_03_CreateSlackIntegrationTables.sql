-- ============================================================================
-- FRP-04: Tasks, Messaging & Slack Integration
-- Migration: Create Slack Integration Tables
-- ============================================================================
-- NOTE: Applies schema objects for Slack workspaces, channel mappings, and
--       notification queue used by the Harvestry Tasks service.
-- ============================================================================

BEGIN;

CREATE SCHEMA IF NOT EXISTS tasks;

-- --------------------------------------------------------------------------
-- Slack Workspaces
-- --------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS tasks.slack_workspaces
(
    slack_workspace_id      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id                 UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    workspace_id            VARCHAR(100) NOT NULL,
    workspace_name          VARCHAR(200) NOT NULL,
    bot_token_encrypted     TEXT,
    refresh_token_encrypted TEXT,
    is_active               BOOLEAN NOT NULL DEFAULT TRUE,
    installed_by_user_id    UUID NOT NULL REFERENCES users(user_id) ON DELETE RESTRICT,
    installed_at            TIMESTAMPTZ NOT NULL DEFAULT timezone('utc', now()),
    last_verified_at        TIMESTAMPTZ
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_slack_workspaces_site_workspace
    ON tasks.slack_workspaces (site_id, workspace_id);

-- --------------------------------------------------------------------------
-- Slack Channel Mappings
-- --------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS tasks.slack_channel_mappings
(
    slack_channel_mapping_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id                  UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    slack_workspace_id       UUID NOT NULL REFERENCES tasks.slack_workspaces(slack_workspace_id) ON DELETE CASCADE,
    channel_id               VARCHAR(100) NOT NULL,
    channel_name             VARCHAR(200) NOT NULL,
    notification_type        VARCHAR(50)  NOT NULL,
    is_active                BOOLEAN NOT NULL DEFAULT TRUE,
    created_by_user_id       UUID NOT NULL REFERENCES users(user_id) ON DELETE RESTRICT,
    created_at               TIMESTAMPTZ NOT NULL DEFAULT timezone('utc', now())
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_slack_channel_mappings_workspace_channel_type
    ON tasks.slack_channel_mappings (slack_workspace_id, channel_id, notification_type);

-- --------------------------------------------------------------------------
-- Slack Message Bridge Log (idempotency tracking for outbound posting)
-- --------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS tasks.slack_message_bridge_log
(
    slack_message_bridge_log_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id                     UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    slack_workspace_id          UUID NOT NULL REFERENCES tasks.slack_workspaces(slack_workspace_id) ON DELETE CASCADE,
    internal_message_id         UUID,
    internal_message_type       VARCHAR(50) NOT NULL,
    slack_channel_id            VARCHAR(100) NOT NULL,
    slack_message_ts            VARCHAR(50) NOT NULL,
    slack_thread_ts             VARCHAR(50),
    request_id                  VARCHAR(100) NOT NULL,
    status                      VARCHAR(20) NOT NULL DEFAULT 'pending',
    attempt_count               INT NOT NULL DEFAULT 0,
    last_attempt_at             TIMESTAMPTZ,
    error_message               TEXT,
    sent_at                     TIMESTAMPTZ,
    created_at                  TIMESTAMPTZ NOT NULL DEFAULT timezone('utc', now()),
    UNIQUE (slack_workspace_id, slack_message_ts),
    UNIQUE (request_id, slack_workspace_id)
);

CREATE INDEX IF NOT EXISTS ix_slack_message_bridge_log_internal
    ON tasks.slack_message_bridge_log (internal_message_id);

-- --------------------------------------------------------------------------
-- Slack Notification Queue (outbox pattern)
-- --------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS tasks.slack_notification_queue
(
    slack_notification_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id               UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    slack_workspace_id    UUID NOT NULL REFERENCES tasks.slack_workspaces(slack_workspace_id) ON DELETE CASCADE,
    channel_id            VARCHAR(100) NOT NULL,
    notification_type     SMALLINT NOT NULL,
    payload_json          JSONB NOT NULL,
    request_id            VARCHAR(100) NOT NULL,
    status                SMALLINT NOT NULL DEFAULT 0,
    priority              INT NOT NULL DEFAULT 5 CHECK (priority BETWEEN 1 AND 10),
    attempt_count         INT NOT NULL DEFAULT 0,
    max_attempts          INT NOT NULL DEFAULT 3,
    next_attempt_at       TIMESTAMPTZ NOT NULL DEFAULT timezone('utc', now()),
    last_error            TEXT,
    created_at            TIMESTAMPTZ NOT NULL DEFAULT timezone('utc', now())
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_slack_notification_queue_request
    ON tasks.slack_notification_queue (request_id, slack_workspace_id, channel_id);

CREATE INDEX IF NOT EXISTS ix_slack_notification_queue_next_attempt
    ON tasks.slack_notification_queue (next_attempt_at)
    WHERE status IN (0, 3);

CREATE INDEX IF NOT EXISTS ix_slack_notification_queue_site_status
    ON tasks.slack_notification_queue (site_id, status);

-- --------------------------------------------------------------------------
-- Row Level Security
-- --------------------------------------------------------------------------
ALTER TABLE tasks.slack_workspaces ENABLE ROW LEVEL SECURITY;
ALTER TABLE tasks.slack_channel_mappings ENABLE ROW LEVEL SECURITY;
ALTER TABLE tasks.slack_message_bridge_log ENABLE ROW LEVEL SECURITY;
ALTER TABLE tasks.slack_notification_queue ENABLE ROW LEVEL SECURITY;

CREATE POLICY slack_workspaces_site_access ON tasks.slack_workspaces
    FOR ALL
    USING (
        (current_setting('app.site_id', TRUE) IS NOT NULL AND site_id::text = current_setting('app.site_id', TRUE))
        OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
    )
    WITH CHECK (
        (current_setting('app.site_id', TRUE) IS NOT NULL AND site_id::text = current_setting('app.site_id', TRUE))
        OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
    );

CREATE POLICY slack_channel_mappings_site_access ON tasks.slack_channel_mappings
    FOR ALL
    USING (
        (current_setting('app.site_id', TRUE) IS NOT NULL AND site_id::text = current_setting('app.site_id', TRUE))
        OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
    )
    WITH CHECK (
        (current_setting('app.site_id', TRUE) IS NOT NULL AND site_id::text = current_setting('app.site_id', TRUE))
        OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
    );

CREATE POLICY slack_message_bridge_log_site_access ON tasks.slack_message_bridge_log
    FOR ALL
    USING (
        (current_setting('app.site_id', TRUE) IS NOT NULL AND site_id::text = current_setting('app.site_id', TRUE))
        OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
    )
    WITH CHECK (
        (current_setting('app.site_id', TRUE) IS NOT NULL AND site_id::text = current_setting('app.site_id', TRUE))
        OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
    );

CREATE POLICY slack_notification_queue_site_access ON tasks.slack_notification_queue
    FOR ALL
    USING (
        (current_setting('app.site_id', TRUE) IS NOT NULL AND site_id::text = current_setting('app.site_id', TRUE))
        OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
    )
    WITH CHECK (
        (current_setting('app.site_id', TRUE) IS NOT NULL AND site_id::text = current_setting('app.site_id', TRUE))
        OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
    );

COMMIT;
