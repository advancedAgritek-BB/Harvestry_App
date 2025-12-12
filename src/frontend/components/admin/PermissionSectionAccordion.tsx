'use client';

import React from 'react';
import { ChevronDown, Check, Minus, AlertTriangle } from 'lucide-react';
import { cn } from '@/lib/utils';
import type {
  PermissionSectionWithIcon,
  PermissionSectionAccordionProps,
  SectionSelectionState,
} from '@/types/permissions';
import { calculateSectionState, isElevatedPermission } from '@/types/permissions';

/**
 * PermissionSectionAccordion
 *
 * A collapsible section for managing permissions within a category.
 * Features:
 * - Expandable/collapsible section with icon
 * - "Select All" checkbox with indeterminate state
 * - Individual permission checkboxes
 * - Visual indicators for elevated permissions (two-person approval, requires reason)
 */
export function PermissionSectionAccordion({
  section,
  selectedPermissions,
  onTogglePermission,
  onToggleSection,
  isExpanded,
  onToggleExpanded,
  readOnly = false,
}: PermissionSectionAccordionProps) {
  const Icon = section.icon;
  const selectionState = calculateSectionState(section, selectedPermissions);
  const selectedCount = section.permissions.filter((p) =>
    selectedPermissions.has(p.key)
  ).length;
  const totalCount = section.permissions.length;

  const handleSectionToggle = () => {
    if (readOnly) return;
    // If all selected, deselect all; otherwise select all
    onToggleSection(section.id, selectionState !== 'all');
  };

  return (
    <div className="border border-border rounded-lg overflow-hidden bg-surface/50">
      {/* Section Header */}
      <div
        className={cn(
          'flex items-center gap-3 px-4 py-3 cursor-pointer transition-colors',
          'hover:bg-muted/50',
          isExpanded && 'bg-muted/30'
        )}
        onClick={onToggleExpanded}
      >
        {/* Section Checkbox */}
        <div
          onClick={(e) => {
            e.stopPropagation();
            handleSectionToggle();
          }}
          className="flex-shrink-0"
        >
          <SectionCheckbox
            state={selectionState}
            disabled={readOnly}
          />
        </div>

        {/* Section Icon */}
        <div className="flex-shrink-0 w-8 h-8 rounded-lg bg-violet-500/10 flex items-center justify-center">
          <Icon className="w-4 h-4 text-violet-400" />
        </div>

        {/* Section Label & Count */}
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2">
            <span className="font-medium text-foreground truncate">
              {section.label}
            </span>
            <span className="text-xs text-muted-foreground px-2 py-0.5 bg-muted rounded-full">
              {selectedCount}/{totalCount}
            </span>
          </div>
          {section.description && (
            <p className="text-xs text-muted-foreground truncate mt-0.5">
              {section.description}
            </p>
          )}
        </div>

        {/* Expand/Collapse Icon */}
        <ChevronDown
          className={cn(
            'w-5 h-5 text-muted-foreground transition-transform duration-200',
            isExpanded && 'rotate-180'
          )}
        />
      </div>

      {/* Expanded Permissions List */}
      {isExpanded && (
        <div className="border-t border-border bg-muted/20">
          <div className="px-4 py-3 space-y-1">
            {section.permissions.map((permission) => {
              const isSelected = selectedPermissions.has(permission.key);
              const isElevated = isElevatedPermission(permission);

              return (
                <div
                  key={permission.key}
                  className={cn(
                    'flex items-start gap-3 p-2 rounded-lg transition-colors',
                    'hover:bg-muted/50',
                    isSelected && 'bg-violet-500/5'
                  )}
                >
                  {/* Permission Checkbox */}
                  <button
                    type="button"
                    aria-label={`${isSelected ? 'Remove' : 'Add'} ${permission.label} permission`}
                    disabled={readOnly}
                    onClick={() => onTogglePermission(permission.key)}
                    className={cn(
                      'mt-0.5 flex-shrink-0 w-5 h-5 rounded border flex items-center justify-center',
                      'transition-all duration-200',
                      'focus:outline-none focus:ring-2 focus:ring-violet-500/30',
                      isSelected
                        ? 'bg-violet-600 border-violet-600'
                        : 'bg-transparent border-border hover:border-muted-foreground',
                      readOnly && 'opacity-50 cursor-not-allowed'
                    )}
                  >
                    {isSelected && (
                      <Check className="w-3 h-3 text-white" strokeWidth={3} />
                    )}
                  </button>

                  {/* Permission Info */}
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2">
                      <span
                        className={cn(
                          'text-sm',
                          isSelected ? 'text-foreground' : 'text-muted-foreground'
                        )}
                      >
                        {permission.label}
                      </span>
                      {isElevated && (
                        <span className="flex items-center gap-1 text-xs text-amber-400 bg-amber-500/10 px-1.5 py-0.5 rounded">
                          <AlertTriangle className="w-3 h-3" />
                          {permission.requiresTwoPersonApproval
                            ? '2-Person'
                            : 'Reason Required'}
                        </span>
                      )}
                    </div>
                    {permission.description && (
                      <p className="text-xs text-muted-foreground mt-0.5">
                        {permission.description}
                      </p>
                    )}
                    <span className="text-xs text-muted-foreground/50 font-mono">
                      {permission.key}
                    </span>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
}

/**
 * Section checkbox with three states: none, partial (indeterminate), all
 */
interface SectionCheckboxProps {
  state: SectionSelectionState;
  disabled?: boolean;
}

function SectionCheckbox({ state, disabled }: SectionCheckboxProps) {
  return (
    <div
      className={cn(
        'w-5 h-5 rounded border flex items-center justify-center transition-all duration-200',
        state === 'all'
          ? 'bg-violet-600 border-violet-600'
          : state === 'partial'
          ? 'bg-violet-600/50 border-violet-600'
          : 'bg-transparent border-border hover:border-muted-foreground',
        disabled && 'opacity-50 cursor-not-allowed',
        !disabled && 'cursor-pointer'
      )}
    >
      {state === 'all' && (
        <Check className="w-3 h-3 text-white" strokeWidth={3} />
      )}
      {state === 'partial' && (
        <Minus className="w-3 h-3 text-white" strokeWidth={3} />
      )}
    </div>
  );
}

export default PermissionSectionAccordion;
