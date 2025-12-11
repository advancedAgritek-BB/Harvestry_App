'use client';

import React from 'react';
import {
  Users,
  DollarSign,
  Clock,
  TrendingUp,
  TrendingDown,
  Minus,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { LaborMetrics } from '../../types';
import { PlannerCard } from '../ui';

interface MetricsRowProps {
  metrics: LaborMetrics;
  className?: string;
}

type TrendDirection = 'up' | 'down' | 'flat';

function TrendIndicator({
  trend,
  value,
  inverted = false,
}: {
  trend: TrendDirection;
  value?: number;
  inverted?: boolean;
}) {
  const Icon =
    trend === 'up' ? TrendingUp : trend === 'down' ? TrendingDown : Minus;

  // For some metrics, "down" is good (e.g., costs under budget)
  const isPositive = inverted
    ? trend === 'down'
    : trend === 'up';

  const colorClass =
    trend === 'flat'
      ? 'text-muted-foreground'
      : isPositive
        ? 'text-emerald-400'
        : 'text-rose-400';

  return (
    <div className={cn('flex items-center gap-1 text-sm', colorClass)}>
      <Icon className="w-4 h-4" />
      {value !== undefined && (
        <span className="font-medium tabular-nums">
          {trend === 'up' ? '+' : trend === 'down' ? '-' : ''}
          {Math.abs(value).toFixed(1)}%
        </span>
      )}
    </div>
  );
}

function CoverageCard({
  coverage,
}: {
  coverage: LaborMetrics['coverage'];
}) {
  const { percentage, filledPositions, requiredPositions, trend, trendValue } =
    coverage;

  const getColorClass = (pct: number) => {
    if (pct >= 95) return 'text-emerald-400';
    if (pct >= 80) return 'text-amber-400';
    return 'text-rose-400';
  };

  const getVariant = (pct: number): 'emerald' | 'amber' | 'rose' => {
    if (pct >= 95) return 'emerald';
    if (pct >= 80) return 'amber';
    return 'rose';
  };

  return (
    <PlannerCard variant={getVariant(percentage)} className="flex-1 min-w-0">
      <div className="flex items-start justify-between gap-2">
        <div className="flex items-center gap-2">
          <div className="p-2 rounded-xl bg-white/[0.06]">
            <Users className="w-5 h-5 text-muted-foreground" />
          </div>
          <span className="text-sm font-medium text-muted-foreground">
            Coverage
          </span>
        </div>
        <TrendIndicator trend={trend} value={trendValue} />
      </div>

      <div className="mt-3">
        <div className="flex items-baseline gap-1">
          <span
            className={cn(
              'text-4xl font-bold tabular-nums',
              getColorClass(percentage)
            )}
          >
            {percentage.toFixed(0)}
          </span>
          <span className="text-xl text-muted-foreground">%</span>
        </div>
        <p className="text-sm text-muted-foreground mt-1">
          {filledPositions} of {requiredPositions} positions filled
        </p>
      </div>
    </PlannerCard>
  );
}

function LaborCostCard({
  laborCost,
}: {
  laborCost: LaborMetrics['laborCost'];
}) {
  const { actual, budget, variance, variancePercent, period } = laborCost;
  const isUnderBudget = variance <= 0;

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(amount);
  };

  const periodLabel =
    period === 'day' ? 'Today' : period === 'week' ? 'This Week' : 'This Month';

  return (
    <PlannerCard
      variant={isUnderBudget ? 'emerald' : 'amber'}
      className="flex-1 min-w-0"
    >
      <div className="flex items-start justify-between gap-2">
        <div className="flex items-center gap-2">
          <div className="p-2 rounded-xl bg-white/[0.06]">
            <DollarSign className="w-5 h-5 text-muted-foreground" />
          </div>
          <span className="text-sm font-medium text-muted-foreground">
            Labor Cost
          </span>
        </div>
        <TrendIndicator
          trend={variance > 0 ? 'up' : variance < 0 ? 'down' : 'flat'}
          value={Math.abs(variancePercent)}
          inverted
        />
      </div>

      <div className="mt-3">
        <div className="flex items-baseline gap-1">
          <span className="text-4xl font-bold tabular-nums text-foreground">
            {formatCurrency(actual)}
          </span>
        </div>
        <p className="text-sm text-muted-foreground mt-1">
          {periodLabel} • Budget: {formatCurrency(budget)}
        </p>
      </div>

      {/* Variance indicator */}
      <div
        className={cn(
          'mt-3 pt-3 border-t border-white/[0.06]',
          'flex items-center justify-between'
        )}
      >
        <span className="text-xs text-muted-foreground">Variance</span>
        <span
          className={cn(
            'text-sm font-semibold tabular-nums',
            isUnderBudget ? 'text-emerald-400' : 'text-amber-400'
          )}
        >
          {isUnderBudget ? '' : '+'}
          {formatCurrency(variance)}
        </span>
      </div>
    </PlannerCard>
  );
}

function OvertimeCard({ overtime }: { overtime: LaborMetrics['overtime'] }) {
  const { hoursThisWeek, hoursLastWeek, trend, cost } = overtime;
  const diff = hoursThisWeek - hoursLastWeek;

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(amount);
  };

  return (
    <PlannerCard
      variant={trend === 'down' ? 'emerald' : trend === 'up' ? 'amber' : 'default'}
      className="flex-1 min-w-0"
    >
      <div className="flex items-start justify-between gap-2">
        <div className="flex items-center gap-2">
          <div className="p-2 rounded-xl bg-white/[0.06]">
            <Clock className="w-5 h-5 text-muted-foreground" />
          </div>
          <span className="text-sm font-medium text-muted-foreground">
            Overtime
          </span>
        </div>
        <TrendIndicator trend={trend} inverted />
      </div>

      <div className="mt-3">
        <div className="flex items-baseline gap-1">
          <span className="text-4xl font-bold tabular-nums text-foreground">
            {hoursThisWeek.toFixed(1)}
          </span>
          <span className="text-xl text-muted-foreground">hrs</span>
        </div>
        <p className="text-sm text-muted-foreground mt-1">
          This week • {formatCurrency(cost)} cost
        </p>
      </div>

      {/* Comparison to last week */}
      <div
        className={cn(
          'mt-3 pt-3 border-t border-white/[0.06]',
          'flex items-center justify-between'
        )}
      >
        <span className="text-xs text-muted-foreground">vs Last Week</span>
        <span
          className={cn(
            'text-sm font-semibold tabular-nums',
            diff <= 0 ? 'text-emerald-400' : 'text-amber-400'
          )}
        >
          {diff > 0 ? '+' : ''}
          {diff.toFixed(1)} hrs
        </span>
      </div>
    </PlannerCard>
  );
}

export function MetricsRow({ metrics, className }: MetricsRowProps) {
  return (
    <div
      className={cn(
        'grid gap-4',
        'grid-cols-1 sm:grid-cols-2 lg:grid-cols-3',
        className
      )}
    >
      <CoverageCard coverage={metrics.coverage} />
      <LaborCostCard laborCost={metrics.laborCost} />
      <OvertimeCard overtime={metrics.overtime} />
    </div>
  );
}

export default MetricsRow;
