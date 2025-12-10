'use client';

import React, { useState, useCallback, useEffect, useMemo } from 'react';
import { cn } from '@/lib/utils';
import {
  X,
  Tag,
  AlertCircle,
  Info,
  ChevronDown,
  Check,
  Hash,
  CheckCircle,
} from 'lucide-react';
import { usePlantStore } from '../stores';
import { type AssignTagsRequest } from '../types';

// =============================================================================
// TYPES
// =============================================================================

interface AssignTagsModalProps {
  batchId: string;
  batchName: string;
  rooms: Array<{ id: string; name: string; code: string; roomClass: string }>;
}

// =============================================================================
// TAG VALIDATION
// =============================================================================

function validateTagRange(
  tagStart: string,
  tagEnd: string,
  expectedCount: number
): { valid: boolean; error?: string; actualCount?: number } {
  if (!tagStart || !tagEnd) {
    return { valid: false, error: 'Both start and end tags are required' };
  }

  // Extract numeric portions
  const startMatch = tagStart.match(/(\d+)$/);
  const endMatch = tagEnd.match(/(\d+)$/);

  if (!startMatch || !endMatch) {
    return { valid: false, error: 'Tags must end with a number' };
  }

  // Check prefixes match
  const startPrefix = tagStart.replace(/\d+$/, '');
  const endPrefix = tagEnd.replace(/\d+$/, '');

  if (startPrefix !== endPrefix) {
    return { valid: false, error: 'Tag prefixes must match' };
  }

  const startNum = parseInt(startMatch[1], 10);
  const endNum = parseInt(endMatch[1], 10);

  if (endNum < startNum) {
    return { valid: false, error: 'End tag must be greater than start tag' };
  }

  const actualCount = endNum - startNum + 1;

  if (actualCount !== expectedCount) {
    return {
      valid: false,
      error: `Tag range covers ${actualCount} tags, but ${expectedCount} plants selected`,
      actualCount,
    };
  }

  return { valid: true, actualCount };
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
          isOpen ? 'border-amber-500/50' : 'border-border hover:border-amber-500/30'
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
                  option.value === value ? 'bg-amber-500/10' : 'hover:bg-muted/50'
                )}
              >
                <span className="text-foreground">{option.label}</span>
                {option.value === value && <Check className="w-4 h-4 text-amber-400" />}
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

export function AssignTagsModal({
  batchId,
  batchName,
  rooms,
}: AssignTagsModalProps) {
  const { 
    showAssignTagsModal, 
    closeAssignTagsModal, 
    assignTags, 
    plantBatchesByBatch,
    countsByBatch,
    isLoading, 
    error 
  } = usePlantStore();

  // Get current state
  const plantBatches = plantBatchesByBatch[batchId] || [];
  const counts = countsByBatch[batchId];
  const plantBatchId = plantBatches[0]?.id || '';
  const untaggedCount = counts?.untagged || 0;

  // Form state
  const [tagStart, setTagStart] = useState('');
  const [tagEnd, setTagEnd] = useState('');
  const [quantity, setQuantity] = useState(untaggedCount);
  const [roomId, setRoomId] = useState('');

  // Filter rooms for veg phase
  const vegRooms = useMemo(() => {
    return rooms.filter((r) => r.roomClass === 'veg');
  }, [rooms]);

  // Validate tag range
  const tagValidation = useMemo(() => {
    if (!tagStart && !tagEnd) return { valid: false };
    return validateTagRange(tagStart, tagEnd, quantity);
  }, [tagStart, tagEnd, quantity]);

  // Reset form when modal opens
  useEffect(() => {
    if (showAssignTagsModal) {
      setTagStart('');
      setTagEnd('');
      setQuantity(untaggedCount);
      setRoomId(vegRooms[0]?.id || '');
    }
  }, [showAssignTagsModal, untaggedCount, vegRooms]);

  // Auto-calculate end tag when start and quantity change
  useEffect(() => {
    if (tagStart && quantity > 0) {
      const startMatch = tagStart.match(/(\d+)$/);
      if (startMatch) {
        const prefix = tagStart.replace(/\d+$/, '');
        const startNum = parseInt(startMatch[1], 10);
        const endNum = startNum + quantity - 1;
        const padLength = startMatch[1].length;
        setTagEnd(`${prefix}${String(endNum).padStart(padLength, '0')}`);
      }
    }
  }, [tagStart, quantity]);

  const handleSubmit = useCallback(
    async (e: React.FormEvent) => {
      e.preventDefault();

      if (!tagValidation.valid) return;

      const request: AssignTagsRequest = {
        batchId,
        plantBatchId,
        tagStart,
        tagEnd,
        quantity,
        roomId,
      };

      try {
        await assignTags(request);
      } catch {
        // Error handled by store
      }
    },
    [batchId, plantBatchId, tagStart, tagEnd, quantity, roomId, tagValidation.valid, assignTags]
  );

  const handleClose = useCallback(() => {
    closeAssignTagsModal();
  }, [closeAssignTagsModal]);

  // Escape key handler
  useEffect(() => {
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && showAssignTagsModal) handleClose();
    };
    document.addEventListener('keydown', handleEscape);
    return () => document.removeEventListener('keydown', handleEscape);
  }, [showAssignTagsModal, handleClose]);

  if (!showAssignTagsModal) return null;

  const canSubmit = tagValidation.valid && roomId && quantity > 0;

  // Room options
  const roomOptions: SelectOption[] = vegRooms.map((r) => ({
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
            <div className="w-10 h-10 rounded-xl bg-amber-500/10 flex items-center justify-center">
              <Tag className="w-5 h-5 text-amber-400" />
            </div>
            <div>
              <h2 className="text-lg font-semibold text-foreground">Assign METRC Tags</h2>
              <p className="text-sm text-muted-foreground">Convert immature plants to tagged</p>
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
              {untaggedCount} untagged plants available
            </div>
          </div>

          {/* Quantity */}
          <div>
            <label className="block text-sm font-medium text-foreground mb-1.5">
              Number of Plants to Tag <span className="text-rose-400">*</span>
            </label>
            <input
              type="number"
              value={quantity}
              onChange={(e) => setQuantity(parseInt(e.target.value) || 0)}
              min={1}
              max={untaggedCount}
              className="w-full px-3 py-2.5 bg-muted/30 border border-border rounded-lg text-sm text-foreground focus:outline-none focus:border-amber-500/50"
            />
          </div>

          {/* Tag Range */}
          <div>
            <label className="block text-sm font-medium text-foreground mb-1.5">
              METRC Tag Range <span className="text-rose-400">*</span>
            </label>
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs text-muted-foreground mb-1">
                  First Tag
                </label>
                <div className="relative">
                  <Hash className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
                  <input
                    type="text"
                    value={tagStart}
                    onChange={(e) => setTagStart(e.target.value.toUpperCase())}
                    placeholder="1A4000000000001"
                    className="w-full pl-10 pr-3 py-2.5 bg-muted/30 border border-border rounded-lg text-sm text-foreground font-mono focus:outline-none focus:border-amber-500/50"
                  />
                </div>
              </div>
              <div>
                <label className="block text-xs text-muted-foreground mb-1">
                  Last Tag
                </label>
                <div className="relative">
                  <Hash className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
                  <input
                    type="text"
                    value={tagEnd}
                    onChange={(e) => setTagEnd(e.target.value.toUpperCase())}
                    placeholder="1A4000000000100"
                    className="w-full pl-10 pr-3 py-2.5 bg-muted/30 border border-border rounded-lg text-sm text-foreground font-mono focus:outline-none focus:border-amber-500/50"
                  />
                </div>
              </div>
            </div>
            
            {/* Tag Validation Feedback */}
            {tagStart && tagEnd && (
              <div className={cn(
                'flex items-center gap-1.5 mt-2 text-xs',
                tagValidation.valid ? 'text-emerald-400' : 'text-red-400'
              )}>
                {tagValidation.valid ? (
                  <>
                    <CheckCircle className="w-3 h-3" />
                    Tag range valid: {quantity} tags
                  </>
                ) : (
                  <>
                    <AlertCircle className="w-3 h-3" />
                    {tagValidation.error}
                  </>
                )}
              </div>
            )}
          </div>

          {/* Destination Room */}
          <div>
            <label className="block text-sm font-medium text-foreground mb-1.5">
              Destination Room <span className="text-rose-400">*</span>
            </label>
            <SimpleSelect
              value={roomId}
              onChange={setRoomId}
              options={roomOptions}
              placeholder="Select room..."
            />
            <p className="text-xs text-muted-foreground mt-1.5">
              Tagged plants will be moved to vegetative phase
            </p>
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
              <div className="text-xs text-amber-200">
                <p className="mb-1">
                  <strong>Important:</strong> Ensure the tag range matches physical
                  tags in your possession. This action cannot be undone.
                </p>
                <p>
                  Plants will be created in METRC with these tags and moved to
                  vegetative growth phase.
                </p>
              </div>
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
                  ? 'bg-amber-500 text-black hover:bg-amber-400'
                  : 'bg-muted text-muted-foreground cursor-not-allowed'
              )}
            >
              {isLoading ? 'Assigning...' : 'Assign Tags'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default AssignTagsModal;




