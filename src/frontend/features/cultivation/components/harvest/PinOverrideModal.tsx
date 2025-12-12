'use client';

/**
 * PinOverrideModal Component
 * Secure PIN entry for locked weight adjustments with audit trail
 */

import { useState, useRef, useEffect } from 'react';
import { cn } from '@/lib/utils';
import { WEIGHT_ADJUSTMENT_REASONS, type WeightAdjustmentReasonCode } from '@/features/inventory/types';

interface PinOverrideModalProps {
  /** Whether the modal is open */
  isOpen: boolean;
  /** Callback to close the modal */
  onClose: () => void;
  /** Callback when PIN is validated and adjustment submitted */
  onSubmit: (pin: string, reasonCode: WeightAdjustmentReasonCode, notes?: string) => Promise<void>;
  /** Current weight being adjusted */
  currentWeight: number;
  /** New weight value */
  newWeight: number;
  /** Unit of measurement */
  uom?: string;
  /** Weight type being adjusted */
  weightType: string;
  /** Plant tag if adjusting individual plant */
  plantTag?: string;
  /** Loading state */
  isLoading?: boolean;
  /** Error message */
  error?: string | null;
}

export function PinOverrideModal({
  isOpen,
  onClose,
  onSubmit,
  currentWeight,
  newWeight,
  uom = 'g',
  weightType,
  plantTag,
  isLoading = false,
  error = null,
}: PinOverrideModalProps) {
  const [pin, setPin] = useState('');
  const [reasonCode, setReasonCode] = useState<WeightAdjustmentReasonCode>('RECOUNTED');
  const [notes, setNotes] = useState('');
  const [showPin, setShowPin] = useState(false);
  const pinInputRef = useRef<HTMLInputElement>(null);

  // Focus PIN input when modal opens
  useEffect(() => {
    if (isOpen && pinInputRef.current) {
      pinInputRef.current.focus();
    }
  }, [isOpen]);

  // Reset form when modal closes
  useEffect(() => {
    if (!isOpen) {
      setPin('');
      setReasonCode('RECOUNTED');
      setNotes('');
      setShowPin(false);
    }
  }, [isOpen]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (pin.length < 4) return;
    
    await onSubmit(pin, reasonCode, notes || undefined);
  };

  const adjustment = newWeight - currentWeight;
  const adjustmentPercent = currentWeight > 0 
    ? ((adjustment / currentWeight) * 100).toFixed(1)
    : '0';

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      {/* Backdrop */}
      <div 
        className="absolute inset-0 bg-black/60 backdrop-blur-sm"
        onClick={onClose}
      />
      
      {/* Modal */}
      <div className="relative bg-card border border-border rounded-lg shadow-xl w-full max-w-md mx-4">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-border">
          <h2 className="text-lg font-semibold">PIN Override Required</h2>
          <button
            onClick={onClose}
            className="text-muted-foreground hover:text-foreground transition-colors"
          >
            ✕
          </button>
        </div>
        
        {/* Content */}
        <form onSubmit={handleSubmit} className="p-6 space-y-4">
          {/* Weight change summary */}
          <div className="bg-muted/50 rounded-lg p-4">
            <div className="text-sm text-muted-foreground mb-2">
              {plantTag ? `Adjusting ${weightType} for ${plantTag}` : `Adjusting ${weightType}`}
            </div>
            <div className="flex items-center justify-between">
              <div>
                <div className="text-xs text-muted-foreground">Current</div>
                <div className="text-lg font-mono">{currentWeight.toFixed(1)}{uom}</div>
              </div>
              <div className="text-2xl text-muted-foreground">→</div>
              <div>
                <div className="text-xs text-muted-foreground">New</div>
                <div className="text-lg font-mono">{newWeight.toFixed(1)}{uom}</div>
              </div>
              <div className={cn(
                'px-2 py-1 rounded text-sm font-mono',
                adjustment > 0 ? 'bg-emerald-500/10 text-emerald-400' : 'bg-rose-500/10 text-rose-400'
              )}>
                {adjustment > 0 ? '+' : ''}{adjustment.toFixed(1)}{uom}
                <span className="text-xs ml-1">({adjustmentPercent}%)</span>
              </div>
            </div>
          </div>
          
          {/* Reason code */}
          <div>
            <label className="block text-sm font-medium mb-1.5">
              Reason for Adjustment *
            </label>
            <select
              value={reasonCode}
              onChange={(e) => setReasonCode(e.target.value as WeightAdjustmentReasonCode)}
              className="w-full px-3 py-2 bg-background border border-border rounded-md text-sm"
              required
            >
              {Object.entries(WEIGHT_ADJUSTMENT_REASONS).map(([code, label]) => (
                <option key={code} value={code}>{label}</option>
              ))}
            </select>
          </div>
          
          {/* Notes */}
          <div>
            <label className="block text-sm font-medium mb-1.5">
              Notes {reasonCode === 'OTHER' && '*'}
            </label>
            <textarea
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              placeholder="Additional details about this adjustment..."
              className="w-full px-3 py-2 bg-background border border-border rounded-md text-sm resize-none"
              rows={2}
              required={reasonCode === 'OTHER'}
            />
          </div>
          
          {/* PIN entry */}
          <div>
            <label className="block text-sm font-medium mb-1.5">
              Enter your PIN *
            </label>
            <div className="relative">
              <input
                ref={pinInputRef}
                type={showPin ? 'text' : 'password'}
                value={pin}
                onChange={(e) => setPin(e.target.value.replace(/\D/g, '').slice(0, 6))}
                placeholder="••••"
                className="w-full px-3 py-2 bg-background border border-border rounded-md text-sm font-mono tracking-widest text-center text-lg"
                pattern="\d{4,6}"
                inputMode="numeric"
                autoComplete="off"
                required
              />
              <button
                type="button"
                onClick={() => setShowPin(!showPin)}
                className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
              >
                {showPin ? '👁️' : '👁️‍🗨️'}
              </button>
            </div>
            <p className="mt-1 text-xs text-muted-foreground">
              Enter your 4-6 digit override PIN
            </p>
          </div>
          
          {/* Error message */}
          {error && (
            <div className="p-3 bg-rose-500/10 border border-rose-500/20 rounded-md">
              <p className="text-sm text-rose-400">{error}</p>
            </div>
          )}
          
          {/* Actions */}
          <div className="flex gap-3 pt-2">
            <button
              type="button"
              onClick={onClose}
              disabled={isLoading}
              className="flex-1 px-4 py-2 bg-muted hover:bg-muted/80 rounded-md text-sm font-medium transition-colors disabled:opacity-50"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={isLoading || pin.length < 4 || (reasonCode === 'OTHER' && !notes)}
              className={cn(
                'flex-1 px-4 py-2 rounded-md text-sm font-medium transition-colors',
                'bg-amber-600 hover:bg-amber-500 text-white',
                'disabled:opacity-50 disabled:cursor-not-allowed'
              )}
            >
              {isLoading ? 'Verifying...' : 'Confirm Adjustment'}
            </button>
          </div>
          
          {/* Audit notice */}
          <p className="text-xs text-muted-foreground text-center">
            This adjustment will be logged in the audit trail with your user ID.
          </p>
        </form>
      </div>
    </div>
  );
}

export default PinOverrideModal;





