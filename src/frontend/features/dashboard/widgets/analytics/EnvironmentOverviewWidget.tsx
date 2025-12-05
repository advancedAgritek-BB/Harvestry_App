'use client';

import React from 'react';
import { Thermometer, Droplets, Wind, Sun } from 'lucide-react';
import { cn } from '@/lib/utils';

interface EnvMetric {
  id: string;
  label: string;
  value: string;
  unit: string;
  icon: any;
  color: string;
  bgColor: string;
  status: 'ok' | 'warn' | 'crit';
  history: number[]; // Simple array for sparkline
}

const MOCK_ENV: EnvMetric[] = [
  {
    id: 'temp',
    label: 'Avg. Temp',
    value: '78.4',
    unit: '°F',
    icon: Thermometer,
    color: 'text-rose-400',
    bgColor: 'bg-rose-500/10',
    status: 'ok',
    history: [76, 76.5, 77, 78, 78.4, 78.2, 77.8, 78.4],
  },
  {
    id: 'rh',
    label: 'Humidity',
    value: '62.1',
    unit: '%',
    icon: Droplets,
    color: 'text-cyan-400',
    bgColor: 'bg-cyan-500/10',
    status: 'warn', // Slightly high
    history: [58, 59, 60, 61, 62, 63, 62.5, 62.1],
  },
  {
    id: 'vpd',
    label: 'VPD',
    value: '1.2',
    unit: 'kPa',
    icon: Wind,
    color: 'text-emerald-400',
    bgColor: 'bg-emerald-500/10',
    status: 'ok',
    history: [1.0, 1.1, 1.1, 1.2, 1.2, 1.2, 1.1, 1.2],
  },
  {
    id: 'ppfd',
    label: 'PPFD',
    value: '850',
    unit: 'µmol',
    icon: Sun,
    color: 'text-amber-400',
    bgColor: 'bg-amber-500/10',
    status: 'ok',
    history: [0, 100, 400, 800, 850, 850, 850, 850],
  },
];

export function EnvironmentOverviewWidget() {
  return (
    <div className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-2 xl:grid-cols-4 gap-3 h-full">
      {MOCK_ENV.map((metric) => {
        const Icon = metric.icon;
        return (
          <div
            key={metric.id}
            className="flex flex-col p-3 rounded-lg tile-premium group relative overflow-hidden"
          >
            <div className="flex items-center justify-between mb-2">
              <div className={cn('p-1.5 rounded-md', metric.bgColor)}>
                <Icon className={cn('w-4 h-4', metric.color)} />
              </div>
              {metric.status === 'warn' && <span className="w-1.5 h-1.5 rounded-full bg-amber-500 animate-pulse shadow-[0_0_8px_rgba(245,158,11,0.5)]" />}
              {metric.status === 'crit' && <span className="w-1.5 h-1.5 rounded-full bg-rose-500 animate-pulse shadow-[0_0_8px_rgba(244,63,94,0.5)]" />}
            </div>

            <div className="flex-1 z-10">
              <div className="text-[10px] uppercase font-bold text-muted-foreground tracking-widest mb-0.5">
                {metric.label}
              </div>
              <div className="flex items-baseline gap-1">
                <span className="text-xl font-bold text-foreground tabular-nums tracking-tighter">
                  {metric.value}
                </span>
                <span className="text-[10px] text-muted-foreground font-medium">
                  {metric.unit}
                </span>
              </div>
            </div>

            {/* Micro Sparkline (CSS/SVG) - Subtle background effect */}
            <div className="absolute bottom-0 left-0 right-0 h-8 flex items-end gap-0.5 opacity-20 group-hover:opacity-40 transition-opacity px-1 pb-1">
               {metric.history.map((val, i) => (
                  <div 
                    key={i} 
                    className={cn("flex-1 rounded-t-[1px]", metric.color.replace('text-', 'bg-'))}
                    style={{ height: `${(val / Math.max(...metric.history)) * 100}%` }}
                  />
               ))}
            </div>
          </div>
        );
      })}
    </div>
  );
}
