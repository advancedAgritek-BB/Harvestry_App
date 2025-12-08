'use client';

import React from 'react';
import { Play, CalendarClock, AlertTriangle } from 'lucide-react';
import { cn } from '@/lib/utils';

interface Zone {
  id: string;
  name: string;
  status: 'running' | 'idle' | 'fault';
  program: string;
  progress?: number;
  nextRun?: string;
}

const MOCK_ZONES: Zone[] = [
  { id: '1', name: 'Zone 1', status: 'running', program: 'Flower P1', progress: 45 },
  { id: '2', name: 'Zone 2', status: 'idle', program: 'Flower P1', nextRun: '16:00' },
  { id: '3', name: 'Zone 3', status: 'idle', program: 'Veg P2', nextRun: '16:30' },
  { id: '4', name: 'Zone 4', status: 'fault', program: 'Veg P2' },
];

const STATUS_CONFIG = {
  running: {
    cardBg: 'bg-gradient-to-r from-cyan-500/15 via-cyan-500/5 to-transparent',
    shadow: 'shadow-md shadow-cyan-500/10',
    iconBg: 'bg-gradient-to-br from-cyan-500/30 to-cyan-600/20 ring-1 ring-cyan-400/30',
    iconColor: 'text-cyan-400',
    accentBar: 'bg-cyan-400',
    textColor: 'text-cyan-400',
  },
  fault: {
    cardBg: 'bg-gradient-to-r from-rose-500/15 via-rose-500/5 to-transparent',
    shadow: 'shadow-md shadow-rose-500/10',
    iconBg: 'bg-gradient-to-br from-rose-500/30 to-rose-600/20 ring-1 ring-rose-400/30',
    iconColor: 'text-rose-400',
    accentBar: 'bg-rose-400',
    textColor: 'text-rose-400',
  },
  idle: {
    cardBg: 'bg-white/[0.03]',
    shadow: 'shadow-sm shadow-black/5',
    iconBg: 'bg-white/5 ring-1 ring-white/10',
    iconColor: 'text-muted-foreground',
    accentBar: 'bg-muted-foreground/30',
    textColor: 'text-muted-foreground',
  },
};

export function IrrigationStatusWidget() {
  return (
    <div className="flex flex-col">
      <div className="space-y-2.5">
        {MOCK_ZONES.map((zone) => {
          const config = STATUS_CONFIG[zone.status];
          return (
            <div
              key={zone.id}
              className={cn(
                'group relative flex items-center gap-3 p-3 pl-4 rounded-xl',
                'transition-all duration-300 overflow-hidden',
                config.cardBg,
                config.shadow,
                'hover:shadow-lg hover:-translate-y-0.5'
              )}
            >
              {/* Left accent bar */}
              <div className={cn(
                'absolute left-0 top-2 bottom-2 w-1 rounded-full',
                config.accentBar,
                zone.status === 'running' && 'animate-pulse'
              )} />

              {/* Status Icon */}
              <div className={cn(
                'p-2 rounded-lg shrink-0 transition-transform group-hover:scale-105',
                config.iconBg
              )}>
                {zone.status === 'running' && <Play className={cn("w-4 h-4", config.iconColor)} />}
                {zone.status === 'idle' && <CalendarClock className={cn("w-4 h-4", config.iconColor)} />}
                {zone.status === 'fault' && <AlertTriangle className={cn("w-4 h-4 animate-pulse", config.iconColor)} />}
              </div>

              {/* Zone Info */}
              <div className="flex-1 min-w-0">
                <div className="flex items-center justify-between">
                  <span className="text-sm font-semibold text-foreground group-hover:text-white transition-colors">{zone.name}</span>
                  <span className={cn(
                    'text-sm font-mono tabular-nums',
                    config.textColor
                  )}>
                    {zone.status === 'running' && `${zone.progress}%`}
                    {zone.status === 'idle' && zone.nextRun && `Next: ${zone.nextRun}`}
                    {zone.status === 'fault' && 'Fault'}
                  </span>
                </div>
                <p className="text-xs text-muted-foreground/70 mt-0.5">
                  {zone.status === 'fault' ? 'Valve Failure' : zone.program}
                </p>
              </div>

              {/* Progress bar for running zones */}
              {zone.status === 'running' && (
                <div className="absolute bottom-0 left-0 right-0 h-1.5 bg-black/20">
                  <div 
                    className="h-full bg-gradient-to-r from-cyan-500 to-cyan-400 rounded-r-full transition-all duration-1000 shadow-[0_0_8px_rgba(6,182,212,0.5)]"
                    style={{ width: `${zone.progress}%` }}
                  />
                </div>
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
}
