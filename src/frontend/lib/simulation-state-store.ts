// src/frontend/lib/simulation-state-store.ts
// In-memory store for simulation state (active simulations, profiles, etc.)

import { simulationDataStore, SensorStream } from './simulation-store';

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
}

// Default profiles by stream type
const DEFAULT_PROFILES: Record<number, SimulationProfile> = {
  // Temperature (type 1)
  1: { behavior: SimulationBehavior.SineWave24H, min: 65, max: 85, noise: 2 },
  // Humidity (type 2)
  2: { behavior: SimulationBehavior.InverseSineWave24H, min: 40, max: 70, noise: 3 },
  // CO2 (type 3)
  3: { behavior: SimulationBehavior.RandomWalk, min: 400, max: 1200, noise: 50, volatility: 0.1 },
  // VPD (type 4)
  4: { behavior: SimulationBehavior.SineWave24H, min: 0.8, max: 1.4, noise: 0.1 },
  // EC (type 10)
  10: { behavior: SimulationBehavior.StaticWithNoise, min: 1.0, max: 2.5, noise: 0.1 },
  // pH (type 11)
  11: { behavior: SimulationBehavior.StaticWithNoise, min: 5.5, max: 6.5, noise: 0.1 },
};

// ============================================================================
// SIMULATION STATE STORE
// ============================================================================

class SimulationStateStore {
  private activeSimulations: Map<string, SimulationState> = new Map();
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
    return Array.from(this.activeSimulations.values()).filter(s => s.isRunning);
  }

  getByStreamId(streamId: string): SimulationState | undefined {
    return this.activeSimulations.get(streamId);
  }

  // ---------------------------------------------------------------------------
  // TOGGLE / START / STOP
  // ---------------------------------------------------------------------------

  toggle(streamId: string): string {
    const existing = this.activeSimulations.get(streamId);
    
    if (existing) {
      existing.isRunning = !existing.isRunning;
      if (existing.isRunning) {
        return `Simulation resumed for stream ${streamId}`;
      } else {
        return `Simulation paused for stream ${streamId}`;
      }
    }

    // Start new simulation for this stream
    const stream = this.findStream(streamId);
    if (!stream) {
      return `Stream ${streamId} not found`;
    }

    const profile = this.getProfileForType(stream.streamType);
    const state: SimulationState = {
      streamId,
      stream: {
        id: stream.id,
        displayName: stream.displayName,
        streamType: stream.streamType,
        unit: stream.unit,
        siteId: stream.siteId,
        equipmentId: stream.equipmentId
      },
      profile,
      lastValue: this.generateValue(profile),
      isRunning: true
    };

    this.activeSimulations.set(streamId, state);
    return `Simulation started for stream ${stream.displayName}`;
  }

  startByType(streamType: number): string {
    // Find all streams of this type and start simulations
    const streams = this.findStreamsByType(streamType);
    if (streams.length === 0) {
      return `No streams found for type ${streamType}`;
    }

    let started = 0;
    for (const stream of streams) {
      if (!this.activeSimulations.has(stream.id)) {
        this.toggle(stream.id);
        started++;
      } else {
        const sim = this.activeSimulations.get(stream.id)!;
        if (!sim.isRunning) {
          sim.isRunning = true;
          started++;
        }
      }
    }

    return `Started ${started} simulation(s) for stream type ${streamType}`;
  }

  stopByType(streamType: number): string {
    let stopped = 0;
    for (const [, state] of this.activeSimulations) {
      if (state.stream.streamType === streamType && state.isRunning) {
        state.isRunning = false;
        stopped++;
      }
    }

    return `Stopped ${stopped} simulation(s) for stream type ${streamType}`;
  }

  // ---------------------------------------------------------------------------
  // PROFILE MANAGEMENT
  // ---------------------------------------------------------------------------

  updateProfile(streamType: number, profile: SimulationProfile): string {
    this.typeProfiles.set(streamType, profile);

    // Update all active simulations of this type
    for (const [, state] of this.activeSimulations) {
      if (state.stream.streamType === streamType) {
        state.profile = profile;
      }
    }

    return `Updated profile for stream type ${streamType}`;
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
  // HELPERS
  // ---------------------------------------------------------------------------

  private findStream(streamId: string): SensorStream | undefined {
    // Search across all sites
    const sites = simulationDataStore.getSites();
    for (const site of sites) {
      const streams = simulationDataStore.getStreams(site.id);
      const found = streams.find(s => s.id === streamId);
      if (found) return found;
    }
    return undefined;
  }

  private findStreamsByType(streamType: number): SensorStream[] {
    const result: SensorStream[] = [];
    const sites = simulationDataStore.getSites();
    for (const site of sites) {
      const streams = simulationDataStore.getStreams(site.id);
      result.push(...streams.filter(s => s.streamType === streamType));
    }
    return result;
  }

  private generateValue(profile: SimulationProfile): number {
    const range = profile.max - profile.min;
    const base = profile.min + (range / 2);
    const noise = (Math.random() - 0.5) * profile.noise * 2;
    return base + noise;
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

