/**
 * Irrigation Profile Configuration Types
 * 
 * Defines the data structures for irrigation programs, 
 * shot calibration, and VWC targeting across P1/P2/P3 phases.
 */

export type VolumeUnit = 'gallons' | 'liters';

export interface ZoneCalibration {
  zoneId: string;
  zoneName: string;
  method: 'container' | 'flow_meter';
  targetVolumeMl: number;
  measuredTimeSeconds: number;
  runsCount: number;
  emitterFlowMlPerSecond: number;
  emittersPerPlant: number;
  /** Media volume per plant */
  mediaVolume: number;
  /** Unit for media volume */
  mediaVolumeUnit: VolumeUnit;
  pressureKpa?: number;
  flowPulses?: number;
  calibratedByUserId: string;
  calibratedAt: string;
}

/**
 * Convert volume to milliliters
 */
export function convertToMl(value: number, unit: VolumeUnit): number {
  switch (unit) {
    case 'gallons':
      return value * 3785.41; // 1 gallon = 3785.41 mL
    case 'liters':
      return value * 1000; // 1 liter = 1000 mL
    default:
      return value;
  }
}

/**
 * Convert milliliters to specified unit
 */
export function convertFromMl(valueMl: number, unit: VolumeUnit): number {
  switch (unit) {
    case 'gallons':
      return valueMl / 3785.41;
    case 'liters':
      return valueMl / 1000;
    default:
      return valueMl;
  }
}

/**
 * Calculate recommended shot size based on media volume
 * Rule of thumb: 2-5% of media volume per shot for substrate
 */
export function calculateRecommendedShotSize(
  mediaVolumeMl: number, 
  targetVwcIncrease: number = 3
): number {
  // Approximately 2-4% of media volume per shot depending on substrate
  const shotPercentage = targetVwcIncrease / 100;
  return Math.round(mediaVolumeMl * shotPercentage);
}

export interface ShotConfiguration {
  /** Target dose per plant in milliliters */
  shotSizeMl: number;
  /** Calculated shot duration in seconds (derived from calibration) */
  calculatedDurationSeconds: number;
  /** Expected VWC increase per shot (percentage points) */
  expectedVwcIncreasePercent: number;
  /** Minimum soak time between shots in minutes */
  minSoakTimeMinutes: number;
  /** Maximum shots allowed per day */
  maxShotsPerDay: number;
}

export type ShotCountMode = 'manual' | 'auto';

export interface PhaseTargets {
  /** P1 - Ramp Phase: Target VWC to reach at end of ramp */
  p1TargetVwcPercent: number;
  /** P1 - Number of shots in ramp phase (used when p1ShotCountMode is 'manual') */
  p1ShotCount: number;
  /** P1 - Shot count mode: 'manual' for user-defined, 'auto' for calculated from targets */
  p1ShotCountMode: ShotCountMode;
  /** P2 - Maintenance Phase: Target VWC to maintain */
  p2TargetVwcPercent: number;
  /** P2 - Number of shots in maintenance phase (used when p2ShotCountMode is 'manual') */
  p2ShotCount: number;
  /** P2 - Shot count mode: 'manual' for user-defined, 'auto' for sensor-driven */
  p2ShotCountMode: ShotCountMode;
  /** P3 - Dryback Phase: Target dryback percentage */
  p3TargetDrybackPercent: number;
  /** P3 - Allow shots in dryback phase (emergency only) */
  p3AllowEmergencyShots: boolean;
}

/**
 * Calculate P1 shot count based on targets
 * Formula: shots = (P1 Target VWC - Starting VWC) / VWC increase per shot
 * Starting VWC is derived from dryback: P1 Target - Dryback %
 */
export function calculateP1ShotCount(
  p1TargetVwcPercent: number,
  p3DrybackPercent: number,
  vwcIncreasePerShot: number
): number {
  if (vwcIncreasePerShot <= 0) return 0;
  // Starting VWC after overnight dryback
  const startingVwc = p1TargetVwcPercent - p3DrybackPercent;
  // Shots needed to reach P1 target
  const shotsNeeded = (p1TargetVwcPercent - startingVwc) / vwcIncreasePerShot;
  // Round up to ensure we reach target
  return Math.ceil(Math.max(1, shotsNeeded));
}

export interface DayProfile {
  /** Profile name (e.g., "Day Profile") */
  name: string;
  /** Whether this profile is active */
  enabled: boolean;
  /** Shot configuration for this profile */
  shotConfig: ShotConfiguration;
  /** Phase targets for this profile */
  phaseTargets: PhaseTargets;
  /** Start time (HH:MM) */
  startTime: string;
  /** End time (HH:MM) */
  endTime: string;
  /** Description of the irrigation strategy */
  description: string;
}

export interface NightProfile {
  /** Profile name (e.g., "Night Profile") */
  name: string;
  /** Whether this profile is active */
  enabled: boolean;
  /** Allow irrigation during night */
  allowIrrigation: boolean;
  /** If allowed, target VWC to maintain */
  maintainVwcPercent?: number;
  /** Maximum shots allowed during night */
  maxNightShots: number;
  /** Start time (HH:MM) */
  startTime: string;
  /** End time (HH:MM) */
  endTime: string;
  /** Description of the night strategy */
  description: string;
}

export interface SafetyPolicy {
  /** Maximum volume per plant per day (mL) */
  maxVolumeMlPerPlantPerDay: number;
  /** Maximum EC threshold */
  maxEc: number;
  /** Minimum EC threshold */
  minEc: number;
  /** Maximum pH threshold */
  maxPh: number;
  /** Minimum pH threshold */
  minPh: number;
  /** Require flow verification */
  requireFlowVerification: boolean;
  /** Require pressure verification */
  requirePressureVerification: boolean;
}

export interface IrrigationProgramProfile {
  id: string;
  name: string;
  description: string;
  groupId: string;
  recipeId: string;
  dayProfile: DayProfile;
  nightProfile: NightProfile;
  safetyPolicy: SafetyPolicy;
  zoneCalibrations: ZoneCalibration[];
  enabled: boolean;
  createdAt: string;
  updatedAt: string;
}

/**
 * Calibration wizard step
 */
export type CalibrationStep = 
  | 'select_zone'
  | 'prepare'
  | 'running'
  | 'measure'
  | 'confirm';

/**
 * Calibration session state
 */
export interface CalibrationSession {
  step: CalibrationStep;
  zoneId: string;
  zoneName: string;
  targetVolumeMl: number;
  startedAt?: string;
  stoppedAt?: string;
  measuredTimeSeconds?: number;
  emittersPerPlant: number;
  calculatedFlowRate?: number;
  isValveOpen: boolean;
}

/**
 * Calculate emitter flow rate from calibration
 */
export function calculateEmitterFlowRate(
  targetVolumeMl: number,
  measuredTimeSeconds: number,
  emittersPerPlant: number
): number {
  if (measuredTimeSeconds <= 0 || emittersPerPlant <= 0) return 0;
  // Total flow rate for all emitters divided by emitters per plant
  return targetVolumeMl / measuredTimeSeconds / emittersPerPlant;
}

/**
 * Calculate shot duration from dose and flow rate
 */
export function calculateShotDuration(
  doseMlPerPlant: number,
  emitterFlowRateMlPerSecond: number,
  emittersPerPlant: number
): number {
  if (emitterFlowRateMlPerSecond <= 0 || emittersPerPlant <= 0) return 0;
  // t_run = dose_ml_per_plant / (q_e * emitters_per_plant)
  return doseMlPerPlant / (emitterFlowRateMlPerSecond * emittersPerPlant);
}

/**
 * Default shot configuration
 */
export const DEFAULT_SHOT_CONFIG: ShotConfiguration = {
  shotSizeMl: 50,
  calculatedDurationSeconds: 0,
  expectedVwcIncreasePercent: 2,
  minSoakTimeMinutes: 30,
  maxShotsPerDay: 12,
};

/**
 * Default phase targets
 */
export const DEFAULT_PHASE_TARGETS: PhaseTargets = {
  p1TargetVwcPercent: 65,
  p1ShotCount: 6,
  p1ShotCountMode: 'manual',
  p2TargetVwcPercent: 55,
  p2ShotCount: 4,
  p2ShotCountMode: 'manual',
  p3TargetDrybackPercent: 25,
  p3AllowEmergencyShots: false,
};

/**
 * Default safety policy
 */
export const DEFAULT_SAFETY_POLICY: SafetyPolicy = {
  maxVolumeMlPerPlantPerDay: 200,
  maxEc: 3.5,
  minEc: 1.0,
  maxPh: 6.5,
  minPh: 5.5,
  requireFlowVerification: true,
  requirePressureVerification: true,
};

