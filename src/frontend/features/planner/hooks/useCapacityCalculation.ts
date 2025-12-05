/**
 * Capacity Calculation Hook
 * 
 * Manages room capacity calculations and conflict detection
 */

import { useMemo, useCallback } from 'react';
import { usePlannerStore } from '../stores/plannerStore';
import { RoomCapacity, DailyCapacity, PlannedBatch, Room, DateRange } from '../types/planner.types';
import { 
  calculateAllRoomCapacities, 
  calculateRoomOccupancy,
  findOverCapacityDays,
  getCapacityPercentage,
  findNextAvailableSlot 
} from '../utils/capacityUtils';

export function useCapacityCalculation() {
  const { batches, rooms, dateRange } = usePlannerStore();

  /**
   * Calculate capacity for all rooms
   */
  const roomCapacities = useMemo((): RoomCapacity[] => {
    if (rooms.length === 0 || batches.length === 0) {
      return rooms.map((room) => ({
        roomId: room.id,
        roomName: room.name,
        roomClass: room.roomClass,
        maxCapacity: room.maxCapacity,
        dailyOccupancy: [],
      }));
    }

    return calculateAllRoomCapacities(rooms, batches, dateRange);
  }, [rooms, batches, dateRange]);

  /**
   * Get capacity for a specific room
   */
  const getRoomCapacity = useCallback((roomId: string): RoomCapacity | undefined => {
    return roomCapacities.find((rc) => rc.roomId === roomId);
  }, [roomCapacities]);

  /**
   * Get all over-capacity days across all rooms
   */
  const overCapacityDays = useMemo(() => {
    const allOverCapacity: { 
      roomId: string; 
      roomName: string;
      date: Date; 
      plantCount: number; 
      overage: number;
    }[] = [];

    for (const rc of roomCapacities) {
      const overDays = findOverCapacityDays(rc);
      for (const day of overDays) {
        allOverCapacity.push({
          roomId: rc.roomId,
          roomName: rc.roomName,
          ...day,
        });
      }
    }

    return allOverCapacity.sort((a, b) => a.date.getTime() - b.date.getTime());
  }, [roomCapacities]);

  /**
   * Check if any room is over capacity
   */
  const hasCapacityIssues = useMemo(() => {
    return overCapacityDays.length > 0;
  }, [overCapacityDays]);

  /**
   * Get utilization summary for a room
   */
  const getRoomUtilizationSummary = useCallback((roomId: string) => {
    const capacity = getRoomCapacity(roomId);
    if (!capacity || capacity.dailyOccupancy.length === 0) {
      return { avg: 0, max: 0, min: 0 };
    }

    const percentages = capacity.dailyOccupancy.map(
      (d) => d.plantCount / capacity.maxCapacity
    );

    return {
      avg: percentages.reduce((a, b) => a + b, 0) / percentages.length,
      max: Math.max(...percentages),
      min: Math.min(...percentages),
    };
  }, [getRoomCapacity]);

  /**
   * Find next available slot in a room
   */
  const findAvailableSlot = useCallback((
    roomId: string,
    plantCount: number,
    minDuration: number,
    startAfter: Date
  ) => {
    const capacity = getRoomCapacity(roomId);
    if (!capacity) return null;

    return findNextAvailableSlot(capacity, plantCount, minDuration, startAfter);
  }, [getRoomCapacity]);

  /**
   * Get rooms with capacity issues
   */
  const roomsWithIssues = useMemo(() => {
    const issueRoomIds = new Set(overCapacityDays.map((d) => d.roomId));
    return rooms.filter((r) => issueRoomIds.has(r.id));
  }, [rooms, overCapacityDays]);

  /**
   * Calculate total utilization across all rooms
   */
  const totalUtilization = useMemo(() => {
    if (roomCapacities.length === 0) return 0;

    let totalCapacity = 0;
    let totalUsed = 0;

    for (const rc of roomCapacities) {
      if (rc.dailyOccupancy.length === 0) continue;
      
      // Use average daily occupancy
      const avgOccupancy = rc.dailyOccupancy.reduce(
        (sum, d) => sum + d.plantCount, 0
      ) / rc.dailyOccupancy.length;

      totalCapacity += rc.maxCapacity;
      totalUsed += avgOccupancy;
    }

    return totalCapacity > 0 ? totalUsed / totalCapacity : 0;
  }, [roomCapacities]);

  return {
    roomCapacities,
    getRoomCapacity,
    overCapacityDays,
    hasCapacityIssues,
    getRoomUtilizationSummary,
    findAvailableSlot,
    roomsWithIssues,
    totalUtilization,
  };
}

