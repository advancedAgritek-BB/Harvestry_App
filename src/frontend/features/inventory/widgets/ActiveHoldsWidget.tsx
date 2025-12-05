'use client';

import React from 'react';
import { AlertTriangle, Clock, XCircle, ArrowRight, Shield } from 'lucide-react';
import { cn } from '@/lib/utils';
import type { Hold } from '../types';

interface ActiveHoldsWidgetProps {
  holds: Hold[];
  onViewHold?: (holdId: string) => void;
  onReleaseHold?: (holdId: string) => void;
  loading?: boolean;
  className?: string;
}

const REASON_LABELS: Record<string, { label: string; icon: React.ElementType; color: string }> = {
  coa_failed: { label: 'COA Failed', icon: XCircle, color: 'text-rose-400 bg-rose-500/10' },
  coa_pending: { label: 'COA Pending', icon: Clock, color: 'text-amber-400 bg-amber-500/10' },
  contamination: { label: 'Contamination', icon: AlertTriangle, color: 'text-rose-400 bg-rose-500/10' },
  quality_issue: { label: 'Quality Issue', icon: AlertTriangle, color: 'text-amber-400 bg-amber-500/10' },
  regulatory: { label: 'Regulatory Hold', icon: Shield, color: 'text-violet-400 bg-violet-500/10' },
  customer_return: { label: 'Customer Return', icon: ArrowRight, color: 'text-cyan-400 bg-cyan-500/10' },
  investigation: { label: 'Under Investigation', icon: Clock, color: 'text-amber-400 bg-amber-500/10' },
  other: { label: 'Other', icon: AlertTriangle, color: 'text-muted-foreground bg-white/5' },
};

interface HoldItemProps {
  hold: Hold;
  onView?: () => void;
  onRelease?: () => void;
}

function HoldItem({ hold, onView, onRelease }: HoldItemProps) {
  const reasonConfig = REASON_LABELS[hold.reasonCode] ?? REASON_LABELS.other;
  const Icon = reasonConfig.icon;
  
  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
    
    if (diffHours < 1) return 'Just now';
    if (diffHours < 24) return `${diffHours}h ago`;
    if (diffHours < 48) return 'Yesterday';
    return date.toLocaleDateString();
  };

  return (
    <div className="group flex items-center gap-3 p-3 rounded-lg bg-muted/30 border border-border hover:border-rose-500/30 transition-all">
      <div className={cn(
        'w-9 h-9 rounded-lg flex items-center justify-center shrink-0',
        reasonConfig.color.split(' ')[1]
      )}>
        <Icon className={cn('w-4 h-4', reasonConfig.color.split(' ')[0])} />
      </div>

      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2">
          <span className="text-sm font-mono text-foreground truncate">
            {hold.lotNumber}
          </span>
          {hold.requiresTwoPersonApproval && (
            <span className="px-1.5 py-0.5 rounded text-[10px] font-medium bg-violet-500/10 text-violet-400">
              2-PERSON
            </span>
          )}
        </div>
        <div className="flex items-center gap-2 mt-0.5">
          <span className={cn('text-xs', reasonConfig.color.split(' ')[0])}>
            {reasonConfig.label}
          </span>
          <span className="text-[10px] text-muted-foreground">
            • {formatDate(hold.createdAt)}
          </span>
        </div>
      </div>

      <div className="flex items-center gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
        <button
          onClick={onView}
          className="p-1.5 rounded-md hover:bg-white/5 text-muted-foreground hover:text-foreground transition-colors"
        >
          <ArrowRight className="w-4 h-4" />
        </button>
      </div>
    </div>
  );
}

export function ActiveHoldsWidget({
  holds,
  onViewHold,
  onReleaseHold,
  loading,
  className,
}: ActiveHoldsWidgetProps) {
  const activeHolds = holds.filter((h) => h.isActive);
  const criticalHolds = activeHolds.filter(
    (h) => h.reasonCode === 'coa_failed' || h.reasonCode === 'contamination'
  );

  if (loading) {
    return (
      <div className={cn('space-y-4', className)}>
        <div className="flex items-center justify-between">
          <h3 className="text-sm font-semibold text-foreground">Active Holds</h3>
        </div>
        <div className="space-y-2">
          {[1, 2, 3].map((i) => (
            <div key={i} className="rounded-lg p-3 bg-muted/30 border border-border animate-pulse">
              <div className="flex items-center gap-3">
                <div className="w-9 h-9 rounded-lg bg-white/5" />
                <div className="flex-1">
                  <div className="h-4 w-24 bg-white/5 rounded mb-1" />
                  <div className="h-3 w-16 bg-white/5 rounded" />
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
          <h3 className="text-sm font-semibold text-foreground">Active Holds</h3>
          {activeHolds.length > 0 && (
            <span className={cn(
              'px-2 py-0.5 rounded-full text-xs font-medium',
              criticalHolds.length > 0 
                ? 'bg-rose-500/10 text-rose-400' 
                : 'bg-amber-500/10 text-amber-400'
            )}>
              {activeHolds.length}
            </span>
          )}
        </div>
        {activeHolds.length > 0 && (
          <button className="text-xs text-cyan-400 hover:underline">
            View All
          </button>
        )}
      </div>

      {/* Critical Alert Banner */}
      {criticalHolds.length > 0 && (
        <div className="flex items-center gap-3 p-3 rounded-lg bg-rose-500/10 border border-rose-500/20">
          <AlertTriangle className="w-4 h-4 text-rose-400 shrink-0" />
          <div className="flex-1">
            <p className="text-xs font-medium text-rose-400">
              {criticalHolds.length} critical hold{criticalHolds.length > 1 ? 's' : ''} require immediate attention
            </p>
          </div>
        </div>
      )}

      {/* Holds List */}
      {activeHolds.length === 0 ? (
        <div className="text-center py-8">
          <div className="w-12 h-12 rounded-full bg-emerald-500/10 flex items-center justify-center mx-auto mb-3">
            <Shield className="w-6 h-6 text-emerald-400" />
          </div>
          <p className="text-sm text-muted-foreground">No active holds</p>
          <p className="text-xs text-muted-foreground mt-1">All lots are cleared for operations</p>
        </div>
      ) : (
        <div className="space-y-2 max-h-[300px] overflow-y-auto scrollbar-thin scrollbar-thumb-white/10">
          {activeHolds.slice(0, 10).map((hold) => (
            <HoldItem
              key={hold.id}
              hold={hold}
              onView={() => onViewHold?.(hold.id)}
              onRelease={() => onReleaseHold?.(hold.id)}
            />
          ))}
          {activeHolds.length > 10 && (
            <button className="w-full py-2 text-xs text-muted-foreground hover:text-foreground text-center">
              +{activeHolds.length - 10} more holds
            </button>
          )}
        </div>
      )}
    </div>
  );
}

export default ActiveHoldsWidget;
