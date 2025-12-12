import { create } from 'zustand';
import { persist } from 'zustand/middleware';

export interface Alert {
  id: string;
  title: string;
  source: string;
  severity: 'critical' | 'warning' | 'info';
  timestamp: string;
  dismissed: boolean;
}

interface AlertsState {
  alerts: Alert[];
  addAlert: (alert: Alert) => void;
  dismissAlert: (id: string) => void;
  clearAlerts: () => void;
  setAlerts: (alerts: Alert[]) => void;
}

export const useAlertsStore = create<AlertsState>()(
  persist(
    (set) => ({
      alerts: [],
      addAlert: (alert) => set((state) => ({ alerts: [alert, ...state.alerts] })),
      dismissAlert: (id) => set((state) => ({
        alerts: state.alerts.map((a) => 
          a.id === id ? { ...a, dismissed: true } : a
        )
      })),
      clearAlerts: () => set({ alerts: [] }),
      setAlerts: (alerts) => set({ alerts }),
    }),
    {
      name: 'harvestry-alerts',
    }
  )
);





