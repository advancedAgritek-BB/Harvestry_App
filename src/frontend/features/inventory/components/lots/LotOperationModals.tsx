'use client';

import React, { useState } from 'react';
import { X, Scissors, Layers, Plus, Minus, MapPin, AlertTriangle } from 'lucide-react';
import { cn } from '@/lib/utils';
import type { InventoryLot, AdjustmentReasonCode } from '../../types';

interface ModalProps {
  isOpen: boolean;
  onClose: () => void;
}

interface LotSplitModalProps extends ModalProps {
  lot: InventoryLot;
  onSplit: (quantities: number[], locationIds?: string[]) => Promise<void>;
}

interface LotMergeModalProps extends ModalProps {
  lots: InventoryLot[];
  onMerge: (destinationLocationId: string) => Promise<void>;
}

interface LotAdjustModalProps extends ModalProps {
  lot: InventoryLot;
  onAdjust: (quantityChange: number, reasonCode: AdjustmentReasonCode, notes?: string) => Promise<void>;
}

interface LotTransferModalProps extends ModalProps {
  lot: InventoryLot;
  onTransfer: (destinationLocationId: string, quantity: number) => Promise<void>;
}

const REASON_CODES: { value: AdjustmentReasonCode; label: string }[] = [
  { value: 'damage', label: 'Damage' },
  { value: 'theft', label: 'Theft/Loss' },
  { value: 'spoilage', label: 'Spoilage' },
  { value: 'measurement_error', label: 'Measurement Error' },
  { value: 'cycle_count', label: 'Cycle Count' },
  { value: 'quality_issue', label: 'Quality Issue' },
  { value: 'contamination', label: 'Contamination' },
  { value: 'sample', label: 'Sample' },
  { value: 'other', label: 'Other' },
];

function ModalWrapper({ isOpen, onClose, title, children }: {
  isOpen: boolean;
  onClose: () => void;
  title: string;
  children: React.ReactNode;
}) {
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="absolute inset-0 bg-black/60 backdrop-blur-sm" onClick={onClose} />
      <div className="relative w-full max-w-lg mx-4 bg-surface border border-border rounded-xl shadow-2xl">
        <div className="flex items-center justify-between px-5 py-4 border-b border-border">
          <h2 className="text-lg font-semibold text-foreground">{title}</h2>
          <button
            onClick={onClose}
            className="p-1.5 rounded-lg hover:bg-white/5 text-muted-foreground hover:text-foreground transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>
        {children}
      </div>
    </div>
  );
}

/**
 * Split Lot Modal
 * Allows splitting a lot into multiple child lots
 */
export function LotSplitModal({ isOpen, onClose, lot, onSplit }: LotSplitModalProps) {
  const [splits, setSplits] = useState<{ quantity: number; locationId?: string }[]>([
    { quantity: lot.quantity / 2 },
    { quantity: lot.quantity / 2 },
  ]);
  const [loading, setLoading] = useState(false);

  const totalSplit = splits.reduce((sum, s) => sum + s.quantity, 0);
  const isValid = Math.abs(totalSplit - lot.quantity) < 0.01 && splits.every(s => s.quantity > 0);

  const handleAddSplit = () => {
    setSplits([...splits, { quantity: 0 }]);
  };

  const handleRemoveSplit = (index: number) => {
    if (splits.length > 2) {
      setSplits(splits.filter((_, i) => i !== index));
    }
  };

  const handleQuantityChange = (index: number, value: number) => {
    const newSplits = [...splits];
    newSplits[index].quantity = value;
    setSplits(newSplits);
  };

  const handleSubmit = async () => {
    if (!isValid) return;
    setLoading(true);
    try {
      await onSplit(
        splits.map(s => s.quantity),
        splits.map(s => s.locationId).filter(Boolean) as string[]
      );
      onClose();
    } catch (error) {
      console.error('Split failed:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <ModalWrapper isOpen={isOpen} onClose={onClose} title="Split Lot">
      <div className="p-5 space-y-4">
        {/* Source Lot Info */}
        <div className="p-3 rounded-lg bg-muted/30 border border-border">
          <div className="flex items-center gap-3">
            <div className="w-8 h-8 rounded-lg bg-cyan-500/10 flex items-center justify-center">
              <Scissors className="w-4 h-4 text-cyan-400" />
            </div>
            <div>
              <div className="text-sm font-mono text-foreground">{lot.lotNumber}</div>
              <div className="text-xs text-muted-foreground">
                Available: {lot.quantity.toLocaleString()} {lot.uom}
              </div>
            </div>
          </div>
        </div>

        {/* Split Quantities */}
        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <label className="text-sm font-medium text-foreground">Split Into</label>
            <button
              onClick={handleAddSplit}
              className="flex items-center gap-1 text-xs text-cyan-400 hover:underline"
            >
              <Plus className="w-3 h-3" />
              Add Split
            </button>
          </div>

          {splits.map((split, index) => (
            <div key={index} className="flex items-center gap-3">
              <div className="flex-1">
                <input
                  type="number"
                  value={split.quantity}
                  onChange={(e) => handleQuantityChange(index, parseFloat(e.target.value) || 0)}
                  className="w-full px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground focus:outline-none focus:border-cyan-500/30"
                  placeholder="Quantity"
                />
              </div>
              <span className="text-sm text-muted-foreground w-8">{lot.uom}</span>
              {splits.length > 2 && (
                <button
                  onClick={() => handleRemoveSplit(index)}
                  className="p-1.5 rounded-lg hover:bg-rose-500/10 text-muted-foreground hover:text-rose-400 transition-colors"
                >
                  <X className="w-4 h-4" />
                </button>
              )}
            </div>
          ))}
        </div>

        {/* Validation */}
        <div className={cn(
          'p-3 rounded-lg',
          isValid ? 'bg-emerald-500/10 border border-emerald-500/20' : 'bg-rose-500/10 border border-rose-500/20'
        )}>
          <div className="flex items-center justify-between text-sm">
            <span className={isValid ? 'text-emerald-400' : 'text-rose-400'}>
              {isValid ? 'Quantities balance correctly' : 'Quantities must equal source quantity'}
            </span>
            <span className={cn('font-mono', isValid ? 'text-emerald-400' : 'text-rose-400')}>
              {totalSplit.toFixed(1)} / {lot.quantity.toFixed(1)} {lot.uom}
            </span>
          </div>
        </div>
      </div>

      {/* Footer */}
      <div className="px-5 py-4 border-t border-border flex justify-end gap-3">
        <button
          onClick={onClose}
          className="px-4 py-2 rounded-lg bg-white/5 text-foreground hover:bg-white/10 transition-colors"
        >
          Cancel
        </button>
        <button
          onClick={handleSubmit}
          disabled={!isValid || loading}
          className="px-4 py-2 rounded-lg bg-cyan-500 text-black font-medium hover:bg-cyan-400 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        >
          {loading ? 'Splitting...' : 'Split Lot'}
        </button>
      </div>
    </ModalWrapper>
  );
}

/**
 * Adjust Lot Modal
 * Allows adjusting lot quantity with reason codes
 */
export function LotAdjustModal({ isOpen, onClose, lot, onAdjust }: LotAdjustModalProps) {
  const [adjustmentType, setAdjustmentType] = useState<'add' | 'remove'>('remove');
  const [quantity, setQuantity] = useState(0);
  const [reasonCode, setReasonCode] = useState<AdjustmentReasonCode>('cycle_count');
  const [notes, setNotes] = useState('');
  const [loading, setLoading] = useState(false);

  const adjustedQuantity = adjustmentType === 'add' 
    ? lot.quantity + quantity 
    : lot.quantity - quantity;
  const isValid = quantity > 0 && adjustedQuantity >= 0;

  const handleSubmit = async () => {
    if (!isValid) return;
    setLoading(true);
    try {
      const change = adjustmentType === 'add' ? quantity : -quantity;
      await onAdjust(change, reasonCode, notes || undefined);
      onClose();
    } catch (error) {
      console.error('Adjustment failed:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <ModalWrapper isOpen={isOpen} onClose={onClose} title="Adjust Lot Quantity">
      <div className="p-5 space-y-4">
        {/* Source Lot Info */}
        <div className="p-3 rounded-lg bg-muted/30 border border-border">
          <div className="flex items-center justify-between">
            <div>
              <div className="text-sm font-mono text-foreground">{lot.lotNumber}</div>
              <div className="text-xs text-muted-foreground">{lot.strainName}</div>
            </div>
            <div className="text-right">
              <div className="text-lg font-bold text-foreground tabular-nums">
                {lot.quantity.toLocaleString()} {lot.uom}
              </div>
              <div className="text-xs text-muted-foreground">Current Quantity</div>
            </div>
          </div>
        </div>

        {/* Adjustment Type Toggle */}
        <div className="flex rounded-lg bg-muted/30 p-1">
          <button
            onClick={() => setAdjustmentType('remove')}
            className={cn(
              'flex-1 flex items-center justify-center gap-2 py-2 rounded-lg text-sm font-medium transition-colors',
              adjustmentType === 'remove' 
                ? 'bg-rose-500/10 text-rose-400' 
                : 'text-muted-foreground hover:text-foreground'
            )}
          >
            <Minus className="w-4 h-4" />
            Remove
          </button>
          <button
            onClick={() => setAdjustmentType('add')}
            className={cn(
              'flex-1 flex items-center justify-center gap-2 py-2 rounded-lg text-sm font-medium transition-colors',
              adjustmentType === 'add' 
                ? 'bg-emerald-500/10 text-emerald-400' 
                : 'text-muted-foreground hover:text-foreground'
            )}
          >
            <Plus className="w-4 h-4" />
            Add
          </button>
        </div>

        {/* Quantity */}
        <div>
          <label className="block text-sm font-medium text-foreground mb-2">Quantity</label>
          <div className="flex items-center gap-3">
            <input
              type="number"
              value={quantity}
              onChange={(e) => setQuantity(parseFloat(e.target.value) || 0)}
              className="flex-1 px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground focus:outline-none focus:border-cyan-500/30"
              placeholder="0"
            />
            <span className="text-sm text-muted-foreground w-8">{lot.uom}</span>
          </div>
        </div>

        {/* Reason Code */}
        <div>
          <label className="block text-sm font-medium text-foreground mb-2">Reason</label>
          <select
            value={reasonCode}
            onChange={(e) => setReasonCode(e.target.value as AdjustmentReasonCode)}
            className="w-full px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground focus:outline-none focus:border-cyan-500/30"
          >
            {REASON_CODES.map((rc) => (
              <option key={rc.value} value={rc.value}>{rc.label}</option>
            ))}
          </select>
        </div>

        {/* Notes */}
        <div>
          <label className="block text-sm font-medium text-foreground mb-2">Notes (optional)</label>
          <textarea
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            rows={2}
            className="w-full px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-cyan-500/30 resize-none"
            placeholder="Add details about this adjustment..."
          />
        </div>

        {/* Result Preview */}
        <div className={cn(
          'p-3 rounded-lg border',
          isValid ? 'bg-muted/30 border-border' : 'bg-rose-500/10 border-rose-500/20'
        )}>
          <div className="flex items-center justify-between text-sm">
            <span className="text-muted-foreground">New Quantity</span>
            <span className={cn('font-mono font-bold', 
              !isValid ? 'text-rose-400' : 'text-foreground'
            )}>
              {adjustedQuantity.toLocaleString()} {lot.uom}
            </span>
          </div>
          {!isValid && adjustedQuantity < 0 && (
            <div className="flex items-center gap-2 mt-2 text-xs text-rose-400">
              <AlertTriangle className="w-3 h-3" />
              Cannot remove more than available quantity
            </div>
          )}
        </div>
      </div>

      {/* Footer */}
      <div className="px-5 py-4 border-t border-border flex justify-end gap-3">
        <button
          onClick={onClose}
          className="px-4 py-2 rounded-lg bg-white/5 text-foreground hover:bg-white/10 transition-colors"
        >
          Cancel
        </button>
        <button
          onClick={handleSubmit}
          disabled={!isValid || loading}
          className={cn(
            'px-4 py-2 rounded-lg font-medium disabled:opacity-50 disabled:cursor-not-allowed transition-colors',
            adjustmentType === 'remove' 
              ? 'bg-rose-500 text-foreground hover:bg-rose-400' 
              : 'bg-emerald-500 text-foreground hover:bg-emerald-400'
          )}
        >
          {loading ? 'Adjusting...' : `${adjustmentType === 'add' ? 'Add' : 'Remove'} Quantity`}
        </button>
      </div>
    </ModalWrapper>
  );
}

/**
 * Transfer Lot Modal
 * Allows moving a lot to a different location
 */
export function LotTransferModal({ isOpen, onClose, lot, onTransfer }: LotTransferModalProps) {
  const [quantity, setQuantity] = useState(lot.quantity);
  const [destinationId, setDestinationId] = useState('');
  const [loading, setLoading] = useState(false);

  const isValid = quantity > 0 && quantity <= lot.quantity && destinationId;

  const handleSubmit = async () => {
    if (!isValid) return;
    setLoading(true);
    try {
      await onTransfer(destinationId, quantity);
      onClose();
    } catch (error) {
      console.error('Transfer failed:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <ModalWrapper isOpen={isOpen} onClose={onClose} title="Move Lot">
      <div className="p-5 space-y-4">
        {/* Source Info */}
        <div className="p-3 rounded-lg bg-muted/30 border border-border">
          <div className="flex items-center gap-3">
            <MapPin className="w-4 h-4 text-muted-foreground" />
            <div>
              <div className="text-xs text-muted-foreground">From</div>
              <div className="text-sm text-foreground">{lot.locationPath}</div>
            </div>
          </div>
        </div>

        {/* Destination */}
        <div>
          <label className="block text-sm font-medium text-foreground mb-2">Destination Location</label>
          <select
            value={destinationId}
            onChange={(e) => setDestinationId(e.target.value)}
            className="w-full px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground focus:outline-none focus:border-cyan-500/30"
          >
            <option value="">Select destination...</option>
            <option value="loc-1">Vault A &gt; Rack 1 &gt; Shelf B</option>
            <option value="loc-2">Vault A &gt; Rack 2 &gt; Shelf A</option>
            <option value="loc-3">Warehouse B &gt; Zone 1</option>
          </select>
        </div>

        {/* Quantity */}
        <div>
          <label className="block text-sm font-medium text-foreground mb-2">Quantity to Move</label>
          <div className="flex items-center gap-3">
            <input
              type="number"
              value={quantity}
              onChange={(e) => setQuantity(parseFloat(e.target.value) || 0)}
              max={lot.quantity}
              className="flex-1 px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground focus:outline-none focus:border-cyan-500/30"
            />
            <span className="text-sm text-muted-foreground">{lot.uom}</span>
            <button
              onClick={() => setQuantity(lot.quantity)}
              className="px-3 py-2 rounded-lg bg-white/5 text-xs text-foreground hover:bg-white/10 transition-colors"
            >
              All
            </button>
          </div>
          <div className="text-xs text-muted-foreground mt-1">
            Available: {lot.quantity.toLocaleString()} {lot.uom}
          </div>
        </div>
      </div>

      {/* Footer */}
      <div className="px-5 py-4 border-t border-border flex justify-end gap-3">
        <button
          onClick={onClose}
          className="px-4 py-2 rounded-lg bg-white/5 text-foreground hover:bg-white/10 transition-colors"
        >
          Cancel
        </button>
        <button
          onClick={handleSubmit}
          disabled={!isValid || loading}
          className="px-4 py-2 rounded-lg bg-cyan-500 text-black font-medium hover:bg-cyan-400 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        >
          {loading ? 'Moving...' : 'Move Lot'}
        </button>
      </div>
    </ModalWrapper>
  );
}

export { LotSplitModal as default };

