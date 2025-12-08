// src/frontend/lib/simulation-state-store.ts
// In-memory store for simulation state - generates dynamic demo values

// ============================================================================
// TYPES
// ============================================================================

export enum SimulationBehavior {
  SineWave24H = 0,
  InverseSineWave24H = 1,
  RandomWalk = 2,
  StaticWithNoise = 3,
  Sawtooth = 4
}

export interface SimulationProfile {
  behavior: SimulationBehavior;
  min: number;
  max: number;
  noise: number;
  volatility?: number;
}

export interface SimulationState {
  streamId: string;
  stream: {
    id: string;
    displayName: string;
    streamType: number;
    unit: number;
    siteId: string;
    equipmentId: string;
  };
  profile: SimulationProfile;
  lastValue: number;
  isRunning: boolean;
  lastUpdated: number; // timestamp for random walk continuity
}

// Stream type to display name mapping
const STREAM_TYPE_NAMES: Record<number, string> = {
  1: 'Temperature',
  2: 'Humidity',
  3: 'CO₂',
  4: 'VPD',
  5: 'Light PAR',
  6: 'Light PPFD',
  10: 'EC',
  11: 'pH',
  12: 'Dissolved O₂',
  13: 'Water Temp',
  14: 'Water Level',
  20: 'Soil Moisture',
  21: 'Soil Temp',
  22: 'Soil EC',
  30: 'Pressure',
  31: 'Flow Rate',
  32: 'Flow Total',
};

// Default profiles by stream type - calibrated for realistic cultivation values
const DEFAULT_PROFILES: Record<number, SimulationProfile> = {
  // Temperature (type 1) - 72-82°F with daily cycle
  1: { behavior: SimulationBehavior.SineWave24H, min: 72, max: 82, noise: 0.5 },
  // Humidity (type 2) - 50-65% inverse to temp
  2: { behavior: SimulationBehavior.InverseSineWave24H, min: 50, max: 65, noise: 1.5 },
  // CO2 (type 3) - 800-1200 ppm with some walk
  3: { behavior: SimulationBehavior.RandomWalk, min: 900, max: 1150, noise: 25, volatility: 0.05 },
  // VPD (type 4) - 1.0-1.4 kPa following temp
  4: { behavior: SimulationBehavior.SineWave24H, min: 1.0, max: 1.4, noise: 0.05 },
  // Light PAR (type 5)
  5: { behavior: SimulationBehavior.SineWave24H, min: 0, max: 1200, noise: 20 },
  // Light PPFD (type 6) - 800-1100 µmol during day
  6: { behavior: SimulationBehavior.SineWave24H, min: 850, max: 1050, noise: 15 },
  // EC (type 10) - 1.8-2.4 mS/cm
  10: { behavior: SimulationBehavior.StaticWithNoise, min: 1.8, max: 2.4, noise: 0.1 },
  // pH (type 11) - 5.6-6.2
  11: { behavior: SimulationBehavior.StaticWithNoise, min: 5.6, max: 6.2, noise: 0.08 },
  // Dissolved O2 (type 12)
  12: { behavior: SimulationBehavior.StaticWithNoise, min: 6, max: 9, noise: 0.3 },
  // Water Temp (type 13)
  13: { behavior: SimulationBehavior.StaticWithNoise, min: 65, max: 72, noise: 0.5 },
  // Water Level (type 14)
  14: { behavior: SimulationBehavior.Sawtooth, min: 40, max: 100, noise: 2 },
  // Soil Moisture / VWC (type 20) - 45-65%
  20: { behavior: SimulationBehavior.Sawtooth, min: 45, max: 65, noise: 2 },
  // Soil Temp (type 21)
  21: { behavior: SimulationBehavior.StaticWithNoise, min: 68, max: 74, noise: 0.3 },
  // Soil EC (type 22)
  22: { behavior: SimulationBehavior.StaticWithNoise, min: 1.5, max: 2.2, noise: 0.1 },
  // Pressure (type 30) - 40-50 PSI
  30: { behavior: SimulationBehavior.StaticWithNoise, min: 42, max: 48, noise: 1 },
  // Flow Rate (type 31) - 0-5 GPM
  31: { behavior: SimulationBehavior.StaticWithNoise, min: 2, max: 4, noise: 0.3 },
  // Flow Total (type 32)
  32: { behavior: SimulationBehavior.Sawtooth, min: 0, max: 500, noise: 5 },
};

// Default unit by stream type
const DEFAULT_UNITS: Record<number, number> = {
  1: 1,   // Temperature -> Fahrenheit
  2: 10,  // Humidity -> Percent
  3: 20,  // CO2 -> PPM
  4: 30,  // VPD -> kPa
  5: 40,  // Light PAR -> µmol
  6: 40,  // Light PPFD -> µmol
  10: 51, // EC -> mS/cm
  11: 60, // pH
  12: 22, // Dissolved O2 -> mg/L
  13: 1,  // Water Temp -> Fahrenheit
  14: 10, // Water Level -> Percent
  20: 10, // Soil Moisture -> Percent
  21: 1,  // Soil Temp -> Fahrenheit
  22: 51, // Soil EC -> mS/cm
  30: 31, // Pressure -> PSI
  31: 81, // Flow Rate -> GPM
  32: 71, // Flow Total -> Gallons
};

// ============================================================================
// SIMULATION STATE STORE
// ============================================================================

class SimulationStateStore {
  private activeSimulations: Map<number, SimulationState> = new Map(); // keyed by streamType
  private typeProfiles: Map<number, SimulationProfile> = new Map();

  constructor() {
    // Initialize with default profiles
    Object.entries(DEFAULT_PROFILES).forEach(([type, profile]) => {
      this.typeProfiles.set(parseInt(type, 10), profile);
    });
  }

  // ---------------------------------------------------------------------------
  // ACTIVE SIMULATIONS
  // ---------------------------------------------------------------------------

  getActive(): SimulationState[] {
    // Update values before returning
    const now = Date.now();
    for (const state of this.activeSimulations.values()) {
      if (state.isRunning) {
        state.lastValue = this.generateValue(state.profile, state.lastValue, now);
        state.lastUpdated = now;
      }
    }
    return Array.from(this.activeSimulations.values()).filter(s => s.isRunning);
  }

  getByStreamType(streamType: number): SimulationState | undefined {
    return this.activeSimulations.get(streamType);
  }

  // ---------------------------------------------------------------------------
  // TOGGLE / START / STOP
  // ---------------------------------------------------------------------------

  toggle(streamId: string): string {
    // For backwards compatibility, try to parse as stream type
    const streamType = parseInt(streamId, 10);
    if (!isNaN(streamType)) {
      return this.startByType(streamType);
    }
    return `Unknown stream ${streamId}`;
  }

  startByType(streamType: number): string {
    const existing = this.activeSimulations.get(streamType);
    
    if (existing) {
      if (!existing.isRunning) {
        existing.isRunning = true;
        return `Resumed simulation for ${STREAM_TYPE_NAMES[streamType] || `type ${streamType}`}`;
      }
      return `Simulation already running for ${STREAM_TYPE_NAMES[streamType] || `type ${streamType}`}`;
    }

    // Create new simulation for this type
    const profile = this.getProfileForType(streamType);
    const initialValue = this.generateValue(profile, (profile.min + profile.max) / 2, Date.now());
    
    const state: SimulationState = {
      streamId: `sim-${streamType}`,
      stream: {
        id: `sim-${streamType}`,
        displayName: STREAM_TYPE_NAMES[streamType] || `Stream Type ${streamType}`,
        streamType,
        unit: DEFAULT_UNITS[streamType] || 201,
        siteId: 'demo-site',
        equipmentId: 'demo-equipment'
      },
      profile,
      lastValue: initialValue,
      isRunning: true,
      lastUpdated: Date.now()
    };

    this.activeSimulations.set(streamType, state);
    return `Started simulation for ${STREAM_TYPE_NAMES[streamType] || `type ${streamType}`}`;
  }

  stopByType(streamType: number): string {
    const existing = this.activeSimulations.get(streamType);
    if (existing && existing.isRunning) {
      existing.isRunning = false;
      return `Stopped simulation for ${STREAM_TYPE_NAMES[streamType] || `type ${streamType}`}`;
    }
    return `No active simulation for type ${streamType}`;
  }

  // ---------------------------------------------------------------------------
  // PROFILE MANAGEMENT
  // ---------------------------------------------------------------------------

  updateProfile(streamType: number, profile: SimulationProfile): string {
    this.typeProfiles.set(streamType, profile);

    // Update active simulation if exists
    const existing = this.activeSimulations.get(streamType);
    if (existing) {
      existing.profile = profile;
    }

    return `Updated profile for ${STREAM_TYPE_NAMES[streamType] || `type ${streamType}`}`;
  }

  getProfileForType(streamType: number): SimulationProfile {
    return this.typeProfiles.get(streamType) || {
      behavior: SimulationBehavior.StaticWithNoise,
      min: 0,
      max: 100,
      noise: 5
    };
  }

  // ---------------------------------------------------------------------------
  // VALUE GENERATION
  // ---------------------------------------------------------------------------

  private generateValue(profile: SimulationProfile, previousValue: number, timestamp: number): number {
    const range = profile.max - profile.min;
    const midpoint = profile.min + (range / 2);
    
    let baseValue: number;
    
    switch (profile.behavior) {
      case SimulationBehavior.SineWave24H: {
        // 24-hour sine wave - peak at 2pm (14:00), trough at 2am
        const hour = new Date(timestamp).getHours() + new Date(timestamp).getMinutes() / 60;
        const phase = ((hour - 14) / 24) * 2 * Math.PI; // Peak at 14:00
        const sineValue = Math.sin(phase);
        baseValue = midpoint + (sineValue * range / 2);
        break;
      }
      
      case SimulationBehavior.InverseSineWave24H: {
        // Inverse - trough at 2pm, peak at 2am (humidity inverse to temp)
        const hour = new Date(timestamp).getHours() + new Date(timestamp).getMinutes() / 60;
        const phase = ((hour - 14) / 24) * 2 * Math.PI;
        const sineValue = -Math.sin(phase);
        baseValue = midpoint + (sineValue * range / 2);
        break;
      }
      
      case SimulationBehavior.RandomWalk: {
        // Random walk with mean reversion
        const volatility = profile.volatility || 0.1;
        const meanReversion = 0.05;
        const randomStep = (Math.random() - 0.5) * 2 * volatility * range;
        const reversion = (midpoint - previousValue) * meanReversion;
        baseValue = previousValue + randomStep + reversion;
        break;
      }
      
      case SimulationBehavior.Sawtooth: {
        // Gradual increase then sudden drop (like tank levels or irrigation cycles)
        const cycleMinutes = 60; // 1 hour cycle
        const minute = new Date(timestamp).getMinutes();
        const progress = minute / cycleMinutes;
        // Slow rise (90% of cycle), then quick drop
        if (progress < 0.9) {
          baseValue = profile.min + (progress / 0.9) * range;
        } else {
          baseValue = profile.max - ((progress - 0.9) / 0.1) * range * 0.6;
        }
        break;
      }
      
      case SimulationBehavior.StaticWithNoise:
      default: {
        // Stable around midpoint with small random variations
        baseValue = midpoint;
        break;
      }
    }

    // Add noise
    const noise = (Math.random() - 0.5) * profile.noise * 2;
    let finalValue = baseValue + noise;
    
    // Clamp to min/max
    finalValue = Math.max(profile.min, Math.min(profile.max, finalValue));
    
    return finalValue;
  }

  // ---------------------------------------------------------------------------
  // UTILITY
  // ---------------------------------------------------------------------------

  clear(): void {
    this.activeSimulations.clear();
  }
}

// Singleton instance that persists across hot module reloads in development
const globalForSimState = globalThis as unknown as {
  simulationStateStore: SimulationStateStore | undefined;
};

export const simulationStateStore =
  globalForSimState.simulationStateStore ?? new SimulationStateStore();

if (process.env.NODE_ENV !== 'production') {
  globalForSimState.simulationStateStore = simulationStateStore;
}
