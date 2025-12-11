'use client';

import React from 'react';
import Link from 'next/link';
import { Calendar, Clock, Settings, TrendingUp, Link2 } from 'lucide-react';
import { cn } from '@/lib/utils';

// Components
import { CriticalAlertBanner } from '@/features/planner/components/CriticalAlertBanner';
import { MetricsRow } from '@/features/planner/components/MetricsRow';
import { ProductionTimeline } from '@/features/planner/components/ProductionTimeline';
import { TodaySchedule } from '@/features/planner/components/TodaySchedule';
import { ActionItems } from '@/features/planner/components/ActionItems';
import {
  PlannerCard,
  SectionHeader,
  StatusBadge,
} from '@/features/planner/components/ui';

// Data
import {
  criticalAlerts,
  laborMetrics,
  productionTasks,
  todaySchedule,
  actionItems,
  productivity,
  integrationStatuses,
} from '@/features/planner/constants/laborMockData';

// Types
import { ProductivitySnapshot, IntegrationStatus } from '@/features/planner/types';

// ============================================
// Quick Stats Components (Bottom Section)
// ============================================

function ProductivityCard({ metric }: { metric: ProductivitySnapshot }) {
  const TrendIcon =
    metric.trend === 'up'
      ? TrendingUp
      : metric.trend === 'down'
        ? TrendingUp
        : TrendingUp;
  const trendRotation =
    metric.trend === 'down' ? 'rotate-180' : metric.trend === 'flat' ? 'rotate-90' : '';
  const trendColor =
    metric.trend === 'up'
      ? 'text-emerald-400'
      : metric.trend === 'down'
        ? 'text-rose-400'
        : 'text-muted-foreground';

  return (
    <PlannerCard className="space-y-1">
      <p className="text-xs text-muted-foreground/70 uppercase tracking-wide">
        {metric.label}
      </p>
      <div className="flex items-baseline gap-2">
        <span className="text-2xl font-bold tabular-nums text-foreground">
          {metric.value}
        </span>
        <span className="text-sm text-muted-foreground">{metric.unit}</span>
        <TrendIcon className={cn('w-4 h-4 ml-auto', trendColor, trendRotation)} />
      </div>
      {metric.target && (
        <p className="text-xs text-muted-foreground">
          Target: {metric.target} {metric.unit}
        </p>
      )}
    </PlannerCard>
  );
}

function IntegrationRow({ integration }: { integration: IntegrationStatus }) {
  const statusMap: Record<string, 'success' | 'warning' | 'error' | 'neutral'> = {
    connected: 'success',
    pending: 'warning',
    error: 'error',
    disabled: 'neutral',
  };

  return (
    <div
      className={cn(
        'flex items-center gap-3 px-3 py-2 rounded-xl',
        'bg-white/[0.02] hover:bg-white/[0.04]',
        'border border-white/[0.04]',
        'transition-colors duration-200'
      )}
    >
      <div className="p-1.5 rounded-lg bg-white/[0.04]">
        <Link2 className="w-3.5 h-3.5 text-muted-foreground" />
      </div>
      <div className="flex-1 min-w-0">
        <p className="text-sm font-medium text-foreground truncate">
          {integration.name}
        </p>
      </div>
      <StatusBadge status={statusMap[integration.status]} label={integration.status} size="sm" dot />
    </div>
  );
}

// ============================================
// Main Page Component
// ============================================

export default function PlannerHomePage() {
  return (
    <div className="flex flex-col gap-6 p-6 max-w-[1600px] mx-auto">
      {/* Page Header */}
      <header className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-foreground">
            Labor Overview
          </h1>
          <p className="text-sm text-muted-foreground mt-1">
            Workforce status, scheduling, and action items at a glance
          </p>
        </div>
        <div className="flex gap-2">
          <Link
            href="/dashboard/planner/batch-planning"
            className={cn(
              'flex items-center gap-2 px-4 py-2 rounded-xl',
              'bg-emerald-500/10 text-emerald-400 text-sm font-medium',
              'hover:bg-emerald-500/20 transition-colors duration-200'
            )}
          >
            <Calendar className="w-4 h-4" />
            Batch Planning
          </Link>
          <Link
            href="/dashboard/planner/time-approvals"
            className={cn(
              'flex items-center gap-2 px-4 py-2 rounded-xl',
              'bg-white/[0.04] text-foreground text-sm font-medium',
              'hover:bg-white/[0.06] transition-colors duration-200'
            )}
          >
            <Clock className="w-4 h-4" />
            Review Timecards
          </Link>
        </div>
      </header>

      {/* Critical Alerts Banner */}
      {criticalAlerts.length > 0 && (
        <CriticalAlertBanner alerts={criticalAlerts} />
      )}

      {/* Top Metrics Row */}
      <MetricsRow metrics={laborMetrics} />

      {/* Production Timeline */}
      <ProductionTimeline tasks={productionTasks} />

      {/* Two Column Layout: Schedule + Action Items */}
      <div className="grid gap-6 lg:grid-cols-5">
        <div className="lg:col-span-3">
          <TodaySchedule schedule={todaySchedule} />
        </div>
        <div className="lg:col-span-2">
          <ActionItems items={actionItems} />
        </div>
      </div>

      {/* Quick Stats Row */}
      <div className="grid gap-6 lg:grid-cols-3">
        {/* Productivity Metrics */}
        <div className="lg:col-span-2">
          <SectionHeader
            icon={TrendingUp}
            title="Productivity"
            subtitle="Key efficiency metrics"
            actionLabel="Details"
            actionHref="/dashboard/planner/productivity"
          />
          <div className="grid gap-3 sm:grid-cols-3">
            {productivity.map((metric) => (
              <ProductivityCard key={metric.id} metric={metric} />
            ))}
          </div>
        </div>

        {/* Integrations */}
        <div>
          <SectionHeader
            icon={Link2}
            title="Integrations"
            subtitle="Connected systems"
            actionLabel="Settings"
            actionHref="/dashboard/planner/settings"
          />
          <div className="space-y-2">
            {integrationStatuses.map((integration) => (
              <IntegrationRow key={integration.id} integration={integration} />
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
