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

export function IrrigationStatusWidget() {
  return (
    <div className="flex flex-col">
      <div className="space-y-3">
        {MOCK_ZONES.map((zone) => (
          <div
            key={zone.id}
            className={cn(
              'flex items-center gap-3 p-3 rounded-xl border transition-all relative overflow-hidden',
              zone.status === 'running' 
                ? 'bg-gradient-to-r from-cyan-500/15 to-transparent border-cyan-500/30' 
                : zone.status === 'fault'
                  ? 'bg-gradient-to-r from-rose-500/15 to-transparent border-rose-500/30'
                  : 'bg-surface/30 border-border'
            )}
          >
            {/* Status Icon */}
            <div className={cn(
              'p-2 rounded-lg shrink-0',
              zone.status === 'running' && 'bg-cyan-500/20',
              zone.status === 'fault' && 'bg-rose-500/20',
              zone.status === 'idle' && 'bg-muted/50'
            )}>
              {zone.status === 'running' && <Play className="w-4 h-4 text-cyan-400" />}
              {zone.status === 'idle' && <CalendarClock className="w-4 h-4 text-muted-foreground" />}
              {zone.status === 'fault' && <AlertTriangle className="w-4 h-4 text-rose-400 animate-pulse" />}
            </div>

            {/* Zone Info */}
            <div className="flex-1 min-w-0">
              <div className="flex items-center justify-between">
                <span className="text-sm font-semibold text-foreground">{zone.name}</span>
                <span className={cn(
                  'text-sm font-mono',
                  zone.status === 'running' && 'text-cyan-400',
                  zone.status === 'fault' && 'text-rose-400',
                  zone.status === 'idle' && 'text-muted-foreground'
                )}>
                  {zone.status === 'running' && `${zone.progress}%`}
                  {zone.status === 'idle' && zone.nextRun && `Next: ${zone.nextRun}`}
                  {zone.status === 'fault' && 'Fault'}
                </span>
              </div>
              <p className="text-sm text-muted-foreground">
                {zone.status === 'fault' ? 'Valve Failure' : zone.program}
              </p>
            </div>

            {/* Progress bar for running zones */}
            {zone.status === 'running' && (
              <div className="absolute bottom-0 left-0 right-0 h-1 bg-background/30">
                <div 
                  className="h-full bg-gradient-to-r from-cyan-500 to-cyan-400 rounded-full transition-all duration-1000"
                  style={{ width: `${zone.progress}%` }}
                />
              </div>
            )}
          </div>
        ))}
      </div>
      
    </div>
  );
}
