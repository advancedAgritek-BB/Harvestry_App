'use client';

import { useCallback, useEffect, useRef, useState } from 'react';
import { getPackages, type PackageSummaryDto } from '@/features/inventory/services/packages.service';

export interface PackageSelection {
  id: string;
  packageLabel: string;
  itemId: string;
  itemName: string;
  availableQuantity: number;
  unitOfMeasure: string;
}

interface PackageComboboxProps {
  siteId: string;
  onSelect: (pkg: PackageSelection) => void;
  placeholder?: string;
  disabled?: boolean;
  /** Filter to only show packages with available quantity > 0 */
  availableOnly?: boolean;
}

export function PackageCombobox({
  siteId,
  onSelect,
  placeholder = 'Search packages by label or item name...',
  disabled = false,
  availableOnly = true,
}: PackageComboboxProps) {
  const [search, setSearch] = useState('');
  const [isOpen, setIsOpen] = useState(false);
  const [packages, setPackages] = useState<PackageSummaryDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const containerRef = useRef<HTMLDivElement>(null);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  // Debounced search
  const performSearch = useCallback(
    async (query: string) => {
      if (!siteId) return;

      setIsLoading(true);
      setError(null);

      try {
        const result = await getPackages(siteId, { search: query, status: ['Active'] }, 1, 20);
        let filtered = result.packages;

        if (availableOnly) {
          filtered = filtered.filter((p) => p.availableQuantity > 0);
        }

        setPackages(filtered);
      } catch (e) {
        setError(e instanceof Error ? e.message : 'Failed to search packages');
        setPackages([]);
      } finally {
        setIsLoading(false);
      }
    },
    [siteId, availableOnly]
  );

  // Handle search input change with debounce
  useEffect(() => {
    if (debounceRef.current) {
      clearTimeout(debounceRef.current);
    }

    if (search.length >= 1) {
      debounceRef.current = setTimeout(() => {
        performSearch(search);
      }, 300);
    } else {
      setPackages([]);
    }

    return () => {
      if (debounceRef.current) {
        clearTimeout(debounceRef.current);
      }
    };
  }, [search, performSearch]);

  // Close dropdown on click outside
  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    }

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  function handleSelect(pkg: PackageSummaryDto) {
    onSelect({
      id: pkg.id,
      packageLabel: pkg.packageLabel,
      itemId: pkg.id, // Package ID serves as the item reference for allocation
      itemName: pkg.itemName,
      availableQuantity: pkg.availableQuantity,
      unitOfMeasure: pkg.unitOfMeasure,
    });
    setSearch('');
    setIsOpen(false);
    setPackages([]);
  }

  return (
    <div ref={containerRef} className="relative">
      <input
        type="text"
        value={search}
        onChange={(e) => {
          setSearch(e.target.value);
          setIsOpen(true);
        }}
        onFocus={() => setIsOpen(true)}
        placeholder={placeholder}
        disabled={disabled}
        className="w-full h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30 disabled:opacity-50"
      />

      {isOpen && (search.length >= 1 || packages.length > 0) && (
        <div className="absolute z-50 mt-1 w-full max-h-60 overflow-y-auto rounded-lg bg-surface border border-border shadow-lg">
          {isLoading && (
            <div className="px-3 py-2 text-sm text-muted-foreground">Searching...</div>
          )}

          {error && (
            <div className="px-3 py-2 text-sm text-rose-200">{error}</div>
          )}

          {!isLoading && !error && packages.length === 0 && search.length >= 1 && (
            <div className="px-3 py-2 text-sm text-muted-foreground">No packages found</div>
          )}

          {!isLoading && packages.map((pkg) => (
            <button
              key={pkg.id}
              type="button"
              onClick={() => handleSelect(pkg)}
              className="w-full text-left px-3 py-2 hover:bg-muted/50 transition-colors border-b border-border/50 last:border-0"
            >
              <div className="flex items-center justify-between">
                <span className="font-medium text-sm text-foreground">{pkg.packageLabel}</span>
                <span className="text-xs text-emerald-300">
                  {pkg.availableQuantity} {pkg.unitOfMeasure} avail
                </span>
              </div>
              <div className="text-xs text-muted-foreground mt-0.5">
                {pkg.itemName} • {pkg.itemCategory}
              </div>
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
