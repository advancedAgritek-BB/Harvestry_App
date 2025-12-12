'use client';

import Link from 'next/link';
import { useEffect, useMemo, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { useAuthStore } from '@/stores/auth/authStore';
import {
  getOutboundTransfer,
  markReady,
  submitToMetrc,
  voidTransfer,
} from '@/features/transfers/services/outboundTransfers.service';
import type { OutboundTransferDto } from '@/features/transfers/types/outboundTransfers.types';
import { usePermissions } from '@/providers/PermissionsProvider';

export default function OutboundTransferDetailPage() {
  const router = useRouter();
  const params = useParams<{ transferId: string }>();
  const transferId = params.transferId;
  const siteId = useAuthStore((s) => s.currentSiteId);
  const permissions = usePermissions();

  const [transfer, setTransfer] = useState<OutboundTransferDto | null>(null);
  const [isBusy, setIsBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [metrcForm, setMetrcForm] = useState({
    metrcSyncJobId: '',
    licenseNumber: '',
    priority: '100',
  });

  const [voidForm, setVoidForm] = useState({
    metrcSyncJobId: '',
    licenseNumber: '',
    reason: '',
  });

  const refreshKey = useMemo(() => `${siteId ?? 'none'}|${transferId}`, [siteId, transferId]);

  useEffect(() => {
    if (!siteId) return;
    let cancelled = false;
    setIsBusy(true);
    setError(null);
    getOutboundTransfer(siteId, transferId)
      .then((t) => {
        if (cancelled) return;
        setTransfer(t);
      })
      .catch((e) => {
        if (cancelled) return;
        setError(e instanceof Error ? e.message : 'Failed to load transfer');
      })
      .finally(() => {
        if (cancelled) return;
        setIsBusy(false);
      });

    return () => {
      cancelled = true;
    };
  }, [refreshKey, siteId, transferId]);

  async function refresh() {
    if (!siteId) return;
    const t = await getOutboundTransfer(siteId, transferId);
    setTransfer(t);
  }

  async function onReady() {
    if (!siteId) return;
    if (!permissions.has('transfers:create')) {
      setError('You do not have permission to mark transfers ready.');
      return;
    }
    setIsBusy(true);
    setError(null);
    try {
      const updated = await markReady(siteId, transferId);
      setTransfer(updated);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to mark ready');
    } finally {
      setIsBusy(false);
    }
  }

  async function onSubmitToMetrc() {
    if (!siteId) return;
    if (!permissions.has('compliance:metrc-submit')) {
      setError('You do not have permission to submit to METRC.');
      return;
    }
    if (!metrcForm.metrcSyncJobId.trim() || !metrcForm.licenseNumber.trim()) {
      setError('MetrcSyncJobId and LicenseNumber are required.');
      return;
    }
    setIsBusy(true);
    setError(null);
    try {
      const updated = await submitToMetrc(siteId, transferId, {
        metrcSyncJobId: metrcForm.metrcSyncJobId.trim(),
        licenseNumber: metrcForm.licenseNumber.trim(),
        priority: Number(metrcForm.priority || '100'),
      });
      setTransfer(updated);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to submit to METRC');
    } finally {
      setIsBusy(false);
    }
  }

  async function onVoid() {
    if (!siteId) return;
    if (!permissions.has('transfers:create')) {
      setError('You do not have permission to void transfers.');
      return;
    }
    if (!voidForm.metrcSyncJobId.trim() || !voidForm.licenseNumber.trim() || !voidForm.reason.trim()) {
      setError('MetrcSyncJobId, LicenseNumber, and Reason are required.');
      return;
    }
    setIsBusy(true);
    setError(null);
    try {
      const updated = await voidTransfer(siteId, transferId, {
        metrcSyncJobId: voidForm.metrcSyncJobId.trim(),
        licenseNumber: voidForm.licenseNumber.trim(),
        reason: voidForm.reason.trim(),
      });
      setTransfer(updated);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to void transfer');
    } finally {
      setIsBusy(false);
    }
  }

  return (
    <div className="p-6 space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-semibold text-foreground">Outbound Transfer</h1>
          <p className="text-sm text-muted-foreground">{transferId}</p>
        </div>
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={() => router.push('/transfers/outbound')}
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

      {!transfer ? (
        <div className="text-sm text-muted-foreground">{isBusy ? 'Loading…' : 'Not found.'}</div>
      ) : (
        <>
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
            <Card title="Overview">
              <div className="space-y-2 text-sm">
                <Row label="Status">{transfer.status}</Row>
                <Row label="Destination License">{transfer.destinationLicenseNumber}</Row>
                <Row label="Facility">{transfer.destinationFacilityName ?? '—'}</Row>
                <Row label="Shipment">{transfer.shipmentId ?? '—'}</Row>
                <Row label="Sales Order">{transfer.salesOrderId ?? '—'}</Row>
                <Row label="Planned Departure">{transfer.plannedDepartureAt ?? '—'}</Row>
                <Row label="Planned Arrival">{transfer.plannedArrivalAt ?? '—'}</Row>
              </div>
              <div className="mt-4 flex flex-wrap items-center gap-2">
                <button
                  type="button"
                  onClick={onReady}
                  disabled={isBusy || !permissions.has('transfers:create')}
                  className="inline-flex items-center h-10 px-4 rounded-lg bg-violet-600 hover:bg-violet-500 disabled:opacity-60 text-white text-sm font-medium transition-colors"
                >
                  Mark Ready
                </button>
                <Link
                  href={`/transfers/outbound/${transferId}/manifest`}
                  className="inline-flex items-center h-10 px-4 rounded-lg bg-muted hover:bg-muted/80 text-foreground text-sm font-medium transition-colors"
                >
                  Edit Manifest
                </Link>
              </div>
            </Card>

            <Card title="METRC Sync">
              <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
                <input
                  value={metrcForm.metrcSyncJobId}
                  onChange={(e) => setMetrcForm((p) => ({ ...p, metrcSyncJobId: e.target.value }))}
                  placeholder="MetrcSyncJobId (GUID)"
                  className="h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30"
                />
                <input
                  value={metrcForm.licenseNumber}
                  onChange={(e) => setMetrcForm((p) => ({ ...p, licenseNumber: e.target.value }))}
                  placeholder="LicenseNumber"
                  className="h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30"
                />
                <input
                  value={metrcForm.priority}
                  onChange={(e) => setMetrcForm((p) => ({ ...p, priority: e.target.value }))}
                  placeholder="Priority"
                  className="h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30"
                />
              </div>
              <div className="mt-3 flex items-center gap-2">
                <button
                  type="button"
                  onClick={onSubmitToMetrc}
                  disabled={isBusy || !permissions.has('compliance:metrc-submit')}
                  className="inline-flex items-center h-10 px-4 rounded-lg bg-violet-600 hover:bg-violet-500 disabled:opacity-60 text-white text-sm font-medium transition-colors"
                >
                  Submit to METRC
                </button>
                <div className="text-xs text-muted-foreground">
                  Status: {transfer.metrcSyncStatus ?? '—'}
                </div>
              </div>
              {transfer.metrcSyncError && (
                <div className="mt-2 text-xs text-rose-200">{transfer.metrcSyncError}</div>
              )}
            </Card>
          </div>

          <Card title="Packages">
            {transfer.packages.length === 0 ? (
              <div className="text-sm text-muted-foreground">No packages.</div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead className="text-xs text-muted-foreground">
                    <tr className="border-b border-border">
                      <th className="text-left py-2 pr-3">Label</th>
                      <th className="text-left py-2 pr-3">PackageId</th>
                      <th className="text-left py-2 pr-3">Qty</th>
                    </tr>
                  </thead>
                  <tbody>
                    {transfer.packages.map((p) => (
                      <tr key={p.id} className="border-b border-border/60">
                        <td className="py-2 pr-3">{p.packageLabel ?? '—'}</td>
                        <td className="py-2 pr-3 text-xs text-muted-foreground">{p.packageId}</td>
                        <td className="py-2 pr-3 text-muted-foreground">
                          {p.quantity} {p.unitOfMeasure}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </Card>

          <Card title="Void Transfer">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
              <input
                value={voidForm.metrcSyncJobId}
                onChange={(e) => setVoidForm((p) => ({ ...p, metrcSyncJobId: e.target.value }))}
                placeholder="MetrcSyncJobId (GUID)"
                className="h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30"
              />
              <input
                value={voidForm.licenseNumber}
                onChange={(e) => setVoidForm((p) => ({ ...p, licenseNumber: e.target.value }))}
                placeholder="LicenseNumber"
                className="h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30"
              />
              <input
                value={voidForm.reason}
                onChange={(e) => setVoidForm((p) => ({ ...p, reason: e.target.value }))}
                placeholder="Reason"
                className="h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30"
              />
            </div>
            <div className="mt-3">
              <button
                type="button"
                onClick={onVoid}
                disabled={isBusy || !permissions.has('transfers:create')}
                className="inline-flex items-center h-10 px-4 rounded-lg bg-rose-600 hover:bg-rose-500 disabled:opacity-60 text-white text-sm font-medium transition-colors"
              >
                Void
              </button>
            </div>
          </Card>
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

