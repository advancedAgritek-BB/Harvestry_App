/**
 * Application Task Types
 * Defines types for tasks that involve consumable applications (fertilizer, IPM, etc.)
 */

/**
 * Types of applications that require specific permissions
 */
export type ApplicationType =
  | 'fertigation'       // Nutrient feeding
  | 'ipm'               // Integrated Pest Management (pesticides, fungicides, etc.)
  | 'foliar'            // Foliar sprays (nutrients, supplements)
  | 'growth_regulator'  // Plant growth regulators (PGRs)
  | 'beneficial'        // Beneficial insects/microbes
  | 'flush'             // Plain water flush (resets medium)
  | 'co2'               // CO2 supplementation
  | 'sulfur'            // Sulfur burn/treatment
  | 'other';            // Custom application type

/**
 * Application-specific permissions
 * Format: applications:{application_type}
 */
export const APPLICATION_PERMISSIONS: Record<ApplicationType, string> = {
  fertigation: 'applications:fertigation',
  ipm: 'applications:ipm',
  foliar: 'applications:foliar',
  growth_regulator: 'applications:growth_regulator',
  beneficial: 'applications:beneficial',
  flush: 'applications:flush',
  co2: 'applications:co2',
  sulfur: 'applications:sulfur',
  other: 'applications:other',
};

/**
 * Display configuration for application types
 */
export const APPLICATION_TYPE_CONFIG: Record<ApplicationType, {
  label: string;
  description: string;
  color: string;
  icon: string;
  requiresRecipe: boolean;
  requiresInventory: boolean;
}> = {
  fertigation: {
    label: 'Fertigation',
    description: 'Nutrient feeding through irrigation',
    color: 'emerald',
    icon: 'Droplets',
    requiresRecipe: true,
    requiresInventory: true,
  },
  ipm: {
    label: 'IPM Application',
    description: 'Integrated Pest Management - pesticides, fungicides, etc.',
    color: 'amber',
    icon: 'Bug',
    requiresRecipe: true,
    requiresInventory: true,
  },
  foliar: {
    label: 'Foliar Spray',
    description: 'Foliar-applied nutrients or supplements',
    color: 'cyan',
    icon: 'Spray',
    requiresRecipe: true,
    requiresInventory: true,
  },
  growth_regulator: {
    label: 'Growth Regulator',
    description: 'Plant growth regulators (PGRs)',
    color: 'purple',
    icon: 'TrendingUp',
    requiresRecipe: true,
    requiresInventory: true,
  },
  beneficial: {
    label: 'Beneficial Release',
    description: 'Beneficial insects or microbes',
    color: 'lime',
    icon: 'Bug',
    requiresRecipe: false,
    requiresInventory: true,
  },
  flush: {
    label: 'Flush',
    description: 'Plain water flush to reset growing medium',
    color: 'sky',
    icon: 'Droplet',
    requiresRecipe: false,
    requiresInventory: false,
  },
  co2: {
    label: 'CO2 Application',
    description: 'CO2 supplementation',
    color: 'slate',
    icon: 'Wind',
    requiresRecipe: false,
    requiresInventory: true,
  },
  sulfur: {
    label: 'Sulfur Treatment',
    description: 'Sulfur burn or treatment',
    color: 'yellow',
    icon: 'Flame',
    requiresRecipe: false,
    requiresInventory: true,
  },
  other: {
    label: 'Other Application',
    description: 'Custom application type',
    color: 'gray',
    icon: 'Package',
    requiresRecipe: false,
    requiresInventory: false,
  },
};

/**
 * Recipe/formulation attached to an application task
 */
export interface ApplicationRecipe {
  id: string;
  name: string;
  version: number;
  description?: string;
  applicationType: ApplicationType;
  
  // Target parameters
  targetEcMscm?: number;     // For fertigation
  targetEcTolerance?: number;
  targetPh?: number;
  targetPhTolerance?: number;
  
  // Application method
  applicationMethod: 'drip' | 'hand_water' | 'foliar' | 'drench' | 'other';
  dilutionRatio?: string;    // e.g., "1:100", "5ml/L"
  applicationRate?: string;  // e.g., "100ml/plant", "1L/m²"
  
  // Components/ingredients
  components: ApplicationRecipeComponent[];
  
  // Safety & compliance
  reEntryIntervalHours?: number;  // REI for IPM
  preHarvestIntervalDays?: number; // PHI for IPM
  ppeRequired?: string[];          // Personal protective equipment
  
  // Metadata
  createdAt: string;
  createdBy: string;
  isActive: boolean;
}

/**
 * A single component/ingredient in a recipe
 */
export interface ApplicationRecipeComponent {
  id: string;
  recipeId: string;
  order: number;
  
  // Product reference
  productId: string;
  productSku?: string;
  productName: string;
  
  // Quantity per application unit
  amountPerUnit: number;
  amountUom: string;        // ml, g, units, etc.
  perUnitType: 'liter' | 'gallon' | 'plant' | 'sqft' | 'sqm' | 'zone' | 'room';
  
  // Cost tracking
  unitCost?: number;
  
  // Notes
  notes?: string;
}

/**
 * Spatial target for an application task
 * Applications must be traceable to specific locations/plants
 */
export interface ApplicationTarget {
  // Site is always implied from task
  roomId: string;
  roomName: string;
  
  // Can target entire room, specific zones, or individual plants
  targetType: 'room' | 'zones' | 'plants';
  
  // Zone targeting (when targetType is 'zones')
  zoneIds?: string[];
  zoneNames?: string[];
  
  // Plant targeting (when targetType is 'plants')
  plantIds?: string[];
  plantTags?: string[];  // RFID/barcode tags for compliance
  
  // Calculated quantities
  plantCount: number;
  estimatedAreaSqFt?: number;
  estimatedVolumeLiters?: number;
}

/**
 * Planned material for an application task (before execution)
 */
export interface PlannedApplicationMaterial {
  id: string;
  applicationTaskId: string;
  
  // From recipe component
  recipeComponentId: string;
  productId: string;
  productName: string;
  productSku?: string;
  
  // Planned quantities
  plannedQuantity: number;
  uom: string;
  
  // Lot allocation (optional pre-allocation)
  allocatedLotId?: string;
  allocatedLotNumber?: string;
  allocatedQuantity?: number;
  
  // Cost estimate
  estimatedUnitCost?: number;
  estimatedTotalCost?: number;
}

/**
 * Actual material consumption record (after execution)
 */
export interface ApplicationMaterialConsumption {
  id: string;
  applicationTaskId: string;
  plannedMaterialId: string;
  
  // Product info
  productId: string;
  productName: string;
  
  // Lot consumed from
  lotId: string;
  lotNumber: string;
  
  // Quantities
  consumedQuantity: number;
  uom: string;
  
  // Cost at time of consumption
  unitCost: number;
  totalCost: number;
  
  // Traceability
  consumedAt: string;
  consumedByUserId: string;
  consumedByUserName: string;
  
  // Inventory movement reference
  inventoryMovementId?: string;
}

/**
 * Extended Task properties for application tasks
 */
export interface ApplicationTaskDetails {
  // Application type
  applicationType: ApplicationType;
  customApplicationType?: string; // When type is 'other'
  
  // Recipe reference
  recipeId?: string;
  recipeVersionId?: string;
  recipe?: ApplicationRecipe;
  
  // Spatial target
  target: ApplicationTarget;
  
  // Materials
  plannedMaterials: PlannedApplicationMaterial[];
  materialConsumptions: ApplicationMaterialConsumption[];
  
  // Application parameters (may override recipe defaults)
  actualEcMscm?: number;
  actualPh?: number;
  actualTemperatureF?: number;
  applicationVolumeLiters?: number;
  
  // Timing
  applicationStartedAt?: string;
  applicationCompletedAt?: string;
  applicationDurationMinutes?: number;
  
  // Equipment used
  equipmentUsed?: string[];
  
  // Compliance
  ppeUsed?: string[];
  reEntryExpiresAt?: string;  // REI expiration
  preHarvestExpiresAt?: string; // PHI expiration
  
  // Weather/environmental conditions (for outdoor or relevant to application)
  weatherConditions?: {
    temperatureF?: number;
    humidity?: number;
    windSpeedMph?: number;
    conditions?: string;
  };
  
  // Notes and observations
  applicationNotes?: string;
  observations?: string;
  issuesEncountered?: string;
  
  // Cost summary
  totalMaterialCost?: number;
  totalLaborCost?: number;
  totalCost?: number;
}

/**
 * Request to create an application task
 */
export interface CreateApplicationTaskRequest {
  // Base task fields
  title: string;
  description?: string;
  priority: 'low' | 'normal' | 'high' | 'critical';
  assignedToUserId?: string;
  assignedToRole?: string;
  dueDate?: string;
  
  // Application specific
  applicationType: ApplicationType;
  customApplicationType?: string;
  recipeVersionId?: string;
  
  // Target
  roomId: string;
  targetType: 'room' | 'zones' | 'plants';
  zoneIds?: string[];
  plantIds?: string[];
  
  // SOP attachment
  requiredSopIds?: string[];
  
  // Notes
  notes?: string;
}

/**
 * Request to complete an application task
 */
export interface CompleteApplicationTaskRequest {
  taskId: string;
  
  // Material consumption records
  materialConsumptions: {
    plannedMaterialId: string;
    lotId: string;
    consumedQuantity: number;
  }[];
  
  // Actual application parameters
  actualEcMscm?: number;
  actualPh?: number;
  actualTemperatureF?: number;
  applicationVolumeLiters?: number;
  applicationDurationMinutes?: number;
  
  // Equipment and PPE
  equipmentUsed?: string[];
  ppeUsed?: string[];
  
  // Weather (if applicable)
  weatherConditions?: {
    temperatureF?: number;
    humidity?: number;
    windSpeedMph?: number;
    conditions?: string;
  };
  
  // Notes
  applicationNotes?: string;
  observations?: string;
  issuesEncountered?: string;
}

/**
 * User application permissions summary
 */
export interface UserApplicationPermissions {
  userId: string;
  permissions: {
    applicationType: ApplicationType;
    canPerform: boolean;
    canApprove: boolean;
  }[];
  
  // Convenience flags
  canPerformAny: boolean;
  canPerformAll: boolean;
}

/**
 * Validate if a user can be assigned an application task
 */
export function canUserPerformApplicationType(
  userPermissions: string[],
  applicationType: ApplicationType
): boolean {
  // Check for wildcard permission
  if (userPermissions.includes('applications:*') || userPermissions.includes('*:*')) {
    return true;
  }
  
  // Check specific permission
  const requiredPermission = APPLICATION_PERMISSIONS[applicationType];
  return userPermissions.includes(requiredPermission);
}

/**
 * Get list of application types a user can perform
 */
export function getUserAllowedApplicationTypes(
  userPermissions: string[]
): ApplicationType[] {
  // Wildcard grants all
  if (userPermissions.includes('applications:*') || userPermissions.includes('*:*')) {
    return Object.keys(APPLICATION_PERMISSIONS) as ApplicationType[];
  }
  
  return (Object.entries(APPLICATION_PERMISSIONS) as [ApplicationType, string][])
    .filter(([, permission]) => userPermissions.includes(permission))
    .map(([type]) => type);
}


