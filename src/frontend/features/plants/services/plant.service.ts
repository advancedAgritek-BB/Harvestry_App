/**
 * Plant Service
 * 
 * Service for managing plants throughout the cultivation lifecycle.
 * Handles CRUD operations, phase transitions, and METRC sync.
 */

import type {
  Plant,
  PlantBatch,
  PlantCounts,
  PlantLossRecord,
  StartBatchRequest,
  RecordLossRequest,
  TransitionPhaseRequest,
  AssignTagsRequest,
  PlantGrowthPhase,
} from '../types';

// =============================================================================
// MOCK DATA
// =============================================================================

const mockPlantBatches: Map<string, PlantBatch[]> = new Map();
const mockPlants: Map<string, Plant[]> = new Map();
const mockLossRecords: Map<string, PlantLossRecord[]> = new Map();

// Initialize with some mock data
const initMockData = () => {
  // Add a started batch with plants
  mockPlantBatches.set('batch-demo-1', [
    {
      id: 'pb-1',
      siteId: 'site-1',
      batchId: 'batch-demo-1',
      name: 'GSC Clone Run #47',
      strainId: 'strain-1',
      strainName: 'Girl Scout Cookies',
      initialCount: 100,
      currentCount: 92,
      destroyedCount: 8,
      taggedCount: 0,
      sourceType: 'clone',
      growthPhase: 'immature',
      plantedDate: '2024-12-01',
      roomId: 'room-clone-1',
      roomName: 'Clone Room A',
      metrcSyncStatus: 'synced',
      createdAt: '2024-12-01T10:00:00Z',
      updatedAt: '2024-12-05T14:30:00Z',
      createdByUserId: 'user-1',
    },
  ]);

  mockLossRecords.set('batch-demo-1', [
    {
      id: 'loss-1',
      batchId: 'batch-demo-1',
      quantity: 5,
      reason: 'didnt_root',
      phase: 'immature',
      recordedAt: '2024-12-03T09:00:00Z',
      recordedByUserId: 'user-1',
      recordedByUserName: 'John Smith',
      metrcSynced: true,
    },
    {
      id: 'loss-2',
      batchId: 'batch-demo-1',
      quantity: 3,
      reason: 'disease',
      reasonNote: 'Powdery mildew detected',
      phase: 'immature',
      recordedAt: '2024-12-05T14:30:00Z',
      recordedByUserId: 'user-1',
      recordedByUserName: 'John Smith',
      metrcSynced: true,
    },
  ]);
};

initMockData();

// =============================================================================
// SERVICE CLASS
// =============================================================================

export class PlantService {
  /**
   * Get plant counts summary for a batch
   */
  static async getPlantCounts(batchId: string): Promise<PlantCounts> {
    const plantBatches = mockPlantBatches.get(batchId) || [];
    const plants = mockPlants.get(batchId) || [];

    // Calculate counts from plant batches (immature)
    let immatureCount = 0;
    let destroyedFromBatches = 0;
    let startedCount = 0;

    plantBatches.forEach((pb) => {
      startedCount += pb.initialCount;
      if (pb.growthPhase === 'immature') {
        immatureCount += pb.currentCount;
      }
      destroyedFromBatches += pb.destroyedCount;
    });

    // Calculate counts from individual plants (tagged)
    let vegetativeCount = 0;
    let floweringCount = 0;
    let harvestedCount = 0;
    let destroyedTagged = 0;

    plants.forEach((p) => {
      switch (p.growthPhase) {
        case 'vegetative':
          vegetativeCount++;
          break;
        case 'flowering':
          floweringCount++;
          break;
        case 'harvested':
          harvestedCount++;
          break;
        case 'destroyed':
          destroyedTagged++;
          break;
      }
    });

    const taggedCount = plants.filter(
      (p) => p.status === 'active' || p.status === 'quarantined'
    ).length;

    return {
      planned: 100, // Would come from batch data
      started: startedCount,
      current: immatureCount + vegetativeCount + floweringCount,
      destroyed: destroyedFromBatches + destroyedTagged,
      harvested: harvestedCount,
      immature: immatureCount,
      vegetative: vegetativeCount,
      flowering: floweringCount,
      tagged: taggedCount,
      untagged: immatureCount,
    };
  }

  /**
   * Get plant batches (immature groups) for a batch
   */
  static async getPlantBatches(batchId: string): Promise<PlantBatch[]> {
    return mockPlantBatches.get(batchId) || [];
  }

  /**
   * Get individual tagged plants for a batch
   */
  static async getPlants(batchId: string): Promise<Plant[]> {
    return mockPlants.get(batchId) || [];
  }

  /**
   * Get plant loss history for a batch
   */
  static async getLossHistory(batchId: string): Promise<PlantLossRecord[]> {
    return mockLossRecords.get(batchId) || [];
  }

  /**
   * Start a batch with plants - creates initial plant batch record
   */
  static async startBatch(request: StartBatchRequest): Promise<PlantBatch> {
    const newPlantBatch: PlantBatch = {
      id: `pb-${Date.now()}`,
      siteId: 'site-1',
      batchId: request.batchId,
      name: `Batch ${request.batchId.slice(-4)} Plants`,
      strainId: 'strain-1', // Would come from batch
      strainName: 'Unknown', // Would come from batch
      initialCount: request.actualPlantCount,
      currentCount: request.actualPlantCount,
      destroyedCount: 0,
      taggedCount: 0,
      sourceType: request.sourceType,
      sourceMotherBatchId: request.sourceMotherBatchId,
      sourcePackageLabel: request.sourcePackageLabel,
      growthPhase: 'immature',
      plantedDate: request.plantedDate || new Date().toISOString().split('T')[0],
      roomId: request.roomId,
      metrcSyncStatus: 'pending',
      notes: request.notes,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      createdByUserId: 'current-user',
    };

    const existing = mockPlantBatches.get(request.batchId) || [];
    mockPlantBatches.set(request.batchId, [...existing, newPlantBatch]);

    return newPlantBatch;
  }

  /**
   * Record plant loss/destruction
   */
  static async recordLoss(request: RecordLossRequest): Promise<PlantLossRecord> {
    const lossRecord: PlantLossRecord = {
      id: `loss-${Date.now()}`,
      batchId: request.batchId,
      quantity: request.quantity,
      reason: request.reason,
      reasonNote: request.reasonNote,
      phase: 'immature', // Would determine from plant batch or plants
      recordedAt: new Date().toISOString(),
      recordedByUserId: 'current-user',
      recordedByUserName: 'Current User',
      metrcSynced: false,
    };

    // Update plant batch counts
    if (request.plantBatchId) {
      const batches = mockPlantBatches.get(request.batchId) || [];
      const updatedBatches = batches.map((pb) => {
        if (pb.id === request.plantBatchId) {
          return {
            ...pb,
            currentCount: pb.currentCount - request.quantity,
            destroyedCount: pb.destroyedCount + request.quantity,
            updatedAt: new Date().toISOString(),
          };
        }
        return pb;
      });
      mockPlantBatches.set(request.batchId, updatedBatches);
    }

    // Update individual plants if specified
    if (request.plantIds && request.plantIds.length > 0) {
      const plants = mockPlants.get(request.batchId) || [];
      const updatedPlants = plants.map((p) => {
        if (request.plantIds!.includes(p.id)) {
          return {
            ...p,
            growthPhase: 'destroyed' as PlantGrowthPhase,
            status: 'destroyed' as const,
            destroyedDate: request.destroyedDate || new Date().toISOString().split('T')[0],
            destroyReason: request.reason,
            destroyReasonNote: request.reasonNote,
            wasteWeight: request.wasteWeight,
            wasteWeightUnit: request.wasteWeightUnit,
            wasteMethod: request.wasteMethod,
            destroyWitnessUserId: request.witnessUserId,
            updatedAt: new Date().toISOString(),
          };
        }
        return p;
      });
      mockPlants.set(request.batchId, updatedPlants);
    }

    // Store loss record
    const existing = mockLossRecords.get(request.batchId) || [];
    mockLossRecords.set(request.batchId, [...existing, lossRecord]);

    return lossRecord;
  }

  /**
   * Transition plants to next phase
   */
  static async transitionPhase(request: TransitionPhaseRequest): Promise<void> {
    const { batchId, plantBatchId, plantIds, toPhase, quantity, destinationRoomId } = request;

    // Update plant batch phase
    if (plantBatchId) {
      const batches = mockPlantBatches.get(batchId) || [];
      const updatedBatches = batches.map((pb) => {
        if (pb.id === plantBatchId) {
          return {
            ...pb,
            growthPhase: toPhase,
            roomId: destinationRoomId,
            updatedAt: new Date().toISOString(),
          };
        }
        return pb;
      });
      mockPlantBatches.set(batchId, updatedBatches);
    }

    // Update individual plants
    if (plantIds && plantIds.length > 0) {
      const plants = mockPlants.get(batchId) || [];
      const updatedPlants = plants.map((p) => {
        if (plantIds.includes(p.id)) {
          const updates: Partial<Plant> = {
            growthPhase: toPhase,
            roomId: destinationRoomId,
            updatedAt: new Date().toISOString(),
          };
          
          if (toPhase === 'vegetative' && !p.vegetativeDate) {
            updates.vegetativeDate = new Date().toISOString().split('T')[0];
          }
          if (toPhase === 'flowering' && !p.floweringDate) {
            updates.floweringDate = new Date().toISOString().split('T')[0];
          }
          
          return { ...p, ...updates };
        }
        return p;
      });
      mockPlants.set(batchId, updatedPlants);
    }
  }

  /**
   * Assign METRC tags to create individual plants from immature batch
   */
  static async assignTags(request: AssignTagsRequest): Promise<Plant[]> {
    const { batchId, plantBatchId, tagStart, tagEnd, quantity, roomId } = request;

    // Get the plant batch
    const batches = mockPlantBatches.get(batchId) || [];
    const plantBatch = batches.find((pb) => pb.id === plantBatchId);

    if (!plantBatch) {
      throw new Error('Plant batch not found');
    }

    // Generate tag range
    const tags = generateTagRange(tagStart, tagEnd, quantity);

    // Create individual plants
    const newPlants: Plant[] = tags.map((tag) => ({
      id: `plant-${Date.now()}-${tag}`,
      siteId: 'site-1',
      batchId,
      plantTag: tag,
      strainId: plantBatch.strainId,
      strainName: plantBatch.strainName,
      growthPhase: 'vegetative',
      status: 'active',
      plantedDate: plantBatch.plantedDate,
      vegetativeDate: new Date().toISOString().split('T')[0],
      roomId,
      metrcSyncStatus: 'pending',
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      createdByUserId: 'current-user',
      updatedByUserId: 'current-user',
    }));

    // Update plant batch
    const updatedBatches = batches.map((pb) => {
      if (pb.id === plantBatchId) {
        return {
          ...pb,
          currentCount: pb.currentCount - quantity,
          taggedCount: pb.taggedCount + quantity,
          updatedAt: new Date().toISOString(),
        };
      }
      return pb;
    });
    mockPlantBatches.set(batchId, updatedBatches);

    // Store new plants
    const existing = mockPlants.get(batchId) || [];
    mockPlants.set(batchId, [...existing, ...newPlants]);

    return newPlants;
  }

  /**
   * Check if batch has any plant records (started)
   */
  static async isBatchStarted(batchId: string): Promise<boolean> {
    const batches = mockPlantBatches.get(batchId) || [];
    return batches.length > 0;
  }

  /**
   * Check if all plants are tagged (required before flowering)
   */
  static async areAllPlantsTagged(batchId: string): Promise<boolean> {
    const counts = await this.getPlantCounts(batchId);
    return counts.untagged === 0 && counts.current > 0;
  }
}

// =============================================================================
// HELPERS
// =============================================================================

/**
 * Generate a range of METRC tags
 */
function generateTagRange(start: string, end: string, quantity: number): string[] {
  // Simple implementation - in production would parse METRC tag format
  const tags: string[] = [];
  const prefix = start.replace(/\d+$/, '');
  const startNum = parseInt(start.match(/\d+$/)?.[0] || '0', 10);

  for (let i = 0; i < quantity; i++) {
    tags.push(`${prefix}${String(startNum + i).padStart(10, '0')}`);
  }

  return tags;
}

export default PlantService;





