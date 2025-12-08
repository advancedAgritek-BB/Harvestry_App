// src/frontend/features/telemetry/services/crop-steering-data.service.ts
// Generates consistent crop steering chart data with realistic P1/P2/P3 patterns

export interface CropSteeringDataPoint {
  timestamp: number;
  time: string;
  vwc: number;
  temp: number;
  rh: number;
  vpd: number;
  co2: number;
  ppfd: number;
  phase: 'P1' | 'P2' | 'P3' | 'night';
  irrigationEvent?: boolean;
}

export type TimeScale = '1m' | '15m' | '30m' | '1h' | '4h' | '24h';

interface TimeScaleConfig {
  label: string;
  minutes: number;
  dataPoints: number;
  intervalMs: number;
}

export const TIME_SCALES: Record<TimeScale, TimeScaleConfig> = {
  '1m': { label: '1M', minutes: 1, dataPoints: 60, intervalMs: 1000 },
  '15m': { label: '15M', minutes: 15, dataPoints: 90, intervalMs: 10000 },
  '30m': { label: '30M', minutes: 30, dataPoints: 90, intervalMs: 20000 },
  '1h': { label: '1H', minutes: 60, dataPoints: 60, intervalMs: 60000 },
  '4h': { label: '4H', minutes: 240, dataPoints: 48, intervalMs: 300000 },
  '24h': { label: '24H', minutes: 1440, dataPoints: 96, intervalMs: 900000 },
};

// Seeded random number generator for consistent data
class SeededRandom {
  private seed: number;

  constructor(seed: number) {
    this.seed = seed;
  }

  next(): number {
    // Simple LCG algorithm
    this.seed = (this.seed * 1664525 + 1013904223) % 4294967296;
    return this.seed / 4294967296;
  }

  // Get random value in range with bell curve distribution
  gaussian(min: number, max: number, stdDev: number = 0.15): number {
    // Box-Muller transform for gaussian distribution
    const u1 = this.next();
    const u2 = this.next();
    const z0 = Math.sqrt(-2 * Math.log(u1)) * Math.cos(2 * Math.PI * u2);
    const centered = z0 * stdDev + 0.5; // Center around 0.5
    const clamped = Math.max(0, Math.min(1, centered));
    return min + clamped * (max - min);
  }
}

// Crop steering profile configuration
interface CropSteeringProfile {
  lightsOnHour: number;
  lightsOffHour: number;
  p1Duration: number;      // Minutes for P1 (ramp) phase
  p2Duration: number;      // Minutes for P2 (maintenance) phase
  p1TargetVwc: number;     // Target VWC at end of P1
  p2TargetVwc: number;     // Maintenance VWC target
  p3TargetVwc: number;     // Dryback target (end of P3 / start of next day)
  shotVwcIncrease: number; // VWC increase per irrigation shot
  drybackRatePerHour: number; // Natural VWC decline per hour
}

const DEFAULT_PROFILE: CropSteeringProfile = {
  lightsOnHour: 6,
  lightsOffHour: 18,
  p1Duration: 180,        // 3 hours for ramp
  p2Duration: 360,        // 6 hours for maintenance
  p1TargetVwc: 65,
  p2TargetVwc: 55,
  p3TargetVwc: 40,
  shotVwcIncrease: 3,     // ~3% per shot
  drybackRatePerHour: 2.5,
};

/**
 * Calculate what phase we're in based on time of day
 */
function getPhase(hour: number, profile: CropSteeringProfile): 'P1' | 'P2' | 'P3' | 'night' {
  const { lightsOnHour, lightsOffHour, p1Duration, p2Duration } = profile;
  
  if (hour < lightsOnHour || hour >= lightsOffHour) {
    return 'night';
  }
  
  const minutesSinceLightsOn = (hour - lightsOnHour) * 60;
  
  if (minutesSinceLightsOn < p1Duration) {
    return 'P1';
  } else if (minutesSinceLightsOn < p1Duration + p2Duration) {
    return 'P2';
  } else {
    return 'P3';
  }
}

/**
 * Calculate VWC based on phase and time with crop steering saw-tooth pattern
 */
function calculateVwc(
  timestamp: number,
  profile: CropSteeringProfile,
  rng: SeededRandom
): { vwc: number; irrigationEvent: boolean } {
  const date = new Date(timestamp);
  const hour = date.getHours() + date.getMinutes() / 60;
  const phase = getPhase(hour, profile);
  
  const { lightsOnHour, p1Duration, p2Duration, p1TargetVwc, p2TargetVwc, p3TargetVwc, shotVwcIncrease, drybackRatePerHour } = profile;
  
  let baseVwc: number;
  let irrigationEvent = false;
  const minuteOfHour = date.getMinutes();
  
  switch (phase) {
    case 'night': {
      // Night: gradual dryback from previous day's end
      const hoursIntoNight = hour >= profile.lightsOffHour 
        ? hour - profile.lightsOffHour 
        : hour + (24 - profile.lightsOffHour);
      baseVwc = p3TargetVwc - (hoursIntoNight * drybackRatePerHour * 0.3);
      baseVwc = Math.max(baseVwc, p3TargetVwc - 10);
      break;
    }
    
    case 'P1': {
      // P1 Ramp: Saw-tooth pattern climbing from dryback to target
      const minutesSinceLightsOn = (hour - lightsOnHour) * 60;
      const progressInP1 = minutesSinceLightsOn / p1Duration;
      
      // Number of shots delivered so far (roughly every 30 min during P1)
      const shotInterval = 30; // minutes between shots
      const shotNumber = Math.floor(minutesSinceLightsOn / shotInterval);
      const minutesSinceLastShot = minutesSinceLightsOn % shotInterval;
      
      // Start from dryback target
      const startVwc = p3TargetVwc - 5;
      
      // Each shot adds VWC, but it dries back between shots
      const vwcFromShots = shotNumber * shotVwcIncrease;
      const drybackSinceLastShot = (minutesSinceLastShot / 60) * drybackRatePerHour;
      
      baseVwc = startVwc + vwcFromShots - drybackSinceLastShot;
      baseVwc = Math.min(baseVwc, p1TargetVwc);
      
      // Mark irrigation events (near shot times)
      if (minutesSinceLastShot < 2 && shotNumber > 0) {
        irrigationEvent = true;
      }
      break;
    }
    
    case 'P2': {
      // P2 Maintenance: Saw-tooth around maintenance target
      const minutesSinceP1End = ((hour - lightsOnHour) * 60) - p1Duration;
      
      // Shots triggered when VWC drops below target (roughly every 45-60 min)
      const shotInterval = 50;
      const cyclePosition = minutesSinceP1End % shotInterval;
      
      // Dryback within cycle
      const drybackInCycle = (cyclePosition / 60) * drybackRatePerHour;
      
      // Start each cycle near target, dry back, then shot brings it back up
      if (cyclePosition < 3) {
        // Just after irrigation shot
        baseVwc = p2TargetVwc + shotVwcIncrease - (cyclePosition / 60) * drybackRatePerHour;
        irrigationEvent = cyclePosition < 2;
      } else {
        // Drying back phase
        baseVwc = p2TargetVwc + shotVwcIncrease - drybackInCycle;
      }
      break;
    }
    
    case 'P3': {
      // P3 Dryback: Steady decline toward dryback target
      const minutesSinceP2End = ((hour - lightsOnHour) * 60) - p1Duration - p2Duration;
      const p3TotalMinutes = (profile.lightsOffHour - lightsOnHour) * 60 - p1Duration - p2Duration;
      const progressInP3 = minutesSinceP2End / p3TotalMinutes;
      
      // Linear decline from P2 target to P3 target
      baseVwc = p2TargetVwc - (progressInP3 * (p2TargetVwc - p3TargetVwc));
      break;
    }
  }
  
  // Add small noise
  const noise = rng.gaussian(-0.5, 0.5, 0.3);
  
  return {
    vwc: Math.max(30, Math.min(80, baseVwc + noise)),
    irrigationEvent,
  };
}

/**
 * Calculate environmental values based on time and phase
 */
function calculateEnvironmentals(
  timestamp: number,
  profile: CropSteeringProfile,
  rng: SeededRandom
): { temp: number; rh: number; vpd: number; co2: number; ppfd: number } {
  const date = new Date(timestamp);
  const hour = date.getHours() + date.getMinutes() / 60;
  const phase = getPhase(hour, profile);
  
  const isLightsOn = phase !== 'night';
  
  // Temperature: Higher during lights on
  const baseTemp = isLightsOn ? 78 : 72;
  const temp = baseTemp + rng.gaussian(-1, 1);
  
  // RH: Inverse to temp, higher at night
  const baseRh = isLightsOn ? 55 : 65;
  const rh = baseRh + rng.gaussian(-2, 2);
  
  // VPD: Calculated from temp and RH (simplified)
  const vpd = isLightsOn ? 1.1 + rng.gaussian(-0.1, 0.1) : 0.8 + rng.gaussian(-0.05, 0.05);
  
  // CO2: Enriched during lights on
  const co2 = isLightsOn ? 1000 + rng.gaussian(-30, 30) : 450 + rng.gaussian(-20, 20);
  
  // PPFD: Only during lights on
  const ppfd = isLightsOn ? 900 + rng.gaussian(-30, 30) : 0;
  
  return { temp, rh, vpd, co2, ppfd };
}

/**
 * Format timestamp for display based on time scale
 */
export function formatTimeForScale(date: Date, scale: TimeScale): string {
  if (scale === '1m') {
    return `${date.getMinutes()}:${String(date.getSeconds()).padStart(2, '0')}`;
  } else if (scale === '24h') {
    return `${String(date.getHours()).padStart(2, '0')}:${String(date.getMinutes()).padStart(2, '0')}`;
  } else {
    return `${date.getHours()}:${String(date.getMinutes()).padStart(2, '0')}`;
  }
}

/**
 * Generate a single data point for a specific timestamp
 */
function generateDataPoint(
  timestamp: number,
  scale: TimeScale,
  profile: CropSteeringProfile,
  rng: SeededRandom
): CropSteeringDataPoint {
  const date = new Date(timestamp);
  const hour = date.getHours() + date.getMinutes() / 60;
  const phase = getPhase(hour, profile);
  
  const { vwc, irrigationEvent } = calculateVwc(timestamp, profile, rng);
  const environmentals = calculateEnvironmentals(timestamp, profile, rng);
  
  return {
    timestamp,
    time: formatTimeForScale(date, scale),
    vwc,
    phase,
    irrigationEvent,
    ...environmentals,
  };
}

// Cache for storing high-resolution base data
let baseDataCache: CropSteeringDataPoint[] = [];
let baseCacheTimestamp: number = 0;
const BASE_RESOLUTION_MS = 1000; // 1 second resolution for base data
const BASE_DATA_DURATION_MS = 24 * 60 * 60 * 1000; // 24 hours of data

/**
 * Generate or retrieve base high-resolution data
 * This ensures consistency across all time scales
 */
function getBaseData(profile: CropSteeringProfile = DEFAULT_PROFILE): CropSteeringDataPoint[] {
  const now = Date.now();
  
  // Regenerate if cache is stale (older than 30 seconds) or empty
  if (!baseDataCache.length || now - baseCacheTimestamp > 30000) {
    baseCacheTimestamp = now;
    
    // Use a seed based on the start of the current day for consistency
    const dayStart = new Date();
    dayStart.setHours(0, 0, 0, 0);
    const seed = dayStart.getTime();
    const rng = new SeededRandom(seed);
    
    baseDataCache = [];
    const startTime = now - BASE_DATA_DURATION_MS;
    
    // Generate data at 1-minute resolution for efficiency (upgrade from 1s)
    const resolutionMs = 60000; // 1 minute
    const dataPoints = Math.floor(BASE_DATA_DURATION_MS / resolutionMs);
    
    for (let i = 0; i < dataPoints; i++) {
      const timestamp = startTime + (i * resolutionMs);
      baseDataCache.push(generateDataPoint(timestamp, '1m', profile, rng));
    }
  }
  
  return baseDataCache;
}

/**
 * Aggregate data points by averaging values within time buckets
 */
function aggregateDataPoints(
  data: CropSteeringDataPoint[],
  bucketSizeMs: number,
  scale: TimeScale
): CropSteeringDataPoint[] {
  if (data.length === 0) return [];
  
  const buckets: Map<number, CropSteeringDataPoint[]> = new Map();
  
  // Group data into buckets
  for (const point of data) {
    const bucketKey = Math.floor(point.timestamp / bucketSizeMs) * bucketSizeMs;
    if (!buckets.has(bucketKey)) {
      buckets.set(bucketKey, []);
    }
    buckets.get(bucketKey)!.push(point);
  }
  
  // Aggregate each bucket
  const result: CropSteeringDataPoint[] = [];
  const sortedKeys = Array.from(buckets.keys()).sort((a, b) => a - b);
  
  for (const bucketKey of sortedKeys) {
    const points = buckets.get(bucketKey)!;
    const avgPoint: CropSteeringDataPoint = {
      timestamp: bucketKey,
      time: formatTimeForScale(new Date(bucketKey), scale),
      vwc: points.reduce((sum, p) => sum + p.vwc, 0) / points.length,
      temp: points.reduce((sum, p) => sum + p.temp, 0) / points.length,
      rh: points.reduce((sum, p) => sum + p.rh, 0) / points.length,
      vpd: points.reduce((sum, p) => sum + p.vpd, 0) / points.length,
      co2: points.reduce((sum, p) => sum + p.co2, 0) / points.length,
      ppfd: points.reduce((sum, p) => sum + p.ppfd, 0) / points.length,
      phase: points[Math.floor(points.length / 2)].phase, // Use middle point's phase
      irrigationEvent: points.some(p => p.irrigationEvent),
    };
    result.push(avgPoint);
  }
  
  return result;
}

/**
 * Generate high-resolution data for 1m scale (1-second intervals)
 * This is generated on-demand since it's a short time window
 */
function generate1mData(profile: CropSteeringProfile): CropSteeringDataPoint[] {
  const now = Date.now();
  const config = TIME_SCALES['1m'];
  const intervalMs = config.intervalMs; // 1000ms
  const dataPoints = config.dataPoints; // 60 points
  
  // Use a seed based on the current minute for consistency within that minute
  const minuteStart = Math.floor(now / 60000) * 60000;
  const rng = new SeededRandom(minuteStart);
  
  const result: CropSteeringDataPoint[] = [];
  const startTime = now - (dataPoints * intervalMs);
  
  for (let i = 0; i < dataPoints; i++) {
    const timestamp = startTime + (i * intervalMs);
    result.push(generateDataPoint(timestamp, '1m', profile, rng));
  }
  
  return result;
}

/**
 * Main function: Get crop steering data for a specific time scale
 * Data is consistent across time scales - shorter scales show more detail,
 * longer scales show aggregated/downsampled data
 */
export function getCropSteeringData(
  scale: TimeScale,
  profile: CropSteeringProfile = DEFAULT_PROFILE
): CropSteeringDataPoint[] {
  const config = TIME_SCALES[scale];
  const now = Date.now();
  const windowMs = config.minutes * 60 * 1000;
  const startTime = now - windowMs;
  
  // Special handling for 1m scale - generate at 1-second resolution
  if (scale === '1m') {
    return generate1mData(profile);
  }
  
  // Get base data (1-minute resolution)
  const baseData = getBaseData(profile);
  
  // Filter to time window
  const windowData = baseData.filter(p => p.timestamp >= startTime && p.timestamp <= now);
  
  // If we don't have enough data points, return what we have
  if (windowData.length === 0) {
    // Generate some data for the window
    const rng = new SeededRandom(Math.floor(now / 60000));
    const result: CropSteeringDataPoint[] = [];
    const intervalMs = windowMs / config.dataPoints;
    
    for (let i = 0; i < config.dataPoints; i++) {
      const timestamp = startTime + (i * intervalMs);
      result.push(generateDataPoint(timestamp, scale, profile, rng));
    }
    return result;
  }
  
  // Calculate bucket size for aggregation
  const bucketSizeMs = windowMs / config.dataPoints;
  
  // Aggregate if needed (for longer time scales)
  if (bucketSizeMs > 60000) {
    return aggregateDataPoints(windowData, bucketSizeMs, scale);
  }
  
  // For short time scales, just sample
  const targetCount = config.dataPoints;
  if (windowData.length <= targetCount) {
    return windowData.map(p => ({
      ...p,
      time: formatTimeForScale(new Date(p.timestamp), scale),
    }));
  }
  
  // Downsample evenly
  const result: CropSteeringDataPoint[] = [];
  const step = windowData.length / targetCount;
  for (let i = 0; i < targetCount; i++) {
    const idx = Math.floor(i * step);
    const point = windowData[idx];
    result.push({
      ...point,
      time: formatTimeForScale(new Date(point.timestamp), scale),
    });
  }
  
  return result;
}

/**
 * Clear the cache - useful for testing or forcing data regeneration
 */
export function clearCropSteeringCache(): void {
  baseDataCache = [];
  baseCacheTimestamp = 0;
}

/**
 * Get current VWC value (latest from base data)
 */
export function getCurrentVwc(): number {
  const data = getBaseData();
  return data.length > 0 ? data[data.length - 1].vwc : 50;
}

export { DEFAULT_PROFILE, type CropSteeringProfile };
