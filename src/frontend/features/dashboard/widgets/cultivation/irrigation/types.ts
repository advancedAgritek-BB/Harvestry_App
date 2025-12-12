/**
 * Irrigation Windows Widget Types
 */

export type IrrigationPeriod = 'P1 - Ramp' | 'P2 - Maintenance' | 'P3 - Dryback' | 'All';

export interface IrrigationZone {
  id: string;
  name: string;
  isActive: boolean;
}

/** Event status distinguishing actual vs scheduled events */
export type IrrigationEventStatus = 'executed' | 'planned';

export interface IrrigationDataPoint {
  id: number;
  time: string;
  volume: number;
  /** VWC reading taken ~10 min after irrigation (null if pending/not yet measured) */
  endVwc: number | null;
  type: 'manual' | 'automated';
  zone: string;
  /** Which irrigation period this shot belongs to */
  period: Exclude<IrrigationPeriod, 'All'>;
  /** Whether VWC is still pending (within 10 min soak period) */
  vwcPending?: boolean;
  scheduledAt?: string;
  executedAt?: string;
  notes?: string;
  /** Whether this is a planned (ghost) event or an executed (actual) event */
  status: IrrigationEventStatus;
  /** For executed events: actual VWC measured. For trend line display. */
  actualVwc?: number;
}

/**
 * Determine which period a time falls into based on window definitions.
 * P3 spans from last P2 event through to first P1 of next day.
 */
export function getPeriodForTime(
  time: string,
  windows: IrrigationWindow[]
): Exclude<IrrigationPeriod, 'All'> {
  const [hours, minutes] = time.split(':').map(Number);
  const timeMinutes = hours * 60 + minutes;

  for (const window of windows) {
    if (!window.isActive) continue;
    
    const [startH, startM] = window.startTime.split(':').map(Number);
    const [endH, endM] = window.endTime.split(':').map(Number);
    const startMinutes = startH * 60 + startM;
    const endMinutes = endH * 60 + endM;

    if (timeMinutes >= startMinutes && timeMinutes < endMinutes) {
      return window.period;
    }
  }

  // Default to P3 - Dryback for times outside defined windows
  return 'P3 - Dryback';
}

export interface IrrigationWindow {
  id: string;
  period: Exclude<IrrigationPeriod, 'All'>;
  startTime: string; // HH:mm format
  endTime: string;   // HH:mm format
  isActive: boolean;
}

export interface VwcThresholdConfig {
  lowThreshold: number;   // %
  highThreshold: number;  // %
  alertOnLow: boolean;
  alertOnHigh: boolean;
}

export interface IrrigationShotLog {
  id: string;
  timestamp: string;
  volume: number;
  type: 'manual' | 'automated';
  zones: string[];
  period: IrrigationPeriod;
  triggeredBy?: string;
  endVwc?: number;
}

/**
 * Pause behavior mode - configurable by org admin
 * - 'resume_safety': Auto-resumes when daily schedule completes (default)
 * - 'permanent': Stays paused until manually unpaused
 */
export type PauseBehaviorMode = 'resume_safety' | 'permanent';

/**
 * Pause state for zones
 */
export interface ZonePauseState {
  /** Zone IDs that are currently paused */
  pausedZones: string[];
  /** When the pause was initiated */
  pausedAt: string | null;
  /** Who initiated the pause */
  pausedBy: string | null;
  /** Whether pause is currently active */
  isPaused: boolean;
}

/**
 * Pause configuration - organization-level setting
 */
export interface PauseConfig {
  /** Default behavior mode for pause */
  behaviorMode: PauseBehaviorMode;
  /** Whether non-admin users can change the mode per-pause */
  allowModeOverride: boolean;
}

/**
 * Default pause configuration
 */
export const DEFAULT_PAUSE_CONFIG: PauseConfig = {
  behaviorMode: 'resume_safety',
  allowModeOverride: false,
};

// Color constants for consistency
export const IRRIGATION_COLORS = {
  manual: '#fbbf24',           // Amber/Yellow - Manual irrigation
  automated: '#3b82f6',        // Blue - Automated irrigation
  vwc: '#10b981',              // Emerald/Green - VWC percentage
  threshold: '#ef4444',        // Red - Threshold lines
  plannedVolume: '#64748b',    // Slate - Ghost bars for planned volume
  plannedVwc: '#6ee7b7',       // Light Emerald - Ghost bars for expected VWC
} as const;

/**
 * Quick pick volume configuration - admin configurable
 * Up to 5 volume options that users can choose from
 */
export interface QuickPickConfig {
  /** Array of volume options in mL (max 5) */
  volumes: number[];
}

// Default quick pick volumes
export const DEFAULT_QUICK_PICK_CONFIG: QuickPickConfig = {
  volumes: [50, 75, 100, 125],
};

// Default VWC thresholds
export const DEFAULT_VWC_THRESHOLDS: VwcThresholdConfig = {
  lowThreshold: 30,
  highThreshold: 70,
  alertOnLow: true,
  alertOnHigh: true,
};

/**
 * Historical comparison data for a single irrigation event.
 * Compares planned (expected) values against actual execution.
 */
export interface IrrigationComparisonEvent {
  id: string;
  time: string;
  date: string;
  period: Exclude<IrrigationPeriod, 'All'>;
  zone: string;
  /** Expected values from the schedule */
  expected: {
    volume: number;
    vwc: number;
  };
  /** Actual values from execution (null if event didn't execute) */
  actual: {
    volume: number | null;
    vwc: number | null;
    executedAt: string | null;
    type: 'manual' | 'automated' | null;
  };
  /** Variance calculations */
  variance: {
    volumeDelta: number | null;     // actual - expected (mL)
    volumePercent: number | null;   // percentage difference
    vwcDelta: number | null;        // actual - expected (%)
    timingDelta: number | null;     // minutes early/late (negative = early)
    missed: boolean;                // true if expected event didn't execute
  };
}

/**
 * Summary statistics for historical performance.
 */
export interface IrrigationPerformanceSummary {
  dateRange: {
    start: string;
    end: string;
  };
  totalExpectedEvents: number;
  totalActualEvents: number;
  missedEvents: number;
  extraEvents: number;           // Manual shots not in schedule
  /** Volume metrics */
  volume: {
    totalExpected: number;
    totalActual: number;
    averageVariance: number;    // Average mL difference per event
    variancePercent: number;    // Overall percentage variance
  };
  /** VWC metrics */
  vwc: {
    averageExpected: number;
    averageActual: number;
    averageVariance: number;
    withinTargetPercent: number; // % of events where actual VWC was within threshold of expected
  };
  /** Timing metrics */
  timing: {
    averageDelayMinutes: number;
    onTimePercent: number;      // % of events within 5 min of scheduled time
  };
  /** Alignment score (0-100) */
  alignmentScore: number;
}



