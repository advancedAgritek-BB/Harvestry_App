import React, { useState, useEffect } from 'react';
import { X, Plus, Trash2, GripVertical } from 'lucide-react';
import { cn } from '@/lib/utils';
import type { QuickPickConfig } from './types';

interface QuickPickConfigModalProps {
  isOpen: boolean;
  onClose: () => void;
  config: QuickPickConfig;
  onSave: (config: QuickPickConfig) => void;
}

const MAX_VOLUMES = 5;
const MIN_VOLUME = 10;
const MAX_VOLUME = 500;

/**
 * Admin modal for configuring quick pick volume options.
 * Allows adding, removing, and reordering up to 5 volume presets.
 */
export function QuickPickConfigModal({ isOpen, onClose, config, onSave }: QuickPickConfigModalProps) {
  const [volumes, setVolumes] = useState<number[]>([]);
  const [newVolume, setNewVolume] = useState<string>('');
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen) {
      setVolumes([...config.volumes]);
      setNewVolume('');
      setError(null);
    }
  }, [isOpen, config.volumes]);

  if (!isOpen) return null;

  const handleAddVolume = () => {
    const vol = parseInt(newVolume, 10);
    if (isNaN(vol)) {
      setError('Please enter a valid number');
      return;
    }
    if (vol < MIN_VOLUME || vol > MAX_VOLUME) {
      setError(`Volume must be between ${MIN_VOLUME}-${MAX_VOLUME} mL`);
      return;
    }
    if (volumes.includes(vol)) {
      setError('This volume already exists');
      return;
    }
    if (volumes.length >= MAX_VOLUMES) {
      setError(`Maximum ${MAX_VOLUMES} volumes allowed`);
      return;
    }
    setVolumes([...volumes, vol].sort((a, b) => a - b));
    setNewVolume('');
    setError(null);
  };

  const handleRemoveVolume = (vol: number) => {
    setVolumes(volumes.filter(v => v !== vol));
    setError(null);
  };

  const handleSave = () => {
    if (volumes.length === 0) {
      setError('At least one volume is required');
      return;
    }
    onSave({ volumes: [...volumes].sort((a, b) => a - b) });
    onClose();
  };

  const canAdd = volumes.length < MAX_VOLUMES && newVolume.trim() !== '';

  return (
    <div className="absolute inset-0 z-50 flex items-center justify-center bg-background/80 backdrop-blur-sm rounded-xl animate-in fade-in duration-200">
      <div className="bg-surface border border-border rounded-xl shadow-2xl w-full max-w-sm mx-4 overflow-hidden">
        {/* Header */}
        <div className="flex items-center justify-between p-4 border-b border-border">
          <h3 className="text-base font-semibold text-foreground">Quick Pick Volumes</h3>
          <button
            onClick={onClose}
            className="p-1.5 rounded-lg hover:bg-muted transition-colors"
            title="Close"
            aria-label="Close quick pick configuration"
          >
            <X className="w-4 h-4 text-muted-foreground" />
          </button>
        </div>

        {/* Content */}
        <div className="p-4 space-y-4">
          <p className="text-xs text-muted-foreground">
            Configure up to {MAX_VOLUMES} quick pick volume options for your team.
          </p>

          {/* Current Volumes */}
          <div className="space-y-2">
            <label className="text-xs font-medium text-muted-foreground uppercase tracking-wider">
              Current Options ({volumes.length}/{MAX_VOLUMES})
            </label>
            <div className="flex flex-wrap gap-2">
              {volumes.length === 0 ? (
                <span className="text-xs text-muted-foreground/60 italic">No volumes configured</span>
              ) : (
                volumes.map(vol => (
                  <div
                    key={vol}
                    className="flex items-center gap-1.5 px-3 py-1.5 bg-cyan-500/10 border border-cyan-500/30 rounded-lg group"
                  >
                    <GripVertical className="w-3 h-3 text-muted-foreground/50" />
                    <span className="text-sm font-mono font-medium text-cyan-300">{vol} mL</span>
                    <button
                      onClick={() => handleRemoveVolume(vol)}
                      className="p-0.5 rounded hover:bg-red-500/20 text-muted-foreground hover:text-red-400 transition-colors"
                      title={`Remove ${vol} mL`}
                      aria-label={`Remove ${vol} mL option`}
                    >
                      <Trash2 className="w-3 h-3" />
                    </button>
                  </div>
                ))
              )}
            </div>
          </div>

          {/* Add New Volume */}
          <div className="space-y-2">
            <label className="text-xs font-medium text-muted-foreground uppercase tracking-wider">
              Add Volume
            </label>
            <div className="flex gap-2">
              <div className="relative flex-1">
                <input
                  type="number"
                  value={newVolume}
                  onChange={e => { setNewVolume(e.target.value); setError(null); }}
                  onKeyDown={e => e.key === 'Enter' && canAdd && handleAddVolume()}
                  placeholder="e.g. 150"
                  min={MIN_VOLUME}
                  max={MAX_VOLUME}
                  className="w-full px-3 py-2 pr-10 text-sm bg-muted/50 border border-border rounded-lg focus:outline-none focus:ring-1 focus:ring-cyan-500/50 focus:border-cyan-500/50"
                  disabled={volumes.length >= MAX_VOLUMES}
                />
                <span className="absolute right-3 top-1/2 -translate-y-1/2 text-xs text-muted-foreground">
                  mL
                </span>
              </div>
              <button
                onClick={handleAddVolume}
                disabled={!canAdd}
                className={cn(
                  "px-3 py-2 rounded-lg flex items-center gap-1.5 text-sm font-medium transition-colors",
                  canAdd
                    ? "bg-cyan-500/20 text-cyan-300 hover:bg-cyan-500/30 border border-cyan-500/30"
                    : "bg-muted/30 text-muted-foreground/50 border border-border/50 cursor-not-allowed"
                )}
              >
                <Plus className="w-4 h-4" />
                Add
              </button>
            </div>
            {error && (
              <p className="text-xs text-red-400">{error}</p>
            )}
          </div>
        </div>

        {/* Footer */}
        <div className="flex justify-end gap-2 p-4 border-t border-border bg-muted/30">
          <button
            onClick={onClose}
            className="px-4 py-2 text-sm font-medium text-foreground/70 hover:text-foreground bg-muted hover:bg-muted/80 rounded-lg transition-colors"
          >
            Cancel
          </button>
          <button
            onClick={handleSave}
            disabled={volumes.length === 0}
            className={cn(
              "px-4 py-2 text-sm font-medium rounded-lg transition-colors",
              volumes.length > 0
                ? "text-background bg-cyan-500 hover:bg-cyan-400"
                : "text-muted-foreground/50 bg-muted/50 cursor-not-allowed"
            )}
          >
            Save Changes
          </button>
        </div>
      </div>
    </div>
  );
}



