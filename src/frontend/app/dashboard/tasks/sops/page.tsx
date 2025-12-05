'use client';

/**
 * SOPs (Standard Operating Procedures) Page
 * Manage organization-wide SOPs that can be attached to tasks
 */

import { useState } from 'react';
import { Plus, FileText, Edit, Trash2, Eye, Search } from 'lucide-react';
import type { SopSummary, CreateSopRequest } from '@/features/tasks/types/sop.types';
import { CreateSopModal } from '@/features/tasks/components';

// Mock SOPs data
const MOCK_SOPS: SopSummary[] = [
  {
    id: 'sop1',
    title: 'IPM Inspection Protocol',
    category: 'Cultivation',
    version: 3,
    isActive: true,
    updatedAt: '2024-01-20T10:30:00Z',
  },
  {
    id: 'sop2',
    title: 'Defoliation Best Practices',
    category: 'Cultivation',
    version: 2,
    isActive: true,
    updatedAt: '2024-01-18T14:00:00Z',
  },
  {
    id: 'sop3',
    title: 'Harvest Readiness Assessment',
    category: 'Harvest',
    version: 1,
    isActive: true,
    updatedAt: '2024-01-15T09:00:00Z',
  },
  {
    id: 'sop4',
    title: 'Nutrient Mixing Guidelines',
    category: 'Irrigation',
    version: 4,
    isActive: true,
    updatedAt: '2024-01-22T16:45:00Z',
  },
  {
    id: 'sop5',
    title: 'Clone Room Sanitation',
    category: 'Sanitation',
    version: 2,
    isActive: false,
    updatedAt: '2024-01-10T11:00:00Z',
  },
  {
    id: 'sop6',
    title: 'pH Meter Calibration',
    category: 'Equipment',
    version: 1,
    isActive: true,
    updatedAt: '2024-01-12T08:30:00Z',
  },
];

const CATEGORIES = ['All', 'Cultivation', 'Harvest', 'Irrigation', 'Sanitation', 'Equipment', 'Compliance'];

export default function SopsPage() {
  const [sops, setSops] = useState<SopSummary[]>(MOCK_SOPS);
  const [searchQuery, setSearchQuery] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('All');
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);

  const handleCreateSop = async (data: CreateSopRequest) => {
    // In production, this would call an API
    const newSop: SopSummary = {
      id: `sop${Date.now()}`,
      title: data.title,
      category: data.category,
      version: 1,
      isActive: true,
      updatedAt: new Date().toISOString(),
    };
    setSops((prev) => [newSop, ...prev]);
  };

  const filteredSops = sops.filter(sop => {
    const matchesSearch = sop.title.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesCategory = categoryFilter === 'All' || sop.category === categoryFilter;
    return matchesSearch && matchesCategory;
  });

  const formatDate = (dateStr: string) => {
    return new Date(dateStr).toLocaleDateString(undefined, {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  };

  return (
    <div className="space-y-6">
      {/* Search and Filters */}
      <div className="flex items-center gap-4">
        <div className="relative flex-1 max-w-md">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-[var(--text-muted)]" />
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            placeholder="Search SOPs..."
            className="w-full pl-10 pr-4 py-2 rounded-lg border border-[var(--border)] bg-[var(--bg-elevated)] text-[var(--text-primary)] text-sm placeholder:text-[var(--text-subtle)] focus:outline-none focus:border-[var(--accent-cyan)]"
          />
        </div>

        <div className="flex items-center gap-2">
          {CATEGORIES.map(category => (
            <button
              key={category}
              onClick={() => setCategoryFilter(category)}
              className={`px-3 py-1.5 text-sm rounded-full transition-colors ${
                categoryFilter === category
                  ? 'bg-[var(--accent-cyan)] text-white'
                  : 'bg-[var(--bg-tile)] text-[var(--text-muted)] hover:text-[var(--text-primary)]'
              }`}
            >
              {category}
            </button>
          ))}
        </div>

        <button 
          onClick={() => setIsCreateModalOpen(true)}
          className="flex items-center gap-2 px-4 py-2 bg-[var(--accent-cyan)] text-white rounded-lg hover:opacity-90 transition-all font-medium ml-auto"
        >
          <Plus className="w-4 h-4" />
          New SOP
        </button>
      </div>

      {/* Results Count */}
      <p className="text-sm text-[var(--text-muted)]">
        {filteredSops.length} SOP{filteredSops.length !== 1 ? 's' : ''} found
      </p>

      {/* SOPs Table */}
      <div className="card-premium rounded-lg overflow-hidden">
        <table className="w-full">
          <thead className="bg-[var(--bg-tile)]">
            <tr>
              <th className="px-4 py-3 text-left text-xs font-medium text-[var(--text-muted)] uppercase">Title</th>
              <th className="px-4 py-3 text-left text-xs font-medium text-[var(--text-muted)] uppercase">Category</th>
              <th className="px-4 py-3 text-left text-xs font-medium text-[var(--text-muted)] uppercase">Version</th>
              <th className="px-4 py-3 text-left text-xs font-medium text-[var(--text-muted)] uppercase">Status</th>
              <th className="px-4 py-3 text-left text-xs font-medium text-[var(--text-muted)] uppercase">Last Updated</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-[var(--text-muted)] uppercase">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-[var(--border)]">
            {filteredSops.map(sop => (
              <tr key={sop.id} className="hover:bg-[var(--bg-tile-hover)] transition-colors">
                <td className="px-4 py-4">
                  <div className="flex items-center gap-3">
                    <div className="p-2 bg-sky-500/15 rounded-lg">
                      <FileText className="w-4 h-4 text-sky-400" />
                    </div>
                    <span className="font-medium text-[var(--text-primary)]">{sop.title}</span>
                  </div>
                </td>
                <td className="px-4 py-4">
                  <span className="px-2 py-1 bg-[var(--bg-tile)] text-[var(--text-muted)] text-xs rounded-full">
                    {sop.category}
                  </span>
                </td>
                <td className="px-4 py-4">
                  <span className="text-sm text-[var(--text-muted)]">v{sop.version}</span>
                </td>
                <td className="px-4 py-4">
                  <span className={`px-2 py-1 text-xs rounded-full ${
                    sop.isActive 
                      ? 'bg-emerald-500/15 text-emerald-400'
                      : 'bg-[var(--bg-tile)] text-[var(--text-subtle)]'
                  }`}>
                    {sop.isActive ? 'Active' : 'Inactive'}
                  </span>
                </td>
                <td className="px-4 py-4 text-sm text-[var(--text-muted)]">
                  {formatDate(sop.updatedAt)}
                </td>
                <td className="px-4 py-4">
                  <div className="flex items-center justify-end gap-1">
                    <button className="p-2 rounded-lg text-[var(--text-muted)] hover:text-[var(--text-primary)] hover:bg-[var(--bg-tile)] transition-colors" title="View">
                      <Eye className="w-4 h-4" />
                    </button>
                    <button className="p-2 rounded-lg text-[var(--text-muted)] hover:text-[var(--text-primary)] hover:bg-[var(--bg-tile)] transition-colors" title="Edit">
                      <Edit className="w-4 h-4" />
                    </button>
                    <button className="p-2 rounded-lg text-[var(--text-muted)] hover:text-rose-400 hover:bg-rose-500/15 transition-colors" title="Delete">
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        {filteredSops.length === 0 && (
          <div className="text-center py-12 text-[var(--text-muted)]">
            <FileText className="w-12 h-12 mx-auto mb-4 opacity-50" />
            <p>No SOPs found matching your criteria</p>
          </div>
        )}
      </div>

      {/* Create SOP Modal */}
      <CreateSopModal
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
        onSubmit={handleCreateSop}
        categories={CATEGORIES.filter((c) => c !== 'All')}
      />
    </div>
  );
}
