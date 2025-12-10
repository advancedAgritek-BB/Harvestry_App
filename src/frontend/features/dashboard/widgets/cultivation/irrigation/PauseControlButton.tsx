import React, { useState } from 'react';
import { cn } from '@/lib/utils';
import { Pause, Play, AlertTriangle, Clock, Settings } from 'lucide-react';
import type { ZonePauseState, PauseBehaviorMode, PauseConfig } from './types';

interface PauseControlButtonProps {
  /** Current pause state */
  pauseState: ZonePauseState;
  /** Selected zones to pause/resume */
  selectedZones: string[];
  /** Pause configuration */
  pauseConfig: PauseConfig;
  /** Callback when pause is toggled */
  onPauseToggle: (isPaused: boolean, zones: string[], mode: PauseBehaviorMode) => void;
  /** Callback to open config modal (admin only) */
  onOpenConfig?: () => void;
  /** Whether user is an admin */
  isAdmin?: boolean;
  /** Disabled state */
  disabled?: boolean;
}

/**
 * Pause/Resume control button with status indicator.
 * Shows current pause state and allows toggling irrigation on selected zones.
 */
export function PauseControlButton({
  pauseState,
  selectedZones,
  pauseConfig,
  onPauseToggle,
  onOpenConfig,
  isAdmin = false,
  disabled = false,
}: PauseControlButtonProps) {
  const [showConfirm, setShowConfirm] = useState(false);
  const [selectedMode, setSelectedMode] = useState<PauseBehaviorMode>(pauseConfig.behaviorMode);

  const hasSelectedZones = selectedZones.length > 0;
  const isPaused = pauseState.isPaused && pauseState.pausedZones.length > 0;
  
  // Check if any selected zones are paused
  const selectedZonesPaused = selectedZones.some(z => pauseState.pausedZones.includes(z));
  const allSelectedPaused = selectedZones.every(z => pauseState.pausedZones.includes(z));

  const handleClick = () => {
    if (disabled || !hasSelectedZones) return;
    
    if (isPaused && allSelectedPaused) {
      // Resume - no confirmation needed
      onPauseToggle(false, selectedZones, selectedMode);
    } else {
      // Pause - show confirmation
      setShowConfirm(true);
    }
  };

  const confirmPause = () => {
    onPauseToggle(true, selectedZones, selectedMode);
    setShowConfirm(false);
  };

  const getModeLabel = (mode: PauseBehaviorMode) => {
    return mode === 'resume_safety' ? 'Resume at day end' : 'Until manually resumed';
  };

  return (
    <>
      {/* Main Button */}
      <div className="flex items-center gap-1">
        <button
          onClick={handleClick}
          disabled={disabled || !hasSelectedZones}
          className={cn(
            "flex items-center gap-1.5 px-3 py-2 sm:py-1.5 text-xs font-medium rounded border transition-all min-h-[44px] sm:min-h-[32px] touch-manipulation",
            !hasSelectedZones && "opacity-50 cursor-not-allowed",
            disabled && "opacity-50 cursor-not-allowed",
            isPaused && allSelectedPaused
              ? "bg-amber-500/20 border-amber-500/50 text-amber-300 hover:bg-amber-500/30"
              : "bg-red-500/10 border-red-500/30 text-red-400 hover:bg-red-500/20"
          )}
          title={isPaused && allSelectedPaused ? "Resume irrigation" : "Pause irrigation"}
          aria-label={isPaused && allSelectedPaused ? "Resume irrigation on selected zones" : "Pause irrigation on selected zones"}
        >
          {isPaused && allSelectedPaused ? (
            <>
              <Play className="w-3.5 h-3.5" />
              <span className="hidden sm:inline">Resume</span>
            </>
          ) : (
            <>
              <Pause className="w-3.5 h-3.5" />
              <span className="hidden sm:inline">Pause</span>
            </>
          )}
        </button>

        {/* Config button for admins */}
        {isAdmin && onOpenConfig && (
          <button
            onClick={onOpenConfig}
            className="p-2 sm:p-1.5 text-muted-foreground hover:text-foreground hover:bg-muted rounded transition-colors touch-manipulation"
            title="Pause settings"
            aria-label="Configure pause behavior"
          >
            <Settings className="w-3.5 h-3.5" />
          </button>
        )}
      </div>

      {/* Pause Status Indicator */}
      {isPaused && (
        <div className="flex items-center gap-1.5 px-2 py-1 bg-amber-500/10 border border-amber-500/20 rounded text-[10px] text-amber-400">
          <Pause className="w-3 h-3" />
          <span className="font-medium">
            {pauseState.pausedZones.length} zone{pauseState.pausedZones.length !== 1 ? 's' : ''} paused
          </span>
          {pauseConfig.behaviorMode === 'resume_safety' && (
            <span className="text-amber-400/60">· auto-resume</span>
          )}
        </div>
      )}

      {/* Confirmation Modal */}
      {showConfirm && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-background/80 backdrop-blur-sm animate-in fade-in duration-200">
          <div className="bg-surface border border-border p-5 rounded-xl shadow-2xl max-w-sm w-full mx-4">
            <div className="w-12 h-12 rounded-full bg-red-500/20 text-red-500 flex items-center justify-center mx-auto mb-4">
              <AlertTriangle className="w-6 h-6" />
            </div>
            <h4 className="text-lg font-semibold text-foreground text-center mb-2">
              Pause Irrigation
            </h4>
            <p className="text-sm text-muted-foreground text-center mb-4">
              Pause irrigation on <span className="text-foreground font-bold">{selectedZones.length} zone{selectedZones.length !== 1 ? 's' : ''}</span>?
            </p>

            {/* Mode Selection */}
            <div className="bg-muted/30 border border-border/50 rounded-lg p-3 mb-4">
              <div className="text-[10px] font-medium text-muted-foreground uppercase tracking-wider mb-2">
                Resume Behavior
              </div>
              <div className="space-y-2">
                <label className={cn(
                  "flex items-start gap-3 p-2 rounded-lg cursor-pointer transition-colors",
                  selectedMode === 'resume_safety' 
                    ? "bg-cyan-500/10 border border-cyan-500/30" 
                    : "hover:bg-muted"
                )}>
                  <input
                    type="radio"
                    name="pauseMode"
                    value="resume_safety"
                    checked={selectedMode === 'resume_safety'}
                    onChange={() => setSelectedMode('resume_safety')}
                    disabled={!pauseConfig.allowModeOverride && !isAdmin}
                    className="mt-0.5"
                  />
                  <div className="flex-1">
                    <div className="flex items-center gap-1.5 text-xs font-medium text-foreground">
                      <Clock className="w-3 h-3 text-cyan-400" />
                      Resume Safety
                      <span className="text-[9px] px-1 py-0.5 bg-cyan-500/20 text-cyan-400 rounded">Default</span>
                    </div>
                    <p className="text-[10px] text-muted-foreground mt-0.5">
                      Auto-resumes when daily irrigation schedule completes
                    </p>
                  </div>
                </label>

                <label className={cn(
                  "flex items-start gap-3 p-2 rounded-lg cursor-pointer transition-colors",
                  selectedMode === 'permanent' 
                    ? "bg-amber-500/10 border border-amber-500/30" 
                    : "hover:bg-muted",
                  !pauseConfig.allowModeOverride && !isAdmin && "opacity-50 cursor-not-allowed"
                )}>
                  <input
                    type="radio"
                    name="pauseMode"
                    value="permanent"
                    checked={selectedMode === 'permanent'}
                    onChange={() => setSelectedMode('permanent')}
                    disabled={!pauseConfig.allowModeOverride && !isAdmin}
                    className="mt-0.5"
                  />
                  <div className="flex-1">
                    <div className="flex items-center gap-1.5 text-xs font-medium text-foreground">
                      <Pause className="w-3 h-3 text-amber-400" />
                      Permanent Pause
                    </div>
                    <p className="text-[10px] text-muted-foreground mt-0.5">
                      Stays paused until manually resumed
                    </p>
                  </div>
                </label>
              </div>
              
              {!pauseConfig.allowModeOverride && !isAdmin && (
                <p className="text-[9px] text-muted-foreground/60 mt-2 italic">
                  Contact an admin to change pause behavior
                </p>
              )}
            </div>

            <div className="grid grid-cols-2 gap-3">
              <button 
                onClick={() => setShowConfirm(false)}
                className="px-4 py-2 text-sm font-medium text-foreground/70 hover:text-foreground bg-muted hover:bg-muted/80 rounded-lg transition-colors"
              >
                Cancel
              </button>
              <button 
                onClick={confirmPause}
                className="px-4 py-2 text-sm font-medium text-white bg-red-500 hover:bg-red-400 rounded-lg transition-colors shadow-[0_0_15px_-3px_rgba(239,68,68,0.5)]"
              >
                Pause
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}


