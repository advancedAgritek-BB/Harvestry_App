'use client';

import React, { useState, useMemo } from 'react';
import {
  Package,
  Plus,
  Filter,
  Download,
  Upload,
  MoreHorizontal,
  Search,
  ChevronDown,
  ChevronLeft,
  ChevronRight,
  Scissors,
  Layers,
  ArrowUpDown,
  Tag,
  CheckCircle,
  AlertTriangle,
  Clock,
  XCircle,
  RefreshCw,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import type { InventoryLot, LotStatus, ProductType } from '@/features/inventory/types';

const STATUS_CONFIG: Record<LotStatus, { label: string; color: string; icon: React.ElementType }> = {
  available: { label: 'Available', color: 'text-emerald-400 bg-emerald-500/10', icon: CheckCircle },
  on_hold: { label: 'On Hold', color: 'text-amber-400 bg-amber-500/10', icon: AlertTriangle },
  quarantine: { label: 'Quarantine', color: 'text-rose-400 bg-rose-500/10', icon: XCircle },
  pending_coa: { label: 'Pending COA', color: 'text-cyan-400 bg-cyan-500/10', icon: Clock },
  coa_failed: { label: 'COA Failed', color: 'text-rose-400 bg-rose-500/10', icon: XCircle },
  reserved: { label: 'Reserved', color: 'text-violet-400 bg-violet-500/10', icon: Tag },
  in_transit: { label: 'In Transit', color: 'text-blue-400 bg-blue-500/10', icon: ArrowUpDown },
  destroyed: { label: 'Destroyed', color: 'text-muted-foreground bg-muted/50', icon: XCircle },
};

const SYNC_STATUS_COLORS = {
  synced: 'bg-emerald-400',
  pending: 'bg-amber-400 animate-pulse',
  error: 'bg-rose-400',
  stale: 'bg-muted-foreground',
};

// Mock data for demo
const MOCK_LOTS: InventoryLot[] = Array.from({ length: 25 }, (_, i) => ({
  id: `lot-${i + 1}`,
  siteId: 'site-1',
  lotNumber: `LOT-2025-${String(i + 1).padStart(4, '0')}`,
  barcode: `010012345678901110${String(i + 1).padStart(4, '0')}`,
  productType: ['flower', 'trim', 'concentrate', 'preroll'][i % 4] as ProductType,
  strainId: `strain-${(i % 5) + 1}`,
  strainName: ['Blue Dream', 'OG Kush', 'Sour Diesel', 'Girl Scout Cookies', 'Gorilla Glue'][i % 5],
  quantity: Math.floor(Math.random() * 1000) + 100,
  uom: 'g',
  originalQuantity: 1000,
  locationId: `loc-${(i % 3) + 1}`,
  locationPath: ['Vault A > Rack 1 > Shelf A', 'Warehouse B > Zone 1', 'Vault A > Rack 2'][i % 3],
  status: ['available', 'on_hold', 'pending_coa', 'available', 'available'][i % 5] as LotStatus,
  syncStatus: ['synced', 'synced', 'pending', 'synced', 'error'][i % 5] as 'synced' | 'pending' | 'error' | 'stale',
  harvestDate: '2025-01-15',
  packageDate: '2025-01-20',
  expirationDate: '2026-01-20',
  thcPercent: 18 + Math.random() * 10,
  cbdPercent: Math.random() * 2,
  metrcId: i % 3 === 0 ? `1A40500000${String(i).padStart(7, '0')}` : undefined,
  createdAt: new Date(Date.now() - i * 86400000).toISOString(),
  createdBy: 'user-1',
  updatedAt: new Date(Date.now() - i * 3600000).toISOString(),
  updatedBy: 'user-1',
}));

function LotRow({ 
  lot, 
  selected, 
  onSelect,
  onView,
}: { 
  lot: InventoryLot; 
  selected: boolean;
  onSelect: (selected: boolean) => void;
  onView: () => void;
}) {
  const status = STATUS_CONFIG[lot.status];
  const StatusIcon = status.icon;

  return (
    <tr 
      className={cn(
        'group border-b border-border hover:bg-muted/30 transition-colors cursor-pointer',
        selected && 'bg-cyan-500/5'
      )}
      onClick={onView}
    >
      {/* Checkbox */}
      <td className="px-4 py-3 w-12" onClick={(e) => e.stopPropagation()}>
        <input
          type="checkbox"
          checked={selected}
          onChange={(e) => onSelect(e.target.checked)}
          className="w-4 h-4 rounded border-border bg-transparent text-cyan-500 focus:ring-cyan-500/30"
        />
      </td>

      {/* Lot Number */}
      <td className="px-4 py-3">
        <div className="flex items-center gap-2">
          <span className="font-mono text-sm text-foreground">{lot.lotNumber}</span>
          <div className={cn('w-1.5 h-1.5 rounded-full', SYNC_STATUS_COLORS[lot.syncStatus])} />
        </div>
      </td>

      {/* Strain / Product */}
      <td className="px-4 py-3">
        <div>
          <div className="text-sm text-foreground">{lot.strainName}</div>
          <div className="text-xs text-muted-foreground capitalize">{lot.productType}</div>
        </div>
      </td>

      {/* Quantity */}
      <td className="px-4 py-3">
        <span className="text-sm text-foreground tabular-nums">
          {lot.quantity.toLocaleString()} {lot.uom}
        </span>
      </td>

      {/* Location */}
      <td className="px-4 py-3">
        <span className="text-sm text-muted-foreground truncate max-w-[200px] block">
          {lot.locationPath}
        </span>
      </td>

      {/* Status */}
      <td className="px-4 py-3">
        <span className={cn(
          'inline-flex items-center gap-1.5 px-2 py-1 rounded-full text-xs font-medium',
          status.color
        )}>
          <StatusIcon className="w-3 h-3" />
          {status.label}
        </span>
      </td>

      {/* THC/CBD */}
      <td className="px-4 py-3">
        <div className="text-sm tabular-nums">
          <span className="text-foreground">{lot.thcPercent?.toFixed(1)}%</span>
          <span className="text-muted-foreground"> / </span>
          <span className="text-muted-foreground">{lot.cbdPercent?.toFixed(1)}%</span>
        </div>
      </td>

      {/* Expiration */}
      <td className="px-4 py-3">
        <span className="text-sm text-muted-foreground">
          {lot.expirationDate ? new Date(lot.expirationDate).toLocaleDateString() : '—'}
        </span>
      </td>

      {/* Actions */}
      <td className="px-4 py-3 w-12" onClick={(e) => e.stopPropagation()}>
        <button className="p-1.5 rounded-md opacity-0 group-hover:opacity-100 hover:bg-muted text-muted-foreground hover:text-foreground transition-all">
          <MoreHorizontal className="w-4 h-4" />
        </button>
      </td>
    </tr>
  );
}

export default function LotsListPage() {
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<LotStatus[]>([]);
  const [productFilter, setProductFilter] = useState<ProductType[]>([]);
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 20;

  // In real app, this would come from useLots hook
  const lots = MOCK_LOTS;
  const total = lots.length;

  const filteredLots = useMemo(() => {
    return lots.filter((lot) => {
      if (searchQuery) {
        const query = searchQuery.toLowerCase();
        if (
          !lot.lotNumber.toLowerCase().includes(query) &&
          !lot.strainName?.toLowerCase().includes(query) &&
          !lot.locationPath?.toLowerCase().includes(query)
        ) {
          return false;
        }
      }
      if (statusFilter.length > 0 && !statusFilter.includes(lot.status)) {
        return false;
      }
      if (productFilter.length > 0 && !productFilter.includes(lot.productType)) {
        return false;
      }
      return true;
    });
  }, [lots, searchQuery, statusFilter, productFilter]);

  const paginatedLots = filteredLots.slice((currentPage - 1) * pageSize, currentPage * pageSize);
  const totalPages = Math.ceil(filteredLots.length / pageSize);

  const handleSelectAll = (selected: boolean) => {
    if (selected) {
      setSelectedIds(new Set(paginatedLots.map((l) => l.id)));
    } else {
      setSelectedIds(new Set());
    }
  };

  const handleSelect = (lotId: string, selected: boolean) => {
    const next = new Set(selectedIds);
    if (selected) {
      next.add(lotId);
    } else {
      next.delete(lotId);
    }
    setSelectedIds(next);
  };

  const allSelected = paginatedLots.length > 0 && paginatedLots.every((l) => selectedIds.has(l.id));
  const someSelected = selectedIds.size > 0;

  return (
    <div className="min-h-screen bg-background">
      {/* Header */}
      <header className="sticky top-0 z-40 glass-header">
        <div className="px-6 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <a href="/inventory" className="p-2 rounded-lg hover:bg-muted text-muted-foreground hover:text-foreground transition-colors">
                <ChevronLeft className="w-5 h-5" />
              </a>
              <div>
                <h1 className="text-xl font-semibold text-foreground">Lots</h1>
                <p className="text-sm text-muted-foreground">{total.toLocaleString()} total lots</p>
              </div>
            </div>

            {/* Actions */}
            <div className="flex items-center gap-3">
              {someSelected && (
                <div className="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-cyan-500/10 border border-cyan-500/20">
                  <span className="text-sm text-cyan-400">{selectedIds.size} selected</span>
                  <button className="p-1 hover:bg-cyan-500/20 rounded text-cyan-400" title="Move">
                    <ArrowUpDown className="w-4 h-4" />
                  </button>
                  <button className="p-1 hover:bg-cyan-500/20 rounded text-cyan-400" title="Split">
                    <Scissors className="w-4 h-4" />
                  </button>
                  <button className="p-1 hover:bg-cyan-500/20 rounded text-cyan-400" title="Merge">
                    <Layers className="w-4 h-4" />
                  </button>
                </div>
              )}
              <button className="flex items-center gap-2 px-3 py-2 rounded-lg bg-muted/50 text-foreground hover:bg-muted transition-colors">
                <Download className="w-4 h-4" />
                <span className="text-sm">Export</span>
              </button>
              <button className="flex items-center gap-2 px-3 py-2 rounded-lg bg-muted/50 text-foreground hover:bg-muted transition-colors">
                <Upload className="w-4 h-4" />
                <span className="text-sm">Import</span>
              </button>
              <button className="flex items-center gap-2 px-4 py-2 rounded-lg bg-cyan-500/10 text-cyan-400 hover:bg-cyan-500/20 transition-colors">
                <Plus className="w-4 h-4" />
                <span className="text-sm font-medium">New Lot</span>
              </button>
            </div>
          </div>
        </div>
      </header>

      {/* Filters Bar */}
      <div className="px-6 py-4 border-b border-border">
        <div className="flex items-center gap-4">
          {/* Search */}
          <div className="relative flex-1 max-w-md">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
            <input
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              placeholder="Search by lot number, strain, or location..."
              className="w-full pl-10 pr-4 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-cyan-500/30"
            />
          </div>

          {/* Status Filter */}
          <div className="relative">
            <button className="flex items-center gap-2 px-3 py-2 rounded-lg bg-muted/30 border border-border text-sm text-muted-foreground hover:text-foreground hover:border-border/80 transition-colors">
              <Filter className="w-4 h-4" />
              <span>Status</span>
              {statusFilter.length > 0 && (
                <span className="px-1.5 py-0.5 rounded bg-amber-500/10 text-amber-400 text-xs">
                  {statusFilter.length}
                </span>
              )}
              <ChevronDown className="w-3 h-3" />
            </button>
          </div>

          {/* Product Filter */}
          <div className="relative">
            <button className="flex items-center gap-2 px-3 py-2 rounded-lg bg-muted/30 border border-border text-sm text-muted-foreground hover:text-foreground hover:border-border/80 transition-colors">
              <Package className="w-4 h-4" />
              <span>Product Type</span>
              {productFilter.length > 0 && (
                <span className="px-1.5 py-0.5 rounded bg-amber-500/10 text-amber-400 text-xs">
                  {productFilter.length}
                </span>
              )}
              <ChevronDown className="w-3 h-3" />
            </button>
          </div>

          {/* Clear Filters */}
          {(statusFilter.length > 0 || productFilter.length > 0 || searchQuery) && (
            <button
              onClick={() => {
                setStatusFilter([]);
                setProductFilter([]);
                setSearchQuery('');
              }}
              className="text-xs text-muted-foreground hover:text-foreground"
            >
              Clear all
            </button>
          )}

          {/* Refresh */}
          <button className="p-2 rounded-lg hover:bg-muted text-muted-foreground hover:text-foreground transition-colors ml-auto">
            <RefreshCw className="w-4 h-4" />
          </button>
        </div>
      </div>

      {/* Table */}
      <div className="px-6">
        <div className="bg-surface border border-border rounded-xl overflow-hidden">
          <table className="w-full">
            <thead>
              <tr className="border-b border-border bg-muted/30">
                <th className="px-4 py-3 w-12">
                  <input
                    type="checkbox"
                    checked={allSelected}
                    onChange={(e) => handleSelectAll(e.target.checked)}
                    className="w-4 h-4 rounded border-border bg-transparent text-cyan-500 focus:ring-cyan-500/30"
                  />
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                  Lot Number
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                  Strain / Product
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                  Quantity
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                  Location
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                  Status
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                  THC / CBD
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                  Expires
                </th>
                <th className="px-4 py-3 w-12"></th>
              </tr>
            </thead>
            <tbody>
              {paginatedLots.map((lot) => (
                <LotRow
                  key={lot.id}
                  lot={lot}
                  selected={selectedIds.has(lot.id)}
                  onSelect={(selected) => handleSelect(lot.id, selected)}
                  onView={() => window.location.href = `/inventory/lots/${lot.id}`}
                />
              ))}
            </tbody>
          </table>

          {/* Pagination */}
          <div className="px-4 py-3 border-t border-border flex items-center justify-between">
            <div className="text-sm text-muted-foreground">
              Showing {(currentPage - 1) * pageSize + 1} to {Math.min(currentPage * pageSize, filteredLots.length)} of {filteredLots.length} lots
            </div>
            <div className="flex items-center gap-2">
              <button
                onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
                disabled={currentPage === 1}
                className="p-2 rounded-lg hover:bg-muted text-muted-foreground hover:text-foreground disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                <ChevronLeft className="w-4 h-4" />
              </button>
              <span className="text-sm text-muted-foreground">
                Page {currentPage} of {totalPages}
              </span>
              <button
                onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
                disabled={currentPage === totalPages}
                className="p-2 rounded-lg hover:bg-muted text-muted-foreground hover:text-foreground disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
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
