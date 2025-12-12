'use client';

import Link from 'next/link';
import { useEffect, useMemo, useState } from 'react';
import { Search, Package, Truck, ArrowRight } from 'lucide-react';
import { useAuthStore } from '@/stores/auth/authStore';
import { createShipmentFromAllocations, listShipments } from '@/features/sales/services/shipments.service';
import type { ShipmentDto } from '@/features/sales/types/shipments.types';
import {
  Card,
  CardHeader,
  EmptyState,
  LoadingState,
  DemoModeBanner,
  StatusBadge,
} from '@/features/sales/components/shared';
import { IdCopyCell } from '@/features/sales/components/shared/IdDisplay';

// Demo mode mock data
const DEMO_SHIPMENTS: ShipmentDto[] = [
  {
    id: 'demo-shipment-001',
    siteId: 'demo-site',
    salesOrderId: 'demo-order-001',
    shipmentNumber: 'SH-2024-001',
    status: 'Packed',
    pickingStartedAt: new Date(Date.now() - 3600000).toISOString(),
    packedAt: new Date(Date.now() - 1800000).toISOString(),
    shippedAt: null,
    packages: [],
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: 'demo-shipment-002',
    siteId: 'demo-site',
    salesOrderId: 'demo-order-002',
    shipmentNumber: 'SH-2024-002',
    status: 'Picking',
    pickingStartedAt: new Date().toISOString(),
    packedAt: null,
    shippedAt: null,
    packages: [],
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: 'demo-shipment-003',
    siteId: 'demo-site',
    salesOrderId: 'demo-order-003',
    shipmentNumber: 'SH-2024-003',
    status: 'Shipped',
    pickingStartedAt: new Date(Date.now() - 86400000).toISOString(),
    packedAt: new Date(Date.now() - 86400000 + 3600000).toISOString(),
    shippedAt: new Date(Date.now() - 86400000 + 7200000).toISOString(),
    packages: [],
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
];

function getNextAction(status: string): { label: string } | null {
  switch (status) {
    case 'Picking':
      return { label: 'Pack Shipment' };
    case 'Packed':
      return { label: 'Create Transfer' };
    default:
      return null;
  }
}

function formatTimestamp(ts: string | null): string {
  if (!ts) return '—';
  return new Date(ts).toLocaleString('en-US', {
    month: 'short',
    day: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
  });
}

export default function ShipmentsPage() {
  const siteId = useAuthStore((s) => s.currentSiteId);

  const [shipments, setShipments] = useState<ShipmentDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isDemoMode, setIsDemoMode] = useState(false);

  const [status, setStatus] = useState('');
  const [search, setSearch] = useState('');

  const [createForm, setCreateForm] = useState({
    salesOrderId: '',
    shipmentNumber: '',
    notes: '',
  });

  const queryKey = useMemo(
    () => `${siteId ?? 'none'}|${status}`,
    [siteId, status]
  );

  useEffect(() => {
    if (!siteId) return;
    let cancelled = false;
    setIsLoading(true);
    setError(null);
    setIsDemoMode(false);
    listShipments(siteId, {
      page: 1,
      pageSize: 50,
      status: status || undefined,
    })
      .then((res) => {
        if (cancelled) return;
        setShipments(res.shipments ?? []);
      })
      .catch((e) => {
        if (cancelled) return;
        const message = e instanceof Error ? e.message : 'Failed to load shipments';
        if (message.includes('403') || message.includes('Forbidden') || message.includes('Failed to fetch')) {
          setIsDemoMode(true);
          setShipments(DEMO_SHIPMENTS);
          setError(null);
        } else {
          setError(message);
        }
      })
      .finally(() => {
        if (cancelled) return;
        setIsLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [queryKey, siteId, status]);

  async function onCreateShipment() {
    if (!siteId) return;
    if (!createForm.salesOrderId.trim() || !createForm.shipmentNumber.trim()) {
      setError('SalesOrderId and ShipmentNumber are required.');
      return;
    }
    setIsLoading(true);
    setError(null);
    try {
      const created = await createShipmentFromAllocations(siteId, createForm.salesOrderId.trim(), {
        shipmentNumber: createForm.shipmentNumber.trim(),
        notes: createForm.notes?.trim() || undefined,
      });
      setCreateForm({ salesOrderId: '', shipmentNumber: '', notes: '' });
      window.location.href = `/sales/shipments/${created.id}`;
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to create shipment');
    } finally {
      setIsLoading(false);
    }
  }

  const filteredShipments = shipments.filter(
    (s) =>
      s.shipmentNumber.toLowerCase().includes(search.toLowerCase()) ||
      s.salesOrderId.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="p-6 space-y-6">
      {/* Demo Mode Banner */}
      {isDemoMode && <DemoModeBanner />}

      {/* Error */}
      {error && (
        <div className="bg-rose-500/10 border border-rose-500/30 rounded-xl p-3 text-sm text-rose-200">
          {error}
        </div>
      )}

      {/* Create Shipment Card */}
      <Card>
        <CardHeader
          title="Create Shipment"
          subtitle="Create a new shipment from allocated orders"
        />
        <div className="grid grid-cols-1 md:grid-cols-4 gap-3">
          <input
            value={createForm.salesOrderId}
            onChange={(e) => setCreateForm((p) => ({ ...p, salesOrderId: e.target.value }))}
            placeholder="Sales Order ID"
            className="h-10 px-3 rounded-lg bg-muted/30 border border-border text-sm text-foreground focus:outline-none focus:border-amber-500/30"
          />
          <input
            value={createForm.shipmentNumber}
            onChange={(e) => setCreateForm((p) => ({ ...p, shipmentNumber: e.target.value }))}
            placeholder="Shipment Number (e.g., SH-2024-004)"
            className="h-10 px-3 rounded-lg bg-muted/30 border border-border text-sm text-foreground focus:outline-none focus:border-amber-500/30"
          />
          <input
            value={createForm.notes}
            onChange={(e) => setCreateForm((p) => ({ ...p, notes: e.target.value }))}
            placeholder="Notes (optional)"
            className="h-10 px-3 rounded-lg bg-muted/30 border border-border text-sm text-foreground focus:outline-none focus:border-amber-500/30"
          />
          <button
            type="button"
            onClick={onCreateShipment}
            disabled={isLoading || isDemoMode}
            className="h-10 px-4 rounded-lg bg-amber-600 hover:bg-amber-500 disabled:opacity-60 text-white text-sm font-medium transition-colors"
          >
            {isDemoMode ? 'Create (Demo)' : 'Create Shipment'}
          </button>
        </div>
      </Card>

      {/* Filters */}
      <div className="flex flex-col md:flex-row md:items-center gap-3">
        <div className="relative flex-1 max-w-md">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
          <input
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Search shipments..."
            className="w-full pl-10 pr-4 py-2.5 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-amber-500/30"
          />
        </div>
        <select
          value={status}
          onChange={(e) => setStatus(e.target.value)}
          aria-label="Filter by status"
          className="h-10 px-3 rounded-lg bg-muted/30 border border-border text-sm text-foreground focus:outline-none focus:border-amber-500/30"
        >
          <option value="">All statuses</option>
          <option value="Picking">Picking</option>
          <option value="Packed">Packed</option>
          <option value="Shipped">Shipped</option>
          <option value="Cancelled">Cancelled</option>
        </select>
      </div>

      {/* Shipments List */}
      <Card padding="none">
        {isLoading ? (
          <LoadingState message="Loading shipments..." />
        ) : filteredShipments.length === 0 ? (
          <EmptyState
            icon={Package}
            title="No shipments found"
            description="Create a shipment from an allocated order to get started"
          />
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="text-xs text-muted-foreground bg-muted/30">
                <tr className="border-b border-border">
                  <th className="text-left py-3 px-4 font-medium">Shipment</th>
                  <th className="text-left py-3 px-4 font-medium">Order</th>
                  <th className="text-left py-3 px-4 font-medium">Status</th>
                  <th className="text-left py-3 px-4 font-medium">Picked</th>
                  <th className="text-left py-3 px-4 font-medium">Packed</th>
                  <th className="text-left py-3 px-4 font-medium">Shipped</th>
                  <th className="text-left py-3 px-4 font-medium">Next Action</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {filteredShipments.map((s) => {
                  const nextAction = getNextAction(s.status);
                  return (
                    <tr key={s.id} className="hover:bg-muted/20 transition-colors">
                      <td className="py-3 px-4">
                        <Link
                          href={`/sales/shipments/${s.id}`}
                          className="text-amber-400 hover:text-amber-300 font-medium"
                        >
                          {s.shipmentNumber}
                        </Link>
                      </td>
                      <td className="py-3 px-4">
                        <IdCopyCell id={s.salesOrderId} />
                      </td>
                      <td className="py-3 px-4">
                        <StatusBadge status={s.status} />
                      </td>
                      <td className="py-3 px-4 text-muted-foreground text-xs">
                        {formatTimestamp(s.pickingStartedAt)}
                      </td>
                      <td className="py-3 px-4 text-muted-foreground text-xs">
                        {formatTimestamp(s.packedAt)}
                      </td>
                      <td className="py-3 px-4 text-muted-foreground text-xs">
                        {formatTimestamp(s.shippedAt)}
                      </td>
                      <td className="py-3 px-4">
                        {nextAction ? (
                          <Link
                            href={`/sales/shipments/${s.id}`}
                            className="inline-flex items-center gap-1 text-xs text-amber-400 hover:text-amber-300 font-medium"
                          >
                            {nextAction.label}
                            <ArrowRight className="w-3 h-3" />
                          </Link>
                        ) : s.status === 'Shipped' ? (
                          <span className="inline-flex items-center gap-1 text-xs text-emerald-400">
                            <Truck className="w-3 h-3" />
                            Complete
                          </span>
                        ) : (
                          <span className="text-xs text-muted-foreground">—</span>
                        )}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </Card>
    </div>
  );
}
