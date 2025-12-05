'use client';

import React, { useMemo, useCallback } from 'react';
import { cn } from '@/lib/utils';
import { PlannedBatch, PlannerConflict, DateRange, ZoomLevel } from '../../types/planner.types';
import { PhaseBar } from './PhaseBar';
import { TIMELINE_CONFIG, PHASE_ORDER } from '../../constants/phaseConfig';
import { getBatchConflicts } from '../../utils/conflictDetection';

interface GanttRowProps {
  batch: PlannedBatch;
  rowIndex: number;
  dateRange: DateRange;
  zoomLevel: ZoomLevel;
  conflicts: PlannerConflict[];
  selectedBatchId: string | null;
  selectedPhaseId: string | null;
  onSelectBatch: (batchId: string) => void;
  onSelectPhase: (batchId: string, phaseId: string) => void;
  onEditPhase: (batchId: string, phaseId: string) => void;
}

export function GanttRow({
  batch,
  rowIndex,
  dateRange,
  zoomLevel,
  conflicts,
  selectedBatchId,
  selectedPhaseId,
  onSelectBatch,
  onSelectPhase,
  onEditPhase,
}: GanttRowProps) {
  const rowHeight = TIMELINE_CONFIG.rowHeight;
  
  // Get conflicts for this batch
  const batchConflicts = useMemo(
    () => getBatchConflicts(batch, conflicts),
    [batch, conflicts]
  );

  // Sort phases by lifecycle order
  const sortedPhases = useMemo(
    () => [...batch.phases].sort(
      (a, b) => PHASE_ORDER.indexOf(a.phase) - PHASE_ORDER.indexOf(b.phase)
    ),
    [batch.phases]
  );

  const handleSelectPhase = useCallback(
    (phaseId: string) => {
      onSelectPhase(batch.id, phaseId);
    },
    [batch.id, onSelectPhase]
  );

  const handleEditPhase = useCallback(
    (phaseId: string) => {
      onEditPhase(batch.id, phaseId);
    },
    [batch.id, onEditPhase]
  );

  // Find conflict for specific phase
  const getPhaseConflict = (phaseId: string): PlannerConflict | undefined => {
    return batchConflicts.find((c) => c.phaseId === phaseId);
  };

  const isSelected = selectedBatchId === batch.id;

  return (
    <div
      className={cn(
        'absolute left-0 right-0 transition-colors',
        isSelected && 'bg-muted/20'
      )}
      style={{ 
        height: `${rowHeight}px`,
        top: `${rowIndex * rowHeight}px`,
      }}
    >
      {/* Phase bars */}
      {sortedPhases.map((phase) => {
        const phaseConflict = getPhaseConflict(phase.id);
        
        return (
          <PhaseBar
            key={phase.id}
            batch={batch}
            phase={phase}
            dateRange={dateRange}
            zoomLevel={zoomLevel}
            rowHeight={rowHeight}
            isSelected={selectedPhaseId === phase.id}
            hasConflict={!!phaseConflict}
            conflict={phaseConflict}
            onSelect={() => handleSelectPhase(phase.id)}
            onDoubleClick={() => handleEditPhase(phase.id)}
          />
        );
      })}

      {/* Connection lines between phases (optional visual enhancement) */}
      {sortedPhases.length > 1 && (
        <svg 
          className="absolute inset-0 pointer-events-none overflow-visible"
          style={{ height: `${rowHeight}px` }}
        >
          {/* Phase connection lines would go here */}
        </svg>
      )}
    </div>
  );
}
