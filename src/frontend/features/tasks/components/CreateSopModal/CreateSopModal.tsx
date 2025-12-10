'use client';

/**
 * CreateSopModal Component
 * Modal for creating new Standard Operating Procedures (SOPs)
 */

import { useState, useEffect } from 'react';
import { X } from 'lucide-react';
import type { CreateSopRequest } from '../../types/sop.types';

interface CreateSopModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: CreateSopRequest) => Promise<void>;
  categories?: string[];
  isLoading?: boolean;
}

const DEFAULT_CATEGORIES = [
  'Cultivation',
  'Harvest',
  'Irrigation',
  'Sanitation',
  'Equipment',
  'Compliance',
  'Processing',
  'Quality',
];

export function CreateSopModal({
  isOpen,
  onClose,
  onSubmit,
  categories = DEFAULT_CATEGORIES,
  isLoading = false,
}: CreateSopModalProps) {
  const [formData, setFormData] = useState<CreateSopRequest>({
    title: '',
    content: '',
    category: '',
  });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isOpen) {
      setFormData({
        title: '',
        content: '',
        category: '',
      });
      setError(null);
    }
  }, [isOpen]);

  // Handle escape key
  useEffect(() => {
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };

    if (isOpen) {
      document.addEventListener('keydown', handleEscape);
      document.body.style.overflow = 'hidden';
    }

    return () => {
      document.removeEventListener('keydown', handleEscape);
      document.body.style.overflow = 'unset';
    };
  }, [isOpen, onClose]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!formData.title.trim()) {
      setError('Title is required');
      return;
    }

    setSubmitting(true);
    try {
      await onSubmit(formData);
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create SOP');
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
          <h2 className="text-xl font-semibold text-[var(--text-primary)]">
            Create New SOP
          </h2>
          <button
            onClick={onClose}
            className="text-[var(--text-muted)] hover:text-[var(--text-primary)] transition-colors p-1"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="overflow-y-auto max-h-[calc(90vh-140px)]">
          <div className="p-6 space-y-6">
            {/* Title */}
            <div>
              <label className="block text-sm font-medium text-[var(--text-muted)] mb-2">
                Title *
              </label>
              <input
                type="text"
                value={formData.title}
                onChange={(e) =>
                  setFormData((prev) => ({ ...prev, title: e.target.value }))
                }
                className="w-full rounded-lg border border-[var(--border)] bg-[var(--bg-elevated)] text-[var(--text-primary)] px-3 py-2 focus:outline-none focus:border-[var(--accent-cyan)] placeholder:text-[var(--text-subtle)]"
                placeholder="Enter SOP title"
                required
                autoFocus
              />
            </div>

            {/* Category */}
            <div>
              <label className="block text-sm font-medium text-[var(--text-muted)] mb-2">
                Category
              </label>
              <select
                value={formData.category || ''}
                onChange={(e) =>
                  setFormData((prev) => ({
                    ...prev,
                    category: e.target.value || undefined,
                  }))
                }
                className="w-full rounded-lg border border-[var(--border)] bg-[var(--bg-elevated)] text-[var(--text-primary)] px-3 py-2 focus:outline-none focus:border-[var(--accent-cyan)]"
              >
                <option value="">Select a category...</option>
                {categories.map((category) => (
                  <option key={category} value={category}>
                    {category}
                  </option>
                ))}
              </select>
            </div>

            {/* Content/Description */}
            <div>
              <label className="block text-sm font-medium text-[var(--text-muted)] mb-2">
                Description / Initial Content
              </label>
              <textarea
                value={formData.content || ''}
                onChange={(e) =>
                  setFormData((prev) => ({
                    ...prev,
                    content: e.target.value || undefined,
                  }))
                }
                rows={6}
                className="w-full rounded-lg border border-[var(--border)] bg-[var(--bg-elevated)] text-[var(--text-primary)] px-3 py-2 focus:outline-none focus:border-[var(--accent-cyan)] placeholder:text-[var(--text-subtle)] resize-none"
                placeholder="Enter SOP description or initial content. You can add detailed steps after creation."
              />
              <p className="mt-1 text-xs text-[var(--text-subtle)]">
                Tip: You can add structured steps and detailed instructions after creating the SOP.
              </p>
            </div>

            {error && (
              <div className="text-rose-400 text-sm bg-rose-500/10 border border-rose-500/20 rounded-lg px-3 py-2">
                {error}
              </div>
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
              {submitting ? 'Creating...' : 'Create SOP'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}







