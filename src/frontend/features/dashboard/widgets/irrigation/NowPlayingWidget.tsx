import React from 'react';
import { cn } from '@/lib/utils';
import Link from 'next/link';
import { Pause, Square, Clock, ChevronRight } from 'lucide-react';

interface ActiveRun {
  id: string;
  programName: string;
  currentStepIndex: number;
  totalSteps: number;
  currentStepName: string; // e.g., "Shot (75mL)"
  progressPercent: number;
  zones: string[];
  etaMinutes: number;
  status: 'running' | 'paused' | 'waiting';
}

export function NowPlayingWidget() {
  // Mock Data - Multiple Active Runs
  const activeRuns: ActiveRun[] = [
    {
      id: 'run-123',
      programName: 'P1 - Morning Ramp',
      currentStepIndex: 2,
      totalSteps: 5,
      currentStepName: 'Shot (75mL)',
      progressPercent: 40,
      zones: ['A', 'B', 'C'],
      etaMinutes: 12,
      status: 'running'
    },
    {
      id: 'run-124',
      programName: 'P2 - Maintenance',
      currentStepIndex: 1,
      totalSteps: 3,
      currentStepName: 'Flush Line',
      progressPercent: 15,
      zones: ['D', 'E'],
      etaMinutes: 45,
      status: 'running'
    },
    {
      id: 'run-125',
      programName: 'Filter Backwash',
      currentStepIndex: 3,
      totalSteps: 3,
      currentStepName: 'Rinse',
      progressPercent: 85,
      zones: ['System'],
      etaMinutes: 2,
      status: 'paused'
    }
  ];

  if (!activeRuns || activeRuns.length === 0) {
    return (
      <div className="h-full min-h-[160px] bg-surface/50 border border-border rounded-xl p-6 flex flex-col items-center justify-center text-muted-foreground">
        <div className="w-12 h-12 rounded-full bg-muted flex items-center justify-center mb-3">
          <Square className="w-5 h-5 opacity-50" />
        </div>
        <p className="text-sm font-medium">No active irrigation runs</p>
        <p className="text-xs opacity-70">System is idle</p>
      </div>
    );
  }

  // Single Run View (Detailed)
  if (activeRuns.length === 1) {
    const run = activeRuns[0];
    return (
      <div className="h-full min-h-[160px] bg-surface/50 border border-border rounded-xl p-5 flex flex-col relative overflow-hidden group">
        <div className="absolute -top-10 -right-10 w-32 h-32 bg-cyan-500/5 rounded-full blur-3xl animate-pulse" />
        
        {/* Header */}
        <div className="flex items-center justify-between mb-4 z-10">
          <div className="flex items-center gap-2">
            <div className="relative">
              <div className="w-2.5 h-2.5 bg-emerald-500 rounded-full animate-pulse" />
              <div className="absolute inset-0 bg-emerald-500 rounded-full animate-ping opacity-20" />
            </div>
            <h3 className="text-sm font-bold text-foreground uppercase tracking-wide">Now Playing</h3>
          </div>
          <div className="flex items-center gap-2">
            <button className="p-1.5 hover:bg-muted rounded text-muted-foreground hover:text-foreground transition-colors">
              <Pause className="w-4 h-4" />
            </button>
            <button className="p-1.5 hover:bg-muted rounded text-rose-400 hover:text-rose-300 transition-colors">
              <Square className="w-4 h-4" />
            </button>
          </div>
        </div>

        {/* Content */}
        <div className="flex-1 z-10">
          <h2 className="text-lg font-bold text-foreground mb-1">{run.programName}</h2>
          <div className="text-sm text-cyan-400 font-medium mb-4 flex items-center gap-2">
             <span>{run.currentStepName}</span>
             <span className="text-muted-foreground/50">•</span>
             <span className="text-muted-foreground text-xs">Step {run.currentStepIndex} of {run.totalSteps}</span>
          </div>

          <div className="space-y-1.5 mb-4">
            <div className="flex justify-between text-xs font-medium">
               <span className="text-muted-foreground">Progress</span>
               <span className="text-foreground">{run.progressPercent}%</span>
            </div>
            <div className="h-2 w-full bg-muted rounded-full overflow-hidden">
              <div 
                className="h-full bg-gradient-to-r from-cyan-500 to-blue-500 rounded-full transition-all duration-1000 ease-linear"
                style={{ width: `${run.progressPercent}%` }}
              />
            </div>
          </div>

          <div className="flex items-center justify-between text-xs">
            <div className="flex items-center gap-2">
              <span className="text-muted-foreground">Zones:</span>
              <div className="flex gap-1">
                {run.zones.map(z => (
                  <span key={z} className="px-1.5 py-0.5 bg-emerald-500/10 text-emerald-400 border border-emerald-500/20 rounded font-medium">{z}</span>
                ))}
              </div>
            </div>
            <div className="flex items-center gap-1.5 text-foreground/70 bg-muted/50 px-2 py-1 rounded">
               <Clock className="w-3 h-3 text-cyan-500" />
               <span>{run.etaMinutes}m left</span>
            </div>
          </div>
        </div>
      </div>
    );
  }

  // Multiple Runs View (Compact List)
  return (
    <div className="h-full min-h-[160px] bg-surface/50 border border-border rounded-xl p-5 flex flex-col relative overflow-hidden">
      {/* Header */}
      <div className="flex items-center justify-between mb-3 z-10 shrink-0">
        <div className="flex items-center gap-2">
          <div className="flex items-center justify-center w-5 h-5 rounded-full bg-emerald-500/20 text-emerald-400 text-xs font-bold">
            {activeRuns.length}
          </div>
          <h3 className="text-sm font-bold text-foreground uppercase tracking-wide">Active Runs</h3>
        </div>
        <Link 
          href="/dashboard/irrigation/history"
          className="text-xs text-cyan-400 hover:text-cyan-300 font-medium flex items-center gap-1 transition-colors"
        >
          View All <ChevronRight className="w-3 h-3" />
        </Link>
      </div>

      {/* Scrollable List */}
      <div className="flex-1 overflow-y-auto custom-scrollbar -mr-2 pr-2 space-y-2">
        {activeRuns.map(run => (
          <div key={run.id} className="bg-muted/30 border border-border rounded-lg p-3 hover:bg-muted/50 transition-colors group">
            <div className="flex justify-between items-start mb-2">
              <div>
                <h4 className="text-sm font-bold text-foreground truncate">{run.programName}</h4>
                <div className="flex items-center gap-2 text-[10px] text-muted-foreground mt-0.5">
                  <span className="text-cyan-400">{run.currentStepName}</span>
                  <span>•</span>
                  <span>Step {run.currentStepIndex}/{run.totalSteps}</span>
                </div>
              </div>
              <div className="flex flex-col items-end gap-1">
                {run.status === 'paused' ? (
                  <span className="text-[10px] font-bold text-amber-400 uppercase bg-amber-500/10 px-1.5 py-0.5 rounded border border-amber-500/20">Paused</span>
                ) : (
                  <span className="text-[10px] font-bold text-muted-foreground bg-muted px-1.5 py-0.5 rounded border border-border flex items-center gap-1">
                    <Clock className="w-2.5 h-2.5" /> {run.etaMinutes}m
                  </span>
                )}
              </div>
            </div>

            {/* Mini Progress Bar */}
            <div className="h-1.5 w-full bg-background rounded-full overflow-hidden mb-2">
              <div 
                className={cn(
                  "h-full rounded-full transition-all duration-1000 ease-linear",
                  run.status === 'paused' ? "bg-amber-500/50" : "bg-cyan-500"
                )}
                style={{ width: `${run.progressPercent}%` }}
              />
            </div>

            {/* Footer */}
            <div className="flex justify-between items-center">
              <div className="flex gap-1">
                {run.zones.map(z => (
                  <span key={z} className="text-[10px] px-1.5 py-0.5 bg-emerald-500/10 text-emerald-400 border border-emerald-500/20 rounded font-medium">
                    {z}
                  </span>
                ))}
              </div>
              <div className="flex gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                <button className="p-1 hover:bg-muted rounded text-muted-foreground hover:text-foreground">
                  <Pause className="w-3 h-3" />
                </button>
                <button className="p-1 hover:bg-muted rounded text-rose-400 hover:text-rose-300">
                  <Square className="w-3 h-3" />
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
