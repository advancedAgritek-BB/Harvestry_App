-- =====================================================
-- FRP05: Telemetry Service - Logical Replication Setup
-- Migration: 006_create_replication_publication.sql
-- Description: Creates publication and replica identity required for WAL fan-out
-- =====================================================

-- Ensure replica identity includes all columns so updates emit full row data
ALTER TABLE sensor_readings
    REPLICA IDENTITY FULL;

-- Create publication consumed by the WAL fan-out worker (idempotent)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_publication WHERE pubname = 'telemetry_publication') THEN
        CREATE PUBLICATION telemetry_publication FOR TABLE sensor_readings;
    END IF;
END$$;
