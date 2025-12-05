/**
 * Conflict Detection Utilities
 * 
 * Functions for detecting scheduling conflicts and issues
 */

import {
  PlannedBatch,
  BatchPhase,
  Room,
  RoomCapacity,
  PlannerConflict,
  ConflictType,
  ChangeImpact,
  PhaseType,
} from '../types/planner.types';
import { doRangesOverlap } from './dateUtils';
import { differenceInDays } from 'date-fns';
import { PHASE_CONFIGS, PHASE_ORDER } from '../constants/phaseConfig';

/**
 * Generate a unique conflict ID
 */
function generateConflictId(): string {
  return `conflict-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
}

/**
 * Detect all conflicts for a set of batches
 */
export function detectAllConflicts(
  batches: PlannedBatch[],
  rooms: Room[],
  roomCapacities: RoomCapacity[]
): PlannerConflict[] {
  const conflicts: PlannerConflict[] = [];

  // Check capacity conflicts
  for (const roomCapacity of roomCapacities) {
    const capacityConflicts = detectCapacityConflicts(roomCapacity, batches);
    conflicts.push(...capacityConflicts);
  }

  // Check phase duration violations
  for (const batch of batches) {
    const durationConflicts = detectDurationViolations(batch);
    conflicts.push(...durationConflicts);
  }

  // Check phase sequence violations
  for (const batch of batches) {
    const sequenceConflicts = detectSequenceViolations(batch);
    conflicts.push(...sequenceConflicts);
  }

  return conflicts;
}

/**
 * Detect capacity exceeded conflicts
 */
export function detectCapacityConflicts(
  roomCapacity: RoomCapacity,
  batches: PlannedBatch[]
): PlannerConflict[] {
  const conflicts: PlannerConflict[] = [];

  for (const dayCapacity of roomCapacity.dailyOccupancy) {
    if (dayCapacity.plantCount > roomCapacity.maxCapacity) {
      const overage = dayCapacity.plantCount - roomCapacity.maxCapacity;
      
      conflicts.push({
        id: generateConflictId(),
        type: 'capacity_exceeded',
        severity: overage > roomCapacity.maxCapacity * 0.1 ? 'error' : 'warning',
        batchId: dayCapacity.batchIds[0] || '',
        roomId: roomCapacity.roomId,
        date: dayCapacity.date,
        message: `Room ${roomCapacity.roomName} over capacity by ${overage} plants on ${dayCapacity.date.toLocaleDateString()}`,
        affectedBatchIds: dayCapacity.batchIds,
      });
    }
  }

  return conflicts;
}

/**
 * Detect phase duration violations
 */
export function detectDurationViolations(batch: PlannedBatch): PlannerConflict[] {
  const conflicts: PlannerConflict[] = [];

  for (const phase of batch.phases) {
    const config = PHASE_CONFIGS[phase.phase];
    const duration = Math.ceil(
      (phase.plannedEnd.getTime() - phase.plannedStart.getTime()) / (1000 * 60 * 60 * 24)
    );

    // Check for unusually short phases (less than 50% of default)
    if (duration < config.defaultDays * 0.5) {
      conflicts.push({
        id: generateConflictId(),
        type: 'genetics_violation',
        severity: 'warning',
        batchId: batch.id,
        phaseId: phase.id,
        message: `${config.label} phase for ${batch.name} is only ${duration} days (typical: ${config.defaultDays})`,
      });
    }

    // Check for unusually long phases (more than 150% of default)
    if (duration > config.defaultDays * 1.5) {
      conflicts.push({
        id: generateConflictId(),
        type: 'genetics_violation',
        severity: 'warning',
        batchId: batch.id,
        phaseId: phase.id,
        message: `${config.label} phase for ${batch.name} is ${duration} days (typical: ${config.defaultDays})`,
      });
    }
  }

  return conflicts;
}

/**
 * Detect phase sequence violations (gaps or overlaps)
 */
export function detectSequenceViolations(batch: PlannedBatch): PlannerConflict[] {
  const conflicts: PlannerConflict[] = [];
  const sortedPhases = [...batch.phases].sort(
    (a, b) => PHASE_ORDER.indexOf(a.phase) - PHASE_ORDER.indexOf(b.phase)
  );

  for (let i = 1; i < sortedPhases.length; i++) {
    const prevPhase = sortedPhases[i - 1];
    const currentPhase = sortedPhases[i];

    // Check for gaps between phases
    const gap = Math.ceil(
      (currentPhase.plannedStart.getTime() - prevPhase.plannedEnd.getTime()) / (1000 * 60 * 60 * 24)
    );

    if (gap > 1) {
      conflicts.push({
        id: generateConflictId(),
        type: 'scheduling_conflict',
        severity: 'warning',
        batchId: batch.id,
        phaseId: currentPhase.id,
        message: `${gap} day gap between ${PHASE_CONFIGS[prevPhase.phase].label} and ${PHASE_CONFIGS[currentPhase.phase].label} for ${batch.name}`,
      });
    }

    // Check for overlapping phases
    if (gap < 0) {
      conflicts.push({
        id: generateConflictId(),
        type: 'phase_overlap',
        severity: 'error',
        batchId: batch.id,
        phaseId: currentPhase.id,
        message: `${PHASE_CONFIGS[prevPhase.phase].label} and ${PHASE_CONFIGS[currentPhase.phase].label} overlap by ${Math.abs(gap)} days for ${batch.name}`,
      });
    }
  }

  return conflicts;
}

/**
 * Calculate the impact of moving a phase
 */
export function calculateMoveImpact(
  batch: PlannedBatch,
  phaseId: string,
  newStart: Date
): ChangeImpact {
  const phaseIndex = batch.phases.findIndex((p) => p.id === phaseId);
  const phase = batch.phases[phaseIndex];
  
  if (!phase) {
    throw new Error(`Phase ${phaseId} not found in batch ${batch.id}`);
  }

  const daysDelta = Math.ceil(
    (newStart.getTime() - phase.plannedStart.getTime()) / (1000 * 60 * 60 * 24)
  );

  // Calculate cascade effects on downstream phases
  const cascadeEffects = batch.phases
    .slice(phaseIndex + 1)
    .map((p) => ({
      phaseId: p.id,
      phase: p.phase,
      daysDelta,
    }));

  return {
    batchId: batch.id,
    batchName: batch.name,
    phaseId,
    phase: phase.phase,
    originalStart: phase.plannedStart,
    newStart,
    daysDelta,
    cascadeEffects,
  };
}

/**
 * Apply cascade effects to a batch (for preview)
 */
export function applyMoveWithCascade(
  batch: PlannedBatch,
  phaseId: string,
  newStart: Date
): PlannedBatch {
  const phaseIndex = batch.phases.findIndex((p) => p.id === phaseId);
  if (phaseIndex === -1) return batch;

  const phase = batch.phases[phaseIndex];
  const duration = phase.plannedEnd.getTime() - phase.plannedStart.getTime();
  const daysDelta = newStart.getTime() - phase.plannedStart.getTime();

  const updatedPhases = batch.phases.map((p, index) => {
    if (index < phaseIndex) return p;

    const phaseDuration = p.plannedEnd.getTime() - p.plannedStart.getTime();
    const newPhaseStart = new Date(p.plannedStart.getTime() + daysDelta);
    const newPhaseEnd = new Date(newPhaseStart.getTime() + phaseDuration);

    return {
      ...p,
      plannedStart: newPhaseStart,
      plannedEnd: newPhaseEnd,
    };
  });

  return {
    ...batch,
    phases: updatedPhases,
    updatedAt: new Date(),
  };
}

/**
 * Check if a batch has any conflicts
 */
export function batchHasConflicts(
  batch: PlannedBatch,
  conflicts: PlannerConflict[]
): boolean {
  return conflicts.some(
    (c) => c.batchId === batch.id || c.affectedBatchIds?.includes(batch.id)
  );
}

/**
 * Get conflicts for a specific batch
 */
export function getBatchConflicts(
  batch: PlannedBatch,
  conflicts: PlannerConflict[]
): PlannerConflict[] {
  return conflicts.filter(
    (c) => c.batchId === batch.id || c.affectedBatchIds?.includes(batch.id)
  );
}

/**
 * Get the most severe conflict for a batch
 */
export function getMostSevereConflict(
  batch: PlannedBatch,
  conflicts: PlannerConflict[]
): PlannerConflict | null {
  const batchConflicts = getBatchConflicts(batch, conflicts);
  if (batchConflicts.length === 0) return null;

  const errors = batchConflicts.filter((c) => c.severity === 'error');
  if (errors.length > 0) return errors[0];

  return batchConflicts[0];
}

