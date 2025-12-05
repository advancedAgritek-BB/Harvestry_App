'use client';

import React, { useState, useEffect } from 'react';
import { 
  Scissors,
  Plus,
  Trash2,
  AlertCircle,
  MapPin,
  Package,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import type { InventoryLot, LotSplitRequest } from '../../types';

interface LotSplitModalProps {
  isOpen: boolean;
  onClose: () => void;
  lot: InventoryLot | null;
  onSplit: (request: LotSplitRequest) => Promise<void>;
  isSubmitting?: boolean;
}

interface SplitRow {
  id: string;
  quantity: number;
  locationId?: string;
}

function generateId(): string {
  return Math.random().toString(36).substring(2, 9);
}

export function LotSplitModal({
  isOpen,
  onClose,
  lot,
  onSplit,
  isSubmitting = false,
}: LotSplitModalProps) {
  const [splits, setSplits] = useState<SplitRow[]>([
    { id: generateId(), quantity: 0 },
    { id: generateId(), quantity: 0 },
  ]);
  const [notes, setNotes] = useState('');
  const [error, setError] = useState<string | null>(null);
  
  // Reset state when modal opens
  useEffect(() => {
    if (isOpen && lot) {
      const halfQuantity = Math.floor(lot.quantity / 2);
      setSplits([
        { id: generateId(), quantity: halfQuantity },
        { id: generateId(), quantity: lot.quantity - halfQuantity },
      ]);
      setNotes('');
      setError(null);
    }
  }, [isOpen, lot]);
  
  if (!isOpen || !lot) return null;
  
  const totalAllocated = splits.reduce((sum, s) => sum + (s.quantity || 0), 0);
  const remaining = lot.quantity - totalAllocated;
  const isValid = remaining === 0 && splits.every((s) => s.quantity > 0);
  
  const handleAddSplit = () => {
    setSplits([...splits, { id: generateId(), quantity: 0 }]);
  };
  
  const handleRemoveSplit = (id: string) => {
    if (splits.length <= 2) return;
    setSplits(splits.filter((s) => s.id !== id));
  };
  
  const handleQuantityChange = (id: string, value: number) => {
    setSplits(splits.map((s) => 
      s.id === id ? { ...s, quantity: Math.max(0, value) } : s
    ));
    setError(null);
  };
  
  const handleDistributeEvenly = () => {
    const count = splits.length;
    const baseQuantity = Math.floor(lot.quantity / count);
    const remainder = lot.quantity % count;
    
    setSplits(splits.map((s, i) => ({
      ...s,
      quantity: baseQuantity + (i < remainder ? 1 : 0),
    })));
  };
  
  const handleSubmit = async () => {
    if (!isValid) {
      setError('Total quantity must equal the source lot quantity');
      return;
    }
    
    try {
      await onSplit({
        sourceLotId: lot.id,
        quantities: splits.map((s) => s.quantity),
        destinationLocationIds: splits.map((s) => s.locationId).filter(Boolean) as string[],
        notes: notes || undefined,
      });
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to split lot');
    }
  };
  
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      {/* Backdrop */}
      <div 
        className="absolute inset-0 bg-black/60 backdrop-blur-sm"
        onClick={onClose}
      />
      
      {/* Modal */}
      <div className="relative w-full max-w-lg mx-4 bg-surface border border-border rounded-2xl shadow-2xl">
        {/* Header */}
        <div className="flex items-center gap-3 p-5 border-b border-border">
          <div className="w-10 h-10 rounded-xl bg-amber-500/10 flex items-center justify-center">
            <Scissors className="w-5 h-5 text-amber-400" />
          </div>
          <div>
            <h2 className="text-lg font-semibold text-foreground">Split Lot</h2>
            <p className="text-sm text-muted-foreground">
              Divide {lot.lotNumber} into multiple lots
            </p>
          </div>
        </div>
        
        {/* Content */}
        <div className="p-5 space-y-5">
          {/* Source Lot Info */}
          <div className="p-4 rounded-xl bg-muted/40 border border-border">
            <div className="flex items-center justify-between mb-2">
              <span className="font-mono text-sm text-foreground">{lot.lotNumber}</span>
              <span className="text-sm text-muted-foreground">{lot.productName}</span>
            </div>
            <div className="flex items-center gap-4 text-sm">
              <div className="flex items-center gap-1.5">
                <Package className="w-4 h-4 text-muted-foreground" />
                <span className="text-foreground tabular-nums">
                  {lot.quantity.toLocaleString()} {lot.uom}
                </span>
              </div>
              <div className="flex items-center gap-1.5">
                <MapPin className="w-4 h-4 text-muted-foreground" />
                <span className="text-muted-foreground truncate">{lot.locationPath}</span>
              </div>
            </div>
          </div>
          
          {/* Split Rows */}
          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <h3 className="text-sm font-medium text-foreground">Split Quantities</h3>
              <button
                onClick={handleDistributeEvenly}
                className="text-xs text-amber-400 hover:text-amber-300 transition-colors"
              >
                Distribute Evenly
              </button>
            </div>
            
            {splits.map((split, index) => (
              <div key={split.id} className="flex items-center gap-3">
                <span className="w-6 h-6 rounded-full bg-white/5 flex items-center justify-center text-xs text-muted-foreground">
                  {index + 1}
                </span>
                
                <div className="flex-1">
                  <input
                    type="number"
                    value={split.quantity || ''}
                    onChange={(e) => handleQuantityChange(split.id, parseFloat(e.target.value) || 0)}
                    placeholder="Quantity"
                    className={cn(
                      'w-full px-3 py-2 rounded-lg text-sm',
                      'bg-white/5 border border-border text-foreground tabular-nums',
                      'focus:outline-none focus:border-amber-500/30',
                      'placeholder:text-muted-foreground'
                    )}
                  />
                </div>
                
                <span className="text-sm text-muted-foreground w-12">{lot.uom}</span>
                
                {splits.length > 2 && (
                  <button
                    onClick={() => handleRemoveSplit(split.id)}
                    className="p-2 rounded-lg hover:bg-rose-500/10 text-muted-foreground hover:text-rose-400 transition-colors"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                )}
              </div>
            ))}
            
            <button
              onClick={handleAddSplit}
              className="flex items-center gap-2 w-full px-3 py-2 rounded-lg border border-dashed border-border text-sm text-muted-foreground hover:text-foreground hover:border-white/20 transition-colors"
            >
              <Plus className="w-4 h-4" />
              Add Another Split
            </button>
          </div>
          
          {/* Remaining Indicator */}
          <div className={cn(
            'p-3 rounded-lg flex items-center justify-between',
            remaining === 0 
              ? 'bg-emerald-500/10 border border-emerald-500/20' 
              : remaining > 0
                ? 'bg-amber-500/10 border border-amber-500/20'
                : 'bg-rose-500/10 border border-rose-500/20'
          )}>
            <span className="text-sm text-muted-foreground">Remaining to allocate:</span>
            <span className={cn(
              'text-sm font-medium tabular-nums',
              remaining === 0 
                ? 'text-emerald-400' 
                : remaining > 0
                  ? 'text-amber-400'
                  : 'text-rose-400'
            )}>
              {remaining.toLocaleString()} {lot.uom}
            </span>
          </div>
          
          {/* Notes */}
          <div>
            <label className="block text-sm font-medium text-foreground mb-1.5">
              Notes (optional)
            </label>
            <textarea
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              placeholder="Reason for split..."
              rows={2}
              className={cn(
                'w-full px-3 py-2 rounded-lg text-sm',
                'bg-white/5 border border-border text-foreground',
                'focus:outline-none focus:border-amber-500/30',
                'placeholder:text-muted-foreground resize-none'
              )}
            />
          </div>
          
          {/* Error */}
          {error && (
            <div className="flex items-center gap-2 p-3 rounded-lg bg-rose-500/10 border border-rose-500/20 text-rose-400 text-sm">
              <AlertCircle className="w-4 h-4 shrink-0" />
              {error}
            </div>
          )}
        </div>
        
        {/* Footer */}
        <div className="flex items-center justify-end gap-3 p-5 border-t border-border">
          <button
            onClick={onClose}
            disabled={isSubmitting}
            className="px-4 py-2 rounded-lg text-sm font-medium text-muted-foreground hover:text-foreground hover:bg-white/5 transition-colors"
          >
            Cancel
          </button>
          <button
            onClick={handleSubmit}
            disabled={!isValid || isSubmitting}
            className={cn(
              'flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition-colors',
              isValid && !isSubmitting
                ? 'bg-amber-500 hover:bg-amber-400 text-black'
                : 'bg-white/10 text-muted-foreground cursor-not-allowed'
            )}
          >
            <Scissors className="w-4 h-4" />
            {isSubmitting ? 'Splitting...' : `Split into ${splits.length} Lots`}
          </button>
        </div>
      </div>
    </div>
  );
}

export default LotSplitModal;

