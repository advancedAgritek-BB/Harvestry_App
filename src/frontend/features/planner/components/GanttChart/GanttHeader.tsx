'use client';

import React, { useMemo } from 'react';
import { format, isToday, isWeekend, isSameMonth, startOfMonth } from 'date-fns';
import { cn } from '@/lib/utils';
import { DateRange, ZoomLevel } from '../../types/planner.types';
import { getDaysInRange, getDayWidth, getMonthBoundaries } from '../../utils/dateUtils';

interface GanttHeaderProps {
  dateRange: DateRange;
  zoomLevel: ZoomLevel;
}

export function GanttHeader({ dateRange, zoomLevel }: GanttHeaderProps) {
  const dayWidth = getDayWidth(zoomLevel);
  const days = useMemo(() => getDaysInRange(dateRange), [dateRange]);
  const months = useMemo(() => getMonthBoundaries(dateRange), [dateRange]);

  // Format day label based on zoom level
  const formatDayLabel = (date: Date): string => {
    switch (zoomLevel) {
      case 'day':
        return format(date, 'd');
      case 'week':
        return format(date, 'd');
      case 'month':
        return '';
    }
  };

  // Get day of week label
  const getDayOfWeek = (date: Date): string => {
    if (zoomLevel === 'day') {
      return format(date, 'EEE');
    }
    return '';
  };

  return (
    <div className="sticky top-0 z-20 bg-surface/95 backdrop-blur-sm border-b border-border/50">
      {/* Month Row */}
      <div className="flex h-8 border-b border-border/50">
        {months.map((monthStart, index) => {
          const monthDays = days.filter((d) => isSameMonth(d, monthStart));
          const width = monthDays.length * dayWidth;
          
          if (width <= 0) return null;

          return (
            <div
              key={monthStart.toISOString()}
              className="flex items-center justify-center text-xs font-medium text-muted-foreground border-r border-border/30"
              style={{ width: `${width}px`, minWidth: `${width}px` }}
            >
              {format(monthStart, 'MMMM yyyy')}
            </div>
          );
        })}
      </div>

      {/* Days Row */}
      <div className="flex h-10">
        {days.map((date, index) => {
          const isCurrentDay = isToday(date);
          const isWeekendDay = isWeekend(date);
          const isMonthStart = date.getDate() === 1;

          return (
            <div
              key={date.toISOString()}
              className={cn(
                'flex flex-col items-center justify-center text-xs border-r transition-colors',
                isCurrentDay
                  ? 'bg-cyan-500/20 text-cyan-400 font-semibold border-cyan-500/30'
                  : isWeekendDay
                  ? 'bg-muted/30 text-muted-foreground border-border/30'
                  : 'text-muted-foreground border-border/30',
                isMonthStart && 'border-l-2 border-l-border'
              )}
              style={{ width: `${dayWidth}px`, minWidth: `${dayWidth}px` }}
            >
              {zoomLevel === 'day' && (
                <span className={cn(
                  'text-[10px] uppercase tracking-wider',
                  isCurrentDay ? 'text-cyan-400' : 'text-muted-foreground/70'
                )}>
                  {getDayOfWeek(date)}
                </span>
              )}
              <span className={cn(
                isCurrentDay && 'bg-cyan-500 text-background rounded-full w-5 h-5 flex items-center justify-center text-[11px] font-bold'
              )}>
                {formatDayLabel(date)}
              </span>
            </div>
          );
        })}
      </div>
    </div>
  );
}
