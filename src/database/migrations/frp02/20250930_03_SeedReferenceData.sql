-- =====================================================================
-- FRP-02: Reference Data & Built-in Templates
-- File: 20250930_03_SeedReferenceData.sql
-- Description: Seed data for fault codes and built-in equipment templates
--              (HSES12, HSEA24, HydroCore, RoomHub, EdgePods)
-- =====================================================================

-- =====================================================================
-- 1) Fault Reason Codes (Standardized)
-- =====================================================================

INSERT INTO fault_reason_codes (code, category, description, severity, recommended_action) VALUES
    -- Network faults
    ('NET_001', 'Network', 'Device offline - no heartbeat received', 'High', 'Check network connection, verify power supply, inspect MQTT broker connectivity'),
    ('NET_002', 'Network', 'MQTT connection lost', 'High', 'Verify MQTT broker status, check network stability, review device logs'),
    ('NET_003', 'Network', 'IP address conflict detected', 'Medium', 'Assign static IP or resolve DHCP conflict'),
    ('NET_004', 'Network', 'Weak signal strength', 'Low', 'Relocate device or improve network infrastructure'),
    ('NET_005', 'Network', 'Firmware update failed', 'Medium', 'Retry firmware update, verify image integrity'),
    
    -- Sensor faults
    ('SNS_001', 'Sensor', 'Sensor reading out of valid range', 'High', 'Recalibrate sensor or replace if faulty'),
    ('SNS_002', 'Sensor', 'Sensor calibration overdue', 'Medium', 'Schedule calibration maintenance'),
    ('SNS_003', 'Sensor', 'Sensor probe disconnected', 'Critical', 'Check probe connection, inspect cable integrity'),
    ('SNS_004', 'Sensor', 'Sensor drift detected', 'High', 'Recalibrate sensor immediately'),
    ('SNS_005', 'Sensor', 'Sensor value frozen/stale', 'High', 'Check sensor power and connection'),
    ('SNS_006', 'Sensor', 'Temperature sensor fault', 'High', 'Verify thermistor connection, check for shorts'),
    ('SNS_007', 'Sensor', 'Humidity sensor fault', 'Medium', 'Clean sensor, check for water damage'),
    ('SNS_008', 'Sensor', 'EC/pH probe fault', 'High', 'Clean probe, verify electrolyte levels, recalibrate'),
    ('SNS_009', 'Sensor', 'CO2 sensor fault', 'Medium', 'Clean optical sensor, verify gas chamber'),
    ('SNS_010', 'Sensor', 'VWC sensor fault', 'High', 'Check probe insertion depth, verify soil contact'),
    
    -- Actuator faults
    ('ACT_001', 'Actuator', 'Valve stuck open', 'Critical', 'Emergency shut-off, inspect valve mechanism, check for debris'),
    ('ACT_002', 'Actuator', 'Valve stuck closed', 'High', 'Inspect valve solenoid, check power supply'),
    ('ACT_003', 'Actuator', 'Pump failure', 'Critical', 'Check pump power, inspect impeller, verify no blockage'),
    ('ACT_004', 'Actuator', 'Pump over-current detected', 'Critical', 'Immediate shutdown, inspect for blockage or mechanical failure'),
    ('ACT_005', 'Actuator', 'Relay failure', 'High', 'Replace relay, inspect contacts for wear'),
    ('ACT_006', 'Actuator', 'Injector channel fault', 'High', 'Check injector pump, verify tubing integrity'),
    ('ACT_007', 'Actuator', 'Dosing pump calibration required', 'Medium', 'Run calibration routine, verify dose accuracy'),
    
    -- Safety faults
    ('SAF_001', 'Safety', 'E-STOP activated', 'Critical', 'Identify safety issue, address root cause, manually reset after clearance'),
    ('SAF_002', 'Safety', 'Door open interlock triggered', 'Critical', 'Close door, verify interlock switch operation'),
    ('SAF_003', 'Safety', 'High temperature alarm', 'Critical', 'Check cooling system, verify HVAC operation'),
    ('SAF_004', 'Safety', 'Low temperature alarm', 'Critical', 'Check heating system, verify environmental controls'),
    ('SAF_005', 'Safety', 'Tank level critically low', 'Critical', 'Refill tank immediately, check for leaks'),
    ('SAF_006', 'Safety', 'EC/pH out of bounds - run aborted', 'High', 'Verify mix tank recipe, recalibrate dosing'),
    ('SAF_007', 'Safety', 'CO2 exhaust lockout active', 'High', 'Wait for exhaust cycle to complete, verify ventilation system'),
    ('SAF_008', 'Safety', 'Leak detected', 'Critical', 'Locate and repair leak, inspect floor sensors'),
    ('SAF_009', 'Safety', 'Fire/smoke alarm', 'Critical', 'Evacuate, follow emergency procedures, contact authorities'),
    ('SAF_010', 'Safety', 'Over-pressure detected', 'Critical', 'Emergency vent, inspect pressure relief valves'),
    
    -- Controller faults
    ('CTL_001', 'Controller', 'Controller boot failure', 'Critical', 'Power cycle device, check SD card integrity, reflash firmware'),
    ('CTL_002', 'Controller', 'Watchdog timer reset', 'Medium', 'Investigate last operation, check for firmware bugs'),
    ('CTL_003', 'Controller', 'Memory corruption detected', 'High', 'Reflash firmware, replace controller if persistent'),
    ('CTL_004', 'Controller', 'RTC battery low', 'Low', 'Replace backup battery, verify time accuracy'),
    ('CTL_005', 'Controller', 'Storage full', 'Medium', 'Clear old logs, expand storage capacity'),
    ('CTL_006', 'Controller', 'Certificate expired', 'High', 'Renew device certificate for secure communication'),
    
    -- Power faults
    ('PWR_001', 'Power', 'Power supply voltage out of range', 'High', 'Check power supply, verify AC input'),
    ('PWR_002', 'Power', 'Battery backup low', 'Medium', 'Replace batteries, verify charging system'),
    ('PWR_003', 'Power', 'PoE power insufficient', 'High', 'Use PoE+ injector or switch to AC power'),
    ('PWR_004', 'Power', 'Power brownout detected', 'Medium', 'Install UPS, check building electrical system'),
    ('PWR_005', 'Power', 'Generator switchover', 'Low', 'Normal operation, monitor generator runtime');

-- =====================================================================
-- 2) Built-in Equipment Type Templates
-- Note: These will be inserted with a system user ID
-- In production, use a service account UUID
-- =====================================================================

-- System user for template creation (placeholder - replace with actual system user)
-- For now, we'll use a well-known UUID that can be updated later
DO $$
DECLARE
    system_user_id UUID := '00000000-0000-0000-0000-000000000001';  -- Placeholder
    harvestry_org_id UUID;
BEGIN
    -- Get or create Harvestry system org for templates
    INSERT INTO organizations (organization_id, name, slug, created_by, updated_by)
    VALUES (
        '00000000-0000-0000-0000-000000000001',
        'Harvestry System',
        'harvestry-system',
        system_user_id,
        system_user_id
    )
    ON CONFLICT (organization_id) DO NOTHING;
    
    harvestry_org_id := '00000000-0000-0000-0000-000000000001';
    
    -- ================================================================
    -- HSES12: 12-Channel EC Sensor (Substrate Sensing)
    -- ================================================================
    INSERT INTO equipment_type_registry (
        org_id,
        type_code,
        core_enum,
        display_name,
        description,
        is_template,
        template_name,
        schema_json,
        created_by_user_id
    ) VALUES (
        harvestry_org_id,
        'HSES12',
        'sensor',
        'HSES12 - 12-Channel EC Sensor',
        'Substrate EC sensing for 12 zones with individual channel calibration',
        true,
        'HSES12',
        jsonb_build_object(
            'channels', 12,
            'measurements', jsonb_build_array('ec', 'temperature'),
            'range_ec_ms_cm', jsonb_build_object('min', 0, 'max', 5),
            'accuracy_pct', 2,
            'calibration_method', 'TwoPoint'
        ),
        system_user_id
    );
    
    -- ================================================================
    -- HSEA24: 24-Channel EC Sensor (Substrate Sensing)
    -- ================================================================
    INSERT INTO equipment_type_registry (
        org_id,
        type_code,
        core_enum,
        display_name,
        description,
        is_template,
        template_name,
        schema_json,
        created_by_user_id
    ) VALUES (
        harvestry_org_id,
        'HSEA24',
        'sensor',
        'HSEA24 - 24-Channel EC Sensor',
        'High-density substrate EC sensing for 24 zones',
        true,
        'HSEA24',
        jsonb_build_object(
            'channels', 24,
            'measurements', jsonb_build_array('ec', 'vwc', 'temperature'),
            'range_ec_ms_cm', jsonb_build_object('min', 0, 'max', 5),
            'range_vwc_pct', jsonb_build_object('min', 0, 'max', 100),
            'accuracy_pct', 2,
            'calibration_method', 'TwoPoint'
        ),
        system_user_id
    );
    
    -- ================================================================
    -- HydroCore v2: Irrigation + Dosing Controller
    -- ================================================================
    INSERT INTO equipment_type_registry (
        org_id,
        type_code,
        core_enum,
        display_name,
        description,
        is_template,
        template_name,
        schema_json,
        created_by_user_id
    ) VALUES (
        harvestry_org_id,
        'HydroCore_v2',
        'controller',
        'HydroCore v2 - Irrigation Controller',
        'Irrigation + dosing controller with SkidLink and AUX 24V',
        true,
        'HydroCore',
        jsonb_build_object(
            'channels', jsonb_build_object(
                'valves', 5,
                'aux_24v', 1,
                'skidlink', 1
            ),
            'features', jsonb_build_array('irrigation', 'dosing', 'interlocks'),
            'voltage', '24VDC',
            'max_current_amp', 10,
            'protocol', 'MQTT'
        ),
        system_user_id
    );
    
    -- ================================================================
    -- RoomHub v2: In-Room Sensing + Lighting Controller
    -- ================================================================
    INSERT INTO equipment_type_registry (
        org_id,
        type_code,
        core_enum,
        display_name,
        description,
        is_template,
        template_name,
        schema_json,
        created_by_user_id
    ) VALUES (
        harvestry_org_id,
        'RoomHub_v2',
        'controller',
        'RoomHub v2 - Environment Controller',
        'In-room sensing (temp/RH/VWC/EC) + lighting control (AO + relay)',
        true,
        'RoomHub',
        jsonb_build_object(
            'sensors', jsonb_build_object(
                'temperature', 8,
                'humidity', 8,
                'vwc_ec', true,
                'co2', true,
                'ppfd', true
            ),
            'outputs', jsonb_build_object(
                'analog_0_10v', 4,
                'relay', 4
            ),
            'voltage', '24VDC',
            'protocol', 'MQTT'
        ),
        system_user_id
    );
    
    -- ================================================================
    -- EdgePods-IP: Network-Assigned Expansion Pods (PoE)
    -- ================================================================
    INSERT INTO equipment_type_registry (
        org_id,
        type_code,
        core_enum,
        display_name,
        description,
        is_template,
        template_name,
        schema_json,
        created_by_user_id
    ) VALUES (
        harvestry_org_id,
        'EdgePod_IP',
        'controller',
        'EdgePod-IP - Network Expansion Pod',
        'PoE-powered expansion pod with SD4x6, DI8, DO8, Pod-AO4, Pod-VAL8',
        true,
        'EdgePod-IP',
        jsonb_build_object(
            'modules', jsonb_build_array(
                'SD4x6 (4-channel sensor, 6-layer PCB)',
                'DI8 (8 digital inputs)',
                'DO8 (8 digital outputs)',
                'Pod-AO4 (4 analog outputs)',
                'Pod-VAL8 (8 valve outputs)'
            ),
            'power', 'PoE (802.3af/at)',
            'placement', 'Near signals - offline micro-programs',
            'protocol', 'MQTT'
        ),
        system_user_id
    );
    
    -- ================================================================
    -- Generic Equipment Types (not templates)
    -- ================================================================
    
    -- Temperature Sensor (Generic)
    INSERT INTO equipment_type_registry (
        org_id,
        type_code,
        core_enum,
        display_name,
        description,
        is_template,
        created_by_user_id
    ) VALUES (
        harvestry_org_id,
        'temp_sensor_generic',
        'sensor',
        'Temperature Sensor (Generic)',
        'Generic temperature sensor (NTC, PT100, thermocouple)',
        false,
        system_user_id
    );
    
    -- Humidity Sensor (Generic)
    INSERT INTO equipment_type_registry (
        org_id,
        type_code,
        core_enum,
        display_name,
        description,
        is_template,
        created_by_user_id
    ) VALUES (
        harvestry_org_id,
        'humidity_sensor_generic',
        'sensor',
        'Humidity Sensor (Generic)',
        'Generic relative humidity sensor',
        false,
        system_user_id
    );
    
    -- CO2 Sensor (Generic)
    INSERT INTO equipment_type_registry (
        org_id,
        type_code,
        core_enum,
        display_name,
        description,
        is_template,
        created_by_user_id
    ) VALUES (
        harvestry_org_id,
        'co2_sensor_generic',
        'sensor',
        'CO2 Sensor (Generic)',
        'Generic CO2 sensor (NDIR)',
        false,
        system_user_id
    );
    
    -- Solenoid Valve (Generic)
    INSERT INTO equipment_type_registry (
        org_id,
        type_code,
        core_enum,
        display_name,
        description,
        is_template,
        created_by_user_id
    ) VALUES (
        harvestry_org_id,
        'valve_solenoid_generic',
        'valve',
        'Solenoid Valve (Generic)',
        'Generic solenoid valve (24VAC/VDC)',
        false,
        system_user_id
    );
    
    -- Dosing Pump (Generic)
    INSERT INTO equipment_type_registry (
        org_id,
        type_code,
        core_enum,
        display_name,
        description,
        is_template,
        created_by_user_id
    ) VALUES (
        harvestry_org_id,
        'pump_dosing_generic',
        'pump',
        'Dosing Pump (Generic)',
        'Generic peristaltic dosing pump',
        false,
        system_user_id
    );
    
END $$;

-- =====================================================================
-- 3) Indexes for Performance
-- =====================================================================

-- Add GIN index for equipment device_twin_json searches
CREATE INDEX idx_equipment_device_twin_gin ON equipment USING GIN (device_twin_json);

-- Add GIN index for equipment_type_registry schema_json searches
CREATE INDEX idx_equipment_type_schema_gin ON equipment_type_registry USING GIN (schema_json);

-- =====================================================================
-- End of Reference Data Migration
-- =====================================================================
