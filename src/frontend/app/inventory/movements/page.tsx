'use client';

import React, { useState, useMemo } from 'react';
import {
  ArrowUpDown,
  Plus,
  Filter,
  Download,
  ChevronLeft,
  ChevronRight,
  Search,
  ChevronDown,
  ArrowRight,
  ArrowDownRight,
  ArrowUpRight,
  RotateCcw,
  Scissors,
  Layers,
  Package,
  RefreshCw,
  Scan,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import type { InventoryMovement, MovementType } from '@/features/inventory/types';

const MOVEMENT_CONFIG: Record<MovementType, { 
  icon: React.ElementType; 
  label: string; 
  color: string;
  bgColor: string;
}> = {
  transfer: { icon: ArrowRight, label: 'Transfer', color: 'text-cyan-400', bgColor: 'bg-cyan-500/10' },
  receive: { icon: ArrowDownRight, label: 'Receive', color: 'text-emerald-400', bgColor: 'bg-emerald-500/10' },
  ship: { icon: ArrowUpRight, label: 'Ship', color: 'text-violet-400', bgColor: 'bg-violet-500/10' },
  return: { icon: RotateCcw, label: 'Return', color: 'text-amber-400', bgColor: 'bg-amber-500/10' },
  adjustment: { icon: Package, label: 'Adjustment', color: 'text-muted-foreground', bgColor: 'bg-muted/50' },
  split: { icon: Scissors, label: 'Split', color: 'text-blue-400', bgColor: 'bg-blue-500/10' },
  merge: { icon: Layers, label: 'Merge', color: 'text-indigo-400', bgColor: 'bg-indigo-500/10' },
  process_input: { icon: ArrowDownRight, label: 'Process In', color: 'text-amber-400', bgColor: 'bg-amber-500/10' },
  process_output: { icon: ArrowUpRight, label: 'Process Out', color: 'text-emerald-400', bgColor: 'bg-emerald-500/10' },
  destruction: { icon: Package, label: 'Destruction', color: 'text-rose-400', bgColor: 'bg-rose-500/10' },
  cycle_count: { icon: RefreshCw, label: 'Cycle Count', color: 'text-muted-foreground', bgColor: 'bg-muted/50' },
};

// Mock data
const MOCK_MOVEMENTS: InventoryMovement[] = Array.from({ length: 50 }, (_, i) => ({
  id: `mov-${i + 1}`,
  siteId: 'site-1',
  movementType: ['transfer', 'receive', 'adjustment', 'split', 'ship'][i % 5] as MovementType,
  status: 'completed' as const,
  lotId: `lot-${(i % 10) + 1}`,
  lotNumber: `LOT-2025-${String((i % 10) + 1).padStart(4, '0')}`,
  fromLocationId: i % 5 !== 1 ? `loc-${(i % 3) + 1}` : undefined,
  fromLocationPath: i % 5 !== 1 ? ['Vault A > Rack 1 > Shelf A', 'Warehouse B > Zone 1', 'Vault A > Rack 2'][i % 3] : undefined,
  toLocationId: `loc-${((i + 1) % 3) + 1}`,
  toLocationPath: ['Vault A > Rack 2 > Shelf B', 'Vault A > Rack 1 > Shelf A', 'Staging'][i % 3],
  quantity: Math.floor(Math.random() * 500) + 50,
  uom: 'g',
  syncStatus: ['synced', 'synced', 'pending', 'synced', 'error'][i % 5] as 'synced' | 'pending' | 'error',
  createdAt: new Date(Date.now() - i * 3600000 * 2).toISOString(),
  createdBy: 'user-1',
  completedAt: new Date(Date.now() - i * 3600000 * 2 + 300000).toISOString(),
}));

function MovementRow({ movement }: { movement: InventoryMovement }) {
  const config = MOVEMENT_CONFIG[movement.movementType];
  const Icon = config.icon;

  const formatDateTime = (dateString: string) => {
    return new Date(dateString).toLocaleString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  return (
    <tr className="group border-b border-border hover:bg-muted/30 transition-colors">
      {/* Type */}
      <td className="px-4 py-3">
        <div className="flex items-center gap-2">
          <div className={cn('w-7 h-7 rounded-lg flex items-center justify-center', config.bgColor)}>
            <Icon className={cn('w-3.5 h-3.5', config.color)} />
          </div>
          <span className={cn('text-sm font-medium', config.color)}>{config.label}</span>
        </div>
      </td>

      {/* Lot */}
      <td className="px-4 py-3">
        <span className="text-sm font-mono text-foreground">{movement.lotNumber}</span>
      </td>

      {/* From */}
      <td className="px-4 py-3">
        <span className="text-sm text-muted-foreground truncate max-w-[180px] block">
          {movement.fromLocationPath || '—'}
        </span>
      </td>

      {/* To */}
      <td className="px-4 py-3">
        <span className="text-sm text-muted-foreground truncate max-w-[180px] block">
          {movement.toLocationPath || '—'}
        </span>
      </td>

      {/* Quantity */}
      <td className="px-4 py-3">
        <span className="text-sm text-foreground tabular-nums">
          {movement.quantity.toLocaleString()} {movement.uom}
        </span>
      </td>

      {/* Sync Status */}
      <td className="px-4 py-3">
        <div className={cn(
          'w-2 h-2 rounded-full',
          movement.syncStatus === 'synced' && 'bg-emerald-400',
          movement.syncStatus === 'pending' && 'bg-amber-400 animate-pulse',
          movement.syncStatus === 'error' && 'bg-rose-400'
        )} />
      </td>

      {/* Date */}
      <td className="px-4 py-3">
        <span className="text-sm text-muted-foreground">
          {formatDateTime(movement.createdAt)}
        </span>
      </td>
    </tr>
  );
}

export default function MovementsPage() {
  const [searchQuery, setSearchQuery] = useState('');
  const [typeFilter, setTypeFilter] = useState<MovementType[]>([]);
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 20;

  const filteredMovements = useMemo(() => {
    return MOCK_MOVEMENTS.filter((mov) => {
      if (searchQuery) {
        const query = searchQuery.toLowerCase();
        if (
          !mov.lotNumber.toLowerCase().includes(query) &&
          !mov.fromLocationPath?.toLowerCase().includes(query) &&
          !mov.toLocationPath?.toLowerCase().includes(query)
        ) {
          return false;
        }
      }
      if (typeFilter.length > 0 && !typeFilter.includes(mov.movementType)) {
        return false;
      }
      return true;
    });
  }, [searchQuery, typeFilter]);

  const paginatedMovements = filteredMovements.slice(
    (currentPage - 1) * pageSize,
    currentPage * pageSize
  );
  const totalPages = Math.ceil(filteredMovements.length / pageSize);

  // Stats
  const todayCount = MOCK_MOVEMENTS.filter(
    (m) => new Date(m.createdAt).toDateString() === new Date().toDateString()
  ).length;

  return (
    <div className="min-h-screen bg-background">
      {/* Header */}
      <header className="sticky top-0 z-40 glass-header">
        <div className="px-6 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <a href="/inventory" className="p-2 rounded-lg hover:bg-white/5 text-muted-foreground hover:text-foreground transition-colors">
                <ChevronLeft className="w-5 h-5" />
              </a>
              <div>
                <h1 className="text-xl font-semibold text-foreground">Movements</h1>
                <p className="text-sm text-muted-foreground">
                  {filteredMovements.length.toLocaleString()} movements • {todayCount} today
                </p>
              </div>
            </div>

            {/* Actions */}
            <div className="flex items-center gap-3">
              <button className="flex items-center gap-2 px-3 py-2 rounded-lg bg-white/5 text-foreground hover:bg-white/10 transition-colors">
                <Scan className="w-4 h-4" />
                <span className="text-sm">Quick Move</span>
              </button>
              <button className="flex items-center gap-2 px-3 py-2 rounded-lg bg-white/5 text-foreground hover:bg-white/10 transition-colors">
                <Download className="w-4 h-4" />
                <span className="text-sm">Export</span>
              </button>
              <button className="flex items-center gap-2 px-4 py-2 rounded-lg bg-cyan-500/10 text-cyan-400 hover:bg-cyan-500/20 transition-colors">
                <Plus className="w-4 h-4" />
                <span className="text-sm font-medium">New Movement</span>
              </button>
            </div>
          </div>
        </div>
      </header>

      {/* Stats Bar */}
      <div className="px-6 py-4 border-b border-border">
        <div className="grid grid-cols-6 gap-4">
          {Object.entries(MOVEMENT_CONFIG).slice(0, 6).map(([type, config]) => {
            const count = MOCK_MOVEMENTS.filter((m) => m.movementType === type).length;
            const Icon = config.icon;
            return (
              <button
                key={type}
                onClick={() => {
                  if (typeFilter.includes(type as MovementType)) {
                    setTypeFilter(typeFilter.filter((t) => t !== type));
                  } else {
                    setTypeFilter([...typeFilter, type as MovementType]);
                  }
                }}
                className={cn(
                  'p-3 rounded-lg border transition-all',
                  typeFilter.includes(type as MovementType)
                    ? 'bg-white/5 border-border'
                    : 'bg-white/[0.01] border-border hover:border-border'
                )}
              >
                <div className="flex items-center justify-between mb-2">
                  <Icon className={cn('w-4 h-4', config.color)} />
                  <span className="text-lg font-bold text-foreground tabular-nums">{count}</span>
                </div>
                <span className="text-xs text-muted-foreground">{config.label}</span>
              </button>
            );
          })}
        </div>
      </div>

      {/* Filters Bar */}
      <div className="px-6 py-4 border-b border-border">
        <div className="flex items-center gap-4">
          <div className="relative flex-1 max-w-md">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
            <input
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              placeholder="Search by lot number or location..."
              className="w-full pl-10 pr-4 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-cyan-500/30"
            />
          </div>

          {/* Type Filter */}
          <div className="relative">
            <button className="flex items-center gap-2 px-3 py-2 rounded-lg bg-muted/30 border border-border text-sm text-muted-foreground hover:text-foreground hover:border-border transition-colors">
              <Filter className="w-4 h-4" />
              <span>Type</span>
              {typeFilter.length > 0 && (
                <span className="px-1.5 py-0.5 rounded bg-cyan-500/10 text-cyan-400 text-xs">
                  {typeFilter.length}
                </span>
              )}
              <ChevronDown className="w-3 h-3" />
            </button>
          </div>

          {/* Date Range */}
          <div className="relative">
            <button className="flex items-center gap-2 px-3 py-2 rounded-lg bg-muted/30 border border-border text-sm text-muted-foreground hover:text-foreground hover:border-border transition-colors">
              <span>Last 7 days</span>
              <ChevronDown className="w-3 h-3" />
            </button>
          </div>

          {/* Clear Filters */}
          {(typeFilter.length > 0 || searchQuery) && (
            <button
              onClick={() => {
                setTypeFilter([]);
                setSearchQuery('');
              }}
              className="text-xs text-muted-foreground hover:text-foreground"
            >
              Clear all
            </button>
          )}

          <button className="p-2 rounded-lg hover:bg-white/5 text-muted-foreground hover:text-foreground transition-colors ml-auto">
            <RefreshCw className="w-4 h-4" />
          </button>
        </div>
      </div>

      {/* Table */}
      <div className="px-6 py-4">
        <div className="bg-surface border border-border rounded-xl overflow-hidden">
          <table className="w-full">
            <thead>
              <tr className="border-b border-border bg-muted/30">
                <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                  Type
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                  Lot
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                  From
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                  To
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                  Quantity
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider w-12">
                  Sync
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                  Date
                </th>
              </tr>
            </thead>
            <tbody>
              {paginatedMovements.map((movement) => (
                <MovementRow key={movement.id} movement={movement} />
              ))}
            </tbody>
          </table>

          {/* Pagination */}
          <div className="px-4 py-3 border-t border-border flex items-center justify-between">
            <div className="text-sm text-muted-foreground">
              Showing {(currentPage - 1) * pageSize + 1} to {Math.min(currentPage * pageSize, filteredMovements.length)} of {filteredMovements.length}
            </div>
            <div className="flex items-center gap-2">
              <button
                onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
                disabled={currentPage === 1}
                className="p-2 rounded-lg hover:bg-white/5 text-muted-foreground hover:text-foreground disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                <ChevronLeft className="w-4 h-4" />
              </button>
              <span className="text-sm text-muted-foreground">
                Page {currentPage} of {totalPages}
              </span>
              <button
                onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
                disabled={currentPage === totalPages}
                className="p-2 rounded-lg hover:bg-white/5 text-muted-foreground hover:text-foreground disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                <ChevronRight className="w-4 h-4" />
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
