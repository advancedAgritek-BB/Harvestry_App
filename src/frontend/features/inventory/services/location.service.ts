/**
 * Location Service
 * Location hierarchy management, capacity tracking, and spatial operations
 */

import type {
  InventoryLocation,
  LocationTreeNode,
  LocationFilters,
  LocationCapacity,
  LocationSummary,
  LocationCreateRequest,
  LocationUpdateRequest,
  LocationWithLots,
  ApiResponse,
} from '../types';

const API_BASE = '/api/v1/inventory/locations';

/**
 * Generic fetch wrapper
 */
async function fetchApi<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<ApiResponse<T>> {
  try {
    const response = await fetch(`${API_BASE}${endpoint}`, {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        ...options.headers,
      },
    });
    
    const data = await response.json();
    
    if (!response.ok) {
      return {
        success: false,
        error: {
          code: data.code || 'UNKNOWN_ERROR',
          message: data.message || 'An error occurred',
          details: data.details,
          timestamp: new Date().toISOString(),
        },
      };
    }
    
    return { success: true, data };
  } catch (error) {
    return {
      success: false,
      error: {
        code: 'NETWORK_ERROR',
        message: error instanceof Error ? error.message : 'Network error',
        timestamp: new Date().toISOString(),
      },
    };
  }
}

/**
 * Location Service API
 */
export const LocationService = {
  // === Location CRUD ===
  
  /**
   * Get locations with filters
   */
  async getLocations(
    filters: LocationFilters = {},
    page = 1,
    pageSize = 100
  ): Promise<ApiResponse<{ locations: InventoryLocation[]; total: number }>> {
    const params = new URLSearchParams({
      page: String(page),
      pageSize: String(pageSize),
    });
    
    if (filters.search) params.set('search', filters.search);
    if (filters.locationType?.length) params.set('locationType', filters.locationType.join(','));
    if (filters.status?.length) params.set('status', filters.status.join(','));
    if (filters.parentId) params.set('parentId', filters.parentId);
    if (filters.roomId) params.set('roomId', filters.roomId);
    if (filters.minUtilization !== undefined) params.set('minUtilization', String(filters.minUtilization));
    if (filters.maxUtilization !== undefined) params.set('maxUtilization', String(filters.maxUtilization));
    if (filters.hasCapacity !== undefined) params.set('hasCapacity', String(filters.hasCapacity));
    
    return fetchApi(`?${params}`);
  },
  
  /**
   * Get location tree (hierarchical structure)
   */
  async getLocationTree(
    siteId?: string,
    rootId?: string
  ): Promise<ApiResponse<LocationTreeNode[]>> {
    const params = new URLSearchParams();
    if (siteId) params.set('siteId', siteId);
    if (rootId) params.set('rootId', rootId);
    
    return fetchApi<LocationTreeNode[]>(`/tree?${params}`);
  },
  
  /**
   * Get a single location
   */
  async getLocation(locationId: string): Promise<ApiResponse<InventoryLocation>> {
    return fetchApi<InventoryLocation>(`/${locationId}`);
  },
  
  /**
   * Get location with all contained lots
   */
  async getLocationWithLots(locationId: string): Promise<ApiResponse<LocationWithLots>> {
    return fetchApi<LocationWithLots>(`/${locationId}/lots`);
  },
  
  /**
   * Create a new location
   */
  async createLocation(request: LocationCreateRequest): Promise<ApiResponse<InventoryLocation>> {
    return fetchApi<InventoryLocation>('', {
      method: 'POST',
      body: JSON.stringify(request),
    });
  },
  
  /**
   * Update a location
   */
  async updateLocation(
    locationId: string,
    updates: LocationUpdateRequest
  ): Promise<ApiResponse<InventoryLocation>> {
    return fetchApi<InventoryLocation>(`/${locationId}`, {
      method: 'PATCH',
      body: JSON.stringify(updates),
    });
  },
  
  /**
   * Delete a location (must be empty)
   */
  async deleteLocation(locationId: string): Promise<ApiResponse<void>> {
    return fetchApi(`/${locationId}`, { method: 'DELETE' });
  },
  
  // === Capacity ===
  
  /**
   * Get capacity summary for locations
   */
  async getCapacity(
    locationIds?: string[]
  ): Promise<ApiResponse<LocationCapacity[]>> {
    const params = locationIds?.length 
      ? `?locationIds=${locationIds.join(',')}` 
      : '';
    return fetchApi<LocationCapacity[]>(`/capacity${params}`);
  },
  
  /**
   * Get location summary statistics
   */
  async getSummary(siteId?: string): Promise<ApiResponse<LocationSummary>> {
    const params = siteId ? `?siteId=${siteId}` : '';
    return fetchApi<LocationSummary>(`/summary${params}`);
  },
  
  // === Barcode/QR ===
  
  /**
   * Generate location barcode/QR code
   */
  async generateLocationCode(
    locationId: string,
    format: 'barcode' | 'qr' = 'qr'
  ): Promise<ApiResponse<{ imageUrl: string; code: string }>> {
    return fetchApi(`/${locationId}/code?format=${format}`, { method: 'POST' });
  },
  
  /**
   * Lookup location by barcode
   */
  async lookupByBarcode(barcode: string): Promise<ApiResponse<InventoryLocation>> {
    return fetchApi<InventoryLocation>(`/lookup?barcode=${encodeURIComponent(barcode)}`);
  },
  
  // === Hierarchy Operations ===
  
  /**
   * Move a location to a new parent
   */
  async moveLocation(
    locationId: string,
    newParentId: string
  ): Promise<ApiResponse<InventoryLocation>> {
    return fetchApi<InventoryLocation>(`/${locationId}/move`, {
      method: 'POST',
      body: JSON.stringify({ newParentId }),
    });
  },
  
  /**
   * Get location ancestors (path to root)
   */
  async getAncestors(locationId: string): Promise<ApiResponse<InventoryLocation[]>> {
    return fetchApi<InventoryLocation[]>(`/${locationId}/ancestors`);
  },
  
  /**
   * Get location descendants (all children recursively)
   */
  async getDescendants(locationId: string): Promise<ApiResponse<InventoryLocation[]>> {
    return fetchApi<InventoryLocation[]>(`/${locationId}/descendants`);
  },
  
  // === Batch Operations ===
  
  /**
   * Create multiple locations at once
   */
  async createBatch(
    locations: LocationCreateRequest[]
  ): Promise<ApiResponse<{ created: InventoryLocation[]; errors: { index: number; message: string }[] }>> {
    return fetchApi('/batch', {
      method: 'POST',
      body: JSON.stringify({ locations }),
    });
  },
  
  /**
   * Update status for multiple locations
   */
  async updateStatusBatch(
    locationIds: string[],
    status: InventoryLocation['status']
  ): Promise<ApiResponse<{ updated: number }>> {
    return fetchApi('/batch/status', {
      method: 'PATCH',
      body: JSON.stringify({ locationIds, status }),
    });
  },
};

export default LocationService;

