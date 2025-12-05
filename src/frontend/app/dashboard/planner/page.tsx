'use client';

import React, { useEffect, useState, useCallback, useMemo } from 'react';
import { addDays, startOfMonth, endOfMonth } from 'date-fns';
import { 
  GanttChart, 
  GanttRow, 
  PlannerToolbar, 
  BatchDetailPanel,
  ImpactPanel,
  CapacityLane,
} from '@/features/planner/components';
import { 
  usePlannerStore,
  usePlannerKeyboard,
  useImpactAnalysis,
  useCapacityCalculation,
} from '@/features/planner';
import { 
  PlannedBatch, 
  BatchPhase, 
  Room, 
  PhaseType,
  PlannerConflict 
} from '@/features/planner/types';
import { detectAllConflicts } from '@/features/planner/utils';
import { PHASE_ORDER } from '@/features/planner/constants';
import '@/features/planner/styles/planner-animations.css';

// Mock data for demonstration
const MOCK_ROOMS: Room[] = [
  { id: 'room-1', name: 'Veg Room A', code: 'VEG-A', roomClass: 'veg', maxCapacity: 500, siteId: 'site-1' },
  { id: 'room-2', name: 'Veg Room B', code: 'VEG-B', roomClass: 'veg', maxCapacity: 400, siteId: 'site-1' },
  { id: 'room-3', name: 'Flower Room 1', code: 'FLR-1', roomClass: 'flower', maxCapacity: 600, siteId: 'site-1' },
  { id: 'room-4', name: 'Flower Room 2', code: 'FLR-2', roomClass: 'flower', maxCapacity: 600, siteId: 'site-1' },
  { id: 'room-5', name: 'Dry Room', code: 'DRY-1', roomClass: 'drying', maxCapacity: 300, siteId: 'site-1' },
  { id: 'room-6', name: 'Cure Vault', code: 'CURE-1', roomClass: 'cure', maxCapacity: 400, siteId: 'site-1' },
];

function generateMockPhases(startDate: Date, genetics: { defaultVegDays: number; defaultFlowerDays: number; defaultCureDays?: number }): BatchPhase[] {
  let currentDate = startDate;
  const phases: BatchPhase[] = [];

  // Clone phase (14 days)
  phases.push({
    id: `phase-${Date.now()}-clone`,
    phase: 'clone',
    plannedStart: currentDate,
    plannedEnd: addDays(currentDate, 13),
    roomId: 'room-1',
    roomName: 'Veg Room A',
  });
  currentDate = addDays(currentDate, 14);

  // Veg phase
  phases.push({
    id: `phase-${Date.now()}-veg`,
    phase: 'veg',
    plannedStart: currentDate,
    plannedEnd: addDays(currentDate, genetics.defaultVegDays - 1),
    roomId: 'room-1',
    roomName: 'Veg Room A',
  });
  currentDate = addDays(currentDate, genetics.defaultVegDays);

  // Flower phase
  phases.push({
    id: `phase-${Date.now()}-flower`,
    phase: 'flower',
    plannedStart: currentDate,
    plannedEnd: addDays(currentDate, genetics.defaultFlowerDays - 1),
    roomId: 'room-3',
    roomName: 'Flower Room 1',
  });
  currentDate = addDays(currentDate, genetics.defaultFlowerDays);

  // Harvest phase (3 days)
  phases.push({
    id: `phase-${Date.now()}-harvest`,
    phase: 'harvest',
    plannedStart: currentDate,
    plannedEnd: addDays(currentDate, 2),
    roomId: 'room-5',
    roomName: 'Dry Room',
  });
  currentDate = addDays(currentDate, 3);

  // Cure phase
  const cureDays = genetics.defaultCureDays || 14;
  phases.push({
    id: `phase-${Date.now()}-cure`,
    phase: 'cure',
    plannedStart: currentDate,
    plannedEnd: addDays(currentDate, cureDays - 1),
    roomId: 'room-6',
    roomName: 'Cure Vault',
  });

  return phases;
}

const MOCK_BATCHES: PlannedBatch[] = [
  {
    id: 'batch-1',
    name: 'OG Kush Run #203',
    code: 'B-203-OGK',
    strain: 'OG Kush',
    genetics: { id: 'gen-1', name: 'OG Kush', defaultVegDays: 21, defaultFlowerDays: 56, defaultCureDays: 14 },
    plantCount: 450,
    phases: generateMockPhases(addDays(new Date(), -30), { defaultVegDays: 21, defaultFlowerDays: 56, defaultCureDays: 14 }),
    status: 'active',
    createdAt: new Date(),
    updatedAt: new Date(),
  },
  {
    id: 'batch-2',
    name: 'Blue Dream #204',
    code: 'B-204-BDR',
    strain: 'Blue Dream',
    genetics: { id: 'gen-2', name: 'Blue Dream', defaultVegDays: 28, defaultFlowerDays: 63, defaultCureDays: 14 },
    plantCount: 320,
    phases: generateMockPhases(addDays(new Date(), -10), { defaultVegDays: 28, defaultFlowerDays: 63, defaultCureDays: 14 }),
    status: 'active',
    createdAt: new Date(),
    updatedAt: new Date(),
  },
  {
    id: 'batch-3',
    name: 'GSC #205',
    code: 'B-205-GSC',
    strain: 'Girl Scout Cookies',
    genetics: { id: 'gen-3', name: 'Girl Scout Cookies', defaultVegDays: 24, defaultFlowerDays: 60, defaultCureDays: 14 },
    plantCount: 500,
    phases: generateMockPhases(addDays(new Date(), 5), { defaultVegDays: 24, defaultFlowerDays: 60, defaultCureDays: 14 }),
    status: 'planned',
    createdAt: new Date(),
    updatedAt: new Date(),
  },
  {
    id: 'batch-4',
    name: 'Gorilla Glue #206',
    code: 'B-206-GG',
    strain: 'Gorilla Glue #4',
    genetics: { id: 'gen-4', name: 'Gorilla Glue #4', defaultVegDays: 21, defaultFlowerDays: 58, defaultCureDays: 14 },
    plantCount: 380,
    phases: generateMockPhases(addDays(new Date(), 20), { defaultVegDays: 21, defaultFlowerDays: 58, defaultCureDays: 14 }),
    status: 'planned',
    createdAt: new Date(),
    updatedAt: new Date(),
  },
  {
    id: 'batch-5',
    name: 'Wedding Cake #207',
    code: 'B-207-WC',
    strain: 'Wedding Cake',
    genetics: { id: 'gen-5', name: 'Wedding Cake', defaultVegDays: 25, defaultFlowerDays: 62, defaultCureDays: 14 },
    plantCount: 420,
    phases: generateMockPhases(addDays(new Date(), 35), { defaultVegDays: 25, defaultFlowerDays: 62, defaultCureDays: 14 }),
    status: 'planned',
    createdAt: new Date(),
    updatedAt: new Date(),
  },
];

export default function PlannerPage() {
  const [isDetailPanelOpen, setIsDetailPanelOpen] = useState(false);
  const [showNewBatchModal, setShowNewBatchModal] = useState(false);

  // Store
  const {
    batches,
    rooms,
    dateRange,
    settings,
    selectedBatchId,
    selectedPhaseId,
    dragState,
    conflicts,
    setBatches,
    setRooms,
    selectBatch,
    selectPhase,
    deleteBatch,
    duplicateBatch,
    recalculateConflicts,
  } = usePlannerStore();

  // Hooks
  usePlannerKeyboard();
  const { currentDragImpact } = useImpactAnalysis();
  const { roomCapacities, hasCapacityIssues } = useCapacityCalculation();

  // Initialize with mock data
  useEffect(() => {
    if (batches.length === 0) {
      setBatches(MOCK_BATCHES);
    }
    if (rooms.length === 0) {
      setRooms(MOCK_ROOMS);
    }
  }, [batches.length, rooms.length, setBatches, setRooms]);

  // Recalculate conflicts when batches change
  useEffect(() => {
    recalculateConflicts();
  }, [batches, recalculateConflicts]);

  // Get selected batch
  const selectedBatch = useMemo(
    () => batches.find((b) => b.id === selectedBatchId) || null,
    [batches, selectedBatchId]
  );

  // Calculate all conflicts
  const allConflicts = useMemo(
    () => detectAllConflicts(batches, rooms, roomCapacities),
    [batches, rooms, roomCapacities]
  );

  // Handlers
  const handleSelectBatch = useCallback((batchId: string) => {
    selectBatch(batchId);
    setIsDetailPanelOpen(true);
  }, [selectBatch]);

  const handleSelectPhase = useCallback((batchId: string, phaseId: string) => {
    selectPhase(batchId, phaseId);
    setIsDetailPanelOpen(true);
  }, [selectPhase]);

  const handleEditPhase = useCallback((batchId: string, phaseId: string) => {
    selectPhase(batchId, phaseId);
    // Open phase edit modal (to be implemented)
    console.log('Edit phase:', batchId, phaseId);
  }, [selectPhase]);

  const handleCloseDetailPanel = useCallback(() => {
    setIsDetailPanelOpen(false);
  }, []);

  const handleNewBatch = useCallback(() => {
    setShowNewBatchModal(true);
  }, []);

  const handleDeleteBatch = useCallback((batch: PlannedBatch) => {
    if (confirm(`Are you sure you want to delete ${batch.name}?`)) {
      deleteBatch(batch.id);
      setIsDetailPanelOpen(false);
    }
  }, [deleteBatch]);

  const handleDuplicateBatch = useCallback((batch: PlannedBatch) => {
    duplicateBatch(batch.id);
  }, [duplicateBatch]);

  return (
    <div className="flex flex-col h-screen bg-background">
      {/* Page Header */}
      <div className="flex-shrink-0 px-6 py-4 border-b border-border">
        <h1 className="text-2xl font-semibold text-foreground">Batch Planner</h1>
        <p className="text-sm text-muted-foreground mt-1">
          Plan and visualize your cultivation batches across the lifecycle
        </p>
      </div>

      {/* Toolbar */}
      <PlannerToolbar 
        onNewBatch={handleNewBatch}
        conflictCount={allConflicts.length}
      />

      {/* Main Content */}
      <div className="flex-1 flex overflow-hidden">
        {/* Gantt Chart */}
        <div className="flex-1 overflow-hidden p-4">
          <GanttChart className="h-full">
            {batches.map((batch, index) => (
              <GanttRow
                key={batch.id}
                batch={batch}
                rowIndex={index}
                dateRange={dateRange}
                zoomLevel={settings.zoomLevel}
                conflicts={allConflicts}
                selectedBatchId={selectedBatchId}
                selectedPhaseId={selectedPhaseId}
                onSelectBatch={handleSelectBatch}
                onSelectPhase={handleSelectPhase}
                onEditPhase={handleEditPhase}
              />
            ))}
          </GanttChart>
        </div>

        {/* Detail Panel */}
        <BatchDetailPanel
          batch={selectedBatch}
          conflicts={allConflicts}
          isOpen={isDetailPanelOpen && !!selectedBatch}
          onClose={handleCloseDetailPanel}
          onEdit={() => console.log('Edit batch')}
          onDuplicate={handleDuplicateBatch}
          onDelete={handleDeleteBatch}
          onEditPhase={handleEditPhase}
        />
      </div>

      {/* Impact Panel (shown during drag) */}
      {dragState.isDragging && currentDragImpact && (
        <ImpactPanel
          impact={currentDragImpact.impact}
          conflicts={currentDragImpact.conflicts}
          isVisible={true}
          onConfirm={() => {}}
          onCancel={() => {}}
        />
      )}

      {/* What-If Mode Indicator */}
      {settings.whatIfMode && (
        <div className="fixed bottom-4 left-4 flex items-center gap-2 px-4 py-2 bg-violet-500/20 border border-violet-500/30 rounded-lg text-violet-400 text-sm font-medium shadow-lg">
          <div className="w-2 h-2 rounded-full bg-violet-500 animate-pulse" />
          What-If Mode Active
          <span className="text-violet-400/70 text-xs ml-2">
            Changes are not saved
          </span>
        </div>
      )}

      {/* Keyboard Shortcuts Hint */}
      <div className="fixed bottom-4 left-1/2 -translate-x-1/2 text-xs text-muted-foreground/60">
        Press <kbd className="px-1.5 py-0.5 bg-muted rounded text-muted-foreground mx-0.5">?</kbd> for keyboard shortcuts
      </div>
    </div>
  );
}

