'use client';

import React, { useMemo, useCallback, useState, useEffect } from 'react';
import { cn } from '@/lib/utils';
import { PlannedBatch, PlannerConflict, DateRange, ZoomLevel, BatchPhase } from '../../types/planner.types';
import { PhaseBarDraggable } from './PhaseBarDraggable';
import { PhaseAdjustmentConfirmDialog } from './PhaseAdjustmentConfirmDialog';
import { TIMELINE_CONFIG, PHASE_ORDER, PHASE_CONFIGS } from '../../constants/phaseConfig';
import { getBatchConflicts } from '../../utils/conflictDetection';
import { getDayWidth } from '../../utils/dateUtils';
import { addDays, differenceInDays } from 'date-fns';

interface PendingAdjustment {
  phaseId: string;
  phaseName: string;
  phaseType: BatchPhase['phase'];
  originalStart: Date;
  originalEnd: Date;
  newStart: Date;
  newEnd: Date;
  daysDelta: number;
  dragType: 'move' | 'resize-start' | 'resize-end';
}

interface GanttRowDraggableProps {
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
  onMovePhase: (batchId: string, phaseId: string, newStart: Date) => void;
  onResizePhase: (batchId: string, phaseId: string, newStart: Date, newEnd: Date) => void;
  requireConfirmation?: boolean;
}

export function GanttRowDraggable({
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
  onMovePhase,
  onResizePhase,
  requireConfirmation = true,
}: GanttRowDraggableProps) {
  const rowHeight = TIMELINE_CONFIG.rowHeight;
  const dayWidth = getDayWidth(zoomLevel);
  
  // Drag state
  const [draggingPhaseId, setDraggingPhaseId] = useState<string | null>(null);
  const [dragType, setDragType] = useState<'move' | 'resize-start' | 'resize-end' | null>(null);
  const [dragStartX, setDragStartX] = useState<number>(0);
  const [dragOriginalStart, setDragOriginalStart] = useState<Date | null>(null);
  const [dragOriginalEnd, setDragOriginalEnd] = useState<Date | null>(null);
  const [previewStart, setPreviewStart] = useState<Date | null>(null);
  const [previewEnd, setPreviewEnd] = useState<Date | null>(null);
  
  // Confirmation dialog state
  const [showConfirmDialog, setShowConfirmDialog] = useState(false);
  const [pendingAdjustment, setPendingAdjustment] = useState<PendingAdjustment | null>(null);
  
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

  // Handle drag start
  const handleDragStart = useCallback((phaseId: string, type: 'move' | 'resize-start' | 'resize-end') => {
    const phase = batch.phases.find(p => p.id === phaseId);
    if (!phase) return;

    setDraggingPhaseId(phaseId);
    setDragType(type);
    setDragOriginalStart(phase.plannedStart);
    setDragOriginalEnd(phase.plannedEnd);
    setPreviewStart(phase.plannedStart);
    setPreviewEnd(phase.plannedEnd);
  }, [batch.phases]);

  // Handle drag move
  const handleDragMove = useCallback((deltaX: number) => {
    if (!draggingPhaseId || !dragOriginalStart || !dragOriginalEnd || !dragType) return;

    const deltaDays = Math.round(deltaX / dayWidth);
    
    let newStart: Date;
    let newEnd: Date;

    switch (dragType) {
      case 'move':
        newStart = addDays(dragOriginalStart, deltaDays);
        newEnd = addDays(dragOriginalEnd, deltaDays);
        break;
      case 'resize-start':
        newStart = addDays(dragOriginalStart, deltaDays);
        newEnd = dragOriginalEnd;
        // Ensure minimum 1 day duration
        if (newStart >= newEnd) {
          newStart = addDays(newEnd, -1);
        }
        break;
      case 'resize-end':
        newStart = dragOriginalStart;
        newEnd = addDays(dragOriginalEnd, deltaDays);
        // Ensure minimum 1 day duration
        if (newEnd <= newStart) {
          newEnd = addDays(newStart, 1);
        }
        break;
      default:
        return;
    }

    setPreviewStart(newStart);
    setPreviewEnd(newEnd);
  }, [draggingPhaseId, dragOriginalStart, dragOriginalEnd, dragType, dayWidth]);

  // Handle drag end
  const handleDragEnd = useCallback((commit: boolean) => {
    if (!draggingPhaseId || !dragOriginalStart || !dragOriginalEnd || !previewStart || !previewEnd || !dragType) {
      resetDragState();
      return;
    }

    if (!commit) {
      resetDragState();
      return;
    }

    // Check if there's actually a change
    const hasChanged = 
      previewStart.getTime() !== dragOriginalStart.getTime() ||
      previewEnd.getTime() !== dragOriginalEnd.getTime();

    if (!hasChanged) {
      resetDragState();
      return;
    }

    const phase = batch.phases.find(p => p.id === draggingPhaseId);
    if (!phase) {
      resetDragState();
      return;
    }

    const daysDelta = differenceInDays(previewStart, dragOriginalStart);
    
    const adjustment: PendingAdjustment = {
      phaseId: draggingPhaseId,
      phaseName: PHASE_CONFIGS[phase.phase].label,
      phaseType: phase.phase,
      originalStart: dragOriginalStart,
      originalEnd: dragOriginalEnd,
      newStart: previewStart,
      newEnd: previewEnd,
      daysDelta,
      dragType,
    };

    if (requireConfirmation) {
      // Show confirmation dialog
      setPendingAdjustment(adjustment);
      setShowConfirmDialog(true);
    } else {
      // Apply immediately
      applyAdjustment(adjustment);
    }
  }, [draggingPhaseId, dragOriginalStart, dragOriginalEnd, previewStart, previewEnd, dragType, batch.phases, requireConfirmation]);

  // Apply the adjustment
  const applyAdjustment = useCallback((adjustment: PendingAdjustment) => {
    switch (adjustment.dragType) {
      case 'move':
        onMovePhase(batch.id, adjustment.phaseId, adjustment.newStart);
        break;
      case 'resize-start':
      case 'resize-end':
        onResizePhase(batch.id, adjustment.phaseId, adjustment.newStart, adjustment.newEnd);
        break;
    }
    resetDragState();
    setShowConfirmDialog(false);
    setPendingAdjustment(null);
  }, [batch.id, onMovePhase, onResizePhase]);

  // Reset drag state
  const resetDragState = () => {
    setDraggingPhaseId(null);
    setDragType(null);
    setDragStartX(0);
    setDragOriginalStart(null);
    setDragOriginalEnd(null);
    setPreviewStart(null);
    setPreviewEnd(null);
  };

  // Handle confirmation
  const handleConfirm = useCallback(() => {
    if (pendingAdjustment) {
      applyAdjustment(pendingAdjustment);
    }
  }, [pendingAdjustment, applyAdjustment]);

  // Handle cancel
  const handleCancel = useCallback(() => {
    setShowConfirmDialog(false);
    setPendingAdjustment(null);
    resetDragState();
  }, []);

  // Check for conflicts with the pending adjustment
  const pendingConflict = useMemo(() => {
    if (!pendingAdjustment) return null;
    // Check if the new dates would cause a conflict
    const phaseConflict = getPhaseConflict(pendingAdjustment.phaseId);
    return phaseConflict;
  }, [pendingAdjustment, batchConflicts]);

  return (
    <>
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
          const isDragging = draggingPhaseId === phase.id;
          
          // Calculate preview dates for this phase
          const displayPreviewStart = isDragging ? previewStart : undefined;
          const displayPreviewEnd = isDragging ? previewEnd : undefined;
          
          return (
            <PhaseBarDraggable
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
              onDragStart={(phaseId, dragType) => handleDragStart(phaseId, dragType)}
              onDragMove={handleDragMove}
              onDragEnd={handleDragEnd}
              isDragging={isDragging}
              previewStart={displayPreviewStart || undefined}
              previewEnd={displayPreviewEnd || undefined}
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

      {/* Confirmation Dialog */}
      {pendingAdjustment && (
        <PhaseAdjustmentConfirmDialog
          isOpen={showConfirmDialog}
          onClose={handleCancel}
          onConfirm={handleConfirm}
          batchName={batch.name}
          adjustment={pendingAdjustment}
          hasConflicts={!!pendingConflict}
          conflictMessage={pendingConflict?.message}
        />
      )}
    </>
  );
}


