/**
 * Irrigation Program Types
 * 
 * A Program is the primary entity for irrigation control.
 * It contains shot configuration, phase targets, schedule, and safety policy.
 */

import {
  ShotConfiguration,
  PhaseTargets,
  SafetyPolicy,
  ZoneCalibration,
  DEFAULT_SHOT_CONFIG,
  DEFAULT_PHASE_TARGETS,
  DEFAULT_SAFETY_POLICY,
} from '@/components/irrigation/types';

export type ProgramType = 'ramp' | 'maintenance' | 'dryback' | 'flush' | 'custom';

export type ScheduleTriggerType = 'time' | 'sensor' | 'hybrid';

/**
 * Schedule configuration embedded within a program
 */
export interface ProgramSchedule {
  /** Trigger type: time-based, sensor-based, or hybrid */
  triggerType: ScheduleTriggerType;
  /** Time to start (HH:MM format) - for time/hybrid triggers */
  startTime: string;
  /** Days of the week */
  days: ('Mon' | 'Tue' | 'Wed' | 'Thu' | 'Fri' | 'Sat' | 'Sun')[];
  /** Sensor trigger conditions - for sensor/hybrid triggers */
  sensorTriggers: SensorTrigger[];
  /** Guard conditions that must ALL be met before running */
  guardConditions: SensorTrigger[];
  /** Quiet hours - don't run during this window */
  quietHoursStart: string;
  quietHoursEnd: string;
  /** Whether the schedule is enabled */
  enabled: boolean;
}

export type SensorAggregation = 'average' | 'min' | 'max' | 'any';

export interface SensorTrigger {
  /** Selected sensor IDs to monitor (1 or more) */
  sensorIds: string[];
  /** Aggregation method when using multiple sensors */
  aggregation: SensorAggregation;
  /** Metric to evaluate */
  metric: 'VWC' | 'EC' | 'pH' | 'Temperature' | 'Humidity' | 'VPD';
  /** Comparison operator */
  operator: '<' | '<=' | '>' | '>=' | '==' | '!=';
  /** Threshold value */
  value: number;
  /** Unit for display */
  unit: string;
}

/**
 * Available sensors for selection
 */
export interface SensorOption {
  id: string;
  name: string;
  zone: string;
  type: 'substrate' | 'climate' | 'water';
  metrics: SensorTrigger['metric'][];
}

/**
 * Night profile configuration
 */
export interface NightProfile {
  allowIrrigation: boolean;
  maintainVwcPercent: number;
  maxNightShots: number;
  startTime: string;
  endTime: string;
}

/**
 * Complete Irrigation Program
 */
export interface IrrigationProgram {
  id: string;
  name: string;
  description: string;
  type: ProgramType;
  /** Target zones for this program */
  targetZones: string[];
  /** Shot configuration */
  shotConfig: ShotConfiguration;
  /** Phase targets (P1/P2/P3) */
  phaseTargets: PhaseTargets;
  /** Night profile */
  nightProfile: NightProfile;
  /** Safety policy */
  safetyPolicy: SafetyPolicy;
  /** Schedule - when to run */
  schedule: ProgramSchedule;
  /** Zone calibration data */
  calibration: ZoneCalibration | null;
  /** Whether the program is enabled */
  enabled: boolean;
  /** Timestamps */
  createdAt: string;
  updatedAt: string;
}

/**
 * Form data for creating/editing a program
 */
export interface ProgramFormData {
  name: string;
  description: string;
  type: ProgramType;
  targetZones: string[];
  shotConfig: ShotConfiguration;
  phaseTargets: PhaseTargets;
  nightProfile: NightProfile;
  safetyPolicy: SafetyPolicy;
  schedule: ProgramSchedule;
  enabled: boolean;
}

export const PROGRAM_TYPES: { value: ProgramType; label: string; description: string }[] = [
  { value: 'ramp', label: 'Ramp (P1)', description: 'Morning saturation phase' },
  { value: 'maintenance', label: 'Maintenance (P2)', description: 'Midday VWC maintenance' },
  { value: 'dryback', label: 'Dryback (P3)', description: 'Evening dry-down phase' },
  { value: 'flush', label: 'Flush', description: 'Line flush / system maintenance' },
  { value: 'custom', label: 'Custom', description: 'Custom irrigation program' },
];

export const DAYS_OF_WEEK = [
  { value: 'Mon', label: 'Mon' },
  { value: 'Tue', label: 'Tue' },
  { value: 'Wed', label: 'Wed' },
  { value: 'Thu', label: 'Thu' },
  { value: 'Fri', label: 'Fri' },
  { value: 'Sat', label: 'Sat' },
  { value: 'Sun', label: 'Sun' },
] as const;

export const SENSOR_METRICS: { value: SensorTrigger['metric']; label: string; unit: string }[] = [
  { value: 'VWC', label: 'Volumetric Water Content', unit: '%' },
  { value: 'EC', label: 'Electrical Conductivity', unit: 'mS/cm' },
  { value: 'pH', label: 'pH', unit: '' },
  { value: 'Temperature', label: 'Temperature', unit: '°F' },
  { value: 'Humidity', label: 'Humidity', unit: '%' },
  { value: 'VPD', label: 'Vapor Pressure Deficit', unit: 'kPa' },
];

export const SENSOR_AGGREGATIONS: { value: SensorAggregation; label: string; description: string }[] = [
  { value: 'average', label: 'Average', description: 'Mean of selected sensors' },
  { value: 'min', label: 'Minimum', description: 'Lowest reading' },
  { value: 'max', label: 'Maximum', description: 'Highest reading' },
  { value: 'any', label: 'Any', description: 'Trigger if ANY sensor meets condition' },
];

/**
 * Mock sensors - in production, fetched from API
 */
export const AVAILABLE_SENSORS: SensorOption[] = [
  { id: 'sens-f1-1', name: 'F1 Substrate A', zone: 'Flower Room 1', type: 'substrate', metrics: ['VWC', 'EC', 'Temperature'] },
  { id: 'sens-f1-2', name: 'F1 Substrate B', zone: 'Flower Room 1', type: 'substrate', metrics: ['VWC', 'EC', 'Temperature'] },
  { id: 'sens-f1-3', name: 'F1 Substrate C', zone: 'Flower Room 1', type: 'substrate', metrics: ['VWC', 'EC', 'Temperature'] },
  { id: 'sens-f1-4', name: 'F1 Climate', zone: 'Flower Room 1', type: 'climate', metrics: ['Temperature', 'Humidity', 'VPD'] },
  { id: 'sens-f2-1', name: 'F2 Substrate A', zone: 'Flower Room 2', type: 'substrate', metrics: ['VWC', 'EC', 'Temperature'] },
  { id: 'sens-f2-2', name: 'F2 Substrate B', zone: 'Flower Room 2', type: 'substrate', metrics: ['VWC', 'EC', 'Temperature'] },
  { id: 'sens-v1-1', name: 'V1 Substrate A', zone: 'Veg Room 1', type: 'substrate', metrics: ['VWC', 'EC', 'Temperature'] },
  { id: 'sens-v1-2', name: 'V1 Climate', zone: 'Veg Room 1', type: 'climate', metrics: ['Temperature', 'Humidity', 'VPD'] },
  { id: 'sens-res-1', name: 'Reservoir pH/EC', zone: 'Reservoir', type: 'water', metrics: ['pH', 'EC', 'Temperature'] },
];

/**
 * Group sensors by zone for easier selection
 */
export function getSensorsByZone(): Record<string, SensorOption[]> {
  return AVAILABLE_SENSORS.reduce((acc, sensor) => {
    if (!acc[sensor.zone]) acc[sensor.zone] = [];
    acc[sensor.zone].push(sensor);
    return acc;
  }, {} as Record<string, SensorOption[]>);
}

/**
 * Get common metrics available across selected sensors
 */
export function getCommonMetrics(sensorIds: string[]): SensorTrigger['metric'][] {
  if (sensorIds.length === 0) return [];
  
  const sensors = sensorIds.map(id => AVAILABLE_SENSORS.find(s => s.id === id)).filter(Boolean) as SensorOption[];
  if (sensors.length === 0) return [];
  
  // Find intersection of all sensor metrics
  return sensors[0].metrics.filter(metric => 
    sensors.every(sensor => sensor.metrics.includes(metric))
  );
}

/**
 * Get display names for selected sensors
 */
export function getSensorDisplayNames(sensorIds: string[]): string {
  if (sensorIds.length === 0) return 'No sensors selected';
  if (sensorIds.length === 1) {
    const sensor = AVAILABLE_SENSORS.find(s => s.id === sensorIds[0]);
    return sensor?.name || 'Unknown';
  }
  return `${sensorIds.length} sensors selected`;
}

export const DEFAULT_NIGHT_PROFILE: NightProfile = {
  allowIrrigation: false,
  maintainVwcPercent: 45,
  maxNightShots: 2,
  startTime: '18:00',
  endTime: '06:00',
};

export const DEFAULT_SCHEDULE: ProgramSchedule = {
  triggerType: 'time',
  startTime: '06:00',
  days: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
  sensorTriggers: [],
  guardConditions: [],
  quietHoursStart: '18:00',
  quietHoursEnd: '06:00',
  enabled: true,
};

export const DEFAULT_PROGRAM_FORM: ProgramFormData = {
  name: '',
  description: '',
  type: 'ramp',
  targetZones: [],
  shotConfig: DEFAULT_SHOT_CONFIG,
  phaseTargets: DEFAULT_PHASE_TARGETS,
  nightProfile: DEFAULT_NIGHT_PROFILE,
  safetyPolicy: DEFAULT_SAFETY_POLICY,
  schedule: DEFAULT_SCHEDULE,
  enabled: true,
};

/**
 * Get display color for program type
 */
export function getProgramTypeColor(type: ProgramType): string {
  switch (type) {
    case 'ramp':
      return 'text-violet-400 bg-violet-500/10 border-violet-500/20';
    case 'maintenance':
      return 'text-cyan-400 bg-cyan-500/10 border-cyan-500/20';
    case 'dryback':
      return 'text-amber-400 bg-amber-500/10 border-amber-500/20';
    case 'flush':
      return 'text-blue-400 bg-blue-500/10 border-blue-500/20';
    case 'custom':
      return 'text-emerald-400 bg-emerald-500/10 border-emerald-500/20';
    default:
      return 'text-muted-foreground bg-muted border-border';
  }
}

/**
 * Format days array for display
 */
export function formatDays(days: string[]): string {
  if (days.length === 7) return 'Daily';
  if (days.length === 5 && !days.includes('Sat') && !days.includes('Sun')) return 'Weekdays';
  if (days.length === 2 && days.includes('Sat') && days.includes('Sun')) return 'Weekends';
  return days.join(', ');
}

