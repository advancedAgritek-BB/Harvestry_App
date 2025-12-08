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
    bg: 'bg-cyan-500/20',
    cardGradient: 'from-cyan-500/15 via-cyan-500/5 to-transparent',
    shadowColor: 'shadow-cyan-500/10',
    hoverShadow: 'hover:shadow-cyan-500/20',
    label: 'Clone'
  },
  veg: { 
    icon: Leaf, 
    color: 'text-emerald-400', 
    bg: 'bg-emerald-500/20',
    cardGradient: 'from-emerald-500/15 via-emerald-500/5 to-transparent',
    shadowColor: 'shadow-emerald-500/10',
    hoverShadow: 'hover:shadow-emerald-500/20',
    label: 'Vegetative'
  },
  flower: { 
    icon: Flower, 
    color: 'text-rose-400', 
    bg: 'bg-rose-500/20',
    cardGradient: 'from-rose-500/15 via-rose-500/5 to-transparent',
    shadowColor: 'shadow-rose-500/10',
    hoverShadow: 'hover:shadow-rose-500/20',
    label: 'Flowering'
  },
  drying: { 
    icon: Package, 
    color: 'text-amber-400', 
    bg: 'bg-amber-500/20',
    cardGradient: 'from-amber-500/15 via-amber-500/5 to-transparent',
    shadowColor: 'shadow-amber-500/10',
    hoverShadow: 'hover:shadow-amber-500/20',
    label: 'Drying'
  },
};

const STATUS_CONFIG = {
  healthy: {
    dot: 'bg-emerald-400',
    ring: 'ring-2 ring-emerald-400/30',
    glow: 'shadow-[0_0_12px_rgba(52,211,153,0.5)]',
    cardAccent: ''
  },
  warning: {
    dot: 'bg-amber-400',
    ring: 'ring-2 ring-amber-400/40',
    glow: 'shadow-[0_0_12px_rgba(251,191,36,0.5)]',
    cardAccent: 'ring-1 ring-amber-500/20'
  },
  critical: {
    dot: 'bg-rose-400 animate-pulse',
    ring: 'ring-2 ring-rose-400/50',
    glow: 'shadow-[0_0_14px_rgba(251,113,133,0.6)]',
    cardAccent: 'ring-1 ring-rose-500/30'
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
        "bg-gradient-to-br",
        stageConfig.cardGradient,
        "bg-surface/50 backdrop-blur-sm",
        "shadow-lg transition-all duration-300 text-left",
        stageConfig.shadowColor,
        stageConfig.hoverShadow,
        "hover:shadow-xl hover:-translate-y-1",
        "active:scale-[0.98] cursor-pointer",
        statusConfig.cardAccent
      )}
    >
      {/* Status Indicator - Ambient Glow Ring */}
      <div className="absolute top-4 right-4">
        <div className={cn(
          "w-3.5 h-3.5 rounded-full",
          statusConfig.dot,
          statusConfig.ring,
          statusConfig.glow
        )} />
      </div>

      {/* Header */}
      <div className="flex items-center gap-3 mb-4">
        <div className={cn(
          "p-2.5 rounded-xl shadow-lg",
          stageConfig.bg,
          "ring-1 ring-white/10"
        )}>
          <StageIcon className={cn("w-5 h-5", stageConfig.color)} />
        </div>
        <div className="flex-1 min-w-0 pr-6">
          <h3 className="text-base font-semibold text-foreground group-hover:text-white transition-colors truncate">
            {room.name}
          </h3>
          <p className={cn("text-sm", stageConfig.color, "opacity-70")}>{stageConfig.label}</p>
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

      {/* Footer - Gradient divider */}
      <div className="relative pt-3 mt-1">
        <div className="absolute top-0 left-0 right-0 h-px bg-gradient-to-r from-transparent via-white/10 to-transparent" />
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            {room.plantCount && (
              <span className="text-sm font-medium text-foreground/80">
                {room.plantCount.toLocaleString()} plants
              </span>
            )}
            {room.lightCycle && (
              <span className={cn(
                "px-2 py-1 rounded-lg text-xs font-semibold",
                "bg-gradient-to-r from-yellow-500/20 to-amber-500/10",
                "text-yellow-300"
              )}>
                {room.lightCycle}
              </span>
            )}
          </div>
          <ChevronRight className={cn(
            "w-5 h-5 transition-all duration-300",
            "text-muted-foreground/50 group-hover:text-foreground",
            "group-hover:translate-x-0.5"
          )} />
        </div>
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
    <div className={cn(
      "flex items-center gap-2.5 px-3 py-2.5 rounded-xl",
      "bg-white/[0.03] backdrop-blur-sm",
      "transition-colors duration-200",
      isOffTarget && "bg-amber-500/10"
    )}>
      <div className={cn(
        "p-1.5 rounded-lg",
        isOffTarget ? "bg-amber-500/20" : "bg-white/5"
      )}>
        <Icon className={cn(
          "w-4 h-4 flex-shrink-0", 
          isOffTarget ? "text-amber-400" : "text-muted-foreground"
        )} />
      </div>
      <div className="min-w-0">
        <p className="text-[10px] text-muted-foreground/70 uppercase tracking-wider font-medium">{label}</p>
        <p className={cn(
          "text-lg font-bold font-mono leading-tight tracking-tight",
          isOffTarget ? "text-amber-300" : "text-foreground"
        )}>
          {value}
        </p>
      </div>
    </div>
  );
}
