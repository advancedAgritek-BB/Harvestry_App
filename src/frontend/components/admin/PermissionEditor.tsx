'use client';

import React, { useState, useMemo, useCallback } from 'react';
import { Search, Sparkles, Check, ChevronDown } from 'lucide-react';
import { cn } from '@/lib/utils';
import { PermissionSectionAccordion } from './PermissionSectionAccordion';
import type {
  PermissionEditorProps,
  PermissionSectionWithIcon,
  PermissionBundle,
} from '@/types/permissions';
import { getSectionPermissionKeys } from '@/types/permissions';
import {
  PERMISSION_SECTIONS,
  PERMISSION_BUNDLES,
  doesBundleMatch,
} from '@/lib/permissions/registry';

/**
 * PermissionEditor
 *
 * Complete permission management interface with:
 * - Bundle quick-select for common role presets
 * - Search to filter permissions across all sections
 * - Expandable sections with individual permission control
 */
export function PermissionEditor({
  selectedPermissions,
  onChange,
  readOnly = false,
  showBundles = true,
  className,
}: PermissionEditorProps) {
  const [searchQuery, setSearchQuery] = useState('');
  const [expandedSections, setExpandedSections] = useState<Set<string>>(new Set());
  const [showBundleDropdown, setShowBundleDropdown] = useState(false);

  // Convert selected permissions to Set for efficient lookups
  const selectedSet = useMemo(
    () => new Set(selectedPermissions),
    [selectedPermissions]
  );

  // Filter sections based on search query
  const filteredSections = useMemo(() => {
    if (!searchQuery.trim()) return PERMISSION_SECTIONS;

    const query = searchQuery.toLowerCase();
    return PERMISSION_SECTIONS.map((section) => ({
      ...section,
      permissions: section.permissions.filter(
        (p) =>
          p.label.toLowerCase().includes(query) ||
          p.key.toLowerCase().includes(query) ||
          p.description?.toLowerCase().includes(query)
      ),
    })).filter((section) => section.permissions.length > 0);
  }, [searchQuery]);

  // Auto-expand sections when searching
  React.useEffect(() => {
    if (searchQuery.trim()) {
      setExpandedSections(new Set(filteredSections.map((s) => s.id)));
    }
  }, [searchQuery, filteredSections]);

  // Find matching bundle for current selection
  const matchingBundle = useMemo(() => {
    for (const bundle of PERMISSION_BUNDLES) {
      if (doesBundleMatch(bundle, selectedSet) === 'full') {
        return bundle;
      }
    }
    return null;
  }, [selectedSet]);

  // Toggle a single permission
  const handleTogglePermission = useCallback(
    (permissionKey: string) => {
      const newPermissions = selectedSet.has(permissionKey)
        ? selectedPermissions.filter((p) => p !== permissionKey)
        : [...selectedPermissions, permissionKey];
      onChange(newPermissions);
    },
    [selectedPermissions, selectedSet, onChange]
  );

  // Toggle all permissions in a section
  const handleToggleSection = useCallback(
    (sectionId: string, selectAll: boolean) => {
      const section = PERMISSION_SECTIONS.find((s) => s.id === sectionId);
      if (!section) return;

      const sectionKeys = getSectionPermissionKeys(section);

      if (selectAll) {
        // Add all section permissions
        const newPermissions = new Set([...selectedPermissions, ...sectionKeys]);
        onChange(Array.from(newPermissions));
      } else {
        // Remove all section permissions
        const keysToRemove = new Set(sectionKeys);
        onChange(selectedPermissions.filter((p) => !keysToRemove.has(p)));
      }
    },
    [selectedPermissions, onChange]
  );

  // Toggle section expansion
  const handleToggleExpanded = useCallback((sectionId: string) => {
    setExpandedSections((prev) => {
      const next = new Set(prev);
      if (next.has(sectionId)) {
        next.delete(sectionId);
      } else {
        next.add(sectionId);
      }
      return next;
    });
  }, []);

  // Apply a bundle
  const handleApplyBundle = useCallback(
    (bundle: PermissionBundle) => {
      onChange([...bundle.permissions]);
      setShowBundleDropdown(false);
    },
    [onChange]
  );

  // Calculate total counts
  const totalPermissions = PERMISSION_SECTIONS.reduce(
    (sum, s) => sum + s.permissions.length,
    0
  );

  return (
    <div className={cn('space-y-4', className)}>
      {/* Header with Search and Bundle Select */}
      <div className="flex items-center gap-3">
        {/* Search Input */}
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
          <input
            type="text"
            placeholder="Search permissions..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className={cn(
              'w-full h-10 pl-10 pr-4 bg-muted border border-border rounded-lg text-sm text-foreground',
              'placeholder:text-muted-foreground/50',
              'focus:outline-none focus:ring-2 focus:ring-violet-500/30 focus:border-violet-500/50',
              'transition-all duration-200'
            )}
          />
        </div>

        {/* Bundle Quick Select */}
        {showBundles && !readOnly && (
          <div className="relative">
            <button
              type="button"
              onClick={() => setShowBundleDropdown(!showBundleDropdown)}
              className={cn(
                'flex items-center gap-2 h-10 px-4 bg-muted border border-border rounded-lg',
                'text-sm text-foreground hover:bg-muted/80 transition-colors',
                'focus:outline-none focus:ring-2 focus:ring-violet-500/30'
              )}
            >
              <Sparkles className="w-4 h-4 text-violet-400" />
              <span>{matchingBundle?.name ?? 'Apply Bundle'}</span>
              <ChevronDown className="w-4 h-4 text-muted-foreground" />
            </button>

            {/* Bundle Dropdown */}
            {showBundleDropdown && (
              <BundleDropdown
                bundles={PERMISSION_BUNDLES}
                selectedSet={selectedSet}
                onApply={handleApplyBundle}
                onClose={() => setShowBundleDropdown(false)}
              />
            )}
          </div>
        )}
      </div>

      {/* Selection Summary */}
      <div className="flex items-center justify-between text-sm">
        <span className="text-muted-foreground">
          {selectedPermissions.length} of {totalPermissions} permissions selected
        </span>
        {!readOnly && (
          <div className="flex items-center gap-2">
            <button
              type="button"
              onClick={() => onChange([])}
              className="text-xs text-muted-foreground hover:text-foreground transition-colors"
              disabled={selectedPermissions.length === 0}
            >
              Clear All
            </button>
            <span className="text-muted-foreground/30">|</span>
            <button
              type="button"
              onClick={() => {
                const allKeys = PERMISSION_SECTIONS.flatMap((s) =>
                  s.permissions.map((p) => p.key)
                );
                onChange(allKeys);
              }}
              className="text-xs text-muted-foreground hover:text-foreground transition-colors"
            >
              Select All
            </button>
          </div>
        )}
      </div>

      {/* Permission Sections */}
      <div className="space-y-2 max-h-[400px] overflow-y-auto pr-1">
        {filteredSections.length === 0 ? (
          <div className="text-center py-8 text-muted-foreground">
            No permissions match "{searchQuery}"
          </div>
        ) : (
          filteredSections.map((section) => (
            <PermissionSectionAccordion
              key={section.id}
              section={section}
              selectedPermissions={selectedSet}
              onTogglePermission={handleTogglePermission}
              onToggleSection={handleToggleSection}
              isExpanded={expandedSections.has(section.id)}
              onToggleExpanded={() => handleToggleExpanded(section.id)}
              readOnly={readOnly}
            />
          ))
        )}
      </div>
    </div>
  );
}

/**
 * Bundle dropdown component
 */
interface BundleDropdownProps {
  bundles: PermissionBundle[];
  selectedSet: Set<string>;
  onApply: (bundle: PermissionBundle) => void;
  onClose: () => void;
}

function BundleDropdown({
  bundles,
  selectedSet,
  onApply,
  onClose,
}: BundleDropdownProps) {
  // Group bundles by category
  const categories = useMemo(() => {
    const grouped = new Map<string, PermissionBundle[]>();
    bundles.forEach((bundle) => {
      const category = bundle.category ?? 'Other';
      if (!grouped.has(category)) {
        grouped.set(category, []);
      }
      grouped.get(category)!.push(bundle);
    });
    return grouped;
  }, [bundles]);

  // Close on outside click
  React.useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      const target = e.target as HTMLElement;
      if (!target.closest('.bundle-dropdown')) {
        onClose();
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [onClose]);

  return (
    <div className="bundle-dropdown absolute right-0 top-12 z-50 w-72 bg-surface border border-border rounded-xl shadow-2xl overflow-hidden">
      <div className="px-3 py-2 border-b border-border bg-muted/50">
        <span className="text-xs font-medium text-muted-foreground uppercase tracking-wider">
          Permission Bundles
        </span>
      </div>
      <div className="max-h-80 overflow-y-auto">
        {Array.from(categories.entries()).map(([category, categoryBundles]) => (
          <div key={category}>
            <div className="px-3 py-1.5 text-xs font-medium text-muted-foreground bg-muted/30">
              {category}
            </div>
            {categoryBundles.map((bundle) => {
              const matchState = doesBundleMatch(bundle, selectedSet);
              const isActive = matchState === 'full';

              return (
                <button
                  key={bundle.id}
                  type="button"
                  onClick={() => onApply(bundle)}
                  className={cn(
                    'w-full flex items-start gap-3 px-3 py-2 text-left transition-colors',
                    'hover:bg-muted/50',
                    isActive && 'bg-violet-500/10'
                  )}
                >
                  <div
                    className={cn(
                      'mt-0.5 w-4 h-4 rounded-full border flex items-center justify-center flex-shrink-0',
                      isActive
                        ? 'bg-violet-600 border-violet-600'
                        : 'border-border'
                    )}
                  >
                    {isActive && (
                      <Check className="w-2.5 h-2.5 text-white" strokeWidth={3} />
                    )}
                  </div>
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center justify-between gap-2">
                      <span className="text-sm font-medium text-foreground">
                        {bundle.name}
                      </span>
                      <span className="text-xs text-muted-foreground">
                        {bundle.permissionCount} perms
                      </span>
                    </div>
                    <p className="text-xs text-muted-foreground mt-0.5 line-clamp-2">
                      {bundle.description}
                    </p>
                  </div>
                </button>
              );
            })}
          </div>
        ))}
      </div>
    </div>
  );
}

export default PermissionEditor;
