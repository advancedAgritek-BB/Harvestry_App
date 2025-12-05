'use client';

import React, { useState, useMemo } from 'react';
import { cn } from '@/lib/utils';
import { 
  X, 
  Calendar, 
  Clock, 
  Sprout, 
  Leaf, 
  Flower2, 
  Scissors, 
  Package,
  MapPin,
  Users,
  AlertTriangle,
  ChevronRight,
  Edit3,
  Copy,
  Trash2,
  FileText,
  Settings,
  LayoutGrid,
} from 'lucide-react';
import { PlannedBatch, BatchPhase, PhaseType, PlannerConflict } from '../../types/planner.types';
import { PhaseBlueprint } from '../../types/blueprint.types';
import { PHASE_CONFIGS, PHASE_ORDER } from '../../constants/phaseConfig';
import { formatDuration } from '../../utils/dateUtils';
import { getBatchConflicts } from '../../utils/conflictDetection';
import { BlueprintSelector } from '../BlueprintSelector';
import { ParameterEditor } from '../ParameterEditor';
import { useBlueprintStore } from '../../stores/blueprintStore';
import { differenceInDays, format } from 'date-fns';

const PHASE_ICONS: Record<PhaseType, React.ElementType> = {
  clone: Sprout,
  veg: Leaf,
  flower: Flower2,
  harvest: Scissors,
  cure: Package,
};

type TabType = 'overview' | 'blueprint';

interface BatchDetailPanelProps {
  batch: PlannedBatch | null;
  conflicts: PlannerConflict[];
  isOpen: boolean;
  onClose: () => void;
  onEdit?: (batch: PlannedBatch) => void;
  onDuplicate?: (batch: PlannedBatch) => void;
  onDelete?: (batch: PlannedBatch) => void;
  onEditPhase?: (batchId: string, phaseId: string) => void;
}

export function BatchDetailPanel({
  batch,
  conflicts,
  isOpen,
  onClose,
  onEdit,
  onDuplicate,
  onDelete,
  onEditPhase,
}: BatchDetailPanelProps) {
  const [activeTab, setActiveTab] = useState<TabType>('overview');
  const [selectedPhase, setSelectedPhase] = useState<PhaseType>('veg');
  const [blueprintMode, setBlueprintMode] = useState<'batch' | 'phase'>('batch');
  const [isEditingParams, setIsEditingParams] = useState(false);
  const [localBlueprint, setLocalBlueprint] = useState<PhaseBlueprint | null>(null);

  const {
    phaseBlueprints,
    batchBlueprints,
    batchAssignments,
    assignBlueprintToBatch,
    getEffectiveBlueprintForPhase,
    addPhaseBlueprint,
  } = useBlueprintStore();

  // Get effective blueprint for selected phase - must be before early return
  const effectiveBlueprint = useMemo(() => {
    if (localBlueprint) return localBlueprint;
    if (!batch) return null;
    return getEffectiveBlueprintForPhase(batch.id, selectedPhase);
  }, [batch?.id, selectedPhase, localBlueprint, getEffectiveBlueprintForPhase, batch]);

  // Early return AFTER all hooks
  if (!batch) return null;

  const batchConflicts = getBatchConflicts(batch, conflicts);
  const hasConflicts = batchConflicts.length > 0;
  const assignment = batchAssignments.get(batch.id);

  // Sort phases by lifecycle order
  const sortedPhases = [...batch.phases].sort(
    (a, b) => PHASE_ORDER.indexOf(a.phase) - PHASE_ORDER.indexOf(b.phase)
  );

  // Calculate total duration
  const firstPhase = sortedPhases[0];
  const lastPhase = sortedPhases[sortedPhases.length - 1];
  const totalDays = firstPhase && lastPhase
    ? differenceInDays(lastPhase.plannedEnd, firstPhase.plannedStart) + 1
    : 0;

  const handleBlueprintChange = (updates: Partial<PhaseBlueprint>) => {
    if (!effectiveBlueprint) return;
    setLocalBlueprint({
      ...effectiveBlueprint,
      ...updates,
    } as PhaseBlueprint);
    setIsEditingParams(true);
  };

  const handleSaveAsNewBlueprint = (name: string) => {
    if (!localBlueprint) return;
    
    const newBlueprint = addPhaseBlueprint({
      ...localBlueprint,
      name,
      isDefault: false,
      isPublic: false,
      createdBy: 'user',
    });

    // Assign the new blueprint to this batch's phase
    assignBlueprintToBatch(batch.id, {
      phaseBlueprintOverrides: {
        ...assignment?.phaseBlueprintOverrides,
        [selectedPhase]: newBlueprint.id,
      },
    });

    setLocalBlueprint(null);
    setIsEditingParams(false);
  };

  const handleResetParams = () => {
    setLocalBlueprint(null);
    setIsEditingParams(false);
  };

  return (
    <div 
      className={cn(
        'fixed top-0 right-0 h-full w-[420px] bg-surface border-l border-border shadow-2xl z-40',
        'transform transition-transform duration-300 ease-out',
        isOpen ? 'translate-x-0' : 'translate-x-full'
      )}
    >
      {/* Header */}
      <div className="flex items-center justify-between px-4 py-3 border-b border-border">
        <div className="flex items-center gap-2">
          <div 
            className={cn(
              'w-2 h-2 rounded-full',
              batch.status === 'active' && 'bg-emerald-500',
              batch.status === 'planned' && 'bg-cyan-500',
              batch.status === 'completed' && 'bg-muted-foreground',
              batch.status === 'cancelled' && 'bg-red-500'
            )}
          />
          <span className="text-xs text-muted-foreground uppercase tracking-wider">
            {batch.status}
          </span>
        </div>
        <button
          onClick={onClose}
          className="p-1.5 text-muted-foreground hover:text-foreground hover:bg-muted rounded-md transition-colors"
        >
          <X className="w-4 h-4" />
        </button>
      </div>

      {/* Tabs */}
      <div className="flex border-b border-border">
        <button
          onClick={() => setActiveTab('overview')}
          className={cn(
            'flex-1 flex items-center justify-center gap-2 px-4 py-2.5 text-sm font-medium transition-colors',
            activeTab === 'overview'
              ? 'text-cyan-400 border-b-2 border-cyan-400 bg-cyan-500/5'
              : 'text-muted-foreground hover:text-foreground/70'
          )}
        >
          <LayoutGrid className="w-4 h-4" />
          Overview
        </button>
        <button
          onClick={() => setActiveTab('blueprint')}
          className={cn(
            'flex-1 flex items-center justify-center gap-2 px-4 py-2.5 text-sm font-medium transition-colors',
            activeTab === 'blueprint'
              ? 'text-cyan-400 border-b-2 border-cyan-400 bg-cyan-500/5'
              : 'text-muted-foreground hover:text-foreground/70'
          )}
        >
          <FileText className="w-4 h-4" />
          Blueprint
        </button>
      </div>

      {/* Content */}
      <div className="flex flex-col h-[calc(100%-180px)] overflow-y-auto">
        {activeTab === 'overview' ? (
          <>
            {/* Batch Info */}
            <div className="p-4 border-b border-border/50">
              <h2 className="text-xl font-semibold text-foreground mb-1">{batch.name}</h2>
              <p className="text-sm text-muted-foreground">{batch.code}</p>
              
              <div className="mt-4 space-y-2">
                <div className="flex items-center gap-2 text-sm">
                  <Leaf className="w-4 h-4 text-emerald-400" />
                  <span className="text-muted-foreground">Strain:</span>
                  <span className="text-foreground">{batch.strain}</span>
                </div>
                <div className="flex items-center gap-2 text-sm">
                  <Users className="w-4 h-4 text-blue-400" />
                  <span className="text-muted-foreground">Plants:</span>
                  <span className="text-foreground">{batch.plantCount.toLocaleString()}</span>
                </div>
                <div className="flex items-center gap-2 text-sm">
                  <Clock className="w-4 h-4 text-violet-400" />
                  <span className="text-muted-foreground">Total Duration:</span>
                  <span className="text-foreground">{formatDuration(totalDays)}</span>
                </div>
              </div>
            </div>

            {/* Conflicts Warning */}
            {hasConflicts && (
              <div className="mx-4 mt-4 p-3 bg-red-500/10 border border-red-500/30 rounded-lg">
                <div className="flex items-center gap-2 text-red-400 mb-2">
                  <AlertTriangle className="w-4 h-4" />
                  <span className="text-sm font-medium">
                    {batchConflicts.length} {batchConflicts.length === 1 ? 'Conflict' : 'Conflicts'}
                  </span>
                </div>
                <div className="space-y-1">
                  {batchConflicts.slice(0, 3).map((conflict) => (
                    <p key={conflict.id} className="text-xs text-red-300/80">
                      {conflict.message}
                    </p>
                  ))}
                  {batchConflicts.length > 3 && (
                    <p className="text-xs text-red-400">
                      +{batchConflicts.length - 3} more
                    </p>
                  )}
                </div>
              </div>
            )}

            {/* Phase Timeline */}
            <div className="p-4">
              <h3 className="text-xs text-muted-foreground uppercase tracking-wider mb-3">
                Lifecycle Phases
              </h3>
              <div className="space-y-2">
                {sortedPhases.map((phase, index) => {
                  const config = PHASE_CONFIGS[phase.phase];
                  const Icon = PHASE_ICONS[phase.phase];
                  const duration = differenceInDays(phase.plannedEnd, phase.plannedStart) + 1;
                  const phaseConflict = batchConflicts.find((c) => c.phaseId === phase.id);

                  return (
                    <div
                      key={phase.id}
                      className={cn(
                        'relative p-3 rounded-lg border transition-colors cursor-pointer group',
                        phaseConflict
                          ? 'bg-red-500/5 border-red-500/30 hover:bg-red-500/10'
                          : 'bg-muted/30 border-border/50 hover:bg-muted/50'
                      )}
                      onClick={() => {
                        setSelectedPhase(phase.phase);
                        setActiveTab('blueprint');
                      }}
                    >
                      {index < sortedPhases.length - 1 && (
                        <div className="absolute left-6 -bottom-3 w-0.5 h-4 bg-hover" />
                      )}

                      <div className="flex items-start gap-3">
                        <div 
                          className="w-8 h-8 rounded-lg flex items-center justify-center flex-shrink-0"
                          style={{ backgroundColor: `${config.color}20` }}
                        >
                          <Icon className="w-4 h-4" style={{ color: config.color }} />
                        </div>
                        
                        <div className="flex-1 min-w-0">
                          <div className="flex items-center justify-between mb-1">
                            <span className="font-medium text-foreground">{config.label}</span>
                            <span className="text-xs text-muted-foreground">
                              {formatDuration(duration)}
                            </span>
                          </div>
                          
                          <div className="flex items-center gap-2 text-xs text-muted-foreground">
                            <Calendar className="w-3 h-3" />
                            <span>
                              {format(phase.plannedStart, 'MMM d')} - {format(phase.plannedEnd, 'MMM d')}
                            </span>
                          </div>

                          {phase.roomName && (
                            <div className="flex items-center gap-2 text-xs text-muted-foreground mt-1">
                              <MapPin className="w-3 h-3" />
                              <span>{phase.roomName}</span>
                            </div>
                          )}

                          {phaseConflict && (
                            <div className="flex items-center gap-1 mt-2 text-xs text-red-400">
                              <AlertTriangle className="w-3 h-3" />
                              <span className="truncate">{phaseConflict.message}</span>
                            </div>
                          )}
                        </div>

                        <ChevronRight className="w-4 h-4 text-muted-foreground/60 group-hover:text-muted-foreground transition-colors" />
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>
          </>
        ) : (
          /* Blueprint Tab */
          <div className="p-4 space-y-4">
            {/* Blueprint Assignment */}
            <div>
              <h3 className="text-xs text-muted-foreground uppercase tracking-wider mb-3">
                Blueprint Assignment
              </h3>
              <BlueprintSelector
                batchBlueprints={batchBlueprints}
                phaseBlueprints={phaseBlueprints}
                selectedBatchBlueprintId={assignment?.batchBlueprintId}
                phaseOverrides={assignment?.phaseBlueprintOverrides as Record<PhaseType, string>}
                onSelectBatchBlueprint={(id) => {
                  assignBlueprintToBatch(batch.id, { batchBlueprintId: id || undefined });
                }}
                onSelectPhaseBlueprint={(phase, id) => {
                  assignBlueprintToBatch(batch.id, {
                    phaseBlueprintOverrides: {
                      ...assignment?.phaseBlueprintOverrides,
                      [phase]: id || undefined,
                    },
                  });
                }}
                mode={blueprintMode}
                onModeChange={setBlueprintMode}
              />
            </div>

            {/* Phase Selector for Parameters */}
            <div>
              <h3 className="text-xs text-muted-foreground uppercase tracking-wider mb-3">
                Phase Parameters
              </h3>
              <div className="flex gap-1 mb-3">
                {(['clone', 'veg', 'flower', 'harvest', 'cure'] as PhaseType[]).map((phase) => {
                  const config = PHASE_CONFIGS[phase];
                  return (
                    <button
                      key={phase}
                      onClick={() => {
                        setSelectedPhase(phase);
                        setLocalBlueprint(null);
                        setIsEditingParams(false);
                      }}
                      className={cn(
                        'flex-1 py-1.5 text-xs font-medium rounded transition-colors',
                        selectedPhase === phase
                          ? 'text-foreground'
                          : 'text-muted-foreground hover:text-foreground/70'
                      )}
                      style={{
                        backgroundColor: selectedPhase === phase ? `${config.color}30` : 'transparent',
                      }}
                    >
                      {config.label.slice(0, 3)}
                    </button>
                  );
                })}
              </div>

              {/* Parameter Editor */}
              <ParameterEditor
                blueprint={effectiveBlueprint}
                phase={selectedPhase}
                isEditing={isEditingParams}
                onChange={handleBlueprintChange}
                onSaveAsNew={handleSaveAsNewBlueprint}
                onReset={handleResetParams}
              />

              {/* Edit Toggle */}
              {!isEditingParams && effectiveBlueprint && (
                <button
                  onClick={() => setIsEditingParams(true)}
                  className="w-full mt-3 flex items-center justify-center gap-2 px-4 py-2 text-sm font-medium text-foreground/70 bg-muted/50 hover:bg-hover/50 border border-border rounded-lg transition-colors"
                >
                  <Settings className="w-4 h-4" />
                  Edit Parameters
                </button>
              )}
            </div>
          </div>
        )}
      </div>

      {/* Actions Footer */}
      <div className="absolute bottom-0 left-0 right-0 p-4 border-t border-border bg-surface">
        <div className="flex items-center gap-2">
          <button
            onClick={() => onEdit?.(batch)}
            className="flex-1 flex items-center justify-center gap-2 px-4 py-2 text-sm font-medium text-foreground bg-cyan-500 hover:bg-cyan-400 rounded-lg transition-colors"
          >
            <Edit3 className="w-4 h-4" />
            Edit Batch
          </button>
          
          <button
            onClick={() => onDuplicate?.(batch)}
            className="p-2 text-muted-foreground hover:text-foreground hover:bg-muted rounded-lg transition-colors"
            title="Duplicate"
          >
            <Copy className="w-4 h-4" />
          </button>
          
          <button
            onClick={() => onDelete?.(batch)}
            className="p-2 text-muted-foreground hover:text-red-400 hover:bg-red-500/10 rounded-lg transition-colors"
            title="Delete"
          >
            <Trash2 className="w-4 h-4" />
          </button>
        </div>
      </div>
    </div>
  );
}
