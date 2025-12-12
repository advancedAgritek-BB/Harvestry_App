'use client';

import { useMemo, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuthStore } from '@/stores/auth/authStore';
import { createSalesOrder } from '@/features/sales/services/salesOrders.service';
import type { CreateSalesOrderRequest } from '@/features/sales/types/salesOrders.types';
import { usePermissions } from '@/providers/PermissionsProvider';

export default function CreateSalesOrderPage() {
  const router = useRouter();
  const siteId = useAuthStore((s) => s.currentSiteId);
  const permissions = usePermissions();

  const defaultOrderNumber = useMemo(() => {
    const now = new Date();
    const y = now.getFullYear();
    const m = String(now.getMonth() + 1).padStart(2, '0');
    const d = String(now.getDate()).padStart(2, '0');
    return `SO-${y}${m}${d}-001`;
  }, []);

  const [form, setForm] = useState<CreateSalesOrderRequest>({
    orderNumber: defaultOrderNumber,
    customerName: '',
    destinationLicenseNumber: '',
    destinationFacilityName: '',
    requestedShipDate: '',
    notes: '',
  });

  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function onSubmit() {
    if (!permissions.has('sales:orders:create')) {
      setError('You do not have permission to create sales orders.');
      return;
    }
    if (!siteId) {
      setError('No site selected.');
      return;
    }
    if (!form.orderNumber.trim() || !form.customerName.trim() || !form.destinationLicenseNumber.trim()) {
      setError('Order number, customer name, and destination license are required.');
      return;
    }

    setIsSaving(true);
    setError(null);
    try {
      const created = await createSalesOrder(siteId, {
        ...form,
        orderNumber: form.orderNumber.trim(),
        customerName: form.customerName.trim(),
        destinationLicenseNumber: form.destinationLicenseNumber.trim(),
        destinationFacilityName: form.destinationFacilityName?.trim() || undefined,
        requestedShipDate: form.requestedShipDate?.trim() || undefined,
        notes: form.notes?.trim() || undefined,
      });
      router.push(`/sales/orders/${created.id}`);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to create sales order');
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <div className="p-6 space-y-4 max-w-3xl">
      <div>
        <h1 className="text-xl font-semibold text-foreground">Create Sales Order</h1>
        <p className="text-sm text-muted-foreground">
          Draft an outbound order (customer + destination license).
        </p>
      </div>

      {error && (
        <div className="bg-rose-500/10 border border-rose-500/30 rounded-xl p-3 text-sm text-rose-200">
          {error}
        </div>
      )}

      <div className="bg-surface border border-border rounded-xl p-4 space-y-4">
        <Field
          label="Order Number"
          value={form.orderNumber}
          onChange={(v) => setForm((p) => ({ ...p, orderNumber: v }))}
        />
        <Field
          label="Customer Name"
          value={form.customerName}
          onChange={(v) => setForm((p) => ({ ...p, customerName: v }))}
        />
        <Field
          label="Destination License Number"
          value={form.destinationLicenseNumber}
          onChange={(v) => setForm((p) => ({ ...p, destinationLicenseNumber: v }))}
        />
        <Field
          label="Destination Facility Name (optional)"
          value={form.destinationFacilityName ?? ''}
          onChange={(v) => setForm((p) => ({ ...p, destinationFacilityName: v }))}
        />
        <Field
          label="Requested Ship Date (YYYY-MM-DD, optional)"
          value={form.requestedShipDate ?? ''}
          onChange={(v) => setForm((p) => ({ ...p, requestedShipDate: v }))}
        />
        <Field
          label="Notes (optional)"
          value={form.notes ?? ''}
          onChange={(v) => setForm((p) => ({ ...p, notes: v }))}
          multiline
        />

        <div className="flex items-center gap-3">
          <button
            type="button"
            onClick={onSubmit}
            disabled={isSaving}
            className="inline-flex items-center h-10 px-4 rounded-lg bg-violet-600 hover:bg-violet-500 disabled:opacity-60 text-white text-sm font-medium transition-colors"
          >
            {isSaving ? 'Creating…' : 'Create Draft'}
          </button>

          <button
            type="button"
            onClick={() => router.back()}
            className="inline-flex items-center h-10 px-4 rounded-lg bg-muted hover:bg-muted/80 text-foreground text-sm font-medium transition-colors"
          >
            Cancel
          </button>
        </div>
      </div>
    </div>
  );
}

function Field({
  label,
  value,
  onChange,
  multiline,
}: {
  label: string;
  value: string;
  onChange: (value: string) => void;
  multiline?: boolean;
}) {
  return (
    <div className="space-y-1.5">
      <div className="text-xs font-medium text-muted-foreground">{label}</div>
      {multiline ? (
        <textarea
          value={value}
          onChange={(e) => onChange(e.target.value)}
          className="w-full min-h-[96px] px-3 py-2 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30"
        />
      ) : (
        <input
          value={value}
          onChange={(e) => onChange(e.target.value)}
          className="w-full h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30"
        />
      )}
    </div>
  );
}

