'use client';

import React, { useMemo } from 'react';
import { cn } from '@/lib/utils';
import {
  Layers,
  Check,
  Star,
  Dna,
  Calendar,
  ArrowRight,
  AlertCircle,
  Plus,
} from 'lucide-react';
import { useBlueprintStore } from '../../stores/blueprintStore';
import { BatchBlueprint, PhaseBlueprint } from '../../types/blueprint.types';
import { PHASE_CONFIGS } from '../../constants/phaseConfig';
import type { PhaseType } from '../../types/planner.types';

// =============================================================================
// TYPES
// =============================================================================

export interface BlueprintTabProps {
  selectedGeneticsId: string | null;
  selectedGeneticsName: string | null;
  selectedBlueprintId: string | null;
  onSelectBlueprint: (blueprintId: string | null) => void;
}

interface BlueprintWithMatch extends BatchBlueprint {
  matchScore: number;
  matchReason?: string;
}

// =============================================================================
// HELPER FUNCTIONS
// =============================================================================

function calculateMatchScore(
  blueprint: BatchBlueprint,
  geneticsId: string | null
): { score: number; reason?: string } {
  if (!geneticsId) {
    return { score: 0 };
  }

  // Exact genetics match
  if (blueprint.geneticsIds?.includes(geneticsId)) {
    return { score: 100, reason: 'Exact match for this genetics' };
  }

  // Strain match (if strainIds present)
  if (blueprint.strainIds && blueprint.strainIds.length > 0) {
    return { score: 50, reason: 'Compatible strain types' };
  }

  // Generic/universal blueprint
  if (!blueprint.geneticsIds || blueprint.geneticsIds.length === 0) {
    return { score: 25, reason: 'Universal blueprint' };
  }

  return { score: 0 };
}

function getTotalDuration(blueprint: BatchBlueprint, phaseBlueprints: PhaseBlueprint[]): number {
  const phases: (keyof BatchBlueprint)[] = [
    'cloneBlueprintId',
    'vegBlueprintId',
    'flowerBlueprintId',
    'harvestBlueprintId',
    'cureBlueprintId',
  ];

  return phases.reduce((total, key) => {
    const bpId = blueprint[key] as string | undefined;
    if (bpId) {
      const phaseBp = phaseBlueprints.find((p) => p.id === bpId);
      if (phaseBp) {
        return total + phaseBp.defaultDurationDays;
      }
    }
    return total;
  }, 0);
}

// =============================================================================
// BLUEPRINT CARD COMPONENT
// =============================================================================

interface BlueprintCardProps {
  blueprint: BlueprintWithMatch;
  phaseBlueprints: PhaseBlueprint[];
  isSelected: boolean;
  onSelect: () => void;
}

function BlueprintCard({
  blueprint,
  phaseBlueprints,
  isSelected,
  onSelect,
}: BlueprintCardProps) {
  const totalDays = getTotalDuration(blueprint, phaseBlueprints);

  // Get phase durations for display
  const phaseDurations = useMemo(() => {
    const phases: { phase: PhaseType; bpKey: keyof BatchBlueprint }[] = [
      { phase: 'clone', bpKey: 'cloneBlueprintId' },
      { phase: 'veg', bpKey: 'vegBlueprintId' },
      { phase: 'flower', bpKey: 'flowerBlueprintId' },
      { phase: 'harvest', bpKey: 'harvestBlueprintId' },
      { phase: 'cure', bpKey: 'cureBlueprintId' },
    ];

    return phases.map(({ phase, bpKey }) => {
      const bpId = blueprint[bpKey] as string | undefined;
      const phaseBp = bpId
        ? phaseBlueprints.find((p) => p.id === bpId)
        : undefined;
      return {
        phase,
        days: phaseBp?.defaultDurationDays ?? 0,
        color: PHASE_CONFIGS[phase].color,
      };
    });
  }, [blueprint, phaseBlueprints]);

  const isRecommended = blueprint.matchScore >= 50;

  return (
    <button
      type="button"
      onClick={onSelect}
      className={cn(
        'w-full p-4 rounded-xl border-2 text-left transition-all relative',
        isSelected
          ? 'border-cyan-500 bg-cyan-500/10 ring-1 ring-cyan-500/30'
          : 'border-border hover:border-cyan-500/30 bg-muted/20 hover:bg-muted/40'
      )}
    >
      {/* Recommended badge */}
      {isRecommended && (
        <div className="absolute -top-2 -right-2 flex items-center gap-1 px-2 py-0.5 bg-amber-500 text-black text-[10px] font-bold rounded-full">
          <Star className="w-3 h-3" fill="currentColor" />
          Recommended
        </div>
      )}

      {/* Header */}
      <div className="flex items-start justify-between gap-3 mb-3">
        <div className="flex items-center gap-3">
          <div
            className={cn(
              'w-10 h-10 rounded-lg flex items-center justify-center',
              isSelected ? 'bg-cyan-500/20' : 'bg-violet-500/10'
            )}
          >
            <Layers
              className={cn(
                'w-5 h-5',
                isSelected ? 'text-cyan-400' : 'text-violet-400'
              )}
            />
          </div>
          <div>
            <div className="font-medium text-foreground">{blueprint.name}</div>
            {blueprint.matchReason && (
              <div className="flex items-center gap-1 text-xs text-emerald-400">
                <Dna className="w-3 h-3" />
                {blueprint.matchReason}
              </div>
            )}
          </div>
        </div>

        {isSelected && (
          <div className="w-6 h-6 rounded-full bg-cyan-500 flex items-center justify-center">
            <Check className="w-4 h-4 text-black" />
          </div>
        )}
      </div>

      {/* Description */}
      {blueprint.description && (
        <p className="text-xs text-muted-foreground mb-3 line-clamp-2">
          {blueprint.description}
        </p>
      )}

      {/* Phase timeline visualization */}
      <div className="flex items-center gap-0.5 h-2 rounded-full overflow-hidden mb-2">
        {phaseDurations.map(({ phase, days, color }) =>
          days > 0 ? (
            <div
              key={phase}
              className="h-full"
              style={{
                backgroundColor: color,
                flex: days,
              }}
              title={`${PHASE_CONFIGS[phase].label}: ${days} days`}
            />
          ) : null
        )}
      </div>

      {/* Duration info */}
      <div className="flex items-center gap-4 text-xs">
        <div className="flex items-center gap-1 text-muted-foreground">
          <Calendar className="w-3 h-3" />
          <span>{totalDays} days total</span>
        </div>
        <div className="flex items-center gap-1 text-muted-foreground">
          {phaseDurations.map(({ phase, days }, idx) => (
            <React.Fragment key={phase}>
              {idx > 0 && <ArrowRight className="w-2 h-2" />}
              <span
                className="px-1 py-0.5 rounded text-[10px]"
                style={{
                  backgroundColor: `${PHASE_CONFIGS[phase].color}20`,
                  color: PHASE_CONFIGS[phase].color,
                }}
              >
                {days}d
              </span>
            </React.Fragment>
          ))}
        </div>
      </div>
    </button>
  );
}

// =============================================================================
// MAIN COMPONENT
// =============================================================================

export function BlueprintTab({
  selectedGeneticsId,
  selectedGeneticsName,
  selectedBlueprintId,
  onSelectBlueprint,
}: BlueprintTabProps) {
  const { batchBlueprints, phaseBlueprints } = useBlueprintStore();

  // Sort blueprints by match score
  const sortedBlueprints = useMemo((): BlueprintWithMatch[] => {
    return batchBlueprints
      .map((bp) => {
        const { score, reason } = calculateMatchScore(bp, selectedGeneticsId);
        return {
          ...bp,
          matchScore: score,
          matchReason: reason,
        };
      })
      .sort((a, b) => b.matchScore - a.matchScore);
  }, [batchBlueprints, selectedGeneticsId]);

  const recommendedBlueprints = sortedBlueprints.filter((bp) => bp.matchScore >= 50);
  const otherBlueprints = sortedBlueprints.filter((bp) => bp.matchScore < 50);

  if (batchBlueprints.length === 0) {
    return (
      <div className="p-8 text-center">
        <div className="w-16 h-16 rounded-full bg-violet-500/10 flex items-center justify-center mx-auto mb-4">
          <Layers className="w-8 h-8 text-violet-400" />
        </div>
        <h3 className="text-lg font-medium text-foreground mb-2">
          No Blueprints Available
        </h3>
        <p className="text-sm text-muted-foreground mb-4">
          Create blueprints to define environmental, irrigation, and lighting parameters for your batches.
        </p>
        <a
          href="/library/blueprints"
          className="inline-flex items-center gap-2 px-4 py-2 bg-violet-500 text-white rounded-lg text-sm font-medium hover:bg-violet-400 transition-colors"
        >
          <Plus className="w-4 h-4" />
          Create Blueprint
        </a>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {/* No genetics selected warning */}
      {!selectedGeneticsId && (
        <div className="flex items-center gap-3 p-3 bg-amber-500/10 border border-amber-500/20 rounded-lg">
          <AlertCircle className="w-5 h-5 text-amber-400 shrink-0" />
          <div className="text-sm text-amber-200">
            Select genetics above to see recommended blueprints for that strain.
          </div>
        </div>
      )}

      {/* Recommended section */}
      {recommendedBlueprints.length > 0 && (
        <div>
          <div className="flex items-center gap-2 mb-3">
            <Star className="w-4 h-4 text-amber-400" />
            <h4 className="text-sm font-medium text-foreground">
              Recommended for {selectedGeneticsName || 'this genetics'}
            </h4>
          </div>
          <div className="space-y-3">
            {recommendedBlueprints.map((blueprint) => (
              <BlueprintCard
                key={blueprint.id}
                blueprint={blueprint}
                phaseBlueprints={phaseBlueprints}
                isSelected={selectedBlueprintId === blueprint.id}
                onSelect={() => onSelectBlueprint(blueprint.id)}
              />
            ))}
          </div>
        </div>
      )}

      {/* Other blueprints */}
      {otherBlueprints.length > 0 && (
        <div>
          <h4 className="text-sm font-medium text-muted-foreground mb-3">
            {recommendedBlueprints.length > 0
              ? 'Other Blueprints'
              : 'Available Blueprints'}
          </h4>
          <div className="space-y-3">
            {otherBlueprints.map((blueprint) => (
              <BlueprintCard
                key={blueprint.id}
                blueprint={blueprint}
                phaseBlueprints={phaseBlueprints}
                isSelected={selectedBlueprintId === blueprint.id}
                onSelect={() => onSelectBlueprint(blueprint.id)}
              />
            ))}
          </div>
        </div>
      )}

      {/* Clear selection button */}
      {selectedBlueprintId && (
        <button
          type="button"
          onClick={() => onSelectBlueprint(null)}
          className="w-full py-2 text-sm text-muted-foreground hover:text-foreground transition-colors"
        >
          Clear blueprint selection
        </button>
      )}
    </div>
  );
}

export default BlueprintTab;





