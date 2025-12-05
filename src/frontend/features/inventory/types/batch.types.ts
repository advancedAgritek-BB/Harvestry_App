/**
 * Cultivation Batch Type Definitions
 * Tracks plants from seed/clone through harvest - the origin of all cannabis inventory
 */

import type { InventoryLot } from './lot.types';

/** Batch origin type */
export type BatchOriginType =
  | 'seed'
  | 'clone'
  | 'mother_cutting'
  | 'tissue_culture';

/** Cultivation phase */
export type CultivationPhase =
  | 'germination'
  | 'propagation'
  | 'vegetative'
  | 'flowering'
  | 'harvest'
  | 'drying'
  | 'curing'
  | 'complete';

/** Batch status */
export type BatchStatus =
  | 'planned'
  | 'active'
  | 'harvested'
  | 'processing'
  | 'complete'
  | 'destroyed'
  | 'cancelled';

/** Plant loss reason */
export type PlantLossReason =
  | 'culling'
  | 'disease'
  | 'pest'
  | 'environmental'
  | 'male_identification'
  | 'hermaphrodite'
  | 'nutrient_deficiency'
  | 'light_burn'
  | 'root_rot'
  | 'mechanical_damage'
  | 'theft'
  | 'other';

/** Cultivation Batch - the source of all cannabis inventory */
export interface CultivationBatch {
  id: string;
  siteId: string;

  // Identification
  batchNumber: string;
  name?: string;
  description?: string;

  // ORIGIN - The TRUE source for traceability
  originType: BatchOriginType;
  originLotId?: string;           // Seed/clone lot that was consumed
  originLot?: InventoryLot;
  originMotherPlantId?: string;   // If from mother cutting
  originMotherPlantTag?: string;

  // Genetics
  geneticId: string;
  geneticName: string;
  strainId: string;
  strainName: string;
  phenotypeId?: string;
  phenotypeName?: string;

  // Generation tracking for multi-generational traceability
  generationNumber: number;       // How many generations from original seed
  parentBatchId?: string;         // If cloned from another batch
  parentBatchNumber?: string;
  rootAncestorBatchId?: string;   // Original seed batch in lineage

  // Lifecycle
  currentPhase: CultivationPhase;
  phaseHistory: BatchPhaseEvent[];
  status: BatchStatus;

  // Plant tracking
  initialPlantCount: number;
  currentPlantCount: number;
  plantTags?: string[];           // Individual METRC plant tags
  plantLossEvents: PlantLossEvent[];
  totalPlantsLost: number;
  survivalRate: number;           // Calculated: current / initial

  // Location
  currentRoomId: string;
  currentRoomName: string;
  currentZoneId?: string;
  currentZoneName?: string;
  locationHistory: BatchLocationEvent[];

  // Scheduling
  startDate: string;
  expectedHarvestDate?: string;
  actualHarvestDate?: string;
  expectedDays: number;
  actualDays?: number;

  // Environment targets (can override strain defaults)
  environmentProfileId?: string;
  customTargets?: BatchEnvironmentTargets;

  // Yield tracking
  projectedYieldGrams?: number;
  actualWetWeightGrams?: number;
  actualDryWeightGrams?: number;
  yieldPerPlantGrams?: number;
  dryToWetRatio?: number;

  // Quality metrics
  qualityGrade?: 'A' | 'B' | 'C' | 'D';
  qualityNotes?: string;
  averageThcPercent?: number;
  averageCbdPercent?: number;
  averageTerpenePercent?: number;

  // Cost accumulation (WIP value)
  costs: BatchCosts;
  costPerPlant: number;
  costPerGram?: number;

  // Compliance
  metrcBatchId?: string;
  metrcPlantingId?: string;
  biotrackBatchId?: string;
  isCompliant: boolean;
  complianceNotes?: string;

  // Output - links to inventory created from this batch
  harvestEventIds: string[];
  outputLotIds: string[];         // All lots created from this batch

  // Notes
  notes?: string;
  cultivatorNotes?: string;

  // Audit
  createdAt: string;
  createdBy: string;
  updatedAt: string;
  updatedBy: string;
}

/** Batch costs breakdown */
export interface BatchCosts {
  // Direct costs
  seedCloneCost: number;
  nutrientCost: number;
  laborCost: number;
  
  // Indirect costs
  utilityCost: number;
  facilityCost: number;
  equipmentCost: number;
  overheadCost: number;
  
  // Totals
  totalDirectCost: number;
  totalIndirectCost: number;
  totalCost: number;
  
  // Allocations
  costAllocations: CostAllocation[];
}

/** Cost allocation entry */
export interface CostAllocation {
  id: string;
  batchId: string;
  costType: 'seed_clone' | 'nutrient' | 'labor' | 'utility' | 'facility' | 'equipment' | 'overhead' | 'other';
  description: string;
  amount: number;
  date: string;
  sourceDocument?: string;
  allocatedBy: string;
}

/** Phase transition event */
export interface BatchPhaseEvent {
  id: string;
  batchId: string;
  fromPhase: CultivationPhase | null;
  toPhase: CultivationPhase;
  transitionDate: string;
  expectedDate?: string;
  daysInPreviousPhase?: number;
  notes?: string;
  performedBy: string;
  
  // Environmental snapshot at transition
  environmentSnapshot?: {
    temperature: number;
    humidity: number;
    vpd: number;
    co2?: number;
    lightHours?: number;
  };
}

/** Plant loss event */
export interface PlantLossEvent {
  id: string;
  batchId: string;
  lossDate: string;
  plantCount: number;
  reason: PlantLossReason;
  reasonDetails?: string;
  plantTags?: string[];          // Specific plant tags if tracked individually
  
  // Disposition
  disposition: 'destroyed' | 'composted' | 'transferred' | 'other';
  destructionWitnessId?: string;
  destructionWitnessName?: string;
  
  // Compliance
  metrcDestructionId?: string;
  photoUrls?: string[];
  
  notes?: string;
  recordedBy: string;
  recordedAt: string;
}

/** Batch location event */
export interface BatchLocationEvent {
  id: string;
  batchId: string;
  moveDate: string;
  
  fromRoomId?: string;
  fromRoomName?: string;
  fromZoneId?: string;
  fromZoneName?: string;
  
  toRoomId: string;
  toRoomName: string;
  toZoneId?: string;
  toZoneName?: string;
  
  reason?: string;
  movedBy: string;
}

/** Environment targets override */
export interface BatchEnvironmentTargets {
  // Temperature
  dayTempF?: number;
  nightTempF?: number;
  
  // Humidity
  dayRhPercent?: number;
  nightRhPercent?: number;
  
  // VPD
  targetVpd?: number;
  vpdTolerance?: number;
  
  // CO2
  co2Ppm?: number;
  
  // Light
  ppfd?: number;
  dli?: number;
  lightHours?: number;
  
  // Irrigation
  targetEc?: number;
  targetPh?: number;
  drybackPercent?: number;
}

/** Harvest event */
export interface HarvestEvent {
  id: string;
  batchId: string;
  batch?: CultivationBatch;
  
  // Timing
  harvestDate: string;
  harvestStartTime?: string;
  harvestEndTime?: string;
  
  // What was harvested
  plantCount: number;
  plantTags?: string[];
  
  // Weights
  wetWeightGrams: number;
  estimatedDryWeightGrams?: number;
  
  // Output lots created
  outputLotIds: string[];
  outputLots?: InventoryLot[];
  
  // Location
  harvestRoomId: string;
  harvestRoomName: string;
  destinationRoomId?: string;
  destinationRoomName?: string;
  
  // Quality
  qualityGrade?: string;
  qualityNotes?: string;
  
  // Labor
  harvestedBy: string[];
  laborHours?: number;
  
  // Compliance
  metrcHarvestId?: string;
  
  notes?: string;
  createdAt: string;
  createdBy: string;
}

/** Batch filter options */
export interface BatchFilterOptions {
  siteId?: string;
  status?: BatchStatus[];
  currentPhase?: CultivationPhase[];
  strainId?: string;
  geneticId?: string;
  roomId?: string;
  originType?: BatchOriginType[];
  startDateFrom?: string;
  startDateTo?: string;
  expectedHarvestFrom?: string;
  expectedHarvestTo?: string;
  search?: string;
  hasLoss?: boolean;
}

/** Create batch request */
export interface CreateBatchRequest {
  originType: BatchOriginType;
  originLotId?: string;
  originMotherPlantId?: string;
  strainId: string;
  initialPlantCount: number;
  startDate: string;
  roomId: string;
  zoneId?: string;
  expectedHarvestDate?: string;
  environmentProfileId?: string;
  parentBatchId?: string;
  notes?: string;
}

/** Record harvest request */
export interface RecordHarvestRequest {
  batchId: string;
  harvestDate: string;
  plantCount: number;
  wetWeightGrams: number;
  destinationLocationId: string;
  harvestedBy: string[];
  plantTags?: string[];
  qualityGrade?: string;
  notes?: string;
}

/** Batch list response */
export interface BatchListResponse {
  items: CultivationBatch[];
  total: number;
  page: number;
  pageSize: number;
  hasMore: boolean;
}

/** Batch summary for dashboard */
export interface BatchSummary {
  totalBatches: number;
  activeBatches: number;
  byPhase: Record<CultivationPhase, number>;
  byStrain: { strainId: string; strainName: string; count: number }[];
  
  totalPlants: number;
  plantsLostThisWeek: number;
  averageSurvivalRate: number;
  
  upcomingHarvests: {
    batchId: string;
    batchNumber: string;
    strainName: string;
    expectedDate: string;
    plantCount: number;
    projectedYield: number;
  }[];
  
  recentHarvests: {
    batchId: string;
    batchNumber: string;
    strainName: string;
    harvestDate: string;
    wetWeight: number;
    dryWeight?: number;
  }[];
}

/** Phase configuration for display */
export const PHASE_CONFIG: Record<CultivationPhase, {
  label: string;
  description: string;
  color: string;
  bgColor: string;
  icon: string;
  typicalDays: number;
}> = {
  germination: {
    label: 'Germination',
    description: 'Seeds sprouting',
    color: 'text-amber-400',
    bgColor: 'bg-amber-500/10',
    icon: 'Sparkles',
    typicalDays: 5,
  },
  propagation: {
    label: 'Propagation',
    description: 'Clones rooting',
    color: 'text-lime-400',
    bgColor: 'bg-lime-500/10',
    icon: 'Sprout',
    typicalDays: 14,
  },
  vegetative: {
    label: 'Vegetative',
    description: 'Active growth phase',
    color: 'text-emerald-400',
    bgColor: 'bg-emerald-500/10',
    icon: 'Leaf',
    typicalDays: 28,
  },
  flowering: {
    label: 'Flowering',
    description: 'Bud development',
    color: 'text-violet-400',
    bgColor: 'bg-violet-500/10',
    icon: 'Flower2',
    typicalDays: 56,
  },
  harvest: {
    label: 'Harvest',
    description: 'Plants being cut',
    color: 'text-orange-400',
    bgColor: 'bg-orange-500/10',
    icon: 'Scissors',
    typicalDays: 1,
  },
  drying: {
    label: 'Drying',
    description: 'Moisture reduction',
    color: 'text-amber-400',
    bgColor: 'bg-amber-500/10',
    icon: 'Wind',
    typicalDays: 10,
  },
  curing: {
    label: 'Curing',
    description: 'Flavor development',
    color: 'text-cyan-400',
    bgColor: 'bg-cyan-500/10',
    icon: 'Timer',
    typicalDays: 21,
  },
  complete: {
    label: 'Complete',
    description: 'Batch finished',
    color: 'text-muted-foreground',
    bgColor: 'bg-muted/50',
    icon: 'CheckCircle',
    typicalDays: 0,
  },
};

export const LOSS_REASON_CONFIG: Record<PlantLossReason, {
  label: string;
  icon: string;
}> = {
  culling: { label: 'Culling (Quality)', icon: 'Trash2' },
  disease: { label: 'Disease', icon: 'Bug' },
  pest: { label: 'Pest Damage', icon: 'Bug' },
  environmental: { label: 'Environmental Stress', icon: 'Thermometer' },
  male_identification: { label: 'Male Identification', icon: 'AlertTriangle' },
  hermaphrodite: { label: 'Hermaphrodite', icon: 'AlertTriangle' },
  nutrient_deficiency: { label: 'Nutrient Deficiency', icon: 'Droplets' },
  light_burn: { label: 'Light Burn', icon: 'Sun' },
  root_rot: { label: 'Root Rot', icon: 'Waves' },
  mechanical_damage: { label: 'Mechanical Damage', icon: 'Hammer' },
  theft: { label: 'Theft/Loss', icon: 'ShieldAlert' },
  other: { label: 'Other', icon: 'HelpCircle' },
};

