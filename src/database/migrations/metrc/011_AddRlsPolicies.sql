-- Migration: Add RLS policies for METRC tables
-- Version: 011
-- Description: Adds Row Level Security policies for all METRC-related tables

-- Enable RLS on all METRC tables
ALTER TABLE plants ENABLE ROW LEVEL SECURITY;
ALTER TABLE plant_events ENABLE ROW LEVEL SECURITY;
ALTER TABLE harvests ENABLE ROW LEVEL SECURITY;
ALTER TABLE harvest_plants ENABLE ROW LEVEL SECURITY;
ALTER TABLE harvest_waste ENABLE ROW LEVEL SECURITY;
ALTER TABLE packages ENABLE ROW LEVEL SECURITY;
ALTER TABLE package_adjustments ENABLE ROW LEVEL SECURITY;
ALTER TABLE package_remediations ENABLE ROW LEVEL SECURITY;
ALTER TABLE items ENABLE ROW LEVEL SECURITY;
ALTER TABLE lab_test_batches ENABLE ROW LEVEL SECURITY;
ALTER TABLE lab_test_results ENABLE ROW LEVEL SECURITY;
ALTER TABLE processing_job_types ENABLE ROW LEVEL SECURITY;
ALTER TABLE processing_jobs ENABLE ROW LEVEL SECURITY;
ALTER TABLE processing_job_inputs ENABLE ROW LEVEL SECURITY;
ALTER TABLE processing_job_outputs ENABLE ROW LEVEL SECURITY;
ALTER TABLE metrc_sync_queue ENABLE ROW LEVEL SECURITY;
ALTER TABLE metrc_sync_events ENABLE ROW LEVEL SECURITY;
ALTER TABLE metrc_sync_errors ENABLE ROW LEVEL SECURITY;
ALTER TABLE metrc_inbound_cache ENABLE ROW LEVEL SECURITY;
ALTER TABLE metrc_tags ENABLE ROW LEVEL SECURITY;

-- RLS policies for plants
CREATE POLICY plants_site_isolation ON plants
    USING (site_id = current_setting('app.current_site_id', TRUE)::UUID);

CREATE POLICY plant_events_site_isolation ON plant_events
    USING (site_id = current_setting('app.current_site_id', TRUE)::UUID);

-- RLS policies for harvests
CREATE POLICY harvests_site_isolation ON harvests
    USING (site_id = current_setting('app.current_site_id', TRUE)::UUID);

CREATE POLICY harvest_plants_isolation ON harvest_plants
    USING (harvest_id IN (
        SELECT id FROM harvests WHERE site_id = current_setting('app.current_site_id', TRUE)::UUID
    ));

CREATE POLICY harvest_waste_isolation ON harvest_waste
    USING (harvest_id IN (
        SELECT id FROM harvests WHERE site_id = current_setting('app.current_site_id', TRUE)::UUID
    ));

-- RLS policies for packages
CREATE POLICY packages_site_isolation ON packages
    USING (site_id = current_setting('app.current_site_id', TRUE)::UUID);

CREATE POLICY package_adjustments_isolation ON package_adjustments
    USING (package_id IN (
        SELECT id FROM packages WHERE site_id = current_setting('app.current_site_id', TRUE)::UUID
    ));

CREATE POLICY package_remediations_isolation ON package_remediations
    USING (package_id IN (
        SELECT id FROM packages WHERE site_id = current_setting('app.current_site_id', TRUE)::UUID
    ));

-- RLS policies for items
CREATE POLICY items_site_isolation ON items
    USING (site_id = current_setting('app.current_site_id', TRUE)::UUID);

-- RLS policies for lab tests
CREATE POLICY lab_test_batches_site_isolation ON lab_test_batches
    USING (site_id = current_setting('app.current_site_id', TRUE)::UUID);

CREATE POLICY lab_test_results_isolation ON lab_test_results
    USING (lab_test_batch_id IN (
        SELECT id FROM lab_test_batches WHERE site_id = current_setting('app.current_site_id', TRUE)::UUID
    ));

-- RLS policies for processing jobs
CREATE POLICY processing_job_types_site_isolation ON processing_job_types
    USING (site_id = current_setting('app.current_site_id', TRUE)::UUID);

CREATE POLICY processing_jobs_site_isolation ON processing_jobs
    USING (site_id = current_setting('app.current_site_id', TRUE)::UUID);

CREATE POLICY processing_job_inputs_isolation ON processing_job_inputs
    USING (processing_job_id IN (
        SELECT id FROM processing_jobs WHERE site_id = current_setting('app.current_site_id', TRUE)::UUID
    ));

CREATE POLICY processing_job_outputs_isolation ON processing_job_outputs
    USING (processing_job_id IN (
        SELECT id FROM processing_jobs WHERE site_id = current_setting('app.current_site_id', TRUE)::UUID
    ));

-- RLS policies for METRC sync tables
CREATE POLICY metrc_sync_queue_site_isolation ON metrc_sync_queue
    USING (site_id = current_setting('app.current_site_id', TRUE)::UUID);

CREATE POLICY metrc_sync_events_site_isolation ON metrc_sync_events
    USING (site_id = current_setting('app.current_site_id', TRUE)::UUID);

CREATE POLICY metrc_sync_errors_site_isolation ON metrc_sync_errors
    USING (site_id = current_setting('app.current_site_id', TRUE)::UUID);

CREATE POLICY metrc_inbound_cache_site_isolation ON metrc_inbound_cache
    USING (site_id = current_setting('app.current_site_id', TRUE)::UUID);

CREATE POLICY metrc_tags_site_isolation ON metrc_tags
    USING (site_id = current_setting('app.current_site_id', TRUE)::UUID);

COMMENT ON TABLE plants IS 'RLS enabled - filtered by app.current_site_id';
COMMENT ON TABLE harvests IS 'RLS enabled - filtered by app.current_site_id';
COMMENT ON TABLE packages IS 'RLS enabled - filtered by app.current_site_id';
COMMENT ON TABLE items IS 'RLS enabled - filtered by app.current_site_id';
COMMENT ON TABLE lab_test_batches IS 'RLS enabled - filtered by app.current_site_id';
COMMENT ON TABLE processing_jobs IS 'RLS enabled - filtered by app.current_site_id';









