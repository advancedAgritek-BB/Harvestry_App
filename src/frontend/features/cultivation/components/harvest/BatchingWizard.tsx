'use client';

/**
 * BatchingWizard Component
 * Multi-step wizard for grouping dried flower into batches
 */

import { useState, useMemo } from 'react';
import { cn } from '@/lib/utils';
import type { HarvestBatchingMode, HarvestWorkflowState } from '@/features/inventory/types';

interface BatchingWizardProps {
  /** Available harvests to batch */
  harvests: HarvestWorkflowState[];
  /** Callback when batching is complete */
  onComplete: (data: BatchingResult) => Promise<void>;
  /** Callback to cancel */
  onCancel: () => void;
  /** Loading state */
  isLoading?: boolean;
}

export interface BatchingResult {
  batchingMode: HarvestBatchingMode;
  sourceHarvestIds: string[];
  batchNumber?: string;
  notes?: string;
}

type WizardStep = 'select-mode' | 'select-harvests' | 'confirm';

const BATCHING_MODES: { value: HarvestBatchingMode; label: string; description: string; icon: string }[] = [
  {
    value: 'single_strain',
    label: 'Single Strain Batch',
    description: 'Create a batch from one strain only. Best for premium flower.',
    icon: '🌿',
  },
  {
    value: 'mixed_strain',
    label: 'Mixed Strain Batch',
    description: 'Combine multiple strains into one batch. Good for blends.',
    icon: '🎨',
  },
  {
    value: 'sub_lot',
    label: 'Sub-Lot Split',
    description: 'Split a single harvest into multiple batches by quality or destination.',
    icon: '✂️',
  },
];

export function BatchingWizard({
  harvests,
  onComplete,
  onCancel,
  isLoading = false,
}: BatchingWizardProps) {
  const [currentStep, setCurrentStep] = useState<WizardStep>('select-mode');
  const [batchingMode, setBatchingMode] = useState<HarvestBatchingMode | null>(null);
  const [selectedHarvestIds, setSelectedHarvestIds] = useState<string[]>([]);
  const [batchNumber, setBatchNumber] = useState('');
  const [notes, setNotes] = useState('');

  // Filter available harvests (must be in dry_weighed or later phase)
  const availableHarvests = useMemo(() => 
    harvests.filter(h => 
      h.phase === 'dry_weighed' || 
      h.phase === 'batched'
    ),
    [harvests]
  );

  // Get unique strains from selected harvests
  const selectedStrains = useMemo(() => {
    const selected = availableHarvests.filter(h => selectedHarvestIds.includes(h.harvestId));
    return Array.from(new Set(selected.map(h => h.strainName)));
  }, [availableHarvests, selectedHarvestIds]);

  // Calculate total weight of selected harvests
  const totalWeight = useMemo(() => {
    return availableHarvests
      .filter(h => selectedHarvestIds.includes(h.harvestId))
      .reduce((sum, h) => sum + (h.metrics.buckedFlowerWeight || h.currentWeight), 0);
  }, [availableHarvests, selectedHarvestIds]);

  // Validation
  const canProceed = useMemo(() => {
    switch (currentStep) {
      case 'select-mode':
        return batchingMode !== null;
      case 'select-harvests':
        if (selectedHarvestIds.length === 0) return false;
        if (batchingMode === 'single_strain' && selectedStrains.length > 1) return false;
        return true;
      case 'confirm':
        return true;
      default:
        return false;
    }
  }, [currentStep, batchingMode, selectedHarvestIds, selectedStrains]);

  const handleNext = () => {
    switch (currentStep) {
      case 'select-mode':
        setCurrentStep('select-harvests');
        break;
      case 'select-harvests':
        setCurrentStep('confirm');
        break;
      case 'confirm':
        handleComplete();
        break;
    }
  };

  const handleBack = () => {
    switch (currentStep) {
      case 'select-harvests':
        setCurrentStep('select-mode');
        break;
      case 'confirm':
        setCurrentStep('select-harvests');
        break;
    }
  };

  const handleComplete = async () => {
    if (!batchingMode) return;
    
    await onComplete({
      batchingMode,
      sourceHarvestIds: selectedHarvestIds,
      batchNumber: batchNumber || undefined,
      notes: notes || undefined,
    });
  };

  const toggleHarvestSelection = (harvestId: string) => {
    setSelectedHarvestIds(prev => 
      prev.includes(harvestId)
        ? prev.filter(id => id !== harvestId)
        : [...prev, harvestId]
    );
  };

  return (
    <div className="flex flex-col h-full">
      {/* Header */}
      <div className="px-6 py-4 border-b border-border">
        <h2 className="text-lg font-semibold">Create Batch</h2>
        <p className="text-sm text-muted-foreground">
          Group dried flower into batches for lot creation
        </p>
      </div>

      {/* Progress indicator */}
      <div className="px-6 py-3 border-b border-border/50 bg-muted/30">
        <div className="flex items-center gap-2">
          {['select-mode', 'select-harvests', 'confirm'].map((step, index) => (
            <div key={step} className="flex items-center">
              <div className={cn(
                'w-8 h-8 rounded-full flex items-center justify-center text-sm font-medium',
                currentStep === step ? 'bg-primary text-primary-foreground' :
                index < ['select-mode', 'select-harvests', 'confirm'].indexOf(currentStep)
                  ? 'bg-emerald-500 text-white' : 'bg-muted text-muted-foreground'
              )}>
                {index + 1}
              </div>
              {index < 2 && (
                <div className={cn(
                  'w-12 h-0.5 mx-1',
                  index < ['select-mode', 'select-harvests', 'confirm'].indexOf(currentStep)
                    ? 'bg-emerald-500' : 'bg-muted'
                )} />
              )}
            </div>
          ))}
        </div>
      </div>

      {/* Content */}
      <div className="flex-1 overflow-y-auto p-6">
        {/* Step 1: Select Mode */}
        {currentStep === 'select-mode' && (
          <div className="space-y-4">
            <h3 className="font-medium">Select Batching Mode</h3>
            <div className="grid gap-3">
              {BATCHING_MODES.map(mode => (
                <button
                  key={mode.value}
                  onClick={() => setBatchingMode(mode.value)}
                  className={cn(
                    'p-4 rounded-lg border text-left transition-colors',
                    batchingMode === mode.value
                      ? 'border-primary bg-primary/5'
                      : 'border-border hover:border-border/80 hover:bg-muted/50'
                  )}
                >
                  <div className="flex items-start gap-3">
                    <span className="text-2xl">{mode.icon}</span>
                    <div>
                      <div className="font-medium">{mode.label}</div>
                      <div className="text-sm text-muted-foreground">{mode.description}</div>
                    </div>
                  </div>
                </button>
              ))}
            </div>
          </div>
        )}

        {/* Step 2: Select Harvests */}
        {currentStep === 'select-harvests' && (
          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <h3 className="font-medium">Select Harvests</h3>
              <span className="text-sm text-muted-foreground">
                {selectedHarvestIds.length} selected • {totalWeight.toFixed(1)}g
              </span>
            </div>

            {batchingMode === 'single_strain' && selectedStrains.length > 1 && (
              <div className="p-3 bg-amber-500/10 border border-amber-500/20 rounded-md">
                <p className="text-sm text-amber-400">
                  Single strain batches can only contain one strain. 
                  Selected: {selectedStrains.join(', ')}
                </p>
              </div>
            )}

            <div className="space-y-2">
              {availableHarvests.map(harvest => (
                <button
                  key={harvest.harvestId}
                  onClick={() => toggleHarvestSelection(harvest.harvestId)}
                  className={cn(
                    'w-full p-4 rounded-lg border text-left transition-colors',
                    selectedHarvestIds.includes(harvest.harvestId)
                      ? 'border-primary bg-primary/5'
                      : 'border-border hover:border-border/80'
                  )}
                >
                  <div className="flex items-center justify-between">
                    <div>
                      <div className="font-medium">{harvest.harvestName}</div>
                      <div className="text-sm text-muted-foreground">
                        {harvest.strainName} • {harvest.metrics.buckedFlowerWeight || harvest.currentWeight}g
                      </div>
                    </div>
                    <div className={cn(
                      'w-5 h-5 rounded border flex items-center justify-center',
                      selectedHarvestIds.includes(harvest.harvestId)
                        ? 'bg-primary border-primary text-primary-foreground'
                        : 'border-border'
                    )}>
                      {selectedHarvestIds.includes(harvest.harvestId) && '✓'}
                    </div>
                  </div>
                </button>
              ))}

              {availableHarvests.length === 0 && (
                <div className="text-center py-8 text-muted-foreground">
                  <p>No harvests available for batching.</p>
                  <p className="text-sm">Harvests must complete drying first.</p>
                </div>
              )}
            </div>
          </div>
        )}

        {/* Step 3: Confirm */}
        {currentStep === 'confirm' && (
          <div className="space-y-4">
            <h3 className="font-medium">Confirm Batch Details</h3>

            {/* Summary */}
            <div className="bg-card/50 rounded-lg border border-border p-4 space-y-3">
              <div className="flex justify-between">
                <span className="text-muted-foreground">Batching Mode</span>
                <span className="font-medium">
                  {BATCHING_MODES.find(m => m.value === batchingMode)?.label}
                </span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Harvests</span>
                <span className="font-medium">{selectedHarvestIds.length}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Strains</span>
                <span className="font-medium">{selectedStrains.join(', ')}</span>
              </div>
              <div className="flex justify-between border-t border-border pt-3">
                <span className="text-muted-foreground">Total Weight</span>
                <span className="font-mono font-semibold text-lg">{totalWeight.toFixed(1)}g</span>
              </div>
            </div>

            {/* Optional fields */}
            <div className="space-y-3">
              <div>
                <label className="block text-sm font-medium mb-1.5">
                  Batch Number (optional)
                </label>
                <input
                  type="text"
                  value={batchNumber}
                  onChange={(e) => setBatchNumber(e.target.value)}
                  placeholder="Auto-generated if blank"
                  className="w-full px-3 py-2 bg-background border border-border rounded-md text-sm"
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1.5">
                  Notes (optional)
                </label>
                <textarea
                  value={notes}
                  onChange={(e) => setNotes(e.target.value)}
                  placeholder="Any notes about this batch..."
                  className="w-full px-3 py-2 bg-background border border-border rounded-md text-sm resize-none"
                  rows={3}
                />
              </div>
            </div>
          </div>
        )}
      </div>

      {/* Footer */}
      <div className="px-6 py-4 border-t border-border flex justify-between">
        <button
          onClick={currentStep === 'select-mode' ? onCancel : handleBack}
          disabled={isLoading}
          className="px-4 py-2 bg-muted hover:bg-muted/80 rounded-md text-sm font-medium transition-colors"
        >
          {currentStep === 'select-mode' ? 'Cancel' : 'Back'}
        </button>
        <button
          onClick={handleNext}
          disabled={!canProceed || isLoading}
          className={cn(
            'px-4 py-2 rounded-md text-sm font-medium transition-colors',
            'bg-primary hover:bg-primary/90 text-primary-foreground',
            'disabled:opacity-50 disabled:cursor-not-allowed'
          )}
        >
          {isLoading ? 'Creating...' : currentStep === 'confirm' ? 'Create Batch' : 'Next'}
        </button>
      </div>
    </div>
  );
}

export default BatchingWizard;
