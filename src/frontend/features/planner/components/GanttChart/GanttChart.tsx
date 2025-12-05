'use client';

import React, { useRef, useCallback, useMemo } from 'react';
import { cn } from '@/lib/utils';
import { usePlannerStore } from '../../stores/plannerStore';
import { GanttHeader } from './GanttHeader';
import { GanttGrid } from './GanttGrid';
import { getTimelineWidth, getDayWidth } from '../../utils/dateUtils';
import { TIMELINE_CONFIG } from '../../constants/phaseConfig';

interface GanttChartProps {
  children?: React.ReactNode;
  className?: string;
}

export function GanttChart({ children, className }: GanttChartProps) {
  const scrollContainerRef = useRef<HTMLDivElement>(null);
  const { dateRange, settings, batches } = usePlannerStore();
  
  const timelineWidth = useMemo(
    () => getTimelineWidth(dateRange, settings.zoomLevel),
    [dateRange, settings.zoomLevel]
  );
  
  const dayWidth = getDayWidth(settings.zoomLevel);
  const rowHeight = TIMELINE_CONFIG.rowHeight;
  
  // Calculate total height based on batches
  const rowCount = Math.max(batches.length, 10); // Minimum 10 rows for visual effect
  const contentHeight = rowCount * rowHeight;

  // Handle horizontal scroll
  const handleScroll = useCallback((e: React.UIEvent<HTMLDivElement>) => {
    // Could sync scroll with other components if needed
  }, []);

  return (
    <div className={cn('flex flex-col h-full bg-background rounded-xl overflow-hidden border border-border', className)}>
      {/* Fixed Header */}
      <div className="flex-shrink-0">
        <div 
          className="overflow-hidden"
          style={{ marginLeft: '200px' }} // Space for batch labels
        >
          <div 
            ref={scrollContainerRef}
            className="overflow-x-auto scrollbar-thin"
            onScroll={handleScroll}
          >
            <div style={{ width: `${timelineWidth}px` }}>
              <GanttHeader dateRange={dateRange} zoomLevel={settings.zoomLevel} />
            </div>
          </div>
        </div>
      </div>

      {/* Scrollable Content Area */}
      <div className="flex-1 flex overflow-hidden">
        {/* Batch Labels Column (Fixed) */}
        <div 
          className="flex-shrink-0 w-[200px] bg-surface/50 border-r border-border overflow-y-auto"
          style={{ height: `${contentHeight}px` }}
        >
          {batches.map((batch, index) => (
            <div
              key={batch.id}
              className="flex items-center px-3 border-b border-border/30 hover:bg-muted/30 transition-colors cursor-pointer"
              style={{ height: `${rowHeight}px` }}
            >
              <div className="flex-1 min-w-0">
                <div className="text-sm font-medium text-foreground truncate">
                  {batch.name}
                </div>
                <div className="text-xs text-muted-foreground truncate">
                  {batch.strain} · {batch.plantCount} plants
                </div>
              </div>
            </div>
          ))}
          
          {/* Empty rows for visual consistency */}
          {Array.from({ length: Math.max(0, 10 - batches.length) }).map((_, i) => (
            <div
              key={`empty-${i}`}
              className="border-b border-border/20"
              style={{ height: `${rowHeight}px` }}
            />
          ))}
        </div>

        {/* Timeline Content (Scrollable) */}
        <div 
          className="flex-1 overflow-auto scrollbar-thin"
          onScroll={handleScroll}
        >
          <div 
            className="relative"
            style={{ 
              width: `${timelineWidth}px`,
              height: `${contentHeight}px`,
              minHeight: '100%'
            }}
          >
            {/* Background Grid */}
            <GanttGrid 
              dateRange={dateRange} 
              zoomLevel={settings.zoomLevel}
              rowCount={rowCount}
              rowHeight={rowHeight}
            />
            
            {/* Batch Rows (Phase Bars) */}
            <div className="relative z-10">
              {children}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
