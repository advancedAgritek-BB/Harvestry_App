'use client';

import { useMemo, useState } from 'react';
import type { SalesOrderDto, SalesOrderLineDto } from '@/features/sales/types/salesOrders.types';
import type {
  AllocateSalesOrderRequest,
  SalesAllocationDto,
  UnallocateSalesOrderRequest,
} from '@/features/sales/types/allocations.types';
import { Field } from '../ui/Field';
import { usePermissions } from '@/providers/PermissionsProvider';
import { PackageCombobox, type PackageSelection } from '../../shared/PackageCombobox';
import { IdCopyCell } from '../../shared/IdDisplay';

type SelectedAllocationsState = Record<string, boolean>;

interface AllocationsPanelProps {
  siteId: string;
  salesOrder: SalesOrderDto;
  allocations: SalesAllocationDto[];
  disabled: boolean;
  onAllocate: (request: AllocateSalesOrderRequest) => Promise<void> | void;
  onUnallocate: (request: UnallocateSalesOrderRequest) => Promise<void> | void;
  onRefresh: () => Promise<void> | void;
}

export function AllocationsPanel({
  siteId,
  salesOrder,
  allocations,
  disabled,
  onAllocate,
  onUnallocate,
  onRefresh,
}: AllocationsPanelProps) {
  const permissions = usePermissions();

  // Form state
  const [selectedLineId, setSelectedLineId] = useState('');
  const [selectedPackage, setSelectedPackage] = useState<PackageSelection | null>(null);
  const [quantity, setQuantity] = useState('0');
  const [useManualEntry, setUseManualEntry] = useState(false);
  const [manualForm, setManualForm] = useState({
    salesOrderLineId: '',
    packageId: '',
    quantity: '0',
  });

  const [unallocReason, setUnallocReason] = useState('');
  const [selected, setSelected] = useState<SelectedAllocationsState>({});
  const [error, setError] = useState<string | null>(null);

  const activeAllocations = useMemo(
    () => allocations.filter((a) => !a.isCancelled),
    [allocations]
  );

  // Get lines that still need allocation (requested > allocated)
  const linesNeedingAllocation = useMemo(() => {
    return (salesOrder.lines ?? []).filter(
      (line) => line.requestedQuantity > line.allocatedQuantity
    );
  }, [salesOrder.lines]);

  // Get selected line details
  const selectedLine = useMemo(() => {
    return (salesOrder.lines ?? []).find((l) => l.id === selectedLineId);
  }, [salesOrder.lines, selectedLineId]);

  function handlePackageSelect(pkg: PackageSelection) {
    setSelectedPackage(pkg);
    // Pre-fill quantity with the min of available and remaining needed
    if (selectedLine) {
      const remaining = selectedLine.requestedQuantity - selectedLine.allocatedQuantity;
      const suggestedQty = Math.min(pkg.availableQuantity, remaining);
      setQuantity(String(suggestedQty));
    } else {
      setQuantity(String(pkg.availableQuantity));
    }
    setError(null);
  }

  function clearSelection() {
    setSelectedPackage(null);
    setQuantity('0');
  }

  async function submitAllocate() {
    if (!permissions.has('sales:allocate')) {
      setError('You do not have permission to allocate inventory.');
      return;
    }

    let lineId: string;
    let pkgId: string;
    let qty: number;

    if (useManualEntry) {
      lineId = manualForm.salesOrderLineId.trim();
      pkgId = manualForm.packageId.trim();
      qty = Number(manualForm.quantity);
    } else {
      lineId = selectedLineId;
      pkgId = selectedPackage?.id ?? '';
      qty = Number(quantity);
    }

    if (!lineId || !pkgId || !qty || qty <= 0) {
      setError('Line, package, and quantity are required.');
      return;
    }

    setError(null);
    const request: AllocateSalesOrderRequest = {
      lines: [{ salesOrderLineId: lineId, packages: [{ packageId: pkgId, quantity: qty }] }],
    };

    await onAllocate(request);

    // Reset form
    if (useManualEntry) {
      setManualForm({ salesOrderLineId: '', packageId: '', quantity: '0' });
    } else {
      setSelectedLineId('');
      clearSelection();
    }
    setSelected({});
    await onRefresh();
  }

  async function submitUnallocateSelected() {
    if (!permissions.has('sales:allocate')) {
      setError('You do not have permission to unallocate inventory.');
      return;
    }
    const ids = Object.entries(selected)
      .filter(([, isSelected]) => isSelected)
      .map(([id]) => id);

    if (ids.length === 0) {
      setError('Select at least one allocation to unallocate.');
      return;
    }

    const reason = unallocReason.trim();
    if (!reason) {
      setError('Unallocation reason is required.');
      return;
    }

    setError(null);
    await onUnallocate({ allocationIds: ids, reason });
    setUnallocReason('');
    setSelected({});
    await onRefresh();
  }

  function toggle(id: string) {
    setSelected((prev) => ({ ...prev, [id]: !prev[id] }));
  }

  function getLineDisplayName(line: SalesOrderLineDto) {
    const remaining = line.requestedQuantity - line.allocatedQuantity;
    return `#${line.lineNumber}: ${line.itemName} (${remaining} ${line.unitOfMeasure} needed)`;
  }

  return (
    <div className="space-y-4">
      {error && <div className="text-sm text-rose-200">{error}</div>}

      {/* Toggle between smart and manual entry */}
      <div className="flex items-center gap-2 text-xs">
        <button
          type="button"
          onClick={() => setUseManualEntry(false)}
          className={`px-2 py-1 rounded ${
            !useManualEntry
              ? 'bg-violet-600 text-white'
              : 'bg-muted text-muted-foreground hover:text-foreground'
          }`}
        >
          Smart Allocation
        </button>
        <button
          type="button"
          onClick={() => setUseManualEntry(true)}
          className={`px-2 py-1 rounded ${
            useManualEntry
              ? 'bg-violet-600 text-white'
              : 'bg-muted text-muted-foreground hover:text-foreground'
          }`}
        >
          Manual Entry
        </button>
      </div>

      {!useManualEntry ? (
        /* Smart Allocation Mode */
        <div className="space-y-3">
          {/* Line Selector */}
          <div className="space-y-1.5">
            <div className="text-xs font-medium text-muted-foreground">Select Order Line</div>
            <select
              value={selectedLineId}
              onChange={(e) => {
                setSelectedLineId(e.target.value);
                clearSelection();
              }}
              disabled={disabled}
              aria-label="Select order line"
              className="w-full h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30"
            >
              <option value="">-- Select a line --</option>
              {linesNeedingAllocation.map((line) => (
                <option key={line.id} value={line.id}>
                  {getLineDisplayName(line)}
                </option>
              ))}
              {linesNeedingAllocation.length === 0 && (
                <option value="" disabled>
                  All lines fully allocated
                </option>
              )}
            </select>
          </div>

          {/* Package Selector - only show when line is selected */}
          {selectedLineId && (
            <div className="space-y-1.5">
              <div className="text-xs font-medium text-muted-foreground">Search Package</div>
              <PackageCombobox
                siteId={siteId}
                onSelect={handlePackageSelect}
                disabled={disabled}
                placeholder="Type to search packages..."
              />
            </div>
          )}

          {/* Selected Package Info & Quantity */}
          {selectedPackage && (
            <div className="bg-muted/50 rounded-lg p-3 space-y-2">
              <div className="flex items-center justify-between">
                <span className="text-sm font-medium text-foreground">
                  {selectedPackage.packageLabel}
                </span>
                <button
                  type="button"
                  onClick={clearSelection}
                  className="text-xs text-muted-foreground hover:text-foreground"
                >
                  Clear
                </button>
              </div>
              <div className="text-xs text-muted-foreground">
                {selectedPackage.itemName} • {selectedPackage.availableQuantity}{' '}
                {selectedPackage.unitOfMeasure} available
              </div>

              <Field
                label="Quantity to Allocate"
                value={quantity}
                type="number"
                onChange={setQuantity}
              />
            </div>
          )}
        </div>
      ) : (
        /* Manual Entry Mode */
        <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
          <Field
            label="Sales Order Line ID"
            value={manualForm.salesOrderLineId}
            placeholder="Line GUID"
            onChange={(v) => setManualForm((p) => ({ ...p, salesOrderLineId: v }))}
          />
          <Field
            label="Package ID"
            value={manualForm.packageId}
            placeholder="Package GUID"
            onChange={(v) => setManualForm((p) => ({ ...p, packageId: v }))}
          />
          <Field
            label="Qty"
            value={manualForm.quantity}
            type="number"
            onChange={(v) => setManualForm((p) => ({ ...p, quantity: v }))}
          />
        </div>
      )}

      <div className="flex items-center gap-2">
        <button
          type="button"
          onClick={submitAllocate}
          disabled={disabled || !permissions.has('sales:allocate')}
          className="inline-flex items-center h-10 px-4 rounded-lg bg-violet-600 hover:bg-violet-500 disabled:opacity-60 text-white text-sm font-medium transition-colors"
        >
          Allocate
        </button>
        <div className="text-xs text-muted-foreground">Order: {salesOrder.orderNumber}</div>
      </div>

      <div className="border-t border-border pt-4">
        <div className="flex items-center gap-2">
          <input
            value={unallocReason}
            onChange={(e) => setUnallocReason(e.target.value)}
            placeholder="Reason to unallocate selected…"
            className="flex-1 h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30"
          />
          <button
            type="button"
            onClick={submitUnallocateSelected}
            disabled={disabled || !permissions.has('sales:allocate')}
            className="inline-flex items-center h-10 px-4 rounded-lg bg-rose-600 hover:bg-rose-500 disabled:opacity-60 text-white text-sm font-medium transition-colors"
          >
            Unallocate Selected
          </button>
        </div>
      </div>

      {activeAllocations.length === 0 ? (
        <div className="text-sm text-muted-foreground">No allocations yet.</div>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="text-xs text-muted-foreground">
              <tr className="border-b border-border">
                <th className="text-left py-2 pr-3 w-10">Select</th>
                <th className="text-left py-2 pr-3">Line</th>
                <th className="text-left py-2 pr-3">Package</th>
                <th className="text-left py-2 pr-3">Qty</th>
              </tr>
            </thead>
            <tbody>
              {activeAllocations.map((a) => {
                const line = (salesOrder.lines ?? []).find((l) => l.id === a.salesOrderLineId);
                return (
                  <tr key={a.id} className="border-b border-border/60 hover:bg-muted/30">
                    <td className="py-2 pr-3">
                      <input
                        type="checkbox"
                        checked={selected[a.id] === true}
                        onChange={() => toggle(a.id)}
                        aria-label={`Select allocation for ${a.packageLabel ?? 'package'}`}
                        className="rounded"
                      />
                    </td>
                    <td className="py-2 pr-3">
                      <div className="font-medium">
                        #{line?.lineNumber ?? '?'}: {line?.itemName ?? 'Unknown'}
                      </div>
                      <IdCopyCell id={a.salesOrderLineId} />
                    </td>
                    <td className="py-2 pr-3">
                      <div className="font-medium">{a.packageLabel ?? '—'}</div>
                      <IdCopyCell id={a.packageId} />
                    </td>
                    <td className="py-2 pr-3 text-muted-foreground">
                      {a.allocatedQuantity} {a.unitOfMeasure}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
