import React, { useState } from 'react';
import { cn } from '@/lib/utils';
import { X, Clock, Save, Trash2 } from 'lucide-react';
import type { IrrigationWindow, IrrigationPeriod } from './types';

interface WindowEditModalProps {
  isOpen: boolean;
  onClose: () => void;
  windows: IrrigationWindow[];
  onSave: (windows: IrrigationWindow[]) => void;
  readOnly?: boolean;
}

// Default windows for demonstration
const DEFAULT_WINDOWS: IrrigationWindow[] = [
  { id: 'p1', period: 'P1 - Ramp', startTime: '06:00', endTime: '10:00', isActive: true },
  { id: 'p2', period: 'P2 - Maintenance', startTime: '10:00', endTime: '16:00', isActive: true },
  { id: 'p3', period: 'P3 - Dryback', startTime: '16:00', endTime: '20:00', isActive: true },
];

const PERIOD_COLORS: Record<Exclude<IrrigationPeriod, 'All'>, string> = {
  'P1 - Ramp': 'bg-emerald-500/20 text-emerald-400 border-emerald-500/30',
  'P2 - Maintenance': 'bg-cyan-500/20 text-cyan-400 border-cyan-500/30',
  'P3 - Dryback': 'bg-amber-500/20 text-amber-400 border-amber-500/30',
};

/**
 * Modal for editing irrigation window time ranges.
 * Allows configuring start/end times for P1, P2, P3 periods.
 */
export function WindowEditModal({
  isOpen,
  onClose,
  windows = DEFAULT_WINDOWS,
  onSave,
  readOnly = false,
}: WindowEditModalProps) {
  const [editedWindows, setEditedWindows] = useState<IrrigationWindow[]>(windows);
  const [hasChanges, setHasChanges] = useState(false);

  const handleTimeChange = (
    windowId: string, 
    field: 'startTime' | 'endTime', 
    value: string
  ) => {
    setEditedWindows(prev => 
      prev.map(w => w.id === windowId ? { ...w, [field]: value } : w)
    );
    setHasChanges(true);
  };

  const handleActiveToggle = (windowId: string) => {
    setEditedWindows(prev =>
      prev.map(w => w.id === windowId ? { ...w, isActive: !w.isActive } : w)
    );
    setHasChanges(true);
  };

  const handleSave = () => {
    onSave(editedWindows);
    setHasChanges(false);
    onClose();
  };

  const handleCancel = () => {
    setEditedWindows(windows);
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
      <div className="fixed left-1/2 top-1/2 z-50 -translate-x-1/2 -translate-y-1/2 w-full max-w-lg bg-surface border border-border rounded-2xl shadow-2xl animate-in fade-in zoom-in-95 duration-200">
        {/* Header */}
        <div className="flex items-center justify-between p-4 border-b border-border">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-cyan-500/20 flex items-center justify-center">
              <Clock className="w-5 h-5 text-cyan-400" />
            </div>
            <div>
              <h2 className="text-lg font-semibold text-foreground">Edit Windows</h2>
              <p className="text-xs text-muted-foreground">
                Configure irrigation period time ranges
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
        <div className="p-4 space-y-4">
          {editedWindows.map(window => (
            <div
              key={window.id}
              className={cn(
                "border rounded-xl p-4 transition-all",
                window.isActive 
                  ? "bg-muted/30 border-border" 
                  : "bg-muted/10 border-border/50 opacity-60"
              )}
            >
              {/* Period Header */}
              <div className="flex items-center justify-between mb-3">
                <span className={cn(
                  "px-2.5 py-1 text-xs font-medium rounded-lg border",
                  PERIOD_COLORS[window.period]
                )}>
                  {window.period}
                </span>
                <label className="flex items-center gap-2 cursor-pointer">
                  <span className="text-xs text-muted-foreground">Active</span>
                  <button
                    type="button"
                    role="switch"
                    aria-checked={window.isActive}
                    disabled={readOnly}
                    onClick={() => handleActiveToggle(window.id)}
                    className={cn(
                      "relative w-9 h-5 rounded-full transition-colors",
                      window.isActive ? "bg-cyan-500" : "bg-muted",
                      readOnly && "opacity-50 cursor-not-allowed"
                    )}
                  >
                    <span className={cn(
                      "absolute top-0.5 left-0.5 w-4 h-4 rounded-full bg-white shadow transition-transform",
                      window.isActive && "translate-x-4"
                    )} />
                  </button>
                </label>
              </div>

              {/* Time Inputs */}
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-[10px] font-medium text-muted-foreground uppercase tracking-wider mb-1.5">
                    Start Time
                  </label>
                  <input
                    type="time"
                    value={window.startTime}
                    onChange={(e) => handleTimeChange(window.id, 'startTime', e.target.value)}
                    disabled={readOnly || !window.isActive}
                    className={cn(
                      "w-full px-3 py-2 text-sm font-mono bg-background border border-border rounded-lg",
                      "focus:outline-none focus:ring-2 focus:ring-cyan-500/50 focus:border-cyan-500",
                      "disabled:opacity-50 disabled:cursor-not-allowed"
                    )}
                  />
                </div>
                <div>
                  <label className="block text-[10px] font-medium text-muted-foreground uppercase tracking-wider mb-1.5">
                    End Time
                  </label>
                  <input
                    type="time"
                    value={window.endTime}
                    onChange={(e) => handleTimeChange(window.id, 'endTime', e.target.value)}
                    disabled={readOnly || !window.isActive}
                    className={cn(
                      "w-full px-3 py-2 text-sm font-mono bg-background border border-border rounded-lg",
                      "focus:outline-none focus:ring-2 focus:ring-cyan-500/50 focus:border-cyan-500",
                      "disabled:opacity-50 disabled:cursor-not-allowed"
                    )}
                  />
                </div>
              </div>

              {/* Duration Display */}
              <div className="mt-2 text-[10px] text-muted-foreground">
                Duration: {calculateDuration(window.startTime, window.endTime)}
              </div>
            </div>
          ))}

          {/* Info */}
          <div className="bg-muted/30 border border-border/50 rounded-lg p-3 text-xs text-muted-foreground">
            <strong className="text-foreground">Note:</strong> Windows define when each irrigation phase is active.
            The system will follow these schedules for automated irrigation.
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
              Save Changes
            </button>
          )}
        </div>
      </div>
    </>
  );
}

function calculateDuration(start: string, end: string): string {
  const [startH, startM] = start.split(':').map(Number);
  const [endH, endM] = end.split(':').map(Number);
  
  let totalMinutes = (endH * 60 + endM) - (startH * 60 + startM);
  if (totalMinutes < 0) totalMinutes += 24 * 60; // Handle overnight
  
  const hours = Math.floor(totalMinutes / 60);
  const minutes = totalMinutes % 60;
  
  if (hours === 0) return `${minutes}m`;
  if (minutes === 0) return `${hours}h`;
  return `${hours}h ${minutes}m`;
}


