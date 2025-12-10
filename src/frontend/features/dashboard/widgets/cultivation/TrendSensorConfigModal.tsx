'use client';

import React, { useState, useMemo, useEffect } from 'react';
import { createPortal } from 'react-dom';
import { cn } from '@/lib/utils';
import { X, Check, Wifi, WifiOff, Palette } from 'lucide-react';
import {
  MetricSensor,
  SensorMetricType,
  getMetricSensorsByZone,
  getSensorPlacementLabel,
  METRIC_CONFIGS,
} from './types';

// Default color palette for trend lines
const DEFAULT_COLORS = [
  '#22d3ee', // cyan
  '#a78bfa', // purple
  '#60a5fa', // blue
  '#34d399', // green
  '#fbbf24', // yellow
  '#f472b6', // pink
  '#fb923c', // orange
  '#ef4444', // red
];

export interface TrendSensorConfig {
  sensorId: string;
  color: string;
  enabled: boolean;
}

interface TrendSensorConfigModalProps {
  isOpen: boolean;
  onClose: () => void;
  /** Metric type (e.g., 'VWC', 'Temperature') */
  metricKey: string;
  metricName: string;
  metricType: SensorMetricType;
  /** Default color for the metric */
  defaultColor: string;
  /** Currently configured sensors */
  currentConfig: TrendSensorConfig[];
  /** Callback when configuration is saved */
  onSave: (config: TrendSensorConfig[]) => void;
}

export function TrendSensorConfigModal({
  isOpen,
  onClose,
  metricKey,
  metricName,
  metricType,
  defaultColor,
  currentConfig,
  onSave,
}: TrendSensorConfigModalProps) {
  const [sensorConfigs, setSensorConfigs] = useState<TrendSensorConfig[]>(currentConfig);
  const [customColorInput, setCustomColorInput] = useState<string>('');
  const [editingColorFor, setEditingColorFor] = useState<string | null>(null);

  // Reset state when modal opens
  useEffect(() => {
    if (isOpen) {
      setSensorConfigs(currentConfig);
      setEditingColorFor(null);
      setCustomColorInput('');
    }
  }, [isOpen, currentConfig]);

  // Handle escape key
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

  const sensorsByZone = useMemo(() => getMetricSensorsByZone(metricType), [metricType]);

  const toggleSensor = (sensorId: string) => {
    const existing = sensorConfigs.find(c => c.sensorId === sensorId);
    if (existing) {
      setSensorConfigs(sensorConfigs.map(c => 
        c.sensorId === sensorId ? { ...c, enabled: !c.enabled } : c
      ));
    } else {
      // Add new sensor with next available color
      const usedColors = sensorConfigs.map(c => c.color);
      const nextColor = DEFAULT_COLORS.find(c => !usedColors.includes(c)) || defaultColor;
      setSensorConfigs([...sensorConfigs, { sensorId, color: nextColor, enabled: true }]);
    }
  };

  const updateColor = (sensorId: string, color: string) => {
    setSensorConfigs(sensorConfigs.map(c => 
      c.sensorId === sensorId ? { ...c, color } : c
    ));
    setEditingColorFor(null);
    setCustomColorInput('');
  };

  const applyCustomColor = (sensorId: string) => {
    if (/^#[0-9A-Fa-f]{6}$/.test(customColorInput)) {
      updateColor(sensorId, customColorInput);
    }
  };

  const getSensorConfig = (sensorId: string): TrendSensorConfig | undefined => {
    return sensorConfigs.find(c => c.sensorId === sensorId);
  };

  const handleSave = () => {
    onSave(sensorConfigs.filter(c => c.enabled));
    onClose();
  };

  const enabledCount = sensorConfigs.filter(c => c.enabled).length;

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
            <h2 className="text-lg font-semibold text-foreground flex items-center gap-2">
              <Palette className="w-5 h-5 text-cyan-400" />
              Configure {metricName} Sensors
            </h2>
            <p className="text-sm text-muted-foreground mt-1">
              Select sensors to display on the trend chart and customize their colors.
            </p>
          </div>
          <button
            type="button"
            onClick={onClose}
            aria-label="Close modal"
            className="p-1.5 rounded-lg text-muted-foreground hover:text-foreground hover:bg-muted transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Content */}
        <div className="px-5 py-4 max-h-[50vh] overflow-y-auto space-y-4">
          {/* Selected count */}
          {enabledCount > 0 && (
            <div className="p-2 bg-cyan-500/10 border border-cyan-500/30 rounded-lg">
              <span className="text-sm text-cyan-400">
                {enabledCount} sensor{enabledCount > 1 ? 's' : ''} will display
              </span>
            </div>
          )}

          {/* Sensor list by zone */}
          {Object.entries(sensorsByZone).map(([zone, sensors]) => (
            <div key={zone} className="space-y-2">
              <div className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">
                {zone}
              </div>

              <div className="space-y-1">
                {sensors.map(sensor => {
                  const config = getSensorConfig(sensor.id);
                  const isEnabled = config?.enabled ?? false;
                  const color = config?.color || defaultColor;

                  return (
                    <div key={sensor.id} className="space-y-2">
                      <button
                        type="button"
                        onClick={() => toggleSensor(sensor.id)}
                        className={cn(
                          'w-full flex items-center justify-between p-2.5 rounded-lg border transition-all',
                          isEnabled
                            ? 'bg-cyan-500/10 border-cyan-500/40'
                            : 'bg-white/5 border-border hover:border-muted-foreground/50'
                        )}
                      >
                        <div className="flex items-center gap-3">
                          {/* Checkbox */}
                          <div
                            className={cn(
                              'w-4 h-4 rounded border flex items-center justify-center',
                              isEnabled ? 'bg-cyan-500 border-cyan-500' : 'border-muted-foreground'
                            )}
                          >
                            {isEnabled && <Check className="w-3 h-3 text-white" />}
                          </div>

                          {/* Color indicator */}
                          {isEnabled && (
                            <button
                              type="button"
                              onClick={(e) => {
                                e.stopPropagation();
                                setEditingColorFor(editingColorFor === sensor.id ? null : sensor.id);
                              }}
                              className="w-5 h-5 rounded-full border-2 border-white/20 hover:scale-110 transition-transform"
                              style={{ backgroundColor: color }}
                              title="Click to change color"
                            />
                          )}

                          {/* Sensor info */}
                          <div className="text-left">
                            <div className="flex items-center gap-2">
                              <span className={cn('text-sm font-medium', isEnabled ? 'text-foreground' : 'text-muted-foreground')}>
                                {sensor.name}
                              </span>
                              {sensor.isOnline ? (
                                <Wifi className="w-3 h-3 text-emerald-400" />
                              ) : (
                                <WifiOff className="w-3 h-3 text-rose-400" />
                              )}
                            </div>
                            <span
                              className={cn(
                                'px-1.5 py-0.5 rounded text-[10px] font-medium uppercase',
                                sensor.placement === 'room' && 'bg-emerald-500/20 text-emerald-400',
                                sensor.placement === 'runoff' && 'bg-violet-500/20 text-violet-400',
                                sensor.placement === 'batch_tank' && 'bg-amber-500/20 text-amber-400',
                                sensor.placement === 'inline' && 'bg-cyan-500/20 text-cyan-400'
                              )}
                            >
                              {getSensorPlacementLabel(sensor.placement)}
                            </span>
                          </div>
                        </div>

                        {/* Current value */}
                        <div className="text-right">
                          {sensor.currentValue !== undefined ? (
                            <div className={cn('text-sm font-mono font-medium', isEnabled ? 'text-cyan-400' : 'text-foreground')}>
                              {sensor.currentValue.toFixed(METRIC_CONFIGS[metricType].decimals)}
                              {METRIC_CONFIGS[metricType].unit}
                            </div>
                          ) : (
                            <div className="text-xs text-muted-foreground">No data</div>
                          )}
                        </div>
                      </button>

                      {/* Color picker dropdown */}
                      {isEnabled && editingColorFor === sensor.id && (
                        <div className="ml-8 p-3 bg-muted/50 rounded-lg border border-border space-y-3">
                          {/* Preset colors */}
                          <div className="flex flex-wrap gap-2">
                            {DEFAULT_COLORS.map(presetColor => (
                              <button
                                key={presetColor}
                                type="button"
                                onClick={() => updateColor(sensor.id, presetColor)}
                                aria-label={`Select color ${presetColor}`}
                                className={cn(
                                  'w-7 h-7 rounded-full border-2 hover:scale-110 transition-transform',
                                  color === presetColor ? 'border-white' : 'border-transparent'
                                )}
                                style={{ backgroundColor: presetColor }}
                              />
                            ))}
                          </div>

                          {/* Custom hex input */}
                          <div className="flex items-center gap-2">
                            <input
                              type="text"
                              value={customColorInput}
                              onChange={(e) => setCustomColorInput(e.target.value)}
                              placeholder="#22d3ee"
                              className="flex-1 h-8 px-2 bg-surface border border-border rounded text-sm text-foreground placeholder:text-muted-foreground"
                            />
                            <button
                              type="button"
                              onClick={() => applyCustomColor(sensor.id)}
                              disabled={!/^#[0-9A-Fa-f]{6}$/.test(customColorInput)}
                              className="px-3 h-8 text-xs font-medium bg-cyan-600 hover:bg-cyan-500 disabled:bg-muted disabled:text-muted-foreground text-white rounded transition-colors"
                            >
                              Apply
                            </button>
                          </div>
                        </div>
                      )}
                    </div>
                  );
                })}
              </div>
            </div>
          ))}

          {Object.keys(sensorsByZone).length === 0 && (
            <div className="text-center py-8 text-muted-foreground">
              No {metricName} sensors available
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="flex items-center justify-end gap-3 px-5 py-4 border-t border-border">
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
            Save Configuration
          </button>
        </div>
      </div>
    </div>
  );

  return createPortal(modalContent, document.body);
}


