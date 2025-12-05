'use client';

import React, { useEffect, useState } from 'react';
import Link from 'next/link';
import { useParams } from 'next/navigation';
import {
  Package,
  ChevronLeft,
  Edit2,
  Copy,
  Trash2,
  MoreHorizontal,
  Boxes,
  Clock,
  PackageCheck,
  Recycle,
  DollarSign,
  Scale,
  Beaker,
  Calendar,
  MapPin,
  FileText,
  History,
  Layers,
  BarChart3,
  AlertTriangle,
  CheckCircle,
  Tag,
  Box,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { ProductService } from '@/features/inventory/services/product.service';
import { BomService } from '@/features/inventory/services/bom.service';
import {
  CATEGORY_CONFIG,
  PRODUCT_TYPE_CONFIG,
  type Product,
  type ProductWithInventory,
  type InventoryCategory,
  type BillOfMaterials,
} from '@/features/inventory/types';

// Category icons
const CATEGORY_ICONS: Record<InventoryCategory, React.ElementType> = {
  raw_material: Boxes,
  work_in_progress: Clock,
  finished_good: PackageCheck,
  consumable: Package,
  byproduct: Recycle,
};

// Tab definitions
type TabId = 'overview' | 'inventory' | 'boms' | 'costing' | 'compliance' | 'history';

const TABS: { id: TabId; label: string; icon: React.ElementType }[] = [
  { id: 'overview', label: 'Overview', icon: FileText },
  { id: 'inventory', label: 'Inventory', icon: Package },
  { id: 'boms', label: 'BOMs', icon: Layers },
  { id: 'costing', label: 'Costing', icon: DollarSign },
  { id: 'compliance', label: 'Compliance', icon: CheckCircle },
  { id: 'history', label: 'History', icon: History },
];

// Status badge
function StatusBadge({ status }: { status: string }) {
  const config: Record<string, { label: string; color: string; bg: string }> = {
    active: { label: 'Active', color: 'text-emerald-400', bg: 'bg-emerald-500/10' },
    inactive: { label: 'Inactive', color: 'text-muted-foreground', bg: 'bg-muted/50' },
    discontinued: { label: 'Discontinued', color: 'text-rose-400', bg: 'bg-rose-500/10' },
    pending_approval: { label: 'Pending', color: 'text-amber-400', bg: 'bg-amber-500/10' },
  };
  const { label, color, bg } = config[status] || config.active;
  
  return (
    <span className={cn('px-2.5 py-1 rounded-full text-xs font-medium', color, bg)}>
      {label}
    </span>
  );
}

// Info card component
function InfoCard({
  label,
  value,
  icon: Icon,
  subValue,
}: {
  label: string;
  value: string | number;
  icon?: React.ElementType;
  subValue?: string;
}) {
  return (
    <div className="p-4 bg-muted/30 rounded-xl border border-border">
      <div className="flex items-start justify-between">
        <div>
          <div className="text-xs text-muted-foreground mb-1">{label}</div>
          <div className="text-lg font-semibold text-foreground">{value}</div>
          {subValue && <div className="text-xs text-muted-foreground mt-1">{subValue}</div>}
        </div>
        {Icon && (
          <div className="w-8 h-8 rounded-lg bg-cyan-500/10 flex items-center justify-center">
            <Icon className="w-4 h-4 text-cyan-400" />
          </div>
        )}
      </div>
    </div>
  );
}

// Detail row component
function DetailRow({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div className="flex items-center justify-between py-3 border-b border-border last:border-0">
      <span className="text-sm text-muted-foreground">{label}</span>
      <span className="text-sm text-foreground">{value || '—'}</span>
    </div>
  );
}

// BOM card component
function BomCard({ bom }: { bom: BillOfMaterials }) {
  return (
    <div className="p-4 bg-muted/30 rounded-xl border border-border hover:border-cyan-500/30 transition-all">
      <div className="flex items-start justify-between mb-3">
        <div>
          <div className="font-mono text-sm text-cyan-400">{bom.bomNumber}</div>
          <div className="text-sm font-medium text-foreground">{bom.name}</div>
        </div>
        {bom.isDefault && (
          <span className="px-2 py-0.5 rounded text-xs bg-emerald-500/10 text-emerald-400">
            Default
          </span>
        )}
      </div>
      
      <div className="grid grid-cols-3 gap-3 text-xs">
        <div>
          <div className="text-muted-foreground">Output</div>
          <div className="text-foreground">{bom.outputQuantity} {bom.outputUom}</div>
        </div>
        <div>
          <div className="text-muted-foreground">Yield</div>
          <div className="text-foreground">{bom.expectedYieldPercent}%</div>
        </div>
        <div>
          <div className="text-muted-foreground">Inputs</div>
          <div className="text-foreground">{bom.inputLines.length} items</div>
        </div>
      </div>
      
      {bom.estimatedUnitCost && (
        <div className="mt-3 pt-3 border-t border-border flex items-center justify-between">
          <span className="text-xs text-muted-foreground">Est. Unit Cost</span>
          <span className="text-sm font-mono text-foreground">
            ${bom.estimatedUnitCost.toFixed(2)}
          </span>
        </div>
      )}
    </div>
  );
}

// Overview tab content
function OverviewTab({ product }: { product: Product }) {
  const categoryConfig = CATEGORY_CONFIG[product.category];
  const typeConfig = PRODUCT_TYPE_CONFIG[product.productType];
  const CategoryIcon = CATEGORY_ICONS[product.category];

  return (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
      {/* Basic Info */}
      <div className="bg-surface border border-border rounded-xl p-5">
        <h3 className="text-sm font-semibold text-foreground mb-4 flex items-center gap-2">
          <Tag className="w-4 h-4 text-cyan-400" />
          Basic Information
        </h3>
        
        <div className="space-y-0">
          <DetailRow label="SKU" value={<span className="font-mono">{product.sku}</span>} />
          <DetailRow label="Name" value={product.name} />
          <DetailRow label="Description" value={product.description} />
          <DetailRow 
            label="Category" 
            value={
              <span className={cn('px-2 py-0.5 rounded text-xs', categoryConfig.bgColor, categoryConfig.color)}>
                {categoryConfig.label}
              </span>
            } 
          />
          <DetailRow label="Type" value={typeConfig.label} />
          <DetailRow label="Strain" value={product.strainName} />
          <DetailRow label="Default UoM" value={product.defaultUom} />
        </div>
      </div>

      {/* Specifications */}
      <div className="bg-surface border border-border rounded-xl p-5">
        <h3 className="text-sm font-semibold text-foreground mb-4 flex items-center gap-2">
          <Scale className="w-4 h-4 text-cyan-400" />
          Specifications
        </h3>
        
        <div className="space-y-0">
          {product.netWeight && (
            <DetailRow label="Net Weight" value={`${product.netWeight} ${product.netWeightUom}`} />
          )}
          {product.packageSize && (
            <DetailRow label="Package Size" value={`${product.packageSize} ${product.packageSizeUom || ''}`} />
          )}
          <DetailRow label="Shelf Life" value={product.shelfLifeDays ? `${product.shelfLifeDays} days` : null} />
          <DetailRow label="Storage" value={product.storageRequirements} />
          <DetailRow 
            label="COA Required" 
            value={
              product.requiresCoa 
                ? <span className="text-emerald-400">Yes</span>
                : <span className="text-muted-foreground">No</span>
            }
          />
          <DetailRow 
            label="Lot Tracked" 
            value={
              product.isLotTracked 
                ? <span className="text-emerald-400">Yes</span>
                : <span className="text-muted-foreground">No</span>
            }
          />
        </div>
      </div>

      {/* Flags */}
      <div className="bg-surface border border-border rounded-xl p-5">
        <h3 className="text-sm font-semibold text-foreground mb-4 flex items-center gap-2">
          <Box className="w-4 h-4 text-cyan-400" />
          Product Flags
        </h3>
        
        <div className="grid grid-cols-2 gap-3">
          <div className={cn(
            'p-3 rounded-lg border',
            product.isSellable 
              ? 'bg-emerald-500/5 border-emerald-500/20' 
              : 'bg-muted/30 border-border'
          )}>
            <div className="text-xs text-muted-foreground mb-1">Sellable</div>
            <div className={cn('text-sm font-medium', product.isSellable ? 'text-emerald-400' : 'text-muted-foreground')}>
              {product.isSellable ? 'Yes' : 'No'}
            </div>
          </div>
          <div className={cn(
            'p-3 rounded-lg border',
            product.isPurchasable 
              ? 'bg-blue-500/5 border-blue-500/20' 
              : 'bg-muted/30 border-border'
          )}>
            <div className="text-xs text-muted-foreground mb-1">Purchasable</div>
            <div className={cn('text-sm font-medium', product.isPurchasable ? 'text-blue-400' : 'text-muted-foreground')}>
              {product.isPurchasable ? 'Yes' : 'No'}
            </div>
          </div>
          <div className={cn(
            'p-3 rounded-lg border',
            product.isProducible 
              ? 'bg-violet-500/5 border-violet-500/20' 
              : 'bg-muted/30 border-border'
          )}>
            <div className="text-xs text-muted-foreground mb-1">Producible</div>
            <div className={cn('text-sm font-medium', product.isProducible ? 'text-violet-400' : 'text-muted-foreground')}>
              {product.isProducible ? 'Yes' : 'No'}
            </div>
          </div>
          <div className="p-3 rounded-lg border bg-muted/30 border-border">
            <div className="text-xs text-muted-foreground mb-1">Status</div>
            <StatusBadge status={product.status} />
          </div>
        </div>
      </div>

      {/* Pricing */}
      <div className="bg-surface border border-border rounded-xl p-5">
        <h3 className="text-sm font-semibold text-foreground mb-4 flex items-center gap-2">
          <DollarSign className="w-4 h-4 text-cyan-400" />
          Pricing & Costing
        </h3>
        
        <div className="space-y-0">
          <DetailRow 
            label="Standard Cost" 
            value={product.standardCost ? `$${product.standardCost.toFixed(2)}` : null} 
          />
          <DetailRow 
            label="Last Cost" 
            value={product.lastCost ? `$${product.lastCost.toFixed(2)}` : null} 
          />
          <DetailRow 
            label="Average Cost" 
            value={product.averageCost ? `$${product.averageCost.toFixed(2)}` : null} 
          />
          <DetailRow label="Cost Method" value={product.costMethod?.toUpperCase()} />
          <DetailRow 
            label="List Price" 
            value={product.listPrice ? `$${product.listPrice.toFixed(2)}` : null} 
          />
          <DetailRow 
            label="Wholesale Price" 
            value={product.wholesalePrice ? `$${product.wholesalePrice.toFixed(2)}` : null} 
          />
        </div>
      </div>
    </div>
  );
}

// Inventory tab content
function InventoryTab({ product, inventory }: { product: Product; inventory: ProductWithInventory['inventory'] | null }) {
  if (!inventory) {
    return (
      <div className="text-center py-12">
        <Package className="w-12 h-12 text-muted-foreground mx-auto mb-3" />
        <p className="text-muted-foreground">No inventory data available</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Summary Cards */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <InfoCard
          label="Total Quantity"
          value={inventory.totalQuantity.toLocaleString()}
          icon={Package}
          subValue={product.defaultUom}
        />
        <InfoCard
          label="Available"
          value={inventory.availableQuantity.toLocaleString()}
          icon={CheckCircle}
          subValue={product.defaultUom}
        />
        <InfoCard
          label="Reserved"
          value={inventory.reservedQuantity.toLocaleString()}
          icon={Clock}
          subValue="Allocated to orders"
        />
        <InfoCard
          label="On-Hand Value"
          value={`$${inventory.onHandValue.toLocaleString()}`}
          icon={DollarSign}
        />
      </div>

      {/* Distribution */}
      <div className="bg-surface border border-border rounded-xl p-5">
        <h3 className="text-sm font-semibold text-foreground mb-4">Inventory Distribution</h3>
        
        <div className="grid grid-cols-2 gap-4">
          <div className="p-4 bg-muted/30 rounded-lg">
            <div className="text-xs text-muted-foreground mb-1">Active Lots</div>
            <div className="text-2xl font-bold text-foreground">{inventory.lotCount}</div>
          </div>
          <div className="p-4 bg-muted/30 rounded-lg">
            <div className="text-xs text-muted-foreground mb-1">Locations</div>
            <div className="text-2xl font-bold text-foreground">{inventory.locationCount}</div>
          </div>
        </div>

        <div className="mt-4 pt-4 border-t border-border">
          <Link
            href={`/inventory/lots?productId=${product.id}`}
            className="text-sm text-cyan-400 hover:underline"
          >
            View all lots for this product →
          </Link>
        </div>
      </div>
    </div>
  );
}

// BOMs tab content
function BomsTab({ product, boms }: { product: Product; boms: BillOfMaterials[] }) {
  if (!product.isProducible) {
    return (
      <div className="text-center py-12">
        <Layers className="w-12 h-12 text-muted-foreground mx-auto mb-3" />
        <p className="text-muted-foreground">This product is not producible</p>
        <p className="text-sm text-muted-foreground/60 mt-1">
          BOMs are only available for producible products
        </p>
      </div>
    );
  }

  if (boms.length === 0) {
    return (
      <div className="text-center py-12">
        <Layers className="w-12 h-12 text-muted-foreground mx-auto mb-3" />
        <p className="text-muted-foreground">No BOMs defined</p>
        <p className="text-sm text-muted-foreground/60 mt-1">
          Create a BOM to define how this product is manufactured
        </p>
        <Link
          href="/inventory/boms/new"
          className="mt-4 inline-flex items-center gap-2 px-4 py-2 rounded-lg bg-cyan-500/10 text-cyan-400 hover:bg-cyan-500/20 transition-colors"
        >
          Create BOM
        </Link>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="text-sm font-semibold text-foreground">
          Bill of Materials ({boms.length})
        </h3>
        <Link
          href="/inventory/boms/new"
          className="text-sm text-cyan-400 hover:underline"
        >
          + Create New BOM
        </Link>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        {boms.map((bom) => (
          <BomCard key={bom.id} bom={bom} />
        ))}
      </div>
    </div>
  );
}

// Costing tab content
function CostingTab({ product }: { product: Product }) {
  return (
    <div className="space-y-6">
      <div className="bg-surface border border-border rounded-xl p-5">
        <h3 className="text-sm font-semibold text-foreground mb-4 flex items-center gap-2">
          <BarChart3 className="w-4 h-4 text-cyan-400" />
          Cost Analysis
        </h3>
        
        <div className="grid grid-cols-3 gap-4 mb-6">
          <div className="p-4 bg-muted/30 rounded-lg text-center">
            <div className="text-xs text-muted-foreground mb-1">Standard Cost</div>
            <div className="text-2xl font-bold text-foreground">
              ${product.standardCost?.toFixed(2) || '0.00'}
            </div>
          </div>
          <div className="p-4 bg-muted/30 rounded-lg text-center">
            <div className="text-xs text-muted-foreground mb-1">Last Cost</div>
            <div className="text-2xl font-bold text-foreground">
              ${product.lastCost?.toFixed(2) || '0.00'}
            </div>
          </div>
          <div className="p-4 bg-muted/30 rounded-lg text-center">
            <div className="text-xs text-muted-foreground mb-1">Average Cost</div>
            <div className="text-2xl font-bold text-foreground">
              ${product.averageCost?.toFixed(2) || '0.00'}
            </div>
          </div>
        </div>

        <div className="space-y-0">
          <DetailRow label="Cost Method" value={product.costMethod?.toUpperCase()} />
          {product.listPrice && product.standardCost && (
            <DetailRow 
              label="Gross Margin" 
              value={`${(((product.listPrice - product.standardCost) / product.listPrice) * 100).toFixed(1)}%`} 
            />
          )}
        </div>
      </div>

      {/* Cost History placeholder */}
      <div className="bg-surface border border-border rounded-xl p-5">
        <h3 className="text-sm font-semibold text-foreground mb-4">Cost History</h3>
        <div className="text-center py-8 text-muted-foreground text-sm">
          Cost history will be displayed here when available
        </div>
      </div>
    </div>
  );
}

// Compliance tab content  
function ComplianceTab({ product }: { product: Product }) {
  return (
    <div className="space-y-6">
      <div className="bg-surface border border-border rounded-xl p-5">
        <h3 className="text-sm font-semibold text-foreground mb-4 flex items-center gap-2">
          <Beaker className="w-4 h-4 text-cyan-400" />
          Testing Requirements
        </h3>
        
        <div className="space-y-0">
          <DetailRow 
            label="COA Required" 
            value={
              product.requiresCoa 
                ? <span className="flex items-center gap-1 text-emerald-400"><CheckCircle className="w-4 h-4" /> Yes</span>
                : <span className="text-muted-foreground">No</span>
            }
          />
          {product.coaTestTypes && product.coaTestTypes.length > 0 && (
            <DetailRow 
              label="Required Tests" 
              value={product.coaTestTypes.join(', ')} 
            />
          )}
        </div>
      </div>

      <div className="bg-surface border border-border rounded-xl p-5">
        <h3 className="text-sm font-semibold text-foreground mb-4">Regulatory Mapping</h3>
        
        <div className="space-y-0">
          <DetailRow label="METRC Category" value={product.metrcItemCategory} />
          <DetailRow label="METRC UoM" value={product.metrcUnitOfMeasure} />
          <DetailRow label="BioTrack Type" value={product.biotrackProductType} />
          <DetailRow label="Regulatory Category" value={product.regulatoryCategory} />
        </div>
      </div>
    </div>
  );
}

// History tab content
function HistoryTab({ product }: { product: Product }) {
  return (
    <div className="bg-surface border border-border rounded-xl p-5">
      <h3 className="text-sm font-semibold text-foreground mb-4">Audit History</h3>
      
      <div className="space-y-4">
        <div className="flex items-start gap-3 p-3 bg-muted/30 rounded-lg">
          <div className="w-8 h-8 rounded-full bg-cyan-500/10 flex items-center justify-center flex-shrink-0">
            <History className="w-4 h-4 text-cyan-400" />
          </div>
          <div>
            <div className="text-sm text-foreground">Product created</div>
            <div className="text-xs text-muted-foreground mt-0.5">
              by {product.createdBy} on {new Date(product.createdAt).toLocaleDateString()}
            </div>
          </div>
        </div>
        
        <div className="flex items-start gap-3 p-3 bg-muted/30 rounded-lg">
          <div className="w-8 h-8 rounded-full bg-violet-500/10 flex items-center justify-center flex-shrink-0">
            <Edit2 className="w-4 h-4 text-violet-400" />
          </div>
          <div>
            <div className="text-sm text-foreground">Last updated</div>
            <div className="text-xs text-muted-foreground mt-0.5">
              by {product.updatedBy} on {new Date(product.updatedAt).toLocaleDateString()}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default function ProductDetailPage() {
  const params = useParams();
  const productId = params.id as string;
  
  const [product, setProduct] = useState<ProductWithInventory | null>(null);
  const [boms, setBoms] = useState<BillOfMaterials[]>([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<TabId>('overview');
  const [menuOpen, setMenuOpen] = useState(false);

  useEffect(() => {
    const loadProduct = async () => {
      setLoading(true);
      try {
        const productData = await ProductService.getProductWithInventory(productId);
        setProduct(productData);

        if (productData?.isProducible) {
          const bomsData = await BomService.getBomsForProduct(productId);
          setBoms(bomsData);
        }
      } catch (error) {
        console.error('Failed to load product:', error);
      } finally {
        setLoading(false);
      }
    };

    loadProduct();
  }, [productId]);

  if (loading) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center">
        <div className="animate-spin w-8 h-8 border-2 border-cyan-500 border-t-transparent rounded-full" />
      </div>
    );
  }

  if (!product) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center">
        <div className="text-center">
          <AlertTriangle className="w-12 h-12 text-amber-400 mx-auto mb-3" />
          <p className="text-foreground">Product not found</p>
          <Link
            href="/inventory/products"
            className="mt-4 inline-block text-cyan-400 hover:underline"
          >
            ← Back to Product Catalog
          </Link>
        </div>
      </div>
    );
  }

  const categoryConfig = CATEGORY_CONFIG[product.category];
  const CategoryIcon = CATEGORY_ICONS[product.category];

  return (
    <div className="min-h-screen bg-background">
      {/* Header */}
      <header className="sticky top-0 z-40 glass-header">
        <div className="px-6 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <Link
                href="/inventory/products"
                className="p-2 rounded-lg hover:bg-white/5 text-muted-foreground hover:text-foreground transition-colors"
              >
                <ChevronLeft className="w-5 h-5" />
              </Link>
              <div className="flex items-center gap-3">
                <div className={cn('w-12 h-12 rounded-xl flex items-center justify-center', categoryConfig.bgColor)}>
                  <CategoryIcon className={cn('w-6 h-6', categoryConfig.color)} />
                </div>
                <div>
                  <div className="flex items-center gap-3">
                    <h1 className="text-xl font-mono font-semibold text-foreground">
                      {product.sku}
                    </h1>
                    <StatusBadge status={product.status} />
                  </div>
                  <p className="text-sm text-muted-foreground">{product.name}</p>
                </div>
              </div>
            </div>

            {/* Actions */}
            <div className="flex items-center gap-3">
              <button className="flex items-center gap-2 px-4 py-2 rounded-lg bg-white/5 text-foreground hover:bg-white/10 transition-colors">
                <Edit2 className="w-4 h-4" />
                <span className="text-sm">Edit</span>
              </button>
              
              <div className="relative">
                <button
                  onClick={() => setMenuOpen(!menuOpen)}
                  className="p-2 rounded-lg hover:bg-white/5 text-muted-foreground hover:text-foreground transition-colors"
                >
                  <MoreHorizontal className="w-5 h-5" />
                </button>

                {menuOpen && (
                  <>
                    <div
                      className="fixed inset-0 z-10"
                      onClick={() => setMenuOpen(false)}
                    />
                    <div className="absolute right-0 top-10 z-20 w-48 py-1 bg-surface border border-border rounded-lg shadow-xl">
                      <button className="w-full flex items-center gap-2 px-3 py-2 text-sm text-foreground hover:bg-white/5">
                        <Copy className="w-4 h-4" />
                        Duplicate Product
                      </button>
                      <button className="w-full flex items-center gap-2 px-3 py-2 text-sm text-foreground hover:bg-white/5">
                        <Layers className="w-4 h-4" />
                        Create BOM
                      </button>
                      <hr className="my-1 border-border" />
                      <button className="w-full flex items-center gap-2 px-3 py-2 text-sm text-rose-400 hover:bg-rose-500/10">
                        <Trash2 className="w-4 h-4" />
                        Delete Product
                      </button>
                    </div>
                  </>
                )}
              </div>
            </div>
          </div>
        </div>
      </header>

      {/* Tabs */}
      <div className="px-6 py-2 border-b border-border bg-background/50">
        <div className="flex items-center gap-1">
          {TABS.map((tab) => {
            const Icon = tab.icon;
            const isActive = activeTab === tab.id;
            return (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id)}
                className={cn(
                  'flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition-all',
                  isActive
                    ? 'bg-cyan-500/10 text-cyan-400'
                    : 'text-muted-foreground hover:text-foreground hover:bg-white/5'
                )}
              >
                <Icon className="w-4 h-4" />
                {tab.label}
              </button>
            );
          })}
        </div>
      </div>

      {/* Content */}
      <main className="px-6 py-6">
        {activeTab === 'overview' && <OverviewTab product={product} />}
        {activeTab === 'inventory' && <InventoryTab product={product} inventory={product.inventory} />}
        {activeTab === 'boms' && <BomsTab product={product} boms={boms} />}
        {activeTab === 'costing' && <CostingTab product={product} />}
        {activeTab === 'compliance' && <ComplianceTab product={product} />}
        {activeTab === 'history' && <HistoryTab product={product} />}
      </main>
    </div>
  );
}

