import type { IrrigationDataPoint, IrrigationWindow, IrrigationZone, IrrigationPeriod, IrrigationEventStatus } from './types';

// Default zones (in production, fetched from API)
export const DEFAULT_ZONES: IrrigationZone[] = [
  { id: 'A', name: 'A', isActive: true },
  { id: 'B', name: 'B', isActive: true },
  { id: 'C', name: 'C', isActive: true },
  { id: 'D', name: 'D', isActive: true },
  { id: 'E', name: 'E', isActive: true },
  { id: 'F', name: 'F', isActive: true },
];

// Default window definitions for period assignment
export const DEFAULT_WINDOWS: IrrigationWindow[] = [
  { id: 'p1', period: 'P1 - Ramp', startTime: '08:00', endTime: '12:00', isActive: true },
  { id: 'p2', period: 'P2 - Maintenance', startTime: '12:00', endTime: '16:00', isActive: true },
  { id: 'p3', period: 'P3 - Dryback', startTime: '16:00', endTime: '20:00', isActive: true },
];

/**
 * Parse time string to minutes since midnight for comparison.
 */
function parseTimeToMinutes(time: string): number {
  const [hours, minutes] = time.split(':').map(Number);
  return hours * 60 + minutes;
}

/**
 * Get current time in minutes since midnight.
 */
function getCurrentTimeMinutes(): number {
  const now = new Date();
  return now.getHours() * 60 + now.getMinutes();
}

/**
 * Phase configuration for shot volumes and VWC behavior.
 * 
 * VWC behavior by period:
 * - P1 (Ramp): Starts at 8am with 50mL, then 100mL shots to reach field capacity (60%)
 * - P2 (Maintenance): 5% dryback between shots, each shot returns to field capacity (60%)
 * - P3 (Dryback): No shots, VWC falls progressively as substrate dries
 */
interface PhaseConfig {
  shotVolume: number;     // Standard automated shot volume for this phase
  firstShotVolume?: number; // Optional different volume for first shot
  startVwc: number;       // VWC at start of phase
  fieldCapacity: number;  // Target VWC (field capacity)
  drybackPercent?: number; // Dryback between shots (P2)
}

const PHASE_CONFIG: Record<Exclude<IrrigationPeriod, 'All'>, PhaseConfig> = {
  'P1 - Ramp': { 
    shotVolume: 100,       // 100mL shots during ramp
    firstShotVolume: 50,   // First shot is 50mL
    startVwc: 30,          // Overnight low
    fieldCapacity: 60,     // Target VWC
  },
  'P2 - Maintenance': { 
    shotVolume: 100,       // Shots to restore field capacity
    startVwc: 55,          // After 5% dryback from 60%
    fieldCapacity: 60,     // Field capacity target
    drybackPercent: 5,     // 5% dryback between shots
  },
  'P3 - Dryback': { 
    shotVolume: 0,         // No shots during dryback
    startVwc: 60,          // Starts at field capacity
    fieldCapacity: 60,
  },
};

// Export for use in quick pick defaults
export const PHASE_SHOT_VOLUMES = {
  'P1 - Ramp': PHASE_CONFIG['P1 - Ramp'].shotVolume,
  'P2 - Maintenance': PHASE_CONFIG['P2 - Maintenance'].shotVolume,
  'P3 - Dryback': 50, // Emergency/manual shots during dryback
} as const;

/**
 * Determine event status based on time comparison.
 * Events before current time are executed, events at or after are planned.
 */
function getEventStatus(eventTimeMinutes: number, currentMinutes: number): IrrigationEventStatus {
  return eventTimeMinutes < currentMinutes ? 'executed' : 'planned';
}

/**
 * Generate realistic mock irrigation data matching cultivation patterns.
 * 
 * Schedule:
 * - P1 (8am-12pm): 50mL starter shot at 8am, then 100mL shots to reach field capacity (60%)
 * - P2 (12pm-4pm): 5% dryback between shots, each shot restores to field capacity (60%)
 * - P3 (4pm-8pm): No shots, VWC falls as substrate dries for overnight
 * 
 * VWC reading is taken ~10 min AFTER each shot (soak time).
 * 
 * Events before current time are marked as 'executed' with actualVwc for trend line.
 * Events at or after current time are marked as 'planned' (ghost bars).
 */
export function generateMockData(_windows: IrrigationWindow[]): IrrigationDataPoint[] {
  const data: IrrigationDataPoint[] = [];
  let id = 0;
  const p1Config = PHASE_CONFIG['P1 - Ramp'];
  const p2Config = PHASE_CONFIG['P2 - Maintenance'];
  const currentMinutes = getCurrentTimeMinutes();
  
  // P1 - Ramp (8:00 - 11:30)
  // First shot 50mL at 8am, then 100mL shots every 30 min to reach 60% field capacity
  let currentVwc = p1Config.startVwc; // ~30% overnight low
  const p1Shots = 8; // 8am, 8:30, 9:00, 9:30, 10:00, 10:30, 11:00, 11:30
  const vwcGainPerShot = (p1Config.fieldCapacity - p1Config.startVwc) / p1Shots;
  
  for (let i = 0; i < p1Shots; i++) {
    const hour = 8 + Math.floor(i / 2);
    const minutes = (i % 2) * 30;
    const time = `${hour}:${minutes.toString().padStart(2, '0')}`;
    const eventMinutes = parseTimeToMinutes(time);
    const volume = i === 0 ? (p1Config.firstShotVolume ?? 50) : p1Config.shotVolume;
    const status = getEventStatus(eventMinutes, currentMinutes);
    
    // VWC rises with each shot toward field capacity
    currentVwc += vwcGainPerShot * (i === 0 ? 0.5 : 1); // First shot adds less
    currentVwc = Math.min(currentVwc, p1Config.fieldCapacity);
    const vwcValue = Math.round(currentVwc * 10) / 10;
    
    data.push({
      id: id++, time, volume,
      endVwc: vwcValue,
      type: 'automated',
      zone: ['A', 'B', 'C', 'D', 'E', 'F'][i % 6],
      period: 'P1 - Ramp',
      status,
      // Only executed events have actualVwc for trend line
      actualVwc: status === 'executed' ? vwcValue : undefined,
    });
  }

  // P2 - Maintenance (12:00 - 15:30)
  // 5% dryback between shots, each shot restores to field capacity (60%)
  const p2Shots = 6; // Every ~40 min: 12:00, 12:40, 13:20, 14:00, 14:40, 15:20
  
  for (let i = 0; i < p2Shots; i++) {
    const totalMinutes = 12 * 60 + i * 40;
    const hour = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;
    const time = `${hour}:${minutes.toString().padStart(2, '0')}`;
    const eventMinutes = parseTimeToMinutes(time);
    const status = getEventStatus(eventMinutes, currentMinutes);
    const vwcValue = p2Config.fieldCapacity;
    
    // VWC has dried back 5% since last shot, this shot restores to field capacity (60%)
    data.push({
      id: id++, time,
      volume: p2Config.shotVolume,
      endVwc: vwcValue, // Reading after shot = 60% field capacity
      type: 'automated',
      zone: ['A', 'B', 'C', 'D', 'E', 'F'][i % 6],
      period: 'P2 - Maintenance',
      status,
      actualVwc: status === 'executed' ? vwcValue : undefined,
    });
  }

  // P3 - Dryback (16:00 - 19:00) - no shots, just VWC readings as it falls
  // VWC falls from 60% toward ~35% overnight target
  currentVwc = p2Config.fieldCapacity; // Start at 60%
  const p3Readings = 4; // 16:00, 17:00, 18:00, 19:00
  const p3FallPerHour = 6; // ~6% drop per hour
  
  for (let i = 0; i < p3Readings; i++) {
    const hour = 16 + i;
    const time = `${hour}:00`;
    const eventMinutes = parseTimeToMinutes(time);
    const status = getEventStatus(eventMinutes, currentMinutes);
    
    // VWC falls as substrate dries
    if (i > 0) currentVwc -= p3FallPerHour + (Math.random() - 0.5);
    currentVwc = Math.max(currentVwc, 32);
    const vwcValue = Math.round(currentVwc * 10) / 10;
    
    data.push({
      id: id++, time,
      volume: 0, // No shots during dryback
      endVwc: vwcValue,
      type: 'automated',
      zone: ['A', 'B', 'C', 'D', 'E', 'F'][i % 6],
      period: 'P3 - Dryback',
      status,
      actualVwc: status === 'executed' ? vwcValue : undefined,
    });
  }

  return data;
}

// Time required after irrigation before VWC can be accurately measured (minutes)
export const VWC_SOAK_TIME_MINUTES = 10;

/**
 * Create a new manual shot data point to add to the chart.
 * VWC is initially null (pending) because we need ~10 min soak time for accurate reading.
 * Manual shots are always 'executed' status since they're triggered by user action.
 */
export function createManualShot(
  volume: number,
  zones: string[],
  currentPeriod: Exclude<IrrigationPeriod, 'All'>,
  _lastVwc: number, // Kept for future use when we simulate VWC arrival
  nextId: number
): IrrigationDataPoint {
  const now = new Date();
  const time = `${now.getHours()}:${now.getMinutes().toString().padStart(2, '0')}`;
  
  return {
    id: nextId,
    time,
    volume,
    endVwc: null, // VWC not yet available - needs 10 min soak time
    vwcPending: true,
    type: 'manual',
    zone: zones[0] || 'A',
    period: currentPeriod,
    executedAt: now.toISOString(),
    status: 'executed', // Manual shots are immediately executed
    // actualVwc will be set once soak period completes and reading is available
  };
}

/**
 * Estimate what VWC will be after soak period based on shot volume.
 * Used for simulation/preview purposes.
 */
export function estimateVwcAfterShot(lastVwc: number, volume: number): number {
  // Roughly 0.3% increase per 10mL (more realistic than 0.1%)
  const vwcIncrease = (volume / 10) * 0.3;
  return Math.min(Math.round((lastVwc + vwcIncrease) * 10) / 10, 75);
}



