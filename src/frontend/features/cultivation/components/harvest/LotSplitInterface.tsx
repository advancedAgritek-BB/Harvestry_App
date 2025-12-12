'use client';

/**
 * LotSplitInterface Component
 * Interface for splitting batches into inventory lots by quality, destination, etc.
 */

import { useState, useMemo } from 'react';
import { cn } from '@/lib/utils';
import type { LotSplitItem } from '@/features/inventory/types';

interface LotSplitInterfaceProps {
  /** Source batch ID */
  batchId: string;
  /** Total available weight */
  totalWeight: number;
  /** Strain name for display */
  strainName: string;
  /** Unit of measurement */
  uom?: string;
  /** Callback when lots are created */
  onCreateLots: (splits: LotSplitItem[]) => Promise<void>;
  /** Callback to cancel */
  onCancel: () => void;
  /** Loading state */
  isLoading?: boolean;
}

type QualityGrade = 'A' | 'B' | 'C' | 'D';
type Destination = 'retail' | 'wholesale' | 'manufacturing' | 'sample';

interface SplitEntry {
  id: string;
  quantity: number;
  qualityGrade: QualityGrade;
  destination: Destination;
  notes: string;
}

const QUALITY_GRADES: { value: QualityGrade; label: string; color: string }[] = [
  { value: 'A', label: 'A Grade - Premium', color: 'bg-emerald-500/20 text-emerald-400' },
  { value: 'B', label: 'B Grade - Standard', color: 'bg-blue-500/20 text-blue-400' },
  { value: 'C', label: 'C Grade - Economy', color: 'bg-amber-500/20 text-amber-400' },
  { value: 'D', label: 'D Grade - Processing', color: 'bg-slate-500/20 text-slate-400' },
];

const DESTINATIONS: { value: Destination; label: string; icon: string }[] = [
  { value: 'retail', label: 'Retail Sale', icon: '🏪' },
  { value: 'wholesale', label: 'Wholesale', icon: '📦' },
  { value: 'manufacturing', label: 'Manufacturing', icon: '🏭' },
  { value: 'sample', label: 'Sample/QA', icon: '🔬' },
];

export function LotSplitInterface({
  batchId,
  totalWeight,
  strainName,
  uom = 'g',
  onCreateLots,
  onCancel,
  isLoading = false,
}: LotSplitInterfaceProps) {
  const [splits, setSplits] = useState<SplitEntry[]>([
    { id: '1', quantity: 0, qualityGrade: 'A', destination: 'retail', notes: '' },
  ]);

  // Calculate totals
  const allocatedWeight = useMemo(
    () => splits.reduce((sum, s) => sum + s.quantity, 0),
    [splits]
  );
  const remainingWeight = totalWeight - allocatedWeight;
  const allocationPercent = totalWeight > 0 ? (allocatedWeight / totalWeight) * 100 : 0;

  // Validation
  const isValid = useMemo(() => {
    if (allocatedWeight <= 0) return false;
    if (allocatedWeight > totalWeight) return false;
    return splits.every(s => s.quantity > 0);
  }, [splits, allocatedWeight, totalWeight]);

  const addSplit = () => {
    setSplits(prev => [
      ...prev,
      {
        id: String(Date.now()),
        quantity: 0,
        qualityGrade: 'B',
        destination: 'retail',
        notes: '',
      },
    ]);
  };

  const removeSplit = (id: string) => {
    if (splits.length === 1) return;
    setSplits(prev => prev.filter(s => s.id !== id));
  };

  const updateSplit = (id: string, field: keyof SplitEntry, value: any) => {
    setSplits(prev => prev.map(s =>
      s.id === id ? { ...s, [field]: value } : s
    ));
  };

  const distributeEvenly = () => {
    const perSplit = totalWeight / splits.length;
    setSplits(prev => prev.map(s => ({
      ...s,
      quantity: Math.round(perSplit * 10) / 10,
    })));
  };

  const allocateRemaining = (id: string) => {
    if (remainingWeight <= 0) return;
    updateSplit(id, 'quantity', splits.find(s => s.id === id)!.quantity + remainingWeight);
  };

  const handleSubmit = async () => {
    const lotSplits: LotSplitItem[] = splits
      .filter(s => s.quantity > 0)
      .map(s => ({
        quantity: s.quantity,
        uom,
        qualityGrade: s.qualityGrade,
        destination: s.destination,
        locationId: '', // Would be set by UI
        notes: s.notes || undefined,
      }));

    await onCreateLots(lotSplits);
  };

  return (
    <div className="flex flex-col h-full">
      {/* Header */}
      <div className="px-6 py-4 border-b border-border">
        <h2 className="text-lg font-semibold">Split into Lots</h2>
        <p className="text-sm text-muted-foreground">
          {strainName} • {totalWeight.toFixed(1)}{uom} available
        </p>
      </div>

      {/* Allocation summary */}
      <div className="px-6 py-3 border-b border-border/50 bg-muted/30">
        <div className="flex items-center justify-between mb-2">
          <span className="text-sm">Allocation</span>
          <span className={cn(
            'text-sm font-mono',
            remainingWeight < 0 ? 'text-rose-400' :
            remainingWeight === 0 ? 'text-emerald-400' : 'text-muted-foreground'
          )}>
            {allocatedWeight.toFixed(1)} / {totalWeight.toFixed(1)}{uom}
          </span>
        </div>
        <div className="h-2 bg-muted rounded-full overflow-hidden">
          <div 
            className={cn(
              'h-full transition-all',
              remainingWeight < 0 ? 'bg-rose-500' :
              remainingWeight === 0 ? 'bg-emerald-500' : 'bg-primary'
            )}
            style={{ width: `${Math.min(100, allocationPercent)}%` }}
          />
        </div>
        {remainingWeight > 0 && (
          <p className="text-xs text-muted-foreground mt-1">
            {remainingWeight.toFixed(1)}{uom} remaining
          </p>
        )}
        {remainingWeight < 0 && (
          <p className="text-xs text-rose-400 mt-1">
            Over-allocated by {Math.abs(remainingWeight).toFixed(1)}{uom}
          </p>
        )}
      </div>

      {/* Split entries */}
      <div className="flex-1 overflow-y-auto p-6 space-y-4">
        {splits.map((split, index) => (
          <div
            key={split.id}
            className="bg-card/50 rounded-lg border border-border p-4 space-y-3"
          >
            <div className="flex items-center justify-between">
              <span className="font-medium">Lot {index + 1}</span>
              {splits.length > 1 && (
                <button
                  onClick={() => removeSplit(split.id)}
                  className="text-sm text-muted-foreground hover:text-rose-400 transition-colors"
                >
                  Remove
                </button>
              )}
            </div>

            {/* Quantity */}
            <div>
              <label className="block text-sm text-muted-foreground mb-1">Quantity</label>
              <div className="flex items-center gap-2">
                <input
                  type="number"
                  value={split.quantity || ''}
                  onChange={(e) => updateSplit(split.id, 'quantity', parseFloat(e.target.value) || 0)}
                  placeholder="0.0"
                  step="0.1"
                  min="0"
                  max={totalWeight}
                  className="flex-1 px-3 py-2 bg-background border border-border rounded-md font-mono"
                />
                <span className="text-muted-foreground w-6">{uom}</span>
                {remainingWeight > 0 && (
                  <button
                    onClick={() => allocateRemaining(split.id)}
                    className="px-2 py-1 text-xs bg-muted hover:bg-muted/80 rounded transition-colors"
                  >
                    +{remainingWeight.toFixed(1)}
                  </button>
                )}
              </div>
            </div>

            {/* Quality Grade */}
            <div>
              <label className="block text-sm text-muted-foreground mb-1">Quality Grade</label>
              <div className="grid grid-cols-4 gap-2">
                {QUALITY_GRADES.map(grade => (
                  <button
                    key={grade.value}
                    onClick={() => updateSplit(split.id, 'qualityGrade', grade.value)}
                    className={cn(
                      'px-3 py-2 rounded-md text-sm font-medium transition-colors border',
                      split.qualityGrade === grade.value
                        ? grade.color + ' border-current'
                        : 'bg-muted border-transparent hover:border-border'
                    )}
                  >
                    {grade.value}
                  </button>
                ))}
              </div>
            </div>

            {/* Destination */}
            <div>
              <label className="block text-sm text-muted-foreground mb-1">Destination</label>
              <div className="grid grid-cols-2 gap-2">
                {DESTINATIONS.map(dest => (
                  <button
                    key={dest.value}
                    onClick={() => updateSplit(split.id, 'destination', dest.value)}
                    className={cn(
                      'px-3 py-2 rounded-md text-sm transition-colors border text-left',
                      split.destination === dest.value
                        ? 'bg-primary/10 border-primary text-primary'
                        : 'bg-muted border-transparent hover:border-border'
                    )}
                  >
                    <span className="mr-2">{dest.icon}</span>
                    {dest.label}
                  </button>
                ))}
              </div>
            </div>

            {/* Notes */}
            <div>
              <label className="block text-sm text-muted-foreground mb-1">Notes</label>
              <input
                type="text"
                value={split.notes}
                onChange={(e) => updateSplit(split.id, 'notes', e.target.value)}
                placeholder="Optional notes..."
                className="w-full px-3 py-2 bg-background border border-border rounded-md text-sm"
              />
            </div>
          </div>
        ))}

        {/* Add split button */}
        <button
          onClick={addSplit}
          className="w-full py-3 border-2 border-dashed border-border rounded-lg text-muted-foreground hover:border-primary hover:text-primary transition-colors"
        >
          + Add Another Lot
        </button>
      </div>

      {/* Footer */}
      <div className="px-6 py-4 border-t border-border">
        <div className="flex justify-between items-center mb-3">
          <button
            onClick={distributeEvenly}
            className="text-sm text-primary hover:text-primary/80 transition-colors"
          >
            Distribute Evenly
          </button>
          <span className="text-sm text-muted-foreground">
            Creating {splits.filter(s => s.quantity > 0).length} lot(s)
          </span>
        </div>
        <div className="flex gap-3">
          <button
            onClick={onCancel}
            disabled={isLoading}
            className="flex-1 px-4 py-2 bg-muted hover:bg-muted/80 rounded-md text-sm font-medium transition-colors"
          >
            Cancel
          </button>
          <button
            onClick={handleSubmit}
            disabled={!isValid || isLoading}
            className={cn(
              'flex-1 px-4 py-2 rounded-md text-sm font-medium transition-colors',
              'bg-emerald-600 hover:bg-emerald-500 text-white',
              'disabled:opacity-50 disabled:cursor-not-allowed'
            )}
          >
            {isLoading ? 'Creating Lots...' : 'Create Lots'}
          </button>
        </div>
      </div>
    </div>
  );
}

export default LotSplitInterface;





