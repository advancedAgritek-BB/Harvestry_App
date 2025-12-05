/**
 * Capacity Utilities
 * 
 * Functions for calculating room capacity and occupancy
 */

import { PlannedBatch, Room, RoomCapacity, DailyCapacity, DateRange } from '../types/planner.types';
import { getDaysInRange, isDateInRange, doRangesOverlap } from './dateUtils';
import { CAPACITY_THRESHOLDS } from '../constants/phaseConfig';

/**
 * Calculate daily occupancy for a room based on batches
 */
export function calculateRoomOccupancy(
  room: Room,
  batches: PlannedBatch[],
  dateRange: DateRange
): DailyCapacity[] {
  const days = getDaysInRange(dateRange);
  const dailyCapacities: DailyCapacity[] = [];

  for (const day of days) {
    const batchesInRoom: string[] = [];
    let plantCount = 0;

    for (const batch of batches) {
      for (const phase of batch.phases) {
        if (phase.roomId !== room.id) continue;
        
        const phaseRange = {
          start: phase.plannedStart,
          end: phase.plannedEnd,
        };
        
        if (isDateInRange(day, phaseRange)) {
          batchesInRoom.push(batch.id);
          plantCount += batch.plantCount;
        }
      }
    }

    dailyCapacities.push({
      date: day,
      roomId: room.id,
      plantCount,
      batchIds: Array.from(new Set(batchesInRoom)),
    });
  }

  return dailyCapacities;
}

/**
 * Calculate capacity for all rooms
 */
export function calculateAllRoomCapacities(
  rooms: Room[],
  batches: PlannedBatch[],
  dateRange: DateRange
): RoomCapacity[] {
  return rooms.map((room) => ({
    roomId: room.id,
    roomName: room.name,
    roomClass: room.roomClass,
    maxCapacity: room.maxCapacity,
    dailyOccupancy: calculateRoomOccupancy(room, batches, dateRange),
  }));
}

/**
 * Get capacity utilization percentage for a specific day
 */
export function getCapacityPercentage(
  roomCapacity: RoomCapacity,
  date: Date
): number {
  const dayCapacity = roomCapacity.dailyOccupancy.find(
    (d) => d.date.toDateString() === date.toDateString()
  );
  
  if (!dayCapacity || roomCapacity.maxCapacity === 0) {
    return 0;
  }
  
  return dayCapacity.plantCount / roomCapacity.maxCapacity;
}

/**
 * Get capacity status color based on utilization
 */
export function getCapacityColor(percentage: number): string {
  if (percentage >= CAPACITY_THRESHOLDS.high) {
    return '#ef4444'; // red-500
  }
  if (percentage >= CAPACITY_THRESHOLDS.medium) {
    return '#f59e0b'; // amber-500
  }
  if (percentage >= CAPACITY_THRESHOLDS.low) {
    return '#eab308'; // yellow-500
  }
  return '#22c55e'; // green-500
}

/**
 * Get capacity status label
 */
export function getCapacityStatus(percentage: number): 'critical' | 'warning' | 'normal' | 'low' {
  if (percentage >= CAPACITY_THRESHOLDS.high) return 'critical';
  if (percentage >= CAPACITY_THRESHOLDS.medium) return 'warning';
  if (percentage >= CAPACITY_THRESHOLDS.low) return 'normal';
  return 'low';
}

/**
 * Find days where capacity is exceeded
 */
export function findOverCapacityDays(
  roomCapacity: RoomCapacity
): { date: Date; plantCount: number; overage: number }[] {
  return roomCapacity.dailyOccupancy
    .filter((day) => day.plantCount > roomCapacity.maxCapacity)
    .map((day) => ({
      date: day.date,
      plantCount: day.plantCount,
      overage: day.plantCount - roomCapacity.maxCapacity,
    }));
}

/**
 * Check if moving a batch would cause capacity issues
 */
export function checkCapacityImpact(
  batch: PlannedBatch,
  newPhaseStart: Date,
  phaseId: string,
  roomCapacity: RoomCapacity
): { hasConflict: boolean; conflictDays: Date[] } {
  const phase = batch.phases.find((p) => p.id === phaseId);
  if (!phase || phase.roomId !== roomCapacity.roomId) {
    return { hasConflict: false, conflictDays: [] };
  }

  const duration = phase.plannedEnd.getTime() - phase.plannedStart.getTime();
  const newPhaseEnd = new Date(newPhaseStart.getTime() + duration);
  
  const conflictDays: Date[] = [];
  
  for (const dayCapacity of roomCapacity.dailyOccupancy) {
    if (!isDateInRange(dayCapacity.date, { start: newPhaseStart, end: newPhaseEnd })) {
      continue;
    }
    
    // Calculate new capacity (existing - this batch + this batch at new position)
    const existingBatchHere = dayCapacity.batchIds.includes(batch.id);
    const newPlantCount = existingBatchHere
      ? dayCapacity.plantCount
      : dayCapacity.plantCount + batch.plantCount;
    
    if (newPlantCount > roomCapacity.maxCapacity) {
      conflictDays.push(dayCapacity.date);
    }
  }

  return {
    hasConflict: conflictDays.length > 0,
    conflictDays,
  };
}

/**
 * Get the next available slot in a room for a given plant count
 */
export function findNextAvailableSlot(
  roomCapacity: RoomCapacity,
  plantCount: number,
  minDuration: number,
  startAfter: Date
): { start: Date; end: Date } | null {
  const sortedDays = roomCapacity.dailyOccupancy
    .filter((d) => d.date >= startAfter)
    .sort((a, b) => a.date.getTime() - b.date.getTime());

  let consecutiveAvailable = 0;
  let slotStart: Date | null = null;

  for (const day of sortedDays) {
    const availableCapacity = roomCapacity.maxCapacity - day.plantCount;
    
    if (availableCapacity >= plantCount) {
      if (consecutiveAvailable === 0) {
        slotStart = day.date;
      }
      consecutiveAvailable++;
      
      if (consecutiveAvailable >= minDuration && slotStart) {
        return {
          start: slotStart,
          end: day.date,
        };
      }
    } else {
      consecutiveAvailable = 0;
      slotStart = null;
    }
  }

  return null;
}

/**
 * Calculate capacity data for visualization
 */
export function getCapacityVisualizationData(
  roomCapacity: RoomCapacity
): { date: Date; percentage: number; color: string; status: string }[] {
  return roomCapacity.dailyOccupancy.map((day) => {
    const percentage = roomCapacity.maxCapacity > 0
      ? day.plantCount / roomCapacity.maxCapacity
      : 0;
    
    return {
      date: day.date,
      percentage: Math.min(percentage, 1.5), // Cap at 150% for visualization
      color: getCapacityColor(percentage),
      status: getCapacityStatus(percentage),
    };
  });
}

