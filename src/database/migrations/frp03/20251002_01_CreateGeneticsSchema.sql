-- ============================================================================
-- FRP-03: Genetics, Strains & Batches
-- Migration: Create Genetics Schema (Domain + Lifecycle + Mother Plants)
-- ----------------------------------------------------------------------------
-- This migration creates the full relational model for FRP-03, including:
--   * Genetics / Phenotypes / Strains master data
--   * Batch lifecycle (stages, transitions, history, events, relationships)
--   * Batch code rules for jurisdictional compliance
--   * Mother plant management, health logs, and propagation controls
--   * Propagation override workflow tracking
-- All tables are scoped by site_id to support RLS policies applied separately.
-- ============================================================================

CREATE SCHEMA IF NOT EXISTS genetics;

-- ---------------------------------------------------------------------------
-- 1) Genetics Master Data
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS genetics.genetics (
    id UUID PRIMARY KEY,
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    name VARCHAR(200) NOT NULL,
    description TEXT NOT NULL,
    genetic_type VARCHAR(50) NOT NULL,
    thc_min_percentage NUMERIC(5,2),
    thc_max_percentage NUMERIC(5,2),
    cbd_min_percentage NUMERIC(5,2),
    cbd_max_percentage NUMERIC(5,2),
    flowering_time_days INTEGER,
    yield_potential VARCHAR(50) NOT NULL,
    growth_characteristics JSONB NOT NULL DEFAULT '{}'::JSONB,
    terpene_profile JSONB NOT NULL DEFAULT '{}'::JSONB,
    breeding_notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by_user_id UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by_user_id UUID NOT NULL,
    CONSTRAINT uq_genetics_site_name UNIQUE (site_id, name)
);

CREATE INDEX IF NOT EXISTS ix_genetics_site ON genetics.genetics(site_id);
CREATE INDEX IF NOT EXISTS ix_genetics_name ON genetics.genetics(site_id, lower(name));

CREATE TABLE IF NOT EXISTS genetics.phenotypes (
    id UUID PRIMARY KEY,
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    genetics_id UUID NOT NULL REFERENCES genetics.genetics(id) ON DELETE CASCADE,
    name VARCHAR(200) NOT NULL,
    description TEXT NOT NULL,
    expression_notes TEXT,
    visual_characteristics JSONB NOT NULL DEFAULT '{}'::JSONB,
    aroma_profile JSONB NOT NULL DEFAULT '{}'::JSONB,
    growth_pattern JSONB NOT NULL DEFAULT '{}'::JSONB,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by_user_id UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by_user_id UUID NOT NULL,
    CONSTRAINT uq_phenotypes_site_genetics_name UNIQUE (site_id, genetics_id, name)
);

CREATE INDEX IF NOT EXISTS ix_phenotypes_site ON genetics.phenotypes(site_id);
CREATE INDEX IF NOT EXISTS ix_phenotypes_genetics ON genetics.phenotypes(genetics_id);

CREATE TABLE IF NOT EXISTS genetics.strains (
    id UUID PRIMARY KEY,
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    genetics_id UUID NOT NULL REFERENCES genetics.genetics(id) ON DELETE CASCADE,
    phenotype_id UUID REFERENCES genetics.phenotypes(id) ON DELETE SET NULL,
    name VARCHAR(200) NOT NULL,
    breeder VARCHAR(200),
    seed_bank VARCHAR(200),
    description TEXT NOT NULL,
    cultivation_notes TEXT,
    expected_harvest_window_days INTEGER,
    target_environment JSONB NOT NULL DEFAULT '{}'::JSONB,
    compliance_requirements JSONB NOT NULL DEFAULT '{}'::JSONB,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by_user_id UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by_user_id UUID NOT NULL,
    CONSTRAINT uq_strains_site_name UNIQUE (site_id, name)
);

CREATE INDEX IF NOT EXISTS ix_strains_site ON genetics.strains(site_id);
CREATE INDEX IF NOT EXISTS ix_strains_genetics ON genetics.strains(genetics_id);
CREATE INDEX IF NOT EXISTS ix_strains_phenotype ON genetics.strains(phenotype_id);

-- ---------------------------------------------------------------------------
-- 2) Batch Lifecycle Configuration (Stages & Transitions)
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS genetics.batch_stage_definitions (
    id UUID PRIMARY KEY,
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    stage_key VARCHAR(50) NOT NULL,
    display_name VARCHAR(200) NOT NULL,
    description TEXT,
    sequence_order INTEGER NOT NULL,
    is_terminal BOOLEAN NOT NULL DEFAULT FALSE,
    requires_harvest_metrics BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by_user_id UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by_user_id UUID NOT NULL,
    CONSTRAINT uq_batch_stage_site_key UNIQUE (site_id, stage_key)
);

CREATE INDEX IF NOT EXISTS ix_batch_stage_definitions_site ON genetics.batch_stage_definitions(site_id);
CREATE INDEX IF NOT EXISTS ix_batch_stage_definitions_sequence ON genetics.batch_stage_definitions(site_id, sequence_order);

CREATE TABLE IF NOT EXISTS genetics.batch_stage_transitions (
    id UUID PRIMARY KEY,
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    from_stage_id UUID NOT NULL REFERENCES genetics.batch_stage_definitions(id) ON DELETE CASCADE,
    to_stage_id UUID NOT NULL REFERENCES genetics.batch_stage_definitions(id) ON DELETE CASCADE,
    auto_advance BOOLEAN NOT NULL DEFAULT FALSE,
    requires_approval BOOLEAN NOT NULL DEFAULT FALSE,
    approval_role VARCHAR(100),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by_user_id UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by_user_id UUID NOT NULL,
    CONSTRAINT uq_stage_transition UNIQUE (site_id, from_stage_id, to_stage_id)
);

CREATE INDEX IF NOT EXISTS ix_batch_stage_transitions_site ON genetics.batch_stage_transitions(site_id);
CREATE INDEX IF NOT EXISTS ix_batch_stage_transitions_from ON genetics.batch_stage_transitions(site_id, from_stage_id);
CREATE INDEX IF NOT EXISTS ix_batch_stage_transitions_to ON genetics.batch_stage_transitions(site_id, to_stage_id);

CREATE TABLE IF NOT EXISTS genetics.batch_stage_history (
    id UUID PRIMARY KEY,
    batch_id UUID NOT NULL,
    from_stage_id UUID,
    to_stage_id UUID NOT NULL,
    changed_by_user_id UUID NOT NULL,
    changed_at TIMESTAMPTZ NOT NULL,
    notes TEXT,
    CONSTRAINT fk_stage_history_to_stage FOREIGN KEY (to_stage_id) REFERENCES genetics.batch_stage_definitions(id) ON DELETE RESTRICT,
    CONSTRAINT fk_stage_history_from_stage FOREIGN KEY (from_stage_id) REFERENCES genetics.batch_stage_definitions(id) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS ix_batch_stage_history_batch ON genetics.batch_stage_history(batch_id);
CREATE INDEX IF NOT EXISTS ix_batch_stage_history_to_stage ON genetics.batch_stage_history(to_stage_id);

-- ---------------------------------------------------------------------------
-- 3) Batches + Audit Trail
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS genetics.batches (
    id UUID PRIMARY KEY,
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    strain_id UUID NOT NULL REFERENCES genetics.strains(id) ON DELETE RESTRICT,
    batch_code VARCHAR(50) NOT NULL,
    batch_name VARCHAR(200) NOT NULL,
    batch_type VARCHAR(50) NOT NULL,
    source_type VARCHAR(50) NOT NULL,
    parent_batch_id UUID REFERENCES genetics.batches(id) ON DELETE SET NULL,
    generation INTEGER NOT NULL DEFAULT 1,
    plant_count INTEGER NOT NULL,
    target_plant_count INTEGER NOT NULL,
    current_stage_id UUID NOT NULL REFERENCES genetics.batch_stage_definitions(id) ON DELETE RESTRICT,
    stage_started_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    expected_harvest_date DATE,
    actual_harvest_date DATE,
    location_id UUID,
    room_id UUID,
    zone_id UUID,
    status VARCHAR(50) NOT NULL,
    notes TEXT,
    metadata JSONB NOT NULL DEFAULT '{}'::JSONB,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by_user_id UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by_user_id UUID NOT NULL,
    CONSTRAINT uq_batches_site_code UNIQUE (site_id, batch_code)
);

CREATE INDEX IF NOT EXISTS ix_batches_site ON genetics.batches(site_id);
CREATE INDEX IF NOT EXISTS ix_batches_strain ON genetics.batches(site_id, strain_id);
CREATE INDEX IF NOT EXISTS ix_batches_stage ON genetics.batches(site_id, current_stage_id);
CREATE INDEX IF NOT EXISTS ix_batches_status ON genetics.batches(site_id, status);
CREATE INDEX IF NOT EXISTS ix_batches_parent ON genetics.batches(parent_batch_id);

CREATE TABLE IF NOT EXISTS genetics.batch_events (
    id UUID PRIMARY KEY,
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    batch_id UUID NOT NULL REFERENCES genetics.batches(id) ON DELETE CASCADE,
    event_type VARCHAR(100) NOT NULL,
    event_data JSONB NOT NULL DEFAULT '{}'::JSONB,
    performed_by_user_id UUID NOT NULL,
    performed_at TIMESTAMPTZ NOT NULL,
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS ix_batch_events_batch ON genetics.batch_events(batch_id);
CREATE INDEX IF NOT EXISTS ix_batch_events_site_type ON genetics.batch_events(site_id, event_type);

CREATE TABLE IF NOT EXISTS genetics.batch_relationships (
    id UUID PRIMARY KEY,
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    parent_batch_id UUID NOT NULL REFERENCES genetics.batches(id) ON DELETE CASCADE,
    child_batch_id UUID NOT NULL REFERENCES genetics.batches(id) ON DELETE CASCADE,
    relationship_type VARCHAR(50) NOT NULL,
    plant_count_transferred INTEGER,
    transfer_date DATE NOT NULL,
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by_user_id UUID NOT NULL,
    CONSTRAINT uq_batch_relationship UNIQUE (parent_batch_id, child_batch_id, relationship_type, transfer_date)
);

CREATE INDEX IF NOT EXISTS ix_batch_relationships_site ON genetics.batch_relationships(site_id);
CREATE INDEX IF NOT EXISTS ix_batch_relationships_parent ON genetics.batch_relationships(parent_batch_id);
CREATE INDEX IF NOT EXISTS ix_batch_relationships_child ON genetics.batch_relationships(child_batch_id);

CREATE TABLE IF NOT EXISTS genetics.batch_code_rules (
    id UUID PRIMARY KEY,
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    name VARCHAR(100) NOT NULL,
    rule_definition JSONB NOT NULL,
    reset_policy VARCHAR(50) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by_user_id UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by_user_id UUID NOT NULL,
    CONSTRAINT uq_batch_code_rule_name UNIQUE (site_id, name)
);

CREATE INDEX IF NOT EXISTS ix_batch_code_rules_site ON genetics.batch_code_rules(site_id);
CREATE INDEX IF NOT EXISTS ix_batch_code_rules_active ON genetics.batch_code_rules(site_id) WHERE is_active;

-- ---------------------------------------------------------------------------
-- 4) Mother Plants & Health Tracking
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS genetics.mother_plants (
    id UUID PRIMARY KEY,
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    batch_id UUID NOT NULL REFERENCES genetics.batches(id) ON DELETE RESTRICT,
    strain_id UUID NOT NULL REFERENCES genetics.strains(id) ON DELETE RESTRICT,
    plant_tag VARCHAR(50) NOT NULL,
    status VARCHAR(50) NOT NULL,
    location_id UUID,
    room_id UUID,
    date_established DATE NOT NULL,
    last_propagation_date DATE,
    propagation_count INTEGER NOT NULL DEFAULT 0,
    max_propagation_count INTEGER,
    notes TEXT,
    metadata JSONB NOT NULL DEFAULT '{}'::JSONB,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by_user_id UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by_user_id UUID NOT NULL,
    CONSTRAINT uq_mother_plants_site_tag UNIQUE (site_id, plant_tag)
);

CREATE INDEX IF NOT EXISTS ix_mother_plants_site ON genetics.mother_plants(site_id);
CREATE INDEX IF NOT EXISTS ix_mother_plants_status ON genetics.mother_plants(site_id, status);
CREATE INDEX IF NOT EXISTS ix_mother_plants_strain ON genetics.mother_plants(site_id, strain_id);

CREATE TABLE IF NOT EXISTS genetics.mother_health_logs (
    id UUID PRIMARY KEY,
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    mother_plant_id UUID NOT NULL REFERENCES genetics.mother_plants(id) ON DELETE CASCADE,
    log_date DATE NOT NULL,
    health_status VARCHAR(50) NOT NULL,
    pest_pressure VARCHAR(50) NOT NULL,
    disease_pressure VARCHAR(50) NOT NULL,
    nutrient_deficiencies TEXT[] NOT NULL DEFAULT ARRAY[]::TEXT[],
    observations TEXT,
    treatments_applied TEXT,
    environmental_notes TEXT,
    photo_urls TEXT[] NOT NULL DEFAULT ARRAY[]::TEXT[],
    logged_by_user_id UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_mother_health_unique_log UNIQUE (mother_plant_id, log_date)
);

CREATE INDEX IF NOT EXISTS ix_mother_health_logs_plant ON genetics.mother_health_logs(mother_plant_id, log_date DESC);
CREATE INDEX IF NOT EXISTS ix_mother_health_logs_site ON genetics.mother_health_logs(site_id);


CREATE TABLE IF NOT EXISTS genetics.mother_propagation_events (
    id UUID PRIMARY KEY,
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    mother_plant_id UUID NOT NULL REFERENCES genetics.mother_plants(id) ON DELETE CASCADE,
    propagated_count INTEGER NOT NULL CHECK (propagated_count > 0),
    recorded_on DATE NOT NULL,
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by_user_id UUID NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_mother_propagation_events_site ON genetics.mother_propagation_events(site_id, recorded_on DESC);
CREATE INDEX IF NOT EXISTS ix_mother_propagation_events_mother ON genetics.mother_propagation_events(mother_plant_id, recorded_on DESC);

-- ---------------------------------------------------------------------------
-- 5) Propagation Settings & Overrides
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS genetics.propagation_settings (
    id UUID PRIMARY KEY,
    site_id UUID NOT NULL UNIQUE REFERENCES sites(site_id) ON DELETE CASCADE,
    daily_limit INTEGER,
    weekly_limit INTEGER,
    mother_propagation_limit INTEGER,
    requires_override_approval BOOLEAN NOT NULL DEFAULT TRUE,
    approver_role VARCHAR(100),
    approver_policy JSONB NOT NULL DEFAULT '{}'::JSONB,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by_user_id UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by_user_id UUID NOT NULL
);

CREATE TABLE IF NOT EXISTS genetics.propagation_override_requests (
    id UUID PRIMARY KEY,
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    requested_by_user_id UUID NOT NULL,
    mother_plant_id UUID REFERENCES genetics.mother_plants(id) ON DELETE SET NULL,
    batch_id UUID REFERENCES genetics.batches(id) ON DELETE SET NULL,
    requested_quantity INTEGER NOT NULL,
    reason TEXT NOT NULL,
    status VARCHAR(30) NOT NULL,
    requested_on TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    approved_by_user_id UUID,
    resolved_on TIMESTAMPTZ,
    decision_notes TEXT
);

CREATE INDEX IF NOT EXISTS ix_propagation_override_site ON genetics.propagation_override_requests(site_id);
CREATE INDEX IF NOT EXISTS ix_propagation_override_status ON genetics.propagation_override_requests(site_id, status);
CREATE INDEX IF NOT EXISTS ix_propagation_override_mother ON genetics.propagation_override_requests(mother_plant_id);

-- ---------------------------------------------------------------------------
-- End of schema creation
-- ---------------------------------------------------------------------------
