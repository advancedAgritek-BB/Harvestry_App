'use client';

import React, { useState } from 'react';
import { X, Scan, ArrowRight, CheckCircle, Package, MapPin } from 'lucide-react';
import { cn } from '@/lib/utils';
import { ScannerInput } from './ScannerInput';
import type { ScanResult } from '../../services/scanning.service';

interface QuickMoveModalProps {
  isOpen: boolean;
  onClose: () => void;
  onComplete: (sourceBarcode: string, destBarcode: string, quantity?: number) => Promise<void>;
}

type Step = 'source' | 'destination' | 'confirm';

export function QuickMoveModal({ isOpen, onClose, onComplete }: QuickMoveModalProps) {
  const [step, setStep] = useState<Step>('source');
  const [sourceResult, setSourceResult] = useState<ScanResult | null>(null);
  const [destResult, setDestResult] = useState<ScanResult | null>(null);
  const [quantity, setQuantity] = useState<number | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (!isOpen) return null;

  const handleSourceScan = async (barcode: string): Promise<ScanResult | void> => {
    // In real implementation, this would call the scanning service
    const mockResult: ScanResult = {
      barcode,
      parsedData: {
        raw: barcode,
        lotNumber: 'LOT-2025-0001',
        isValid: true,
        errors: [],
      },
      lot: {
        id: 'lot-1',
        siteId: 'site-1',
        lotNumber: 'LOT-2025-0001',
        barcode,
        productType: 'flower',
        strainName: 'Blue Dream',
        quantity: 500,
        uom: 'g',
        originalQuantity: 1000,
        reservedQuantity: 0,
        availableQuantity: 500,
        locationId: 'loc-1',
        locationPath: 'Vault A > Rack 1 > Shelf A',
        status: 'available',
        syncStatus: 'synced',
        originType: 'cultivation',
        parentLotIds: [],
        parentRelationships: [],
        childLotIds: [],
        ancestryChain: [],
        generationDepth: 0,
        materialCost: 0,
        laborCost: 0,
        overheadCost: 0,
        totalCost: 0,
        unitCost: 0,
        createdAt: new Date().toISOString(),
        createdBy: 'user-1',
        updatedAt: new Date().toISOString(),
        updatedBy: 'user-1',
      },
      suggestedActions: [],
    };
    setSourceResult(mockResult);
    setQuantity(mockResult.lot?.quantity ?? null);
    setStep('destination');
    return mockResult;
  };

  const handleDestScan = async (barcode: string): Promise<ScanResult | void> => {
    const mockResult: ScanResult = {
      barcode,
      parsedData: {
        raw: barcode,
        lotNumber: undefined,
        isValid: true,
        errors: [],
      },
      suggestedActions: [],
    };
    setDestResult(mockResult);
    setStep('confirm');
    return mockResult;
  };

  const handleConfirm = async () => {
    if (!sourceResult || !destResult) return;
    
    setLoading(true);
    setError(null);
    
    try {
      await onComplete(sourceResult.barcode, destResult.barcode, quantity ?? undefined);
      onClose();
      // Reset state
      setStep('source');
      setSourceResult(null);
      setDestResult(null);
      setQuantity(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Move failed');
    } finally {
      setLoading(false);
    }
  };

  const reset = () => {
    setStep('source');
    setSourceResult(null);
    setDestResult(null);
    setQuantity(null);
    setError(null);
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="absolute inset-0 bg-black/80 backdrop-blur-sm" onClick={onClose} />
      <div className="relative w-full max-w-md mx-4 bg-surface border border-border rounded-2xl shadow-2xl overflow-hidden">
        {/* Header */}
        <div className="flex items-center justify-between px-5 py-4 border-b border-border">
          <div className="flex items-center gap-3">
            <div className="w-8 h-8 rounded-lg bg-cyan-500/10 flex items-center justify-center">
              <ArrowRight className="w-4 h-4 text-cyan-400" />
            </div>
            <h2 className="text-lg font-semibold text-foreground">Quick Move</h2>
          </div>
          <button
            onClick={onClose}
            className="p-1.5 rounded-lg hover:bg-white/5 text-muted-foreground hover:text-foreground transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Progress Steps */}
        <div className="flex items-center px-5 py-3 border-b border-border bg-white/[0.01]">
          <StepIndicator 
            number={1} 
            label="Source" 
            active={step === 'source'} 
            completed={!!sourceResult} 
          />
          <div className="flex-1 h-px bg-white/10 mx-2" />
          <StepIndicator 
            number={2} 
            label="Destination" 
            active={step === 'destination'} 
            completed={!!destResult} 
          />
          <div className="flex-1 h-px bg-white/10 mx-2" />
          <StepIndicator 
            number={3} 
            label="Confirm" 
            active={step === 'confirm'} 
            completed={false} 
          />
        </div>

        {/* Content */}
        <div className="p-5">
          {step === 'source' && (
            <div className="space-y-4">
              <div className="text-center mb-6">
                <Scan className="w-12 h-12 text-cyan-400 mx-auto mb-3" />
                <h3 className="text-sm font-medium text-foreground">Scan Source Lot</h3>
                <p className="text-xs text-muted-foreground mt-1">
                  Scan the barcode on the lot you want to move
                </p>
              </div>
              <ScannerInput
                onScan={handleSourceScan}
                placeholder="Scan source lot barcode..."
                showCamera={true}
              />
            </div>
          )}

          {step === 'destination' && (
            <div className="space-y-4">
              {/* Source Summary */}
              <div className="p-3 rounded-lg bg-emerald-500/5 border border-emerald-500/20">
                <div className="flex items-center gap-3">
                  <CheckCircle className="w-5 h-5 text-emerald-400" />
                  <div>
                    <div className="text-sm font-mono text-foreground">
                      {sourceResult?.lot?.lotNumber}
                    </div>
                    <div className="text-xs text-muted-foreground">
                      {sourceResult?.lot?.strainName} • {sourceResult?.lot?.quantity} {sourceResult?.lot?.uom}
                    </div>
                  </div>
                </div>
              </div>

              <div className="text-center mb-4">
                <MapPin className="w-12 h-12 text-cyan-400 mx-auto mb-3" />
                <h3 className="text-sm font-medium text-foreground">Scan Destination</h3>
                <p className="text-xs text-muted-foreground mt-1">
                  Scan the location barcode where you want to move it
                </p>
              </div>

              <ScannerInput
                onScan={handleDestScan}
                placeholder="Scan destination barcode..."
                showCamera={true}
              />
            </div>
          )}

          {step === 'confirm' && (
            <div className="space-y-4">
              {/* Move Summary */}
              <div className="space-y-3">
                {/* Source */}
                <div className="p-3 rounded-lg bg-muted/30 border border-border">
                  <div className="text-xs text-muted-foreground mb-1">From</div>
                  <div className="flex items-center gap-2">
                    <Package className="w-4 h-4 text-cyan-400" />
                    <span className="text-sm font-mono text-foreground">
                      {sourceResult?.lot?.lotNumber}
                    </span>
                  </div>
                  <div className="text-xs text-muted-foreground mt-1">
                    {sourceResult?.lot?.locationPath}
                  </div>
                </div>

                {/* Arrow */}
                <div className="flex justify-center">
                  <ArrowRight className="w-5 h-5 text-cyan-400" />
                </div>

                {/* Destination */}
                <div className="p-3 rounded-lg bg-muted/30 border border-border">
                  <div className="text-xs text-muted-foreground mb-1">To</div>
                  <div className="flex items-center gap-2">
                    <MapPin className="w-4 h-4 text-cyan-400" />
                    <span className="text-sm text-foreground">
                      {destResult?.barcode || 'Destination Location'}
                    </span>
                  </div>
                </div>
              </div>

              {/* Quantity */}
              <div>
                <label className="block text-xs text-muted-foreground mb-2">Quantity to Move</label>
                <div className="flex items-center gap-3">
                  <input
                    type="number"
                    value={quantity ?? ''}
                    onChange={(e) => setQuantity(parseFloat(e.target.value) || null)}
                    className="flex-1 px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground focus:outline-none focus:border-cyan-500/30"
                  />
                  <span className="text-sm text-muted-foreground">
                    {sourceResult?.lot?.uom}
                  </span>
                </div>
                <div className="text-xs text-muted-foreground mt-1">
                  Available: {sourceResult?.lot?.quantity} {sourceResult?.lot?.uom}
                </div>
              </div>

              {error && (
                <div className="p-3 rounded-lg bg-rose-500/10 border border-rose-500/20">
                  <span className="text-sm text-rose-400">{error}</span>
                </div>
              )}
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="px-5 py-4 border-t border-border flex justify-between">
          <button
            onClick={step === 'source' ? onClose : reset}
            className="px-4 py-2 rounded-lg bg-white/5 text-foreground hover:bg-white/10 transition-colors"
          >
            {step === 'source' ? 'Cancel' : 'Start Over'}
          </button>
          
          {step === 'confirm' && (
            <button
              onClick={handleConfirm}
              disabled={loading || !quantity}
              className="px-4 py-2 rounded-lg bg-cyan-500 text-black font-medium hover:bg-cyan-400 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              {loading ? 'Moving...' : 'Confirm Move'}
            </button>
          )}
        </div>
      </div>
    </div>
  );
}

function StepIndicator({ 
  number, 
  label, 
  active, 
  completed 
}: { 
  number: number; 
  label: string; 
  active: boolean; 
  completed: boolean;
}) {
  return (
    <div className="flex items-center gap-2">
      <div className={cn(
        'w-6 h-6 rounded-full flex items-center justify-center text-xs font-medium transition-colors',
        completed 
          ? 'bg-emerald-500 text-foreground' 
          : active 
            ? 'bg-cyan-500/20 text-cyan-400 border border-cyan-500/30' 
            : 'bg-white/5 text-muted-foreground'
      )}>
        {completed ? <CheckCircle className="w-3 h-3" /> : number}
      </div>
      <span className={cn(
        'text-xs font-medium',
        active ? 'text-foreground' : 'text-muted-foreground'
      )}>
        {label}
      </span>
    </div>
  );
}

export default QuickMoveModal;
