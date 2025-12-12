'use client';

import { useState } from 'react';
import { Field } from '../ui/Field';
import { usePermissions } from '@/providers/PermissionsProvider';
import { PackageCombobox, type PackageSelection } from '../../shared/PackageCombobox';

interface AddLineFormProps {
  siteId: string;
  disabled: boolean;
  onAddLine: (input: {
    itemId: string;
    itemName: string;
    requestedQuantity: number;
    unitOfMeasure: string;
    unitPrice?: number;
  }) => Promise<void> | void;
}

export function AddLineForm({ siteId, disabled, onAddLine }: AddLineFormProps) {
  const permissions = usePermissions();
  const [useManualEntry, setUseManualEntry] = useState(false);
  const [selectedPackage, setSelectedPackage] = useState<PackageSelection | null>(null);

  const [form, setForm] = useState({
    itemId: '',
    itemName: '',
    requestedQuantity: '0',
    unitOfMeasure: 'g',
    unitPrice: '',
  });

  const [error, setError] = useState<string | null>(null);

  function handlePackageSelect(pkg: PackageSelection) {
    setSelectedPackage(pkg);
    setForm((prev) => ({
      ...prev,
      itemId: pkg.itemId,
      itemName: pkg.itemName,
      unitOfMeasure: pkg.unitOfMeasure,
      requestedQuantity: String(Math.min(pkg.availableQuantity, 100)), // Default to available or 100
    }));
    setError(null);
  }

  function clearSelection() {
    setSelectedPackage(null);
    setForm({
      itemId: '',
      itemName: '',
      requestedQuantity: '0',
      unitOfMeasure: 'g',
      unitPrice: '',
    });
  }

  async function submit() {
    if (!permissions.has('sales:orders:create')) {
      setError('You do not have permission to edit sales orders.');
      return;
    }
    const itemId = form.itemId.trim();
    const itemName = form.itemName.trim();
    const unitOfMeasure = form.unitOfMeasure.trim();
    const requestedQuantity = Number(form.requestedQuantity);
    const unitPrice = form.unitPrice ? Number(form.unitPrice) : undefined;

    if (!itemId || !itemName || !unitOfMeasure) {
      setError('Item ID, item name, and unit of measure are required.');
      return;
    }
    if (!requestedQuantity || requestedQuantity <= 0) {
      setError('Requested quantity must be > 0.');
      return;
    }

    setError(null);
    await onAddLine({ itemId, itemName, unitOfMeasure, requestedQuantity, unitPrice });
    clearSelection();
  }

  return (
    <div className="space-y-3">
      {error && <div className="text-sm text-rose-200">{error}</div>}

      {/* Toggle between package search and manual entry */}
      <div className="flex items-center gap-2 text-xs">
        <button
          type="button"
          onClick={() => {
            setUseManualEntry(false);
            clearSelection();
          }}
          className={`px-2 py-1 rounded ${
            !useManualEntry
              ? 'bg-violet-600 text-white'
              : 'bg-muted text-muted-foreground hover:text-foreground'
          }`}
        >
          Search Packages
        </button>
        <button
          type="button"
          onClick={() => {
            setUseManualEntry(true);
            clearSelection();
          }}
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
        /* Package Search Mode */
        <div className="space-y-3">
          <div className="space-y-1.5">
            <div className="text-xs font-medium text-muted-foreground">Search Package</div>
            <PackageCombobox
              siteId={siteId}
              onSelect={handlePackageSelect}
              disabled={disabled}
              placeholder="Type to search packages by label or item..."
            />
          </div>

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

              <div className="grid grid-cols-2 gap-3 pt-2">
                <Field
                  label="Requested Qty"
                  value={form.requestedQuantity}
                  type="number"
                  onChange={(v) => setForm((p) => ({ ...p, requestedQuantity: v }))}
                />
                <Field
                  label="Unit Price (optional)"
                  value={form.unitPrice}
                  type="number"
                  onChange={(v) => setForm((p) => ({ ...p, unitPrice: v }))}
                />
              </div>
            </div>
          )}
        </div>
      ) : (
        /* Manual Entry Mode */
        <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
          <Field
            label="Item ID (GUID)"
            value={form.itemId}
            onChange={(v) => setForm((p) => ({ ...p, itemId: v }))}
          />
          <Field
            label="Item Name"
            value={form.itemName}
            onChange={(v) => setForm((p) => ({ ...p, itemName: v }))}
          />
          <Field
            label="UOM"
            value={form.unitOfMeasure}
            onChange={(v) => setForm((p) => ({ ...p, unitOfMeasure: v }))}
          />
          <Field
            label="Requested Qty"
            value={form.requestedQuantity}
            type="number"
            onChange={(v) => setForm((p) => ({ ...p, requestedQuantity: v }))}
          />
          <Field
            label="Unit Price (optional)"
            value={form.unitPrice}
            type="number"
            onChange={(v) => setForm((p) => ({ ...p, unitPrice: v }))}
          />
        </div>
      )}

      <button
        type="button"
        onClick={submit}
        disabled={disabled || !permissions.has('sales:orders:create')}
        className="inline-flex items-center h-10 px-4 rounded-lg bg-violet-600 hover:bg-violet-500 disabled:opacity-60 text-white text-sm font-medium transition-colors"
      >
        Add Line
      </button>
    </div>
  );
}
