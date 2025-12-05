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
      {/* Rooms Status */}
      <div className="flex items-center gap-4 p-4 rounded-xl bg-surface/40 border border-border">
        <div className="flex items-center gap-1.5">
          {rooms.map((room) => (
            <div
              key={room.id}
              title={`${room.name}: ${room.status}`}
              className={cn(
                'w-2.5 h-8 rounded-full transition-all hover:scale-110 cursor-pointer',
                STATUS_COLORS[room.status]
              )}
            />
          ))}
        </div>
        <div>
          <p className="text-2xl font-bold text-foreground">
            {healthyCount}/{rooms.length}
          </p>
          <p className="text-sm text-muted-foreground">Rooms OK</p>
        </div>
        {allHealthy && (
          <CheckCircle2 className="w-5 h-5 text-emerald-400 ml-auto" />
        )}
      </div>

      {/* Alerts */}
      <Link
        href="/dashboard/cultivation"
        className={cn(
          'flex items-center gap-4 p-4 rounded-xl border transition-all hover:scale-[1.02]',
          alertCount > 0 
            ? 'bg-amber-500/10 border-amber-500/30 hover:border-amber-500/50' 
            : 'bg-surface/40 border-border hover:border-border/80'
        )}
      >
        <div className={cn(
          'p-3 rounded-xl',
          alertCount > 0 ? 'bg-amber-500/20' : 'bg-muted/50'
        )}>
          <AlertTriangle className={cn(
            'w-6 h-6',
            alertCount > 0 ? 'text-amber-400' : 'text-muted-foreground'
          )} />
        </div>
        <div>
          <p className={cn(
            'text-2xl font-bold',
            alertCount > 0 ? 'text-amber-400' : 'text-foreground'
          )}>
            {alertCount}
          </p>
          <p className="text-sm text-muted-foreground">Active Alerts</p>
        </div>
      </Link>

      {/* Tasks */}
      <Link
        href="/dashboard/tasks"
        className="flex items-center gap-4 p-4 rounded-xl bg-surface/40 border border-border hover:border-border/80 transition-all hover:scale-[1.02]"
      >
        <div className="p-3 rounded-xl bg-violet-500/20">
          <ClipboardList className="w-6 h-6 text-violet-400" />
        </div>
        <div>
          <p className="text-2xl font-bold text-foreground">
            {taskCount}
          </p>
          <p className="text-sm text-muted-foreground">Pending Tasks</p>
        </div>
      </Link>

      {/* System Health */}
      <div className="flex items-center gap-4 p-4 rounded-xl bg-surface/40 border border-border">
        <div className={cn(
          'p-3 rounded-xl',
          systemHealth >= 90 ? 'bg-emerald-500/20' :
          systemHealth >= 70 ? 'bg-amber-500/20' : 'bg-rose-500/20'
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
              'text-2xl font-bold',
              systemHealth >= 90 ? 'text-emerald-400' :
              systemHealth >= 70 ? 'text-amber-400' : 'text-rose-400'
            )}>
              {systemHealth}%
            </p>
            <span className="text-sm text-muted-foreground">Health</span>
          </div>
          <div className="w-full h-2 rounded-full bg-background/50 overflow-hidden mt-1">
            <div
              className={cn(
                'h-full rounded-full transition-all duration-500',
                systemHealth >= 90 ? 'bg-emerald-400' :
                systemHealth >= 70 ? 'bg-amber-400' : 'bg-rose-400'
              )}
              style={{ width: `${systemHealth}%` }}
            />
          </div>
        </div>
      </div>
    </div>
  );
}
