import React, { useState, useRef, useEffect } from 'react';
import { cn } from '@/lib/utils';
import { ChevronDown, Check, X } from 'lucide-react';
import type { IrrigationZone } from './types';

interface ZoneSelectorProps {
  zones: IrrigationZone[];
  selectedZones: string[];
  onSelectionChange: (zones: string[]) => void;
  disabled?: boolean;
  /** Threshold to switch from buttons to dropdown (default: 8) */
  dropdownThreshold?: number;
}

/**
 * Zone selector that adapts between inline buttons and dropdown
 * based on zone count. Supports All/None quick toggles.
 */
export function ZoneSelector({
  zones,
  selectedZones,
  onSelectionChange,
  disabled = false,
  dropdownThreshold = 8,
}: ZoneSelectorProps) {
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  const useDropdown = zones.length > dropdownThreshold;
  const allSelected = selectedZones.length === zones.length;
  const noneSelected = selectedZones.length === 0;

  // Close dropdown on outside click
  useEffect(() => {
    if (!isOpen) return;
    
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [isOpen]);

  const toggleZone = (zoneId: string) => {
    if (disabled) return;
    
    if (selectedZones.includes(zoneId)) {
      onSelectionChange(selectedZones.filter(z => z !== zoneId));
    } else {
      onSelectionChange([...selectedZones, zoneId].sort());
    }
  };

  const selectAll = () => {
    if (disabled) return;
    onSelectionChange(zones.map(z => z.id));
  };

  const selectNone = () => {
    if (disabled) return;
    onSelectionChange([]);
  };

  // Inline buttons for small zone counts
  if (!useDropdown) {
    return (
      <div className="flex items-center gap-2 overflow-x-auto pb-1">
        {/* Quick toggles */}
        <div className="flex items-center gap-1">
          <button
            onClick={selectAll}
            disabled={disabled || allSelected}
            className={cn(
              "px-2 py-1 text-[10px] font-medium rounded transition-colors min-h-[32px]",
              allSelected
                ? "bg-muted/30 text-muted-foreground cursor-not-allowed"
                : "bg-muted/50 text-muted-foreground hover:bg-muted hover:text-foreground",
              disabled && "opacity-50 cursor-not-allowed"
            )}
          >
            All
          </button>
          <button
            onClick={selectNone}
            disabled={disabled || noneSelected}
            className={cn(
              "px-2 py-1 text-[10px] font-medium rounded transition-colors min-h-[32px]",
              noneSelected
                ? "bg-muted/30 text-muted-foreground cursor-not-allowed"
                : "bg-muted/50 text-muted-foreground hover:bg-muted hover:text-foreground",
              disabled && "opacity-50 cursor-not-allowed"
            )}
          >
            None
          </button>
        </div>

        <div className="w-px h-6 bg-border" />

        {/* Zone buttons */}
        {zones.map(zone => (
          <button
            key={zone.id}
            onClick={() => toggleZone(zone.id)}
            disabled={disabled}
            className={cn(
              "flex items-center justify-center min-w-[32px] min-h-[32px] h-8 px-2 text-xs font-bold rounded border transition-all",
              selectedZones.includes(zone.id)
                ? "bg-blue-500/20 border-blue-500/50 text-blue-300"
                : "bg-muted/50 border-border text-muted-foreground hover:border-border/80",
              disabled && "opacity-50 cursor-not-allowed"
            )}
          >
            {zone.name}
          </button>
        ))}

        {/* Selection count badge */}
        <span className="text-[10px] text-muted-foreground whitespace-nowrap ml-1">
          {selectedZones.length} of {zones.length}
        </span>
      </div>
    );
  }

  // Dropdown for large zone counts
  return (
    <div className="relative" ref={dropdownRef}>
      <button
        onClick={() => !disabled && setIsOpen(!isOpen)}
        disabled={disabled}
        className={cn(
          "flex items-center gap-2 px-3 py-1.5 text-xs font-medium rounded border transition-colors min-h-[32px]",
          "bg-muted/50 border-border text-foreground hover:bg-muted",
          disabled && "opacity-50 cursor-not-allowed"
        )}
      >
        <span>
          Zones: <span className="text-blue-300">{selectedZones.length}</span>
          <span className="text-muted-foreground"> of {zones.length}</span>
        </span>
        <ChevronDown className={cn(
          "w-3.5 h-3.5 text-muted-foreground transition-transform",
          isOpen && "rotate-180"
        )} />
      </button>

      {isOpen && (
        <div className="absolute top-full left-0 mt-1 z-50 w-64 max-h-72 overflow-auto bg-surface border border-border rounded-lg shadow-xl animate-in fade-in slide-in-from-top-1 duration-150">
          {/* Quick toggles header */}
          <div className="sticky top-0 bg-surface border-b border-border p-2 flex items-center justify-between">
            <span className="text-[10px] font-medium text-muted-foreground uppercase tracking-wider">
              Select Zones
            </span>
            <div className="flex items-center gap-1">
              <button
                onClick={selectAll}
                disabled={allSelected}
                className={cn(
                  "px-2 py-0.5 text-[10px] font-medium rounded transition-colors",
                  allSelected
                    ? "text-muted-foreground/50 cursor-not-allowed"
                    : "text-blue-400 hover:bg-blue-500/10"
                )}
              >
                All
              </button>
              <span className="text-muted-foreground/30">|</span>
              <button
                onClick={selectNone}
                disabled={noneSelected}
                className={cn(
                  "px-2 py-0.5 text-[10px] font-medium rounded transition-colors",
                  noneSelected
                    ? "text-muted-foreground/50 cursor-not-allowed"
                    : "text-muted-foreground hover:bg-muted"
                )}
              >
                None
              </button>
            </div>
          </div>

          {/* Zone list */}
          <div className="p-1">
            {zones.map(zone => {
              const isSelected = selectedZones.includes(zone.id);
              return (
                <button
                  key={zone.id}
                  onClick={() => toggleZone(zone.id)}
                  className={cn(
                    "w-full flex items-center gap-2 px-2 py-1.5 text-xs rounded transition-colors",
                    isSelected
                      ? "bg-blue-500/10 text-blue-300"
                      : "text-foreground/80 hover:bg-muted"
                  )}
                >
                  <div className={cn(
                    "w-4 h-4 rounded border flex items-center justify-center transition-colors",
                    isSelected
                      ? "bg-blue-500 border-blue-500"
                      : "border-border"
                  )}>
                    {isSelected && <Check className="w-3 h-3 text-white" />}
                  </div>
                  <span className="font-medium">{zone.name}</span>
                  {!zone.isActive && (
                    <span className="ml-auto text-[9px] text-muted-foreground/50 uppercase">
                      Inactive
                    </span>
                  )}
                </button>
              );
            })}
          </div>

          {/* Footer with selected count */}
          <div className="sticky bottom-0 bg-surface border-t border-border p-2 flex items-center justify-between">
            <span className="text-[10px] text-muted-foreground">
              {selectedZones.length} zone{selectedZones.length !== 1 ? 's' : ''} selected
            </span>
            <button
              onClick={() => setIsOpen(false)}
              className="px-2 py-0.5 text-[10px] font-medium text-foreground bg-muted hover:bg-muted/80 rounded transition-colors"
            >
              Done
            </button>
          </div>
        </div>
      )}
    </div>
  );
}



