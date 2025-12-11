import { WidgetRegistryItem } from '../types/widget.types';
import { 
  TaskQueueWidget, 
  ActiveBatchesWidget, 
  AlertsWidget, 
  IrrigationStatusWidget 
} from './operations';
import {
  KPICardsWidget,
  PerformanceChartWidget,
  EnvironmentOverviewWidget
} from './analytics';
import {
  EnvironmentalMetricsWidget
} from './cultivation/EnvironmentalMetricsWidget';
import {
  EnvironmentalTrendsWidget
} from './cultivation/EnvironmentalTrendsWidget';
import {
  IrrigationWindowsWidget
} from './cultivation/IrrigationWindowsWidget';
import {
  ZoneHeatmapWidget
} from './cultivation/ZoneHeatmapWidget';
import {
  RoomsStatusWidget
} from './cultivation/RoomsStatusWidget';
import {
  ActiveAlertsListWidget,
  TargetsVsCurrentWidget,
  QuickActionsWidget
} from './cultivation/SidebarWidgets';

// Registry will be populated as we build widgets
export const widgetRegistry: Record<string, WidgetRegistryItem> = {
  // Cultivation Widgets (New)
  'cultivation-metrics': {
    type: 'cultivation-metrics',
    component: EnvironmentalMetricsWidget,
    defaultSize: '3x2', // Full width row concept
    title: 'Key Metrics',
    description: 'Temp, RH, CO2, Light, Substrate',
    category: 'cultivation',
    permissions: ['cultivation:read']
  },
  'cultivation-trends': {
    type: 'cultivation-trends',
    component: EnvironmentalTrendsWidget,
    defaultSize: '2x2', 
    title: 'Environmental Trends',
    description: 'Historical chart for environmental data',
    category: 'cultivation',
    permissions: ['cultivation:read']
  },
  'cultivation-irrigation': {
    type: 'cultivation-irrigation',
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    component: IrrigationWindowsWidget as any,
    defaultSize: '2x1',
    title: 'Irrigation Windows',
    description: 'Dual bar chart for volume and VWC',
    category: 'cultivation',
    permissions: ['cultivation:read']
  },
  'cultivation-heatmap': {
    type: 'cultivation-heatmap',
    component: ZoneHeatmapWidget,
    defaultSize: '1x1',
    title: 'Zone Heatmap',
    description: 'Spatial sensor visualization',
    category: 'cultivation',
    permissions: ['cultivation:read']
  },
  'cultivation-rooms': {
    type: 'cultivation-rooms',
    component: RoomsStatusWidget,
    defaultSize: '3x2', 
    title: 'Rooms Overview',
    description: 'Navigation and status for other rooms',
    category: 'cultivation',
    permissions: ['cultivation:read']
  },
  'cultivation-alerts': {
    type: 'cultivation-alerts',
    component: ActiveAlertsListWidget,
    defaultSize: '1x1',
    title: 'Cultivation Alerts',
    description: 'Active alerts list with actions',
    category: 'cultivation',
    permissions: ['cultivation:read']
  },
  'cultivation-targets': {
    type: 'cultivation-targets',
    component: TargetsVsCurrentWidget,
    defaultSize: '1x1',
    title: 'Targets vs Current',
    description: 'Table comparing metrics to targets',
    category: 'cultivation',
    permissions: ['cultivation:read']
  },
  'cultivation-actions': {
    type: 'cultivation-actions',
    component: QuickActionsWidget,
    defaultSize: '1x1',
    title: 'Quick Actions',
    description: 'Common operational tasks',
    category: 'cultivation',
    permissions: ['cultivation:write']
  },

  // Operations
  'task-queue': {
    type: 'task-queue',
    component: TaskQueueWidget,
    defaultSize: '2x2',
    title: 'Task Queue',
    description: 'View and manage urgent tasks sorted by SLA',
    category: 'operations',
    permissions: ['operations:read']
  },
  'active-batches': {
    type: 'active-batches',
    component: ActiveBatchesWidget,
    defaultSize: '1x1',
    title: 'Active Batches',
    description: 'Monitor batches by lifecycle stage',
    category: 'operations',
    permissions: ['operations:read']
  },
  'alerts': {
    type: 'alerts',
    component: AlertsWidget,
    defaultSize: '1x1',
    title: 'Active Alerts',
    description: 'Real-time alerts and warnings',
    category: 'operations',
    permissions: ['operations:read']
  },
  'irrigation-status': {
    type: 'irrigation-status',
    component: IrrigationStatusWidget,
    defaultSize: '1x1',
    title: 'Irrigation Status',
    description: 'Zone status and active programs',
    category: 'operations',
    permissions: ['operations:read']
  },
  // Analytics
  'kpi-cards': {
    type: 'kpi-cards',
    component: KPICardsWidget,
    defaultSize: '2x1',
    title: 'KPI Cards',
    description: 'High-level performance metrics',
    category: 'analytics',
    permissions: ['analytics:read']
  },
  'performance-chart': {
    type: 'performance-chart',
    component: PerformanceChartWidget,
    defaultSize: '2x2',
    title: 'Performance Overview',
    description: 'Yield and efficiency trends over time',
    category: 'analytics',
    permissions: ['analytics:read']
  },
  'environment-overview': {
    type: 'environment-overview',
    component: EnvironmentOverviewWidget,
    defaultSize: '2x1',
    title: 'Environment Overview',
    description: 'Real-time climate metrics sparklines',
    category: 'analytics',
    permissions: ['analytics:read']
  }
};

export function getWidgetComponent(type: string) {
  return widgetRegistry[type]?.component;
}
