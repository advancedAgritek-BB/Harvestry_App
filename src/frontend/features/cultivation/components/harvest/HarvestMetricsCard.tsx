'use client';

/**
 * HarvestMetricsCard Component
 * Displays calculated harvest metrics with visual indicators
 */

import { cn } from '@/lib/utils';
import type { HarvestMetrics } from '@/features/inventory/types';

interface HarvestMetricsCardProps {
  /** Harvest metrics data */
  metrics: HarvestMetrics;
  /** Unit of measurement */
  uom?: string;
  /** Show detailed breakdown */
  showDetails?: boolean;
  /** Additional class names */
  className?: string;
}

interface MetricItemProps {
  label: string;
  value: string | number;
  suffix?: string;
  color?: string;
  subValue?: string;
}

function MetricItem({ label, value, suffix = '', color, subValue }: MetricItemProps) {
  return (
    <div className="flex flex-col">
      <span className="text-xs text-muted-foreground">{label}</span>
      <span className={cn('text-lg font-mono font-semibold', color)}>
        {typeof value === 'number' ? value.toFixed(1) : value}
        {suffix && <span className="text-sm font-normal text-muted-foreground ml-0.5">{suffix}</span>}
      </span>
      {subValue && (
        <span className="text-xs text-muted-foreground">{subValue}</span>
      )}
    </div>
  );
}

export function HarvestMetricsCard({
  metrics,
  uom = 'g',
  showDetails = true,
  className,
}: HarvestMetricsCardProps) {
  // Determine color based on value quality
  const getMoistureLossColor = (percent: number | undefined) => {
    if (percent === undefined) return 'text-muted-foreground';
    if (percent >= 70 && percent <= 80) return 'text-emerald-400'; // Ideal range
    if (percent >= 65 && percent <= 85) return 'text-amber-400'; // Acceptable
    return 'text-rose-400'; // Outside normal range
  };

  const getUsableFlowerColor = (percent: number | undefined) => {
    if (percent === undefined) return 'text-muted-foreground';
    if (percent >= 70) return 'text-emerald-400'; // Good yield
    if (percent >= 60) return 'text-amber-400'; // Average
    return 'text-rose-400'; // Low yield
  };

  const getWasteColor = (percent: number | undefined) => {
    if (percent === undefined) return 'text-muted-foreground';
    if (percent <= 15) return 'text-emerald-400'; // Low waste
    if (percent <= 25) return 'text-amber-400'; // Normal
    return 'text-rose-400'; // High waste
  };

  return (
    <div className={cn('bg-card/50 rounded-lg border border-border/50', className)}>
      {/* Header */}
      <div className="px-4 py-3 border-b border-border/50">
        <h3 className="text-sm font-medium">Harvest Metrics</h3>
      </div>
      
      <div className="p-4 space-y-4">
        {/* Primary weights */}
        <div className="grid grid-cols-2 gap-4">
          <MetricItem
            label="Wet Weight"
            value={metrics.wetWeight}
            suffix={uom}
            color="text-cyan-400"
          />
          <MetricItem
            label="Dry Weight"
            value={metrics.buckedFlowerWeight || metrics.dryWeight}
            suffix={uom}
            color="text-amber-400"
            subValue={metrics.buckedFlowerWeight > 0 ? 'Bucked flower' : undefined}
          />
        </div>
        
        {/* Key ratios */}
        <div className="grid grid-cols-3 gap-4 pt-2 border-t border-border/30">
          <MetricItem
            label="Moisture Loss"
            value={metrics.moistureLossPercent ?? '—'}
            suffix={metrics.moistureLossPercent !== undefined ? '%' : ''}
            color={getMoistureLossColor(metrics.moistureLossPercent)}
          />
          <MetricItem
            label="Dry:Wet Ratio"
            value={metrics.dryToWetRatio ? (metrics.dryToWetRatio * 100).toFixed(0) : '—'}
            suffix={metrics.dryToWetRatio !== undefined ? '%' : ''}
            color="text-foreground"
          />
          <MetricItem
            label="Usable Flower"
            value={metrics.usableFlowerPercent ?? '—'}
            suffix={metrics.usableFlowerPercent !== undefined ? '%' : ''}
            color={getUsableFlowerColor(metrics.usableFlowerPercent)}
          />
        </div>
        
        {/* Waste breakdown */}
        {showDetails && (
          <div className="pt-2 border-t border-border/30">
            <div className="flex items-center justify-between mb-2">
              <span className="text-xs text-muted-foreground">Waste Breakdown</span>
              <span className={cn('text-sm font-mono', getWasteColor(metrics.wastePercent))}>
                {metrics.totalWasteWeight.toFixed(1)}{uom}
                {metrics.wastePercent !== undefined && (
                  <span className="text-xs ml-1">({metrics.wastePercent.toFixed(1)}%)</span>
                )}
              </span>
            </div>
            
            {/* Waste bar chart */}
            <div className="h-4 bg-muted rounded overflow-hidden flex">
              {metrics.stemWaste > 0 && (
                <div 
                  className="bg-amber-500/60 h-full"
                  style={{ width: `${(metrics.stemWaste / metrics.totalWasteWeight) * 100}%` }}
                  title={`Stems: ${metrics.stemWaste.toFixed(1)}${uom}`}
                />
              )}
              {metrics.leafWaste > 0 && (
                <div 
                  className="bg-emerald-500/60 h-full"
                  style={{ width: `${(metrics.leafWaste / metrics.totalWasteWeight) * 100}%` }}
                  title={`Leaves: ${metrics.leafWaste.toFixed(1)}${uom}`}
                />
              )}
              {metrics.otherWaste > 0 && (
                <div 
                  className="bg-slate-500/60 h-full"
                  style={{ width: `${(metrics.otherWaste / metrics.totalWasteWeight) * 100}%` }}
                  title={`Other: ${metrics.otherWaste.toFixed(1)}${uom}`}
                />
              )}
            </div>
            
            {/* Legend */}
            <div className="flex gap-4 mt-2 text-xs">
              <div className="flex items-center gap-1">
                <span className="w-2 h-2 rounded bg-amber-500/60" />
                <span className="text-muted-foreground">Stems {metrics.stemWaste.toFixed(1)}{uom}</span>
              </div>
              <div className="flex items-center gap-1">
                <span className="w-2 h-2 rounded bg-emerald-500/60" />
                <span className="text-muted-foreground">Leaves {metrics.leafWaste.toFixed(1)}{uom}</span>
              </div>
              <div className="flex items-center gap-1">
                <span className="w-2 h-2 rounded bg-slate-500/60" />
                <span className="text-muted-foreground">Other {metrics.otherWaste.toFixed(1)}{uom}</span>
              </div>
            </div>
          </div>
        )}
        
        {/* Yield per plant */}
        {metrics.yieldPerPlant !== undefined && metrics.yieldPerPlant > 0 && (
          <div className="pt-2 border-t border-border/30 flex justify-between items-center">
            <span className="text-xs text-muted-foreground">Yield per Plant</span>
            <span className="text-lg font-mono font-semibold text-violet-400">
              {metrics.yieldPerPlant.toFixed(1)}
              <span className="text-sm font-normal text-muted-foreground ml-0.5">{uom}</span>
            </span>
          </div>
        )}
      </div>
    </div>
  );
}

export default HarvestMetricsCard;





