import { create } from 'zustand';
import { persist } from 'zustand/middleware';

export type OverrideType = 'manual' | 'recipe' | 'schedule' | 'emergency';
export type OverrideSeverity = 'critical' | 'warning' | 'info';

export interface Override {
  id: string;
  type: OverrideType;
  label: string;
  details: string;
  severity: OverrideSeverity;
  metric?: string; // e.g., 'co2', 'temp', 'ec'
  targetValue?: number;
  unit?: string;
  expiresAt?: string; // ISO timestamp
  createdAt: string;
  roomId?: string;
  isActive: boolean;
}

interface OverridesState {
  overrides: Override[];
  
  // Actions
  addOverride: (override: Omit<Override, 'id' | 'createdAt' | 'isActive'>) => void;
  cancelOverride: (id: string) => void;
  clearExpired: () => void;
  setOverrides: (overrides: Override[]) => void;
  
  // Selectors
  getActiveOverrides: (roomId?: string) => Override[];
}

export const useOverridesStore = create<OverridesState>()(
  persist(
    (set, get) => ({
      overrides: [],
      
      addOverride: (override) => {
        const newOverride: Override = {
          ...override,
          id: `override-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
          createdAt: new Date().toISOString(),
          isActive: true,
        };
        set((state) => ({ overrides: [newOverride, ...state.overrides] }));
      },
      
      cancelOverride: (id) => {
        set((state) => ({
          overrides: state.overrides.map((o) =>
            o.id === id ? { ...o, isActive: false } : o
          ),
        }));
      },
      
      clearExpired: () => {
        const now = new Date().toISOString();
        set((state) => ({
          overrides: state.overrides.map((o) =>
            o.expiresAt && o.expiresAt < now ? { ...o, isActive: false } : o
          ),
        }));
      },
      
      setOverrides: (overrides) => set({ overrides }),
      
      getActiveOverrides: (roomId) => {
        const { overrides } = get();
        const now = new Date().toISOString();
        
        return overrides.filter((o) => {
          if (!o.isActive) return false;
          if (o.expiresAt && o.expiresAt < now) return false;
          if (roomId && o.roomId && o.roomId !== roomId) return false;
          return true;
        });
      },
    }),
    {
      name: 'harvestry-overrides',
    }
  )
);





