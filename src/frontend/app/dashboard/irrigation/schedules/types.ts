/**
 * Irrigation Schedule Types
 *
 * Defines the data structures for irrigation schedules
 * including time-based, sensor-triggered, and hybrid configurations.
 */

export type ScheduleType = 'time' | 'sensor' | 'hybrid';

export type ScheduleStatus = 'active' | 'paused';

/** Supported metrics for predicate conditions */
export type PredicateMetric = 
  | 'vwc'      // Volumetric Water Content (%)
  | 'ec'       // Electrical Conductivity (mS/cm)
  | 'ph'       // pH level
  | 'temp'     // Temperature (°F)
  | 'humidity' // Relative Humidity (%)
  | 'vpd';     // Vapor Pressure Deficit (kPa)

/** Comparison operators for predicates */
export type PredicateOperator = '<' | '<=' | '>' | '>=' | '==' | '!=';

/** A single predicate condition */
export interface SchedulePredicate {
  id: string;
  metric: PredicateMetric;
  operator: PredicateOperator;
  value: number;
}

/** Metric configuration for display and validation */
export interface MetricConfig {
  label: string;
  unit: string;
  min: number;
  max: number;
  step: number;
  defaultValue: number;
}

export const PREDICATE_METRICS: Record<PredicateMetric, MetricConfig> = {
  vwc: { label: 'VWC', unit: '%', min: 0, max: 100, step: 1, defaultValue: 55 },
  ec: { label: 'EC', unit: 'mS/cm', min: 0, max: 10, step: 0.1, defaultValue: 2.0 },
  ph: { label: 'pH', unit: '', min: 0, max: 14, step: 0.1, defaultValue: 6.0 },
  temp: { label: 'Temp', unit: '°F', min: 32, max: 120, step: 1, defaultValue: 72 },
  humidity: { label: 'RH', unit: '%', min: 0, max: 100, step: 1, defaultValue: 60 },
  vpd: { label: 'VPD', unit: 'kPa', min: 0, max: 3, step: 0.05, defaultValue: 1.0 },
};

export const PREDICATE_OPERATORS: { value: PredicateOperator; label: string }[] = [
  { value: '<', label: '< less than' },
  { value: '<=', label: '≤ at most' },
  { value: '>', label: '> greater than' },
  { value: '>=', label: '≥ at least' },
  { value: '==', label: '= equals' },
  { value: '!=', label: '≠ not equal' },
];

export interface IrrigationSchedule {
  id: string;
  name: string;
  program: string;
  scheduleType: ScheduleType;
  /** Time for the schedule trigger (HH:MM format) */
  time: string;
  /** Days of the week (e.g., 'Daily', 'Mon, Wed, Fri') */
  days: string;
  /** Target zones for this schedule */
  targetZones: string;
  /** Trigger conditions (for sensor/hybrid types) - structured predicates */
  triggers: SchedulePredicate[];
  /** Guard conditions that must ALL be met before irrigation runs */
  guardConditions: SchedulePredicate[];
  /** Minimum wait time between irrigation shots (minutes) - allows substrate to absorb water */
  minSoakMinutes: number;
  /** Maximum number of irrigation shots allowed per day - prevents over-watering */
  maxShotsPerDay: number;
  /** Quiet hours start time (HH:MM) */
  quietHoursStart: string;
  /** Quiet hours end time (HH:MM) */
  quietHoursEnd: string;
  /** Whether the schedule is enabled */
  enabled: boolean;
  status: ScheduleStatus;
}

export interface ScheduleFormData {
  name: string;
  program: string;
  scheduleType: ScheduleType;
  time: string;
  days: string;
  targetZones: string;
  triggers: SchedulePredicate[];
  guardConditions: SchedulePredicate[];
  minSoakMinutes: number;
  maxShotsPerDay: number;
  quietHoursStart: string;
  quietHoursEnd: string;
  enabled: boolean;
}

export const SCHEDULE_TYPE_OPTIONS = [
  { value: 'time', label: 'Time-based (Fixed Schedule)' },
  { value: 'sensor', label: 'Sensor-triggered (VWC/EC/pH)' },
  { value: 'hybrid', label: 'Hybrid (Time OR Sensor)' },
] as const;

export const PROGRAM_OPTIONS = [
  { value: 'P1 - Morning Ramp', label: 'P1 - Morning Ramp' },
  { value: 'P2 - Maintenance', label: 'P2 - Maintenance' },
  { value: 'P3 - Dryback', label: 'P3 - Dryback' },
  { value: 'Flush Lines', label: 'Flush Lines' },
  { value: 'Emergency Override', label: 'Emergency Override' },
] as const;

export const DAY_OPTIONS = [
  { value: 'Daily', label: 'Daily' },
  { value: 'Mon, Wed, Fri', label: 'Mon, Wed, Fri' },
  { value: 'Tue, Thu, Sat', label: 'Tue, Thu, Sat' },
  { value: 'Weekdays', label: 'Weekdays (Mon-Fri)' },
  { value: 'Weekends', label: 'Weekends (Sat-Sun)' },
] as const;

export const ZONE_OPTIONS = [
  { value: 'All Zones', label: 'All Zones' },
  { value: 'Zones A, B', label: 'Zones A, B' },
  { value: 'Zones C, D', label: 'Zones C, D' },
  { value: 'Zones D, E', label: 'Zones D, E' },
  { value: 'Zone A', label: 'Zone A Only' },
  { value: 'Zone B', label: 'Zone B Only' },
] as const;

export const DEFAULT_SCHEDULE_FORM: ScheduleFormData = {
  name: '',
  program: 'P1 - Morning Ramp',
  scheduleType: 'time',
  time: '08:00',
  days: 'Daily',
  targetZones: 'All Zones',
  triggers: [],
  guardConditions: [],
  minSoakMinutes: 30,
  maxShotsPerDay: 8,
  quietHoursStart: '18:00',
  quietHoursEnd: '06:00',
  enabled: true,
};

/** Generate a unique ID for predicates */
export function generatePredicateId(): string {
  return `pred_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
}

/** Create a default predicate */
export function createDefaultPredicate(metric: PredicateMetric = 'vwc'): SchedulePredicate {
  const config = PREDICATE_METRICS[metric];
  return {
    id: generatePredicateId(),
    metric,
    operator: '<',
    value: config.defaultValue,
  };
}

/** Format a predicate for display */
export function formatPredicate(predicate: SchedulePredicate): string {
  const config = PREDICATE_METRICS[predicate.metric];
  return `${config.label} ${predicate.operator} ${predicate.value}${config.unit}`;
}

/** Format multiple predicates for display */
export function formatPredicates(predicates: SchedulePredicate[]): string {
  if (predicates.length === 0) return '';
  return predicates.map(formatPredicate).join(', ');
}
