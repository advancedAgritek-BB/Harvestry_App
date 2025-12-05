'use client';

import React, { useState, useEffect } from 'react';
import { 
  Scale,
  Plus,
  Minus,
  AlertCircle,
  Package,
  Upload,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import type { InventoryLot, LotAdjustmentRequest, AdjustmentReasonCode } from '../../types';

interface LotAdjustModalProps {
  isOpen: boolean;
  onClose: () => void;
  lot: InventoryLot | null;
  reasonCodes: AdjustmentReasonCode[];
  onAdjust: (request: LotAdjustmentRequest) => Promise<void>;
  isSubmitting?: boolean;
}

export function LotAdjustModal({
  isOpen,
  onClose,
  lot,
  reasonCodes,
  onAdjust,
  isSubmitting = false,
}: LotAdjustModalProps) {
  const [adjustmentType, setAdjustmentType] = useState<'increase' | 'decrease'>('decrease');
  const [quantity, setQuantity] = useState<number>(0);
  const [reasonCode, setReasonCode] = useState<string>('');
  const [notes, setNotes] = useState('');
  const [error, setError] = useState<string | null>(null);
  
  // Get filtered reason codes based on adjustment type
  const filteredReasonCodes = reasonCodes.filter(
    (rc) => rc.category === adjustmentType || rc.category === 'both'
  );
  
  const selectedReasonCode = reasonCodes.find((rc) => rc.code === reasonCode);
  
  // Reset state when modal opens
  useEffect(() => {
    if (isOpen && lot) {
      setAdjustmentType('decrease');
      setQuantity(0);
      setReasonCode('');
      setNotes('');
      setError(null);
    }
  }, [isOpen, lot]);
  
  if (!isOpen || !lot) return null;
  
  const newQuantity = adjustmentType === 'increase' 
    ? lot.quantity + quantity 
    : lot.quantity - quantity;
  
  const isValid = 
    quantity > 0 && 
    reasonCode && 
    newQuantity >= 0 &&
    (!selectedReasonCode?.requiresEvidence || notes.length > 0);
  
  const handleSubmit = async () => {
    if (!isValid) {
      setError('Please fill in all required fields');
      return;
    }
    
    if (newQuantity < 0) {
      setError('Cannot adjust below zero');
      return;
    }
    
    try {
      await onAdjust({
        lotId: lot.id,
        quantityChange: adjustmentType === 'increase' ? quantity : -quantity,
        reasonCode,
        notes: notes || undefined,
      });
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to adjust lot');
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
      <div className="relative w-full max-w-md mx-4 bg-surface border border-border rounded-2xl shadow-2xl">
        {/* Header */}
        <div className="flex items-center gap-3 p-5 border-b border-border">
          <div className="w-10 h-10 rounded-xl bg-cyan-500/10 flex items-center justify-center">
            <Scale className="w-5 h-5 text-cyan-400" />
          </div>
          <div>
            <h2 className="text-lg font-semibold text-foreground">Adjust Quantity</h2>
            <p className="text-sm text-muted-foreground">
              Adjust inventory for {lot.lotNumber}
            </p>
          </div>
        </div>
        
        {/* Content */}
        <div className="p-5 space-y-5">
          {/* Current Quantity */}
          <div className="p-4 rounded-xl bg-muted/40 border border-border">
            <div className="flex items-center justify-between">
              <span className="text-sm text-muted-foreground">Current Quantity</span>
              <span className="text-lg font-medium text-foreground tabular-nums">
                {lot.quantity.toLocaleString()} {lot.uom}
              </span>
            </div>
          </div>
          
          {/* Adjustment Type */}
          <div className="flex gap-2">
            <button
              onClick={() => setAdjustmentType('increase')}
              className={cn(
                'flex-1 flex items-center justify-center gap-2 px-4 py-3 rounded-xl text-sm font-medium transition-all',
                adjustmentType === 'increase'
                  ? 'bg-emerald-500/10 border border-emerald-500/30 text-emerald-400'
                  : 'bg-white/5 border border-border text-muted-foreground hover:text-foreground'
              )}
            >
              <Plus className="w-4 h-4" />
              Increase
            </button>
            <button
              onClick={() => setAdjustmentType('decrease')}
              className={cn(
                'flex-1 flex items-center justify-center gap-2 px-4 py-3 rounded-xl text-sm font-medium transition-all',
                adjustmentType === 'decrease'
                  ? 'bg-rose-500/10 border border-rose-500/30 text-rose-400'
                  : 'bg-white/5 border border-border text-muted-foreground hover:text-foreground'
              )}
            >
              <Minus className="w-4 h-4" />
              Decrease
            </button>
          </div>
          
          {/* Quantity Input */}
          <div>
            <label className="block text-sm font-medium text-foreground mb-1.5">
              Adjustment Quantity
            </label>
            <div className="flex items-center gap-3">
              <span className={cn(
                'text-xl font-bold',
                adjustmentType === 'increase' ? 'text-emerald-400' : 'text-rose-400'
              )}>
                {adjustmentType === 'increase' ? '+' : '-'}
              </span>
              <input
                type="number"
                value={quantity || ''}
                onChange={(e) => {
                  setQuantity(Math.max(0, parseFloat(e.target.value) || 0));
                  setError(null);
                }}
                placeholder="0"
                className={cn(
                  'flex-1 px-4 py-3 rounded-xl text-lg font-medium',
                  'bg-white/5 border border-border text-foreground tabular-nums',
                  'focus:outline-none focus:border-cyan-500/30',
                  'placeholder:text-muted-foreground'
                )}
              />
              <span className="text-sm text-muted-foreground w-12">{lot.uom}</span>
            </div>
          </div>
          
          {/* New Quantity Preview */}
          {quantity > 0 && (
            <div className={cn(
              'p-3 rounded-xl flex items-center justify-between',
              newQuantity >= 0 
                ? 'bg-muted/40 border border-border' 
                : 'bg-rose-500/10 border border-rose-500/20'
            )}>
              <span className="text-sm text-muted-foreground">New Quantity:</span>
              <span className={cn(
                'text-lg font-medium tabular-nums',
                newQuantity >= 0 ? 'text-foreground' : 'text-rose-400'
              )}>
                {newQuantity.toLocaleString()} {lot.uom}
              </span>
            </div>
          )}
          
          {/* Reason Code */}
          <div>
            <label className="block text-sm font-medium text-foreground mb-1.5">
              Reason Code <span className="text-rose-400">*</span>
            </label>
            <select
              value={reasonCode}
              onChange={(e) => {
                setReasonCode(e.target.value);
                setError(null);
              }}
              className={cn(
                'w-full px-4 py-3 rounded-xl text-sm',
                'bg-white/5 border border-border text-foreground',
                'focus:outline-none focus:border-cyan-500/30',
                !reasonCode && 'text-muted-foreground'
              )}
            >
              <option value="">Select a reason...</option>
              {filteredReasonCodes.map((rc) => (
                <option key={rc.code} value={rc.code}>
                  {rc.label}
                  {rc.requiresApproval && ' (Requires Approval)'}
                </option>
              ))}
            </select>
          </div>
          
          {/* Notes */}
          <div>
            <label className="block text-sm font-medium text-foreground mb-1.5">
              Notes {selectedReasonCode?.requiresEvidence && <span className="text-rose-400">*</span>}
            </label>
            <textarea
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              placeholder={selectedReasonCode?.requiresEvidence 
                ? 'Please provide details for this adjustment...' 
                : 'Optional notes...'
              }
              rows={3}
              className={cn(
                'w-full px-4 py-3 rounded-xl text-sm',
                'bg-white/5 border border-border text-foreground',
                'focus:outline-none focus:border-cyan-500/30',
                'placeholder:text-muted-foreground resize-none'
              )}
            />
          </div>
          
          {/* Evidence Upload (if required) */}
          {selectedReasonCode?.requiresEvidence && (
            <div>
              <label className="block text-sm font-medium text-foreground mb-1.5">
                Evidence
              </label>
              <button className="w-full flex items-center justify-center gap-2 p-4 rounded-xl border border-dashed border-border text-muted-foreground hover:text-foreground hover:border-white/20 transition-colors">
                <Upload className="w-4 h-4" />
                <span className="text-sm">Upload supporting documents</span>
              </button>
            </div>
          )}
          
          {/* Compliance Warning */}
          {selectedReasonCode?.complianceReportable && (
            <div className="flex items-start gap-2 p-3 rounded-xl bg-amber-500/10 border border-amber-500/20 text-amber-400">
              <AlertCircle className="w-4 h-4 shrink-0 mt-0.5" />
              <p className="text-xs">
                This adjustment will be reported to compliance systems (METRC/BioTrack).
              </p>
            </div>
          )}
          
          {/* Error */}
          {error && (
            <div className="flex items-center gap-2 p-3 rounded-xl bg-rose-500/10 border border-rose-500/20 text-rose-400 text-sm">
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
                ? 'bg-cyan-500 hover:bg-cyan-400 text-black'
                : 'bg-white/10 text-muted-foreground cursor-not-allowed'
            )}
          >
            <Scale className="w-4 h-4" />
            {isSubmitting ? 'Saving...' : 'Apply Adjustment'}
          </button>
        </div>
      </div>
    </div>
  );
}

export default LotAdjustModal;

