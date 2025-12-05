/**
 * Bill of Materials (BOM) Type Definitions
 * Defines manufacturing recipes - inputs required to produce outputs
 */

import type { Product, InventoryCategory, CannabisProductType } from './product.types';

/** BOM operation type */
export type BomType =
  | 'production'     // Standard manufacturing (e.g., packaging flower)
  | 'processing'     // Extraction/refinement (e.g., making concentrate)
  | 'cultivation'    // Growing operation (seed/clone → harvest)
  | 'packaging'      // Packaging operation
  | 'assembly'       // Kit assembly
  | 'disassembly'    // Breaking down products
  | 'conversion';    // Converting between forms (e.g., wet → dry)

/** BOM status */
export type BomStatus = 'draft' | 'active' | 'inactive' | 'obsolete';

/** Input line type */
export type InputLineType = 'material' | 'labor' | 'overhead' | 'consumable';

/** Bill of Materials header */
export interface BillOfMaterials {
  id: string;
  siteId: string;

  // Identification
  bomNumber: string;
  name: string;
  description?: string;
  bomType: BomType;

  // Output product
  outputProductId: string;
  outputProduct?: Product;
  outputQuantity: number;
  outputUom: string;

  // Process details
  workCenterId?: string;
  workCenterName?: string;
  estimatedDurationMinutes?: number;
  setupTimeMinutes?: number;
  cleanupTimeMinutes?: number;

  // Labor
  laborHoursPerBatch?: number;
  laborHoursPerUnit?: number;
  requiredSkills?: string[];
  minOperators?: number;
  maxOperators?: number;

  // Yield expectations
  expectedYieldPercent: number;
  yieldVarianceThreshold: number; // Alert if variance exceeds this %

  // Versioning
  version: number;
  previousVersionId?: string;
  effectiveDate: string;
  expirationDate?: string;

  // Status
  status: BomStatus;
  isDefault: boolean; // Default BOM for this output product

  // Lines
  inputLines: BomInputLine[];
  byproductLines: BomByproductLine[];
  operationSteps?: BomOperationStep[];

  // Quality
  requiresQaApproval: boolean;
  qaCheckpoints?: QaCheckpoint[];
  requiredCertifications?: string[];

  // Compliance
  metrcProductionType?: string;
  requiresMetrcReporting: boolean;

  // Costing (calculated)
  estimatedMaterialCost?: number;
  estimatedLaborCost?: number;
  estimatedOverheadCost?: number;
  estimatedTotalCost?: number;
  estimatedUnitCost?: number;

  // Notes
  notes?: string;
  internalNotes?: string;

  // Audit
  createdAt: string;
  createdBy: string;
  updatedAt: string;
  updatedBy: string;
  approvedAt?: string;
  approvedBy?: string;
}

/** BOM input line - materials consumed */
export interface BomInputLine {
  id: string;
  bomId: string;
  lineNumber: number;
  lineType: InputLineType;

  // Input product (for material type)
  inputProductId?: string;
  inputProduct?: Product;

  // Quantity per output batch
  quantityPer: number;
  uom: string;

  // Flexibility
  isOptional: boolean;
  isPhantom: boolean; // Explode to sub-BOM
  phantomBomId?: string;

  // Substitutes
  allowSubstitutes: boolean;
  substituteProductIds?: string[];
  substitutePreference?: number; // Order of preference

  // Scrap/waste
  scrapPercent: number;
  scrapReasonDefault?: string;

  // Sourcing
  preferredVendorId?: string;
  fixedLotId?: string; // For specific lot requirements

  // Location
  defaultPickLocationId?: string;

  // Costing
  estimatedCost?: number;

  // Instructions
  instructions?: string;
  qualityNotes?: string;
}

/** BOM byproduct line - outputs besides main product */
export interface BomByproductLine {
  id: string;
  bomId: string;
  lineNumber: number;

  // Byproduct
  byproductProductId: string;
  byproductProduct?: Product;

  // Expected quantity per output batch
  expectedQuantityPer: number;
  uom: string;
  quantityVariancePercent: number;

  // Recovery
  isRecoverable: boolean;
  recoveryBomId?: string; // BOM to process this byproduct

  // Disposition
  defaultDisposition: 'inventory' | 'waste' | 'recycle';
  defaultLocationId?: string;

  // Value
  estimatedValue?: number;
  costAllocationPercent?: number; // What % of cost to allocate here
}

/** BOM operation step */
export interface BomOperationStep {
  id: string;
  bomId: string;
  stepNumber: number;

  // Step details
  name: string;
  description?: string;
  instructions?: string;

  // Timing
  estimatedDurationMinutes: number;
  waitTimeMinutes?: number; // Wait after this step

  // Work center
  workCenterId?: string;
  workCenterName?: string;

  // Labor
  laborHours?: number;
  requiredSkills?: string[];

  // Equipment
  requiredEquipmentIds?: string[];

  // Quality
  isQaCheckpoint: boolean;
  qaInstructions?: string;

  // Dependencies
  predecessorStepIds?: string[];
}

/** QA checkpoint definition */
export interface QaCheckpoint {
  id: string;
  name: string;
  description?: string;
  checkType: 'visual' | 'measurement' | 'weight' | 'lab_test' | 'documentation';
  isRequired: boolean;
  passCriteria?: string;
  failAction: 'hold' | 'reject' | 'rework' | 'notify';
  atStep?: number;
}

/** BOM with calculated costs */
export interface BomWithCosts extends BillOfMaterials {
  calculatedCosts: {
    materialCost: number;
    laborCost: number;
    overheadCost: number;
    byproductCredit: number;
    totalCost: number;
    unitCost: number;
    marginPercent?: number;
  };
}

/** BOM filter options */
export interface BomFilterOptions {
  siteId?: string;
  bomType?: BomType[];
  status?: BomStatus[];
  outputProductId?: string;
  outputCategory?: InventoryCategory[];
  outputProductType?: CannabisProductType[];
  search?: string;
  isDefault?: boolean;
}

/** Create BOM request */
export interface CreateBomRequest {
  name: string;
  description?: string;
  bomType: BomType;
  outputProductId: string;
  outputQuantity: number;
  outputUom: string;
  expectedYieldPercent?: number;
  inputLines: Omit<BomInputLine, 'id' | 'bomId'>[];
  byproductLines?: Omit<BomByproductLine, 'id' | 'bomId'>[];
  isDefault?: boolean;
}

/** Update BOM request */
export interface UpdateBomRequest extends Partial<CreateBomRequest> {
  id: string;
}

/** Clone BOM request (creates new version) */
export interface CloneBomRequest {
  sourceBomId: string;
  newName?: string;
  makeActive?: boolean;
}

/** BOM list response */
export interface BomListResponse {
  items: BillOfMaterials[];
  total: number;
  page: number;
  pageSize: number;
  hasMore: boolean;
}

/** BOM explosion - flattened view of all materials */
export interface BomExplosion {
  bomId: string;
  bomName: string;
  outputProductId: string;
  outputProductName: string;
  outputQuantity: number;

  // Flattened materials (including phantom BOM contents)
  flattenedMaterials: {
    productId: string;
    productName: string;
    productSku: string;
    totalQuantityRequired: number;
    uom: string;
    level: number; // BOM level depth
    sourceBomId: string;
    sourceBomName: string;
  }[];

  // Total costs
  totalMaterialCost: number;
  totalLaborCost: number;
  totalOverheadCost: number;
  grandTotal: number;
}

/** BOM type configuration for display */
export const BOM_TYPE_CONFIG: Record<BomType, {
  label: string;
  description: string;
  color: string;
  bgColor: string;
  icon: string;
}> = {
  production: {
    label: 'Production',
    description: 'Standard manufacturing process',
    color: 'text-cyan-400',
    bgColor: 'bg-cyan-500/10',
    icon: 'Factory',
  },
  processing: {
    label: 'Processing',
    description: 'Extraction or refinement',
    color: 'text-violet-400',
    bgColor: 'bg-violet-500/10',
    icon: 'Beaker',
  },
  cultivation: {
    label: 'Cultivation',
    description: 'Growing operation',
    color: 'text-emerald-400',
    bgColor: 'bg-emerald-500/10',
    icon: 'Sprout',
  },
  packaging: {
    label: 'Packaging',
    description: 'Package finished products',
    color: 'text-blue-400',
    bgColor: 'bg-blue-500/10',
    icon: 'Package',
  },
  assembly: {
    label: 'Assembly',
    description: 'Assemble kits or bundles',
    color: 'text-amber-400',
    bgColor: 'bg-amber-500/10',
    icon: 'Layers',
  },
  disassembly: {
    label: 'Disassembly',
    description: 'Break down into components',
    color: 'text-rose-400',
    bgColor: 'bg-rose-500/10',
    icon: 'Ungroup',
  },
  conversion: {
    label: 'Conversion',
    description: 'Convert between forms',
    color: 'text-orange-400',
    bgColor: 'bg-orange-500/10',
    icon: 'ArrowRightLeft',
  },
};

