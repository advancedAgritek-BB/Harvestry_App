'use client';

/**
 * Task Templates Page
 * Manage reusable task templates (Task Library)
 */

import { useState } from 'react';
import { Plus, Copy, Edit, Trash2, Bookmark, Search } from 'lucide-react';
import type { TaskLibraryItem } from '@/features/tasks/types/sop.types';

// Mock templates data
const MOCK_TEMPLATES: TaskLibraryItem[] = [
  {
    id: 'tpl1',
    orgId: 'org1',
    title: 'Weekly IPM Inspection',
    description: 'Standard weekly integrated pest management inspection for all cultivation rooms',
    defaultPriority: 'high',
    taskType: 'cultivation',
    defaultAssignedToRole: 'Cultivator',
    defaultDueDaysOffset: 7,
    isActive: true,
    createdByUserId: 'u1',
    createdAt: '2024-01-10T00:00:00Z',
    updatedAt: '2024-01-15T00:00:00Z',
    defaultSopIds: ['sop1'],
  },
  {
    id: 'tpl2',
    orgId: 'org1',
    title: 'pH Sensor Calibration',
    description: 'Monthly calibration of all pH sensors in irrigation system',
    defaultPriority: 'normal',
    taskType: 'maintenance',
    defaultAssignedToRole: 'Maintenance Tech',
    defaultDueDaysOffset: 30,
    isActive: true,
    createdByUserId: 'u1',
    createdAt: '2024-01-08T00:00:00Z',
    updatedAt: '2024-01-08T00:00:00Z',
    defaultSopIds: ['sop6'],
  },
  {
    id: 'tpl3',
    orgId: 'org1',
    title: 'Nutrient Tank Refill',
    description: 'Refill and check nutrient stock tanks',
    defaultPriority: 'high',
    taskType: 'irrigation',
    defaultAssignedToRole: 'Irrigation Specialist',
    isActive: true,
    createdByUserId: 'u2',
    createdAt: '2024-01-12T00:00:00Z',
    updatedAt: '2024-01-12T00:00:00Z',
    defaultSopIds: ['sop4'],
  },
  {
    id: 'tpl4',
    orgId: 'org1',
    title: 'End of Day Room Lockup',
    description: 'Standard end of day security check and room lockup procedure',
    defaultPriority: 'normal',
    taskType: 'compliance',
    defaultAssignedToRole: 'Lead Cultivator',
    isActive: false,
    createdByUserId: 'u1',
    createdAt: '2024-01-05T00:00:00Z',
    updatedAt: '2024-01-20T00:00:00Z',
    defaultSopIds: [],
  },
];

const PRIORITY_STYLES: Record<string, string> = {
  low: 'bg-[var(--bg-tile)] text-[var(--text-subtle)]',
  normal: 'bg-sky-500/15 text-sky-400',
  high: 'bg-orange-500/15 text-orange-400',
  critical: 'bg-rose-500/15 text-rose-400',
};

const TYPE_STYLES: Record<string, string> = {
  cultivation: 'bg-emerald-500/15 text-emerald-400',
  irrigation: 'bg-cyan-500/15 text-cyan-400',
  harvest: 'bg-amber-500/15 text-amber-400',
  processing: 'bg-purple-500/15 text-purple-400',
  maintenance: 'bg-slate-500/15 text-slate-400',
  compliance: 'bg-rose-500/15 text-rose-400',
  quality: 'bg-indigo-500/15 text-indigo-400',
  custom: 'bg-[var(--bg-tile)] text-[var(--text-muted)]',
};

export default function TemplatesPage() {
  const [templates] = useState<TaskLibraryItem[]>(MOCK_TEMPLATES);
  const [searchQuery, setSearchQuery] = useState('');

  const filteredTemplates = templates.filter(template =>
    template.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
    template.description?.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const activeCount = templates.filter(t => t.isActive).length;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <div className="relative flex-1 max-w-md">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-[var(--text-muted)]" />
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            placeholder="Search templates..."
            className="w-full pl-10 pr-4 py-2 rounded-lg border border-[var(--border)] bg-[var(--bg-elevated)] text-[var(--text-primary)] text-sm placeholder:text-[var(--text-subtle)] focus:outline-none focus:border-[var(--accent-cyan)]"
          />
        </div>

        <div className="flex items-center gap-2 text-sm text-[var(--text-muted)]">
          <Bookmark className="w-4 h-4" />
          <span>{activeCount} active templates</span>
        </div>

        <button className="flex items-center gap-2 px-4 py-2 bg-[var(--accent-cyan)] text-white rounded-lg hover:opacity-90 transition-all font-medium ml-auto">
          <Plus className="w-4 h-4" />
          New Template
        </button>
      </div>

      {/* Templates Grid */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {filteredTemplates.map(template => (
          <div
            key={template.id}
            className={`p-5 rounded-xl border transition-all ${
              template.isActive
                ? 'tile-premium hover:shadow-lg hover:border-emerald-500/50'
                : 'bg-[var(--bg-tile)]/50 border-[var(--border)]/50 opacity-60'
            }`}
          >
            {/* Header */}
            <div className="flex items-start justify-between mb-3">
              <h3 className="font-semibold text-[var(--text-primary)] leading-tight">{template.title}</h3>
              <span className={`px-2 py-0.5 text-xs rounded-full ${PRIORITY_STYLES[template.defaultPriority] || PRIORITY_STYLES.normal}`}>
                {template.defaultPriority}
              </span>
            </div>

            {/* Description */}
            {template.description && (
              <p className="text-sm text-[var(--text-muted)] mb-4 line-clamp-2">
                {template.description}
              </p>
            )}

            {/* Tags */}
            <div className="flex flex-wrap gap-2 mb-4">
              <span className={`px-2 py-1 text-xs rounded-full ${TYPE_STYLES[template.taskType] || TYPE_STYLES.custom}`}>
                {template.taskType}
              </span>
              {template.defaultAssignedToRole && (
                <span className="px-2 py-1 bg-violet-500/15 text-violet-400 text-xs rounded-full">
                  {template.defaultAssignedToRole}
                </span>
              )}
              {template.defaultDueDaysOffset && (
                <span className="px-2 py-1 bg-amber-500/15 text-amber-400 text-xs rounded-full">
                  Due in {template.defaultDueDaysOffset}d
                </span>
              )}
            </div>

            {/* SOPs indicator */}
            {template.defaultSopIds.length > 0 && (
              <div className="text-xs text-[var(--text-muted)] mb-3">
                {template.defaultSopIds.length} SOP{template.defaultSopIds.length > 1 ? 's' : ''} attached
              </div>
            )}

            {/* Actions */}
            <div className="flex items-center justify-between pt-3 border-t border-[var(--border)]">
              <button className="flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-emerald-400 hover:bg-emerald-500/15 rounded-lg transition-colors">
                <Copy className="w-3.5 h-3.5" />
                Use Template
              </button>
              
              <div className="flex items-center gap-1">
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

      {filteredTemplates.length === 0 && (
        <div className="text-center py-12 text-[var(--text-muted)]">
          <Bookmark className="w-12 h-12 mx-auto mb-4 opacity-50" />
          <p className="font-medium">No templates found</p>
          <p className="text-sm mt-1">Create a template to quickly assign common tasks</p>
        </div>
      )}
    </div>
  );
}
