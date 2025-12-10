// src/frontend/stores/siteRoomStore.ts
// Store for managing currently selected site and room context

import { create } from 'zustand';
import { persist } from 'zustand/middleware';

export interface SiteOption {
  id: string;
  name: string;
}

export interface RoomOption {
  id: string;
  name: string;
  siteId: string;
  type?: string;
}

interface SiteRoomState {
  // Currently selected
  selectedSiteId: string | null;
  selectedRoomId: string | null;
  
  // Available options (cached from API)
  sites: SiteOption[];
  rooms: RoomOption[];
  
  // Actions
  setSelectedSite: (siteId: string | null) => void;
  setSelectedRoom: (roomId: string | null) => void;
  setSites: (sites: SiteOption[]) => void;
  setRooms: (rooms: RoomOption[]) => void;
  
  // Derived getters
  getSelectedSite: () => SiteOption | undefined;
  getSelectedRoom: () => RoomOption | undefined;
  getRoomsForSelectedSite: () => RoomOption[];
}

// Default mock data for development
const DEFAULT_SITES: SiteOption[] = [
  { id: 'site-1', name: 'Evergreen' },
  { id: 'site-2', name: 'Mountain View' },
  { id: 'site-3', name: 'Riverside' },
];

const DEFAULT_ROOMS: RoomOption[] = [
  // Evergreen site
  { id: 'f1', name: 'Flower • F1', siteId: 'site-1', type: 'Flowering' },
  { id: 'f2', name: 'Flower • F2', siteId: 'site-1', type: 'Flowering' },
  { id: 'f3', name: 'Flower • F3', siteId: 'site-1', type: 'Flowering' },
  { id: 'v1', name: 'Veg • V1', siteId: 'site-1', type: 'Vegetative' },
  { id: 'v2', name: 'Veg • V2', siteId: 'site-1', type: 'Vegetative' },
  { id: 'c1', name: 'Clone Room', siteId: 'site-1', type: 'Clone' },
  { id: 'd1', name: 'Dry Room', siteId: 'site-1', type: 'Drying' },
  // Mountain View site
  { id: 'mv-f1', name: 'Flower • F1', siteId: 'site-2', type: 'Flowering' },
  { id: 'mv-f2', name: 'Flower • F2', siteId: 'site-2', type: 'Flowering' },
  { id: 'mv-v1', name: 'Veg • V1', siteId: 'site-2', type: 'Vegetative' },
  // Riverside site
  { id: 'rs-f1', name: 'Flower • F1', siteId: 'site-3', type: 'Flowering' },
  { id: 'rs-v1', name: 'Veg • V1', siteId: 'site-3', type: 'Vegetative' },
];

export const useSiteRoomStore = create<SiteRoomState>()(
  persist(
    (set, get) => ({
      selectedSiteId: 'site-1',
      selectedRoomId: 'f1',
      sites: DEFAULT_SITES,
      rooms: DEFAULT_ROOMS,
      
      setSelectedSite: (siteId) => {
        set({ selectedSiteId: siteId });
        // Reset room selection when site changes
        const rooms = get().rooms.filter(r => r.siteId === siteId);
        if (rooms.length > 0) {
          set({ selectedRoomId: rooms[0].id });
        } else {
          set({ selectedRoomId: null });
        }
      },
      
      setSelectedRoom: (roomId) => set({ selectedRoomId: roomId }),
      
      setSites: (sites) => set({ sites }),
      
      setRooms: (rooms) => set({ rooms }),
      
      getSelectedSite: () => {
        const { selectedSiteId, sites } = get();
        return sites.find(s => s.id === selectedSiteId);
      },
      
      getSelectedRoom: () => {
        const { selectedRoomId, rooms } = get();
        return rooms.find(r => r.id === selectedRoomId);
      },
      
      getRoomsForSelectedSite: () => {
        const { selectedSiteId, rooms } = get();
        return rooms.filter(r => r.siteId === selectedSiteId);
      },
    }),
    {
      name: 'harvestry-site-room',
      partialize: (state) => ({
        selectedSiteId: state.selectedSiteId,
        selectedRoomId: state.selectedRoomId,
      }),
    }
  )
);

// Selector hooks for convenience
export const useSelectedSite = () => useSiteRoomStore(state => state.getSelectedSite());
export const useSelectedRoom = () => useSiteRoomStore(state => state.getSelectedRoom());
export const useAvailableSites = () => useSiteRoomStore(state => state.sites);
export const useAvailableRooms = () => useSiteRoomStore(state => state.getRoomsForSelectedSite());


