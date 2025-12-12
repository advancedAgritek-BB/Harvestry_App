/**
 * Permission System Types
 *
 * Types for the granular, section-based permission system.
 * Mirrors backend DTOs from PermissionRegistryDto.cs
 */

import { LucideIcon } from 'lucide-react';

// ============================================================================
// CORE PERMISSION TYPES
// ============================================================================

/**
 * A single permission definition
 */
export interface Permission {
  /** Unique permission key in resource:action format (e.g., "cultivation:view") */
  key: string;
  /** Human-readable label for display */
  label: string;
  /** Optional description of what this permission grants */
  description?: string;
  /** Whether actions using this permission require two-person approval */
  requiresTwoPersonApproval: boolean;
  /** Whether actions using this permission require a reason to be provided */
  requiresReason: boolean;
}

/**
 * A logical grouping of permissions within a functional area
 */
export interface PermissionSection {
  /** Unique identifier for this section (e.g., "cultivation") */
  id: string;
  /** Human-readable label for display (e.g., "Cultivation & Environment") */
  label: string;
  /** Optional description of what this section covers */
  description?: string;
  /** Order in which to display this section */
  displayOrder: number;
  /** All permissions within this section */
  permissions: Permission[];
}

/**
 * Extended section with UI metadata (icon)
 */
export interface PermissionSectionWithIcon extends PermissionSection {
  /** Lucide icon component for this section */
  icon: LucideIcon;
}

/**
 * A preset bundle of permissions for quick role assignment
 */
export interface PermissionBundle {
  /** Unique identifier for this bundle (e.g., "cultivation_operator") */
  id: string;
  /** Human-readable name for display (e.g., "Cultivation Operator") */
  name: string;
  /** Description of what this bundle provides */
  description: string;
  /** Optional category for grouping bundles (e.g., "Cultivation", "Admin") */
  category?: string;
  /** Order in which to display this bundle */
  displayOrder: number;
  /** Count of permissions in this bundle */
  permissionCount: number;
  /** All permission keys included in this bundle */
  permissions: string[];
}

// ============================================================================
// API RESPONSE TYPES
// ============================================================================

/**
 * Complete permission registry response from GET /api/v1/permissions/registry
 */
export interface PermissionRegistryResponse {
  /** All permission sections in display order */
  sections: PermissionSection[];
  /** All permission bundles in display order */
  bundles: PermissionBundle[];
  /** All unique bundle categories */
  bundleCategories: string[];
}

// ============================================================================
// UI STATE TYPES
// ============================================================================

/**
 * Selection state for a permission section
 */
export type SectionSelectionState = 'none' | 'partial' | 'all';

/**
 * State for a section in the permission editor
 */
export interface PermissionSectionState {
  /** Section ID */
  sectionId: string;
  /** Whether this section is expanded in the UI */
  isExpanded: boolean;
  /** Selection state based on selected permissions */
  selectionState: SectionSelectionState;
  /** Count of selected permissions in this section */
  selectedCount: number;
  /** Total permissions in this section */
  totalCount: number;
}

/**
 * Props for permission editor component
 */
export interface PermissionEditorProps {
  /** Currently selected permission keys */
  selectedPermissions: string[];
  /** Callback when permissions change */
  onChange: (permissions: string[]) => void;
  /** Whether the editor is read-only */
  readOnly?: boolean;
  /** Whether to show bundle quick-select */
  showBundles?: boolean;
  /** Optional class name for styling */
  className?: string;
}

/**
 * Props for permission section accordion
 */
export interface PermissionSectionAccordionProps {
  /** The section to render */
  section: PermissionSectionWithIcon;
  /** Currently selected permission keys */
  selectedPermissions: Set<string>;
  /** Callback when a permission is toggled */
  onTogglePermission: (permissionKey: string) => void;
  /** Callback when all section permissions are toggled */
  onToggleSection: (sectionId: string, selectAll: boolean) => void;
  /** Whether the section is expanded */
  isExpanded: boolean;
  /** Callback when expansion state changes */
  onToggleExpanded: () => void;
  /** Whether the editor is read-only */
  readOnly?: boolean;
}

/**
 * Props for bundle quick-select component
 */
export interface BundleQuickSelectProps {
  /** All available bundles */
  bundles: PermissionBundle[];
  /** Currently selected permission keys */
  selectedPermissions: Set<string>;
  /** Callback when a bundle is applied */
  onApplyBundle: (bundleId: string) => void;
  /** Optional class name for styling */
  className?: string;
}

// ============================================================================
// UTILITY TYPES
// ============================================================================

/**
 * Map of section ID to its permissions
 */
export type SectionPermissionMap = Record<string, string[]>;

/**
 * Map of permission key to its section ID
 */
export type PermissionSectionMap = Record<string, string>;

// ============================================================================
// UTILITY FUNCTIONS (Type Guards)
// ============================================================================

/**
 * Check if a permission requires elevated handling
 */
export function isElevatedPermission(permission: Permission): boolean {
  return permission.requiresTwoPersonApproval || permission.requiresReason;
}

/**
 * Calculate selection state for a section
 */
export function calculateSectionState(
  section: PermissionSection,
  selectedPermissions: Set<string>
): SectionSelectionState {
  const sectionKeys = section.permissions.map((p) => p.key);
  const selectedInSection = sectionKeys.filter((k) => selectedPermissions.has(k));

  if (selectedInSection.length === 0) return 'none';
  if (selectedInSection.length === sectionKeys.length) return 'all';
  return 'partial';
}

/**
 * Get all permission keys from a section
 */
export function getSectionPermissionKeys(section: PermissionSection): string[] {
  return section.permissions.map((p) => p.key);
}
