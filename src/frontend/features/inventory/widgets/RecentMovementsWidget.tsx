'use client';

import React from 'react';
import { ArrowRight, ArrowUpRight, ArrowDownRight, RotateCcw, Scissors, Layers, Package } from 'lucide-react';
import { cn } from '@/lib/utils';
import type { InventoryMovement, MovementType } from '../types';

interface RecentMovementsWidgetProps {
  movements: InventoryMovement[];
  onViewMovement?: (movementId: string) => void;
  loading?: boolean;
  className?: string;
}

const MOVEMENT_CONFIG: Record<MovementType, { 
  icon: React.ElementType; 
  label: string; 
  color: string 
}> = {
  transfer: { icon: ArrowRight, label: 'Transfer', color: 'text-cyan-400 bg-cyan-500/10' },
  receive: { icon: ArrowDownRight, label: 'Receive', color: 'text-emerald-400 bg-emerald-500/10' },
  ship: { icon: ArrowUpRight, label: 'Ship', color: 'text-violet-400 bg-violet-500/10' },
  return: { icon: RotateCcw, label: 'Return', color: 'text-amber-400 bg-amber-500/10' },
  adjustment: { icon: Package, label: 'Adjustment', color: 'text-muted-foreground bg-muted/50' },
  split: { icon: Scissors, label: 'Split', color: 'text-blue-400 bg-blue-500/10' },
  merge: { icon: Layers, label: 'Merge', color: 'text-indigo-400 bg-indigo-500/10' },
  process_input: { icon: ArrowDownRight, label: 'Process In', color: 'text-amber-400 bg-amber-500/10' },
  process_output: { icon: ArrowUpRight, label: 'Process Out', color: 'text-emerald-400 bg-emerald-500/10' },
  destruction: { icon: Package, label: 'Destruction', color: 'text-rose-400 bg-rose-500/10' },
  cycle_count: { icon: Package, label: 'Cycle Count', color: 'text-muted-foreground bg-muted/50' },
};

interface MovementItemProps {
  movement: InventoryMovement;
  onClick?: () => void;
}

function MovementItem({ movement, onClick }: MovementItemProps) {
  const config = MOVEMENT_CONFIG[movement.movementType] ?? MOVEMENT_CONFIG.transfer;
  const Icon = config.icon;
  
  const formatTime = (dateString: string) => {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / (1000 * 60));
    
    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m`;
    if (diffMins < 1440) return `${Math.floor(diffMins / 60)}h`;
    return date.toLocaleDateString(undefined, { month: 'short', day: 'numeric' });
  };

  const formatQuantity = (qty: number, uom: string) => {
    if (qty >= 1000) {
      return `${(qty / 1000).toFixed(1)}k ${uom}`;
    }
    return `${qty} ${uom}`;
  };

  const getSyncIndicator = () => {
    switch (movement.syncStatus) {
      case 'synced':
        return <span className="w-1.5 h-1.5 rounded-full bg-emerald-400" />;
      case 'pending':
        return <span className="w-1.5 h-1.5 rounded-full bg-amber-400 animate-pulse" />;
      case 'error':
        return <span className="w-1.5 h-1.5 rounded-full bg-rose-400" />;
      default:
        return null;
    }
  };

  return (
    <button
      onClick={onClick}
      className="group w-full flex items-center gap-3 p-3 rounded-lg bg-muted/10 hover:bg-muted/30 border border-transparent hover:border-border transition-all text-left"
    >
      {/* Icon */}
      <div className={cn(
        'w-8 h-8 rounded-lg flex items-center justify-center shrink-0 transition-transform group-hover:scale-105',
        config.color.split(' ')[1]
      )}>
        <Icon className={cn('w-4 h-4', config.color.split(' ')[0])} />
      </div>

      {/* Content */}
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2">
          <span className="text-sm font-mono text-foreground truncate">
            {movement.lotNumber}
          </span>
          {getSyncIndicator()}
        </div>
        <div className="flex items-center gap-1.5 mt-0.5">
          <span className={cn('text-xs', config.color.split(' ')[0])}>
            {config.label}
          </span>
          {movement.fromLocationPath && movement.toLocationPath && (
            <>
              <span className="text-[10px] text-muted-foreground">•</span>
              <span className="text-[10px] text-muted-foreground truncate max-w-[100px]">
                {movement.fromLocationPath.split('>').pop()?.trim()} → {movement.toLocationPath.split('>').pop()?.trim()}
              </span>
            </>
          )}
        </div>
      </div>

      {/* Quantity & Time */}
      <div className="text-right shrink-0">
        <div className="text-sm font-medium text-foreground tabular-nums">
          {formatQuantity(movement.quantity, movement.uom)}
        </div>
        <div className="text-[10px] text-muted-foreground">
          {formatTime(movement.createdAt)}
        </div>
      </div>
    </button>
  );
}

export function RecentMovementsWidget({
  movements,
  onViewMovement,
  loading,
  className,
}: RecentMovementsWidgetProps) {
  if (loading) {
    return (
      <div className={cn('space-y-4', className)}>
        <div className="flex items-center justify-between">
          <h3 className="text-sm font-semibold text-foreground">Recent Movements</h3>
        </div>
        <div className="space-y-1">
          {[1, 2, 3, 4, 5].map((i) => (
            <div key={i} className="flex items-center gap-3 p-3 animate-pulse">
              <div className="w-8 h-8 rounded-lg bg-muted" />
              <div className="flex-1">
                <div className="h-4 w-24 bg-muted rounded mb-1" />
                <div className="h-3 w-16 bg-muted rounded" />
              </div>
              <div className="h-4 w-12 bg-muted rounded" />
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
          <h3 className="text-sm font-semibold text-foreground">Recent Movements</h3>
          {movements.length > 0 && (
            <span className="px-1.5 py-0.5 rounded text-[10px] font-medium bg-muted text-muted-foreground">
              LIVE
            </span>
          )}
        </div>
        <button className="text-xs text-cyan-400 hover:underline">
          View All
        </button>
      </div>

      {/* Movements List */}
      {movements.length === 0 ? (
        <div className="text-center py-8">
          <div className="w-12 h-12 rounded-full bg-muted flex items-center justify-center mx-auto mb-3">
            <Package className="w-6 h-6 text-muted-foreground" />
          </div>
          <p className="text-sm text-muted-foreground">No recent movements</p>
          <p className="text-xs text-muted-foreground mt-1">Movements will appear here in real-time</p>
        </div>
      ) : (
        <div className="space-y-1 max-h-[400px] overflow-y-auto scrollbar-thin">
          {movements.map((movement) => (
            <MovementItem
              key={movement.id}
              movement={movement}
              onClick={() => onViewMovement?.(movement.id)}
            />
          ))}
        </div>
      )}

      {/* Live indicator */}
      {movements.length > 0 && (
        <div className="flex items-center justify-center gap-2 pt-2 border-t border-border">
          <span className="relative flex h-2 w-2">
            <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-cyan-400 opacity-75" />
            <span className="relative inline-flex rounded-full h-2 w-2 bg-cyan-400" />
          </span>
          <span className="text-[10px] text-muted-foreground">Real-time updates active</span>
        </div>
      )}
    </div>
  );
}

export default RecentMovementsWidget;
