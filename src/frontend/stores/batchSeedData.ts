/**
 * Batch Seed Data
 * 
 * Initial mock data for the shared batch store.
 * This data is used by both inventory and planner features.
 */

import { addDays, subDays, format } from 'date-fns';
import type { CultivationBatch } from '@/features/inventory/types/batch.types';

const now = new Date();
const today = format(now, "yyyy-MM-dd'T'HH:mm:ss'Z'");

/**
 * Generate a batch with realistic dates and data
 */
function createBatch(config: {
  id: string;
  batchNumber: string;
  name: string;
  strainId: string;
  strainName: string;
  geneticId: string;
  geneticName: string;
  originType: 'seed' | 'clone' | 'mother_cutting' | 'tissue_culture';
  initialPlantCount: number;
  currentPlantCount: number;
  currentPhase: CultivationBatch['currentPhase'];
  status: CultivationBatch['status'];
  startDaysAgo: number;
  expectedHarvestDaysFromNow: number;
  currentRoomId: string;
  currentRoomName: string;
  generationNumber?: number;
}): CultivationBatch {
  const startDate = subDays(now, config.startDaysAgo);
  const expectedHarvest = addDays(now, config.expectedHarvestDaysFromNow);
  const totalPlantsLost = config.initialPlantCount - config.currentPlantCount;
  const survivalRate = Math.round((config.currentPlantCount / config.initialPlantCount) * 100);

  return {
    id: config.id,
    siteId: 'site-1',
    batchNumber: config.batchNumber,
    name: config.name,
    originType: config.originType,
    geneticId: config.geneticId,
    geneticName: config.geneticName,
    strainId: config.strainId,
    strainName: config.strainName,
    generationNumber: config.generationNumber ?? 1,
    currentPhase: config.currentPhase,
    phaseHistory: [],
    status: config.status,
    initialPlantCount: config.initialPlantCount,
    currentPlantCount: config.currentPlantCount,
    plantLossEvents: [],
    totalPlantsLost,
    survivalRate,
    currentRoomId: config.currentRoomId,
    currentRoomName: config.currentRoomName,
    locationHistory: [],
    startDate: startDate.toISOString(),
    expectedHarvestDate: expectedHarvest.toISOString(),
    expectedDays: config.startDaysAgo + config.expectedHarvestDaysFromNow,
    projectedYieldGrams: config.currentPlantCount * 200, // ~200g per plant estimate
    costs: {
      seedCloneCost: config.initialPlantCount * 8,
      nutrientCost: 250,
      laborCost: 800,
      utilityCost: 600,
      facilityCost: 300,
      equipmentCost: 100,
      overheadCost: 200,
      totalDirectCost: config.initialPlantCount * 8 + 1050,
      totalIndirectCost: 1200,
      totalCost: config.initialPlantCount * 8 + 2250,
      costAllocations: [],
    },
    costPerPlant: Math.round((config.initialPlantCount * 8 + 2250) / config.currentPlantCount * 100) / 100,
    isCompliant: true,
    harvestEventIds: [],
    outputLotIds: [],
    createdAt: startDate.toISOString(),
    createdBy: 'admin',
    updatedAt: today,
    updatedBy: 'system',
  };
}

/**
 * Initial batch data - shared across inventory and planner
 */
export const SEED_BATCHES: CultivationBatch[] = [
  // Active batch in flowering - uses Flower Room 1
  createBatch({
    id: 'batch-001',
    batchNumber: 'BATCH-2025-001',
    name: 'Blue Dream #1',
    strainId: 'strain-001',
    strainName: 'Blue Dream',
    geneticId: 'gen-001',
    geneticName: 'Blue Dream',
    originType: 'clone',
    initialPlantCount: 200,
    currentPlantCount: 195,
    currentPhase: 'flowering',
    status: 'active',
    startDaysAgo: 60,
    expectedHarvestDaysFromNow: 25,
    currentRoomId: 'room-3',
    currentRoomName: 'Flower Room 1',
    generationNumber: 3,
  }),

  // Active batch in vegetative - uses Veg Room B
  createBatch({
    id: 'batch-002',
    batchNumber: 'BATCH-2025-002',
    name: 'OG Kush #1',
    strainId: 'strain-002',
    strainName: 'OG Kush',
    geneticId: 'gen-002',
    geneticName: 'OG Kush',
    originType: 'seed',
    initialPlantCount: 180,
    currentPlantCount: 172,
    currentPhase: 'vegetative',
    status: 'active',
    startDaysAgo: 35,
    expectedHarvestDaysFromNow: 65,
    currentRoomId: 'room-2',
    currentRoomName: 'Veg Room B',
    generationNumber: 1,
  }),

  // Batch in drying phase
  createBatch({
    id: 'batch-003',
    batchNumber: 'BATCH-2025-003',
    name: 'Gelato #1',
    strainId: 'strain-003',
    strainName: 'Gelato',
    geneticId: 'gen-003',
    geneticName: 'Gelato',
    originType: 'clone',
    initialPlantCount: 150,
    currentPlantCount: 145,
    currentPhase: 'drying',
    status: 'harvested',
    startDaysAgo: 95,
    expectedHarvestDaysFromNow: -5, // Already past harvest
    currentRoomId: 'room-5',
    currentRoomName: 'Dry Room',
    generationNumber: 2,
  }),

  // Planned batch - uses Veg Room A, may cause conflict
  createBatch({
    id: 'batch-004',
    batchNumber: 'BATCH-2025-004',
    name: 'Gorilla Glue #4',
    strainId: 'strain-004',
    strainName: 'Gorilla Glue #4',
    geneticId: 'gen-004',
    geneticName: 'Gorilla Glue #4',
    originType: 'clone',
    initialPlantCount: 350,
    currentPlantCount: 350,
    currentPhase: 'propagation',
    status: 'planned',
    startDaysAgo: -30, // Starts 30 days from now
    expectedHarvestDaysFromNow: 90,
    currentRoomId: 'room-1',
    currentRoomName: 'Veg Room A',
    generationNumber: 2,
  }),

  // Planned batch - INTENTIONAL CONFLICT with batch-004 in Veg Room A
  createBatch({
    id: 'batch-005',
    batchNumber: 'BATCH-2025-005',
    name: 'Wedding Cake #1',
    strainId: 'strain-005',
    strainName: 'Wedding Cake',
    geneticId: 'gen-005',
    geneticName: 'Wedding Cake',
    originType: 'clone',
    initialPlantCount: 250,
    currentPlantCount: 250,
    currentPhase: 'propagation',
    status: 'planned',
    startDaysAgo: -35, // Starts 35 days from now, overlaps with batch-004
    expectedHarvestDaysFromNow: 95,
    currentRoomId: 'room-1',
    currentRoomName: 'Veg Room A',
    generationNumber: 1,
  }),

  // Another planned batch - uses Veg Room A, later start
  createBatch({
    id: 'batch-006',
    batchNumber: 'BATCH-2025-006',
    name: 'GSC #1',
    strainId: 'strain-006',
    strainName: 'Girl Scout Cookies',
    geneticId: 'gen-006',
    geneticName: 'Girl Scout Cookies',
    originType: 'clone',
    initialPlantCount: 150,
    currentPlantCount: 150,
    currentPhase: 'propagation',
    status: 'planned',
    startDaysAgo: -60, // Starts 60 days from now
    expectedHarvestDaysFromNow: 120,
    currentRoomId: 'room-1',
    currentRoomName: 'Veg Room A',
    generationNumber: 1,
  }),
];

/**
 * Genetics data for the batches (used for phase duration calculations)
 */
export const GENETICS_MAP = new Map<string, { vegDays: number; flowerDays: number; cureDays: number }>([
  ['gen-001', { vegDays: 28, flowerDays: 63, cureDays: 14 }],  // Blue Dream
  ['gen-002', { vegDays: 21, flowerDays: 56, cureDays: 14 }],  // OG Kush
  ['gen-003', { vegDays: 24, flowerDays: 58, cureDays: 14 }],  // Gelato
  ['gen-004', { vegDays: 21, flowerDays: 58, cureDays: 14 }],  // Gorilla Glue
  ['gen-005', { vegDays: 25, flowerDays: 62, cureDays: 14 }],  // Wedding Cake
  ['gen-006', { vegDays: 24, flowerDays: 60, cureDays: 14 }],  // GSC
]);



