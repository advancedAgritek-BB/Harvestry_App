/**
 * Production Order Type Definitions
 * Work orders that consume inputs and produce outputs
 */

import type { Product, InventoryCategory } from './product.types';
import type { BillOfMaterials, BomType } from './bom.types';
import type { InventoryLot } from './lot.types';

/** Production order status */
export type ProductionOrderStatus =
  | 'draft'              // Created but not confirmed
  | 'pending_materials'  // Waiting for material availability
  | 'pending_approval'   // Awaiting approval to start
  | 'ready'              // Materials allocated, ready to start
  | 'in_progress'        // Production started
  | 'on_hold'            // Paused
  | 'pending_qa'         // Awaiting QA release
  | 'completed'          // Successfully completed
  | 'cancelled'          // Cancelled
  | 'failed';            // Failed/aborted

/** Production order priority */
export type ProductionPriority = 'low' | 'normal' | 'high' | 'urgent' | 'critical';

/** Material line status */
export type MaterialLineStatus =
  | 'pending'      // Not yet allocated
  | 'allocated'    // Lot reserved
  | 'issued'       // Lot consumed
  | 'partial'      // Partially issued
  | 'returned'     // Material returned
  | 'substituted'; // Different material used

/** Production Order header */
export interface ProductionOrder {
  id: string;
  siteId: string;

  // Identification
  orderNumber: string;
  externalRef?: string;
  description?: string;

  // What we're making
  bomId: string;
  bom?: BillOfMaterials;
  bomVersion: number;
  outputProductId: string;
  outputProduct?: Product;

  // Quantities
  plannedQuantity: number;
  plannedUom: string;
  actualQuantity?: number;
  varianceQuantity?: number;
  variancePercent?: number;

  // Status
  status: ProductionOrderStatus;
  priority: ProductionPriority;
  onHoldReason?: string;
  failureReason?: string;

  // Scheduling
  requestedDate?: string;
  plannedStartDate: string;
  plannedEndDate: string;
  actualStartDate?: string;
  actualEndDate?: string;
  dueDate?: string;

  // Progress
  progressPercent: number;
  currentStepNumber?: number;
  currentStepName?: string;

  // Location
  workCenterId?: string;
  workCenterName?: string;
  productionLocationId?: string;
  productionLocationPath?: string;

  // Source cultivation batch (if from harvest)
  sourceBatchId?: string;
  sourceBatchNumber?: string;

  // Parent order (for sub-assemblies)
  parentOrderId?: string;
  parentOrderNumber?: string;

  // Labor tracking
  estimatedLaborHours: number;
  actualLaborHours: number;
  laborEntries: ProductionLaborEntry[];

  // Yield tracking
  expectedYieldPercent: number;
  actualYieldPercent?: number;
  yieldVariance?: number;

  // Quality
  requiresQaRelease: boolean;
  qaStatus?: 'pending' | 'approved' | 'rejected' | 'conditional';
  qaReleasedAt?: string;
  qaReleasedBy?: string;
  qaNotes?: string;

  // Compliance
  metrcProductionBatchId?: string;
  metrcReported: boolean;
  syncStatus: 'pending' | 'synced' | 'error' | 'not_required';
  syncError?: string;

  // Cost tracking
  estimatedCost: ProductionCostSummary;
  actualCost?: ProductionCostSummary;
  costVariance?: ProductionCostSummary;

  // Lines
  materialLines: ProductionMaterialLine[];
  outputLots: ProductionOutputLot[];
  byproductLots: ProductionByproductLot[];
  wasteEntries: ProductionWasteEntry[];

  // Notes & attachments
  notes?: string;
  internalNotes?: string;
  attachmentUrls?: string[];

  // Audit
  createdAt: string;
  createdBy: string;
  updatedAt: string;
  updatedBy: string;
  startedBy?: string;
  completedBy?: string;
  cancelledBy?: string;
  cancelledAt?: string;
}

/** Cost summary */
export interface ProductionCostSummary {
  materialCost: number;
  laborCost: number;
  overheadCost: number;
  byproductCredit: number;
  wasteCost: number;
  totalCost: number;
  unitCost: number;
}

/** Material line - inputs being consumed */
export interface ProductionMaterialLine {
  id: string;
  productionOrderId: string;
  lineNumber: number;

  // From BOM
  bomLineId?: string;
  productId: string;
  product?: Product;

  // Planned quantities
  plannedQuantity: number;
  uom: string;
  scrapAllowance: number;

  // Status
  status: MaterialLineStatus;

  // Allocation
  allocatedLotId?: string;
  allocatedLot?: InventoryLot;
  allocatedQuantity?: number;
  allocatedAt?: string;
  allocatedBy?: string;

  // Substitution
  isSubstitute: boolean;
  originalProductId?: string;
  substituteReason?: string;
  substituteApprovedBy?: string;

  // Issuance (actual consumption)
  issuedQuantity?: number;
  issuedAt?: string;
  issuedBy?: string;
  issuedFromLocationId?: string;
  issuedFromLocationPath?: string;

  // Returns
  returnedQuantity?: number;
  returnedAt?: string;
  returnedBy?: string;
  returnReason?: string;

  // Cost
  unitCost?: number;
  totalCost?: number;

  // FEFO tracking
  lotExpirationDate?: string;
  lotHarvestDate?: string;

  // Notes
  notes?: string;
}

/** Output lot - what's being produced */
export interface ProductionOutputLot {
  id: string;
  productionOrderId: string;

  // The lot created
  lotId: string;
  lot?: InventoryLot;
  lotNumber?: string;

  // Quantity
  quantity: number;
  uom: string;

  // Location
  destinationLocationId?: string;
  destinationLocationPath?: string;

  // Quality
  gradeCode?: string;
  qualityNotes?: string;

  // Cost allocation
  materialCost: number;
  laborCost: number;
  overheadCost: number;
  totalCost: number;
  unitCost: number;

  // Compliance
  metrcPackageId?: string;
  labTestRequired: boolean;
  labTestStatus?: 'pending' | 'submitted' | 'passed' | 'failed';

  createdAt: string;
  createdBy: string;
}

/** Byproduct lot */
export interface ProductionByproductLot {
  id: string;
  productionOrderId: string;
  bomByproductLineId?: string;

  // Product
  productId: string;
  product?: Product;

  // Lot created
  lotId?: string;
  lot?: InventoryLot;
  lotNumber?: string;

  // Quantity
  quantity: number;
  uom: string;
  expectedQuantity?: number;
  variancePercent?: number;

  // Disposition
  disposition: 'inventory' | 'waste' | 'recycle' | 'pending';
  destinationLocationId?: string;

  // Value
  unitValue?: number;
  totalValue?: number;
  costAllocationPercent?: number;

  createdAt: string;
  createdBy: string;
}

/** Waste entry */
export interface ProductionWasteEntry {
  id: string;
  productionOrderId: string;

  // What was wasted
  productId?: string;
  product?: Product;
  lotId?: string;

  // Quantity
  quantity: number;
  uom: string;

  // Reason
  wasteType: 'process_loss' | 'quality_reject' | 'contamination' | 'spillage' | 'expired' | 'other';
  reasonCode?: string;
  description?: string;

  // Cost impact
  wasteCost?: number;

  // Compliance
  requiresDestruction: boolean;
  destructionCompleted?: boolean;
  destructionDate?: string;
  witnessedBy?: string;
  metrcWasteEventId?: string;

  // Evidence
  photoUrls?: string[];

  recordedAt: string;
  recordedBy: string;
}

/** Labor entry */
export interface ProductionLaborEntry {
  id: string;
  productionOrderId: string;

  // Who
  userId: string;
  userName: string;
  teamId?: string;
  teamName?: string;

  // When
  startTime: string;
  endTime?: string;
  hoursWorked: number;
  breakMinutes?: number;

  // Type
  laborType: 'direct' | 'indirect' | 'setup' | 'cleanup' | 'rework' | 'qa';
  operationStepId?: string;
  operationStepName?: string;

  // Rate & cost
  hourlyRate?: number;
  overtimeMultiplier?: number;
  totalCost?: number;

  // Notes
  notes?: string;
  taskDescription?: string;

  createdAt: string;
}

/** Production order filter options */
export interface ProductionOrderFilterOptions {
  siteId?: string;
  status?: ProductionOrderStatus[];
  priority?: ProductionPriority[];
  bomType?: BomType[];
  outputProductId?: string;
  outputCategory?: InventoryCategory[];
  workCenterId?: string;
  sourceBatchId?: string;
  dateFrom?: string;
  dateTo?: string;
  search?: string;
  hasVariance?: boolean;
  requiresQa?: boolean;
}

/** Create production order request */
export interface CreateProductionOrderRequest {
  bomId: string;
  plannedQuantity: number;
  plannedStartDate: string;
  plannedEndDate?: string;
  priority?: ProductionPriority;
  workCenterId?: string;
  sourceBatchId?: string;
  notes?: string;
  autoAllocateMaterials?: boolean;
}

/** Update production order request */
export interface UpdateProductionOrderRequest {
  id: string;
  plannedQuantity?: number;
  plannedStartDate?: string;
  plannedEndDate?: string;
  priority?: ProductionPriority;
  notes?: string;
}

/** Start production request */
export interface StartProductionRequest {
  productionOrderId: string;
  actualStartDate?: string;
  notes?: string;
}

/** Complete production request */
export interface CompleteProductionRequest {
  productionOrderId: string;
  actualQuantity: number;
  outputLocationId: string;
  byproducts?: {
    bomByproductLineId: string;
    quantity: number;
    disposition: 'inventory' | 'waste';
    locationId?: string;
  }[];
  waste?: {
    productId?: string;
    quantity: number;
    uom: string;
    wasteType: string;
    reasonCode?: string;
  }[];
  notes?: string;
}

/** Allocate materials request */
export interface AllocateMaterialsRequest {
  productionOrderId: string;
  allocations: {
    materialLineId: string;
    lotId: string;
    quantity: number;
  }[];
}

/** Issue materials request */
export interface IssueMaterialsRequest {
  productionOrderId: string;
  issues: {
    materialLineId: string;
    quantity: number;
  }[];
}

/** Production order list response */
export interface ProductionOrderListResponse {
  items: ProductionOrder[];
  total: number;
  page: number;
  pageSize: number;
  hasMore: boolean;
}

/** Production schedule view */
export interface ProductionSchedule {
  date: string;
  orders: ProductionOrderScheduleItem[];
  totalPlannedHours: number;
  capacityHours: number;
  utilizationPercent: number;
}

export interface ProductionOrderScheduleItem {
  id: string;
  orderNumber: string;
  outputProductName: string;
  plannedQuantity: number;
  status: ProductionOrderStatus;
  priority: ProductionPriority;
  plannedStartTime: string;
  plannedEndTime: string;
  estimatedHours: number;
  workCenterName?: string;
  assignedTeam?: string;
}

/** Production dashboard summary */
export interface ProductionDashboardSummary {
  // Order counts by status
  ordersByStatus: Record<ProductionOrderStatus, number>;

  // Today's schedule
  todaySchedule: {
    plannedOrders: number;
    completedOrders: number;
    inProgressOrders: number;
    plannedQuantity: number;
    completedQuantity: number;
  };

  // Yield metrics
  yieldMetrics: {
    averageYield: number;
    targetYield: number;
    variancePercent: number;
    ordersWithVariance: number;
  };

  // Material status
  materialStatus: {
    pendingAllocation: number;
    allocated: number;
    lowStockAlerts: number;
  };

  // QA status
  qaStatus: {
    pendingRelease: number;
    awaitingResults: number;
    rejectedToday: number;
  };
}

/** Status configuration for display */
export const PRODUCTION_STATUS_CONFIG: Record<ProductionOrderStatus, {
  label: string;
  color: string;
  bgColor: string;
  icon: string;
}> = {
  draft: { label: 'Draft', color: 'text-muted-foreground', bgColor: 'bg-muted/50', icon: 'FileEdit' },
  pending_materials: { label: 'Pending Materials', color: 'text-amber-400', bgColor: 'bg-amber-500/10', icon: 'Package' },
  pending_approval: { label: 'Pending Approval', color: 'text-orange-400', bgColor: 'bg-orange-500/10', icon: 'Clock' },
  ready: { label: 'Ready', color: 'text-cyan-400', bgColor: 'bg-cyan-500/10', icon: 'CheckCircle' },
  in_progress: { label: 'In Progress', color: 'text-blue-400', bgColor: 'bg-blue-500/10', icon: 'Play' },
  on_hold: { label: 'On Hold', color: 'text-amber-400', bgColor: 'bg-amber-500/10', icon: 'Pause' },
  pending_qa: { label: 'Pending QA', color: 'text-violet-400', bgColor: 'bg-violet-500/10', icon: 'ClipboardCheck' },
  completed: { label: 'Completed', color: 'text-emerald-400', bgColor: 'bg-emerald-500/10', icon: 'CheckCircle2' },
  cancelled: { label: 'Cancelled', color: 'text-muted-foreground', bgColor: 'bg-muted/50', icon: 'XCircle' },
  failed: { label: 'Failed', color: 'text-rose-400', bgColor: 'bg-rose-500/10', icon: 'AlertTriangle' },
};

export const PRIORITY_CONFIG: Record<ProductionPriority, {
  label: string;
  color: string;
  bgColor: string;
}> = {
  low: { label: 'Low', color: 'text-muted-foreground', bgColor: 'bg-muted/50' },
  normal: { label: 'Normal', color: 'text-blue-400', bgColor: 'bg-blue-500/10' },
  high: { label: 'High', color: 'text-amber-400', bgColor: 'bg-amber-500/10' },
  urgent: { label: 'Urgent', color: 'text-orange-400', bgColor: 'bg-orange-500/10' },
  critical: { label: 'Critical', color: 'text-rose-400', bgColor: 'bg-rose-500/10' },
};

