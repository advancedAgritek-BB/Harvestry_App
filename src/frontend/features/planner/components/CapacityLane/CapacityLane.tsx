'use client';

import React, { useMemo } from 'react';
import { cn } from '@/lib/utils';
import { RoomCapacity, DateRange, ZoomLevel } from '../../types/planner.types';
import { getDayWidth, getDaysInRange } from '../../utils/dateUtils';
import { getCapacityVisualizationData, getCapacityStatus } from '../../utils/capacityUtils';
import { TIMELINE_CONFIG } from '../../constants/phaseConfig';

interface CapacityLaneProps {
  roomCapacity: RoomCapacity;
  dateRange: DateRange;
  zoomLevel: ZoomLevel;
  isCollapsed?: boolean;
}

export function CapacityLane({
  roomCapacity,
  dateRange,
  zoomLevel,
  isCollapsed = false,
}: CapacityLaneProps) {
  const dayWidth = getDayWidth(zoomLevel);
  const days = useMemo(() => getDaysInRange(dateRange), [dateRange]);
  const capacityData = useMemo(
    () => getCapacityVisualizationData(roomCapacity),
    [roomCapacity]
  );

  const laneHeight = TIMELINE_CONFIG.capacityLaneHeight;

  if (isCollapsed) {
    return null;
  }

  return (
    <div 
      className="relative flex bg-surface/30 border-t border-border/30"
      style={{ height: `${laneHeight}px` }}
    >
      {days.map((date, index) => {
        const dayData = capacityData.find(
          (d) => d.date.toDateString() === date.toDateString()
        );
        
        if (!dayData) {
          return (
            <div
              key={date.toISOString()}
              className="border-r border-border/20"
              style={{ width: `${dayWidth}px`, minWidth: `${dayWidth}px` }}
            />
          );
        }

        const percentage = Math.min(dayData.percentage, 1);
        const overCapacity = dayData.percentage > 1;
        const status = getCapacityStatus(dayData.percentage);

        return (
          <div
            key={date.toISOString()}
            className="relative border-r border-border/20 group"
            style={{ width: `${dayWidth}px`, minWidth: `${dayWidth}px` }}
          >
            {/* Capacity bar */}
            <div 
              className={cn(
                'absolute bottom-0 left-0 right-0 transition-all duration-150',
                overCapacity && 'animate-pulse'
              )}
              style={{ 
                height: `${percentage * 100}%`,
                backgroundColor: dayData.color,
                opacity: 0.6,
              }}
            />
            
            {/* Over-capacity indicator */}
            {overCapacity && (
              <div 
                className="absolute top-0 left-0 right-0 bg-red-500/30"
                style={{ 
                  height: `${Math.min((dayData.percentage - 1) * 100, 100)}%`,
                }}
              />
            )}

            {/* Tooltip on hover */}
            <div className="absolute bottom-full left-1/2 -translate-x-1/2 mb-1 opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none z-50">
              <div className="bg-muted border border-border rounded px-2 py-1 shadow-lg whitespace-nowrap">
                <div className="text-xs text-foreground/70">
                  {Math.round(dayData.percentage * 100)}% capacity
                </div>
                <div className="text-[10px] text-muted-foreground">
                  {date.toLocaleDateString('en-US', { 
                    weekday: 'short',
                    month: 'short', 
                    day: 'numeric' 
                  })}
                </div>
              </div>
            </div>
          </div>
        );
      })}

      {/* Capacity threshold lines */}
      <div 
        className="absolute left-0 right-0 border-t border-dashed border-amber-500/30 pointer-events-none"
        style={{ bottom: `${70}%` }}
      />
      <div 
        className="absolute left-0 right-0 border-t border-dashed border-red-500/30 pointer-events-none"
        style={{ bottom: `${90}%` }}
      />
    </div>
  );
}

/**
 * Capacity Legend component for displaying the color scale
 */
export function CapacityLegend() {
  return (
    <div className="flex items-center gap-3 text-xs text-muted-foreground">
      <span>Capacity:</span>
      <div className="flex items-center gap-1">
        <div className="w-3 h-3 rounded-sm bg-emerald-500/60" />
        <span>&lt;70%</span>
      </div>
      <div className="flex items-center gap-1">
        <div className="w-3 h-3 rounded-sm bg-yellow-500/60" />
        <span>70-90%</span>
      </div>
      <div className="flex items-center gap-1">
        <div className="w-3 h-3 rounded-sm bg-red-500/60" />
        <span>&gt;90%</span>
      </div>
    </div>
  );
}

