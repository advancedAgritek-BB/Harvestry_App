'use client';

import React, { useState } from 'react';
import {
  Package,
  ChevronLeft,
  Edit2,
  Scissors,
  Layers,
  ArrowUpDown,
  Tag,
  Printer,
  MoreHorizontal,
  CheckCircle,
  AlertTriangle,
  Clock,
  XCircle,
  RefreshCw,
  MapPin,
  Beaker,
  FileText,
  History,
  Shield,
  GitBranch,
  ExternalLink,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import type { LotStatus } from '@/features/inventory/types';

const STATUS_CONFIG: Record<LotStatus, { label: string; color: string; bgColor: string; icon: React.ElementType }> = {
  available: { label: 'Available', color: 'text-emerald-400', bgColor: 'bg-emerald-500/10 border-emerald-500/20', icon: CheckCircle },
  on_hold: { label: 'On Hold', color: 'text-amber-400', bgColor: 'bg-amber-500/10 border-amber-500/20', icon: AlertTriangle },
  quarantine: { label: 'Quarantine', color: 'text-rose-400', bgColor: 'bg-rose-500/10 border-rose-500/20', icon: XCircle },
  pending_coa: { label: 'Pending COA', color: 'text-cyan-400', bgColor: 'bg-cyan-500/10 border-cyan-500/20', icon: Clock },
  coa_failed: { label: 'COA Failed', color: 'text-rose-400', bgColor: 'bg-rose-500/10 border-rose-500/20', icon: XCircle },
  reserved: { label: 'Reserved', color: 'text-violet-400', bgColor: 'bg-violet-500/10 border-violet-500/20', icon: Tag },
  allocated: { label: 'Allocated', color: 'text-indigo-400', bgColor: 'bg-indigo-500/10 border-indigo-500/20', icon: Tag },
  in_transit: { label: 'In Transit', color: 'text-blue-400', bgColor: 'bg-blue-500/10 border-blue-500/20', icon: ArrowUpDown },
  in_production: { label: 'In Production', color: 'text-orange-400', bgColor: 'bg-orange-500/10 border-orange-500/20', icon: Clock },
  consumed: { label: 'Consumed', color: 'text-muted-foreground', bgColor: 'bg-muted/50 border-border', icon: CheckCircle },
  destroyed: { label: 'Destroyed', color: 'text-muted-foreground', bgColor: 'bg-muted/50 border-border', icon: XCircle },
};

// Mock lot data
const MOCK_LOT = {
  id: 'lot-1',
  siteId: 'site-1',
  lotNumber: 'LOT-2025-0001',
  barcode: '01001234567890111012345',
  externalId: undefined,
  productType: 'flower' as const,
  strainId: 'strain-1',
  strainName: 'Blue Dream',
  batchId: 'batch-1',
  batchName: 'BD-2025-F01',
  quantity: 856.5,
  uom: 'g',
  originalQuantity: 1000,
  locationId: 'loc-1',
  locationPath: 'Vault A > Rack 1 > Shelf A',
  status: 'available' as LotStatus,
  holdReason: undefined,
  harvestDate: '2025-01-15',
  packageDate: '2025-01-20',
  expirationDate: '2026-01-20',
  labOrderId: 'lab-order-1',
  coaStatus: 'passed' as 'passed' | 'failed' | 'pending',
  thcPercent: 22.5,
  cbdPercent: 0.8,
  metrcId: '1A4050000000001234567',
  biotrackId: undefined,
  lastSyncAt: new Date().toISOString(),
  syncStatus: 'synced' as const,
  unitCost: 2.50,
  totalCost: 2141.25,
  notes: 'Premium grade flower from harvest batch BD-2025-F01. Excellent terpene profile.',
  createdAt: '2025-01-20T10:30:00Z',
  createdBy: 'John Smith',
  updatedAt: '2025-01-25T14:15:00Z',
  updatedBy: 'Jane Doe',
};

const MOCK_MOVEMENTS = [
  { id: 'm1', type: 'receive', from: 'Receiving', to: 'Vault A > Staging', quantity: 1000, date: '2025-01-20T10:30:00Z', user: 'John Smith' },
  { id: 'm2', type: 'transfer', from: 'Vault A > Staging', to: 'Vault A > Rack 1 > Shelf A', quantity: 1000, date: '2025-01-20T14:00:00Z', user: 'Jane Doe' },
  { id: 'm3', type: 'adjustment', from: 'Vault A > Rack 1 > Shelf A', to: '-', quantity: -143.5, date: '2025-01-22T09:15:00Z', user: 'John Smith', reason: 'Sample for testing' },
];

const MOCK_LINEAGE = [
  { id: 'parent-1', lotNumber: 'LOT-2025-BATCH-001', relationship: 'split_from', quantity: 1000 },
];

const TABS = [
  { id: 'overview', label: 'Overview', icon: Package },
  { id: 'lineage', label: 'Lineage', icon: GitBranch },
  { id: 'movements', label: 'Movements', icon: History },
  { id: 'lab', label: 'Lab Results', icon: Beaker },
  { id: 'compliance', label: 'Compliance', icon: Shield },
];

export default function LotDetailPage({ params }: { params: { id: string } }) {
  const [activeTab, setActiveTab] = useState('overview');
  const lot = MOCK_LOT;
  const status = STATUS_CONFIG[lot.status];
  const StatusIcon = status.icon;

  const formatDate = (dateString?: string) => {
    if (!dateString) return '—';
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  const formatDateTime = (dateString: string) => {
    return new Date(dateString).toLocaleString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  return (
    <div className="min-h-screen bg-background">
      {/* Header */}
      <header className="sticky top-0 z-40 glass-header">
        <div className="px-6 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <a href="/inventory/lots" className="p-2 rounded-lg hover:bg-white/5 text-muted-foreground hover:text-foreground transition-colors">
                <ChevronLeft className="w-5 h-5" />
              </a>
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-xl bg-cyan-500/10 flex items-center justify-center">
                  <Package className="w-5 h-5 text-cyan-400" />
                </div>
                <div>
                  <div className="flex items-center gap-3">
                    <h1 className="text-xl font-mono font-semibold text-foreground">{lot.lotNumber}</h1>
                    <div className={cn('w-2 h-2 rounded-full', 
                      lot.syncStatus === 'synced' ? 'bg-emerald-400' :
                      lot.syncStatus === 'pending' ? 'bg-amber-400 animate-pulse' :
                      'bg-rose-400'
                    )} />
                  </div>
                  <p className="text-sm text-muted-foreground">{lot.strainName} • {lot.productType}</p>
                </div>
              </div>
            </div>

            {/* Actions */}
            <div className="flex items-center gap-3">
              <button className="flex items-center gap-2 px-3 py-2 rounded-lg bg-white/5 text-foreground hover:bg-white/10 transition-colors">
                <Edit2 className="w-4 h-4" />
                <span className="text-sm">Edit</span>
              </button>
              <button className="flex items-center gap-2 px-3 py-2 rounded-lg bg-white/5 text-foreground hover:bg-white/10 transition-colors">
                <Scissors className="w-4 h-4" />
                <span className="text-sm">Split</span>
              </button>
              <button className="flex items-center gap-2 px-3 py-2 rounded-lg bg-white/5 text-foreground hover:bg-white/10 transition-colors">
                <ArrowUpDown className="w-4 h-4" />
                <span className="text-sm">Move</span>
              </button>
              <button className="flex items-center gap-2 px-3 py-2 rounded-lg bg-white/5 text-foreground hover:bg-white/10 transition-colors">
                <Printer className="w-4 h-4" />
                <span className="text-sm">Print Label</span>
              </button>
              <button className="p-2 rounded-lg hover:bg-white/5 text-muted-foreground hover:text-foreground transition-colors">
                <MoreHorizontal className="w-5 h-5" />
              </button>
            </div>
          </div>
        </div>
      </header>

      {/* Status Banner */}
      <div className={cn('mx-6 mt-4 p-4 rounded-xl border flex items-center justify-between', status.bgColor)}>
        <div className="flex items-center gap-3">
          <StatusIcon className={cn('w-5 h-5', status.color)} />
          <div>
            <span className={cn('text-sm font-medium', status.color)}>{status.label}</span>
            {lot.holdReason && (
              <span className="text-sm text-muted-foreground ml-2">— {lot.holdReason}</span>
            )}
          </div>
        </div>
        <div className="flex items-center gap-4 text-sm">
          <span className="text-muted-foreground">
            Last synced: {formatDateTime(lot.lastSyncAt!)}
          </span>
          <button className="flex items-center gap-1.5 text-cyan-400 hover:underline">
            <RefreshCw className="w-3 h-3" />
            Sync Now
          </button>
        </div>
      </div>

      {/* Tabs */}
      <div className="mx-6 mt-6 border-b border-border">
        <div className="flex gap-1">
          {TABS.map((tab) => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={cn(
                'flex items-center gap-2 px-4 py-3 text-sm font-medium border-b-2 transition-colors',
                activeTab === tab.id
                  ? 'text-amber-400 border-amber-400'
                  : 'text-muted-foreground border-transparent hover:text-foreground'
              )}
            >
              <tab.icon className="w-4 h-4" />
              {tab.label}
            </button>
          ))}
        </div>
      </div>

      {/* Tab Content */}
      <div className="px-6 py-6">
        {activeTab === 'overview' && (
          <div className="grid grid-cols-12 gap-6">
            {/* Main Info */}
            <div className="col-span-8 space-y-6">
              {/* Quantity Card */}
              <div className="bg-surface border border-border rounded-xl p-5">
                <h3 className="text-sm font-semibold text-foreground mb-4">Quantity & Value</h3>
                <div className="grid grid-cols-4 gap-4">
                  <div>
                    <div className="text-xs text-muted-foreground mb-1">Current Quantity</div>
                    <div className="text-2xl font-bold text-foreground tabular-nums">
                      {lot.quantity.toLocaleString()} <span className="text-sm font-normal text-muted-foreground">{lot.uom}</span>
                    </div>
                  </div>
                  <div>
                    <div className="text-xs text-muted-foreground mb-1">Original Quantity</div>
                    <div className="text-lg font-medium text-muted-foreground tabular-nums">
                      {lot.originalQuantity.toLocaleString()} {lot.uom}
                    </div>
                  </div>
                  <div>
                    <div className="text-xs text-muted-foreground mb-1">Unit Cost</div>
                    <div className="text-lg font-medium text-foreground tabular-nums">
                      ${lot.unitCost?.toFixed(2)}
                    </div>
                  </div>
                  <div>
                    <div className="text-xs text-muted-foreground mb-1">Total Value</div>
                    <div className="text-lg font-medium text-emerald-400 tabular-nums">
                      ${lot.totalCost?.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                    </div>
                  </div>
                </div>
              </div>

              {/* Details Card */}
              <div className="bg-surface border border-border rounded-xl p-5">
                <h3 className="text-sm font-semibold text-foreground mb-4">Lot Details</h3>
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-3">
                    <div className="flex justify-between py-2 border-b border-border">
                      <span className="text-sm text-muted-foreground">Strain</span>
                      <span className="text-sm text-foreground">{lot.strainName}</span>
                    </div>
                    <div className="flex justify-between py-2 border-b border-border">
                      <span className="text-sm text-muted-foreground">Product Type</span>
                      <span className="text-sm text-foreground capitalize">{lot.productType}</span>
                    </div>
                    <div className="flex justify-between py-2 border-b border-border">
                      <span className="text-sm text-muted-foreground">Batch</span>
                      <span className="text-sm text-foreground">{lot.batchName}</span>
                    </div>
                    <div className="flex justify-between py-2 border-b border-border">
                      <span className="text-sm text-muted-foreground">Harvest Date</span>
                      <span className="text-sm text-foreground">{formatDate(lot.harvestDate)}</span>
                    </div>
                  </div>
                  <div className="space-y-3">
                    <div className="flex justify-between py-2 border-b border-border">
                      <span className="text-sm text-muted-foreground">Package Date</span>
                      <span className="text-sm text-foreground">{formatDate(lot.packageDate)}</span>
                    </div>
                    <div className="flex justify-between py-2 border-b border-border">
                      <span className="text-sm text-muted-foreground">Expiration Date</span>
                      <span className="text-sm text-foreground">{formatDate(lot.expirationDate)}</span>
                    </div>
                    <div className="flex justify-between py-2 border-b border-border">
                      <span className="text-sm text-muted-foreground">THC %</span>
                      <span className="text-sm text-foreground">{lot.thcPercent?.toFixed(1)}%</span>
                    </div>
                    <div className="flex justify-between py-2 border-b border-border">
                      <span className="text-sm text-muted-foreground">CBD %</span>
                      <span className="text-sm text-foreground">{lot.cbdPercent?.toFixed(1)}%</span>
                    </div>
                  </div>
                </div>
              </div>

              {/* Notes */}
              {lot.notes && (
                <div className="bg-surface border border-border rounded-xl p-5">
                  <h3 className="text-sm font-semibold text-foreground mb-3">Notes</h3>
                  <p className="text-sm text-muted-foreground">{lot.notes}</p>
                </div>
              )}
            </div>

            {/* Sidebar */}
            <div className="col-span-4 space-y-6">
              {/* Location Card */}
              <div className="bg-surface border border-border rounded-xl p-5">
                <div className="flex items-center justify-between mb-3">
                  <h3 className="text-sm font-semibold text-foreground">Location</h3>
                  <button className="text-xs text-cyan-400 hover:underline">Change</button>
                </div>
                <div className="flex items-start gap-3">
                  <MapPin className="w-4 h-4 text-amber-400 mt-0.5" />
                  <span className="text-sm text-foreground">{lot.locationPath}</span>
                </div>
              </div>

              {/* COA Status */}
              <div className="bg-surface border border-border rounded-xl p-5">
                <div className="flex items-center justify-between mb-3">
                  <h3 className="text-sm font-semibold text-foreground">Lab Testing</h3>
                  <span className={cn(
                    'px-2 py-0.5 rounded text-xs font-medium',
                    lot.coaStatus === 'passed' && 'bg-emerald-500/10 text-emerald-400',
                    lot.coaStatus === 'failed' && 'bg-rose-500/10 text-rose-400',
                    lot.coaStatus === 'pending' && 'bg-amber-500/10 text-amber-400'
                  )}>
                    {lot.coaStatus?.toUpperCase()}
                  </span>
                </div>
                <button className="flex items-center gap-2 text-sm text-cyan-400 hover:underline">
                  <FileText className="w-4 h-4" />
                  View COA
                </button>
              </div>

              {/* Compliance IDs */}
              <div className="bg-surface border border-border rounded-xl p-5">
                <h3 className="text-sm font-semibold text-foreground mb-3">Compliance IDs</h3>
                <div className="space-y-3">
                  {lot.metrcId && (
                    <div className="flex items-center justify-between">
                      <span className="text-xs text-muted-foreground">METRC ID</span>
                      <div className="flex items-center gap-1">
                        <span className="text-xs font-mono text-emerald-400">{lot.metrcId}</span>
                        <ExternalLink className="w-3 h-3 text-muted-foreground" />
                      </div>
                    </div>
                  )}
                  {lot.biotrackId && (
                    <div className="flex items-center justify-between">
                      <span className="text-xs text-muted-foreground">BioTrack ID</span>
                      <div className="flex items-center gap-1">
                        <span className="text-xs font-mono text-blue-400">{lot.biotrackId}</span>
                        <ExternalLink className="w-3 h-3 text-muted-foreground" />
                      </div>
                    </div>
                  )}
                  {!lot.metrcId && !lot.biotrackId && (
                    <p className="text-xs text-muted-foreground">No compliance IDs assigned</p>
                  )}
                </div>
              </div>

              {/* Audit Info */}
              <div className="bg-surface border border-border rounded-xl p-5">
                <h3 className="text-sm font-semibold text-foreground mb-3">Audit</h3>
                <div className="space-y-3 text-xs">
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">Created</span>
                    <span className="text-foreground">{formatDateTime(lot.createdAt)} by {lot.createdBy}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">Updated</span>
                    <span className="text-foreground">{formatDateTime(lot.updatedAt)} by {lot.updatedBy}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        )}

        {activeTab === 'movements' && (
          <div className="bg-surface border border-border rounded-xl overflow-hidden">
            <table className="w-full">
              <thead>
                <tr className="border-b border-border bg-muted/30">
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">Type</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">From</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">To</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">Quantity</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">Date</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">User</th>
                </tr>
              </thead>
              <tbody>
                {MOCK_MOVEMENTS.map((m) => (
                  <tr key={m.id} className="border-b border-border hover:bg-muted/30">
                    <td className="px-4 py-3">
                      <span className="text-sm text-foreground capitalize">{m.type}</span>
                    </td>
                    <td className="px-4 py-3">
                      <span className="text-sm text-muted-foreground">{m.from}</span>
                    </td>
                    <td className="px-4 py-3">
                      <span className="text-sm text-muted-foreground">{m.to}</span>
                    </td>
                    <td className="px-4 py-3">
                      <span className={cn('text-sm tabular-nums', m.quantity < 0 ? 'text-rose-400' : 'text-foreground')}>
                        {m.quantity > 0 ? '+' : ''}{m.quantity} g
                      </span>
                    </td>
                    <td className="px-4 py-3">
                      <span className="text-sm text-muted-foreground">{formatDateTime(m.date)}</span>
                    </td>
                    <td className="px-4 py-3">
                      <span className="text-sm text-muted-foreground">{m.user}</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {activeTab === 'lineage' && (
          <div className="bg-surface border border-border rounded-xl p-5">
            <h3 className="text-sm font-semibold text-foreground mb-4">Lot Lineage</h3>
            <div className="space-y-4">
              {MOCK_LINEAGE.map((rel) => (
                <div key={rel.id} className="flex items-center gap-4 p-3 rounded-lg bg-muted/30 border border-border">
                  <GitBranch className="w-5 h-5 text-cyan-400" />
                  <div className="flex-1">
                    <div className="text-sm font-mono text-foreground">{rel.lotNumber}</div>
                    <div className="text-xs text-muted-foreground capitalize">{rel.relationship.replace('_', ' ')}</div>
                  </div>
                  <div className="text-sm text-muted-foreground tabular-nums">{rel.quantity} g</div>
                  <button className="text-xs text-cyan-400 hover:underline">View</button>
                </div>
              ))}
            </div>
          </div>
        )}

        {activeTab === 'lab' && (
          <div className="bg-surface border border-border rounded-xl p-5 text-center py-12">
            <Beaker className="w-12 h-12 text-muted-foreground mx-auto mb-3" />
            <p className="text-sm text-muted-foreground">Lab results will be displayed here</p>
            <button className="mt-4 text-xs text-cyan-400 hover:underline">View Full COA →</button>
          </div>
        )}

        {activeTab === 'compliance' && (
          <div className="bg-surface border border-border rounded-xl p-5 text-center py-12">
            <Shield className="w-12 h-12 text-muted-foreground mx-auto mb-3" />
            <p className="text-sm text-muted-foreground">Compliance sync history will be displayed here</p>
          </div>
        )}
      </div>
    </div>
  );
}
