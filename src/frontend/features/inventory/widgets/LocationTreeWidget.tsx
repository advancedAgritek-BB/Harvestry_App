'use client';

import React, { useState } from 'react';
import { ChevronRight, ChevronDown, MapPin, Box, Warehouse, Layers, Grid3X3 } from 'lucide-react';
import { cn } from '@/lib/utils';
import type { LocationTreeNode, LocationType } from '../types';

interface LocationTreeWidgetProps {
  tree: LocationTreeNode[];
  selectedId?: string | null;
  expandedIds?: Set<string>;
  onSelect?: (locationId: string) => void;
  onToggleExpand?: (locationId: string) => void;
  loading?: boolean;
  className?: string;
}

const LOCATION_ICONS: Record<LocationType, React.ElementType> = {
  room: Warehouse,
  zone: Grid3X3,
  sub_zone: Layers,
  row: Layers,
  position: MapPin,
  rack: Box,
  shelf: Layers,
  bin: Box,
  vault: Warehouse,
};

const LOCATION_COLORS: Record<LocationType, string> = {
  room: 'text-violet-400',
  zone: 'text-cyan-400',
  sub_zone: 'text-blue-400',
  row: 'text-indigo-400',
  position: 'text-emerald-400',
  rack: 'text-amber-400',
  shelf: 'text-orange-400',
  bin: 'text-rose-400',
  vault: 'text-violet-400',
};

interface TreeNodeProps {
  node: LocationTreeNode;
  depth: number;
  isExpanded: boolean;
  isSelected: boolean;
  onSelect: () => void;
  onToggleExpand: () => void;
  expandedIds: Set<string>;
  selectedId: string | null;
  onSelectNode: (id: string) => void;
  onToggleNode: (id: string) => void;
}

function TreeNode({
  node,
  depth,
  isExpanded,
  isSelected,
  onSelect,
  onToggleExpand,
  expandedIds,
  selectedId,
  onSelectNode,
  onToggleNode,
}: TreeNodeProps) {
  const Icon = LOCATION_ICONS[node.locationType] ?? Box;
  const colorClass = LOCATION_COLORS[node.locationType] ?? 'text-muted-foreground';
  const hasChildren = node.children && node.children.length > 0;

  const getCapacityColor = (percent?: number) => {
    if (percent === undefined) return 'bg-white/10';
    if (percent >= 90) return 'bg-rose-500';
    if (percent >= 70) return 'bg-amber-500';
    return 'bg-emerald-500';
  };

  const getStatusColor = () => {
    switch (node.status) {
      case 'full': return 'text-rose-400';
      case 'quarantine': return 'text-amber-400';
      case 'reserved': return 'text-violet-400';
      case 'inactive': return 'text-muted-foreground';
      default: return '';
    }
  };

  return (
    <div>
      <div
        className={cn(
          'group flex items-center gap-2 py-1.5 px-2 rounded-lg cursor-pointer transition-all',
          isSelected 
            ? 'bg-cyan-500/10 border border-cyan-500/30' 
            : 'hover:bg-muted/40 border border-transparent'
        )}
        style={{ paddingLeft: `${depth * 16 + 8}px` }}
        onClick={onSelect}
      >
        {/* Expand/Collapse Button */}
        <button
          onClick={(e) => {
            e.stopPropagation();
            onToggleExpand();
          }}
          className={cn(
            'w-5 h-5 flex items-center justify-center rounded transition-colors',
            hasChildren ? 'hover:bg-white/10' : 'invisible'
          )}
        >
          {hasChildren && (
            isExpanded 
              ? <ChevronDown className="w-3.5 h-3.5 text-muted-foreground" />
              : <ChevronRight className="w-3.5 h-3.5 text-muted-foreground" />
          )}
        </button>

        {/* Icon */}
        <Icon className={cn('w-4 h-4 shrink-0', colorClass)} />

        {/* Name */}
        <span className={cn(
          'flex-1 text-sm truncate',
          isSelected ? 'text-foreground font-medium' : 'text-muted-foreground group-hover:text-foreground'
        )}>
          {node.name}
        </span>

        {/* Status Badge */}
        {node.status !== 'active' && (
          <span className={cn('text-[10px] uppercase font-medium', getStatusColor())}>
            {node.status}
          </span>
        )}

        {/* Lot Count */}
        {node.lotCount > 0 && (
          <span className="text-xs text-muted-foreground tabular-nums">
            {node.lotCount}
          </span>
        )}

        {/* Capacity Bar */}
        {node.capacityPercent !== undefined && (
          <div className="w-12 h-1.5 bg-white/5 rounded-full overflow-hidden">
            <div
              className={cn('h-full rounded-full transition-all', getCapacityColor(node.capacityPercent))}
              style={{ width: `${node.capacityPercent}%` }}
            />
          </div>
        )}
      </div>

      {/* Children */}
      {hasChildren && isExpanded && (
        <div>
          {node.children.map((child) => (
            <TreeNode
              key={child.id}
              node={child}
              depth={depth + 1}
              isExpanded={expandedIds.has(child.id)}
              isSelected={selectedId === child.id}
              onSelect={() => onSelectNode(child.id)}
              onToggleExpand={() => onToggleNode(child.id)}
              expandedIds={expandedIds}
              selectedId={selectedId}
              onSelectNode={onSelectNode}
              onToggleNode={onToggleNode}
            />
          ))}
        </div>
      )}
    </div>
  );
}

export function LocationTreeWidget({
  tree,
  selectedId = null,
  expandedIds: externalExpandedIds,
  onSelect,
  onToggleExpand,
  loading,
  className,
}: LocationTreeWidgetProps) {
  // Internal state for expanded nodes if not controlled externally
  const [internalExpandedIds, setInternalExpandedIds] = useState<Set<string>>(new Set());
  const expandedIds = externalExpandedIds ?? internalExpandedIds;

  const handleToggleExpand = (locationId: string) => {
    if (onToggleExpand) {
      onToggleExpand(locationId);
    } else {
      setInternalExpandedIds((prev) => {
        const next = new Set(prev);
        if (next.has(locationId)) {
          next.delete(locationId);
        } else {
          next.add(locationId);
        }
        return next;
      });
    }
  };

  if (loading) {
    return (
      <div className={cn('space-y-4', className)}>
        <div className="flex items-center justify-between">
          <h3 className="text-sm font-semibold text-foreground">Locations</h3>
        </div>
        <div className="space-y-1">
          {[1, 2, 3, 4, 5].map((i) => (
            <div key={i} className="flex items-center gap-2 p-2 animate-pulse" style={{ paddingLeft: `${(i % 3) * 16 + 8}px` }}>
              <div className="w-5 h-5 rounded bg-white/5" />
              <div className="w-4 h-4 rounded bg-white/5" />
              <div className="h-4 flex-1 bg-white/5 rounded" />
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
        <h3 className="text-sm font-semibold text-foreground">Locations</h3>
        <button className="text-xs text-cyan-400 hover:underline">
          Manage
        </button>
      </div>

      {/* Search (optional) */}
      <div className="relative">
        <input
          type="text"
          placeholder="Search locations..."
          className="w-full px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-cyan-500/30"
        />
      </div>

      {/* Tree */}
      {tree.length === 0 ? (
        <div className="text-center py-8">
          <div className="w-12 h-12 rounded-full bg-white/5 flex items-center justify-center mx-auto mb-3">
            <Warehouse className="w-6 h-6 text-muted-foreground" />
          </div>
          <p className="text-sm text-muted-foreground">No locations configured</p>
          <button className="mt-2 text-xs text-cyan-400 hover:underline">
            Add Location →
          </button>
        </div>
      ) : (
        <div className="max-h-[400px] overflow-y-auto scrollbar-thin scrollbar-thumb-white/10 space-y-0.5">
          {tree.map((node) => (
            <TreeNode
              key={node.id}
              node={node}
              depth={0}
              isExpanded={expandedIds.has(node.id)}
              isSelected={selectedId === node.id}
              onSelect={() => onSelect?.(node.id)}
              onToggleExpand={() => handleToggleExpand(node.id)}
              expandedIds={expandedIds}
              selectedId={selectedId}
              onSelectNode={(id) => onSelect?.(id)}
              onToggleNode={handleToggleExpand}
            />
          ))}
        </div>
      )}

      {/* Legend */}
      <div className="flex flex-wrap gap-3 pt-3 border-t border-border">
        <div className="flex items-center gap-1.5">
          <div className="w-2 h-2 rounded-full bg-emerald-500" />
          <span className="text-[10px] text-muted-foreground">&lt;70%</span>
        </div>
        <div className="flex items-center gap-1.5">
          <div className="w-2 h-2 rounded-full bg-amber-500" />
          <span className="text-[10px] text-muted-foreground">70-90%</span>
        </div>
        <div className="flex items-center gap-1.5">
          <div className="w-2 h-2 rounded-full bg-rose-500" />
          <span className="text-[10px] text-muted-foreground">&gt;90%</span>
        </div>
      </div>
    </div>
  );
}

export default LocationTreeWidget;
