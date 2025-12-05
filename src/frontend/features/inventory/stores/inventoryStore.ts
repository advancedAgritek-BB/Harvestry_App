/**
 * Inventory Store
 * Zustand store for inventory state management with real-time updates
 */

import { create } from 'zustand';
import { devtools, subscribeWithSelector } from 'zustand/middleware';
import type {
  InventoryLot,
  LotSummary,
  LotFilterOptions,
  InventoryMovement,
  MovementSummary,
  InventoryLocation,
  LocationTreeNode,
  LocationSummary,
  ComplianceSummary,
  SyncQueueStatus,
  Hold,
} from '../types';

/** Store state interface */
interface InventoryState {
  // Current site context
  currentSiteId: string | null;
  
  // Lots
  lots: InventoryLot[];
  lotsTotal: number;
  lotsPage: number;
  lotsFilters: LotFilterOptions;
  lotsLoading: boolean;
  lotsError: string | null;
  lotsSummary: LotSummary | null;
  selectedLotIds: string[];
  
  // Current lot detail
  currentLot: InventoryLot | null;
  currentLotLoading: boolean;
  
  // Movements
  recentMovements: InventoryMovement[];
  movementsSummary: MovementSummary | null;
  movementsLoading: boolean;
  
  // Locations
  locationTree: LocationTreeNode[];
  locationsSummary: LocationSummary | null;
  locationsLoading: boolean;
  expandedLocationIds: Set<string>;
  selectedLocationId: string | null;
  
  // Compliance
  complianceSummary: ComplianceSummary | null;
  syncQueueStatus: SyncQueueStatus[];
  activeHolds: Hold[];
  complianceLoading: boolean;
  
  // UI State
  viewMode: 'grid' | 'list' | 'table';
  sidebarOpen: boolean;
  scannerActive: boolean;
  
  // Real-time
  isConnected: boolean;
  lastUpdate: string | null;
}

/** Store actions interface */
interface InventoryActions {
  // Site context
  setSiteId: (siteId: string) => void;
  
  // Lots
  setLots: (lots: InventoryLot[], total: number) => void;
  addLot: (lot: InventoryLot) => void;
  updateLot: (lotId: string, updates: Partial<InventoryLot>) => void;
  removeLot: (lotId: string) => void;
  setLotsFilters: (filters: LotFilterOptions) => void;
  setLotsPage: (page: number) => void;
  setLotsLoading: (loading: boolean) => void;
  setLotsError: (error: string | null) => void;
  setLotsSummary: (summary: LotSummary) => void;
  selectLot: (lotId: string, selected: boolean) => void;
  selectAllLots: (selected: boolean) => void;
  clearLotSelection: () => void;
  
  // Current lot
  setCurrentLot: (lot: InventoryLot | null) => void;
  setCurrentLotLoading: (loading: boolean) => void;
  
  // Movements
  setRecentMovements: (movements: InventoryMovement[]) => void;
  addMovement: (movement: InventoryMovement) => void;
  setMovementsSummary: (summary: MovementSummary) => void;
  setMovementsLoading: (loading: boolean) => void;
  
  // Locations
  setLocationTree: (tree: LocationTreeNode[]) => void;
  setLocationsSummary: (summary: LocationSummary) => void;
  setLocationsLoading: (loading: boolean) => void;
  toggleLocationExpanded: (locationId: string) => void;
  setSelectedLocation: (locationId: string | null) => void;
  
  // Compliance
  setComplianceSummary: (summary: ComplianceSummary) => void;
  setSyncQueueStatus: (status: SyncQueueStatus[]) => void;
  setActiveHolds: (holds: Hold[]) => void;
  setComplianceLoading: (loading: boolean) => void;
  
  // UI State
  setViewMode: (mode: 'grid' | 'list' | 'table') => void;
  toggleSidebar: () => void;
  setScannerActive: (active: boolean) => void;
  
  // Real-time
  setConnected: (connected: boolean) => void;
  handleRealtimeUpdate: (event: RealtimeEvent) => void;
  
  // Reset
  reset: () => void;
}

/** Real-time event types */
type RealtimeEvent =
  | { type: 'lot_created'; payload: InventoryLot }
  | { type: 'lot_updated'; payload: { id: string; updates: Partial<InventoryLot> } }
  | { type: 'lot_deleted'; payload: { id: string } }
  | { type: 'movement_created'; payload: InventoryMovement }
  | { type: 'sync_status_changed'; payload: SyncQueueStatus }
  | { type: 'hold_created'; payload: Hold }
  | { type: 'hold_released'; payload: { id: string } };

/** Initial state */
const initialState: InventoryState = {
  currentSiteId: null,
  
  lots: [],
  lotsTotal: 0,
  lotsPage: 1,
  lotsFilters: {},
  lotsLoading: false,
  lotsError: null,
  lotsSummary: null,
  selectedLotIds: [],
  
  currentLot: null,
  currentLotLoading: false,
  
  recentMovements: [],
  movementsSummary: null,
  movementsLoading: false,
  
  locationTree: [],
  locationsSummary: null,
  locationsLoading: false,
  expandedLocationIds: new Set(),
  selectedLocationId: null,
  
  complianceSummary: null,
  syncQueueStatus: [],
  activeHolds: [],
  complianceLoading: false,
  
  viewMode: 'table',
  sidebarOpen: true,
  scannerActive: false,
  
  isConnected: false,
  lastUpdate: null,
};

/** Create the store */
export const useInventoryStore = create<InventoryState & InventoryActions>()(
  devtools(
    subscribeWithSelector((set, get) => ({
      ...initialState,
      
      // Site context
      setSiteId: (siteId) => set({ currentSiteId: siteId }),
      
      // Lots
      setLots: (lots, total) => set({ lots, lotsTotal: total }),
      
      addLot: (lot) => set((state) => ({
        lots: [lot, ...state.lots],
        lotsTotal: state.lotsTotal + 1,
        lastUpdate: new Date().toISOString(),
      })),
      
      updateLot: (lotId, updates) => set((state) => ({
        lots: state.lots.map((l) =>
          l.id === lotId ? { ...l, ...updates } : l
        ),
        currentLot: state.currentLot?.id === lotId
          ? { ...state.currentLot, ...updates }
          : state.currentLot,
        lastUpdate: new Date().toISOString(),
      })),
      
      removeLot: (lotId) => set((state) => ({
        lots: state.lots.filter((l) => l.id !== lotId),
        lotsTotal: state.lotsTotal - 1,
        selectedLotIds: state.selectedLotIds.filter((id) => id !== lotId),
        lastUpdate: new Date().toISOString(),
      })),
      
      setLotsFilters: (filters) => set({ lotsFilters: filters, lotsPage: 1 }),
      setLotsPage: (page) => set({ lotsPage: page }),
      setLotsLoading: (loading) => set({ lotsLoading: loading }),
      setLotsError: (error) => set({ lotsError: error }),
      setLotsSummary: (summary) => set({ lotsSummary: summary }),
      
      selectLot: (lotId, selected) => set((state) => ({
        selectedLotIds: selected
          ? [...state.selectedLotIds, lotId]
          : state.selectedLotIds.filter((id) => id !== lotId),
      })),
      
      selectAllLots: (selected) => set((state) => ({
        selectedLotIds: selected ? state.lots.map((l) => l.id) : [],
      })),
      
      clearLotSelection: () => set({ selectedLotIds: [] }),
      
      // Current lot
      setCurrentLot: (lot) => set({ currentLot: lot }),
      setCurrentLotLoading: (loading) => set({ currentLotLoading: loading }),
      
      // Movements
      setRecentMovements: (movements) => set({ recentMovements: movements }),
      
      addMovement: (movement) => set((state) => ({
        recentMovements: [movement, ...state.recentMovements.slice(0, 49)],
        lastUpdate: new Date().toISOString(),
      })),
      
      setMovementsSummary: (summary) => set({ movementsSummary: summary }),
      setMovementsLoading: (loading) => set({ movementsLoading: loading }),
      
      // Locations
      setLocationTree: (tree) => set({ locationTree: tree }),
      setLocationsSummary: (summary) => set({ locationsSummary: summary }),
      setLocationsLoading: (loading) => set({ locationsLoading: loading }),
      
      toggleLocationExpanded: (locationId) => set((state) => {
        const newExpanded = new Set(state.expandedLocationIds);
        if (newExpanded.has(locationId)) {
          newExpanded.delete(locationId);
        } else {
          newExpanded.add(locationId);
        }
        return { expandedLocationIds: newExpanded };
      }),
      
      setSelectedLocation: (locationId) => set({ selectedLocationId: locationId }),
      
      // Compliance
      setComplianceSummary: (summary) => set({ complianceSummary: summary }),
      setSyncQueueStatus: (status) => set({ syncQueueStatus: status }),
      setActiveHolds: (holds) => set({ activeHolds: holds }),
      setComplianceLoading: (loading) => set({ complianceLoading: loading }),
      
      // UI State
      setViewMode: (mode) => set({ viewMode: mode }),
      toggleSidebar: () => set((state) => ({ sidebarOpen: !state.sidebarOpen })),
      setScannerActive: (active) => set({ scannerActive: active }),
      
      // Real-time
      setConnected: (connected) => set({ isConnected: connected }),
      
      handleRealtimeUpdate: (event) => {
        const state = get();
        
        switch (event.type) {
          case 'lot_created':
            state.addLot(event.payload);
            break;
            
          case 'lot_updated':
            state.updateLot(event.payload.id, event.payload.updates);
            break;
            
          case 'lot_deleted':
            state.removeLot(event.payload.id);
            break;
            
          case 'movement_created':
            state.addMovement(event.payload);
            break;
            
          case 'sync_status_changed':
            set((s) => ({
              syncQueueStatus: s.syncQueueStatus.map((sq) =>
                sq.provider === event.payload.provider && sq.siteId === event.payload.siteId
                  ? event.payload
                  : sq
              ),
            }));
            break;
            
          case 'hold_created':
            set((s) => ({
              activeHolds: [event.payload, ...s.activeHolds],
            }));
            break;
            
          case 'hold_released':
            set((s) => ({
              activeHolds: s.activeHolds.filter((h) => h.id !== event.payload.id),
            }));
            break;
        }
      },
      
      // Reset
      reset: () => set(initialState),
    })),
    { name: 'inventory-store' }
  )
);

/** Selector hooks for optimized re-renders */
export const useLotsState = () => useInventoryStore((state) => ({
  lots: state.lots,
  total: state.lotsTotal,
  page: state.lotsPage,
  filters: state.lotsFilters,
  loading: state.lotsLoading,
  error: state.lotsError,
  selectedIds: state.selectedLotIds,
}));

export const useCurrentLot = () => useInventoryStore((state) => ({
  lot: state.currentLot,
  loading: state.currentLotLoading,
}));

export const useComplianceState = () => useInventoryStore((state) => ({
  summary: state.complianceSummary,
  syncStatus: state.syncQueueStatus,
  holds: state.activeHolds,
  loading: state.complianceLoading,
}));

export const useLocationState = () => useInventoryStore((state) => ({
  tree: state.locationTree,
  summary: state.locationsSummary,
  loading: state.locationsLoading,
  expandedIds: state.expandedLocationIds,
  selectedId: state.selectedLocationId,
}));

export const useInventoryUI = () => useInventoryStore((state) => ({
  viewMode: state.viewMode,
  sidebarOpen: state.sidebarOpen,
  scannerActive: state.scannerActive,
  isConnected: state.isConnected,
  lastUpdate: state.lastUpdate,
}));
