import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface IntegrationState {
  slackConnected: boolean;
  slackWorkspace: string;
  slackMirrorMode: boolean;
  qboConnected: boolean;
  qboCompany: string;
  qboSyncMode: string;
  qboLastSync: string;
  
  // Actions
  setSlackConnected: (connected: boolean) => void;
  setQboConnected: (connected: boolean) => void;
  setQboLastSync: (time: string) => void;
}

export const useIntegrationStore = create<IntegrationState>()(
  persist(
    (set) => ({
      slackConnected: false,
      slackWorkspace: 'Harvestry Team',
      slackMirrorMode: false,
      qboConnected: false,
      qboCompany: 'Harvestry LLC',
      qboSyncMode: 'Item-level',
      qboLastSync: 'Never',

      setSlackConnected: (connected) => set({ slackConnected: connected }),
      setQboConnected: (connected) => set({ qboConnected: connected, qboLastSync: connected ? 'Just now' : 'Never' }),
      setQboLastSync: (time) => set({ qboLastSync: time }),
    }),
    {
      name: 'harvestry-integrations',
    }
  )
);




