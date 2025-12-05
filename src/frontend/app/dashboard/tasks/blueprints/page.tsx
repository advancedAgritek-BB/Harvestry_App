'use client';

/**
 * Task Blueprints Page
 * Manage automated task generation rules
 */

import { useState } from 'react';
import { Plus, Play, Pause, Trash2, Edit, Zap } from 'lucide-react';
import type { TaskBlueprint, GrowthPhaseType, BlueprintRoomType } from '@/features/tasks/types/blueprint.types';

// Mock blueprints data
const MOCK_BLUEPRINTS: TaskBlueprint[] = [
  {
    id: 'bp1',
    siteId: 's1',
    title: 'Week 1 Flower Stretch Check',
    description: 'Inspect plants for stretch management needs during first week of flower',
    growthPhase: 'flowering',
    roomType: 'flower',
    priority: 'high',
    timeOffsetHours: 168, // 7 days after phase start
    assignedToRole: 'Cultivator',
    isActive: true,
    createdByUserId: 'u1',
    createdAt: '2024-01-15T00:00:00Z',
    updatedAt: '2024-01-15T00:00:00Z',
    requiredSopIds: ['sop1'],
    requiredTrainingIds: [],
  },
  {
    id: 'bp2',
    siteId: 's1',
    title: 'Week 3 Defoliation',
    description: 'Standard defoliation task for week 3 of flower',
    growthPhase: 'flowering',
    roomType: 'flower',
    priority: 'high',
    timeOffsetHours: 504, // 21 days
    assignedToRole: 'Cultivator',
    isActive: true,
    createdByUserId: 'u1',
    createdAt: '2024-01-15T00:00:00Z',
    updatedAt: '2024-01-15T00:00:00Z',
    requiredSopIds: ['sop2'],
    requiredTrainingIds: [],
  },
  {
    id: 'bp3',
    siteId: 's1',
    title: 'Transplant to Veg',
    description: 'Move rooted clones to vegetative room',
    growthPhase: 'vegetative',
    roomType: 'veg',
    priority: 'critical',
    timeOffsetHours: 0, // Immediately on phase start
    assignedToRole: 'Cultivator',
    isActive: true,
    createdByUserId: 'u1',
    createdAt: '2024-01-10T00:00:00Z',
    updatedAt: '2024-01-10T00:00:00Z',
    requiredSopIds: [],
    requiredTrainingIds: [],
  },
  {
    id: 'bp4',
    siteId: 's1',
    title: 'Harvest Readiness Check',
    description: 'Final trichome inspection before harvest',
    growthPhase: 'flowering',
    roomType: 'flower',
    strainId: 'strain1',
    priority: 'critical',
    timeOffsetHours: 1344, // 56 days
    assignedToRole: 'Lead Cultivator',
    isActive: false,
    createdByUserId: 'u1',
    createdAt: '2024-01-05T00:00:00Z',
    updatedAt: '2024-01-20T00:00:00Z',
    requiredSopIds: ['sop3'],
    requiredTrainingIds: ['train1'],
  },
];

const PHASE_LABELS: Record<GrowthPhaseType, string> = {
  any: 'Any Phase',
  immature: 'Immature',
  vegetative: 'Vegetative',
  flowering: 'Flowering',
  mother: 'Mother',
  harvested: 'Harvested',
  drying: 'Drying',
  curing: 'Curing',
};

const ROOM_LABELS: Record<BlueprintRoomType, string> = {
  any: 'Any Room',
  veg: 'Veg',
  flower: 'Flower',
  mother: 'Mother',
  clone: 'Clone',
  dry: 'Dry',
  cure: 'Cure',
  extraction: 'Extraction',
  manufacturing: 'Manufacturing',
  vault: 'Vault',
};

export default function BlueprintsPage() {
  const [blueprints, setBlueprints] = useState<TaskBlueprint[]>(MOCK_BLUEPRINTS);

  const handleToggleActive = (blueprintId: string) => {
    setBlueprints(prev =>
      prev.map(bp =>
        bp.id === blueprintId ? { ...bp, isActive: !bp.isActive } : bp
      )
    );
  };

  const formatOffset = (hours: number) => {
    if (hours === 0) return 'Immediately';
    if (hours < 24) return `${hours} hours`;
    const days = Math.floor(hours / 24);
    return `Day ${days}`;
  };

  const activeCount = blueprints.filter(bp => bp.isActive).length;

  return (
    <div className="space-y-6">
      {/* Header Stats */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <div className="flex items-center gap-2 px-3 py-1.5 bg-emerald-500/15 text-emerald-400 rounded-full">
            <Zap className="w-4 h-4" />
            <span className="text-sm font-medium">{activeCount} Active Blueprints</span>
          </div>
          <span className="text-sm text-[var(--text-muted)]">
            {blueprints.length - activeCount} inactive
          </span>
        </div>

        <button className="flex items-center gap-2 px-4 py-2 bg-[var(--accent-cyan)] text-white rounded-lg hover:opacity-90 transition-all font-medium">
          <Plus className="w-4 h-4" />
          New Blueprint
        </button>
      </div>

      {/* Blueprints Grid */}
      <div className="grid gap-4 md:grid-cols-2">
        {blueprints.map(blueprint => (
          <div
            key={blueprint.id}
            className={`p-5 rounded-xl border transition-all ${
              blueprint.isActive
                ? 'tile-premium hover:border-emerald-500/50 hover:shadow-lg'
                : 'bg-[var(--bg-tile)]/50 border-[var(--border)]/50 opacity-75'
            }`}
          >
            {/* Header */}
            <div className="flex items-start justify-between mb-3">
              <div className="flex-1">
                <h3 className="font-semibold text-[var(--text-primary)]">{blueprint.title}</h3>
                {blueprint.description && (
                  <p className="text-sm text-[var(--text-muted)] mt-1 line-clamp-2">
                    {blueprint.description}
                  </p>
                )}
              </div>
              <div className={`px-2 py-1 rounded text-xs font-medium ${
                blueprint.priority === 'critical' ? 'bg-rose-500/15 text-rose-400' :
                blueprint.priority === 'high' ? 'bg-orange-500/15 text-orange-400' :
                'bg-sky-500/15 text-sky-400'
              }`}>
                {blueprint.priority}
              </div>
            </div>

            {/* Matching Criteria */}
            <div className="flex flex-wrap gap-2 mb-4">
              <span className="px-2 py-1 bg-violet-500/15 text-violet-400 text-xs rounded-full">
                {PHASE_LABELS[blueprint.growthPhase]}
              </span>
              <span className="px-2 py-1 bg-cyan-500/15 text-cyan-400 text-xs rounded-full">
                {ROOM_LABELS[blueprint.roomType]}
              </span>
              <span className="px-2 py-1 bg-amber-500/15 text-amber-400 text-xs rounded-full">
                {formatOffset(blueprint.timeOffsetHours)}
              </span>
              {blueprint.strainId && (
                <span className="px-2 py-1 bg-pink-500/15 text-pink-400 text-xs rounded-full">
                  Strain-specific
                </span>
              )}
            </div>

            {/* Footer */}
            <div className="flex items-center justify-between pt-3 border-t border-[var(--border)]">
              <div className="text-xs text-[var(--text-muted)]">
                {blueprint.assignedToRole && (
                  <span>Assigns to: {blueprint.assignedToRole}</span>
                )}
              </div>
              
              <div className="flex items-center gap-1">
                <button
                  onClick={() => handleToggleActive(blueprint.id)}
                  className={`p-2 rounded-lg transition-colors ${
                    blueprint.isActive
                      ? 'text-emerald-400 hover:bg-emerald-500/15'
                      : 'text-[var(--text-muted)] hover:bg-[var(--bg-tile)]'
                  }`}
                  title={blueprint.isActive ? 'Deactivate' : 'Activate'}
                >
                  {blueprint.isActive ? <Pause className="w-4 h-4" /> : <Play className="w-4 h-4" />}
                </button>
                <button className="p-2 rounded-lg text-[var(--text-muted)] hover:text-[var(--text-primary)] hover:bg-[var(--bg-tile)] transition-colors" title="Edit">
                  <Edit className="w-4 h-4" />
                </button>
                <button className="p-2 rounded-lg text-[var(--text-muted)] hover:text-rose-400 hover:bg-rose-500/15 transition-colors" title="Delete">
                  <Trash2 className="w-4 h-4" />
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>

      {blueprints.length === 0 && (
        <div className="text-center py-12 text-[var(--text-muted)]">
          <Zap className="w-12 h-12 mx-auto mb-4 opacity-50" />
          <p className="font-medium">No blueprints yet</p>
          <p className="text-sm mt-1">Create your first blueprint to automate task generation</p>
        </div>
      )}
    </div>
  );
}
