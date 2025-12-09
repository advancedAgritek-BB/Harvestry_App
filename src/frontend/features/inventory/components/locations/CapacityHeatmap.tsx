'use client';

import React from 'react';
import { cn } from '@/lib/utils';
import type { InventoryLocation, LocationType } from '../../types';

interface CapacityHeatmapProps {
  locations: InventoryLocation[];
  onSelectLocation?: (locationId: string) => void;
  selectedLocationId?: string;
  viewMode?: 'grid' | 'list';
}

function getUtilizationColor(percent: number): string {
  if (percent >= 95) return 'bg-rose-500';
  if (percent >= 85) return 'bg-rose-400';
  if (percent >= 75) return 'bg-amber-500';
  if (percent >= 65) return 'bg-amber-400';
  if (percent >= 50) return 'bg-cyan-500';
  if (percent >= 25) return 'bg-cyan-400';
  if (percent > 0) return 'bg-emerald-500';
  return 'bg-muted';
}

function getUtilizationTextColor(percent: number): string {
  if (percent >= 95) return 'text-rose-400';
  if (percent >= 75) return 'text-amber-400';
  if (percent >= 50) return 'text-cyan-400';
  return 'text-emerald-400';
}

interface HeatmapCellProps {
  location: InventoryLocation;
  onClick?: () => void;
  isSelected?: boolean;
  size?: 'sm' | 'md' | 'lg';
}

function HeatmapCell({ location, onClick, isSelected, size = 'md' }: HeatmapCellProps) {
  const sizeClasses = {
    sm: 'w-8 h-8',
    md: 'w-12 h-12',
    lg: 'w-16 h-16',
  };
  
  return (
    <button
      onClick={onClick}
      className={cn(
        'rounded-lg transition-all relative group',
        sizeClasses[size],
        getUtilizationColor(location.utilizationPercent ?? 0),
        isSelected 
          ? 'ring-2 ring-amber-400 ring-offset-2 ring-offset-background' 
          : 'hover:ring-2 hover:ring-foreground/30 hover:ring-offset-1 hover:ring-offset-background',
        location.status === 'inactive' && 'opacity-30',
        location.status === 'quarantine' && 'ring-2 ring-rose-500'
      )}
      title={`${location.name} (${(location.utilizationPercent ?? 0).toFixed(0)}%)`}
    >
      {/* Tooltip on hover */}
      <div className={cn(
        'absolute bottom-full left-1/2 -translate-x-1/2 mb-2 z-10',
        'px-2 py-1 rounded bg-surface border border-border shadow-xl',
        'opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none',
        'whitespace-nowrap'
      )}>
        <div className="text-xs font-medium text-foreground">{location.name}</div>
        <div className="text-[10px] text-muted-foreground">
          {(location.utilizationPercent ?? 0).toFixed(0)}% • {location.lotCount ?? 0} lots
        </div>
      </div>
    </button>
  );
}

interface LegendProps {
  className?: string;
}

function Legend({ className }: LegendProps) {
  const levels = [
    { label: '0%', color: 'bg-muted' },
    { label: '25%', color: 'bg-emerald-500' },
    { label: '50%', color: 'bg-cyan-500' },
    { label: '75%', color: 'bg-amber-500' },
    { label: '95%+', color: 'bg-rose-500' },
  ];
  
  return (
    <div className={cn('flex items-center gap-4', className)}>
      <span className="text-xs text-muted-foreground">Utilization:</span>
      <div className="flex items-center gap-1">
        {levels.map((level) => (
          <div key={level.label} className="flex items-center gap-1">
            <div className={cn('w-4 h-4 rounded', level.color)} />
            <span className="text-[10px] text-muted-foreground">{level.label}</span>
          </div>
        ))}
      </div>
    </div>
  );
}

export function CapacityHeatmap({
  locations,
  onSelectLocation,
  selectedLocationId,
  viewMode = 'grid',
}: CapacityHeatmapProps) {
  // Group locations by room/parent
  const groupedLocations = locations.reduce<Record<string, InventoryLocation[]>>(
    (acc, location) => {
      const roomId = location.roomId || 'ungrouped';
      if (!acc[roomId]) {
        acc[roomId] = [];
      }
      acc[roomId].push(location);
      return acc;
    },
    {}
  );
  
  // Get room names for headers
  const roomNames = Object.keys(groupedLocations).reduce<Record<string, string>>(
    (acc, roomId) => {
      const roomLocation = locations.find(l => l.id === roomId);
      acc[roomId] = roomLocation?.name || 'Ungrouped';
      return acc;
    },
    {}
  );
  
  // Calculate stats
  const totalLocations = locations.length;
  const avgUtilization = locations.length > 0
    ? locations.reduce((sum, l) => sum + (l.utilizationPercent ?? 0), 0) / locations.length
    : 0;
  const fullLocations = locations.filter(l => (l.utilizationPercent ?? 0) >= 95).length;
  
  if (locations.length === 0) {
    return (
      <div className="text-center py-12">
        <div className="w-16 h-16 rounded-2xl bg-white/5 flex items-center justify-center mx-auto mb-4">
          <div className="grid grid-cols-3 gap-1">
            {[...Array(9)].map((_, i) => (
              <div key={i} className="w-3 h-3 rounded-sm bg-white/10" />
            ))}
          </div>
        </div>
        <h3 className="text-lg font-medium text-foreground mb-2">No Locations</h3>
        <p className="text-sm text-muted-foreground">
          Add locations to see the capacity heatmap
        </p>
      </div>
    );
  }
  
  return (
    <div className="space-y-6">
      {/* Stats Bar */}
      <div className="flex items-center justify-between p-4 rounded-xl bg-muted/30 border border-border">
        <div className="flex items-center gap-8">
          <div>
            <div className="text-2xl font-bold text-foreground">{totalLocations}</div>
            <div className="text-xs text-muted-foreground">Total Locations</div>
          </div>
          <div>
            <div className={cn(
              'text-2xl font-bold',
              getUtilizationTextColor(avgUtilization)
            )}>
              {avgUtilization.toFixed(0)}%
            </div>
            <div className="text-xs text-muted-foreground">Avg Utilization</div>
          </div>
          <div>
            <div className={cn(
              'text-2xl font-bold',
              fullLocations > 0 ? 'text-rose-400' : 'text-emerald-400'
            )}>
              {fullLocations}
            </div>
            <div className="text-xs text-muted-foreground">Full/Near Full</div>
          </div>
        </div>
        
        <Legend />
      </div>
      
      {/* Heatmap Grid */}
      {Object.entries(groupedLocations).map(([roomId, roomLocations]) => (
        <div key={roomId} className="space-y-3">
          <h4 className="text-sm font-medium text-foreground">{roomNames[roomId]}</h4>
          
          <div className={cn(
            viewMode === 'grid' 
              ? 'flex flex-wrap gap-2' 
              : 'grid grid-cols-8 md:grid-cols-12 lg:grid-cols-16 gap-1'
          )}>
            {roomLocations
              .sort((a, b) => (a.code || '').localeCompare(b.code || ''))
              .map((location) => (
                <HeatmapCell
                  key={location.id}
                  location={location}
                  onClick={() => onSelectLocation?.(location.id)}
                  isSelected={selectedLocationId === location.id}
                  size={viewMode === 'grid' ? 'md' : 'sm'}
                />
              ))}
          </div>
        </div>
      ))}
    </div>
  );
}

export default CapacityHeatmap;

