-- ============================================================================
-- Labor: Teams & Team Members
-- Migration: Create Teams Tables with RLS
-- 
-- Description: Teams for organizing employees with team leads, site-scoped
-- Dependencies: frp01/20250929_01_CreateIdentityTables.sql (users, sites)
-- ============================================================================

BEGIN;

-- ============================================================================
-- TEAMS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS teams (
    team_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    status VARCHAR(20) DEFAULT 'active'
        CHECK (status IN ('active', 'inactive', 'archived')),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    created_by UUID REFERENCES users(user_id),
    updated_by UUID REFERENCES users(user_id)
);

CREATE INDEX ix_teams_site ON teams (site_id) WHERE status = 'active';
CREATE INDEX ix_teams_status ON teams (status);
CREATE UNIQUE INDEX ux_teams_site_name ON teams (site_id, name) WHERE status = 'active';

COMMENT ON TABLE teams IS 'Teams for organizing employees within a site';
COMMENT ON COLUMN teams.status IS 'Team status: active, inactive, or archived';

-- ============================================================================
-- TEAM_MEMBERS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS team_members (
    team_member_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    team_id UUID NOT NULL REFERENCES teams(team_id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    is_team_lead BOOLEAN DEFAULT FALSE,
    joined_at TIMESTAMPTZ DEFAULT NOW(),
    removed_at TIMESTAMPTZ,
    removed_by UUID REFERENCES users(user_id),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    CONSTRAINT ux_team_members_active UNIQUE (team_id, user_id)
);

CREATE INDEX ix_team_members_team ON team_members (team_id) WHERE removed_at IS NULL;
CREATE INDEX ix_team_members_user ON team_members (user_id) WHERE removed_at IS NULL;
CREATE INDEX ix_team_members_leads ON team_members (team_id, is_team_lead) 
    WHERE is_team_lead = TRUE AND removed_at IS NULL;

COMMENT ON TABLE team_members IS 'Junction table linking users to teams with lead designation';
COMMENT ON COLUMN team_members.is_team_lead IS 'Whether this member is a team lead (can assign tasks to team)';

-- ============================================================================
-- UPDATED_AT TRIGGERS
-- ============================================================================
CREATE OR REPLACE FUNCTION set_teams_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS teams_set_updated_at ON teams;
CREATE TRIGGER teams_set_updated_at
    BEFORE UPDATE ON teams
    FOR EACH ROW
    EXECUTE FUNCTION set_teams_updated_at();

DROP TRIGGER IF EXISTS team_members_set_updated_at ON team_members;
CREATE TRIGGER team_members_set_updated_at
    BEFORE UPDATE ON team_members
    FOR EACH ROW
    EXECUTE FUNCTION set_teams_updated_at();

-- ============================================================================
-- ROW LEVEL SECURITY
-- ============================================================================

-- TEAMS: Site-scoped access
ALTER TABLE teams ENABLE ROW LEVEL SECURITY;

CREATE POLICY teams_site_access ON teams
    FOR ALL
    USING (
        site_id IN (
            SELECT site_id FROM user_sites
            WHERE user_id = current_setting('app.current_user_id', TRUE)::UUID
            AND revoked_at IS NULL
        )
        OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
    )
    WITH CHECK (
        site_id IN (
            SELECT site_id FROM user_sites
            WHERE user_id = current_setting('app.current_user_id', TRUE)::UUID
            AND revoked_at IS NULL
        )
        OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
    );

-- TEAM_MEMBERS: Access through team's site
ALTER TABLE team_members ENABLE ROW LEVEL SECURITY;

CREATE POLICY team_members_site_access ON team_members
    FOR SELECT
    USING (
        EXISTS (
            SELECT 1 FROM teams t
            JOIN user_sites us ON us.site_id = t.site_id
            WHERE t.team_id = team_members.team_id
            AND us.user_id = current_setting('app.current_user_id', TRUE)::UUID
            AND us.revoked_at IS NULL
        )
        OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
    );

-- Only managers, supervisors, team leads, or admins can modify team members
CREATE POLICY team_members_modify ON team_members
    FOR INSERT
    WITH CHECK (
        current_setting('app.user_role', TRUE) IN ('admin', 'manager', 'supervisor', 'service_account')
        OR EXISTS (
            SELECT 1 FROM team_members tm
            WHERE tm.team_id = team_members.team_id
            AND tm.user_id = current_setting('app.current_user_id', TRUE)::UUID
            AND tm.is_team_lead = TRUE
            AND tm.removed_at IS NULL
        )
    );

CREATE POLICY team_members_update ON team_members
    FOR UPDATE
    USING (
        current_setting('app.user_role', TRUE) IN ('admin', 'manager', 'supervisor', 'service_account')
        OR EXISTS (
            SELECT 1 FROM team_members tm
            WHERE tm.team_id = team_members.team_id
            AND tm.user_id = current_setting('app.current_user_id', TRUE)::UUID
            AND tm.is_team_lead = TRUE
            AND tm.removed_at IS NULL
        )
    )
    WITH CHECK (
        current_setting('app.user_role', TRUE) IN ('admin', 'manager', 'supervisor', 'service_account')
        OR EXISTS (
            SELECT 1 FROM team_members tm
            WHERE tm.team_id = team_members.team_id
            AND tm.user_id = current_setting('app.current_user_id', TRUE)::UUID
            AND tm.is_team_lead = TRUE
            AND tm.removed_at IS NULL
        )
    );

CREATE POLICY team_members_delete ON team_members
    FOR DELETE
    USING (
        current_setting('app.user_role', TRUE) IN ('admin', 'manager', 'service_account')
    );

-- ============================================================================
-- GRANTS
-- ============================================================================
GRANT SELECT, INSERT, UPDATE, DELETE ON teams TO harvestry_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON team_members TO harvestry_app;

-- ============================================================================
-- COMPLETION
-- ============================================================================

COMMIT;

DO $$
BEGIN
    RAISE NOTICE '✓ Teams tables created successfully';
    RAISE NOTICE '  - teams, team_members';
    RAISE NOTICE '  - RLS policies enabled for site-scoped access';
    RAISE NOTICE '  - Team leads can manage their team members';
END $$;
