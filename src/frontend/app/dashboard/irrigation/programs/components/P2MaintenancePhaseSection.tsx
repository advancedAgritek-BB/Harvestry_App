'use client';

import React from 'react';
import { Settings2, Sparkles, Gauge } from 'lucide-react';
import { cn } from '@/lib/utils';
import { FormField, Input } from '@/components/admin/AdminForm';
import { PhaseTargets, ShotCountMode } from '@/components/irrigation/types';

interface P2MaintenancePhaseSectionProps {
  phaseTargets: PhaseTargets;
  expectedVwcIncreasePercent: number;
  onUpdate: (updates: Partial<PhaseTargets>) => void;
}

/**
 * P2 Maintenance Phase configuration with optional auto shot count
 * Auto mode is sensor-driven - shots trigger when VWC drops below target
 */
export function P2MaintenancePhaseSection({
  phaseTargets,
  expectedVwcIncreasePercent,
  onUpdate,
}: P2MaintenancePhaseSectionProps) {
  const isAutoMode = phaseTargets.p2ShotCountMode === 'auto';

  const handleModeChange = (mode: ShotCountMode) => {
    onUpdate({ p2ShotCountMode: mode });
  };

  return (
    <div className="p-4 bg-white/5 rounded-lg space-y-3">
      <div className="flex items-center gap-2">
        <span className="text-xs font-bold px-2 py-0.5 rounded bg-cyan-500/20 text-cyan-400">P2</span>
        <span className="font-medium text-foreground">Maintenance Phase</span>
        <span className="text-xs text-muted-foreground ml-auto">Midday stability</span>
      </div>

      <div className="grid grid-cols-2 gap-4">
        <FormField label="Target VWC %" description="Maintain at">
          <Input
            type="number"
            min={30}
            max={70}
            value={phaseTargets.p2TargetVwcPercent}
            onChange={e => onUpdate({ p2TargetVwcPercent: parseInt(e.target.value) || 0 })}
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
                    ? 'bg-cyan-500/20 text-cyan-400'
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
                    ? 'bg-cyan-500/20 text-cyan-400'
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
            <div className="h-10 px-3 bg-cyan-500/10 border border-cyan-500/20 rounded-lg flex items-center justify-between">
              <span className="text-sm font-medium text-cyan-400">Sensor-driven</span>
              <Gauge className="w-4 h-4 text-cyan-400" />
            </div>
          ) : (
            <Input
              type="number"
              min={1}
              max={12}
              value={phaseTargets.p2ShotCount}
              onChange={e => onUpdate({ p2ShotCount: parseInt(e.target.value) || 0 })}
            />
          )}
        </div>
      </div>

      {/* Auto Mode Explanation */}
      {isAutoMode && (
        <div className="p-2 bg-cyan-500/5 border border-cyan-500/20 rounded-lg">
          <div className="flex items-start gap-2">
            <Sparkles className="w-4 h-4 text-cyan-400 mt-0.5 shrink-0" />
            <div className="text-xs text-muted-foreground">
              <span className="text-cyan-400 font-medium">Sensor-driven:</span>{' '}
              Shots triggered when VWC drops below {phaseTargets.p2TargetVwcPercent}%.
              Each shot adds ~{expectedVwcIncreasePercent}% VWC.
              Actual count depends on plant uptake rate.
            </div>
          </div>
        </div>
      )}
    </div>
  );
}



