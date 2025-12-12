'use client';

/**
 * WasteRecordingForm Component
 * Quick entry form for recording waste during bucking
 */

import { useState } from 'react';
import { cn } from '@/lib/utils';

interface WasteRecordingFormProps {
  /** Callback when waste is recorded */
  onSubmit: (data: WasteRecordData) => Promise<void>;
  /** Unit of measurement */
  uom?: string;
  /** Current dry weight for validation */
  currentDryWeight?: number;
  /** Whether form is in loading state */
  isLoading?: boolean;
  /** Additional class names */
  className?: string;
}

export interface WasteRecordData {
  buckedFlowerWeight: number;
  stemWaste: number;
  leafWaste: number;
  otherWaste: number;
}

interface WasteCategory {
  key: keyof Omit<WasteRecordData, 'buckedFlowerWeight'>;
  label: string;
  color: string;
  icon: string;
}

const WASTE_CATEGORIES: WasteCategory[] = [
  { key: 'stemWaste', label: 'Stems', color: 'bg-amber-500/20 border-amber-500/40', icon: '🌿' },
  { key: 'leafWaste', label: 'Fan Leaves', color: 'bg-emerald-500/20 border-emerald-500/40', icon: '🍃' },
  { key: 'otherWaste', label: 'Other', color: 'bg-slate-500/20 border-slate-500/40', icon: '📦' },
];

export function WasteRecordingForm({
  onSubmit,
  uom = 'g',
  currentDryWeight = 0,
  isLoading = false,
  className,
}: WasteRecordingFormProps) {
  const [buckedFlowerWeight, setBuckedFlowerWeight] = useState<string>('');
  const [stemWaste, setStemWaste] = useState<string>('');
  const [leafWaste, setLeafWaste] = useState<string>('');
  const [otherWaste, setOtherWaste] = useState<string>('');
  const [error, setError] = useState<string | null>(null);

  const parseWeight = (value: string): number => {
    const parsed = parseFloat(value);
    return isNaN(parsed) ? 0 : parsed;
  };

  const totalWaste = parseWeight(stemWaste) + parseWeight(leafWaste) + parseWeight(otherWaste);
  const totalOutput = parseWeight(buckedFlowerWeight) + totalWaste;
  const variance = currentDryWeight > 0 ? ((totalOutput - currentDryWeight) / currentDryWeight * 100) : 0;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    const flower = parseWeight(buckedFlowerWeight);
    if (flower <= 0) {
      setError('Bucked flower weight is required');
      return;
    }

    // Validate total doesn't exceed reasonable variance from dry weight
    if (currentDryWeight > 0 && Math.abs(variance) > 10) {
      setError(`Total output varies ${Math.abs(variance).toFixed(1)}% from dry weight. Please verify.`);
      return;
    }

    try {
      await onSubmit({
        buckedFlowerWeight: flower,
        stemWaste: parseWeight(stemWaste),
        leafWaste: parseWeight(leafWaste),
        otherWaste: parseWeight(otherWaste),
      });
    } catch (err) {
      setError('Failed to record waste. Please try again.');
    }
  };

  const setValueForCategory = (key: string, value: string) => {
    switch (key) {
      case 'stemWaste':
        setStemWaste(value);
        break;
      case 'leafWaste':
        setLeafWaste(value);
        break;
      case 'otherWaste':
        setOtherWaste(value);
        break;
    }
  };

  const getValueForCategory = (key: string): string => {
    switch (key) {
      case 'stemWaste':
        return stemWaste;
      case 'leafWaste':
        return leafWaste;
      case 'otherWaste':
        return otherWaste;
      default:
        return '';
    }
  };

  return (
    <form onSubmit={handleSubmit} className={cn('space-y-4', className)}>
      {/* Bucked flower weight */}
      <div className="bg-emerald-500/10 border border-emerald-500/30 rounded-lg p-4">
        <label className="block text-sm font-medium mb-2">
          <span className="mr-2">🌸</span>
          Bucked Flower Weight *
        </label>
        <div className="flex items-center gap-2">
          <input
            type="number"
            value={buckedFlowerWeight}
            onChange={(e) => setBuckedFlowerWeight(e.target.value)}
            placeholder="0.0"
            step="0.1"
            min="0"
            className="flex-1 px-4 py-3 bg-background border border-border rounded-md text-lg font-mono text-center"
            required
          />
          <span className="text-lg font-mono text-muted-foreground w-8">{uom}</span>
        </div>
      </div>

      {/* Waste categories */}
      <div className="space-y-3">
        <h4 className="text-sm font-medium text-muted-foreground">Waste Breakdown</h4>
        
        {WASTE_CATEGORIES.map((category) => (
          <div
            key={category.key}
            className={cn('border rounded-lg p-3', category.color)}
          >
            <label className="block text-sm font-medium mb-2">
              <span className="mr-2">{category.icon}</span>
              {category.label}
            </label>
            <div className="flex items-center gap-2">
              <input
                type="number"
                value={getValueForCategory(category.key)}
                onChange={(e) => setValueForCategory(category.key, e.target.value)}
                placeholder="0.0"
                step="0.1"
                min="0"
                className="flex-1 px-3 py-2 bg-background border border-border rounded-md font-mono text-center"
              />
              <span className="font-mono text-muted-foreground w-8">{uom}</span>
            </div>
          </div>
        ))}
      </div>

      {/* Summary */}
      <div className="bg-card/50 border border-border rounded-lg p-4 space-y-2">
        <div className="flex justify-between text-sm">
          <span className="text-muted-foreground">Bucked Flower</span>
          <span className="font-mono">{parseWeight(buckedFlowerWeight).toFixed(1)}{uom}</span>
        </div>
        <div className="flex justify-between text-sm">
          <span className="text-muted-foreground">Total Waste</span>
          <span className="font-mono">{totalWaste.toFixed(1)}{uom}</span>
        </div>
        <div className="border-t border-border pt-2 flex justify-between font-medium">
          <span>Total Output</span>
          <span className="font-mono">{totalOutput.toFixed(1)}{uom}</span>
        </div>
        
        {currentDryWeight > 0 && (
          <div className={cn(
            'flex justify-between text-sm pt-1',
            Math.abs(variance) <= 2 ? 'text-emerald-400' :
            Math.abs(variance) <= 5 ? 'text-amber-400' : 'text-rose-400'
          )}>
            <span>Variance from Dry Weight</span>
            <span className="font-mono">
              {variance > 0 ? '+' : ''}{variance.toFixed(1)}%
            </span>
          </div>
        )}
      </div>

      {/* Error message */}
      {error && (
        <div className="p-3 bg-rose-500/10 border border-rose-500/20 rounded-md">
          <p className="text-sm text-rose-400">{error}</p>
        </div>
      )}

      {/* Submit button */}
      <button
        type="submit"
        disabled={isLoading || !buckedFlowerWeight}
        className={cn(
          'w-full px-4 py-3 rounded-md text-sm font-medium transition-colors',
          'bg-emerald-600 hover:bg-emerald-500 text-white',
          'disabled:opacity-50 disabled:cursor-not-allowed'
        )}
      >
        {isLoading ? 'Recording...' : 'Record Bucking Results'}
      </button>
    </form>
  );
}

export default WasteRecordingForm;





