'use client';

import React, { useState } from 'react';
import Link from 'next/link';
import {
  Calendar as CalendarIcon,
  Clock,
  ArrowRight,
  Gauge,
  Zap,
  Pause,
  Play,
  Settings,
  ChevronRight,
  Droplets,
  AlertCircle,
  TrendingUp,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { 
  QueueDelayBadge, 
  QueueInfo,
  CompactSuggestionBanner,
  SmartSuggestionCard,
  ScheduleSuggestion,
} from '@/components/irrigation';

/**
 * Program Schedule - represents a schedule embedded within a program
 * Schedules are owned by programs, not standalone entities
 */
interface ProgramSchedule {
  programId: string;
  programName: string;
  programType: 'ramp' | 'maintenance' | 'dryback' | 'flush';
  /** Schedule trigger configuration */
  schedule: {
    type: 'time' | 'sensor' | 'hybrid';
    time: string;
    days: string;
    sensorTrigger?: string;
  };
  /** Shot configuration from program */
  shotConfig: {
    shotSizeMl: number;
    shotCount: number;
    soakMinutes: number;
  };
  targetZones: string;
  enabled: boolean;
  nextRun?: string;
  /** Queue information if event is delayed */
  queueInfo?: QueueInfo;
}

// Mock data - in reality, this would come from programs
const PROGRAM_SCHEDULES: ProgramSchedule[] = [
  {
    programId: 'prog-1',
    programName: 'F1 Morning Ramp',
    programType: 'ramp',
    schedule: {
      type: 'time',
      time: '06:00',
      days: 'Daily',
    },
    shotConfig: {
      shotSizeMl: 50,
      shotCount: 6,
      soakMinutes: 30,
    },
    targetZones: 'Flower Room 1',
    enabled: true,
    nextRun: 'Tomorrow 6:00 AM',
    // Example: This event is queued due to flow rate limits
    queueInfo: {
      isQueued: true,
      originalTime: '6:00 AM',
      expectedTime: '6:12 AM',
      delayMinutes: 12,
      queuePosition: 2,
      reason: 'Flow rate limit: 125 L/min would exceed 114 L/min limit',
    },
  },
  {
    programId: 'prog-2',
    programName: 'F1 Sensor Maintenance',
    programType: 'maintenance',
    schedule: {
      type: 'sensor',
      time: '08:00',
      days: 'Daily',
      sensorTrigger: 'VWC < 55%',
    },
    shotConfig: {
      shotSizeMl: 40,
      shotCount: 4,
      soakMinutes: 45,
    },
    targetZones: 'Flower Room 1',
    enabled: true,
    nextRun: 'When VWC < 55%',
  },
  {
    programId: 'prog-3',
    programName: 'V1 Hybrid Schedule',
    programType: 'maintenance',
    schedule: {
      type: 'hybrid',
      time: '10:00',
      days: 'Mon, Wed, Fri',
      sensorTrigger: 'VWC < 60%',
    },
    shotConfig: {
      shotSizeMl: 45,
      shotCount: 3,
      soakMinutes: 30,
    },
    targetZones: 'Veg Room 1',
    enabled: false,
    nextRun: 'Paused',
  },
  {
    programId: 'prog-4',
    programName: 'F2 Morning Ramp',
    programType: 'ramp',
    schedule: {
      type: 'time',
      time: '06:00',
      days: 'Daily',
    },
    shotConfig: {
      shotSizeMl: 50,
      shotCount: 6,
      soakMinutes: 30,
    },
    targetZones: 'Flower Room 2',
    enabled: true,
    nextRun: 'Tomorrow 6:08 AM',
    // Example: This event is also queued
    queueInfo: {
      isQueued: true,
      originalTime: '6:00 AM',
      expectedTime: '6:08 AM',
      delayMinutes: 8,
      queuePosition: 1,
      reason: 'Waiting for F1 Morning Ramp to complete',
    },
  },
];

// Mock smart suggestions
const MOCK_SUGGESTIONS: ScheduleSuggestion[] = [
  {
    id: 'sug-1',
    type: 'time-shift',
    title: 'Stagger morning ramp schedules',
    description: 'F1 and F2 Morning Ramp are both scheduled at 6:00 AM, causing queue delays. Consider shifting F2 to 6:15 AM.',
    currentValue: 'Both at 6:00 AM',
    suggestedValue: 'F2 at 6:15 AM',
    estimatedImpactMinutes: 12,
    priority: 'high',
    affectedPrograms: ['F1 Morning Ramp', 'F2 Morning Ramp'],
  },
  {
    id: 'sug-2',
    type: 'sequential',
    title: 'Use zone sequencing for large rooms',
    description: 'Flower Room 1 runs all zones simultaneously. Consider sequencing zones A-B then C-D to reduce peak flow.',
    currentValue: 'Simultaneous (4 zones)',
    suggestedValue: 'Sequential (2+2 zones)',
    estimatedImpactMinutes: 8,
    priority: 'medium',
    affectedPrograms: ['F1 Morning Ramp', 'F1 Sensor Maintenance'],
  },
];

export default function SchedulesPage() {
  const [schedules] = useState<ProgramSchedule[]>(PROGRAM_SCHEDULES);
  const [suggestions] = useState<ScheduleSuggestion[]>(MOCK_SUGGESTIONS);
  const [showSuggestions, setShowSuggestions] = useState(false);
  const [dismissedSuggestions, setDismissedSuggestions] = useState<Set<string>>(new Set());

  const getScheduleTypeIcon = (type: 'time' | 'sensor' | 'hybrid') => {
    switch (type) {
      case 'time':
        return <Clock className="w-4 h-4 text-cyan-400" />;
      case 'sensor':
        return <Gauge className="w-4 h-4 text-emerald-400" />;
      case 'hybrid':
        return <Zap className="w-4 h-4 text-amber-400" />;
    }
  };

  const getProgramTypeColor = (type: string) => {
    switch (type) {
      case 'ramp':
        return 'text-cyan-400 bg-cyan-500/10 border-cyan-500/20';
      case 'maintenance':
        return 'text-emerald-400 bg-emerald-500/10 border-emerald-500/20';
      case 'dryback':
        return 'text-amber-400 bg-amber-500/10 border-amber-500/20';
      case 'flush':
        return 'text-violet-400 bg-violet-500/10 border-violet-500/20';
      default:
        return 'text-muted-foreground bg-muted border-border';
    }
  };

  const activeCount = schedules.filter((s) => s.enabled).length;
  const pausedCount = schedules.filter((s) => !s.enabled).length;
  const queuedCount = schedules.filter((s) => s.queueInfo?.isQueued).length;
  
  const activeSuggestions = suggestions.filter(s => !dismissedSuggestions.has(s.id));

  const handleDismissSuggestion = (id: string) => {
    setDismissedSuggestions(prev => new Set([...prev, id]));
  };

  return (
    <div className="max-w-5xl mx-auto">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h2 className="text-lg font-bold text-foreground">Schedule Overview</h2>
          <p className="text-sm text-muted-foreground">
            View and manage when irrigation programs run
          </p>
        </div>
        <Link
          href="/dashboard/irrigation/programs"
          className="flex items-center gap-2 px-4 py-2 bg-cyan-600 hover:bg-cyan-500 text-foreground rounded-lg font-medium transition-colors"
        >
          <Settings className="w-4 h-4" />
          Manage Programs
        </Link>
      </div>

      {/* Queue Status Alert */}
      {queuedCount > 0 && (
        <div className="flex items-start gap-3 p-4 mb-4 rounded-xl bg-amber-500/5 border border-amber-500/20">
          <Clock className="w-5 h-5 text-amber-400 shrink-0 mt-0.5" />
          <div className="flex-1">
            <p className="text-sm text-foreground font-medium">
              {queuedCount} schedule{queuedCount !== 1 ? 's' : ''} queued due to flow rate limits
            </p>
            <p className="text-xs text-muted-foreground mt-1">
              Some events will run later than scheduled to stay within your system flow rate capacity.
              Look for the <span className="text-amber-400 font-medium">+[time]</span> indicators below.
            </p>
          </div>
          {activeSuggestions.length > 0 && !showSuggestions && (
            <button
              onClick={() => setShowSuggestions(true)}
              className="flex items-center gap-2 px-3 py-1.5 text-xs font-medium rounded-lg bg-amber-500/20 hover:bg-amber-500/30 text-amber-400 transition-colors shrink-0"
            >
              <TrendingUp className="w-3.5 h-3.5" />
              View {activeSuggestions.length} suggestion{activeSuggestions.length !== 1 ? 's' : ''}
            </button>
          )}
        </div>
      )}

      {/* Smart Suggestions Panel */}
      {showSuggestions && activeSuggestions.length > 0 && (
        <div className="mb-6">
          <SmartSuggestionCard
            suggestions={activeSuggestions}
            onDismiss={handleDismissSuggestion}
            onApply={(suggestion) => {
              console.log('Apply suggestion:', suggestion);
              // In real app, this would navigate to program editor or apply changes
            }}
          />
          <button
            onClick={() => setShowSuggestions(false)}
            className="mt-2 text-xs text-muted-foreground hover:text-foreground transition-colors"
          >
            Hide suggestions
          </button>
        </div>
      )}

      {/* Info Banner */}
      <div className="flex items-start gap-3 p-4 mb-6 rounded-xl bg-cyan-500/5 border border-cyan-500/20">
        <AlertCircle className="w-5 h-5 text-cyan-400 shrink-0 mt-0.5" />
        <div>
          <p className="text-sm text-foreground font-medium">
            Schedules are configured within Programs
          </p>
          <p className="text-xs text-muted-foreground mt-1">
            Each irrigation program contains its own schedule settings (timing, triggers, shot configuration). 
            Click on a program below to edit its schedule.
          </p>
        </div>
      </div>

      {/* Main Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Schedule List */}
        <div className="lg:col-span-2 space-y-3">
          <div className="flex items-center justify-between mb-2">
            <h3 className="text-sm font-bold text-muted-foreground uppercase tracking-wider">
              Program Schedules
            </h3>
            <span className="text-xs text-muted-foreground">
              {activeCount} active, {pausedCount} paused
            </span>
          </div>

          {schedules.length === 0 ? (
            <div className="bg-surface/50 border border-border rounded-xl p-8 text-center">
              <Droplets className="w-10 h-10 text-muted-foreground/40 mx-auto mb-3" />
              <p className="text-muted-foreground text-sm">No programs configured</p>
              <Link
                href="/dashboard/irrigation/programs"
                className="mt-4 inline-block text-cyan-400 hover:text-cyan-300 text-sm font-medium transition-colors"
              >
                Create your first program →
              </Link>
            </div>
          ) : (
            schedules.map((item) => (
              <Link
                key={item.programId}
                href={`/dashboard/irrigation/programs?edit=${item.programId}`}
                className={cn(
                  'block bg-surface/50 border rounded-xl p-4 transition-all group',
                  item.enabled
                    ? 'border-border hover:border-cyan-500/30'
                    : 'border-border/50 opacity-60 hover:opacity-80'
                )}
              >
                <div className="flex items-center gap-4">
                  {/* Time/Type Badge */}
                  <div className="flex flex-col items-center justify-center w-14 h-14 bg-muted rounded-lg border border-border relative">
                    {getScheduleTypeIcon(item.schedule.type)}
                    <span className="text-xs font-bold text-foreground mt-1">
                      {item.schedule.time}
                    </span>
                    {/* Queue delay indicator on time badge */}
                    {item.queueInfo?.isQueued && (
                      <div className="absolute -top-1 -right-1">
                        <QueueDelayBadge 
                          queueInfo={item.queueInfo} 
                          size="sm"
                          showTooltip={true}
                        />
                      </div>
                    )}
                  </div>

                  {/* Program Info */}
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 mb-1 flex-wrap">
                      <h4 className="font-bold text-foreground truncate">
                        {item.programName}
                      </h4>
                      <span
                        className={cn(
                          'text-[10px] uppercase font-medium px-1.5 py-0.5 rounded border',
                          getProgramTypeColor(item.programType)
                        )}
                      >
                        {item.programType}
                      </span>
                      {!item.enabled && (
                        <span className="flex items-center gap-1 text-[10px] uppercase font-bold px-2 py-0.5 rounded border bg-amber-500/10 text-amber-400 border-amber-500/20">
                          <Pause className="w-3 h-3" />
                          Paused
                        </span>
                      )}
                      {item.queueInfo?.isQueued && (
                        <span className="flex items-center gap-1 text-[10px] uppercase font-bold px-2 py-0.5 rounded border bg-amber-500/10 text-amber-400 border-amber-500/20">
                          <Clock className="w-3 h-3" />
                          Queued
                        </span>
                      )}
                    </div>

                    <div className="flex items-center gap-2 text-xs text-muted-foreground">
                      <span>{item.schedule.days}</span>
                      <ArrowRight className="w-3 h-3 text-muted-foreground/60" />
                      <span>{item.targetZones}</span>
                    </div>

                    {/* Shot Config Summary */}
                    <div className="flex items-center gap-3 mt-1.5 text-xs">
                      <span className="text-cyan-400/80">
                        {item.shotConfig.shotCount} shots × {item.shotConfig.shotSizeMl}mL
                      </span>
                      <span className="text-muted-foreground/60">•</span>
                      <span className="text-muted-foreground/80">
                        {item.shotConfig.soakMinutes}min between
                      </span>
                      {item.schedule.sensorTrigger && (
                        <>
                          <span className="text-muted-foreground/60">•</span>
                          <span className="text-emerald-400/80">
                            {item.schedule.sensorTrigger}
                          </span>
                        </>
                      )}
                    </div>

                    {/* Next Run - with queue delay info */}
                    <div className="mt-1 text-[10px] text-muted-foreground/60">
                      {item.queueInfo?.isQueued ? (
                        <span>
                          Originally: {item.queueInfo.originalTime} → 
                          <span className="text-amber-400 font-medium ml-1">
                            Expected: {item.queueInfo.expectedTime}
                          </span>
                          <span className="text-amber-400/70 ml-1">
                            (#{item.queueInfo.queuePosition} in queue)
                          </span>
                        </span>
                      ) : (
                        <span>Next: {item.nextRun}</span>
                      )}
                    </div>
                  </div>

                  {/* Edit Arrow */}
                  <ChevronRight className="w-5 h-5 text-muted-foreground/40 group-hover:text-cyan-400 transition-colors shrink-0" />
                </div>
              </Link>
            ))
          )}
        </div>

        {/* Sidebar */}
        <div className="space-y-4">
          {/* Calendar Mini */}
          <div className="bg-surface/30 border border-border rounded-xl p-4">
            <div className="flex items-center gap-2 mb-4 text-foreground/70">
              <CalendarIcon className="w-4 h-4" />
              <span className="font-bold">Today's Schedule</span>
            </div>
            <div className="space-y-2">
              {schedules
                .filter((s) => s.enabled)
                .slice(0, 4)
                .map((item) => (
                  <div
                    key={item.programId}
                    className="flex items-center gap-2 p-2 rounded-lg bg-white/5"
                  >
                    <span className="text-xs font-mono text-cyan-400 w-12">
                      {item.schedule.time}
                    </span>
                    <span className="text-xs text-foreground truncate flex-1">
                      {item.programName}
                    </span>
                  </div>
                ))}
            </div>
          </div>

          {/* Quick Stats */}
          <div className="bg-surface/30 border border-border rounded-xl p-4">
            <h4 className="font-bold text-sm text-foreground/70 mb-3">Summary</h4>
            <div className="space-y-2">
              <div className="flex items-center justify-between text-sm">
                <span className="flex items-center gap-2 text-muted-foreground">
                  <Play className="w-3 h-3 text-emerald-400" />
                  Active Programs
                </span>
                <span className="font-bold text-emerald-400">{activeCount}</span>
              </div>
              <div className="flex items-center justify-between text-sm">
                <span className="flex items-center gap-2 text-muted-foreground">
                  <Pause className="w-3 h-3 text-amber-400" />
                  Paused
                </span>
                <span className="font-bold text-amber-400">{pausedCount}</span>
              </div>
              {queuedCount > 0 && (
                <div className="flex items-center justify-between text-sm">
                  <span className="flex items-center gap-2 text-muted-foreground">
                    <Clock className="w-3 h-3 text-orange-400" />
                    Queued
                  </span>
                  <span className="font-bold text-orange-400">{queuedCount}</span>
                </div>
              )}
              <div className="flex items-center justify-between text-sm">
                <span className="flex items-center gap-2 text-muted-foreground">
                  <Clock className="w-3 h-3 text-cyan-400" />
                  Time-based
                </span>
                <span className="font-bold text-foreground">
                  {schedules.filter((s) => s.schedule.type === 'time').length}
                </span>
              </div>
              <div className="flex items-center justify-between text-sm">
                <span className="flex items-center gap-2 text-muted-foreground">
                  <Gauge className="w-3 h-3 text-emerald-400" />
                  Sensor-based
                </span>
                <span className="font-bold text-foreground">
                  {schedules.filter((s) => s.schedule.type === 'sensor').length}
                </span>
              </div>
            </div>
          </div>

          {/* Flow Rate Status */}
          <div className="bg-surface/30 border border-border rounded-xl p-4">
            <h4 className="font-bold text-sm text-foreground/70 mb-3">Flow Rate Status</h4>
            <div className="space-y-3">
              <div>
                <div className="flex items-center justify-between text-xs mb-1">
                  <span className="text-muted-foreground">Current Usage</span>
                  <span className="text-foreground font-medium">45.2 / 114 L/min</span>
                </div>
                <div className="h-2 bg-muted rounded-full overflow-hidden">
                  <div 
                    className="h-full bg-emerald-500 rounded-full"
                    style={{ width: '39.6%' }}
                  />
                </div>
              </div>
              <div className="text-xs text-muted-foreground">
                <span className="text-emerald-400 font-medium">39.6%</span> of effective limit
              </div>
              {queuedCount > 0 && (
                <div className="p-2 bg-amber-500/10 border border-amber-500/20 rounded-lg text-xs text-amber-400">
                  {queuedCount} event{queuedCount !== 1 ? 's' : ''} queued to prevent exceeding flow rate
                </div>
              )}
            </div>
          </div>

          {/* Quick Actions */}
          <div className="bg-surface/30 border border-border rounded-xl p-4">
            <h4 className="font-bold text-sm text-foreground/70 mb-3">Quick Actions</h4>
            <div className="space-y-2">
              <Link
                href="/dashboard/irrigation/programs"
                className="flex items-center justify-between p-2 rounded-lg bg-white/5 hover:bg-white/10 transition-colors text-sm text-foreground"
              >
                <span>Create New Program</span>
                <ChevronRight className="w-4 h-4 text-muted-foreground" />
              </Link>
              <Link
                href="/dashboard/irrigation/history"
                className="flex items-center justify-between p-2 rounded-lg bg-white/5 hover:bg-white/10 transition-colors text-sm text-foreground"
              >
                <span>View Run History</span>
                <ChevronRight className="w-4 h-4 text-muted-foreground" />
              </Link>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
