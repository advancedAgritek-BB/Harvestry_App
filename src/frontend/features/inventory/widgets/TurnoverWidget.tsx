'use client';

import React from 'react';
import { RefreshCw, Calendar, TrendingUp } from 'lucide-react';
import { cn } from '@/lib/utils';
import type { TurnoverMetrics } from '../types/financial.types';

interface TurnoverWidgetProps {
  turnoverMetrics: TurnoverMetrics | null;
  isLoading?: boolean;
}

const formatCurrency = (value: number) => {
  if (value >= 1000000) return `$${(value / 1000000).toFixed(2)}M`;
  if (value >= 1000) return `$${(value / 1000).toFixed(1)}K`;
  return `$${value.toFixed(0)}`;
};

export function TurnoverWidget({ turnoverMetrics, isLoading = false }: TurnoverWidgetProps) {
  if (isLoading) {
    return (
      <div className="bg-muted/30 rounded-xl p-6 border border-border">
        <div className="flex items-center gap-2 mb-6">
          <RefreshCw className="h-5 w-5 text-violet-500" />
          <h3 className="text-lg font-semibold text-foreground">Inventory Turnover</h3>
        </div>
        <div className="h-32 bg-muted/50 rounded-lg animate-pulse" />
      </div>
    );
  }

  if (!turnoverMetrics) return null;

  const {
    averageInventoryValue,
    cogsLast30Days,
    cogsLast90Days,
    turnoverRateAnnualized,
    daysOnHand,
  } = turnoverMetrics;

  const turnoverQuality =
    turnoverRateAnnualized >= 6 ? 'excellent' :
    turnoverRateAnnualized >= 4 ? 'good' :
    turnoverRateAnnualized >= 2 ? 'average' : 'low';

  const qualityColors = {
    excellent: 'text-emerald-400 bg-emerald-500/10 border-emerald-500/30',
    good: 'text-green-400 bg-green-500/10 border-green-500/30',
    average: 'text-amber-400 bg-amber-500/10 border-amber-500/30',
    low: 'text-rose-400 bg-rose-500/10 border-rose-500/30',
  };

  return (
    <div className="bg-muted/30 rounded-xl p-6 border border-border">
      <div className="flex items-center gap-2 mb-6">
        <RefreshCw className="h-5 w-5 text-violet-500" />
        <h3 className="text-lg font-semibold text-foreground">Inventory Turnover</h3>
      </div>

      <div className="space-y-4">
        {/* Main turnover metric */}
        <div className={cn('rounded-lg p-4 border', qualityColors[turnoverQuality])}>
          <div className="text-3xl font-bold">
            {turnoverRateAnnualized.toFixed(2)}x
          </div>
          <div className="text-sm opacity-80">Annualized Turnover Rate</div>
          <div className="text-xs mt-1 capitalize">{turnoverQuality} performance</div>
        </div>

        {/* Days on hand */}
        <div className="flex items-center gap-3 p-3 rounded-lg bg-muted/50">
          <Calendar className="h-8 w-8 text-muted-foreground" />
          <div>
            <div className="text-2xl font-bold text-foreground">
              {daysOnHand ?? '—'}
            </div>
            <div className="text-xs text-muted-foreground">Days of Inventory on Hand</div>
          </div>
        </div>

        {/* COGS breakdown */}
        <div className="space-y-2">
          <h4 className="text-sm font-medium text-muted-foreground">Cost of Goods Sold</h4>
          <div className="grid grid-cols-2 gap-3">
            <div className="rounded-lg border border-border p-3 bg-background/50">
              <div className="text-lg font-semibold text-foreground">{formatCurrency(cogsLast30Days)}</div>
              <div className="text-xs text-muted-foreground">Last 30 Days</div>
            </div>
            <div className="rounded-lg border border-border p-3 bg-background/50">
              <div className="text-lg font-semibold text-foreground">{formatCurrency(cogsLast90Days)}</div>
              <div className="text-xs text-muted-foreground">Last 90 Days</div>
            </div>
          </div>
        </div>

        {/* Average inventory value */}
        <div className="pt-2 border-t border-border flex justify-between text-sm">
          <span className="text-muted-foreground">Avg. Inventory Value</span>
          <span className="font-medium text-foreground">{formatCurrency(averageInventoryValue)}</span>
        </div>

        {/* Guidance */}
        <div className="text-xs text-muted-foreground bg-muted/50 p-2 rounded">
          <TrendingUp className="h-3 w-3 inline mr-1" />
          Higher turnover (4-6x annually) indicates efficient inventory management.
          Lower rates may indicate overstocking or slow-moving items.
        </div>
      </div>
    </div>
  );
}

export default TurnoverWidget;



