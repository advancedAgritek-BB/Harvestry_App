/**
 * Product Service
 * API operations for product catalog (SKU) management
 */

import type {
  Product,
  ProductWithInventory,
  ProductFilterOptions,
  CreateProductRequest,
  UpdateProductRequest,
  ProductListResponse,
  ProductCategorySummary,
  InventoryCategory,
} from '../types';

// API base path - will be replaced with actual API when backend is ready
const API_BASE = '/api/inventory/products';

/**
 * Product Service Class
 * Handles all product catalog CRUD operations
 */
export class ProductService {
  /**
   * Get paginated list of products
   */
  static async getProducts(
    filters?: ProductFilterOptions,
    page = 1,
    pageSize = 25,
    sortBy = 'name',
    sortDirection: 'asc' | 'desc' = 'asc'
  ): Promise<ProductListResponse> {
    // TODO: Replace with actual API call when backend is ready
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
      sortBy,
      sortDirection,
      ...(filters?.siteId && { siteId: filters.siteId }),
      ...(filters?.search && { search: filters.search }),
    });

    if (filters?.category?.length) {
      params.append('category', filters.category.join(','));
    }
    if (filters?.productType?.length) {
      params.append('productType', filters.productType.join(','));
    }
    if (filters?.status?.length) {
      params.append('status', filters.status.join(','));
    }

    // For now, return mock data
    return this.getMockProducts(filters, page, pageSize);
  }

  /**
   * Get single product by ID
   */
  static async getProduct(id: string): Promise<Product | null> {
    // TODO: Replace with actual API call
    const mockProducts = await this.getMockProductList();
    return mockProducts.find((p) => p.id === id) || null;
  }

  /**
   * Get product with inventory summary
   */
  static async getProductWithInventory(id: string): Promise<ProductWithInventory | null> {
    const product = await this.getProduct(id);
    if (!product) return null;

    // TODO: Fetch actual inventory data
    return {
      ...product,
      inventory: {
        totalQuantity: Math.floor(Math.random() * 10000),
        availableQuantity: Math.floor(Math.random() * 8000),
        reservedQuantity: Math.floor(Math.random() * 2000),
        onHandValue: Math.floor(Math.random() * 50000),
        lotCount: Math.floor(Math.random() * 50),
        locationCount: Math.floor(Math.random() * 10),
      },
    };
  }

  /**
   * Create new product
   */
  static async createProduct(request: CreateProductRequest): Promise<Product> {
    // TODO: Replace with actual API call
    console.log('Creating product:', request);

    const newProduct: Product = {
      id: `prod-${Date.now()}`,
      siteId: 'site-1',
      sku: request.sku,
      name: request.name,
      description: request.description,
      category: request.category,
      productType: request.productType,
      strainId: request.strainId,
      defaultUom: request.defaultUom,
      requiresCoa: request.requiresCoa ?? true,
      shelfLifeDays: request.shelfLifeDays,
      costMethod: request.costMethod ?? 'average',
      standardCost: request.standardCost,
      status: 'active',
      isSellable: request.isSellable ?? false,
      isPurchasable: request.isPurchasable ?? false,
      isProducible: request.isProducible ?? true,
      isLotTracked: true,
      isSerialTracked: false,
      attributes: request.attributes,
      createdAt: new Date().toISOString(),
      createdBy: 'current-user',
      updatedAt: new Date().toISOString(),
      updatedBy: 'current-user',
    };

    return newProduct;
  }

  /**
   * Update existing product
   */
  static async updateProduct(request: UpdateProductRequest): Promise<Product> {
    // TODO: Replace with actual API call
    console.log('Updating product:', request);

    const existing = await this.getProduct(request.id);
    if (!existing) {
      throw new Error('Product not found');
    }

    return {
      ...existing,
      ...request,
      updatedAt: new Date().toISOString(),
      updatedBy: 'current-user',
    };
  }

  /**
   * Delete product (soft delete - sets status to discontinued)
   */
  static async deleteProduct(id: string): Promise<void> {
    // TODO: Replace with actual API call
    console.log('Deleting product:', id);
  }

  /**
   * Get category summary
   */
  static async getCategorySummary(siteId?: string): Promise<ProductCategorySummary[]> {
    // TODO: Replace with actual API call
    return [
      { category: 'raw_material', productCount: 45, totalValue: 125000, lotCount: 156 },
      { category: 'work_in_progress', productCount: 28, totalValue: 340000, lotCount: 89 },
      { category: 'finished_good', productCount: 82, totalValue: 280000, lotCount: 234 },
      { category: 'consumable', productCount: 35, totalValue: 15000, lotCount: 45 },
      { category: 'byproduct', productCount: 12, totalValue: 45000, lotCount: 38 },
    ];
  }

  /**
   * Get products by category
   */
  static async getProductsByCategory(category: InventoryCategory): Promise<Product[]> {
    const response = await this.getProducts({ category: [category] });
    return response.items;
  }

  /**
   * Search products
   */
  static async searchProducts(query: string, limit = 10): Promise<Product[]> {
    const response = await this.getProducts({ search: query }, 1, limit);
    return response.items;
  }

  /**
   * Check if SKU is available
   */
  static async checkSkuAvailability(sku: string, excludeId?: string): Promise<boolean> {
    // TODO: Replace with actual API call
    const existing = await this.getMockProductList();
    return !existing.some((p) => p.sku === sku && p.id !== excludeId);
  }

  // ============ Mock Data Helpers ============

  private static async getMockProducts(
    filters?: ProductFilterOptions,
    page = 1,
    pageSize = 25
  ): Promise<ProductListResponse> {
    let items = await this.getMockProductList();

    // Apply filters
    if (filters?.category?.length) {
      items = items.filter((p) => filters.category!.includes(p.category));
    }
    if (filters?.productType?.length) {
      items = items.filter((p) => filters.productType!.includes(p.productType));
    }
    if (filters?.search) {
      const search = filters.search.toLowerCase();
      items = items.filter(
        (p) =>
          p.name.toLowerCase().includes(search) ||
          p.sku.toLowerCase().includes(search) ||
          p.strainName?.toLowerCase().includes(search)
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

  private static async getMockProductList(): Promise<Product[]> {
    return MOCK_PRODUCTS;
  }
}

// ============ Mock Data ============

const MOCK_PRODUCTS: Product[] = [
  // Raw Materials
  {
    id: 'prod-001',
    siteId: 'site-1',
    sku: 'SEED-BD-001',
    name: 'Blue Dream Seeds',
    description: 'Premium Blue Dream feminized seeds',
    category: 'raw_material',
    productType: 'seed',
    strainId: 'strain-001',
    strainName: 'Blue Dream',
    defaultUom: 'ea',
    requiresCoa: false,
    costMethod: 'average',
    standardCost: 15.0,
    status: 'active',
    isSellable: false,
    isPurchasable: true,
    isProducible: false,
    isLotTracked: true,
    isSerialTracked: false,
    createdAt: '2025-01-01T00:00:00Z',
    createdBy: 'admin',
    updatedAt: '2025-01-01T00:00:00Z',
    updatedBy: 'admin',
  },
  {
    id: 'prod-002',
    siteId: 'site-1',
    sku: 'CLONE-OG-001',
    name: 'OG Kush Clone',
    description: 'Rooted OG Kush cutting',
    category: 'raw_material',
    productType: 'clone',
    strainId: 'strain-002',
    strainName: 'OG Kush',
    defaultUom: 'ea',
    requiresCoa: false,
    costMethod: 'average',
    standardCost: 8.0,
    status: 'active',
    isSellable: false,
    isPurchasable: true,
    isProducible: true,
    isLotTracked: true,
    isSerialTracked: false,
    createdAt: '2025-01-01T00:00:00Z',
    createdBy: 'admin',
    updatedAt: '2025-01-01T00:00:00Z',
    updatedBy: 'admin',
  },
  // Work in Progress
  {
    id: 'prod-003',
    siteId: 'site-1',
    sku: 'WIP-FLOWER-BD',
    name: 'Blue Dream Cured Flower (Bulk)',
    description: 'Cured Blue Dream flower ready for packaging',
    category: 'work_in_progress',
    productType: 'cured_flower',
    strainId: 'strain-001',
    strainName: 'Blue Dream',
    defaultUom: 'g',
    requiresCoa: true,
    costMethod: 'average',
    standardCost: 2.5,
    status: 'active',
    isSellable: false,
    isPurchasable: false,
    isProducible: true,
    isLotTracked: true,
    isSerialTracked: false,
    createdAt: '2025-01-01T00:00:00Z',
    createdBy: 'admin',
    updatedAt: '2025-01-01T00:00:00Z',
    updatedBy: 'admin',
  },
  {
    id: 'prod-004',
    siteId: 'site-1',
    sku: 'WIP-DIST-001',
    name: 'THC Distillate (Bulk)',
    description: 'Refined THC distillate for infusion',
    category: 'work_in_progress',
    productType: 'distillate',
    defaultUom: 'g',
    requiresCoa: true,
    costMethod: 'average',
    standardCost: 8.0,
    status: 'active',
    isSellable: false,
    isPurchasable: false,
    isProducible: true,
    isLotTracked: true,
    isSerialTracked: false,
    createdAt: '2025-01-01T00:00:00Z',
    createdBy: 'admin',
    updatedAt: '2025-01-01T00:00:00Z',
    updatedBy: 'admin',
  },
  // Byproducts
  {
    id: 'prod-005',
    siteId: 'site-1',
    sku: 'TRIM-BD-001',
    name: 'Blue Dream Trim',
    description: 'Trim from Blue Dream harvest',
    category: 'byproduct',
    productType: 'trim',
    strainId: 'strain-001',
    strainName: 'Blue Dream',
    defaultUom: 'g',
    requiresCoa: false,
    costMethod: 'average',
    standardCost: 0.5,
    status: 'active',
    isSellable: false,
    isPurchasable: false,
    isProducible: true,
    isLotTracked: true,
    isSerialTracked: false,
    createdAt: '2025-01-01T00:00:00Z',
    createdBy: 'admin',
    updatedAt: '2025-01-01T00:00:00Z',
    updatedBy: 'admin',
  },
  // Finished Goods
  {
    id: 'prod-006',
    siteId: 'site-1',
    sku: 'FG-BD-3.5G',
    name: 'Blue Dream 3.5g',
    description: 'Packaged Blue Dream flower, 3.5g jar',
    category: 'finished_good',
    productType: 'packaged_flower',
    strainId: 'strain-001',
    strainName: 'Blue Dream',
    defaultUom: 'ea',
    netWeight: 3.5,
    netWeightUom: 'g',
    requiresCoa: true,
    shelfLifeDays: 365,
    costMethod: 'average',
    standardCost: 12.0,
    listPrice: 35.0,
    wholesalePrice: 20.0,
    status: 'active',
    isSellable: true,
    isPurchasable: false,
    isProducible: true,
    isLotTracked: true,
    isSerialTracked: false,
    createdAt: '2025-01-01T00:00:00Z',
    createdBy: 'admin',
    updatedAt: '2025-01-01T00:00:00Z',
    updatedBy: 'admin',
  },
  {
    id: 'prod-007',
    siteId: 'site-1',
    sku: 'FG-PR-BD-1G',
    name: 'Blue Dream Pre-Roll 1g',
    description: 'Single 1g pre-roll, Blue Dream',
    category: 'finished_good',
    productType: 'preroll',
    strainId: 'strain-001',
    strainName: 'Blue Dream',
    defaultUom: 'ea',
    netWeight: 1.0,
    netWeightUom: 'g',
    requiresCoa: true,
    shelfLifeDays: 180,
    costMethod: 'average',
    standardCost: 4.0,
    listPrice: 12.0,
    wholesalePrice: 6.0,
    status: 'active',
    isSellable: true,
    isPurchasable: false,
    isProducible: true,
    isLotTracked: true,
    isSerialTracked: false,
    createdAt: '2025-01-01T00:00:00Z',
    createdBy: 'admin',
    updatedAt: '2025-01-01T00:00:00Z',
    updatedBy: 'admin',
  },
  {
    id: 'prod-008',
    siteId: 'site-1',
    sku: 'FG-CART-1G',
    name: 'THC Vape Cartridge 1g',
    description: '1g distillate vape cartridge',
    category: 'finished_good',
    productType: 'vape_cartridge',
    defaultUom: 'ea',
    netWeight: 1.0,
    netWeightUom: 'g',
    requiresCoa: true,
    shelfLifeDays: 365,
    costMethod: 'average',
    standardCost: 15.0,
    listPrice: 45.0,
    wholesalePrice: 25.0,
    status: 'active',
    isSellable: true,
    isPurchasable: false,
    isProducible: true,
    isLotTracked: true,
    isSerialTracked: false,
    createdAt: '2025-01-01T00:00:00Z',
    createdBy: 'admin',
    updatedAt: '2025-01-01T00:00:00Z',
    updatedBy: 'admin',
  },
  // Consumables
  {
    id: 'prod-009',
    siteId: 'site-1',
    sku: 'PKG-JAR-3.5',
    name: 'Glass Jar 3.5g (CR)',
    description: 'Child-resistant glass jar for 3.5g flower',
    category: 'consumable',
    productType: 'packaging_material',
    defaultUom: 'ea',
    requiresCoa: false,
    costMethod: 'average',
    standardCost: 0.85,
    status: 'active',
    isSellable: false,
    isPurchasable: true,
    isProducible: false,
    isLotTracked: true,
    isSerialTracked: false,
    createdAt: '2025-01-01T00:00:00Z',
    createdBy: 'admin',
    updatedAt: '2025-01-01T00:00:00Z',
    updatedBy: 'admin',
  },
  {
    id: 'prod-010',
    siteId: 'site-1',
    sku: 'PKG-TUBE-PR',
    name: 'Pre-Roll Tube (CR)',
    description: 'Child-resistant tube for pre-rolls',
    category: 'consumable',
    productType: 'packaging_material',
    defaultUom: 'ea',
    requiresCoa: false,
    costMethod: 'average',
    standardCost: 0.35,
    status: 'active',
    isSellable: false,
    isPurchasable: true,
    isProducible: false,
    isLotTracked: true,
    isSerialTracked: false,
    createdAt: '2025-01-01T00:00:00Z',
    createdBy: 'admin',
    updatedAt: '2025-01-01T00:00:00Z',
    updatedBy: 'admin',
  },
];

export default ProductService;

