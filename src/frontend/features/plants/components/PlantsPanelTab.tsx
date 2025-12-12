'use client';

import React, { useEffect } from 'react';
import { cn } from '@/lib/utils';
import {
  Sprout,
  Leaf,
  Flower2,
  TrendingDown,
  PlayCircle,
  Plus,
  ArrowRight,
  Tag,
  List,
  RefreshCw,
  AlertCircle,
  CheckCircle,
  Clock,
} from 'lucide-react';
import { usePlantStore } from '../stores';
import {
  GROWTH_PHASE_CONFIG,
  type PlantCounts,
  type PlantLossRecord,
  type PlantBatch,
} from '../types';

// =============================================================================
// TYPES
// =============================================================================

interface PlantsPanelTabProps {
  batchId: string;
  batchStatus: 'planned' | 'active' | 'completed' | 'cancelled';
  plannedPlantCount: number;
}

// =============================================================================
// SUMMARY CARD COMPONENT
// =============================================================================

interface SummaryCardProps {
  counts: PlantCounts;
  plannedCount: number;
}

function SummaryCard({ counts, plannedCount }: SummaryCardProps) {
  const healthPercentage = counts.started > 0 
    ? Math.round((counts.current / counts.started) * 100) 
    : 0;
  
  const lossPercentage = counts.started > 0 
    ? Math.round((counts.destroyed / counts.started) * 100) 
    : 0;

  return (
    <div className="p-4 bg-muted/30 rounded-xl border border-border/50">
      {/* Main Stats */}
      <div className="grid grid-cols-4 gap-3 mb-4">
        <div className="text-center">
          <div className="text-xs text-muted-foreground mb-1">Planned</div>
          <div className="text-lg font-semibold text-foreground">{plannedCount}</div>
        </div>
        <div className="text-center">
          <div className="text-xs text-muted-foreground mb-1">Started</div>
          <div className="text-lg font-semibold text-cyan-400">{counts.started}</div>
        </div>
        <div className="text-center">
          <div className="text-xs text-muted-foreground mb-1">Current</div>
          <div className="text-lg font-semibold text-emerald-400">{counts.current}</div>
        </div>
        <div className="text-center">
          <div className="text-xs text-muted-foreground mb-1">Lost</div>
          <div className="text-lg font-semibold text-red-400">{counts.destroyed}</div>
        </div>
      </div>

      {/* Health Bar */}
      <div className="mb-3">
        <div className="flex items-center justify-between text-xs mb-1">
          <span className="text-muted-foreground">Survival Rate</span>
          <span className={cn(
            healthPercentage >= 90 ? 'text-emerald-400' :
            healthPercentage >= 75 ? 'text-amber-400' : 'text-red-400'
          )}>
            {healthPercentage}%
          </span>
        </div>
        <div className="h-2 bg-muted rounded-full overflow-hidden">
          <div 
            className="h-full bg-gradient-to-r from-emerald-500 to-emerald-400 transition-all"
            style={{ width: `${healthPercentage}%` }}
          />
        </div>
      </div>

      {/* Phase Breakdown */}
      <div className="flex items-center gap-2 text-xs">
        <div className="flex items-center gap-1.5 px-2 py-1 rounded bg-emerald-500/10">
          <Sprout className="w-3 h-3 text-emerald-400" />
          <span className="text-emerald-400">{counts.immature}</span>
        </div>
        <ArrowRight className="w-3 h-3 text-muted-foreground" />
        <div className="flex items-center gap-1.5 px-2 py-1 rounded bg-blue-500/10">
          <Leaf className="w-3 h-3 text-blue-400" />
          <span className="text-blue-400">{counts.vegetative}</span>
        </div>
        <ArrowRight className="w-3 h-3 text-muted-foreground" />
        <div className="flex items-center gap-1.5 px-2 py-1 rounded bg-violet-500/10">
          <Flower2 className="w-3 h-3 text-violet-400" />
          <span className="text-violet-400">{counts.flowering}</span>
        </div>
      </div>

      {/* Tag Status */}
      {counts.current > 0 && (
        <div className="flex items-center justify-between mt-3 pt-3 border-t border-border/50 text-xs">
          <div className="flex items-center gap-1.5">
            <Tag className="w-3 h-3 text-muted-foreground" />
            <span className="text-muted-foreground">Tagged:</span>
            <span className="text-foreground">{counts.tagged}</span>
          </div>
          <div className="flex items-center gap-1.5">
            <span className="text-muted-foreground">Untagged:</span>
            <span className={counts.untagged > 0 ? 'text-amber-400' : 'text-foreground'}>
              {counts.untagged}
            </span>
          </div>
        </div>
      )}
    </div>
  );
}

// =============================================================================
// ACTIONS SECTION COMPONENT
// =============================================================================

interface ActionsSectionProps {
  batchId: string;
  batchStatus: 'planned' | 'active' | 'completed' | 'cancelled';
  isStarted: boolean;
  hasUntaggedPlants: boolean;
  currentPhase: string;
}

function ActionsSection({ 
  batchId, 
  batchStatus, 
  isStarted,
  hasUntaggedPlants,
  currentPhase,
}: ActionsSectionProps) {
  const { 
    openStartBatchModal, 
    openRecordLossModal,
    openTransitionModal,
    openAssignTagsModal,
  } = usePlantStore();

  // Planned batch - show Start Batch button
  if (batchStatus === 'planned' || !isStarted) {
    return (
      <div className="space-y-3">
        <button
          onClick={() => openStartBatchModal(batchId)}
          className="w-full flex items-center justify-center gap-2 px-4 py-3 bg-cyan-500 hover:bg-cyan-400 text-black rounded-lg font-medium transition-colors"
        >
          <PlayCircle className="w-5 h-5" />
          Start Batch
        </button>
        <p className="text-xs text-muted-foreground text-center">
          Confirm starting plant count and source to begin tracking
        </p>
      </div>
    );
  }

  // Active batch - show management actions
  return (
    <div className="space-y-2">
      <h4 className="text-xs text-muted-foreground uppercase tracking-wider mb-2">
        Quick Actions
      </h4>
      
      <div className="grid grid-cols-2 gap-2">
        <button
          onClick={() => openRecordLossModal(batchId)}
          className="flex items-center justify-center gap-2 px-3 py-2.5 bg-red-500/10 hover:bg-red-500/20 text-red-400 rounded-lg text-sm font-medium transition-colors border border-red-500/20"
        >
          <TrendingDown className="w-4 h-4" />
          Record Loss
        </button>
        
        <button
          onClick={() => openTransitionModal(batchId)}
          className="flex items-center justify-center gap-2 px-3 py-2.5 bg-violet-500/10 hover:bg-violet-500/20 text-violet-400 rounded-lg text-sm font-medium transition-colors border border-violet-500/20"
        >
          <ArrowRight className="w-4 h-4" />
          Transition
        </button>
      </div>

      {hasUntaggedPlants && (
        <button
          onClick={() => openAssignTagsModal(batchId)}
          className="w-full flex items-center justify-center gap-2 px-3 py-2.5 bg-amber-500/10 hover:bg-amber-500/20 text-amber-400 rounded-lg text-sm font-medium transition-colors border border-amber-500/20"
        >
          <Tag className="w-4 h-4" />
          Assign METRC Tags
        </button>
      )}
    </div>
  );
}

// =============================================================================
// LOSS HISTORY COMPONENT
// =============================================================================

interface LossHistoryProps {
  records: PlantLossRecord[];
}

function LossHistory({ records }: LossHistoryProps) {
  if (records.length === 0) {
    return (
      <div className="p-4 text-center text-sm text-muted-foreground">
        <CheckCircle className="w-8 h-8 mx-auto mb-2 text-emerald-400/50" />
        No losses recorded
      </div>
    );
  }

  return (
    <div className="space-y-2">
      <h4 className="text-xs text-muted-foreground uppercase tracking-wider">
        Loss History
      </h4>
      <div className="space-y-2 max-h-48 overflow-y-auto">
        {records.map((record) => (
          <div 
            key={record.id}
            className="p-3 bg-red-500/5 border border-red-500/20 rounded-lg"
          >
            <div className="flex items-center justify-between mb-1">
              <span className="text-sm font-medium text-red-400">
                -{record.quantity} plants
              </span>
              <span className="text-xs text-muted-foreground">
                {new Date(record.recordedAt).toLocaleDateString()}
              </span>
            </div>
            <div className="text-xs text-muted-foreground">
              {record.reason.replace('_', ' ').replace(/\b\w/g, l => l.toUpperCase())}
              {record.reasonNote && ` - ${record.reasonNote}`}
            </div>
            <div className="flex items-center gap-1.5 mt-1.5 text-xs">
              {record.metrcSynced ? (
                <span className="flex items-center gap-1 text-emerald-400">
                  <CheckCircle className="w-3 h-3" />
                  Synced
                </span>
              ) : (
                <span className="flex items-center gap-1 text-amber-400">
                  <Clock className="w-3 h-3" />
                  Pending
                </span>
              )}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

// =============================================================================
// MAIN COMPONENT
// =============================================================================

export function PlantsPanelTab({ 
  batchId, 
  batchStatus,
  plannedPlantCount,
}: PlantsPanelTabProps) {
  const { 
    loadPlantsForBatch,
    countsByBatch,
    lossHistoryByBatch,
    plantBatchesByBatch,
    isLoading,
    loadingBatchId,
    error,
  } = usePlantStore();

  // Load plant data when batch changes
  useEffect(() => {
    if (batchId) {
      loadPlantsForBatch(batchId);
    }
  }, [batchId, loadPlantsForBatch]);

  const counts = countsByBatch[batchId];
  const lossHistory = lossHistoryByBatch[batchId] || [];
  const plantBatches = plantBatchesByBatch[batchId] || [];
  const isStarted = plantBatches.length > 0;
  const isLoadingThis = isLoading && loadingBatchId === batchId;

  // Determine current phase from plant batches
  const currentPhase = plantBatches[0]?.growthPhase || 'immature';
  const hasUntaggedPlants = counts ? counts.untagged > 0 : false;

  if (isLoadingThis) {
    return (
      <div className="p-8 flex flex-col items-center justify-center">
        <RefreshCw className="w-8 h-8 text-cyan-400 animate-spin mb-3" />
        <p className="text-sm text-muted-foreground">Loading plant data...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="p-4 m-4 bg-red-500/10 border border-red-500/30 rounded-lg">
        <div className="flex items-center gap-2 text-red-400">
          <AlertCircle className="w-5 h-5" />
          <span className="text-sm font-medium">Error loading plants</span>
        </div>
        <p className="text-xs text-red-300/80 mt-1">{error}</p>
      </div>
    );
  }

  return (
    <div className="p-4 space-y-4">
      {/* Summary - only show if batch has started */}
      {isStarted && counts ? (
        <SummaryCard counts={counts} plannedCount={plannedPlantCount} />
      ) : (
        <div className="p-6 bg-muted/20 rounded-xl border border-border/50 text-center">
          <Sprout className="w-12 h-12 mx-auto mb-3 text-muted-foreground/50" />
          <h3 className="text-sm font-medium text-foreground mb-1">
            Batch Not Started
          </h3>
          <p className="text-xs text-muted-foreground mb-1">
            Target: <span className="text-foreground">{plannedPlantCount} plants</span>
          </p>
          <p className="text-xs text-muted-foreground">
            Start the batch to begin tracking plants
          </p>
        </div>
      )}

      {/* Actions */}
      <ActionsSection
        batchId={batchId}
        batchStatus={batchStatus}
        isStarted={isStarted}
        hasUntaggedPlants={hasUntaggedPlants}
        currentPhase={currentPhase}
      />

      {/* Loss History */}
      {isStarted && lossHistory.length > 0 && (
        <LossHistory records={lossHistory} />
      )}

      {/* METRC Status */}
      {isStarted && plantBatches.length > 0 && (
        <div className="p-3 bg-surface border border-border rounded-lg">
          <h4 className="text-xs text-muted-foreground uppercase tracking-wider mb-2">
            METRC Sync Status
          </h4>
          {plantBatches.map((pb) => (
            <div key={pb.id} className="flex items-center justify-between text-sm">
              <span className="text-foreground">{pb.name}</span>
              <span className={cn(
                'flex items-center gap-1 text-xs',
                pb.metrcSyncStatus === 'synced' && 'text-emerald-400',
                pb.metrcSyncStatus === 'pending' && 'text-amber-400',
                pb.metrcSyncStatus === 'error' && 'text-red-400'
              )}>
                {pb.metrcSyncStatus === 'synced' && <CheckCircle className="w-3 h-3" />}
                {pb.metrcSyncStatus === 'pending' && <Clock className="w-3 h-3" />}
                {pb.metrcSyncStatus === 'error' && <AlertCircle className="w-3 h-3" />}
                {pb.metrcSyncStatus ? pb.metrcSyncStatus.charAt(0).toUpperCase() + pb.metrcSyncStatus.slice(1) : '—'}
              </span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default PlantsPanelTab;





