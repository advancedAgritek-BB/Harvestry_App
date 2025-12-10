'use client';

import React, { useState, useMemo } from 'react';
import { cn } from '@/lib/utils';
import {
  Search,
  Filter,
  Grid3X3,
  List,
  MoreVertical,
  Edit2,
  Trash2,
  Eye,
  ChevronDown,
  Dna,
  Leaf,
  FlaskConical,
  TrendingUp,
} from 'lucide-react';
import Link from 'next/link';
import type { Genetics, GeneticType, YieldPotential } from '../../types';
import { GENETIC_TYPE_CONFIG, YIELD_POTENTIAL_CONFIG } from '../../types';

// =============================================================================
// TYPES
// =============================================================================

interface GeneticsListProps {
  genetics: Genetics[];
  isLoading?: boolean;
  viewMode?: 'table' | 'grid';
  onViewModeChange?: (mode: 'table' | 'grid') => void;
  onEdit?: (genetics: Genetics) => void;
  onDelete?: (genetics: Genetics) => void;
  onSelect?: (genetics: Genetics) => void;
}

// =============================================================================
// HELPER COMPONENTS
// =============================================================================

function GeneticTypeBadge({ type }: { type: GeneticType }) {
  const config = GENETIC_TYPE_CONFIG[type];
  return (
    <span
      className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium"
      style={{
        backgroundColor: `${config.color}20`,
        color: config.color,
      }}
    >
      <span
        className="w-1.5 h-1.5 rounded-full"
        style={{ backgroundColor: config.color }}
      />
      {config.label}
    </span>
  );
}

function YieldBadge({ yield: yieldPotential }: { yield: YieldPotential }) {
  const config = YIELD_POTENTIAL_CONFIG[yieldPotential];
  return (
    <span
      className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium"
      style={{
        backgroundColor: `${config.color}15`,
        color: config.color,
      }}
    >
      {config.label}
    </span>
  );
}

function CannabinoidRange({ 
  label, 
  min, 
  max, 
  color 
}: { 
  label: string; 
  min: number; 
  max: number; 
  color: string;
}) {
  return (
    <div className="flex items-center gap-2">
      <span className="text-xs text-muted-foreground w-8">{label}</span>
      <div className="flex-1 h-1.5 bg-muted/30 rounded-full overflow-hidden max-w-[80px]">
        <div
          className="h-full rounded-full"
          style={{
            width: `${Math.min(max, 30) / 30 * 100}%`,
            backgroundColor: color,
          }}
        />
      </div>
      <span className="text-xs font-medium text-foreground/70 w-14 text-right">
        {min === max ? `${min}%` : `${min}-${max}%`}
      </span>
    </div>
  );
}

function ActionMenu({ 
  genetics, 
  onEdit, 
  onDelete 
}: { 
  genetics: Genetics; 
  onEdit?: (g: Genetics) => void;
  onDelete?: (g: Genetics) => void;
}) {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <div className="relative">
      <button
        onClick={(e) => {
          e.stopPropagation();
          setIsOpen(!isOpen);
        }}
        className="p-1.5 rounded-lg hover:bg-muted/50 text-muted-foreground hover:text-foreground transition-colors"
      >
        <MoreVertical className="w-4 h-4" />
      </button>

      {isOpen && (
        <>
          <div
            className="fixed inset-0 z-40"
            onClick={() => setIsOpen(false)}
          />
          <div className="absolute right-0 top-full mt-1 w-36 bg-surface border border-border rounded-lg shadow-xl z-50 py-1">
            <Link
              href={`/library/genetics/${genetics.id}`}
              className="flex items-center gap-2 px-3 py-2 text-sm text-foreground hover:bg-muted/50 transition-colors"
            >
              <Eye className="w-4 h-4" />
              View Details
            </Link>
            {onEdit && (
              <button
                onClick={(e) => {
                  e.stopPropagation();
                  onEdit(genetics);
                  setIsOpen(false);
                }}
                className="w-full flex items-center gap-2 px-3 py-2 text-sm text-foreground hover:bg-muted/50 transition-colors"
              >
                <Edit2 className="w-4 h-4" />
                Edit
              </button>
            )}
            {onDelete && (
              <button
                onClick={(e) => {
                  e.stopPropagation();
                  onDelete(genetics);
                  setIsOpen(false);
                }}
                className="w-full flex items-center gap-2 px-3 py-2 text-sm text-rose-400 hover:bg-rose-500/10 transition-colors"
              >
                <Trash2 className="w-4 h-4" />
                Delete
              </button>
            )}
          </div>
        </>
      )}
    </div>
  );
}

// =============================================================================
// TABLE VIEW
// =============================================================================

function GeneticsTable({
  genetics,
  onEdit,
  onDelete,
  onSelect,
}: {
  genetics: Genetics[];
  onEdit?: (g: Genetics) => void;
  onDelete?: (g: Genetics) => void;
  onSelect?: (g: Genetics) => void;
}) {
  return (
    <div className="overflow-x-auto rounded-lg border border-border">
      <table className="w-full">
        <thead>
          <tr className="bg-muted/30 border-b border-border">
            <th className="px-4 py-3 text-left text-xs font-semibold text-muted-foreground uppercase tracking-wider">
              Name
            </th>
            <th className="px-4 py-3 text-left text-xs font-semibold text-muted-foreground uppercase tracking-wider">
              Type
            </th>
            <th className="px-4 py-3 text-left text-xs font-semibold text-muted-foreground uppercase tracking-wider">
              THC
            </th>
            <th className="px-4 py-3 text-left text-xs font-semibold text-muted-foreground uppercase tracking-wider">
              CBD
            </th>
            <th className="px-4 py-3 text-left text-xs font-semibold text-muted-foreground uppercase tracking-wider">
              Flowering
            </th>
            <th className="px-4 py-3 text-left text-xs font-semibold text-muted-foreground uppercase tracking-wider">
              Yield
            </th>
            <th className="px-4 py-3 text-right text-xs font-semibold text-muted-foreground uppercase tracking-wider">
              Actions
            </th>
          </tr>
        </thead>
        <tbody className="divide-y divide-border">
          {genetics.map((item) => (
            <tr
              key={item.id}
              onClick={() => onSelect?.(item)}
              className={cn(
                "bg-surface hover:bg-muted/20 transition-colors",
                onSelect && "cursor-pointer"
              )}
            >
              <td className="px-4 py-3">
                <Link
                  href={`/library/genetics/${item.id}`}
                  className="font-medium text-foreground hover:text-emerald-400 transition-colors"
                  onClick={(e) => e.stopPropagation()}
                >
                  {item.name}
                </Link>
                <p className="text-xs text-muted-foreground line-clamp-1 mt-0.5 max-w-xs">
                  {item.description}
                </p>
              </td>
              <td className="px-4 py-3">
                <GeneticTypeBadge type={item.geneticType} />
              </td>
              <td className="px-4 py-3">
                <span className="text-sm font-medium text-emerald-400">
                  {item.thcMin === item.thcMax 
                    ? `${item.thcMin}%` 
                    : `${item.thcMin}-${item.thcMax}%`}
                </span>
              </td>
              <td className="px-4 py-3">
                <span className="text-sm text-muted-foreground">
                  {item.cbdMin === item.cbdMax 
                    ? `${item.cbdMin}%` 
                    : `${item.cbdMin}-${item.cbdMax}%`}
                </span>
              </td>
              <td className="px-4 py-3">
                {item.floweringTimeDays ? (
                  <span className="text-sm text-foreground">
                    {item.floweringTimeDays} days
                  </span>
                ) : (
                  <span className="text-sm text-muted-foreground">—</span>
                )}
              </td>
              <td className="px-4 py-3">
                <YieldBadge yield={item.yieldPotential} />
              </td>
              <td className="px-4 py-3 text-right">
                <ActionMenu
                  genetics={item}
                  onEdit={onEdit}
                  onDelete={onDelete}
                />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

// =============================================================================
// GRID VIEW (CARDS)
// =============================================================================

function GeneticsCard({
  genetics,
  onEdit,
  onDelete,
  onSelect,
}: {
  genetics: Genetics;
  onEdit?: (g: Genetics) => void;
  onDelete?: (g: Genetics) => void;
  onSelect?: (g: Genetics) => void;
}) {
  const typeConfig = GENETIC_TYPE_CONFIG[genetics.geneticType];

  return (
    <div
      onClick={() => onSelect?.(genetics)}
      className={cn(
        "group relative bg-surface border border-border rounded-xl overflow-hidden transition-all duration-200 hover:border-emerald-500/30 hover:shadow-lg hover:shadow-emerald-500/5",
        onSelect && "cursor-pointer"
      )}
    >
      {/* Header with gradient */}
      <div
        className="h-2"
        style={{
          background: `linear-gradient(90deg, ${typeConfig.color}, ${typeConfig.color}50)`,
        }}
      />

      <div className="p-4 space-y-3">
        {/* Title and Type */}
        <div className="flex items-start justify-between gap-2">
          <div className="flex-1 min-w-0">
            <Link
              href={`/library/genetics/${genetics.id}`}
              className="font-semibold text-foreground hover:text-emerald-400 transition-colors line-clamp-1"
              onClick={(e) => e.stopPropagation()}
            >
              {genetics.name}
            </Link>
            <GeneticTypeBadge type={genetics.geneticType} />
          </div>
          <ActionMenu
            genetics={genetics}
            onEdit={onEdit}
            onDelete={onDelete}
          />
        </div>

        {/* Description */}
        <p className="text-sm text-muted-foreground line-clamp-2">
          {genetics.description}
        </p>

        {/* Cannabinoid Ranges */}
        <div className="space-y-1.5">
          <CannabinoidRange
            label="THC"
            min={genetics.thcMin}
            max={genetics.thcMax}
            color="#10B981"
          />
          <CannabinoidRange
            label="CBD"
            min={genetics.cbdMin}
            max={genetics.cbdMax}
            color="#8B5CF6"
          />
        </div>

        {/* Footer Stats */}
        <div className="flex items-center justify-between pt-2 border-t border-border/50">
          <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
            <FlaskConical className="w-3.5 h-3.5" />
            <span>
              {genetics.floweringTimeDays 
                ? `${genetics.floweringTimeDays} days` 
                : 'Variable'}
            </span>
          </div>
          <YieldBadge yield={genetics.yieldPotential} />
        </div>

        {/* Terpene Tags */}
        {genetics.terpeneProfile.dominantTerpenes && (
          <div className="flex flex-wrap gap-1">
            {Object.keys(genetics.terpeneProfile.dominantTerpenes)
              .slice(0, 3)
              .map((terpene) => (
                <span
                  key={terpene}
                  className="px-1.5 py-0.5 text-[10px] bg-muted/50 text-muted-foreground rounded capitalize"
                >
                  {terpene}
                </span>
              ))}
          </div>
        )}
      </div>
    </div>
  );
}

// =============================================================================
// MAIN COMPONENT
// =============================================================================

export function GeneticsList({
  genetics,
  isLoading = false,
  viewMode = 'table',
  onViewModeChange,
  onEdit,
  onDelete,
  onSelect,
}: GeneticsListProps) {
  const [searchQuery, setSearchQuery] = useState('');
  const [typeFilter, setTypeFilter] = useState<GeneticType | 'all'>('all');
  const [showFilters, setShowFilters] = useState(false);

  // Filter genetics
  const filteredGenetics = useMemo(() => {
    let result = genetics;

    if (searchQuery) {
      const query = searchQuery.toLowerCase();
      result = result.filter(
        (g) =>
          g.name.toLowerCase().includes(query) ||
          g.description.toLowerCase().includes(query)
      );
    }

    if (typeFilter !== 'all') {
      result = result.filter((g) => g.geneticType === typeFilter);
    }

    return result;
  }, [genetics, searchQuery, typeFilter]);

  // Loading state
  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <div className="flex flex-col items-center gap-3">
          <div className="w-8 h-8 border-2 border-emerald-500/30 border-t-emerald-500 rounded-full animate-spin" />
          <p className="text-sm text-muted-foreground">Loading genetics...</p>
        </div>
      </div>
    );
  }

  // Empty state
  if (genetics.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-16 text-center">
        <div className="w-16 h-16 rounded-2xl bg-emerald-500/10 flex items-center justify-center mb-4">
          <Dna className="w-8 h-8 text-emerald-400" />
        </div>
        <h3 className="text-lg font-semibold text-foreground mb-1">
          No genetics yet
        </h3>
        <p className="text-sm text-muted-foreground max-w-sm mb-4">
          Start building your genetics library by adding your first strain genetics.
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {/* Toolbar */}
      <div className="flex items-center justify-between gap-4">
        {/* Search */}
        <div className="relative flex-1 max-w-md">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            placeholder="Search genetics by name or description..."
            className="w-full pl-10 pr-4 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-emerald-500/50 transition-colors"
          />
        </div>

        {/* Filters */}
        <div className="flex items-center gap-2">
          {/* Type Filter */}
          <div className="relative">
            <button
              onClick={() => setShowFilters(!showFilters)}
              className={cn(
                "flex items-center gap-1.5 px-3 py-2 rounded-lg border transition-colors",
                typeFilter !== 'all'
                  ? "border-emerald-500/50 bg-emerald-500/10 text-emerald-400"
                  : "border-border bg-muted/30 text-muted-foreground hover:text-foreground"
              )}
            >
              <Filter className="w-4 h-4" />
              <span className="text-sm">
                {typeFilter === 'all' ? 'All Types' : GENETIC_TYPE_CONFIG[typeFilter].label}
              </span>
              <ChevronDown className="w-3 h-3" />
            </button>

            {showFilters && (
              <>
                <div
                  className="fixed inset-0 z-40"
                  onClick={() => setShowFilters(false)}
                />
                <div className="absolute right-0 top-full mt-1 w-44 bg-surface border border-border rounded-lg shadow-xl z-50 py-1">
                  <button
                    onClick={() => {
                      setTypeFilter('all');
                      setShowFilters(false);
                    }}
                    className={cn(
                      "w-full flex items-center gap-2 px-3 py-2 text-sm transition-colors",
                      typeFilter === 'all'
                        ? "bg-emerald-500/10 text-emerald-400"
                        : "text-foreground hover:bg-muted/50"
                    )}
                  >
                    All Types
                  </button>
                  {(Object.keys(GENETIC_TYPE_CONFIG) as GeneticType[]).map((type) => (
                    <button
                      key={type}
                      onClick={() => {
                        setTypeFilter(type);
                        setShowFilters(false);
                      }}
                      className={cn(
                        "w-full flex items-center gap-2 px-3 py-2 text-sm transition-colors",
                        typeFilter === type
                          ? "bg-emerald-500/10 text-emerald-400"
                          : "text-foreground hover:bg-muted/50"
                      )}
                    >
                      <span
                        className="w-2 h-2 rounded-full"
                        style={{ backgroundColor: GENETIC_TYPE_CONFIG[type].color }}
                      />
                      {GENETIC_TYPE_CONFIG[type].label}
                    </button>
                  ))}
                </div>
              </>
            )}
          </div>

          {/* View Toggle */}
          {onViewModeChange && (
            <div className="flex items-center bg-muted/30 rounded-lg p-1 border border-border">
              <button
                onClick={() => onViewModeChange('table')}
                className={cn(
                  "p-1.5 rounded transition-colors",
                  viewMode === 'table'
                    ? "bg-emerald-500/20 text-emerald-400"
                    : "text-muted-foreground hover:text-foreground"
                )}
              >
                <List className="w-4 h-4" />
              </button>
              <button
                onClick={() => onViewModeChange('grid')}
                className={cn(
                  "p-1.5 rounded transition-colors",
                  viewMode === 'grid'
                    ? "bg-emerald-500/20 text-emerald-400"
                    : "text-muted-foreground hover:text-foreground"
                )}
              >
                <Grid3X3 className="w-4 h-4" />
              </button>
            </div>
          )}
        </div>
      </div>

      {/* Results Count */}
      <div className="text-sm text-muted-foreground">
        Showing {filteredGenetics.length} of {genetics.length} genetics
      </div>

      {/* Content */}
      {filteredGenetics.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-12 text-center">
          <Search className="w-8 h-8 text-muted-foreground mb-3" />
          <p className="text-muted-foreground">No genetics match your search</p>
          <button
            onClick={() => {
              setSearchQuery('');
              setTypeFilter('all');
            }}
            className="mt-2 text-sm text-emerald-400 hover:text-emerald-300"
          >
            Clear filters
          </button>
        </div>
      ) : viewMode === 'table' ? (
        <GeneticsTable
          genetics={filteredGenetics}
          onEdit={onEdit}
          onDelete={onDelete}
          onSelect={onSelect}
        />
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 2xl:grid-cols-4 gap-4">
          {filteredGenetics.map((item) => (
            <GeneticsCard
              key={item.id}
              genetics={item}
              onEdit={onEdit}
              onDelete={onDelete}
              onSelect={onSelect}
            />
          ))}
        </div>
      )}
    </div>
  );
}

export default GeneticsList;


