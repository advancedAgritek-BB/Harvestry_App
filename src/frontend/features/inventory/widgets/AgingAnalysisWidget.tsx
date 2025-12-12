'use client';

import React from 'react';
import { Clock } from 'lucide-react';
import { cn } from '@/lib/utils';
import type { AgingAnalysis } from '../types/financial.types';

interface AgingAnalysisWidgetProps {
  agingAnalysis: AgingAnalysis | null;
  isLoading?: boolean;
  onBucketClick?: (bucket: string, category?: string) => void;
}

const formatCurrency = (value: number) => {
  if (value >= 1000000) return `$${(value / 1000000).toFixed(1)}M`;
  if (value >= 1000) return `$${(value / 1000).toFixed(1)}K`;
  return `$${value.toFixed(0)}`;
};

const bucketLabels = ['0-30', '31-60', '61-90', '91-180', '180+'];
const bucketColors = ['bg-emerald-500', 'bg-green-400', 'bg-amber-400', 'bg-orange-400', 'bg-rose-500'];

export function AgingAnalysisWidget({
  agingAnalysis,
  isLoading = false,
  onBucketClick,
}: AgingAnalysisWidgetProps) {
  if (isLoading) {
    return (
      <div className="bg-muted/30 rounded-xl p-6 border border-border">
        <div className="flex items-center gap-2 mb-6">
          <Clock className="h-5 w-5 text-cyan-500" />
          <h3 className="text-lg font-semibold text-foreground">Inventory Aging</h3>
        </div>
        <div className="h-48 bg-muted/50 rounded-lg animate-pulse" />
      </div>
    );
  }

  if (!agingAnalysis) return null;

  const { total } = agingAnalysis;
  const bucketValues = [
    total.value0To30,
    total.value31To60,
    total.value61To90,
    total.value91To180,
    total.value180Plus,
  ];
  const bucketCounts = [
    total.count0To30,
    total.count31To60,
    total.count61To90,
    total.count91To180,
    total.count180Plus,
  ];
  const totalValue = bucketValues.reduce((a, b) => a + b, 0);
  const totalCount = bucketCounts.reduce((a, b) => a + b, 0);

  return (
    <div className="bg-muted/30 rounded-xl p-6 border border-border">
      <div className="flex items-center gap-2 mb-6">
        <Clock className="h-5 w-5 text-cyan-500" />
        <h3 className="text-lg font-semibold text-foreground">Inventory Aging</h3>
      </div>

      <div className="space-y-4">
        {/* Stacked bar */}
        <div className="h-8 flex rounded-lg overflow-hidden">
          {bucketValues.map((value, i) => {
            const percent = totalValue > 0 ? (value / totalValue) * 100 : 0;
            if (percent < 1) return null;
            return (
              <button
                key={bucketLabels[i]}
                className={cn(bucketColors[i], 'hover:opacity-90 transition-opacity relative group')}
                style={{ width: `${percent}%` }}
                onClick={() => onBucketClick?.(bucketLabels[i])}
                title={`${bucketLabels[i]} days: ${formatCurrency(value)}`}
              >
                <div className="absolute inset-0 flex items-center justify-center text-xs font-medium text-white opacity-0 group-hover:opacity-100">
                  {percent.toFixed(0)}%
                </div>
              </button>
            );
          })}
        </div>

        {/* Legend */}
        <div className="grid grid-cols-5 gap-2 text-center">
          {bucketLabels.map((label, i) => (
            <button
              key={label}
              className="text-xs hover:bg-muted rounded p-1 transition-colors"
              onClick={() => onBucketClick?.(label)}
            >
              <div className={cn('w-3 h-3 rounded mx-auto mb-1', bucketColors[i])} />
              <div className="font-medium text-foreground">{label}</div>
              <div className="text-muted-foreground">{formatCurrency(bucketValues[i])}</div>
              <div className="text-muted-foreground">{bucketCounts[i]} pkgs</div>
            </button>
          ))}
        </div>

        {/* Summary */}
        <div className="pt-2 border-t border-border flex justify-between text-sm">
          <span className="text-muted-foreground">Total Packages</span>
          <span className="font-medium text-foreground">{totalCount}</span>
        </div>
        <div className="flex justify-between text-sm">
          <span className="text-muted-foreground">Total Value</span>
          <span className="font-medium text-foreground">{formatCurrency(totalValue)}</span>
        </div>

        {/* Alert for old inventory */}
        {total.value180Plus > 0 && (
          <div className="rounded-lg bg-rose-500/10 border border-rose-500/30 p-3 text-sm">
            <span className="font-medium text-rose-400">
              {formatCurrency(total.value180Plus)}
            </span>
            <span className="text-rose-300">
              {' '}in inventory over 180 days ({total.count180Plus} packages)
            </span>
          </div>
        )}
      </div>
    </div>
  );
}

export default AgingAnalysisWidget;




