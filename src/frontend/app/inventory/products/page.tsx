'use client';

import React, { useEffect, useState } from 'react';
import Link from 'next/link';
import {
  Package,
  Plus,
  Search,
  Filter,
  ChevronDown,
  LayoutGrid,
  List,
  Table2,
  MoreHorizontal,
  Edit2,
  Copy,
  Trash2,
  Eye,
  Boxes,
  Clock,
  PackageCheck,
  Recycle,
  ChevronLeft,
  ArrowUpDown,
  Printer,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { useManufacturingStore } from '@/features/inventory/stores/manufacturingStore';
import { ProductService } from '@/features/inventory/services/product.service';
import {
  CATEGORY_CONFIG,
  PRODUCT_TYPE_CONFIG,
  type Product,
  type InventoryCategory,
  type ProductStatus,
} from '@/features/inventory/types';
import { LabelPreviewSlideout, PrinterSettings } from '@/features/inventory/components/labels';
import type { LabelTemplate } from '@/features/inventory/services/labels.service';

// Product label templates
const PRODUCT_LABEL_TEMPLATES: LabelTemplate[] = [
  {
    id: 'prod-tpl-1',
    siteId: 'site-1',
    name: 'Product Label - Standard',
    jurisdiction: 'ALL',
    labelType: 'product',
    format: 'zpl',
    barcodeFormat: 'gs1-128',
    barcodePosition: { x: 10, y: 40, width: 180, height: 30 },
    widthInches: 2,
    heightInches: 1,
    fields: [],
    requiredPhrases: [],
    jurisdictionRules: {},
    isActive: true,
    isDefault: true,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
];

// Category icons mapping
const CATEGORY_ICONS: Record<InventoryCategory, React.ElementType> = {
  raw_material: Boxes,
  work_in_progress: Clock,
  finished_good: PackageCheck,
  consumable: Package,
  byproduct: Recycle,
};

// Status badge component
function StatusBadge({ status }: { status: ProductStatus }) {
  const config = {
    active: { label: 'Active', color: 'text-emerald-400', bg: 'bg-emerald-500/10' },
    inactive: { label: 'Inactive', color: 'text-muted-foreground', bg: 'bg-muted/50' },
    discontinued: { label: 'Discontinued', color: 'text-rose-400', bg: 'bg-rose-500/10' },
    pending_approval: { label: 'Pending', color: 'text-amber-400', bg: 'bg-amber-500/10' },
  };
  const { label, color, bg } = config[status];
  
  return (
    <span className={cn('px-2 py-0.5 rounded-full text-xs font-medium', color, bg)}>
      {label}
    </span>
  );
}

// Category filter tabs
function CategoryTabs({
  selected,
  onChange,
  counts,
}: {
  selected: InventoryCategory | 'all';
  onChange: (cat: InventoryCategory | 'all') => void;
  counts: Record<string, number>;
}) {
  const categories: (InventoryCategory | 'all')[] = [
    'all',
    'raw_material',
    'work_in_progress',
    'finished_good',
    'consumable',
    'byproduct',
  ];

  return (
    <div className="flex items-center gap-1 p-1 bg-muted/30 rounded-lg">
      {categories.map((cat) => {
        const isSelected = selected === cat;
        const Icon = cat === 'all' ? Package : CATEGORY_ICONS[cat];
        const label = cat === 'all' ? 'All' : CATEGORY_CONFIG[cat].label;
        const count = cat === 'all' 
          ? Object.values(counts).reduce((a, b) => a + b, 0)
          : counts[cat] || 0;

        return (
          <button
            key={cat}
            onClick={() => onChange(cat)}
            className={cn(
              'flex items-center gap-2 px-3 py-1.5 rounded-md text-sm font-medium transition-all',
              isSelected
                ? 'bg-cyan-500/10 text-cyan-400'
                : 'text-muted-foreground hover:text-foreground hover:bg-white/5'
            )}
          >
            <Icon className="w-4 h-4" />
            <span>{label}</span>
            <span className={cn(
              'px-1.5 py-0.5 rounded text-xs',
              isSelected ? 'bg-cyan-500/20' : 'bg-white/5'
            )}>
              {count}
            </span>
          </button>
        );
      })}
    </div>
  );
}

// Product row component
function ProductRow({
  product,
  onEdit,
  onView,
  onPrintLabel,
}: {
  product: Product;
  onEdit: () => void;
  onView: () => void;
  onPrintLabel: () => void;
}) {
  const [menuOpen, setMenuOpen] = useState(false);
  const categoryConfig = CATEGORY_CONFIG[product.category];
  const typeConfig = PRODUCT_TYPE_CONFIG[product.productType];
  const CategoryIcon = CATEGORY_ICONS[product.category];

  return (
    <tr className="group border-b border-border hover:bg-muted/30 transition-colors">
      {/* SKU & Name */}
      <td className="px-4 py-3">
        <div className="flex items-center gap-3">
          <div className={cn('w-10 h-10 rounded-lg flex items-center justify-center', categoryConfig.bgColor)}>
            <CategoryIcon className={cn('w-5 h-5', categoryConfig.color)} />
          </div>
          <div>
            <div className="font-mono text-sm text-foreground">{product.sku}</div>
            <div className="text-sm text-muted-foreground truncate max-w-[200px]">
              {product.name}
            </div>
          </div>
        </div>
      </td>

      {/* Category */}
      <td className="px-4 py-3">
        <span className={cn('px-2 py-1 rounded text-xs font-medium', categoryConfig.bgColor, categoryConfig.color)}>
          {categoryConfig.label}
        </span>
      </td>

      {/* Type */}
      <td className="px-4 py-3">
        <span className="text-sm text-muted-foreground">{typeConfig.label}</span>
      </td>

      {/* Strain */}
      <td className="px-4 py-3">
        <span className="text-sm text-foreground">{product.strainName || '—'}</span>
      </td>

      {/* UoM */}
      <td className="px-4 py-3">
        <span className="text-sm text-muted-foreground">{product.defaultUom}</span>
      </td>

      {/* Cost */}
      <td className="px-4 py-3 text-right">
        <span className="text-sm font-mono text-foreground">
          {product.standardCost ? `$${product.standardCost.toFixed(2)}` : '—'}
        </span>
      </td>

      {/* Status */}
      <td className="px-4 py-3">
        <StatusBadge status={product.status} />
      </td>

      {/* Actions */}
      <td className="px-4 py-3">
        <div className="relative">
          <button
            onClick={() => setMenuOpen(!menuOpen)}
            className="p-1.5 rounded-lg hover:bg-white/5 text-muted-foreground hover:text-foreground transition-colors opacity-0 group-hover:opacity-100"
          >
            <MoreHorizontal className="w-4 h-4" />
          </button>

          {menuOpen && (
            <>
              <div
                className="fixed inset-0 z-10"
                onClick={() => setMenuOpen(false)}
              />
              <div className="absolute right-0 top-8 z-20 w-40 py-1 bg-surface border border-border rounded-lg shadow-xl">
                <button
                  onClick={() => {
                    onView();
                    setMenuOpen(false);
                  }}
                  className="w-full flex items-center gap-2 px-3 py-2 text-sm text-foreground hover:bg-white/5"
                >
                  <Eye className="w-4 h-4" />
                  View
                </button>
                <button
                  onClick={() => {
                    onEdit();
                    setMenuOpen(false);
                  }}
                  className="w-full flex items-center gap-2 px-3 py-2 text-sm text-foreground hover:bg-white/5"
                >
                  <Edit2 className="w-4 h-4" />
                  Edit
                </button>
                <button className="w-full flex items-center gap-2 px-3 py-2 text-sm text-foreground hover:bg-white/5">
                  <Copy className="w-4 h-4" />
                  Duplicate
                </button>
                <button
                  onClick={() => {
                    onPrintLabel();
                    setMenuOpen(false);
                  }}
                  className="w-full flex items-center gap-2 px-3 py-2 text-sm text-foreground hover:bg-white/5"
                >
                  <Printer className="w-4 h-4" />
                  Print Label
                </button>
                <hr className="my-1 border-border" />
                <button className="w-full flex items-center gap-2 px-3 py-2 text-sm text-rose-400 hover:bg-rose-500/10">
                  <Trash2 className="w-4 h-4" />
                  Delete
                </button>
              </div>
            </>
          )}
        </div>
      </td>
    </tr>
  );
}

// Product card for grid view
function ProductCard({ product, onClick }: { product: Product; onClick: () => void }) {
  const categoryConfig = CATEGORY_CONFIG[product.category];
  const CategoryIcon = CATEGORY_ICONS[product.category];

  return (
    <button
      onClick={onClick}
      className="text-left p-4 bg-surface border border-border rounded-xl hover:border-cyan-500/30 transition-all group"
    >
      <div className="flex items-start justify-between mb-3">
        <div className={cn('w-10 h-10 rounded-lg flex items-center justify-center', categoryConfig.bgColor)}>
          <CategoryIcon className={cn('w-5 h-5', categoryConfig.color)} />
        </div>
        <StatusBadge status={product.status} />
      </div>

      <div className="font-mono text-sm text-cyan-400 mb-1">{product.sku}</div>
      <div className="font-medium text-foreground mb-2 line-clamp-2">{product.name}</div>

      {product.strainName && (
        <div className="text-xs text-muted-foreground mb-2">{product.strainName}</div>
      )}

      <div className="flex items-center justify-between text-xs">
        <span className={cn('px-2 py-0.5 rounded', categoryConfig.bgColor, categoryConfig.color)}>
          {categoryConfig.label}
        </span>
        {product.standardCost && (
          <span className="font-mono text-foreground">
            ${product.standardCost.toFixed(2)}
          </span>
        )}
      </div>
    </button>
  );
}

export default function ProductCatalogPage() {
  const store = useManufacturingStore();
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<InventoryCategory | 'all'>('all');
  const [viewMode, setViewMode] = useState<'table' | 'grid'>('table');
  const [sortBy, setSortBy] = useState<'name' | 'sku' | 'category'>('name');
  const [sortDir, setSortDir] = useState<'asc' | 'desc'>('asc');
  
  // Label preview state
  const [isPreviewOpen, setIsPreviewOpen] = useState(false);
  const [previewProduct, setPreviewProduct] = useState<Product | null>(null);
  const [selectedTemplate, setSelectedTemplate] = useState<LabelTemplate | null>(PRODUCT_LABEL_TEMPLATES[0]);
  const [isPrinterSettingsOpen, setIsPrinterSettingsOpen] = useState(false);

  // Load products on mount
  useEffect(() => {
    const loadProducts = async () => {
      store.setProductsLoading(true);
      try {
        const response = await ProductService.getProducts({
          category: selectedCategory === 'all' ? undefined : [selectedCategory],
          search: searchQuery || undefined,
        });
        store.setProducts(response.items);

        const summary = await ProductService.getCategorySummary();
        store.setProductCategorySummary(summary);
      } catch (error) {
        store.setProductsError('Failed to load products');
      } finally {
        store.setProductsLoading(false);
      }
    };

    loadProducts();
  }, [selectedCategory, searchQuery]);

  // Calculate category counts
  const categoryCounts = store.productCategorySummary.reduce((acc, cat) => {
    acc[cat.category] = cat.productCount;
    return acc;
  }, {} as Record<string, number>);

  // Filter and sort products
  let filteredProducts = store.products;
  
  if (selectedCategory !== 'all') {
    filteredProducts = filteredProducts.filter((p) => p.category === selectedCategory);
  }
  
  if (searchQuery) {
    const query = searchQuery.toLowerCase();
    filteredProducts = filteredProducts.filter(
      (p) =>
        p.name.toLowerCase().includes(query) ||
        p.sku.toLowerCase().includes(query) ||
        p.strainName?.toLowerCase().includes(query)
    );
  }

  // Sort
  filteredProducts = [...filteredProducts].sort((a, b) => {
    let cmp = 0;
    if (sortBy === 'name') cmp = a.name.localeCompare(b.name);
    if (sortBy === 'sku') cmp = a.sku.localeCompare(b.sku);
    if (sortBy === 'category') cmp = a.category.localeCompare(b.category);
    return sortDir === 'asc' ? cmp : -cmp;
  });

  const handleSort = (col: 'name' | 'sku' | 'category') => {
    if (sortBy === col) {
      setSortDir(sortDir === 'asc' ? 'desc' : 'asc');
    } else {
      setSortBy(col);
      setSortDir('asc');
    }
  };

  return (
    <div className="min-h-screen bg-background">
      {/* Header */}
      <header className="sticky top-0 z-40 glass-header">
        <div className="px-6 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <Link
                href="/inventory"
                className="p-2 rounded-lg hover:bg-white/5 text-muted-foreground hover:text-foreground transition-colors"
              >
                <ChevronLeft className="w-5 h-5" />
              </Link>
              <div className="w-10 h-10 rounded-xl bg-cyan-500/10 flex items-center justify-center">
                <Package className="w-5 h-5 text-cyan-400" />
              </div>
              <div>
                <h1 className="text-xl font-semibold text-foreground">Product Catalog</h1>
                <p className="text-sm text-muted-foreground">
                  Manage SKUs, raw materials, and finished goods
                </p>
              </div>
            </div>

            {/* Actions */}
            <div className="flex items-center gap-3">
              <button
                onClick={() => store.openProductModal('create')}
                className="flex items-center gap-2 px-4 py-2 rounded-lg bg-cyan-500 text-black font-medium hover:bg-cyan-400 transition-colors"
              >
                <Plus className="w-4 h-4" />
                <span className="text-sm">New Product</span>
              </button>
            </div>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="px-6 py-6 space-y-6">
        {/* Category Tabs */}
        <CategoryTabs
          selected={selectedCategory}
          onChange={setSelectedCategory}
          counts={categoryCounts}
        />

        {/* Toolbar */}
        <div className="flex items-center justify-between gap-4">
          {/* Search */}
          <div className="relative flex-1 max-w-md">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
            <input
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              placeholder="Search products by SKU, name, or strain..."
              className="w-full pl-10 pr-4 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-cyan-500/30"
            />
          </div>

          {/* View Toggle */}
          <div className="flex items-center gap-2">
            <div className="flex items-center bg-muted/30 rounded-lg p-1">
              <button
                onClick={() => setViewMode('table')}
                className={cn(
                  'p-1.5 rounded transition-colors',
                  viewMode === 'table'
                    ? 'bg-cyan-500/10 text-cyan-400'
                    : 'text-muted-foreground hover:text-foreground'
                )}
              >
                <Table2 className="w-4 h-4" />
              </button>
              <button
                onClick={() => setViewMode('grid')}
                className={cn(
                  'p-1.5 rounded transition-colors',
                  viewMode === 'grid'
                    ? 'bg-cyan-500/10 text-cyan-400'
                    : 'text-muted-foreground hover:text-foreground'
                )}
              >
                <LayoutGrid className="w-4 h-4" />
              </button>
            </div>

            <button className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-muted/30 text-muted-foreground hover:text-foreground transition-colors">
              <Filter className="w-3.5 h-3.5" />
              <span className="text-xs">Filter</span>
              <ChevronDown className="w-3 h-3" />
            </button>
          </div>
        </div>

        {/* Results Count */}
        <div className="text-sm text-muted-foreground">
          Showing {filteredProducts.length} products
        </div>

        {/* Loading State */}
        {store.productsLoading ? (
          <div className="flex items-center justify-center py-12">
            <div className="animate-spin w-8 h-8 border-2 border-cyan-500 border-t-transparent rounded-full" />
          </div>
        ) : viewMode === 'table' ? (
          /* Table View */
          <div className="bg-surface border border-border rounded-xl overflow-hidden">
            <table className="w-full">
              <thead>
                <tr className="border-b border-border bg-muted/30">
                  <th
                    className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider cursor-pointer hover:text-foreground"
                    onClick={() => handleSort('sku')}
                  >
                    <div className="flex items-center gap-1">
                      SKU / Name
                      {sortBy === 'sku' && (
                        <ArrowUpDown className="w-3 h-3" />
                      )}
                    </div>
                  </th>
                  <th
                    className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider cursor-pointer hover:text-foreground"
                    onClick={() => handleSort('category')}
                  >
                    <div className="flex items-center gap-1">
                      Category
                      {sortBy === 'category' && (
                        <ArrowUpDown className="w-3 h-3" />
                      )}
                    </div>
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                    Type
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                    Strain
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                    UoM
                  </th>
                  <th className="px-4 py-3 text-right text-xs font-medium text-muted-foreground uppercase tracking-wider">
                    Std Cost
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                    Status
                  </th>
                  <th className="px-4 py-3 w-12"></th>
                </tr>
              </thead>
              <tbody>
                {filteredProducts.map((product) => (
                  <ProductRow
                    key={product.id}
                    product={product}
                    onEdit={() => {
                      store.selectProduct(product.id);
                      store.openProductModal('edit');
                    }}
                    onView={() => {
                      // Navigate to product detail
                      window.location.href = `/inventory/products/${product.id}`;
                    }}
                    onPrintLabel={() => {
                      setPreviewProduct(product);
                      setIsPreviewOpen(true);
                    }}
                  />
                ))}
              </tbody>
            </table>

            {filteredProducts.length === 0 && (
              <div className="py-12 text-center">
                <Package className="w-12 h-12 text-muted-foreground mx-auto mb-3" />
                <p className="text-muted-foreground">No products found</p>
                <p className="text-sm text-muted-foreground/60 mt-1">
                  Try adjusting your filters or create a new product
                </p>
              </div>
            )}
          </div>
        ) : (
          /* Grid View */
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
            {filteredProducts.map((product) => (
              <ProductCard
                key={product.id}
                product={product}
                onClick={() => {
                  window.location.href = `/inventory/products/${product.id}`;
                }}
              />
            ))}
          </div>
        )}
      </main>

      {/* Label Preview Slideout */}
      <LabelPreviewSlideout
        isOpen={isPreviewOpen}
        onClose={() => setIsPreviewOpen(false)}
        template={selectedTemplate}
        availableTemplates={PRODUCT_LABEL_TEMPLATES}
        onTemplateChange={(id) => {
          const t = PRODUCT_LABEL_TEMPLATES.find(tpl => tpl.id === id);
          if (t) setSelectedTemplate(t);
        }}
        entityData={previewProduct ? {
          productName: previewProduct.name,
          strainName: previewProduct.strainName,
          lotNumber: previewProduct.sku,
        } : null}
        entityType="product"
        onPrint={async () => console.log('Printing product label:', previewProduct?.sku)}
        onDownload={async (format) => console.log('Downloading as:', format)}
        onOpenSettings={() => {
          setIsPreviewOpen(false);
          setIsPrinterSettingsOpen(true);
        }}
      />

      {/* Printer Settings Modal */}
      <PrinterSettings
        isOpen={isPrinterSettingsOpen}
        onClose={() => setIsPrinterSettingsOpen(false)}
      />
    </div>
  );
}

