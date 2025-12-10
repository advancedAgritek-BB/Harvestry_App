/**
 * Genetics Service
 * 
 * API operations for genetics management.
 * Integrates with backend genetics API at /api/v1/sites/{siteId}/genetics
 */

import type {
  Genetics,
  GeneticsResponse,
  CreateGeneticsRequest,
  UpdateGeneticsRequest,
  GeneticsFilters,
  GeneticsListResponse,
  PlannerGenetics,
  GeneticType,
  YieldPotential,
} from '../types';
import { toPlannerGenetics } from '../types';

// API base path - will use actual backend when ready
const API_BASE = '/api/v1/sites';

/**
 * Genetics Service Class
 * Handles all genetics CRUD operations
 */
export class GeneticsService {
  /**
   * Get all genetics for a site
   */
  static async getGenetics(
    siteId: string,
    filters?: GeneticsFilters
  ): Promise<GeneticsListResponse> {
    // TODO: Replace with actual API call when backend is connected
    // const response = await fetch(`${API_BASE}/${siteId}/genetics`);
    // return response.json();
    
    return this.getMockGenetics(filters);
  }

  /**
   * Get single genetics by ID
   */
  static async getGeneticsById(
    siteId: string,
    geneticsId: string
  ): Promise<Genetics | null> {
    // TODO: Replace with actual API call
    // const response = await fetch(`${API_BASE}/${siteId}/genetics/${geneticsId}`);
    // return response.json();
    
    const mockData = await this.getMockGeneticsList();
    return mockData.find((g) => g.id === geneticsId) || null;
  }

  /**
   * Create new genetics
   */
  static async createGenetics(
    siteId: string,
    request: CreateGeneticsRequest
  ): Promise<Genetics> {
    // TODO: Replace with actual API call
    // const response = await fetch(`${API_BASE}/${siteId}/genetics`, {
    //   method: 'POST',
    //   headers: { 'Content-Type': 'application/json' },
    //   body: JSON.stringify(request),
    // });
    // return response.json();
    
    console.log('Creating genetics:', request);
    
    const newGenetics: Genetics = {
      id: `gen-${Date.now()}`,
      siteId,
      name: request.name,
      description: request.description,
      geneticType: request.geneticType,
      thcMin: request.thcMin,
      thcMax: request.thcMax,
      cbdMin: request.cbdMin,
      cbdMax: request.cbdMax,
      floweringTimeDays: request.floweringTimeDays,
      yieldPotential: request.yieldPotential,
      growthCharacteristics: request.growthCharacteristics,
      terpeneProfile: request.terpeneProfile,
      breedingNotes: request.breedingNotes,
      // Phenotype fields
      expressionNotes: request.expressionNotes,
      visualCharacteristics: request.visualCharacteristics,
      aromaProfile: request.aromaProfile,
      growthPattern: request.growthPattern,
      // Source fields
      breeder: request.breeder,
      seedBank: request.seedBank,
      cultivationNotes: request.cultivationNotes,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      createdByUserId: 'current-user',
      updatedByUserId: 'current-user',
    };

    return newGenetics;
  }

  /**
   * Update existing genetics
   */
  static async updateGenetics(
    siteId: string,
    geneticsId: string,
    request: UpdateGeneticsRequest
  ): Promise<Genetics> {
    // TODO: Replace with actual API call
    // const response = await fetch(`${API_BASE}/${siteId}/genetics/${geneticsId}`, {
    //   method: 'PUT',
    //   headers: { 'Content-Type': 'application/json' },
    //   body: JSON.stringify(request),
    // });
    // return response.json();
    
    console.log('Updating genetics:', geneticsId, request);
    
    const existing = await this.getGeneticsById(siteId, geneticsId);
    if (!existing) {
      throw new Error('Genetics not found');
    }

    return {
      ...existing,
      description: request.description,
      thcMin: request.thcMin,
      thcMax: request.thcMax,
      cbdMin: request.cbdMin,
      cbdMax: request.cbdMax,
      floweringTimeDays: request.floweringTimeDays,
      yieldPotential: request.yieldPotential,
      growthCharacteristics: request.growthCharacteristics,
      terpeneProfile: request.terpeneProfile,
      breedingNotes: request.breedingNotes,
      // Phenotype fields
      expressionNotes: request.expressionNotes,
      visualCharacteristics: request.visualCharacteristics,
      aromaProfile: request.aromaProfile,
      growthPattern: request.growthPattern,
      // Source fields
      breeder: request.breeder,
      seedBank: request.seedBank,
      cultivationNotes: request.cultivationNotes,
      updatedAt: new Date().toISOString(),
      updatedByUserId: 'current-user',
    };
  }

  /**
   * Delete genetics
   */
  static async deleteGenetics(siteId: string, geneticsId: string): Promise<void> {
    // TODO: Replace with actual API call
    // await fetch(`${API_BASE}/${siteId}/genetics/${geneticsId}`, {
    //   method: 'DELETE',
    // });
    
    console.log('Deleting genetics:', geneticsId);
  }

  /**
   * Get genetics formatted for batch planner use
   */
  static async getGeneticsForPlanner(siteId: string): Promise<PlannerGenetics[]> {
    const response = await this.getGenetics(siteId);
    return response.items.map(toPlannerGenetics);
  }

  /**
   * Search genetics by name
   */
  static async searchGenetics(
    siteId: string,
    query: string,
    limit = 10
  ): Promise<Genetics[]> {
    const response = await this.getGenetics(siteId, { search: query });
    return response.items.slice(0, limit);
  }

  /**
   * Check if genetics name is available
   */
  static async checkNameAvailability(
    siteId: string,
    name: string,
    excludeId?: string
  ): Promise<boolean> {
    const mockData = await this.getMockGeneticsList();
    return !mockData.some(
      (g) => g.name.toLowerCase() === name.toLowerCase() && g.id !== excludeId
    );
  }

  // ============ Mock Data Helpers ============

  private static async getMockGenetics(
    filters?: GeneticsFilters
  ): Promise<GeneticsListResponse> {
    let items = await this.getMockGeneticsList();

    // Apply filters
    if (filters?.search) {
      const search = filters.search.toLowerCase();
      items = items.filter(
        (g) =>
          g.name.toLowerCase().includes(search) ||
          g.description.toLowerCase().includes(search)
      );
    }
    if (filters?.geneticTypes?.length) {
      items = items.filter((g) => filters.geneticTypes!.includes(g.geneticType));
    }
    if (filters?.yieldPotentials?.length) {
      items = items.filter((g) => filters.yieldPotentials!.includes(g.yieldPotential));
    }
    if (filters?.thcMin !== undefined) {
      items = items.filter((g) => g.thcMax >= filters.thcMin!);
    }
    if (filters?.thcMax !== undefined) {
      items = items.filter((g) => g.thcMin <= filters.thcMax!);
    }
    if (filters?.floweringTimeMin !== undefined) {
      items = items.filter(
        (g) => g.floweringTimeDays && g.floweringTimeDays >= filters.floweringTimeMin!
      );
    }
    if (filters?.floweringTimeMax !== undefined) {
      items = items.filter(
        (g) => g.floweringTimeDays && g.floweringTimeDays <= filters.floweringTimeMax!
      );
    }

    return {
      items,
      total: items.length,
      page: 1,
      pageSize: items.length,
    };
  }

  private static async getMockGeneticsList(): Promise<Genetics[]> {
    return MOCK_GENETICS;
  }
}

// ============ Mock Data ============

const MOCK_GENETICS: Genetics[] = [
  {
    id: 'gen-001',
    siteId: 'site-1',
    name: 'OG Kush',
    description: 'Classic indica-dominant hybrid known for its complex aroma and potent effects. Features earthy pine and sour lemon scents with a body-focused high.',
    geneticType: 'hybrid',
    thcMin: 20,
    thcMax: 26,
    cbdMin: 0.1,
    cbdMax: 0.3,
    floweringTimeDays: 56,
    yieldPotential: 'high',
    growthCharacteristics: {
      stretchTendency: 'moderate',
      branchingPattern: 'dense',
      leafMorphology: 'broad',
      internodeSpacing: 'tight',
      rootVigour: 'strong',
      optimalTemperatureMin: 68,
      optimalTemperatureMax: 80,
      optimalHumidityMin: 40,
      optimalHumidityMax: 55,
      lightIntensityPreference: 'high',
      nutrientSensitivity: 'moderate',
    },
    terpeneProfile: {
      dominantTerpenes: {
        myrcene: 0.8,
        limonene: 0.5,
        caryophyllene: 0.4,
      },
      aromaDescriptors: ['earthy', 'pine', 'citrus', 'fuel'],
      flavorDescriptors: ['woody', 'lemon', 'spicy'],
      overallProfile: 'Complex earthy and citrus profile with fuel undertones',
    },
    breedingNotes: 'Originally from Southern California. Parent of many modern hybrids.',
    // Phenotype fields
    expressionNotes: 'Typically expresses with dense, chunky buds covered in trichomes. May show slight purple hues in cooler temperatures. Strong lateral branching with good node spacing.',
    visualCharacteristics: {
      leafShape: 'broad',
      budStructure: 'dense',
      primaryColors: ['deep green', 'orange pistils'],
      secondaryColors: ['light purple'],
      trichomeDensity: 'heavy',
      pistilColor: 'orange',
    },
    aromaProfile: {
      primaryScents: ['fuel', 'pine'],
      secondaryNotes: ['lemon', 'earth'],
      intensity: 'strong',
      developmentNotes: 'Aroma intensifies significantly during late flower. Best after 2-week cure.',
    },
    growthPattern: {
      colaStructure: 'multiCola',
      canopyBehavior: 'compact',
      trainingResponse: 'excellent',
      preferredTrainingMethods: ['LST', 'topping', 'SCROG'],
      internodeLength: 'medium',
      lateralBranching: 'strong',
    },
    // Source fields
    breeder: 'Unknown (Southern California)',
    seedBank: 'Multiple sources available',
    cultivationNotes: 'Benefits from moderate defoliation during veg. Watch for PM in high humidity. Feed medium-heavy through flower.',
    createdAt: '2024-01-15T10:00:00Z',
    updatedAt: '2024-06-20T14:30:00Z',
    createdByUserId: 'user-1',
    updatedByUserId: 'user-1',
  },
  {
    id: 'gen-002',
    siteId: 'site-1',
    name: 'Blue Dream',
    description: 'Sativa-dominant hybrid offering balanced full-body relaxation with gentle cerebral invigoration. Sweet berry aroma with herbal notes.',
    geneticType: 'sativa',
    thcMin: 17,
    thcMax: 24,
    cbdMin: 0.1,
    cbdMax: 0.2,
    floweringTimeDays: 65,
    yieldPotential: 'veryHigh',
    growthCharacteristics: {
      stretchTendency: 'high',
      branchingPattern: 'open',
      leafMorphology: 'narrow',
      internodeSpacing: 'wide',
      rootVigour: 'strong',
      optimalTemperatureMin: 70,
      optimalTemperatureMax: 85,
      optimalHumidityMin: 45,
      optimalHumidityMax: 60,
      lightIntensityPreference: 'high',
      nutrientSensitivity: 'low',
    },
    terpeneProfile: {
      dominantTerpenes: {
        myrcene: 0.6,
        pinene: 0.4,
        caryophyllene: 0.3,
      },
      aromaDescriptors: ['blueberry', 'sweet', 'herbal', 'vanilla'],
      flavorDescriptors: ['berry', 'sweet', 'smooth'],
      overallProfile: 'Sweet blueberry with subtle herbal notes',
    },
    breedingNotes: 'Cross of Blueberry indica and Haze sativa. Easy to grow, high yielding.',
    // Phenotype fields
    expressionNotes: 'Tall, stretchy plants with long internodes. Produces large, airy colas with excellent trichome coverage. Consistent expression across phenotypes.',
    visualCharacteristics: {
      leafShape: 'narrow',
      budStructure: 'medium',
      primaryColors: ['bright green', 'blue hues'],
      secondaryColors: ['purple tips'],
      trichomeDensity: 'moderate',
      pistilColor: 'light orange',
    },
    aromaProfile: {
      primaryScents: ['blueberry', 'sweet berry'],
      secondaryNotes: ['vanilla', 'herbal'],
      intensity: 'moderate',
      developmentNotes: 'Berry notes most prominent after proper cure. Fresh cut has more herbal notes.',
    },
    growthPattern: {
      colaStructure: 'christmasTree',
      canopyBehavior: 'vertical',
      trainingResponse: 'good',
      preferredTrainingMethods: ['topping', 'mainlining'],
      internodeLength: 'long',
      lateralBranching: 'moderate',
    },
    // Source fields
    breeder: 'DJ Short (original Blueberry)',
    seedBank: 'Humboldt Seed Company',
    cultivationNotes: 'Heavy feeder, can handle high EC. Benefits from support in late flower. Great for beginners due to resilience.',
    createdAt: '2024-01-20T09:00:00Z',
    updatedAt: '2024-05-15T11:00:00Z',
    createdByUserId: 'user-1',
    updatedByUserId: 'user-1',
  },
  {
    id: 'gen-003',
    siteId: 'site-1',
    name: 'Girl Scout Cookies',
    description: 'Potent indica-dominant hybrid with euphoric effects. Features sweet and earthy aroma with hints of mint and chocolate.',
    geneticType: 'hybrid',
    thcMin: 25,
    thcMax: 28,
    cbdMin: 0.1,
    cbdMax: 0.2,
    floweringTimeDays: 63,
    yieldPotential: 'medium',
    growthCharacteristics: {
      stretchTendency: 'moderate',
      branchingPattern: 'dense',
      leafMorphology: 'mixed',
      internodeSpacing: 'tight',
      rootVigour: 'moderate',
      optimalTemperatureMin: 68,
      optimalTemperatureMax: 78,
      optimalHumidityMin: 40,
      optimalHumidityMax: 50,
      lightIntensityPreference: 'high',
      nutrientSensitivity: 'moderate',
    },
    terpeneProfile: {
      dominantTerpenes: {
        caryophyllene: 0.7,
        limonene: 0.5,
        humulene: 0.3,
      },
      aromaDescriptors: ['sweet', 'earthy', 'mint', 'chocolate'],
      flavorDescriptors: ['cookie', 'sweet', 'nutty'],
      overallProfile: 'Sweet dessert-like profile with earthy undertones',
    },
    breedingNotes: 'OG Kush x Durban Poison cross. Known for beautiful purple coloring.',
    // Phenotype fields
    expressionNotes: 'Compact plants with stunning bag appeal. Expresses beautiful purple and orange colors in cooler temperatures. Frosty trichome coverage.',
    visualCharacteristics: {
      leafShape: 'medium',
      budStructure: 'dense',
      primaryColors: ['dark green', 'deep purple'],
      secondaryColors: ['orange', 'pink pistils'],
      trichomeDensity: 'frosty',
      pistilColor: 'orange-pink',
    },
    aromaProfile: {
      primaryScents: ['sweet dough', 'mint'],
      secondaryNotes: ['chocolate', 'earth'],
      intensity: 'strong',
      developmentNotes: 'Cookie/dough notes become more pronounced with extended cure.',
    },
    growthPattern: {
      colaStructure: 'multiCola',
      canopyBehavior: 'compact',
      trainingResponse: 'good',
      preferredTrainingMethods: ['LST', 'topping'],
      internodeLength: 'short',
      lateralBranching: 'moderate',
    },
    // Source fields
    breeder: 'Cookie Fam Genetics',
    seedBank: 'Archive Seed Bank',
    cultivationNotes: 'Lower temps in late flower bring out colors. Medium feeder - avoid overfeeding. Can be finicky but worth the effort.',
    createdAt: '2024-02-01T14:00:00Z',
    updatedAt: '2024-07-10T09:30:00Z',
    createdByUserId: 'user-1',
    updatedByUserId: 'user-2',
  },
  {
    id: 'gen-004',
    siteId: 'site-1',
    name: 'Gorilla Glue #4',
    description: 'Extremely potent hybrid known for heavy resin production. Earthy, sour, and piney aroma with powerful euphoric effects.',
    geneticType: 'hybrid',
    thcMin: 25,
    thcMax: 30,
    cbdMin: 0.05,
    cbdMax: 0.1,
    floweringTimeDays: 58,
    yieldPotential: 'high',
    growthCharacteristics: {
      stretchTendency: 'moderate',
      branchingPattern: 'dense',
      leafMorphology: 'broad',
      internodeSpacing: 'medium',
      rootVigour: 'strong',
      optimalTemperatureMin: 68,
      optimalTemperatureMax: 80,
      optimalHumidityMin: 40,
      optimalHumidityMax: 50,
      lightIntensityPreference: 'high',
      nutrientSensitivity: 'low',
    },
    terpeneProfile: {
      dominantTerpenes: {
        caryophyllene: 0.8,
        myrcene: 0.5,
        limonene: 0.4,
      },
      aromaDescriptors: ['diesel', 'earthy', 'pine', 'sour'],
      flavorDescriptors: ['coffee', 'chocolate', 'diesel'],
      overallProfile: 'Pungent diesel and earthy notes with coffee undertones',
    },
    breedingNotes: 'Accidental cross that became legendary. Extremely resinous.',
    createdAt: '2024-02-15T10:00:00Z',
    updatedAt: '2024-06-01T16:00:00Z',
    createdByUserId: 'user-1',
    updatedByUserId: 'user-1',
  },
  {
    id: 'gen-005',
    siteId: 'site-1',
    name: 'Wedding Cake',
    description: 'Indica-dominant hybrid with relaxing and euphoric effects. Rich vanilla and sweet earth aroma profile.',
    geneticType: 'indica',
    thcMin: 22,
    thcMax: 27,
    cbdMin: 0.1,
    cbdMax: 0.3,
    floweringTimeDays: 60,
    yieldPotential: 'high',
    growthCharacteristics: {
      stretchTendency: 'low',
      branchingPattern: 'dense',
      leafMorphology: 'broad',
      internodeSpacing: 'tight',
      rootVigour: 'strong',
      optimalTemperatureMin: 65,
      optimalTemperatureMax: 78,
      optimalHumidityMin: 40,
      optimalHumidityMax: 50,
      lightIntensityPreference: 'medium-high',
      nutrientSensitivity: 'moderate',
    },
    terpeneProfile: {
      dominantTerpenes: {
        limonene: 0.7,
        caryophyllene: 0.5,
        myrcene: 0.4,
      },
      aromaDescriptors: ['vanilla', 'sweet', 'earthy', 'pepper'],
      flavorDescriptors: ['cake', 'vanilla', 'creamy'],
      overallProfile: 'Sweet vanilla cake with peppery undertones',
    },
    breedingNotes: 'Triangle Kush x Animal Mints cross. Excellent bag appeal.',
    createdAt: '2024-03-01T11:00:00Z',
    updatedAt: '2024-08-05T13:45:00Z',
    createdByUserId: 'user-2',
    updatedByUserId: 'user-2',
  },
  {
    id: 'gen-006',
    siteId: 'site-1',
    name: 'Northern Lights',
    description: 'Pure indica known for resinous buds, fast flowering, and resilience. Sweet and spicy with relaxing body effects.',
    geneticType: 'indica',
    thcMin: 16,
    thcMax: 21,
    cbdMin: 0.1,
    cbdMax: 0.3,
    floweringTimeDays: 49,
    yieldPotential: 'medium',
    growthCharacteristics: {
      stretchTendency: 'low',
      branchingPattern: 'compact',
      leafMorphology: 'broad',
      internodeSpacing: 'tight',
      rootVigour: 'strong',
      optimalTemperatureMin: 65,
      optimalTemperatureMax: 75,
      optimalHumidityMin: 40,
      optimalHumidityMax: 50,
      lightIntensityPreference: 'medium',
      nutrientSensitivity: 'low',
    },
    terpeneProfile: {
      dominantTerpenes: {
        myrcene: 0.9,
        caryophyllene: 0.3,
        pinene: 0.2,
      },
      aromaDescriptors: ['sweet', 'spicy', 'pine', 'earthy'],
      flavorDescriptors: ['sweet', 'herbal', 'honey'],
      overallProfile: 'Sweet and spicy with earthy pine notes',
    },
    breedingNotes: 'Legendary indica from the Pacific Northwest. Parent of many strains.',
    createdAt: '2024-03-15T08:00:00Z',
    updatedAt: '2024-07-20T10:00:00Z',
    createdByUserId: 'user-1',
    updatedByUserId: 'user-1',
  },
  {
    id: 'gen-007',
    siteId: 'site-1',
    name: 'Sour Diesel',
    description: 'Legendary sativa with energizing, dreamy cerebral effects. Pungent diesel aroma with hints of citrus.',
    geneticType: 'sativa',
    thcMin: 19,
    thcMax: 25,
    cbdMin: 0.1,
    cbdMax: 0.2,
    floweringTimeDays: 77,
    yieldPotential: 'high',
    growthCharacteristics: {
      stretchTendency: 'very high',
      branchingPattern: 'open',
      leafMorphology: 'narrow',
      internodeSpacing: 'wide',
      rootVigour: 'strong',
      optimalTemperatureMin: 72,
      optimalTemperatureMax: 85,
      optimalHumidityMin: 45,
      optimalHumidityMax: 55,
      lightIntensityPreference: 'high',
      nutrientSensitivity: 'moderate',
    },
    terpeneProfile: {
      dominantTerpenes: {
        caryophyllene: 0.6,
        limonene: 0.5,
        myrcene: 0.4,
      },
      aromaDescriptors: ['diesel', 'citrus', 'pungent', 'herbal'],
      flavorDescriptors: ['diesel', 'lemon', 'earthy'],
      overallProfile: 'Unmistakable diesel fuel aroma with citrus notes',
    },
    breedingNotes: 'East Coast legend. Longer flowering time but worth the wait.',
    createdAt: '2024-04-01T12:00:00Z',
    updatedAt: '2024-09-01T14:00:00Z',
    createdByUserId: 'user-1',
    updatedByUserId: 'user-1',
  },
  {
    id: 'gen-008',
    siteId: 'site-1',
    name: 'Charlotte\'s Web',
    description: 'High-CBD, low-THC hemp cultivar bred for therapeutic applications. Mild, herbal aroma with no intoxicating effects.',
    geneticType: 'hemp',
    thcMin: 0.1,
    thcMax: 0.3,
    cbdMin: 15,
    cbdMax: 20,
    floweringTimeDays: 70,
    yieldPotential: 'medium',
    growthCharacteristics: {
      stretchTendency: 'moderate',
      branchingPattern: 'open',
      leafMorphology: 'narrow',
      internodeSpacing: 'medium',
      rootVigour: 'strong',
      optimalTemperatureMin: 65,
      optimalTemperatureMax: 80,
      optimalHumidityMin: 40,
      optimalHumidityMax: 55,
      lightIntensityPreference: 'medium',
      nutrientSensitivity: 'low',
    },
    terpeneProfile: {
      dominantTerpenes: {
        myrcene: 0.4,
        pinene: 0.3,
        caryophyllene: 0.2,
      },
      aromaDescriptors: ['herbal', 'pine', 'subtle', 'fresh'],
      flavorDescriptors: ['mild', 'herbal', 'pleasant'],
      overallProfile: 'Subtle herbal profile, non-intoxicating',
    },
    breedingNotes: 'Bred by Stanley Brothers. Named after Charlotte Figi.',
    createdAt: '2024-04-15T09:00:00Z',
    updatedAt: '2024-08-10T11:30:00Z',
    createdByUserId: 'user-2',
    updatedByUserId: 'user-2',
  },
];

export default GeneticsService;


