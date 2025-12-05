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
  clone: { icon: Sprout, color: 'text-cyan-400', bg: 'bg-cyan-500/15' },
  veg: { icon: Leaf, color: 'text-emerald-400', bg: 'bg-emerald-500/15' },
  flower: { icon: Flower, color: 'text-rose-400', bg: 'bg-rose-500/15' },
  drying: { icon: Package, color: 'text-amber-400', bg: 'bg-amber-500/15' },
};

export function ActiveBatchesWidget() {
  return (
    <div className="flex flex-col space-y-3">
      {MOCK_BATCHES.map((batch) => {
        const config = STAGE_CONFIG[batch.stage];
        const Icon = config.icon;
        
        return (
          <div
            key={batch.id}
            className="flex items-center gap-3 p-3 rounded-xl bg-surface/30 hover:bg-surface/50 border border-border hover:border-border/80 cursor-pointer group transition-all duration-200"
          >
            <div className={cn('p-2 rounded-lg', config.bg)}>
              <Icon className={cn('w-4 h-4', config.color)} />
            </div>
            
            <div className="flex-1 min-w-0">
              <div className="flex items-center justify-between">
                <h4 className="text-sm font-semibold text-foreground truncate group-hover:text-cyan-300 transition-colors">
                  {batch.name}
                </h4>
                <span className={cn(
                  'text-sm font-mono px-2 py-0.5 rounded-md',
                  batch.health < 90 ? 'text-amber-400 bg-amber-500/15' : 'text-emerald-400 bg-emerald-500/15'
                )}>
                  {batch.health}%
                </span>
              </div>
              <div className="flex items-center justify-between text-sm text-muted-foreground mt-0.5">
                <span>{batch.strain}</span>
                <span>Day {batch.daysInStage}</span>
              </div>
            </div>
          </div>
        );
      })}

      {MOCK_BATCHES.length === 0 && (
        <div className="flex flex-col items-center justify-center py-6 text-center">
          <p className="text-sm text-muted-foreground mb-3">No active batches</p>
          <button className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-foreground bg-gradient-to-r from-cyan-500 to-cyan-600 hover:from-cyan-400 hover:to-cyan-500 rounded-lg transition-all shadow-lg shadow-cyan-500/20">
            <Plus className="w-4 h-4" />
            Start Batch
          </button>
        </div>
      )}
    </div>
  );
}
