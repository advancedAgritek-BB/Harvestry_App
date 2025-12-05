import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { WidgetConfig } from '../types/widget.types';

interface DashboardState {
  widgets: WidgetConfig[];
  isEditMode: boolean;
  
  // Actions
  addWidget: (type: string, size?: WidgetConfig['size']) => void;
  removeWidget: (id: string) => void;
  updateWidgetPosition: (id: string, newPosition: any) => void; // Placeholder for grid coords
  toggleEditMode: () => void;
  resetLayout: () => void;
}

const DEFAULT_LAYOUT: WidgetConfig[] = [
  { id: 'kpi-1', type: 'kpi-cards', title: 'Key Performance Indicators', size: '2x1' },
  { id: 'env-1', type: 'environment-overview', title: 'Environment Overview', size: '2x1' },
  { id: 'task-1', type: 'task-queue', title: 'Task Queue', size: '2x2' },
  { id: 'perf-1', type: 'performance-chart', title: 'Performance Overview', size: '2x2' },
  { id: 'batch-1', type: 'active-batches', title: 'Active Batches', size: '1x1' },
  { id: 'alert-1', type: 'alerts', title: 'Alerts', size: '1x1' },
  { id: 'irrig-1', type: 'irrigation-status', title: 'Irrigation Status', size: '1x1' },
];

export const useDashboardStore = create<DashboardState>()(
  persist(
    (set) => ({
      widgets: DEFAULT_LAYOUT,
      isEditMode: false,

      addWidget: (type, size = '1x1') => set((state) => ({
        widgets: [
          ...state.widgets,
          {
            id: `${type}-${Date.now()}`,
            type,
            title: 'New Widget', // Should be looked up from registry
            size,
          }
        ]
      })),

      removeWidget: (id) => set((state) => ({
        widgets: state.widgets.filter((w) => w.id !== id)
      })),

      updateWidgetPosition: (id, _newPosition) => {
        // Placeholder: in a real grid lib, we'd update x/y/w/h here
        console.log('Update position', id);
      },

      toggleEditMode: () => set((state) => ({ isEditMode: !state.isEditMode })),
      
      resetLayout: () => set({ widgets: DEFAULT_LAYOUT }),
    }),
    {
      name: 'harvestry-dashboard-layout',
    }
  )
);

