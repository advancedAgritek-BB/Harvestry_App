'use client';

import React from 'react';
import { 
  Package, 
  Scan, 
  Plus, 
  RefreshCw, 
  ArrowUpDown, 
  Filter, 
  LayoutGrid, 
  List, 
  Table2,
  ChevronDown,
  Search
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { useInventoryStore } from '@/features/inventory/stores/inventoryStore';
import {
  InventoryKPIWidget,
  SyncStatusWidget,
  ActiveHoldsWidget,
  RecentMovementsWidget,
  LocationTreeWidget,
  BalanceAlertWidget,
} from '@/features/inventory/widgets';
import type { LotSummary, ComplianceSummary, InventoryMovement, Hold, ComplianceIntegration, SyncQueueStatus } from '@/features/inventory/types';

// Mock data for development (will be replaced with API calls when backend is ready)
const MOCK_LOT_SUMMARY: LotSummary = {
  totalLots: 156,
  totalQuantity: 45000,
  byStatus: {
    available: 128,
    on_hold: 12,
    quarantine: 3,
    pending_coa: 8,
    coa_failed: 2,
    reserved: 3,
    allocated: 0,
    in_transit: 0,
    in_production: 0,
    consumed: 0,
    destroyed: 0,
  },
  byProductType: {
    flower: 80,
    trim: 25,
    concentrate: 30,
    preroll: 15,
    edible: 6,
    tincture: 0,
    topical: 0,
    clone: 0,
    seed: 0,
    shake: 0,
    other: 0,
  },
  onHold: 12,
  pendingSync: 5,
  expiringIn7Days: 8,
  expiringIn30Days: 23,
};

// Note: A user will only ever have either METRC or BIOTRACK, never both
const MOCK_COMPLIANCE_SUMMARY: ComplianceSummary = {
  integrations: [
    { provider: 'metrc', siteId: 'site-1', siteName: 'Main Facility', isConnected: true, lastSyncAt: new Date().toISOString(), pendingCount: 3, errorCount: 0 },
  ],
  activeHolds: 12,
  pendingDestructions: 2,
  pendingLabOrders: 5,
  dlqTotal: 0,
  syncHealth: 'healthy',
};

// Note: A user will only ever have either METRC or BIOTRACK, never both
const MOCK_INTEGRATIONS: ComplianceIntegration[] = [
  {
    id: 'int-1',
    siteId: 'site-1',
    provider: 'metrc',
    apiKeyMasked: '****1234',
    isConnected: true,
    lastConnectionTest: new Date().toISOString(),
    syncMode: 'realtime',
    retryPolicy: '3x exponential',
    dlqEnabled: true,
    lastSyncAt: new Date(Date.now() - 300000).toISOString(),
    lastSyncStatus: 'success',
    pendingCount: 3,
    errorCount: 0,
    createdAt: '2025-01-01',
    updatedAt: new Date().toISOString(),
  },
];

const MOCK_SYNC_STATUS: SyncQueueStatus[] = [
  { provider: 'metrc', siteId: 'site-1', pendingCount: 3, processingCount: 1, failedCount: 0, dlqCount: 0, successRate: 99.8, avgProcessingTimeMs: 250, lastSuccessAt: new Date().toISOString() },
];

const MOCK_MOVEMENTS: InventoryMovement[] = [
  { id: 'm1', siteId: 'site-1', movementType: 'transfer', status: 'completed', lotId: 'lot-1', lotNumber: 'LOT-2025-0001', fromLocationId: 'loc-1', fromLocationPath: 'Vault A > Rack 1', toLocationId: 'loc-2', toLocationPath: 'Vault A > Rack 2', quantity: 250, uom: 'g', syncStatus: 'synced', createdAt: new Date(Date.now() - 1800000).toISOString(), createdBy: 'user-1' },
  { id: 'm2', siteId: 'site-1', movementType: 'receive', status: 'completed', lotId: 'lot-2', lotNumber: 'LOT-2025-0005', toLocationId: 'loc-1', toLocationPath: 'Vault A > Rack 1', quantity: 1000, uom: 'g', syncStatus: 'pending', createdAt: new Date(Date.now() - 3600000).toISOString(), createdBy: 'user-1' },
  { id: 'm3', siteId: 'site-1', movementType: 'adjustment', status: 'completed', lotId: 'lot-3', lotNumber: 'LOT-2025-0012', fromLocationId: 'loc-2', fromLocationPath: 'Vault A > Rack 2', toLocationId: 'loc-2', toLocationPath: 'Vault A > Rack 2', quantity: -15, uom: 'g', syncStatus: 'synced', createdAt: new Date(Date.now() - 7200000).toISOString(), createdBy: 'user-2' },
];

const MOCK_HOLDS: Hold[] = [
  { id: 'hold-1', siteId: 'site-1', lotId: 'lot-5', lotNumber: 'LOT-2025-0025', reasonCode: 'coa_failed', reasonNotes: 'Pesticide levels exceeded', isActive: true, requiresTwoPersonApproval: true, syncStatus: 'synced', createdAt: new Date(Date.now() - 86400000).toISOString(), createdBy: 'John Smith', updatedAt: new Date(Date.now() - 86400000).toISOString() },
  { id: 'hold-2', siteId: 'site-1', lotId: 'lot-8', lotNumber: 'LOT-2025-0042', reasonCode: 'coa_pending', isActive: true, requiresTwoPersonApproval: false, syncStatus: 'synced', createdAt: new Date(Date.now() - 172800000).toISOString(), createdBy: 'Jane Doe', updatedAt: new Date(Date.now() - 172800000).toISOString() },
];

// Mock location tree data for demo
const MOCK_LOCATION_TREE = [
  {
    id: 'room-1',
    name: 'Vault A',
    code: 'VA',
    locationType: 'vault' as const,
    status: 'active' as const,
    path: 'Vault A',
    depth: 0,
    capacityPercent: 65,
    lotCount: 42,
    children: [
      {
        id: 'rack-1',
        name: 'Rack 1',
        code: 'R1',
        locationType: 'rack' as const,
        status: 'active' as const,
        path: 'Vault A > Rack 1',
        depth: 1,
        capacityPercent: 85,
        lotCount: 15,
        children: [
          {
            id: 'shelf-1',
            name: 'Shelf A',
            code: 'SA',
            locationType: 'shelf' as const,
            status: 'active' as const,
            path: 'Vault A > Rack 1 > Shelf A',
            depth: 2,
            capacityPercent: 100,
            lotCount: 8,
            children: [],
          },
          {
            id: 'shelf-2',
            name: 'Shelf B',
            code: 'SB',
            locationType: 'shelf' as const,
            status: 'active' as const,
            path: 'Vault A > Rack 1 > Shelf B',
            depth: 2,
            capacityPercent: 70,
            lotCount: 7,
            children: [],
          },
        ],
      },
      {
        id: 'rack-2',
        name: 'Rack 2',
        code: 'R2',
        locationType: 'rack' as const,
        status: 'active' as const,
        path: 'Vault A > Rack 2',
        depth: 1,
        capacityPercent: 45,
        lotCount: 12,
        children: [],
      },
    ],
  },
  {
    id: 'room-2',
    name: 'Warehouse B',
    code: 'WB',
    locationType: 'room' as const,
    status: 'active' as const,
    path: 'Warehouse B',
    depth: 0,
    capacityPercent: 35,
    lotCount: 28,
    children: [],
  },
];

export default function InventoryDashboardPage() {
  const store = useInventoryStore();
  
  // Using mock data for development - will be replaced with API hooks when backend is ready
  const lotSummary = MOCK_LOT_SUMMARY;
  const complianceSummary = MOCK_COMPLIANCE_SUMMARY;
  const integrations = MOCK_INTEGRATIONS;
  const syncQueueStatus = MOCK_SYNC_STATUS;
  const recentMovements = MOCK_MOVEMENTS;
  const activeHolds = MOCK_HOLDS;
  const loading = false;

  const handleRefreshAll = () => {
    // Will trigger API refresh when backend is ready
    console.log('Refreshing data...');
  };

  const handleSync = async (provider: 'metrc' | 'biotrack', siteId: string) => {
    // Will trigger sync when backend is ready
    console.log('Syncing:', provider, siteId);
  };

  return (
    <div className="min-h-screen bg-background">
      {/* Header */}
      <header className="sticky top-0 z-40 glass-header">
        <div className="px-6 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <div className="w-10 h-10 rounded-xl bg-cyan-500/10 flex items-center justify-center">
                <Package className="w-5 h-5 text-cyan-400" />
              </div>
              <div>
                <h1 className="text-xl font-semibold text-foreground">Inventory</h1>
                <p className="text-sm text-muted-foreground">Lot management & compliance tracking</p>
              </div>
            </div>

            {/* Quick Actions */}
            <div className="flex items-center gap-3">
              <button
                onClick={() => store.setScannerActive(true)}
                className="flex items-center gap-2 px-4 py-2 rounded-lg bg-cyan-500/10 text-cyan-400 hover:bg-cyan-500/20 transition-colors"
              >
                <Scan className="w-4 h-4" />
                <span className="text-sm font-medium">Scan</span>
              </button>
              <button className="flex items-center gap-2 px-4 py-2 rounded-lg bg-muted/50 text-foreground hover:bg-muted transition-colors">
                <ArrowUpDown className="w-4 h-4" />
                <span className="text-sm font-medium">Move</span>
              </button>
              <button className="flex items-center gap-2 px-4 py-2 rounded-lg bg-cyan-500/10 text-cyan-400 hover:bg-cyan-500/20 transition-colors">
                <Plus className="w-4 h-4" />
                <span className="text-sm font-medium">New Lot</span>
              </button>
              <button 
                onClick={handleRefreshAll}
                className="p-2 rounded-lg hover:bg-muted/50 text-muted-foreground hover:text-foreground transition-colors"
              >
                <RefreshCw className="w-5 h-5" />
              </button>
            </div>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="px-6 py-6 space-y-6">
        {/* Quick Navigation */}
        <div className="grid grid-cols-2 sm:grid-cols-4 lg:grid-cols-6 gap-3">
          <a
            href="/inventory/products"
            className="flex items-center gap-3 p-4 bg-surface border border-border rounded-xl hover:border-cyan-500/30 transition-all group"
          >
            <div className="w-10 h-10 rounded-lg bg-blue-500/10 flex items-center justify-center">
              <Package className="w-5 h-5 text-blue-400" />
            </div>
            <div>
              <div className="text-sm font-medium text-foreground group-hover:text-cyan-400">Products</div>
              <div className="text-xs text-muted-foreground">SKU Catalog</div>
            </div>
          </a>
          <a
            href="/inventory/lots"
            className="flex items-center gap-3 p-4 bg-surface border border-border rounded-xl hover:border-cyan-500/30 transition-all group"
          >
            <div className="w-10 h-10 rounded-lg bg-emerald-500/10 flex items-center justify-center">
              <Package className="w-5 h-5 text-emerald-400" />
            </div>
            <div>
              <div className="text-sm font-medium text-foreground group-hover:text-cyan-400">Lots</div>
              <div className="text-xs text-muted-foreground">Inventory</div>
            </div>
          </a>
          <a
            href="/inventory/production"
            className="flex items-center gap-3 p-4 bg-surface border border-border rounded-xl hover:border-cyan-500/30 transition-all group"
          >
            <div className="w-10 h-10 rounded-lg bg-violet-500/10 flex items-center justify-center">
              <ArrowUpDown className="w-5 h-5 text-violet-400" />
            </div>
            <div>
              <div className="text-sm font-medium text-foreground group-hover:text-cyan-400">Production</div>
              <div className="text-xs text-muted-foreground">Work Orders</div>
            </div>
          </a>
          <a
            href="/inventory/boms"
            className="flex items-center gap-3 p-4 bg-surface border border-border rounded-xl hover:border-cyan-500/30 transition-all group"
          >
            <div className="w-10 h-10 rounded-lg bg-amber-500/10 flex items-center justify-center">
              <Filter className="w-5 h-5 text-amber-400" />
            </div>
            <div>
              <div className="text-sm font-medium text-foreground group-hover:text-cyan-400">BOMs</div>
              <div className="text-xs text-muted-foreground">Recipes</div>
            </div>
          </a>
          <a
            href="/inventory/batches"
            className="flex items-center gap-3 p-4 bg-surface border border-border rounded-xl hover:border-cyan-500/30 transition-all group"
          >
            <div className="w-10 h-10 rounded-lg bg-lime-500/10 flex items-center justify-center">
              <Package className="w-5 h-5 text-lime-400" />
            </div>
            <div>
              <div className="text-sm font-medium text-foreground group-hover:text-cyan-400">Batches</div>
              <div className="text-xs text-muted-foreground">Cultivation</div>
            </div>
          </a>
          <a
            href="/inventory/movements"
            className="flex items-center gap-3 p-4 bg-surface border border-border rounded-xl hover:border-cyan-500/30 transition-all group"
          >
            <div className="w-10 h-10 rounded-lg bg-cyan-500/10 flex items-center justify-center">
              <ArrowUpDown className="w-5 h-5 text-cyan-400" />
            </div>
            <div>
              <div className="text-sm font-medium text-foreground group-hover:text-cyan-400">Movements</div>
              <div className="text-xs text-muted-foreground">History</div>
            </div>
          </a>
        </div>

        {/* KPI Strip */}
        <InventoryKPIWidget
          lotSummary={lotSummary}
          complianceSummary={complianceSummary}
          loading={loading}
        />

        {/* Main Grid */}
        <div className="grid grid-cols-12 gap-6">
          {/* Left Column - 8 cols */}
          <div className="col-span-12 lg:col-span-8 space-y-6">
            {/* Sync Status */}
            <div className="bg-surface border border-border rounded-xl p-5">
              <SyncStatusWidget
                integrations={integrations}
                syncStatus={syncQueueStatus}
                onSync={handleSync}
                loading={loading}
              />
            </div>

            {/* Recent Movements */}
            <div className="bg-surface border border-border rounded-xl p-5">
              <RecentMovementsWidget
                movements={recentMovements}
                loading={loading}
              />
            </div>

            {/* Lot List Preview */}
            <div className="bg-surface border border-border rounded-xl p-5">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-sm font-semibold text-foreground">Lot Overview</h3>
                <div className="flex items-center gap-2">
                  {/* View Toggle */}
                  <div className="flex items-center bg-muted/30 rounded-lg p-1">
                    <button
                      onClick={() => store.setViewMode('table')}
                      className={cn(
                        'p-1.5 rounded transition-colors',
                        store.viewMode === 'table' 
                          ? 'bg-cyan-500/10 text-cyan-400' 
                          : 'text-muted-foreground hover:text-foreground'
                      )}
                    >
                      <Table2 className="w-4 h-4" />
                    </button>
                    <button
                      onClick={() => store.setViewMode('grid')}
                      className={cn(
                        'p-1.5 rounded transition-colors',
                        store.viewMode === 'grid' 
                          ? 'bg-cyan-500/10 text-cyan-400' 
                          : 'text-muted-foreground hover:text-foreground'
                      )}
                    >
                      <LayoutGrid className="w-4 h-4" />
                    </button>
                    <button
                      onClick={() => store.setViewMode('list')}
                      className={cn(
                        'p-1.5 rounded transition-colors',
                        store.viewMode === 'list' 
                          ? 'bg-cyan-500/10 text-cyan-400' 
                          : 'text-muted-foreground hover:text-foreground'
                      )}
                    >
                      <List className="w-4 h-4" />
                    </button>
                  </div>
                  {/* Filter */}
                  <button className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-muted/30 text-muted-foreground hover:text-foreground transition-colors">
                    <Filter className="w-3.5 h-3.5" />
                    <span className="text-xs">Filter</span>
                    <ChevronDown className="w-3 h-3" />
                  </button>
                  {/* View All */}
                  <a href="/inventory/lots" className="text-xs text-cyan-400 hover:underline">
                    View All →
                  </a>
                </div>
              </div>

              {/* Search */}
              <div className="relative mb-4">
                <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
                <input
                  type="text"
                  placeholder="Search lots by ID, strain, or location..."
                  className="w-full pl-10 pr-4 py-2.5 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-cyan-500/30"
                />
              </div>

              {/* Quick Stats */}
              <div className="grid grid-cols-4 gap-3 mb-4">
                <div className="p-3 rounded-lg bg-muted/30 border border-border">
                  <div className="text-xs text-muted-foreground mb-1">Available</div>
                  <div className="text-lg font-semibold text-emerald-400 tabular-nums">
                    {lotSummary?.byStatus?.available ?? 0}
                  </div>
                </div>
                <div className="p-3 rounded-lg bg-muted/30 border border-border">
                  <div className="text-xs text-muted-foreground mb-1">On Hold</div>
                  <div className="text-lg font-semibold text-amber-400 tabular-nums">
                    {lotSummary?.onHold ?? 0}
                  </div>
                </div>
                <div className="p-3 rounded-lg bg-muted/30 border border-border">
                  <div className="text-xs text-muted-foreground mb-1">Expiring (7d)</div>
                  <div className="text-lg font-semibold text-rose-400 tabular-nums">
                    {lotSummary?.expiringIn7Days ?? 0}
                  </div>
                </div>
                <div className="p-3 rounded-lg bg-muted/30 border border-border">
                  <div className="text-xs text-muted-foreground mb-1">Pending Sync</div>
                  <div className="text-lg font-semibold text-cyan-400 tabular-nums">
                    {lotSummary?.pendingSync ?? 0}
                  </div>
                </div>
              </div>

              {/* Placeholder for lot list/grid */}
              <div className="text-center py-8 border border-dashed border-border rounded-lg">
                <p className="text-sm text-muted-foreground">
                  Lot list will display here based on selected view mode
                </p>
                <a href="/inventory/lots" className="mt-2 inline-block text-xs text-cyan-400 hover:underline">
                  Go to full lot management →
                </a>
              </div>
            </div>
          </div>

          {/* Right Column - 4 cols */}
          <div className="col-span-12 lg:col-span-4 space-y-6">
            {/* Active Holds */}
            <div className="bg-surface border border-border rounded-xl p-5">
              <ActiveHoldsWidget
                holds={activeHolds}
                loading={loading}
              />
            </div>

            {/* Location Tree */}
            <div className="bg-surface border border-border rounded-xl p-5">
              <LocationTreeWidget
                tree={MOCK_LOCATION_TREE}
                selectedId={store.selectedLocationId}
                expandedIds={store.expandedLocationIds}
                onSelect={(id) => store.setSelectedLocation(id)}
                onToggleExpand={(id) => store.toggleLocationExpanded(id)}
              />
            </div>

            {/* Balance Alerts */}
            <div className="bg-surface border border-border rounded-xl p-5">
              <BalanceAlertWidget
                overallVariancePercent={0.12}
                lastReconcileAt={new Date().toISOString()}
              />
            </div>
          </div>
        </div>
      </main>

      {/* Scanner Mode Overlay (placeholder) */}
      {store.scannerActive && (
        <div className="fixed inset-0 z-50 bg-background/90 flex items-center justify-center">
          <div className="text-center">
            <div className="w-64 h-64 border-2 border-cyan-400 rounded-xl mb-4 flex items-center justify-center">
              <Scan className="w-16 h-16 text-cyan-400 animate-pulse" />
            </div>
            <p className="text-foreground mb-2">Scanner Mode Active</p>
            <p className="text-sm text-muted-foreground mb-4">Point camera at barcode or use hardware scanner</p>
            <button
              onClick={() => store.setScannerActive(false)}
              className="px-4 py-2 rounded-lg bg-muted text-foreground hover:bg-muted/80 transition-colors"
            >
              Close Scanner
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
