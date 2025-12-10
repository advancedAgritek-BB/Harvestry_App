/**
 * Shared Batch Store
 * 
 * Single source of truth for cultivation batches across all features.
 * The CultivationBatch type is the canonical representation.
 * Adapter functions convert to feature-specific display formats (e.g., PlannedBatch for planner).
 */

import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { addDays, differenceInDays, parseISO } from 'date-fns';
import type {
  CultivationBatch,
  CultivationPhase,
  BatchStatus,
  BatchPhaseEvent,
} from '@/features/inventory/types/batch.types';

// Phase duration defaults (used for scheduling)
export const PHASE_DURATION_DEFAULTS: Record<CultivationPhase, number> = {
  germination: 5,
  propagation: 14,
  vegetative: 28,
  flowering: 56,
  harvest: 1,
  drying: 10,
  curing: 21,
  complete: 0,
};

// Phase order for lifecycle sequencing
export const PHASE_ORDER: CultivationPhase[] = [
  'germination',
  'propagation',
  'vegetative',
  'flowering',
  'harvest',
  'drying',
  'curing',
  'complete',
];

interface BatchStore {
  // State
  batches: CultivationBatch[];
  isLoading: boolean;
  lastSyncedAt: Date | null;
  
  // CRUD Operations
  setBatches: (batches: CultivationBatch[]) => void;
  addBatch: (batch: CultivationBatch) => void;
  updateBatch: (id: string, updates: Partial<CultivationBatch>) => void;
  deleteBatch: (id: string) => void;
  
  // Batch Operations
  duplicateBatch: (id: string) => string | null;
  splitBatch: (id: string, splits: { plantCount: number; suffix: string }[]) => string[];
  
  // Phase Operations
  transitionPhase: (batchId: string, toPhase: CultivationPhase, notes?: string) => void;
  updateBatchRoom: (batchId: string, roomId: string, roomName: string) => void;
  
  // Plant Count Operations
  updatePlantCount: (batchId: string, newCount: number, lossReason?: string) => void;
  
  // Schedule Operations
  shiftSchedule: (batchId: string, daysDelta: number) => void;
  updateExpectedHarvestDate: (batchId: string, date: string) => void;
  
  // Query helpers
  getBatchById: (id: string) => CultivationBatch | undefined;
  getBatchesByStatus: (status: BatchStatus[]) => CultivationBatch[];
  getBatchesByPhase: (phase: CultivationPhase) => CultivationBatch[];
  getBatchesByRoom: (roomId: string) => CultivationBatch[];
  getActiveBatches: () => CultivationBatch[];
  
  // Sync
  setLoading: (loading: boolean) => void;
  markSynced: () => void;
}

export const useBatchStore = create<BatchStore>()(
  persist(
    (set, get) => ({
      batches: [],
      isLoading: false,
      lastSyncedAt: null,

      // CRUD
      setBatches: (batches) => set({ batches }),

      addBatch: (batch) => set((state) => ({
        batches: [...state.batches, batch],
      })),

      updateBatch: (id, updates) => set((state) => ({
        batches: state.batches.map((b) =>
          b.id === id
            ? { ...b, ...updates, updatedAt: new Date().toISOString() }
            : b
        ),
      })),

      deleteBatch: (id) => set((state) => ({
        batches: state.batches.filter((b) => b.id !== id),
      })),

      duplicateBatch: (id) => {
        const batch = get().getBatchById(id);
        if (!batch) return null;

        const newId = `batch-${Date.now()}`;
        const now = new Date().toISOString();

        const newBatch: CultivationBatch = {
          ...batch,
          id: newId,
          batchNumber: `${batch.batchNumber}-COPY`,
          name: batch.name ? `${batch.name} (Copy)` : undefined,
          status: 'planned',
          currentPhase: 'propagation',
          phaseHistory: [],
          plantLossEvents: [],
          totalPlantsLost: 0,
          survivalRate: 100,
          currentPlantCount: batch.initialPlantCount,
          actualHarvestDate: undefined,
          actualDays: undefined,
          actualWetWeightGrams: undefined,
          actualDryWeightGrams: undefined,
          harvestEventIds: [],
          outputLotIds: [],
          metrcBatchId: undefined,
          metrcPlantingId: undefined,
          createdAt: now,
          createdBy: 'system',
          updatedAt: now,
          updatedBy: 'system',
        };

        set((state) => ({
          batches: [...state.batches, newBatch],
        }));

        return newId;
      },

      splitBatch: (id, splits) => {
        const original = get().getBatchById(id);
        if (!original) return [];

        const newIds: string[] = [];
        const now = new Date().toISOString();

        const newBatches = splits.map((split, index) => {
          const newId = `batch-${Date.now()}-${index}`;
          newIds.push(newId);

          const newBatch: CultivationBatch = {
            ...original,
            id: newId,
            batchNumber: `${original.batchNumber}${split.suffix}`,
            name: original.name ? `${original.name}${split.suffix}` : undefined,
            initialPlantCount: split.plantCount,
            currentPlantCount: split.plantCount,
            status: 'planned',
            parentBatchId: original.id,
            parentBatchNumber: original.batchNumber,
            phaseHistory: [],
            plantLossEvents: [],
            totalPlantsLost: 0,
            survivalRate: 100,
            harvestEventIds: [],
            outputLotIds: [],
            createdAt: now,
            createdBy: 'system',
            updatedAt: now,
            updatedBy: 'system',
          };

          return newBatch;
        });

        set((state) => ({
          batches: [
            ...state.batches.filter((b) => b.id !== id),
            ...newBatches,
          ],
        }));

        return newIds;
      },

      // Phase Operations
      transitionPhase: (batchId, toPhase, notes) => {
        const batch = get().getBatchById(batchId);
        if (!batch) return;

        const now = new Date().toISOString();
        const phaseEvent: BatchPhaseEvent = {
          id: `phase-event-${Date.now()}`,
          batchId,
          fromPhase: batch.currentPhase,
          toPhase,
          transitionDate: now,
          notes,
          performedBy: 'system',
        };

        set((state) => ({
          batches: state.batches.map((b) =>
            b.id === batchId
              ? {
                  ...b,
                  currentPhase: toPhase,
                  phaseHistory: [...b.phaseHistory, phaseEvent],
                  updatedAt: now,
                }
              : b
          ),
        }));
      },

      updateBatchRoom: (batchId, roomId, roomName) => set((state) => ({
        batches: state.batches.map((b) =>
          b.id === batchId
            ? {
                ...b,
                currentRoomId: roomId,
                currentRoomName: roomName,
                updatedAt: new Date().toISOString(),
              }
            : b
        ),
      })),

      // Plant Count
      updatePlantCount: (batchId, newCount, _lossReason) => {
        const batch = get().getBatchById(batchId);
        if (!batch) return;

        const lostPlants = batch.currentPlantCount - newCount;
        const newSurvivalRate = Math.round((newCount / batch.initialPlantCount) * 100);

        set((state) => ({
          batches: state.batches.map((b) =>
            b.id === batchId
              ? {
                  ...b,
                  currentPlantCount: newCount,
                  totalPlantsLost: b.totalPlantsLost + Math.max(0, lostPlants),
                  survivalRate: newSurvivalRate,
                  updatedAt: new Date().toISOString(),
                }
              : b
          ),
        }));
      },

      // Schedule Operations
      shiftSchedule: (batchId, daysDelta) => {
        if (daysDelta === 0) return;

        const batch = get().getBatchById(batchId);
        if (!batch) return;

        const newStartDate = addDays(parseISO(batch.startDate), daysDelta);
        const newExpectedHarvest = batch.expectedHarvestDate
          ? addDays(parseISO(batch.expectedHarvestDate), daysDelta)
          : undefined;

        set((state) => ({
          batches: state.batches.map((b) =>
            b.id === batchId
              ? {
                  ...b,
                  startDate: newStartDate.toISOString(),
                  expectedHarvestDate: newExpectedHarvest?.toISOString(),
                  updatedAt: new Date().toISOString(),
                }
              : b
          ),
        }));
      },

      updateExpectedHarvestDate: (batchId, date) => set((state) => ({
        batches: state.batches.map((b) =>
          b.id === batchId
            ? {
                ...b,
                expectedHarvestDate: date,
                expectedDays: differenceInDays(parseISO(date), parseISO(b.startDate)),
                updatedAt: new Date().toISOString(),
              }
            : b
        ),
      })),

      // Query helpers
      getBatchById: (id) => get().batches.find((b) => b.id === id),

      getBatchesByStatus: (statuses) =>
        get().batches.filter((b) => statuses.includes(b.status)),

      getBatchesByPhase: (phase) =>
        get().batches.filter((b) => b.currentPhase === phase),

      getBatchesByRoom: (roomId) =>
        get().batches.filter((b) => b.currentRoomId === roomId),

      getActiveBatches: () =>
        get().batches.filter((b) => b.status === 'active' || b.status === 'planned'),

      // Sync
      setLoading: (loading) => set({ isLoading: loading }),
      markSynced: () => set({ lastSyncedAt: new Date() }),
    }),
    {
      name: 'harvestry-batches',
      partialize: (state) => ({
        batches: state.batches,
        lastSyncedAt: state.lastSyncedAt,
      }),
    }
  )
);



