'use client';

import React from 'react';
import { TrendingUp, ArrowUpRight, ArrowDownRight, Minus, Target, Lightbulb } from 'lucide-react';
import { productivity } from '@/features/planner/constants/laborMockData';
import { ProductivitySnapshot } from '@/features/planner/types';
import { PlannerCard, SectionHeader } from '@/features/planner/components/ui';
import { cn } from '@/lib/utils';

function MetricCard({ metric }: { metric: ProductivitySnapshot }) {
  const TrendIcon = metric.trend === 'up' ? ArrowUpRight : metric.trend === 'down' ? ArrowDownRight : Minus;
  const trendColor = metric.trend === 'up' ? 'text-emerald-400' : metric.trend === 'down' ? 'text-rose-400' : 'text-muted-foreground';
  const trendBg = metric.trend === 'up' ? 'bg-emerald-500/10' : metric.trend === 'down' ? 'bg-rose-500/10' : 'bg-white/[0.04]';
  const trendLabel = metric.trend === 'up' ? 'Improving' : metric.trend === 'down' ? 'Declining' : 'Stable';

  const percentOfTarget = metric.target ? Math.round((metric.value / metric.target) * 100) : null;

  return (
    <PlannerCard className="flex flex-col gap-4">
      <div className="flex items-start justify-between">
        <div>
          <p className="text-xs text-muted-foreground/70 uppercase tracking-wide">{metric.metric}</p>
          <h3 className="text-lg font-semibold text-foreground mt-1">{metric.label}</h3>
        </div>
        <div className={cn('flex items-center gap-1.5 px-2.5 py-1 rounded-lg', trendBg)}>
          <TrendIcon className={cn('w-4 h-4', trendColor)} />
          <span className={cn('text-xs font-medium', trendColor)}>{trendLabel}</span>
        </div>
      </div>

      <div className="flex items-baseline gap-3">
        <span className="text-4xl font-bold tabular-nums text-foreground">{metric.value}</span>
        <span className="text-lg text-muted-foreground">{metric.unit}</span>
      </div>

      {metric.target && (
        <div className="pt-3 border-t border-white/[0.04]">
          <div className="flex items-center justify-between mb-2">
            <span className="text-xs text-muted-foreground flex items-center gap-1.5">
              <Target className="w-3.5 h-3.5" />
              Target: {metric.target} {metric.unit}
            </span>
            <span className={cn(
              'text-xs font-semibold',
              percentOfTarget && percentOfTarget >= 100 ? 'text-emerald-400' : 'text-amber-400'
            )}>
              {percentOfTarget}%
            </span>
          </div>
          <div className="h-2 rounded-full bg-white/[0.06] overflow-hidden">
            <div
              className={cn(
                'h-full rounded-full transition-all duration-500',
                percentOfTarget && percentOfTarget >= 100 ? 'bg-emerald-400' : 'bg-amber-400'
              )}
              style={{ width: `${Math.min(100, percentOfTarget || 0)}%` }}
            />
          </div>
        </div>
      )}
    </PlannerCard>
  );
}

const INSIGHTS = [
  {
    title: 'Trim efficiency trending above target',
    description: 'Current staffing mix is working well. Maintain shift composition.',
    type: 'success' as const,
  },
  {
    title: 'Room turnover improving',
    description: 'Down to 11.2 days from 12. Watch cure room before harvest week.',
    type: 'success' as const,
  },
  {
    title: 'Units per labor hour flat',
    description: 'Consider task-level telemetry mapping for deeper attribution.',
    type: 'info' as const,
  },
];

export default function ProductivityPage() {
  return (
    <div className="flex flex-col gap-6 p-6 max-w-[1400px] mx-auto">
      {/* Page Header */}
      <div>
        <h1 className="text-2xl font-semibold text-foreground">Productivity & Efficiency</h1>
        <p className="text-sm text-muted-foreground mt-1">
          Track labor performance metrics and identify optimization opportunities
        </p>
      </div>

      {/* Metrics Grid */}
      <section>
        <SectionHeader
          icon={TrendingUp}
          title="Key Metrics"
          subtitle="Current performance indicators"
        />
        <div className="grid gap-4 md:grid-cols-3">
          {productivity.map((metric) => (
            <MetricCard key={metric.id} metric={metric} />
          ))}
        </div>
      </section>

      {/* Insights */}
      <section>
        <SectionHeader
          icon={Lightbulb}
          title="Insights"
          subtitle="Analysis and recommendations"
        />
        <PlannerCard className="space-y-4">
          {INSIGHTS.map((insight, index) => (
            <div
              key={index}
              className={cn(
                'flex gap-4 p-4 rounded-xl',
                insight.type === 'success' ? 'bg-emerald-500/10' : 'bg-cyan-500/10'
              )}
            >
              <div className={cn(
                'p-2 rounded-lg h-fit',
                insight.type === 'success' ? 'bg-emerald-500/20' : 'bg-cyan-500/20'
              )}>
                <Lightbulb className={cn(
                  'w-4 h-4',
                  insight.type === 'success' ? 'text-emerald-400' : 'text-cyan-400'
                )} />
              </div>
              <div>
                <p className="text-sm font-semibold text-foreground">{insight.title}</p>
                <p className="text-sm text-muted-foreground mt-0.5">{insight.description}</p>
              </div>
            </div>
          ))}
        </PlannerCard>
      </section>
    </div>
  );
}



