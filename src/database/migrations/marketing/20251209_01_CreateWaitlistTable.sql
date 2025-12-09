-- Migration: Create Waitlist Entries Table
-- Purpose: Store early access signups from the landing page
-- Date: 2025-12-09

-- Create schema for marketing if it doesn't exist
CREATE SCHEMA IF NOT EXISTS marketing;

-- Waitlist entries table
CREATE TABLE IF NOT EXISTS marketing.waitlist_entries (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) NOT NULL,
    name VARCHAR(255),
    company VARCHAR(255),
    facility_size VARCHAR(50),
    source VARCHAR(100) DEFAULT 'landing_page',
    status VARCHAR(50) DEFAULT 'pending',
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    contacted_at TIMESTAMPTZ,
    converted_at TIMESTAMPTZ,
    
    -- Ensure email uniqueness (case-insensitive)
    CONSTRAINT waitlist_entries_email_unique UNIQUE (email)
);

-- Create index for email lookups
CREATE INDEX IF NOT EXISTS idx_waitlist_entries_email ON marketing.waitlist_entries (LOWER(email));

-- Create index for status filtering
CREATE INDEX IF NOT EXISTS idx_waitlist_entries_status ON marketing.waitlist_entries (status);

-- Create index for created_at for chronological queries
CREATE INDEX IF NOT EXISTS idx_waitlist_entries_created_at ON marketing.waitlist_entries (created_at DESC);

-- Add trigger to auto-update updated_at
CREATE OR REPLACE FUNCTION marketing.update_waitlist_timestamp()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_waitlist_entries_updated_at ON marketing.waitlist_entries;
CREATE TRIGGER trg_waitlist_entries_updated_at
    BEFORE UPDATE ON marketing.waitlist_entries
    FOR EACH ROW
    EXECUTE FUNCTION marketing.update_waitlist_timestamp();

-- Comment on table
COMMENT ON TABLE marketing.waitlist_entries IS 'Stores early access waitlist signups from the landing page';
COMMENT ON COLUMN marketing.waitlist_entries.facility_size IS 'Options: under_5k, 5k_15k, 15k_50k, 50k_plus';
COMMENT ON COLUMN marketing.waitlist_entries.source IS 'Where the signup came from: landing_page, waitlist_page, demo_form, etc.';
COMMENT ON COLUMN marketing.waitlist_entries.status IS 'pending, contacted, converted, unsubscribed';

-- Grant permissions (adjust based on your Supabase setup)
-- For Supabase, the table needs to be in 'public' schema or have proper RLS policies

-- Alternative: Create in public schema for Supabase compatibility
CREATE TABLE IF NOT EXISTS public.waitlist_entries (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) NOT NULL,
    name VARCHAR(255),
    company VARCHAR(255),
    facility_size VARCHAR(50),
    source VARCHAR(100) DEFAULT 'landing_page',
    status VARCHAR(50) DEFAULT 'pending',
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    contacted_at TIMESTAMPTZ,
    converted_at TIMESTAMPTZ,
    
    CONSTRAINT public_waitlist_entries_email_unique UNIQUE (email)
);

-- Indexes for public schema version
CREATE INDEX IF NOT EXISTS idx_public_waitlist_email ON public.waitlist_entries (LOWER(email));
CREATE INDEX IF NOT EXISTS idx_public_waitlist_status ON public.waitlist_entries (status);
CREATE INDEX IF NOT EXISTS idx_public_waitlist_created ON public.waitlist_entries (created_at DESC);

-- Enable RLS on public table
ALTER TABLE public.waitlist_entries ENABLE ROW LEVEL SECURITY;

-- Policy: Only service role can insert (API calls)
CREATE POLICY "Service role can insert waitlist entries" ON public.waitlist_entries
    FOR INSERT
    TO service_role
    WITH CHECK (true);

-- Policy: Service role can read all entries
CREATE POLICY "Service role can read waitlist entries" ON public.waitlist_entries
    FOR SELECT
    TO service_role
    USING (true);

-- Policy: Service role can update entries
CREATE POLICY "Service role can update waitlist entries" ON public.waitlist_entries
    FOR UPDATE
    TO service_role
    USING (true);

COMMENT ON TABLE public.waitlist_entries IS 'Stores early access waitlist signups - Supabase compatible';

