// Irrigation Windows Widget Components
export { ZoneSelector } from './ZoneSelector';
export { ShotDetailsPopover } from './ShotDetailsPopover';
export { HistoryPanel } from './HistoryPanel';
export { PerformancePanel } from './PerformancePanel';
export { WindowEditModal } from './WindowEditModal';
export { AlertsConfigModal } from './AlertsConfigModal';
export { CustomTooltip, ChartLegend } from './ChartComponents';
export { PauseControlButton } from './PauseControlButton';
export { PauseConfigModal } from './PauseConfigModal';
export { QuickPickConfigModal } from './QuickPickConfigModal';

// Mock data utilities
export { 
  DEFAULT_ZONES, 
  DEFAULT_WINDOWS, 
  generateMockData, 
  createManualShot, 
  estimateVwcAfterShot,
  PHASE_SHOT_VOLUMES,
  VWC_SOAK_TIME_MINUTES,
} from './mockData';

// Types
export * from './types';



