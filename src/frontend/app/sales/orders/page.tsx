'use client';

import Link from 'next/link';
import { useEffect, useMemo, useState } from 'react';
import { Search, FileText, Plus, Package, ArrowRight } from 'lucide-react';
import { useAuthStore } from '@/stores/auth/authStore';
import { listSalesOrders } from '@/features/sales/services/salesOrders.service';
import type { SalesOrderDto } from '@/features/sales/types/salesOrders.types';
import { usePermissions } from '@/providers/PermissionsProvider';
import {
  Card,
  EmptyState,
  LoadingState,
  DemoModeBanner,
  StatusBadge,
  ComplianceBadge,
} from '@/features/sales/components/shared';

// Demo mode mock data - used when backend is unavailable
const DEMO_ORDERS: SalesOrderDto[] = [
  {
    id: 'demo-order-001',
    siteId: 'demo-site',
    orderNumber: 'SO-2024-001',
    customerName: 'Green Valley Dispensary',
    destinationLicenseNumber: 'LIC-2024-001',
    destinationFacilityName: 'Green Valley Retail',
    status: 'Submitted',
    requestedShipDate: new Date().toISOString().split('T')[0],
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: 'demo-order-002',
    siteId: 'demo-site',
    orderNumber: 'SO-2024-002',
    customerName: 'Mountain Top Cannabis',
    destinationLicenseNumber: 'LIC-2024-002',
    destinationFacilityName: 'Mountain Top Retail',
    status: 'Draft',
    requestedShipDate: null,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: 'demo-order-003',
    siteId: 'demo-site',
    orderNumber: 'SO-2024-003',
    customerName: 'Coastal Wellness',
    destinationLicenseNumber: 'LIC-2024-003',
    destinationFacilityName: 'Coastal Wellness Center',
    status: 'Allocated',
    requestedShipDate: new Date(Date.now() + 86400000).toISOString().split('T')[0],
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
];

// Next action helper for workflow guidance
function getNextAction(status: string): { label: string; href?: string } | null {
  switch (status) {
    case 'Draft':
      return { label: 'Submit Order' };
    case 'Submitted':
      return { label: 'Allocate' };
    case 'Allocated':
      return { label: 'Create Shipment' };
    default:
      return null;
  }
}

export default function SalesOrdersPage() {
  const siteId = useAuthStore((s) => s.currentSiteId);
  const permissions = usePermissions();

  const [orders, setOrders] = useState<SalesOrderDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isDemoMode, setIsDemoMode] = useState(false);

  const [search, setSearch] = useState('');
  const [status, setStatus] = useState<string>('');

  const queryKey = useMemo(() => `${siteId ?? 'none'}|${search}|${status}`, [siteId, search, status]);

  useEffect(() => {
    if (!siteId) return;

    let cancelled = false;
    setIsLoading(true);
    setError(null);
    setIsDemoMode(false);

    listSalesOrders(siteId, { page: 1, pageSize: 50, search: search || undefined, status: status || undefined })
      .then((res) => {
        if (cancelled) return;
        setOrders(res.orders ?? []);
      })
      .catch((e) => {
        if (cancelled) return;
        const message = e instanceof Error ? e.message : 'Failed to load sales orders';
        // Enable demo mode if backend is unavailable (403 or connection errors)
        if (message.includes('403') || message.includes('Forbidden') || message.includes('Failed to fetch')) {
          setIsDemoMode(true);
          setOrders(DEMO_ORDERS);
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
  }, [queryKey, siteId, search, status]);

  return (
    <div className="p-6 space-y-6">
      {/* Demo Mode Banner */}
      {isDemoMode && <DemoModeBanner />}

      {/* Filters */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div className="flex items-center gap-3 flex-1">
          <div className="relative flex-1 max-w-md">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
            <input
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search order #, customer, destination..."
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
            <option value="Draft">Draft</option>
            <option value="Submitted">Submitted</option>
            <option value="Allocated">Allocated</option>
            <option value="Shipped">Shipped</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        </div>

        {permissions.has('sales:orders:create') && (
          <Link
            href="/sales/orders/new"
            className="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-amber-500/10 text-amber-400 hover:bg-amber-500/20 transition-colors"
          >
            <Plus className="w-4 h-4" />
            <span className="text-sm font-medium">New Order</span>
          </Link>
        )}
      </div>

      {/* Orders List */}
      <Card padding="none">
        {!siteId ? (
          <div className="p-8 text-center text-rose-300">
            No site selected. Select a site to load orders.
          </div>
        ) : error ? (
          <div className="p-8 text-center text-rose-300">{error}</div>
        ) : isLoading ? (
          <LoadingState message="Loading orders..." />
        ) : orders.length === 0 ? (
          <EmptyState
            icon={FileText}
            title="No orders found"
            description={search ? 'Try adjusting your search criteria' : 'Create your first order to get started'}
            action={
              permissions.has('sales:orders:create')
                ? { label: 'New Order', onClick: () => (window.location.href = '/sales/orders/new') }
                : undefined
            }
          />
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="text-xs text-muted-foreground bg-muted/30">
                <tr className="border-b border-border">
                  <th className="text-left py-3 px-4 font-medium">Order</th>
                  <th className="text-left py-3 px-4 font-medium">Customer</th>
                  <th className="text-left py-3 px-4 font-medium">Destination</th>
                  <th className="text-left py-3 px-4 font-medium">Status</th>
                  <th className="text-left py-3 px-4 font-medium">Ship Date</th>
                  <th className="text-left py-3 px-4 font-medium">Next Action</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {orders.map((o) => {
                  const nextAction = getNextAction(o.status);
                  return (
                    <tr key={o.id} className="hover:bg-muted/20 transition-colors">
                      <td className="py-3 px-4">
                        <Link
                          href={`/sales/orders/${o.id}`}
                          className="text-amber-400 hover:text-amber-300 font-medium"
                        >
                          {o.orderNumber}
                        </Link>
                      </td>
                      <td className="py-3 px-4 font-medium text-foreground">
                        {o.customerName}
                      </td>
                      <td className="py-3 px-4">
                        <div className="flex items-center gap-2">
                          <div>
                            <div className="text-foreground">{o.destinationFacilityName ?? '—'}</div>
                            <div className="text-xs text-muted-foreground">
                              {o.destinationLicenseNumber ?? '—'}
                            </div>
                          </div>
                          <ComplianceBadge status="Verified" showLabel={false} />
                        </div>
                      </td>
                      <td className="py-3 px-4">
                        <StatusBadge status={o.status} />
                      </td>
                      <td className="py-3 px-4 text-muted-foreground">
                        {o.requestedShipDate ?? '—'}
                      </td>
                      <td className="py-3 px-4">
                        {nextAction ? (
                          <Link
                            href={`/sales/orders/${o.id}`}
                            className="inline-flex items-center gap-1 text-xs text-amber-400 hover:text-amber-300 font-medium"
                          >
                            {nextAction.label}
                            <ArrowRight className="w-3 h-3" />
                          </Link>
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

