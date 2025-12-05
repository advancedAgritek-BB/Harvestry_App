'use client';

import React, { useMemo } from 'react';
import { isToday, isWeekend, getDay } from 'date-fns';
import { cn } from '@/lib/utils';
import { DateRange, ZoomLevel } from '../../types/planner.types';
import { getDaysInRange, getDayWidth, getTodayPosition } from '../../utils/dateUtils';

interface GanttGridProps {
  dateRange: DateRange;
  zoomLevel: ZoomLevel;
  rowCount: number;
  rowHeight: number;
}

export function GanttGrid({ dateRange, zoomLevel, rowCount, rowHeight }: GanttGridProps) {
  const dayWidth = getDayWidth(zoomLevel);
  const days = useMemo(() => getDaysInRange(dateRange), [dateRange]);
  const todayPosition = useMemo(() => getTodayPosition(dateRange.start, zoomLevel), [dateRange.start, zoomLevel]);
  const totalHeight = rowCount * rowHeight;

  return (
    <div 
      className="absolute inset-0 pointer-events-none"
      style={{ height: `${totalHeight}px` }}
    >
      {/* Vertical grid lines and weekend shading */}
      <div className="absolute inset-0 flex">
        {days.map((date, index) => {
          const isWeekendDay = isWeekend(date);
          const isMonday = getDay(date) === 1;
          const isFirstOfMonth = date.getDate() === 1;

          return (
            <div
              key={date.toISOString()}
              className={cn(
                'border-r',
                isWeekendDay 
                  ? 'bg-muted/20 border-border/20' 
                  : 'border-border/10',
                isMonday && 'border-l border-l-border/30',
                isFirstOfMonth && 'border-l-2 border-l-border/50'
              )}
              style={{ 
                width: `${dayWidth}px`, 
                minWidth: `${dayWidth}px`,
                height: `${totalHeight}px`
              }}
            />
          );
        })}
      </div>

      {/* Horizontal row lines */}
      <div className="absolute inset-0">
        {Array.from({ length: rowCount }).map((_, index) => (
          <div
            key={index}
            className="border-b border-border/30"
            style={{ 
              height: `${rowHeight}px`,
              top: `${index * rowHeight}px`
            }}
          />
        ))}
      </div>

      {/* Today marker */}
      {todayPosition !== null && todayPosition >= 0 && (
        <div
          className="absolute top-0 bottom-0 w-0.5 bg-cyan-500 z-10"
          style={{ left: `${todayPosition + dayWidth / 2}px` }}
        >
          {/* Today indicator dot at top */}
          <div className="absolute -top-1 left-1/2 -translate-x-1/2 w-2 h-2 rounded-full bg-cyan-500 shadow-lg shadow-cyan-500/50" />
          
          {/* Glow effect */}
          <div 
            className="absolute inset-0 w-4 -ml-[7px] bg-gradient-to-r from-transparent via-cyan-500/10 to-transparent"
            style={{ height: `${totalHeight}px` }}
          />
        </div>
      )}
    </div>
  );
}
