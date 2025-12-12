'use client';

import React, { useState, useCallback, useRef, useEffect } from 'react';
import { Search, X, Package, MapPin, Tag, History, Loader2 } from 'lucide-react';
import { cn } from '@/lib/utils';

interface SearchResult {
  type: 'package' | 'item' | 'location';
  id: string;
  label: string;
  sublabel: string;
  icon: React.ReactNode;
  metadata?: Record<string, string>;
}

interface GlobalInventorySearchProps {
  siteId: string;
  onSelect: (result: SearchResult) => void;
  placeholder?: string;
  className?: string;
}

function useDebounce<T>(value: T, delay: number): T {
  const [debouncedValue, setDebouncedValue] = useState<T>(value);

  useEffect(() => {
    const handler = setTimeout(() => setDebouncedValue(value), delay);
    return () => clearTimeout(handler);
  }, [value, delay]);

  return debouncedValue;
}

export function GlobalInventorySearch({
  siteId,
  onSelect,
  placeholder = 'Search packages, items, or locations...',
  className = '',
}: GlobalInventorySearchProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<SearchResult[]>([]);
  const [recentSearches, setRecentSearches] = useState<string[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);
  const debouncedQuery = useDebounce(query, 300);

  // Load recent searches from localStorage
  useEffect(() => {
    const saved = localStorage.getItem(`inventory-search-history-${siteId}`);
    if (saved) {
      try {
        setRecentSearches(JSON.parse(saved).slice(0, 5));
      } catch {
        // Ignore parse errors
      }
    }
  }, [siteId]);

  // Close on click outside
  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setIsOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const saveSearch = useCallback((searchQuery: string) => {
    if (!searchQuery.trim()) return;
    const updated = [searchQuery, ...recentSearches.filter(s => s !== searchQuery)].slice(0, 5);
    setRecentSearches(updated);
    localStorage.setItem(`inventory-search-history-${siteId}`, JSON.stringify(updated));
  }, [siteId, recentSearches]);

  const search = useCallback(async (searchQuery: string) => {
    if (!searchQuery.trim() || searchQuery.length < 2) {
      setResults([]);
      return;
    }

    setIsLoading(true);
    try {
      // Search packages
      const packagesRes = await fetch(
        `/api/v1/sites/${siteId}/packages?search=${encodeURIComponent(searchQuery)}&pageSize=5`
      );
      const packagesData = packagesRes.ok ? await packagesRes.json() : { packages: [] };

      // Search items
      const itemsRes = await fetch(
        `/api/v1/sites/${siteId}/items?search=${encodeURIComponent(searchQuery)}&pageSize=5`
      );
      const itemsData = itemsRes.ok ? await itemsRes.json() : { items: [] };

      // Combine results
      const combinedResults: SearchResult[] = [
        ...packagesData.packages.map((p: any) => ({
          type: 'package' as const,
          id: p.id,
          label: p.packageLabel,
          sublabel: `${p.itemName} • ${p.quantity} ${p.unitOfMeasure}`,
          icon: <Package className="h-4 w-4 text-cyan-500" />,
          metadata: { status: p.status, location: p.locationName },
        })),
        ...itemsData.items.map((i: any) => ({
          type: 'item' as const,
          id: i.id,
          label: i.name,
          sublabel: i.sku ? `SKU: ${i.sku}` : i.category,
          icon: <Tag className="h-4 w-4 text-emerald-500" />,
          metadata: { category: i.category },
        })),
      ];

      setResults(combinedResults.slice(0, 10));
    } catch (error) {
      console.error('Search error:', error);
      setResults([]);
    } finally {
      setIsLoading(false);
    }
  }, [siteId]);

  useEffect(() => {
    search(debouncedQuery);
  }, [debouncedQuery, search]);

  const handleSelect = useCallback((result: SearchResult) => {
    saveSearch(result.label);
    onSelect(result);
    setIsOpen(false);
    setQuery('');
  }, [onSelect, saveSearch]);

  const handleClear = useCallback(() => {
    setQuery('');
    setResults([]);
    inputRef.current?.focus();
  }, []);

  return (
    <div ref={containerRef} className={cn('relative', className)}>
      <div className="relative">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <input
          ref={inputRef}
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          onFocus={() => setIsOpen(true)}
          placeholder={placeholder}
          className="w-full pl-10 pr-8 py-2 rounded-lg border border-border bg-background text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/50"
        />
        {query && (
          <button
            className="absolute right-2 top-1/2 -translate-y-1/2 p-1 rounded hover:bg-muted transition-colors"
            onClick={handleClear}
          >
            <X className="h-4 w-4 text-muted-foreground" />
          </button>
        )}
      </div>
      
      {isOpen && (
        <div className="absolute top-full left-0 right-0 mt-1 bg-background border border-border rounded-lg shadow-xl z-50 overflow-hidden">
          <div className="max-h-80 overflow-y-auto">
            {isLoading && (
              <div className="flex items-center justify-center p-4">
                <Loader2 className="h-5 w-5 animate-spin text-muted-foreground" />
              </div>
            )}

            {!isLoading && results.length > 0 && (
              <div className="p-1">
                {results.map((result) => (
                  <button
                    key={`${result.type}-${result.id}`}
                    className="w-full flex items-start gap-3 p-2 rounded hover:bg-muted text-left transition-colors"
                    onClick={() => handleSelect(result)}
                  >
                    <div className="mt-0.5">{result.icon}</div>
                    <div className="flex-1 min-w-0">
                      <div className="font-medium text-foreground truncate">{result.label}</div>
                      <div className="text-sm text-muted-foreground truncate">{result.sublabel}</div>
                    </div>
                    <span className="text-xs px-2 py-0.5 rounded-full bg-muted text-muted-foreground">
                      {result.type}
                    </span>
                  </button>
                ))}
              </div>
            )}

            {!isLoading && query && results.length === 0 && (
              <div className="p-4 text-center text-muted-foreground text-sm">
                No results found for "{query}"
              </div>
            )}

            {!query && recentSearches.length > 0 && (
              <div className="p-2">
                <div className="text-xs font-medium text-muted-foreground px-2 pb-2 flex items-center gap-1">
                  <History className="h-3 w-3" />
                  Recent Searches
                </div>
                {recentSearches.map((search, i) => (
                  <button
                    key={i}
                    className="w-full text-left px-2 py-1.5 text-sm text-foreground rounded hover:bg-muted transition-colors"
                    onClick={() => {
                      setQuery(search);
                      inputRef.current?.focus();
                    }}
                  >
                    {search}
                  </button>
                ))}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

export default GlobalInventorySearch;




