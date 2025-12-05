-- ============================================================================
-- FRP-04: Tasks, Messaging & Slack Integration
-- Migration: Create Messaging & Conversation Tables
-- ============================================================================
-- NOTE: Apply after 20251015_01_CreateTaskTables.sql
-- ============================================================================

BEGIN;

-- --------------------------------------------------------------------------
-- conversations
-- --------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS conversations (
    conversation_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    conversation_type SMALLINT NOT NULL,
    title VARCHAR(500),
    related_entity_type VARCHAR(100),
    related_entity_id UUID,
    status SMALLINT NOT NULL DEFAULT 1,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by_user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE RESTRICT,
    last_message_at TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_conversations_site_type ON conversations(site_id, conversation_type);
CREATE INDEX IF NOT EXISTS ix_conversations_site_status ON conversations(site_id, status);
CREATE INDEX IF NOT EXISTS ix_conversations_last_message_at ON conversations(site_id, last_message_at DESC);

ALTER TABLE conversations
ADD CONSTRAINT related_entity_both_or_none
CHECK ((related_entity_type IS NULL AND related_entity_id IS NULL)
       OR (related_entity_type IS NOT NULL AND related_entity_id IS NOT NULL));

-- --------------------------------------------------------------------------
-- conversation_participants
-- --------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS conversation_participants (
    conversation_participant_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    conversation_id UUID NOT NULL REFERENCES conversations(conversation_id) ON DELETE CASCADE,
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    role SMALLINT NOT NULL DEFAULT 3,
    joined_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    last_read_at TIMESTAMPTZ
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_conversation_participants_conversation_user
    ON conversation_participants(conversation_id, user_id);
CREATE INDEX IF NOT EXISTS ix_conversation_participants_site ON conversation_participants(site_id, conversation_id);

ALTER TABLE conversation_participants
ADD CONSTRAINT chk_conversation_participants_site_id_matches_conversation
CHECK (site_id = (SELECT site_id FROM conversations WHERE conversation_id = conversation_participants.conversation_id));

-- --------------------------------------------------------------------------
-- messages
-- --------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS messages (
    message_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    conversation_id UUID NOT NULL REFERENCES conversations(conversation_id) ON DELETE CASCADE,
    parent_message_id UUID REFERENCES messages(message_id) ON DELETE SET NULL,
    sender_user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE RESTRICT,
    content TEXT NOT NULL,
    is_edited BOOLEAN NOT NULL DEFAULT FALSE,
    edited_at TIMESTAMPTZ,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    deleted_at TIMESTAMPTZ,
    metadata JSONB,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_messages_conversation_created_at
    ON messages(conversation_id, created_at);
CREATE INDEX IF NOT EXISTS ix_messages_site_sender
    ON messages(site_id, sender_user_id);

ALTER TABLE messages
ADD CONSTRAINT chk_messages_site_id_matches_conversation
CHECK (site_id = (SELECT site_id FROM conversations WHERE conversation_id = messages.conversation_id));

-- Function to update conversation last_message_at
CREATE OR REPLACE FUNCTION update_conversation_last_message_at()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE conversations
    SET last_message_at = NEW.created_at
    WHERE conversation_id = NEW.conversation_id;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Trigger to automatically update conversation last_message_at when messages are inserted
CREATE TRIGGER trigger_update_conversation_last_message_at
    AFTER INSERT ON messages
    FOR EACH ROW
    EXECUTE FUNCTION update_conversation_last_message_at();

-- --------------------------------------------------------------------------
-- message_attachments
-- --------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS message_attachments (
    message_attachment_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    message_id UUID NOT NULL REFERENCES messages(message_id) ON DELETE CASCADE,
    attachment_type SMALLINT NOT NULL,
    file_name VARCHAR(500),
    file_url TEXT NOT NULL,
    file_size_bytes BIGINT,
    mime_type VARCHAR(150),
    metadata JSONB,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_message_attachments_message
    ON message_attachments(message_id);

-- --------------------------------------------------------------------------
-- message_read_receipts
-- --------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS message_read_receipts (
    message_read_receipt_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    message_id UUID NOT NULL REFERENCES messages(message_id) ON DELETE CASCADE,
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    read_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_message_read_receipts_message_user
    ON message_read_receipts(message_id, user_id);
CREATE INDEX IF NOT EXISTS ix_message_read_receipts_site ON message_read_receipts(site_id, message_id);

-- --------------------------------------------------------------------------
-- Updated-at triggers
-- --------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION set_conversations_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS conversations_set_updated_at ON conversations;
CREATE TRIGGER conversations_set_updated_at
    BEFORE UPDATE ON conversations
    FOR EACH ROW
    EXECUTE FUNCTION set_conversations_updated_at();

CREATE OR REPLACE FUNCTION set_messages_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS messages_set_updated_at ON messages;
CREATE TRIGGER messages_set_updated_at
    BEFORE UPDATE ON messages
    FOR EACH ROW
    EXECUTE FUNCTION set_messages_updated_at();

-- --------------------------------------------------------------------------
-- Row Level Security
-- --------------------------------------------------------------------------
ALTER TABLE conversations ENABLE ROW LEVEL SECURITY;
ALTER TABLE conversation_participants ENABLE ROW LEVEL SECURITY;
ALTER TABLE messages ENABLE ROW LEVEL SECURITY;
ALTER TABLE message_attachments ENABLE ROW LEVEL SECURITY;
ALTER TABLE message_read_receipts ENABLE ROW LEVEL SECURITY;

CREATE POLICY conversations_site_access ON conversations
    FOR ALL
    USING (
        (current_setting('app.site_id', TRUE) IS NOT NULL AND site_id::text = current_setting('app.site_id', TRUE))
        OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
    )
    WITH CHECK (
        (current_setting('app.site_id', TRUE) IS NOT NULL AND site_id::text = current_setting('app.site_id', TRUE))
        OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
    );

CREATE POLICY conversation_participants_site_access ON conversation_participants
    FOR ALL
    USING (
        EXISTS (
            SELECT 1
            FROM conversations c
            WHERE c.conversation_id = conversation_participants.conversation_id
              AND (
                  (current_setting('app.site_id', TRUE) IS NOT NULL AND c.site_id::text = current_setting('app.site_id', TRUE))
                  OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
              )
        )
    )
    WITH CHECK (
        EXISTS (
            SELECT 1
            FROM conversations c
            WHERE c.conversation_id = conversation_participants.conversation_id
              AND (
                  (current_setting('app.site_id', TRUE) IS NOT NULL AND c.site_id::text = current_setting('app.site_id', TRUE))
                  OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
              )
        )
    );

CREATE POLICY messages_site_access ON messages
    FOR ALL
    USING (
        EXISTS (
            SELECT 1
            FROM conversations c
            WHERE c.conversation_id = messages.conversation_id
              AND (
                  (current_setting('app.site_id', TRUE) IS NOT NULL AND c.site_id::text = current_setting('app.site_id', TRUE))
                  OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
              )
        )
    )
    WITH CHECK (
        EXISTS (
            SELECT 1
            FROM conversations c
            WHERE c.conversation_id = messages.conversation_id
              AND (
                  (current_setting('app.site_id', TRUE) IS NOT NULL AND c.site_id::text = current_setting('app.site_id', TRUE))
                  OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
              )
        )
    );

CREATE POLICY message_attachments_site_access ON message_attachments
    FOR ALL
    USING (
        EXISTS (
            SELECT 1
            FROM messages m
            JOIN conversations c ON c.conversation_id = m.conversation_id
            WHERE m.message_id = message_attachments.message_id
              AND (
                  (current_setting('app.site_id', TRUE) IS NOT NULL AND c.site_id::text = current_setting('app.site_id', TRUE))
                  OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
              )
        )
    )
    WITH CHECK (
        EXISTS (
            SELECT 1
            FROM messages m
            JOIN conversations c ON c.conversation_id = m.conversation_id
            WHERE m.message_id = message_attachments.message_id
              AND (
                  (current_setting('app.site_id', TRUE) IS NOT NULL AND c.site_id::text = current_setting('app.site_id', TRUE))
                  OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
              )
        )
    );

CREATE POLICY message_read_receipts_site_access ON message_read_receipts
    FOR ALL
    USING (
        EXISTS (
            SELECT 1
            FROM messages m
            JOIN conversations c ON c.conversation_id = m.conversation_id
            WHERE m.message_id = message_read_receipts.message_id
              AND (
                  (current_setting('app.site_id', TRUE) IS NOT NULL AND c.site_id::text = current_setting('app.site_id', TRUE))
                  OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
              )
        )
    )
    WITH CHECK (
        EXISTS (
            SELECT 1
            FROM messages m
            JOIN conversations c ON c.conversation_id = m.conversation_id
            WHERE m.message_id = message_read_receipts.message_id
              AND (
                  (current_setting('app.site_id', TRUE) IS NOT NULL AND c.site_id::text = current_setting('app.site_id', TRUE))
                  OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
              )
        )
    );

COMMIT;
