'use client';

import React from 'react';
import { useRouter } from 'next/navigation';
import { cn } from '@/lib/utils';
import { Sprout, Leaf, Flower, Package, ChevronRight, Thermometer, Droplets, Wind, Zap } from 'lucide-react';

interface RoomData {
  id: string;
  name: string;
  stage: 'clone' | 'veg' | 'flower' | 'drying';
  temp: number;
  tempTarget: number;
  rh: number;
  rhTarget: number;
  ec?: number;
  vpd: number;
  status: 'healthy' | 'warning' | 'critical';
  lightCycle?: string;
  plantCount?: number;
}

interface RoomCardProps {
  room: RoomData;
  compact?: boolean;
}

const STAGE_CONFIG = {
  clone: { 
    icon: Sprout, 
    color: 'text-cyan-400', 
    bg: 'bg-cyan-500/15',
    label: 'Clone'
  },
  veg: { 
    icon: Leaf, 
    color: 'text-emerald-400', 
    bg: 'bg-emerald-500/15',
    label: 'Vegetative'
  },
  flower: { 
    icon: Flower, 
    color: 'text-rose-400', 
    bg: 'bg-rose-500/15',
    label: 'Flowering'
  },
  drying: { 
    icon: Package, 
    color: 'text-amber-400', 
    bg: 'bg-amber-500/15',
    label: 'Drying'
  },
};

const STATUS_CONFIG = {
  healthy: {
    dot: 'bg-emerald-400',
    glow: 'shadow-[0_0_10px_rgba(52,211,153,0.6)]',
    border: 'border-border hover:border-emerald-500/40'
  },
  warning: {
    dot: 'bg-amber-400',
    glow: 'shadow-[0_0_10px_rgba(251,191,36,0.6)]',
    border: 'border-amber-500/30 hover:border-amber-500/50'
  },
  critical: {
    dot: 'bg-rose-400 animate-pulse',
    glow: 'shadow-[0_0_10px_rgba(251,113,133,0.7)]',
    border: 'border-rose-500/30 hover:border-rose-500/50'
  },
};

export function RoomCard({ room, compact = true }: RoomCardProps) {
  const router = useRouter();
  const stageConfig = STAGE_CONFIG[room.stage];
  const statusConfig = STATUS_CONFIG[room.status];
  const StageIcon = stageConfig.icon;

  const handleClick = () => {
    router.push(`/dashboard/cultivation?room=${room.id}`);
  };

  const tempDiff = Math.abs(room.temp - room.tempTarget);
  const rhDiff = Math.abs(room.rh - room.rhTarget);

  return (
    <button
      onClick={handleClick}
      className={cn(
        "group relative flex flex-col p-4 rounded-2xl overflow-hidden",
        "bg-gradient-to-br from-surface/60 to-surface/30 backdrop-blur-sm",
        "border transition-all duration-200 text-left",
        "hover:shadow-xl hover:shadow-background/30 hover:-translate-y-1",
        "active:scale-[0.98] cursor-pointer",
        statusConfig.border
      )}
    >
      {/* Status Indicator */}
      <div className="absolute top-4 right-4">
        <div className={cn("w-3 h-3 rounded-full", statusConfig.dot, statusConfig.glow)} />
      </div>

      {/* Header */}
      <div className="flex items-center gap-3 mb-4">
        <div className={cn("p-2.5 rounded-xl", stageConfig.bg)}>
          <StageIcon className={cn("w-5 h-5", stageConfig.color)} />
        </div>
        <div className="flex-1 min-w-0 pr-6">
          <h3 className="text-base font-semibold text-foreground group-hover:text-cyan-300 transition-colors truncate">
            {room.name}
          </h3>
          <p className="text-sm text-muted-foreground">{stageConfig.label}</p>
        </div>
      </div>

      {/* Metrics Grid - 2x2 with larger text */}
      <div className="grid grid-cols-2 gap-2 mb-3">
        <MetricCell 
          icon={Thermometer}
          label="Temp"
          value={`${room.temp}°`}
          isOffTarget={tempDiff > 1}
        />
        <MetricCell 
          icon={Droplets}
          label="RH"
          value={`${room.rh}%`}
          isOffTarget={rhDiff > 3}
        />
        <MetricCell 
          icon={Wind}
          label="VPD"
          value={room.vpd.toFixed(2)}
        />
        {room.ec !== undefined ? (
          <MetricCell 
            icon={Zap}
            label="EC"
            value={room.ec.toFixed(1)}
          />
        ) : (
          <div /> // Empty cell for alignment
        )}
      </div>

      {/* Footer */}
      <div className="flex items-center justify-between pt-3 border-t border-border/30">
        <div className="flex items-center gap-3">
          {room.plantCount && (
            <span className="text-sm font-medium text-foreground/80">
              {room.plantCount.toLocaleString()} plants
            </span>
          )}
          {room.lightCycle && (
            <span className="px-2 py-1 rounded-md bg-yellow-500/15 text-yellow-300 text-xs font-semibold">
              {room.lightCycle}
            </span>
          )}
        </div>
        <ChevronRight className="w-5 h-5 text-muted-foreground group-hover:text-cyan-400 group-hover:translate-x-0.5 transition-all" />
      </div>
    </button>
  );
}

// Metric cell with larger, more readable text
interface MetricCellProps {
  icon: React.ElementType;
  label: string;
  value: string;
  isOffTarget?: boolean;
}

function MetricCell({ icon: Icon, label, value, isOffTarget }: MetricCellProps) {
  return (
    <div className="flex items-center gap-2 px-3 py-2 rounded-xl bg-background/40">
      <Icon className={cn(
        "w-4 h-4 flex-shrink-0", 
        isOffTarget ? "text-amber-400" : "text-muted-foreground"
      )} />
      <div className="min-w-0">
        <p className="text-xs text-muted-foreground uppercase tracking-wide">{label}</p>
        <p className={cn(
          "text-lg font-bold font-mono leading-tight",
          isOffTarget ? "text-amber-300" : "text-foreground"
        )}>
          {value}
        </p>
      </div>
    </div>
  );
}
