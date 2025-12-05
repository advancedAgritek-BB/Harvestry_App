'use client';

import React from 'react';
import { Package, AlertTriangle, RefreshCw, BarChart3 } from 'lucide-react';
import { cn } from '@/lib/utils';
import type { LotSummary, ComplianceSummary } from '../types';

interface InventoryKPIWidgetProps {
  lotSummary: LotSummary | null;
  complianceSummary: ComplianceSummary | null;
  loading?: boolean;
  className?: string;
}

interface KPICardProps {
  label: string;
  value: string | number;
  subValue?: string;
  icon: React.ElementType;
  trend?: 'up' | 'down' | 'neutral';
  trendValue?: string;
  accent: 'amber' | 'rose' | 'cyan' | 'emerald';
  pulse?: boolean;
}

function KPICard({
  label,
  value,
  subValue,
  icon: Icon,
  trend,
  trendValue,
  accent,
  pulse,
}: KPICardProps) {
  const accentColors = {
    amber: 'text-amber-400 bg-amber-500/10',
    rose: 'text-rose-400 bg-rose-500/10',
    cyan: 'text-cyan-400 bg-cyan-500/10',
    emerald: 'text-emerald-400 bg-emerald-500/10',
  };

  const trendColors = {
    up: 'text-emerald-400',
    down: 'text-rose-400',
    neutral: 'text-muted-foreground',
  };

  return (
    <div className="relative bg-muted/30 rounded-xl p-5 border border-border hover:border-border transition-all group">
      {/* Pulse indicator for active states */}
      {pulse && (
        <div className="absolute top-3 right-3">
          <span className="relative flex h-2 w-2">
            <span className={cn(
              'animate-ping absolute inline-flex h-full w-full rounded-full opacity-75',
              accent === 'amber' && 'bg-amber-400',
              accent === 'rose' && 'bg-rose-400',
              accent === 'cyan' && 'bg-cyan-400',
              accent === 'emerald' && 'bg-emerald-400'
            )} />
            <span className={cn(
              'relative inline-flex rounded-full h-2 w-2',
              accent === 'amber' && 'bg-amber-400',
              accent === 'rose' && 'bg-rose-400',
              accent === 'cyan' && 'bg-cyan-400',
              accent === 'emerald' && 'bg-emerald-400'
            )} />
          </span>
        </div>
      )}

      <div className="flex items-start justify-between">
        <div className="space-y-3">
          <span className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
            {label}
          </span>
          <div className="flex items-baseline gap-2">
            <span className="text-3xl font-bold tracking-tight text-foreground tabular-nums">
              {value}
            </span>
            {subValue && (
              <span className="text-sm text-muted-foreground">{subValue}</span>
            )}
          </div>
          {trend && trendValue && (
            <div className={cn('text-xs font-medium', trendColors[trend])}>
              {trend === 'up' && '↑ '}
              {trend === 'down' && '↓ '}
              {trendValue}
            </div>
          )}
        </div>
        <div className={cn(
          'w-10 h-10 rounded-lg flex items-center justify-center',
          accentColors[accent]
        )}>
          <Icon className="w-5 h-5" />
        </div>
      </div>

      {/* Hover glow effect */}
      <div className={cn(
        'absolute inset-0 rounded-xl opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none',
        accent === 'amber' && 'shadow-[0_0_30px_rgba(245,158,11,0.1)]',
        accent === 'rose' && 'shadow-[0_0_30px_rgba(244,63,94,0.1)]',
        accent === 'cyan' && 'shadow-[0_0_30px_rgba(6,182,212,0.1)]',
        accent === 'emerald' && 'shadow-[0_0_30px_rgba(16,185,129,0.1)]'
      )} />
    </div>
  );
}

export function InventoryKPIWidget({
  lotSummary,
  complianceSummary,
  loading,
  className,
}: InventoryKPIWidgetProps) {
  if (loading) {
    return (
      <div className={cn('grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4', className)}>
        {[1, 2, 3, 4].map((i) => (
          <div key={i} className="bg-muted/30 rounded-xl p-5 border border-border animate-pulse">
            <div className="h-4 w-24 bg-white/5 rounded mb-3" />
            <div className="h-8 w-16 bg-white/10 rounded" />
          </div>
        ))}
      </div>
    );
  }

  const totalLots = lotSummary?.totalLots ?? 0;
  const activeHolds = complianceSummary?.activeHolds ?? 0;
  const pendingSyncs = complianceSummary?.integrations?.reduce(
    (sum, i) => sum + i.pendingCount,
    0
  ) ?? 0;
  const variancePercent = lotSummary 
    ? ((lotSummary.byStatus.on_hold || 0) / Math.max(totalLots, 1) * 100).toFixed(1)
    : '0.0';

  return (
    <div className={cn('grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4', className)}>
      <KPICard
        label="Total Lots"
        value={totalLots.toLocaleString()}
        subValue="active"
        icon={Package}
        accent="cyan"
        trend="up"
        trendValue="+12 today"
      />
      <KPICard
        label="Active Holds"
        value={activeHolds}
        icon={AlertTriangle}
        accent="rose"
        pulse={activeHolds > 0}
        trend={activeHolds > 0 ? 'down' : 'neutral'}
        trendValue={activeHolds > 0 ? 'Action required' : 'All clear'}
      />
      <KPICard
        label="Pending Syncs"
        value={pendingSyncs}
        icon={RefreshCw}
        accent="cyan"
        pulse={pendingSyncs > 0}
        trend="neutral"
        trendValue={complianceSummary?.syncHealth === 'healthy' ? 'Healthy' : 'Check status'}
      />
      <KPICard
        label="Balance Variance"
        value={`${variancePercent}%`}
        icon={BarChart3}
        accent="emerald"
        trend="neutral"
        trendValue="Within threshold"
      />
    </div>
  );
}

export default InventoryKPIWidget;
