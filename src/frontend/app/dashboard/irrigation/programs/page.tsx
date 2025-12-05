'use client';

import React, { useState, useCallback } from 'react';
import { 
  Plus, 
  Edit2, 
  Trash2, 
  Play, 
  Pause,
  Clock,
  Gauge,
  Zap,
  Droplets,
  AlertCircle,
  CheckCircle,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { AddEditProgramModal } from './AddEditProgramModal';
import {
  IrrigationProgram,
  ProgramFormData,
  getProgramTypeColor,
  formatDays,
  DEFAULT_PROGRAM_FORM,
  DEFAULT_SCHEDULE,
  DEFAULT_NIGHT_PROFILE,
} from './types';
import {
  DEFAULT_SHOT_CONFIG,
  DEFAULT_PHASE_TARGETS,
  DEFAULT_SAFETY_POLICY,
  ZoneCalibration,
} from '@/components/irrigation/types';

// Mock zone calibration data - in production this would come from Zone Settings API
const MOCK_ZONE_CALIBRATIONS: Record<string, ZoneCalibration> = {
  'Flower Room 1': {
    zoneId: 'zone-f1',
    zoneName: 'Flower Room 1',
    method: 'container',
    targetVolumeMl: 500,
    measuredTimeSeconds: 120,
    runsCount: 1,
    emitterFlowMlPerSecond: 2.08, // 500mL / 120s / 2 emitters
    emittersPerPlant: 2,
    mediaVolume: 1,
    mediaVolumeUnit: 'gallons',
    calibratedByUserId: 'user-1',
    calibratedAt: new Date().toISOString(),
  },
  'Flower Room 2': {
    zoneId: 'zone-f2',
    zoneName: 'Flower Room 2',
    method: 'container',
    targetVolumeMl: 500,
    measuredTimeSeconds: 100,
    runsCount: 1,
    emitterFlowMlPerSecond: 2.5, // 500mL / 100s / 2 emitters
    emittersPerPlant: 2,
    mediaVolume: 1,
    mediaVolumeUnit: 'gallons',
    calibratedByUserId: 'user-1',
    calibratedAt: new Date().toISOString(),
  },
  'Veg Room 1': {
    zoneId: 'zone-v1',
    zoneName: 'Veg Room 1',
    method: 'container',
    targetVolumeMl: 500,
    measuredTimeSeconds: 150,
    runsCount: 1,
    emitterFlowMlPerSecond: 3.33, // 500mL / 150s / 1 emitter
    emittersPerPlant: 1,
    mediaVolume: 0.5,
    mediaVolumeUnit: 'gallons',
    calibratedByUserId: 'user-1',
    calibratedAt: new Date().toISOString(),
  },
  // Note: Veg Room 2 and Clone Room are NOT calibrated to demonstrate warning state
};

// Mock data - in production this would come from API
const INITIAL_PROGRAMS: IrrigationProgram[] = [
  {
    id: 'prog-1',
    name: 'F1 Morning Ramp',
    description: 'Morning saturation for Flower Room 1',
    type: 'ramp',
    targetZones: ['Flower Room 1'],
    shotConfig: { ...DEFAULT_SHOT_CONFIG, shotSizeMl: 50, maxShotsPerDay: 12 },
    phaseTargets: { ...DEFAULT_PHASE_TARGETS, p1ShotCount: 6 },
    nightProfile: DEFAULT_NIGHT_PROFILE,
    safetyPolicy: DEFAULT_SAFETY_POLICY,
    schedule: {
      ...DEFAULT_SCHEDULE,
      triggerType: 'time',
      startTime: '06:00',
      days: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
    },
    calibration: null,
    enabled: true,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: 'prog-2',
    name: 'F1 Sensor Maintenance',
    description: 'VWC-triggered maintenance shots',
    type: 'maintenance',
    targetZones: ['Flower Room 1'],
    shotConfig: { ...DEFAULT_SHOT_CONFIG, shotSizeMl: 40, maxShotsPerDay: 8 },
    phaseTargets: { ...DEFAULT_PHASE_TARGETS, p2ShotCount: 4 },
    nightProfile: DEFAULT_NIGHT_PROFILE,
    safetyPolicy: DEFAULT_SAFETY_POLICY,
    schedule: {
      ...DEFAULT_SCHEDULE,
      triggerType: 'sensor',
      startTime: '08:00',
      sensorTriggers: [{ sensorIds: ['sens-f1-1'], aggregation: 'average', metric: 'VWC', operator: '<', value: 55, unit: '%' }],
    },
    calibration: null,
    enabled: true,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: 'prog-3',
    name: 'V1 Hybrid Schedule',
    description: 'Time window with sensor confirmation',
    type: 'maintenance',
    targetZones: ['Veg Room 1'],
    shotConfig: { ...DEFAULT_SHOT_CONFIG, shotSizeMl: 45 },
    phaseTargets: DEFAULT_PHASE_TARGETS,
    nightProfile: DEFAULT_NIGHT_PROFILE,
    safetyPolicy: DEFAULT_SAFETY_POLICY,
    schedule: {
      ...DEFAULT_SCHEDULE,
      triggerType: 'hybrid',
      startTime: '10:00',
      days: ['Mon', 'Wed', 'Fri'],
      sensorTriggers: [{ sensorIds: ['sens-v1-1'], aggregation: 'average', metric: 'VWC', operator: '<', value: 60, unit: '%' }],
      enabled: false,
    },
    calibration: null,
    enabled: true,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: 'prog-4',
    name: 'System Flush',
    description: 'Weekly line flush for all zones',
    type: 'flush',
    targetZones: ['Flower Room 1', 'Flower Room 2', 'Veg Room 1'],
    shotConfig: { ...DEFAULT_SHOT_CONFIG, shotSizeMl: 100, maxShotsPerDay: 2 },
    phaseTargets: DEFAULT_PHASE_TARGETS,
    nightProfile: DEFAULT_NIGHT_PROFILE,
    safetyPolicy: DEFAULT_SAFETY_POLICY,
    schedule: {
      ...DEFAULT_SCHEDULE,
      triggerType: 'time',
      startTime: '05:00',
      days: ['Sun'],
    },
    calibration: null,
    enabled: true,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
];

export default function ProgramsPage() {
  const [programs, setPrograms] = useState<IrrigationProgram[]>(INITIAL_PROGRAMS);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingProgram, setEditingProgram] = useState<IrrigationProgram | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [runningProgramId, setRunningProgramId] = useState<string | null>(null);

  const handleCreateProgram = useCallback(() => {
    setEditingProgram(null);
    setIsModalOpen(true);
  }, []);

  const handleEditProgram = useCallback((program: IrrigationProgram) => {
    setEditingProgram(program);
    setIsModalOpen(true);
  }, []);

  const handleDeleteProgram = useCallback((programId: string) => {
    if (confirm('Are you sure you want to delete this program?')) {
      setPrograms(prev => prev.filter(p => p.id !== programId));
    }
  }, []);

  const handleToggleProgramEnabled = useCallback((programId: string) => {
    setPrograms(prev => prev.map(p => 
      p.id === programId ? { ...p, enabled: !p.enabled } : p
    ));
  }, []);

  const handleRunNow = useCallback((programId: string) => {
    setRunningProgramId(programId);
    // Simulate running
    setTimeout(() => {
      setRunningProgramId(null);
    }, 3000);
  }, []);

  const handleSaveProgram = useCallback((data: ProgramFormData) => {
    setIsSubmitting(true);
    
    // Simulate API call
    setTimeout(() => {
      if (editingProgram) {
        // Update existing
        setPrograms(prev => prev.map(p => 
          p.id === editingProgram.id
            ? { ...p, ...data, updatedAt: new Date().toISOString() }
            : p
        ));
      } else {
        // Create new
        const newProgram: IrrigationProgram = {
          id: `prog-${Date.now()}`,
          ...data,
          calibration: null,
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
        };
        setPrograms(prev => [...prev, newProgram]);
      }
      
      setIsSubmitting(false);
      setIsModalOpen(false);
      setEditingProgram(null);
    }, 500);
  }, [editingProgram]);

  const getScheduleIcon = (type: IrrigationProgram['schedule']['triggerType']) => {
    switch (type) {
      case 'time':
        return <Clock className="w-3 h-3 text-cyan-400" />;
      case 'sensor':
        return <Gauge className="w-3 h-3 text-emerald-400" />;
      case 'hybrid':
        return <Zap className="w-3 h-3 text-amber-400" />;
    }
  };

  const enabledCount = programs.filter(p => p.enabled && p.schedule.enabled).length;
  const totalShots = programs.reduce((acc, p) => 
    p.enabled ? acc + p.phaseTargets.p1ShotCount + p.phaseTargets.p2ShotCount : acc, 0
  );

  return (
    <div className="max-w-5xl mx-auto">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h2 className="text-lg font-bold text-foreground">Irrigation Programs</h2>
          <p className="text-sm text-muted-foreground">
            Configure complete irrigation programs with embedded schedules
          </p>
        </div>
        <button 
          onClick={handleCreateProgram}
          className="flex items-center gap-2 px-4 py-2 bg-cyan-600 hover:bg-cyan-500 text-foreground rounded-lg font-medium transition-colors"
        >
          <Plus className="w-4 h-4" />
          Create Program
        </button>
      </div>

      {/* Stats Bar */}
      <div className="grid grid-cols-3 gap-4 mb-6">
        <div className="bg-surface/50 border border-border rounded-xl p-4">
          <div className="text-2xl font-bold text-foreground">{programs.length}</div>
          <div className="text-xs text-muted-foreground">Total Programs</div>
        </div>
        <div className="bg-surface/50 border border-border rounded-xl p-4">
          <div className="text-2xl font-bold text-emerald-400">{enabledCount}</div>
          <div className="text-xs text-muted-foreground">Active Schedules</div>
        </div>
        <div className="bg-surface/50 border border-border rounded-xl p-4">
          <div className="text-2xl font-bold text-cyan-400">{totalShots}</div>
          <div className="text-xs text-muted-foreground">Daily Shot Capacity</div>
        </div>
      </div>

      {/* Program List */}
      <div className="space-y-3">
        {programs.length === 0 ? (
          <div className="bg-surface/50 border border-border rounded-xl p-8 text-center">
            <Droplets className="w-12 h-12 text-muted-foreground/40 mx-auto mb-3" />
            <h3 className="font-medium text-foreground mb-1">No Programs Yet</h3>
            <p className="text-sm text-muted-foreground mb-4">
              Create your first irrigation program to get started
            </p>
            <button 
              onClick={handleCreateProgram}
              className="inline-flex items-center gap-2 px-4 py-2 bg-cyan-600 hover:bg-cyan-500 text-foreground rounded-lg font-medium transition-colors"
            >
              <Plus className="w-4 h-4" />
              Create Program
            </button>
          </div>
        ) : (
          programs.map(program => (
            <div 
              key={program.id} 
              className={cn(
                'bg-surface/50 border rounded-xl p-4 transition-all group',
                program.enabled 
                  ? 'border-border hover:border-cyan-500/30' 
                  : 'border-border/50 opacity-60'
              )}
            >
              <div className="flex items-center gap-4">
                {/* Type Badge */}
                <div className={cn(
                  'w-12 h-12 rounded-xl flex items-center justify-center text-lg font-bold',
                  getProgramTypeColor(program.type)
                )}>
                  {program.type.charAt(0).toUpperCase()}
                </div>

                {/* Info */}
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2 mb-1">
                    <h3 className="font-bold text-foreground truncate">{program.name}</h3>
                    <span className={cn(
                      'text-[10px] uppercase font-bold px-1.5 py-0.5 rounded border',
                      getProgramTypeColor(program.type)
                    )}>
                      {program.type}
                    </span>
                    {!program.enabled && (
                      <span className="text-[10px] uppercase font-bold px-1.5 py-0.5 rounded border bg-rose-500/10 text-rose-400 border-rose-500/20">
                        Disabled
                      </span>
                    )}
                    {program.enabled && !program.schedule.enabled && (
                      <span className="text-[10px] uppercase font-bold px-1.5 py-0.5 rounded border bg-amber-500/10 text-amber-400 border-amber-500/20">
                        Schedule Paused
                      </span>
                    )}
                    {runningProgramId === program.id && (
                      <span className="text-[10px] uppercase font-bold px-1.5 py-0.5 rounded border bg-emerald-500/10 text-emerald-400 border-emerald-500/20 animate-pulse">
                        Running...
                      </span>
                    )}
                  </div>

                  <div className="flex items-center gap-3 text-xs text-muted-foreground">
                    <span className="flex items-center gap-1">
                      {getScheduleIcon(program.schedule.triggerType)}
                      {program.schedule.startTime}
                    </span>
                    <span>•</span>
                    <span>{formatDays(program.schedule.days)}</span>
                    <span>•</span>
                    <span>{program.targetZones.join(', ')}</span>
                  </div>

                  <div className="flex items-center gap-3 mt-1 text-xs">
                    <span className="text-cyan-400/80">
                      {program.phaseTargets.p1ShotCount + program.phaseTargets.p2ShotCount} shots × {program.shotConfig.shotSizeMl}mL
                    </span>
                    <span className="text-muted-foreground/60">•</span>
                    <span className="text-muted-foreground/80">
                      {program.shotConfig.minSoakTimeMinutes}min soak
                    </span>
                    {program.schedule.sensorTriggers.length > 0 && (
                      <>
                        <span className="text-muted-foreground/60">•</span>
                        <span className="text-emerald-400/80">
                          {program.schedule.sensorTriggers.map(t => 
                            `${t.metric} ${t.operator} ${t.value}${t.unit}`
                          ).join(', ')}
                        </span>
                      </>
                    )}
                  </div>
                </div>

                {/* Actions */}
                <div className="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                  <button
                    onClick={() => handleRunNow(program.id)}
                    disabled={!program.enabled || runningProgramId !== null}
                    className={cn(
                      'p-2 rounded-lg transition-colors',
                      runningProgramId === program.id
                        ? 'bg-emerald-500/20 text-emerald-400'
                        : 'hover:bg-emerald-500/20 text-emerald-400'
                    )}
                    title="Run Now"
                  >
                    {runningProgramId === program.id ? (
                      <CheckCircle className="w-4 h-4 animate-spin" />
                    ) : (
                      <Play className="w-4 h-4" />
                    )}
                  </button>
                  <button
                    onClick={() => handleToggleProgramEnabled(program.id)}
                    className={cn(
                      'p-2 rounded-lg transition-colors',
                      program.enabled
                        ? 'hover:bg-amber-500/20 text-amber-400'
                        : 'hover:bg-emerald-500/20 text-emerald-400'
                    )}
                    title={program.enabled ? 'Disable Program' : 'Enable Program'}
                  >
                    {program.enabled ? <Pause className="w-4 h-4" /> : <Play className="w-4 h-4" />}
                  </button>
                  <button
                    onClick={() => handleEditProgram(program)}
                    className="p-2 hover:bg-muted text-muted-foreground hover:text-foreground rounded-lg transition-colors"
                    title="Edit Program"
                  >
                    <Edit2 className="w-4 h-4" />
                  </button>
                  <button
                    onClick={() => handleDeleteProgram(program.id)}
                    className="p-2 hover:bg-rose-500/20 text-rose-400 rounded-lg transition-colors"
                    title="Delete Program"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
              </div>
            </div>
          ))
        )}
      </div>

      {/* Modal */}
      <AddEditProgramModal
        isOpen={isModalOpen}
        onClose={() => {
          setIsModalOpen(false);
          setEditingProgram(null);
        }}
        program={editingProgram}
        onSave={handleSaveProgram}
        isSubmitting={isSubmitting}
        zoneCalibrations={MOCK_ZONE_CALIBRATIONS}
      />
    </div>
  );
}
