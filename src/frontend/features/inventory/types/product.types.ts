/**
 * Product (SKU) Type Definitions
 * Defines products that can be manufactured, purchased, or sold
 */

/** Inventory classification category */
export type InventoryCategory =
  | 'raw_material'
  | 'work_in_progress'
  | 'finished_good'
  | 'consumable'
  | 'byproduct';

/** Cannabis-specific product types */
export type CannabisProductType =
  // Origin materials
  | 'seed'
  | 'clone'
  | 'mother_plant'
  // Cultivation stages
  | 'live_plant'
  | 'wet_flower'
  | 'dry_flower'
  | 'cured_flower'
  // Byproducts
  | 'trim'
  | 'shake'
  | 'stems'
  | 'fan_leaves'
  // Concentrates
  | 'crude_extract'
  | 'distillate'
  | 'live_resin'
  | 'rosin'
  | 'shatter'
  | 'wax'
  | 'budder'
  | 'diamonds'
  | 'hash'
  | 'kief'
  // Finished goods
  | 'preroll'
  | 'infused_preroll'
  | 'edible'
  | 'beverage'
  | 'capsule'
  | 'tincture'
  | 'topical'
  | 'vape_cartridge'
  // Packaged
  | 'packaged_flower'
  | 'packaged_concentrate'
  | 'packaged_edible'
  | 'packaged_preroll'
  // Non-cannabis
  | 'packaging_material'
  | 'label'
  | 'nutrient'
  | 'growing_medium'
  | 'other';

/** Cost calculation method */
export type CostMethod = 'fifo' | 'lifo' | 'average' | 'specific' | 'standard';

/** Product status */
export type ProductStatus = 'active' | 'inactive' | 'discontinued' | 'pending_approval';

/** Unit of measure conversion */
export interface UomConversion {
  fromUom: string;
  toUom: string;
  conversionFactor: number;
}

/** Product attribute definition */
export interface ProductAttribute {
  key: string;
  label: string;
  value: string | number | boolean;
  type: 'text' | 'number' | 'boolean' | 'select';
  options?: string[];
}

/** Product (SKU) definition */
export interface Product {
  id: string;
  siteId: string;

  // Identification
  sku: string;
  name: string;
  description?: string;
  barcode?: string;

  // Classification
  category: InventoryCategory;
  productType: CannabisProductType;

  // Genetics (for cannabis products)
  strainId?: string;
  strainName?: string;

  // Units
  defaultUom: string;
  trackingUom?: string; // For compliance (e.g., always track in grams)
  uomConversions?: UomConversion[];

  // For finished goods - package specifications
  netWeight?: number;
  netWeightUom?: string;
  packageSize?: number;
  packageSizeUom?: string;
  unitsPerCase?: number;

  // Compliance
  requiresCoa: boolean;
  coaTestTypes?: string[];
  shelfLifeDays?: number;
  storageRequirements?: string;
  regulatoryCategory?: string;

  // METRC/BioTrack mapping
  metrcItemCategory?: string;
  metrcUnitOfMeasure?: string;
  biotrackProductType?: string;

  // Costing
  costMethod: CostMethod;
  standardCost?: number;
  lastCost?: number;
  averageCost?: number;

  // Pricing (for finished goods)
  listPrice?: number;
  wholesalePrice?: number;

  // Reorder settings
  reorderPoint?: number;
  reorderQuantity?: number;
  minOrderQuantity?: number;
  maxOrderQuantity?: number;
  leadTimeDays?: number;

  // Flags
  status: ProductStatus;
  isSellable: boolean;
  isPurchasable: boolean;
  isProducible: boolean;
  isLotTracked: boolean;
  isSerialTracked: boolean;

  // Default locations
  defaultReceivingLocationId?: string;
  defaultProductionLocationId?: string;
  defaultStorageLocationId?: string;

  // Custom attributes
  attributes?: ProductAttribute[];

  // Images
  imageUrl?: string;
  thumbnailUrl?: string;

  // Audit
  createdAt: string;
  createdBy: string;
  updatedAt: string;
  updatedBy: string;
}

/** Product with inventory summary */
export interface ProductWithInventory extends Product {
  inventory: {
    totalQuantity: number;
    availableQuantity: number;
    reservedQuantity: number;
    onHandValue: number;
    lotCount: number;
    locationCount: number;
  };
}

/** Product filter options */
export interface ProductFilterOptions {
  siteId?: string;
  category?: InventoryCategory[];
  productType?: CannabisProductType[];
  status?: ProductStatus[];
  strainId?: string;
  isSellable?: boolean;
  isPurchasable?: boolean;
  isProducible?: boolean;
  search?: string;
  hasLowStock?: boolean;
}

/** Create product request */
export interface CreateProductRequest {
  sku: string;
  name: string;
  description?: string;
  category: InventoryCategory;
  productType: CannabisProductType;
  strainId?: string;
  defaultUom: string;
  costMethod?: CostMethod;
  standardCost?: number;
  requiresCoa?: boolean;
  shelfLifeDays?: number;
  isSellable?: boolean;
  isPurchasable?: boolean;
  isProducible?: boolean;
  attributes?: ProductAttribute[];
}

/** Update product request */
export interface UpdateProductRequest extends Partial<CreateProductRequest> {
  id: string;
}

/** Product list response */
export interface ProductListResponse {
  items: Product[];
  total: number;
  page: number;
  pageSize: number;
  hasMore: boolean;
}

/** Product category summary */
export interface ProductCategorySummary {
  category: InventoryCategory;
  productCount: number;
  totalValue: number;
  lotCount: number;
}

/** Category display configuration */
export const CATEGORY_CONFIG: Record<InventoryCategory, {
  label: string;
  description: string;
  color: string;
  bgColor: string;
  icon: string;
}> = {
  raw_material: {
    label: 'Raw Material',
    description: 'Seeds, clones, nutrients, packaging materials',
    color: 'text-blue-400',
    bgColor: 'bg-blue-500/10',
    icon: 'Boxes',
  },
  work_in_progress: {
    label: 'Work in Progress',
    description: 'Plants in cultivation, drying, curing',
    color: 'text-amber-400',
    bgColor: 'bg-amber-500/10',
    icon: 'Clock',
  },
  finished_good: {
    label: 'Finished Good',
    description: 'Packaged products ready for sale',
    color: 'text-emerald-400',
    bgColor: 'bg-emerald-500/10',
    icon: 'PackageCheck',
  },
  consumable: {
    label: 'Consumable',
    description: 'Labels, bags, supplies',
    color: 'text-muted-foreground',
    bgColor: 'bg-muted/50',
    icon: 'Package',
  },
  byproduct: {
    label: 'Byproduct',
    description: 'Trim, shake, stems - can be inputs to other products',
    color: 'text-violet-400',
    bgColor: 'bg-violet-500/10',
    icon: 'Recycle',
  },
};

/** Product type display configuration */
export const PRODUCT_TYPE_CONFIG: Record<CannabisProductType, {
  label: string;
  category: InventoryCategory;
  defaultUom: string;
}> = {
  seed: { label: 'Seed', category: 'raw_material', defaultUom: 'ea' },
  clone: { label: 'Clone', category: 'raw_material', defaultUom: 'ea' },
  mother_plant: { label: 'Mother Plant', category: 'work_in_progress', defaultUom: 'ea' },
  live_plant: { label: 'Live Plant', category: 'work_in_progress', defaultUom: 'ea' },
  wet_flower: { label: 'Wet Flower', category: 'work_in_progress', defaultUom: 'g' },
  dry_flower: { label: 'Dry Flower', category: 'work_in_progress', defaultUom: 'g' },
  cured_flower: { label: 'Cured Flower', category: 'work_in_progress', defaultUom: 'g' },
  trim: { label: 'Trim', category: 'byproduct', defaultUom: 'g' },
  shake: { label: 'Shake', category: 'byproduct', defaultUom: 'g' },
  stems: { label: 'Stems', category: 'byproduct', defaultUom: 'g' },
  fan_leaves: { label: 'Fan Leaves', category: 'byproduct', defaultUom: 'g' },
  crude_extract: { label: 'Crude Extract', category: 'work_in_progress', defaultUom: 'g' },
  distillate: { label: 'Distillate', category: 'work_in_progress', defaultUom: 'g' },
  live_resin: { label: 'Live Resin', category: 'work_in_progress', defaultUom: 'g' },
  rosin: { label: 'Rosin', category: 'work_in_progress', defaultUom: 'g' },
  shatter: { label: 'Shatter', category: 'work_in_progress', defaultUom: 'g' },
  wax: { label: 'Wax', category: 'work_in_progress', defaultUom: 'g' },
  budder: { label: 'Budder', category: 'work_in_progress', defaultUom: 'g' },
  diamonds: { label: 'Diamonds', category: 'work_in_progress', defaultUom: 'g' },
  hash: { label: 'Hash', category: 'work_in_progress', defaultUom: 'g' },
  kief: { label: 'Kief', category: 'byproduct', defaultUom: 'g' },
  preroll: { label: 'Pre-Roll', category: 'finished_good', defaultUom: 'ea' },
  infused_preroll: { label: 'Infused Pre-Roll', category: 'finished_good', defaultUom: 'ea' },
  edible: { label: 'Edible', category: 'finished_good', defaultUom: 'ea' },
  beverage: { label: 'Beverage', category: 'finished_good', defaultUom: 'ea' },
  capsule: { label: 'Capsule', category: 'finished_good', defaultUom: 'ea' },
  tincture: { label: 'Tincture', category: 'finished_good', defaultUom: 'ml' },
  topical: { label: 'Topical', category: 'finished_good', defaultUom: 'ml' },
  vape_cartridge: { label: 'Vape Cartridge', category: 'finished_good', defaultUom: 'ea' },
  packaged_flower: { label: 'Packaged Flower', category: 'finished_good', defaultUom: 'ea' },
  packaged_concentrate: { label: 'Packaged Concentrate', category: 'finished_good', defaultUom: 'ea' },
  packaged_edible: { label: 'Packaged Edible', category: 'finished_good', defaultUom: 'ea' },
  packaged_preroll: { label: 'Packaged Pre-Roll', category: 'finished_good', defaultUom: 'ea' },
  packaging_material: { label: 'Packaging Material', category: 'consumable', defaultUom: 'ea' },
  label: { label: 'Label', category: 'consumable', defaultUom: 'ea' },
  nutrient: { label: 'Nutrient', category: 'raw_material', defaultUom: 'L' },
  growing_medium: { label: 'Growing Medium', category: 'raw_material', defaultUom: 'L' },
  other: { label: 'Other', category: 'raw_material', defaultUom: 'ea' },
};

