// src/frontend/features/telemetry/services/provisioning.service.ts

// ============================================================================
// ENUMS (matching backend)
// ============================================================================

export enum LocationType {
  Room = 'Room',
  Zone = 'Zone',
  SubZone = 'SubZone',
  Row = 'Row',
  Position = 'Position',
  Rack = 'Rack',
  Shelf = 'Shelf',
  Bin = 'Bin'
}

export enum CoreEquipmentType {
  // Controllers
  Controller = 'Controller',
  EcPhController = 'EcPhController',
  
  // Water Quality Sensors
  PhSensor = 'PhSensor',
  EcSensor = 'EcSensor',
  DoSensor = 'DoSensor',
  OrpSensor = 'OrpSensor',
  WaterTempSensor = 'WaterTempSensor',
  
  // Environmental Sensors
  TempHumiditySensor = 'TempHumiditySensor',
  Co2Sensor = 'Co2Sensor',
  LightSensor = 'LightSensor',
  SubstrateSensor = 'SubstrateSensor',
  
  // Flow & Level Sensors
  FlowMeter = 'FlowMeter',
  PressureSensor = 'PressureSensor',
  LevelSensor = 'LevelSensor',
  
  // Actuators & Pumps
  Valve = 'Valve',
  Pump = 'Pump',
  Injector = 'Injector',
  Actuator = 'Actuator',
  
  // Vessels & Tanks
  MixTank = 'MixTank',
  Reservoir = 'Reservoir',
  
  // Legacy/Generic
  Sensor = 'Sensor',
  Meter = 'Meter'
}

// Human-readable labels for equipment types
export const EquipmentTypeLabels: Record<CoreEquipmentType, string> = {
  [CoreEquipmentType.Controller]: 'Controller',
  [CoreEquipmentType.EcPhController]: 'EC/pH Controller',
  [CoreEquipmentType.PhSensor]: 'pH Sensor',
  [CoreEquipmentType.EcSensor]: 'EC Sensor',
  [CoreEquipmentType.DoSensor]: 'Dissolved Oxygen (DO)',
  [CoreEquipmentType.OrpSensor]: 'ORP Sensor',
  [CoreEquipmentType.WaterTempSensor]: 'Water Temp Sensor',
  [CoreEquipmentType.TempHumiditySensor]: 'Temp/Humidity Sensor',
  [CoreEquipmentType.Co2Sensor]: 'CO₂ Sensor',
  [CoreEquipmentType.LightSensor]: 'Light (PAR/PPFD)',
  [CoreEquipmentType.SubstrateSensor]: 'Substrate/VWC Sensor',
  [CoreEquipmentType.FlowMeter]: 'Flow Meter',
  [CoreEquipmentType.PressureSensor]: 'Pressure Sensor',
  [CoreEquipmentType.LevelSensor]: 'Level Sensor',
  [CoreEquipmentType.Valve]: 'Valve',
  [CoreEquipmentType.Pump]: 'Pump',
  [CoreEquipmentType.Injector]: 'Injector/Dosing Pump',
  [CoreEquipmentType.Actuator]: 'Actuator',
  [CoreEquipmentType.MixTank]: 'Mix Tank',
  [CoreEquipmentType.Reservoir]: 'Reservoir',
  [CoreEquipmentType.Sensor]: 'Sensor (Generic)',
  [CoreEquipmentType.Meter]: 'Meter (Generic)'
};

// Grouped equipment types for organized dropdowns
export const EquipmentTypeGroups: Record<string, CoreEquipmentType[]> = {
  'Water Quality': [
    CoreEquipmentType.PhSensor,
    CoreEquipmentType.EcSensor,
    CoreEquipmentType.DoSensor,
    CoreEquipmentType.OrpSensor,
    CoreEquipmentType.WaterTempSensor
  ],
  'Environmental': [
    CoreEquipmentType.TempHumiditySensor,
    CoreEquipmentType.Co2Sensor,
    CoreEquipmentType.LightSensor,
    CoreEquipmentType.SubstrateSensor
  ],
  'Flow & Level': [
    CoreEquipmentType.FlowMeter,
    CoreEquipmentType.PressureSensor,
    CoreEquipmentType.LevelSensor
  ],
  'Actuators': [
    CoreEquipmentType.Valve,
    CoreEquipmentType.Pump,
    CoreEquipmentType.Injector,
    CoreEquipmentType.Actuator
  ],
  'Controllers': [
    CoreEquipmentType.Controller,
    CoreEquipmentType.EcPhController
  ],
  'Vessels': [
    CoreEquipmentType.MixTank,
    CoreEquipmentType.Reservoir
  ]
};

// Sensor placement/installation context
export enum SensorPlacement {
  Inline = 'Inline',
  BatchTank = 'BatchTank',
  Runoff = 'Runoff',
  Reservoir = 'Reservoir',
  Ambient = 'Ambient',
  Substrate = 'Substrate'
}

export const SensorPlacementLabels: Record<SensorPlacement, string> = {
  [SensorPlacement.Inline]: 'In-Line (piping)',
  [SensorPlacement.BatchTank]: 'Batch/Mix Tank',
  [SensorPlacement.Runoff]: 'Runoff/Drain',
  [SensorPlacement.Reservoir]: 'Reservoir',
  [SensorPlacement.Ambient]: 'Ambient/Environmental',
  [SensorPlacement.Substrate]: 'Substrate/Media'
};

export enum EquipmentStatus {
  Active = 'Active',
  Inactive = 'Inactive',
  Maintenance = 'Maintenance',
  Faulty = 'Faulty'
}

// ============================================================================
// REQUEST INTERFACES
// ============================================================================

export interface CreateSiteRequest {
  name: string;
  organizationId?: string;
}

export interface CreateRoomRequest {
  siteId: string;
  name: string;
  type: string;
}

export interface CreateLocationRequest {
  siteId: string;
  roomId?: string;
  parentLocationId?: string;
  locationType: LocationType;
  code: string;
  name: string;
  barcode?: string;
  notes?: string;
}

export interface CreateEquipmentRequest {
  siteId: string;
  locationId: string;
  code: string;
  typeCode: string;
  coreType: CoreEquipmentType;
  manufacturer?: string;
  model?: string;
  serialNumber?: string;
  firmwareVersion?: string;
  notes?: string;
}

export interface CreateSensorStreamRequest {
  siteId: string;
  equipmentId: string;
  equipmentChannelId?: string;
  streamType: number;
  unit: number;
  displayName: string;
  locationId?: string;
  roomId?: string;
  zoneId?: string;
}

// ============================================================================
// RESPONSE INTERFACES
// ============================================================================

export interface Site {
  id: string;
  name: string;
}

export interface Room {
  id: string;
  siteId: string;
  name: string;
  type: string;
}

export interface Location {
  id: string;
  siteId: string;
  roomId?: string;
  parentId?: string;
  locationType: LocationType;
  code: string;
  name: string;
  path: string;
  depth: number;
  children?: Location[];
}

// Alias for backward compatibility
export interface Zone {
  id: string;
  siteId: string;
  roomId: string;
  name: string;
  code: string;
  path: string;
}

export interface Equipment {
  id: string;
  siteId: string;
  locationId?: string;
  code: string;
  typeCode: string;
  coreType: CoreEquipmentType;
  status: EquipmentStatus;
  manufacturer?: string;
  model?: string;
  serialNumber?: string;
  online: boolean;
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
// API BASE URLs
// ============================================================================

// All services use the same base path pattern - the gateway or proxy routes appropriately
// When backend is running: requests are proxied to respective services
// When backend is down: Next.js API routes provide in-memory fallback
const SITES_API = '/api/v1/sites';
const SIMULATION_API = '/api/v1/simulation';

// Helper to get default headers with user ID (for dev purposes)
const getHeaders = (): HeadersInit => ({
  'Content-Type': 'application/json',
  'X-User-Id': 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa' // Dev user placeholder
});

// ============================================================================
// PROVISIONING SERVICE
// ============================================================================

export const provisioningService = {
  
  // ---------------------------------------------------------------------------
  // SITES
  // ---------------------------------------------------------------------------
  
  async getSites(): Promise<Site[]> {
    const res = await fetch(SITES_API, { headers: getHeaders() });
    if (!res.ok) throw new Error('Failed to fetch sites');
    return res.json();
  },

  async createSite(data: CreateSiteRequest): Promise<Site> {
    const res = await fetch(SITES_API, {
      method: 'POST',
      headers: getHeaders(),
      body: JSON.stringify(data)
    });
    if (!res.ok) throw new Error('Failed to create site');
    return res.json();
  },

  // ---------------------------------------------------------------------------
  // ROOMS
  // ---------------------------------------------------------------------------

  async getRooms(siteId?: string): Promise<Room[]> {
    const url = siteId 
      ? `${SITES_API}/${siteId}/rooms`
      : '/api/spatial/v1/rooms';
    const res = await fetch(url);
    if (!res.ok) throw new Error('Failed to fetch rooms');
    return res.json();
  },

  async createRoom(data: CreateRoomRequest): Promise<Room> {
    const res = await fetch(`${SITES_API}/${data.siteId}/rooms`, {
      method: 'POST',
      headers: getHeaders(),
      body: JSON.stringify(data)
    });
    if (!res.ok) throw new Error('Failed to create room');
    return res.json();
  },

  // ---------------------------------------------------------------------------
  // LOCATIONS (Zones, SubZones, etc.)
  // ---------------------------------------------------------------------------

  async getLocations(siteId: string, roomId?: string): Promise<Location[]> {
    // Get root locations for the site, optionally filtered by room
    const url = roomId
      ? `${SITES_API}/${siteId}/locations/${roomId}/children`
      : `${SITES_API}/${siteId}/locations`;
    
    try {
      const res = await fetch(url, { headers: getHeaders() });
      if (!res.ok) {
        console.warn(`Failed to fetch locations: ${res.status}`);
        return [];
      }
      return res.json();
    } catch (error) {
      console.error('Error fetching locations:', error);
      return [];
    }
  },

  async getZones(siteId?: string): Promise<Zone[]> {
    // Fetch locations and filter for zones
    // If no siteId, we can't fetch zones
    if (!siteId) return [];
    
    try {
      const locations = await this.getLocations(siteId);
      // Recursively flatten and filter for Zone type
      const zones: Zone[] = [];
      
      const collectZones = (locs: Location[], parentRoomId?: string) => {
        for (const loc of locs) {
          if (loc.locationType === LocationType.Zone) {
            zones.push({
              id: loc.id,
              siteId: loc.siteId,
              roomId: loc.roomId || parentRoomId || '',
              name: loc.name,
              code: loc.code,
              path: loc.path
            });
          }
          if (loc.children && loc.children.length > 0) {
            collectZones(loc.children, loc.roomId || parentRoomId);
          }
        }
      };
      
      collectZones(locations);
      return zones;
    } catch (error) {
      console.error('Error fetching zones:', error);
      return [];
    }
  },

  async createLocation(data: CreateLocationRequest): Promise<Location> {
    const res = await fetch(`${SITES_API}/${data.siteId}/locations`, {
      method: 'POST',
      headers: getHeaders(),
      body: JSON.stringify({
        siteId: data.siteId,
        roomId: data.roomId,
        parentLocationId: data.parentLocationId,
        locationType: data.locationType,
        code: data.code,
        name: data.name,
        barcode: data.barcode,
        notes: data.notes
      })
    });
    if (!res.ok) {
      const errorText = await res.text();
      throw new Error(`Failed to create location: ${errorText}`);
    }
    return res.json();
  },

  async createZone(data: { 
    siteId: string; 
    roomId: string; 
    name: string; 
    code?: string; 
  }): Promise<Zone> {
    const location = await this.createLocation({
      siteId: data.siteId,
      roomId: data.roomId,
      parentLocationId: data.roomId, // Parent is the room's root location
      locationType: LocationType.Zone,
      code: data.code || data.name.toUpperCase().replace(/\s+/g, '-'),
      name: data.name
    });
    
    return {
      id: location.id,
      siteId: location.siteId,
      roomId: data.roomId,
      name: location.name,
      code: location.code,
      path: location.path
    };
  },

  // ---------------------------------------------------------------------------
  // EQUIPMENT
  // ---------------------------------------------------------------------------

  async getEquipment(siteId: string): Promise<Equipment[]> {
    try {
      const res = await fetch(`${SITES_API}/${siteId}/equipment`, {
        headers: getHeaders()
      });
      if (!res.ok) {
        console.warn(`Failed to fetch equipment: ${res.status}`);
        return [];
      }
      const data = await res.json();
      // API returns { items: Equipment[], ... } for list endpoint
      return data.items || data || [];
    } catch (error) {
      console.error('Error fetching equipment:', error);
      return [];
    }
  },

  async createEquipment(data: CreateEquipmentRequest): Promise<Equipment> {
    const res = await fetch(`${SITES_API}/${data.siteId}/equipment`, {
      method: 'POST',
      headers: getHeaders(),
      body: JSON.stringify({
        siteId: data.siteId,
        locationId: data.locationId,
        code: data.code,
        typeCode: data.typeCode,
        coreType: data.coreType,
        manufacturer: data.manufacturer,
        model: data.model,
        serialNumber: data.serialNumber,
        firmwareVersion: data.firmwareVersion,
        notes: data.notes
      })
    });
    if (!res.ok) {
      const errorText = await res.text();
      throw new Error(`Failed to create equipment: ${errorText}`);
    }
    return res.json();
  },

  // ---------------------------------------------------------------------------
  // SENSOR STREAMS
  // ---------------------------------------------------------------------------

  async getSensorStreams(siteId: string): Promise<SensorStream[]> {
    try {
      const res = await fetch(`${SITES_API}/${siteId}/streams`, {
        headers: getHeaders()
      });
      if (!res.ok) {
        console.warn(`Failed to fetch sensor streams: ${res.status}`);
        return [];
      }
      return res.json();
    } catch (error) {
      console.error('Error fetching sensor streams:', error);
      return [];
    }
  },

  async createSensorStream(data: CreateSensorStreamRequest): Promise<SensorStream> {
    const res = await fetch(`${SITES_API}/${data.siteId}/streams`, {
      method: 'POST',
      headers: getHeaders(),
      body: JSON.stringify({
        equipmentId: data.equipmentId,
        equipmentChannelId: data.equipmentChannelId,
        streamType: data.streamType,
        unit: data.unit,
        displayName: data.displayName,
        locationId: data.locationId,
        roomId: data.roomId,
        zoneId: data.zoneId
      })
    });
    if (!res.ok) {
      const errorText = await res.text();
      throw new Error(`Failed to create sensor stream: ${errorText}`);
    }
    return res.json();
  },

  // Legacy alias for backward compatibility
  async createSensor(data: CreateSensorStreamRequest): Promise<SensorStream> {
    return this.createSensorStream(data);
  },

  // ---------------------------------------------------------------------------
  // TEST ENVIRONMENT HELPER
  // ---------------------------------------------------------------------------

  async createFullTestEnvironment(siteName: string) {
    const site = await this.createSite({ name: siteName });
    const room = await this.createRoom({ 
      siteId: site.id, 
      name: 'Main Grow Room', 
      type: 'Indoor' 
    });
    
    // Create a zone within the room
    const zone = await this.createZone({
      siteId: site.id,
      roomId: room.id,
      name: 'Zone A'
    });

    return { site, room, zones: [zone] };
  }
};
