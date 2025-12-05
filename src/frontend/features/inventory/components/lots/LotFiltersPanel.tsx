'use client';

import React from 'react';
import { 
  Search,
  Filter,
  X,
  ChevronDown,
  Calendar,
  MapPin,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import type { LotFilters, LotStatus, QAStatus, SyncState } from '../../types';

interface LotFiltersPanelProps {
  filters: LotFilters;
  onFiltersChange: (filters: Partial<LotFilters>) => void;
  onClearFilters: () => void;
  isExpanded?: boolean;
  onToggleExpand?: () => void;
}

const STATUS_OPTIONS: { value: LotStatus; label: string }[] = [
  { value: 'available', label: 'Available' },
  { value: 'reserved', label: 'Reserved' },
  { value: 'on_hold', label: 'On Hold' },
  { value: 'quarantine', label: 'Quarantine' },
  { value: 'in_transit', label: 'In Transit' },
  { value: 'consumed', label: 'Consumed' },
  { value: 'destroyed', label: 'Destroyed' },
];

const QA_STATUS_OPTIONS: { value: QAStatus; label: string }[] = [
  { value: 'passed', label: 'Passed' },
  { value: 'pending', label: 'Pending' },
  { value: 'failed', label: 'Failed' },
  { value: 'expired', label: 'Expired' },
];

const SYNC_STATUS_OPTIONS: { value: SyncState; label: string }[] = [
  { value: 'synced', label: 'Synced' },
  { value: 'pending', label: 'Pending' },
  { value: 'error', label: 'Error' },
  { value: 'stale', label: 'Stale' },
];

interface FilterTagProps {
  label: string;
  onRemove: () => void;
}

function FilterTag({ label, onRemove }: FilterTagProps) {
  return (
    <span className="flex items-center gap-1 px-2 py-0.5 rounded-full bg-amber-500/10 text-amber-400 text-xs">
      {label}
      <button 
        onClick={onRemove}
        className="hover:bg-amber-500/20 rounded-full p-0.5 transition-colors"
      >
        <X className="w-3 h-3" />
      </button>
    </span>
  );
}

interface MultiSelectProps {
  label: string;
  options: { value: string; label: string }[];
  selected: string[];
  onChange: (values: string[]) => void;
}

function MultiSelect({ label, options, selected, onChange }: MultiSelectProps) {
  const [isOpen, setIsOpen] = React.useState(false);
  
  const toggleOption = (value: string) => {
    if (selected.includes(value)) {
      onChange(selected.filter((v) => v !== value));
    } else {
      onChange([...selected, value]);
    }
  };
  
  return (
    <div className="relative">
      <button
        onClick={() => setIsOpen(!isOpen)}
        className={cn(
          'flex items-center justify-between gap-2 w-full px-3 py-2 rounded-lg',
          'bg-white/5 border border-border text-sm text-foreground',
          'hover:border-border transition-colors',
          isOpen && 'border-amber-500/30'
        )}
      >
        <span className={selected.length > 0 ? 'text-foreground' : 'text-muted-foreground'}>
          {selected.length > 0 ? `${selected.length} selected` : label}
        </span>
        <ChevronDown className={cn(
          'w-4 h-4 text-muted-foreground transition-transform',
          isOpen && 'rotate-180'
        )} />
      </button>
      
      {isOpen && (
        <>
          <div 
            className="fixed inset-0 z-10" 
            onClick={() => setIsOpen(false)} 
          />
          <div className={cn(
            'absolute z-20 top-full left-0 right-0 mt-1',
            'bg-surface border border-border rounded-lg shadow-xl',
            'max-h-48 overflow-y-auto'
          )}>
            {options.map((option) => (
              <button
                key={option.value}
                onClick={() => toggleOption(option.value)}
                className={cn(
                  'w-full px-3 py-2 text-left text-sm transition-colors',
                  'hover:bg-white/5',
                  selected.includes(option.value) 
                    ? 'text-amber-400 bg-amber-500/5' 
                    : 'text-foreground'
                )}
              >
                <div className="flex items-center gap-2">
                  <div className={cn(
                    'w-4 h-4 rounded border-2 flex items-center justify-center',
                    selected.includes(option.value)
                      ? 'border-amber-500 bg-amber-500'
                      : 'border-white/20'
                  )}>
                    {selected.includes(option.value) && (
                      <svg className="w-2.5 h-2.5 text-black" viewBox="0 0 12 12">
                        <path fill="currentColor" d="M10.3 2.3L4 8.6 1.7 6.3c-.4-.4-1-.4-1.4 0s-.4 1 0 1.4l3 3c.4.4 1 .4 1.4 0l7-7c.4-.4.4-1 0-1.4s-1-.4-1.4 0z"/>
                      </svg>
                    )}
                  </div>
                  {option.label}
                </div>
              </button>
            ))}
          </div>
        </>
      )}
    </div>
  );
}

export function LotFiltersPanel({
  filters,
  onFiltersChange,
  onClearFilters,
  isExpanded = false,
  onToggleExpand,
}: LotFiltersPanelProps) {
  const activeFilterCount = [
    filters.status?.length,
    filters.qaStatus?.length,
    filters.syncStatus?.length,
    filters.locationId,
    filters.productType,
    filters.strainId,
    filters.dateRange,
    filters.expiringWithinDays,
  ].filter(Boolean).length;
  
  return (
    <div className="space-y-4">
      {/* Search and Toggle */}
      <div className="flex items-center gap-3">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
          <input
            type="text"
            value={filters.search || ''}
            onChange={(e) => onFiltersChange({ search: e.target.value })}
            placeholder="Search lots by number, product, or strain..."
            className={cn(
              'w-full pl-9 pr-4 py-2.5 rounded-lg text-sm',
              'bg-white/5 border border-border',
              'placeholder:text-muted-foreground text-foreground',
              'focus:outline-none focus:border-amber-500/30 focus:ring-1 focus:ring-amber-500/30',
              'transition-colors'
            )}
          />
        </div>
        
        <button
          onClick={onToggleExpand}
          className={cn(
            'flex items-center gap-2 px-4 py-2.5 rounded-lg text-sm font-medium transition-colors',
            isExpanded 
              ? 'bg-amber-500/10 text-amber-400 border border-amber-500/30' 
              : 'bg-white/5 text-foreground hover:bg-white/10 border border-transparent'
          )}
        >
          <Filter className="w-4 h-4" />
          Filters
          {activeFilterCount > 0 && (
            <span className="px-1.5 py-0.5 rounded-full bg-amber-500 text-black text-xs">
              {activeFilterCount}
            </span>
          )}
        </button>
        
        {activeFilterCount > 0 && (
          <button
            onClick={onClearFilters}
            className="px-3 py-2.5 rounded-lg text-sm text-muted-foreground hover:text-foreground hover:bg-white/5 transition-colors"
          >
            Clear All
          </button>
        )}
      </div>
      
      {/* Expanded Filters */}
      {isExpanded && (
        <div className="p-4 rounded-xl bg-muted/30 border border-border space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            <MultiSelect
              label="Status"
              options={STATUS_OPTIONS}
              selected={filters.status || []}
              onChange={(values) => onFiltersChange({ status: values as LotStatus[] })}
            />
            
            <MultiSelect
              label="QA Status"
              options={QA_STATUS_OPTIONS}
              selected={filters.qaStatus || []}
              onChange={(values) => onFiltersChange({ qaStatus: values as QAStatus[] })}
            />
            
            <MultiSelect
              label="Sync Status"
              options={SYNC_STATUS_OPTIONS}
              selected={filters.syncStatus || []}
              onChange={(values) => onFiltersChange({ syncStatus: values as SyncState[] })}
            />
            
            <div className="relative">
              <MapPin className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
              <input
                type="text"
                placeholder="Location..."
                className={cn(
                  'w-full pl-9 pr-4 py-2 rounded-lg text-sm',
                  'bg-white/5 border border-border',
                  'placeholder:text-muted-foreground text-foreground',
                  'focus:outline-none focus:border-amber-500/30'
                )}
              />
            </div>
          </div>
          
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            <div>
              <label className="block text-xs text-muted-foreground mb-1">Expiring Within</label>
              <select
                value={filters.expiringWithinDays || ''}
                onChange={(e) => onFiltersChange({ 
                  expiringWithinDays: e.target.value ? parseInt(e.target.value) : undefined 
                })}
                className={cn(
                  'w-full px-3 py-2 rounded-lg text-sm',
                  'bg-white/5 border border-border text-foreground',
                  'focus:outline-none focus:border-amber-500/30'
                )}
              >
                <option value="">Any time</option>
                <option value="7">7 days</option>
                <option value="14">14 days</option>
                <option value="30">30 days</option>
                <option value="60">60 days</option>
                <option value="90">90 days</option>
              </select>
            </div>
            
            <div>
              <label className="block text-xs text-muted-foreground mb-1">Date Range</label>
              <div className="flex items-center gap-2">
                <input
                  type="date"
                  className={cn(
                    'flex-1 px-3 py-2 rounded-lg text-sm',
                    'bg-white/5 border border-border text-foreground',
                    'focus:outline-none focus:border-amber-500/30'
                  )}
                />
                <span className="text-muted-foreground">to</span>
                <input
                  type="date"
                  className={cn(
                    'flex-1 px-3 py-2 rounded-lg text-sm',
                    'bg-white/5 border border-border text-foreground',
                    'focus:outline-none focus:border-amber-500/30'
                  )}
                />
              </div>
            </div>
          </div>
        </div>
      )}
      
      {/* Active Filter Tags */}
      {activeFilterCount > 0 && !isExpanded && (
        <div className="flex flex-wrap gap-2">
          {filters.status?.map((status) => (
            <FilterTag
              key={status}
              label={STATUS_OPTIONS.find((o) => o.value === status)?.label || status}
              onRemove={() => onFiltersChange({ 
                status: filters.status?.filter((s) => s !== status) 
              })}
            />
          ))}
          {filters.qaStatus?.map((status) => (
            <FilterTag
              key={status}
              label={QA_STATUS_OPTIONS.find((o) => o.value === status)?.label || status}
              onRemove={() => onFiltersChange({ 
                qaStatus: filters.qaStatus?.filter((s) => s !== status) 
              })}
            />
          ))}
          {filters.expiringWithinDays && (
            <FilterTag
              label={`Expiring in ${filters.expiringWithinDays}d`}
              onRemove={() => onFiltersChange({ expiringWithinDays: undefined })}
            />
          )}
        </div>
      )}
    </div>
  );
}

export default LotFiltersPanel;

