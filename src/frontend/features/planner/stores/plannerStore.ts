/**
 * Planner Store
 * 
 * Zustand store for Gantt chart planner state management
 */

import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import {
  PlannedBatch,
  BatchPhase,
  Room,
  RoomCapacity,
  PlannerConflict,
  ChangeImpact,
  DragState,
  DateRange,
  PlannerFilters,
  PlannerSettings,
  ZoomLevel,
  BatchStatus,
} from '../types/planner.types';
import { addDays, startOfMonth, endOfMonth } from 'date-fns';

interface PlannerState {
  // Data
  batches: PlannedBatch[];
  rooms: Room[];
  roomCapacities: RoomCapacity[];
  conflicts: PlannerConflict[];
  
  // Viewport
  dateRange: DateRange;
  selectedBatchId: string | null;
  selectedPhaseId: string | null;
  
  // Drag state
  dragState: DragState;
  pendingImpacts: ChangeImpact[];
  
  // Filters
  filters: PlannerFilters;
  
  // Settings
  settings: PlannerSettings;
  
  // Actions - Batches
  addBatch: (batch: PlannedBatch) => void;
  updateBatch: (id: string, updates: Partial<PlannedBatch>) => void;
  deleteBatch: (id: string) => void;
  duplicateBatch: (id: string) => void;
  
  // Actions - Phases
  updatePhase: (batchId: string, phaseId: string, updates: Partial<BatchPhase>) => void;
  movePhase: (batchId: string, phaseId: string, newStart: Date) => void;
  resizePhase: (batchId: string, phaseId: string, newStart: Date, newEnd: Date) => void;
  
  // Actions - Selection
  selectBatch: (batchId: string | null) => void;
  selectPhase: (batchId: string | null, phaseId: string | null) => void;
  
  // Actions - Viewport
  setDateRange: (range: DateRange) => void;
  setZoomLevel: (level: ZoomLevel) => void;
  navigateToToday: () => void;
  navigateBy: (days: number) => void;
  
  // Actions - Drag
  startDrag: (batchId: string, phaseId: string, originalStart: Date, dragType: 'move' | 'resize-start' | 'resize-end') => void;
  updateDrag: (currentStart: Date) => void;
  endDrag: (commit: boolean) => void;
  
  // Actions - Filters
  setFilters: (filters: Partial<PlannerFilters>) => void;
  clearFilters: () => void;
  
  // Actions - Settings
  updateSettings: (settings: Partial<PlannerSettings>) => void;
  toggleWhatIfMode: () => void;
  
  // Actions - Conflicts
  recalculateConflicts: () => void;
  dismissConflict: (conflictId: string) => void;
  
  // Actions - Conflict Resolution
  changePhaseRoom: (batchId: string, phaseId: string, newRoomId: string) => void;
  updateBatchPlantCount: (batchId: string, newPlantCount: number) => void;
  splitBatch: (batchId: string, splitConfig: { plantCount: number; suffix: string }[]) => string[];
  shiftBatchSchedule: (batchId: string, daysDelta: number) => void;
  
  // Actions - Data Loading
  setRooms: (rooms: Room[]) => void;
  setBatches: (batches: PlannedBatch[]) => void;
}

// Default date range - viewport starting from today (fills widescreen)
const getDefaultDateRange = (): DateRange => {
  const today = new Date();
  return {
    start: addDays(today, -30), // Start 1 month in the past
    end: addDays(today, 335), // Show ~1 year ahead to fill widescreen
  };
};

// Default filters
const defaultFilters: PlannerFilters = {
  strains: [],
  geneticsIds: [],
  roomIds: [],
  statuses: [],
  searchQuery: '',
};

// Default settings
const defaultSettings: PlannerSettings = {
  zoomLevel: 'month', // Start with month view for better overview
  showCapacityLanes: true,
  showConflicts: true,
  showActualDates: true,
  snapToDay: true,
  whatIfMode: false,
};

// Default drag state
const defaultDragState: DragState = {
  isDragging: false,
  batchId: null,
  phaseId: null,
  originalStart: null,
  currentStart: null,
  dragType: null,
};

export const usePlannerStore = create<PlannerState>()(
  persist(
    (set, get) => ({
      // Initial state
      batches: [],
      rooms: [],
      roomCapacities: [],
      conflicts: [],
      dateRange: getDefaultDateRange(),
      selectedBatchId: null,
      selectedPhaseId: null,
      dragState: defaultDragState,
      pendingImpacts: [],
      filters: defaultFilters,
      settings: defaultSettings,

      // Batch actions
      addBatch: (batch) => set((state) => ({
        batches: [...state.batches, batch],
      })),

      updateBatch: (id, updates) => set((state) => ({
        batches: state.batches.map((b) =>
          b.id === id ? { ...b, ...updates, updatedAt: new Date() } : b
        ),
      })),

      deleteBatch: (id) => set((state) => ({
        batches: state.batches.filter((b) => b.id !== id),
        selectedBatchId: state.selectedBatchId === id ? null : state.selectedBatchId,
      })),

      duplicateBatch: (id) => {
        const state = get();
        const batch = state.batches.find((b) => b.id === id);
        if (!batch) return;

        const newBatch: PlannedBatch = {
          ...batch,
          id: `batch-${Date.now()}`,
          name: `${batch.name} (Copy)`,
          code: `${batch.code}-COPY`,
          status: 'planned',
          createdAt: new Date(),
          updatedAt: new Date(),
          phases: batch.phases.map((p) => ({
            ...p,
            id: `phase-${Date.now()}-${p.phase}`,
            actualStart: undefined,
            actualEnd: undefined,
          })),
        };

        set((state) => ({
          batches: [...state.batches, newBatch],
        }));
      },

      // Phase actions
      updatePhase: (batchId, phaseId, updates) => set((state) => ({
        batches: state.batches.map((b) =>
          b.id === batchId
            ? {
                ...b,
                updatedAt: new Date(),
                phases: b.phases.map((p) =>
                  p.id === phaseId ? { ...p, ...updates } : p
                ),
              }
            : b
        ),
      })),

      movePhase: (batchId, phaseId, newStart) => {
        const state = get();
        const batch = state.batches.find((b) => b.id === batchId);
        if (!batch) return;

        const phase = batch.phases.find((p) => p.id === phaseId);
        if (!phase) return;

        const duration = phase.plannedEnd.getTime() - phase.plannedStart.getTime();
        const newEnd = new Date(newStart.getTime() + duration);

        set((s) => ({
          batches: s.batches.map((b) =>
            b.id === batchId
              ? {
                  ...b,
                  updatedAt: new Date(),
                  phases: b.phases.map((p) =>
                    p.id === phaseId
                      ? { ...p, plannedStart: newStart, plannedEnd: newEnd }
                      : p
                  ),
                }
              : b
          ),
        }));
      },

      resizePhase: (batchId, phaseId, newStart, newEnd) => set((state) => ({
        batches: state.batches.map((b) =>
          b.id === batchId
            ? {
                ...b,
                updatedAt: new Date(),
                phases: b.phases.map((p) =>
                  p.id === phaseId
                    ? { ...p, plannedStart: newStart, plannedEnd: newEnd }
                    : p
                ),
              }
            : b
        ),
      })),

      // Selection actions
      selectBatch: (batchId) => set({
        selectedBatchId: batchId,
        selectedPhaseId: null,
      }),

      selectPhase: (batchId, phaseId) => set({
        selectedBatchId: batchId,
        selectedPhaseId: phaseId,
      }),

      // Viewport actions
      setDateRange: (range) => set({ dateRange: range }),

      setZoomLevel: (level) => {
        const { dateRange } = get();
        // Larger viewport sizes for widescreen monitors
        // Day: ~3 months, Week: ~9 months, Month: ~18 months
        const viewportDays = level === 'day' ? 90 : level === 'week' ? 270 : 545;
        
        set((state) => ({
          settings: { ...state.settings, zoomLevel: level },
          dateRange: {
            start: dateRange.start,
            end: addDays(dateRange.start, viewportDays),
          },
        }));
      },

      navigateToToday: () => {
        const today = new Date();
        const { settings } = get();
        // Viewport size based on zoom level - larger for widescreen
        const viewportDays = settings.zoomLevel === 'day' ? 90 : settings.zoomLevel === 'week' ? 270 : 545;
        
        set({
          dateRange: {
            start: addDays(today, -14), // Start 2 weeks before today
            end: addDays(today, viewportDays),
          },
        });
      },

      navigateBy: (days) => {
        const { dateRange } = get();
        const minDate = addDays(new Date(), -365); // Can go 1 year in past
        const maxDate = addDays(new Date(), 365 * 3); // Can go 3 years in future
        
        const newStart = addDays(dateRange.start, days);
        const newEnd = addDays(dateRange.end, days);
        
        // Clamp to valid range
        if (newStart < minDate || newEnd > maxDate) {
          return; // Don't navigate beyond bounds
        }
        
        set({
          dateRange: {
            start: newStart,
            end: newEnd,
          },
        });
      },

      // Drag actions
      startDrag: (batchId, phaseId, originalStart, dragType) => set({
        dragState: {
          isDragging: true,
          batchId,
          phaseId,
          originalStart,
          currentStart: originalStart,
          dragType,
        },
      }),

      updateDrag: (currentStart) => set((state) => ({
        dragState: { ...state.dragState, currentStart },
      })),

      endDrag: (commit) => {
        const state = get();
        const { dragState } = state;

        if (commit && dragState.batchId && dragState.phaseId && dragState.currentStart) {
          state.movePhase(dragState.batchId, dragState.phaseId, dragState.currentStart);
        }

        set({
          dragState: defaultDragState,
          pendingImpacts: [],
        });
      },

      // Filter actions
      setFilters: (filters) => set((state) => ({
        filters: { ...state.filters, ...filters },
      })),

      clearFilters: () => set({ filters: defaultFilters }),

      // Settings actions
      updateSettings: (settings) => set((state) => ({
        settings: { ...state.settings, ...settings },
      })),

      toggleWhatIfMode: () => set((state) => ({
        settings: { ...state.settings, whatIfMode: !state.settings.whatIfMode },
      })),

      // Conflict actions
      recalculateConflicts: () => {
        // Conflict detection will be implemented in utils
        // For now, just clear conflicts
        set({ conflicts: [] });
      },

      dismissConflict: (conflictId) => set((state) => ({
        conflicts: state.conflicts.filter((c) => c.id !== conflictId),
      })),

      // Conflict resolution actions
      changePhaseRoom: (batchId, phaseId, newRoomId) => {
        const state = get();
        const room = state.rooms.find((r) => r.id === newRoomId);
        if (!room) return;

        set((s) => ({
          batches: s.batches.map((b) =>
            b.id === batchId
              ? {
                  ...b,
                  updatedAt: new Date(),
                  phases: b.phases.map((p) =>
                    p.id === phaseId
                      ? { ...p, roomId: newRoomId, roomName: room.name }
                      : p
                  ),
                }
              : b
          ),
        }));
      },

      updateBatchPlantCount: (batchId, newPlantCount) => set((state) => ({
        batches: state.batches.map((b) =>
          b.id === batchId
            ? { ...b, plantCount: newPlantCount, updatedAt: new Date() }
            : b
        ),
      })),

      splitBatch: (batchId, splitConfig) => {
        const state = get();
        const originalBatch = state.batches.find((b) => b.id === batchId);
        if (!originalBatch) return [];

        const newBatchIds: string[] = [];
        const newBatches: PlannedBatch[] = [];

        // Create new batches from split config
        splitConfig.forEach((config, index) => {
          const newId = `batch-${Date.now()}-${index}`;
          newBatchIds.push(newId);

          const newBatch: PlannedBatch = {
            ...originalBatch,
            id: newId,
            name: `${originalBatch.name}${config.suffix}`,
            code: `${originalBatch.code}${config.suffix}`,
            plantCount: config.plantCount,
            status: 'planned',
            createdAt: new Date(),
            updatedAt: new Date(),
            phases: originalBatch.phases.map((p) => ({
              ...p,
              id: `phase-${Date.now()}-${index}-${p.phase}`,
              actualStart: undefined,
              actualEnd: undefined,
            })),
          };

          newBatches.push(newBatch);
        });

        set((s) => ({
          batches: [
            ...s.batches.filter((b) => b.id !== batchId), // Remove original
            ...newBatches, // Add new split batches
          ],
          selectedBatchId: newBatchIds[0] || null, // Select first new batch
        }));

        return newBatchIds;
      },

      shiftBatchSchedule: (batchId, daysDelta) => {
        if (daysDelta === 0) return;

        set((state) => ({
          batches: state.batches.map((b) =>
            b.id === batchId
              ? {
                  ...b,
                  updatedAt: new Date(),
                  phases: b.phases.map((p) => ({
                    ...p,
                    plannedStart: addDays(p.plannedStart, daysDelta),
                    plannedEnd: addDays(p.plannedEnd, daysDelta),
                    // Keep actual dates as is - only shift planned
                  })),
                }
              : b
          ),
        }));
      },

      // Data loading actions
      setRooms: (rooms) => set({ rooms }),
      setBatches: (batches) => set({ batches }),
    }),
    {
      name: 'harvestry-planner',
      partialize: (state) => ({
        settings: state.settings,
        filters: state.filters,
      }),
    }
  )
);

