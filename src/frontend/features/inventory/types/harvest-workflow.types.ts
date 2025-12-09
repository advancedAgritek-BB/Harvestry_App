/**
 * Harvest Workflow Type Definitions
 * Types for the complete harvest workflow: wet weight → drying → bucking → batching → lots
 */

// ===== ENUMS =====

/** Current phase in the harvest workflow */
export type HarvestPhase =
  | 'wet_harvest'   // Plants just cut, capturing wet weight
  | 'drying'        // In drying room
  | 'bucking'       // Being processed (separating flower from stems)
  | 'dry_weighed'   // Final dry weight recorded
  | 'batched'       // Grouped for lot creation
  | 'lot_created'   // Inventory lots created
  | 'complete';     // Workflow finished

/** How a harvest batch is composed */
export type HarvestBatchingMode =
  | 'single_strain' // One strain only
  | 'mixed_strain'  // Multiple strains blended
  | 'sub_lot';      // Portion of a larger harvest

/** Type of weight measurement */
export type WeightType =
  | 'wet_plant'
  | 'dry_plant'
  | 'bucked_flower'
  | 'stem_waste'
  | 'leaf_waste'
  | 'other_waste';

/** Source of weight capture */
export type WeightSource = 'scale' | 'manual';

/** Scale capture state machine */
export type ScaleCaptureState =
  | 'waiting_for_plant'    // Ready to receive plant
  | 'plant_detected'       // Weight detected, not yet stable
  | 'stabilizing'          // Weight changing, waiting for stability
  | 'stable_captured'      // Stable weight captured
  | 'waiting_for_removal'  // Weight captured, waiting for plant removal
  | 'scale_clear';         // Plant removed, ready for next

// ===== SCALE DEVICE =====

/** Scale device configuration */
export interface ScaleDevice {
  id: string;
  siteId: string;
  deviceName: string;
  deviceSerialNumber?: string;
  manufacturer?: string;
  model?: string;
  capacityGrams?: number;
  readabilityGrams?: number;
  connectionType: 'usb' | 'serial' | 'network' | 'bluetooth';
  connectionConfigJson?: string;
  locationId?: string;
  locationName?: string;
  isActive: boolean;
  requiresCalibration: boolean;
  calibrationIntervalDays: number;
  currentCalibration?: ScaleCalibration;
  isCalibrationValid: boolean;
  createdAt: string;
  updatedAt: string;
}

/** Scale calibration record */
export interface ScaleCalibration {
  id: string;
  scaleDeviceId: string;
  calibrationDate: string;
  calibrationDueDate: string;
  calibrationType: 'internal' | 'external' | 'certified';
  performedBy?: string;
  certifiedBy?: string;
  certificationNumber?: string;
  calibrationCompany?: string;
  testWeightsUsed?: TestWeight[];
  passed: boolean;
  deviationGrams?: number;
  deviationPercent?: number;
  notes?: string;
  certificateUrl?: string;
  isValid: boolean;
  recordedAt: string;
  recordedBy: string;
}

/** Test weight used in calibration */
export interface TestWeight {
  nominal: number;    // Expected weight
  actual: number;     // Certified actual weight
  measured: number;   // What scale read
}

/** Scale reading record */
export interface ScaleReading {
  id: string;
  harvestId?: string;
  harvestPlantId?: string;
  lotId?: string;
  scaleDeviceId?: string;
  calibrationId?: string;
  calibrationDate?: string;
  calibrationDueDate?: string;
  calibrationWasValid: boolean;
  grossWeight: number;
  tareWeight: number;
  netWeight: number;
  unitOfWeight: string;
  isStable: boolean;
  stabilityDurationMs?: number;
  readingTimestamp: string;
  rawScaleDataJson?: string;
  recordedBy: string;
  createdAt: string;
}

// ===== WEIGHT CAPTURE =====

/** Weight capture for a single plant or harvest */
export interface HarvestWeightCapture {
  harvestId: string;
  harvestPlantId?: string;
  weightType: WeightType;
  weight: number;
  uom: string;
  source: WeightSource;
  scaleReadingId?: string;
  scaleDeviceId?: string;
  calibrationWasValid?: boolean;
  isLocked: boolean;
  lockedAt?: string;
  lockedBy?: string;
}

/** Weight adjustment record */
export interface WeightAdjustment {
  id: string;
  harvestId: string;
  harvestPlantId?: string;
  weightType: WeightType;
  previousWeight: number;
  newWeight: number;
  adjustmentAmount: number;
  reasonCode: string;
  notes?: string;
  adjustedByUserId: string;
  adjustedByUserName?: string;
  pinOverrideUsed: boolean;
  adjustedAt: string;
}

/** Weight adjustment reason codes */
export const WEIGHT_ADJUSTMENT_REASONS = {
  SCALE_ERROR: 'Scale malfunction or error',
  RECOUNTED: 'Recounted/reweighed',
  DATA_ENTRY_ERROR: 'Data entry correction',
  SPILLAGE: 'Spillage adjustment',
  MOISTURE_CORRECTION: 'Moisture correction',
  FOREIGN_MATERIAL: 'Foreign material removed',
  OTHER: 'Other (see notes)',
} as const;

export type WeightAdjustmentReasonCode = keyof typeof WEIGHT_ADJUSTMENT_REASONS;

// ===== HARVEST PLANT =====

/** Harvest plant with scale integration */
export interface HarvestPlantWeighing {
  id: string;
  harvestId: string;
  plantId: string;
  plantTag: string;
  wetWeight: number;
  unitOfWeight: string;
  harvestedAt: string;
  scaleReadingId?: string;
  weightSource: WeightSource;
  isWeightLocked: boolean;
  weightLockedAt?: string;
  weightLockedBy?: string;
  
  // UI state (not persisted)
  status?: 'pending' | 'weighing' | 'weighed' | 'error';
  errorMessage?: string;
}

// ===== HARVEST METRICS =====

/** Calculated harvest metrics */
export interface HarvestMetrics {
  wetWeight: number;
  dryWeight: number;
  buckedFlowerWeight: number;
  totalWasteWeight: number;
  stemWaste: number;
  leafWaste: number;
  otherWaste: number;
  moistureLossPercent: number;
  dryToWetRatio: number;
  usableFlowerPercent: number;
  wastePercent: number;
  yieldPerPlant?: number;
  plantsHarvested: number;
  plantsWeighed: number;
}

/** Harvest workflow state */
export interface HarvestWorkflowState {
  harvestId: string;
  harvestName: string;
  strainName: string;
  phase: HarvestPhase;
  
  // Drying
  dryingStartDate?: string;
  dryingEndDate?: string;
  dryingDurationDays?: number;
  dryingLocationId?: string;
  dryingLocationName?: string;
  
  // Weights
  totalWetWeight: number;
  totalDryWeight: number;
  currentWeight: number;
  
  // Lock status
  wetWeightLocked: boolean;
  dryWeightLocked: boolean;
  
  // Batching
  batchingMode?: HarvestBatchingMode;
  parentHarvestId?: string;
  childHarvestIds: string[];
  
  // Metrics
  metrics: HarvestMetrics;
  
  // Plants
  plants: HarvestPlantWeighing[];
  
  // Adjustments
  adjustments: WeightAdjustment[];
}

// ===== DRY BATCHING =====

/** Dry batch for grouping dried flower */
export interface DryBatch {
  id: string;
  batchNumber: string;
  batchingMode: HarvestBatchingMode;
  sourceHarvestIds: string[];
  strainIds: string[];
  strainNames: string[];
  totalWeight: number;
  unitOfWeight: string;
  status: 'pending' | 'active' | 'lotted' | 'complete';
  outputLotIds: string[];
  createdAt: string;
  createdBy: string;
}

// ===== LOT SPLITTING =====

/** Lot split request */
export interface LotSplitRequest {
  sourceBatchId?: string;
  sourceLotId?: string;
  splits?: LotSplitItem[];
  quantities?: number[];
  destinationLocationIds?: string[];
  notes?: string;
}

/** Individual lot split item */
export interface LotSplitItem {
  quantity: number;
  uom: string;
  qualityGrade?: 'A' | 'B' | 'C' | 'D';
  destination: 'retail' | 'wholesale' | 'manufacturing' | 'sample';
  productId?: string;
  locationId: string;
  notes?: string;
}

// ===== SCALE CAPTURE SETTINGS =====

/** Settings for scale auto-capture behavior */
export interface ScaleCaptureSettings {
  /** Minimum weight in grams to detect a plant on scale */
  plantDetectionThreshold: number;
  
  /** Weight in grams below which scale is considered "clear" */
  scaleClearThreshold: number;
  
  /** Minimum weight change in grams required between captures */
  minimumWeightChange: number;
  
  /** Minimum time in ms between captures */
  captureCooldownMs: number;
  
  /** Stability duration required before auto-capture (ms) */
  stabilityDurationMs: number;
  
  /** Percentage threshold for duplicate weight warning */
  duplicateWarningPercent: number;
  
  /** Number of previous weights to compare for duplicate detection */
  duplicateComparisonCount: number;
  
  /** Auto-capture when stable (vs manual capture button) */
  autoCaptureOnStable: boolean;
  
  /** Play audio cues */
  audioEnabled: boolean;
}

/** Default scale capture settings */
export const DEFAULT_SCALE_CAPTURE_SETTINGS: ScaleCaptureSettings = {
  plantDetectionThreshold: 10,      // 10g minimum to detect plant
  scaleClearThreshold: 5,           // <5g = scale clear
  minimumWeightChange: 20,          // 20g minimum change between captures
  captureCooldownMs: 1500,          // 1.5 second cooldown
  stabilityDurationMs: 750,         // 750ms stable before capture
  duplicateWarningPercent: 5,       // Warn if within 5% of recent weights
  duplicateComparisonCount: 3,      // Compare to last 3 weights
  autoCaptureOnStable: true,        // Auto-capture by default
  audioEnabled: true,               // Audio cues on
};

// ===== API REQUESTS =====

/** Request to record wet weight for a plant */
export interface RecordPlantWetWeightRequest {
  harvestId: string;
  plantId: string;
  plantTag: string;
  wetWeight: number;
  uom: string;
  scaleReadingId?: string;
  scaleDeviceId?: string;
}

/** Request to start drying phase */
export interface StartDryingRequest {
  harvestId: string;
  dryingLocationId?: string;
  dryingLocationName?: string;
}

/** Request to record bucking results */
export interface RecordBuckingResultsRequest {
  harvestId: string;
  buckedFlowerWeight: number;
  stemWaste: number;
  leafWaste: number;
  otherWaste: number;
  uom: string;
}

/** Request to adjust a weight */
export interface AdjustWeightRequest {
  harvestId: string;
  harvestPlantId?: string;
  weightType: WeightType;
  newWeight: number;
  reasonCode: WeightAdjustmentReasonCode;
  notes?: string;
  pin: string;
}

/** Request to create a dry batch */
export interface CreateDryBatchRequest {
  sourceHarvestIds: string[];
  batchingMode: HarvestBatchingMode;
  batchNumber?: string;
}

// ===== UI STATE =====

/** Weighing session state for UI */
export interface WeighingSessionState {
  harvestId: string;
  scaleDeviceId?: string;
  scaleDevice?: ScaleDevice;
  captureState: ScaleCaptureState;
  currentWeight: number;
  isStable: boolean;
  stabilityDurationMs: number;
  lastCapturedWeight?: number;
  lastCapturedAt?: string;
  captureSettings: ScaleCaptureSettings;
  recentWeights: number[];
  errorMessage?: string;
  warningMessage?: string;
}

// ===== PHASE CONFIGURATION =====

/** Phase configuration for display */
export const HARVEST_PHASE_CONFIG: Record<HarvestPhase, {
  label: string;
  description: string;
  color: string;
  bgColor: string;
  icon: string;
}> = {
  wet_harvest: {
    label: 'Wet Harvest',
    description: 'Weighing freshly cut plants',
    color: 'text-emerald-400',
    bgColor: 'bg-emerald-500/10',
    icon: 'Scale',
  },
  drying: {
    label: 'Drying',
    description: 'Plants drying in controlled environment',
    color: 'text-amber-400',
    bgColor: 'bg-amber-500/10',
    icon: 'Wind',
  },
  bucking: {
    label: 'Bucking',
    description: 'Separating flower from stems',
    color: 'text-orange-400',
    bgColor: 'bg-orange-500/10',
    icon: 'Scissors',
  },
  dry_weighed: {
    label: 'Dry Weighed',
    description: 'Final dry weight recorded',
    color: 'text-cyan-400',
    bgColor: 'bg-cyan-500/10',
    icon: 'Scale',
  },
  batched: {
    label: 'Batched',
    description: 'Ready for lot creation',
    color: 'text-violet-400',
    bgColor: 'bg-violet-500/10',
    icon: 'Package',
  },
  lot_created: {
    label: 'Lots Created',
    description: 'Inventory lots created',
    color: 'text-blue-400',
    bgColor: 'bg-blue-500/10',
    icon: 'Boxes',
  },
  complete: {
    label: 'Complete',
    description: 'Harvest workflow finished',
    color: 'text-muted-foreground',
    bgColor: 'bg-muted/50',
    icon: 'CheckCircle',
  },
};

/** Capture state configuration for display */
export const CAPTURE_STATE_CONFIG: Record<ScaleCaptureState, {
  label: string;
  instruction: string;
  color: string;
  bgColor: string;
}> = {
  waiting_for_plant: {
    label: 'Ready',
    instruction: 'Place plant on scale',
    color: 'text-muted-foreground',
    bgColor: 'bg-muted/30',
  },
  plant_detected: {
    label: 'Detected',
    instruction: 'Weight detected, stabilizing...',
    color: 'text-amber-400',
    bgColor: 'bg-amber-500/10',
  },
  stabilizing: {
    label: 'Stabilizing',
    instruction: 'Waiting for stable weight...',
    color: 'text-amber-400',
    bgColor: 'bg-amber-500/10',
  },
  stable_captured: {
    label: 'Captured!',
    instruction: 'Weight recorded successfully',
    color: 'text-emerald-400',
    bgColor: 'bg-emerald-500/10',
  },
  waiting_for_removal: {
    label: 'Remove Plant',
    instruction: 'Remove plant from scale',
    color: 'text-cyan-400',
    bgColor: 'bg-cyan-500/10',
  },
  scale_clear: {
    label: 'Clear',
    instruction: 'Ready for next plant',
    color: 'text-emerald-400',
    bgColor: 'bg-emerald-500/10',
  },
};
