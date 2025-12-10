/**
 * Plant Types
 * 
 * Types for plant tracking throughout the cultivation lifecycle.
 * Matches backend Plant entity and METRC compliance requirements.
 */

// =============================================================================
// ENUMS
// =============================================================================

/**
 * Plant growth phase - matches METRC plant phases
 */
export type PlantGrowthPhase =
  | 'immature'    // Clone/seedling not yet tagged individually
  | 'vegetative'  // Vegetative growth phase
  | 'flowering'   // Flowering phase (must be individually tagged)
  | 'mother'      // Mother plant used for propagation
  | 'harvested'   // Plant has been harvested
  | 'destroyed';  // Plant has been destroyed

/**
 * Plant status
 */
export type PlantStatus =
  | 'active'      // Healthy and growing
  | 'quarantined' // Isolated due to issue
  | 'harvested'   // Successfully harvested
  | 'destroyed';  // Destroyed/culled

/**
 * Destruction reason for METRC compliance
 */
export type PlantDestroyReason =
  | 'didnt_root'     // Clone didn't root
  | 'disease'        // Disease/pathogen
  | 'pest'           // Pest damage
  | 'environmental'  // Environmental stress
  | 'male'           // Male plant (must cull)
  | 'hermaphrodite'  // Hermaphrodite plant
  | 'poor_quality'   // Poor growth/quality
  | 'culled'         // Intentional culling
  | 'other';         // Other reason

/**
 * Waste disposal method
 */
export type WasteMethod =
  | 'grinder'
  | 'compost'
  | 'incinerator'
  | 'mixed_waste';

/**
 * Source type for plant creation
 */
export type PlantSourceType =
  | 'clone'            // Clone cut from mother
  | 'seed'             // Germinated seed
  | 'purchased_clone'  // Purchased immature plants
  | 'tissue_culture';  // Tissue culture propagation

// =============================================================================
// CORE INTERFACES
// =============================================================================

/**
 * Individual tagged plant - created when METRC tags are assigned
 */
export interface Plant {
  id: string;
  siteId: string;
  batchId: string;
  
  // Identification
  plantTag: string;           // METRC plant tag
  strainId: string;
  strainName: string;
  
  // Growth tracking
  growthPhase: PlantGrowthPhase;
  status: PlantStatus;
  plantedDate: string;        // ISO date
  vegetativeDate?: string;    // When entered veg
  floweringDate?: string;     // When entered flower
  
  // Location
  locationId?: string;
  roomId?: string;
  roomName?: string;
  sublocationName?: string;   // METRC sublocation
  
  // Harvest tracking
  harvestId?: string;
  harvestDate?: string;
  harvestWetWeight?: number;
  harvestWeightUnit?: string;
  
  // Destruction tracking
  destroyedDate?: string;
  destroyReason?: PlantDestroyReason;
  destroyReasonNote?: string;
  wasteWeight?: number;
  wasteWeightUnit?: string;
  wasteMethod?: WasteMethod;
  destroyedByUserId?: string;
  destroyWitnessUserId?: string;
  
  // METRC sync
  metrcPlantId?: number;
  metrcLastSyncAt?: string;
  metrcSyncStatus?: 'synced' | 'pending' | 'error';
  
  // Medical (for medical states)
  patientLicenseNumber?: string;
  
  // Metadata
  notes?: string;
  metadata?: Record<string, unknown>;
  createdAt: string;
  updatedAt: string;
  createdByUserId: string;
  updatedByUserId: string;
}

/**
 * Immature plant batch - tracks group of untagged plants
 * Plants are tracked as a group until METRC tags are assigned
 */
export interface PlantBatch {
  id: string;
  siteId: string;
  batchId: string;           // Parent cultivation batch
  
  // Identification
  name: string;              // e.g., "GSC Clone Run #47"
  strainId: string;
  strainName: string;
  
  // Counts
  initialCount: number;      // Starting plant count
  currentCount: number;      // Current live count
  destroyedCount: number;    // Total destroyed
  taggedCount: number;       // Converted to tagged plants
  
  // Source
  sourceType: PlantSourceType;
  sourceMotherBatchId?: string;    // If clones, which mother batch
  sourcePackageLabel?: string;     // If from package
  
  // Growth
  growthPhase: PlantGrowthPhase;
  plantedDate: string;
  
  // Location
  locationId?: string;
  roomId?: string;
  roomName?: string;
  
  // METRC sync
  metrcPlantBatchId?: number;
  metrcLastSyncAt?: string;
  metrcSyncStatus?: 'synced' | 'pending' | 'error';
  
  // Medical
  patientLicenseNumber?: string;
  
  // Metadata
  notes?: string;
  createdAt: string;
  updatedAt: string;
  createdByUserId: string;
}

// =============================================================================
// REQUEST/RESPONSE TYPES
// =============================================================================

/**
 * Request to start a batch with plants
 */
export interface StartBatchRequest {
  batchId: string;
  actualPlantCount: number;
  sourceType: PlantSourceType;
  sourceMotherBatchId?: string;
  sourcePackageLabel?: string;
  roomId: string;
  plantedDate?: string;       // Defaults to today
  notes?: string;
}

/**
 * Request to record plant loss
 */
export interface RecordLossRequest {
  batchId: string;
  plantBatchId?: string;      // For immature plants
  plantIds?: string[];        // For tagged plants (individual)
  quantity: number;           // For immature plants
  reason: PlantDestroyReason;
  reasonNote?: string;
  wasteWeight?: number;
  wasteWeightUnit?: string;
  wasteMethod?: WasteMethod;
  witnessUserId?: string;     // Required in some states
  destroyedDate?: string;     // Defaults to today
}

/**
 * Request to transition plants to next phase
 */
export interface TransitionPhaseRequest {
  batchId: string;
  plantBatchId?: string;      // For immature plants
  plantIds?: string[];        // For tagged plants
  fromPhase: PlantGrowthPhase;
  toPhase: PlantGrowthPhase;
  quantity?: number;          // For partial transitions
  destinationRoomId: string;
  transitionDate?: string;    // Defaults to today
}

/**
 * Request to assign METRC tags to plants
 */
export interface AssignTagsRequest {
  batchId: string;
  plantBatchId: string;       // Source immature batch
  tagStart: string;           // First tag in range
  tagEnd: string;             // Last tag in range
  quantity: number;           // Must match tag range
  roomId: string;
  assignmentDate?: string;    // Defaults to today
}

/**
 * Plant counts summary for a batch
 */
export interface PlantCounts {
  planned: number;            // Original target from batch creation
  started: number;            // Initial actual count when batch started
  current: number;            // Current live plant count
  destroyed: number;          // Total destroyed/lost
  harvested: number;          // Successfully harvested
  
  // By phase
  immature: number;
  vegetative: number;
  flowering: number;
  
  // Tagged vs untagged
  tagged: number;
  untagged: number;
}

/**
 * Plant loss record for history tracking
 */
export interface PlantLossRecord {
  id: string;
  batchId: string;
  quantity: number;
  reason: PlantDestroyReason;
  reasonNote?: string;
  phase: PlantGrowthPhase;
  recordedAt: string;
  recordedByUserId: string;
  recordedByUserName: string;
  metrcSynced: boolean;
}

// =============================================================================
// UI HELPER TYPES
// =============================================================================

/**
 * Plant action available based on batch state
 */
export type PlantAction =
  | 'start_batch'
  | 'add_plants'
  | 'record_loss'
  | 'transition_phase'
  | 'assign_tags'
  | 'view_plants';

/**
 * Get available actions based on batch state
 */
export function getAvailablePlantActions(
  batchStatus: 'planned' | 'active' | 'completed',
  currentPhase: PlantGrowthPhase,
  hasUntaggedPlants: boolean,
  hasTaggedPlants: boolean
): PlantAction[] {
  const actions: PlantAction[] = [];
  
  if (batchStatus === 'planned') {
    actions.push('start_batch');
    return actions;
  }
  
  if (batchStatus === 'active') {
    actions.push('add_plants');
    actions.push('record_loss');
    
    if (currentPhase !== 'flowering' && currentPhase !== 'harvested') {
      actions.push('transition_phase');
    }
    
    if (hasUntaggedPlants) {
      actions.push('assign_tags');
    }
    
    if (hasTaggedPlants) {
      actions.push('view_plants');
    }
  }
  
  return actions;
}

// =============================================================================
// DISPLAY HELPERS
// =============================================================================

export const GROWTH_PHASE_CONFIG: Record<PlantGrowthPhase, {
  label: string;
  color: string;
  icon: string;
}> = {
  immature: { label: 'Immature', color: '#22c55e', icon: 'Sprout' },
  vegetative: { label: 'Vegetative', color: '#3b82f6', icon: 'Leaf' },
  flowering: { label: 'Flowering', color: '#a855f7', icon: 'Flower' },
  mother: { label: 'Mother', color: '#f59e0b', icon: 'Crown' },
  harvested: { label: 'Harvested', color: '#6b7280', icon: 'Scissors' },
  destroyed: { label: 'Destroyed', color: '#ef4444', icon: 'Trash' },
};

export const DESTROY_REASON_LABELS: Record<PlantDestroyReason, string> = {
  didnt_root: "Didn't Root",
  disease: 'Disease/Pathogen',
  pest: 'Pest Damage',
  environmental: 'Environmental Stress',
  male: 'Male Plant',
  hermaphrodite: 'Hermaphrodite',
  poor_quality: 'Poor Quality',
  culled: 'Intentional Cull',
  other: 'Other',
};

export const SOURCE_TYPE_LABELS: Record<PlantSourceType, string> = {
  clone: 'Clone (from mother)',
  seed: 'Seed',
  purchased_clone: 'Purchased Immature',
  tissue_culture: 'Tissue Culture',
};




