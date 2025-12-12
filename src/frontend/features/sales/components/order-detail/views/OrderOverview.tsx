'use client';

import type { SalesOrderDto } from '@/features/sales/types/salesOrders.types';
import { usePermissions } from '@/providers/PermissionsProvider';

export function OrderOverview({
  order,
  disabled,
  onSubmit,
  onCancel,
}: {
  order: SalesOrderDto;
  disabled: boolean;
  onSubmit: () => void;
  onCancel: (reason: string) => void;
}) {
  const permissions = usePermissions();

  function handleCancelClick() {
    const reason = prompt('Cancel reason?') ?? '';
    if (!reason.trim()) return;
    onCancel(reason.trim());
  }

  return (
    <div className="space-y-4">
      <div className="space-y-2 text-sm">
        <Row label="Order #">{order.orderNumber}</Row>
        <Row label="Customer">{order.customerName}</Row>
        <Row label="Destination License">{order.destinationLicenseNumber ?? '—'}</Row>
        <Row label="Destination Facility">{order.destinationFacilityName ?? '—'}</Row>
        <Row label="Status">{order.status}</Row>
        <Row label="Requested Ship">{order.requestedShipDate ?? '—'}</Row>
      </div>

      <div className="flex items-center gap-2">
        <button
          type="button"
          onClick={onSubmit}
          disabled={disabled || !permissions.has('sales:orders:submit')}
          className="inline-flex items-center h-10 px-4 rounded-lg bg-violet-600 hover:bg-violet-500 disabled:opacity-60 text-white text-sm font-medium transition-colors"
        >
          Submit
        </button>
        <button
          type="button"
          onClick={handleCancelClick}
          disabled={disabled || !permissions.has('sales:orders:cancel')}
          className="inline-flex items-center h-10 px-4 rounded-lg bg-rose-600 hover:bg-rose-500 disabled:opacity-60 text-white text-sm font-medium transition-colors"
        >
          Cancel
        </button>
      </div>
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

