'use client';

import React, { useState, useMemo, useEffect } from 'react';
import { createPortal } from 'react-dom';
import { cn } from '@/lib/utils';
import { X, Check, Wifi, WifiOff } from 'lucide-react';
import {
  MetricSensor,
  SensorMetricType,
  SensorAssignment,
  getMetricSensorsByZone,
  getSensorPlacementLabel,
  METRIC_CONFIGS,
} from './types';

interface SensorSelectorModalProps {
  isOpen: boolean;
  onClose: () => void;
  title: string;
  description?: string;
  /** Metric type to filter sensors by */
  metric: SensorMetricType;
  /** Currently selected sensor assignment */
  currentAssignment: SensorAssignment | null;
  /** Callback when selection is saved */
  onSave: (assignment: SensorAssignment | null) => void;
  /** Allow selecting multiple sensors */
  allowMultiple?: boolean;
}

export function SensorSelectorModal({
  isOpen,
  onClose,
  title,
  description,
  metric,
  currentAssignment,
  onSave,
  allowMultiple = true,
}: SensorSelectorModalProps) {
  const [selectedIds, setSelectedIds] = useState<string[]>(
    currentAssignment?.sensorIds ?? []
  );
  const [aggregation, setAggregation] = useState<SensorAssignment['aggregation']>(
    currentAssignment?.aggregation ?? 'average'
  );

  // Reset state when modal opens or currentAssignment changes
  useEffect(() => {
    if (isOpen) {
      setSelectedIds(currentAssignment?.sensorIds ?? []);
      setAggregation(currentAssignment?.aggregation ?? 'average');
    }
  }, [isOpen, currentAssignment]);

  // Handle escape key to close modal
  useEffect(() => {
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && isOpen) {
        onClose();
      }
    };

    if (isOpen) {
      document.addEventListener('keydown', handleEscape);
      document.body.style.overflow = 'hidden';
    }

    return () => {
      document.removeEventListener('keydown', handleEscape);
      document.body.style.overflow = 'unset';
    };
  }, [isOpen, onClose]);

  const sensorsByZone = useMemo(() => getMetricSensorsByZone(metric), [metric]);

  const toggleSensor = (sensorId: string) => {
    if (!allowMultiple) {
      setSelectedIds([sensorId]);
      return;
    }
    
    if (selectedIds.includes(sensorId)) {
      setSelectedIds(selectedIds.filter(id => id !== sensorId));
    } else {
      setSelectedIds([...selectedIds, sensorId]);
    }
  };

  const selectAllInZone = (zone: string) => {
    const zoneIds = sensorsByZone[zone].map(s => s.id);
    const allSelected = zoneIds.every(id => selectedIds.includes(id));
    
    if (allSelected) {
      setSelectedIds(selectedIds.filter(id => !zoneIds.includes(id)));
    } else {
      // Merge arrays and deduplicate
      const merged = [...selectedIds];
      zoneIds.forEach(id => {
        if (!merged.includes(id)) {
          merged.push(id);
        }
      });
      setSelectedIds(merged);
    }
  };

  const handleSave = () => {
    if (selectedIds.length === 0) {
      onSave(null);
    } else {
      onSave({ sensorIds: selectedIds, aggregation });
    }
    onClose();
  };

  const handleClear = () => {
    onSave(null);
    onClose();
  };

  // Don't render on server or when closed
  if (!isOpen || typeof window === 'undefined') return null;

  const modalContent = (
    <div className="fixed inset-0 z-[9999] flex items-center justify-center p-4">
      {/* Backdrop */}
      <div
        className="absolute inset-0 bg-background/60 backdrop-blur-sm"
        onClick={onClose}
      />

      {/* Modal */}
      <div className="relative w-full max-w-lg bg-surface border border-border rounded-xl shadow-2xl z-10">
        {/* Header */}
        <div className="flex items-start justify-between px-5 py-4 border-b border-border">
          <div>
            <h2 className="text-lg font-semibold text-foreground">{title}</h2>
            {description && (
              <p className="text-sm text-muted-foreground mt-1">{description}</p>
            )}
          </div>
          <button
            type="button"
            onClick={onClose}
            className="p-1.5 rounded-lg text-muted-foreground hover:text-foreground hover:bg-muted transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Content */}
        <div className="px-5 py-4 max-h-[50vh] overflow-y-auto space-y-4">
          {/* Selected count indicator */}
          {selectedIds.length > 0 && (
            <div className="flex items-center justify-between p-2 bg-cyan-500/10 border border-cyan-500/30 rounded-lg">
              <span className="text-sm text-cyan-400">
                {selectedIds.length} sensor{selectedIds.length > 1 ? 's' : ''} selected
              </span>
              {allowMultiple && selectedIds.length > 1 && (
                <select
                  value={aggregation}
                  onChange={(e) => setAggregation(e.target.value as SensorAssignment['aggregation'])}
                  className="h-7 px-2 bg-surface border border-border rounded text-xs text-foreground"
                >
                  <option value="average">Average</option>
                  <option value="min">Minimum</option>
                  <option value="max">Maximum</option>
                </select>
              )}
            </div>
          )}

          {/* Sensor list by zone */}
          {Object.entries(sensorsByZone).map(([zone, sensors]) => (
            <div key={zone} className="space-y-2">
              {/* Zone header with select all */}
              {allowMultiple && (
                <button
                  type="button"
                  onClick={() => selectAllInZone(zone)}
                  className="flex items-center gap-2 text-xs font-semibold text-muted-foreground hover:text-foreground uppercase tracking-wider transition-colors"
                >
                  <span
                    className={cn(
                      'w-3.5 h-3.5 rounded border flex items-center justify-center',
                      sensors.every(s => selectedIds.includes(s.id))
                        ? 'bg-cyan-500 border-cyan-500'
                        : sensors.some(s => selectedIds.includes(s.id))
                        ? 'bg-cyan-500/50 border-cyan-500'
                        : 'border-border'
                    )}
                  >
                    {sensors.every(s => selectedIds.includes(s.id)) && (
                      <Check className="w-2.5 h-2.5 text-white" />
                    )}
                  </span>
                  {zone}
                </button>
              )}
              {!allowMultiple && (
                <div className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">
                  {zone}
                </div>
              )}

              {/* Sensor list */}
              <div className="space-y-1 ml-1">
                {sensors.map(sensor => (
                  <SensorRow
                    key={sensor.id}
                    sensor={sensor}
                    metric={metric}
                    isSelected={selectedIds.includes(sensor.id)}
                    onToggle={() => toggleSensor(sensor.id)}
                    showRadio={!allowMultiple}
                  />
                ))}
              </div>
            </div>
          ))}

          {Object.keys(sensorsByZone).length === 0 && (
            <div className="text-center py-8 text-muted-foreground">
              No {metric} sensors available
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="flex items-center justify-between gap-3 px-5 py-4 border-t border-border">
          <button
            type="button"
            onClick={handleClear}
            className="px-3 py-2 text-sm text-muted-foreground hover:text-foreground transition-colors"
          >
            Clear Selection
          </button>
          <div className="flex gap-2">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-sm text-muted-foreground hover:text-foreground border border-border rounded-lg transition-colors"
            >
              Cancel
            </button>
            <button
              type="button"
              onClick={handleSave}
              className="px-4 py-2 text-sm font-medium text-white bg-cyan-600 hover:bg-cyan-500 rounded-lg transition-colors"
            >
              Save
            </button>
          </div>
        </div>
      </div>
    </div>
  );

  // Use portal to render at document body level
  return createPortal(modalContent, document.body);
}

interface SensorRowProps {
  sensor: MetricSensor;
  metric: SensorMetricType;
  isSelected: boolean;
  onToggle: () => void;
  showRadio?: boolean;
}

function SensorRow({ sensor, metric, isSelected, onToggle, showRadio }: SensorRowProps) {
  const placementLabel = getSensorPlacementLabel(sensor.placement);
  const displayValue = sensor.placement === 'inline' 
    ? sensor.lastEventValue 
    : sensor.currentValue;
  
  const config = METRIC_CONFIGS[metric];
  const getUnit = () => config.unit ? ` ${config.unit}` : '';

  return (
    <button
      type="button"
      onClick={onToggle}
      className={cn(
        'w-full flex items-center justify-between p-2.5 rounded-lg border transition-all',
        isSelected
          ? 'bg-cyan-500/10 border-cyan-500/40'
          : 'bg-white/5 border-border hover:border-muted-foreground/50'
      )}
    >
      <div className="flex items-center gap-3">
        {/* Checkbox/Radio */}
        {showRadio ? (
          <div
            className={cn(
              'w-4 h-4 rounded-full border-2 flex items-center justify-center',
              isSelected ? 'border-cyan-500' : 'border-muted-foreground'
            )}
          >
            {isSelected && <div className="w-2 h-2 rounded-full bg-cyan-500" />}
          </div>
        ) : (
          <div
            className={cn(
              'w-4 h-4 rounded border flex items-center justify-center',
              isSelected ? 'bg-cyan-500 border-cyan-500' : 'border-muted-foreground'
            )}
          >
            {isSelected && <Check className="w-3 h-3 text-white" />}
          </div>
        )}

        {/* Sensor info */}
        <div className="text-left">
          <div className="flex items-center gap-2">
            <span className={cn('text-sm font-medium', isSelected ? 'text-foreground' : 'text-muted-foreground')}>
              {sensor.name}
            </span>
            {sensor.isOnline ? (
              <Wifi className="w-3 h-3 text-emerald-400" />
            ) : (
              <WifiOff className="w-3 h-3 text-rose-400" />
            )}
          </div>
          <div className="flex items-center gap-2 mt-0.5">
            <span
              className={cn(
                'px-1.5 py-0.5 rounded text-[10px] font-medium uppercase',
                sensor.placement === 'room' && 'bg-emerald-500/20 text-emerald-400',
                sensor.placement === 'runoff' && 'bg-violet-500/20 text-violet-400',
                sensor.placement === 'batch_tank' && 'bg-amber-500/20 text-amber-400',
                sensor.placement === 'inline' && 'bg-cyan-500/20 text-cyan-400'
              )}
            >
              {placementLabel}
            </span>
            {sensor.placement === 'batch_tank' && sensor.batchTankName && (
              <span className="text-[10px] text-muted-foreground">{sensor.batchTankName}</span>
            )}
          </div>
        </div>
      </div>

      {/* Current value */}
      <div className="text-right">
        {displayValue !== undefined ? (
          <>
            <div className={cn('text-sm font-mono font-medium', isSelected ? 'text-cyan-400' : 'text-foreground')}>
              {displayValue.toFixed(config.decimals)}{getUnit()}
            </div>
            {sensor.placement === 'inline' && (
              <div className="text-[10px] text-muted-foreground">last event</div>
            )}
          </>
        ) : (
          <div className="text-xs text-muted-foreground">No data</div>
        )}
      </div>
    </button>
  );
}

