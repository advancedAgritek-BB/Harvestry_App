-- =====================================================
-- FRP05: Telemetry Service - Seed Data
-- Migration: 005_seed_data.sql
-- Description: Test and development seed data
-- Author: AI Agent
-- Date: 2025-10-02
-- Dependencies: All previous migrations
-- WARNING: This is for development/testing only!
-- =====================================================

-- =====================================================
-- TEST SITE DATA
-- =====================================================

-- Insert test site (adjust UUID as needed)
DO $$
DECLARE
    test_site_id UUID := '11111111-1111-1111-1111-111111111111';
    test_equipment_id UUID := '22222222-2222-2222-2222-222222222222';
    test_user_id UUID := '33333333-3333-3333-3333-333333333333';
BEGIN
    RAISE NOTICE 'Creating test data...';
    RAISE NOTICE 'Test Site ID: %', test_site_id;
    RAISE NOTICE 'Test Equipment ID: %', test_equipment_id;
    
    -- =====================================================
    -- SENSOR STREAMS
    -- =====================================================
    
    -- Temperature sensor
    INSERT INTO sensor_streams (
        id, site_id, equipment_id, stream_type, unit, display_name, is_active
    ) VALUES (
        uuid_generate_v4(),
        test_site_id,
        test_equipment_id,
        'Temperature',
        'DegreesFahrenheit',
        'Room 1 - Temperature',
        true
    ) ON CONFLICT DO NOTHING;
    
    -- Humidity sensor
    INSERT INTO sensor_streams (
        id, site_id, equipment_id, stream_type, unit, display_name, is_active
    ) VALUES (
        uuid_generate_v4(),
        test_site_id,
        test_equipment_id,
        'Humidity',
        'Percent',
        'Room 1 - Humidity',
        true
    ) ON CONFLICT DO NOTHING;
    
    -- CO2 sensor
    INSERT INTO sensor_streams (
        id, site_id, equipment_id, stream_type, unit, display_name, is_active
    ) VALUES (
        uuid_generate_v4(),
        test_site_id,
        test_equipment_id,
        'Co2',
        'PartsPerMillion',
        'Room 1 - CO2',
        true
    ) ON CONFLICT DO NOTHING;
    
    -- VPD sensor
    INSERT INTO sensor_streams (
        id, site_id, equipment_id, stream_type, unit, display_name, is_active
    ) VALUES (
        uuid_generate_v4(),
        test_site_id,
        test_equipment_id,
        'Vpd',
        'Kilopascals',
        'Room 1 - VPD',
        true
    ) ON CONFLICT DO NOTHING;
    
    -- Light PAR sensor
    INSERT INTO sensor_streams (
        id, site_id, equipment_id, stream_type, unit, display_name, is_active
    ) VALUES (
        uuid_generate_v4(),
        test_site_id,
        test_equipment_id,
        'LightPar',
        'Micromoles',
        'Room 1 - PAR Light',
        true
    ) ON CONFLICT DO NOTHING;
    
    RAISE NOTICE 'Created % sensor streams', 5;
    
    -- =====================================================
    -- ALERT RULES
    -- =====================================================
    
    -- High temperature alert
    INSERT INTO alert_rules (
        id, site_id, rule_name, rule_type, stream_ids, threshold_config,
        evaluation_window_minutes, cooldown_minutes, severity,
        is_active, notify_channels, created_by, updated_by
    ) VALUES (
        uuid_generate_v4(),
        test_site_id,
        'High Temperature Alert',
        'Threshold',
        ARRAY(SELECT id FROM sensor_streams WHERE stream_type = 'Temperature' AND site_id = test_site_id LIMIT 1),
        '{"highThreshold": 85.0, "operator": "GreaterThan"}'::jsonb,
        5,
        15,
        'Warning',
        true,
        ARRAY['email', 'sms'],
        test_user_id,
        test_user_id
    ) ON CONFLICT DO NOTHING;
    
    -- Low humidity alert
    INSERT INTO alert_rules (
        id, site_id, rule_name, rule_type, stream_ids, threshold_config,
        evaluation_window_minutes, cooldown_minutes, severity,
        is_active, notify_channels, created_by, updated_by
    ) VALUES (
        uuid_generate_v4(),
        test_site_id,
        'Low Humidity Alert',
        'Threshold',
        ARRAY(SELECT id FROM sensor_streams WHERE stream_type = 'Humidity' AND site_id = test_site_id LIMIT 1),
        '{"lowThreshold": 40.0, "operator": "LessThan"}'::jsonb,
        10,
        30,
        'Warning',
        true,
        ARRAY['email'],
        test_user_id,
        test_user_id
    ) ON CONFLICT DO NOTHING;
    
    -- Critical high CO2 alert
    INSERT INTO alert_rules (
        id, site_id, rule_name, rule_type, stream_ids, threshold_config,
        evaluation_window_minutes, cooldown_minutes, severity,
        is_active, notify_channels, created_by, updated_by
    ) VALUES (
        uuid_generate_v4(),
        test_site_id,
        'Critical High CO2',
        'Threshold',
        ARRAY(SELECT id FROM sensor_streams WHERE stream_type = 'Co2' AND site_id = test_site_id LIMIT 1),
        '{"highThreshold": 2000.0, "operator": "GreaterThan"}'::jsonb,
        5,
        15,
        'Critical',
        true,
        ARRAY['email', 'sms', 'push'],
        test_user_id,
        test_user_id
    ) ON CONFLICT DO NOTHING;
    
    RAISE NOTICE 'Created % alert rules', 3;
    
    -- =====================================================
    -- SAMPLE SENSOR READINGS (Last 24 hours)
    -- =====================================================
    
    -- Generate sample readings for the last 24 hours
    -- Temperature: varies between 70-80°F
    INSERT INTO sensor_readings (time, stream_id, value, quality_code, ingestion_timestamp)
    SELECT
        NOW() - (interval '1 minute' * generate_series),
        (SELECT id FROM sensor_streams WHERE stream_type = 'Temperature' AND site_id = test_site_id LIMIT 1),
        70 + (10 * random()),  -- Random between 70-80
        0,  -- Good quality
        NOW() - (interval '1 minute' * generate_series)
    FROM generate_series(0, 1439, 1)  -- Last 24 hours, 1 reading per minute
    ON CONFLICT DO NOTHING;
    
    -- Humidity: varies between 50-70%
    INSERT INTO sensor_readings (time, stream_id, value, quality_code, ingestion_timestamp)
    SELECT
        NOW() - (interval '1 minute' * generate_series),
        (SELECT id FROM sensor_streams WHERE stream_type = 'Humidity' AND site_id = test_site_id LIMIT 1),
        50 + (20 * random()),  -- Random between 50-70
        0,
        NOW() - (interval '1 minute' * generate_series)
    FROM generate_series(0, 1439, 1)
    ON CONFLICT DO NOTHING;
    
    -- CO2: varies between 400-1200 PPM
    INSERT INTO sensor_readings (time, stream_id, value, quality_code, ingestion_timestamp)
    SELECT
        NOW() - (interval '1 minute' * generate_series),
        (SELECT id FROM sensor_streams WHERE stream_type = 'Co2' AND site_id = test_site_id LIMIT 1),
        400 + (800 * random()),  -- Random between 400-1200
        0,
        NOW() - (interval '1 minute' * generate_series)
    FROM generate_series(0, 1439, 1)
    ON CONFLICT DO NOTHING;
    
    RAISE NOTICE 'Created ~4,320 sample sensor readings (1440 readings × 3 sensors)';
    
    -- =====================================================
    -- REFRESH CONTINUOUS AGGREGATES
    -- =====================================================
    
    -- Manually refresh continuous aggregates for seed data
    CALL refresh_continuous_aggregate('sensor_readings_1min', NULL, NULL);
    CALL refresh_continuous_aggregate('sensor_readings_5min', NULL, NULL);
    CALL refresh_continuous_aggregate('sensor_readings_1hour', NULL, NULL);
    CALL refresh_continuous_aggregate('sensor_readings_1day', NULL, NULL);
    
    RAISE NOTICE 'Refreshed all continuous aggregates';
END $$;

-- =====================================================
-- DATA VERIFICATION
-- =====================================================

DO $$
DECLARE
    stream_count INT;
    reading_count BIGINT;
    rule_count INT;
BEGIN
    SELECT COUNT(*) INTO stream_count FROM sensor_streams;
    SELECT COUNT(*) INTO reading_count FROM sensor_readings;
    SELECT COUNT(*) INTO rule_count FROM alert_rules;
    
    RAISE NOTICE '';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'SEED DATA SUMMARY:';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'Sensor Streams: %', stream_count;
    RAISE NOTICE 'Sensor Readings: %', reading_count;
    RAISE NOTICE 'Alert Rules: %', rule_count;
    RAISE NOTICE '';
    
    -- Show sample aggregated data
    RAISE NOTICE 'Sample 1-hour aggregates (last 5):';
    FOR i IN 1..5 LOOP
        RAISE NOTICE '  %', (
            SELECT FORMAT('%s | %s | avg: %.2f, min: %.2f, max: %.2f',
                TO_CHAR(bucket, 'YYYY-MM-DD HH24:MI'),
                (SELECT display_name FROM sensor_streams WHERE id = stream_id LIMIT 1),
                avg_value, min_value, max_value
            )
            FROM sensor_readings_1hour
            ORDER BY bucket DESC
            LIMIT 1 OFFSET (i-1)
        );
    END LOOP;
    
    RAISE NOTICE '========================================';
END $$;

-- =====================================================
-- HELPER: Clear seed data (for testing)
-- =====================================================

CREATE OR REPLACE FUNCTION clear_seed_data()
RETURNS void AS $$
DECLARE
    test_site_id UUID := '11111111-1111-1111-1111-111111111111';
BEGIN
    DELETE FROM ingestion_errors WHERE site_id = test_site_id;
    DELETE FROM ingestion_sessions WHERE site_id = test_site_id;
    DELETE FROM alert_instances WHERE site_id = test_site_id;
    DELETE FROM alert_rules WHERE site_id = test_site_id;
    DELETE FROM sensor_readings WHERE stream_id IN (
        SELECT id FROM sensor_streams WHERE site_id = test_site_id
    );
    DELETE FROM sensor_streams WHERE site_id = test_site_id;
    
    RAISE NOTICE 'Seed data cleared for test site';
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION clear_seed_data() IS 'Clear all seed data for the test site (development only)';

-- =====================================================
-- SUCCESS MESSAGE
-- =====================================================

DO $$
BEGIN
    RAISE NOTICE '';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'Migration 005_seed_data.sql completed successfully';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'Test data created:';
    RAISE NOTICE '  ✓ Test site and equipment';
    RAISE NOTICE '  ✓ 5 sensor streams (Temp, Humidity, CO2, VPD, PAR)';
    RAISE NOTICE '  ✓ 3 alert rules';
    RAISE NOTICE '  ✓ ~4,320 sensor readings (24h history)';
    RAISE NOTICE '  ✓ Continuous aggregates refreshed';
    RAISE NOTICE '';
    RAISE NOTICE 'Use clear_seed_data() to remove test data';
    RAISE NOTICE '========================================';
END $$;

