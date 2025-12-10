'use client';

import React, { useState, useEffect, useMemo, useCallback } from 'react';
import { cn } from '@/lib/utils';
import {
  X,
  Plus,
  Calendar,
  Dna,
  ChevronDown,
  Check,
  Search,
  Leaf,
  Layers,
  Settings2,
  Sparkles,
  ChevronRight,
} from 'lucide-react';
import { addDays, format } from 'date-fns';
import { useGeneticsStore } from '@/features/genetics/stores';
import { toPlannerGenetics, GENETIC_TYPE_CONFIG } from '@/features/genetics/types';
import type { Genetics } from '@/features/genetics/types';
import type { Room, PlannedBatch, BatchPhase, PhaseType } from '../../types';
import { useBlueprintStore } from '../../stores/blueprintStore';
import { BatchNamingService } from '../../services/batchNaming.service';
import { BlueprintTab } from './BlueprintTab';
import { CustomConfigTab, type PhaseConfig } from './CustomConfigTab';

// =============================================================================
// TYPES
// =============================================================================

interface NewBatchModalProps {
  isOpen: boolean;
  onClose: () => void;
  onCreateBatch: (batch: PlannedBatch) => void;
  rooms: Room[];
}

type ConfigMode = 'blueprint' | 'custom';

// =============================================================================
// GENETICS SELECTOR COMPONENT
// =============================================================================

function GeneticsSelector({
  genetics,
  selectedId,
  onSelect,
  isLoading,
}: {
  genetics: Genetics[];
  selectedId: string | null;
  onSelect: (genetics: Genetics) => void;
  isLoading: boolean;
}) {
  const [isOpen, setIsOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');

  const selected = genetics.find((g) => g.id === selectedId);

  const filtered = useMemo(() => {
    if (!searchQuery) return genetics;
    const query = searchQuery.toLowerCase();
    return genetics.filter(
      (g) =>
        g.name.toLowerCase().includes(query) ||
        g.geneticType.toLowerCase().includes(query)
    );
  }, [genetics, searchQuery]);

  if (isLoading) {
    return (
      <div className="px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-muted-foreground">
        Loading genetics...
      </div>
    );
  }

  return (
    <div className="relative">
      <button
        type="button"
        onClick={() => setIsOpen(!isOpen)}
        className={cn(
          'w-full flex items-center justify-between px-3 py-2.5 bg-muted/30 border rounded-lg text-sm transition-colors',
          isOpen
            ? 'border-emerald-500/50'
            : 'border-border hover:border-emerald-500/30'
        )}
      >
        {selected ? (
          <div className="flex items-center gap-2">
            <div
              className="w-2 h-2 rounded-full"
              style={{ backgroundColor: GENETIC_TYPE_CONFIG[selected.geneticType].color }}
            />
            <span className="font-medium text-foreground">{selected.name}</span>
            <span className="text-xs text-muted-foreground">
              ({GENETIC_TYPE_CONFIG[selected.geneticType].label})
            </span>
          </div>
        ) : (
          <span className="text-muted-foreground">Select genetics...</span>
        )}
        <ChevronDown className={cn('w-4 h-4 transition-transform', isOpen && 'rotate-180')} />
      </button>

      {isOpen && (
        <>
          <div className="fixed inset-0 z-40" onClick={() => setIsOpen(false)} />
          <div className="absolute top-full left-0 right-0 mt-1 bg-surface border border-border rounded-lg shadow-xl z-50 overflow-hidden">
            {/* Search */}
            <div className="p-2 border-b border-border">
              <div className="relative">
                <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
                <input
                  type="text"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  placeholder="Search genetics..."
                  className="w-full pl-9 pr-3 py-2 bg-muted/30 border border-border rounded-lg text-sm focus:outline-none focus:border-emerald-500/50"
                  autoFocus
                />
              </div>
            </div>

            {/* Options */}
            <div className="max-h-64 overflow-y-auto">
              {filtered.length === 0 ? (
                <div className="px-4 py-8 text-center">
                  <Dna className="w-8 h-8 text-muted-foreground mx-auto mb-2" />
                  <p className="text-sm text-muted-foreground">No genetics found</p>
                  <a
                    href="/library/genetics"
                    className="text-xs text-emerald-400 hover:text-emerald-300 mt-1 inline-block"
                  >
                    Add genetics in Library
                  </a>
                </div>
              ) : (
                <>
                  {filtered.map((g) => {
                    const typeConfig = GENETIC_TYPE_CONFIG[g.geneticType];
                    const isSelected = g.id === selectedId;
                    return (
                      <button
                        key={g.id}
                        type="button"
                        onClick={() => {
                          onSelect(g);
                          setIsOpen(false);
                          setSearchQuery('');
                        }}
                        className={cn(
                          'w-full flex items-center gap-3 px-3 py-2.5 text-left transition-colors',
                          isSelected ? 'bg-emerald-500/10' : 'hover:bg-muted/50'
                        )}
                      >
                        <div
                          className="w-8 h-8 rounded-lg flex items-center justify-center shrink-0"
                          style={{ backgroundColor: `${typeConfig.color}15` }}
                        >
                          <Dna className="w-4 h-4" style={{ color: typeConfig.color }} />
                        </div>
                        <div className="flex-1 min-w-0">
                          <div className="flex items-center gap-2">
                            <span className="font-medium text-foreground truncate">
                              {g.name}
                            </span>
                            <span
                              className="text-xs px-1.5 py-0.5 rounded"
                              style={{
                                backgroundColor: `${typeConfig.color}20`,
                                color: typeConfig.color,
                              }}
                            >
                              {typeConfig.label}
                            </span>
                          </div>
                          <div className="flex items-center gap-3 text-xs text-muted-foreground">
                            <span>THC: {g.thcMin}-{g.thcMax}%</span>
                            {g.floweringTimeDays && <span>Flower: {g.floweringTimeDays}d</span>}
                          </div>
                        </div>
                        {isSelected && <Check className="w-4 h-4 text-emerald-400 shrink-0" />}
                      </button>
                    );
                  })}
                </>
              )}
            </div>

            {/* Add New Genetic */}
            <div className="border-t border-border">
              <a
                href="/library/genetics"
                className="flex items-center gap-3 px-3 py-2.5 text-sm text-emerald-400 hover:bg-emerald-500/10 transition-colors"
              >
                <div className="w-8 h-8 rounded-lg bg-emerald-500/10 flex items-center justify-center shrink-0">
                  <Plus className="w-4 h-4 text-emerald-400" />
                </div>
                <span className="font-medium">Add New Genetic</span>
              </a>
            </div>
          </div>
        </>
      )}
    </div>
  );
}

// =============================================================================
// MAIN COMPONENT
// =============================================================================

export function NewBatchModal({ isOpen, onClose, onCreateBatch, rooms }: NewBatchModalProps) {
  // Stores
  const { genetics, geneticsLoading, setSiteId } = useGeneticsStore();
  const { batchBlueprints, phaseBlueprints } = useBlueprintStore();

  // Form state
  const [batchName, setBatchName] = useState('');
  const [isNameAutoGenerated, setIsNameAutoGenerated] = useState(true);
  const [selectedGeneticsId, setSelectedGeneticsId] = useState<string | null>(null);
  const [plantCount, setPlantCount] = useState(100);
  const [startDate, setStartDate] = useState(new Date());
  const [configMode, setConfigMode] = useState<ConfigMode>('blueprint');
  const [selectedBlueprintId, setSelectedBlueprintId] = useState<string | null>(null);
  const [phaseConfigs, setPhaseConfigs] = useState<Record<PhaseType, PhaseConfig>>({
    clone: { phase: 'clone', roomId: '', duration: 14 },
    veg: { phase: 'veg', roomId: '', duration: 21 },
    flower: { phase: 'flower', roomId: '', duration: 56 },
    harvest: { phase: 'harvest', roomId: '', duration: 3 },
    cure: { phase: 'cure', roomId: '', duration: 14 },
  });

  // Initialize genetics store
  useEffect(() => {
    if (isOpen) {
      setSiteId('site-1');
    }
  }, [isOpen, setSiteId]);

  // Get selected genetics
  const selectedGenetics = useMemo(
    () => genetics.find((g) => g.id === selectedGeneticsId),
    [genetics, selectedGeneticsId]
  );

  // Auto-generate batch name when genetics changes
  useEffect(() => {
    const generateName = async () => {
      if (isNameAutoGenerated && selectedGenetics) {
        try {
          const name = await BatchNamingService.previewBatchName('site-1', {
            strainName: selectedGenetics.name,
            geneticType: selectedGenetics.geneticType as 'indica' | 'sativa' | 'hybrid',
            siteCode: 'SITE',
            date: startDate,
          });
          setBatchName(name);
        } catch (error) {
          // Fallback to simple name
          setBatchName(`${selectedGenetics.name} Run`);
        }
      }
    };
    generateName();
  }, [selectedGenetics, startDate, isNameAutoGenerated]);

  // Auto-set default rooms when modal opens
  useEffect(() => {
    if (isOpen && rooms.length > 0) {
      const PHASE_ROOM_CLASS_MAP: Record<PhaseType, string[]> = {
        clone: ['propagation', 'veg'],
        veg: ['veg'],
        flower: ['flower'],
        harvest: ['drying', 'processing'],
        cure: ['cure'],
      };

      const getDefaultRoom = (phase: PhaseType): string => {
        const allowedClasses = PHASE_ROOM_CLASS_MAP[phase];
        const room = rooms.find((r) => allowedClasses.includes(r.roomClass));
        return room?.id || '';
      };

      setPhaseConfigs((prev) => ({
        clone: { ...prev.clone, roomId: prev.clone.roomId || getDefaultRoom('clone') },
        veg: { ...prev.veg, roomId: prev.veg.roomId || getDefaultRoom('veg') },
        flower: { ...prev.flower, roomId: prev.flower.roomId || getDefaultRoom('flower') },
        harvest: { ...prev.harvest, roomId: prev.harvest.roomId || getDefaultRoom('harvest') },
        cure: { ...prev.cure, roomId: prev.cure.roomId || getDefaultRoom('cure') },
      }));
    }
  }, [isOpen, rooms]);

  // Update durations when genetics changes (only for custom config)
  useEffect(() => {
    if (selectedGenetics && configMode === 'custom') {
      const plannerGenetics = toPlannerGenetics(selectedGenetics);
      setPhaseConfigs((prev) => ({
        clone: { ...prev.clone, duration: plannerGenetics.defaultCloneDays },
        veg: { ...prev.veg, duration: plannerGenetics.defaultVegDays },
        flower: { ...prev.flower, duration: plannerGenetics.defaultFlowerDays },
        harvest: { ...prev.harvest, duration: plannerGenetics.defaultHarvestDays },
        cure: { ...prev.cure, duration: plannerGenetics.defaultCureDays },
      }));
    }
  }, [selectedGenetics, configMode]);

  // Update durations when blueprint is selected
  useEffect(() => {
    if (selectedBlueprintId && configMode === 'blueprint') {
      const blueprint = batchBlueprints.find((bp) => bp.id === selectedBlueprintId);
      if (blueprint) {
        const phases: { phase: PhaseType; bpKey: keyof typeof blueprint }[] = [
          { phase: 'clone', bpKey: 'cloneBlueprintId' },
          { phase: 'veg', bpKey: 'vegBlueprintId' },
          { phase: 'flower', bpKey: 'flowerBlueprintId' },
          { phase: 'harvest', bpKey: 'harvestBlueprintId' },
          { phase: 'cure', bpKey: 'cureBlueprintId' },
        ];

        setPhaseConfigs((prev) => {
          const newConfigs = { ...prev };
          phases.forEach(({ phase, bpKey }) => {
            const bpId = blueprint[bpKey] as string | undefined;
            const phaseBp = bpId
              ? phaseBlueprints.find((p) => p.id === bpId)
              : undefined;
            if (phaseBp) {
              newConfigs[phase] = {
                ...newConfigs[phase],
                duration: phaseBp.defaultDurationDays,
              };
            }
          });
          return newConfigs;
        });
      }
    }
  }, [selectedBlueprintId, configMode, batchBlueprints, phaseBlueprints]);

  // Handle genetics selection
  const handleSelectGenetics = useCallback((g: Genetics) => {
    setSelectedGeneticsId(g.id);
  }, []);

  // Handle batch name change
  const handleBatchNameChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    setBatchName(e.target.value);
    setIsNameAutoGenerated(false);
  }, []);

  // Handle phase config change
  const handlePhaseConfigChange = useCallback(
    (phase: PhaseType, updates: Partial<PhaseConfig>) => {
      setPhaseConfigs((prev) => ({
        ...prev,
        [phase]: { ...prev[phase], ...updates },
      }));
    },
    []
  );

  // Generate batch phases
  const generatePhases = useCallback((): BatchPhase[] => {
    const phases: BatchPhase[] = [];
    let currentDate = startDate;
    const phaseOrder: PhaseType[] = ['clone', 'veg', 'flower', 'harvest', 'cure'];

    for (const phase of phaseOrder) {
      const config = phaseConfigs[phase];
      const room = rooms.find((r) => r.id === config.roomId);

      phases.push({
        id: `phase-${Date.now()}-${phase}`,
        phase,
        plannedStart: currentDate,
        plannedEnd: addDays(currentDate, config.duration - 1),
        roomId: config.roomId,
        roomName: room?.name,
      });

      currentDate = addDays(currentDate, config.duration);
    }

    return phases;
  }, [startDate, phaseConfigs, rooms]);

  // Handle submit
  const handleSubmit = useCallback(
    async (e: React.FormEvent) => {
      e.preventDefault();

      if (!selectedGenetics || !batchName.trim()) {
        return;
      }

      // Get actual batch code from naming service
      let batchCode: string;
      try {
        batchCode = await BatchNamingService.generateBatchName('site-1', {
          strainName: selectedGenetics.name,
          geneticType: selectedGenetics.geneticType as 'indica' | 'sativa' | 'hybrid',
          siteCode: 'SITE',
          date: startDate,
        });
      } catch {
        batchCode = `B-${Date.now().toString().slice(-6)}-${selectedGenetics.name.slice(0, 3).toUpperCase()}`;
      }

      const batch: PlannedBatch = {
        id: `batch-${Date.now()}`,
        name: batchName.trim(),
        code: batchCode,
        strain: selectedGenetics.name,
        genetics: {
          id: selectedGenetics.id,
          name: selectedGenetics.name,
          defaultVegDays: phaseConfigs.veg.duration,
          defaultFlowerDays: phaseConfigs.flower.duration,
          defaultCureDays: phaseConfigs.cure.duration,
        },
        plantCount,
        phases: generatePhases(),
        status: 'planned',
        blueprintId: configMode === 'blueprint' ? selectedBlueprintId || undefined : undefined,
        createdAt: new Date(),
        updatedAt: new Date(),
      };

      onCreateBatch(batch);
      handleClose();
    },
    [
      selectedGenetics,
      batchName,
      plantCount,
      phaseConfigs,
      generatePhases,
      configMode,
      selectedBlueprintId,
      startDate,
      onCreateBatch,
    ]
  );

  // Handle close and reset
  const handleClose = useCallback(() => {
    setBatchName('');
    setIsNameAutoGenerated(true);
    setSelectedGeneticsId(null);
    setPlantCount(100);
    setStartDate(new Date());
    setConfigMode('blueprint');
    setSelectedBlueprintId(null);
    onClose();
  }, [onClose]);

  // Escape key handler
  useEffect(() => {
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && isOpen) handleClose();
    };
    document.addEventListener('keydown', handleEscape);
    return () => document.removeEventListener('keydown', handleEscape);
  }, [isOpen, handleClose]);

  // Prevent body scroll
  useEffect(() => {
    if (isOpen) document.body.style.overflow = 'hidden';
    return () => {
      document.body.style.overflow = 'unset';
    };
  }, [isOpen]);

  if (!isOpen) return null;

  // Calculate total cycle duration
  const totalDays = Object.values(phaseConfigs).reduce((sum, c) => sum + c.duration, 0);
  const estimatedEndDate = addDays(startDate, totalDays - 1);

  // Validation
  const canSubmit =
    batchName.trim() &&
    selectedGeneticsId &&
    plantCount > 0 &&
    (configMode === 'custom' || selectedBlueprintId);

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      {/* Backdrop */}
      <div
        className="absolute inset-0 bg-background/80 backdrop-blur-sm"
        onClick={handleClose}
      />

      {/* Modal */}
      <div className="relative w-full max-w-2xl max-h-[90vh] bg-surface border border-border rounded-xl shadow-2xl flex flex-col overflow-hidden">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-border shrink-0">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-cyan-500/10 flex items-center justify-center">
              <Plus className="w-5 h-5 text-cyan-400" />
            </div>
            <div>
              <h2 className="text-lg font-semibold text-foreground">Create New Batch</h2>
              <p className="text-sm text-muted-foreground">Plan a new cultivation batch</p>
            </div>
          </div>
          <button
            onClick={handleClose}
            className="p-2 rounded-lg text-muted-foreground hover:text-foreground hover:bg-muted/50 transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Content */}
        <form onSubmit={handleSubmit} className="flex-1 overflow-y-auto">
          <div className="p-6 space-y-6">
            {/* Basic Info */}
            <div className="grid grid-cols-2 gap-4">
              {/* Batch Name */}
              <div className="col-span-2">
                <label className="block text-sm font-medium text-foreground mb-1.5">
                  Batch Name <span className="text-rose-400">*</span>
                </label>
                <div className="relative">
                  <input
                    type="text"
                    value={batchName}
                    onChange={handleBatchNameChange}
                    placeholder="Auto-generated based on naming rules..."
                    className={cn(
                      'w-full px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-cyan-500/50',
                      isNameAutoGenerated && 'pr-24'
                    )}
                  />
                  {isNameAutoGenerated && batchName && (
                    <div className="absolute right-2 top-1/2 -translate-y-1/2 flex items-center gap-1 px-2 py-0.5 bg-violet-500/10 text-violet-400 text-[10px] rounded">
                      <Sparkles className="w-3 h-3" />
                      Auto-generated
                    </div>
                  )}
                </div>
                <p className="mt-1 text-xs text-muted-foreground">
                  Auto-generated from{' '}
                  <a href="/admin/batch-naming" className="text-violet-400 hover:underline">
                    naming rules
                  </a>
                  . Edit to customize.
                </p>
              </div>

              {/* Genetics */}
              <div className="col-span-2">
                <label className="block text-sm font-medium text-foreground mb-1.5">
                  Genetics <span className="text-rose-400">*</span>
                </label>
                <GeneticsSelector
                  genetics={genetics}
                  selectedId={selectedGeneticsId}
                  onSelect={handleSelectGenetics}
                  isLoading={geneticsLoading}
                />
                {genetics.length === 0 && !geneticsLoading && (
                  <p className="mt-1.5 text-xs text-amber-400">
                    No genetics in library.{' '}
                    <a href="/library/genetics" className="underline hover:text-amber-300">
                      Add genetics first
                    </a>
                  </p>
                )}
              </div>

              {/* Plant Count */}
              <div>
                <label className="block text-sm font-medium text-foreground mb-1.5">
                  Plant Count <span className="text-rose-400">*</span>
                </label>
                <input
                  type="number"
                  value={plantCount}
                  onChange={(e) => setPlantCount(parseInt(e.target.value) || 0)}
                  min={1}
                  className="w-full px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground focus:outline-none focus:border-cyan-500/50"
                />
              </div>

              {/* Start Date */}
              <div>
                <label className="block text-sm font-medium text-foreground mb-1.5">
                  Start Date <span className="text-rose-400">*</span>
                </label>
                <div className="relative">
                  <Calendar className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
                  <input
                    type="date"
                    value={format(startDate, 'yyyy-MM-dd')}
                    onChange={(e) => setStartDate(new Date(e.target.value))}
                    className="w-full pl-10 pr-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground focus:outline-none focus:border-cyan-500/50"
                  />
                </div>
              </div>
            </div>

            {/* Configuration Mode Tabs */}
            <div>
              <div className="flex items-center gap-1 p-1 bg-muted/30 rounded-lg mb-4">
                <button
                  type="button"
                  onClick={() => setConfigMode('blueprint')}
                  className={cn(
                    'flex-1 flex items-center justify-center gap-2 px-4 py-2.5 rounded-md text-sm font-medium transition-all',
                    configMode === 'blueprint'
                      ? 'bg-surface text-foreground shadow-sm'
                      : 'text-muted-foreground hover:text-foreground'
                  )}
                >
                  <Layers className="w-4 h-4" />
                  Use Blueprint
                </button>
                <button
                  type="button"
                  onClick={() => setConfigMode('custom')}
                  className={cn(
                    'flex-1 flex items-center justify-center gap-2 px-4 py-2.5 rounded-md text-sm font-medium transition-all',
                    configMode === 'custom'
                      ? 'bg-surface text-foreground shadow-sm'
                      : 'text-muted-foreground hover:text-foreground'
                  )}
                >
                  <Settings2 className="w-4 h-4" />
                  Custom Configuration
                </button>
              </div>

              {/* Tab Content */}
              {configMode === 'blueprint' ? (
                <BlueprintTab
                  selectedGeneticsId={selectedGeneticsId}
                  selectedGeneticsName={selectedGenetics?.name || null}
                  selectedBlueprintId={selectedBlueprintId}
                  onSelectBlueprint={setSelectedBlueprintId}
                />
              ) : (
                <CustomConfigTab
                  startDate={startDate}
                  rooms={rooms}
                  phaseConfigs={phaseConfigs}
                  onPhaseConfigChange={handlePhaseConfigChange}
                />
              )}
            </div>

            {/* Timeline Preview (only for blueprint mode when selected) */}
            {configMode === 'blueprint' && selectedBlueprintId && (
              <div className="flex items-center justify-between p-3 bg-surface border border-border rounded-lg">
                <div className="flex items-center gap-2 text-sm">
                  <Calendar className="w-4 h-4 text-cyan-400" />
                  <span className="text-foreground font-medium">{totalDays} days total</span>
                </div>
                <div className="flex items-center gap-2 text-sm text-muted-foreground">
                  <span>Ends</span>
                  <ChevronRight className="w-3 h-3" />
                  <span className="text-foreground font-medium">
                    {format(estimatedEndDate, 'MMM d, yyyy')}
                  </span>
                </div>
              </div>
            )}
          </div>

          {/* Footer */}
          <div className="flex items-center justify-end gap-3 px-6 py-4 border-t border-border bg-muted/10 shrink-0">
            <button
              type="button"
              onClick={handleClose}
              className="px-4 py-2 text-sm font-medium text-muted-foreground hover:text-foreground transition-colors"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={!canSubmit}
              className={cn(
                'px-6 py-2 rounded-lg text-sm font-medium transition-all',
                canSubmit
                  ? 'bg-cyan-500 text-black hover:bg-cyan-400 shadow-lg shadow-cyan-500/20'
                  : 'bg-muted text-muted-foreground cursor-not-allowed'
              )}
            >
              Create Batch
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default NewBatchModal;
