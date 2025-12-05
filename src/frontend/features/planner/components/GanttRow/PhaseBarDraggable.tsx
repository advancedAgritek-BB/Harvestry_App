'use client';

import React, { useCallback, useEffect, useRef, useState } from 'react';
import { cn } from '@/lib/utils';
import { Sprout, Leaf, Flower2, Scissors, Package, AlertCircle, GripVertical } from 'lucide-react';
import { BatchPhase, PhaseType, PlannedBatch, PlannerConflict, DateRange, ZoomLevel } from '../../types/planner.types';
import { PHASE_CONFIGS } from '../../constants/phaseConfig';
import { getDatePosition, getDateRangeWidth, formatDuration, getDayWidth, snapToDay } from '../../utils/dateUtils';
import { differenceInDays, addDays } from 'date-fns';

const PHASE_ICONS: Record<PhaseType, React.ElementType> = {
  clone: Sprout,
  veg: Leaf,
  flower: Flower2,
  harvest: Scissors,
  cure: Package,
};

interface PhaseBarDraggableProps {
  batch: PlannedBatch;
  phase: BatchPhase;
  dateRange: DateRange;
  zoomLevel: ZoomLevel;
  rowHeight: number;
  isSelected: boolean;
  hasConflict: boolean;
  conflict?: PlannerConflict;
  onSelect: () => void;
  onDoubleClick: () => void;
  onDragStart: (phaseId: string, dragType: 'move' | 'resize-start' | 'resize-end') => void;
  onDragMove: (deltaX: number) => void;
  onDragEnd: (commit: boolean) => void;
  isDragging: boolean;
  previewStart?: Date;
  previewEnd?: Date;
}

export function PhaseBarDraggable({
  batch,
  phase,
  dateRange,
  zoomLevel,
  rowHeight,
  isSelected,
  hasConflict,
  conflict,
  onSelect,
  onDoubleClick,
  onDragStart,
  onDragMove,
  onDragEnd,
  isDragging,
  previewStart,
  previewEnd,
}: PhaseBarDraggableProps) {
  const config = PHASE_CONFIGS[phase.phase];
  const Icon = PHASE_ICONS[phase.phase];
  const barRef = useRef<HTMLDivElement>(null);
  const dragStartX = useRef<number>(0);
  const [localDragging, setLocalDragging] = useState(false);

  // Use preview dates if dragging, otherwise use phase dates
  const displayStart = previewStart || phase.plannedStart;
  const displayEnd = previewEnd || phase.plannedEnd;

  // Calculate position and dimensions
  const left = getDatePosition(displayStart, dateRange.start, zoomLevel);
  const width = getDateRangeWidth(displayStart, displayEnd, zoomLevel);
  const duration = differenceInDays(displayEnd, displayStart) + 1;

  // Don't render if completely outside viewport
  const viewportWidth = getDateRangeWidth(dateRange.start, dateRange.end, zoomLevel);
  if (left + width < 0 || left > viewportWidth) {
    return null;
  }

  const barHeight = rowHeight - 16;

  // Handle mouse down for drag start
  const handleMouseDown = useCallback((e: React.MouseEvent, dragType: 'move' | 'resize-start' | 'resize-end') => {
    e.preventDefault();
    e.stopPropagation();
    dragStartX.current = e.clientX;
    setLocalDragging(true);
    onDragStart(phase.id, dragType);
  }, [phase.id, onDragStart]);

  // Handle mouse move during drag
  useEffect(() => {
    if (!localDragging) return;

    const handleMouseMove = (e: MouseEvent) => {
      const deltaX = e.clientX - dragStartX.current;
      onDragMove(deltaX);
    };

    const handleMouseUp = () => {
      setLocalDragging(false);
      onDragEnd(true);
    };

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        setLocalDragging(false);
        onDragEnd(false);
      }
    };

    window.addEventListener('mousemove', handleMouseMove);
    window.addEventListener('mouseup', handleMouseUp);
    window.addEventListener('keydown', handleKeyDown);

    return () => {
      window.removeEventListener('mousemove', handleMouseMove);
      window.removeEventListener('mouseup', handleMouseUp);
      window.removeEventListener('keydown', handleKeyDown);
    };
  }, [localDragging, onDragMove, onDragEnd]);

  return (
    <div
      ref={barRef}
      className={cn(
        'absolute top-2 group',
        localDragging && 'z-50'
      )}
      style={{ 
        left: `${left}px`,
        width: `${width}px`,
        height: `${barHeight}px`,
      }}
    >
      {/* Main Phase Bar */}
      <div
        className={cn(
          'absolute inset-0 rounded-lg cursor-grab transition-all duration-150',
          'shadow-lg hover:shadow-xl',
          localDragging ? 'cursor-grabbing scale-105 shadow-2xl' : 'hover:-translate-y-0.5',
          isSelected && 'ring-2 ring-foreground/50 ring-offset-1 ring-offset-background',
          hasConflict && 'ring-2 ring-red-500/70'
        )}
        style={{
          background: `linear-gradient(135deg, ${config.gradientFrom} 0%, ${config.gradientTo} 100%)`,
        }}
        onClick={(e) => {
          e.stopPropagation();
          onSelect();
        }}
        onDoubleClick={(e) => {
          e.stopPropagation();
          onDoubleClick();
        }}
        onMouseDown={(e) => handleMouseDown(e, 'move')}
      >
        {/* Inner glow effect */}
        <div 
          className="absolute inset-0 rounded-lg opacity-30"
          style={{
            background: `linear-gradient(to bottom, rgba(255,255,255,0.3) 0%, transparent 50%, rgba(0,0,0,0.1) 100%)`,
          }}
        />

        {/* Content */}
        <div className="relative flex items-center h-full px-2 gap-1.5 overflow-hidden">
          {/* Drag Handle */}
          <GripVertical className="w-3 h-3 text-foreground/50 flex-shrink-0 cursor-grab" />
          
          {/* Phase Icon */}
          <Icon className="w-4 h-4 text-foreground/90 flex-shrink-0" />
          
          {/* Phase Label */}
          {width > 80 && (
            <span className="text-xs font-medium text-foreground/90 truncate">
              {config.label}
            </span>
          )}
          
          {/* Duration */}
          {width > 120 && (
            <span className="text-xs text-foreground/70 ml-auto flex-shrink-0">
              {formatDuration(duration)}
            </span>
          )}

          {/* Conflict indicator */}
          {hasConflict && (
            <div className="absolute -top-1 -right-1 w-4 h-4 bg-red-500 rounded-full flex items-center justify-center shadow-lg animate-pulse">
              <AlertCircle className="w-3 h-3 text-foreground" />
            </div>
          )}
        </div>

        {/* Resize handle - Left */}
        <div 
          className="absolute left-0 top-0 bottom-0 w-3 cursor-ew-resize opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center"
          onMouseDown={(e) => handleMouseDown(e, 'resize-start')}
        >
          <div className="w-1 h-8 bg-white/40 rounded-full" />
        </div>
        
        {/* Resize handle - Right */}
        <div 
          className="absolute right-0 top-0 bottom-0 w-3 cursor-ew-resize opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center"
          onMouseDown={(e) => handleMouseDown(e, 'resize-end')}
        >
          <div className="w-1 h-8 bg-white/40 rounded-full" />
        </div>
      </div>

      {/* Tooltip on hover (hide during drag) */}
      {!localDragging && (
        <div className="absolute left-1/2 -translate-x-1/2 -top-20 opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none z-50">
          <div className="bg-muted/95 backdrop-blur-sm border border-border rounded-lg px-3 py-2 shadow-xl min-w-[180px]">
            <div className="text-sm font-medium text-foreground mb-1">{batch.name}</div>
            <div className="text-xs text-muted-foreground space-y-0.5">
              <div className="flex justify-between gap-4">
                <span>{config.label}</span>
                <span className="text-foreground/70">{formatDuration(duration)}</span>
              </div>
              <div className="flex justify-between gap-4">
                <span>Start</span>
                <span className="text-foreground/70">{displayStart.toLocaleDateString()}</span>
              </div>
              <div className="flex justify-between gap-4">
                <span>End</span>
                <span className="text-foreground/70">{displayEnd.toLocaleDateString()}</span>
              </div>
              {hasConflict && conflict && (
                <div className="pt-1 mt-1 border-t border-border">
                  <div className="flex items-center gap-1 text-red-400">
                    <AlertCircle className="w-3 h-3" />
                    <span className="truncate">{conflict.message}</span>
                  </div>
                </div>
              )}
            </div>
            {/* Arrow */}
            <div className="absolute left-1/2 -translate-x-1/2 -bottom-1 w-2 h-2 bg-muted border-r border-b border-border transform rotate-45" />
          </div>
        </div>
      )}

      {/* Drag preview indicator */}
      {localDragging && (
        <div className="absolute -bottom-6 left-1/2 -translate-x-1/2 bg-cyan-500 text-foreground text-xs px-2 py-0.5 rounded shadow-lg whitespace-nowrap">
          {displayStart.toLocaleDateString('en-US', { month: 'short', day: 'numeric' })}
        </div>
      )}
    </div>
  );
}

