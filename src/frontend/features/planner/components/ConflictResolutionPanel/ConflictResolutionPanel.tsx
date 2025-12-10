'use client';

import React, { useState, useMemo, useCallback } from 'react';
import { cn } from '@/lib/utils';
import {
  AlertTriangle,
  ChevronDown,
  ChevronUp,
  MapPin,
  Users,
  Calendar,
  ArrowRight,
  Scissors,
  RefreshCw,
  Eye,
  Check,
  Plus,
  Minus,
  ArrowLeftRight,
  X,
} from 'lucide-react';
import { PlannerConflict, Room, PlannedBatch, PhaseType } from '../../types/planner.types';
import { format, addDays } from 'date-fns';

export interface SplitConfig {
  plantCount: number;
  suffix: string;
}

export interface ConflictResolution {
  type: 'change_room' | 'reduce_plants' | 'shift_schedule' | 'split_batch' | 'dismiss';
  conflictId: string;
  batchId: string;
  phaseId?: string;
  payload: {
    newRoomId?: string;
    newPlantCount?: number;
    daysDelta?: number;
    splitConfig?: SplitConfig[];
  };
}

interface ConflictResolutionPanelProps {
  conflicts: PlannerConflict[];
  batch: PlannedBatch;
  rooms: Room[];
  onResolve: (resolution: ConflictResolution) => void;
  onViewAffectedBatches?: (batchIds: string[]) => void;
  className?: string;
}

interface GroupedConflict {
  roomId: string;
  roomName: string;
  conflicts: PlannerConflict[];
  totalOverage: number;
  dateRange: { start: Date; end: Date };
}

export function ConflictResolutionPanel({
  conflicts,
  batch,
  rooms,
  onResolve,
  onViewAffectedBatches,
  className,
}: ConflictResolutionPanelProps) {
  const [isExpanded, setIsExpanded] = useState(false);
  const [selectedResolution, setSelectedResolution] = useState<string | null>(null);
  const [selectedRoom, setSelectedRoom] = useState<string>('');
  const [newPlantCount, setNewPlantCount] = useState<number>(batch.plantCount);
  
  // Split batch state
  const [splitParts, setSplitParts] = useState<SplitConfig[]>([
    { plantCount: Math.ceil(batch.plantCount / 2), suffix: '-A' },
    { plantCount: Math.floor(batch.plantCount / 2), suffix: '-B' },
  ]);
  
  // Shift schedule state
  const [shiftDays, setShiftDays] = useState<number>(7);

  // Group conflicts by room for better organization
  const groupedConflicts = useMemo(() => {
    const groups = new Map<string, GroupedConflict>();

    for (const conflict of conflicts) {
      if (conflict.type !== 'capacity_exceeded' || !conflict.roomId) continue;

      const roomId = conflict.roomId;
      const room = rooms.find((r) => r.id === roomId);
      if (!room) continue;

      if (!groups.has(roomId)) {
        groups.set(roomId, {
          roomId,
          roomName: room.name,
          conflicts: [],
          totalOverage: 0,
          dateRange: { start: conflict.date!, end: conflict.date! },
        });
      }

      const group = groups.get(roomId)!;
      group.conflicts.push(conflict);

      // Parse overage from message
      const overageMatch = conflict.message.match(/over capacity by (\d+)/);
      if (overageMatch) {
        group.totalOverage = Math.max(group.totalOverage, parseInt(overageMatch[1]));
      }

      // Update date range
      if (conflict.date) {
        if (conflict.date < group.dateRange.start) {
          group.dateRange.start = conflict.date;
        }
        if (conflict.date > group.dateRange.end) {
          group.dateRange.end = conflict.date;
        }
      }
    }

    return Array.from(groups.values());
  }, [conflicts, rooms]);

  // Get available rooms for phase reassignment
  const getAvailableRooms = (currentRoomId: string): Room[] => {
    const currentRoom = rooms.find((r) => r.id === currentRoomId);
    if (!currentRoom) return [];

    // Filter rooms of the same class with higher capacity
    return rooms.filter(
      (r) =>
        r.id !== currentRoomId &&
        r.roomClass === currentRoom.roomClass &&
        r.maxCapacity >= batch.plantCount
    );
  };

  const handleResolutionSelect = (resolutionType: string, roomId?: string) => {
    setSelectedResolution(resolutionType);
    if (roomId) {
      const availableRooms = getAvailableRooms(roomId);
      if (availableRooms.length > 0) {
        setSelectedRoom(availableRooms[0].id);
      }
    }
    // Reset split parts when selecting split
    if (resolutionType === 'split_batch') {
      setSplitParts([
        { plantCount: Math.ceil(batch.plantCount / 2), suffix: '-A' },
        { plantCount: Math.floor(batch.plantCount / 2), suffix: '-B' },
      ]);
    }
  };

  // Split batch helpers
  const splitTotal = useMemo(() => 
    splitParts.reduce((sum, part) => sum + part.plantCount, 0),
    [splitParts]
  );

  const addSplitPart = useCallback(() => {
    const nextLetter = String.fromCharCode(65 + splitParts.length); // A=65, B=66, etc.
    setSplitParts([...splitParts, { plantCount: 0, suffix: `-${nextLetter}` }]);
  }, [splitParts]);

  const removeSplitPart = useCallback((index: number) => {
    if (splitParts.length > 2) {
      setSplitParts(splitParts.filter((_, i) => i !== index));
    }
  }, [splitParts]);

  const updateSplitPart = useCallback((index: number, plantCount: number) => {
    setSplitParts(splitParts.map((part, i) => 
      i === index ? { ...part, plantCount: Math.max(0, plantCount) } : part
    ));
  }, [splitParts]);

  // Auto-balance split to match original count
  const autoBalanceSplit = useCallback(() => {
    const count = splitParts.length;
    const baseCount = Math.floor(batch.plantCount / count);
    const remainder = batch.plantCount % count;
    
    setSplitParts(splitParts.map((part, i) => ({
      ...part,
      plantCount: baseCount + (i < remainder ? 1 : 0),
    })));
  }, [batch.plantCount, splitParts]);

  // Get the date range after shift
  const shiftedDateRange = useMemo(() => {
    if (batch.phases.length === 0) return null;
    const firstPhase = batch.phases[0];
    const lastPhase = batch.phases[batch.phases.length - 1];
    return {
      start: addDays(firstPhase.plannedStart, shiftDays),
      end: addDays(lastPhase.plannedEnd, shiftDays),
    };
  }, [batch.phases, shiftDays]);

  const handleApplyResolution = (conflict: PlannerConflict, resolutionType: string) => {
    const phase = batch.phases.find((p) => p.roomId === conflict.roomId);

    switch (resolutionType) {
      case 'change_room':
        if (selectedRoom && phase) {
          onResolve({
            type: 'change_room',
            conflictId: conflict.id,
            batchId: batch.id,
            phaseId: phase.id,
            payload: { newRoomId: selectedRoom },
          });
        }
        break;

      case 'reduce_plants':
        if (newPlantCount < batch.plantCount) {
          onResolve({
            type: 'reduce_plants',
            conflictId: conflict.id,
            batchId: batch.id,
            payload: { newPlantCount },
          });
        }
        break;

      case 'split_batch':
        if (splitParts.length >= 2 && splitTotal === batch.plantCount) {
          onResolve({
            type: 'split_batch',
            conflictId: conflict.id,
            batchId: batch.id,
            payload: { splitConfig: splitParts },
          });
        }
        break;

      case 'shift_schedule':
        if (shiftDays !== 0) {
          onResolve({
            type: 'shift_schedule',
            conflictId: conflict.id,
            batchId: batch.id,
            payload: { daysDelta: shiftDays },
          });
        }
        break;

      case 'dismiss':
        onResolve({
          type: 'dismiss',
          conflictId: conflict.id,
          batchId: batch.id,
          payload: {},
        });
        break;
    }

    setSelectedResolution(null);
  };

  if (conflicts.length === 0) return null;

  const capacityConflicts = conflicts.filter((c) => c.type === 'capacity_exceeded');
  const otherConflicts = conflicts.filter((c) => c.type !== 'capacity_exceeded');

  return (
    <div className={cn('rounded-lg overflow-hidden', className)}>
      {/* Header */}
      <button
        onClick={() => setIsExpanded(!isExpanded)}
        className="w-full flex items-center justify-between p-3 bg-red-500/10 border border-red-500/30 hover:bg-red-500/15 transition-colors rounded-lg"
      >
        <div className="flex items-center gap-2">
          <AlertTriangle className="w-4 h-4 text-red-400" />
          <span className="text-sm font-medium text-red-400">
            {conflicts.length} {conflicts.length === 1 ? 'Conflict' : 'Conflicts'}
          </span>
        </div>
        <div className="flex items-center gap-2">
          <span className="text-xs text-red-300/70">Click to resolve</span>
          {isExpanded ? (
            <ChevronUp className="w-4 h-4 text-red-400" />
          ) : (
            <ChevronDown className="w-4 h-4 text-red-400" />
          )}
        </div>
      </button>

      {/* Expanded Content */}
      {isExpanded && (
        <div className="mt-2 space-y-3">
          {/* Capacity Conflicts */}
          {groupedConflicts.map((group) => (
            <div
              key={group.roomId}
              className="p-3 bg-red-500/5 border border-red-500/20 rounded-lg"
            >
              <div className="flex items-start justify-between mb-2">
                <div>
                  <div className="flex items-center gap-2 text-sm font-medium text-red-300">
                    <MapPin className="w-3.5 h-3.5" />
                    {group.roomName}
                  </div>
                  <p className="text-xs text-red-300/70 mt-1">
                    Over capacity by {group.totalOverage} plants
                    <span className="mx-1">•</span>
                    {group.conflicts.length} days affected
                  </p>
                  <p className="text-xs text-muted-foreground mt-0.5">
                    {format(group.dateRange.start, 'MMM d')} -{' '}
                    {format(group.dateRange.end, 'MMM d, yyyy')}
                  </p>
                </div>

                {/* Affected Batches */}
                {group.conflicts[0]?.affectedBatchIds &&
                  group.conflicts[0].affectedBatchIds.length > 1 && (
                    <button
                      onClick={() =>
                        onViewAffectedBatches?.(group.conflicts[0].affectedBatchIds!)
                      }
                      className="flex items-center gap-1 px-2 py-1 text-xs text-cyan-400 hover:text-cyan-300 bg-cyan-500/10 hover:bg-cyan-500/20 rounded transition-colors"
                    >
                      <Eye className="w-3 h-3" />
                      {group.conflicts[0].affectedBatchIds.length} batches
                    </button>
                  )}
              </div>

              {/* Resolution Options */}
              <div className="space-y-2 mt-3">
                <p className="text-xs text-muted-foreground uppercase tracking-wider">
                  Resolution Options
                </p>

                {/* Option 1: Change Room */}
                {getAvailableRooms(group.roomId).length > 0 && (
                  <div className="p-2 bg-surface/50 rounded border border-border/50">
                    <button
                      onClick={() => handleResolutionSelect('change_room', group.roomId)}
                      className={cn(
                        'w-full flex items-center gap-2 text-left text-sm',
                        selectedResolution === 'change_room'
                          ? 'text-cyan-400'
                          : 'text-foreground/80 hover:text-foreground'
                      )}
                    >
                      <ArrowRight className="w-3.5 h-3.5" />
                      Move to different room
                    </button>

                    {selectedResolution === 'change_room' && (
                      <div className="mt-2 pl-5 space-y-2">
                        <label className="sr-only" htmlFor={`room-select-${group.roomId}`}>
                          Select new room
                        </label>
                        <select
                          id={`room-select-${group.roomId}`}
                          value={selectedRoom}
                          onChange={(e) => setSelectedRoom(e.target.value)}
                          className="w-full px-2 py-1.5 text-sm bg-background border border-border rounded text-foreground"
                          title="Select a room with more capacity"
                        >
                          {getAvailableRooms(group.roomId).map((room) => (
                            <option key={room.id} value={room.id}>
                              {room.name} (capacity: {room.maxCapacity})
                            </option>
                          ))}
                        </select>
                        <button
                          onClick={() =>
                            handleApplyResolution(group.conflicts[0], 'change_room')
                          }
                          className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium text-white bg-cyan-500 hover:bg-cyan-400 rounded transition-colors"
                        >
                          <Check className="w-3 h-3" />
                          Apply Change
                        </button>
                      </div>
                    )}
                  </div>
                )}

                {/* Option 2: Reduce Plant Count */}
                <div className="p-2 bg-surface/50 rounded border border-border/50">
                  <button
                    onClick={() => handleResolutionSelect('reduce_plants', group.roomId)}
                    className={cn(
                      'w-full flex items-center gap-2 text-left text-sm',
                      selectedResolution === 'reduce_plants'
                        ? 'text-cyan-400'
                        : 'text-foreground/80 hover:text-foreground'
                    )}
                  >
                    <Users className="w-3.5 h-3.5" />
                    Reduce plant count
                  </button>

                    {selectedResolution === 'reduce_plants' && (
                      <div className="mt-2 pl-5 space-y-2">
                        <div className="flex items-center gap-2">
                          <label className="sr-only" htmlFor={`plant-count-${batch.id}`}>
                            New plant count
                          </label>
                          <input
                            id={`plant-count-${batch.id}`}
                            type="number"
                            value={newPlantCount}
                            onChange={(e) =>
                              setNewPlantCount(Math.max(1, parseInt(e.target.value) || 0))
                            }
                            max={batch.plantCount}
                            className="w-24 px-2 py-1.5 text-sm bg-background border border-border rounded text-foreground"
                            title="Enter new plant count"
                            placeholder="Plant count"
                          />
                        <span className="text-xs text-muted-foreground">
                          (current: {batch.plantCount})
                        </span>
                      </div>
                      <p className="text-xs text-amber-400/80">
                        Reducing by {batch.plantCount - newPlantCount} plants
                      </p>
                      <button
                        onClick={() =>
                          handleApplyResolution(group.conflicts[0], 'reduce_plants')
                        }
                        disabled={newPlantCount >= batch.plantCount}
                        className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium text-white bg-cyan-500 hover:bg-cyan-400 rounded transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                      >
                        <Check className="w-3 h-3" />
                        Apply Change
                      </button>
                    </div>
                  )}
                </div>

                {/* Option 3: Split Batch */}
                <div className="p-2 bg-surface/50 rounded border border-border/50">
                  <button
                    onClick={() => handleResolutionSelect('split_batch', group.roomId)}
                    className={cn(
                      'w-full flex items-center gap-2 text-left text-sm',
                      selectedResolution === 'split_batch'
                        ? 'text-cyan-400'
                        : 'text-foreground/80 hover:text-foreground'
                    )}
                  >
                    <Scissors className="w-3.5 h-3.5" />
                    Split batch into multiple
                  </button>

                  {selectedResolution === 'split_batch' && (
                    <div className="mt-3 pl-5 space-y-3">
                      <div className="space-y-2">
                        {splitParts.map((part, index) => (
                          <div key={index} className="flex items-center gap-2">
                            <span className="text-xs text-muted-foreground w-8">
                              {part.suffix}
                            </span>
                            <input
                              type="number"
                              value={part.plantCount}
                              onChange={(e) =>
                                updateSplitPart(index, parseInt(e.target.value) || 0)
                              }
                              min={0}
                              max={batch.plantCount}
                              className="w-20 px-2 py-1.5 text-sm bg-background border border-border rounded text-foreground"
                              title={`Plant count for batch ${part.suffix}`}
                            />
                            <span className="text-xs text-muted-foreground">plants</span>
                            {splitParts.length > 2 && (
                              <button
                                onClick={() => removeSplitPart(index)}
                                className="p-1 text-muted-foreground hover:text-rose-400 transition-colors"
                                title="Remove this split"
                              >
                                <X className="w-3 h-3" />
                              </button>
                            )}
                          </div>
                        ))}
                      </div>

                      <div className="flex items-center gap-2">
                        <button
                          onClick={addSplitPart}
                          className="flex items-center gap-1 px-2 py-1 text-xs text-cyan-400 hover:bg-cyan-500/10 rounded transition-colors"
                        >
                          <Plus className="w-3 h-3" />
                          Add Split
                        </button>
                        <button
                          onClick={autoBalanceSplit}
                          className="flex items-center gap-1 px-2 py-1 text-xs text-muted-foreground hover:text-foreground hover:bg-muted/50 rounded transition-colors"
                        >
                          <ArrowLeftRight className="w-3 h-3" />
                          Auto-balance
                        </button>
                      </div>

                      <div className={cn(
                        'text-xs',
                        splitTotal === batch.plantCount
                          ? 'text-emerald-400'
                          : 'text-amber-400'
                      )}>
                        Total: {splitTotal} / {batch.plantCount} plants
                        {splitTotal !== batch.plantCount && (
                          <span className="ml-1">
                            ({splitTotal > batch.plantCount ? '+' : ''}{splitTotal - batch.plantCount})
                          </span>
                        )}
                      </div>

                      <button
                        onClick={() =>
                          handleApplyResolution(group.conflicts[0], 'split_batch')
                        }
                        disabled={splitTotal !== batch.plantCount || splitParts.length < 2}
                        className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium text-white bg-cyan-500 hover:bg-cyan-400 rounded transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                      >
                        <Check className="w-3 h-3" />
                        Split Batch
                      </button>
                    </div>
                  )}
                </div>

                {/* Option 4: Shift Schedule */}
                <div className="p-2 bg-surface/50 rounded border border-border/50">
                  <button
                    onClick={() => handleResolutionSelect('shift_schedule', group.roomId)}
                    className={cn(
                      'w-full flex items-center gap-2 text-left text-sm',
                      selectedResolution === 'shift_schedule'
                        ? 'text-cyan-400'
                        : 'text-foreground/80 hover:text-foreground'
                    )}
                  >
                    <Calendar className="w-3.5 h-3.5" />
                    Shift to different dates
                  </button>

                  {selectedResolution === 'shift_schedule' && (
                    <div className="mt-3 pl-5 space-y-3">
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => setShiftDays(Math.max(-365, shiftDays - 7))}
                          className="p-1.5 text-muted-foreground hover:text-foreground hover:bg-muted/50 rounded transition-colors"
                          title="Shift earlier"
                        >
                          <Minus className="w-4 h-4" />
                        </button>
                        <input
                          type="number"
                          value={shiftDays}
                          onChange={(e) => setShiftDays(parseInt(e.target.value) || 0)}
                          className="w-20 px-2 py-1.5 text-sm bg-background border border-border rounded text-foreground text-center"
                          title="Days to shift (negative = earlier)"
                        />
                        <button
                          onClick={() => setShiftDays(Math.min(365, shiftDays + 7))}
                          className="p-1.5 text-muted-foreground hover:text-foreground hover:bg-muted/50 rounded transition-colors"
                          title="Shift later"
                        >
                          <Plus className="w-4 h-4" />
                        </button>
                        <span className="text-xs text-muted-foreground">days</span>
                      </div>

                      <div className="flex items-center gap-2 text-xs">
                        <button
                          onClick={() => setShiftDays(-7)}
                          className={cn(
                            'px-2 py-1 rounded transition-colors',
                            shiftDays === -7
                              ? 'bg-cyan-500/20 text-cyan-400'
                              : 'text-muted-foreground hover:bg-muted/50'
                          )}
                        >
                          -1 week
                        </button>
                        <button
                          onClick={() => setShiftDays(7)}
                          className={cn(
                            'px-2 py-1 rounded transition-colors',
                            shiftDays === 7
                              ? 'bg-cyan-500/20 text-cyan-400'
                              : 'text-muted-foreground hover:bg-muted/50'
                          )}
                        >
                          +1 week
                        </button>
                        <button
                          onClick={() => setShiftDays(14)}
                          className={cn(
                            'px-2 py-1 rounded transition-colors',
                            shiftDays === 14
                              ? 'bg-cyan-500/20 text-cyan-400'
                              : 'text-muted-foreground hover:bg-muted/50'
                          )}
                        >
                          +2 weeks
                        </button>
                        <button
                          onClick={() => setShiftDays(30)}
                          className={cn(
                            'px-2 py-1 rounded transition-colors',
                            shiftDays === 30
                              ? 'bg-cyan-500/20 text-cyan-400'
                              : 'text-muted-foreground hover:bg-muted/50'
                          )}
                        >
                          +1 month
                        </button>
                      </div>

                      {shiftedDateRange && (
                        <div className="text-xs text-muted-foreground">
                          <span className="text-foreground/70">New schedule: </span>
                          {format(shiftedDateRange.start, 'MMM d')} → {format(shiftedDateRange.end, 'MMM d, yyyy')}
                        </div>
                      )}

                      <p className={cn(
                        'text-xs',
                        shiftDays > 0 ? 'text-cyan-400/80' : shiftDays < 0 ? 'text-amber-400/80' : 'text-muted-foreground'
                      )}>
                        {shiftDays > 0 
                          ? `Moving batch ${shiftDays} days later`
                          : shiftDays < 0
                            ? `Moving batch ${Math.abs(shiftDays)} days earlier`
                            : 'No change'}
                      </p>

                      <button
                        onClick={() =>
                          handleApplyResolution(group.conflicts[0], 'shift_schedule')
                        }
                        disabled={shiftDays === 0}
                        className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium text-white bg-cyan-500 hover:bg-cyan-400 rounded transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                      >
                        <Check className="w-3 h-3" />
                        Shift Schedule
                      </button>
                    </div>
                  )}
                </div>

                {/* Dismiss Option */}
                <button
                  onClick={() => handleApplyResolution(group.conflicts[0], 'dismiss')}
                  className="w-full flex items-center justify-center gap-1.5 px-3 py-1.5 text-xs text-muted-foreground hover:text-foreground/70 hover:bg-muted/50 rounded transition-colors"
                >
                  <RefreshCw className="w-3 h-3" />
                  Dismiss warning (acknowledge risk)
                </button>
              </div>
            </div>
          ))}

          {/* Other Conflicts (non-capacity) */}
          {otherConflicts.length > 0 && (
            <div className="space-y-2">
              <p className="text-xs text-muted-foreground uppercase tracking-wider px-1">
                Other Issues
              </p>
              {otherConflicts.slice(0, 3).map((conflict) => (
                <div
                  key={conflict.id}
                  className="p-2 bg-amber-500/5 border border-amber-500/20 rounded-lg"
                >
                  <p className="text-xs text-amber-300/80">{conflict.message}</p>
                  <button
                    onClick={() => handleApplyResolution(conflict, 'dismiss')}
                    className="mt-2 text-xs text-muted-foreground hover:text-foreground/70 underline"
                  >
                    Dismiss
                  </button>
                </div>
              ))}
              {otherConflicts.length > 3 && (
                <p className="text-xs text-muted-foreground px-1">
                  +{otherConflicts.length - 3} more issues
                </p>
              )}
            </div>
          )}
        </div>
      )}

      {/* Summary when collapsed */}
      {!isExpanded && conflicts.length > 0 && (
        <div className="mt-1 space-y-0.5 px-1">
          {conflicts.slice(0, 3).map((conflict) => (
            <p key={conflict.id} className="text-xs text-red-300/70 truncate">
              {conflict.message}
            </p>
          ))}
          {conflicts.length > 3 && (
            <p className="text-xs text-red-400/80">+{conflicts.length - 3} more</p>
          )}
        </div>
      )}
    </div>
  );
}

