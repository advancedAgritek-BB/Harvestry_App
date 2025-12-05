'use client';

import React from 'react';
import { cn } from '@/lib/utils';
import { Settings2, Wifi, WifiOff } from 'lucide-react';
import {
  MetricSensor,
  SensorMetricType,
  SensorAssignment,
  getReadingDisplayInfo,
  getAggregatedReading,
  METRIC_SENSORS,
  METRIC_CONFIGS,
} from './types';

interface ClickableMetricValueProps {
  /** The metric being displayed */
  metric: SensorMetricType;
  /** Current sensor assignment */
  assignment: SensorAssignment | null;
  /** Fallback value when no sensor is assigned */
  fallbackValue?: number;
  /** Callback when clicked to configure */
  onClick: () => void;
  /** Size variant */
  size?: 'sm' | 'md';
  /** Additional className */
  className?: string;
}

/**
 * Clickable metric value that shows current reading based on sensor assignment
 * 
 * Display logic:
 * - Runoff sensor: Shows current reading with "runoff" label
 * - Batch tank sensor: Shows current reading with batch tank name
 * - Inline sensor: Shows last event reading with "last event" label
 * - Multiple sensors: Shows aggregated value with sensor count
 */
export function ClickableMetricValue({
  metric,
  assignment,
  fallbackValue,
  onClick,
  size = 'sm',
  className,
}: ClickableMetricValueProps) {
  // Get sensor(s) from assignment
  const sensors = assignment?.sensorIds
    .map(id => METRIC_SENSORS.find(s => s.id === id))
    .filter((s): s is MetricSensor => s !== undefined) ?? [];

  // Calculate display value and label
  let displayValue: number | null = null;
  let displayLabel = '';
  let isLive = false;

  if (sensors.length === 0) {
    // No sensor assigned - use fallback
    displayValue = fallbackValue ?? null;
    displayLabel = 'not configured';
  } else if (sensors.length === 1) {
    // Single sensor - use its reading info
    const reading = getReadingDisplayInfo(sensors[0], metric);
    displayValue = reading.value;
    displayLabel = reading.label;
    isLive = reading.isLive;
  } else {
    // Multiple sensors - aggregate
    const reading = getAggregatedReading(sensors, assignment!.aggregation, metric);
    if (reading) {
      displayValue = reading.value;
      displayLabel = `avg of ${sensors.length}`;
      isLive = reading.isLive;
    }
  }

  const config = METRIC_CONFIGS[metric];
  
  const getUnit = () => {
    if (!config.unit) return '';
    return ` ${config.unit}`;
  };

  const formatValue = (val: number | null) => {
    if (val === null) return '—';
    return val.toFixed(config.decimals);
  };

  const handleClick = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    onClick();
  };

  return (
    <button
      type="button"
      onClick={handleClick}
      className={cn(
        'group flex items-center gap-1 rounded transition-all cursor-pointer',
        'hover:bg-cyan-500/10 active:scale-95',
        size === 'sm' && 'px-1.5 py-0.5 -mx-1.5',
        size === 'md' && 'px-2 py-1 -mx-2',
        className
      )}
      title={`Click to configure ${metric} sensor`}
    >
      {/* Value */}
      <span
        className={cn(
          'font-mono transition-colors',
          size === 'sm' && 'text-sm',
          size === 'md' && 'text-base font-medium',
          displayValue !== null ? 'text-foreground' : 'text-muted-foreground'
        )}
      >
        {formatValue(displayValue)}{displayValue !== null && getUnit()}
      </span>

      {/* Label/Source indicator */}
      {assignment && sensors.length > 0 && (
        <span
          className={cn(
            'text-muted-foreground opacity-70 group-hover:opacity-100 transition-opacity',
            size === 'sm' && 'text-[9px]',
            size === 'md' && 'text-[10px]'
          )}
        >
          ({displayLabel})
        </span>
      )}

      {/* Status indicator */}
      {assignment && sensors.length > 0 && (
        isLive ? (
          <Wifi className="w-2.5 h-2.5 text-emerald-400 opacity-60" />
        ) : (
          <WifiOff className="w-2.5 h-2.5 text-amber-400 opacity-60" />
        )
      )}

      {/* Config indicator on hover */}
      <Settings2
        className={cn(
          'opacity-0 group-hover:opacity-60 transition-opacity text-muted-foreground',
          size === 'sm' && 'w-3 h-3',
          size === 'md' && 'w-3.5 h-3.5'
        )}
      />
    </button>
  );
}

interface SubstrateMetricDisplayProps {
  /** Main metric (EC) */
  ecAssignment: SensorAssignment | null;
  ecFallback?: number;
  onEcClick: () => void;
  /** Sub metric (pH) */
  phAssignment: SensorAssignment | null;
  phFallback?: number;
  onPhClick: () => void;
}

/**
 * Combined display for Substrate metrics showing EC as primary and pH as secondary
 * Both values are independently clickable for sensor configuration
 */
export function SubstrateMetricDisplay({
  ecAssignment,
  ecFallback,
  onEcClick,
  phAssignment,
  phFallback,
  onPhClick,
}: SubstrateMetricDisplayProps) {
  return (
    <div className="flex flex-col gap-1">
      {/* Primary: EC - handled by main card, this is just the pH sub-value */}
      <div className="text-xs text-muted-foreground flex items-center gap-1">
        <span>pH</span>
        <ClickableMetricValue
          metric="pH"
          assignment={phAssignment}
          fallbackValue={phFallback}
          onClick={onPhClick}
          size="sm"
        />
      </div>
    </div>
  );
}

