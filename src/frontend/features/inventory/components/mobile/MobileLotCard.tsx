'use client';

import React from 'react';
import { 
  Package, 
  MapPin, 
  CheckCircle, 
  AlertTriangle, 
  Clock, 
  XCircle,
  ChevronRight,
  MoreHorizontal,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import type { InventoryLot, LotStatus } from '../../types';

interface MobileLotCardProps {
  lot: InventoryLot;
  onPress?: () => void;
  onLongPress?: () => void;
  selected?: boolean;
  showActions?: boolean;
}

const STATUS_CONFIG: Record<LotStatus, { icon: React.ElementType; color: string; label: string }> = {
  available: { icon: CheckCircle, color: 'text-emerald-400', label: 'Available' },
  on_hold: { icon: AlertTriangle, color: 'text-amber-400', label: 'On Hold' },
  quarantine: { icon: XCircle, color: 'text-rose-400', label: 'Quarantine' },
  pending_coa: { icon: Clock, color: 'text-cyan-400', label: 'Pending COA' },
  coa_failed: { icon: XCircle, color: 'text-rose-400', label: 'COA Failed' },
  reserved: { icon: Package, color: 'text-violet-400', label: 'Reserved' },
  in_transit: { icon: Package, color: 'text-blue-400', label: 'In Transit' },
  destroyed: { icon: XCircle, color: 'text-muted-foreground', label: 'Destroyed' },
};

export function MobileLotCard({ 
  lot, 
  onPress, 
  onLongPress, 
  selected,
  showActions = true,
}: MobileLotCardProps) {
  const status = STATUS_CONFIG[lot.status];
  const StatusIcon = status.icon;

  const handleTouchStart = () => {
    // Set up long press timer
  };

  const handleTouchEnd = () => {
    // Clear long press timer
  };

  return (
    <div
      onClick={onPress}
      onTouchStart={handleTouchStart}
      onTouchEnd={handleTouchEnd}
      className={cn(
        'p-4 rounded-2xl transition-all active:scale-[0.98]',
        selected 
          ? 'bg-amber-500/10 border-2 border-amber-500/30' 
          : 'bg-muted/30 border border-border',
        onPress && 'cursor-pointer'
      )}
    >
      {/* Header */}
      <div className="flex items-start justify-between mb-3">
        <div className="flex items-center gap-3">
          <div className="w-12 h-12 rounded-xl bg-cyan-500/10 flex items-center justify-center">
            <Package className="w-6 h-6 text-cyan-400" />
          </div>
          <div>
            <div className="flex items-center gap-2">
              <span className="text-base font-mono text-foreground">{lot.lotNumber}</span>
              <div className={cn(
                'w-2 h-2 rounded-full',
                lot.syncStatus === 'synced' && 'bg-emerald-400',
                lot.syncStatus === 'pending' && 'bg-amber-400 animate-pulse',
                lot.syncStatus === 'error' && 'bg-rose-400'
              )} />
            </div>
            <div className="text-sm text-muted-foreground">{lot.strainName}</div>
          </div>
        </div>

        {showActions && (
          <button className="p-1.5 rounded-lg hover:bg-white/5 text-muted-foreground">
            <MoreHorizontal className="w-5 h-5" />
          </button>
        )}
      </div>

      {/* Stats Row */}
      <div className="flex items-center gap-4 mb-3">
        <div className="flex-1">
          <div className="text-xs text-muted-foreground mb-0.5">Quantity</div>
          <div className="text-lg font-semibold text-foreground tabular-nums">
            {lot.quantity.toLocaleString()} <span className="text-sm font-normal text-muted-foreground">{lot.uom}</span>
          </div>
        </div>
        <div className="flex-1">
          <div className="text-xs text-muted-foreground mb-0.5">THC</div>
          <div className="text-lg font-semibold text-foreground tabular-nums">
            {lot.thcPercent?.toFixed(1)}%
          </div>
        </div>
        <div className="flex-1">
          <div className="text-xs text-muted-foreground mb-0.5">Status</div>
          <div className={cn('flex items-center gap-1', status.color)}>
            <StatusIcon className="w-4 h-4" />
            <span className="text-sm font-medium">{status.label}</span>
          </div>
        </div>
      </div>

      {/* Location */}
      <div className="flex items-center justify-between pt-3 border-t border-border">
        <div className="flex items-center gap-2 text-sm text-muted-foreground">
          <MapPin className="w-4 h-4" />
          <span className="truncate max-w-[200px]">{lot.locationPath}</span>
        </div>
        {onPress && (
          <ChevronRight className="w-5 h-5 text-muted-foreground" />
        )}
      </div>
    </div>
  );
}

export default MobileLotCard;

