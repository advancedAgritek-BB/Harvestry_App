'use client';

import Link from 'next/link';
import { useEffect, useMemo, useState } from 'react';
import { useAuthStore } from '@/stores/auth/authStore';
import { createInboundReceiptDraft, listInboundReceipts } from '@/features/transfers/services/inboundReceipts.service';
import type {
  CreateInboundReceiptLineRequest,
  InboundReceiptDto,
} from '@/features/transfers/types/inboundReceipts.types';
import { usePermissions } from '@/providers/PermissionsProvider';

export default function InboundReceiptsPage() {
  const siteId = useAuthStore((s) => s.currentSiteId);
  const permissions = usePermissions();

  const [receipts, setReceipts] = useState<InboundReceiptDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [status, setStatus] = useState('');

  const [draftForm, setDraftForm] = useState({
    outboundTransferId: '',
    metrcTransferId: '',
    metrcTransferNumber: '',
    notes: '',
  });

  const [lines, setLines] = useState<CreateInboundReceiptLineRequest[]>([
    { packageLabel: '', receivedQuantity: 0, unitOfMeasure: 'g', accepted: true, rejectionReason: '' },
  ]);

  const queryKey = useMemo(() => `${siteId ?? 'none'}|${status}`, [siteId, status]);

  useEffect(() => {
    if (!siteId) return;
    let cancelled = false;
    setIsLoading(true);
    setError(null);
    listInboundReceipts(siteId, { page: 1, pageSize: 50, status: status || undefined })
      .then((res) => {
        if (cancelled) return;
        setReceipts(res.receipts ?? []);
      })
      .catch((e) => {
        if (cancelled) return;
        setError(e instanceof Error ? e.message : 'Failed to load receipts');
      })
      .finally(() => {
        if (cancelled) return;
        setIsLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [queryKey, siteId, status]);

  function updateLine(idx: number, patch: Partial<CreateInboundReceiptLineRequest>) {
    setLines((prev) => prev.map((l, i) => (i === idx ? { ...l, ...patch } : l)));
  }

  function addLine() {
    setLines((prev) => [
      ...prev,
      { packageLabel: '', receivedQuantity: 0, unitOfMeasure: 'g', accepted: true, rejectionReason: '' },
    ]);
  }

  function removeLine(idx: number) {
    setLines((prev) => prev.filter((_, i) => i !== idx));
  }

  async function createDraft() {
    if (!siteId) return;
    if (!permissions.has('transfers:create')) {
      setError('You do not have permission to create inbound receipts.');
      return;
    }
    const cleanedLines = lines
      .map((l) => ({
        ...l,
        packageLabel: l.packageLabel.trim(),
        unitOfMeasure: l.unitOfMeasure.trim(),
        rejectionReason: l.rejectionReason?.trim() || undefined,
      }))
      .filter((l) => l.packageLabel && l.receivedQuantity > 0);

    if (cleanedLines.length === 0) {
      setError('Add at least one line (package label + quantity).');
      return;
    }

    setIsLoading(true);
    setError(null);
    try {
      const created = await createInboundReceiptDraft(siteId, {
        outboundTransferId: draftForm.outboundTransferId.trim() || undefined,
        metrcTransferId: draftForm.metrcTransferId ? Number(draftForm.metrcTransferId) : undefined,
        metrcTransferNumber: draftForm.metrcTransferNumber.trim() || undefined,
        notes: draftForm.notes.trim() || undefined,
        lines: cleanedLines,
      });
      window.location.href = `/transfers/inbound/receipts/${created.id}`;
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to create draft receipt');
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <div className="p-6 space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-semibold text-foreground">Inbound Receipts</h1>
          <p className="text-sm text-muted-foreground">
            Create drafts and accept/reject incoming transfers.
          </p>
        </div>
        <Link
          href="/transfers/outbound"
          className="inline-flex items-center h-10 px-4 rounded-lg bg-muted hover:bg-muted/80 text-foreground text-sm font-medium transition-colors"
        >
          Outbound Transfers
        </Link>
      </div>

      {error && (
        <div className="bg-rose-500/10 border border-rose-500/30 rounded-xl p-3 text-sm text-rose-200">
          {error}
        </div>
      )}

      <div className="bg-surface border border-border rounded-xl p-4 space-y-3">
        <div className="text-sm font-medium text-foreground">Create Draft Receipt</div>
        <div className="grid grid-cols-1 md:grid-cols-4 gap-3">
          <input
            value={draftForm.outboundTransferId}
            onChange={(e) => setDraftForm((p) => ({ ...p, outboundTransferId: e.target.value }))}
            placeholder="OutboundTransferId (optional)"
            className="h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30"
          />
          <input
            value={draftForm.metrcTransferId}
            onChange={(e) => setDraftForm((p) => ({ ...p, metrcTransferId: e.target.value }))}
            placeholder="MetrcTransferId (optional)"
            className="h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30"
          />
          <input
            value={draftForm.metrcTransferNumber}
            onChange={(e) => setDraftForm((p) => ({ ...p, metrcTransferNumber: e.target.value }))}
            placeholder="MetrcTransferNumber (optional)"
            className="h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30"
          />
          <input
            value={draftForm.notes}
            onChange={(e) => setDraftForm((p) => ({ ...p, notes: e.target.value }))}
            placeholder="Notes (optional)"
            className="h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30"
          />
        </div>

        <div className="space-y-2">
          {lines.map((l, idx) => (
            <div key={idx} className="grid grid-cols-1 md:grid-cols-5 gap-2">
              <input
                value={l.packageLabel}
                onChange={(e) => updateLine(idx, { packageLabel: e.target.value })}
                placeholder="PackageLabel"
                className="h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30"
              />
              <input
                value={String(l.receivedQuantity)}
                onChange={(e) => updateLine(idx, { receivedQuantity: Number(e.target.value) })}
                placeholder="Qty"
                type="number"
                className="h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30"
              />
              <input
                value={l.unitOfMeasure}
                onChange={(e) => updateLine(idx, { unitOfMeasure: e.target.value })}
                placeholder="UOM"
                className="h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30"
              />
              <select
                value={l.accepted === false ? 'rejected' : 'accepted'}
                onChange={(e) => updateLine(idx, { accepted: e.target.value === 'accepted' })}
                className="h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30"
              >
                <option value="accepted">Accepted</option>
                <option value="rejected">Rejected</option>
              </select>
              <input
                value={l.rejectionReason ?? ''}
                onChange={(e) => updateLine(idx, { rejectionReason: e.target.value })}
                placeholder="Rejection reason (if rejected)"
                className="h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30"
              />

              <div className="md:col-span-5 flex justify-end">
                {lines.length > 1 && (
                  <button
                    type="button"
                    onClick={() => removeLine(idx)}
                    className="text-xs text-rose-200 hover:text-rose-100"
                  >
                    Remove line
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>

        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={addLine}
            className="inline-flex items-center h-10 px-4 rounded-lg bg-muted hover:bg-muted/80 text-foreground text-sm font-medium transition-colors"
          >
            Add Line
          </button>
          <button
            type="button"
            onClick={createDraft}
            disabled={isLoading || !permissions.has('transfers:create')}
            className="inline-flex items-center h-10 px-4 rounded-lg bg-violet-600 hover:bg-violet-500 disabled:opacity-60 text-white text-sm font-medium transition-colors"
          >
            Create Draft
          </button>
        </div>
      </div>

      <div className="bg-surface border border-border rounded-xl p-4 space-y-3">
        <div className="flex items-center justify-between">
          <div className="text-sm font-medium text-foreground">Receipts</div>
          <select
            value={status}
            onChange={(e) => setStatus(e.target.value)}
            className="h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30"
          >
            <option value="">All statuses</option>
            <option value="Draft">Draft</option>
            <option value="Accepted">Accepted</option>
            <option value="Rejected">Rejected</option>
          </select>
        </div>

        {isLoading ? (
          <div className="text-sm text-muted-foreground">Loading…</div>
        ) : receipts.length === 0 ? (
          <div className="text-sm text-muted-foreground">No receipts found.</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="text-xs text-muted-foreground">
                <tr className="border-b border-border">
                  <th className="text-left py-2 pr-3">Receipt</th>
                  <th className="text-left py-2 pr-3">Status</th>
                  <th className="text-left py-2 pr-3">Transfer</th>
                  <th className="text-left py-2 pr-3">METRC</th>
                </tr>
              </thead>
              <tbody>
                {receipts.map((r) => (
                  <tr key={r.id} className="border-b border-border/60 hover:bg-muted/30">
                    <td className="py-2 pr-3">
                      <Link
                        href={`/transfers/inbound/receipts/${r.id}`}
                        className="text-violet-300 hover:text-violet-200 font-medium"
                      >
                        {r.id}
                      </Link>
                    </td>
                    <td className="py-2 pr-3">{r.status}</td>
                    <td className="py-2 pr-3 text-xs text-muted-foreground">{r.outboundTransferId ?? '—'}</td>
                    <td className="py-2 pr-3 text-xs text-muted-foreground">{r.metrcTransferNumber ?? '—'}</td>
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

