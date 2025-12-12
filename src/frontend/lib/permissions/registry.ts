/**
 * Permission Registry
 *
 * Client-side permission registry with icon mappings for the UI.
 * Can be used standalone or synchronized with backend via API.
 */

import {
  LayoutDashboard,
  Leaf,
  Droplets,
  Package,
  ShoppingCart,
  Truck,
  Sprout,
  Scissors,
  FlaskConical,
  FileCheck,
  ClipboardList,
  Users,
  Library,
  BarChart3,
  Settings,
  Play,
  LucideIcon,
} from 'lucide-react';
import type {
  PermissionSection,
  PermissionSectionWithIcon,
  PermissionBundle,
  PermissionRegistryResponse,
} from '@/types/permissions';

// ============================================================================
// ICON MAPPINGS
// ============================================================================

/**
 * Map of section IDs to their Lucide icons
 */
export const SECTION_ICONS: Record<string, LucideIcon> = {
  dashboard: LayoutDashboard,
  cultivation: Leaf,
  irrigation: Droplets,
  inventory: Package,
  sales: ShoppingCart,
  plants: Sprout,
  harvests: Scissors,
  labtests: FlaskConical,
  compliance: FileCheck,
  transfers: Truck,
  tasks: ClipboardList,
  labor: Users,
  library: Library,
  analytics: BarChart3,
  admin: Settings,
  simulator: Play,
};

/**
 * Get the icon for a section ID
 */
export function getSectionIcon(sectionId: string): LucideIcon {
  return SECTION_ICONS[sectionId] ?? Settings;
}

// ============================================================================
// STATIC REGISTRY (Client-Side Fallback)
// ============================================================================

/**
 * Static permission sections for client-side use.
 * This mirrors the backend PermissionSections but can be used offline.
 */
export const PERMISSION_SECTIONS: PermissionSectionWithIcon[] = [
  {
    id: 'dashboard',
    label: 'Dashboard & Overview',
    description: 'Access to dashboards, widgets, and data visualization',
    displayOrder: 1,
    icon: LayoutDashboard,
    permissions: [
      { key: 'dashboard:view', label: 'View Dashboard', description: 'Access main dashboard and overview screens', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'dashboard:customize', label: 'Customize Widgets', description: 'Add, remove, and rearrange dashboard widgets', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'dashboard:export', label: 'Export Dashboard Data', description: 'Export dashboard data to CSV/PDF', requiresTwoPersonApproval: false, requiresReason: false },
    ],
  },
  {
    id: 'cultivation',
    label: 'Cultivation & Environment',
    description: 'Environmental monitoring and control for grow rooms',
    displayOrder: 2,
    icon: Leaf,
    permissions: [
      { key: 'cultivation:view', label: 'View Cultivation Data', description: 'View environmental readings and status', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'cultivation:control', label: 'Control Environment', description: 'Manually override environmental controls', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'cultivation:configure', label: 'Configure Setpoints', description: 'Set target values for temperature, humidity, CO2, etc.', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'cultivation:alerts', label: 'Manage Alerts', description: 'Configure alert thresholds and notifications', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'cultivation:recipes', label: 'Manage Environment Recipes', description: 'Create and edit environment recipe profiles', requiresTwoPersonApproval: false, requiresReason: false },
    ],
  },
  {
    id: 'irrigation',
    label: 'Irrigation & Fertigation',
    description: 'Watering schedules, nutrient delivery, and fertigation management',
    displayOrder: 3,
    icon: Droplets,
    permissions: [
      { key: 'irrigation:view', label: 'View Irrigation Data', description: 'View irrigation schedules and history', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'irrigation:manual-trigger', label: 'Manual Irrigation', description: 'Manually trigger irrigation events', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'irrigation:programs', label: 'Manage Programs', description: 'Create and edit irrigation programs', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'irrigation:schedules', label: 'Manage Schedules', description: 'Configure irrigation schedules and timing', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'irrigation:recipes', label: 'Manage Feed Recipes', description: 'Create and edit nutrient recipes', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'irrigation:tanks', label: 'Manage Mix Tanks', description: 'Configure mix tanks and stock solutions', requiresTwoPersonApproval: false, requiresReason: false },
    ],
  },
  {
    id: 'inventory',
    label: 'Inventory & Packages',
    description: 'Package tracking, inventory management, and movements',
    displayOrder: 4,
    icon: Package,
    permissions: [
      { key: 'inventory:view', label: 'View Inventory', description: 'View packages, lots, and inventory levels', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'inventory:create', label: 'Create Packages', description: 'Create new packages and lots', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'inventory:adjust', label: 'Adjust Inventory', description: 'Make inventory adjustments and corrections', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'inventory:transfer', label: 'Transfer Inventory', description: 'Move inventory between locations', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'inventory:destroy', label: 'Destroy Inventory', description: 'Destroy packages (compliance tracked)', requiresTwoPersonApproval: true, requiresReason: true },
      { key: 'inventory:holds', label: 'Manage Holds', description: 'Place and release inventory holds', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'inventory:labels', label: 'Print Labels', description: 'Generate and print package labels', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'inventory:ship', label: 'Ship Inventory', description: 'Confirm shipments and decrement inventory', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'inventory:receive', label: 'Receive Inventory', description: 'Receive inbound transfers and inventory', requiresTwoPersonApproval: false, requiresReason: false },
    ],
  },
  {
    id: 'sales',
    label: 'Sales & Fulfillment',
    description: 'Sales orders, allocation, picking, packing, and shipping',
    displayOrder: 4,
    icon: ShoppingCart,
    permissions: [
      { key: 'sales:orders:view', label: 'View Sales Orders', description: 'View sales orders and related documents', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'sales:orders:create', label: 'Create Sales Orders', description: 'Create and edit sales orders', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'sales:orders:submit', label: 'Submit Sales Orders', description: 'Submit sales orders for fulfillment', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'sales:orders:cancel', label: 'Cancel Sales Orders', description: 'Cancel sales orders', requiresTwoPersonApproval: false, requiresReason: true },
      { key: 'sales:allocate', label: 'Allocate Inventory', description: 'Allocate/reserve packages to sales orders', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'sales:shipments:create', label: 'Create Shipments', description: 'Create pick/pack shipments from allocations', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'sales:shipments:pack', label: 'Pack Shipments', description: 'Confirm packing of shipments', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'sales:shipments:ship', label: 'Ship Shipments', description: 'Confirm shipment shipment and generate inventory movements', requiresTwoPersonApproval: false, requiresReason: false },
    ],
  },
  {
    id: 'plants',
    label: 'Plants & Batches',
    description: 'Plant lifecycle management from clone to harvest',
    displayOrder: 5,
    icon: Sprout,
    permissions: [
      { key: 'plants:view', label: 'View Plants', description: 'View plant records and batch information', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'plants:create', label: 'Create Plants', description: 'Create new plant records and batches', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'plants:move', label: 'Move Plants', description: 'Move plants between rooms and locations', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'plants:phase', label: 'Change Growth Phase', description: 'Transition plants between growth phases', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'plants:destroy', label: 'Destroy Plants', description: 'Record plant destruction (compliance tracked)', requiresTwoPersonApproval: true, requiresReason: true },
    ],
  },
  {
    id: 'harvests',
    label: 'Harvests & Processing',
    description: 'Harvest workflows, weighing, and processing',
    displayOrder: 6,
    icon: Scissors,
    permissions: [
      { key: 'harvests:view', label: 'View Harvests', description: 'View harvest records and history', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'harvests:create', label: 'Create Harvests', description: 'Initiate new harvest batches', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'harvests:weigh', label: 'Record Weights', description: 'Record wet and dry weights', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'harvests:waste', label: 'Record Waste', description: 'Record harvest waste and byproducts', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'harvests:finalize', label: 'Finalize Harvests', description: 'Complete and lock harvest records', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'harvests:processing', label: 'Manage Processing', description: 'Create and manage processing jobs', requiresTwoPersonApproval: false, requiresReason: false },
    ],
  },
  {
    id: 'labtests',
    label: 'Lab Testing & QA',
    description: 'Laboratory testing, quality assurance, and certifications',
    displayOrder: 7,
    icon: FlaskConical,
    permissions: [
      { key: 'labtests:view', label: 'View Lab Results', description: 'View test results and certificates', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'labtests:submit', label: 'Submit for Testing', description: 'Submit samples to labs for testing', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'labtests:review', label: 'Review Results', description: 'Review and approve test results', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'labtests:certify', label: 'Certify Batches', description: 'Mark batches as lab-certified for sale', requiresTwoPersonApproval: false, requiresReason: false },
    ],
  },
  {
    id: 'compliance',
    label: 'Compliance & Reporting',
    description: 'Regulatory compliance, METRC integration, and audit trails',
    displayOrder: 8,
    icon: FileCheck,
    permissions: [
      { key: 'compliance:view', label: 'View Compliance Data', description: 'View compliance status and reports', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'compliance:reports', label: 'Generate Reports', description: 'Create compliance and regulatory reports', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'compliance:metrc-sync', label: 'Sync with METRC', description: 'Manually trigger METRC synchronization', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'compliance:metrc-submit', label: 'Submit to METRC', description: 'Submit data to METRC/state system', requiresTwoPersonApproval: false, requiresReason: true },
      { key: 'compliance:audit', label: 'Access Audit Logs', description: 'View detailed audit trail and history', requiresTwoPersonApproval: false, requiresReason: false },
    ],
  },
  {
    id: 'transfers',
    label: 'Transfers & Manifests',
    description: 'Outbound transfers, transport manifests, and inbound receipts',
    displayOrder: 8,
    icon: Truck,
    permissions: [
      { key: 'transfers:view', label: 'View Transfers', description: 'View outbound/inbound transfer records', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'transfers:create', label: 'Create Transfers', description: 'Create outbound transfers and manifests', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'transfers:submit', label: 'Submit Transfers', description: 'Submit transfer templates to METRC', requiresTwoPersonApproval: false, requiresReason: true },
      { key: 'transfers:void', label: 'Void Transfers', description: 'Void/cancel transfers', requiresTwoPersonApproval: false, requiresReason: true },
      { key: 'transfers:receive', label: 'Receive Transfers', description: 'Accept/reject inbound transfers and record receipts', requiresTwoPersonApproval: false, requiresReason: true },
    ],
  },
  {
    id: 'tasks',
    label: 'Tasks & SOPs',
    description: 'Task management, workflows, and standard operating procedures',
    displayOrder: 9,
    icon: ClipboardList,
    permissions: [
      { key: 'tasks:view', label: 'View Tasks', description: 'View assigned and available tasks', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'tasks:create', label: 'Create Tasks', description: 'Create new tasks and work orders', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'tasks:assign', label: 'Assign Tasks', description: 'Assign tasks to team members', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'tasks:complete', label: 'Complete Tasks', description: 'Mark tasks as completed', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'tasks:templates', label: 'Manage Templates', description: 'Create and edit task templates', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'tasks:sops', label: 'Manage SOPs', description: 'Create and edit standard operating procedures', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'tasks:blueprints', label: 'Manage Blueprints', description: 'Create and edit task blueprints', requiresTwoPersonApproval: false, requiresReason: false },
    ],
  },
  {
    id: 'labor',
    label: 'Labor & Scheduling',
    description: 'Employee scheduling, time tracking, and team management',
    displayOrder: 10,
    icon: Users,
    permissions: [
      { key: 'labor:view', label: 'View Schedules', description: 'View shift schedules and assignments', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'labor:schedule', label: 'Manage Schedules', description: 'Create and edit shift schedules', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'labor:time-entry', label: 'Enter Time', description: 'Submit time entries and clock in/out', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'labor:approve-time', label: 'Approve Time', description: 'Approve or reject time entries', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'labor:teams', label: 'Manage Teams', description: 'Create and manage team assignments', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'labor:productivity', label: 'View Productivity', description: 'Access productivity metrics and reports', requiresTwoPersonApproval: false, requiresReason: false },
    ],
  },
  {
    id: 'library',
    label: 'Library & Genetics',
    description: 'Strain library, genetics, and recipe management',
    displayOrder: 11,
    icon: Library,
    permissions: [
      { key: 'library:view', label: 'View Library', description: 'View strains, genetics, and recipes', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'library:genetics', label: 'Manage Genetics', description: 'Create and edit genetic profiles', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'library:environment-recipes', label: 'Manage Environment Recipes', description: 'Create environment recipes', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'library:fertigation-recipes', label: 'Manage Fertigation Recipes', description: 'Create fertigation recipes', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'library:lighting-recipes', label: 'Manage Lighting Recipes', description: 'Create lighting schedules', requiresTwoPersonApproval: false, requiresReason: false },
    ],
  },
  {
    id: 'analytics',
    label: 'Analytics & Reports',
    description: 'Business intelligence, custom reports, and data analysis',
    displayOrder: 12,
    icon: BarChart3,
    permissions: [
      { key: 'analytics:view', label: 'View Analytics', description: 'Access analytics dashboards', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'analytics:reports', label: 'Create Reports', description: 'Build custom reports', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'analytics:share', label: 'Share Reports', description: 'Share reports with other users', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'analytics:export', label: 'Export Data', description: 'Export analytics data', requiresTwoPersonApproval: false, requiresReason: false },
    ],
  },
  {
    id: 'admin',
    label: 'Administration',
    description: 'System administration, user management, and configuration',
    displayOrder: 13,
    icon: Settings,
    permissions: [
      { key: 'admin:users', label: 'Manage Users', description: 'Create, edit, and deactivate users', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'admin:roles', label: 'Manage Roles', description: 'Create and edit role definitions', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'admin:sites', label: 'Manage Sites', description: 'Configure site settings and access', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'admin:equipment', label: 'Manage Equipment', description: 'Configure sensors and equipment', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'admin:spatial', label: 'Manage Spatial', description: 'Configure rooms, zones, and locations', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'admin:settings', label: 'System Settings', description: 'Modify system-wide settings', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'admin:integrations', label: 'Manage Integrations', description: 'Configure third-party integrations', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'admin:feature-flags', label: 'Manage Feature Flags', description: 'Enable/disable feature flags', requiresTwoPersonApproval: false, requiresReason: false },
    ],
  },
  {
    id: 'simulator',
    label: 'Simulator & Dev Tools',
    description: 'Development tools and simulation environment',
    displayOrder: 14,
    icon: Play,
    permissions: [
      { key: 'simulator:access', label: 'Access Simulator', description: 'Access the simulation environment', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'simulator:configure', label: 'Configure Simulator', description: 'Modify simulator settings and streams', requiresTwoPersonApproval: false, requiresReason: false },
      { key: 'simulator:provision', label: 'Provision Sites', description: 'Create simulated sites and data', requiresTwoPersonApproval: false, requiresReason: false },
    ],
  },
];

/**
 * Static permission bundles for client-side use
 */
export const PERMISSION_BUNDLES: PermissionBundle[] = [
  {
    id: 'viewer',
    name: 'Viewer',
    description: 'Read-only access to dashboards and basic data',
    category: 'Basic',
    displayOrder: 1,
    permissionCount: 6,
    permissions: ['dashboard:view', 'cultivation:view', 'irrigation:view', 'inventory:view', 'plants:view', 'tasks:view'],
  },
  {
    id: 'cultivation_operator',
    name: 'Cultivation Operator',
    description: 'Day-to-day cultivation and grow room operations',
    category: 'Cultivation',
    displayOrder: 10,
    permissionCount: 12,
    permissions: [
      'dashboard:view', 'cultivation:view', 'cultivation:control', 'irrigation:view', 'irrigation:manual-trigger',
      'plants:view', 'plants:move', 'plants:phase', 'tasks:view', 'tasks:complete', 'labor:view', 'labor:time-entry',
    ],
  },
  {
    id: 'harvest_operator',
    name: 'Harvest Operator',
    description: 'Harvest workflow and processing operations',
    category: 'Cultivation',
    displayOrder: 11,
    permissionCount: 12,
    permissions: [
      'dashboard:view', 'plants:view', 'harvests:view', 'harvests:create', 'harvests:weigh', 'harvests:waste',
      'inventory:view', 'inventory:create', 'tasks:view', 'tasks:complete', 'labor:view', 'labor:time-entry',
    ],
  },
  {
    id: 'inventory_operator',
    name: 'Inventory Operator',
    description: 'Inventory management and package handling',
    category: 'Inventory',
    displayOrder: 12,
    permissionCount: 12,
    permissions: [
      'dashboard:view', 'inventory:view', 'inventory:create', 'inventory:adjust', 'inventory:transfer',
      'inventory:ship', 'inventory:receive', 'inventory:labels', 'tasks:view', 'tasks:complete', 'labor:view', 'labor:time-entry',
    ],
  },
  {
    id: 'fulfillment_operator',
    name: 'Fulfillment Operator',
    description: 'Sales order fulfillment: allocate, pack, and ship',
    category: 'Sales',
    displayOrder: 13,
    permissionCount: 12,
    permissions: [
      'dashboard:view',
      'inventory:view', 'inventory:ship',
      'sales:orders:view', 'sales:allocate', 'sales:shipments:create', 'sales:shipments:pack', 'sales:shipments:ship',
      'transfers:view', 'transfers:create',
      'compliance:metrc-submit',
      'tasks:view', 'tasks:complete',
    ],
  },
  {
    id: 'cultivation_manager',
    name: 'Cultivation Manager',
    description: 'Full cultivation management including recipes and configuration',
    category: 'Management',
    displayOrder: 20,
    permissionCount: 40,
    permissions: [
      'dashboard:view', 'dashboard:customize', 'dashboard:export',
      'cultivation:view', 'cultivation:control', 'cultivation:configure', 'cultivation:alerts', 'cultivation:recipes',
      'irrigation:view', 'irrigation:manual-trigger', 'irrigation:programs', 'irrigation:schedules', 'irrigation:recipes', 'irrigation:tanks',
      'plants:view', 'plants:create', 'plants:move', 'plants:phase',
      'harvests:view', 'harvests:create', 'harvests:weigh', 'harvests:waste', 'harvests:finalize', 'harvests:processing',
      'labtests:view', 'labtests:submit',
      'tasks:view', 'tasks:create', 'tasks:assign', 'tasks:complete', 'tasks:templates',
      'labor:view', 'labor:schedule', 'labor:time-entry', 'labor:approve-time', 'labor:teams', 'labor:productivity',
      'library:view', 'library:environment-recipes', 'library:fertigation-recipes', 'library:lighting-recipes',
      'analytics:view', 'analytics:reports',
    ],
  },
  {
    id: 'compliance_officer',
    name: 'Compliance Officer',
    description: 'Full compliance management and regulatory reporting',
    category: 'Compliance',
    displayOrder: 30,
    permissionCount: 17,
    permissions: [
      'dashboard:view', 'dashboard:export',
      'inventory:view', 'inventory:holds',
      'plants:view', 'harvests:view',
      'labtests:view', 'labtests:review', 'labtests:certify',
      'compliance:view', 'compliance:reports', 'compliance:metrc-sync', 'compliance:metrc-submit', 'compliance:audit',
      'analytics:view', 'analytics:reports', 'analytics:export',
    ],
  },
  {
    id: 'technician',
    name: 'Technician',
    description: 'Equipment maintenance and sensor configuration',
    category: 'Technical',
    displayOrder: 31,
    permissionCount: 10,
    permissions: [
      'dashboard:view',
      'cultivation:view', 'cultivation:control', 'cultivation:alerts',
      'irrigation:view', 'irrigation:manual-trigger',
      'tasks:view', 'tasks:complete',
      'admin:equipment', 'admin:spatial',
      'labor:view', 'labor:time-entry',
    ],
  },
];

// ============================================================================
// UTILITY FUNCTIONS
// ============================================================================

/**
 * Enhance API response sections with icons
 */
export function enhanceSectionsWithIcons(
  sections: PermissionSection[]
): PermissionSectionWithIcon[] {
  return sections.map((section) => ({
    ...section,
    icon: getSectionIcon(section.id),
  }));
}

/**
 * Get all permission keys from all sections
 */
export function getAllPermissionKeys(): string[] {
  return PERMISSION_SECTIONS.flatMap((s) => s.permissions.map((p) => p.key));
}

/**
 * Find which section contains a permission
 */
export function findSectionForPermission(
  permissionKey: string
): PermissionSectionWithIcon | undefined {
  return PERMISSION_SECTIONS.find((s) =>
    s.permissions.some((p) => p.key === permissionKey)
  );
}

/**
 * Check if a bundle matches the currently selected permissions
 */
export function doesBundleMatch(
  bundle: PermissionBundle,
  selectedPermissions: Set<string>
): 'full' | 'partial' | 'none' {
  const matchingCount = bundle.permissions.filter((p) =>
    selectedPermissions.has(p)
  ).length;

  if (matchingCount === 0) return 'none';
  if (matchingCount === bundle.permissions.length) return 'full';
  return 'partial';
}
