'use client';

import React from 'react';
import { 
  Package,
  MapPin,
  Clock,
  AlertTriangle,
  CheckCircle,
  RefreshCw,
  MoreVertical,
  ChevronRight,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import type { InventoryLot, LotStatus, QAStatus, SyncState } from '../../types';

interface LotCardProps {
  lot: InventoryLot;
  isSelected?: boolean;
  onSelect?: () => void;
  onClick?: () => void;
  onAction?: (action: 'view' | 'move' | 'split' | 'adjust' | 'hold') => void;
  compact?: boolean;
}

function getStatusConfig(status: LotStatus) {
  switch (status) {
    case 'available':
      return { color: 'text-emerald-400', bgColor: 'bg-emerald-500/10', label: 'Available' };
    case 'reserved':
      return { color: 'text-cyan-400', bgColor: 'bg-cyan-500/10', label: 'Reserved' };
    case 'on_hold':
      return { color: 'text-rose-400', bgColor: 'bg-rose-500/10', label: 'On Hold' };
    case 'quarantine':
      return { color: 'text-rose-400', bgColor: 'bg-rose-500/10', label: 'Quarantine' };
    case 'in_transit':
      return { color: 'text-amber-400', bgColor: 'bg-amber-500/10', label: 'In Transit' };
    case 'consumed':
      return { color: 'text-muted-foreground', bgColor: 'bg-muted/50', label: 'Consumed' };
    case 'destroyed':
      return { color: 'text-muted-foreground', bgColor: 'bg-muted/50', label: 'Destroyed' };
    default:
      return { color: 'text-muted-foreground', bgColor: 'bg-white/5', label: 'Unknown' };
  }
}

function getQAStatusConfig(status: QAStatus) {
  switch (status) {
    case 'passed':
      return { icon: CheckCircle, color: 'text-emerald-400' };
    case 'failed':
      return { icon: AlertTriangle, color: 'text-rose-400' };
    case 'pending':
      return { icon: Clock, color: 'text-amber-400' };
    case 'expired':
      return { icon: AlertTriangle, color: 'text-rose-400' };
    default:
      return { icon: Clock, color: 'text-muted-foreground' };
  }
}

function getSyncStatusConfig(status: SyncState) {
  switch (status) {
    case 'synced':
      return { icon: CheckCircle, color: 'text-emerald-400', label: 'Synced' };
    case 'pending':
      return { icon: RefreshCw, color: 'text-amber-400', label: 'Syncing', animate: true };
    case 'error':
      return { icon: AlertTriangle, color: 'text-rose-400', label: 'Sync Error' };
    case 'stale':
      return { icon: Clock, color: 'text-amber-400', label: 'Stale' };
    case 'not_required':
      return { icon: null, color: '', label: '' };
    default:
      return { icon: null, color: '', label: '' };
  }
}

function formatDaysUntilExpiration(expirationDate?: string): { label: string; isUrgent: boolean } | null {
  if (!expirationDate) return null;
  
  const now = new Date();
  const expiry = new Date(expirationDate);
  const diffDays = Math.floor((expiry.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));
  
  if (diffDays < 0) return { label: 'Expired', isUrgent: true };
  if (diffDays === 0) return { label: 'Expires today', isUrgent: true };
  if (diffDays <= 7) return { label: `${diffDays}d left`, isUrgent: true };
  if (diffDays <= 30) return { label: `${diffDays}d left`, isUrgent: false };
  
  return null;
}

export function LotCard({ 
  lot, 
  isSelected, 
  onSelect, 
  onClick, 
  onAction,
  compact = false,
}: LotCardProps) {
  const statusConfig = getStatusConfig(lot.status);
  const qaConfig = getQAStatusConfig(lot.qaStatus);
  const syncConfig = getSyncStatusConfig(lot.syncStatus);
  const expirationInfo = formatDaysUntilExpiration(lot.expirationDate);
  const QAIcon = qaConfig.icon;
  const SyncIcon = syncConfig.icon;
  
  if (compact) {
    return (
      <div
        className={cn(
          'flex items-center gap-3 p-3 rounded-lg border transition-all cursor-pointer group',
          'bg-gradient-to-r from-white/[0.02] to-transparent',
          isSelected 
            ? 'border-amber-500/50 bg-amber-500/5' 
            : 'border-border hover:border-border'
        )}
        onClick={onClick}
      >
        {/* Checkbox */}
        {onSelect && (
          <button
            onClick={(e) => {
              e.stopPropagation();
              onSelect();
            }}
            className={cn(
              'w-5 h-5 rounded border-2 flex items-center justify-center shrink-0 transition-colors',
              isSelected 
                ? 'border-amber-500 bg-amber-500' 
                : 'border-white/20 hover:border-white/40'
            )}
          >
            {isSelected && <CheckCircle className="w-3 h-3 text-black" />}
          </button>
        )}
        
        {/* Lot Number */}
        <span className="font-mono text-sm text-foreground w-32 truncate">
          {lot.lotNumber}
        </span>
        
        {/* Product */}
        <span className="text-sm text-foreground/80 flex-1 truncate">
          {lot.productName}
        </span>
        
        {/* Quantity */}
        <span className="text-sm text-muted-foreground tabular-nums w-24 text-right">
          {lot.quantity.toLocaleString()} {lot.uom}
        </span>
        
        {/* Status */}
        <span className={cn(
          'text-xs px-2 py-0.5 rounded-full shrink-0',
          statusConfig.bgColor,
          statusConfig.color
        )}>
          {statusConfig.label}
        </span>
        
        {/* Sync */}
        {SyncIcon && (
          <SyncIcon className={cn(
            'w-4 h-4 shrink-0',
            syncConfig.color,
            syncConfig.animate && 'animate-spin'
          )} />
        )}
        
        {/* Arrow */}
        <ChevronRight className="w-4 h-4 text-muted-foreground group-hover:text-foreground transition-colors shrink-0" />
      </div>
    );
  }
  
  return (
    <div
      className={cn(
        'p-4 rounded-xl border transition-all cursor-pointer group',
        'bg-gradient-to-br from-white/[0.03] to-transparent',
        isSelected 
          ? 'border-amber-500/50 bg-amber-500/5 shadow-lg shadow-amber-500/5' 
          : 'border-border hover:border-border'
      )}
      onClick={onClick}
    >
      {/* Header */}
      <div className="flex items-start justify-between mb-3">
        <div className="flex items-center gap-3">
          {onSelect && (
            <button
              onClick={(e) => {
                e.stopPropagation();
                onSelect();
              }}
              className={cn(
                'w-5 h-5 rounded border-2 flex items-center justify-center shrink-0 transition-colors',
                isSelected 
                  ? 'border-amber-500 bg-amber-500' 
                  : 'border-white/20 hover:border-white/40'
              )}
            >
              {isSelected && <CheckCircle className="w-3 h-3 text-black" />}
            </button>
          )}
          
          <div>
            <div className="flex items-center gap-2">
              <span className="font-mono text-sm font-medium text-foreground">
                {lot.lotNumber}
              </span>
              <span className={cn(
                'text-[10px] px-1.5 py-0.5 rounded-full uppercase',
                statusConfig.bgColor,
                statusConfig.color
              )}>
                {statusConfig.label}
              </span>
            </div>
            <p className="text-sm text-muted-foreground mt-0.5">
              {lot.productName}
            </p>
          </div>
        </div>
        
        <button
          onClick={(e) => {
            e.stopPropagation();
            // Toggle dropdown menu
          }}
          className="p-1 rounded hover:bg-white/5 text-muted-foreground hover:text-foreground transition-colors"
        >
          <MoreVertical className="w-4 h-4" />
        </button>
      </div>
      
      {/* Details Grid */}
      <div className="grid grid-cols-2 gap-3 mb-3">
        <div className="flex items-center gap-2">
          <Package className="w-4 h-4 text-muted-foreground" />
          <span className="text-sm text-foreground tabular-nums">
            {lot.quantity.toLocaleString()} {lot.uom}
          </span>
        </div>
        
        <div className="flex items-center gap-2">
          <MapPin className="w-4 h-4 text-muted-foreground" />
          <span className="text-sm text-muted-foreground truncate">
            {lot.locationPath}
          </span>
        </div>
        
        {lot.strainName && (
          <div className="col-span-2 flex items-center gap-2">
            <span className="text-xs text-muted-foreground">Strain:</span>
            <span className="text-sm text-foreground">{lot.strainName}</span>
          </div>
        )}
      </div>
      
      {/* Footer */}
      <div className="flex items-center justify-between pt-3 border-t border-border">
        <div className="flex items-center gap-3">
          {/* QA Status */}
          <div className="flex items-center gap-1">
            <QAIcon className={cn('w-3.5 h-3.5', qaConfig.color)} />
            <span className={cn('text-xs', qaConfig.color)}>
              {lot.qaStatus}
            </span>
          </div>
          
          {/* Sync Status */}
          {SyncIcon && (
            <div className="flex items-center gap-1">
              <SyncIcon className={cn(
                'w-3.5 h-3.5',
                syncConfig.color,
                syncConfig.animate && 'animate-spin'
              )} />
              <span className={cn('text-xs', syncConfig.color)}>
                {syncConfig.label}
              </span>
            </div>
          )}
          
          {/* Compliance Tags */}
          <div className="flex items-center gap-1">
            {lot.metrcTag && (
              <span className="text-[10px] px-1.5 py-0.5 rounded bg-emerald-500/10 text-emerald-400">
                METRC
              </span>
            )}
            {lot.biotrackId && (
              <span className="text-[10px] px-1.5 py-0.5 rounded bg-cyan-500/10 text-cyan-400">
                BioTrack
              </span>
            )}
          </div>
        </div>
        
        {/* Expiration Warning */}
        {expirationInfo && (
          <span className={cn(
            'text-xs px-2 py-0.5 rounded-full',
            expirationInfo.isUrgent 
              ? 'bg-rose-500/10 text-rose-400' 
              : 'bg-amber-500/10 text-amber-400'
          )}>
            {expirationInfo.label}
          </span>
        )}
      </div>
    </div>
  );
}

export default LotCard;

