import React from 'react';
import { cn } from '@/lib/utils';
import { TankGraphic } from './TankGraphic';
import Link from 'next/link';
import { Droplets, Clock, ArrowRight } from 'lucide-react';

export interface TankData {
  id: string;
  name: string;
  fillPercentage: number;
  capacityGallons: number;
  currentGallons: number;
  sensors: {
    ec: number;
    ph: number;
    temp: number;
    do?: number;
  };
  lastFill: string; // "2h ago"
  estDepletion: string; // "~4h"
  recipe: {
    id: string;
    name: string;
  };
  zones: string[];
  status: 'mixing' | 'feeding' | 'filling' | 'evacuating' | 'empty' | 'error' | 'idle';
}

interface TankCardProps {
  tank: TankData;
  className?: string;
}

export function TankCard({ tank, className }: TankCardProps) {
  const statusColors = {
    mixing: 'text-violet-400 border-violet-500/30 bg-violet-500/10',
    feeding: 'text-emerald-400 border-emerald-500/30 bg-emerald-500/10',
    filling: 'text-cyan-400 border-cyan-500/30 bg-cyan-500/10',
    evacuating: 'text-amber-400 border-amber-500/30 bg-amber-500/10',
    empty: 'text-muted-foreground border-border bg-muted/50',
    error: 'text-rose-400 border-rose-500/30 bg-rose-500/10',
    idle: 'text-muted-foreground border-border bg-muted/30',
  };

  return (
    <div className={cn("flex flex-col w-full h-full min-h-[330px] bg-surface/50 border border-border rounded-xl overflow-hidden group hover:border-border/80 transition-all", className)}>
      
      {/* 1. Header & Status Badge */}
      <div className="flex items-center justify-between p-3 pb-0 shrink-0">
        <span className="text-sm font-bold text-foreground truncate">{tank.name}</span>
        <div className={cn("px-2 py-0.5 rounded text-[10px] uppercase font-bold tracking-wider border", statusColors[tank.status])}>
          {tank.status}
        </div>
      </div>

      {/* 2. Hero Graphic Area */}
      <div className="relative flex-1 w-full p-2 flex items-center justify-center min-h-[160px]">
        <TankGraphic fillPercentage={tank.fillPercentage} status={tank.status} className="h-full w-auto max-h-[180px]" />
      </div>

      {/* 3. Key Metrics Grid */}
      <div className="grid grid-cols-3 gap-px bg-muted/50 border-y border-border shrink-0">
        <div className="p-2 text-center">
          <div className="text-[10px] text-muted-foreground uppercase">EC</div>
          <div className="text-sm font-mono font-medium text-cyan-300">{tank.sensors.ec.toFixed(1)}</div>
        </div>
        <div className="p-2 text-center border-x border-border/50">
          <div className="text-[10px] text-muted-foreground uppercase">pH</div>
          <div className="text-sm font-mono font-medium text-purple-300">{tank.sensors.ph.toFixed(1)}</div>
        </div>
        <div className="p-2 text-center">
          <div className="text-[10px] text-muted-foreground uppercase">Temp</div>
          <div className="text-sm font-mono font-medium text-amber-300">{tank.sensors.temp.toFixed(0)}°</div>
        </div>
      </div>

      {/* 4. Details List */}
      <div className="flex flex-col p-3 pb-4 space-y-2 text-xs shrink-0 bg-surface/30 h-[120px]">
        
        {/* Volume & Time */}
        <div className="flex items-center justify-between text-muted-foreground shrink-0">
          <div className="flex items-center gap-1.5">
            <Droplets className="w-3 h-3 text-cyan-500" />
            <span>
              <span className="text-foreground font-medium">{tank.currentGallons}</span>
              <span className="opacity-70"> / {tank.capacityGallons} gal</span>
            </span>
          </div>
          <div className="flex items-center gap-1.5" title="Estimated depletion">
             <Clock className="w-3 h-3 text-muted-foreground" />
             <span>{tank.estDepletion} left</span>
          </div>
        </div>

        {/* Recipe Link */}
        <div className="pt-2 border-t border-border/50 shrink-0">
          <div className="text-[10px] text-muted-foreground mb-0.5">Active Recipe</div>
          <Link 
            href={`/dashboard/recipes/fertigation/${tank.recipe.id}`}
            className="flex items-center justify-between text-cyan-400 hover:text-cyan-300 transition-colors group/link"
          >
            <span className="font-medium truncate pr-2">{tank.recipe.name}</span>
            <ArrowRight className="w-3 h-3 opacity-0 group-hover/link:opacity-100 transition-opacity" />
          </Link>
        </div>

        {/* Zones */}
        <div className="flex flex-wrap gap-1 pt-1 flex-1 content-start">
          {tank.zones.length > 0 ? tank.zones.map(zone => (
            <span key={zone} className="px-1.5 py-0.5 bg-muted rounded text-[10px] text-muted-foreground border border-border">
              {zone}
            </span>
          )) : (
            <span className="text-[10px] text-muted-foreground/50 italic px-1">No zones active</span>
          )}
        </div>

      </div>
    </div>
  );
}
