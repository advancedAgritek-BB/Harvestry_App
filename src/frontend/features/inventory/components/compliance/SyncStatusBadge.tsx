'use client';

import React from 'react';
import { 
  CheckCircle, 
  RefreshCw, 
  AlertTriangle, 
  Clock,
  XCircle,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import type { SyncStatus } from '../../types';

type SyncState = SyncStatus;

interface SyncStatusBadgeProps {
  status: SyncState;
  lastSyncAt?: string;
  showLabel?: boolean;
  size?: 'sm' | 'md';
  onClick?: () => void;
}

function getSyncConfig(status: SyncState) {
  switch (status) {
    case 'synced':
      return {
        icon: CheckCircle,
        color: 'text-emerald-400',
        bgColor: 'bg-emerald-500/10',
        borderColor: 'border-emerald-500/20',
        label: 'Synced',
      };
    case 'pending':
      return {
        icon: RefreshCw,
        color: 'text-amber-400',
        bgColor: 'bg-amber-500/10',
        borderColor: 'border-amber-500/20',
        label: 'Syncing',
        animate: true,
      };
    case 'error':
      return {
        icon: XCircle,
        color: 'text-rose-400',
        bgColor: 'bg-rose-500/10',
        borderColor: 'border-rose-500/20',
        label: 'Error',
      };
    case 'stale':
      return {
        icon: Clock,
        color: 'text-amber-400',
        bgColor: 'bg-amber-500/10',
        borderColor: 'border-amber-500/20',
        label: 'Stale',
      };
    case 'not_required':
      return {
        icon: null,
        color: 'text-muted-foreground',
        bgColor: 'bg-white/5',
        borderColor: 'border-border',
        label: 'N/A',
      };
    default:
      return {
        icon: AlertTriangle,
        color: 'text-muted-foreground',
        bgColor: 'bg-white/5',
        borderColor: 'border-border',
        label: 'Unknown',
      };
  }
}

function formatTimeAgo(timestamp?: string): string {
  if (!timestamp) return '';
  
  const now = new Date();
  const then = new Date(timestamp);
  const diffMs = now.getTime() - then.getTime();
  const diffMins = Math.floor(diffMs / 60000);
  
  if (diffMins < 1) return 'Just now';
  if (diffMins < 60) return `${diffMins}m ago`;
  
  const diffHours = Math.floor(diffMins / 60);
  if (diffHours < 24) return `${diffHours}h ago`;
  
  const diffDays = Math.floor(diffHours / 24);
  return `${diffDays}d ago`;
}

export function SyncStatusBadge({
  status,
  lastSyncAt,
  showLabel = true,
  size = 'sm',
  onClick,
}: SyncStatusBadgeProps) {
  const config = getSyncConfig(status);
  const Icon = config.icon;
  
  if (!Icon) return null;
  
  const iconSize = size === 'sm' ? 'w-3 h-3' : 'w-4 h-4';
  const padding = size === 'sm' ? 'px-1.5 py-0.5' : 'px-2 py-1';
  const textSize = size === 'sm' ? 'text-[10px]' : 'text-xs';
  
  const badge = (
    <div
      className={cn(
        'inline-flex items-center gap-1 rounded-full border',
        config.bgColor,
        config.borderColor,
        padding,
        onClick && 'cursor-pointer hover:opacity-80 transition-opacity'
      )}
      onClick={onClick}
      title={lastSyncAt ? `Last sync: ${formatTimeAgo(lastSyncAt)}` : undefined}
    >
      <Icon
        className={cn(
          iconSize,
          config.color,
          config.animate && 'animate-spin'
        )}
      />
      {showLabel && (
        <span className={cn(textSize, 'font-medium', config.color)}>
          {config.label}
        </span>
      )}
      {lastSyncAt && size === 'md' && (
        <span className={cn(textSize, 'text-muted-foreground')}>
          • {formatTimeAgo(lastSyncAt)}
        </span>
      )}
    </div>
  );
  
  return badge;
}

export default SyncStatusBadge;

