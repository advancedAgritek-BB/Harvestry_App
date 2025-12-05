'use client';

import React from 'react';
import { AlertTriangle, CheckCircle, Scale, ArrowRight, RefreshCw } from 'lucide-react';
import { cn } from '@/lib/utils';

interface BalanceDiscrepancy {
  id: string;
  lotId: string;
  lotNumber: string;
  locationPath: string;
  expectedQuantity: number;
  actualQuantity: number;
  variance: number;
  variancePercent: number;
  uom: string;
  detectedAt: string;
  status: 'open' | 'investigating' | 'resolved';
}

interface BalanceAlertWidgetProps {
  discrepancies?: BalanceDiscrepancy[];
  overallVariancePercent?: number;
  lastReconcileAt?: string;
  onViewDiscrepancy?: (id: string) => void;
  onReconcile?: () => void;
  loading?: boolean;
  className?: string;
}

function DiscrepancyItem({
  discrepancy,
  onView,
}: {
  discrepancy: BalanceDiscrepancy;
  onView?: () => void;
}) {
  const isPositive = discrepancy.variance > 0;
  
  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
    
    if (diffHours < 1) return 'Just now';
    if (diffHours < 24) return `${diffHours}h ago`;
    return date.toLocaleDateString(undefined, { month: 'short', day: 'numeric' });
  };

  return (
    <div className="group flex items-center gap-3 p-3 rounded-lg bg-muted/30 border border-border hover:border-amber-500/30 transition-all">
      <div className={cn(
        'w-8 h-8 rounded-lg flex items-center justify-center shrink-0',
        isPositive ? 'bg-emerald-500/10' : 'bg-rose-500/10'
      )}>
        <Scale className={cn('w-4 h-4', isPositive ? 'text-emerald-400' : 'text-rose-400')} />
      </div>

      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2">
          <span className="text-sm font-mono text-foreground truncate">
            {discrepancy.lotNumber}
          </span>
          <span className={cn(
            'text-xs px-1.5 py-0.5 rounded font-medium',
            discrepancy.status === 'open' && 'bg-amber-500/10 text-amber-400',
            discrepancy.status === 'investigating' && 'bg-blue-500/10 text-blue-400',
            discrepancy.status === 'resolved' && 'bg-emerald-500/10 text-emerald-400'
          )}>
            {discrepancy.status}
          </span>
        </div>
        <div className="text-[10px] text-muted-foreground mt-0.5 truncate">
          {discrepancy.locationPath}
        </div>
      </div>

      <div className="text-right shrink-0">
        <div className={cn(
          'text-sm font-medium tabular-nums',
          isPositive ? 'text-emerald-400' : 'text-rose-400'
        )}>
          {isPositive ? '+' : ''}{discrepancy.variance} {discrepancy.uom}
        </div>
        <div className="text-[10px] text-muted-foreground">
          {formatDate(discrepancy.detectedAt)}
        </div>
      </div>

      <button
        onClick={onView}
        className="p-1.5 rounded-md opacity-0 group-hover:opacity-100 hover:bg-white/5 text-muted-foreground hover:text-foreground transition-all"
      >
        <ArrowRight className="w-4 h-4" />
      </button>
    </div>
  );
}

export function BalanceAlertWidget({
  discrepancies = [],
  overallVariancePercent = 0,
  lastReconcileAt,
  onViewDiscrepancy,
  onReconcile,
  loading,
  className,
}: BalanceAlertWidgetProps) {
  const openDiscrepancies = discrepancies.filter((d) => d.status === 'open');
  const hasIssues = openDiscrepancies.length > 0 || Math.abs(overallVariancePercent) > 0.5;

  const formatLastReconcile = () => {
    if (!lastReconcileAt) return 'Never';
    const date = new Date(lastReconcileAt);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
    
    if (diffHours < 1) return 'Just now';
    if (diffHours < 24) return `${diffHours} hours ago`;
    if (diffHours < 48) return 'Yesterday';
    return date.toLocaleDateString();
  };

  if (loading) {
    return (
      <div className={cn('space-y-4', className)}>
        <div className="flex items-center justify-between">
          <h3 className="text-sm font-semibold text-foreground">Balance Alerts</h3>
        </div>
        <div className="space-y-2">
          {[1, 2, 3].map((i) => (
            <div key={i} className="rounded-lg p-3 bg-muted/30 border border-border animate-pulse">
              <div className="flex items-center gap-3">
                <div className="w-8 h-8 rounded-lg bg-white/5" />
                <div className="flex-1">
                  <div className="h-4 w-24 bg-white/5 rounded mb-1" />
                  <div className="h-3 w-32 bg-white/5 rounded" />
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className={cn('space-y-4', className)}>
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <h3 className="text-sm font-semibold text-foreground">Balance Alerts</h3>
          {hasIssues && (
            <span className="px-2 py-0.5 rounded-full text-xs font-medium bg-amber-500/10 text-amber-400">
              {openDiscrepancies.length}
            </span>
          )}
        </div>
        <button 
          onClick={onReconcile}
          className="flex items-center gap-1.5 text-xs text-cyan-400 hover:underline"
        >
          <RefreshCw className="w-3 h-3" />
          Reconcile
        </button>
      </div>

      {/* Status Card */}
      <div className={cn(
        'p-4 rounded-xl border',
        hasIssues 
          ? 'bg-amber-500/5 border-amber-500/20' 
          : 'bg-emerald-500/5 border-emerald-500/20'
      )}>
        <div className="flex items-center justify-between mb-3">
          <div className="flex items-center gap-2">
            {hasIssues ? (
              <AlertTriangle className="w-4 h-4 text-amber-400" />
            ) : (
              <CheckCircle className="w-4 h-4 text-emerald-400" />
            )}
            <span className={cn(
              'text-sm font-medium',
              hasIssues ? 'text-amber-400' : 'text-emerald-400'
            )}>
              {hasIssues ? 'Discrepancies Detected' : 'All Balanced'}
            </span>
          </div>
          <div className={cn(
            'text-lg font-bold tabular-nums',
            Math.abs(overallVariancePercent) > 0.5 ? 'text-amber-400' : 'text-emerald-400'
          )}>
            {overallVariancePercent >= 0 ? '+' : ''}{overallVariancePercent.toFixed(2)}%
          </div>
        </div>
        <div className="flex items-center justify-between text-xs text-muted-foreground">
          <span>Overall variance</span>
          <span>Last reconcile: {formatLastReconcile()}</span>
        </div>
      </div>

      {/* Discrepancies List */}
      {openDiscrepancies.length > 0 && (
        <div className="space-y-2 max-h-[250px] overflow-y-auto scrollbar-thin scrollbar-thumb-white/10">
          {openDiscrepancies.slice(0, 5).map((discrepancy) => (
            <DiscrepancyItem
              key={discrepancy.id}
              discrepancy={discrepancy}
              onView={() => onViewDiscrepancy?.(discrepancy.id)}
            />
          ))}
          {openDiscrepancies.length > 5 && (
            <button className="w-full py-2 text-xs text-muted-foreground hover:text-foreground text-center">
              +{openDiscrepancies.length - 5} more discrepancies
            </button>
          )}
        </div>
      )}

      {/* Empty State */}
      {!hasIssues && openDiscrepancies.length === 0 && (
        <div className="text-center py-4">
          <p className="text-xs text-muted-foreground">
            No balance discrepancies detected
          </p>
        </div>
      )}
    </div>
  );
}

export default BalanceAlertWidget;
