'use client';

import { useState, useEffect } from 'react';
import { Truck, Search, Filter, ExternalLink, CheckCircle, Clock, AlertTriangle } from 'lucide-react';
import Link from 'next/link';
import {
  Card,
  CardHeader,
  EmptyState,
  DemoModeBanner,
  StatusBadge,
  MetrcBadge,
} from '@/features/sales/components/shared';
import { useAuthStore } from '@/stores/auth/authStore';
import { listOutboundTransfers } from '@/features/transfers/services/outboundTransfers.service';
import type { OutboundTransferDto } from '@/features/transfers/types/outboundTransfers.types';

// Demo transfers data
const DEMO_TRANSFERS: OutboundTransferDto[] = [
  {
    id: 'transfer-001',
    siteId: 'demo-site',
    shipmentId: 'ship-001',
    salesOrderId: 'order-001',
    destinationLicenseNumber: 'LIC-2024-001',
    destinationFacilityName: 'Green Valley Dispensary',
    status: 'Submitted',
    metrcTransferNumber: 'MT-2024-00142',
    metrcSyncStatus: 'Synced',
    plannedDepartureAt: new Date().toISOString(),
    packages: [],
  },
  {
    id: 'transfer-002',
    siteId: 'demo-site',
    shipmentId: 'ship-002',
    salesOrderId: 'order-002',
    destinationLicenseNumber: 'LIC-2024-002',
    destinationFacilityName: 'Mountain Top Cannabis',
    status: 'Ready',
    metrcSyncStatus: 'Pending',
    packages: [],
  },
  {
    id: 'transfer-003',
    siteId: 'demo-site',
    shipmentId: 'ship-003',
    destinationLicenseNumber: 'LIC-2024-003',
    destinationFacilityName: 'Coastal Wellness',
    status: 'Draft',
    metrcSyncStatus: null,
    packages: [],
  },
];

export default function SalesTransfersPage() {
  const siteId = useAuthStore((s) => s.currentSiteId);

  const [isDemoMode, setIsDemoMode] = useState(false);
  const [transfers, setTransfers] = useState<OutboundTransferDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');

  useEffect(() => {
    if (!siteId) return;

    setIsLoading(true);
    listOutboundTransfers(siteId, { page: 1, pageSize: 50, status: statusFilter || undefined })
      .then((res) => {
        setTransfers(res.transfers ?? []);
        setIsDemoMode(false);
      })
      .catch((e) => {
        const message = e instanceof Error ? e.message : '';
        if (message.includes('403') || message.includes('Forbidden') || message.includes('Failed to fetch')) {
          setIsDemoMode(true);
          setTransfers(DEMO_TRANSFERS);
        }
      })
      .finally(() => setIsLoading(false));
  }, [siteId, statusFilter]);

  const filteredTransfers = transfers.filter(
    (t) =>
      t.destinationFacilityName?.toLowerCase().includes(search.toLowerCase()) ||
      t.destinationLicenseNumber.toLowerCase().includes(search.toLowerCase()) ||
      t.metrcTransferNumber?.toLowerCase().includes(search.toLowerCase())
  );

  // Compliance summary
  const syncedCount = transfers.filter((t) => t.metrcSyncStatus === 'Synced').length;
  const pendingCount = transfers.filter((t) => t.metrcSyncStatus === 'Pending').length;
  const failedCount = transfers.filter((t) => t.metrcSyncStatus === 'Failed').length;

  return (
    <div className="p-6 space-y-6">
      {/* Demo Mode Banner */}
      {isDemoMode && <DemoModeBanner />}

      {/* Compliance Summary Strip */}
      <div className="grid grid-cols-3 gap-4">
        <div className="flex items-center gap-3 p-4 rounded-xl bg-emerald-500/10 border border-emerald-500/30">
          <CheckCircle className="w-5 h-5 text-emerald-400" />
          <div>
            <div className="text-2xl font-semibold text-emerald-400 tabular-nums">{syncedCount}</div>
            <div className="text-xs text-emerald-300/70">Synced to METRC</div>
          </div>
        </div>
        <div className="flex items-center gap-3 p-4 rounded-xl bg-amber-500/10 border border-amber-500/30">
          <Clock className="w-5 h-5 text-amber-400" />
          <div>
            <div className="text-2xl font-semibold text-amber-400 tabular-nums">{pendingCount}</div>
            <div className="text-xs text-amber-300/70">Pending Sync</div>
          </div>
        </div>
        <div className="flex items-center gap-3 p-4 rounded-xl bg-rose-500/10 border border-rose-500/30">
          <AlertTriangle className="w-5 h-5 text-rose-400" />
          <div>
            <div className="text-2xl font-semibold text-rose-400 tabular-nums">{failedCount}</div>
            <div className="text-xs text-rose-300/70">Failed</div>
          </div>
        </div>
      </div>

      {/* Filters */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div className="flex items-center gap-3 flex-1">
          <div className="relative flex-1 max-w-md">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
            <input
              type="text"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search by destination, license, METRC #..."
              className="w-full pl-10 pr-4 py-2.5 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-amber-500/30"
            />
          </div>
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            aria-label="Filter by status"
            className="h-10 px-3 rounded-lg bg-muted/30 border border-border text-sm text-foreground focus:outline-none focus:border-amber-500/30"
          >
            <option value="">All statuses</option>
            <option value="Draft">Draft</option>
            <option value="Ready">Ready</option>
            <option value="Submitted">Submitted</option>
            <option value="Voided">Voided</option>
          </select>
        </div>

        <Link
          href="/transfers/outbound"
          className="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-muted/30 border border-border text-muted-foreground hover:text-foreground transition-colors"
        >
          <ExternalLink className="w-4 h-4" />
          <span className="text-sm">Full Transfers Module</span>
        </Link>
      </div>

      {/* Transfers List */}
      <Card padding="none">
        {isLoading ? (
          <div className="p-8 text-center text-muted-foreground">Loading transfers...</div>
        ) : filteredTransfers.length === 0 ? (
          <EmptyState
            icon={Truck}
            title="No transfers found"
            description="Create a transfer from an order or shipment to get started"
          />
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="text-xs text-muted-foreground bg-muted/30">
                <tr className="border-b border-border">
                  <th className="text-left py-3 px-4 font-medium">Transfer</th>
                  <th className="text-left py-3 px-4 font-medium">Destination</th>
                  <th className="text-left py-3 px-4 font-medium">Status</th>
                  <th className="text-left py-3 px-4 font-medium">METRC #</th>
                  <th className="text-left py-3 px-4 font-medium">Sync Status</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {filteredTransfers.map((transfer) => (
                  <tr key={transfer.id} className="hover:bg-muted/20 transition-colors">
                    <td className="py-3 px-4">
                      <Link
                        href={`/transfers/outbound/${transfer.id}`}
                        className="text-amber-400 hover:text-amber-300 font-medium"
                      >
                        {transfer.id.slice(0, 12)}...
                      </Link>
                      {transfer.shipmentId && (
                        <div className="text-xs text-muted-foreground mt-0.5">
                          Shipment: {transfer.shipmentId.slice(0, 8)}...
                        </div>
                      )}
                    </td>
                    <td className="py-3 px-4">
                      <div className="font-medium text-foreground">
                        {transfer.destinationFacilityName ?? '—'}
                      </div>
                      <div className="text-xs text-muted-foreground">
                        {transfer.destinationLicenseNumber}
                      </div>
                    </td>
                    <td className="py-3 px-4">
                      <StatusBadge status={transfer.status} />
                    </td>
                    <td className="py-3 px-4 text-muted-foreground">
                      {transfer.metrcTransferNumber ?? '—'}
                    </td>
                    <td className="py-3 px-4">
                      <MetrcBadge status={transfer.metrcSyncStatus} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>
    </div>
  );
}
