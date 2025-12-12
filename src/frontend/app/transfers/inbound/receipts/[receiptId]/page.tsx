'use client';

import { useEffect, useMemo, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { useAuthStore } from '@/stores/auth/authStore';
import {
  acceptInboundReceipt,
  getInboundReceipt,
  rejectInboundReceipt,
} from '@/features/transfers/services/inboundReceipts.service';
import type { InboundReceiptDto } from '@/features/transfers/types/inboundReceipts.types';
import { usePermissions } from '@/providers/PermissionsProvider';

export default function InboundReceiptDetailPage() {
  const router = useRouter();
  const params = useParams<{ receiptId: string }>();
  const receiptId = params.receiptId;
  const siteId = useAuthStore((s) => s.currentSiteId);
  const permissions = usePermissions();

  const [receipt, setReceipt] = useState<InboundReceiptDto | null>(null);
  const [isBusy, setIsBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [acceptNotes, setAcceptNotes] = useState('');
  const [rejectReason, setRejectReason] = useState('');

  const refreshKey = useMemo(() => `${siteId ?? 'none'}|${receiptId}`, [siteId, receiptId]);

  useEffect(() => {
    if (!siteId) return;
    let cancelled = false;
    setIsBusy(true);
    setError(null);
    getInboundReceipt(siteId, receiptId)
      .then((r) => {
        if (cancelled) return;
        setReceipt(r);
        setAcceptNotes(r.notes ?? '');
      })
      .catch((e) => {
        if (cancelled) return;
        setError(e instanceof Error ? e.message : 'Failed to load receipt');
      })
      .finally(() => {
        if (cancelled) return;
        setIsBusy(false);
      });

    return () => {
      cancelled = true;
    };
  }, [refreshKey, siteId, receiptId]);

  async function refresh() {
    if (!siteId) return;
    const r = await getInboundReceipt(siteId, receiptId);
    setReceipt(r);
  }

  async function onAccept() {
    if (!siteId) return;
    if (!permissions.has('transfers:create')) {
      setError('You do not have permission to accept inbound receipts.');
      return;
    }
    setIsBusy(true);
    setError(null);
    try {
      const updated = await acceptInboundReceipt(siteId, receiptId, {
        notes: acceptNotes.trim() || undefined,
      });
      setReceipt(updated);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to accept receipt');
    } finally {
      setIsBusy(false);
    }
  }

  async function onReject() {
    if (!siteId) return;
    if (!permissions.has('transfers:create')) {
      setError('You do not have permission to reject inbound receipts.');
      return;
    }
    if (!rejectReason.trim()) {
      setError('Reject reason is required.');
      return;
    }
    setIsBusy(true);
    setError(null);
    try {
      const updated = await rejectInboundReceipt(siteId, receiptId, {
        reason: rejectReason.trim(),
      });
      setReceipt(updated);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to reject receipt');
    } finally {
      setIsBusy(false);
    }
  }

  return (
    <div className="p-6 space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-semibold text-foreground">Inbound Receipt</h1>
          <p className="text-sm text-muted-foreground">{receiptId}</p>
        </div>
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={() => router.push('/transfers/inbound/receipts')}
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

      {!receipt ? (
        <div className="text-sm text-muted-foreground">{isBusy ? 'Loading…' : 'Not found.'}</div>
      ) : (
        <>
          <Card title="Overview">
            <div className="space-y-2 text-sm">
              <Row label="Status">{receipt.status}</Row>
              <Row label="OutboundTransferId">{receipt.outboundTransferId ?? '—'}</Row>
              <Row label="METRC Transfer #">{receipt.metrcTransferNumber ?? '—'}</Row>
              <Row label="ReceivedAt">{receipt.receivedAt ?? '—'}</Row>
            </div>
          </Card>

          <Card title="Lines">
            {receipt.lines.length === 0 ? (
              <div className="text-sm text-muted-foreground">No lines.</div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead className="text-xs text-muted-foreground">
                    <tr className="border-b border-border">
                      <th className="text-left py-2 pr-3">Label</th>
                      <th className="text-left py-2 pr-3">Qty</th>
                      <th className="text-left py-2 pr-3">Accepted</th>
                      <th className="text-left py-2 pr-3">Reason</th>
                    </tr>
                  </thead>
                  <tbody>
                    {receipt.lines.map((l) => (
                      <tr key={l.id} className="border-b border-border/60">
                        <td className="py-2 pr-3">{l.packageLabel}</td>
                        <td className="py-2 pr-3 text-muted-foreground">
                          {l.receivedQuantity} {l.unitOfMeasure}
                        </td>
                        <td className="py-2 pr-3">
                          {l.accepted ? (
                            <span className="text-emerald-200">Yes</span>
                          ) : (
                            <span className="text-rose-200">No</span>
                          )}
                        </td>
                        <td className="py-2 pr-3 text-muted-foreground">{l.rejectionReason ?? '—'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </Card>

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
            <Card title="Accept">
              <div className="space-y-2">
                <div className="text-xs text-muted-foreground">Notes (optional)</div>
                <textarea
                  value={acceptNotes}
                  onChange={(e) => setAcceptNotes(e.target.value)}
                  className="w-full min-h-[96px] px-3 py-2 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30"
                />
                <button
                  type="button"
                  onClick={onAccept}
                  disabled={isBusy || !permissions.has('transfers:create')}
                  className="inline-flex items-center h-10 px-4 rounded-lg bg-violet-600 hover:bg-violet-500 disabled:opacity-60 text-white text-sm font-medium transition-colors"
                >
                  Accept Receipt
                </button>
              </div>
            </Card>

            <Card title="Reject">
              <div className="space-y-2">
                <div className="text-xs text-muted-foreground">Reason</div>
                <input
                  value={rejectReason}
                  onChange={(e) => setRejectReason(e.target.value)}
                  placeholder="Reason for rejection…"
                  className="w-full h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30"
                />
                <button
                  type="button"
                  onClick={onReject}
                  disabled={isBusy || !permissions.has('transfers:create')}
                  className="inline-flex items-center h-10 px-4 rounded-lg bg-rose-600 hover:bg-rose-500 disabled:opacity-60 text-white text-sm font-medium transition-colors"
                >
                  Reject Receipt
                </button>
              </div>
            </Card>
          </div>
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

