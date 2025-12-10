/**
 * Batch Adapter
 * 
 * Converts between CultivationBatch (source of truth) and PlannedBatch (planner display format).
 * This ensures the planner can display batches while inventory maintains the authoritative data.
 */

import { addDays, parseISO, differenceInDays } from 'date-fns';
import type { CultivationBatch, CultivationPhase } from '@/features/inventory/types/batch.types';
import type { PlannedBatch, BatchPhase, PhaseType, BatchStatus } from '../types/planner.types';
import { PHASE_DURATION_DEFAULTS } from '@/stores/batchStore';

/**
 * Map inventory phases to planner phases
 * The planner uses a simplified 5-phase model (clone, veg, flower, harvest, cure)
 * The inventory uses a more detailed model (germination, propagation, vegetative, flowering, harvest, drying, curing, complete)
 */
const PHASE_MAPPING: Record<CultivationPhase, PhaseType | null> = {
  germination: 'clone',     // Seeds/early propagation → clone phase
  propagation: 'clone',     // Cloning → clone phase
  vegetative: 'veg',
  flowering: 'flower',
  harvest: 'harvest',
  drying: 'harvest',        // Drying is part of harvest in planner
  curing: 'cure',
  complete: 'cure',         // Complete batches show in cure
};

/**
 * Map inventory status to planner status
 */
const STATUS_MAPPING: Record<CultivationBatch['status'], BatchStatus> = {
  planned: 'planned',
  active: 'active',
  harvested: 'active',      // Still in process
  processing: 'active',
  complete: 'completed',
  destroyed: 'cancelled',
  cancelled: 'cancelled',
};

/**
 * Generate phase schedule from a CultivationBatch
 * Creates the 5-phase planner schedule based on start date and phase durations
 */
function generatePlannerPhases(
  batch: CultivationBatch,
  geneticsOverrides?: {
    vegDays?: number;
    flowerDays?: number;
    cureDays?: number;
  }
): BatchPhase[] {
  const phases: BatchPhase[] = [];
  const startDate = parseISO(batch.startDate);
  
  // Use genetics overrides or defaults
  const cloneDays = PHASE_DURATION_DEFAULTS.propagation;
  const vegDays = geneticsOverrides?.vegDays ?? PHASE_DURATION_DEFAULTS.vegetative;
  const flowerDays = geneticsOverrides?.flowerDays ?? PHASE_DURATION_DEFAULTS.flowering;
  const harvestDays = PHASE_DURATION_DEFAULTS.harvest + PHASE_DURATION_DEFAULTS.drying;
  const cureDays = geneticsOverrides?.cureDays ?? PHASE_DURATION_DEFAULTS.curing;
  
  let currentDate = startDate;
  
  // Clone/Propagation phase
  phases.push({
    id: `${batch.id}-clone`,
    phase: 'clone',
    plannedStart: currentDate,
    plannedEnd: addDays(currentDate, cloneDays - 1),
    roomId: batch.currentRoomId,
    roomName: batch.currentRoomName,
  });
  currentDate = addDays(currentDate, cloneDays);
  
  // Vegetative phase
  phases.push({
    id: `${batch.id}-veg`,
    phase: 'veg',
    plannedStart: currentDate,
    plannedEnd: addDays(currentDate, vegDays - 1),
    roomId: batch.currentRoomId,
    roomName: batch.currentRoomName,
  });
  currentDate = addDays(currentDate, vegDays);
  
  // Flowering phase
  phases.push({
    id: `${batch.id}-flower`,
    phase: 'flower',
    plannedStart: currentDate,
    plannedEnd: addDays(currentDate, flowerDays - 1),
    roomId: batch.currentRoomId,
    roomName: batch.currentRoomName,
  });
  currentDate = addDays(currentDate, flowerDays);
  
  // Harvest phase (includes drying)
  phases.push({
    id: `${batch.id}-harvest`,
    phase: 'harvest',
    plannedStart: currentDate,
    plannedEnd: addDays(currentDate, harvestDays - 1),
    roomId: batch.currentRoomId,
    roomName: batch.currentRoomName,
  });
  currentDate = addDays(currentDate, harvestDays);
  
  // Cure phase
  phases.push({
    id: `${batch.id}-cure`,
    phase: 'cure',
    plannedStart: currentDate,
    plannedEnd: addDays(currentDate, cureDays - 1),
    roomId: batch.currentRoomId,
    roomName: batch.currentRoomName,
  });
  
  return phases;
}

/**
 * Convert a CultivationBatch to PlannedBatch for planner display
 */
export function cultivationBatchToPlannedBatch(
  batch: CultivationBatch,
  geneticsOverrides?: {
    vegDays?: number;
    flowerDays?: number;
    cureDays?: number;
  }
): PlannedBatch {
  const phases = generatePlannerPhases(batch, geneticsOverrides);
  
  return {
    id: batch.id,
    name: batch.name || batch.batchNumber,
    code: batch.batchNumber,
    strain: batch.strainName,
    genetics: {
      id: batch.geneticId,
      name: batch.geneticName,
      defaultVegDays: geneticsOverrides?.vegDays ?? PHASE_DURATION_DEFAULTS.vegetative,
      defaultFlowerDays: geneticsOverrides?.flowerDays ?? PHASE_DURATION_DEFAULTS.flowering,
      defaultCureDays: geneticsOverrides?.cureDays ?? PHASE_DURATION_DEFAULTS.curing,
    },
    phenotypeId: batch.phenotypeId,
    plantCount: batch.currentPlantCount,
    phases,
    status: STATUS_MAPPING[batch.status],
    createdAt: parseISO(batch.createdAt),
    updatedAt: parseISO(batch.updatedAt),
  };
}

/**
 * Convert multiple CultivationBatches to PlannedBatches
 */
export function cultivationBatchesToPlannedBatches(
  batches: CultivationBatch[],
  geneticsMap?: Map<string, { vegDays?: number; flowerDays?: number; cureDays?: number }>
): PlannedBatch[] {
  return batches.map((batch) => 
    cultivationBatchToPlannedBatch(batch, geneticsMap?.get(batch.geneticId))
  );
}

/**
 * Get current planner phase from cultivation phase
 */
export function getPlannerPhase(cultivationPhase: CultivationPhase): PhaseType {
  return PHASE_MAPPING[cultivationPhase] ?? 'veg';
}

/**
 * Get cultivation phase from planner phase
 */
export function getCultivationPhase(plannerPhase: PhaseType): CultivationPhase {
  const reverseMapping: Record<PhaseType, CultivationPhase> = {
    clone: 'propagation',
    veg: 'vegetative',
    flower: 'flowering',
    harvest: 'harvest',
    cure: 'curing',
  };
  return reverseMapping[plannerPhase];
}

/**
 * Calculate expected end date from start date and genetics
 */
export function calculateExpectedEndDate(
  startDate: Date,
  geneticsOverrides?: {
    vegDays?: number;
    flowerDays?: number;
    cureDays?: number;
  }
): Date {
  const cloneDays = PHASE_DURATION_DEFAULTS.propagation;
  const vegDays = geneticsOverrides?.vegDays ?? PHASE_DURATION_DEFAULTS.vegetative;
  const flowerDays = geneticsOverrides?.flowerDays ?? PHASE_DURATION_DEFAULTS.flowering;
  const harvestDays = PHASE_DURATION_DEFAULTS.harvest + PHASE_DURATION_DEFAULTS.drying;
  const cureDays = geneticsOverrides?.cureDays ?? PHASE_DURATION_DEFAULTS.curing;
  
  const totalDays = cloneDays + vegDays + flowerDays + harvestDays + cureDays;
  return addDays(startDate, totalDays);
}

/**
 * Extract the current phase index for progress display
 */
export function getPhaseProgress(batch: CultivationBatch): {
  currentPhaseIndex: number;
  totalPhases: number;
  percentComplete: number;
} {
  const plannerPhase = getPlannerPhase(batch.currentPhase);
  const phases: PhaseType[] = ['clone', 'veg', 'flower', 'harvest', 'cure'];
  const currentIndex = phases.indexOf(plannerPhase);
  
  // Estimate progress within current phase based on dates
  const startDate = parseISO(batch.startDate);
  const today = new Date();
  const daysSinceStart = differenceInDays(today, startDate);
  
  // Rough estimate of expected total days
  const totalExpectedDays = batch.expectedDays || 120;
  const percentComplete = Math.min(100, Math.round((daysSinceStart / totalExpectedDays) * 100));
  
  return {
    currentPhaseIndex: Math.max(0, currentIndex),
    totalPhases: phases.length,
    percentComplete,
  };
}



