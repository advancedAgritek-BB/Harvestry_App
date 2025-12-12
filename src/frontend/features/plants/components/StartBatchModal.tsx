'use client';

import React, { useState, useCallback, useEffect } from 'react';
import { cn } from '@/lib/utils';
import {
  X,
  Sprout,
  ChevronDown,
  Check,
  AlertCircle,
  Info,
  Calendar,
  Home,
} from 'lucide-react';
import { usePlantStore } from '../stores';
import { SOURCE_TYPE_LABELS, type PlantSourceType, type StartBatchRequest } from '../types';

// =============================================================================
// TYPES
// =============================================================================

interface StartBatchModalProps {
  batchId: string;
  batchName: string;
  plannedPlantCount: number;
  strainName: string;
  rooms: Array<{ id: string; name: string; code: string }>;
}

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
          isOpen ? 'border-cyan-500/50' : 'border-border hover:border-cyan-500/30'
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
                  option.value === value ? 'bg-cyan-500/10' : 'hover:bg-muted/50'
                )}
              >
                <span className="text-foreground">{option.label}</span>
                {option.value === value && <Check className="w-4 h-4 text-cyan-400" />}
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

export function StartBatchModal({
  batchId,
  batchName,
  plannedPlantCount,
  strainName,
  rooms,
}: StartBatchModalProps) {
  const { showStartBatchModal, closeStartBatchModal, startBatch, isLoading, error } = usePlantStore();

  // Form state
  const [actualCount, setActualCount] = useState(plannedPlantCount);
  const [sourceType, setSourceType] = useState<PlantSourceType>('clone');
  const [roomId, setRoomId] = useState('');
  const [plantedDate, setPlantedDate] = useState(new Date().toISOString().split('T')[0]);
  const [notes, setNotes] = useState('');

  // Reset form when modal opens
  useEffect(() => {
    if (showStartBatchModal) {
      setActualCount(plannedPlantCount);
      setSourceType('clone');
      setRoomId(rooms[0]?.id || '');
      setPlantedDate(new Date().toISOString().split('T')[0]);
      setNotes('');
    }
  }, [showStartBatchModal, plannedPlantCount, rooms]);

  const handleSubmit = useCallback(
    async (e: React.FormEvent) => {
      e.preventDefault();

      const request: StartBatchRequest = {
        batchId,
        actualPlantCount: actualCount,
        sourceType,
        roomId,
        plantedDate,
        notes: notes || undefined,
      };

      try {
        await startBatch(request);
      } catch {
        // Error handled by store
      }
    },
    [batchId, actualCount, sourceType, roomId, plantedDate, notes, startBatch]
  );

  const handleClose = useCallback(() => {
    closeStartBatchModal();
  }, [closeStartBatchModal]);

  // Escape key handler
  useEffect(() => {
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && showStartBatchModal) handleClose();
    };
    document.addEventListener('keydown', handleEscape);
    return () => document.removeEventListener('keydown', handleEscape);
  }, [showStartBatchModal, handleClose]);

  if (!showStartBatchModal) return null;

  const countDiff = actualCount - plannedPlantCount;
  const canSubmit = actualCount > 0 && roomId;

  // Source type options
  const sourceOptions: SelectOption[] = (Object.keys(SOURCE_TYPE_LABELS) as PlantSourceType[]).map(
    (key) => ({
      value: key,
      label: SOURCE_TYPE_LABELS[key],
    })
  );

  // Room options
  const roomOptions: SelectOption[] = rooms.map((r) => ({
    value: r.id,
    label: `${r.code} - ${r.name}`,
  }));

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
            <div className="w-10 h-10 rounded-xl bg-emerald-500/10 flex items-center justify-center">
              <Sprout className="w-5 h-5 text-emerald-400" />
            </div>
            <div>
              <h2 className="text-lg font-semibold text-foreground">Start Batch</h2>
              <p className="text-sm text-muted-foreground">Confirm initial plants</p>
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
              {strainName} • Planned: {plannedPlantCount} plants
            </div>
          </div>

          {/* Actual Plant Count */}
          <div>
            <label className="block text-sm font-medium text-foreground mb-1.5">
              Actual Plant Count <span className="text-rose-400">*</span>
            </label>
            <input
              type="number"
              value={actualCount}
              onChange={(e) => setActualCount(parseInt(e.target.value) || 0)}
              min={1}
              className="w-full px-3 py-2.5 bg-muted/30 border border-border rounded-lg text-sm text-foreground focus:outline-none focus:border-cyan-500/50"
            />
            {countDiff !== 0 && (
              <div className={cn(
                'flex items-center gap-1.5 mt-1.5 text-xs',
                countDiff > 0 ? 'text-emerald-400' : 'text-amber-400'
              )}>
                <Info className="w-3 h-3" />
                {countDiff > 0
                  ? `${countDiff} more than planned`
                  : `${Math.abs(countDiff)} fewer than planned`}
              </div>
            )}
          </div>

          {/* Source Type */}
          <div>
            <label className="block text-sm font-medium text-foreground mb-1.5">
              Plant Source <span className="text-rose-400">*</span>
            </label>
            <SimpleSelect
              value={sourceType}
              onChange={(v) => setSourceType(v as PlantSourceType)}
              options={sourceOptions}
              placeholder="Select source..."
            />
          </div>

          {/* Starting Room */}
          <div>
            <label className="block text-sm font-medium text-foreground mb-1.5">
              Starting Room <span className="text-rose-400">*</span>
            </label>
            <SimpleSelect
              value={roomId}
              onChange={setRoomId}
              options={roomOptions}
              placeholder="Select room..."
            />
          </div>

          {/* Planted Date */}
          <div>
            <label className="block text-sm font-medium text-foreground mb-1.5">
              Planted Date
            </label>
            <div className="relative">
              <Calendar className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
              <input
                type="date"
                value={plantedDate}
                onChange={(e) => setPlantedDate(e.target.value)}
                className="w-full pl-10 pr-3 py-2.5 bg-muted/30 border border-border rounded-lg text-sm text-foreground focus:outline-none focus:border-cyan-500/50"
              />
            </div>
          </div>

          {/* Notes */}
          <div>
            <label className="block text-sm font-medium text-foreground mb-1.5">
              Notes <span className="text-muted-foreground">(optional)</span>
            </label>
            <textarea
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              rows={2}
              placeholder="Any notes about this batch start..."
              className="w-full px-3 py-2.5 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-cyan-500/50 resize-none"
            />
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
          <div className="p-3 bg-violet-500/10 border border-violet-500/20 rounded-lg">
            <div className="flex items-start gap-2">
              <Info className="w-4 h-4 text-violet-400 mt-0.5 shrink-0" />
              <p className="text-xs text-violet-200">
                This will create an immature plant batch in METRC. Plants will be
                tracked as a group until METRC tags are assigned.
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
                  ? 'bg-emerald-500 text-black hover:bg-emerald-400'
                  : 'bg-muted text-muted-foreground cursor-not-allowed'
              )}
            >
              {isLoading ? 'Starting...' : 'Start Batch'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default StartBatchModal;





