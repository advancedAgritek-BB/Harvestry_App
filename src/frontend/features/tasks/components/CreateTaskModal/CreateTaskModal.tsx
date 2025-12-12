'use client';

/**
 * CreateTaskModal Component
 * Modal for creating new tasks with template selection, SOP picker, and assignee selection
 */

import { useState, useEffect } from 'react';
import { X } from 'lucide-react';
import type { TaskLibraryItem, SopSummary } from '../../types/sop.types';
import type { TaskAssignee, TaskPriority } from '../../types/task.types';
import { TeamMemberPicker } from '@/features/labor/components/TeamMemberPicker';
import type { AssignableMember } from '@/features/labor/types/team.types';

interface CreateTaskModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: CreateTaskFormData) => Promise<void>;
  siteId: string;
  templates?: TaskLibraryItem[];
  sops?: SopSummary[];
  /** @deprecated Use TeamMemberPicker instead - this prop is kept for backwards compatibility */
  assignees?: TaskAssignee[];
  isLoading?: boolean;
}

export interface CreateTaskFormData {
  title: string;
  description?: string;
  priority: TaskPriority;
  assignedToUserId?: string;
  assignedToRole?: string;
  dueDate?: string;
  requiredSopIds?: string[];
  saveAsTemplate?: boolean;
  templateName?: string;
}

export function CreateTaskModal({
  isOpen,
  onClose,
  onSubmit,
  siteId,
  templates = [],
  sops = [],
  assignees = [],
  isLoading = false,
}: CreateTaskModalProps) {
  const [formData, setFormData] = useState<CreateTaskFormData>({
    title: '',
    description: '',
    priority: 'normal',
    requiredSopIds: [],
  });
  const [selectedTemplate, setSelectedTemplate] = useState<string>('');
  const [saveAsTemplate, setSaveAsTemplate] = useState(false);
  const [templateName, setTemplateName] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isOpen) {
      setFormData({
        title: '',
        description: '',
        priority: 'normal',
        requiredSopIds: [],
      });
      setSelectedTemplate('');
      setSaveAsTemplate(false);
      setTemplateName('');
      setError(null);
    }
  }, [isOpen]);

  const handleTemplateSelect = (templateId: string) => {
    setSelectedTemplate(templateId);
    const template = templates.find(t => t.id === templateId);
    if (template) {
      setFormData(prev => ({
        ...prev,
        title: template.title,
        description: template.description || '',
        priority: template.defaultPriority as TaskPriority,
        assignedToRole: template.defaultAssignedToRole,
        requiredSopIds: template.defaultSopIds,
      }));
    }
  };

  const handleSopToggle = (sopId: string) => {
    setFormData(prev => ({
      ...prev,
      requiredSopIds: prev.requiredSopIds?.includes(sopId)
        ? prev.requiredSopIds.filter(id => id !== sopId)
        : [...(prev.requiredSopIds || []), sopId],
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    
    if (!formData.title.trim()) {
      setError('Title is required');
      return;
    }

    setSubmitting(true);
    try {
      await onSubmit({
        ...formData,
        saveAsTemplate,
        templateName: saveAsTemplate ? templateName : undefined,
      });
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create task');
    } finally {
      setSubmitting(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      {/* Backdrop */}
      <div 
        className="absolute inset-0 bg-black/60 backdrop-blur-sm"
        onClick={onClose}
      />
      
      {/* Modal */}
      <div className="relative task-detail-modal rounded-xl shadow-2xl w-full max-w-2xl max-h-[90vh] overflow-hidden">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-[var(--border)]">
          <h2 className="text-xl font-semibold text-[var(--text-primary)]">Create Task</h2>
          <button
            onClick={onClose}
            className="text-[var(--text-muted)] hover:text-[var(--text-primary)] transition-colors p-1"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="overflow-y-auto max-h-[calc(90vh-140px)]">
          <div className="p-6 space-y-6">
            {/* Template Selector */}
            {templates.length > 0 && (
              <div>
                <label className="block text-sm font-medium text-[var(--text-muted)] mb-2">
                  Start from Template (optional)
                </label>
                <select
                  value={selectedTemplate}
                  onChange={(e) => handleTemplateSelect(e.target.value)}
                  className="w-full rounded-lg border border-[var(--border)] bg-[var(--bg-elevated)] text-[var(--text-primary)] px-3 py-2 focus:outline-none focus:border-[var(--accent-cyan)]"
                >
                  <option value="">Select a template...</option>
                  {templates.map(template => (
                    <option key={template.id} value={template.id}>
                      {template.title}
                    </option>
                  ))}
                </select>
              </div>
            )}

            {/* Title */}
            <div>
              <label className="block text-sm font-medium text-[var(--text-muted)] mb-2">
                Title *
              </label>
              <input
                type="text"
                value={formData.title}
                onChange={(e) => setFormData(prev => ({ ...prev, title: e.target.value }))}
                className="w-full rounded-lg border border-[var(--border)] bg-[var(--bg-elevated)] text-[var(--text-primary)] px-3 py-2 focus:outline-none focus:border-[var(--accent-cyan)] placeholder:text-[var(--text-subtle)]"
                placeholder="Enter task title"
                required
              />
            </div>

            {/* Description */}
            <div>
              <label className="block text-sm font-medium text-[var(--text-muted)] mb-2">
                Description
              </label>
              <textarea
                value={formData.description}
                onChange={(e) => setFormData(prev => ({ ...prev, description: e.target.value }))}
                rows={3}
                className="w-full rounded-lg border border-[var(--border)] bg-[var(--bg-elevated)] text-[var(--text-primary)] px-3 py-2 focus:outline-none focus:border-[var(--accent-cyan)] placeholder:text-[var(--text-subtle)] resize-none"
                placeholder="Enter task description"
              />
            </div>

            {/* Priority & Due Date */}
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-[var(--text-muted)] mb-2">
                  Priority
                </label>
                <select
                  value={formData.priority}
                  onChange={(e) => setFormData(prev => ({ ...prev, priority: e.target.value as TaskPriority }))}
                  className="w-full rounded-lg border border-[var(--border)] bg-[var(--bg-elevated)] text-[var(--text-primary)] px-3 py-2 focus:outline-none focus:border-[var(--accent-cyan)]"
                >
                  <option value="low">Low</option>
                  <option value="normal">Normal</option>
                  <option value="high">High</option>
                  <option value="critical">Critical</option>
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-[var(--text-muted)] mb-2">
                  Due Date
                </label>
                <input
                  type="datetime-local"
                  value={formData.dueDate}
                  onChange={(e) => setFormData(prev => ({ ...prev, dueDate: e.target.value }))}
                  className="w-full rounded-lg border border-[var(--border)] bg-[var(--bg-elevated)] text-[var(--text-primary)] px-3 py-2 focus:outline-none focus:border-[var(--accent-cyan)]"
                />
              </div>
            </div>

            {/* Assignee - Team Member Picker */}
            <div>
              <label className="block text-sm font-medium text-[var(--text-muted)] mb-2">
                Assign To
              </label>
              <TeamMemberPicker
                siteId={siteId}
                selectedUserId={formData.assignedToUserId}
                onSelect={(member: AssignableMember | null) => {
                  setFormData(prev => ({
                    ...prev,
                    assignedToUserId: member?.userId,
                    assignedToRole: member?.role,
                  }));
                }}
                placeholder="Select team member..."
              />
            </div>

            {/* SOPs */}
            {sops.length > 0 && (
              <div>
                <label className="block text-sm font-medium text-[var(--text-muted)] mb-2">
                  Required SOPs
                </label>
                <div className="space-y-2 max-h-40 overflow-y-auto border border-[var(--border)] rounded-lg p-3 bg-[var(--bg-tile)]">
                  {sops.map(sop => (
                    <label key={sop.id} className="flex items-center gap-3 cursor-pointer hover:bg-[var(--bg-tile-hover)] rounded p-1 -mx-1">
                      <input
                        type="checkbox"
                        checked={formData.requiredSopIds?.includes(sop.id) || false}
                        onChange={() => handleSopToggle(sop.id)}
                        className="rounded border-[var(--border)] bg-[var(--bg-elevated)] text-[var(--accent-cyan)] focus:ring-[var(--accent-cyan)] focus:ring-offset-0"
                      />
                      <span className="text-sm text-[var(--text-primary)]">{sop.title}</span>
                      {sop.category && (
                        <span className="text-xs text-[var(--text-subtle)]">({sop.category})</span>
                      )}
                    </label>
                  ))}
                </div>
              </div>
            )}

            {/* Save as Template */}
            <div className="border-t border-[var(--border)] pt-4">
              <label className="flex items-center gap-3 cursor-pointer">
                <input
                  type="checkbox"
                  checked={saveAsTemplate}
                  onChange={(e) => setSaveAsTemplate(e.target.checked)}
                  className="rounded border-[var(--border)] bg-[var(--bg-elevated)] text-[var(--accent-cyan)] focus:ring-[var(--accent-cyan)] focus:ring-offset-0"
                />
                <span className="text-sm text-[var(--text-muted)]">
                  Save as template for future use
                </span>
              </label>
              
              {saveAsTemplate && (
                <input
                  type="text"
                  value={templateName}
                  onChange={(e) => setTemplateName(e.target.value)}
                  className="mt-3 w-full rounded-lg border border-[var(--border)] bg-[var(--bg-elevated)] text-[var(--text-primary)] px-3 py-2 focus:outline-none focus:border-[var(--accent-cyan)] placeholder:text-[var(--text-subtle)]"
                  placeholder="Template name"
                />
              )}
            </div>

            {error && (
              <div className="text-rose-400 text-sm bg-rose-500/10 border border-rose-500/20 rounded-lg px-3 py-2">{error}</div>
            )}
          </div>

          {/* Footer */}
          <div className="flex items-center justify-end gap-3 px-6 py-4 bg-[var(--bg-surface)] border-t border-[var(--border)]">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-sm font-medium text-[var(--text-muted)] hover:text-[var(--text-primary)] border border-[var(--border)] rounded-lg hover:bg-[var(--bg-tile)] transition-colors"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={submitting || isLoading}
              className="px-4 py-2 text-sm font-medium text-white bg-[var(--accent-cyan)] rounded-lg hover:opacity-90 disabled:opacity-50 disabled:cursor-not-allowed transition-all"
            >
              {submitting ? 'Creating...' : 'Create Task'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
