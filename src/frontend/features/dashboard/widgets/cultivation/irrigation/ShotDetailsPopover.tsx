import React from 'react';
import { cn } from '@/lib/utils';
import { Droplets, Clock, MapPin, Edit2, X } from 'lucide-react';
import type { IrrigationDataPoint } from './types';
import { IRRIGATION_COLORS } from './types';

interface ShotDetailsPopoverProps {
  shot: IrrigationDataPoint;
  position: { x: number; y: number };
  onClose: () => void;
  onEdit?: (shot: IrrigationDataPoint) => void;
  readOnly?: boolean;
}

/**
 * Popover displaying shot details when a bar is clicked.
 * Positioned near the clicked bar with edit option.
 */
export function ShotDetailsPopover({
  shot,
  position,
  onClose,
  onEdit,
  readOnly = false,
}: ShotDetailsPopoverProps) {
  const isManual = shot.type === 'manual';

  return (
    <>
      {/* Backdrop */}
      <div 
        className="fixed inset-0 z-40" 
        onClick={onClose}
      />
      
      {/* Popover */}
      <div
        className="absolute z-50 bg-surface border border-border rounded-xl shadow-2xl w-64 animate-in fade-in zoom-in-95 duration-150"
        style={{
          left: Math.min(position.x, window.innerWidth - 280),
          top: Math.max(position.y - 100, 10),
        }}
      >
        {/* Header */}
        <div className="flex items-center justify-between p-3 border-b border-border">
          <div className="flex items-center gap-2">
            <div className={cn(
              "w-8 h-8 rounded-lg flex items-center justify-center",
              isManual ? "bg-amber-500/20" : "bg-blue-500/20"
            )}>
              <Droplets className={cn(
                "w-4 h-4",
                isManual ? "text-amber-400" : "text-blue-400"
              )} />
            </div>
            <div>
              <div className="text-sm font-semibold text-foreground">
                {shot.volume} mL
              </div>
              <div className={cn(
                "text-[10px] font-medium uppercase tracking-wider",
                isManual ? "text-amber-400" : "text-blue-400"
              )}>
                {isManual ? 'Manual Shot' : 'Automated Shot'}
              </div>
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-1 text-muted-foreground hover:text-foreground rounded transition-colors"
            title="Close"
            aria-label="Close shot details"
          >
            <X className="w-4 h-4" />
          </button>
        </div>

        {/* Details */}
        <div className="p-3 space-y-2.5">
          <div className="flex items-center gap-2 text-xs">
            <Clock className="w-3.5 h-3.5 text-muted-foreground" />
            <span className="text-muted-foreground">Time:</span>
            <span className="text-foreground font-medium ml-auto">{shot.time}</span>
          </div>
          
          <div className="flex items-center gap-2 text-xs">
            <MapPin className="w-3.5 h-3.5 text-muted-foreground" />
            <span className="text-muted-foreground">Zone:</span>
            <span className="text-foreground font-medium ml-auto">{shot.zone}</span>
          </div>
          
          <div className="flex items-center gap-2 text-xs">
            <div 
              className="w-3.5 h-3.5 rounded-sm" 
              style={{ backgroundColor: IRRIGATION_COLORS.vwc }}
            />
            <span className="text-muted-foreground">End VWC:</span>
            <span className="text-foreground font-medium ml-auto">
              {shot.endVwc?.toFixed(1) ?? '—'}%
            </span>
          </div>

          {shot.notes && (
            <div className="pt-2 border-t border-border/50">
              <p className="text-[11px] text-muted-foreground italic">
                {shot.notes}
              </p>
            </div>
          )}
        </div>

        {/* Actions */}
        {!readOnly && onEdit && (
          <div className="p-3 pt-0">
            <button
              onClick={() => onEdit(shot)}
              className="w-full flex items-center justify-center gap-1.5 px-3 py-1.5 text-xs font-medium text-foreground bg-muted hover:bg-muted/80 border border-border rounded-lg transition-colors"
            >
              <Edit2 className="w-3 h-3" />
              Edit Shot
            </button>
          </div>
        )}
      </div>
    </>
  );
}


