/**
 * Production Order Service
 * API operations for production order management
 */

import type {
  ProductionOrder,
  ProductionOrderStatus,
  ProductionOrderFilterOptions,
  CreateProductionOrderRequest,
  UpdateProductionOrderRequest,
  StartProductionRequest,
  CompleteProductionRequest,
  AllocateMaterialsRequest,
  IssueMaterialsRequest,
  ProductionOrderListResponse,
  ProductionSchedule,
  ProductionDashboardSummary,
} from '../types';

/**
 * Production Order Service Class
 * Handles all production order CRUD and workflow operations
 */
export class ProductionService {
  /**
   * Get paginated list of production orders
   */
  static async getProductionOrders(
    filters?: ProductionOrderFilterOptions,
    page = 1,
    pageSize = 25
  ): Promise<ProductionOrderListResponse> {
    let items = [...MOCK_PRODUCTION_ORDERS];

    // Apply filters
    if (filters?.status?.length) {
      items = items.filter((o) => filters.status!.includes(o.status));
    }
    if (filters?.priority?.length) {
      items = items.filter((o) => filters.priority!.includes(o.priority));
    }
    if (filters?.outputProductId) {
      items = items.filter((o) => o.outputProductId === filters.outputProductId);
    }
    if (filters?.search) {
      const search = filters.search.toLowerCase();
      items = items.filter(
        (o) =>
          o.orderNumber.toLowerCase().includes(search) ||
          o.description?.toLowerCase().includes(search)
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
   * Get single production order by ID
   */
  static async getProductionOrder(id: string): Promise<ProductionOrder | null> {
    return MOCK_PRODUCTION_ORDERS.find((o) => o.id === id) || null;
  }

  /**
   * Create new production order
   */
  static async createProductionOrder(
    request: CreateProductionOrderRequest
  ): Promise<ProductionOrder> {
    console.log('Creating production order:', request);

    const orderNumber = `PO-${new Date().getFullYear()}-${String(Date.now()).slice(-6)}`;

    const newOrder: ProductionOrder = {
      id: `po-${Date.now()}`,
      siteId: 'site-1',
      orderNumber,
      bomId: request.bomId,
      bomVersion: 1,
      outputProductId: '', // Would be fetched from BOM
      plannedQuantity: request.plannedQuantity,
      plannedUom: 'ea',
      status: 'draft',
      priority: request.priority || 'normal',
      plannedStartDate: request.plannedStartDate,
      plannedEndDate: request.plannedEndDate || request.plannedStartDate,
      progressPercent: 0,
      estimatedLaborHours: 0,
      actualLaborHours: 0,
      laborEntries: [],
      expectedYieldPercent: 100,
      requiresQaRelease: true,
      metrcReported: false,
      syncStatus: 'pending',
      estimatedCost: {
        materialCost: 0,
        laborCost: 0,
        overheadCost: 0,
        byproductCredit: 0,
        wasteCost: 0,
        totalCost: 0,
        unitCost: 0,
      },
      materialLines: [],
      outputLots: [],
      byproductLots: [],
      wasteEntries: [],
      notes: request.notes,
      createdAt: new Date().toISOString(),
      createdBy: 'current-user',
      updatedAt: new Date().toISOString(),
      updatedBy: 'current-user',
    };

    return newOrder;
  }

  /**
   * Update production order
   */
  static async updateProductionOrder(
    request: UpdateProductionOrderRequest
  ): Promise<ProductionOrder> {
    const existing = await this.getProductionOrder(request.id);
    if (!existing) {
      throw new Error('Production order not found');
    }

    return {
      ...existing,
      ...request,
      updatedAt: new Date().toISOString(),
      updatedBy: 'current-user',
    };
  }

  /**
   * Update order status
   */
  static async updateStatus(
    id: string,
    status: ProductionOrderStatus,
    notes?: string
  ): Promise<ProductionOrder> {
    const existing = await this.getProductionOrder(id);
    if (!existing) {
      throw new Error('Production order not found');
    }

    return {
      ...existing,
      status,
      notes: notes || existing.notes,
      updatedAt: new Date().toISOString(),
      updatedBy: 'current-user',
    };
  }

  /**
   * Allocate materials to order
   */
  static async allocateMaterials(request: AllocateMaterialsRequest): Promise<ProductionOrder> {
    console.log('Allocating materials:', request);
    const order = await this.getProductionOrder(request.productionOrderId);
    if (!order) {
      throw new Error('Production order not found');
    }

    // Update material lines with allocations
    const updatedLines = order.materialLines.map((line) => {
      const allocation = request.allocations.find((a) => a.materialLineId === line.id);
      if (allocation) {
        return {
          ...line,
          status: 'allocated' as const,
          allocatedLotId: allocation.lotId,
          allocatedQuantity: allocation.quantity,
          allocatedAt: new Date().toISOString(),
          allocatedBy: 'current-user',
        };
      }
      return line;
    });

    // Check if all materials are allocated
    const allAllocated = updatedLines.every(
      (line) => line.status === 'allocated' || line.isOptional
    );

    return {
      ...order,
      materialLines: updatedLines,
      status: allAllocated ? 'ready' : 'pending_materials',
      updatedAt: new Date().toISOString(),
      updatedBy: 'current-user',
    };
  }

  /**
   * Issue materials (consume inventory)
   */
  static async issueMaterials(request: IssueMaterialsRequest): Promise<ProductionOrder> {
    console.log('Issuing materials:', request);
    const order = await this.getProductionOrder(request.productionOrderId);
    if (!order) {
      throw new Error('Production order not found');
    }

    const updatedLines = order.materialLines.map((line) => {
      const issue = request.issues.find((i) => i.materialLineId === line.id);
      if (issue) {
        return {
          ...line,
          status: 'issued' as const,
          issuedQuantity: issue.quantity,
          issuedAt: new Date().toISOString(),
          issuedBy: 'current-user',
        };
      }
      return line;
    });

    return {
      ...order,
      materialLines: updatedLines,
      updatedAt: new Date().toISOString(),
      updatedBy: 'current-user',
    };
  }

  /**
   * Start production
   */
  static async startProduction(request: StartProductionRequest): Promise<ProductionOrder> {
    console.log('Starting production:', request);
    const order = await this.getProductionOrder(request.productionOrderId);
    if (!order) {
      throw new Error('Production order not found');
    }

    return {
      ...order,
      status: 'in_progress',
      actualStartDate: request.actualStartDate || new Date().toISOString(),
      startedBy: 'current-user',
      updatedAt: new Date().toISOString(),
      updatedBy: 'current-user',
    };
  }

  /**
   * Complete production
   */
  static async completeProduction(request: CompleteProductionRequest): Promise<ProductionOrder> {
    console.log('Completing production:', request);
    const order = await this.getProductionOrder(request.productionOrderId);
    if (!order) {
      throw new Error('Production order not found');
    }

    const variance = request.actualQuantity - order.plannedQuantity;
    const variancePercent = (variance / order.plannedQuantity) * 100;

    return {
      ...order,
      status: order.requiresQaRelease ? 'pending_qa' : 'completed',
      actualQuantity: request.actualQuantity,
      varianceQuantity: variance,
      variancePercent,
      actualYieldPercent: (request.actualQuantity / order.plannedQuantity) * 100,
      actualEndDate: new Date().toISOString(),
      completedBy: 'current-user',
      notes: request.notes || order.notes,
      updatedAt: new Date().toISOString(),
      updatedBy: 'current-user',
    };
  }

  /**
   * Release from QA
   */
  static async releaseFromQa(
    id: string,
    approved: boolean,
    notes?: string
  ): Promise<ProductionOrder> {
    const order = await this.getProductionOrder(id);
    if (!order) {
      throw new Error('Production order not found');
    }

    return {
      ...order,
      status: approved ? 'completed' : 'on_hold',
      qaStatus: approved ? 'approved' : 'rejected',
      qaReleasedAt: new Date().toISOString(),
      qaReleasedBy: 'current-user',
      qaNotes: notes,
      updatedAt: new Date().toISOString(),
      updatedBy: 'current-user',
    };
  }

  /**
   * Cancel production order
   */
  static async cancelOrder(id: string, reason: string): Promise<ProductionOrder> {
    const order = await this.getProductionOrder(id);
    if (!order) {
      throw new Error('Production order not found');
    }

    return {
      ...order,
      status: 'cancelled',
      notes: reason,
      cancelledAt: new Date().toISOString(),
      cancelledBy: 'current-user',
      updatedAt: new Date().toISOString(),
      updatedBy: 'current-user',
    };
  }

  /**
   * Get production schedule for date range
   */
  static async getSchedule(startDate: string, endDate: string): Promise<ProductionSchedule[]> {
    // TODO: Implement actual schedule calculation
    return [];
  }

  /**
   * Get dashboard summary
   */
  static async getDashboardSummary(): Promise<ProductionDashboardSummary> {
    const orders = MOCK_PRODUCTION_ORDERS;

    const ordersByStatus = orders.reduce((acc, order) => {
      acc[order.status] = (acc[order.status] || 0) + 1;
      return acc;
    }, {} as Record<ProductionOrderStatus, number>);

    return {
      ordersByStatus,
      todaySchedule: {
        plannedOrders: 5,
        completedOrders: 2,
        inProgressOrders: 2,
        plannedQuantity: 500,
        completedQuantity: 180,
      },
      yieldMetrics: {
        averageYield: 96.5,
        targetYield: 98,
        variancePercent: -1.5,
        ordersWithVariance: 3,
      },
      materialStatus: {
        pendingAllocation: 8,
        allocated: 12,
        lowStockAlerts: 2,
      },
      qaStatus: {
        pendingRelease: 3,
        awaitingResults: 5,
        rejectedToday: 0,
      },
    };
  }
}

// ============ Mock Data ============

const MOCK_PRODUCTION_ORDERS: ProductionOrder[] = [
  {
    id: 'po-001',
    siteId: 'site-1',
    orderNumber: 'PO-2025-000001',
    description: 'Package Blue Dream flower',
    bomId: 'bom-001',
    bomVersion: 1,
    outputProductId: 'prod-006',
    plannedQuantity: 100,
    plannedUom: 'ea',
    actualQuantity: 98,
    varianceQuantity: -2,
    variancePercent: -2,
    status: 'completed',
    priority: 'normal',
    plannedStartDate: '2025-01-15T08:00:00Z',
    plannedEndDate: '2025-01-15T16:00:00Z',
    actualStartDate: '2025-01-15T08:15:00Z',
    actualEndDate: '2025-01-15T15:30:00Z',
    progressPercent: 100,
    workCenterId: 'wc-001',
    workCenterName: 'Packaging Line 1',
    estimatedLaborHours: 8,
    actualLaborHours: 7.25,
    laborEntries: [],
    expectedYieldPercent: 100,
    actualYieldPercent: 98,
    requiresQaRelease: true,
    qaStatus: 'approved',
    qaReleasedAt: '2025-01-15T16:00:00Z',
    qaReleasedBy: 'qa-manager',
    metrcReported: true,
    syncStatus: 'synced',
    estimatedCost: {
      materialCost: 960,
      laborCost: 200,
      overheadCost: 144,
      byproductCredit: 0,
      wasteCost: 20,
      totalCost: 1324,
      unitCost: 13.24,
    },
    materialLines: [
      {
        id: 'ml-001-1',
        productionOrderId: 'po-001',
        lineNumber: 1,
        productId: 'prod-003',
        plannedQuantity: 350,
        uom: 'g',
        scrapAllowance: 7,
        status: 'issued',
        allocatedLotId: 'lot-flower-001',
        allocatedQuantity: 350,
        issuedQuantity: 350,
        isSubstitute: false,
      },
    ],
    outputLots: [],
    byproductLots: [],
    wasteEntries: [],
    createdAt: '2025-01-14T10:00:00Z',
    createdBy: 'production-manager',
    updatedAt: '2025-01-15T16:00:00Z',
    updatedBy: 'qa-manager',
    completedBy: 'operator-1',
  },
  {
    id: 'po-002',
    siteId: 'site-1',
    orderNumber: 'PO-2025-000002',
    description: 'Roll pre-rolls batch',
    bomId: 'bom-002',
    bomVersion: 1,
    outputProductId: 'prod-007',
    plannedQuantity: 200,
    plannedUom: 'ea',
    status: 'in_progress',
    priority: 'high',
    plannedStartDate: '2025-01-16T08:00:00Z',
    plannedEndDate: '2025-01-16T14:00:00Z',
    actualStartDate: '2025-01-16T08:30:00Z',
    progressPercent: 65,
    currentStepNumber: 2,
    currentStepName: 'Rolling',
    workCenterId: 'wc-002',
    workCenterName: 'Pre-Roll Station',
    estimatedLaborHours: 6,
    actualLaborHours: 4.5,
    laborEntries: [],
    expectedYieldPercent: 95,
    requiresQaRelease: false,
    metrcReported: false,
    syncStatus: 'pending',
    estimatedCost: {
      materialCost: 620,
      laborCost: 150,
      overheadCost: 93,
      byproductCredit: 0,
      wasteCost: 0,
      totalCost: 863,
      unitCost: 4.32,
    },
    materialLines: [
      {
        id: 'ml-002-1',
        productionOrderId: 'po-002',
        lineNumber: 1,
        productId: 'prod-003',
        plannedQuantity: 220,
        uom: 'g',
        scrapAllowance: 11,
        status: 'issued',
        allocatedLotId: 'lot-flower-002',
        allocatedQuantity: 220,
        issuedQuantity: 220,
        isSubstitute: false,
      },
    ],
    outputLots: [],
    byproductLots: [],
    wasteEntries: [],
    createdAt: '2025-01-15T14:00:00Z',
    createdBy: 'production-manager',
    updatedAt: '2025-01-16T12:30:00Z',
    updatedBy: 'operator-2',
  },
  {
    id: 'po-003',
    siteId: 'site-1',
    orderNumber: 'PO-2025-000003',
    description: 'Extraction run - Distillate',
    bomId: 'bom-004',
    bomVersion: 1,
    outputProductId: 'prod-004',
    plannedQuantity: 100,
    plannedUom: 'g',
    status: 'pending_materials',
    priority: 'normal',
    plannedStartDate: '2025-01-18T08:00:00Z',
    plannedEndDate: '2025-01-18T16:00:00Z',
    progressPercent: 0,
    workCenterId: 'wc-003',
    workCenterName: 'Extraction Lab',
    estimatedLaborHours: 8,
    actualLaborHours: 0,
    laborEntries: [],
    expectedYieldPercent: 15,
    requiresQaRelease: true,
    metrcReported: false,
    syncStatus: 'pending',
    estimatedCost: {
      materialCost: 333,
      laborCost: 150,
      overheadCost: 100,
      byproductCredit: 0,
      wasteCost: 0,
      totalCost: 583,
      unitCost: 5.83,
    },
    materialLines: [
      {
        id: 'ml-003-1',
        productionOrderId: 'po-003',
        lineNumber: 1,
        productId: 'prod-005',
        plannedQuantity: 666,
        uom: 'g',
        scrapAllowance: 0,
        status: 'pending',
        isSubstitute: false,
      },
    ],
    outputLots: [],
    byproductLots: [],
    wasteEntries: [],
    createdAt: '2025-01-16T10:00:00Z',
    createdBy: 'production-manager',
    updatedAt: '2025-01-16T10:00:00Z',
    updatedBy: 'production-manager',
  },
];

export default ProductionService;

