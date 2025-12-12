import React, { useState } from 'react';
import { cn } from '@/lib/utils';
import { X, Shield, Clock, Pause, Save, Info } from 'lucide-react';
import type { PauseConfig, PauseBehaviorMode } from './types';
import { DEFAULT_PAUSE_CONFIG } from './types';

interface PauseConfigModalProps {
  isOpen: boolean;
  onClose: () => void;
  config: PauseConfig;
  onSave: (config: PauseConfig) => void;
}

/**
 * Admin-only modal for configuring pause behavior at the organization level.
 */
export function PauseConfigModal({
  isOpen,
  onClose,
  config = DEFAULT_PAUSE_CONFIG,
  onSave,
}: PauseConfigModalProps) {
  const [editedConfig, setEditedConfig] = useState<PauseConfig>(config);
  const [hasChanges, setHasChanges] = useState(false);

  const handleModeChange = (mode: PauseBehaviorMode) => {
    setEditedConfig(prev => ({ ...prev, behaviorMode: mode }));
    setHasChanges(true);
  };

  const handleOverrideToggle = () => {
    setEditedConfig(prev => ({ ...prev, allowModeOverride: !prev.allowModeOverride }));
    setHasChanges(true);
  };

  const handleSave = () => {
    onSave(editedConfig);
    setHasChanges(false);
    onClose();
  };

  const handleCancel = () => {
    setEditedConfig(config);
    setHasChanges(false);
    onClose();
  };

  if (!isOpen) return null;

  return (
    <>
      {/* Backdrop */}
      <div 
        className="fixed inset-0 z-50 bg-background/80 backdrop-blur-sm animate-in fade-in duration-200"
        onClick={handleCancel}
      />
      
      {/* Modal */}
      <div className="fixed left-1/2 top-1/2 z-50 -translate-x-1/2 -translate-y-1/2 w-full max-w-md bg-surface border border-border rounded-2xl shadow-2xl animate-in fade-in zoom-in-95 duration-200">
        {/* Header */}
        <div className="flex items-center justify-between p-4 border-b border-border">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-purple-500/20 flex items-center justify-center">
              <Shield className="w-5 h-5 text-purple-400" />
            </div>
            <div>
              <h2 className="text-lg font-semibold text-foreground">Pause Settings</h2>
              <p className="text-xs text-muted-foreground">
                Organization-level configuration
              </p>
            </div>
          </div>
          <button
            onClick={handleCancel}
            className="p-2 text-muted-foreground hover:text-foreground rounded-lg hover:bg-muted transition-colors"
            title="Close"
            aria-label="Close modal"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Content */}
        <div className="p-4 space-y-5">
          {/* Admin Notice */}
          <div className="flex items-start gap-2 p-3 bg-purple-500/10 border border-purple-500/20 rounded-lg">
            <Info className="w-4 h-4 text-purple-400 mt-0.5 shrink-0" />
            <p className="text-xs text-purple-300">
              These settings apply to all users in your organization. Changes take effect immediately.
            </p>
          </div>

          {/* Default Behavior Mode */}
          <div>
            <label className="block text-xs font-medium text-foreground mb-3">
              Default Pause Behavior
            </label>
            <div className="space-y-2">
              <button
                onClick={() => handleModeChange('resume_safety')}
                className={cn(
                  "w-full flex items-start gap-3 p-3 rounded-xl border transition-all text-left",
                  editedConfig.behaviorMode === 'resume_safety'
                    ? "bg-cyan-500/10 border-cyan-500/30"
                    : "bg-muted/30 border-border hover:border-border/80"
                )}
              >
                <div className={cn(
                  "w-8 h-8 rounded-lg flex items-center justify-center shrink-0",
                  editedConfig.behaviorMode === 'resume_safety'
                    ? "bg-cyan-500/20"
                    : "bg-muted"
                )}>
                  <Clock className={cn(
                    "w-4 h-4",
                    editedConfig.behaviorMode === 'resume_safety'
                      ? "text-cyan-400"
                      : "text-muted-foreground"
                  )} />
                </div>
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2">
                    <span className="text-sm font-medium text-foreground">Resume Safety</span>
                    <span className="text-[9px] px-1.5 py-0.5 bg-cyan-500/20 text-cyan-400 rounded font-medium">
                      Recommended
                    </span>
                  </div>
                  <p className="text-[11px] text-muted-foreground mt-1">
                    Paused zones automatically resume when the daily irrigation schedule completes and resets for the next day. This prevents accidental permanent pauses.
                  </p>
                </div>
                <div className={cn(
                  "w-5 h-5 rounded-full border-2 flex items-center justify-center shrink-0",
                  editedConfig.behaviorMode === 'resume_safety'
                    ? "border-cyan-500 bg-cyan-500"
                    : "border-muted-foreground/30"
                )}>
                  {editedConfig.behaviorMode === 'resume_safety' && (
                    <div className="w-2 h-2 rounded-full bg-white" />
                  )}
                </div>
              </button>

              <button
                onClick={() => handleModeChange('permanent')}
                className={cn(
                  "w-full flex items-start gap-3 p-3 rounded-xl border transition-all text-left",
                  editedConfig.behaviorMode === 'permanent'
                    ? "bg-amber-500/10 border-amber-500/30"
                    : "bg-muted/30 border-border hover:border-border/80"
                )}
              >
                <div className={cn(
                  "w-8 h-8 rounded-lg flex items-center justify-center shrink-0",
                  editedConfig.behaviorMode === 'permanent'
                    ? "bg-amber-500/20"
                    : "bg-muted"
                )}>
                  <Pause className={cn(
                    "w-4 h-4",
                    editedConfig.behaviorMode === 'permanent'
                      ? "text-amber-400"
                      : "text-muted-foreground"
                  )} />
                </div>
                <div className="flex-1 min-w-0">
                  <span className="text-sm font-medium text-foreground">Permanent Pause</span>
                  <p className="text-[11px] text-muted-foreground mt-1">
                    Paused zones stay paused indefinitely until manually resumed. Use with caution as this can lead to missed irrigation events.
                  </p>
                </div>
                <div className={cn(
                  "w-5 h-5 rounded-full border-2 flex items-center justify-center shrink-0",
                  editedConfig.behaviorMode === 'permanent'
                    ? "border-amber-500 bg-amber-500"
                    : "border-muted-foreground/30"
                )}>
                  {editedConfig.behaviorMode === 'permanent' && (
                    <div className="w-2 h-2 rounded-full bg-white" />
                  )}
                </div>
              </button>
            </div>
          </div>

          {/* Allow Override Toggle */}
          <div className="flex items-center justify-between p-3 bg-muted/30 border border-border/50 rounded-xl">
            <div className="flex-1 min-w-0 pr-3">
              <div className="text-sm font-medium text-foreground">Allow User Override</div>
              <p className="text-[11px] text-muted-foreground mt-0.5">
                Let non-admin users choose pause behavior when pausing
              </p>
            </div>
            <button
              type="button"
              role="switch"
              aria-checked={editedConfig.allowModeOverride ? "true" : "false"}
              aria-label="Toggle allow user override"
              title={editedConfig.allowModeOverride ? "Disable user override" : "Enable user override"}
              onClick={handleOverrideToggle}
              className={cn(
                "relative w-11 h-6 rounded-full transition-colors shrink-0",
                editedConfig.allowModeOverride ? "bg-cyan-500" : "bg-muted"
              )}
            >
              <span className={cn(
                "absolute top-1 left-1 w-4 h-4 rounded-full bg-white shadow transition-transform",
                editedConfig.allowModeOverride && "translate-x-5"
              )} />
            </button>
          </div>
        </div>

        {/* Footer */}
        <div className="flex items-center justify-end gap-3 p-4 border-t border-border">
          <button
            onClick={handleCancel}
            className="px-4 py-2 text-sm font-medium text-foreground/70 hover:text-foreground bg-muted hover:bg-muted/80 rounded-lg transition-colors"
          >
            Cancel
          </button>
          <button
            onClick={handleSave}
            disabled={!hasChanges}
            className={cn(
              "flex items-center gap-2 px-4 py-2 text-sm font-medium rounded-lg transition-colors",
              hasChanges
                ? "text-background bg-cyan-500 hover:bg-cyan-400 shadow-[0_0_15px_-3px_rgba(6,182,212,0.5)]"
                : "text-muted-foreground bg-muted cursor-not-allowed"
            )}
          >
            <Save className="w-4 h-4" />
            Save Settings
          </button>
        </div>
      </div>
    </>
  );
}



