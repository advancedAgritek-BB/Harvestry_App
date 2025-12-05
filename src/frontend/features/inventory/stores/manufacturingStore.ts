/**
 * Manufacturing Store
 * Zustand store for product catalog, BOMs, and production orders
 */

import { create } from 'zustand';
import { devtools } from 'zustand/middleware';
import type {
  Product,
  ProductFilterOptions,
  ProductCategorySummary,
  InventoryCategory,
  BillOfMaterials,
  BomFilterOptions,
  ProductionOrder,
  ProductionOrderFilterOptions,
  ProductionOrderStatus,
  ProductionDashboardSummary,
} from '../types';

/** View mode for lists */
export type ViewMode = 'table' | 'grid' | 'kanban';

/** Manufacturing store state */
interface ManufacturingState {
  // ===== PRODUCTS =====
  products: Product[];
  selectedProductId: string | null;
  productFilters: ProductFilterOptions;
  productCategorySummary: ProductCategorySummary[];
  productsLoading: boolean;
  productsError: string | null;

  // ===== BOMS =====
  boms: BillOfMaterials[];
  selectedBomId: string | null;
  bomFilters: BomFilterOptions;
  bomsLoading: boolean;
  bomsError: string | null;
  bomEditorOpen: boolean;
  bomEditorMode: 'create' | 'edit' | 'view';

  // ===== PRODUCTION ORDERS =====
  productionOrders: ProductionOrder[];
  selectedOrderId: string | null;
  orderFilters: ProductionOrderFilterOptions;
  ordersLoading: boolean;
  ordersError: string | null;
  productionSummary: ProductionDashboardSummary | null;

  // ===== UI STATE =====
  viewMode: ViewMode;
  activeTab: 'products' | 'boms' | 'production' | 'batches';
  sidebarCollapsed: boolean;
  
  // Product modals
  productModalOpen: boolean;
  productModalMode: 'create' | 'edit';
  
  // Production modals
  createOrderModalOpen: boolean;
  allocateMaterialsModalOpen: boolean;
  completeOrderModalOpen: boolean;

  // ===== ACTIONS =====
  // Products
  setProducts: (products: Product[]) => void;
  addProduct: (product: Product) => void;
  updateProduct: (product: Product) => void;
  removeProduct: (id: string) => void;
  selectProduct: (id: string | null) => void;
  setProductFilters: (filters: ProductFilterOptions) => void;
  setProductCategorySummary: (summary: ProductCategorySummary[]) => void;
  setProductsLoading: (loading: boolean) => void;
  setProductsError: (error: string | null) => void;

  // BOMs
  setBoms: (boms: BillOfMaterials[]) => void;
  addBom: (bom: BillOfMaterials) => void;
  updateBom: (bom: BillOfMaterials) => void;
  removeBom: (id: string) => void;
  selectBom: (id: string | null) => void;
  setBomFilters: (filters: BomFilterOptions) => void;
  setBomsLoading: (loading: boolean) => void;
  setBomsError: (error: string | null) => void;
  openBomEditor: (mode: 'create' | 'edit' | 'view', bomId?: string) => void;
  closeBomEditor: () => void;

  // Production Orders
  setProductionOrders: (orders: ProductionOrder[]) => void;
  addProductionOrder: (order: ProductionOrder) => void;
  updateProductionOrder: (order: ProductionOrder) => void;
  removeProductionOrder: (id: string) => void;
  selectOrder: (id: string | null) => void;
  setOrderFilters: (filters: ProductionOrderFilterOptions) => void;
  setOrdersLoading: (loading: boolean) => void;
  setOrdersError: (error: string | null) => void;
  setProductionSummary: (summary: ProductionDashboardSummary | null) => void;

  // UI
  setViewMode: (mode: ViewMode) => void;
  setActiveTab: (tab: 'products' | 'boms' | 'production' | 'batches') => void;
  toggleSidebar: () => void;
  openProductModal: (mode: 'create' | 'edit') => void;
  closeProductModal: () => void;
  openCreateOrderModal: () => void;
  closeCreateOrderModal: () => void;
  openAllocateMaterialsModal: () => void;
  closeAllocateMaterialsModal: () => void;
  openCompleteOrderModal: () => void;
  closeCompleteOrderModal: () => void;

  // Reset
  reset: () => void;
}

/** Initial state */
const initialState = {
  // Products
  products: [],
  selectedProductId: null,
  productFilters: {},
  productCategorySummary: [],
  productsLoading: false,
  productsError: null,

  // BOMs
  boms: [],
  selectedBomId: null,
  bomFilters: {},
  bomsLoading: false,
  bomsError: null,
  bomEditorOpen: false,
  bomEditorMode: 'create' as const,

  // Production Orders
  productionOrders: [],
  selectedOrderId: null,
  orderFilters: {},
  ordersLoading: false,
  ordersError: null,
  productionSummary: null,

  // UI
  viewMode: 'table' as ViewMode,
  activeTab: 'products' as const,
  sidebarCollapsed: false,
  productModalOpen: false,
  productModalMode: 'create' as const,
  createOrderModalOpen: false,
  allocateMaterialsModalOpen: false,
  completeOrderModalOpen: false,
};

/**
 * Manufacturing Store
 */
export const useManufacturingStore = create<ManufacturingState>()(
  devtools(
    (set, get) => ({
      ...initialState,

      // ===== PRODUCT ACTIONS =====
      setProducts: (products) => set({ products }),
      
      addProduct: (product) =>
        set((state) => ({ products: [...state.products, product] })),
      
      updateProduct: (product) =>
        set((state) => ({
          products: state.products.map((p) =>
            p.id === product.id ? product : p
          ),
        })),
      
      removeProduct: (id) =>
        set((state) => ({
          products: state.products.filter((p) => p.id !== id),
          selectedProductId:
            state.selectedProductId === id ? null : state.selectedProductId,
        })),
      
      selectProduct: (id) => set({ selectedProductId: id }),
      
      setProductFilters: (filters) => set({ productFilters: filters }),
      
      setProductCategorySummary: (summary) =>
        set({ productCategorySummary: summary }),
      
      setProductsLoading: (loading) => set({ productsLoading: loading }),
      
      setProductsError: (error) => set({ productsError: error }),

      // ===== BOM ACTIONS =====
      setBoms: (boms) => set({ boms }),
      
      addBom: (bom) => set((state) => ({ boms: [...state.boms, bom] })),
      
      updateBom: (bom) =>
        set((state) => ({
          boms: state.boms.map((b) => (b.id === bom.id ? bom : b)),
        })),
      
      removeBom: (id) =>
        set((state) => ({
          boms: state.boms.filter((b) => b.id !== id),
          selectedBomId: state.selectedBomId === id ? null : state.selectedBomId,
        })),
      
      selectBom: (id) => set({ selectedBomId: id }),
      
      setBomFilters: (filters) => set({ bomFilters: filters }),
      
      setBomsLoading: (loading) => set({ bomsLoading: loading }),
      
      setBomsError: (error) => set({ bomsError: error }),
      
      openBomEditor: (mode, bomId) =>
        set({
          bomEditorOpen: true,
          bomEditorMode: mode,
          selectedBomId: bomId || null,
        }),
      
      closeBomEditor: () =>
        set({ bomEditorOpen: false, selectedBomId: null }),

      // ===== PRODUCTION ORDER ACTIONS =====
      setProductionOrders: (orders) => set({ productionOrders: orders }),
      
      addProductionOrder: (order) =>
        set((state) => ({ productionOrders: [...state.productionOrders, order] })),
      
      updateProductionOrder: (order) =>
        set((state) => ({
          productionOrders: state.productionOrders.map((o) =>
            o.id === order.id ? order : o
          ),
        })),
      
      removeProductionOrder: (id) =>
        set((state) => ({
          productionOrders: state.productionOrders.filter((o) => o.id !== id),
          selectedOrderId:
            state.selectedOrderId === id ? null : state.selectedOrderId,
        })),
      
      selectOrder: (id) => set({ selectedOrderId: id }),
      
      setOrderFilters: (filters) => set({ orderFilters: filters }),
      
      setOrdersLoading: (loading) => set({ ordersLoading: loading }),
      
      setOrdersError: (error) => set({ ordersError: error }),
      
      setProductionSummary: (summary) => set({ productionSummary: summary }),

      // ===== UI ACTIONS =====
      setViewMode: (mode) => set({ viewMode: mode }),
      
      setActiveTab: (tab) => set({ activeTab: tab }),
      
      toggleSidebar: () =>
        set((state) => ({ sidebarCollapsed: !state.sidebarCollapsed })),
      
      openProductModal: (mode) =>
        set({ productModalOpen: true, productModalMode: mode }),
      
      closeProductModal: () =>
        set({ productModalOpen: false }),
      
      openCreateOrderModal: () => set({ createOrderModalOpen: true }),
      
      closeCreateOrderModal: () => set({ createOrderModalOpen: false }),
      
      openAllocateMaterialsModal: () =>
        set({ allocateMaterialsModalOpen: true }),
      
      closeAllocateMaterialsModal: () =>
        set({ allocateMaterialsModalOpen: false }),
      
      openCompleteOrderModal: () => set({ completeOrderModalOpen: true }),
      
      closeCompleteOrderModal: () => set({ completeOrderModalOpen: false }),

      // ===== RESET =====
      reset: () => set(initialState),
    }),
    { name: 'manufacturing-store' }
  )
);

// ===== SELECTORS =====

/**
 * Get selected product
 */
export const useSelectedProduct = () =>
  useManufacturingStore((state) =>
    state.products.find((p) => p.id === state.selectedProductId) || null
  );

/**
 * Get selected BOM
 */
export const useSelectedBom = () =>
  useManufacturingStore((state) =>
    state.boms.find((b) => b.id === state.selectedBomId) || null
  );

/**
 * Get selected production order
 */
export const useSelectedOrder = () =>
  useManufacturingStore((state) =>
    state.productionOrders.find((o) => o.id === state.selectedOrderId) || null
  );

/**
 * Get products by category
 */
export const useProductsByCategory = (category: InventoryCategory) =>
  useManufacturingStore((state) =>
    state.products.filter((p) => p.category === category)
  );

/**
 * Get production orders by status
 */
export const useOrdersByStatus = (status: ProductionOrderStatus) =>
  useManufacturingStore((state) =>
    state.productionOrders.filter((o) => o.status === status)
  );

/**
 * Get active production orders
 */
export const useActiveOrders = () =>
  useManufacturingStore((state) =>
    state.productionOrders.filter(
      (o) => o.status === 'in_progress' || o.status === 'ready'
    )
  );

/**
 * Get orders pending materials
 */
export const usePendingMaterialOrders = () =>
  useManufacturingStore((state) =>
    state.productionOrders.filter((o) => o.status === 'pending_materials')
  );

/**
 * Get BOMs for a product
 */
export const useBomsForProduct = (productId: string) =>
  useManufacturingStore((state) =>
    state.boms.filter((b) => b.outputProductId === productId)
  );

export default useManufacturingStore;

