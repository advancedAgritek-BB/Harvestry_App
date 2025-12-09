/**
 * Inventory Location Type Definitions
 * Types for hierarchical location management
 */

/** Location type in hierarchical system */
export type LocationType =
  | 'room'
  | 'zone'
  | 'sub_zone'
  | 'row'
  | 'position'
  | 'rack'
  | 'shelf'
  | 'bin'
  | 'vault';

/** Location status */
export type LocationStatus =
  | 'active'
  | 'inactive'
  | 'full'
  | 'reserved'
  | 'quarantine';

/** Room type classification */
export type RoomType =
  | 'veg'
  | 'flower'
  | 'mother'
  | 'clone'
  | 'dry'
  | 'cure'
  | 'extraction'
  | 'manufacturing'
  | 'vault'
  | 'warehouse'
  | 'staging'
  | 'custom';

/** Core inventory location entity */
export interface InventoryLocation {
  id: string;
  siteId: string;
  
  // Hierarchy
  parentId?: string;
  roomId?: string;
  locationType: LocationType;
  
  // Identification
  code: string;
  name: string;
  barcode?: string;
  
  // Path (materialized for fast queries)
  path: string;
  depth: number;
  
  // Status
  status: LocationStatus;
  
  // Dimensions
  lengthFt?: number;
  widthFt?: number;
  heightFt?: number;
  
  // Cultivation specific
  plantCapacity?: number;
  currentPlantCount?: number;
  
  // Matrix coordinates
  rowNumber?: number;
  columnNumber?: number;
  
  // Warehouse specific
  weightCapacityLbs?: number;
  currentWeightLbs?: number;
  
  // Metadata
  notes?: string;
  metadata?: Record<string, unknown>;
  
  // Audit
  createdAt: string;
  createdBy: string;
  updatedAt: string;
  updatedBy: string;
  
  // Nested children (for tree views)
  children?: InventoryLocation[];
  lotCount?: number;
  
  // Utilization
  utilizationPercent?: number;
}

/** Room entity (top-level container) */
export interface Room {
  id: string;
  siteId: string;
  name: string;
  code: string;
  roomType: RoomType;
  status: LocationStatus;
  
  // Capacity
  plantCapacity?: number;
  currentPlantCount?: number;
  
  // Environment (for cultivation rooms)
  targetTemp?: number;
  targetHumidity?: number;
  
  // Metadata
  notes?: string;
  
  // Audit
  createdAt: string;
  updatedAt: string;
}

/** Location tree node for UI rendering */
export interface LocationTreeNode {
  id: string;
  name: string;
  code: string;
  locationType: LocationType;
  status: LocationStatus;
  path: string;
  depth: number;
  
  // Capacity info
  capacityPercent?: number;
  lotCount: number;
  
  // Visual state
  isExpanded?: boolean;
  isSelected?: boolean;
  isLoading?: boolean;
  
  children: LocationTreeNode[];
}

/** Location capacity summary */
export interface LocationCapacity {
  locationId: string;
  locationType: LocationType;
  
  // Plant capacity (cultivation)
  plantCapacity?: number;
  currentPlants?: number;
  plantUtilization?: number;
  
  // Weight capacity (warehouse)
  weightCapacityLbs?: number;
  currentWeightLbs?: number;
  weightUtilization?: number;
  
  // Lot count
  lotCount: number;
}

/** Create location request */
export interface CreateLocationRequest {
  parentId?: string;
  roomId?: string;
  locationType: LocationType;
  code: string;
  name: string;
  barcode?: string;
  plantCapacity?: number;
  weightCapacityLbs?: number;
  notes?: string;
}

/** Alias for backward compatibility */
export type LocationCreateRequest = CreateLocationRequest;

/** Update location request */
export interface LocationUpdateRequest extends Partial<CreateLocationRequest> {
  id: string;
}

/** Location filter options */
export interface LocationFilterOptions {
  siteId?: string;
  parentId?: string;
  roomId?: string;
  locationType?: LocationType[];
  status?: LocationStatus[];
  hasCapacity?: boolean;
  minUtilization?: number;
  maxUtilization?: number;
  search?: string;
}

/** Alias for backward compatibility */
export type LocationFilters = LocationFilterOptions;

/** Location summary for dashboard */
export interface LocationSummary {
  totalLocations: number;
  byType: Record<LocationType, number>;
  byStatus: Record<LocationStatus, number>;
  totalCapacity: number;
  usedCapacity: number;
  utilizationPercent: number;
  quarantineCount: number;
  fullCount: number;
}

/** Location with contained lots */
export interface LocationWithLots extends InventoryLocation {
  lots: {
    id: string;
    lotNumber: string;
    productType: string;
    quantity: number;
    uom: string;
    status: string;
  }[];
}
