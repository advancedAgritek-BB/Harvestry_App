/**
 * Inventory Movement Type Definitions
 * Types for movements, adjustments, and transaction tracking
 */

/** Movement type classification */
export type MovementType =
  | 'transfer'
  | 'receive'
  | 'ship'
  | 'return'
  | 'adjustment'
  | 'split'
  | 'merge'
  | 'process_input'
  | 'process_output'
  | 'destruction'
  | 'cycle_count';

/** Movement status */
export type MovementStatus =
  | 'pending'
  | 'in_progress'
  | 'completed'
  | 'cancelled'
  | 'failed';

/** Adjustment reason codes */
export type AdjustmentReasonCode =
  | 'damage'
  | 'theft'
  | 'spoilage'
  | 'measurement_error'
  | 'cycle_count'
  | 'quality_issue'
  | 'contamination'
  | 'regulatory_destruction'
  | 'sample'
  | 'other';

/** Core movement entity */
export interface InventoryMovement {
  id: string;
  siteId: string;
  
  // Movement details
  movementType: MovementType;
  status: MovementStatus;
  
  // Lot reference
  lotId: string;
  lotNumber: string;
  
  // Locations
  fromLocationId?: string;
  fromLocationPath?: string;
  toLocationId?: string;
  toLocationPath?: string;
  
  // Quantity
  quantity: number;
  uom: string;
  
  // Processing reference
  processRunId?: string;
  
  // Compliance
  metrcManifestId?: string;
  biotrackTransferId?: string;
  syncStatus: 'synced' | 'pending' | 'error';
  
  // Verification
  verifiedBy?: string;
  verifiedAt?: string;
  scanData?: string;
  
  // Metadata
  notes?: string;
  evidenceUrls?: string[];
  
  // Audit
  createdAt: string;
  createdBy: string;
  completedAt?: string;
  completedBy?: string;
}

/** Inventory adjustment entity */
export interface InventoryAdjustment {
  id: string;
  siteId: string;
  movementId: string;
  
  // Lot reference
  lotId: string;
  lotNumber: string;
  
  // Adjustment details
  reasonCode: AdjustmentReasonCode;
  quantityBefore: number;
  quantityChange: number;
  quantityAfter: number;
  uom: string;
  
  // Evidence
  notes?: string;
  evidenceUrls?: string[];
  
  // Approval (for significant adjustments)
  requiresApproval: boolean;
  approvedBy?: string;
  approvedAt?: string;
  
  // Compliance
  syncStatus: 'synced' | 'pending' | 'error';
  
  // Audit
  createdAt: string;
  createdBy: string;
}

/** Create movement request */
export interface CreateMovementRequest {
  lotId: string;
  movementType: MovementType;
  fromLocationId?: string;
  toLocationId: string;
  quantity: number;
  notes?: string;
  scanData?: string;
}

/** Batch movement request */
export interface BatchMovementRequest {
  movements: CreateMovementRequest[];
  notes?: string;
}

/** Movement filter options */
export interface MovementFilterOptions {
  siteId?: string;
  lotId?: string;
  locationId?: string;
  movementType?: MovementType[];
  status?: MovementStatus[];
  syncStatus?: ('synced' | 'pending' | 'error')[];
  createdBy?: string;
  createdAfter?: string;
  createdBefore?: string;
  search?: string;
}

/** Paginated movement list response */
export interface MovementListResponse {
  items: InventoryMovement[];
  total: number;
  page: number;
  pageSize: number;
  hasMore: boolean;
}

/** Movement summary for dashboard */
export interface MovementSummary {
  todayCount: number;
  weekCount: number;
  byType: Record<MovementType, number>;
  pendingCount: number;
  failedCount: number;
  recentMovements: InventoryMovement[];
}

/** Receive/receiving workflow */
export interface ReceiveWorkflow {
  id: string;
  purchaseOrderId?: string;
  vendorId?: string;
  vendorName?: string;
  expectedItems: ReceiveLineItem[];
  receivedItems: ReceiveLineItem[];
  status: 'pending' | 'in_progress' | 'completed' | 'partial';
  notes?: string;
  createdAt: string;
  completedAt?: string;
}

/** Receive line item */
export interface ReceiveLineItem {
  productType: string;
  strainId?: string;
  expectedQuantity: number;
  receivedQuantity?: number;
  uom: string;
  lotId?: string;
  locationId?: string;
  notes?: string;
}
