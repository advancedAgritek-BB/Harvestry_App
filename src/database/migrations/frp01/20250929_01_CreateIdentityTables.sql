-- ============================================================================
-- FRP-01: Identity, Authentication & Authorization
-- Migration: Create Identity Tables with RLS
-- 
-- Description: Users, Roles, Sites, Badges, Sessions with site-scoped RLS
-- Dependencies: baseline/20250929_CreateAuditHashChain.sql
-- ============================================================================

-- ============================================================================
-- USERS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS users (
    user_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) UNIQUE NOT NULL,
    email_verified BOOLEAN DEFAULT FALSE,
    phone_number VARCHAR(20),
    phone_verified BOOLEAN DEFAULT FALSE,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    display_name VARCHAR(200),
    password_hash VARCHAR(255), -- NULL for badge-only users
    password_salt VARCHAR(255),
    failed_login_attempts INT DEFAULT 0,
    locked_until TIMESTAMPTZ,
    last_login_at TIMESTAMPTZ,
    profile_photo_url TEXT,
    language_preference VARCHAR(10) DEFAULT 'en',
    timezone VARCHAR(50) DEFAULT 'UTC',
    status VARCHAR(20) DEFAULT 'active' 
        CHECK (status IN ('active', 'inactive', 'suspended', 'terminated')),
    metadata JSONB DEFAULT '{}'::JSONB,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    created_by UUID,
    updated_by UUID
);

CREATE INDEX ix_users_email ON users (email);
CREATE INDEX ix_users_status ON users (status) WHERE status = 'active';
CREATE INDEX ix_users_last_login ON users (last_login_at DESC);

COMMENT ON TABLE users IS 'Core user accounts for authentication and authorization';
COMMENT ON COLUMN users.password_hash IS 'Nullable for badge-only operators';
COMMENT ON COLUMN users.locked_until IS 'Account lock expiration after failed login attempts';

-- ============================================================================
-- ROLES TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS roles (
    role_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    role_name VARCHAR(100) UNIQUE NOT NULL,
    display_name VARCHAR(200) NOT NULL,
    description TEXT,
    permissions JSONB NOT NULL DEFAULT '[]'::JSONB, -- Array of permission strings
    is_system_role BOOLEAN DEFAULT FALSE, -- Cannot be deleted if true
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX ix_roles_name ON roles (role_name);

COMMENT ON TABLE roles IS 'Role definitions with associated permissions';
COMMENT ON COLUMN roles.permissions IS 'JSON array of permission strings, e.g., ["users:read", "tasks:write"]';

-- ============================================================================
-- ORGANIZATIONS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS organizations (
    organization_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(200) NOT NULL,
    slug VARCHAR(100) UNIQUE NOT NULL,
    status VARCHAR(20) DEFAULT 'active'
        CHECK (status IN ('active', 'inactive', 'pending', 'suspended')),
    metadata JSONB DEFAULT '{}'::JSONB,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    created_by UUID,
    updated_by UUID
);

CREATE INDEX ix_organizations_slug ON organizations (slug);
CREATE INDEX ix_organizations_status ON organizations (status) WHERE status = 'active';

COMMENT ON TABLE organizations IS 'Top-level business entities that own one or more sites/brands';
COMMENT ON COLUMN organizations.slug IS 'URL-friendly identifier (unique per organization)';

-- ============================================================================
-- SITES TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS sites (
    site_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    org_id UUID NOT NULL REFERENCES organizations(organization_id) ON DELETE CASCADE,
    site_name VARCHAR(200) NOT NULL,
    site_code VARCHAR(50) UNIQUE NOT NULL, -- e.g., "DGC-DEN-01"
    address_line1 VARCHAR(255),
    address_line2 VARCHAR(255),
    city VARCHAR(100),
    state_province VARCHAR(100),
    postal_code VARCHAR(20),
    country VARCHAR(2) DEFAULT 'US',
    timezone VARCHAR(50) DEFAULT 'America/Denver',
    license_number VARCHAR(100), -- Compliance license
    license_type VARCHAR(50), -- e.g., "cultivation", "processing"
    license_expiration DATE,
    site_type VARCHAR(50) DEFAULT 'cultivation'
        CHECK (site_type IN ('cultivation', 'processing', 'distribution', 'retail', 'testing')),
    status VARCHAR(20) DEFAULT 'active'
        CHECK (status IN ('active', 'inactive', 'pending', 'suspended')),
    site_policies JSONB DEFAULT '{}'::JSONB, -- e.g., {"manual_irrigation_approval": true}
    metadata JSONB DEFAULT '{}'::JSONB,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX ix_sites_org ON sites (org_id);
CREATE INDEX ix_sites_code ON sites (site_code);
CREATE INDEX ix_sites_status ON sites (status) WHERE status = 'active';

COMMENT ON TABLE sites IS 'Physical grow/processing locations with compliance licenses';
COMMENT ON COLUMN sites.site_policies IS 'Site-specific feature flags and policies, e.g., {"manual_irrigation_approval": true}';

-- ============================================================================
-- USER_SITES (Junction Table)
-- ============================================================================
CREATE TABLE IF NOT EXISTS user_sites (
    user_site_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    role_id UUID NOT NULL REFERENCES roles(role_id) ON DELETE RESTRICT,
    is_primary_site BOOLEAN DEFAULT FALSE,
    assigned_at TIMESTAMPTZ DEFAULT NOW(),
    assigned_by UUID REFERENCES users(user_id),
    revoked_at TIMESTAMPTZ,
    revoked_by UUID REFERENCES users(user_id),
    revoke_reason TEXT,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    created_by UUID,
    updated_by UUID,
    UNIQUE (user_id, site_id) -- User can be assigned to a site only once
);

CREATE INDEX ix_user_sites_user ON user_sites (user_id) WHERE revoked_at IS NULL;
CREATE INDEX ix_user_sites_site ON user_sites (site_id) WHERE revoked_at IS NULL;
CREATE INDEX ix_user_sites_role ON user_sites (role_id);

COMMENT ON TABLE user_sites IS 'Many-to-many: Users assigned to sites with specific roles';
COMMENT ON COLUMN user_sites.is_primary_site IS 'Default site when user logs in';

-- ============================================================================
-- BADGES TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS badges (
    badge_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    badge_code VARCHAR(100) UNIQUE NOT NULL, -- Scanned barcode/RFID
    badge_type VARCHAR(50) DEFAULT 'physical' 
        CHECK (badge_type IN ('physical', 'virtual', 'temp')),
    status VARCHAR(20) DEFAULT 'active'
        CHECK (status IN ('active', 'inactive', 'lost', 'revoked')),
    issued_at TIMESTAMPTZ DEFAULT NOW(),
    expires_at TIMESTAMPTZ,
    last_used_at TIMESTAMPTZ,
    revoked_at TIMESTAMPTZ,
    revoked_by UUID REFERENCES users(user_id),
    revoke_reason TEXT,
    metadata JSONB DEFAULT '{}'::JSONB,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX ix_badges_code ON badges (badge_code) WHERE status = 'active';
CREATE INDEX ix_badges_user ON badges (user_id) WHERE status = 'active';
CREATE INDEX ix_badges_site ON badges (site_id);
CREATE INDEX ix_badges_expires ON badges (expires_at) WHERE expires_at IS NOT NULL AND status = 'active';

COMMENT ON TABLE badges IS 'Physical/virtual badges for operator authentication';
COMMENT ON COLUMN badges.badge_code IS 'Barcode or RFID identifier scanned for login';

-- ============================================================================
-- SESSIONS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS sessions (
    session_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    site_id UUID REFERENCES sites(site_id) ON DELETE SET NULL,
    session_token VARCHAR(255) UNIQUE NOT NULL,
    refresh_token VARCHAR(255),
    device_fingerprint TEXT,
    ip_address INET,
    user_agent TEXT,
    login_method VARCHAR(50) DEFAULT 'password'
        CHECK (login_method IN ('password', 'badge', 'sso', 'api_key')),
    session_start TIMESTAMPTZ DEFAULT NOW(),
    session_end TIMESTAMPTZ,
    last_activity TIMESTAMPTZ DEFAULT NOW(),
    expires_at TIMESTAMPTZ NOT NULL,
    is_revoked BOOLEAN DEFAULT FALSE,
    revoke_reason TEXT,
    metadata JSONB DEFAULT '{}'::JSONB
);

CREATE INDEX ix_sessions_token ON sessions (session_token) WHERE is_revoked = FALSE;
CREATE INDEX ix_sessions_user ON sessions (user_id, session_start DESC);
CREATE INDEX ix_sessions_expires ON sessions (expires_at) WHERE is_revoked = FALSE;
CREATE INDEX ix_sessions_active ON sessions (user_id, is_revoked, expires_at);

COMMENT ON TABLE sessions IS 'Active user sessions with login metadata and revocation support';
COMMENT ON COLUMN sessions.login_method IS 'How user authenticated: password, badge, SSO, or API key';

-- ============================================================================
-- RLS POLICIES
-- ============================================================================

-- USERS: User-scoped (can only see themselves) + Service Account bypass
ALTER TABLE users ENABLE ROW LEVEL SECURITY;

CREATE POLICY users_self_access ON users
FOR ALL
USING (
    user_id = current_setting('app.current_user_id', TRUE)::UUID
    OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
);

-- ROLES: Read-only for all users, admin-only for modifications
ALTER TABLE roles ENABLE ROW LEVEL SECURITY;

CREATE POLICY roles_read_all ON roles
FOR SELECT
USING (TRUE); -- Anyone can read roles

CREATE POLICY roles_admin_delete ON roles
FOR DELETE
USING (current_setting('app.user_role', TRUE) IN ('admin', 'service_account'));

CREATE POLICY roles_admin_insert ON roles
FOR INSERT
WITH CHECK (current_setting('app.user_role', TRUE) IN ('admin', 'service_account'));

CREATE POLICY roles_admin_update ON roles
FOR UPDATE
USING (current_setting('app.user_role', TRUE) IN ('admin', 'service_account'))
WITH CHECK (current_setting('app.user_role', TRUE) IN ('admin', 'service_account'));

-- SITES: Site-scoped access
ALTER TABLE sites ENABLE ROW LEVEL SECURITY;

CREATE POLICY sites_member_access ON sites
FOR ALL
USING (
    site_id IN (
        SELECT site_id FROM user_sites
        WHERE user_id = current_setting('app.current_user_id', TRUE)::UUID
        AND revoked_at IS NULL
    )
    OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
);

-- USER_SITES: Can see own assignments + admins see all
ALTER TABLE user_sites ENABLE ROW LEVEL SECURITY;

CREATE POLICY user_sites_self_access ON user_sites
FOR SELECT
USING (
    user_id = current_setting('app.current_user_id', TRUE)::UUID
    OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
);

CREATE POLICY user_sites_admin_insert ON user_sites
FOR INSERT
WITH CHECK (current_setting('app.user_role', TRUE) IN ('admin', 'manager', 'service_account'));

CREATE POLICY user_sites_admin_update ON user_sites
FOR UPDATE
USING (current_setting('app.user_role', TRUE) IN ('admin', 'manager', 'service_account'))
WITH CHECK (current_setting('app.user_role', TRUE) IN ('admin', 'manager', 'service_account'));

CREATE POLICY user_sites_admin_delete ON user_sites
FOR DELETE
USING (current_setting('app.user_role', TRUE) IN ('admin', 'manager'));

-- BADGES: Site-scoped (can see badges for sites they're assigned to)
ALTER TABLE badges ENABLE ROW LEVEL SECURITY;

CREATE POLICY badges_site_scoped ON badges
FOR ALL
USING (
    site_id IN (
        SELECT site_id FROM user_sites
        WHERE user_id = current_setting('app.current_user_id', TRUE)::UUID
        AND revoked_at IS NULL
    )
    OR current_setting('app.user_role', TRUE) = 'service_account'
);

-- SESSIONS: User-scoped (can only see own sessions)
ALTER TABLE sessions ENABLE ROW LEVEL SECURITY;

CREATE POLICY sessions_self_access ON sessions
FOR SELECT
USING (
    user_id = current_setting('app.current_user_id', TRUE)::UUID
    OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
);

CREATE POLICY sessions_service_insert ON sessions
FOR INSERT
WITH CHECK (current_setting('app.user_role', TRUE) IN ('service_account', 'admin'));

CREATE POLICY sessions_service_update ON sessions
FOR UPDATE
USING (current_setting('app.user_role', TRUE) IN ('service_account', 'admin'))
WITH CHECK (current_setting('app.user_role', TRUE) IN ('service_account', 'admin'));

CREATE POLICY sessions_service_delete ON sessions
FOR DELETE
USING (current_setting('app.user_role', TRUE) IN ('service_account', 'admin'));

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'harvestry_app') THEN
        CREATE ROLE harvestry_app;
    END IF;
END $$;

GRANT SELECT, INSERT, UPDATE, DELETE ON users TO harvestry_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON user_sites TO harvestry_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON badges TO harvestry_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON sessions TO harvestry_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON roles TO harvestry_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON sites TO harvestry_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON organizations TO harvestry_app;
GRANT USAGE ON ALL SEQUENCES IN SCHEMA public TO harvestry_app;

-- Update outbox / dead-letter RLS now that user_sites exists
DO $$
BEGIN
    IF to_regclass('public.outbox_messages') IS NOT NULL THEN
        EXECUTE 'DROP POLICY IF EXISTS outbox_messages_site_isolation ON outbox_messages';
        EXECUTE '
            CREATE POLICY outbox_messages_site_isolation ON outbox_messages
            FOR ALL
            USING (
                site_id IN (
                    SELECT site_id FROM user_sites
                    WHERE user_id = current_setting(''app.current_user_id'', TRUE)::UUID
                )
                OR current_setting(''app.user_role'', TRUE) = ''service_account''
            )
        ';
    END IF;

    IF to_regclass('public.dead_letter_queue') IS NOT NULL THEN
        EXECUTE 'DROP POLICY IF EXISTS dlq_site_isolation ON dead_letter_queue';
        EXECUTE '
            CREATE POLICY dlq_site_isolation ON dead_letter_queue
            FOR ALL
            USING (
                site_id IN (
                    SELECT site_id FROM user_sites
                    WHERE user_id = current_setting(''app.current_user_id'', TRUE)::UUID
                )
                OR current_setting(''app.user_role'', TRUE) = ''service_account''
            )
        ';
    END IF;
END;
$$;

-- ============================================================================
-- SEED DEFAULT ROLES
-- ============================================================================

INSERT INTO roles (role_name, display_name, description, permissions, is_system_role) VALUES
    ('operator', 'Operator', 'Floor-level operator with task execution permissions', 
     '["tasks:read", "tasks:start", "tasks:complete", "inventory:scan", "inventory:move"]'::JSONB, TRUE),
    ('supervisor', 'Supervisor', 'Team lead with approval and oversight permissions',
     '["tasks:read", "tasks:create", "tasks:assign", "inventory:read", "inventory:adjust", "users:read"]'::JSONB, TRUE),
    ('manager', 'Manager', 'Site manager with full operational control',
     '["tasks:*", "inventory:*", "processing:*", "users:read", "users:assign", "irrigation:override"]'::JSONB, TRUE),
    ('admin', 'Administrator', 'System administrator with full access',
     '["*:*"]'::JSONB, TRUE),
    ('service_account', 'Service Account', 'Background worker and system integration account',
     '["*:*"]'::JSONB, TRUE)
ON CONFLICT (role_name) DO NOTHING;

-- ============================================================================
-- AUDIT TRIGGERS
-- ============================================================================

-- Trigger to update updated_at timestamp
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER users_updated_at BEFORE UPDATE ON users
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER sites_updated_at BEFORE UPDATE ON sites
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER roles_updated_at BEFORE UPDATE ON roles
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER badges_updated_at BEFORE UPDATE ON badges
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER user_sites_updated_at BEFORE UPDATE ON user_sites
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Note: sessions table uses last_activity instead of updated_at, so no trigger needed

-- ============================================================================
-- COMPLETION
-- ============================================================================

DO $$
BEGIN
    RAISE NOTICE '✓ FRP-01 Identity tables created successfully';
    RAISE NOTICE '  - users, roles, sites, user_sites, badges, sessions';
    RAISE NOTICE '  - RLS policies enabled for site-scoped access';
    RAISE NOTICE '  - Default roles seeded (operator, supervisor, manager, admin, service_account)';
END $$;
