-- ============================================================================
-- Knowledge: Seed Crop Steering Reference Data
-- Migration: Populate Reference Tables with Quick Reference Values
-- ----------------------------------------------------------------------------
-- Seeds the steering_lever_references and irrigation_signal_references tables
-- with canonical values from the Crop Steering & Irrigation Quick Reference.
-- 
-- Reference: "Crop Steering & Irrigation Quick Reference (Combined)"
-- - Vegetative vs Generative steering levers
-- - Phases (P1/P2/P3) - Shot sizing math - Dryback/EC adjustment tactics
-- ============================================================================

-- ---------------------------------------------------------------------------
-- 1) Seed Steering Levers
-- ---------------------------------------------------------------------------
-- Based on "Steering levers at a glance" from quick reference
-- These are "dials" to push plants toward vegetative (leaf/stem) or generative (flower/fruit)

INSERT INTO knowledge.steering_lever_references (
    metric_name, display_name, description,
    vegetative_trend, generative_trend,
    vegetative_min_value, vegetative_max_value,
    generative_min_value, generative_max_value,
    unit, stream_type_id
) VALUES
-- Substrate EC: Lower for veg, Higher for gen
(
    'SubstrateEC', 
    'Substrate EC', 
    'Electrical conductivity of the root zone substrate. Lower EC promotes vegetative growth; higher EC stresses plants toward generative production.',
    'Lower', 'Higher',
    1.5, 2.5,   -- Vegetative range (mS/cm)
    2.5, 4.0,   -- Generative range (mS/cm)
    'mS/cm',
    22          -- StreamType.SoilEc
),
-- VWC (Volumetric Water Content): Higher for veg, Lower for gen
-- Note: "Water content (WC%)" in reference = VWC in our system
(
    'VWC', 
    'Volumetric Water Content (VWC)', 
    'Percentage of water in the substrate volume. Higher VWC promotes vegetative growth; lower VWC (controlled dryback) promotes generative.',
    'Higher', 'Lower',
    55.0, 70.0, -- Vegetative range (%)
    40.0, 55.0, -- Generative range (%)
    '%',
    20          -- StreamType.SoilMoisture
),
-- VPD: Lower for veg, Higher for gen
(
    'VPD', 
    'Vapor Pressure Deficit', 
    'Difference between saturated and actual vapor pressure. Lower VPD reduces plant stress (vegetative); higher VPD increases transpiration (generative).',
    'Lower', 'Higher',
    0.8, 1.0,   -- Vegetative range (kPa)
    1.2, 1.5,   -- Generative range (kPa)
    'kPa',
    4           -- StreamType.Vpd
),
-- Temperature: Higher for veg, Lower for gen
(
    'Temperature', 
    'Air Temperature', 
    'Ambient temperature in the grow space. Higher temps promote vegetative growth; cooler temps promote generative.',
    'Higher', 'Lower',
    78.0, 84.0, -- Vegetative range (°F)
    72.0, 78.0, -- Generative range (°F)
    '°F',
    1           -- StreamType.Temperature
),
-- Irrigation Frequency: More frequent for veg, Less frequent for gen
(
    'IrrigationFrequency', 
    'Irrigation Frequency', 
    'How often irrigation events occur. More frequent watering promotes vegetative; less frequent promotes generative through dryback stress.',
    'MoreFrequent', 'LessFrequent',
    NULL, NULL, -- Qualitative - no fixed numeric range
    NULL, NULL,
    'events/day',
    NULL        -- No direct stream type
),
-- Feed Duration: Longer for veg, Shorter for gen
(
    'FeedDuration', 
    'Feed Duration', 
    'Total time irrigating per cycle. Longer durations keep substrate wetter (vegetative); shorter durations allow more dryback (generative).',
    'Longer', 'Shorter',
    NULL, NULL,
    NULL, NULL,
    'minutes',
    NULL
)
ON CONFLICT (metric_name) DO UPDATE SET
    display_name = EXCLUDED.display_name,
    description = EXCLUDED.description,
    vegetative_trend = EXCLUDED.vegetative_trend,
    generative_trend = EXCLUDED.generative_trend,
    vegetative_min_value = EXCLUDED.vegetative_min_value,
    vegetative_max_value = EXCLUDED.vegetative_max_value,
    generative_min_value = EXCLUDED.generative_min_value,
    generative_max_value = EXCLUDED.generative_max_value,
    unit = EXCLUDED.unit,
    stream_type_id = EXCLUDED.stream_type_id;

-- ---------------------------------------------------------------------------
-- 2) Seed Irrigation Signals
-- ---------------------------------------------------------------------------
-- Based on "Typical irrigation signals" from quick reference
-- Phase-specific values for shot sizing and dryback targets

INSERT INTO knowledge.irrigation_signal_references (
    signal_name, display_name, description,
    vegetative_value, generative_value,
    applicable_phase,
    vegetative_min_value, vegetative_max_value,
    generative_min_value, generative_max_value,
    unit
) VALUES
-- Shot size (as % of substrate volume)
(
    'ShotSize', 
    'Shot Size', 
    'Volume of water per irrigation shot as a percentage of total substrate volume.',
    '2-4%', '4-10%',
    'All',
    2.0, 4.0,   -- Vegetative: 2-4% of substrate volume
    4.0, 10.0,  -- Generative: 4-10% of substrate volume
    '% substrate volume'
),
-- Irrigation interval during lights-on
(
    'IrrigationInterval', 
    'Irrigation Interval', 
    'Time between irrigation events during the lights-on period.',
    'Every 15-40 min', 'Every 40 min-2 hr',
    'All',
    15.0, 40.0,   -- Vegetative: every 15-40 minutes
    40.0, 120.0,  -- Generative: every 40 minutes to 2 hours
    'minutes'
),
-- Daily dryback (max → min VWC)
-- P3 phase specific
(
    'DailyDryback', 
    'Daily Dryback', 
    'Total VWC reduction from daily max to min. Example framing: ~25% of max field capacity (veg) vs ~50% (gen).',
    '10-20%', '25-50%',
    'P3',
    10.0, 20.0,   -- Vegetative: 10-20% VWC drop
    25.0, 50.0,   -- Generative: 25-50% VWC drop
    '% VWC change'
),
-- Intershot dryback (drop between events)
-- P1 and P2 phase specific
(
    'IntershotDryback', 
    'Intershot Dryback', 
    'VWC drop allowed between irrigation shots before triggering next shot.',
    '1-4%', '4-6%',
    'P1,P2',
    1.0, 4.0,     -- Vegetative: 1-4% VWC drop between shots
    4.0, 6.0,     -- Generative: 4-6% VWC drop between shots
    '% VWC'
)
ON CONFLICT (signal_name, applicable_phase) DO UPDATE SET
    display_name = EXCLUDED.display_name,
    description = EXCLUDED.description,
    vegetative_value = EXCLUDED.vegetative_value,
    generative_value = EXCLUDED.generative_value,
    vegetative_min_value = EXCLUDED.vegetative_min_value,
    vegetative_max_value = EXCLUDED.vegetative_max_value,
    generative_min_value = EXCLUDED.generative_min_value,
    generative_max_value = EXCLUDED.generative_max_value,
    unit = EXCLUDED.unit;

-- ---------------------------------------------------------------------------
-- 3) Add Comments for Documentation
-- ---------------------------------------------------------------------------
COMMENT ON TABLE knowledge.steering_lever_references IS 
'Reference steering levers from Crop Steering Quick Reference. 
Levers are environmental/irrigation parameters that shift plants between vegetative (leaf/stem) and generative (flower/fruit) growth.';

COMMENT ON TABLE knowledge.irrigation_signal_references IS 
'Reference irrigation signals from Crop Steering Quick Reference.
Signals define phase-specific (P1/P2/P3) targets for shot sizing, intervals, and dryback.
P1 = Ramp (post lights-on saturation)
P2 = Maintenance (sustain VWC during photosynthesis)
P3 = Dryback (controlled drying before lights-off)';

