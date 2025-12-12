import React, { useState } from 'react';
import { cn } from '@/lib/utils';
import { X, Bell, AlertTriangle, Save } from 'lucide-react';
import type { VwcThresholdConfig } from './types';
import { DEFAULT_VWC_THRESHOLDS } from './types';

interface AlertsConfigModalProps {
  isOpen: boolean;
  onClose: () => void;
  config: VwcThresholdConfig;
  onSave: (config: VwcThresholdConfig) => void;
  readOnly?: boolean;
}

/**
 * Modal for configuring VWC threshold alerts.
 * Allows setting high/low thresholds and enabling/disabling alerts.
 */
export function AlertsConfigModal({
  isOpen,
  onClose,
  config = DEFAULT_VWC_THRESHOLDS,
  onSave,
  readOnly = false,
}: AlertsConfigModalProps) {
  const [editedConfig, setEditedConfig] = useState<VwcThresholdConfig>(config);
  const [hasChanges, setHasChanges] = useState(false);

  const handleThresholdChange = (
    field: 'lowThreshold' | 'highThreshold',
    value: number
  ) => {
    setEditedConfig(prev => ({ ...prev, [field]: value }));
    setHasChanges(true);
  };

  const handleAlertToggle = (field: 'alertOnLow' | 'alertOnHigh') => {
    setEditedConfig(prev => ({ ...prev, [field]: !prev[field] }));
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

  const handleReset = () => {
    setEditedConfig(DEFAULT_VWC_THRESHOLDS);
    setHasChanges(true);
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
            <div className="w-10 h-10 rounded-xl bg-amber-500/20 flex items-center justify-center">
              <Bell className="w-5 h-5 text-amber-400" />
            </div>
            <div>
              <h2 className="text-lg font-semibold text-foreground">VWC Alerts</h2>
              <p className="text-xs text-muted-foreground">
                Configure threshold alerts
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
          {/* Low Threshold */}
          <div className="bg-red-500/5 border border-red-500/20 rounded-xl p-4">
            <div className="flex items-center justify-between mb-3">
              <div className="flex items-center gap-2">
                <AlertTriangle className="w-4 h-4 text-red-400" />
                <span className="text-sm font-medium text-foreground">Low VWC Alert</span>
              </div>
              <label className="flex items-center gap-2 cursor-pointer">
                <span className="text-xs text-muted-foreground">Active</span>
                <button
                  type="button"
                  role="switch"
                  aria-checked={editedConfig.alertOnLow}
                  disabled={readOnly}
                  onClick={() => handleAlertToggle('alertOnLow')}
                  className={cn(
                    "relative w-9 h-5 rounded-full transition-colors",
                    editedConfig.alertOnLow ? "bg-red-500" : "bg-muted",
                    readOnly && "opacity-50 cursor-not-allowed"
                  )}
                >
                  <span className={cn(
                    "absolute top-0.5 left-0.5 w-4 h-4 rounded-full bg-white shadow transition-transform",
                    editedConfig.alertOnLow && "translate-x-4"
                  )} />
                </button>
              </label>
            </div>
            
            <div>
              <label className="block text-[10px] font-medium text-muted-foreground uppercase tracking-wider mb-2">
                Threshold (%)
              </label>
              <div className="flex items-center gap-3">
                <input
                  type="range"
                  min={0}
                  max={50}
                  value={editedConfig.lowThreshold}
                  onChange={(e) => handleThresholdChange('lowThreshold', Number(e.target.value))}
                  disabled={readOnly || !editedConfig.alertOnLow}
                  className={cn(
                    "flex-1 h-2 bg-muted rounded-full appearance-none cursor-pointer",
                    "[&::-webkit-slider-thumb]:appearance-none [&::-webkit-slider-thumb]:w-4 [&::-webkit-slider-thumb]:h-4 [&::-webkit-slider-thumb]:rounded-full [&::-webkit-slider-thumb]:bg-red-500 [&::-webkit-slider-thumb]:cursor-pointer",
                    "disabled:opacity-50 disabled:cursor-not-allowed"
                  )}
                />
                <div className="w-16 px-2 py-1 text-sm font-mono text-center bg-background border border-border rounded">
                  {editedConfig.lowThreshold}%
                </div>
              </div>
              <p className="mt-2 text-[10px] text-muted-foreground">
                Alert when VWC drops below this level
              </p>
            </div>
          </div>

          {/* High Threshold */}
          <div className="bg-amber-500/5 border border-amber-500/20 rounded-xl p-4">
            <div className="flex items-center justify-between mb-3">
              <div className="flex items-center gap-2">
                <AlertTriangle className="w-4 h-4 text-amber-400" />
                <span className="text-sm font-medium text-foreground">High VWC Alert</span>
              </div>
              <label className="flex items-center gap-2 cursor-pointer">
                <span className="text-xs text-muted-foreground">Active</span>
                <button
                  type="button"
                  role="switch"
                  aria-checked={editedConfig.alertOnHigh}
                  disabled={readOnly}
                  onClick={() => handleAlertToggle('alertOnHigh')}
                  className={cn(
                    "relative w-9 h-5 rounded-full transition-colors",
                    editedConfig.alertOnHigh ? "bg-amber-500" : "bg-muted",
                    readOnly && "opacity-50 cursor-not-allowed"
                  )}
                >
                  <span className={cn(
                    "absolute top-0.5 left-0.5 w-4 h-4 rounded-full bg-white shadow transition-transform",
                    editedConfig.alertOnHigh && "translate-x-4"
                  )} />
                </button>
              </label>
            </div>
            
            <div>
              <label className="block text-[10px] font-medium text-muted-foreground uppercase tracking-wider mb-2">
                Threshold (%)
              </label>
              <div className="flex items-center gap-3">
                <input
                  type="range"
                  min={50}
                  max={100}
                  value={editedConfig.highThreshold}
                  onChange={(e) => handleThresholdChange('highThreshold', Number(e.target.value))}
                  disabled={readOnly || !editedConfig.alertOnHigh}
                  className={cn(
                    "flex-1 h-2 bg-muted rounded-full appearance-none cursor-pointer",
                    "[&::-webkit-slider-thumb]:appearance-none [&::-webkit-slider-thumb]:w-4 [&::-webkit-slider-thumb]:h-4 [&::-webkit-slider-thumb]:rounded-full [&::-webkit-slider-thumb]:bg-amber-500 [&::-webkit-slider-thumb]:cursor-pointer",
                    "disabled:opacity-50 disabled:cursor-not-allowed"
                  )}
                />
                <div className="w-16 px-2 py-1 text-sm font-mono text-center bg-background border border-border rounded">
                  {editedConfig.highThreshold}%
                </div>
              </div>
              <p className="mt-2 text-[10px] text-muted-foreground">
                Alert when VWC exceeds this level
              </p>
            </div>
          </div>

          {/* Visual Preview */}
          <div className="bg-muted/30 border border-border/50 rounded-lg p-3">
            <div className="text-[10px] font-medium text-muted-foreground uppercase tracking-wider mb-2">
              Threshold Preview
            </div>
            <div className="relative h-4 bg-gradient-to-r from-red-500/30 via-emerald-500/30 to-amber-500/30 rounded overflow-hidden">
              {/* Low threshold marker */}
              <div 
                className="absolute top-0 bottom-0 w-0.5 bg-red-500"
                style={{ left: `${editedConfig.lowThreshold}%` }}
              />
              {/* High threshold marker */}
              <div 
                className="absolute top-0 bottom-0 w-0.5 bg-amber-500"
                style={{ left: `${editedConfig.highThreshold}%` }}
              />
            </div>
            <div className="flex justify-between mt-1 text-[9px] text-muted-foreground">
              <span>0%</span>
              <span className="text-red-400">{editedConfig.lowThreshold}%</span>
              <span className="text-emerald-400">Optimal</span>
              <span className="text-amber-400">{editedConfig.highThreshold}%</span>
              <span>100%</span>
            </div>
          </div>
        </div>

        {/* Footer */}
        <div className="flex items-center justify-between p-4 border-t border-border">
          <button
            onClick={handleReset}
            disabled={readOnly}
            className={cn(
              "text-xs text-muted-foreground hover:text-foreground transition-colors",
              readOnly && "opacity-50 cursor-not-allowed"
            )}
          >
            Reset to defaults
          </button>
          <div className="flex items-center gap-3">
            <button
              onClick={handleCancel}
              className="px-4 py-2 text-sm font-medium text-foreground/70 hover:text-foreground bg-muted hover:bg-muted/80 rounded-lg transition-colors"
            >
              Cancel
            </button>
            {!readOnly && (
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
                Save
              </button>
            )}
          </div>
        </div>
      </div>
    </>
  );
}



