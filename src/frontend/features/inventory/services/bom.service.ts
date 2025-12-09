/**
 * Bill of Materials Service
 * API operations for BOM (recipe) management
 */

import type {
  BillOfMaterials,
  BomWithCosts,
  BomFilterOptions,
  CreateBomRequest,
  UpdateBomRequest,
  CloneBomRequest,
  BomListResponse,
  BomExplosion,
  BomType,
} from '../types';

/**
 * BOM Service Class
 * Handles all Bill of Materials CRUD operations
 */
export class BomService {
  /**
   * Get paginated list of BOMs
   */
  static async getBoms(
    filters?: BomFilterOptions,
    page = 1,
    pageSize = 25
  ): Promise<BomListResponse> {
    // TODO: Replace with actual API call when backend is ready
    let items = [...MOCK_BOMS];

    // Apply filters
    if (filters?.bomType?.length) {
      items = items.filter((b) => filters.bomType!.includes(b.bomType));
    }
    if (filters?.status?.length) {
      items = items.filter((b) => filters.status!.includes(b.status));
    }
    if (filters?.outputProductId) {
      items = items.filter((b) => b.outputProductId === filters.outputProductId);
    }
    if (filters?.search) {
      const search = filters.search.toLowerCase();
      items = items.filter(
        (b) =>
          b.name.toLowerCase().includes(search) ||
          b.bomNumber.toLowerCase().includes(search)
      );
    }

    const total = items.length;
    const start = (page - 1) * pageSize;
    const end = start + pageSize;
    items = items.slice(start, end);

    return {
      items,
      total,
      page,
      pageSize,
      hasMore: end < total,
    };
  }

  /**
   * Get single BOM by ID
   */
  static async getBom(id: string): Promise<BillOfMaterials | null> {
    return MOCK_BOMS.find((b) => b.id === id) || null;
  }

  /**
   * Get BOM with calculated costs
   */
  static async getBomWithCosts(id: string): Promise<BomWithCosts | null> {
    const bom = await this.getBom(id);
    if (!bom) return null;

    // Calculate costs from input lines
    const materialCost = bom.inputLines.reduce((sum, line) => {
      return sum + (line.estimatedCost || 0);
    }, 0);

    const laborCost = (bom.laborHoursPerBatch || 0) * 25; // Assume $25/hr
    const overheadCost = materialCost * 0.15; // 15% overhead
    const byproductCredit = bom.byproductLines.reduce((sum, line) => {
      return sum + (line.estimatedValue || 0);
    }, 0);

    const totalCost = materialCost + laborCost + overheadCost - byproductCredit;
    const unitCost = totalCost / bom.outputQuantity;

    return {
      ...bom,
      calculatedCosts: {
        materialCost,
        laborCost,
        overheadCost,
        byproductCredit,
        totalCost,
        unitCost,
      },
    };
  }

  /**
   * Get default BOM for a product
   */
  static async getDefaultBomForProduct(productId: string): Promise<BillOfMaterials | null> {
    return MOCK_BOMS.find((b) => b.outputProductId === productId && b.isDefault) || null;
  }

  /**
   * Get all BOMs for a product
   */
  static async getBomsForProduct(productId: string): Promise<BillOfMaterials[]> {
    return MOCK_BOMS.filter((b) => b.outputProductId === productId);
  }

  /**
   * Create new BOM
   */
  static async createBom(request: CreateBomRequest): Promise<BillOfMaterials> {
    console.log('Creating BOM:', request);

    const newBom: BillOfMaterials = {
      id: `bom-${Date.now()}`,
      siteId: 'site-1',
      bomNumber: `BOM-${Date.now()}`,
      name: request.name,
      description: request.description,
      bomType: request.bomType,
      outputProductId: request.outputProductId,
      outputQuantity: request.outputQuantity,
      outputUom: request.outputUom,
      expectedYieldPercent: request.expectedYieldPercent || 100,
      yieldVarianceThreshold: 5,
      version: 1,
      effectiveDate: new Date().toISOString(),
      status: 'draft',
      isDefault: request.isDefault || false,
      inputLines: request.inputLines.map((line, idx) => ({
        ...line,
        id: `line-${Date.now()}-${idx}`,
        bomId: `bom-${Date.now()}`,
        lineNumber: idx + 1,
        lineType: 'material',
      })),
      byproductLines: (request.byproductLines || []).map((line, idx) => ({
        ...line,
        id: `bp-${Date.now()}-${idx}`,
        bomId: `bom-${Date.now()}`,
        lineNumber: idx + 1,
      })),
      requiresQaApproval: true,
      requiresMetrcReporting: true,
      createdAt: new Date().toISOString(),
      createdBy: 'current-user',
      updatedAt: new Date().toISOString(),
      updatedBy: 'current-user',
    };

    return newBom;
  }

  /**
   * Update existing BOM
   */
  static async updateBom(request: UpdateBomRequest): Promise<BillOfMaterials> {
    console.log('Updating BOM:', request);

    const existing = await this.getBom(request.id);
    if (!existing) {
      throw new Error('BOM not found');
    }

    // Process input lines if provided to add required ids
    const updatedBom = {
      ...existing,
      ...request,
      updatedAt: new Date().toISOString(),
      updatedBy: 'current-user',
      // Override inputLines with properly formatted ones
      inputLines: request.inputLines 
        ? request.inputLines.map((line, idx) => ({
            ...line,
            id: `line-${Date.now()}-${idx}`,
            bomId: request.id,
          }))
        : existing.inputLines,
      byproductLines: request.byproductLines 
        ? request.byproductLines.map((line, idx) => ({
            ...line,
            id: `bp-${Date.now()}-${idx}`,
            bomId: request.id,
          }))
        : existing.byproductLines,
    };
    
    return updatedBom as BillOfMaterials;
  }

  /**
   * Clone BOM to create new version
   */
  static async cloneBom(request: CloneBomRequest): Promise<BillOfMaterials> {
    const source = await this.getBom(request.sourceBomId);
    if (!source) {
      throw new Error('Source BOM not found');
    }

    const newBom: BillOfMaterials = {
      ...source,
      id: `bom-${Date.now()}`,
      bomNumber: `${source.bomNumber}-V${source.version + 1}`,
      name: request.newName || `${source.name} (Copy)`,
      version: source.version + 1,
      previousVersionId: source.id,
      status: 'draft',
      isDefault: request.makeActive || false,
      effectiveDate: new Date().toISOString(),
      createdAt: new Date().toISOString(),
      createdBy: 'current-user',
      updatedAt: new Date().toISOString(),
      updatedBy: 'current-user',
    };

    return newBom;
  }

  /**
   * Activate BOM
   */
  static async activateBom(id: string): Promise<BillOfMaterials> {
    const bom = await this.getBom(id);
    if (!bom) {
      throw new Error('BOM not found');
    }

    return {
      ...bom,
      status: 'active',
      updatedAt: new Date().toISOString(),
    };
  }

  /**
   * Get BOM explosion (flattened view of all materials)
   */
  static async getBomExplosion(id: string, quantity = 1): Promise<BomExplosion | null> {
    const bom = await this.getBom(id);
    if (!bom) return null;

    const flattenedMaterials = bom.inputLines
      .filter((line) => line.lineType === 'material' && line.inputProductId)
      .map((line) => ({
        productId: line.inputProductId!,
        productName: line.inputProduct?.name || 'Unknown',
        productSku: line.inputProduct?.sku || 'N/A',
        totalQuantityRequired: line.quantityPer * quantity,
        uom: line.uom,
        level: 1,
        sourceBomId: bom.id,
        sourceBomName: bom.name,
      }));

    return {
      bomId: bom.id,
      bomName: bom.name,
      outputProductId: bom.outputProductId,
      outputProductName: bom.outputProduct?.name || 'Unknown',
      outputQuantity: bom.outputQuantity * quantity,
      flattenedMaterials,
      totalMaterialCost: bom.estimatedMaterialCost || 0,
      totalLaborCost: bom.estimatedLaborCost || 0,
      totalOverheadCost: bom.estimatedOverheadCost || 0,
      grandTotal: bom.estimatedTotalCost || 0,
    };
  }

  /**
   * Get BOMs by type
   */
  static async getBomsByType(bomType: BomType): Promise<BillOfMaterials[]> {
    return MOCK_BOMS.filter((b) => b.bomType === bomType);
  }
}

// ============ Mock Data ============

const MOCK_BOMS: BillOfMaterials[] = [
  // Packaging BOM - Flower into packaged jar
  {
    id: 'bom-001',
    siteId: 'site-1',
    bomNumber: 'BOM-PKG-BD-3.5G',
    name: 'Package Blue Dream 3.5g',
    description: 'Package cured flower into 3.5g retail jar',
    bomType: 'packaging',
    outputProductId: 'prod-006', // Blue Dream 3.5g
    outputQuantity: 1,
    outputUom: 'ea',
    estimatedDurationMinutes: 5,
    laborHoursPerBatch: 0.08,
    expectedYieldPercent: 100,
    yieldVarianceThreshold: 2,
    version: 1,
    effectiveDate: '2025-01-01',
    status: 'active',
    isDefault: true,
    inputLines: [
      {
        id: 'line-001-1',
        bomId: 'bom-001',
        lineNumber: 1,
        lineType: 'material',
        inputProductId: 'prod-003', // Cured flower bulk
        quantityPer: 3.5,
        uom: 'g',
        isOptional: false,
        isPhantom: false,
        allowSubstitutes: false,
        scrapPercent: 2,
        estimatedCost: 8.75,
      },
      {
        id: 'line-001-2',
        bomId: 'bom-001',
        lineNumber: 2,
        lineType: 'material',
        inputProductId: 'prod-009', // Glass jar
        quantityPer: 1,
        uom: 'ea',
        isOptional: false,
        isPhantom: false,
        allowSubstitutes: false,
        scrapPercent: 0,
        estimatedCost: 0.85,
      },
    ],
    byproductLines: [],
    requiresQaApproval: false,
    requiresMetrcReporting: true,
    estimatedMaterialCost: 9.60,
    estimatedLaborCost: 2.00,
    estimatedOverheadCost: 1.44,
    estimatedTotalCost: 13.04,
    estimatedUnitCost: 13.04,
    createdAt: '2025-01-01T00:00:00Z',
    createdBy: 'admin',
    updatedAt: '2025-01-01T00:00:00Z',
    updatedBy: 'admin',
  },
  // Pre-roll production BOM
  {
    id: 'bom-002',
    siteId: 'site-1',
    bomNumber: 'BOM-PR-BD-1G',
    name: 'Roll Blue Dream Pre-Roll 1g',
    description: 'Produce single 1g pre-roll',
    bomType: 'production',
    outputProductId: 'prod-007', // Pre-roll
    outputQuantity: 1,
    outputUom: 'ea',
    estimatedDurationMinutes: 3,
    laborHoursPerBatch: 0.05,
    expectedYieldPercent: 95,
    yieldVarianceThreshold: 5,
    version: 1,
    effectiveDate: '2025-01-01',
    status: 'active',
    isDefault: true,
    inputLines: [
      {
        id: 'line-002-1',
        bomId: 'bom-002',
        lineNumber: 1,
        lineType: 'material',
        inputProductId: 'prod-003', // Cured flower bulk
        quantityPer: 1.1, // 10% overage for rolling loss
        uom: 'g',
        isOptional: false,
        isPhantom: false,
        allowSubstitutes: false,
        scrapPercent: 5,
        estimatedCost: 2.75,
      },
      {
        id: 'line-002-2',
        bomId: 'bom-002',
        lineNumber: 2,
        lineType: 'material',
        inputProductId: 'prod-010', // Pre-roll tube
        quantityPer: 1,
        uom: 'ea',
        isOptional: false,
        isPhantom: false,
        allowSubstitutes: false,
        scrapPercent: 0,
        estimatedCost: 0.35,
      },
    ],
    byproductLines: [],
    requiresQaApproval: false,
    requiresMetrcReporting: true,
    estimatedMaterialCost: 3.10,
    estimatedLaborCost: 1.25,
    estimatedOverheadCost: 0.47,
    estimatedTotalCost: 4.82,
    estimatedUnitCost: 4.82,
    createdAt: '2025-01-01T00:00:00Z',
    createdBy: 'admin',
    updatedAt: '2025-01-01T00:00:00Z',
    updatedBy: 'admin',
  },
  // Drying conversion BOM
  {
    id: 'bom-003',
    siteId: 'site-1',
    bomNumber: 'BOM-DRY-001',
    name: 'Dry Wet Flower to Cured',
    description: 'Convert wet flower to cured flower (drying process)',
    bomType: 'conversion',
    outputProductId: 'prod-003', // Cured flower bulk
    outputQuantity: 1000, // 1kg output
    outputUom: 'g',
    estimatedDurationMinutes: 20160, // 14 days
    laborHoursPerBatch: 4,
    expectedYieldPercent: 25, // Typical wet-to-dry ratio
    yieldVarianceThreshold: 3,
    version: 1,
    effectiveDate: '2025-01-01',
    status: 'active',
    isDefault: true,
    inputLines: [
      {
        id: 'line-003-1',
        bomId: 'bom-003',
        lineNumber: 1,
        lineType: 'material',
        inputProductId: 'prod-wet', // Wet flower (would need to create)
        quantityPer: 4000, // 4kg wet = 1kg dry
        uom: 'g',
        isOptional: false,
        isPhantom: false,
        allowSubstitutes: false,
        scrapPercent: 0,
        estimatedCost: 2000, // $0.50/g wet
      },
    ],
    byproductLines: [
      {
        id: 'bp-003-1',
        bomId: 'bom-003',
        lineNumber: 1,
        byproductProductId: 'prod-005', // Trim
        expectedQuantityPer: 200, // 200g trim per kg output
        uom: 'g',
        quantityVariancePercent: 20,
        isRecoverable: true,
        defaultDisposition: 'inventory',
        estimatedValue: 100, // $0.50/g
        costAllocationPercent: 5,
      },
    ],
    requiresQaApproval: true,
    requiresMetrcReporting: true,
    estimatedMaterialCost: 2000,
    estimatedLaborCost: 100,
    estimatedOverheadCost: 300,
    estimatedTotalCost: 2400,
    estimatedUnitCost: 2.40,
    createdAt: '2025-01-01T00:00:00Z',
    createdBy: 'admin',
    updatedAt: '2025-01-01T00:00:00Z',
    updatedBy: 'admin',
  },
  // Extraction BOM
  {
    id: 'bom-004',
    siteId: 'site-1',
    bomNumber: 'BOM-EXT-DIST',
    name: 'Extract Distillate from Trim',
    description: 'Hydrocarbon extraction to produce THC distillate',
    bomType: 'processing',
    outputProductId: 'prod-004', // Distillate
    outputQuantity: 100, // 100g output
    outputUom: 'g',
    estimatedDurationMinutes: 480, // 8 hours
    laborHoursPerBatch: 6,
    expectedYieldPercent: 15, // 15% extraction yield
    yieldVarianceThreshold: 2,
    version: 1,
    effectiveDate: '2025-01-01',
    status: 'active',
    isDefault: true,
    inputLines: [
      {
        id: 'line-004-1',
        bomId: 'bom-004',
        lineNumber: 1,
        lineType: 'material',
        inputProductId: 'prod-005', // Trim
        quantityPer: 666, // ~666g trim = 100g distillate at 15% yield
        uom: 'g',
        isOptional: false,
        isPhantom: false,
        allowSubstitutes: true,
        substituteProductIds: ['prod-shake'],
        scrapPercent: 0,
        estimatedCost: 333, // $0.50/g trim
      },
    ],
    byproductLines: [],
    requiresQaApproval: true,
    qaCheckpoints: [
      {
        id: 'qa-004-1',
        name: 'Residual Solvent Test',
        checkType: 'lab_test',
        isRequired: true,
        failAction: 'reject',
      },
    ],
    requiresMetrcReporting: true,
    estimatedMaterialCost: 333,
    estimatedLaborCost: 150,
    estimatedOverheadCost: 100,
    estimatedTotalCost: 583,
    estimatedUnitCost: 5.83,
    createdAt: '2025-01-01T00:00:00Z',
    createdBy: 'admin',
    updatedAt: '2025-01-01T00:00:00Z',
    updatedBy: 'admin',
  },
];

export default BomService;

