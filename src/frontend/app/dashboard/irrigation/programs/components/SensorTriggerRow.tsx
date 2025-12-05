'use client';

import React, { useState } from 'react';
import { Trash2 } from 'lucide-react';
import { cn } from '@/lib/utils';
import { Input } from '@/components/admin/AdminForm';
import {
  SensorTrigger,
  SensorAggregation,
  SENSOR_METRICS,
  SENSOR_AGGREGATIONS,
  AVAILABLE_SENSORS,
  getCommonMetrics,
  getSensorsByZone,
} from '../types';

interface SensorTriggerRowProps {
  trigger: SensorTrigger;
  onChange: (updates: Partial<SensorTrigger>) => void;
  onRemove: () => void;
}

/**
 * Reusable sensor trigger row component with multi-select
 * Allows selecting multiple sensors and configuring trigger conditions
 */
export function SensorTriggerRow({
  trigger,
  onChange,
  onRemove,
}: SensorTriggerRowProps) {
  const [isExpanded, setIsExpanded] = useState(false);
  const sensorsByZone = getSensorsByZone();
  const commonMetrics = getCommonMetrics(trigger.sensorIds);
  const selectedCount = trigger.sensorIds.length;

  const toggleSensor = (sensorId: string) => {
    const current = trigger.sensorIds;
    if (current.includes(sensorId)) {
      onChange({ sensorIds: current.filter(id => id !== sensorId) });
    } else {
      onChange({ sensorIds: [...current, sensorId] });
    }
  };

  const selectAllInZone = (zone: string) => {
    const zoneIds = sensorsByZone[zone].map(s => s.id);
    const allSelected = zoneIds.every(id => trigger.sensorIds.includes(id));
    if (allSelected) {
      onChange({ sensorIds: trigger.sensorIds.filter(id => !zoneIds.includes(id)) });
    } else {
      const newIds = [...new Set([...trigger.sensorIds, ...zoneIds])];
      onChange({ sensorIds: newIds });
    }
  };
  
  return (
    <div className="p-3 bg-white/5 rounded-lg space-y-2">
      {/* Row 1: Sensor Selection */}
      <div className="flex items-center gap-2">
        <button
          type="button"
          onClick={() => setIsExpanded(!isExpanded)}
          className={cn(
            'flex-1 h-9 px-3 text-left text-sm rounded-lg border transition-colors flex items-center justify-between',
            selectedCount > 0
              ? 'bg-cyan-500/10 border-cyan-500/30 text-foreground'
              : 'bg-white/5 border-border text-muted-foreground'
          )}
        >
          <span>
            {selectedCount === 0 && 'Select sensors...'}
            {selectedCount === 1 && AVAILABLE_SENSORS.find(s => s.id === trigger.sensorIds[0])?.name}
            {selectedCount > 1 && `${selectedCount} sensors selected`}
          </span>
          <span className={cn('transition-transform', isExpanded && 'rotate-180')}>▾</span>
        </button>

        {/* Aggregation (only when multiple sensors) */}
        {selectedCount > 1 && (
          <select
            value={trigger.aggregation}
            onChange={e => onChange({ aggregation: e.target.value as SensorAggregation })}
            className="w-24 h-9 px-2 bg-white/5 border border-border rounded-lg text-xs text-foreground"
            title="How to combine multiple sensor readings"
          >
            {SENSOR_AGGREGATIONS.map(a => (
              <option key={a.value} value={a.value}>{a.label}</option>
            ))}
          </select>
        )}

        {/* Delete */}
        <button
          type="button"
          onClick={onRemove}
          className="p-2 text-muted-foreground hover:text-rose-400 transition-colors"
        >
          <Trash2 className="w-4 h-4" />
        </button>
      </div>

      {/* Sensor Selection Dropdown */}
      {isExpanded && (
        <div className="p-2 bg-black/20 rounded-lg max-h-48 overflow-y-auto space-y-2">
          {Object.entries(sensorsByZone).map(([zone, sensors]) => (
            <div key={zone}>
              <button
                type="button"
                onClick={() => selectAllInZone(zone)}
                className="text-xs font-medium text-muted-foreground hover:text-foreground mb-1 flex items-center gap-1"
              >
                <span className={cn(
                  'w-3 h-3 rounded border flex items-center justify-center text-[8px]',
                  sensors.every(s => trigger.sensorIds.includes(s.id))
                    ? 'bg-cyan-500 border-cyan-500 text-white'
                    : sensors.some(s => trigger.sensorIds.includes(s.id))
                    ? 'bg-cyan-500/50 border-cyan-500'
                    : 'border-border'
                )}>
                  {sensors.every(s => trigger.sensorIds.includes(s.id)) && '✓'}
                </span>
                {zone}
              </button>
              <div className="flex flex-wrap gap-1 ml-4">
                {sensors.map(sensor => (
                  <button
                    key={sensor.id}
                    type="button"
                    onClick={() => toggleSensor(sensor.id)}
                    className={cn(
                      'px-2 py-1 text-xs rounded border transition-colors',
                      trigger.sensorIds.includes(sensor.id)
                        ? 'bg-cyan-500/20 border-cyan-500/40 text-cyan-400'
                        : 'border-border text-muted-foreground hover:text-foreground hover:border-muted-foreground/50'
                    )}
                  >
                    {sensor.name.replace(zone.split(' ')[0] + ' ', '')}
                  </button>
                ))}
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Row 2: Condition (only show if sensors selected) */}
      {selectedCount > 0 && (
        <div className="flex items-center gap-2 pl-1">
          <span className="text-xs text-muted-foreground">When</span>
          <select
            value={trigger.metric}
            onChange={e => onChange({ metric: e.target.value as SensorTrigger['metric'] })}
            className="h-8 px-2 bg-white/5 border border-border rounded-lg text-sm text-foreground"
          >
            {commonMetrics.length > 0 ? (
              commonMetrics.map(m => {
                const metric = SENSOR_METRICS.find(sm => sm.value === m);
                return <option key={m} value={m}>{metric?.label || m}</option>;
              })
            ) : (
              SENSOR_METRICS.map(m => (
                <option key={m.value} value={m.value}>{m.label}</option>
              ))
            )}
          </select>
          <select
            value={trigger.operator}
            onChange={e => onChange({ operator: e.target.value as SensorTrigger['operator'] })}
            className="w-16 h-8 px-2 bg-white/5 border border-border rounded-lg text-sm text-foreground"
          >
            <option value="<">&lt;</option>
            <option value="<=">&le;</option>
            <option value=">">&gt;</option>
            <option value=">=">&ge;</option>
            <option value="==">=</option>
            <option value="!=">&ne;</option>
          </select>
          <Input
            type="number"
            value={trigger.value}
            onChange={e => onChange({ value: parseFloat(e.target.value) || 0 })}
            className="w-20 h-8"
          />
          <span className="text-xs text-muted-foreground">{trigger.unit}</span>
        </div>
      )}

      {/* Helper text */}
      {selectedCount === 0 && (
        <p className="text-xs text-amber-400/80 pl-1">⚠ Select at least one sensor</p>
      )}
      {selectedCount > 1 && (
        <p className="text-xs text-muted-foreground/70 pl-1">
          Using {trigger.aggregation} of {selectedCount} sensors
        </p>
      )}
    </div>
  );
}




