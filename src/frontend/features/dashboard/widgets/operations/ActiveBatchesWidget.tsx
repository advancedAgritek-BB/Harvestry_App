'use client';

import React from 'react';
import { Sprout, Leaf, Flower, Package, Plus } from 'lucide-react';
import { cn } from '@/lib/utils';

interface Batch {
  id: string;
  name: string;
  strain: string;
  stage: 'clone' | 'veg' | 'flower' | 'drying';
  plantCount: number;
  daysInStage: number;
  health: number;
}

const MOCK_BATCHES: Batch[] = [
  {
    id: '1',
    name: 'B-203-OGK',
    strain: 'OG Kush',
    stage: 'flower',
    plantCount: 450,
    daysInStage: 23,
    health: 98,
  },
  {
    id: '2',
    name: 'B-204-BDR',
    strain: 'Blue Dream',
    stage: 'veg',
    plantCount: 320,
    daysInStage: 14,
    health: 92,
  },
  {
    id: '3',
    name: 'B-205-GSC',
    strain: 'GSC',
    stage: 'clone',
    plantCount: 500,
    daysInStage: 5,
    health: 95,
  },
];

const STAGE_CONFIG = {
  clone: { 
    icon: Sprout, 
    color: 'text-cyan-400', 
    iconBg: 'bg-gradient-to-br from-cyan-500/30 to-cyan-600/20 ring-1 ring-cyan-400/30',
    cardBg: 'bg-gradient-to-r from-cyan-500/10 via-cyan-500/5 to-transparent',
    shadow: 'shadow-md shadow-cyan-500/10',
    accentBar: 'bg-cyan-400',
  },
  veg: { 
    icon: Leaf, 
    color: 'text-emerald-400', 
    iconBg: 'bg-gradient-to-br from-emerald-500/30 to-emerald-600/20 ring-1 ring-emerald-400/30',
    cardBg: 'bg-gradient-to-r from-emerald-500/10 via-emerald-500/5 to-transparent',
    shadow: 'shadow-md shadow-emerald-500/10',
    accentBar: 'bg-emerald-400',
  },
  flower: { 
    icon: Flower, 
    color: 'text-rose-400', 
    iconBg: 'bg-gradient-to-br from-rose-500/30 to-rose-600/20 ring-1 ring-rose-400/30',
    cardBg: 'bg-gradient-to-r from-rose-500/10 via-rose-500/5 to-transparent',
    shadow: 'shadow-md shadow-rose-500/10',
    accentBar: 'bg-rose-400',
  },
  drying: { 
    icon: Package, 
    color: 'text-amber-400', 
    iconBg: 'bg-gradient-to-br from-amber-500/30 to-amber-600/20 ring-1 ring-amber-400/30',
    cardBg: 'bg-gradient-to-r from-amber-500/10 via-amber-500/5 to-transparent',
    shadow: 'shadow-md shadow-amber-500/10',
    accentBar: 'bg-amber-400',
  },
};

export function ActiveBatchesWidget() {
  return (
    <div className="flex flex-col space-y-2.5">
      {MOCK_BATCHES.map((batch) => {
        const config = STAGE_CONFIG[batch.stage];
        const Icon = config.icon;
        const isHealthWarning = batch.health < 90;
        
        return (
          <div
            key={batch.id}
            className={cn(
              "group relative flex items-center gap-3 p-3 pl-4 rounded-xl",
              "cursor-pointer transition-all duration-300",
              config.cardBg,
              config.shadow,
              "hover:shadow-lg hover:-translate-y-0.5"
            )}
          >
            {/* Left accent bar */}
            <div className={cn(
              'absolute left-0 top-2 bottom-2 w-1 rounded-full',
              config.accentBar
            )} />

            <div className={cn(
              'p-2 rounded-lg transition-transform group-hover:scale-105',
              config.iconBg
            )}>
              <Icon className={cn('w-4 h-4', config.color)} />
            </div>
            
            <div className="flex-1 min-w-0">
              <div className="flex items-center justify-between gap-2">
                <h4 className="text-sm font-semibold text-foreground truncate group-hover:text-white transition-colors">
                  {batch.name}
                </h4>
                <span className={cn(
                  'text-xs font-mono font-semibold tabular-nums px-2 py-1 rounded-full',
                  isHealthWarning 
                    ? 'bg-gradient-to-r from-amber-500/20 to-amber-600/10 text-amber-400 ring-1 ring-amber-400/30' 
                    : 'bg-gradient-to-r from-emerald-500/20 to-emerald-600/10 text-emerald-400 ring-1 ring-emerald-400/30'
                )}>
                  {batch.health}%
                </span>
              </div>
              <div className="flex items-center justify-between text-xs text-muted-foreground/70 mt-1">
                <span>{batch.strain}</span>
                <span className={cn("font-medium", config.color, "opacity-70")}>Day {batch.daysInStage}</span>
              </div>
            </div>
          </div>
        );
      })}

      {MOCK_BATCHES.length === 0 && (
        <div className="flex flex-col items-center justify-center py-8 text-center">
          <p className="text-sm text-muted-foreground/70 mb-4">No active batches</p>
          <button className="flex items-center gap-2 px-5 py-2.5 text-sm font-semibold text-white bg-gradient-to-r from-cyan-500 to-cyan-600 hover:from-cyan-400 hover:to-cyan-500 rounded-xl transition-all shadow-lg shadow-cyan-500/30 hover:shadow-cyan-500/40 hover:-translate-y-0.5">
            <Plus className="w-4 h-4" />
            Start Batch
          </button>
        </div>
      )}
    </div>
  );
}
