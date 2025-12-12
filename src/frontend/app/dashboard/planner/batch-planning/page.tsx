'use client';

import React, { useEffect, useState, useCallback, useMemo } from 'react';
import {
  GanttChart,
  GanttRowDraggable,
  PlannerToolbar,
  BatchDetailPanel,
  ImpactPanel,
  NewBatchModal,
} from '@/features/planner/components';
import {
  usePlannerStore,
  usePlannerKeyboard,
  useImpactAnalysis,
  useCapacityCalculation,
} from '@/features/planner';
import { PlannedBatch, Room } from '@/features/planner/types';
import { detectAllConflicts, cultivationBatchesToPlannedBatches } from '@/features/planner/utils';
import { useBatchStore } from '@/stores/batchStore';
import { SEED_BATCHES, GENETICS_MAP } from '@/stores/batchSeedData';
import '@/features/planner/styles/planner-animations.css';

const ROOMS: Room[] = [
  { id: 'room-1', name: 'Veg Room A', code: 'VEG-A', roomClass: 'veg', maxCapacity: 500, siteId: 'site-1' },
  { id: 'room-2', name: 'Veg Room B', code: 'VEG-B', roomClass: 'veg', maxCapacity: 400, siteId: 'site-1' },
  { id: 'room-3', name: 'Flower Room 1', code: 'FLR-1', roomClass: 'flower', maxCapacity: 600, siteId: 'site-1' },
  { id: 'room-4', name: 'Flower Room 2', code: 'FLR-2', roomClass: 'flower', maxCapacity: 600, siteId: 'site-1' },
  { id: 'room-5', name: 'Dry Room', code: 'DRY-1', roomClass: 'drying', maxCapacity: 300, siteId: 'site-1' },
  { id: 'room-6', name: 'Cure Vault', code: 'CURE-1', roomClass: 'cure', maxCapacity: 400, siteId: 'site-1' },
];

export default function BatchPlanningPage() {
  const [isDetailPanelOpen, setIsDetailPanelOpen] = useState(false);
  const [showNewBatchModal, setShowNewBatchModal] = useState(false);

  const {
    batches: cultivationBatches,
    setBatches: setCultivationBatches,
    addBatch: addSharedBatch,
    deleteBatch: deleteSharedBatch,
    duplicateBatch: duplicateSharedBatch,
    splitBatch: splitSharedBatch,
    shiftSchedule: shiftSharedSchedule,
    updatePlantCount: updateSharedPlantCount,
    updateBatchRoom: updateSharedBatchRoom,
  } = useBatchStore();

  const {
    rooms,
    dateRange,
    settings,
    selectedBatchId,
    selectedPhaseId,
    dragState,
    setBatches: setPlannerBatches,
    setRooms,
    selectBatch,
    selectPhase,
    recalculateConflicts,
    dismissConflict,
  } = usePlannerStore();

  usePlannerKeyboard();
  const { currentDragImpact } = useImpactAnalysis();
  const { roomCapacities } = useCapacityCalculation();

  const plannerBatches = useMemo(() => {
    return cultivationBatchesToPlannedBatches(cultivationBatches, GENETICS_MAP);
  }, [cultivationBatches]);

  useEffect(() => {
    if (cultivationBatches.length === 0) {
      setCultivationBatches(SEED_BATCHES);
    }
  }, [cultivationBatches.length, setCultivationBatches]);

  useEffect(() => {
    setPlannerBatches(plannerBatches);
  }, [plannerBatches, setPlannerBatches]);

  useEffect(() => {
    if (rooms.length === 0) {
      setRooms(ROOMS);
    }
  }, [rooms.length, setRooms]);

  useEffect(() => {
    recalculateConflicts();
  }, [plannerBatches, recalculateConflicts]);

  const selectedBatch = useMemo(
    () => plannerBatches.find((b) => b.id === selectedBatchId) || null,
    [plannerBatches, selectedBatchId]
  );

  const allConflicts = useMemo(
    () => detectAllConflicts(plannerBatches, rooms, roomCapacities),
    [plannerBatches, rooms, roomCapacities]
  );

  const handleSelectBatch = useCallback(
    (batchId: string) => {
      selectBatch(batchId);
      setIsDetailPanelOpen(true);
    },
    [selectBatch]
  );

  const handleSelectPhase = useCallback(
    (batchId: string, phaseId: string) => {
      selectPhase(batchId, phaseId);
      setIsDetailPanelOpen(true);
    },
    [selectPhase]
  );

  const handleEditPhase = useCallback(
    (batchId: string, phaseId: string) => {
      selectPhase(batchId, phaseId);
      console.log('Edit phase:', batchId, phaseId);
    },
    [selectPhase]
  );

  const handleMovePhase = useCallback(
    (batchId: string, phaseId: string, newStart: Date) => {
      const batch = plannerBatches.find((b) => b.id === batchId);
      if (!batch) return;

      const phase = batch.phases.find((p) => p.id === phaseId);
      if (!phase) return;

      const originalStart = phase.plannedStart;
      const deltaDays = Math.round((newStart.getTime() - originalStart.getTime()) / (1000 * 60 * 60 * 24));

      if (deltaDays !== 0) {
        shiftSharedSchedule(batchId, deltaDays);
      }
    },
    [plannerBatches, shiftSharedSchedule]
  );

  const handleResizePhase = useCallback(
    (batchId: string, phaseId: string, newStart: Date, _newEnd: Date) => {
      const batch = plannerBatches.find((b) => b.id === batchId);
      if (!batch) return;

      const phase = batch.phases.find((p) => p.id === phaseId);
      if (!phase) return;

      const originalStart = phase.plannedStart;
      const deltaDays = Math.round((newStart.getTime() - originalStart.getTime()) / (1000 * 60 * 60 * 24));

      if (deltaDays !== 0) {
        shiftSharedSchedule(batchId, deltaDays);
      }
    },
    [plannerBatches, shiftSharedSchedule]
  );

  const handleCloseDetailPanel = useCallback(() => {
    setIsDetailPanelOpen(false);
  }, []);

  const handleNewBatch = useCallback(() => {
    setShowNewBatchModal(true);
  }, []);

  const handleCreateBatch = useCallback(
    (batch: PlannedBatch) => {
      const now = new Date().toISOString();
      const cultivationBatch = {
        id: batch.id,
        siteId: 'site-1',
        batchNumber: batch.code,
        name: batch.name,
        originType: 'clone' as const,
        geneticId: batch.genetics.id,
        geneticName: batch.genetics.name,
        strainId: batch.genetics.id,
        strainName: batch.strain,
        generationNumber: 1,
        currentPhase: 'propagation' as const,
        phaseHistory: [],
        status: 'planned' as const,
        initialPlantCount: batch.plantCount,
        currentPlantCount: batch.plantCount,
        plantLossEvents: [],
        totalPlantsLost: 0,
        survivalRate: 100,
        currentRoomId: batch.phases[0]?.roomId || 'room-1',
        currentRoomName: batch.phases[0]?.roomName || 'Veg Room A',
        locationHistory: [],
        startDate: batch.phases[0]?.plannedStart.toISOString() || now,
        expectedHarvestDate: batch.phases[batch.phases.length - 1]?.plannedEnd.toISOString(),
        expectedDays: 120,
        projectedYieldGrams: batch.plantCount * 200,
        costs: {
          seedCloneCost: batch.plantCount * 8,
          nutrientCost: 250,
          laborCost: 800,
          utilityCost: 600,
          facilityCost: 300,
          equipmentCost: 100,
          overheadCost: 200,
          totalDirectCost: batch.plantCount * 8 + 1050,
          totalIndirectCost: 1200,
          totalCost: batch.plantCount * 8 + 2250,
          costAllocations: [],
        },
        costPerPlant: Math.round(((batch.plantCount * 8 + 2250) / batch.plantCount) * 100) / 100,
        isCompliant: true,
        harvestEventIds: [],
        outputLotIds: [],
        createdAt: now,
        createdBy: 'system',
        updatedAt: now,
        updatedBy: 'system',
      };
      addSharedBatch(cultivationBatch);
      setShowNewBatchModal(false);
    },
    [addSharedBatch]
  );

  const handleDeleteBatch = useCallback(
    (batch: PlannedBatch) => {
      if (confirm(`Are you sure you want to delete ${batch.name}?`)) {
        deleteSharedBatch(batch.id);
        setIsDetailPanelOpen(false);
      }
    },
    [deleteSharedBatch]
  );

  const handleDuplicateBatch = useCallback(
    (batch: PlannedBatch) => {
      const newId = duplicateSharedBatch(batch.id);
      if (newId) {
        selectBatch(newId);
      }
    },
    [duplicateSharedBatch, selectBatch]
  );

  const handleResolveConflict = useCallback(
    (resolution: {
      type: 'change_room' | 'reduce_plants' | 'shift_schedule' | 'split_batch' | 'dismiss';
      conflictId: string;
      batchId: string;
      phaseId?: string;
      payload: {
        newRoomId?: string;
        newPlantCount?: number;
        daysDelta?: number;
        splitConfig?: { plantCount: number; suffix: string }[];
      };
    }) => {
      switch (resolution.type) {
        case 'change_room':
          if (resolution.payload.newRoomId) {
            const room = rooms.find((r) => r.id === resolution.payload.newRoomId);
            if (room) {
              updateSharedBatchRoom(resolution.batchId, room.id, room.name);
            }
          }
          break;
        case 'reduce_plants':
          if (resolution.payload.newPlantCount !== undefined) {
            updateSharedPlantCount(resolution.batchId, resolution.payload.newPlantCount);
          }
          break;
        case 'split_batch':
          if (resolution.payload.splitConfig && resolution.payload.splitConfig.length >= 2) {
            const newBatchIds = splitSharedBatch(resolution.batchId, resolution.payload.splitConfig);
            if (newBatchIds.length > 0) {
              selectBatch(newBatchIds[0]);
            }
          }
          break;
        case 'shift_schedule':
          if (resolution.payload.daysDelta !== undefined && resolution.payload.daysDelta !== 0) {
            shiftSharedSchedule(resolution.batchId, resolution.payload.daysDelta);
          }
          break;
        case 'dismiss':
          dismissConflict(resolution.conflictId);
          break;
      }
    },
    [updateSharedBatchRoom, updateSharedPlantCount, dismissConflict, splitSharedBatch, shiftSharedSchedule, selectBatch, rooms]
  );

  const handleViewAffectedBatches = useCallback(
    (batchIds: string[]) => {
      if (batchIds.length > 0 && batchIds[0] !== selectedBatchId) {
        selectBatch(batchIds[0]);
      }
    },
    [selectBatch, selectedBatchId]
  );

  return (
    <div className="flex h-full flex-col bg-background">
      <div className="flex items-start justify-between px-6 pt-4">
        <div>
          <h2 className="text-xl font-semibold text-foreground">Batch Planning</h2>
          <p className="text-sm text-muted-foreground">
            Visualize batches and adjust timelines; deep links from Planner Home keep context aligned.
          </p>
        </div>
        <PlannerToolbar onNewBatch={handleNewBatch} conflictCount={allConflicts.length} />
      </div>

      <div className="flex-1 flex overflow-hidden">
        <div className="flex-1 overflow-hidden p-4">
          <GanttChart className="h-full" onSelectBatch={handleSelectBatch}>
            {plannerBatches.map((batch, index) => (
              <GanttRowDraggable
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
                onMovePhase={handleMovePhase}
                onResizePhase={handleResizePhase}
                requireConfirmation={true}
              />
            ))}
          </GanttChart>
        </div>

        <BatchDetailPanel
          batch={selectedBatch}
          conflicts={allConflicts}
          rooms={rooms}
          isOpen={isDetailPanelOpen && !!selectedBatch}
          onClose={handleCloseDetailPanel}
          onEdit={() => console.log('Edit batch')}
          onDuplicate={handleDuplicateBatch}
          onDelete={handleDeleteBatch}
          onEditPhase={handleEditPhase}
          onResolveConflict={handleResolveConflict}
          onViewAffectedBatches={handleViewAffectedBatches}
        />
      </div>

      {dragState.isDragging && currentDragImpact && (
        <ImpactPanel
          impact={currentDragImpact.impact}
          conflicts={currentDragImpact.conflicts}
          isVisible={true}
          onConfirm={() => {}}
          onCancel={() => {}}
        />
      )}

      {settings.whatIfMode && (
        <div className="fixed bottom-4 left-4 flex items-center gap-2 px-4 py-2 bg-violet-500/20 border border-violet-500/30 rounded-lg text-violet-400 text-sm font-medium shadow-lg">
          <div className="w-2 h-2 rounded-full bg-violet-500 animate-pulse" />
          What-If Mode Active
          <span className="text-violet-400/70 text-xs ml-2">Changes are not saved</span>
        </div>
      )}

      <div className="fixed bottom-4 left-1/2 -translate-x-1/2 text-xs text-muted-foreground/60">
        Press <kbd className="px-1.5 py-0.5 bg-muted rounded text-muted-foreground mx-0.5">?</kbd> for keyboard
        shortcuts
      </div>

      <NewBatchModal
        isOpen={showNewBatchModal}
        onClose={() => setShowNewBatchModal(false)}
        onCreateBatch={handleCreateBatch}
        rooms={rooms}
      />
    </div>
  );
}



