/**
 * Batch Naming Service
 * 
 * Service for generating batch names based on configurable rules.
 * Supports both template-based and sequential naming modes.
 */

import {
  BatchNamingConfig,
  BatchNameContext,
  UpdateBatchNamingConfigRequest,
  createDefaultBatchNamingConfig,
  generateStrainCode,
  generateStrainAbbreviation,
  getGeneticTypeAbbreviation,
  getPhaseAbbreviation,
  ResetFrequency,
} from '../types/batchNaming.types';

// =============================================================================
// IN-MEMORY STORAGE (Replace with API calls in production)
// =============================================================================

// Mock storage for batch naming configs
const configStorage: Map<string, BatchNamingConfig> = new Map();

// Initialize with a default config for demo site
const defaultConfig = createDefaultBatchNamingConfig('site-1');
configStorage.set('site-1', defaultConfig);

// =============================================================================
// SERVICE CLASS
// =============================================================================

export class BatchNamingService {
  /**
   * Get batch naming configuration for a site
   */
  static async getBatchNamingConfig(siteId: string): Promise<BatchNamingConfig> {
    // TODO: Replace with actual API call
    // const response = await fetch(`/api/v1/sites/${siteId}/batch-naming-config`);
    // return response.json();

    let config = configStorage.get(siteId);
    if (!config) {
      config = createDefaultBatchNamingConfig(siteId);
      configStorage.set(siteId, config);
    }
    return config;
  }

  /**
   * Update batch naming configuration
   */
  static async updateBatchNamingConfig(
    siteId: string,
    updates: UpdateBatchNamingConfigRequest
  ): Promise<BatchNamingConfig> {
    // TODO: Replace with actual API call
    // const response = await fetch(`/api/v1/sites/${siteId}/batch-naming-config`, {
    //   method: 'PUT',
    //   headers: { 'Content-Type': 'application/json' },
    //   body: JSON.stringify(updates),
    // });
    // return response.json();

    const existing = await this.getBatchNamingConfig(siteId);
    const updated: BatchNamingConfig = {
      ...existing,
      ...updates,
      updatedAt: new Date().toISOString(),
    };
    configStorage.set(siteId, updated);
    return updated;
  }

  /**
   * Reset the sequential counter
   */
  static async resetCounter(siteId: string): Promise<BatchNamingConfig> {
    const config = await this.getBatchNamingConfig(siteId);
    config.currentNumber = 0;
    config.lastResetDate = new Date().toISOString();
    config.updatedAt = new Date().toISOString();
    configStorage.set(siteId, config);
    return config;
  }

  /**
   * Get the next sequential number and increment counter
   */
  static async getNextSequentialNumber(siteId: string): Promise<number> {
    const config = await this.getBatchNamingConfig(siteId);
    
    // Check if counter should be reset based on frequency
    if (this.shouldResetCounter(config)) {
      config.currentNumber = 0;
      config.lastResetDate = new Date().toISOString();
    }
    
    config.currentNumber += 1;
    config.updatedAt = new Date().toISOString();
    configStorage.set(siteId, config);
    
    return config.currentNumber;
  }

  /**
   * Generate a batch name using the configured rules
   */
  static async generateBatchName(
    siteId: string,
    context: BatchNameContext
  ): Promise<string> {
    const config = await this.getBatchNamingConfig(siteId);
    const nextNumber = await this.getNextSequentialNumber(siteId);
    
    return this.applyNamingRules(config, context, nextNumber);
  }

  /**
   * Preview a batch name without incrementing counter
   */
  static async previewBatchName(
    siteId: string,
    context: BatchNameContext
  ): Promise<string> {
    const config = await this.getBatchNamingConfig(siteId);
    const nextNumber = config.currentNumber + 1;
    
    return this.applyNamingRules(config, context, nextNumber);
  }

  /**
   * Preview with a specific config (for admin preview without saving)
   */
  static previewWithConfig(
    config: BatchNamingConfig,
    context: BatchNameContext,
    sequenceNumber?: number
  ): string {
    const number = sequenceNumber ?? (config.currentNumber + 1);
    return this.applyNamingRules(config, context, number);
  }

  // ===========================================================================
  // PRIVATE METHODS
  // ===========================================================================

  /**
   * Apply naming rules to generate the batch name
   */
  private static applyNamingRules(
    config: BatchNamingConfig,
    context: BatchNameContext,
    sequenceNumber: number
  ): string {
    if (config.mode === 'sequential') {
      return this.generateSequentialName(config, sequenceNumber);
    }
    return this.generateTemplatedName(config, context, sequenceNumber);
  }

  /**
   * Generate a sequential batch name
   */
  private static generateSequentialName(
    config: BatchNamingConfig,
    sequenceNumber: number
  ): string {
    const digits = config.digitCount ?? 4;
    const paddedNumber = sequenceNumber.toString().padStart(digits, '0');
    const prefix = config.prefix ?? 'B-';
    const suffix = config.suffix ?? '';
    
    return `${prefix}${paddedNumber}${suffix}`;
  }

  /**
   * Generate a templated batch name
   */
  private static generateTemplatedName(
    config: BatchNamingConfig,
    context: BatchNameContext,
    sequenceNumber: number
  ): string {
    const template = config.template ?? '{STRAIN_CODE}-{YYYY}-{###}';
    const date = context.date ?? new Date();
    
    let result = template;
    
    // Replace strain tokens
    result = result.replace(/\{STRAIN\}/gi, context.strainName);
    result = result.replace(/\{STRAIN_CODE\}/gi, generateStrainCode(context.strainName));
    result = result.replace(/\{STRAIN_ABBR\}/gi, generateStrainAbbreviation(context.strainName));
    result = result.replace(/\{TYPE\}/gi, getGeneticTypeAbbreviation(context.geneticType));
    
    // Replace date tokens
    result = result.replace(/\{YYYY\}/gi, date.getFullYear().toString());
    result = result.replace(/\{YY\}/gi, date.getFullYear().toString().slice(-2));
    result = result.replace(/\{MM\}/gi, (date.getMonth() + 1).toString().padStart(2, '0'));
    result = result.replace(/\{DD\}/gi, date.getDate().toString().padStart(2, '0'));
    result = result.replace(/\{Q\}/gi, (Math.floor(date.getMonth() / 3) + 1).toString());
    
    // Replace sequence tokens
    result = result.replace(/\{#####\}/gi, sequenceNumber.toString().padStart(5, '0'));
    result = result.replace(/\{####\}/gi, sequenceNumber.toString().padStart(4, '0'));
    result = result.replace(/\{###\}/gi, sequenceNumber.toString().padStart(3, '0'));
    
    // Replace location tokens
    result = result.replace(/\{SITE\}/gi, context.siteCode ?? 'SITE');
    result = result.replace(/\{SITE_ID\}/gi, context.siteId?.slice(0, 3).toUpperCase() ?? 'S1');
    result = result.replace(/\{ROOM\}/gi, context.roomCode ?? 'ROOM');
    result = result.replace(/\{PHASE\}/gi, getPhaseAbbreviation(context.startingPhase));
    
    return result;
  }

  /**
   * Check if the counter should be reset based on frequency
   */
  private static shouldResetCounter(config: BatchNamingConfig): boolean {
    if (!config.resetFrequency || config.resetFrequency === 'never') {
      return false;
    }

    if (!config.lastResetDate) {
      return false; // First use, don't reset yet
    }

    const lastReset = new Date(config.lastResetDate);
    const now = new Date();

    switch (config.resetFrequency) {
      case 'yearly':
        return now.getFullYear() > lastReset.getFullYear();
      
      case 'quarterly':
        const lastQuarter = Math.floor(lastReset.getMonth() / 3);
        const currentQuarter = Math.floor(now.getMonth() / 3);
        return (
          now.getFullYear() > lastReset.getFullYear() ||
          (now.getFullYear() === lastReset.getFullYear() && currentQuarter > lastQuarter)
        );
      
      case 'monthly':
        return (
          now.getFullYear() > lastReset.getFullYear() ||
          (now.getFullYear() === lastReset.getFullYear() && now.getMonth() > lastReset.getMonth())
        );
      
      default:
        return false;
    }
  }

  /**
   * Validate a template string
   */
  static validateTemplate(template: string): { valid: boolean; errors: string[] } {
    const errors: string[] = [];
    
    // Check for at least one sequence token
    const hasSequence = /\{#{3,5}\}/i.test(template);
    if (!hasSequence) {
      errors.push('Template should include a sequence token ({###}, {####}, or {#####}) to ensure uniqueness');
    }
    
    // Check for invalid tokens
    const validTokenPattern = /\{(STRAIN|STRAIN_CODE|STRAIN_ABBR|TYPE|YYYY|YY|MM|DD|Q|#{3,5}|SITE|SITE_ID|ROOM|PHASE)\}/gi;
    const allTokensPattern = /\{[^}]+\}/g;
    const allTokens = template.match(allTokensPattern) || [];
    const validTokens = template.match(validTokenPattern) || [];
    
    if (allTokens.length > validTokens.length) {
      const invalidTokens = allTokens.filter(t => !validTokens.includes(t));
      errors.push(`Invalid token(s): ${invalidTokens.join(', ')}`);
    }
    
    // Check minimum length
    if (template.length < 5) {
      errors.push('Template is too short');
    }
    
    return {
      valid: errors.length === 0,
      errors,
    };
  }

  /**
   * Get sample batch names for preview
   */
  static getSampleBatchNames(
    config: BatchNamingConfig,
    count: number = 3
  ): string[] {
    const sampleStrains = ['Blue Dream', 'OG Kush', 'Girl Scout Cookies'];
    const sampleTypes: Array<'indica' | 'sativa' | 'hybrid'> = ['sativa', 'indica', 'hybrid'];
    
    return sampleStrains.slice(0, count).map((strain, index) => {
      const context: BatchNameContext = {
        strainName: strain,
        geneticType: sampleTypes[index],
        siteCode: 'DEN',
        siteId: 'site-1',
        roomCode: 'VEG-A',
        startingPhase: 'clone',
        date: new Date(),
      };
      return this.previewWithConfig(config, context, config.currentNumber + index + 1);
    });
  }
}

export default BatchNamingService;




