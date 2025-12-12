'use client';

import React from 'react';
import { cn } from '@/lib/utils';

// Types
export type MetricType = 'Temp' | 'RH' | 'VWC' | 'EC' | 'PPFD';

export interface CellData {
  zoneId: string;
  zoneCode?: string;
  zoneName?: string;
  label?: string;
  value: number;
  hasAlert?: boolean;
  sensorCount?: number;
}

export interface DigitalTwinGridProps {
  rows: number;
  cols: number;
  cells: Map<string, CellData>;
  activeMetric: MetricType;
  onCellClick?: (cellKey: string, data: CellData) => void;
  selectedCell?: string | null;
  showLabels?: boolean;
  className?: string;
}

// Color scale functions for each metric
const METRIC_SCALES: Record<MetricType, { good: [number, number]; warn: [number, number] }> = {
  Temp: { good: [72, 78], warn: [70, 82] },
  RH: { good: [50, 60], warn: [45, 70] },
  VWC: { good: [35, 55], warn: [25, 65] },
  EC: { good: [1.8, 2.5], warn: [1.5, 3.0] },
  PPFD: { good: [800, 1000], warn: [600, 1200] },
};

const METRIC_UNITS: Record<MetricType, string> = {
  Temp: '°F',
  RH: '%',
  VWC: '%',
  EC: '',
  PPFD: '',
};

function getColorClass(metric: MetricType, value: number): string {
  const scale = METRIC_SCALES[metric];
  const [goodMin, goodMax] = scale.good;
  const [warnMin, warnMax] = scale.warn;

  // Within good range
  if (value >= goodMin && value <= goodMax) {
    return 'bg-emerald-500/80 text-white';
  }
  
  // Within warning range (above or below good)
  if (value >= warnMin && value <= warnMax) {
    return 'bg-amber-500/80 text-black';
  }
  
  // Outside warning range (critical)
  return 'bg-rose-500/80 text-white';
}

function getCellKey(row: number, col: number): string {
  return `1-${row}-${col}`;
}

export function DigitalTwinGrid({
  rows,
  cols,
  cells,
  activeMetric,
  onCellClick,
  selectedCell,
  showLabels = true,
  className,
}: DigitalTwinGridProps) {
  const renderCell = (row: number, col: number) => {
    const cellKey = getCellKey(row, col);
    const cellData = cells.get(cellKey);
    
    if (!cellData) {
      // Empty/unassigned cell
      return (
        <div
          key={cellKey}
          className="aspect-square rounded-lg bg-white/5 border border-border/30"
        />
      );
    }

    const colorClass = getColorClass(activeMetric, cellData.value);
    const isSelected = selectedCell === cellKey;
    const displayLabel = cellData.label || cellData.zoneCode || '';
    const unit = METRIC_UNITS[activeMetric];

    return (
      <button
        key={cellKey}
        onClick={() => onCellClick?.(cellKey, cellData)}
        className={cn(
          'relative aspect-square rounded-lg transition-all duration-200',
          'flex flex-col items-center justify-center gap-0.5 p-1',
          'hover:scale-105 hover:z-10 hover:shadow-lg',
          colorClass,
          isSelected && 'ring-2 ring-white scale-105 z-10 shadow-lg'
        )}
      >
        {/* Value */}
        <span className="font-bold text-sm tabular-nums">
          {cellData.value.toFixed(1)}{unit}
        </span>
        
        {/* Label (if showing labels and available) */}
        {showLabels && displayLabel && (
          <span className="text-[10px] opacity-80 truncate max-w-full px-1">
            {displayLabel}
          </span>
        )}

        {/* Alert Indicator */}
        {cellData.hasAlert && (
          <span className="absolute top-1 right-1 w-2 h-2 rounded-full bg-red-600 ring-1 ring-white animate-pulse" />
        )}
      </button>
    );
  };

  const gridCells: React.ReactNode[] = [];
  for (let row = 0; row < rows; row++) {
    for (let col = 0; col < cols; col++) {
      gridCells.push(renderCell(row, col));
    }
  }

  return (
    <div
      className={cn('grid gap-2', className)}
      style={{
        gridTemplateColumns: `repeat(${cols}, minmax(0, 1fr))`,
      }}
    >
      {gridCells}
    </div>
  );
}

export default DigitalTwinGrid;








