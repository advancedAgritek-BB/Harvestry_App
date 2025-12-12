'use client';

import React from 'react';
import { cn } from '@/lib/utils';
import { X, Calendar, Clock, AlertTriangle, ArrowRight } from 'lucide-react';
import { format, differenceInDays } from 'date-fns';
import { BatchPhase, PhaseType } from '../../types/planner.types';
import { PHASE_CONFIGS } from '../../constants/phaseConfig';

interface PhaseAdjustmentDetails {
  phaseId: string;
  phaseName: string;
  phaseType: PhaseType;
  originalStart: Date;
  originalEnd: Date;
  newStart: Date;
  newEnd: Date;
  daysDelta: number;
  dragType: 'move' | 'resize-start' | 'resize-end';
}

interface PhaseAdjustmentConfirmDialogProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  batchName: string;
  adjustment: PhaseAdjustmentDetails;
  hasConflicts?: boolean;
  conflictMessage?: string;
}

export function PhaseAdjustmentConfirmDialog({
  isOpen,
  onClose,
  onConfirm,
  batchName,
  adjustment,
  hasConflicts = false,
  conflictMessage,
}: PhaseAdjustmentConfirmDialogProps) {
  if (!isOpen) return null;

  const config = PHASE_CONFIGS[adjustment.phaseType];
  const originalDuration = differenceInDays(adjustment.originalEnd, adjustment.originalStart) + 1;
  const newDuration = differenceInDays(adjustment.newEnd, adjustment.newStart) + 1;
  const durationChange = newDuration - originalDuration;

  const getActionDescription = () => {
    switch (adjustment.dragType) {
      case 'move':
        return adjustment.daysDelta > 0 
          ? `Move ${Math.abs(adjustment.daysDelta)} day${Math.abs(adjustment.daysDelta) !== 1 ? 's' : ''} later`
          : `Move ${Math.abs(adjustment.daysDelta)} day${Math.abs(adjustment.daysDelta) !== 1 ? 's' : ''} earlier`;
      case 'resize-start':
        return durationChange > 0
          ? `Extend start by ${Math.abs(durationChange)} day${Math.abs(durationChange) !== 1 ? 's' : ''}`
          : `Shorten start by ${Math.abs(durationChange)} day${Math.abs(durationChange) !== 1 ? 's' : ''}`;
      case 'resize-end':
        return durationChange > 0
          ? `Extend end by ${Math.abs(durationChange)} day${Math.abs(durationChange) !== 1 ? 's' : ''}`
          : `Shorten end by ${Math.abs(durationChange)} day${Math.abs(durationChange) !== 1 ? 's' : ''}`;
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      {/* Backdrop */}
      <div
        className="absolute inset-0 bg-background/60 backdrop-blur-sm"
        onClick={onClose}
      />

      {/* Dialog */}
      <div className="relative w-full max-w-md bg-surface border border-border rounded-xl shadow-2xl animate-in fade-in zoom-in-95 duration-200">
        {/* Header */}
        <div className="flex items-start justify-between px-6 py-4 border-b border-border">
          <div className="flex items-center gap-3">
            <div 
              className="w-10 h-10 rounded-lg flex items-center justify-center"
              style={{ background: `linear-gradient(135deg, ${config.gradientFrom}, ${config.gradientTo})` }}
            >
              <Calendar className="w-5 h-5 text-foreground" />
            </div>
            <div>
              <h2 className="text-lg font-semibold text-foreground">Confirm Schedule Change</h2>
              <p className="text-sm text-muted-foreground">{batchName}</p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-1.5 rounded-lg text-muted-foreground hover:text-foreground hover:bg-muted transition-colors"
            aria-label="Close dialog"
            title="Close"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Content */}
        <div className="px-6 py-4 space-y-4">
          {/* Phase being adjusted */}
          <div className="flex items-center gap-2 text-sm">
            <span className="text-muted-foreground">Phase:</span>
            <span 
              className="font-medium px-2 py-0.5 rounded"
              style={{ backgroundColor: `${config.color}20`, color: config.color }}
            >
              {config.label}
            </span>
          </div>

          {/* Action description */}
          <div className="p-3 bg-muted/50 rounded-lg">
            <div className="flex items-center gap-2 text-sm font-medium text-foreground">
              <Clock className="w-4 h-4 text-cyan-500" />
              {getActionDescription()}
            </div>
          </div>

          {/* Date comparison */}
          <div className="grid grid-cols-2 gap-4">
            {/* Original dates */}
            <div className="space-y-2">
              <div className="text-xs font-medium text-muted-foreground uppercase tracking-wider">
                Current
              </div>
              <div className="p-3 bg-muted/30 rounded-lg space-y-1">
                <div className="text-xs text-muted-foreground">Start</div>
                <div className="text-sm font-medium text-foreground">
                  {format(adjustment.originalStart, 'MMM d, yyyy')}
                </div>
                <div className="text-xs text-muted-foreground mt-2">End</div>
                <div className="text-sm font-medium text-foreground">
                  {format(adjustment.originalEnd, 'MMM d, yyyy')}
                </div>
                <div className="text-xs text-muted-foreground mt-2">Duration</div>
                <div className="text-sm font-medium text-foreground">
                  {originalDuration} day{originalDuration !== 1 ? 's' : ''}
                </div>
              </div>
            </div>

            {/* New dates */}
            <div className="space-y-2">
              <div className="text-xs font-medium text-cyan-500 uppercase tracking-wider">
                New
              </div>
              <div className="p-3 bg-cyan-500/10 border border-cyan-500/20 rounded-lg space-y-1">
                <div className="text-xs text-muted-foreground">Start</div>
                <div className={cn(
                  "text-sm font-medium",
                  adjustment.newStart.getTime() !== adjustment.originalStart.getTime() 
                    ? "text-cyan-500" 
                    : "text-foreground"
                )}>
                  {format(adjustment.newStart, 'MMM d, yyyy')}
                </div>
                <div className="text-xs text-muted-foreground mt-2">End</div>
                <div className={cn(
                  "text-sm font-medium",
                  adjustment.newEnd.getTime() !== adjustment.originalEnd.getTime() 
                    ? "text-cyan-500" 
                    : "text-foreground"
                )}>
                  {format(adjustment.newEnd, 'MMM d, yyyy')}
                </div>
                <div className="text-xs text-muted-foreground mt-2">Duration</div>
                <div className={cn(
                  "text-sm font-medium",
                  durationChange !== 0 ? "text-cyan-500" : "text-foreground"
                )}>
                  {newDuration} day{newDuration !== 1 ? 's' : ''}
                  {durationChange !== 0 && (
                    <span className="text-xs ml-1">
                      ({durationChange > 0 ? '+' : ''}{durationChange})
                    </span>
                  )}
                </div>
              </div>
            </div>
          </div>

          {/* Conflict warning */}
          {hasConflicts && (
            <div className="flex items-start gap-2 p-3 bg-red-500/10 border border-red-500/20 rounded-lg">
              <AlertTriangle className="w-4 h-4 text-red-500 flex-shrink-0 mt-0.5" />
              <div>
                <div className="text-sm font-medium text-red-400">
                  This change may cause conflicts
                </div>
                {conflictMessage && (
                  <div className="text-xs text-red-400/80 mt-1">{conflictMessage}</div>
                )}
              </div>
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="flex items-center justify-end gap-3 px-6 py-4 border-t border-border">
          <button
            onClick={onClose}
            className="px-4 py-2 text-sm font-medium text-muted-foreground hover:text-foreground hover:bg-muted rounded-lg transition-colors"
          >
            Cancel
          </button>
          <button
            onClick={onConfirm}
            className={cn(
              "px-4 py-2 text-sm font-medium rounded-lg transition-colors flex items-center gap-2",
              hasConflicts
                ? "bg-amber-600 hover:bg-amber-700 text-foreground"
                : "bg-cyan-600 hover:bg-cyan-700 text-foreground"
            )}
          >
            {hasConflicts ? 'Apply Anyway' : 'Apply Change'}
            <ArrowRight className="w-4 h-4" />
          </button>
        </div>
      </div>
    </div>
  );
}



