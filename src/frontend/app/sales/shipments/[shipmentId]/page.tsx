'use client';

import { useEffect, useMemo, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { useAuthStore } from '@/stores/auth/authStore';
import {
  cancelShipment,
  getShipment,
  markPacked,
  markShipped,
  startPicking,
} from '@/features/sales/services/shipments.service';
import { createOutboundTransferFromShipment } from '@/features/transfers/services/outboundTransfers.service';
import type { ShipmentDto } from '@/features/sales/types/shipments.types';
import { ScannerInput } from '@/features/fulfillment/components/ScannerInput';
import { useShipmentPicking } from '@/features/fulfillment/hooks/useShipmentPicking';
import { usePermissions } from '@/providers/PermissionsProvider';
import { Truck } from 'lucide-react';

export default function ShipmentDetailPage() {
  const router = useRouter();
  const params = useParams<{ shipmentId: string }>();
  const shipmentId = params.shipmentId;
  const siteId = useAuthStore((s) => s.currentSiteId);
  const permissions = usePermissions();

  const [shipment, setShipment] = useState<ShipmentDto | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isBusy, setIsBusy] = useState(false);

  const [shipForm, setShipForm] = useState({
    carrierName: '',
    trackingNumber: '',
    outboundTransferId: '',
  });

  const refreshKey = useMemo(() => `${siteId ?? 'none'}|${shipmentId}`, [siteId, shipmentId]);

  useEffect(() => {
    if (!siteId) return;
    let cancelled = false;
    setIsBusy(true);
    setError(null);
    getShipment(siteId, shipmentId)
      .then((s) => {
        if (cancelled) return;
        setShipment(s);
      })
      .catch((e) => {
        if (cancelled) return;
        setError(e instanceof Error ? e.message : 'Failed to load shipment');
      })
      .finally(() => {
        if (cancelled) return;
        setIsBusy(false);
      });

    return () => {
      cancelled = true;
    };
  }, [refreshKey, siteId, shipmentId]);

  const picking = useShipmentPicking(shipment?.packages ?? []);

  async function refresh() {
    if (!siteId) return;
    const s = await getShipment(siteId, shipmentId);
    setShipment(s);
  }

  async function onStartPicking() {
    if (!permissions.has('sales:shipments:create')) {
      setError('You do not have permission to start picking shipments.');
      return;
    }
    if (!siteId) return;
    setIsBusy(true);
    setError(null);
    try {
      const updated = await startPicking(siteId, shipmentId);
      setShipment(updated);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to start picking');
    } finally {
      setIsBusy(false);
    }
  }

  async function onPack() {
    if (!permissions.has('sales:shipments:pack')) {
      setError('You do not have permission to pack shipments.');
      return;
    }
    if (!siteId) return;
    if (!picking.isPickComplete) {
      const note = prompt('Not all packages scanned. Enter override reason to pack anyway:') ?? '';
      if (!note.trim()) return;
      picking.markManualOverride(note);
    }

    setIsBusy(true);
    setError(null);
    try {
      const updated = await markPacked(siteId, shipmentId);
      setShipment(updated);
      picking.clear();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to mark packed');
    } finally {
      setIsBusy(false);
    }
  }

  async function onShip() {
    if (!permissions.has('sales:shipments:ship')) {
      setError('You do not have permission to ship shipments.');
      return;
    }
    if (!siteId) return;
    setIsBusy(true);
    setError(null);
    try {
      const updated = await markShipped(siteId, shipmentId, {
        carrierName: shipForm.carrierName?.trim() || undefined,
        trackingNumber: shipForm.trackingNumber?.trim() || undefined,
        outboundTransferId: shipForm.outboundTransferId?.trim() || undefined,
      });
      setShipment(updated);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to mark shipped');
    } finally {
      setIsBusy(false);
    }
  }

  async function onCancel() {
    if (!permissions.has('sales:shipments:ship')) {
      setError('You do not have permission to cancel shipments.');
      return;
    }
    if (!siteId) return;
    const reason = prompt('Cancel reason?') ?? '';
    if (!reason.trim()) return;
    setIsBusy(true);
    setError(null);
    try {
      const updated = await cancelShipment(siteId, shipmentId, { reason: reason.trim() });
      setShipment(updated);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to cancel shipment');
    } finally {
      setIsBusy(false);
    }
  }

  async function onCreateTransfer() {
    if (!permissions.has('transfers:create')) {
      setError('You do not have permission to create transfers.');
      return;
    }
    if (!siteId || !shipment) return;
    setIsBusy(true);
    setError(null);
    try {
      const transfer = await createOutboundTransferFromShipment(siteId, {
        shipmentId: shipment.id,
        plannedDepartureAt: undefined,
        plannedArrivalAt: undefined,
      });
      // Navigate to transfer detail
      router.push(`/transfers/outbound/${transfer.id}`);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to create transfer');
      setIsBusy(false);
    }
  }

  // Check if we can create a transfer (shipment is Packed or Shipped)
  const canCreateTransfer = shipment && 
    (shipment.status === 'Packed' || shipment.status === 'Shipped') &&
    shipment.packages.length > 0;

  return (
    <div className="p-6 space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-semibold text-foreground">Shipment</h1>
          <p className="text-sm text-muted-foreground">{shipmentId}</p>
        </div>
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={() => router.push('/sales/shipments')}
            className="inline-flex items-center h-10 px-4 rounded-lg bg-muted hover:bg-muted/80 text-foreground text-sm font-medium transition-colors"
          >
            Back
          </button>
          <button
            type="button"
            onClick={refresh}
            className="inline-flex items-center h-10 px-4 rounded-lg bg-muted hover:bg-muted/80 text-foreground text-sm font-medium transition-colors"
          >
            Refresh
          </button>
        </div>
      </div>

      {error && (
        <div className="bg-rose-500/10 border border-rose-500/30 rounded-xl p-3 text-sm text-rose-200">
          {error}
        </div>
      )}

      {!shipment ? (
        <div className="text-sm text-muted-foreground">{isBusy ? 'Loading…' : 'Not found.'}</div>
      ) : (
        <>
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
            <Card title="Status">
              <div className="space-y-2 text-sm">
                <Row label="Shipment #">{shipment.shipmentNumber}</Row>
                <Row label="Order">{shipment.salesOrderId}</Row>
                <Row label="Status">{shipment.status}</Row>
                <Row label="Picking Started">{shipment.pickingStartedAt ?? '—'}</Row>
                <Row label="Packed">{shipment.packedAt ?? '—'}</Row>
                <Row label="Shipped">{shipment.shippedAt ?? '—'}</Row>
              </div>
              <div className="mt-4 flex flex-wrap items-center gap-2">
                <button
                  type="button"
                  onClick={onStartPicking}
                  disabled={isBusy || !permissions.has('sales:shipments:create')}
                  className="inline-flex items-center h-10 px-4 rounded-lg bg-violet-600 hover:bg-violet-500 disabled:opacity-60 text-white text-sm font-medium transition-colors"
                >
                  Start Picking
                </button>
                <button
                  type="button"
                  onClick={onPack}
                  disabled={isBusy || !permissions.has('sales:shipments:pack')}
                  className="inline-flex items-center h-10 px-4 rounded-lg bg-muted hover:bg-muted/80 disabled:opacity-60 text-foreground text-sm font-medium transition-colors"
                >
                  Mark Packed
                </button>
                <button
                  type="button"
                  onClick={onCancel}
                  disabled={isBusy || !permissions.has('sales:shipments:ship')}
                  className="inline-flex items-center h-10 px-4 rounded-lg bg-rose-600 hover:bg-rose-500 disabled:opacity-60 text-white text-sm font-medium transition-colors"
                >
                  Cancel
                </button>
              </div>
            </Card>

            <Card title="Scanner Pick">
              <ScannerInput disabled={isBusy} onScan={picking.scan} />
              <div className="mt-3 text-sm text-muted-foreground">
                Picked {picking.pickedCount} / {picking.totalCount}
              </div>
              {picking.exceptions.length > 0 && (
                <div className="mt-3 space-y-1 text-xs text-rose-200">
                  {picking.exceptions.slice(-5).map((e, idx) => (
                    <div key={`${e.at}-${idx}`}>{e.message}</div>
                  ))}
                </div>
              )}
            </Card>
          </div>

          <Card title="Packages">
            {shipment.packages.length === 0 ? (
              <div className="text-sm text-muted-foreground">No packages.</div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead className="text-xs text-muted-foreground">
                    <tr className="border-b border-border">
                      <th className="text-left py-2 pr-3">Picked</th>
                      <th className="text-left py-2 pr-3">Label</th>
                      <th className="text-left py-2 pr-3">PackageId</th>
                      <th className="text-left py-2 pr-3">Qty</th>
                      <th className="text-left py-2 pr-3">PackedAt</th>
                    </tr>
                  </thead>
                  <tbody>
                    {shipment.packages.map((p) => {
                      const picked = picking.scannedPackageIds.has(p.packageId);
                      return (
                        <tr key={p.id} className="border-b border-border/60">
                          <td className="py-2 pr-3">
                            <span className={picked ? 'text-emerald-200' : 'text-muted-foreground'}>
                              {picked ? 'Yes' : 'No'}
                            </span>
                          </td>
                          <td className="py-2 pr-3">{p.packageLabel ?? '—'}</td>
                          <td className="py-2 pr-3 text-xs text-muted-foreground">{p.packageId}</td>
                          <td className="py-2 pr-3 text-muted-foreground">
                            {p.quantity} {p.unitOfMeasure}
                          </td>
                          <td className="py-2 pr-3 text-muted-foreground">{p.packedAt ?? '—'}</td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            )}
          </Card>

          <Card title="Ship">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
              <input
                value={shipForm.carrierName}
                onChange={(e) => setShipForm((p) => ({ ...p, carrierName: e.target.value }))}
                placeholder="Carrier (optional)"
                className="h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30"
              />
              <input
                value={shipForm.trackingNumber}
                onChange={(e) => setShipForm((p) => ({ ...p, trackingNumber: e.target.value }))}
                placeholder="Tracking # (optional)"
                className="h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30"
              />
              <input
                value={shipForm.outboundTransferId}
                onChange={(e) => setShipForm((p) => ({ ...p, outboundTransferId: e.target.value }))}
                placeholder="OutboundTransferId (optional)"
                className="h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30"
              />
            </div>
            <div className="mt-3">
              <button
                type="button"
                onClick={onShip}
                disabled={isBusy || !permissions.has('sales:shipments:ship')}
                className="inline-flex items-center h-10 px-4 rounded-lg bg-violet-600 hover:bg-violet-500 disabled:opacity-60 text-white text-sm font-medium transition-colors"
              >
                Mark Shipped
              </button>
            </div>
          </Card>

          {/* Create Transfer Action */}
          {canCreateTransfer && (
            <div className="bg-gradient-to-r from-cyan-500/10 to-violet-500/10 border border-cyan-500/30 rounded-xl p-4">
              <div className="flex items-center justify-between">
                <div>
                  <h3 className="text-sm font-medium text-foreground flex items-center gap-2">
                    <Truck className="w-4 h-4 text-cyan-400" />
                    Ready for Transfer
                  </h3>
                  <p className="text-xs text-muted-foreground mt-1">
                    Create an outbound transfer for METRC compliance
                  </p>
                </div>
                <button
                  type="button"
                  onClick={onCreateTransfer}
                  disabled={isBusy || !permissions.has('transfers:create')}
                  className="inline-flex items-center gap-2 h-10 px-4 rounded-lg bg-cyan-600 hover:bg-cyan-500 disabled:opacity-60 text-white text-sm font-medium transition-colors"
                >
                  <Truck className="w-4 h-4" />
                  Create Transfer
                </button>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
}

function Card({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div className="bg-surface border border-border rounded-xl p-4 space-y-3">
      <div className="text-sm font-medium text-foreground">{title}</div>
      {children}
    </div>
  );
}

function Row({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className="flex items-start justify-between gap-3">
      <div className="text-xs text-muted-foreground">{label}</div>
      <div className="text-sm text-foreground text-right">{children}</div>
    </div>
  );
}

