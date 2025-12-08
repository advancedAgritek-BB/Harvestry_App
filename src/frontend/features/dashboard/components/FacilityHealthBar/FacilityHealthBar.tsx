'use client';

import React from 'react';
import Link from 'next/link';
import { AlertTriangle, ClipboardList, Activity, CheckCircle2 } from 'lucide-react';
import { cn } from '@/lib/utils';

interface RoomStatus {
  id: string;
  name: string;
  status: 'healthy' | 'warning' | 'critical';
}

interface FacilityHealthBarProps {
  rooms: RoomStatus[];
  alertCount: number;
  taskCount: number;
  systemHealth?: number;
  className?: string;
}

const STATUS_COLORS = {
  healthy: 'bg-emerald-400',
  warning: 'bg-amber-400',
  critical: 'bg-rose-400 animate-pulse',
};

const STATUS_SHADOWS = {
  healthy: 'shadow-[0_0_8px_rgba(52,211,153,0.4)]',
  warning: 'shadow-[0_0_8px_rgba(251,191,36,0.4)]',
  critical: 'shadow-[0_0_8px_rgba(251,113,133,0.5)]',
};

export function FacilityHealthBar({
  rooms,
  alertCount,
  taskCount,
  systemHealth = 100,
  className,
}: FacilityHealthBarProps) {
  const healthyCount = rooms.filter(r => r.status === 'healthy').length;
  const warningCount = rooms.filter(r => r.status === 'warning').length;
  const criticalCount = rooms.filter(r => r.status === 'critical').length;

  const allHealthy = criticalCount === 0 && warningCount === 0;

  return (
    <div
      className={cn(
        'grid grid-cols-4 gap-4',
        className
      )}
    >
      {/* Rooms Status - Glassmorphic Tile */}
      <div className={cn(
        "flex items-center gap-4 p-4 rounded-2xl",
        "bg-gradient-to-br from-emerald-500/10 via-surface/40 to-surface/20",
        "backdrop-blur-sm shadow-lg shadow-emerald-500/5",
        "transition-all duration-300 hover:shadow-xl hover:shadow-emerald-500/10"
      )}>
        <div className="flex items-center gap-1.5">
          {rooms.map((room) => (
            <div
              key={room.id}
              title={`${room.name}: ${room.status}`}
              className={cn(
                'w-2 h-7 rounded-full transition-all hover:scale-110 cursor-pointer',
                STATUS_COLORS[room.status],
                STATUS_SHADOWS[room.status]
              )}
            />
          ))}
        </div>
        <div>
          <p className="text-2xl font-bold text-foreground tabular-nums">
            {healthyCount}<span className="text-muted-foreground/50">/{rooms.length}</span>
          </p>
          <p className="text-sm text-emerald-400/70 font-medium">Rooms OK</p>
        </div>
        {allHealthy && (
          <div className="ml-auto p-1.5 rounded-full bg-emerald-500/20 ring-1 ring-emerald-400/30">
            <CheckCircle2 className="w-4 h-4 text-emerald-400" />
          </div>
        )}
      </div>

      {/* Alerts - Glassmorphic Tile */}
      <Link
        href="/dashboard/cultivation"
        className={cn(
          'group flex items-center gap-4 p-4 rounded-2xl transition-all duration-300',
          'backdrop-blur-sm shadow-lg',
          alertCount > 0 
            ? 'bg-gradient-to-br from-amber-500/15 via-amber-500/5 to-transparent shadow-amber-500/10 hover:shadow-xl hover:shadow-amber-500/20' 
            : 'bg-gradient-to-br from-surface/50 via-surface/30 to-transparent shadow-black/10 hover:shadow-lg'
        )}
      >
        <div className={cn(
          'p-3.5 rounded-xl shadow-lg transition-transform group-hover:scale-105',
          alertCount > 0 
            ? 'bg-gradient-to-br from-amber-500/30 to-amber-600/20 shadow-amber-500/20 ring-1 ring-amber-400/20' 
            : 'bg-white/5 ring-1 ring-white/10'
        )}>
          <AlertTriangle className={cn(
            'w-6 h-6',
            alertCount > 0 ? 'text-amber-400' : 'text-muted-foreground'
          )} />
        </div>
        <div>
          <p className={cn(
            'text-2xl font-bold tabular-nums',
            alertCount > 0 ? 'text-amber-400' : 'text-foreground'
          )}>
            {alertCount}
          </p>
          <p className={cn(
            "text-sm font-medium",
            alertCount > 0 ? "text-amber-400/70" : "text-muted-foreground"
          )}>Active Alerts</p>
        </div>
      </Link>

      {/* Tasks - Glassmorphic Tile */}
      <Link
        href="/dashboard/tasks"
        className={cn(
          "group flex items-center gap-4 p-4 rounded-2xl",
          "bg-gradient-to-br from-violet-500/10 via-surface/40 to-surface/20",
          "backdrop-blur-sm shadow-lg shadow-violet-500/5",
          "transition-all duration-300 hover:shadow-xl hover:shadow-violet-500/10"
        )}
      >
        <div className={cn(
          "p-3.5 rounded-xl shadow-lg transition-transform group-hover:scale-105",
          "bg-gradient-to-br from-violet-500/30 to-violet-600/20",
          "shadow-violet-500/20 ring-1 ring-violet-400/20"
        )}>
          <ClipboardList className="w-6 h-6 text-violet-400" />
        </div>
        <div>
          <p className="text-2xl font-bold text-foreground tabular-nums">
            {taskCount}
          </p>
          <p className="text-sm text-violet-400/70 font-medium">Pending Tasks</p>
        </div>
      </Link>

      {/* System Health - Glassmorphic Tile with Progress */}
      <div className={cn(
        "flex items-center gap-4 p-4 rounded-2xl",
        "backdrop-blur-sm shadow-lg transition-all duration-300",
        systemHealth >= 90 
          ? "bg-gradient-to-br from-emerald-500/10 via-surface/40 to-surface/20 shadow-emerald-500/5 hover:shadow-emerald-500/10" 
          : systemHealth >= 70 
            ? "bg-gradient-to-br from-amber-500/10 via-surface/40 to-surface/20 shadow-amber-500/5 hover:shadow-amber-500/10"
            : "bg-gradient-to-br from-rose-500/10 via-surface/40 to-surface/20 shadow-rose-500/5 hover:shadow-rose-500/10"
      )}>
        <div className={cn(
          'p-3.5 rounded-xl shadow-lg ring-1',
          systemHealth >= 90 
            ? 'bg-gradient-to-br from-emerald-500/30 to-emerald-600/20 shadow-emerald-500/20 ring-emerald-400/20' 
            : systemHealth >= 70 
              ? 'bg-gradient-to-br from-amber-500/30 to-amber-600/20 shadow-amber-500/20 ring-amber-400/20'
              : 'bg-gradient-to-br from-rose-500/30 to-rose-600/20 shadow-rose-500/20 ring-rose-400/20'
        )}>
          <Activity className={cn(
            'w-6 h-6',
            systemHealth >= 90 ? 'text-emerald-400' :
            systemHealth >= 70 ? 'text-amber-400' : 'text-rose-400'
          )} />
        </div>
        <div className="flex-1">
          <div className="flex items-baseline gap-2">
            <p className={cn(
              'text-2xl font-bold tabular-nums',
              systemHealth >= 90 ? 'text-emerald-400' :
              systemHealth >= 70 ? 'text-amber-400' : 'text-rose-400'
            )}>
              {systemHealth}%
            </p>
            <span className="text-sm text-muted-foreground">Health</span>
          </div>
          <div className="w-full h-2.5 rounded-full bg-black/30 overflow-hidden mt-2 ring-1 ring-white/5">
            <div
              className={cn(
                'h-full rounded-full transition-all duration-500',
                systemHealth >= 90 
                  ? 'bg-gradient-to-r from-emerald-500 to-emerald-400 shadow-[0_0_10px_rgba(52,211,153,0.5)]' 
                  : systemHealth >= 70 
                    ? 'bg-gradient-to-r from-amber-500 to-amber-400 shadow-[0_0_10px_rgba(251,191,36,0.5)]'
                    : 'bg-gradient-to-r from-rose-500 to-rose-400 shadow-[0_0_10px_rgba(251,113,133,0.5)]'
              )}
              style={{ width: `${systemHealth}%` }}
            />
          </div>
        </div>
      </div>
    </div>
  );
}
