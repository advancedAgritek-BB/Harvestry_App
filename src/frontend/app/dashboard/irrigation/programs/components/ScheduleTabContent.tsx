'use client';

import React from 'react';
import { Clock, Gauge, Zap, Shield, Plus } from 'lucide-react';
import { cn } from '@/lib/utils';
import { FormField, Input, Switch } from '@/components/admin/AdminForm';
import { ProgramSchedule, SensorTrigger, ScheduleTriggerType, DAYS_OF_WEEK } from '../types';
import { SensorTriggerRow } from './SensorTriggerRow';

interface ScheduleTabContentProps {
  schedule: ProgramSchedule;
  onUpdateSchedule: <K extends keyof ProgramSchedule>(key: K, value: ProgramSchedule[K]) => void;
  onAddSensorTrigger: (type: 'sensorTriggers' | 'guardConditions') => void;
  onRemoveSensorTrigger: (type: 'sensorTriggers' | 'guardConditions', index: number) => void;
  onUpdateSensorTrigger: (
    type: 'sensorTriggers' | 'guardConditions',
    index: number,
    updates: Partial<SensorTrigger>
  ) => void;
}

/**
 * Schedule tab content for the program modal
 * Handles trigger type, time settings, sensor triggers, and guard conditions
 */
export function ScheduleTabContent({
  schedule,
  onUpdateSchedule,
  onAddSensorTrigger,
  onRemoveSensorTrigger,
  onUpdateSensorTrigger,
}: ScheduleTabContentProps) {
  const toggleDay = (day: ProgramSchedule['days'][number]) => {
    const current = schedule.days;
    if (current.includes(day)) {
      onUpdateSchedule('days', current.filter(d => d !== day));
    } else {
      onUpdateSchedule('days', [...current, day]);
    }
  };

  return (
    <div className="space-y-4">
      <FormField label="Trigger Type" required>
        <div className="grid grid-cols-3 gap-2">
          {[
            { value: 'time', label: 'Time-Based', icon: <Clock className="w-4 h-4" />, desc: 'Run at specific times' },
            { value: 'sensor', label: 'Sensor-Based', icon: <Gauge className="w-4 h-4" />, desc: 'Run when conditions met' },
            { value: 'hybrid', label: 'Hybrid', icon: <Zap className="w-4 h-4" />, desc: 'Time window + conditions' },
          ].map(trigger => (
            <button
              key={trigger.value}
              type="button"
              onClick={() => onUpdateSchedule('triggerType', trigger.value as ScheduleTriggerType)}
              className={cn(
                'p-3 rounded-lg border text-center transition-all',
                schedule.triggerType === trigger.value
                  ? 'bg-cyan-500/20 text-cyan-400 border-cyan-500/30'
                  : 'border-border text-muted-foreground hover:border-muted-foreground/50'
              )}
            >
              <div className="flex justify-center mb-1">{trigger.icon}</div>
              <div className="font-medium text-sm">{trigger.label}</div>
              <div className="text-xs opacity-70">{trigger.desc}</div>
            </button>
          ))}
        </div>
      </FormField>

      {/* Time Settings */}
      {(schedule.triggerType === 'time' || schedule.triggerType === 'hybrid') && (
        <div className="p-4 bg-white/5 rounded-lg space-y-4">
          <h4 className="text-sm font-medium text-foreground flex items-center gap-2">
            <Clock className="w-4 h-4 text-cyan-400" />
            Time Settings
          </h4>
          <div className="grid grid-cols-2 gap-4">
            <FormField label="Start Time">
              <Input
                type="time"
                value={schedule.startTime}
                onChange={e => onUpdateSchedule('startTime', e.target.value)}
              />
            </FormField>
            <FormField label="Days">
              <div className="flex gap-1">
                {DAYS_OF_WEEK.map(day => (
                  <button
                    key={day.value}
                    type="button"
                    onClick={() => toggleDay(day.value)}
                    className={cn(
                      'w-9 h-9 rounded-lg text-xs font-medium transition-colors',
                      schedule.days.includes(day.value)
                        ? 'bg-cyan-500/20 text-cyan-400'
                        : 'bg-muted text-muted-foreground hover:text-foreground'
                    )}
                  >
                    {day.label.charAt(0)}
                  </button>
                ))}
              </div>
            </FormField>
          </div>
        </div>
      )}

      {/* Sensor Triggers */}
      {(schedule.triggerType === 'sensor' || schedule.triggerType === 'hybrid') && (
        <div className="p-4 bg-white/5 rounded-lg space-y-3">
          <div className="flex items-center justify-between">
            <h4 className="text-sm font-medium text-foreground flex items-center gap-2">
              <Gauge className="w-4 h-4 text-emerald-400" />
              Sensor Triggers
              <span className="text-xs text-muted-foreground font-normal">(ANY condition triggers)</span>
            </h4>
            <button
              type="button"
              onClick={() => onAddSensorTrigger('sensorTriggers')}
              className="text-xs text-cyan-400 hover:text-cyan-300 flex items-center gap-1"
            >
              <Plus className="w-3 h-3" /> Add
            </button>
          </div>
          {schedule.sensorTriggers.length === 0 ? (
            <p className="text-xs text-muted-foreground italic">No sensor triggers configured</p>
          ) : (
            schedule.sensorTriggers.map((trigger, idx) => (
              <SensorTriggerRow
                key={idx}
                trigger={trigger}
                onChange={(updates) => onUpdateSensorTrigger('sensorTriggers', idx, updates)}
                onRemove={() => onRemoveSensorTrigger('sensorTriggers', idx)}
              />
            ))
          )}
        </div>
      )}

      {/* Guard Conditions */}
      <div className="p-4 bg-white/5 rounded-lg space-y-3">
        <div className="flex items-center justify-between">
          <h4 className="text-sm font-medium text-foreground flex items-center gap-2">
            <Shield className="w-4 h-4 text-amber-400" />
            Guard Conditions
            <span className="text-xs text-muted-foreground font-normal">(ALL must be met)</span>
          </h4>
          <button
            type="button"
            onClick={() => onAddSensorTrigger('guardConditions')}
            className="text-xs text-cyan-400 hover:text-cyan-300 flex items-center gap-1"
          >
            <Plus className="w-3 h-3" /> Add
          </button>
        </div>
        {schedule.guardConditions.length === 0 ? (
          <p className="text-xs text-muted-foreground italic">No guard conditions (will run when triggered)</p>
        ) : (
          schedule.guardConditions.map((trigger, idx) => (
            <SensorTriggerRow
              key={idx}
              trigger={trigger}
              onChange={(updates) => onUpdateSensorTrigger('guardConditions', idx, updates)}
              onRemove={() => onRemoveSensorTrigger('guardConditions', idx)}
            />
          ))
        )}
      </div>

      {/* Schedule Enable */}
      <div className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
        <div>
          <div className="text-sm font-medium text-foreground">Schedule Enabled</div>
          <div className="text-xs text-muted-foreground">
            Disable schedule without disabling the entire program
          </div>
        </div>
        <Switch
          checked={schedule.enabled}
          onChange={checked => onUpdateSchedule('enabled', checked)}
        />
      </div>
    </div>
  );
}








