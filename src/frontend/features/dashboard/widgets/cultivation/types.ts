/**
 * Sensor types for Environmental Metrics Widget
 * 
 * Supports different sensor placements:
 * - room: Standard room/zone climate sensor (shows current reading)
 * - runoff: Measures runoff/leachate (shows current reading with "runoff" indicator)
 * - batch_tank: Measures from a batch tank (shows reading with tank name)
 * - inline: Inline sensor during irrigation (shows last event reading)
 */

export type SensorPlacement = 'room' | 'runoff' | 'batch_tank' | 'inline';

export type SensorMetricType = 
  | 'Temperature' 
  | 'Humidity' 
  | 'VPD' 
  | 'CO2' 
  | 'PPFD' 
  | 'DLI'
  | 'pH' 
  | 'EC' 
  | 'VWC';

export interface MetricSensor {
  id: string;
  name: string;
  placement: SensorPlacement;
  /** Associated batch tank name (for batch_tank type) */
  batchTankName?: string;
  /** Zone/location of the sensor */
  zone: string;
  /** Supported metrics this sensor can read */
  metrics: SensorMetricType[];
  /** Current reading value */
  currentValue?: number;
  /** Timestamp of current reading */
  lastReadAt?: string;
  /** For inline sensors: last value when irrigation ended */
  lastEventValue?: number;
  /** For inline sensors: when the last irrigation event ended */
  lastEventEndedAt?: string;
  /** Whether sensor is online */
  isOnline: boolean;
}

export interface SensorAssignment {
  /** Selected sensor IDs */
  sensorIds: string[];
  /** How to aggregate multiple sensor readings */
  aggregation: 'average' | 'min' | 'max';
}

export interface MetricReading {
  value: number | null;
  label: string;
  timestamp?: string;
  isLive: boolean;
}

/**
 * Get display label based on sensor placement
 */
export function getSensorPlacementLabel(placement: SensorPlacement): string {
  switch (placement) {
    case 'room':
      return 'Room';
    case 'runoff':
      return 'Runoff';
    case 'batch_tank':
      return 'Batch Tank';
    case 'inline':
      return 'Last Event';
    default:
      return '';
  }
}

/**
 * Get display info for a sensor reading based on its placement
 */
export function getReadingDisplayInfo(sensor: MetricSensor, metric: SensorMetricType): MetricReading {
  if (sensor.placement === 'inline') {
    return {
      value: sensor.lastEventValue ?? sensor.currentValue ?? null,
      label: 'last event',
      timestamp: sensor.lastEventEndedAt,
      isLive: false,
    };
  }
  
  if (sensor.placement === 'batch_tank') {
    return {
      value: sensor.currentValue ?? null,
      label: sensor.batchTankName || 'batch tank',
      timestamp: sensor.lastReadAt,
      isLive: sensor.isOnline,
    };
  }
  
  if (sensor.placement === 'runoff') {
    return {
      value: sensor.currentValue ?? null,
      label: 'runoff',
      timestamp: sensor.lastReadAt,
      isLive: sensor.isOnline,
    };
  }

  // room sensor
  return {
    value: sensor.currentValue ?? null,
    label: sensor.zone,
    timestamp: sensor.lastReadAt,
    isLive: sensor.isOnline,
  };
}

/**
 * Mock sensors with placement categories - In production, fetched from API
 */
export const METRIC_SENSORS: MetricSensor[] = [
  // ============================================
  // CLIMATE SENSORS (Temperature, Humidity, VPD, CO2)
  // ============================================
  
  // Flower Room 1 Climate
  {
    id: 'climate-f1-a',
    name: 'F1 Climate A',
    placement: 'room',
    zone: 'Flower Room 1',
    metrics: ['Temperature', 'Humidity', 'VPD', 'CO2'],
    currentValue: 75.4,
    lastReadAt: new Date().toISOString(),
    isOnline: true,
  },
  {
    id: 'climate-f1-b',
    name: 'F1 Climate B',
    placement: 'room',
    zone: 'Flower Room 1',
    metrics: ['Temperature', 'Humidity', 'VPD', 'CO2'],
    currentValue: 76.1,
    lastReadAt: new Date().toISOString(),
    isOnline: true,
  },
  // Flower Room 2 Climate
  {
    id: 'climate-f2-a',
    name: 'F2 Climate A',
    placement: 'room',
    zone: 'Flower Room 2',
    metrics: ['Temperature', 'Humidity', 'VPD', 'CO2'],
    currentValue: 74.8,
    lastReadAt: new Date().toISOString(),
    isOnline: true,
  },
  {
    id: 'climate-f2-b',
    name: 'F2 Climate B',
    placement: 'room',
    zone: 'Flower Room 2',
    metrics: ['Temperature', 'Humidity', 'VPD', 'CO2'],
    currentValue: 75.2,
    lastReadAt: new Date().toISOString(),
    isOnline: false,
  },
  // Veg Room Climate
  {
    id: 'climate-v1-a',
    name: 'V1 Climate A',
    placement: 'room',
    zone: 'Veg Room 1',
    metrics: ['Temperature', 'Humidity', 'VPD', 'CO2'],
    currentValue: 78.2,
    lastReadAt: new Date().toISOString(),
    isOnline: true,
  },

  // ============================================
  // LIGHT SENSORS (PPFD, DLI)
  // ============================================
  {
    id: 'light-f1-a',
    name: 'F1 PAR Sensor A',
    placement: 'room',
    zone: 'Flower Room 1',
    metrics: ['PPFD', 'DLI'],
    currentValue: 950,
    lastReadAt: new Date().toISOString(),
    isOnline: true,
  },
  {
    id: 'light-f1-b',
    name: 'F1 PAR Sensor B',
    placement: 'room',
    zone: 'Flower Room 1',
    metrics: ['PPFD', 'DLI'],
    currentValue: 920,
    lastReadAt: new Date().toISOString(),
    isOnline: true,
  },
  {
    id: 'light-f2-a',
    name: 'F2 PAR Sensor A',
    placement: 'room',
    zone: 'Flower Room 2',
    metrics: ['PPFD', 'DLI'],
    currentValue: 980,
    lastReadAt: new Date().toISOString(),
    isOnline: true,
  },
  {
    id: 'light-v1-a',
    name: 'V1 PAR Sensor',
    placement: 'room',
    zone: 'Veg Room 1',
    metrics: ['PPFD', 'DLI'],
    currentValue: 650,
    lastReadAt: new Date().toISOString(),
    isOnline: true,
  },

  // ============================================
  // pH/EC SENSORS
  // ============================================
  
  // Runoff sensors
  {
    id: 'ph-runoff-f1',
    name: 'F1 Runoff pH/EC',
    placement: 'runoff',
    zone: 'Flower Room 1',
    metrics: ['pH', 'EC'],
    currentValue: 5.8,
    lastReadAt: new Date().toISOString(),
    isOnline: true,
  },
  {
    id: 'ph-runoff-f2',
    name: 'F2 Runoff pH/EC',
    placement: 'runoff',
    zone: 'Flower Room 2',
    metrics: ['pH', 'EC'],
    currentValue: 6.1,
    lastReadAt: new Date().toISOString(),
    isOnline: true,
  },
  
  // Batch tank sensors
  {
    id: 'ph-tank-a',
    name: 'Tank A pH/EC',
    placement: 'batch_tank',
    batchTankName: 'Veg Mix Tank',
    zone: 'Mix Room',
    metrics: ['pH', 'EC', 'Temperature'],
    currentValue: 5.9,
    lastReadAt: new Date().toISOString(),
    isOnline: true,
  },
  {
    id: 'ph-tank-b',
    name: 'Tank B pH/EC',
    placement: 'batch_tank',
    batchTankName: 'Flower Mix Tank',
    zone: 'Mix Room',
    metrics: ['pH', 'EC', 'Temperature'],
    currentValue: 6.0,
    lastReadAt: new Date().toISOString(),
    isOnline: true,
  },
  
  // Inline sensors
  {
    id: 'ph-inline-f1',
    name: 'F1 Inline pH/EC',
    placement: 'inline',
    zone: 'Flower Room 1',
    metrics: ['pH', 'EC'],
    currentValue: 5.7,
    lastReadAt: new Date().toISOString(),
    lastEventValue: 5.8,
    lastEventEndedAt: new Date(Date.now() - 2 * 60 * 60 * 1000).toISOString(),
    isOnline: true,
  },
  {
    id: 'ph-inline-f2',
    name: 'F2 Inline pH/EC',
    placement: 'inline',
    zone: 'Flower Room 2',
    metrics: ['pH', 'EC'],
    lastEventValue: 6.0,
    lastEventEndedAt: new Date(Date.now() - 4 * 60 * 60 * 1000).toISOString(),
    isOnline: false,
  },

  // ============================================
  // SUBSTRATE SENSORS (EC, VWC, Temperature)
  // ============================================
  {
    id: 'ec-sub-f1-a',
    name: 'F1 Substrate A',
    placement: 'runoff',
    zone: 'Flower Room 1',
    metrics: ['EC', 'VWC', 'Temperature'],
    currentValue: 2.2,
    lastReadAt: new Date().toISOString(),
    isOnline: true,
  },
  {
    id: 'ec-sub-f1-b',
    name: 'F1 Substrate B',
    placement: 'runoff',
    zone: 'Flower Room 1',
    metrics: ['EC', 'VWC', 'Temperature'],
    currentValue: 2.4,
    lastReadAt: new Date().toISOString(),
    isOnline: true,
  },
  {
    id: 'ec-sub-f2-a',
    name: 'F2 Substrate A',
    placement: 'runoff',
    zone: 'Flower Room 2',
    metrics: ['EC', 'VWC', 'Temperature'],
    currentValue: 2.1,
    lastReadAt: new Date().toISOString(),
    isOnline: true,
  },
];

/**
 * Metric configuration for display
 */
export interface MetricConfig {
  label: string;
  unit: string;
  decimals: number;
  iconColor: string;
}

export const METRIC_CONFIGS: Record<SensorMetricType, MetricConfig> = {
  Temperature: { label: 'Temp', unit: '°F', decimals: 1, iconColor: '#22d3ee' },
  Humidity: { label: 'RH', unit: '%', decimals: 0, iconColor: '#a78bfa' },
  VPD: { label: 'VPD', unit: 'kPa', decimals: 2, iconColor: '#34d399' },
  CO2: { label: 'CO₂', unit: 'ppm', decimals: 0, iconColor: '#f472b6' },
  PPFD: { label: 'PPFD', unit: 'µmol/m²/s', decimals: 0, iconColor: '#fbbf24' },
  DLI: { label: 'DLI', unit: 'mol/m²', decimals: 0, iconColor: '#fbbf24' },
  pH: { label: 'pH', unit: '', decimals: 1, iconColor: '#a78bfa' },
  EC: { label: 'EC', unit: 'mS', decimals: 1, iconColor: '#60a5fa' },
  VWC: { label: 'VWC', unit: '%', decimals: 0, iconColor: '#60a5fa' },
};

/**
 * Group sensors by zone
 */
export function getMetricSensorsByZone(metric?: SensorMetricType): Record<string, MetricSensor[]> {
  const filtered = metric 
    ? METRIC_SENSORS.filter(s => s.metrics.includes(metric))
    : METRIC_SENSORS;
    
  return filtered.reduce((acc, sensor) => {
    if (!acc[sensor.zone]) acc[sensor.zone] = [];
    acc[sensor.zone].push(sensor);
    return acc;
  }, {} as Record<string, MetricSensor[]>);
}

/**
 * Get sensors filtered by metric type
 */
export function getSensorsByMetric(metric: SensorMetricType): MetricSensor[] {
  return METRIC_SENSORS.filter(s => s.metrics.includes(metric));
}

/**
 * Calculate aggregated reading from multiple sensors
 */
export function getAggregatedReading(
  sensors: MetricSensor[],
  aggregation: SensorAssignment['aggregation'],
  metric: SensorMetricType
): MetricReading | null {
  const validSensors = sensors.filter(s => s.metrics.includes(metric));
  if (validSensors.length === 0) return null;

  const readings = validSensors
    .map(s => {
      const info = getReadingDisplayInfo(s, metric);
      return info.value;
    })
    .filter((v): v is number => v !== null);

  if (readings.length === 0) return null;

  let value: number;
  let aggLabel: string;
  switch (aggregation) {
    case 'min':
      value = Math.min(...readings);
      aggLabel = 'low';
      break;
    case 'max':
      value = Math.max(...readings);
      aggLabel = 'high';
      break;
    default:
      value = readings.reduce((a, b) => a + b, 0) / readings.length;
      aggLabel = 'avg';
  }

  const config = METRIC_CONFIGS[metric];
  const roundedValue = Number(value.toFixed(config.decimals));

  return {
    value: roundedValue,
    label: `${aggLabel} of ${validSensors.length}`,
    isLive: validSensors.some(s => s.isOnline),
  };
}

/**
 * Get reading for a metric with sensor assignment
 */
export function getMetricReading(
  metric: SensorMetricType,
  assignment: SensorAssignment | null,
  fallbackValue: number
): { value: number; label: string | null; isLive: boolean } {
  if (!assignment || assignment.sensorIds.length === 0) {
    return { value: fallbackValue, label: null, isLive: false };
  }

  const sensors = assignment.sensorIds
    .map(id => METRIC_SENSORS.find(s => s.id === id))
    .filter((s): s is MetricSensor => s !== undefined);

  if (sensors.length === 0) {
    return { value: fallbackValue, label: null, isLive: false };
  }

  if (sensors.length === 1) {
    const reading = getReadingDisplayInfo(sensors[0], metric);
    return {
      value: reading.value ?? fallbackValue,
      label: reading.label,
      isLive: reading.isLive,
    };
  }

  const aggregated = getAggregatedReading(sensors, assignment.aggregation, metric);
  if (!aggregated || aggregated.value === null) {
    return { value: fallbackValue, label: null, isLive: false };
  }

  return {
    value: aggregated.value,
    label: aggregated.label,
    isLive: aggregated.isLive,
  };
}
