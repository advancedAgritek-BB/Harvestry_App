// src/frontend/lib/simulation-store.ts
// In-memory data store for simulation/test environment provisioning

// Simple UUID v4 generator (avoids external dependency)
function generateId(): string {
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
    const r = (Math.random() * 16) | 0;
    const v = c === 'x' ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
}

// ============================================================================
// TYPES
// ============================================================================

export interface Site {
  id: string;
  name: string;
  organizationId?: string;
  createdAt: string;
}

export interface Room {
  id: string;
  siteId: string;
  name: string;
  type: string;
  createdAt: string;
}

export interface Location {
  id: string;
  siteId: string;
  roomId?: string;
  parentLocationId?: string;
  locationType: string;
  code: string;
  name: string;
  path: string;
  depth: number;
  createdAt: string;
}

export interface Equipment {
  id: string;
  siteId: string;
  locationId?: string;
  code: string;
  typeCode: string;
  coreType: string;
  status: string;
  manufacturer?: string;
  model?: string;
  serialNumber?: string;
  firmwareVersion?: string;
  online: boolean;
  createdAt: string;
}

export interface SensorStream {
  id: string;
  siteId: string;
  equipmentId: string;
  equipmentChannelId?: string;
  streamType: number;
  unit: number;
  displayName: string;
  locationId?: string;
  roomId?: string;
  zoneId?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

// ============================================================================
// IN-MEMORY STORAGE
// ============================================================================

class SimulationDataStore {
  private sites: Map<string, Site> = new Map();
  private rooms: Map<string, Room> = new Map();
  private locations: Map<string, Location> = new Map();
  private equipment: Map<string, Equipment> = new Map();
  private streams: Map<string, SensorStream> = new Map();

  // ---------------------------------------------------------------------------
  // SITES
  // ---------------------------------------------------------------------------

  getSites(): Site[] {
    return Array.from(this.sites.values());
  }

  getSiteById(id: string): Site | undefined {
    return this.sites.get(id);
  }

  createSite(data: { name: string; organizationId?: string }): Site {
    const site: Site = {
      id: generateId(),
      name: data.name,
      organizationId: data.organizationId,
      createdAt: new Date().toISOString()
    };
    this.sites.set(site.id, site);
    return site;
  }

  // ---------------------------------------------------------------------------
  // ROOMS
  // ---------------------------------------------------------------------------

  getRooms(siteId?: string): Room[] {
    const allRooms = Array.from(this.rooms.values());
    if (siteId) {
      return allRooms.filter(r => r.siteId === siteId);
    }
    return allRooms;
  }

  getRoomById(id: string): Room | undefined {
    return this.rooms.get(id);
  }

  createRoom(data: { siteId: string; name: string; type: string }): Room {
    const room: Room = {
      id: generateId(),
      siteId: data.siteId,
      name: data.name,
      type: data.type,
      createdAt: new Date().toISOString()
    };
    this.rooms.set(room.id, room);
    return room;
  }

  // ---------------------------------------------------------------------------
  // LOCATIONS (Zones, etc.)
  // ---------------------------------------------------------------------------

  getLocations(siteId: string, roomId?: string): Location[] {
    const allLocations = Array.from(this.locations.values());
    return allLocations.filter(loc => {
      if (loc.siteId !== siteId) return false;
      if (roomId && loc.roomId !== roomId) return false;
      return true;
    });
  }

  getLocationById(id: string): Location | undefined {
    return this.locations.get(id);
  }

  createLocation(data: {
    siteId: string;
    roomId?: string;
    parentLocationId?: string;
    locationType: string;
    code: string;
    name: string;
  }): Location {
    // Build path based on parent
    let path = data.name;
    let depth = 0;
    
    if (data.parentLocationId) {
      const parent = this.locations.get(data.parentLocationId);
      if (parent) {
        path = `${parent.path} > ${data.name}`;
        depth = parent.depth + 1;
      }
    } else if (data.roomId) {
      const room = this.rooms.get(data.roomId);
      if (room) {
        path = `${room.name} > ${data.name}`;
        depth = 1;
      }
    }

    const location: Location = {
      id: generateId(),
      siteId: data.siteId,
      roomId: data.roomId,
      parentLocationId: data.parentLocationId,
      locationType: data.locationType,
      code: data.code,
      name: data.name,
      path,
      depth,
      createdAt: new Date().toISOString()
    };
    this.locations.set(location.id, location);
    return location;
  }

  // ---------------------------------------------------------------------------
  // EQUIPMENT
  // ---------------------------------------------------------------------------

  getEquipment(siteId: string): Equipment[] {
    return Array.from(this.equipment.values()).filter(e => e.siteId === siteId);
  }

  getEquipmentById(id: string): Equipment | undefined {
    return this.equipment.get(id);
  }

  createEquipment(data: {
    siteId: string;
    locationId?: string;
    code: string;
    typeCode: string;
    coreType: string;
    manufacturer?: string;
    model?: string;
    serialNumber?: string;
    firmwareVersion?: string;
  }): Equipment {
    const equip: Equipment = {
      id: generateId(),
      siteId: data.siteId,
      locationId: data.locationId,
      code: data.code,
      typeCode: data.typeCode,
      coreType: data.coreType,
      status: 'Active',
      manufacturer: data.manufacturer,
      model: data.model,
      serialNumber: data.serialNumber,
      firmwareVersion: data.firmwareVersion,
      online: true,
      createdAt: new Date().toISOString()
    };
    this.equipment.set(equip.id, equip);
    return equip;
  }

  // ---------------------------------------------------------------------------
  // SENSOR STREAMS
  // ---------------------------------------------------------------------------

  getStreams(siteId: string): SensorStream[] {
    return Array.from(this.streams.values()).filter(s => s.siteId === siteId);
  }

  getStreamById(id: string): SensorStream | undefined {
    return this.streams.get(id);
  }

  createStream(data: {
    siteId: string;
    equipmentId: string;
    equipmentChannelId?: string;
    streamType: number;
    unit: number;
    displayName: string;
    locationId?: string;
    roomId?: string;
    zoneId?: string;
  }): SensorStream {
    const now = new Date().toISOString();
    const stream: SensorStream = {
      id: generateId(),
      siteId: data.siteId,
      equipmentId: data.equipmentId,
      equipmentChannelId: data.equipmentChannelId,
      streamType: data.streamType,
      unit: data.unit,
      displayName: data.displayName,
      locationId: data.locationId,
      roomId: data.roomId,
      zoneId: data.zoneId,
      isActive: true,
      createdAt: now,
      updatedAt: now
    };
    this.streams.set(stream.id, stream);
    return stream;
  }

  // ---------------------------------------------------------------------------
  // UTILITY
  // ---------------------------------------------------------------------------

  clear(): void {
    this.sites.clear();
    this.rooms.clear();
    this.locations.clear();
    this.equipment.clear();
    this.streams.clear();
  }
}

// Singleton instance that persists across hot module reloads in development
// In Next.js dev mode, modules can be re-evaluated, losing in-memory state
// Using globalThis ensures the store persists
const globalForSimulation = globalThis as unknown as {
  simulationDataStore: SimulationDataStore | undefined;
};

export const simulationDataStore =
  globalForSimulation.simulationDataStore ?? new SimulationDataStore();

if (process.env.NODE_ENV !== 'production') {
  globalForSimulation.simulationDataStore = simulationDataStore;
}

