/**
 * Planner Drag Hook
 * 
 * Custom hook for handling drag-and-drop interactions in the Gantt planner
 */

import { useCallback, useState, useRef } from 'react';
import { usePlannerStore } from '../stores/plannerStore';
import { PlannedBatch, BatchPhase, ZoomLevel, DateRange } from '../types/planner.types';
import { positionToDate, snapToDay, getDayWidth } from '../utils/dateUtils';
import { calculateMoveImpact, applyMoveWithCascade } from '../utils/conflictDetection';
import { addDays, differenceInDays } from 'date-fns';

interface DragInfo {
  batchId: string;
  phaseId: string;
  startX: number;
  startDate: Date;
  originalStart: Date;
  originalEnd: Date;
  dragType: 'move' | 'resize-start' | 'resize-end';
}

export function usePlannerDrag(
  dateRange: DateRange,
  zoomLevel: ZoomLevel
) {
  const [dragInfo, setDragInfo] = useState<DragInfo | null>(null);
  const [previewDate, setPreviewDate] = useState<Date | null>(null);
  const containerRef = useRef<HTMLDivElement | null>(null);

  const {
    batches,
    startDrag,
    updateDrag,
    endDrag,
    movePhase,
    resizePhase,
    settings,
  } = usePlannerStore();

  const dayWidth = getDayWidth(zoomLevel);

  /**
   * Start a drag operation
   */
  const handleDragStart = useCallback(
    (
      batchId: string,
      phaseId: string,
      event: React.MouseEvent | React.TouchEvent,
      dragType: 'move' | 'resize-start' | 'resize-end' = 'move'
    ) => {
      const batch = batches.find((b) => b.id === batchId);
      if (!batch) return;

      const phase = batch.phases.find((p) => p.id === phaseId);
      if (!phase) return;

      const clientX = 'touches' in event ? event.touches[0].clientX : event.clientX;

      setDragInfo({
        batchId,
        phaseId,
        startX: clientX,
        startDate: phase.plannedStart,
        originalStart: phase.plannedStart,
        originalEnd: phase.plannedEnd,
        dragType,
      });

      startDrag(batchId, phaseId, phase.plannedStart, dragType);
    },
    [batches, startDrag]
  );

  /**
   * Handle drag move
   */
  const handleDragMove = useCallback(
    (event: MouseEvent | TouchEvent) => {
      if (!dragInfo) return;

      const clientX = 'touches' in event ? event.touches[0].clientX : event.clientX;
      const deltaX = clientX - dragInfo.startX;
      const deltaDays = Math.round(deltaX / dayWidth);

      if (deltaDays === 0 && dragInfo.dragType === 'move') {
        setPreviewDate(dragInfo.originalStart);
        return;
      }

      let newDate: Date;

      switch (dragInfo.dragType) {
        case 'move':
          newDate = addDays(dragInfo.originalStart, deltaDays);
          break;
        case 'resize-start':
          newDate = addDays(dragInfo.originalStart, deltaDays);
          // Ensure start doesn't go past end
          if (newDate >= dragInfo.originalEnd) {
            newDate = addDays(dragInfo.originalEnd, -1);
          }
          break;
        case 'resize-end':
          newDate = addDays(dragInfo.originalEnd, deltaDays);
          // Ensure end doesn't go before start
          if (newDate <= dragInfo.originalStart) {
            newDate = addDays(dragInfo.originalStart, 1);
          }
          break;
      }

      // Snap to day if enabled
      if (settings.snapToDay) {
        newDate = snapToDay(newDate);
      }

      setPreviewDate(newDate);
      updateDrag(newDate);
    },
    [dragInfo, dayWidth, settings.snapToDay, updateDrag]
  );

  /**
   * End drag operation
   */
  const handleDragEnd = useCallback(
    (commit: boolean = true) => {
      if (!dragInfo || !previewDate) {
        endDrag(false);
        setDragInfo(null);
        setPreviewDate(null);
        return;
      }

      if (commit) {
        const batch = batches.find((b) => b.id === dragInfo.batchId);
        if (batch) {
          switch (dragInfo.dragType) {
            case 'move':
              movePhase(dragInfo.batchId, dragInfo.phaseId, previewDate);
              break;
            case 'resize-start':
              resizePhase(
                dragInfo.batchId,
                dragInfo.phaseId,
                previewDate,
                dragInfo.originalEnd
              );
              break;
            case 'resize-end':
              resizePhase(
                dragInfo.batchId,
                dragInfo.phaseId,
                dragInfo.originalStart,
                previewDate
              );
              break;
          }
        }
      }

      endDrag(commit);
      setDragInfo(null);
      setPreviewDate(null);
    },
    [dragInfo, previewDate, batches, movePhase, resizePhase, endDrag]
  );

  /**
   * Calculate preview batch with proposed changes
   */
  const getPreviewBatch = useCallback((): PlannedBatch | null => {
    if (!dragInfo || !previewDate) return null;

    const batch = batches.find((b) => b.id === dragInfo.batchId);
    if (!batch) return null;

    if (dragInfo.dragType === 'move') {
      return applyMoveWithCascade(batch, dragInfo.phaseId, previewDate);
    }

    // For resize, just update the single phase
    const updatedPhases = batch.phases.map((p) => {
      if (p.id !== dragInfo.phaseId) return p;

      if (dragInfo.dragType === 'resize-start') {
        return { ...p, plannedStart: previewDate };
      } else {
        return { ...p, plannedEnd: previewDate };
      }
    });

    return { ...batch, phases: updatedPhases };
  }, [dragInfo, previewDate, batches]);

  /**
   * Get impact analysis for current drag
   */
  const getDragImpact = useCallback(() => {
    if (!dragInfo || !previewDate) return null;

    const batch = batches.find((b) => b.id === dragInfo.batchId);
    if (!batch) return null;

    return calculateMoveImpact(batch, dragInfo.phaseId, previewDate);
  }, [dragInfo, previewDate, batches]);

  return {
    dragInfo,
    previewDate,
    previewBatch: getPreviewBatch(),
    dragImpact: getDragImpact(),
    isDragging: !!dragInfo,
    handleDragStart,
    handleDragMove,
    handleDragEnd,
    containerRef,
  };
}

