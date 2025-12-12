'use client';

import React from 'react';
import { cn } from '@/lib/utils';
import { Wifi, WifiOff, Settings2 } from 'lucide-react';
import {
  SensorAssignment,
  SensorMetricType,
  getMetricReading,
  METRIC_CONFIGS,
} from './types';

interface ClickableMetricCardProps {
  title: string;
  metric: SensorMetricType;
  assignment: SensorAssignment | null;
  fallbackValue: number;
  unit: string;
  icon: React.ReactNode;
  iconColor: string;
  status?: 'normal' | 'warning' | 'critical';
  subValue?: React.ReactNode;
  /** Optional content shown between main value and footer */
  middleValue?: React.ReactNode;
  onConfigureClick: () => void;
  /** Whether user has permission to configure sensors */
  canConfigure?: boolean;
}

export function ClickableMetricCard({
  title,
  metric,
  assignment,
  fallbackValue,
  unit,
  icon,
  iconColor,
  status = 'normal',
  subValue,
  middleValue,
  onConfigureClick,
  canConfigure = true,
}: ClickableMetricCardProps) {
  const reading = getMetricReading(metric, assignment, fallbackValue);
  const config = METRIC_CONFIGS[metric];

  return (
    <div
      className={cn(
        'flex flex-col p-4 bg-surface/50 border rounded-xl backdrop-blur-sm transition-all duration-200',
        status === 'normal' && 'border-border hover:border-border/80',
        status === 'warning' && 'border-amber-500/50 bg-amber-500/5',
        status === 'critical' && 'border-rose-500/50 bg-rose-500/5'
      )}
    >
      <div className="flex items-center justify-between mb-2">
        <span className="text-base font-bold text-muted-foreground uppercase tracking-wider">
          {title}
        </span>
        <div
          className={cn(
            'w-4 h-4',
            status === 'warning' && 'text-amber-500',
            status === 'critical' && 'text-rose-500'
          )}
          style={{ color: status === 'normal' ? iconColor : undefined }}
        >
          {icon}
        </div>
      </div>

      {/* Value - Clickable only if user has permission */}
      {canConfigure ? (
        <button
          type="button"
          onClick={(e) => {
            e.preventDefault();
            e.stopPropagation();
            onConfigureClick();
          }}
          className="flex items-baseline gap-1 group text-left hover:bg-cyan-500/10 rounded px-2 -mx-2 py-1 transition-all cursor-pointer"
          title={`Click to configure ${title} sensor`}
        >
          <span className="text-2xl font-bold text-foreground tracking-tight group-hover:text-cyan-400 transition-colors">
            {reading.value.toFixed(config.decimals)}
          </span>
          <span className="text-xs font-medium text-muted-foreground">{unit}</span>

          {/* Source label */}
          {reading.label && (
            <span className="text-[9px] text-muted-foreground opacity-70 ml-1">
              ({reading.label})
            </span>
          )}

          {/* Live indicator */}
          {assignment && assignment.sensorIds.length > 0 && (
            reading.isLive ? (
              <Wifi className="w-2.5 h-2.5 text-emerald-400 opacity-60 ml-1" />
            ) : (
              <WifiOff className="w-2.5 h-2.5 text-amber-400 opacity-60 ml-1" />
            )
          )}

          {/* Configure hint */}
          <Settings2 className="w-3 h-3 text-cyan-400 opacity-0 group-hover:opacity-60 ml-auto transition-opacity" />
        </button>
      ) : (
        <div className="flex items-baseline gap-1 px-2 -mx-2 py-1">
          <span className="text-2xl font-bold text-foreground tracking-tight">
            {reading.value.toFixed(config.decimals)}
          </span>
          <span className="text-xs font-medium text-muted-foreground">{unit}</span>

          {/* Source label */}
          {reading.label && (
            <span className="text-[9px] text-muted-foreground opacity-70 ml-1">
              ({reading.label})
            </span>
          )}

          {/* Live indicator */}
          {assignment && assignment.sensorIds.length > 0 && (
            reading.isLive ? (
              <Wifi className="w-2.5 h-2.5 text-emerald-400 opacity-60 ml-1" />
            ) : (
              <WifiOff className="w-2.5 h-2.5 text-amber-400 opacity-60 ml-1" />
            )
          )}
        </div>
      )}

      {middleValue && (
        <div className="pt-1 px-2 -mx-2 text-xs text-muted-foreground">
          {middleValue}
        </div>
      )}

      {subValue && (
        <div className="mt-auto pt-2">
          {subValue}
        </div>
      )}
    </div>
  );
}



