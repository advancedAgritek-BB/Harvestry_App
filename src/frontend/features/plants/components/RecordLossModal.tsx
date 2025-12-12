'use client';

import React, { useState, useCallback, useEffect } from 'react';
import { cn } from '@/lib/utils';
import {
  X,
  TrendingDown,
  AlertCircle,
  Info,
  Calendar,
  ChevronDown,
  Check,
  Scale,
} from 'lucide-react';
import { usePlantStore } from '../stores';
import {
  DESTROY_REASON_LABELS,
  type PlantDestroyReason,
  type WasteMethod,
  type RecordLossRequest,
} from '../types';

// =============================================================================
// TYPES
// =============================================================================

interface RecordLossModalProps {
  batchId: string;
  batchName: string;
  currentPlantCount: number;
  hasTaggedPlants: boolean;
  requiresWitness?: boolean; // State-specific requirement
}

// =============================================================================
// HELPERS
// =============================================================================

const WASTE_METHOD_LABELS: Record<WasteMethod, string> = {
  grinder: 'Grinder',
  compost: 'Compost',
  incinerator: 'Incinerator',
  mixed_waste: 'Mixed Waste',
};

// =============================================================================
// SELECT COMPONENT
// =============================================================================

interface SelectOption {
  value: string;
  label: string;
}

function SimpleSelect({
  value,
  onChange,
  options,
  placeholder,
}: {
  value: string;
  onChange: (value: string) => void;
  options: SelectOption[];
  placeholder: string;
}) {
  const [isOpen, setIsOpen] = useState(false);
  const selected = options.find((o) => o.value === value);

  return (
    <div className="relative">
      <button
        type="button"
        onClick={() => setIsOpen(!isOpen)}
        className={cn(
          'w-full flex items-center justify-between px-3 py-2.5 bg-muted/30 border rounded-lg text-sm transition-colors',
          isOpen ? 'border-red-500/50' : 'border-border hover:border-red-500/30'
        )}
      >
        <span className={selected ? 'text-foreground' : 'text-muted-foreground'}>
          {selected?.label || placeholder}
        </span>
        <ChevronDown className={cn('w-4 h-4 transition-transform', isOpen && 'rotate-180')} />
      </button>

      {isOpen && (
        <>
          <div className="fixed inset-0 z-40" onClick={() => setIsOpen(false)} />
          <div className="absolute top-full left-0 right-0 mt-1 bg-surface border border-border rounded-lg shadow-xl z-50 max-h-48 overflow-y-auto">
            {options.map((option) => (
              <button
                key={option.value}
                type="button"
                onClick={() => {
                  onChange(option.value);
                  setIsOpen(false);
                }}
                className={cn(
                  'w-full flex items-center justify-between px-3 py-2.5 text-left transition-colors text-sm',
                  option.value === value ? 'bg-red-500/10' : 'hover:bg-muted/50'
                )}
              >
                <span className="text-foreground">{option.label}</span>
                {option.value === value && <Check className="w-4 h-4 text-red-400" />}
              </button>
            ))}
          </div>
        </>
      )}
    </div>
  );
}

// =============================================================================
// MAIN COMPONENT
// =============================================================================

export function RecordLossModal({
  batchId,
  batchName,
  currentPlantCount,
  hasTaggedPlants,
  requiresWitness = false,
}: RecordLossModalProps) {
  const { 
    showRecordLossModal, 
    closeRecordLossModal, 
    recordLoss, 
    plantBatchesByBatch,
    isLoading, 
    error 
  } = usePlantStore();

  // Form state
  const [quantity, setQuantity] = useState(1);
  const [reason, setReason] = useState<PlantDestroyReason>('culled');
  const [reasonNote, setReasonNote] = useState('');
  const [destroyedDate, setDestroyedDate] = useState(new Date().toISOString().split('T')[0]);
  const [wasteWeight, setWasteWeight] = useState<number | ''>('');
  const [wasteMethod, setWasteMethod] = useState<WasteMethod | ''>('');

  // Get plant batch ID
  const plantBatches = plantBatchesByBatch[batchId] || [];
  const plantBatchId = plantBatches[0]?.id;

  // Reset form when modal opens
  useEffect(() => {
    if (showRecordLossModal) {
      setQuantity(1);
      setReason('culled');
      setReasonNote('');
      setDestroyedDate(new Date().toISOString().split('T')[0]);
      setWasteWeight('');
      setWasteMethod('');
    }
  }, [showRecordLossModal]);

  const handleSubmit = useCallback(
    async (e: React.FormEvent) => {
      e.preventDefault();

      const request: RecordLossRequest = {
        batchId,
        plantBatchId,
        quantity,
        reason,
        reasonNote: reasonNote || undefined,
        destroyedDate,
        wasteWeight: wasteWeight !== '' ? wasteWeight : undefined,
        wasteMethod: wasteMethod !== '' ? wasteMethod : undefined,
      };

      try {
        await recordLoss(request);
      } catch {
        // Error handled by store
      }
    },
    [batchId, plantBatchId, quantity, reason, reasonNote, destroyedDate, wasteWeight, wasteMethod, recordLoss]
  );

  const handleClose = useCallback(() => {
    closeRecordLossModal();
  }, [closeRecordLossModal]);

  // Escape key handler
  useEffect(() => {
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && showRecordLossModal) handleClose();
    };
    document.addEventListener('keydown', handleEscape);
    return () => document.removeEventListener('keydown', handleEscape);
  }, [showRecordLossModal, handleClose]);

  if (!showRecordLossModal) return null;

  const canSubmit = quantity > 0 && quantity <= currentPlantCount && reason;

  // Reason options
  const reasonOptions: SelectOption[] = (Object.keys(DESTROY_REASON_LABELS) as PlantDestroyReason[]).map(
    (key) => ({
      value: key,
      label: DESTROY_REASON_LABELS[key],
    })
  );

  // Waste method options
  const wasteMethodOptions: SelectOption[] = (Object.keys(WASTE_METHOD_LABELS) as WasteMethod[]).map(
    (key) => ({
      value: key,
      label: WASTE_METHOD_LABELS[key],
    })
  );

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      {/* Backdrop */}
      <div
        className="absolute inset-0 bg-background/80 backdrop-blur-sm"
        onClick={handleClose}
      />

      {/* Modal */}
      <div className="relative w-full max-w-md bg-surface border border-border rounded-xl shadow-2xl overflow-hidden">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-border">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-red-500/10 flex items-center justify-center">
              <TrendingDown className="w-5 h-5 text-red-400" />
            </div>
            <div>
              <h2 className="text-lg font-semibold text-foreground">Record Loss</h2>
              <p className="text-sm text-muted-foreground">Document plant destruction</p>
            </div>
          </div>
          <button
            onClick={handleClose}
            className="p-2 rounded-lg text-muted-foreground hover:text-foreground hover:bg-muted/50 transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Content */}
        <form onSubmit={handleSubmit} className="p-6 space-y-5">
          {/* Batch Info */}
          <div className="p-3 bg-muted/30 rounded-lg border border-border/50">
            <div className="text-sm font-medium text-foreground">{batchName}</div>
            <div className="text-xs text-muted-foreground mt-1">
              Current: {currentPlantCount} plants
            </div>
          </div>

          {/* Quantity */}
          <div>
            <label className="block text-sm font-medium text-foreground mb-1.5">
              Number of Plants Lost <span className="text-rose-400">*</span>
            </label>
            <input
              type="number"
              value={quantity}
              onChange={(e) => setQuantity(parseInt(e.target.value) || 0)}
              min={1}
              max={currentPlantCount}
              className="w-full px-3 py-2.5 bg-muted/30 border border-border rounded-lg text-sm text-foreground focus:outline-none focus:border-red-500/50"
            />
            {quantity > currentPlantCount && (
              <div className="flex items-center gap-1.5 mt-1.5 text-xs text-red-400">
                <AlertCircle className="w-3 h-3" />
                Cannot exceed current plant count
              </div>
            )}
          </div>

          {/* Reason */}
          <div>
            <label className="block text-sm font-medium text-foreground mb-1.5">
              Reason <span className="text-rose-400">*</span>
            </label>
            <SimpleSelect
              value={reason}
              onChange={(v) => setReason(v as PlantDestroyReason)}
              options={reasonOptions}
              placeholder="Select reason..."
            />
          </div>

          {/* Reason Note */}
          <div>
            <label className="block text-sm font-medium text-foreground mb-1.5">
              Additional Details <span className="text-muted-foreground">(optional)</span>
            </label>
            <textarea
              value={reasonNote}
              onChange={(e) => setReasonNote(e.target.value)}
              rows={2}
              placeholder="Describe the issue..."
              className="w-full px-3 py-2.5 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-red-500/50 resize-none"
            />
          </div>

          {/* Destroyed Date */}
          <div>
            <label className="block text-sm font-medium text-foreground mb-1.5">
              Date of Loss
            </label>
            <div className="relative">
              <Calendar className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
              <input
                type="date"
                value={destroyedDate}
                onChange={(e) => setDestroyedDate(e.target.value)}
                className="w-full pl-10 pr-3 py-2.5 bg-muted/30 border border-border rounded-lg text-sm text-foreground focus:outline-none focus:border-red-500/50"
              />
            </div>
          </div>

          {/* Waste Tracking */}
          <div className="p-3 bg-muted/20 rounded-lg border border-border/50 space-y-3">
            <div className="flex items-center gap-2 text-sm font-medium text-foreground">
              <Scale className="w-4 h-4 text-muted-foreground" />
              Waste Tracking <span className="text-muted-foreground text-xs">(optional)</span>
            </div>
            
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs text-muted-foreground mb-1">
                  Weight (g)
                </label>
                <input
                  type="number"
                  value={wasteWeight}
                  onChange={(e) => setWasteWeight(e.target.value === '' ? '' : parseFloat(e.target.value))}
                  min={0}
                  step={0.1}
                  placeholder="0.0"
                  className="w-full px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground focus:outline-none focus:border-red-500/50"
                />
              </div>
              <div>
                <label className="block text-xs text-muted-foreground mb-1">
                  Disposal Method
                </label>
                <SimpleSelect
                  value={wasteMethod}
                  onChange={(v) => setWasteMethod(v as WasteMethod)}
                  options={wasteMethodOptions}
                  placeholder="Select..."
                />
              </div>
            </div>
          </div>

          {/* Error */}
          {error && (
            <div className="p-3 bg-red-500/10 border border-red-500/30 rounded-lg">
              <div className="flex items-center gap-2 text-red-400">
                <AlertCircle className="w-4 h-4" />
                <span className="text-sm">{error}</span>
              </div>
            </div>
          )}

          {/* METRC Notice */}
          <div className="p-3 bg-amber-500/10 border border-amber-500/20 rounded-lg">
            <div className="flex items-start gap-2">
              <Info className="w-4 h-4 text-amber-400 mt-0.5 shrink-0" />
              <p className="text-xs text-amber-200">
                This loss will be reported to METRC. Ensure the reason and waste
                information is accurate for compliance records.
              </p>
            </div>
          </div>

          {/* Actions */}
          <div className="flex items-center gap-3 pt-2">
            <button
              type="button"
              onClick={handleClose}
              className="flex-1 px-4 py-2.5 text-sm font-medium text-muted-foreground hover:text-foreground transition-colors"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={!canSubmit || isLoading}
              className={cn(
                'flex-1 px-4 py-2.5 rounded-lg text-sm font-medium transition-all',
                canSubmit && !isLoading
                  ? 'bg-red-500 text-white hover:bg-red-400'
                  : 'bg-muted text-muted-foreground cursor-not-allowed'
              )}
            >
              {isLoading ? 'Recording...' : 'Record Loss'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default RecordLossModal;





