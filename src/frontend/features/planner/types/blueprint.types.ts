/**
 * Blueprint Types
 * 
 * Blueprints define environmental, irrigation, and lighting parameters
 * that can be assigned to batches or individual phases.
 */

import { PhaseType } from './planner.types';

/**
 * Environmental parameters for a phase
 */
export interface EnvironmentalParams {
  // Temperature
  tempDayTarget: number;      // °F
  tempNightTarget: number;    // °F
  tempTolerance: number;      // ± degrees
  
  // Humidity
  rhDayTarget: number;        // %
  rhNightTarget: number;      // %
  rhTolerance: number;        // ± %
  
  // VPD (calculated from temp/RH but can have targets)
  vpdDayTarget: number;       // kPa
  vpdNightTarget: number;     // kPa
  vpdTolerance: number;       // ± kPa
  
  // CO2
  co2DayTarget: number;       // ppm
  co2NightTarget: number;     // ppm (usually ambient/off)
  co2Tolerance: number;       // ± ppm
}

/**
 * Lighting parameters for a phase
 */
export interface LightingParams {
  photoperiod: number;        // Hours of light (e.g., 18 for veg, 12 for flower)
  ppfdTarget: number;         // µmol/m²/s
  ppfdRampUp: number;         // Minutes to ramp up
  ppfdRampDown: number;       // Minutes to ramp down
  dliTarget: number;          // mol/m²/day (Daily Light Integral)
  spectrumProfile?: string;   // Optional spectrum preset name
}

/**
 * Irrigation parameters for a phase
 */
export interface IrrigationParams {
  // Feed targets
  ecTarget: number;           // mS/cm
  ecTolerance: number;        // ± mS/cm
  phTarget: number;           // pH units
  phTolerance: number;        // ± pH
  
  // Irrigation strategy
  irrigationMode: 'time-based' | 'sensor-based' | 'hybrid';
  
  // Time-based settings
  shotsPerDay?: number;
  shotVolumeMl?: number;
  firstShotTime?: string;     // HH:MM format
  lastShotTime?: string;      // HH:MM format
  
  // Sensor-based settings
  drybackTargetPercent?: number;
  minSoakMinutes?: number;
  maxShotsPerDay?: number;
  
  // VWC targets
  vwcDayTarget?: number;      // %
  vwcNightTarget?: number;    // %
  
  // Runoff/leachate
  runoffTargetPercent?: number;
}

/**
 * A blueprint for a single phase
 */
export interface PhaseBlueprint {
  id: string;
  name: string;
  description?: string;
  phase: PhaseType;
  
  // Strain associations (optional - can be generic)
  strainIds?: string[];
  geneticsIds?: string[];
  
  // Parameters
  environmental: EnvironmentalParams;
  lighting: LightingParams;
  irrigation: IrrigationParams;
  
  // Recommended duration
  defaultDurationDays: number;
  minDurationDays?: number;
  maxDurationDays?: number;
  
  // Metadata
  createdBy: string;
  createdAt: Date;
  updatedAt: Date;
  isDefault?: boolean;
  isPublic?: boolean;         // Shared across org
}

/**
 * A batch blueprint - links phase blueprints together for a complete lifecycle
 */
export interface BatchBlueprint {
  id: string;
  name: string;
  description?: string;
  
  // Strain associations
  strainIds?: string[];
  geneticsIds?: string[];
  
  // Phase blueprints (in lifecycle order)
  cloneBlueprintId?: string;
  vegBlueprintId?: string;
  flowerBlueprintId?: string;
  harvestBlueprintId?: string;
  cureBlueprintId?: string;
  
  // Resolved phase blueprints (for display)
  cloneBlueprint?: PhaseBlueprint;
  vegBlueprint?: PhaseBlueprint;
  flowerBlueprint?: PhaseBlueprint;
  harvestBlueprint?: PhaseBlueprint;
  cureBlueprint?: PhaseBlueprint;
  
  // Metadata
  createdBy: string;
  createdAt: Date;
  updatedAt: Date;
  isDefault?: boolean;
  isPublic?: boolean;
}

/**
 * Blueprint assignment for a batch
 * Users can assign a batch blueprint OR individual phase blueprints
 */
export interface BatchBlueprintAssignment {
  batchId: string;
  
  // Option 1: Assign a complete batch blueprint
  batchBlueprintId?: string;
  
  // Option 2: Assign individual phase blueprints (overrides batch blueprint)
  phaseBlueprintOverrides?: {
    clone?: string;   // PhaseBlueprint ID
    veg?: string;
    flower?: string;
    harvest?: string;
    cure?: string;
  };
  
  // Custom parameter overrides (per-batch customizations)
  parameterOverrides?: {
    [phaseKey: string]: Partial<EnvironmentalParams & LightingParams & IrrigationParams>;
  };
}

/**
 * Default environmental parameters by phase
 */
export const DEFAULT_ENVIRONMENTAL_PARAMS: Record<PhaseType, EnvironmentalParams> = {
  clone: {
    tempDayTarget: 78, tempNightTarget: 75, tempTolerance: 3,
    rhDayTarget: 75, rhNightTarget: 80, rhTolerance: 5,
    vpdDayTarget: 0.8, vpdNightTarget: 0.6, vpdTolerance: 0.2,
    co2DayTarget: 800, co2NightTarget: 400, co2Tolerance: 100,
  },
  veg: {
    tempDayTarget: 80, tempNightTarget: 72, tempTolerance: 3,
    rhDayTarget: 65, rhNightTarget: 70, rhTolerance: 5,
    vpdDayTarget: 1.0, vpdNightTarget: 0.8, vpdTolerance: 0.2,
    co2DayTarget: 1000, co2NightTarget: 400, co2Tolerance: 100,
  },
  flower: {
    tempDayTarget: 82, tempNightTarget: 68, tempTolerance: 3,
    rhDayTarget: 55, rhNightTarget: 60, rhTolerance: 5,
    vpdDayTarget: 1.2, vpdNightTarget: 1.0, vpdTolerance: 0.2,
    co2DayTarget: 1200, co2NightTarget: 400, co2Tolerance: 100,
  },
  harvest: {
    tempDayTarget: 70, tempNightTarget: 65, tempTolerance: 3,
    rhDayTarget: 50, rhNightTarget: 55, rhTolerance: 5,
    vpdDayTarget: 1.4, vpdNightTarget: 1.2, vpdTolerance: 0.2,
    co2DayTarget: 400, co2NightTarget: 400, co2Tolerance: 50,
  },
  cure: {
    tempDayTarget: 65, tempNightTarget: 62, tempTolerance: 2,
    rhDayTarget: 60, rhNightTarget: 62, rhTolerance: 3,
    vpdDayTarget: 1.0, vpdNightTarget: 0.9, vpdTolerance: 0.1,
    co2DayTarget: 400, co2NightTarget: 400, co2Tolerance: 50,
  },
};

/**
 * Default lighting parameters by phase
 */
export const DEFAULT_LIGHTING_PARAMS: Record<PhaseType, LightingParams> = {
  clone: {
    photoperiod: 18, ppfdTarget: 200, ppfdRampUp: 30, ppfdRampDown: 30, dliTarget: 13,
  },
  veg: {
    photoperiod: 18, ppfdTarget: 600, ppfdRampUp: 60, ppfdRampDown: 60, dliTarget: 39,
  },
  flower: {
    photoperiod: 12, ppfdTarget: 900, ppfdRampUp: 60, ppfdRampDown: 60, dliTarget: 39,
  },
  harvest: {
    photoperiod: 0, ppfdTarget: 0, ppfdRampUp: 0, ppfdRampDown: 0, dliTarget: 0,
  },
  cure: {
    photoperiod: 0, ppfdTarget: 0, ppfdRampUp: 0, ppfdRampDown: 0, dliTarget: 0,
  },
};

/**
 * Default irrigation parameters by phase
 */
export const DEFAULT_IRRIGATION_PARAMS: Record<PhaseType, IrrigationParams> = {
  clone: {
    ecTarget: 0.8, ecTolerance: 0.2, phTarget: 5.8, phTolerance: 0.3,
    irrigationMode: 'time-based',
    shotsPerDay: 4, shotVolumeMl: 50, firstShotTime: '08:00', lastShotTime: '18:00',
  },
  veg: {
    ecTarget: 1.8, ecTolerance: 0.3, phTarget: 5.8, phTolerance: 0.3,
    irrigationMode: 'sensor-based',
    drybackTargetPercent: 10, minSoakMinutes: 45, maxShotsPerDay: 8,
    vwcDayTarget: 65, vwcNightTarget: 55,
  },
  flower: {
    ecTarget: 2.4, ecTolerance: 0.3, phTarget: 6.0, phTolerance: 0.3,
    irrigationMode: 'sensor-based',
    drybackTargetPercent: 15, minSoakMinutes: 60, maxShotsPerDay: 6,
    vwcDayTarget: 55, vwcNightTarget: 45, runoffTargetPercent: 15,
  },
  harvest: {
    ecTarget: 0, ecTolerance: 0, phTarget: 0, phTolerance: 0,
    irrigationMode: 'time-based',
    shotsPerDay: 0,
  },
  cure: {
    ecTarget: 0, ecTolerance: 0, phTarget: 0, phTolerance: 0,
    irrigationMode: 'time-based',
    shotsPerDay: 0,
  },
};










