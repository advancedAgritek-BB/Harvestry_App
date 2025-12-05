/**
 * Impact Analysis Hook
 * 
 * Calculates and manages impact analysis for schedule changes
 */

import { useMemo, useCallback } from 'react';
import { 
  PlannedBatch, 
  BatchPhase, 
  ChangeImpact, 
  PlannerConflict,
  RoomCapacity,
  DateRange 
} from '../types/planner.types';
import { 
  calculateMoveImpact, 
  applyMoveWithCascade,
  detectAllConflicts 
} from '../utils/conflictDetection';
import { calculateAllRoomCapacities, checkCapacityImpact } from '../utils/capacityUtils';
import { usePlannerStore } from '../stores/plannerStore';
import { PHASE_ORDER } from '../constants/phaseConfig';
import { addDays, differenceInDays } from 'date-fns';

interface ImpactAnalysisResult {
  impact: ChangeImpact | null;
  previewBatch: PlannedBatch | null;
  conflicts: PlannerConflict[];
  capacityImpact: {
    roomId: string;
    conflictDays: Date[];
  }[];
  isValid: boolean;
  warnings: string[];
  errors: string[];
}

export function useImpactAnalysis() {
  const { 
    batches, 
    rooms, 
    dateRange,
    dragState 
  } = usePlannerStore();

  /**
   * Analyze impact of a proposed move
   */
  const analyzeMove = useCallback((
    batchId: string,
    phaseId: string,
    newStart: Date
  ): ImpactAnalysisResult => {
    const batch = batches.find((b) => b.id === batchId);
    if (!batch) {
      return {
        impact: null,
        previewBatch: null,
        conflicts: [],
        capacityImpact: [],
        isValid: false,
        warnings: ['Batch not found'],
        errors: [],
      };
    }

    // Calculate the impact
    const impact = calculateMoveImpact(batch, phaseId, newStart);
    
    // Create preview batch with changes applied
    const previewBatch = applyMoveWithCascade(batch, phaseId, newStart);
    
    // Replace batch in list for conflict detection
    const previewBatches = batches.map((b) => 
      b.id === batchId ? previewBatch : b
    );

    // Detect conflicts with new schedule
    const roomCapacities = calculateAllRoomCapacities(rooms, previewBatches, dateRange);
    const conflicts = detectAllConflicts(previewBatches, rooms, roomCapacities);

    // Filter to only conflicts affecting this batch
    const relevantConflicts = conflicts.filter(
      (c) => c.batchId === batchId || c.affectedBatchIds?.includes(batchId)
    );

    // Check capacity impact for each affected room
    const capacityImpact: { roomId: string; conflictDays: Date[] }[] = [];
    const affectedPhases = [
      previewBatch.phases.find((p) => p.id === phaseId),
      ...impact.cascadeEffects.map((e) => 
        previewBatch.phases.find((p) => p.id === e.phaseId)
      ),
    ].filter(Boolean) as BatchPhase[];

    for (const phase of affectedPhases) {
      const roomCapacity = roomCapacities.find((r) => r.roomId === phase.roomId);
      if (roomCapacity) {
        const result = checkCapacityImpact(previewBatch, phase.plannedStart, phase.id, roomCapacity);
        if (result.hasConflict) {
          capacityImpact.push({
            roomId: phase.roomId,
            conflictDays: result.conflictDays,
          });
        }
      }
    }

    // Categorize issues
    const warnings: string[] = [];
    const errors: string[] = [];

    for (const conflict of relevantConflicts) {
      if (conflict.severity === 'error') {
        errors.push(conflict.message);
      } else {
        warnings.push(conflict.message);
      }
    }

    // Check for minimum phase durations
    const phase = previewBatch.phases.find((p) => p.id === phaseId);
    if (phase) {
      const duration = differenceInDays(phase.plannedEnd, phase.plannedStart) + 1;
      if (duration < 1) {
        errors.push('Phase duration must be at least 1 day');
      }
    }

    // Check for phase overlap within batch
    const sortedPhases = [...previewBatch.phases].sort(
      (a, b) => PHASE_ORDER.indexOf(a.phase) - PHASE_ORDER.indexOf(b.phase)
    );
    for (let i = 1; i < sortedPhases.length; i++) {
      const prev = sortedPhases[i - 1];
      const curr = sortedPhases[i];
      if (curr.plannedStart < prev.plannedEnd) {
        errors.push(`${curr.phase} starts before ${prev.phase} ends`);
      }
    }

    return {
      impact,
      previewBatch,
      conflicts: relevantConflicts,
      capacityImpact,
      isValid: errors.length === 0,
      warnings,
      errors,
    };
  }, [batches, rooms, dateRange]);

  /**
   * Analyze impact of a resize operation
   */
  const analyzeResize = useCallback((
    batchId: string,
    phaseId: string,
    newStart: Date,
    newEnd: Date
  ): ImpactAnalysisResult => {
    const batch = batches.find((b) => b.id === batchId);
    if (!batch) {
      return {
        impact: null,
        previewBatch: null,
        conflicts: [],
        capacityImpact: [],
        isValid: false,
        warnings: ['Batch not found'],
        errors: [],
      };
    }

    // Create preview batch with resize applied
    const previewBatch: PlannedBatch = {
      ...batch,
      phases: batch.phases.map((p) => 
        p.id === phaseId 
          ? { ...p, plannedStart: newStart, plannedEnd: newEnd }
          : p
      ),
      updatedAt: new Date(),
    };

    // Detect conflicts
    const previewBatches = batches.map((b) => 
      b.id === batchId ? previewBatch : b
    );
    const roomCapacities = calculateAllRoomCapacities(rooms, previewBatches, dateRange);
    const conflicts = detectAllConflicts(previewBatches, rooms, roomCapacities);
    const relevantConflicts = conflicts.filter(
      (c) => c.batchId === batchId || c.affectedBatchIds?.includes(batchId)
    );

    const warnings: string[] = [];
    const errors: string[] = [];

    // Check duration
    const duration = differenceInDays(newEnd, newStart) + 1;
    if (duration < 1) {
      errors.push('Phase duration must be at least 1 day');
    }

    // Check for overlap with adjacent phases
    const phase = previewBatch.phases.find((p) => p.id === phaseId);
    if (phase) {
      const phaseIndex = PHASE_ORDER.indexOf(phase.phase);
      
      // Check previous phase
      const prevPhase = previewBatch.phases.find(
        (p) => PHASE_ORDER.indexOf(p.phase) === phaseIndex - 1
      );
      if (prevPhase && newStart < prevPhase.plannedEnd) {
        errors.push(`Cannot start before ${prevPhase.phase} ends`);
      }

      // Check next phase
      const nextPhase = previewBatch.phases.find(
        (p) => PHASE_ORDER.indexOf(p.phase) === phaseIndex + 1
      );
      if (nextPhase && newEnd > nextPhase.plannedStart) {
        warnings.push(`Overlaps with ${nextPhase.phase} phase`);
      }
    }

    for (const conflict of relevantConflicts) {
      if (conflict.severity === 'error') {
        errors.push(conflict.message);
      } else {
        warnings.push(conflict.message);
      }
    }

    return {
      impact: phase ? {
        batchId,
        batchName: batch.name,
        phaseId,
        phase: phase.phase,
        originalStart: phase.plannedStart,
        newStart,
        daysDelta: differenceInDays(newStart, phase.plannedStart),
        cascadeEffects: [],
      } : null,
      previewBatch,
      conflicts: relevantConflicts,
      capacityImpact: [],
      isValid: errors.length === 0,
      warnings,
      errors,
    };
  }, [batches, rooms, dateRange]);

  /**
   * Get current drag impact (if dragging)
   */
  const currentDragImpact = useMemo((): ImpactAnalysisResult | null => {
    if (!dragState.isDragging || !dragState.batchId || !dragState.phaseId || !dragState.currentStart) {
      return null;
    }

    if (dragState.dragType === 'move') {
      return analyzeMove(dragState.batchId, dragState.phaseId, dragState.currentStart);
    }

    // For resize, we'd need the end date too
    return null;
  }, [dragState, analyzeMove]);

  return {
    analyzeMove,
    analyzeResize,
    currentDragImpact,
  };
}

