-- ============================================================================
-- FRP-03: Genetics, Strains & Batches
-- Migration: Backfill Unique Constraints (Idempotent)
-- ----------------------------------------------------------------------------
-- Ensures legacy environments that predate the consolidated schema gain the
-- same uniqueness guarantees that new installs receive during creation.
-- Each block safely adds the constraint if it does not already exist, then
-- documents it for future reference.
-- ============================================================================

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'uq_genetics_site_name'
          AND conrelid = 'genetics.genetics'::regclass
    ) THEN
        ALTER TABLE genetics.genetics
            ADD CONSTRAINT uq_genetics_site_name UNIQUE (site_id, name);
    END IF;
END $$;

COMMENT ON CONSTRAINT uq_genetics_site_name ON genetics.genetics
    IS 'Prevents duplicate genetics names within a site, fixes race condition in CreateGeneticsAsync';

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'uq_phenotypes_genetics_name'
          AND conrelid = 'genetics.phenotypes'::regclass
    ) THEN
        ALTER TABLE genetics.phenotypes
            ADD CONSTRAINT uq_phenotypes_genetics_name UNIQUE (site_id, genetics_id, name);
    END IF;
END $$;

COMMENT ON CONSTRAINT uq_phenotypes_genetics_name ON genetics.phenotypes
    IS 'Prevents duplicate phenotype names within a genetics, fixes race condition in CreatePhenotypeAsync';

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'uq_strains_site_name'
          AND conrelid = 'genetics.strains'::regclass
    ) THEN
        ALTER TABLE genetics.strains
            ADD CONSTRAINT uq_strains_site_name UNIQUE (site_id, name);
    END IF;
END $$;

COMMENT ON CONSTRAINT uq_strains_site_name ON genetics.strains
    IS 'Prevents duplicate strain names within a site, fixes race condition in CreateStrainAsync';

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'uq_batches_site_code'
          AND conrelid = 'genetics.batches'::regclass
    ) THEN
        ALTER TABLE genetics.batches
            ADD CONSTRAINT uq_batches_site_code UNIQUE (site_id, batch_code);
    END IF;
END $$;

COMMENT ON CONSTRAINT uq_batches_site_code ON genetics.batches
    IS 'Prevents duplicate batch codes within a site, fixes race condition in batch split/merge operations';
