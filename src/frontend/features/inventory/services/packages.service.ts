/**
 * Packages Service
 * Connects to the real Packages API endpoints
 */

import type {
  LotFilterOptions,
} from '../types';

const getApiBase = (siteId: string) => `/api/v1/sites/${siteId}/packages`;

// Types matching backend DTOs
export interface PackageDto {
  id: string;
  siteId: string;
  packageLabel: string;
  itemId: string;
  itemName: string;
  itemCategory: string;
  quantity: number;
  initialQuantity: number;
  unitOfMeasure: string;
  locationId?: string;
  locationName?: string;
  sublocationName?: string;
  sourceHarvestId?: string;
  sourceHarvestName?: string;
  sourcePackageLabels: string[];
  productionBatchNumber?: string;
  isProductionBatch: boolean;
  isTradeSample: boolean;
  isDonation: boolean;
  productRequiresRemediation: boolean;
  packagedDate: string;
  expirationDate?: string;
  useByDate?: string;
  finishedDate?: string;
  labTestingState: string;
  labTestingStateRequired: boolean;
  thcPercent?: number;
  cbdPercent?: number;
  status: string;
  packageType: string;
  notes?: string;
  metrcPackageId?: number;
  metrcLastSyncAt?: string;
  metrcSyncStatus?: string;
  unitCost?: number;
  materialCost: number;
  laborCost: number;
  overheadCost: number;
  totalCost: number;
  totalValue: number;
  reservedQuantity: number;
  availableQuantity: number;
  inventoryCategory: string;
  holdReasonCode?: string;
  holdPlacedAt?: string;
  holdPlacedByUserId?: string;
  holdReleasedAt?: string;
  requiresTwoPersonRelease: boolean;
  vendorId?: string;
  vendorName?: string;
  vendorLotNumber?: string;
  purchaseOrderId?: string;
  purchaseOrderNumber?: string;
  receivedDate?: string;
  grade?: string;
  qualityScore?: number;
  qualityNotes?: string;
  generationDepth: number;
  rootAncestorId?: string;
  createdAt: string;
  createdByUserId: string;
  updatedAt: string;
  updatedByUserId: string;
}

export interface PackageSummaryDto {
  id: string;
  packageLabel: string;
  itemName: string;
  itemCategory: string;
  quantity: number;
  availableQuantity: number;
  unitOfMeasure: string;
  locationName?: string;
  status: string;
  labTestingState: string;
  packagedDate: string;
  expirationDate?: string;
  unitCost?: number;
  totalValue: number;
  inventoryCategory: string;
  grade?: string;
  holdReasonCode?: string;
  metrcPackageId?: number;
  metrcSyncStatus?: string;
}

export interface PackageListResponse {
  packages: PackageSummaryDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface CreatePackageRequest {
  packageLabel: string;
  itemId: string;
  itemName: string;
  itemCategory?: string;
  quantity: number;
  unitOfMeasure: string;
  locationId?: string;
  locationName?: string;
  sublocationName?: string;
  sourceHarvestId?: string;
  sourceHarvestName?: string;
  sourcePackageLabels?: string[];
  packagedDate?: string;
  expirationDate?: string;
  inventoryCategory?: string;
  unitCost?: number;
  materialCost?: number;
  laborCost?: number;
  overheadCost?: number;
  vendorId?: string;
  vendorName?: string;
  vendorLotNumber?: string;
  receivedDate?: string;
  grade?: string;
  qualityScore?: number;
  notes?: string;
}

export interface AdjustPackageRequest {
  adjustmentQuantity: number;
  reason: string;
  adjustmentDate?: string;
  reasonNote?: string;
  requiresApproval?: boolean;
}

export interface MovePackageRequest {
  toLocationId: string;
  toLocationPath?: string;
  sublocationName?: string;
  notes?: string;
  barcodeScanned?: string;
}

export interface PackageSummaryStats {
  totalPackages: number;
  activePackages: number;
  onHoldPackages: number;
  finishedPackages: number;
  totalQuantity: number;
  totalValue: number;
  expiringInWeek: number;
  expiringInMonth: number;
  pendingLabTest: number;
  failedLabTest: number;
}

/**
 * Get packages with filtering and pagination
 */
export async function getPackages(
  siteId: string,
  filters: LotFilterOptions = {},
  page = 1,
  pageSize = 50
): Promise<PackageListResponse> {
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString(),
  });

  if (filters.status?.length) params.append('status', filters.status.join(','));
  if (filters.labTestingState) params.append('labTestingState', String(filters.labTestingState));
  if (filters.inventoryCategory) params.append('inventoryCategory', String(filters.inventoryCategory));
  if (filters.locationId) params.append('locationId', filters.locationId);
  if (filters.search) params.append('search', filters.search);
  if (filters.onHold !== undefined) params.append('onHold', String(filters.onHold));
  if (filters.expiringSoon !== undefined) params.append('expiringSoon', String(filters.expiringSoon));

  const response = await fetch(`${getApiBase(siteId)}?${params}`);
  if (!response.ok) throw new Error('Failed to fetch packages');
  return response.json();
}

/**
 * Get a single package by ID
 */
export async function getPackage(siteId: string, packageId: string): Promise<PackageDto> {
  const response = await fetch(`${getApiBase(siteId)}/${packageId}`);
  if (!response.ok) throw new Error('Failed to fetch package');
  return response.json();
}

/**
 * Get a package by label
 */
export async function getPackageByLabel(siteId: string, label: string): Promise<PackageDto> {
  const response = await fetch(`${getApiBase(siteId)}/by-label/${encodeURIComponent(label)}`);
  if (!response.ok) throw new Error('Failed to fetch package');
  return response.json();
}

/**
 * Create a new package
 */
export async function createPackage(siteId: string, request: CreatePackageRequest): Promise<PackageDto> {
  const response = await fetch(getApiBase(siteId), {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to create package');
  return response.json();
}

/**
 * Adjust package quantity
 */
export async function adjustPackage(
  siteId: string,
  packageId: string,
  request: AdjustPackageRequest
): Promise<PackageDto> {
  const response = await fetch(`${getApiBase(siteId)}/${packageId}/adjust`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to adjust package');
  return response.json();
}

/**
 * Move package to new location
 */
export async function movePackage(
  siteId: string,
  packageId: string,
  request: MovePackageRequest
): Promise<PackageDto> {
  const response = await fetch(`${getApiBase(siteId)}/${packageId}/move`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to move package');
  return response.json();
}

/**
 * Reserve quantity
 */
export async function reserveQuantity(
  siteId: string,
  packageId: string,
  quantity: number,
  orderId?: string,
  orderNumber?: string
): Promise<PackageDto> {
  const response = await fetch(`${getApiBase(siteId)}/${packageId}/reserve`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ quantity, orderId, orderNumber }),
  });
  if (!response.ok) throw new Error('Failed to reserve quantity');
  return response.json();
}

/**
 * Get package summary statistics
 */
export async function getPackageSummary(siteId: string): Promise<PackageSummaryStats> {
  const response = await fetch(`${getApiBase(siteId)}/summary`);
  if (!response.ok) throw new Error('Failed to fetch summary');
  return response.json();
}

/**
 * Get expiring packages
 */
export async function getExpiringPackages(siteId: string, withinDays = 30): Promise<any[]> {
  const response = await fetch(`${getApiBase(siteId)}/expiring?withinDays=${withinDays}`);
  if (!response.ok) throw new Error('Failed to fetch expiring packages');
  return response.json();
}

/**
 * Get package lineage
 */
export async function getPackageLineage(siteId: string, packageId: string): Promise<any> {
  const response = await fetch(`${getApiBase(siteId)}/${packageId}/lineage`);
  if (!response.ok) throw new Error('Failed to fetch lineage');
  return response.json();
}

/**
 * Place package on hold
 */
export async function placePackageHold(
  siteId: string,
  packageId: string,
  reasonCode: string,
  requiresTwoPersonRelease = false,
  notes?: string
): Promise<PackageDto> {
  const response = await fetch(`/api/v1/sites/${siteId}/holds/packages/${packageId}/hold`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ reasonCode, requiresTwoPersonRelease, notes }),
  });
  if (!response.ok) throw new Error('Failed to place hold');
  return response.json();
}

/**
 * Release package from hold
 */
export async function releasePackageHold(
  siteId: string,
  packageId: string,
  notes?: string
): Promise<PackageDto> {
  const response = await fetch(`/api/v1/sites/${siteId}/holds/packages/${packageId}/release-hold`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ notes }),
  });
  if (!response.ok) throw new Error('Failed to release hold');
  return response.json();
}

/**
 * Get all holds for a site (simple version)
 */
export async function getPackageHolds(siteId: string): Promise<any[]> {
  const response = await fetch(`/api/v1/sites/${siteId}/holds`);
  if (!response.ok) throw new Error('Failed to fetch holds');
  return response.json();
}



