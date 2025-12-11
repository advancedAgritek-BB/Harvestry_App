'use client';

import React, { useState, useCallback } from 'react';
import { cn } from '@/lib/utils';
import {
  Leaf,
  Home,
  ChevronDown,
  Calendar,
  ChevronRight,
  Info,
} from 'lucide-react';
import { addDays, format } from 'date-fns';
import { PHASE_CONFIGS } from '../../constants/phaseConfig';
import type { Room, PhaseType, RoomClass } from '../../types/planner.types';

// =============================================================================
// TYPES
// =============================================================================

export interface PhaseFormConfig {
  phase: PhaseType;
  roomId: string;
  duration: number;
}

export interface CustomConfigTabProps {
  startDate: Date;
  rooms: Room[];
  phaseConfigs: Record<PhaseType, PhaseFormConfig>;
  onPhaseConfigChange: (phase: PhaseType, updates: Partial<PhaseFormConfig>) => void;
}

// =============================================================================
// CONSTANTS
// =============================================================================

const PHASE_ROOM_CLASS_MAP: Record<PhaseType, RoomClass[]> = {
  clone: ['propagation', 'veg'],
  veg: ['veg'],
  flower: ['flower'],
  harvest: ['drying', 'processing'],
  cure: ['cure'],
};

const PHASE_ORDER: PhaseType[] = ['clone', 'veg', 'flower', 'harvest', 'cure'];

// =============================================================================
// ROOM SELECTOR COMPONENT
// =============================================================================

interface RoomSelectorProps {
  rooms: Room[];
  selectedId: string;
  onSelect: (roomId: string) => void;
  phase: PhaseType;
  disabled?: boolean;
}

function RoomSelector({
  rooms,
  selectedId,
  onSelect,
  phase,
  disabled,
}: RoomSelectorProps) {
  const [isOpen, setIsOpen] = useState(false);

  // Filter rooms by phase-appropriate room class
  const allowedRoomClasses = PHASE_ROOM_CLASS_MAP[phase];
  const filteredRooms = rooms.filter((r) => allowedRoomClasses.includes(r.roomClass));
  const selected = rooms.find((r) => r.id === selectedId);

  return (
    <div className="relative">
      <button
        type="button"
        onClick={() => !disabled && setIsOpen(!isOpen)}
        disabled={disabled}
        className={cn(
          'w-full flex items-center justify-between px-3 py-2 bg-muted/30 border rounded-lg text-sm transition-colors',
          disabled ? 'opacity-50 cursor-not-allowed' : 'hover:border-cyan-500/30',
          isOpen ? 'border-cyan-500/50' : 'border-border'
        )}
      >
        <div className="flex items-center gap-2">
          <Home className="w-4 h-4 text-muted-foreground" />
          <span className={selected ? 'text-foreground' : 'text-muted-foreground'}>
            {selected ? `${selected.code} - ${selected.name}` : 'Select room...'}
          </span>
        </div>
        <ChevronDown
          className={cn('w-4 h-4 transition-transform', isOpen && 'rotate-180')}
        />
      </button>

      {isOpen && (
        <>
          <div className="fixed inset-0 z-40" onClick={() => setIsOpen(false)} />
          <div className="absolute top-full left-0 right-0 mt-1 bg-surface border border-border rounded-lg shadow-xl z-50 max-h-48 overflow-y-auto">
            {filteredRooms.length === 0 ? (
              <div className="px-4 py-3 text-sm text-muted-foreground">
                No rooms available for this phase
              </div>
            ) : (
              filteredRooms.map((room) => (
                <button
                  key={room.id}
                  type="button"
                  onClick={() => {
                    onSelect(room.id);
                    setIsOpen(false);
                  }}
                  className={cn(
                    'w-full flex items-center justify-between px-3 py-2.5 text-left transition-colors',
                    room.id === selectedId ? 'bg-cyan-500/10' : 'hover:bg-muted/50'
                  )}
                >
                  <div className="flex items-center gap-2">
                    <span className="text-xs text-muted-foreground font-mono">
                      {room.code}
                    </span>
                    <span className="text-sm text-foreground">{room.name}</span>
                  </div>
                  <span className="text-xs text-muted-foreground">
                    Cap: {room.maxCapacity}
                  </span>
                </button>
              ))
            )}
          </div>
        </>
      )}
    </div>
  );
}

// =============================================================================
// PHASE CONFIG ROW COMPONENT
// =============================================================================

interface PhaseConfigRowProps {
  phase: PhaseType;
  config: PhaseFormConfig;
  rooms: Room[];
  onConfigChange: (updates: Partial<PhaseFormConfig>) => void;
  startDate: Date;
  previousEndDate: Date | null;
}

function PhaseConfigRow({
  phase,
  config,
  rooms,
  onConfigChange,
  startDate,
  previousEndDate,
}: PhaseConfigRowProps) {
  const phaseInfo = PHASE_CONFIGS[phase];
  const phaseStart = previousEndDate ? addDays(previousEndDate, 1) : startDate;
  const phaseEnd = addDays(phaseStart, config.duration - 1);

  return (
    <div className="flex items-center gap-3 p-3 bg-muted/20 rounded-lg border border-border/50">
      {/* Phase indicator */}
      <div
        className="w-10 h-10 rounded-lg flex items-center justify-center shrink-0"
        style={{ backgroundColor: `${phaseInfo.color}20` }}
      >
        <Leaf className="w-5 h-5" style={{ color: phaseInfo.color }} />
      </div>

      {/* Phase name and dates */}
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2">
          <span className="font-medium text-foreground">{phaseInfo.label}</span>
          <span className="text-xs text-muted-foreground">
            {format(phaseStart, 'MMM d')} → {format(phaseEnd, 'MMM d')}
          </span>
        </div>

        {/* Room selector */}
        <div className="mt-2">
          <RoomSelector
            rooms={rooms}
            selectedId={config.roomId}
            onSelect={(roomId) => onConfigChange({ roomId })}
            phase={phase}
          />
        </div>
      </div>

      {/* Duration input */}
      <div className="shrink-0">
        <label className="text-xs text-muted-foreground block mb-1">Days</label>
        <input
          type="number"
          value={config.duration}
          onChange={(e) =>
            onConfigChange({ duration: parseInt(e.target.value) || 1 })
          }
          min={1}
          max={365}
          className="w-16 px-2 py-1.5 bg-muted/30 border border-border rounded text-sm text-center focus:outline-none focus:border-cyan-500/50"
        />
      </div>
    </div>
  );
}

// =============================================================================
// MAIN COMPONENT
// =============================================================================

export function CustomConfigTab({
  startDate,
  rooms,
  phaseConfigs,
  onPhaseConfigChange,
}: CustomConfigTabProps) {
  // Calculate total duration
  const totalDays = Object.values(phaseConfigs).reduce(
    (sum, c) => sum + c.duration,
    0
  );
  const estimatedEndDate = addDays(startDate, totalDays - 1);

  // Calculate cumulative end dates for each phase
  const getPhaseEndDate = useCallback(
    (phase: PhaseType): Date | null => {
      const phaseIndex = PHASE_ORDER.indexOf(phase);
      if (phaseIndex <= 0) return null;

      let date = startDate;
      for (let i = 0; i < phaseIndex; i++) {
        date = addDays(date, phaseConfigs[PHASE_ORDER[i]].duration);
      }
      return addDays(date, -1);
    },
    [startDate, phaseConfigs]
  );

  return (
    <div className="space-y-4">
      {/* Header with totals */}
      <div className="flex items-center justify-between p-3 bg-surface border border-border rounded-lg">
        <div className="flex items-center gap-2 text-sm">
          <Calendar className="w-4 h-4 text-cyan-400" />
          <span className="text-foreground font-medium">{totalDays} days total</span>
        </div>
        <div className="flex items-center gap-2 text-sm text-muted-foreground">
          <span>Ends</span>
          <ChevronRight className="w-3 h-3" />
          <span className="text-foreground font-medium">
            {format(estimatedEndDate, 'MMM d, yyyy')}
          </span>
        </div>
      </div>

      {/* Phase configurations */}
      <div className="space-y-2">
        {PHASE_ORDER.map((phase) => (
          <PhaseConfigRow
            key={phase}
            phase={phase}
            config={phaseConfigs[phase]}
            rooms={rooms}
            onConfigChange={(updates) => onPhaseConfigChange(phase, updates)}
            startDate={startDate}
            previousEndDate={getPhaseEndDate(phase)}
          />
        ))}
      </div>

      {/* Info note */}
      <div className="flex items-start gap-2 p-3 bg-violet-500/10 border border-violet-500/20 rounded-lg">
        <Info className="w-4 h-4 text-violet-400 shrink-0 mt-0.5" />
        <div className="text-xs text-violet-200">
          <strong>Tip:</strong> Phase durations are pre-populated based on the selected
          genetics. Adjust as needed for your specific grow conditions and strain
          characteristics.
        </div>
      </div>
    </div>
  );
}

export default CustomConfigTab;




