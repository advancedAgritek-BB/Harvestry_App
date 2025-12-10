'use client';

import React from 'react';
import { TrendingUp, TrendingDown, Minus, DollarSign, AlertTriangle, Package } from 'lucide-react';
import { cn } from '@/lib/utils';
import type { FinancialSummary, CategoryValue, ValueAtRisk } from '../types/financial.types';

interface FinancialMetricsWidgetProps {
  financialSummary: FinancialSummary | null;
  categoryValues: CategoryValue[];
  valueAtRisk: ValueAtRisk | null;
  isLoading?: boolean;
}

const formatCurrency = (value: number) => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(value);
};

const formatPercent = (value: number) => `${value.toFixed(1)}%`;

const TrendIcon = ({ trend }: { trend: 'up' | 'down' | 'stable' }) => {
  if (trend === 'up') return <TrendingUp className="h-4 w-4 text-emerald-500" />;
  if (trend === 'down') return <TrendingDown className="h-4 w-4 text-rose-500" />;
  return <Minus className="h-4 w-4 text-muted-foreground" />;
};

export function FinancialMetricsWidget({
  financialSummary,
  categoryValues,
  valueAtRisk,
  isLoading = false,
}: FinancialMetricsWidgetProps) {
  if (isLoading) {
    return (
      <div className="bg-muted/30 rounded-xl p-6 border border-border">
        <div className="flex items-center gap-2 mb-6">
          <DollarSign className="h-5 w-5 text-emerald-500" />
          <h3 className="text-lg font-semibold text-foreground">Financial Metrics</h3>
        </div>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          {[...Array(4)].map((_, i) => (
            <div key={i} className="h-24 bg-muted/50 rounded-lg animate-pulse" />
          ))}
        </div>
      </div>
    );
  }

  const totalValue = financialSummary?.totalInventoryValue ?? 0;
  const cogsMonth = financialSummary?.cogsLast30Days ?? 0;
  const grossMargin = financialSummary?.grossMarginPercent ?? 0;
  const riskTotal = valueAtRisk?.total ?? 0;

  return (
    <div className="bg-muted/30 rounded-xl p-6 border border-border">
      <div className="flex items-center gap-2 mb-6">
        <DollarSign className="h-5 w-5 text-emerald-500" />
        <h3 className="text-lg font-semibold text-foreground">Financial Metrics</h3>
      </div>

      <div className="space-y-6">
        {/* Top KPIs */}
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <MetricCard
            label="Total Inventory Value"
            value={formatCurrency(totalValue)}
            icon={<Package className="h-5 w-5 text-cyan-500" />}
          />
          <MetricCard
            label="COGS (30 Days)"
            value={formatCurrency(cogsMonth)}
            icon={<TrendingUp className="h-5 w-5 text-amber-500" />}
          />
          <MetricCard
            label="Gross Margin"
            value={formatPercent(grossMargin)}
            trend={financialSummary?.grossMarginTrend}
            icon={<DollarSign className="h-5 w-5 text-emerald-500" />}
          />
          <MetricCard
            label="Value at Risk"
            value={formatCurrency(riskTotal)}
            icon={<AlertTriangle className="h-5 w-5 text-rose-500" />}
            variant={riskTotal > 0 ? 'warning' : 'default'}
          />
        </div>

        {/* Value by Category */}
        <div>
          <h4 className="text-sm font-medium text-muted-foreground mb-3">Value by Category</h4>
          <div className="space-y-2">
            {categoryValues.map((cat) => (
              <CategoryBar
                key={cat.category}
                category={cat.category}
                value={cat.totalValue}
                total={totalValue}
                count={cat.packageCount}
              />
            ))}
          </div>
        </div>

        {/* Value at Risk Breakdown */}
        {valueAtRisk && riskTotal > 0 && (
          <div>
            <h4 className="text-sm font-medium text-muted-foreground mb-3">Value at Risk Breakdown</h4>
            <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
              <RiskItem label="Expiring (7d)" value={valueAtRisk.expiring7Days} />
              <RiskItem label="Expiring (30d)" value={valueAtRisk.expiring30Days} />
              <RiskItem label="On Hold" value={valueAtRisk.onHold} />
              <RiskItem label="COA Failed" value={valueAtRisk.coaFailed} />
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

interface MetricCardProps {
  label: string;
  value: string;
  icon?: React.ReactNode;
  trend?: 'up' | 'down' | 'stable';
  variant?: 'default' | 'warning';
}

function MetricCard({ label, value, icon, trend, variant = 'default' }: MetricCardProps) {
  return (
    <div
      className={cn(
        'rounded-lg border p-4',
        variant === 'warning' ? 'border-amber-500/30 bg-amber-500/5' : 'border-border bg-background/50'
      )}
    >
      <div className="flex items-center justify-between mb-2">
        {icon}
        {trend && <TrendIcon trend={trend} />}
      </div>
      <div className="text-2xl font-bold text-foreground">{value}</div>
      <div className="text-xs text-muted-foreground">{label}</div>
    </div>
  );
}

interface CategoryBarProps {
  category: string;
  value: number;
  total: number;
  count: number;
}

function CategoryBar({ category, value, total, count }: CategoryBarProps) {
  const percent = total > 0 ? (value / total) * 100 : 0;
  const categoryLabels: Record<string, string> = {
    finished_good: 'Finished Goods',
    raw_material: 'Raw Materials',
    work_in_progress: 'Work in Progress',
    consumable: 'Consumables',
    byproduct: 'Byproducts',
  };

  return (
    <div className="space-y-1">
      <div className="flex justify-between text-sm">
        <span className="text-foreground">{categoryLabels[category] ?? category}</span>
        <span className="font-medium text-foreground">{formatCurrency(value)}</span>
      </div>
      <div className="h-2 bg-muted rounded-full overflow-hidden">
        <div
          className="h-full bg-emerald-500 rounded-full transition-all"
          style={{ width: `${Math.min(percent, 100)}%` }}
        />
      </div>
      <div className="text-xs text-muted-foreground">
        {count} packages • {percent.toFixed(1)}% of total
      </div>
    </div>
  );
}

interface RiskItemProps {
  label: string;
  value: number;
}

function RiskItem({ label, value }: RiskItemProps) {
  if (value === 0) return null;
  return (
    <div className="text-sm">
      <span className="text-muted-foreground">{label}:</span>
      <span className="ml-1 font-medium text-amber-500">{formatCurrency(value)}</span>
    </div>
  );
}

export default FinancialMetricsWidget;



