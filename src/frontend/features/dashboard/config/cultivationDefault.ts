import { WidgetConfig } from '../types/widget.types';

export const DEFAULT_CULTIVATION_LAYOUT: WidgetConfig[] = [
  // 1. Top Metrics Row (Full Width)
  {
    id: 'metrics-row',
    type: 'cultivation-metrics',
    position: { x: 0, y: 0 },
    size: '1x1', // Custom handling in renderer to span full width or use Grid classes
    title: 'Key Environment Metrics'
  },
  
  // 2. Main Content Area (Left Column - 9 cols)
  {
    id: 'trends-chart',
    type: 'cultivation-trends',
    position: { x: 0, y: 1 },
    size: '3x2',
    title: 'Environmental Trends'
  },
  {
    id: 'irrigation-chart',
    type: 'cultivation-irrigation',
    position: { x: 0, y: 2 },
    size: '2x1',
    title: 'Irrigation Windows'
  },
  {
    id: 'zone-heatmap',
    type: 'cultivation-heatmap',
    position: { x: 2, y: 2 },
    size: '1x1',
    title: 'Zone Heatmap'
  },
  {
    id: 'rooms-footer',
    type: 'cultivation-rooms',
    position: { x: 0, y: 3 },
    size: '3x1',
    title: 'Rooms Overview'
  },

  // 3. Sidebar (Right Column - 3 cols)
  {
    id: 'active-alerts',
    type: 'cultivation-alerts',
    position: { x: 3, y: 1 },
    size: '1x1',
    title: 'Active Alerts'
  },
  {
    id: 'targets-current',
    type: 'cultivation-targets',
    position: { x: 3, y: 2 }, // Stacked below alerts
    size: '1x1',
    title: 'Targets vs Current'
  },
  {
    id: 'quick-actions',
    type: 'cultivation-actions',
    position: { x: 3, y: 3 }, // Stacked at bottom
    size: '1x1',
    title: 'Quick Actions'
  }
];











