'use client';

import React from 'react';
import { TrendingUp, TrendingDown, Minus } from 'lucide-react';
import { cn } from '@/lib/utils';

interface KPI {
  id: string;
  label: string;
  value: string;
  unit: string;
  trend: number; 
  trendLabel: string;
  status: 'positive' | 'negative' | 'neutral';
}

const MOCK_KPIS: KPI[] = [
  {
    id: '1',
    label: 'Avg. Yield',
    value: '48.2',
    unit: 'g/sqft',
    trend: 5.2,
    trendLabel: 'vs last harvest',
    status: 'positive',
  },
  {
    id: '2',
    label: 'Daily Throughput',
    value: '1,250',
    unit: 'plants',
    trend: -2.1,
    trendLabel: 'vs yesterday',
    status: 'negative',
  },
  {
    id: '3',
    label: 'Labor Efficiency',
    value: '92',
    unit: '%',
    trend: 0.5,
    trendLabel: 'vs target',
    status: 'neutral',
  },
  {
    id: '4',
    label: 'Tasks Completed',
    value: '34',
    unit: '/ 40',
    trend: 12.5,
    trendLabel: 'completion rate',
    status: 'positive',
  },
];

export function KPICardsWidget() {
  return (
    <div className="grid grid-cols-2 gap-3 h-full">
      {MOCK_KPIS.map((kpi) => (
        <div
          key={kpi.id}
          className="flex flex-col justify-between p-4 rounded-lg tile-premium"
        >
          <div className="flex justify-between items-start">
            <span className="text-[10px] font-bold text-muted-foreground uppercase tracking-widest">
              {kpi.label}
            </span>
            {/* Trend Indicator Pill */}
            <div
              className={cn(
                'flex items-center gap-0.5 text-[10px] font-bold px-1.5 py-0.5 rounded-full border',
                kpi.status === 'positive' && 'text-emerald-400 bg-emerald-500/10 border-emerald-500/20',
                kpi.status === 'negative' && 'text-rose-400 bg-rose-500/10 border-rose-500/20',
                kpi.status === 'neutral' && 'text-muted-foreground bg-muted/50 border-border'
              )}
            >
              {kpi.status === 'positive' && <TrendingUp className="w-3 h-3" />}
              {kpi.status === 'negative' && <TrendingDown className="w-3 h-3" />}
              {kpi.status === 'neutral' && <Minus className="w-3 h-3" />}
              <span>{Math.abs(kpi.trend)}%</span>
            </div>
          </div>
          
          <div className="mt-2">
            <div className="flex items-baseline gap-1">
              <span className="text-2xl font-bold text-foreground tracking-tighter tabular-nums text-shadow-sm">
                {kpi.value}
              </span>
              <span className="text-xs font-medium text-muted-foreground">
                {kpi.unit}
              </span>
            </div>
            <span className="text-[10px] text-muted-foreground/60 mt-1 block">
              {kpi.trendLabel}
            </span>
          </div>
        </div>
      ))}
    </div>
  );
}
