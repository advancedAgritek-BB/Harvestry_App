'use client';

import React, { useState, useCallback, useEffect } from 'react';
import {
  X,
  Droplets,
  Calendar,
  Shield,
  Sun,
  Moon,
  Target,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { FormField, Input, Switch } from '@/components/admin/AdminForm';
import {
  IrrigationProgram,
  ProgramFormData,
  ProgramSchedule,
  SensorTrigger,
  PROGRAM_TYPES,
  SENSOR_METRICS,
  DEFAULT_PROGRAM_FORM,
  getProgramTypeColor,
  getCommonMetrics,
} from './types';
import { ZoneCalibration } from '@/components/irrigation/types';
import { 
  P1RampPhaseSection,
  P2MaintenancePhaseSection,
  ScheduleTabContent,
  ShotsTabContent,
} from './components';

interface AddEditProgramModalProps {
  isOpen: boolean;
  onClose: () => void;
  program: IrrigationProgram | null;
  onSave: (data: ProgramFormData) => void;
  isSubmitting?: boolean;
  availableZones?: string[];
  /** Zone calibrations keyed by zone name - used to calculate shot duration per zone */
  zoneCalibrations?: Record<string, ZoneCalibration>;
}

type TabId = 'basic' | 'schedule' | 'shots' | 'phases' | 'safety';

const TABS: { id: TabId; label: string; icon: React.ReactNode }[] = [
  { id: 'basic', label: 'Basic', icon: <Target className="w-4 h-4" /> },
  { id: 'schedule', label: 'Schedule', icon: <Calendar className="w-4 h-4" /> },
  { id: 'shots', label: 'Shots', icon: <Droplets className="w-4 h-4" /> },
  { id: 'phases', label: 'Phases', icon: <Sun className="w-4 h-4" /> },
  { id: 'safety', label: 'Safety', icon: <Shield className="w-4 h-4" /> },
];

export function AddEditProgramModal({
  isOpen,
  onClose,
  program,
  onSave,
  isSubmitting = false,
  availableZones = ['Flower Room 1', 'Flower Room 2', 'Veg Room 1', 'Veg Room 2', 'Clone Room'],
  zoneCalibrations = {},
}: AddEditProgramModalProps) {
  const [activeTab, setActiveTab] = useState<TabId>('basic');
  const [formData, setFormData] = useState<ProgramFormData>(DEFAULT_PROGRAM_FORM);

  // Initialize form when program changes
  useEffect(() => {
    if (program) {
      setFormData({
        name: program.name,
        description: program.description,
        type: program.type,
        targetZones: program.targetZones,
        shotConfig: program.shotConfig,
        phaseTargets: program.phaseTargets,
        nightProfile: program.nightProfile,
        safetyPolicy: program.safetyPolicy,
        schedule: program.schedule,
        enabled: program.enabled,
      });
    } else {
      setFormData(DEFAULT_PROGRAM_FORM);
    }
    setActiveTab('basic');
  }, [program, isOpen]);

  const updateField = useCallback(<K extends keyof ProgramFormData>(
    key: K,
    value: ProgramFormData[K]
  ) => {
    setFormData(prev => ({ ...prev, [key]: value }));
  }, []);

  const updateSchedule = useCallback(<K extends keyof ProgramSchedule>(
    key: K,
    value: ProgramSchedule[K]
  ) => {
    setFormData(prev => ({
      ...prev,
      schedule: { ...prev.schedule, [key]: value },
    }));
  }, []);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSave(formData);
  };

  const addSensorTrigger = useCallback((type: 'sensorTriggers' | 'guardConditions') => {
    const newTrigger: SensorTrigger = {
      sensorIds: [],
      aggregation: 'average',
      metric: 'VWC',
      operator: '<',
      value: 55,
      unit: '%',
    };
    updateSchedule(type, [...formData.schedule[type], newTrigger]);
  }, [formData.schedule, updateSchedule]);

  const removeSensorTrigger = useCallback((type: 'sensorTriggers' | 'guardConditions', index: number) => {
    updateSchedule(type, formData.schedule[type].filter((_, i) => i !== index));
  }, [formData.schedule, updateSchedule]);

  const updateSensorTrigger = useCallback((
    type: 'sensorTriggers' | 'guardConditions',
    index: number,
    updates: Partial<SensorTrigger>
  ) => {
    const updated = [...formData.schedule[type]];
    const current = updated[index];
    
    // Handle sensor selection changes
    if (updates.sensorIds !== undefined) {
      const newSensorIds = updates.sensorIds;
      const commonMetrics = getCommonMetrics(newSensorIds);
      const currentMetric = updates.metric || current.metric;
      const metricValid = commonMetrics.includes(currentMetric);
      
      updated[index] = {
        ...current,
        ...updates,
        metric: metricValid ? currentMetric : (commonMetrics[0] || 'VWC'),
        unit: SENSOR_METRICS.find(m => m.value === (metricValid ? currentMetric : commonMetrics[0]))?.unit || '%',
      };
    } else if (updates.metric !== undefined) {
      // Handle metric change - update unit
      const metric = SENSOR_METRICS.find(m => m.value === updates.metric);
      updated[index] = { ...current, ...updates, unit: metric?.unit || '' };
    } else {
      updated[index] = { ...current, ...updates };
    }
    
    updateSchedule(type, updated);
  }, [formData.schedule, updateSchedule]);

  const toggleZone = useCallback((zone: string) => {
    const current = formData.targetZones;
    if (current.includes(zone)) {
      updateField('targetZones', current.filter(z => z !== zone));
    } else {
      updateField('targetZones', [...current, zone]);
    }
  }, [formData.targetZones, updateField]);

  const toggleDay = useCallback((day: ProgramSchedule['days'][number]) => {
    const current = formData.schedule.days;
    if (current.includes(day)) {
      updateSchedule('days', current.filter(d => d !== day));
    } else {
      updateSchedule('days', [...current, day]);
    }
  }, [formData.schedule.days, updateSchedule]);

  if (!isOpen) return null;

  const isEditing = !!program;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-black/60 backdrop-blur-sm" onClick={onClose} />
      
      <div className="relative w-full max-w-3xl max-h-[90vh] bg-surface border border-border rounded-2xl shadow-2xl flex flex-col">
        {/* Header */}
        <div className="flex items-center justify-between p-4 border-b border-border shrink-0">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-cyan-500/10 flex items-center justify-center">
              <Droplets className="w-5 h-5 text-cyan-400" />
            </div>
            <div>
              <h2 className="text-lg font-bold text-foreground">
                {isEditing ? 'Edit Program' : 'Create Program'}
              </h2>
              <p className="text-xs text-muted-foreground">
                Configure irrigation program with embedded schedule
              </p>
            </div>
          </div>
          <button onClick={onClose} className="p-2 hover:bg-muted rounded-lg transition-colors">
            <X className="w-5 h-5 text-muted-foreground" />
          </button>
        </div>

        {/* Tabs */}
        <div className="flex border-b border-border px-4 shrink-0">
          {TABS.map(tab => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={cn(
                'flex items-center gap-2 px-4 py-3 text-sm font-medium border-b-2 -mb-px transition-colors',
                activeTab === tab.id
                  ? 'border-cyan-400 text-cyan-400'
                  : 'border-transparent text-muted-foreground hover:text-foreground'
              )}
            >
              {tab.icon}
              {tab.label}
            </button>
          ))}
        </div>

        {/* Content */}
        <form onSubmit={handleSubmit} className="flex-1 overflow-y-auto p-4">
          {/* Basic Tab */}
          {activeTab === 'basic' && (
            <div className="space-y-4">
              <FormField label="Program Name" required>
                <Input
                  value={formData.name}
                  onChange={e => updateField('name', e.target.value)}
                  placeholder="e.g., F1 Morning Ramp"
                />
              </FormField>

              <FormField label="Description">
                <Input
                  value={formData.description}
                  onChange={e => updateField('description', e.target.value)}
                  placeholder="Brief description of this program"
                />
              </FormField>

              <FormField label="Program Type" required>
                <div className="grid grid-cols-3 gap-2">
                  {PROGRAM_TYPES.map(type => (
                    <button
                      key={type.value}
                      type="button"
                      onClick={() => updateField('type', type.value)}
                      className={cn(
                        'p-3 rounded-lg border text-left transition-all',
                        formData.type === type.value
                          ? getProgramTypeColor(type.value) + ' border-current'
                          : 'border-border hover:border-muted-foreground/50 text-muted-foreground'
                      )}
                    >
                      <div className="font-medium text-sm">{type.label}</div>
                      <div className="text-xs opacity-70 mt-0.5">{type.description}</div>
                    </button>
                  ))}
                </div>
              </FormField>

              <FormField label="Target Zones" required>
                <div className="flex flex-wrap gap-2">
                  {availableZones.map(zone => (
                    <button
                      key={zone}
                      type="button"
                      onClick={() => toggleZone(zone)}
                      className={cn(
                        'px-3 py-1.5 rounded-lg text-sm border transition-colors',
                        formData.targetZones.includes(zone)
                          ? 'bg-cyan-500/20 text-cyan-400 border-cyan-500/30'
                          : 'border-border text-muted-foreground hover:text-foreground hover:border-muted-foreground/50'
                      )}
                    >
                      {zone}
                    </button>
                  ))}
                </div>
              </FormField>

              <div className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
                <div>
                  <div className="text-sm font-medium text-foreground">Program Enabled</div>
                  <div className="text-xs text-muted-foreground">
                    Disable to pause without deleting
                  </div>
                </div>
                <Switch
                  checked={formData.enabled}
                  onChange={checked => updateField('enabled', checked)}
                />
              </div>
            </div>
          )}

          {/* Schedule Tab */}
          {activeTab === 'schedule' && (
            <ScheduleTabContent
              schedule={formData.schedule}
              onUpdateSchedule={updateSchedule}
              onAddSensorTrigger={addSensorTrigger}
              onRemoveSensorTrigger={removeSensorTrigger}
              onUpdateSensorTrigger={updateSensorTrigger}
            />
          )}

          {/* Shots Tab */}
          {activeTab === 'shots' && (
            <ShotsTabContent
              shotConfig={formData.shotConfig}
              targetZones={formData.targetZones}
              zoneCalibrations={zoneCalibrations}
              onUpdateShotConfig={(updates) => updateField('shotConfig', { ...formData.shotConfig, ...updates })}
            />
          )}

          {/* Phases Tab */}
          {activeTab === 'phases' && (
            <div className="space-y-4">
              {/* P1 - Ramp */}
              <P1RampPhaseSection
                phaseTargets={formData.phaseTargets}
                expectedVwcIncreasePercent={formData.shotConfig.expectedVwcIncreasePercent}
                onUpdate={(updates) => updateField('phaseTargets', { ...formData.phaseTargets, ...updates })}
              />

              {/* P2 - Maintenance */}
              <P2MaintenancePhaseSection
                phaseTargets={formData.phaseTargets}
                expectedVwcIncreasePercent={formData.shotConfig.expectedVwcIncreasePercent}
                onUpdate={(updates) => updateField('phaseTargets', { ...formData.phaseTargets, ...updates })}
              />

              {/* P3 - Dryback */}
              <div className="p-4 bg-white/5 rounded-lg space-y-3">
                <div className="flex items-center gap-2">
                  <span className="text-xs font-bold px-2 py-0.5 rounded bg-rose-500/20 text-rose-400">P3</span>
                  <span className="font-medium text-foreground">Dryback Phase</span>
                  <span className="text-xs text-muted-foreground ml-auto">Evening dry-down</span>
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <FormField label="Target Dryback %" description="% to dry back">
                    <Input
                      type="number"
                      min={10}
                      max={50}
                      value={formData.phaseTargets.p3TargetDrybackPercent}
                      onChange={e => updateField('phaseTargets', {
                        ...formData.phaseTargets,
                        p3TargetDrybackPercent: parseInt(e.target.value) || 0,
                      })}
                    />
                  </FormField>
                  <FormField label="Emergency Shots" description="Allow if critical">
                    <div className="flex items-center h-10">
                      <Switch
                        checked={formData.phaseTargets.p3AllowEmergencyShots}
                        onChange={checked => updateField('phaseTargets', {
                          ...formData.phaseTargets,
                          p3AllowEmergencyShots: checked,
                        })}
                      />
                      <span className="ml-2 text-sm text-muted-foreground">
                        {formData.phaseTargets.p3AllowEmergencyShots ? 'Enabled' : 'Disabled'}
                      </span>
                    </div>
                  </FormField>
                </div>
              </div>

              {/* Night Profile */}
              <div className="p-4 bg-white/5 rounded-lg space-y-3">
                <div className="flex items-center gap-2">
                  <Moon className="w-4 h-4 text-blue-400" />
                  <span className="font-medium text-foreground">Night Profile</span>
                </div>
                <div className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
                  <div>
                    <div className="text-sm font-medium text-foreground">Allow Night Irrigation</div>
                    <div className="text-xs text-muted-foreground">Enable shots during dark period</div>
                  </div>
                  <Switch
                    checked={formData.nightProfile.allowIrrigation}
                    onChange={checked => updateField('nightProfile', {
                      ...formData.nightProfile,
                      allowIrrigation: checked,
                    })}
                  />
                </div>
                {formData.nightProfile.allowIrrigation && (
                  <div className="grid grid-cols-2 gap-4">
                    <FormField label="Maintain VWC %" description="Target at night">
                      <Input
                        type="number"
                        min={20}
                        max={60}
                        value={formData.nightProfile.maintainVwcPercent}
                        onChange={e => updateField('nightProfile', {
                          ...formData.nightProfile,
                          maintainVwcPercent: parseInt(e.target.value) || 0,
                        })}
                      />
                    </FormField>
                    <FormField label="Max Night Shots" description="Limit overnight">
                      <Input
                        type="number"
                        min={0}
                        max={6}
                        value={formData.nightProfile.maxNightShots}
                        onChange={e => updateField('nightProfile', {
                          ...formData.nightProfile,
                          maxNightShots: parseInt(e.target.value) || 0,
                        })}
                      />
                    </FormField>
                  </div>
                )}
              </div>
            </div>
          )}

          {/* Safety Tab */}
          {activeTab === 'safety' && (
            <div className="space-y-4">
              <FormField label="Max Volume Per Plant Per Day (mL)" description="Daily safety limit">
                <Input
                  type="number"
                  min={50}
                  max={500}
                  value={formData.safetyPolicy.maxVolumeMlPerPlantPerDay}
                  onChange={e => updateField('safetyPolicy', {
                    ...formData.safetyPolicy,
                    maxVolumeMlPerPlantPerDay: parseInt(e.target.value) || 0,
                  })}
                />
              </FormField>

              <div className="grid grid-cols-2 gap-4">
                <FormField label="EC Range" description="Min - Max mS/cm">
                  <div className="flex items-center gap-2">
                    <Input
                      type="number"
                      min={0}
                      max={5}
                      step={0.1}
                      value={formData.safetyPolicy.minEc}
                      onChange={e => updateField('safetyPolicy', {
                        ...formData.safetyPolicy,
                        minEc: parseFloat(e.target.value) || 0,
                      })}
                    />
                    <span className="text-muted-foreground">-</span>
                    <Input
                      type="number"
                      min={0}
                      max={5}
                      step={0.1}
                      value={formData.safetyPolicy.maxEc}
                      onChange={e => updateField('safetyPolicy', {
                        ...formData.safetyPolicy,
                        maxEc: parseFloat(e.target.value) || 0,
                      })}
                    />
                  </div>
                </FormField>
                <FormField label="pH Range" description="Min - Max">
                  <div className="flex items-center gap-2">
                    <Input
                      type="number"
                      min={4}
                      max={8}
                      step={0.1}
                      value={formData.safetyPolicy.minPh}
                      onChange={e => updateField('safetyPolicy', {
                        ...formData.safetyPolicy,
                        minPh: parseFloat(e.target.value) || 0,
                      })}
                    />
                    <span className="text-muted-foreground">-</span>
                    <Input
                      type="number"
                      min={4}
                      max={8}
                      step={0.1}
                      value={formData.safetyPolicy.maxPh}
                      onChange={e => updateField('safetyPolicy', {
                        ...formData.safetyPolicy,
                        maxPh: parseFloat(e.target.value) || 0,
                      })}
                    />
                  </div>
                </FormField>
              </div>

              <div className="space-y-3">
                <label className="flex items-center gap-3 p-3 bg-white/5 rounded-lg cursor-pointer">
                  <Switch
                    checked={formData.safetyPolicy.requireFlowVerification}
                    onChange={checked => updateField('safetyPolicy', {
                      ...formData.safetyPolicy,
                      requireFlowVerification: checked,
                    })}
                  />
                  <div>
                    <div className="text-sm font-medium text-foreground">Require Flow Verification</div>
                    <div className="text-xs text-muted-foreground">Verify flow sensor readings before shots</div>
                  </div>
                </label>
                <label className="flex items-center gap-3 p-3 bg-white/5 rounded-lg cursor-pointer">
                  <Switch
                    checked={formData.safetyPolicy.requirePressureVerification}
                    onChange={checked => updateField('safetyPolicy', {
                      ...formData.safetyPolicy,
                      requirePressureVerification: checked,
                    })}
                  />
                  <div>
                    <div className="text-sm font-medium text-foreground">Require Pressure Verification</div>
                    <div className="text-xs text-muted-foreground">Check system pressure before irrigation</div>
                  </div>
                </label>
              </div>
            </div>
          )}
        </form>

        {/* Footer */}
        <div className="flex items-center justify-between p-4 border-t border-border shrink-0">
          {/* Validation Messages */}
          <div className="text-xs">
            {!formData.name && (
              <span className="text-amber-400">Program name required</span>
            )}
            {formData.name && formData.targetZones.length === 0 && (
              <span className="text-amber-400">Select at least one zone</span>
            )}
          </div>
          
          <div className="flex items-center gap-3">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-muted-foreground hover:text-foreground transition-colors"
            >
              Cancel
            </button>
            <button
              onClick={handleSubmit}
              disabled={isSubmitting || !formData.name || formData.targetZones.length === 0}
              className="flex items-center gap-2 px-4 py-2 bg-cyan-600 hover:bg-cyan-500 disabled:opacity-50 disabled:cursor-not-allowed text-foreground rounded-lg font-medium transition-colors"
            >
              <Droplets className="w-4 h-4" />
              {isEditing ? 'Update Program' : 'Create Program'}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}

