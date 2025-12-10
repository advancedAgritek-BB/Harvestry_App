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
  onSelectBatch?: (batchId: string) => void;
}

export function GanttChart({ children, className, onSelectBatch }: GanttChartProps) {
  const scrollContainerRef = useRef<HTMLDivElement>(null);
  const { dateRange, settings, batches, selectedBatchId, selectBatch } = usePlannerStore();

  // Handle batch selection from label click
  const handleBatchClick = useCallback((batchId: string) => {
    selectBatch(batchId);
    onSelectBatch?.(batchId);
  }, [selectBatch, onSelectBatch]);
  
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
          {batches.map((batch, index) => {
            const isSelected = selectedBatchId === batch.id;
            return (
              <div
                key={batch.id}
                onClick={() => handleBatchClick(batch.id)}
                className={cn(
                  "flex items-center px-3 border-b border-border/30 hover:bg-muted/50 transition-colors cursor-pointer",
                  isSelected && "bg-cyan-500/10 border-l-2 border-l-cyan-500"
                )}
                style={{ height: `${rowHeight}px` }}
              >
                {/* Status indicator */}
                <div 
                  className={cn(
                    "w-2 h-2 rounded-full mr-2 flex-shrink-0",
                    batch.status === 'active' && "bg-emerald-500",
                    batch.status === 'planned' && "bg-cyan-500",
                    batch.status === 'completed' && "bg-muted-foreground",
                    batch.status === 'cancelled' && "bg-red-500"
                  )}
                />
                <div className="flex-1 min-w-0">
                  <div className={cn(
                    "text-sm font-medium truncate",
                    isSelected ? "text-cyan-400" : "text-foreground"
                  )}>
                    {batch.code || batch.name}
                  </div>
                  <div className="text-xs text-muted-foreground truncate">
                    {batch.strain} · {batch.plantCount} plants
                  </div>
                </div>
              </div>
            );
          })}
          
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
