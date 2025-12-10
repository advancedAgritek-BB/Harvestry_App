/**
 * Genetics Store
 * 
 * Zustand store for genetics state management.
 * Provides centralized state for the genetics library and batch planner.
 */

import { create } from 'zustand';
import { devtools, persist } from 'zustand/middleware';
import type {
  Genetics,
  GeneticsFilters,
  PlannerGenetics,
} from '../types';
import { toPlannerGenetics } from '../types';
import { GeneticsService } from '../services';

// =============================================================================
// STATE INTERFACE
// =============================================================================

interface GeneticsState {
  // Site context
  currentSiteId: string | null;
  
  // Genetics data
  genetics: Genetics[];
  geneticsTotal: number;
  geneticsLoading: boolean;
  geneticsError: string | null;
  
  // Filters
  filters: GeneticsFilters;
  
  // Selection
  selectedGeneticsId: string | null;
  selectedGenetics: Genetics | null;
  
  // UI state
  viewMode: 'grid' | 'table';
  isModalOpen: boolean;
  editingGeneticsId: string | null;
}

// =============================================================================
// ACTIONS INTERFACE
// =============================================================================

interface GeneticsActions {
  // Site context
  setSiteId: (siteId: string) => void;
  
  // Data loading
  loadGenetics: (siteId?: string) => Promise<void>;
  refreshGenetics: () => Promise<void>;
  
  // CRUD operations
  createGenetics: (request: Parameters<typeof GeneticsService.createGenetics>[1]) => Promise<Genetics>;
  updateGenetics: (geneticsId: string, request: Parameters<typeof GeneticsService.updateGenetics>[2]) => Promise<Genetics>;
  deleteGenetics: (geneticsId: string) => Promise<void>;
  
  // Selection
  selectGenetics: (geneticsId: string | null) => void;
  getGeneticsById: (geneticsId: string) => Genetics | undefined;
  
  // Planner helpers
  getGeneticsForPlanner: () => PlannerGenetics[];
  getPlannerGeneticsById: (geneticsId: string) => PlannerGenetics | undefined;
  
  // Filters
  setFilters: (filters: Partial<GeneticsFilters>) => void;
  clearFilters: () => void;
  
  // UI state
  setViewMode: (mode: 'grid' | 'table') => void;
  openModal: (geneticsId?: string) => void;
  closeModal: () => void;
  
  // State setters
  setGenetics: (genetics: Genetics[]) => void;
  setGeneticsLoading: (loading: boolean) => void;
  setGeneticsError: (error: string | null) => void;
  
  // Reset
  reset: () => void;
}

// =============================================================================
// INITIAL STATE
// =============================================================================

const initialState: GeneticsState = {
  currentSiteId: null,
  genetics: [],
  geneticsTotal: 0,
  geneticsLoading: false,
  geneticsError: null,
  filters: {},
  selectedGeneticsId: null,
  selectedGenetics: null,
  viewMode: 'table',
  isModalOpen: false,
  editingGeneticsId: null,
};

// =============================================================================
// STORE
// =============================================================================

export const useGeneticsStore = create<GeneticsState & GeneticsActions>()(
  devtools(
    persist(
      (set, get) => ({
        ...initialState,

        // =====================================================================
        // SITE CONTEXT
        // =====================================================================

        setSiteId: (siteId) => {
          set({ currentSiteId: siteId });
          // Automatically load genetics when site changes
          get().loadGenetics(siteId);
        },

        // =====================================================================
        // DATA LOADING
        // =====================================================================

        loadGenetics: async (siteId) => {
          const targetSiteId = siteId || get().currentSiteId;
          if (!targetSiteId) {
            console.warn('Cannot load genetics: no site ID provided');
            return;
          }

          set({ geneticsLoading: true, geneticsError: null });

          try {
            const response = await GeneticsService.getGenetics(targetSiteId, get().filters);
            set({
              genetics: response.items,
              geneticsTotal: response.total,
              geneticsLoading: false,
            });
          } catch (error) {
            const errorMessage = error instanceof Error ? error.message : 'Failed to load genetics';
            set({ geneticsError: errorMessage, geneticsLoading: false });
            console.error('Failed to load genetics:', error);
          }
        },

        refreshGenetics: async () => {
          await get().loadGenetics();
        },

        // =====================================================================
        // CRUD OPERATIONS
        // =====================================================================

        createGenetics: async (request) => {
          const siteId = get().currentSiteId;
          if (!siteId) {
            throw new Error('No site selected');
          }

          const newGenetics = await GeneticsService.createGenetics(siteId, request);
          
          set((state) => ({
            genetics: [...state.genetics, newGenetics],
            geneticsTotal: state.geneticsTotal + 1,
          }));

          return newGenetics;
        },

        updateGenetics: async (geneticsId, request) => {
          const siteId = get().currentSiteId;
          if (!siteId) {
            throw new Error('No site selected');
          }

          const updatedGenetics = await GeneticsService.updateGenetics(siteId, geneticsId, request);
          
          set((state) => ({
            genetics: state.genetics.map((g) =>
              g.id === geneticsId ? updatedGenetics : g
            ),
            selectedGenetics:
              state.selectedGeneticsId === geneticsId ? updatedGenetics : state.selectedGenetics,
          }));

          return updatedGenetics;
        },

        deleteGenetics: async (geneticsId) => {
          const siteId = get().currentSiteId;
          if (!siteId) {
            throw new Error('No site selected');
          }

          await GeneticsService.deleteGenetics(siteId, geneticsId);
          
          set((state) => ({
            genetics: state.genetics.filter((g) => g.id !== geneticsId),
            geneticsTotal: state.geneticsTotal - 1,
            selectedGeneticsId:
              state.selectedGeneticsId === geneticsId ? null : state.selectedGeneticsId,
            selectedGenetics:
              state.selectedGeneticsId === geneticsId ? null : state.selectedGenetics,
          }));
        },

        // =====================================================================
        // SELECTION
        // =====================================================================

        selectGenetics: (geneticsId) => {
          const selectedGenetics = geneticsId
            ? get().genetics.find((g) => g.id === geneticsId) || null
            : null;
          
          set({
            selectedGeneticsId: geneticsId,
            selectedGenetics,
          });
        },

        getGeneticsById: (geneticsId) => {
          return get().genetics.find((g) => g.id === geneticsId);
        },

        // =====================================================================
        // PLANNER HELPERS
        // =====================================================================

        getGeneticsForPlanner: () => {
          return get().genetics.map(toPlannerGenetics);
        },

        getPlannerGeneticsById: (geneticsId) => {
          const genetics = get().getGeneticsById(geneticsId);
          return genetics ? toPlannerGenetics(genetics) : undefined;
        },

        // =====================================================================
        // FILTERS
        // =====================================================================

        setFilters: (filters) => {
          set((state) => ({
            filters: { ...state.filters, ...filters },
          }));
          // Reload with new filters
          get().loadGenetics();
        },

        clearFilters: () => {
          set({ filters: {} });
          get().loadGenetics();
        },

        // =====================================================================
        // UI STATE
        // =====================================================================

        setViewMode: (mode) => {
          set({ viewMode: mode });
        },

        openModal: (geneticsId) => {
          set({
            isModalOpen: true,
            editingGeneticsId: geneticsId || null,
          });
        },

        closeModal: () => {
          set({
            isModalOpen: false,
            editingGeneticsId: null,
          });
        },

        // =====================================================================
        // STATE SETTERS
        // =====================================================================

        setGenetics: (genetics) => {
          set({ genetics, geneticsTotal: genetics.length });
        },

        setGeneticsLoading: (loading) => {
          set({ geneticsLoading: loading });
        },

        setGeneticsError: (error) => {
          set({ geneticsError: error });
        },

        // =====================================================================
        // RESET
        // =====================================================================

        reset: () => {
          set(initialState);
        },
      }),
      {
        name: 'harvestry-genetics',
        partialize: (state) => ({
          // Only persist UI preferences
          viewMode: state.viewMode,
          filters: state.filters,
        }),
      }
    ),
    { name: 'GeneticsStore' }
  )
);

// =============================================================================
// SELECTOR HOOKS
// =============================================================================

/**
 * Hook to get filtered genetics list
 */
export function useFilteredGenetics() {
  return useGeneticsStore((state) => state.genetics);
}

/**
 * Hook to get genetics for the batch planner
 */
export function usePlannerGenetics() {
  return useGeneticsStore((state) => state.getGeneticsForPlanner());
}

/**
 * Hook to get selected genetics
 */
export function useSelectedGenetics() {
  return useGeneticsStore((state) => state.selectedGenetics);
}

/**
 * Hook to get loading state
 */
export function useGeneticsLoading() {
  return useGeneticsStore((state) => state.geneticsLoading);
}

export default useGeneticsStore;


