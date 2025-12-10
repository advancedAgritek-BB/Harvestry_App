/**
 * Batch Naming Configuration Types
 * 
 * Types for configuring how batch names are automatically generated.
 * Supports both template-based (with tokens) and sequential numbering modes.
 */

// =============================================================================
// NAMING MODE
// =============================================================================

/**
 * Naming mode determines how batch names are generated
 */
export type NamingMode = 'template' | 'sequential';

// =============================================================================
// TEMPLATE TOKENS
// =============================================================================

/**
 * Available tokens for template-based naming
 */
export type TemplateToken =
  | '{STRAIN}'        // Full genetics/strain name (e.g., "Blue Dream")
  | '{STRAIN_CODE}'   // First 3 letters uppercase (e.g., "BLU")
  | '{STRAIN_ABBR}'   // Abbreviated strain name (e.g., "BD")
  | '{YYYY}'          // Full year (e.g., "2024")
  | '{YY}'            // 2-digit year (e.g., "24")
  | '{MM}'            // 2-digit month (e.g., "12")
  | '{DD}'            // 2-digit day (e.g., "09")
  | '{Q}'             // Quarter (e.g., "4")
  | '{###}'           // 3-digit auto-increment (e.g., "001")
  | '{####}'          // 4-digit auto-increment (e.g., "0001")
  | '{#####}'         // 5-digit auto-increment (e.g., "00001")
  | '{SITE}'          // Site code (e.g., "DEN")
  | '{SITE_ID}'       // Site ID prefix
  | '{ROOM}'          // Starting room code
  | '{PHASE}'         // Starting phase (e.g., "CLN", "VEG")
  | '{TYPE}'          // Genetic type abbreviation (e.g., "HYB", "IND", "SAT");

/**
 * Token metadata for UI display and documentation
 */
export interface TokenInfo {
  token: TemplateToken;
  label: string;
  description: string;
  example: string;
  category: 'strain' | 'date' | 'sequence' | 'location' | 'other';
}

/**
 * All available tokens with their metadata
 */
export const TEMPLATE_TOKENS: TokenInfo[] = [
  // Strain tokens
  {
    token: '{STRAIN}',
    label: 'Strain Name',
    description: 'Full genetics/strain name',
    example: 'Blue Dream',
    category: 'strain',
  },
  {
    token: '{STRAIN_CODE}',
    label: 'Strain Code',
    description: 'First 3 letters of strain name, uppercase',
    example: 'BLU',
    category: 'strain',
  },
  {
    token: '{STRAIN_ABBR}',
    label: 'Strain Abbreviation',
    description: 'Abbreviated strain initials',
    example: 'BD',
    category: 'strain',
  },
  {
    token: '{TYPE}',
    label: 'Genetic Type',
    description: 'Genetic type abbreviation (IND/SAT/HYB/AUTO/HMP)',
    example: 'HYB',
    category: 'strain',
  },
  // Date tokens
  {
    token: '{YYYY}',
    label: 'Year (4-digit)',
    description: 'Full 4-digit year',
    example: '2024',
    category: 'date',
  },
  {
    token: '{YY}',
    label: 'Year (2-digit)',
    description: '2-digit year',
    example: '24',
    category: 'date',
  },
  {
    token: '{MM}',
    label: 'Month',
    description: '2-digit month with leading zero',
    example: '12',
    category: 'date',
  },
  {
    token: '{DD}',
    label: 'Day',
    description: '2-digit day with leading zero',
    example: '09',
    category: 'date',
  },
  {
    token: '{Q}',
    label: 'Quarter',
    description: 'Quarter of the year (1-4)',
    example: '4',
    category: 'date',
  },
  // Sequence tokens
  {
    token: '{###}',
    label: 'Sequence (3-digit)',
    description: 'Auto-incrementing 3-digit number',
    example: '001',
    category: 'sequence',
  },
  {
    token: '{####}',
    label: 'Sequence (4-digit)',
    description: 'Auto-incrementing 4-digit number',
    example: '0001',
    category: 'sequence',
  },
  {
    token: '{#####}',
    label: 'Sequence (5-digit)',
    description: 'Auto-incrementing 5-digit number',
    example: '00001',
    category: 'sequence',
  },
  // Location tokens
  {
    token: '{SITE}',
    label: 'Site Code',
    description: 'Site/facility code',
    example: 'DEN',
    category: 'location',
  },
  {
    token: '{SITE_ID}',
    label: 'Site ID',
    description: 'Site identifier prefix',
    example: 'S1',
    category: 'location',
  },
  {
    token: '{ROOM}',
    label: 'Room Code',
    description: 'Starting room code',
    example: 'VEG-A',
    category: 'location',
  },
  {
    token: '{PHASE}',
    label: 'Phase',
    description: 'Starting cultivation phase abbreviation',
    example: 'CLN',
    category: 'other',
  },
];

// =============================================================================
// BATCH NAMING CONFIGURATION
// =============================================================================

/**
 * Batch naming configuration for a site
 */
export interface BatchNamingConfig {
  id: string;
  siteId: string;
  mode: NamingMode;
  
  // Template mode settings
  template?: string;              // e.g., "{STRAIN_CODE}-{YYYY}-{###}"
  
  // Sequential mode settings
  prefix?: string;                // e.g., "B-" or "BATCH-"
  suffix?: string;                // Optional suffix
  digitCount?: number;            // Number of digits (3-6)
  
  // Shared counter (used by both modes for {###} tokens)
  currentNumber: number;          // Current counter value
  resetFrequency?: ResetFrequency; // When to reset counter
  lastResetDate?: string;         // Last time counter was reset
  
  // Metadata
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  createdBy?: string;
  updatedBy?: string;
}

/**
 * When to reset the sequential counter
 */
export type ResetFrequency = 'never' | 'yearly' | 'quarterly' | 'monthly';

/**
 * Context needed to generate a batch name
 */
export interface BatchNameContext {
  strainName: string;
  geneticType?: 'indica' | 'sativa' | 'hybrid' | 'autoflower' | 'hemp';
  siteCode?: string;
  siteId?: string;
  roomCode?: string;
  startingPhase?: string;
  date?: Date;
}

/**
 * Request to update batch naming configuration
 */
export interface UpdateBatchNamingConfigRequest {
  mode?: NamingMode;
  template?: string;
  prefix?: string;
  suffix?: string;
  digitCount?: number;
  resetFrequency?: ResetFrequency;
}

// =============================================================================
// STATE COMPLIANCE HINTS
// =============================================================================

/**
 * State-specific batch naming requirements/recommendations
 */
export interface StateComplianceHint {
  stateCode: string;
  stateName: string;
  requirement: string;
  example: string;
  suggestedTemplate: string;
}

/**
 * Common state compliance requirements for batch naming
 */
export const STATE_COMPLIANCE_HINTS: StateComplianceHint[] = [
  {
    stateCode: 'CO',
    stateName: 'Colorado',
    requirement: 'METRC requires unique batch identifiers. Recommend including date and sequence.',
    example: 'BD-2024-0001',
    suggestedTemplate: '{STRAIN_CODE}-{YYYY}-{####}',
  },
  {
    stateCode: 'CA',
    stateName: 'California',
    requirement: 'METRC batch tags. Batch name should be traceable to source.',
    example: 'S1-BLUDRM-24Q4-001',
    suggestedTemplate: '{SITE_ID}-{STRAIN_CODE}-{YY}Q{Q}-{###}',
  },
  {
    stateCode: 'OR',
    stateName: 'Oregon',
    requirement: 'CTS requires unique batch numbers. Include harvest group info.',
    example: 'OR-BD-2024-12-0042',
    suggestedTemplate: '{SITE}-{STRAIN_CODE}-{YYYY}-{MM}-{####}',
  },
  {
    stateCode: 'WA',
    stateName: 'Washington',
    requirement: 'Leaf Data Systems integration. Sequential numbering recommended.',
    example: 'WA-B-00123',
    suggestedTemplate: '{SITE}-B-{#####}',
  },
  {
    stateCode: 'MI',
    stateName: 'Michigan',
    requirement: 'METRC integration. Unique plant batch identifier required.',
    example: 'MI-GG4-24-0015',
    suggestedTemplate: '{SITE}-{STRAIN_CODE}-{YY}-{####}',
  },
  {
    stateCode: 'NV',
    stateName: 'Nevada',
    requirement: 'METRC batch naming. Date-based naming recommended.',
    example: 'NV-241209-OGK-001',
    suggestedTemplate: '{SITE}-{YY}{MM}{DD}-{STRAIN_CODE}-{###}',
  },
];

// =============================================================================
// DEFAULT CONFIGURATIONS
// =============================================================================

/**
 * Default template for new configurations
 */
export const DEFAULT_TEMPLATE = '{STRAIN_CODE}-{YYYY}-{###}';

/**
 * Default sequential prefix
 */
export const DEFAULT_PREFIX = 'B-';

/**
 * Default digit count for sequential numbers
 */
export const DEFAULT_DIGIT_COUNT = 4;

/**
 * Create a default batch naming configuration
 */
export function createDefaultBatchNamingConfig(siteId: string): BatchNamingConfig {
  return {
    id: `bnc-${Date.now()}`,
    siteId,
    mode: 'template',
    template: DEFAULT_TEMPLATE,
    prefix: DEFAULT_PREFIX,
    digitCount: DEFAULT_DIGIT_COUNT,
    currentNumber: 0,
    resetFrequency: 'yearly',
    isActive: true,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  };
}

// =============================================================================
// UTILITY FUNCTIONS
// =============================================================================

/**
 * Get genetic type abbreviation
 */
export function getGeneticTypeAbbreviation(
  type?: 'indica' | 'sativa' | 'hybrid' | 'autoflower' | 'hemp'
): string {
  switch (type) {
    case 'indica': return 'IND';
    case 'sativa': return 'SAT';
    case 'hybrid': return 'HYB';
    case 'autoflower': return 'AUTO';
    case 'hemp': return 'HMP';
    default: return 'UNK';
  }
}

/**
 * Get phase abbreviation
 */
export function getPhaseAbbreviation(phase?: string): string {
  switch (phase?.toLowerCase()) {
    case 'clone': return 'CLN';
    case 'veg': return 'VEG';
    case 'flower': return 'FLR';
    case 'harvest': return 'HRV';
    case 'cure': return 'CUR';
    default: return 'NEW';
  }
}

/**
 * Generate strain code from name (first 3 letters uppercase)
 */
export function generateStrainCode(strainName: string): string {
  return strainName
    .replace(/[^a-zA-Z]/g, '')
    .substring(0, 3)
    .toUpperCase() || 'UNK';
}

/**
 * Generate strain abbreviation (initials of each word)
 */
export function generateStrainAbbreviation(strainName: string): string {
  return strainName
    .split(/\s+/)
    .map(word => word.charAt(0).toUpperCase())
    .join('') || 'U';
}




