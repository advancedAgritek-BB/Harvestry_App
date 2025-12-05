'use client';

import React, { useMemo } from 'react';
import { cn } from '@/lib/utils';
import { Sprout, Leaf, Flower2, Scissors, Package, AlertCircle } from 'lucide-react';
import { BatchPhase, PhaseType, PlannedBatch, PlannerConflict } from '../../types/planner.types';
import { PHASE_CONFIGS } from '../../constants/phaseConfig';
import { getDatePosition, getDateRangeWidth, formatDuration } from '../../utils/dateUtils';
import { ZoomLevel, DateRange } from '../../types/planner.types';
import { differenceInDays } from 'date-fns';

const PHASE_ICONS: Record<PhaseType, React.ElementType> = {
  clone: Sprout,
  veg: Leaf,
  flower: Flower2,
  harvest: Scissors,
  cure: Package,
};

interface PhaseBarProps {
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
}

export function PhaseBar({
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
}: PhaseBarProps) {
  const config = PHASE_CONFIGS[phase.phase];
  const Icon = PHASE_ICONS[phase.phase];

  // Calculate position and dimensions
  const left = useMemo(
    () => getDatePosition(phase.plannedStart, dateRange.start, zoomLevel),
    [phase.plannedStart, dateRange.start, zoomLevel]
  );

  const width = useMemo(
    () => getDateRangeWidth(phase.plannedStart, phase.plannedEnd, zoomLevel),
    [phase.plannedStart, phase.plannedEnd, zoomLevel]
  );

  const duration = useMemo(
    () => differenceInDays(phase.plannedEnd, phase.plannedStart) + 1,
    [phase.plannedStart, phase.plannedEnd]
  );

  // Don't render if completely outside viewport
  if (left + width < 0 || left > getDateRangeWidth(dateRange.start, dateRange.end, zoomLevel)) {
    return null;
  }

  // Check if we have actual dates to show comparison
  const hasActual = phase.actualStart && phase.actualEnd;
  const actualLeft = hasActual
    ? getDatePosition(phase.actualStart!, dateRange.start, zoomLevel)
    : 0;
  const actualWidth = hasActual
    ? getDateRangeWidth(phase.actualStart!, phase.actualEnd!, zoomLevel)
    : 0;

  // Determine if actual differs from planned
  const isDifferent = hasActual && (
    phase.actualStart!.getTime() !== phase.plannedStart.getTime() ||
    phase.actualEnd!.getTime() !== phase.plannedEnd.getTime()
  );

  const barHeight = rowHeight - 16; // 8px padding top and bottom

  return (
    <div
      className="absolute top-2 group"
      style={{ 
        left: `${left}px`,
        width: `${width}px`,
        height: `${barHeight}px`,
      }}
    >
      {/* Main Phase Bar */}
      <div
        className={cn(
          'absolute inset-0 rounded-lg cursor-pointer transition-all duration-200',
          'shadow-lg hover:shadow-xl transform hover:-translate-y-0.5',
          isSelected && 'ring-2 ring-foreground/50 ring-offset-1 ring-offset-background',
          hasConflict && 'ring-2 ring-red-500/70 animate-pulse'
        )}
        style={{
          background: `linear-gradient(135deg, ${config.gradientFrom} 0%, ${config.gradientTo} 100%)`,
        }}
        onClick={onSelect}
        onDoubleClick={onDoubleClick}
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
          {/* Phase Icon */}
          <Icon className="w-4 h-4 text-foreground/90 flex-shrink-0" />
          
          {/* Phase Label (show on wider bars) */}
          {width > 60 && (
            <span className="text-xs font-medium text-foreground/90 truncate">
              {config.label}
            </span>
          )}
          
          {/* Duration (show on wider bars) */}
          {width > 100 && (
            <span className="text-xs text-foreground/70 ml-auto flex-shrink-0">
              {formatDuration(duration)}
            </span>
          )}

          {/* Conflict indicator */}
          {hasConflict && (
            <div className="absolute -top-1 -right-1 w-4 h-4 bg-red-500 rounded-full flex items-center justify-center shadow-lg">
              <AlertCircle className="w-3 h-3 text-foreground" />
            </div>
          )}
        </div>

        {/* Drag handles (visible on hover) */}
        <div 
          className="absolute left-0 top-0 bottom-0 w-2 cursor-ew-resize opacity-0 group-hover:opacity-100 transition-opacity"
          style={{ background: 'linear-gradient(to right, rgba(255,255,255,0.3), transparent)' }}
        />
        <div 
          className="absolute right-0 top-0 bottom-0 w-2 cursor-ew-resize opacity-0 group-hover:opacity-100 transition-opacity"
          style={{ background: 'linear-gradient(to left, rgba(255,255,255,0.3), transparent)' }}
        />
      </div>

      {/* Actual dates overlay (if different from planned) */}
      {isDifferent && (
        <div
          className="absolute rounded-lg pointer-events-none"
          style={{
            left: `${actualLeft - left}px`,
            width: `${actualWidth}px`,
            top: '0',
            height: `${barHeight}px`,
            background: `repeating-linear-gradient(
              -45deg,
              transparent,
              transparent 3px,
              rgba(255,255,255,0.15) 3px,
              rgba(255,255,255,0.15) 6px
            )`,
            border: `2px dashed ${config.color}`,
            borderRadius: '8px',
          }}
        />
      )}

      {/* Tooltip on hover */}
      <div className="absolute left-1/2 -translate-x-1/2 -top-16 opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none z-50">
        <div className="bg-surface/95 backdrop-blur-sm border border-border rounded-lg px-3 py-2 shadow-xl min-w-[180px]">
          <div className="text-sm font-medium text-foreground mb-1">{batch.name}</div>
          <div className="text-xs text-muted-foreground space-y-0.5">
            <div className="flex justify-between">
              <span>{config.label}</span>
              <span className="text-foreground/70">{formatDuration(duration)}</span>
            </div>
            <div className="flex justify-between">
              <span>Start</span>
              <span className="text-foreground/70">{phase.plannedStart.toLocaleDateString()}</span>
            </div>
            <div className="flex justify-between">
              <span>End</span>
              <span className="text-foreground/70">{phase.plannedEnd.toLocaleDateString()}</span>
            </div>
            {hasConflict && conflict && (
              <div className="pt-1 mt-1 border-t border-border">
                <div className="flex items-center gap-1 text-red-400">
                  <AlertCircle className="w-3 h-3" />
                  <span>{conflict.message}</span>
                </div>
              </div>
            )}
          </div>
          {/* Arrow */}
          <div className="absolute left-1/2 -translate-x-1/2 -bottom-1 w-2 h-2 bg-surface border-r border-b border-border transform rotate-45" />
        </div>
      </div>
    </div>
  );
}
