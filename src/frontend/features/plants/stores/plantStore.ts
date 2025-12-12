/**
 * Plant Store
 * 
 * Zustand store for managing plant state throughout cultivation.
 * Handles loading, caching, and mutations for plants and plant batches.
 */

import { create } from 'zustand';
import { devtools } from 'zustand/middleware';
import { PlantService } from '../services';
import type {
  Plant,
  PlantBatch,
  PlantCounts,
  PlantLossRecord,
  StartBatchRequest,
  RecordLossRequest,
  TransitionPhaseRequest,
  AssignTagsRequest,
} from '../types';

// =============================================================================
// STATE TYPES
// =============================================================================

interface PlantState {
  // Data by batch ID
  plantBatchesByBatch: Record<string, PlantBatch[]>;
  plantsByBatch: Record<string, Plant[]>;
  countsByBatch: Record<string, PlantCounts>;
  lossHistoryByBatch: Record<string, PlantLossRecord[]>;
  
  // Loading states
  isLoading: boolean;
  loadingBatchId: string | null;
  
  // Error handling
  error: string | null;
  
  // Currently selected batch for plant operations
  selectedBatchId: string | null;
  
  // Modal states
  showStartBatchModal: boolean;
  showRecordLossModal: boolean;
  showTransitionModal: boolean;
  showAssignTagsModal: boolean;
}

interface PlantActions {
  // Data loading
  loadPlantsForBatch: (batchId: string) => Promise<void>;
  refreshBatch: (batchId: string) => Promise<void>;
  
  // Batch operations
  startBatch: (request: StartBatchRequest) => Promise<PlantBatch>;
  recordLoss: (request: RecordLossRequest) => Promise<PlantLossRecord>;
  transitionPhase: (request: TransitionPhaseRequest) => Promise<void>;
  assignTags: (request: AssignTagsRequest) => Promise<Plant[]>;
  
  // Selection
  selectBatch: (batchId: string | null) => void;
  
  // Modal control
  openStartBatchModal: (batchId: string) => void;
  closeStartBatchModal: () => void;
  openRecordLossModal: (batchId: string) => void;
  closeRecordLossModal: () => void;
  openTransitionModal: (batchId: string) => void;
  closeTransitionModal: () => void;
  openAssignTagsModal: (batchId: string) => void;
  closeAssignTagsModal: () => void;
  
  // Helpers
  isBatchStarted: (batchId: string) => boolean;
  getPlantCounts: (batchId: string) => PlantCounts | null;
  clearError: () => void;
}

// =============================================================================
// DEFAULT STATE
// =============================================================================

const initialState: PlantState = {
  plantBatchesByBatch: {},
  plantsByBatch: {},
  countsByBatch: {},
  lossHistoryByBatch: {},
  isLoading: false,
  loadingBatchId: null,
  error: null,
  selectedBatchId: null,
  showStartBatchModal: false,
  showRecordLossModal: false,
  showTransitionModal: false,
  showAssignTagsModal: false,
};

// =============================================================================
// STORE
// =============================================================================

export const usePlantStore = create<PlantState & PlantActions>()(
  devtools(
    (set, get) => ({
      ...initialState,

      // =========================================================================
      // DATA LOADING
      // =========================================================================

      loadPlantsForBatch: async (batchId: string) => {
        set({ isLoading: true, loadingBatchId: batchId, error: null });

        try {
          const [plantBatches, plants, counts, lossHistory] = await Promise.all([
            PlantService.getPlantBatches(batchId),
            PlantService.getPlants(batchId),
            PlantService.getPlantCounts(batchId),
            PlantService.getLossHistory(batchId),
          ]);

          set((state) => ({
            plantBatchesByBatch: {
              ...state.plantBatchesByBatch,
              [batchId]: plantBatches,
            },
            plantsByBatch: {
              ...state.plantsByBatch,
              [batchId]: plants,
            },
            countsByBatch: {
              ...state.countsByBatch,
              [batchId]: counts,
            },
            lossHistoryByBatch: {
              ...state.lossHistoryByBatch,
              [batchId]: lossHistory,
            },
            isLoading: false,
            loadingBatchId: null,
          }));
        } catch (error) {
          set({
            isLoading: false,
            loadingBatchId: null,
            error: error instanceof Error ? error.message : 'Failed to load plants',
          });
        }
      },

      refreshBatch: async (batchId: string) => {
        await get().loadPlantsForBatch(batchId);
      },

      // =========================================================================
      // BATCH OPERATIONS
      // =========================================================================

      startBatch: async (request: StartBatchRequest) => {
        set({ isLoading: true, error: null });

        try {
          const plantBatch = await PlantService.startBatch(request);

          // Update local state
          set((state) => ({
            plantBatchesByBatch: {
              ...state.plantBatchesByBatch,
              [request.batchId]: [
                ...(state.plantBatchesByBatch[request.batchId] || []),
                plantBatch,
              ],
            },
            isLoading: false,
            showStartBatchModal: false,
          }));

          // Refresh counts
          await get().refreshBatch(request.batchId);

          return plantBatch;
        } catch (error) {
          set({
            isLoading: false,
            error: error instanceof Error ? error.message : 'Failed to start batch',
          });
          throw error;
        }
      },

      recordLoss: async (request: RecordLossRequest) => {
        set({ isLoading: true, error: null });

        try {
          const lossRecord = await PlantService.recordLoss(request);

          // Update local state
          set((state) => ({
            lossHistoryByBatch: {
              ...state.lossHistoryByBatch,
              [request.batchId]: [
                ...(state.lossHistoryByBatch[request.batchId] || []),
                lossRecord,
              ],
            },
            isLoading: false,
            showRecordLossModal: false,
          }));

          // Refresh batch data
          await get().refreshBatch(request.batchId);

          return lossRecord;
        } catch (error) {
          set({
            isLoading: false,
            error: error instanceof Error ? error.message : 'Failed to record loss',
          });
          throw error;
        }
      },

      transitionPhase: async (request: TransitionPhaseRequest) => {
        set({ isLoading: true, error: null });

        try {
          await PlantService.transitionPhase(request);

          set({
            isLoading: false,
            showTransitionModal: false,
          });

          // Refresh batch data
          await get().refreshBatch(request.batchId);
        } catch (error) {
          set({
            isLoading: false,
            error: error instanceof Error ? error.message : 'Failed to transition phase',
          });
          throw error;
        }
      },

      assignTags: async (request: AssignTagsRequest) => {
        set({ isLoading: true, error: null });

        try {
          const newPlants = await PlantService.assignTags(request);

          // Update local state
          set((state) => ({
            plantsByBatch: {
              ...state.plantsByBatch,
              [request.batchId]: [
                ...(state.plantsByBatch[request.batchId] || []),
                ...newPlants,
              ],
            },
            isLoading: false,
            showAssignTagsModal: false,
          }));

          // Refresh batch data
          await get().refreshBatch(request.batchId);

          return newPlants;
        } catch (error) {
          set({
            isLoading: false,
            error: error instanceof Error ? error.message : 'Failed to assign tags',
          });
          throw error;
        }
      },

      // =========================================================================
      // SELECTION
      // =========================================================================

      selectBatch: (batchId: string | null) => {
        set({ selectedBatchId: batchId });
        if (batchId) {
          get().loadPlantsForBatch(batchId);
        }
      },

      // =========================================================================
      // MODAL CONTROL
      // =========================================================================

      openStartBatchModal: (batchId: string) => {
        set({ selectedBatchId: batchId, showStartBatchModal: true });
      },

      closeStartBatchModal: () => {
        set({ showStartBatchModal: false });
      },

      openRecordLossModal: (batchId: string) => {
        set({ selectedBatchId: batchId, showRecordLossModal: true });
      },

      closeRecordLossModal: () => {
        set({ showRecordLossModal: false });
      },

      openTransitionModal: (batchId: string) => {
        set({ selectedBatchId: batchId, showTransitionModal: true });
      },

      closeTransitionModal: () => {
        set({ showTransitionModal: false });
      },

      openAssignTagsModal: (batchId: string) => {
        set({ selectedBatchId: batchId, showAssignTagsModal: true });
      },

      closeAssignTagsModal: () => {
        set({ showAssignTagsModal: false });
      },

      // =========================================================================
      // HELPERS
      // =========================================================================

      isBatchStarted: (batchId: string) => {
        const batches = get().plantBatchesByBatch[batchId];
        return batches && batches.length > 0;
      },

      getPlantCounts: (batchId: string) => {
        return get().countsByBatch[batchId] || null;
      },

      clearError: () => {
        set({ error: null });
      },
    }),
    { name: 'plant-store' }
  )
);

export default usePlantStore;





