'use client';

import Link from 'next/link';
import { useEffect, useMemo, useState } from 'react';
import { useSearchParams } from 'next/navigation';
import { AlertTriangle } from 'lucide-react';
import { useAuthStore } from '@/stores/auth/authStore';
import {
  createOutboundTransferFromShipment,
  listOutboundTransfers,
} from '@/features/transfers/services/outboundTransfers.service';
import type { OutboundTransferDto } from '@/features/transfers/types/outboundTransfers.types';
import { usePermissions } from '@/providers/PermissionsProvider';

// Demo mode mock data - used when backend is unavailable
const DEMO_TRANSFERS: OutboundTransferDto[] = [
  {
    id: 'demo-transfer-001',
    siteId: 'demo-site',
    shipmentId: 'demo-shipment-001',
    destinationLicenseNumber: 'LIC-2024-001',
    destinationFacilityName: 'Green Valley Dispensary',
    status: 'Ready',
    metrcSyncStatus: 'Pending',
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: 'demo-transfer-002',
    siteId: 'demo-site',
    shipmentId: 'demo-shipment-002',
    destinationLicenseNumber: 'LIC-2024-002',
    destinationFacilityName: 'Mountain Top Cannabis',
    status: 'Draft',
    metrcSyncStatus: null,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
];

export default function OutboundTransfersPage() {
  const searchParams = useSearchParams();
  const prefillShipmentId = searchParams.get('shipmentId');

  const siteId = useAuthStore((s) => s.currentSiteId);
  const permissions = usePermissions();

  const [transfers, setTransfers] = useState<OutboundTransferDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [status, setStatus] = useState('');
  const [isDemoMode, setIsDemoMode] = useState(false);

  const [createForm, setCreateForm] = useState({
    shipmentId: prefillShipmentId ?? '',
    plannedDepartureAt: '',
    plannedArrivalAt: '',
  });

  // Update form when URL params change
  useEffect(() => {
    if (prefillShipmentId && prefillShipmentId !== createForm.shipmentId) {
      setCreateForm((prev) => ({ ...prev, shipmentId: prefillShipmentId }));
    }
  }, [prefillShipmentId, createForm.shipmentId]);

  const queryKey = useMemo(() => `${siteId ?? 'none'}|${status}`, [siteId, status]);

  useEffect(() => {
    if (!siteId) return;
    let cancelled = false;
    setIsLoading(true);
    setError(null);
    setIsDemoMode(false);
    listOutboundTransfers(siteId, { page: 1, pageSize: 50, status: status || undefined })
      .then((res) => {
        if (cancelled) return;
        setTransfers(res.transfers ?? []);
      })
      .catch((e) => {
        if (cancelled) return;
        const message = e instanceof Error ? e.message : 'Failed to load outbound transfers';
        // Enable demo mode if backend is unavailable (403 or connection errors)
        if (message.includes('403') || message.includes('Forbidden') || message.includes('Failed to fetch')) {
          setIsDemoMode(true);
          setTransfers(DEMO_TRANSFERS);
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

  async function onCreateFromShipment() {
    if (!siteId) return;
    if (!permissions.has('transfers:create')) {
      setError('You do not have permission to create transfers.');
      return;
    }
    if (!createForm.shipmentId.trim()) {
      setError('ShipmentId is required.');
      return;
    }
    setIsLoading(true);
    setError(null);
    try {
      const created = await createOutboundTransferFromShipment(siteId, {
        shipmentId: createForm.shipmentId.trim(),
        plannedDepartureAt: createForm.plannedDepartureAt?.trim() || undefined,
        plannedArrivalAt: createForm.plannedArrivalAt?.trim() || undefined,
      });
      setCreateForm({ shipmentId: '', plannedDepartureAt: '', plannedArrivalAt: '' });
      window.location.href = `/transfers/outbound/${created.id}`;
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to create transfer');
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <div className="p-6 space-y-4">
      <div>
        <h1 className="text-xl font-semibold text-foreground">Outbound Transfers</h1>
        <p className="text-sm text-muted-foreground">
          Create transfers, manage manifests, and submit to METRC.
        </p>
      </div>

      {isDemoMode && (
        <div className="bg-amber-500/10 border border-amber-500/30 rounded-xl p-3 flex items-center gap-3">
          <AlertTriangle className="w-5 h-5 text-amber-400 flex-shrink-0" />
          <div>
            <div className="text-sm font-medium text-amber-200">Demo Mode</div>
            <div className="text-xs text-amber-300/70">
              Backend unavailable. Showing sample data for demonstration purposes.
            </div>
          </div>
        </div>
      )}

      {error && (
        <div className="bg-rose-500/10 border border-rose-500/30 rounded-xl p-3 text-sm text-rose-200">
          {error}
        </div>
      )}

      <div className={`rounded-xl p-4 space-y-3 ${
        prefillShipmentId 
          ? 'bg-gradient-to-r from-violet-500/10 to-cyan-500/10 border border-violet-500/30' 
          : 'bg-surface border border-border'
      }`}>
        <div className="flex items-center justify-between">
          <div className="text-sm font-medium text-foreground">
            {prefillShipmentId ? 'Create Transfer from Shipment' : 'Create Transfer (from shipment)'}
          </div>
          <Link
            href="/sales/shipments"
            className="text-sm text-violet-300 hover:text-violet-200"
          >
            View Shipments
          </Link>
        </div>

        {prefillShipmentId && (
          <div className="text-xs text-muted-foreground bg-muted/50 rounded-lg px-3 py-2">
            Shipment ID pre-filled from workflow. Review and click Create to continue.
          </div>
        )}

        <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
          <input
            value={createForm.shipmentId}
            onChange={(e) => setCreateForm((p) => ({ ...p, shipmentId: e.target.value }))}
            placeholder="ShipmentId (GUID)"
            className="h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30"
          />
          <input
            value={createForm.plannedDepartureAt}
            onChange={(e) => setCreateForm((p) => ({ ...p, plannedDepartureAt: e.target.value }))}
            placeholder="PlannedDepartureAt (ISO, optional)"
            className="h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30"
          />
          <input
            value={createForm.plannedArrivalAt}
            onChange={(e) => setCreateForm((p) => ({ ...p, plannedArrivalAt: e.target.value }))}
            placeholder="PlannedArrivalAt (ISO, optional)"
            className="h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30"
          />
        </div>
        <button
          type="button"
          onClick={onCreateFromShipment}
          disabled={isLoading || isDemoMode || !permissions.has('transfers:create')}
          className="inline-flex items-center h-10 px-4 rounded-lg bg-violet-600 hover:bg-violet-500 disabled:opacity-60 text-white text-sm font-medium transition-colors"
        >
          {isDemoMode ? 'Create (Demo)' : 'Create'}
        </button>
      </div>

      <div className="bg-surface border border-border rounded-xl p-4 space-y-3">
        <div className="flex flex-col md:flex-row md:items-center gap-3">
          <select
            value={status}
            onChange={(e) => setStatus(e.target.value)}
            aria-label="Filter by status"
            className="h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30"
          >
            <option value="">All statuses</option>
            <option value="Draft">Draft</option>
            <option value="Ready">Ready</option>
            <option value="Submitted">Submitted</option>
            <option value="Voided">Voided</option>
          </select>
        </div>

        {isLoading ? (
          <div className="text-sm text-muted-foreground">Loading…</div>
        ) : transfers.length === 0 ? (
          <div className="text-sm text-muted-foreground">No transfers found.</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="text-xs text-muted-foreground">
                <tr className="border-b border-border">
                  <th className="text-left py-2 pr-3">Transfer</th>
                  <th className="text-left py-2 pr-3">Destination</th>
                  <th className="text-left py-2 pr-3">Status</th>
                  <th className="text-left py-2 pr-3">METRC</th>
                </tr>
              </thead>
              <tbody>
                {transfers.map((t) => (
                  <tr key={t.id} className="border-b border-border/60 hover:bg-muted/30">
                    <td className="py-2 pr-3">
                      <Link
                        href={`/transfers/outbound/${t.id}`}
                        className="text-violet-300 hover:text-violet-200 font-medium"
                      >
                        {t.id}
                      </Link>
                      <div className="text-xs text-muted-foreground">
                        Shipment: {t.shipmentId ?? '—'}
                      </div>
                    </td>
                    <td className="py-2 pr-3">
                      <div className="text-xs text-muted-foreground">{t.destinationLicenseNumber}</div>
                      <div>{t.destinationFacilityName ?? ''}</div>
                    </td>
                    <td className="py-2 pr-3">{t.status}</td>
                    <td className="py-2 pr-3 text-muted-foreground">
                      {t.metrcSyncStatus ?? '—'}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}

