import React from 'react';
import { cn } from '@/lib/utils';
import { Droplets, Hand, Zap, Waves, Clock, Ghost } from 'lucide-react';
import type { IrrigationDataPoint } from './types';
import { IRRIGATION_COLORS } from './types';

// Local color aliases for chart display
const COLORS = {
  manual: IRRIGATION_COLORS.manual,
  automated: IRRIGATION_COLORS.automated,
  vwc: IRRIGATION_COLORS.vwc,
  plannedVolume: IRRIGATION_COLORS.plannedVolume,
  plannedVwc: IRRIGATION_COLORS.plannedVwc,
};

interface TooltipPayload {
  dataKey?: string;
  value?: number | null;
  payload?: IrrigationDataPoint;
}

interface CustomTooltipProps {
  active?: boolean;
  payload?: TooltipPayload[];
  label?: string;
}

/**
 * Custom tooltip for irrigation chart showing shot details.
 * Handles pending VWC (null) values with appropriate messaging.
 * Distinguishes between executed (actual) and planned (scheduled) events.
 */
export function CustomTooltip({ active, payload, label }: CustomTooltipProps) {
  if (!active || !payload || !payload.length) return null;

  const dataPoint = payload[0]?.payload as IrrigationDataPoint;
  const volumeData = payload.find(p => p.dataKey === 'volume');
  const vwcData = payload.find(p => p.dataKey === 'endVwc');
  const isManual = dataPoint?.type === 'manual';
  const isPlanned = dataPoint?.status === 'planned';
  const isVwcPending = dataPoint?.vwcPending || (dataPoint?.endVwc === null && !isPlanned);

  // Determine volume bar color based on event status and type
  const volumeBarColor = isPlanned 
    ? COLORS.plannedVolume 
    : (isManual ? COLORS.manual : COLORS.automated);
  
  // Determine VWC bar color based on event status
  const vwcBarColor = isPlanned ? COLORS.plannedVwc : COLORS.vwc;

  return (
    <div className="bg-surface/95 backdrop-blur-sm border border-border rounded-xl p-3 shadow-2xl min-w-[180px]">
      {/* Header with time and status badge */}
      <div className="flex items-center gap-2 mb-3 pb-2 border-b border-border/50">
        <div className={cn(
          "w-8 h-8 rounded-lg flex items-center justify-center",
          isPlanned ? "bg-slate-500/20" : "bg-muted"
        )}>
          {isPlanned ? (
            <Ghost className="w-4 h-4 text-slate-400" />
          ) : (
            <Droplets className="w-4 h-4 text-cyan-400" />
          )}
        </div>
        <div>
          <div className="flex items-center gap-2">
            <span className="text-sm font-semibold text-foreground">{label}</span>
            {isPlanned && (
              <span className="text-[8px] font-medium px-1.5 py-0.5 rounded bg-slate-500/20 text-slate-400 uppercase tracking-wider">
                Scheduled
              </span>
            )}
          </div>
          <div className="text-[10px] text-muted-foreground uppercase tracking-wider">
            {isPlanned ? 'Planned Event' : 'Irrigation Shot'}
          </div>
        </div>
      </div>

      {/* Volume row */}
      {volumeData && volumeData.value !== 0 && (
        <div className="flex items-center justify-between mb-2">
          <div className="flex items-center gap-2">
            <div 
              className={cn("w-3 h-3 rounded-sm", isPlanned && "opacity-25")}
              style={{ backgroundColor: volumeBarColor }}
            />
            <span className="text-xs text-muted-foreground">
              {isPlanned ? 'Expected Vol' : 'Volume'}
            </span>
          </div>
          <div className="flex items-center gap-1.5">
            <span className={cn(
              "text-sm font-bold",
              isPlanned ? "text-muted-foreground" : "text-foreground"
            )}>
              {volumeData.value} mL
            </span>
            {!isPlanned && (
              <span 
                className={cn(
                  "text-[9px] font-medium px-1.5 py-0.5 rounded uppercase tracking-wider",
                  isManual ? "bg-amber-500/20 text-amber-400" : "bg-blue-500/20 text-blue-400"
                )}
              >
                {isManual ? 'Manual' : 'Auto'}
              </span>
            )}
          </div>
        </div>
      )}

      {/* VWC row - show pending state, expected, or actual value */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <div 
            className={cn("w-3 h-3 rounded-sm", isPlanned && "opacity-20")}
            style={{ backgroundColor: vwcBarColor }} 
          />
          <span className="text-xs text-muted-foreground">
            {isPlanned ? 'Expected VWC' : 'End VWC'}
          </span>
        </div>
        {isPlanned ? (
          <span className="text-sm font-bold text-muted-foreground">
            {vwcData?.value != null ? `~${vwcData.value.toFixed(1)}%` : '—'}
          </span>
        ) : isVwcPending ? (
          <div className="flex items-center gap-1 text-amber-400">
            <Clock className="w-3 h-3" />
            <span className="text-xs font-medium">Pending...</span>
          </div>
        ) : vwcData?.value != null ? (
          <span className="text-sm font-bold text-foreground">{vwcData.value.toFixed(1)}%</span>
        ) : (
          <span className="text-xs text-muted-foreground">—</span>
        )}
      </div>

      {/* Pending soak notice for executed events */}
      {isVwcPending && !isPlanned && (
        <div className="mt-2 pt-2 border-t border-border/50">
          <p className="text-[9px] text-amber-400/80 italic">
            VWC reading available after 10 min soak
          </p>
        </div>
      )}

      {/* Zone indicator */}
      {dataPoint?.zone && !isVwcPending && (
        <div className="mt-2 pt-2 border-t border-border/50">
          <span className="text-[10px] text-muted-foreground">Zone: </span>
          <span className="text-[10px] font-medium text-foreground/70">{dataPoint.zone}</span>
        </div>
      )}
    </div>
  );
}

/**
 * Legend component for irrigation chart.
 * Shows executed events and planned (ghost) events.
 */
export function ChartLegend() {
  return (
    <div className="flex items-center gap-2 sm:gap-3 px-2 flex-wrap text-[10px]">
      {/* Executed Events Group */}
      <div className="flex items-center gap-2 sm:gap-3">
        <div className="flex items-center gap-1.5">
          <div className="w-2.5 h-2.5 rounded-sm" style={{ backgroundColor: COLORS.automated }} />
          <Zap className="w-3 h-3" style={{ color: COLORS.automated }} />
          <span className="font-medium text-muted-foreground">Auto</span>
        </div>
        <div className="flex items-center gap-1.5">
          <div className="w-2.5 h-2.5 rounded-sm" style={{ backgroundColor: COLORS.manual }} />
          <Hand className="w-3 h-3" style={{ color: COLORS.manual }} />
          <span className="font-medium text-muted-foreground">Manual</span>
        </div>
        <div className="flex items-center gap-1.5">
          <div className="w-2.5 h-2.5 rounded-sm" style={{ backgroundColor: COLORS.vwc }} />
          <Waves className="w-3 h-3" style={{ color: COLORS.vwc }} />
          <span className="font-medium text-muted-foreground">VWC</span>
        </div>
      </div>

      {/* Separator */}
      <div className="w-px h-4 bg-border hidden sm:block" />

      {/* Planned Events Group */}
      <div className="flex items-center gap-2 sm:gap-3">
        <div className="flex items-center gap-1.5">
          <div className="w-2.5 h-2.5 rounded-sm opacity-20" style={{ backgroundColor: COLORS.plannedVolume }} />
          <Ghost className="w-3 h-3 opacity-40" style={{ color: COLORS.plannedVolume }} />
          <span className="font-medium text-muted-foreground">Planned</span>
        </div>
        <div className="flex items-center gap-1.5">
          <div className="w-2.5 h-2.5 rounded-sm opacity-20" style={{ backgroundColor: COLORS.plannedVwc }} />
          <span className="font-medium text-muted-foreground">Exp. VWC</span>
        </div>
      </div>
    </div>
  );
}



