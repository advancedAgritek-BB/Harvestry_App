'use client';

import React from 'react';
import { 
  Building2,
  Layers,
  Grid3X3,
  Package,
  Archive,
  Lock,
  ChevronRight,
  MoreVertical,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import type { InventoryLocation, LocationType, LocationStatus } from '../../types';

interface LocationCardProps {
  location: InventoryLocation;
  onClick?: () => void;
  onAction?: (action: 'edit' | 'view' | 'delete') => void;
  showCapacity?: boolean;
  showLotCount?: boolean;
}

function getLocationTypeConfig(type: LocationType) {
  switch (type) {
    case 'room':
      return { icon: Building2, color: 'text-violet-400', bgColor: 'bg-violet-500/10' };
    case 'zone':
    case 'sub_zone':
      return { icon: Layers, color: 'text-cyan-400', bgColor: 'bg-cyan-500/10' };
    case 'row':
    case 'position':
      return { icon: Grid3X3, color: 'text-emerald-400', bgColor: 'bg-emerald-500/10' };
    case 'rack':
    case 'shelf':
      return { icon: Archive, color: 'text-amber-400', bgColor: 'bg-amber-500/10' };
    case 'bin':
      return { icon: Package, color: 'text-amber-400', bgColor: 'bg-amber-500/10' };
    case 'vault':
      return { icon: Lock, color: 'text-rose-400', bgColor: 'bg-rose-500/10' };
    default:
      return { icon: Package, color: 'text-muted-foreground', bgColor: 'bg-white/5' };
  }
}

function getStatusConfig(status: LocationStatus) {
  switch (status) {
    case 'active':
      return { color: 'text-emerald-400', label: 'Active' };
    case 'full':
      return { color: 'text-rose-400', label: 'Full' };
    case 'reserved':
      return { color: 'text-amber-400', label: 'Reserved' };
    case 'quarantine':
      return { color: 'text-rose-400', label: 'Quarantine' };
    case 'inactive':
      return { color: 'text-muted-foreground', label: 'Inactive' };
    default:
      return { color: 'text-muted-foreground', label: status };
  }
}

function getUtilizationColor(percent: number): string {
  if (percent >= 90) return 'bg-rose-500';
  if (percent >= 70) return 'bg-amber-500';
  if (percent >= 50) return 'bg-cyan-500';
  return 'bg-emerald-500';
}

export function LocationCard({
  location,
  onClick,
  onAction,
  showCapacity = true,
  showLotCount = true,
}: LocationCardProps) {
  const typeConfig = getLocationTypeConfig(location.locationType);
  const statusConfig = getStatusConfig(location.status);
  const TypeIcon = typeConfig.icon;
  
  return (
    <div
      className={cn(
        'p-4 rounded-xl border cursor-pointer group transition-all',
        'bg-gradient-to-br from-white/[0.03] to-transparent',
        'border-border hover:border-border'
      )}
      onClick={onClick}
    >
      {/* Header */}
      <div className="flex items-start justify-between mb-3">
        <div className="flex items-center gap-3">
          <div className={cn(
            'w-10 h-10 rounded-lg flex items-center justify-center',
            typeConfig.bgColor
          )}>
            <TypeIcon className={cn('w-5 h-5', typeConfig.color)} />
          </div>
          
          <div>
            <div className="flex items-center gap-2">
              <span className="font-medium text-foreground">{location.name}</span>
              <span className="text-xs text-muted-foreground font-mono">{location.code}</span>
            </div>
            <div className="flex items-center gap-2 text-xs">
              <span className="text-muted-foreground capitalize">{location.locationType.replace('_', ' ')}</span>
              {location.status !== 'active' && (
                <>
                  <span className="text-muted-foreground">•</span>
                  <span className={statusConfig.color}>{statusConfig.label}</span>
                </>
              )}
            </div>
          </div>
        </div>
        
        <button
          onClick={(e) => {
            e.stopPropagation();
            // Show action menu
          }}
          className="p-1 rounded hover:bg-white/5 text-muted-foreground hover:text-foreground transition-colors opacity-0 group-hover:opacity-100"
        >
          <MoreVertical className="w-4 h-4" />
        </button>
      </div>
      
      {/* Path */}
      <div className="text-xs text-muted-foreground mb-3 truncate">
        {location.path}
      </div>
      
      {/* Stats */}
      <div className="flex items-center gap-4">
        {showLotCount && (
          <div className="flex items-center gap-1.5">
            <Package className="w-3.5 h-3.5 text-muted-foreground" />
            <span className="text-sm text-foreground tabular-nums">{location.lotCount}</span>
            <span className="text-xs text-muted-foreground">lots</span>
          </div>
        )}
        
        {showCapacity && (location.utilizationPercent ?? 0) > 0 && (
          <div className="flex-1">
            <div className="flex items-center justify-between mb-1">
              <span className="text-xs text-muted-foreground">Utilization</span>
              <span className="text-xs text-foreground tabular-nums">{(location.utilizationPercent ?? 0).toFixed(0)}%</span>
            </div>
            <div className="h-1.5 rounded-full bg-white/5 overflow-hidden">
              <div 
                className={cn(
                  'h-full rounded-full transition-all',
                  getUtilizationColor(location.utilizationPercent ?? 0)
                )}
                style={{ width: `${Math.min(100, location.utilizationPercent ?? 0)}%` }}
              />
            </div>
          </div>
        )}
        
        <ChevronRight className="w-4 h-4 text-muted-foreground group-hover:text-foreground transition-colors shrink-0" />
      </div>
    </div>
  );
}

export default LocationCard;

