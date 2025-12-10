'use client';

import React, { useState, useCallback, useEffect, useMemo } from 'react';
import { cn } from '@/lib/utils';
import {
  X,
  ArrowRight,
  AlertCircle,
  Info,
  ChevronDown,
  Check,
  Sprout,
  Leaf,
  Flower2,
  AlertTriangle,
  Tag,
} from 'lucide-react';
import { usePlantStore } from '../stores';
import {
  GROWTH_PHASE_CONFIG,
  type PlantGrowthPhase,
  type TransitionPhaseRequest,
} from '../types';

// =============================================================================
// TYPES
// =============================================================================

interface TransitionPhaseModalProps {
  batchId: string;
  batchName: string;
  rooms: Array<{ id: string; name: string; code: string; roomClass: string }>;
}

// =============================================================================
// CONSTANTS
// =============================================================================

const PHASE_TRANSITIONS: Record<PlantGrowthPhase, PlantGrowthPhase[]> = {
  immature: ['vegetative'],
  vegetative: ['flowering'],
  flowering: ['harvested'],
  mother: [],
  harvested: [],
  destroyed: [],
};

const PHASE_ICONS: Record<PlantGrowthPhase, React.ElementType> = {
  immature: Sprout,
  vegetative: Leaf,
  flowering: Flower2,
  mother: Sprout,
  harvested: Sprout,
  destroyed: Sprout,
};

const PHASE_ROOM_CLASSES: Record<PlantGrowthPhase, string[]> = {
  immature: ['propagation', 'veg'],
  vegetative: ['veg'],
  flowering: ['flower'],
  mother: ['veg'],
  harvested: ['drying', 'processing'],
  destroyed: [],
};

// =============================================================================
// SELECT COMPONENT
// =============================================================================

interface SelectOption {
  value: string;
  label: string;
  disabled?: boolean;
}

function SimpleSelect({
  value,
  onChange,
  options,
  placeholder,
  accentColor = 'violet',
}: {
  value: string;
  onChange: (value: string) => void;
  options: SelectOption[];
  placeholder: string;
  accentColor?: string;
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
          isOpen ? `border-${accentColor}-500/50` : `border-border hover:border-${accentColor}-500/30`
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
                disabled={option.disabled}
                onClick={() => {
                  if (!option.disabled) {
                    onChange(option.value);
                    setIsOpen(false);
                  }
                }}
                className={cn(
                  'w-full flex items-center justify-between px-3 py-2.5 text-left transition-colors text-sm',
                  option.disabled && 'opacity-50 cursor-not-allowed',
                  option.value === value ? `bg-${accentColor}-500/10` : 'hover:bg-muted/50'
                )}
              >
                <span className="text-foreground">{option.label}</span>
                {option.value === value && <Check className={`w-4 h-4 text-${accentColor}-400`} />}
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

export function TransitionPhaseModal({
  batchId,
  batchName,
  rooms,
}: TransitionPhaseModalProps) {
  const { 
    showTransitionModal, 
    closeTransitionModal, 
    transitionPhase, 
    plantBatchesByBatch,
    countsByBatch,
    isLoading, 
    error 
  } = usePlantStore();

  // Get current state
  const plantBatches = plantBatchesByBatch[batchId] || [];
  const counts = countsByBatch[batchId];
  const currentPhase = plantBatches[0]?.growthPhase || 'immature';
  const plantBatchId = plantBatches[0]?.id;
  
  // Available transitions
  const availableTransitions = PHASE_TRANSITIONS[currentPhase] || [];

  // Form state
  const [toPhase, setToPhase] = useState<PlantGrowthPhase | ''>(availableTransitions[0] || '');
  const [destinationRoomId, setDestinationRoomId] = useState('');
  const [quantity, setQuantity] = useState(counts?.current || 0);

  // Check if tagging is required
  const requiresTagging = toPhase === 'flowering' && (counts?.untagged || 0) > 0;

  // Filter rooms based on destination phase
  const filteredRooms = useMemo(() => {
    if (!toPhase) return [];
    const allowedClasses = PHASE_ROOM_CLASSES[toPhase];
    return rooms.filter((r) => allowedClasses.includes(r.roomClass));
  }, [toPhase, rooms]);

  // Reset form when modal opens
  useEffect(() => {
    if (showTransitionModal) {
      setToPhase(availableTransitions[0] || '');
      setDestinationRoomId('');
      setQuantity(counts?.current || 0);
    }
  }, [showTransitionModal, availableTransitions, counts]);

  // Set default room when phase changes
  useEffect(() => {
    if (filteredRooms.length > 0 && !destinationRoomId) {
      setDestinationRoomId(filteredRooms[0].id);
    }
  }, [filteredRooms, destinationRoomId]);

  const handleSubmit = useCallback(
    async (e: React.FormEvent) => {
      e.preventDefault();

      if (!toPhase || requiresTagging) return;

      const request: TransitionPhaseRequest = {
        batchId,
        plantBatchId,
        fromPhase: currentPhase,
        toPhase,
        quantity,
        destinationRoomId,
      };

      try {
        await transitionPhase(request);
      } catch {
        // Error handled by store
      }
    },
    [batchId, plantBatchId, currentPhase, toPhase, quantity, destinationRoomId, requiresTagging, transitionPhase]
  );

  const handleClose = useCallback(() => {
    closeTransitionModal();
  }, [closeTransitionModal]);

  // Escape key handler
  useEffect(() => {
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && showTransitionModal) handleClose();
    };
    document.addEventListener('keydown', handleEscape);
    return () => document.removeEventListener('keydown', handleEscape);
  }, [showTransitionModal, handleClose]);

  if (!showTransitionModal) return null;

  const canSubmit = toPhase && destinationRoomId && quantity > 0 && !requiresTagging;

  // Phase options
  const phaseOptions: SelectOption[] = availableTransitions.map((phase) => ({
    value: phase,
    label: GROWTH_PHASE_CONFIG[phase].label,
  }));

  // Room options
  const roomOptions: SelectOption[] = filteredRooms.map((r) => ({
    value: r.id,
    label: `${r.code} - ${r.name}`,
  }));

  const FromIcon = PHASE_ICONS[currentPhase];
  const ToIcon = toPhase ? PHASE_ICONS[toPhase] : null;

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
            <div className="w-10 h-10 rounded-xl bg-violet-500/10 flex items-center justify-center">
              <ArrowRight className="w-5 h-5 text-violet-400" />
            </div>
            <div>
              <h2 className="text-lg font-semibold text-foreground">Transition Phase</h2>
              <p className="text-sm text-muted-foreground">Move plants to next phase</p>
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
              {counts?.current || 0} plants available
            </div>
          </div>

          {/* Phase Transition Visual */}
          <div className="flex items-center justify-center gap-4 p-4 bg-muted/20 rounded-xl">
            <div className="text-center">
              <div 
                className="w-12 h-12 rounded-xl flex items-center justify-center mx-auto mb-2"
                style={{ backgroundColor: `${GROWTH_PHASE_CONFIG[currentPhase].color}20` }}
              >
                <FromIcon 
                  className="w-6 h-6" 
                  style={{ color: GROWTH_PHASE_CONFIG[currentPhase].color }} 
                />
              </div>
              <span className="text-xs font-medium text-foreground">
                {GROWTH_PHASE_CONFIG[currentPhase].label}
              </span>
            </div>
            
            <ArrowRight className="w-6 h-6 text-muted-foreground" />
            
            <div className="text-center">
              {toPhase && ToIcon ? (
                <>
                  <div 
                    className="w-12 h-12 rounded-xl flex items-center justify-center mx-auto mb-2"
                    style={{ backgroundColor: `${GROWTH_PHASE_CONFIG[toPhase].color}20` }}
                  >
                    <ToIcon 
                      className="w-6 h-6" 
                      style={{ color: GROWTH_PHASE_CONFIG[toPhase].color }} 
                    />
                  </div>
                  <span className="text-xs font-medium text-foreground">
                    {GROWTH_PHASE_CONFIG[toPhase].label}
                  </span>
                </>
              ) : (
                <>
                  <div className="w-12 h-12 rounded-xl bg-muted/30 flex items-center justify-center mx-auto mb-2">
                    <span className="text-muted-foreground text-lg">?</span>
                  </div>
                  <span className="text-xs text-muted-foreground">Select phase</span>
                </>
              )}
            </div>
          </div>

          {/* Tagging Warning */}
          {requiresTagging && (
            <div className="p-3 bg-amber-500/10 border border-amber-500/30 rounded-lg">
              <div className="flex items-start gap-2">
                <AlertTriangle className="w-5 h-5 text-amber-400 shrink-0 mt-0.5" />
                <div>
                  <div className="text-sm font-medium text-amber-300">
                    Tagging Required
                  </div>
                  <p className="text-xs text-amber-200/80 mt-1">
                    You have {counts?.untagged} untagged plants. All plants must be 
                    assigned METRC tags before transitioning to flowering.
                  </p>
                  <button
                    type="button"
                    onClick={() => {
                      handleClose();
                      // Would open assign tags modal
                    }}
                    className="mt-2 flex items-center gap-1.5 text-xs text-amber-400 hover:text-amber-300"
                  >
                    <Tag className="w-3 h-3" />
                    Assign Tags First
                  </button>
                </div>
              </div>
            </div>
          )}

          {/* Destination Phase */}
          {availableTransitions.length > 1 && (
            <div>
              <label className="block text-sm font-medium text-foreground mb-1.5">
                Destination Phase <span className="text-rose-400">*</span>
              </label>
              <SimpleSelect
                value={toPhase}
                onChange={(v) => setToPhase(v as PlantGrowthPhase)}
                options={phaseOptions}
                placeholder="Select phase..."
                accentColor="violet"
              />
            </div>
          )}

          {/* Quantity */}
          <div>
            <label className="block text-sm font-medium text-foreground mb-1.5">
              Number of Plants <span className="text-rose-400">*</span>
            </label>
            <input
              type="number"
              value={quantity}
              onChange={(e) => setQuantity(parseInt(e.target.value) || 0)}
              min={1}
              max={counts?.current || 0}
              className="w-full px-3 py-2.5 bg-muted/30 border border-border rounded-lg text-sm text-foreground focus:outline-none focus:border-violet-500/50"
            />
            {quantity < (counts?.current || 0) && (
              <div className="flex items-center gap-1.5 mt-1.5 text-xs text-muted-foreground">
                <Info className="w-3 h-3" />
                {(counts?.current || 0) - quantity} plants will remain in {GROWTH_PHASE_CONFIG[currentPhase].label}
              </div>
            )}
          </div>

          {/* Destination Room */}
          <div>
            <label className="block text-sm font-medium text-foreground mb-1.5">
              Destination Room <span className="text-rose-400">*</span>
            </label>
            <SimpleSelect
              value={destinationRoomId}
              onChange={setDestinationRoomId}
              options={roomOptions}
              placeholder="Select room..."
              accentColor="violet"
            />
            {filteredRooms.length === 0 && toPhase && (
              <div className="flex items-center gap-1.5 mt-1.5 text-xs text-amber-400">
                <AlertCircle className="w-3 h-3" />
                No rooms configured for {GROWTH_PHASE_CONFIG[toPhase].label} phase
              </div>
            )}
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
                This transition will be synced to METRC. Growth phase changes
                are tracked for compliance reporting.
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
                  ? 'bg-violet-500 text-white hover:bg-violet-400'
                  : 'bg-muted text-muted-foreground cursor-not-allowed'
              )}
            >
              {isLoading ? 'Transitioning...' : 'Transition Plants'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default TransitionPhaseModal;




