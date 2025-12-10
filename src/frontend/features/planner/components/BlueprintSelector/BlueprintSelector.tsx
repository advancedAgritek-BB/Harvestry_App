'use client';

import React, { useState } from 'react';
import { cn } from '@/lib/utils';
import { 
  ChevronDown, 
  FileText, 
  Plus, 
  Check,
  Layers,
  BookOpen,
} from 'lucide-react';
import { BatchBlueprint, PhaseBlueprint } from '../../types/blueprint.types';
import { PhaseType } from '../../types/planner.types';
import { PHASE_CONFIGS } from '../../constants/phaseConfig';

interface BlueprintSelectorProps {
  batchBlueprints: BatchBlueprint[];
  phaseBlueprints: PhaseBlueprint[];
  selectedBatchBlueprintId?: string;
  phaseOverrides?: { [key in PhaseType]?: string };
  onSelectBatchBlueprint: (id: string | null) => void;
  onSelectPhaseBlueprint: (phase: PhaseType, id: string | null) => void;
  onCreateNewBlueprint?: () => void;
  mode: 'batch' | 'phase';
  onModeChange: (mode: 'batch' | 'phase') => void;
}

export function BlueprintSelector({
  batchBlueprints,
  phaseBlueprints,
  selectedBatchBlueprintId,
  phaseOverrides = {},
  onSelectBatchBlueprint,
  onSelectPhaseBlueprint,
  onCreateNewBlueprint,
  mode,
  onModeChange,
}: BlueprintSelectorProps) {
  const [expandedPhase, setExpandedPhase] = useState<PhaseType | null>(null);
  const [showBatchDropdown, setShowBatchDropdown] = useState(false);

  const selectedBatchBlueprint = batchBlueprints.find(bp => bp.id === selectedBatchBlueprintId);
  const phases: PhaseType[] = ['clone', 'veg', 'flower', 'harvest', 'cure'];

  return (
    <div className="space-y-3">
      {/* Mode Toggle */}
      <div className="flex items-center gap-1 p-1 bg-muted/50 rounded-lg">
        <button
          onClick={() => onModeChange('batch')}
          className={cn(
            'flex-1 flex items-center justify-center gap-2 px-3 py-1.5 rounded-md text-xs font-medium transition-colors',
            mode === 'batch'
              ? 'bg-cyan-500/20 text-cyan-400'
              : 'text-muted-foreground hover:text-foreground/70'
          )}
        >
          <Layers className="w-3.5 h-3.5" />
          Batch Blueprint
        </button>
        <button
          onClick={() => onModeChange('phase')}
          className={cn(
            'flex-1 flex items-center justify-center gap-2 px-3 py-1.5 rounded-md text-xs font-medium transition-colors',
            mode === 'phase'
              ? 'bg-cyan-500/20 text-cyan-400'
              : 'text-muted-foreground hover:text-foreground/70'
          )}
        >
          <BookOpen className="w-3.5 h-3.5" />
          By Phase
        </button>
      </div>

      {mode === 'batch' ? (
        /* Batch Blueprint Selector */
        <div className="relative">
          <button
            onClick={() => setShowBatchDropdown(!showBatchDropdown)}
            className="w-full flex items-center justify-between px-3 py-2.5 bg-muted/50 border border-border/50 rounded-lg text-sm hover:border-border transition-colors"
          >
            <div className="flex items-center gap-2">
              <FileText className="w-4 h-4 text-muted-foreground" />
              <span className={selectedBatchBlueprint ? 'text-foreground' : 'text-muted-foreground'}>
                {selectedBatchBlueprint?.name || 'Select Blueprint...'}
              </span>
            </div>
            <ChevronDown className={cn(
              'w-4 h-4 text-muted-foreground transition-transform',
              showBatchDropdown && 'rotate-180'
            )} />
          </button>

          {showBatchDropdown && (
            <div className="absolute top-full left-0 right-0 mt-1 bg-muted border border-border rounded-lg shadow-xl z-10 max-h-64 overflow-y-auto">
              {batchBlueprints.map((bp) => (
                <button
                  key={bp.id}
                  onClick={() => {
                    onSelectBatchBlueprint(bp.id);
                    setShowBatchDropdown(false);
                  }}
                  className={cn(
                    'w-full flex items-center justify-between px-3 py-2.5 hover:bg-hover/50 transition-colors',
                    bp.id === selectedBatchBlueprintId && 'bg-cyan-500/10'
                  )}
                >
                  <div className="flex flex-col items-start">
                    <span className="text-sm text-foreground">{bp.name}</span>
                    {bp.description && (
                      <span className="text-xs text-muted-foreground">{bp.description}</span>
                    )}
                  </div>
                  {bp.id === selectedBatchBlueprintId && (
                    <Check className="w-4 h-4 text-cyan-400" />
                  )}
                </button>
              ))}
              
              {onCreateNewBlueprint && (
                <>
                  <div className="border-t border-border" />
                  <button
                    onClick={() => {
                      onCreateNewBlueprint();
                      setShowBatchDropdown(false);
                    }}
                    className="w-full flex items-center gap-2 px-3 py-2.5 text-cyan-400 hover:bg-cyan-500/10 transition-colors"
                  >
                    <Plus className="w-4 h-4" />
                    <span className="text-sm">Create New Blueprint</span>
                  </button>
                </>
              )}
            </div>
          )}
        </div>
      ) : (
        /* Phase-by-Phase Selector */
        <div className="space-y-1.5">
          {phases.map((phase) => {
            const config = PHASE_CONFIGS[phase];
            const availableBlueprints = phaseBlueprints.filter(bp => bp.phase === phase);
            const selectedId = phaseOverrides[phase];
            const selectedBlueprint = availableBlueprints.find(bp => bp.id === selectedId);
            const isExpanded = expandedPhase === phase;

            return (
              <div key={phase} className="relative">
                <button
                  onClick={() => setExpandedPhase(isExpanded ? null : phase)}
                  className={cn(
                    'w-full flex items-center justify-between px-3 py-2 rounded-lg text-sm transition-colors',
                    'bg-muted/30 border border-border/50 hover:border-border'
                  )}
                >
                  <div className="flex items-center gap-2">
                    <div 
                      className="w-3 h-3 rounded-full"
                      style={{ backgroundColor: config.color }}
                    />
                    <span className="text-foreground/70 font-medium">{config.label}</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <span className="text-xs text-muted-foreground">
                      {selectedBlueprint?.name || 'Default'}
                    </span>
                    <ChevronDown className={cn(
                      'w-3.5 h-3.5 text-muted-foreground transition-transform',
                      isExpanded && 'rotate-180'
                    )} />
                  </div>
                </button>

                {isExpanded && (
                  <div className="mt-1 bg-muted border border-border rounded-lg shadow-xl z-10 max-h-48 overflow-y-auto">
                    <button
                      onClick={() => {
                        onSelectPhaseBlueprint(phase, null);
                        setExpandedPhase(null);
                      }}
                      className={cn(
                        'w-full flex items-center justify-between px-3 py-2 hover:bg-hover/50 transition-colors',
                        !selectedId && 'bg-cyan-500/10'
                      )}
                    >
                      <span className="text-sm text-foreground/70">Default</span>
                      {!selectedId && <Check className="w-4 h-4 text-cyan-400" />}
                    </button>
                    
                    {availableBlueprints.map((bp) => (
                      <button
                        key={bp.id}
                        onClick={() => {
                          onSelectPhaseBlueprint(phase, bp.id);
                          setExpandedPhase(null);
                        }}
                        className={cn(
                          'w-full flex items-center justify-between px-3 py-2 hover:bg-hover/50 transition-colors',
                          bp.id === selectedId && 'bg-cyan-500/10'
                        )}
                      >
                        <span className="text-sm text-foreground">{bp.name}</span>
                        {bp.id === selectedId && <Check className="w-4 h-4 text-cyan-400" />}
                      </button>
                    ))}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}










