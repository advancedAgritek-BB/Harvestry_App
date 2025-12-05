'use client';

import React from 'react';
import { cn } from '@/lib/utils';
import { 
  AlertTriangle, 
  ArrowRight, 
  Calendar, 
  Clock, 
  X, 
  Check,
  ChevronDown,
  ChevronUp,
  Zap
} from 'lucide-react';
import { ChangeImpact, PlannerConflict, PhaseType } from '../../types/planner.types';
import { PHASE_CONFIGS } from '../../constants/phaseConfig';
import { formatDuration } from '../../utils/dateUtils';

interface ImpactPanelProps {
  impact: ChangeImpact | null;
  conflicts: PlannerConflict[];
  isVisible: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

export function ImpactPanel({
  impact,
  conflicts,
  isVisible,
  onConfirm,
  onCancel,
}: ImpactPanelProps) {
  const [isExpanded, setIsExpanded] = React.useState(true);

  if (!isVisible || !impact) return null;

  const hasConflicts = conflicts.length > 0;
  const hasCascade = impact.cascadeEffects.length > 0;
  const isPositive = impact.daysDelta > 0;

  return (
    <div className={cn(
      'fixed bottom-4 right-4 z-50 w-96',
      'bg-surface/95 backdrop-blur-sm border rounded-xl shadow-2xl',
      hasConflicts ? 'border-red-500/50' : 'border-border',
      'animate-in slide-in-from-bottom-4 duration-300'
    )}>
      {/* Header */}
      <div 
        className={cn(
          'flex items-center justify-between px-4 py-3 border-b cursor-pointer',
          hasConflicts ? 'border-red-500/30 bg-red-500/10' : 'border-border/50'
        )}
        onClick={() => setIsExpanded(!isExpanded)}
      >
        <div className="flex items-center gap-2">
          <Zap className={cn(
            'w-4 h-4',
            hasConflicts ? 'text-red-400' : 'text-cyan-400'
          )} />
          <span className="font-medium text-foreground">
            {hasConflicts ? 'Scheduling Conflicts' : 'Schedule Impact'}
          </span>
        </div>
        <div className="flex items-center gap-2">
          {hasConflicts && (
            <span className="text-xs text-red-400 bg-red-500/20 px-2 py-0.5 rounded-full">
              {conflicts.length} {conflicts.length === 1 ? 'conflict' : 'conflicts'}
            </span>
          )}
          {isExpanded ? (
            <ChevronDown className="w-4 h-4 text-muted-foreground" />
          ) : (
            <ChevronUp className="w-4 h-4 text-muted-foreground" />
          )}
        </div>
      </div>

      {/* Content */}
      {isExpanded && (
        <div className="p-4 space-y-4 max-h-80 overflow-y-auto">
          {/* Primary Change */}
          <div className="bg-muted/50 rounded-lg p-3">
            <div className="text-xs text-muted-foreground mb-2">Moving</div>
            <div className="flex items-center gap-2">
              <div 
                className="w-3 h-3 rounded-full"
                style={{ backgroundColor: PHASE_CONFIGS[impact.phase].color }}
              />
              <span className="font-medium text-foreground">{impact.batchName}</span>
              <span className="text-muted-foreground">·</span>
              <span className="text-foreground/70">{PHASE_CONFIGS[impact.phase].label}</span>
            </div>
            <div className="flex items-center gap-2 mt-2 text-sm">
              <Calendar className="w-3.5 h-3.5 text-muted-foreground" />
              <span className="text-muted-foreground">
                {impact.originalStart.toLocaleDateString('en-US', { 
                  month: 'short', 
                  day: 'numeric' 
                })}
              </span>
              <ArrowRight className="w-3.5 h-3.5 text-cyan-400" />
              <span className="text-cyan-400 font-medium">
                {impact.newStart.toLocaleDateString('en-US', { 
                  month: 'short', 
                  day: 'numeric' 
                })}
              </span>
              <span className={cn(
                'text-xs px-1.5 py-0.5 rounded',
                isPositive 
                  ? 'bg-amber-500/20 text-amber-400' 
                  : 'bg-emerald-500/20 text-emerald-400'
              )}>
                {isPositive ? '+' : ''}{impact.daysDelta} {Math.abs(impact.daysDelta) === 1 ? 'day' : 'days'}
              </span>
            </div>
          </div>

          {/* Cascade Effects */}
          {hasCascade && (
            <div>
              <div className="text-xs text-muted-foreground mb-2 flex items-center gap-1">
                <Clock className="w-3 h-3" />
                Cascade Effects ({impact.cascadeEffects.length} phases affected)
              </div>
              <div className="space-y-1.5">
                {impact.cascadeEffects.map((effect, index) => (
                  <div 
                    key={effect.phaseId}
                    className="flex items-center gap-2 text-sm bg-muted/30 rounded px-2 py-1.5"
                  >
                    <div 
                      className="w-2 h-2 rounded-full"
                      style={{ backgroundColor: PHASE_CONFIGS[effect.phase].color }}
                    />
                    <span className="text-foreground/70">{PHASE_CONFIGS[effect.phase].label}</span>
                    <span className="ml-auto text-muted-foreground">
                      {effect.daysDelta > 0 ? '+' : ''}{effect.daysDelta}d
                    </span>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Conflicts */}
          {hasConflicts && (
            <div>
              <div className="text-xs text-red-400 mb-2 flex items-center gap-1">
                <AlertTriangle className="w-3 h-3" />
                Conflicts Detected
              </div>
              <div className="space-y-1.5">
                {conflicts.map((conflict) => (
                  <div 
                    key={conflict.id}
                    className="flex items-start gap-2 text-sm bg-red-500/10 border border-red-500/20 rounded px-2 py-1.5"
                  >
                    <AlertTriangle className="w-3.5 h-3.5 text-red-400 flex-shrink-0 mt-0.5" />
                    <span className="text-red-300">{conflict.message}</span>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      )}

      {/* Actions */}
      <div className="flex items-center gap-2 p-3 border-t border-border/50">
        <button
          onClick={onCancel}
          className="flex-1 flex items-center justify-center gap-2 px-4 py-2 text-sm font-medium text-foreground/70 bg-muted hover:bg-hover rounded-lg transition-colors"
        >
          <X className="w-4 h-4" />
          Cancel
        </button>
        <button
          onClick={onConfirm}
          disabled={hasConflicts && conflicts.some(c => c.severity === 'error')}
          className={cn(
            'flex-1 flex items-center justify-center gap-2 px-4 py-2 text-sm font-medium rounded-lg transition-colors',
            hasConflicts && conflicts.some(c => c.severity === 'error')
              ? 'bg-hover text-muted-foreground cursor-not-allowed'
              : 'bg-cyan-500 text-foreground hover:bg-cyan-400'
          )}
        >
          <Check className="w-4 h-4" />
          Apply Change
        </button>
      </div>

      {/* Force Apply Warning */}
      {hasConflicts && !conflicts.some(c => c.severity === 'error') && (
        <div className="px-3 pb-3">
          <div className="text-xs text-amber-400/80 text-center">
            ⚠️ Applying with warnings may cause scheduling issues
          </div>
        </div>
      )}
    </div>
  );
}

