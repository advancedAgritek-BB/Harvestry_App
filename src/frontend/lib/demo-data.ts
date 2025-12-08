import { PlannedBatch } from '@/features/planner/types/planner.types';
import { InventoryLot } from '@/features/inventory/types/lot.types';
import { Task } from '@/features/tasks/types/task.types';

// ============================================================================
// MOCK BATCHES (Planner)
// ============================================================================

export const MOCK_BATCHES: PlannedBatch[] = [
  {
    id: 'batch-001',
    name: 'Blue Dream - B1',
    code: 'B-001-BD',
    strain: 'Blue Dream',
    genetics: {
      id: 'gen-bd',
      name: 'Blue Dream',
      defaultVegDays: 21,
      defaultFlowerDays: 63,
    },
    plantCount: 150,
    status: 'active',
    createdAt: new Date('2023-10-01'),
    updatedAt: new Date(),
    phases: [
      {
        id: 'p1-clone',
        phase: 'clone',
        plannedStart: new Date('2023-10-01'),
        plannedEnd: new Date('2023-10-14'),
        actualStart: new Date('2023-10-01'),
        actualEnd: new Date('2023-10-14'),
        roomId: 'room-clone-1',
        roomName: 'Clone Room 1'
      },
      {
        id: 'p1-veg',
        phase: 'veg',
        plannedStart: new Date('2023-10-15'),
        plannedEnd: new Date('2023-11-05'),
        actualStart: new Date('2023-10-15'),
        actualEnd: new Date('2023-11-05'),
        roomId: 'room-veg-1',
        roomName: 'Veg Room 1'
      },
      {
        id: 'p1-flower',
        phase: 'flower',
        plannedStart: new Date('2023-11-06'),
        plannedEnd: new Date('2024-01-07'),
        actualStart: new Date('2023-11-06'),
        roomId: 'room-flower-1',
        roomName: 'Flower Room 1'
      },
      {
        id: 'p1-harvest',
        phase: 'harvest',
        plannedStart: new Date('2024-01-08'),
        plannedEnd: new Date('2024-01-10'),
        roomId: 'room-dry-1',
        roomName: 'Dry Room 1'
      },
      {
        id: 'p1-cure',
        phase: 'cure',
        plannedStart: new Date('2024-01-11'),
        plannedEnd: new Date('2024-01-25'),
        roomId: 'room-cure-1',
        roomName: 'Cure Room 1'
      }
    ]
  },
  {
    id: 'batch-002',
    name: 'OG Kush - B2',
    code: 'B-002-OG',
    strain: 'OG Kush',
    genetics: {
      id: 'gen-og',
      name: 'OG Kush',
      defaultVegDays: 28,
      defaultFlowerDays: 70,
    },
    plantCount: 200,
    status: 'active',
    createdAt: new Date('2023-11-01'),
    updatedAt: new Date(),
    phases: [
      {
        id: 'p2-clone',
        phase: 'clone',
        plannedStart: new Date('2023-11-01'),
        plannedEnd: new Date('2023-11-14'),
        actualStart: new Date('2023-11-01'),
        actualEnd: new Date('2023-11-14'),
        roomId: 'room-clone-1',
        roomName: 'Clone Room 1'
      },
      {
        id: 'p2-veg',
        phase: 'veg',
        plannedStart: new Date('2023-11-15'),
        plannedEnd: new Date('2023-12-13'),
        actualStart: new Date('2023-11-15'),
        roomId: 'room-veg-2',
        roomName: 'Veg Room 2'
      },
      {
        id: 'p2-flower',
        phase: 'flower',
        plannedStart: new Date('2023-12-14'),
        plannedEnd: new Date('2024-02-21'),
        roomId: 'room-flower-2',
        roomName: 'Flower Room 2'
      }
    ]
  },
  {
    id: 'batch-003',
    name: 'Gelato - B3',
    code: 'B-003-GEL',
    strain: 'Gelato',
    genetics: {
      id: 'gen-gel',
      name: 'Gelato',
      defaultVegDays: 21,
      defaultFlowerDays: 60,
    },
    plantCount: 300,
    status: 'active',
    createdAt: new Date('2023-11-20'),
    updatedAt: new Date(),
    phases: [
      {
        id: 'p3-clone',
        phase: 'clone',
        plannedStart: new Date('2023-11-20'),
        plannedEnd: new Date('2023-12-04'),
        actualStart: new Date('2023-11-20'),
        roomId: 'room-clone-2',
        roomName: 'Clone Room 2'
      }
    ]
  }
];

// ============================================================================
// MOCK TASKS
// ============================================================================

export const MOCK_TASKS: Task[] = [
  {
    id: 'task-1',
    title: 'Inspect Mother Room A for PM',
    description: 'Weekly scout for powdery mildew and mites.',
    category: 'general',
    location: 'Room A (Mothers)',
    dueAt: new Date(Date.now() - 2 * 60 * 60 * 1000).toISOString(), // 2 hours ago
    priority: 'critical',
    status: 'in_progress',
    slaStatus: 'breached',
    assignee: { id: 'u1', firstName: 'Marcus', lastName: 'Johnson', role: 'Grower' },
  },
  {
    id: 'task-2',
    title: 'Transplant Batch B-203',
    description: 'Move from 1gal to 5gal pots.',
    category: 'application',
    location: 'Veg Room 2',
    dueAt: new Date(Date.now() + 4 * 60 * 60 * 1000).toISOString(), // In 4 hours
    priority: 'high',
    status: 'ready',
    slaStatus: 'warning',
    assignee: { id: 'u2', firstName: 'Sarah', lastName: 'Chen', role: 'Lead Grower' },
  },
  {
    id: 'task-3',
    title: 'Calibrate pH Sensors',
    description: 'Recalibrate sensors in Zone 1 irrigation controller.',
    category: 'general',
    location: 'Irrigation Zone 1',
    dueAt: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString(), // Tomorrow
    priority: 'normal',
    status: 'ready',
    slaStatus: 'ok',
  },
  {
    id: 'task-4',
    title: 'Refill Nutrient Tanks',
    description: 'Top off Part A and Part B tanks.',
    category: 'application',
    location: 'Nutrient Room',
    dueAt: new Date(Date.now() + 5 * 60 * 60 * 1000).toISOString(), // In 5 hours
    priority: 'normal',
    status: 'ready',
    slaStatus: 'ok',
    assignee: { id: 'u3', firstName: 'David', lastName: 'Martinez', role: 'Technician' },
  },
  {
    id: 'task-5',
    title: 'Harvest Prep - Room 4',
    description: 'Clean and sanitize dry room before harvest.',
    category: 'harvest',
    location: 'Dry Room 1',
    dueAt: new Date(Date.now() + 48 * 60 * 60 * 1000).toISOString(), // In 2 days
    priority: 'high',
    status: 'ready',
    slaStatus: 'ok',
  }
];

// ============================================================================
// MOCK LOTS (Inventory)
// ============================================================================

export const MOCK_LOTS: InventoryLot[] = [
  {
    id: 'lot-001',
    siteId: 'site-1',
    lotNumber: 'L-231120-001',
    barcode: '1A406030000213',
    productType: 'flower',
    strainId: 'gen-bd',
    strainName: 'Blue Dream',
    originType: 'harvest',
    quantity: 4500,
    uom: 'g',
    originalQuantity: 5000,
    reservedQuantity: 0,
    availableQuantity: 4500,
    locationId: 'loc-vault',
    locationPath: 'Main Vault / Shelf A',
    status: 'available',
    createdAt: new Date().toISOString(),
    createdBy: 'system',
    updatedAt: new Date().toISOString(),
    updatedBy: 'system',
    parentLotIds: [],
    parentRelationships: [],
    childLotIds: [],
    ancestryChain: [],
    generationDepth: 0,
    materialCost: 0,
    laborCost: 0,
    overheadCost: 0,
    totalCost: 0,
    unitCost: 0,
    syncStatus: 'synced',
    coaStatus: 'passed'
  },
  {
    id: 'lot-002',
    siteId: 'site-1',
    lotNumber: 'L-231122-005',
    barcode: '1A406030000218',
    productType: 'trim',
    strainId: 'gen-og',
    strainName: 'OG Kush',
    originType: 'harvest',
    quantity: 12000,
    uom: 'g',
    originalQuantity: 12000,
    reservedQuantity: 5000,
    availableQuantity: 7000,
    locationId: 'loc-freezer',
    locationPath: 'Processing / Freezer 1',
    status: 'allocated',
    createdAt: new Date().toISOString(),
    createdBy: 'system',
    updatedAt: new Date().toISOString(),
    updatedBy: 'system',
    parentLotIds: [],
    parentRelationships: [],
    childLotIds: [],
    ancestryChain: [],
    generationDepth: 0,
    materialCost: 0,
    laborCost: 0,
    overheadCost: 0,
    totalCost: 0,
    unitCost: 0,
    syncStatus: 'pending',
    coaStatus: 'not_required'
  }
];
