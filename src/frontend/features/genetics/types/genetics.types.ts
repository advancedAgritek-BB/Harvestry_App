/**
 * Genetics Type Definitions
 * 
 * Frontend types mirroring the backend genetics domain.
 * Used for managing genetics data in the Library and Batch Planner.
 */

// =============================================================================
// ENUMS
// =============================================================================

/** Type of genetic classification */
export type GeneticType = 'indica' | 'sativa' | 'hybrid' | 'autoflower' | 'hemp';

/** Expected yield potential classification */
export type YieldPotential = 'low' | 'medium' | 'high' | 'veryHigh';

/** Leaf shape classification for visual characteristics */
export type LeafShape = 'narrow' | 'medium' | 'broad';

/** Bud structure density classification */
export type BudStructure = 'airy' | 'medium' | 'dense' | 'rockHard';

/** Trichome density classification */
export type TrichomeDensity = 'light' | 'moderate' | 'heavy' | 'frosty';

/** Aroma intensity classification */
export type AromaIntensity = 'subtle' | 'moderate' | 'strong' | 'pungent';

/** Cola structure classification */
export type ColaStructure = 'singleDominant' | 'multiCola' | 'christmasTree';

/** Canopy behavior classification */
export type CanopyBehavior = 'compact' | 'spreading' | 'vertical';

/** Training response classification */
export type TrainingResponse = 'excellent' | 'good' | 'moderate' | 'sensitive';

// =============================================================================
// VALUE OBJECTS
// =============================================================================

/**
 * Growth characteristics and environmental preferences
 */
export interface GeneticProfile {
  stretchTendency?: string;
  branchingPattern?: string;
  leafMorphology?: string;
  internodeSpacing?: string;
  rootVigour?: string;
  optimalTemperatureMin?: number;
  optimalTemperatureMax?: number;
  optimalHumidityMin?: number;
  optimalHumidityMax?: number;
  lightIntensityPreference?: string;
  nutrientSensitivity?: string;
  additionalCharacteristics?: Record<string, unknown>;
}

/**
 * Terpene profile for aroma and flavor characteristics
 */
export interface TerpeneProfile {
  dominantTerpenes?: Record<string, number>;
  aromaDescriptors?: string[];
  flavorDescriptors?: string[];
  overallProfile?: string;
  additionalData?: Record<string, unknown>;
}

/**
 * Visual characteristics - phenotype expression traits
 */
export interface VisualCharacteristics {
  leafShape?: LeafShape;
  budStructure?: BudStructure;
  primaryColors?: string[];
  secondaryColors?: string[];
  trichomeDensity?: TrichomeDensity;
  pistilColor?: string;
  additionalTraits?: Record<string, unknown>;
}

/**
 * Aroma profile - distinct scent characteristics (separate from terpene analysis)
 */
export interface AromaProfile {
  primaryScents?: string[];
  secondaryNotes?: string[];
  intensity?: AromaIntensity;
  developmentNotes?: string;
}

/**
 * Growth pattern - structural and training behavior
 */
export interface GrowthPattern {
  colaStructure?: ColaStructure;
  canopyBehavior?: CanopyBehavior;
  trainingResponse?: TrainingResponse;
  preferredTrainingMethods?: string[];
  internodeLength?: string;
  lateralBranching?: string;
  additionalNotes?: string;
}

// =============================================================================
// MAIN ENTITIES
// =============================================================================

/**
 * Full genetics entity from the backend
 */
export interface Genetics {
  id: string;
  siteId: string;
  name: string;
  description: string;
  geneticType: GeneticType;
  thcMin: number;
  thcMax: number;
  cbdMin: number;
  cbdMax: number;
  floweringTimeDays?: number;
  yieldPotential: YieldPotential;
  growthCharacteristics: GeneticProfile;
  terpeneProfile: TerpeneProfile;
  breedingNotes?: string;
  // Phenotype fields
  expressionNotes?: string;
  visualCharacteristics?: VisualCharacteristics;
  aromaProfile?: AromaProfile;
  growthPattern?: GrowthPattern;
  // Source/Strain fields
  breeder?: string;
  seedBank?: string;
  cultivationNotes?: string;
  // Metadata
  createdAt: string;
  updatedAt: string;
  createdByUserId: string;
  updatedByUserId: string;
}

/**
 * Simplified genetics type for batch planner use
 * Extends the planner's existing Genetics interface pattern
 */
export interface PlannerGenetics {
  id: string;
  name: string;
  geneticType: GeneticType;
  defaultCloneDays: number;
  defaultVegDays: number;
  defaultFlowerDays: number;
  defaultHarvestDays: number;
  defaultCureDays: number;
  thcRange?: { min: number; max: number };
  cbdRange?: { min: number; max: number };
}

// =============================================================================
// REQUEST/RESPONSE DTOs
// =============================================================================

/**
 * Request to create new genetics
 */
export interface CreateGeneticsRequest {
  name: string;
  description: string;
  geneticType: GeneticType;
  thcMin: number;
  thcMax: number;
  cbdMin: number;
  cbdMax: number;
  floweringTimeDays?: number;
  yieldPotential: YieldPotential;
  growthCharacteristics: GeneticProfile;
  terpeneProfile: TerpeneProfile;
  breedingNotes?: string;
  // Phenotype fields
  expressionNotes?: string;
  visualCharacteristics?: VisualCharacteristics;
  aromaProfile?: AromaProfile;
  growthPattern?: GrowthPattern;
  // Source/Strain fields
  breeder?: string;
  seedBank?: string;
  cultivationNotes?: string;
}

/**
 * Request to update existing genetics
 */
export interface UpdateGeneticsRequest {
  description: string;
  thcMin: number;
  thcMax: number;
  cbdMin: number;
  cbdMax: number;
  floweringTimeDays?: number;
  yieldPotential: YieldPotential;
  growthCharacteristics: GeneticProfile;
  terpeneProfile: TerpeneProfile;
  breedingNotes?: string;
  // Phenotype fields
  expressionNotes?: string;
  visualCharacteristics?: VisualCharacteristics;
  aromaProfile?: AromaProfile;
  growthPattern?: GrowthPattern;
  // Source/Strain fields
  breeder?: string;
  seedBank?: string;
  cultivationNotes?: string;
}

/**
 * Genetics response from API (same as Genetics entity)
 */
export type GeneticsResponse = Genetics;

// =============================================================================
// FILTER & QUERY TYPES
// =============================================================================

/**
 * Filter options for genetics list
 */
export interface GeneticsFilters {
  search?: string;
  geneticTypes?: GeneticType[];
  yieldPotentials?: YieldPotential[];
  thcMin?: number;
  thcMax?: number;
  cbdMin?: number;
  cbdMax?: number;
  floweringTimeMin?: number;
  floweringTimeMax?: number;
}

/**
 * Paginated response for genetics list
 */
export interface GeneticsListResponse {
  items: Genetics[];
  total: number;
  page: number;
  pageSize: number;
}

// =============================================================================
// UI HELPER TYPES
// =============================================================================

/**
 * Display configuration for genetic types
 */
export const GENETIC_TYPE_CONFIG: Record<GeneticType, { label: string; color: string; description: string }> = {
  indica: {
    label: 'Indica',
    color: '#8B5CF6', // violet
    description: 'Relaxing, sedating effects. Typically shorter, bushier plants.',
  },
  sativa: {
    label: 'Sativa',
    color: '#10B981', // emerald
    description: 'Energizing, uplifting effects. Typically taller plants with narrow leaves.',
  },
  hybrid: {
    label: 'Hybrid',
    color: '#F59E0B', // amber
    description: 'Balanced effects combining indica and sativa characteristics.',
  },
  autoflower: {
    label: 'Autoflower',
    color: '#06B6D4', // cyan
    description: 'Automatic flowering independent of light cycle. Fast growing.',
  },
  hemp: {
    label: 'Hemp',
    color: '#84CC16', // lime
    description: 'High CBD, low THC. Used for CBD products and industrial purposes.',
  },
};

/**
 * Display configuration for yield potential
 */
export const YIELD_POTENTIAL_CONFIG: Record<YieldPotential, { label: string; color: string }> = {
  low: { label: 'Low', color: '#EF4444' },
  medium: { label: 'Medium', color: '#F59E0B' },
  high: { label: 'High', color: '#10B981' },
  veryHigh: { label: 'Very High', color: '#8B5CF6' },
};

/**
 * Display configuration for leaf shape
 */
export const LEAF_SHAPE_CONFIG: Record<LeafShape, { label: string }> = {
  narrow: { label: 'Narrow (Sativa-like)' },
  medium: { label: 'Medium' },
  broad: { label: 'Broad (Indica-like)' },
};

/**
 * Display configuration for bud structure
 */
export const BUD_STRUCTURE_CONFIG: Record<BudStructure, { label: string }> = {
  airy: { label: 'Airy' },
  medium: { label: 'Medium Density' },
  dense: { label: 'Dense' },
  rockHard: { label: 'Rock Hard' },
};

/**
 * Display configuration for trichome density
 */
export const TRICHOME_DENSITY_CONFIG: Record<TrichomeDensity, { label: string }> = {
  light: { label: 'Light' },
  moderate: { label: 'Moderate' },
  heavy: { label: 'Heavy' },
  frosty: { label: 'Frosty' },
};

/**
 * Display configuration for aroma intensity
 */
export const AROMA_INTENSITY_CONFIG: Record<AromaIntensity, { label: string }> = {
  subtle: { label: 'Subtle' },
  moderate: { label: 'Moderate' },
  strong: { label: 'Strong' },
  pungent: { label: 'Pungent' },
};

/**
 * Display configuration for cola structure
 */
export const COLA_STRUCTURE_CONFIG: Record<ColaStructure, { label: string }> = {
  singleDominant: { label: 'Single Dominant Cola' },
  multiCola: { label: 'Multi-Cola' },
  christmasTree: { label: 'Christmas Tree' },
};

/**
 * Display configuration for canopy behavior
 */
export const CANOPY_BEHAVIOR_CONFIG: Record<CanopyBehavior, { label: string }> = {
  compact: { label: 'Compact' },
  spreading: { label: 'Spreading' },
  vertical: { label: 'Vertical' },
};

/**
 * Display configuration for training response
 */
export const TRAINING_RESPONSE_CONFIG: Record<TrainingResponse, { label: string; color: string }> = {
  excellent: { label: 'Excellent', color: '#10B981' },
  good: { label: 'Good', color: '#22C55E' },
  moderate: { label: 'Moderate', color: '#F59E0B' },
  sensitive: { label: 'Sensitive', color: '#EF4444' },
};

// =============================================================================
// UTILITY FUNCTIONS
// =============================================================================

/**
 * Convert full Genetics to PlannerGenetics format
 */
export function toPlannerGenetics(genetics: Genetics): PlannerGenetics {
  // Default phase durations based on genetic type
  const defaultDurations = getDefaultPhaseDurations(genetics.geneticType);
  
  return {
    id: genetics.id,
    name: genetics.name,
    geneticType: genetics.geneticType,
    defaultCloneDays: defaultDurations.clone,
    defaultVegDays: defaultDurations.veg,
    defaultFlowerDays: genetics.floweringTimeDays ?? defaultDurations.flower,
    defaultHarvestDays: defaultDurations.harvest,
    defaultCureDays: defaultDurations.cure,
    thcRange: { min: genetics.thcMin, max: genetics.thcMax },
    cbdRange: { min: genetics.cbdMin, max: genetics.cbdMax },
  };
}

/**
 * Get default phase durations based on genetic type
 */
export function getDefaultPhaseDurations(geneticType: GeneticType): {
  clone: number;
  veg: number;
  flower: number;
  harvest: number;
  cure: number;
} {
  switch (geneticType) {
    case 'indica':
      return { clone: 14, veg: 21, flower: 56, harvest: 3, cure: 14 };
    case 'sativa':
      return { clone: 14, veg: 28, flower: 70, harvest: 3, cure: 14 };
    case 'hybrid':
      return { clone: 14, veg: 24, flower: 63, harvest: 3, cure: 14 };
    case 'autoflower':
      return { clone: 7, veg: 21, flower: 49, harvest: 3, cure: 14 };
    case 'hemp':
      return { clone: 14, veg: 28, flower: 60, harvest: 3, cure: 7 };
    default:
      return { clone: 14, veg: 24, flower: 60, harvest: 3, cure: 14 };
  }
}

/**
 * Create empty genetics profile
 */
export function createEmptyGeneticProfile(): GeneticProfile {
  return {};
}

/**
 * Create empty terpene profile
 */
export function createEmptyTerpeneProfile(): TerpeneProfile {
  return {};
}

/**
 * Create empty visual characteristics
 */
export function createEmptyVisualCharacteristics(): VisualCharacteristics {
  return {};
}

/**
 * Create empty aroma profile
 */
export function createEmptyAromaProfile(): AromaProfile {
  return {};
}

/**
 * Create empty growth pattern
 */
export function createEmptyGrowthPattern(): GrowthPattern {
  return {};
}


