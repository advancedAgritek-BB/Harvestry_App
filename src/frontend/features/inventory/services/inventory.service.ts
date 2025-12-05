/**
 * Inventory Service
 * CRUD operations for lots, movements, and adjustments
 */

import type {
  InventoryLot,
  CreateLotRequest,
  SplitLotRequest,
  SplitLotResponse,
  MergeLotRequest,
  LotAdjustmentRequest,
  LotFilterOptions,
  LotListResponse,
  LotSummary,
  InventoryMovement,
  CreateMovementRequest,
  BatchMovementRequest,
  MovementFilterOptions,
  MovementListResponse,
  MovementSummary,
  InventoryLocation,
  LocationFilterOptions,
  LocationSummary,
  LocationTreeNode,
} from '../types';

const API_BASE = '/api/inventory';

/**
 * Lot Operations
 */
export async function getLots(
  filters: LotFilterOptions = {},
  page = 1,
  pageSize = 50
): Promise<LotListResponse> {
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString(),
    ...Object.fromEntries(
      Object.entries(filters)
        .filter(([, v]) => v !== undefined)
        .map(([k, v]) => [k, Array.isArray(v) ? v.join(',') : String(v)])
    ),
  });

  const response = await fetch(`${API_BASE}/lots?${params}`);
  if (!response.ok) throw new Error('Failed to fetch lots');
  return response.json();
}

export async function getLot(lotId: string): Promise<InventoryLot> {
  const response = await fetch(`${API_BASE}/lots/${lotId}`);
  if (!response.ok) throw new Error('Failed to fetch lot');
  return response.json();
}

export async function createLot(request: CreateLotRequest): Promise<InventoryLot> {
  const response = await fetch(`${API_BASE}/lots`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to create lot');
  return response.json();
}

export async function updateLot(
  lotId: string,
  updates: Partial<InventoryLot>
): Promise<InventoryLot> {
  const response = await fetch(`${API_BASE}/lots/${lotId}`, {
    method: 'PATCH',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(updates),
  });
  if (!response.ok) throw new Error('Failed to update lot');
  return response.json();
}

export async function splitLot(request: SplitLotRequest): Promise<SplitLotResponse> {
  const response = await fetch(`${API_BASE}/lots/split`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to split lot');
  return response.json();
}

export async function mergeLots(request: MergeLotRequest): Promise<InventoryLot> {
  const response = await fetch(`${API_BASE}/lots/merge`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to merge lots');
  return response.json();
}

export async function adjustLot(request: LotAdjustmentRequest): Promise<InventoryLot> {
  const response = await fetch(`${API_BASE}/lots/${request.lotId}/adjust`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to adjust lot');
  return response.json();
}

export async function getLotSummary(siteId?: string): Promise<LotSummary> {
  const params = siteId ? `?siteId=${siteId}` : '';
  const response = await fetch(`${API_BASE}/lots/summary${params}`);
  if (!response.ok) throw new Error('Failed to fetch lot summary');
  return response.json();
}

/**
 * Movement Operations
 */
export async function getMovements(
  filters: MovementFilterOptions = {},
  page = 1,
  pageSize = 50
): Promise<MovementListResponse> {
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString(),
    ...Object.fromEntries(
      Object.entries(filters)
        .filter(([, v]) => v !== undefined)
        .map(([k, v]) => [k, Array.isArray(v) ? v.join(',') : String(v)])
    ),
  });

  const response = await fetch(`${API_BASE}/movements?${params}`);
  if (!response.ok) throw new Error('Failed to fetch movements');
  return response.json();
}

export async function getMovement(movementId: string): Promise<InventoryMovement> {
  const response = await fetch(`${API_BASE}/movements/${movementId}`);
  if (!response.ok) throw new Error('Failed to fetch movement');
  return response.json();
}

export async function createMovement(
  request: CreateMovementRequest
): Promise<InventoryMovement> {
  const response = await fetch(`${API_BASE}/movements`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to create movement');
  return response.json();
}

export async function createBatchMovement(
  request: BatchMovementRequest
): Promise<InventoryMovement[]> {
  const response = await fetch(`${API_BASE}/movements/batch`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to create batch movement');
  return response.json();
}

export async function cancelMovement(movementId: string): Promise<void> {
  const response = await fetch(`${API_BASE}/movements/${movementId}/cancel`, {
    method: 'POST',
  });
  if (!response.ok) throw new Error('Failed to cancel movement');
}

export async function getMovementSummary(siteId?: string): Promise<MovementSummary> {
  const params = siteId ? `?siteId=${siteId}` : '';
  const response = await fetch(`${API_BASE}/movements/summary${params}`);
  if (!response.ok) throw new Error('Failed to fetch movement summary');
  return response.json();
}

/**
 * Location Operations
 */
export async function getLocations(
  filters: LocationFilterOptions = {}
): Promise<InventoryLocation[]> {
  const params = new URLSearchParams(
    Object.fromEntries(
      Object.entries(filters)
        .filter(([, v]) => v !== undefined)
        .map(([k, v]) => [k, Array.isArray(v) ? v.join(',') : String(v)])
    )
  );

  const response = await fetch(`${API_BASE}/locations?${params}`);
  if (!response.ok) throw new Error('Failed to fetch locations');
  return response.json();
}

export async function getLocation(locationId: string): Promise<InventoryLocation> {
  const response = await fetch(`${API_BASE}/locations/${locationId}`);
  if (!response.ok) throw new Error('Failed to fetch location');
  return response.json();
}

export async function getLocationTree(
  siteId: string,
  rootId?: string
): Promise<LocationTreeNode[]> {
  const params = rootId ? `?rootId=${rootId}` : '';
  const response = await fetch(`${API_BASE}/locations/${siteId}/tree${params}`);
  if (!response.ok) throw new Error('Failed to fetch location tree');
  return response.json();
}

export async function getLocationSummary(siteId?: string): Promise<LocationSummary> {
  const params = siteId ? `?siteId=${siteId}` : '';
  const response = await fetch(`${API_BASE}/locations/summary${params}`);
  if (!response.ok) throw new Error('Failed to fetch location summary');
  return response.json();
}

/**
 * Balance Operations
 */
export async function getBalancesByLocation(
  locationId: string
): Promise<{ lotId: string; quantity: number; uom: string }[]> {
  const response = await fetch(`${API_BASE}/balances?locationId=${locationId}`);
  if (!response.ok) throw new Error('Failed to fetch balances');
  return response.json();
}

export async function reconcileBalances(
  locationId: string
): Promise<{ discrepancies: { lotId: string; expected: number; actual: number }[] }> {
  const response = await fetch(`${API_BASE}/balances/${locationId}/reconcile`, {
    method: 'POST',
  });
  if (!response.ok) throw new Error('Failed to reconcile balances');
  return response.json();
}
