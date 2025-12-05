'use client';

import React, { useMemo } from 'react';
import { Settings2, Sparkles } from 'lucide-react';
import { cn } from '@/lib/utils';
import { FormField, Input } from '@/components/admin/AdminForm';
import { 
  PhaseTargets,
  calculateP1ShotCount,
  ShotCountMode,
} from '@/components/irrigation/types';

interface P1RampPhaseSectionProps {
  phaseTargets: PhaseTargets;
  expectedVwcIncreasePercent: number;
  onUpdate: (updates: Partial<PhaseTargets>) => void;
}

/**
 * P1 Ramp Phase configuration with optional auto shot count
 * Auto mode calculates shots needed to go from dryback to P1 target
 */
export function P1RampPhaseSection({
  phaseTargets,
  expectedVwcIncreasePercent,
  onUpdate,
}: P1RampPhaseSectionProps) {
  const isAutoMode = phaseTargets.p1ShotCountMode === 'auto';
  
  // Calculate auto shot count
  const calculatedShotCount = useMemo(() => {
    return calculateP1ShotCount(
      phaseTargets.p1TargetVwcPercent,
      phaseTargets.p3TargetDrybackPercent,
      expectedVwcIncreasePercent
    );
  }, [phaseTargets.p1TargetVwcPercent, phaseTargets.p3TargetDrybackPercent, expectedVwcIncreasePercent]);

  // Starting VWC after dryback (for display)
  const startingVwc = phaseTargets.p1TargetVwcPercent - phaseTargets.p3TargetDrybackPercent;

  const handleModeChange = (mode: ShotCountMode) => {
    onUpdate({ 
      p1ShotCountMode: mode,
      // When switching to manual, prefill with the calculated value
      ...(mode === 'manual' && isAutoMode ? { p1ShotCount: calculatedShotCount } : {}),
    });
  };

  return (
    <div className="p-4 bg-white/5 rounded-lg space-y-3">
      <div className="flex items-center gap-2">
        <span className="text-xs font-bold px-2 py-0.5 rounded bg-violet-500/20 text-violet-400">P1</span>
        <span className="font-medium text-foreground">Ramp Phase</span>
        <span className="text-xs text-muted-foreground ml-auto">Morning saturation</span>
      </div>

      <div className="grid grid-cols-2 gap-4">
        <FormField label="Target VWC %" description="Peak saturation">
          <Input
            type="number"
            min={40}
            max={80}
            value={phaseTargets.p1TargetVwcPercent}
            onChange={e => onUpdate({ p1TargetVwcPercent: parseInt(e.target.value) || 0 })}
          />
        </FormField>

        <div className="space-y-2">
          {/* Mode Toggle */}
          <div className="flex items-center justify-between">
            <label className="text-xs font-medium text-muted-foreground">Shot Count</label>
            <div className="flex items-center gap-1 bg-white/5 rounded-lg p-0.5">
              <button
                type="button"
                onClick={() => handleModeChange('manual')}
                className={cn(
                  'flex items-center gap-1 px-2 py-1 rounded text-xs font-medium transition-colors',
                  !isAutoMode
                    ? 'bg-violet-500/20 text-violet-400'
                    : 'text-muted-foreground hover:text-foreground'
                )}
              >
                <Settings2 className="w-3 h-3" />
                Manual
              </button>
              <button
                type="button"
                onClick={() => handleModeChange('auto')}
                className={cn(
                  'flex items-center gap-1 px-2 py-1 rounded text-xs font-medium transition-colors',
                  isAutoMode
                    ? 'bg-violet-500/20 text-violet-400'
                    : 'text-muted-foreground hover:text-foreground'
                )}
              >
                <Sparkles className="w-3 h-3" />
                Auto
              </button>
            </div>
          </div>

          {/* Manual Input or Auto Display */}
          {isAutoMode ? (
            <div className="h-10 px-3 bg-violet-500/10 border border-violet-500/20 rounded-lg flex items-center justify-between">
              <span className="text-lg font-bold text-violet-400">{calculatedShotCount}</span>
              <span className="text-xs text-muted-foreground">shots</span>
            </div>
          ) : (
            <Input
              type="number"
              min={1}
              max={12}
              value={phaseTargets.p1ShotCount}
              onChange={e => onUpdate({ p1ShotCount: parseInt(e.target.value) || 0 })}
            />
          )}
        </div>
      </div>

      {/* Auto Mode Explanation */}
      {isAutoMode && (
        <div className="p-2 bg-violet-500/5 border border-violet-500/20 rounded-lg">
          <div className="flex items-start gap-2">
            <Sparkles className="w-4 h-4 text-violet-400 mt-0.5 shrink-0" />
            <div className="text-xs text-muted-foreground">
              <span className="text-violet-400 font-medium">Auto-calculated:</span>{' '}
              {startingVwc}% → {phaseTargets.p1TargetVwcPercent}% VWC 
              ({phaseTargets.p3TargetDrybackPercent}% dryback) 
              ÷ {expectedVwcIncreasePercent}% per shot 
              = <span className="text-violet-400 font-medium">{calculatedShotCount} shots</span>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}




