'use client';

import { useEffect, useMemo, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuthStore } from '@/stores/auth/authStore';
import {
  addSalesOrderLine,
  allocate,
  cancelSalesOrder,
  getAllocations,
  getSalesOrder,
  submitSalesOrder,
  unallocate,
} from '@/features/sales/services/salesOrders.service';
import { createShipmentFromAllocations } from '@/features/sales/services/shipments.service';
import { usePermissions } from '@/providers/PermissionsProvider';
import { Truck } from 'lucide-react';
import type { SalesOrderDto } from '@/features/sales/types/salesOrders.types';
import type {
  AllocateSalesOrderRequest,
  SalesAllocationDto,
  UnallocateSalesOrderRequest,
} from '@/features/sales/types/allocations.types';
import { SectionCard } from './ui/SectionCard';
import { OrderOverview } from './views/OrderOverview';
import { AddLineForm } from './views/AddLineForm';
import { LinesTable } from './views/LinesTable';
import { AllocationsPanel } from './views/AllocationsPanel';
import { CompliancePanel } from './views/CompliancePanel';

export function SalesOrderDetailView({ salesOrderId }: { salesOrderId: string }) {
  const router = useRouter();
  const siteId = useAuthStore((s) => s.currentSiteId);
  const permissions = usePermissions();

  const [order, setOrder] = useState<SalesOrderDto | null>(null);
  const [allocations, setAllocations] = useState<SalesAllocationDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Check if we have active allocations that can be shipped
  const activeAllocations = useMemo(
    () => allocations.filter((a) => !a.isCancelled),
    [allocations]
  );
  const canCreateShipment = activeAllocations.length > 0 && order?.status !== 'Shipped' && order?.status !== 'Cancelled';

  const refreshKey = useMemo(
    () => `${siteId ?? 'none'}|${salesOrderId}`,
    [siteId, salesOrderId]
  );

  useEffect(() => {
    if (!siteId) return;

    let cancelled = false;
    setIsLoading(true);
    setError(null);

    Promise.all([getSalesOrder(siteId, salesOrderId), getAllocations(siteId, salesOrderId)])
      .then(([o, a]) => {
        if (cancelled) return;
        setOrder(o);
        setAllocations(a ?? []);
      })
      .catch((e) => {
        if (cancelled) return;
        setError(e instanceof Error ? e.message : 'Failed to load sales order');
      })
      .finally(() => {
        if (cancelled) return;
        setIsLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [refreshKey, siteId, salesOrderId]);

  async function refreshOrderAndAllocations() {
    if (!siteId) return;
    const [o, a] = await Promise.all([getSalesOrder(siteId, salesOrderId), getAllocations(siteId, salesOrderId)]);
    setOrder(o);
    setAllocations(a ?? []);
  }

  async function handleSubmit() {
    if (!siteId || !order) return;
    setIsLoading(true);
    setError(null);
    try {
      const updated = await submitSalesOrder(siteId, order.id);
      setOrder(updated);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to submit order');
    } finally {
      setIsLoading(false);
    }
  }

  async function handleCancel(reason: string) {
    if (!siteId || !order) return;
    setIsLoading(true);
    setError(null);
    try {
      const updated = await cancelSalesOrder(siteId, order.id, { reason });
      setOrder(updated);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to cancel order');
    } finally {
      setIsLoading(false);
    }
  }

  async function handleAddLine(input: {
    itemId: string;
    itemName: string;
    requestedQuantity: number;
    unitOfMeasure: string;
    unitPrice?: number;
  }) {
    if (!siteId || !order) return;
    setIsLoading(true);
    setError(null);
    try {
      const nextLineNumber = (order.lines?.length ?? 0) + 1;
      const updated = await addSalesOrderLine(siteId, order.id, {
        lineNumber: nextLineNumber,
        itemId: input.itemId,
        itemName: input.itemName,
        requestedQuantity: input.requestedQuantity,
        unitOfMeasure: input.unitOfMeasure,
        unitPrice: input.unitPrice,
        currencyCode: 'USD',
      });
      setOrder(updated);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to add line');
    } finally {
      setIsLoading(false);
    }
  }

  async function handleAllocate(request: AllocateSalesOrderRequest) {
    if (!siteId || !order) return;
    setIsLoading(true);
    setError(null);
    try {
      const updatedAllocations = await allocate(siteId, order.id, request);
      setAllocations(updatedAllocations ?? []);
      const updatedOrder = await getSalesOrder(siteId, order.id);
      setOrder(updatedOrder);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to allocate');
    } finally {
      setIsLoading(false);
    }
  }

  async function handleUnallocate(request: UnallocateSalesOrderRequest) {
    if (!siteId || !order) return;
    setIsLoading(true);
    setError(null);
    try {
      const updatedAllocations = await unallocate(siteId, order.id, request);
      setAllocations(updatedAllocations ?? []);
      const updatedOrder = await getSalesOrder(siteId, order.id);
      setOrder(updatedOrder);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to unallocate');
    } finally {
      setIsLoading(false);
    }
  }

  async function handleCreateShipment() {
    if (!siteId || !order) return;
    if (!permissions.has('sales:shipments:create')) {
      setError('You do not have permission to create shipments.');
      return;
    }
    if (activeAllocations.length === 0) {
      setError('No allocations to ship. Allocate inventory first.');
      return;
    }

    setIsLoading(true);
    setError(null);
    try {
      // Generate shipment number from order number
      const shipmentNumber = `SH-${order.orderNumber}-001`;
      const shipment = await createShipmentFromAllocations(siteId, order.id, {
        shipmentNumber,
        notes: `Shipment for order ${order.orderNumber}`,
      });
      // Navigate to shipment detail
      router.push(`/sales/shipments/${shipment.id}`);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to create shipment');
      setIsLoading(false);
    }
  }

  return (
    <div className="p-6 space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-semibold text-foreground">Sales Order</h1>
          <p className="text-sm text-muted-foreground">{salesOrderId}</p>
        </div>
        <button
          type="button"
          onClick={() => router.push('/sales/orders')}
          className="inline-flex items-center h-10 px-4 rounded-lg bg-muted hover:bg-muted/80 text-foreground text-sm font-medium transition-colors"
        >
          Back
        </button>
      </div>

      {!siteId && (
        <div className="bg-rose-500/10 border border-rose-500/30 rounded-xl p-3 text-sm text-rose-200">
          No site selected.
        </div>
      )}
      {error && (
        <div className="bg-rose-500/10 border border-rose-500/30 rounded-xl p-3 text-sm text-rose-200">
          {error}
        </div>
      )}
      {isLoading && <div className="text-sm text-muted-foreground">Loading…</div>}

      {order && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
          <div className="lg:col-span-2 space-y-4">
            <SectionCard title="Overview">
              <OrderOverview
                order={order}
                disabled={isLoading}
                onSubmit={handleSubmit}
                onCancel={handleCancel}
              />
            </SectionCard>

            {order.status === 'Draft' && (
              <SectionCard title="Add Line">
                <AddLineForm siteId={siteId} disabled={isLoading} onAddLine={handleAddLine} />
              </SectionCard>
            )}
          </div>

          <div className="space-y-4">
            <SectionCard title="Compliance">
              <CompliancePanel order={order} />
            </SectionCard>
          </div>
        </div>
      )}

      {order && (
        <SectionCard title="Lines">
          <LinesTable lines={order.lines} />
        </SectionCard>
      )}

      {order && siteId && (
        <SectionCard title="Allocations">
          <AllocationsPanel
            siteId={siteId}
            salesOrder={order}
            allocations={allocations}
            disabled={isLoading}
            onAllocate={handleAllocate}
            onUnallocate={handleUnallocate}
            onRefresh={refreshOrderAndAllocations}
          />
        </SectionCard>
      )}

      {/* Quick Actions - Create Shipment */}
      {order && siteId && canCreateShipment && (
        <div className="bg-gradient-to-r from-violet-500/10 to-cyan-500/10 border border-violet-500/30 rounded-xl p-4">
          <div className="flex items-center justify-between">
            <div>
              <h3 className="text-sm font-medium text-foreground flex items-center gap-2">
                <Truck className="w-4 h-4 text-violet-400" />
                Ready to Ship
              </h3>
              <p className="text-xs text-muted-foreground mt-1">
                {activeAllocations.length} package(s) allocated and ready for shipment
              </p>
            </div>
            <button
              type="button"
              onClick={handleCreateShipment}
              disabled={isLoading || !permissions.has('sales:shipments:create')}
              className="inline-flex items-center gap-2 h-10 px-4 rounded-lg bg-violet-600 hover:bg-violet-500 disabled:opacity-60 text-white text-sm font-medium transition-colors"
            >
              <Truck className="w-4 h-4" />
              Create Shipment
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

