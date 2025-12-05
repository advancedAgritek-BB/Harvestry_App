'use client';

import React from 'react';
import { RefreshCw, CheckCircle, AlertCircle, Clock, XCircle } from 'lucide-react';
import { cn } from '@/lib/utils';
import type { ComplianceIntegration, SyncQueueStatus } from '../types';

interface SyncStatusWidgetProps {
  integrations: ComplianceIntegration[];
  syncStatus: SyncQueueStatus[];
  onSync?: (provider: 'metrc' | 'biotrack', siteId: string) => void;
  loading?: boolean;
  className?: string;
}

interface ProviderCardProps {
  provider: 'metrc' | 'biotrack';
  siteName: string;
  siteId: string;
  isConnected: boolean;
  lastSyncAt?: string;
  pendingCount: number;
  errorCount: number;
  successRate: number;
  onSync?: () => void;
  syncing?: boolean;
}

function ProviderCard({
  provider,
  siteName,
  siteId,
  isConnected,
  lastSyncAt,
  pendingCount,
  errorCount,
  successRate,
  onSync,
  syncing,
}: ProviderCardProps) {
  const providerColors = {
    metrc: {
      bg: 'bg-emerald-500/10',
      text: 'text-emerald-400',
      border: 'border-emerald-500/20',
      glow: 'shadow-[0_0_20px_rgba(16,185,129,0.15)]',
    },
    biotrack: {
      bg: 'bg-blue-500/10',
      text: 'text-blue-400',
      border: 'border-blue-500/20',
      glow: 'shadow-[0_0_20px_rgba(59,130,246,0.15)]',
    },
  };

  const colors = providerColors[provider];
  
  const getStatusIcon = () => {
    if (!isConnected) return <XCircle className="w-4 h-4 text-rose-400" />;
    if (errorCount > 0) return <AlertCircle className="w-4 h-4 text-amber-400" />;
    if (pendingCount > 0) return <Clock className="w-4 h-4 text-cyan-400" />;
    return <CheckCircle className="w-4 h-4 text-emerald-400" />;
  };

  const getStatusText = () => {
    if (!isConnected) return 'Disconnected';
    if (errorCount > 0) return `${errorCount} errors`;
    if (pendingCount > 0) return `${pendingCount} pending`;
    return 'Synced';
  };

  const formatLastSync = (timestamp?: string) => {
    if (!timestamp) return 'Never';
    const date = new Date(timestamp);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    
    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    if (diffMins < 1440) return `${Math.floor(diffMins / 60)}h ago`;
    return date.toLocaleDateString();
  };

  return (
    <div className={cn(
      'relative rounded-xl p-4 border transition-all',
      colors.border,
      isConnected ? 'bg-muted/30' : 'bg-rose-500/5',
      isConnected && pendingCount === 0 && errorCount === 0 && colors.glow
    )}>
      {/* Header */}
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center gap-3">
          <div className={cn('w-8 h-8 rounded-lg flex items-center justify-center', colors.bg)}>
            <span className={cn('text-xs font-bold uppercase', colors.text)}>
              {provider === 'metrc' ? 'M' : 'BT'}
            </span>
          </div>
          <div>
            <h4 className={cn('text-sm font-semibold', colors.text)}>
              {provider.toUpperCase()}
            </h4>
            <p className="text-xs text-muted-foreground">{siteName}</p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          {getStatusIcon()}
          <span className="text-xs text-muted-foreground">{getStatusText()}</span>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-3 gap-3 mb-4">
        <div className="text-center">
          <div className="text-lg font-semibold text-foreground tabular-nums">
            {pendingCount}
          </div>
          <div className="text-xs text-muted-foreground">Pending</div>
        </div>
        <div className="text-center">
          <div className={cn(
            'text-lg font-semibold tabular-nums',
            errorCount > 0 ? 'text-rose-400' : 'text-foreground'
          )}>
            {errorCount}
          </div>
          <div className="text-xs text-muted-foreground">Errors</div>
        </div>
        <div className="text-center">
          <div className={cn(
            'text-lg font-semibold tabular-nums',
            successRate >= 99 ? 'text-emerald-400' : successRate >= 95 ? 'text-amber-400' : 'text-rose-400'
          )}>
            {successRate.toFixed(1)}%
          </div>
          <div className="text-xs text-muted-foreground">Success</div>
        </div>
      </div>

      {/* Footer */}
      <div className="flex items-center justify-between pt-3 border-t border-border">
        <span className="text-xs text-muted-foreground">
          Last sync: {formatLastSync(lastSyncAt)}
        </span>
        <button
          onClick={onSync}
          disabled={!isConnected || syncing}
          className={cn(
            'flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-medium transition-all',
            'disabled:opacity-50 disabled:cursor-not-allowed',
            colors.bg,
            colors.text,
            'hover:opacity-80'
          )}
        >
          <RefreshCw className={cn('w-3 h-3', syncing && 'animate-spin')} />
          {syncing ? 'Syncing...' : 'Sync Now'}
        </button>
      </div>
    </div>
  );
}

export function SyncStatusWidget({
  integrations,
  syncStatus,
  onSync,
  loading,
  className,
}: SyncStatusWidgetProps) {
  if (loading) {
    return (
      <div className={cn('space-y-4', className)}>
        <div className="flex items-center justify-between">
          <h3 className="text-sm font-semibold text-foreground">Compliance Sync</h3>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {[1, 2].map((i) => (
            <div key={i} className="rounded-xl p-4 border border-border bg-muted/30 animate-pulse">
              <div className="h-4 w-20 bg-white/5 rounded mb-4" />
              <div className="h-8 w-full bg-white/5 rounded" />
            </div>
          ))}
        </div>
      </div>
    );
  }

  // Combine integrations with sync status
  const combinedData = integrations.map((integration) => {
    const status = syncStatus.find(
      (s) => s.provider === integration.provider && s.siteId === integration.siteId
    );
    return {
      ...integration,
      pendingCount: status?.pendingCount ?? integration.pendingCount,
      errorCount: status?.failedCount ?? integration.errorCount,
      successRate: status?.successRate ?? 100,
    };
  });

  return (
    <div className={cn('space-y-4', className)}>
      <div className="flex items-center justify-between">
        <h3 className="text-sm font-semibold text-foreground">Compliance Sync</h3>
        <div className="flex items-center gap-2">
          {integrations.every((i) => i.isConnected) ? (
            <span className="flex items-center gap-1.5 text-xs text-emerald-400">
              <CheckCircle className="w-3 h-3" />
              All Connected
            </span>
          ) : (
            <span className="flex items-center gap-1.5 text-xs text-amber-400">
              <AlertCircle className="w-3 h-3" />
              Connection Issues
            </span>
          )}
        </div>
      </div>

      {combinedData.length === 0 ? (
        <div className="text-center py-8 text-muted-foreground">
          <p className="text-sm">No compliance integrations configured</p>
          <button className="mt-2 text-xs text-cyan-400 hover:underline">
            Configure Integrations →
          </button>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {combinedData.map((item) => (
            <ProviderCard
              key={`${item.provider}-${item.siteId}`}
              provider={item.provider as 'metrc' | 'biotrack'}
              siteName={item.siteId} // TODO: Resolve site name
              siteId={item.siteId}
              isConnected={item.isConnected}
              lastSyncAt={item.lastSyncAt}
              pendingCount={item.pendingCount}
              errorCount={item.errorCount}
              successRate={item.successRate}
              onSync={() => onSync?.(item.provider as 'metrc' | 'biotrack', item.siteId)}
            />
          ))}
        </div>
      )}
    </div>
  );
}

export default SyncStatusWidget;
