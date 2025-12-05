'use client';

import React, { useEffect, useState } from 'react';
import Link from 'next/link';
import {
  Layers,
  Plus,
  Search,
  Filter,
  ChevronDown,
  ChevronLeft,
  MoreHorizontal,
  Edit2,
  Copy,
  Trash2,
  Eye,
  Factory,
  Beaker,
  Package,
  Sprout,
  ArrowRightLeft,
  Play,
  CheckCircle,
  Clock,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { useManufacturingStore } from '@/features/inventory/stores/manufacturingStore';
import { BomService } from '@/features/inventory/services/bom.service';
import {
  BOM_TYPE_CONFIG,
  type BillOfMaterials,
  type BomType,
  type BomStatus,
} from '@/features/inventory/types';

// BOM Type icons
const BOM_TYPE_ICONS: Record<BomType, React.ElementType> = {
  production: Factory,
  processing: Beaker,
  cultivation: Sprout,
  packaging: Package,
  assembly: Layers,
  disassembly: Layers,
  conversion: ArrowRightLeft,
};

// Status badge
function StatusBadge({ status }: { status: BomStatus }) {
  const config: Record<BomStatus, { label: string; color: string; bg: string; icon: React.ElementType }> = {
    draft: { label: 'Draft', color: 'text-muted-foreground', bg: 'bg-muted/50', icon: Edit2 },
    active: { label: 'Active', color: 'text-emerald-400', bg: 'bg-emerald-500/10', icon: CheckCircle },
    inactive: { label: 'Inactive', color: 'text-amber-400', bg: 'bg-amber-500/10', icon: Clock },
    obsolete: { label: 'Obsolete', color: 'text-rose-400', bg: 'bg-rose-500/10', icon: Trash2 },
  };
  const { label, color, bg, icon: Icon } = config[status];

  return (
    <span className={cn('inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium', color, bg)}>
      <Icon className="w-3 h-3" />
      {label}
    </span>
  );
}

// Type filter tabs
function TypeTabs({
  selected,
  onChange,
  counts,
}: {
  selected: BomType | 'all';
  onChange: (type: BomType | 'all') => void;
  counts: Record<string, number>;
}) {
  const types: (BomType | 'all')[] = [
    'all',
    'production',
    'processing',
    'packaging',
    'conversion',
    'cultivation',
  ];

  return (
    <div className="flex items-center gap-1 p-1 bg-muted/30 rounded-lg overflow-x-auto">
      {types.map((type) => {
        const isSelected = selected === type;
        const Icon = type === 'all' ? Layers : BOM_TYPE_ICONS[type];
        const label = type === 'all' ? 'All' : BOM_TYPE_CONFIG[type].label;
        const count = type === 'all'
          ? Object.values(counts).reduce((a, b) => a + b, 0)
          : counts[type] || 0;

        return (
          <button
            key={type}
            onClick={() => onChange(type)}
            className={cn(
              'flex items-center gap-2 px-3 py-1.5 rounded-md text-sm font-medium transition-all whitespace-nowrap',
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

// BOM card component
function BomCard({
  bom,
  onView,
  onEdit,
}: {
  bom: BillOfMaterials;
  onView: () => void;
  onEdit: () => void;
}) {
  const [menuOpen, setMenuOpen] = useState(false);
  const typeConfig = BOM_TYPE_CONFIG[bom.bomType];
  const TypeIcon = BOM_TYPE_ICONS[bom.bomType];

  return (
    <div className="group p-5 bg-surface border border-border rounded-xl hover:border-cyan-500/30 transition-all">
      {/* Header */}
      <div className="flex items-start justify-between mb-4">
        <div className="flex items-center gap-3">
          <div className={cn('w-10 h-10 rounded-lg flex items-center justify-center', typeConfig.bgColor)}>
            <TypeIcon className={cn('w-5 h-5', typeConfig.color)} />
          </div>
          <div>
            <div className="font-mono text-sm text-cyan-400">{bom.bomNumber}</div>
            <div className="text-sm font-medium text-foreground">{bom.name}</div>
          </div>
        </div>

        <div className="flex items-center gap-2">
          {bom.isDefault && (
            <span className="px-2 py-0.5 rounded text-xs bg-emerald-500/10 text-emerald-400">
              Default
            </span>
          )}
          <StatusBadge status={bom.status} />

          <div className="relative">
            <button
              onClick={() => setMenuOpen(!menuOpen)}
              className="p-1.5 rounded-lg hover:bg-white/5 text-muted-foreground hover:text-foreground opacity-0 group-hover:opacity-100 transition-all"
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
                    Clone
                  </button>
                  <button className="w-full flex items-center gap-2 px-3 py-2 text-sm text-foreground hover:bg-white/5">
                    <Play className="w-4 h-4" />
                    Create Order
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
        </div>
      </div>

      {/* Output Product */}
      <div className="mb-4 p-3 bg-muted/30 rounded-lg">
        <div className="text-xs text-muted-foreground mb-1">Output Product</div>
        <div className="flex items-center justify-between">
          <span className="text-sm text-foreground">
            {bom.outputProduct?.name || 'Unknown Product'}
          </span>
          <span className="text-sm font-mono text-cyan-400">
            {bom.outputQuantity} {bom.outputUom}
          </span>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-3 gap-3 text-xs mb-4">
        <div className="p-2 bg-muted/30 rounded-lg text-center">
          <div className="text-muted-foreground">Inputs</div>
          <div className="text-foreground font-medium">{bom.inputLines.length}</div>
        </div>
        <div className="p-2 bg-muted/30 rounded-lg text-center">
          <div className="text-muted-foreground">Yield</div>
          <div className="text-foreground font-medium">{bom.expectedYieldPercent}%</div>
        </div>
        <div className="p-2 bg-muted/30 rounded-lg text-center">
          <div className="text-muted-foreground">Version</div>
          <div className="text-foreground font-medium">v{bom.version}</div>
        </div>
      </div>

      {/* Footer */}
      <div className="flex items-center justify-between pt-3 border-t border-border">
        <span className={cn('px-2 py-0.5 rounded text-xs', typeConfig.bgColor, typeConfig.color)}>
          {typeConfig.label}
        </span>
        {bom.estimatedUnitCost && (
          <span className="text-sm font-mono text-foreground">
            ${bom.estimatedUnitCost.toFixed(2)}/unit
          </span>
        )}
      </div>
    </div>
  );
}

// BOM row component for table view
function BomRow({
  bom,
  onView,
  onEdit,
}: {
  bom: BillOfMaterials;
  onView: () => void;
  onEdit: () => void;
}) {
  const [menuOpen, setMenuOpen] = useState(false);
  const typeConfig = BOM_TYPE_CONFIG[bom.bomType];
  const TypeIcon = BOM_TYPE_ICONS[bom.bomType];

  return (
    <tr className="group border-b border-border hover:bg-muted/30 transition-colors">
      <td className="px-4 py-3">
        <div className="flex items-center gap-3">
          <div className={cn('w-8 h-8 rounded-lg flex items-center justify-center', typeConfig.bgColor)}>
            <TypeIcon className={cn('w-4 h-4', typeConfig.color)} />
          </div>
          <div>
            <div className="font-mono text-sm text-cyan-400">{bom.bomNumber}</div>
            <div className="text-xs text-muted-foreground truncate max-w-[200px]">{bom.name}</div>
          </div>
        </div>
      </td>
      <td className="px-4 py-3">
        <span className={cn('px-2 py-0.5 rounded text-xs', typeConfig.bgColor, typeConfig.color)}>
          {typeConfig.label}
        </span>
      </td>
      <td className="px-4 py-3 text-sm text-foreground">
        {bom.outputProduct?.name || 'Unknown'}
      </td>
      <td className="px-4 py-3 text-sm font-mono text-foreground">
        {bom.outputQuantity} {bom.outputUom}
      </td>
      <td className="px-4 py-3 text-sm text-foreground">
        {bom.inputLines.length}
      </td>
      <td className="px-4 py-3 text-sm text-foreground">
        {bom.expectedYieldPercent}%
      </td>
      <td className="px-4 py-3">
        <div className="flex items-center gap-2">
          <StatusBadge status={bom.status} />
          {bom.isDefault && (
            <span className="px-1.5 py-0.5 rounded text-xs bg-emerald-500/10 text-emerald-400">
              Default
            </span>
          )}
        </div>
      </td>
      <td className="px-4 py-3 text-right text-sm font-mono text-foreground">
        {bom.estimatedUnitCost ? `$${bom.estimatedUnitCost.toFixed(2)}` : '—'}
      </td>
      <td className="px-4 py-3">
        <div className="relative">
          <button
            onClick={() => setMenuOpen(!menuOpen)}
            className="p-1.5 rounded-lg hover:bg-white/5 text-muted-foreground hover:text-foreground opacity-0 group-hover:opacity-100 transition-all"
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
                  <Play className="w-4 h-4" />
                  Create Order
                </button>
              </div>
            </>
          )}
        </div>
      </td>
    </tr>
  );
}

export default function BomListPage() {
  const store = useManufacturingStore();
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedType, setSelectedType] = useState<BomType | 'all'>('all');
  const [viewMode, setViewMode] = useState<'grid' | 'table'>('grid');

  // Load BOMs on mount
  useEffect(() => {
    const loadBoms = async () => {
      store.setBomsLoading(true);
      try {
        const response = await BomService.getBoms({
          bomType: selectedType === 'all' ? undefined : [selectedType],
          search: searchQuery || undefined,
        });
        store.setBoms(response.items);
      } catch (error) {
        store.setBomsError('Failed to load BOMs');
      } finally {
        store.setBomsLoading(false);
      }
    };

    loadBoms();
  }, [selectedType, searchQuery]);

  // Calculate type counts
  const typeCounts = store.boms.reduce((acc, bom) => {
    acc[bom.bomType] = (acc[bom.bomType] || 0) + 1;
    return acc;
  }, {} as Record<string, number>);

  // Filter BOMs
  let filteredBoms = store.boms;

  if (selectedType !== 'all') {
    filteredBoms = filteredBoms.filter((b) => b.bomType === selectedType);
  }

  if (searchQuery) {
    const query = searchQuery.toLowerCase();
    filteredBoms = filteredBoms.filter(
      (b) =>
        b.name.toLowerCase().includes(query) ||
        b.bomNumber.toLowerCase().includes(query)
    );
  }

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
              <div className="w-10 h-10 rounded-xl bg-amber-500/10 flex items-center justify-center">
                <Layers className="w-5 h-5 text-amber-400" />
              </div>
              <div>
                <h1 className="text-xl font-semibold text-foreground">Bill of Materials</h1>
                <p className="text-sm text-muted-foreground">
                  Manufacturing recipes and production specifications
                </p>
              </div>
            </div>

            {/* Actions */}
            <div className="flex items-center gap-3">
              <Link
                href="/inventory/boms/new"
                className="flex items-center gap-2 px-4 py-2 rounded-lg bg-cyan-500 text-black font-medium hover:bg-cyan-400 transition-colors"
              >
                <Plus className="w-4 h-4" />
                <span className="text-sm">New BOM</span>
              </Link>
            </div>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="px-6 py-6 space-y-6">
        {/* Type Tabs */}
        <TypeTabs
          selected={selectedType}
          onChange={setSelectedType}
          counts={typeCounts}
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
              placeholder="Search BOMs by number or name..."
              className="w-full pl-10 pr-4 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-cyan-500/30"
            />
          </div>

          {/* View Toggle & Filter */}
          <div className="flex items-center gap-2">
            <div className="flex items-center bg-muted/30 rounded-lg p-1">
              <button
                onClick={() => setViewMode('grid')}
                className={cn(
                  'p-1.5 rounded transition-colors',
                  viewMode === 'grid'
                    ? 'bg-cyan-500/10 text-cyan-400'
                    : 'text-muted-foreground hover:text-foreground'
                )}
              >
                <Layers className="w-4 h-4" />
              </button>
              <button
                onClick={() => setViewMode('table')}
                className={cn(
                  'p-1.5 rounded transition-colors',
                  viewMode === 'table'
                    ? 'bg-cyan-500/10 text-cyan-400'
                    : 'text-muted-foreground hover:text-foreground'
                )}
              >
                <Filter className="w-4 h-4" />
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
          Showing {filteredBoms.length} BOMs
        </div>

        {/* Loading State */}
        {store.bomsLoading ? (
          <div className="flex items-center justify-center py-12">
            <div className="animate-spin w-8 h-8 border-2 border-cyan-500 border-t-transparent rounded-full" />
          </div>
        ) : viewMode === 'grid' ? (
          /* Grid View */
          <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
            {filteredBoms.map((bom) => (
              <BomCard
                key={bom.id}
                bom={bom}
                onView={() => {
                  window.location.href = `/inventory/boms/${bom.id}`;
                }}
                onEdit={() => {
                  window.location.href = `/inventory/boms/${bom.id}/edit`;
                }}
              />
            ))}
          </div>
        ) : (
          /* Table View */
          <div className="bg-surface border border-border rounded-xl overflow-hidden">
            <table className="w-full">
              <thead>
                <tr className="border-b border-border bg-muted/30">
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                    BOM
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                    Type
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                    Output Product
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                    Qty
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                    Inputs
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                    Yield
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                    Status
                  </th>
                  <th className="px-4 py-3 text-right text-xs font-medium text-muted-foreground uppercase tracking-wider">
                    Unit Cost
                  </th>
                  <th className="px-4 py-3 w-12"></th>
                </tr>
              </thead>
              <tbody>
                {filteredBoms.map((bom) => (
                  <BomRow
                    key={bom.id}
                    bom={bom}
                    onView={() => {
                      window.location.href = `/inventory/boms/${bom.id}`;
                    }}
                    onEdit={() => {
                      window.location.href = `/inventory/boms/${bom.id}/edit`;
                    }}
                  />
                ))}
              </tbody>
            </table>

            {filteredBoms.length === 0 && (
              <div className="py-12 text-center">
                <Layers className="w-12 h-12 text-muted-foreground mx-auto mb-3" />
                <p className="text-muted-foreground">No BOMs found</p>
                <p className="text-sm text-muted-foreground/60 mt-1">
                  Create a BOM to define manufacturing recipes
                </p>
              </div>
            )}
          </div>
        )}

        {filteredBoms.length === 0 && !store.bomsLoading && (
          <div className="text-center py-8">
            <Link
              href="/inventory/boms/new"
              className="inline-flex items-center gap-2 px-4 py-2 rounded-lg bg-cyan-500/10 text-cyan-400 hover:bg-cyan-500/20 transition-colors"
            >
              <Plus className="w-4 h-4" />
              Create Your First BOM
            </Link>
          </div>
        )}
      </main>
    </div>
  );
}

