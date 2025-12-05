'use client';

import React, { useState } from 'react';
import { 
  AlertTriangle,
  RefreshCw,
  Trash2,
  ChevronDown,
  ChevronRight,
  ExternalLink,
  Clock,
  XCircle,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import type { DeadLetterItem, ComplianceProvider, SyncEntityType } from '../../types';

interface DLQViewerProps {
  items: DeadLetterItem[];
  isLoading?: boolean;
  onRetry?: (itemId: string) => Promise<void>;
  onDismiss?: (itemId: string, notes?: string) => Promise<void>;
  onRetryAll?: (provider: ComplianceProvider) => Promise<void>;
}

function getEntityTypeLabel(type: SyncEntityType): string {
  const labels: Record<SyncEntityType, string> = {
    plants: 'Plants',
    packages: 'Packages',
    transfers: 'Transfers',
    harvests: 'Harvests',
    sales: 'Sales',
    adjustments: 'Adjustments',
    destructions: 'Destructions',
    lab_results: 'Lab Results',
  };
  return labels[type] || type;
}

function formatDateTime(dateString: string): string {
  return new Date(dateString).toLocaleString('en-US', {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

interface DLQItemCardProps {
  item: DeadLetterItem;
  onRetry?: () => Promise<void>;
  onDismiss?: (notes?: string) => Promise<void>;
}

function DLQItemCard({ item, onRetry, onDismiss }: DLQItemCardProps) {
  const [isExpanded, setIsExpanded] = useState(false);
  const [isRetrying, setIsRetrying] = useState(false);
  const [isDismissing, setIsDismissing] = useState(false);
  const [dismissNotes, setDismissNotes] = useState('');
  const [showDismissInput, setShowDismissInput] = useState(false);
  
  const handleRetry = async () => {
    if (!onRetry) return;
    setIsRetrying(true);
    try {
      await onRetry();
    } finally {
      setIsRetrying(false);
    }
  };
  
  const handleDismiss = async () => {
    if (!onDismiss) return;
    setIsDismissing(true);
    try {
      await onDismiss(dismissNotes || undefined);
    } finally {
      setIsDismissing(false);
      setShowDismissInput(false);
      setDismissNotes('');
    }
  };
  
  return (
    <div className={cn(
      'rounded-xl border transition-all',
      'bg-gradient-to-r from-rose-500/5 to-transparent',
      'border-rose-500/20'
    )}>
      {/* Header */}
      <div
        className="flex items-center gap-3 p-4 cursor-pointer"
        onClick={() => setIsExpanded(!isExpanded)}
      >
        <button className="shrink-0">
          {isExpanded ? (
            <ChevronDown className="w-4 h-4 text-muted-foreground" />
          ) : (
            <ChevronRight className="w-4 h-4 text-muted-foreground" />
          )}
        </button>
        
        <div className="w-8 h-8 rounded-lg bg-rose-500/10 flex items-center justify-center shrink-0">
          <XCircle className="w-4 h-4 text-rose-400" />
        </div>
        
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2">
            <span className="font-mono text-sm text-foreground">{item.entityReference}</span>
            <span className={cn(
              'text-[10px] px-1.5 py-0.5 rounded uppercase',
              item.provider === 'metrc' 
                ? 'bg-emerald-500/10 text-emerald-400' 
                : 'bg-cyan-500/10 text-cyan-400'
            )}>
              {item.provider}
            </span>
            <span className="text-[10px] px-1.5 py-0.5 rounded bg-white/5 text-muted-foreground">
              {getEntityTypeLabel(item.entityType)}
            </span>
          </div>
          <p className="text-xs text-rose-400 mt-0.5 truncate">
            {item.lastErrorMessage}
          </p>
        </div>
        
        <div className="text-right shrink-0">
          <div className="text-xs text-muted-foreground">
            {formatDateTime(item.failedAt)}
          </div>
          <div className="text-[10px] text-muted-foreground">
            {item.failureCount} failures
          </div>
        </div>
      </div>
      
      {/* Expanded Content */}
      {isExpanded && (
        <div className="px-4 pb-4 border-t border-rose-500/10 mt-0 pt-4">
          {/* Error Details */}
          <div className="mb-4">
            <h4 className="text-xs text-muted-foreground uppercase tracking-wider mb-2">
              Error Details
            </h4>
            <div className="p-3 rounded-lg bg-black/20 font-mono text-xs text-rose-400 overflow-x-auto">
              <div className="mb-1">
                <span className="text-muted-foreground">Code:</span> {item.lastErrorCode}
              </div>
              <div>
                <span className="text-muted-foreground">Message:</span> {item.lastErrorMessage}
              </div>
            </div>
          </div>
          
          {/* Payload Preview */}
          {item.payload && (
            <div className="mb-4">
              <h4 className="text-xs text-muted-foreground uppercase tracking-wider mb-2">
                Payload
              </h4>
              <div className="p-3 rounded-lg bg-black/20 font-mono text-xs text-muted-foreground overflow-x-auto max-h-32">
                <pre>{JSON.stringify(item.payload, null, 2)}</pre>
              </div>
            </div>
          )}
          
          {/* Dismiss Input */}
          {showDismissInput && (
            <div className="mb-4">
              <label className="block text-xs text-muted-foreground mb-1">
                Dismissal Notes (optional)
              </label>
              <textarea
                value={dismissNotes}
                onChange={(e) => setDismissNotes(e.target.value)}
                placeholder="Why is this being dismissed?"
                rows={2}
                className={cn(
                  'w-full px-3 py-2 rounded-lg text-sm',
                  'bg-white/5 border border-border text-foreground',
                  'focus:outline-none focus:border-amber-500/30',
                  'placeholder:text-muted-foreground resize-none'
                )}
              />
            </div>
          )}
          
          {/* Actions */}
          <div className="flex items-center gap-2">
            <button
              onClick={handleRetry}
              disabled={isRetrying}
              className={cn(
                'flex items-center gap-2 px-3 py-2 rounded-lg text-sm font-medium transition-colors',
                'bg-amber-500/10 hover:bg-amber-500/20 text-amber-400'
              )}
            >
              {isRetrying ? (
                <RefreshCw className="w-4 h-4 animate-spin" />
              ) : (
                <RefreshCw className="w-4 h-4" />
              )}
              Retry
            </button>
            
            {showDismissInput ? (
              <>
                <button
                  onClick={handleDismiss}
                  disabled={isDismissing}
                  className={cn(
                    'flex items-center gap-2 px-3 py-2 rounded-lg text-sm font-medium transition-colors',
                    'bg-rose-500/10 hover:bg-rose-500/20 text-rose-400'
                  )}
                >
                  {isDismissing ? (
                    <RefreshCw className="w-4 h-4 animate-spin" />
                  ) : (
                    <Trash2 className="w-4 h-4" />
                  )}
                  Confirm Dismiss
                </button>
                <button
                  onClick={() => {
                    setShowDismissInput(false);
                    setDismissNotes('');
                  }}
                  className="px-3 py-2 rounded-lg text-sm text-muted-foreground hover:text-foreground transition-colors"
                >
                  Cancel
                </button>
              </>
            ) : (
              <button
                onClick={() => setShowDismissInput(true)}
                className={cn(
                  'flex items-center gap-2 px-3 py-2 rounded-lg text-sm font-medium transition-colors',
                  'bg-white/5 hover:bg-white/10 text-muted-foreground hover:text-foreground'
                )}
              >
                <Trash2 className="w-4 h-4" />
                Dismiss
              </button>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

export function DLQViewer({
  items,
  isLoading,
  onRetry,
  onDismiss,
  onRetryAll,
}: DLQViewerProps) {
  // Group items by provider
  const itemsByProvider = items.reduce<Record<ComplianceProvider, DeadLetterItem[]>>(
    (acc, item) => {
      if (!acc[item.provider]) {
        acc[item.provider] = [];
      }
      acc[item.provider].push(item);
      return acc;
    },
    {} as Record<ComplianceProvider, DeadLetterItem[]>
  );
  
  if (isLoading) {
    return (
      <div className="space-y-3">
        {[...Array(3)].map((_, i) => (
          <div key={i} className="h-20 rounded-xl bg-muted/30 border border-border animate-pulse" />
        ))}
      </div>
    );
  }
  
  if (items.length === 0) {
    return (
      <div className="text-center py-12">
        <div className="w-16 h-16 rounded-2xl bg-emerald-500/10 flex items-center justify-center mx-auto mb-4">
          <AlertTriangle className="w-8 h-8 text-emerald-400" />
        </div>
        <h3 className="text-lg font-medium text-foreground mb-2">No Dead Letter Items</h3>
        <p className="text-sm text-muted-foreground">
          All sync operations are processing successfully
        </p>
      </div>
    );
  }
  
  return (
    <div className="space-y-6">
      {/* Summary */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <AlertTriangle className="w-5 h-5 text-rose-400" />
          <span className="text-sm text-foreground font-medium">
            {items.length} items in dead letter queue
          </span>
        </div>
      </div>
      
      {/* Items by Provider */}
      {Object.entries(itemsByProvider).map(([provider, providerItems]) => (
        <div key={provider} className="space-y-3">
          <div className="flex items-center justify-between">
            <h4 className={cn(
              'text-sm font-semibold uppercase tracking-wider',
              provider === 'metrc' ? 'text-emerald-400' : 'text-cyan-400'
            )}>
              {provider} ({providerItems.length})
            </h4>
            {onRetryAll && providerItems.length > 1 && (
              <button
                onClick={() => onRetryAll(provider as ComplianceProvider)}
                className="flex items-center gap-1 text-xs text-amber-400 hover:text-amber-300 transition-colors"
              >
                <RefreshCw className="w-3 h-3" />
                Retry All
              </button>
            )}
          </div>
          
          <div className="space-y-2">
            {providerItems.map((item) => (
              <DLQItemCard
                key={item.id}
                item={item}
                onRetry={onRetry ? () => onRetry(item.id) : undefined}
                onDismiss={onDismiss ? (notes) => onDismiss(item.id, notes) : undefined}
              />
            ))}
          </div>
        </div>
      ))}
    </div>
  );
}

export default DLQViewer;

