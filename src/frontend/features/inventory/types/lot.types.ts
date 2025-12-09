/**
 * Inventory Lot Type Definitions
 * Core types for lot management, status tracking, relationships, and lineage
 */

import type { InventoryCategory, CannabisProductType, Product } from './product.types';

/** Lot QA/Compliance Status */
export type LotStatus = 
  | 'available'
  | 'on_hold'
  | 'quarantine'
  | 'pending_coa'
  | 'coa_failed'
  | 'reserved'
  | 'allocated'      // Reserved for production order
  | 'in_transit'
  | 'in_production'  // Being consumed in production
  | 'consumed'       // Fully consumed in production
  | 'destroyed';

/** Legacy product type - keeping for backward compatibility */
export type ProductType =
  | 'flower'
  | 'trim'
  | 'shake'
  | 'concentrate'
  | 'edible'
  | 'tincture'
  | 'topical'
  | 'preroll'
  | 'clone'
  | 'seed'
  | 'other';

/** Relationship types between lots */
export type LotRelationshipType = 
  | 'split_from'
  | 'merged_into'
  | 'processed_from'
  | 'harvested_from'
  | 'packaged_from'
  | 'extracted_from'
  | 'return_of'
  | 'rework_of'
  | 'adjustment_of';

/** Origin type - how was this lot created */
export type LotOriginType =
  | 'purchase'       // Purchased from vendor
  | 'cultivation'    // Harvested from cultivation batch
  | 'harvest'        // Alias for cultivation
  | 'production'     // Output from production order
  | 'receipt'        // Received from transfer
  | 'split'          // Split from parent lot
  | 'merge'          // Merged from multiple lots
  | 'adjustment'     // Created via adjustment
  | 'return'         // Customer/transfer return
  | 'opening';       // Opening inventory balance

/** Core inventory lot entity - ENHANCED with full lineage */
export interface InventoryLot {
  id: string;
  siteId: string;
  
  // Identification
  lotNumber: string;
  barcode: string;
  externalId?: string; // METRC/BioTrack ID
  
  // ===== PRODUCT LINK (NEW) =====
  productId?: string;
  product?: Product;
  productName?: string;
  
  // Classification (from product, denormalized for queries)
  category?: InventoryCategory;
  cannabisProductType?: CannabisProductType;
  
  // Legacy product type (keeping for backward compatibility)
  productType: ProductType;
  
  // Genetics
  strainId?: string;
  strainName?: string;
  
  // ===== ORIGIN & LINEAGE (ENHANCED) =====
  originType: LotOriginType;
  
  // If from cultivation
  cultivationBatchId?: string;
  cultivationBatchNumber?: string;
  harvestEventId?: string;
  
  // If from production
  productionOrderId?: string;
  productionOrderNumber?: string;
  
  // If purchased
  purchaseOrderId?: string;
  purchaseOrderLineId?: string;
  vendorId?: string;
  vendorName?: string;
  vendorLotNumber?: string;
  
  // If from transfer
  transferId?: string;
  sourceLocationId?: string;
  
  // PARENT-CHILD LINEAGE
  parentLotIds: string[];
  parentRelationships: LotLineageRelation[];
  childLotIds: string[];
  
  // FULL ANCESTRY (denormalized for fast queries)
  ancestryChain: string[];      // [oldest_ancestor_id, ..., direct_parent_id]
  rootAncestorId?: string;      // The original seed/clone lot
  generationDepth: number;      // How many transformations from origin
  
  // Genetic lineage string for display
  geneticLineage?: string;      // e.g., "Seed S-001 → Batch B-042 → Harvest H-015 → This Lot"
  
  // ===== QUANTITY =====
  quantity: number;
  uom: string;
  originalQuantity: number;
  reservedQuantity: number;     // Allocated to orders
  availableQuantity: number;    // quantity - reservedQuantity
  
  // ===== LOCATION =====
  locationId: string;
  locationPath: string;
  
  // ===== STATUS =====
  status: LotStatus;
  holdReason?: string;
  holdDate?: string;
  holdReleasedAt?: string;
  holdReleasedBy?: string;
  
  // ===== DATES =====
  harvestDate?: string;
  packageDate?: string;
  productionDate?: string;
  receivedDate?: string;
  expirationDate?: string;
  bestByDate?: string;
  
  // ===== LAB/COA =====
  labOrderId?: string;
  coaStatus?: 'not_required' | 'pending' | 'submitted' | 'passed' | 'failed' | 'expired';
  qaStatus?: 'not_required' | 'pending' | 'passed' | 'failed' | 'expired';
  coaReceivedAt?: string;
  testResults?: LotTestResults;
  
  // ===== COMPLIANCE SYNC =====
  metrcId?: string;
  metrcPackageTag?: string;
  biotrackId?: string;
  lastSyncAt?: string;
  syncStatus: 'synced' | 'pending' | 'error' | 'stale' | 'not_required';
  syncError?: string;
  
  // ===== COSTING (ENHANCED) =====
  materialCost: number;
  laborCost: number;
  overheadCost: number;
  totalCost: number;
  unitCost: number;
  
  // Legacy cost fields
  /** @deprecated Use totalCost instead */
  cost?: number;
  
  // ===== METADATA =====
  notes?: string;
  internalNotes?: string;
  attributes?: Record<string, unknown>;
  imageUrls?: string[];
  
  // ===== DENORMALIZED METRICS (for quick display) =====
  thcPercent?: number;
  cbdPercent?: number;
  
  // ===== AUDIT =====
  createdAt: string;
  createdBy: string;
  updatedAt: string;
  updatedBy: string;
}

/** Lab test results */
export interface LotTestResults {
  thcPercent?: number;
  thcaPercent?: number;
  cbdPercent?: number;
  cbdaPercent?: number;
  totalCannabinoids?: number;
  totalTerpenes?: number;
  
  // Terpene profile
  terpeneProfile?: Record<string, number>;
  
  // Contaminants
  moisturePercent?: number;
  waterActivityAw?: number;
  passedPesticides?: boolean;
  passedHeavyMetals?: boolean;
  passedMicrobials?: boolean;
  passedMycotoxins?: boolean;
  passedResidualSolvents?: boolean;
  
  // Foreign matter
  passedForeignMatter?: boolean;
  
  // Overall
  overallResult: 'pass' | 'fail' | 'pending';
  failureReasons?: string[];
  
  labName?: string;
  labLicenseNumber?: string;
  testDate?: string;
  reportUrl?: string;
}

/** Lineage relationship - detailed parent/child link */
export interface LotLineageRelation {
  id: string;
  parentLotId: string;
  parentLotNumber?: string;
  childLotId: string;
  childLotNumber?: string;
  relationshipType: LotRelationshipType;
  
  // Quantity transformation
  quantityConsumed: number;
  quantityProduced: number;
  conversionFactor: number;  // output/input ratio
  uomFrom: string;
  uomTo: string;
  
  // Context
  productionOrderId?: string;
  productionOrderNumber?: string;
  harvestEventId?: string;
  
  // Cost flow
  costAllocated?: number;
  costAllocationPercent?: number;
  
  createdAt: string;
  createdBy: string;
  notes?: string;
}

/** Lot relationship for lineage tracking */
export interface LotRelationship {
  id: string;
  parentLotId: string;
  childLotId: string;
  relationshipType: LotRelationshipType;
  quantity?: number;
  createdAt: string;
  createdBy: string;
}

/** Lot balance at a specific location */
export interface LotBalance {
  lotId: string;
  locationId: string;
  quantity: number;
  uom: string;
  lastMovementAt?: string;
}

/** Create lot request */
export interface CreateLotRequest {
  productType: ProductType;
  strainId?: string;
  quantity: number;
  uom: string;
  locationId: string;
  batchId?: string;
  harvestDate?: string;
  expirationDate?: string;
  notes?: string;
  attributes?: Record<string, unknown>;
}

/** Split lot request */
export interface SplitLotRequest {
  sourceLotId: string;
  quantities: number[];
  destinationLocationIds?: string[];
  notes?: string;
}

/** Split lot response */
export interface SplitLotResponse {
  childLotIds: string[];
  childLots: InventoryLot[];
}

/** Merge lots request */
export interface MergeLotRequest {
  sourceLotIds: string[];
  destinationLocationId: string;
  notes?: string;
}

/** Lot adjustment request */
export interface LotAdjustmentRequest {
  lotId: string;
  quantityChange: number;
  reasonCode: string;
  notes?: string;
  evidenceUrls?: string[];
}

/** Lot filter options */
export interface LotFilterOptions {
  siteId?: string;
  status?: LotStatus[];
  productType?: ProductType | ProductType[];
  locationId?: string;
  strainId?: string;
  batchId?: string;
  syncStatus?: ('synced' | 'pending' | 'error' | 'stale' | 'not_required')[];
  coaStatus?: ('pending' | 'passed' | 'failed' | 'expired' | 'not_required')[];
  qaStatus?: ('pending' | 'passed' | 'failed' | 'expired' | 'not_required')[];
  expiringWithinDays?: number;
  search?: string;
  dateRange?: { from?: string; to?: string };
  createdAfter?: string;
  createdBefore?: string;
}

/** Alias for backward compatibility */
export type LotFilters = LotFilterOptions;

/** QA Status type */
export type QAStatus = 'not_required' | 'pending' | 'passed' | 'failed' | 'expired';

/** Sync state alias */
export type SyncState = 'synced' | 'pending' | 'error' | 'stale' | 'not_required';

/** Paginated lot list response */
export interface LotListResponse {
  items: InventoryLot[];
  total: number;
  page: number;
  pageSize: number;
  hasMore: boolean;
}

/** Lot summary for dashboard widgets */
export interface LotSummary {
  totalLots: number;
  totalQuantity: number;
  byStatus: Record<LotStatus, number>;
  byProductType: Record<ProductType, number>;
  onHold: number;
  pendingSync: number;
  expiringIn7Days: number;
  expiringIn30Days: number;
}
