/**
 * Blueprint Store
 * 
 * Manages cultivation blueprints - templates for environmental,
 * irrigation, and lighting parameters.
 */

import { create } from 'zustand';
import { devtools } from 'zustand/middleware';
import {
  PhaseBlueprint,
  BatchBlueprint,
  BatchBlueprintAssignment,
  DEFAULT_ENVIRONMENTAL_PARAMS,
  DEFAULT_LIGHTING_PARAMS,
  DEFAULT_IRRIGATION_PARAMS,
} from '../types/blueprint.types';
import { PhaseType } from '../types/planner.types';

const generateId = () => Math.random().toString(36).substring(2, 15);

interface BlueprintState {
  phaseBlueprints: PhaseBlueprint[];
  batchBlueprints: BatchBlueprint[];
  batchAssignments: Map<string, BatchBlueprintAssignment>;
  selectedBlueprintId: string | null;
  selectedBlueprintType: 'phase' | 'batch' | null;
  isEditing: boolean;
  hasUnsavedChanges: boolean;
}

interface BlueprintActions {
  addPhaseBlueprint: (blueprint: Omit<PhaseBlueprint, 'id' | 'createdAt' | 'updatedAt'>) => PhaseBlueprint;
  updatePhaseBlueprint: (id: string, updates: Partial<PhaseBlueprint>) => void;
  deletePhaseBlueprint: (id: string) => void;
  duplicatePhaseBlueprint: (id: string, newName: string) => PhaseBlueprint | null;
  addBatchBlueprint: (blueprint: Omit<BatchBlueprint, 'id' | 'createdAt' | 'updatedAt'>) => BatchBlueprint;
  updateBatchBlueprint: (id: string, updates: Partial<BatchBlueprint>) => void;
  deleteBatchBlueprint: (id: string) => void;
  assignBlueprintToBatch: (batchId: string, assignment: Partial<BatchBlueprintAssignment>) => void;
  clearBatchAssignment: (batchId: string) => void;
  getEffectiveBlueprintForPhase: (batchId: string, phase: PhaseType) => PhaseBlueprint | null;
  selectBlueprint: (id: string, type: 'phase' | 'batch') => void;
  clearSelection: () => void;
  setEditing: (editing: boolean) => void;
  setHasUnsavedChanges: (hasChanges: boolean) => void;
  getPhaseBlueprintsForPhase: (phase: PhaseType) => PhaseBlueprint[];
  getBlueprintById: (id: string) => PhaseBlueprint | BatchBlueprint | null;
}

const createDefaultPhaseBlueprints = (): PhaseBlueprint[] => {
  const phases: PhaseType[] = ['clone', 'veg', 'flower', 'harvest', 'cure'];
  const defaultDurations: Record<PhaseType, number> = {
    clone: 14, veg: 28, flower: 56, harvest: 7, cure: 14,
  };
  
  return phases.map(phase => ({
    id: `default-${phase}`,
    name: `Standard ${phase.charAt(0).toUpperCase() + phase.slice(1)}`,
    description: `Default parameters for ${phase} phase`,
    phase,
    environmental: DEFAULT_ENVIRONMENTAL_PARAMS[phase],
    lighting: DEFAULT_LIGHTING_PARAMS[phase],
    irrigation: DEFAULT_IRRIGATION_PARAMS[phase],
    defaultDurationDays: defaultDurations[phase],
    createdBy: 'system',
    createdAt: new Date(),
    updatedAt: new Date(),
    isDefault: true,
    isPublic: true,
  }));
};

const createDefaultBatchBlueprints = (): BatchBlueprint[] => [
  {
    id: 'default-standard',
    name: 'Standard Lifecycle',
    description: 'Standard cultivation lifecycle with default parameters',
    cloneBlueprintId: 'default-clone',
    vegBlueprintId: 'default-veg',
    flowerBlueprintId: 'default-flower',
    harvestBlueprintId: 'default-harvest',
    cureBlueprintId: 'default-cure',
    createdBy: 'system',
    createdAt: new Date(),
    updatedAt: new Date(),
    isDefault: true,
    isPublic: true,
  },
];

export const useBlueprintStore = create<BlueprintState & BlueprintActions>()(
  devtools(
    (set, get) => ({
      phaseBlueprints: createDefaultPhaseBlueprints(),
      batchBlueprints: createDefaultBatchBlueprints(),
      batchAssignments: new Map(),
      selectedBlueprintId: null,
      selectedBlueprintType: null,
      isEditing: false,
      hasUnsavedChanges: false,
      
      addPhaseBlueprint: (blueprint) => {
        const newBlueprint: PhaseBlueprint = {
          ...blueprint,
          id: generateId(),
          createdAt: new Date(),
          updatedAt: new Date(),
        };
        set((state) => ({
          phaseBlueprints: [...state.phaseBlueprints, newBlueprint],
        }));
        return newBlueprint;
      },
      
      updatePhaseBlueprint: (id, updates) => {
        set((state) => ({
          phaseBlueprints: state.phaseBlueprints.map(bp =>
            bp.id === id ? { ...bp, ...updates, updatedAt: new Date() } : bp
          ),
        }));
      },
      
      deletePhaseBlueprint: (id) => {
        set((state) => ({
          phaseBlueprints: state.phaseBlueprints.filter(bp => bp.id !== id),
        }));
      },
      
      duplicatePhaseBlueprint: (id, newName) => {
        const original = get().phaseBlueprints.find(bp => bp.id === id);
        if (!original) return null;
        const duplicate: PhaseBlueprint = {
          ...original,
          id: generateId(),
          name: newName,
          isDefault: false,
          createdAt: new Date(),
          updatedAt: new Date(),
        };
        set((state) => ({
          phaseBlueprints: [...state.phaseBlueprints, duplicate],
        }));
        return duplicate;
      },
      
      addBatchBlueprint: (blueprint) => {
        const newBlueprint: BatchBlueprint = {
          ...blueprint,
          id: generateId(),
          createdAt: new Date(),
          updatedAt: new Date(),
        };
        set((state) => ({
          batchBlueprints: [...state.batchBlueprints, newBlueprint],
        }));
        return newBlueprint;
      },
      
      updateBatchBlueprint: (id, updates) => {
        set((state) => ({
          batchBlueprints: state.batchBlueprints.map(bp =>
            bp.id === id ? { ...bp, ...updates, updatedAt: new Date() } : bp
          ),
        }));
      },
      
      deleteBatchBlueprint: (id) => {
        set((state) => ({
          batchBlueprints: state.batchBlueprints.filter(bp => bp.id !== id),
        }));
      },
      
      assignBlueprintToBatch: (batchId, assignment) => {
        set((state) => {
          const newAssignments = new Map(state.batchAssignments);
          const existing = newAssignments.get(batchId) || { batchId };
          newAssignments.set(batchId, { ...existing, ...assignment });
          return { batchAssignments: newAssignments };
        });
      },
      
      clearBatchAssignment: (batchId) => {
        set((state) => {
          const newAssignments = new Map(state.batchAssignments);
          newAssignments.delete(batchId);
          return { batchAssignments: newAssignments };
        });
      },
      
      getEffectiveBlueprintForPhase: (batchId, phase) => {
        const { batchAssignments, phaseBlueprints, batchBlueprints } = get();
        const assignment = batchAssignments.get(batchId);
        
        if (!assignment) {
          return phaseBlueprints.find(bp => bp.phase === phase && bp.isDefault) || null;
        }
        
        const phaseOverrideId = assignment.phaseBlueprintOverrides?.[phase];
        if (phaseOverrideId) {
          return phaseBlueprints.find(bp => bp.id === phaseOverrideId) || null;
        }
        
        if (assignment.batchBlueprintId) {
          const batchBp = batchBlueprints.find(bp => bp.id === assignment.batchBlueprintId);
          if (batchBp) {
            const key = `${phase}BlueprintId` as keyof BatchBlueprint;
            const phaseBpId = batchBp[key] as string | undefined;
            if (phaseBpId) {
              return phaseBlueprints.find(bp => bp.id === phaseBpId) || null;
            }
          }
        }
        
        return phaseBlueprints.find(bp => bp.phase === phase && bp.isDefault) || null;
      },
      
      selectBlueprint: (id, type) => {
        set({ selectedBlueprintId: id, selectedBlueprintType: type });
      },
      
      clearSelection: () => {
        set({ selectedBlueprintId: null, selectedBlueprintType: null });
      },
      
      setEditing: (editing) => set({ isEditing: editing }),
      setHasUnsavedChanges: (hasChanges) => set({ hasUnsavedChanges: hasChanges }),
      
      getPhaseBlueprintsForPhase: (phase) => {
        return get().phaseBlueprints.filter(bp => bp.phase === phase);
      },
      
      getBlueprintById: (id) => {
        const { phaseBlueprints, batchBlueprints } = get();
        return phaseBlueprints.find(bp => bp.id === id) 
          || batchBlueprints.find(bp => bp.id === id) 
          || null;
      },
    }),
    { name: 'blueprint-store' }
  )
);






