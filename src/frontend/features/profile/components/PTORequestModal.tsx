'use client';

import React, { useEffect, useMemo, useState } from 'react';
import { X } from 'lucide-react';
import { useProfileStore } from '@/stores/profile/profileStore';
import type { CreatePTORequestInput, PTORequestType } from '../types/profile.types';

interface PTORequestModalProps {
  isOpen: boolean;
  userId: string;
  onClose: () => void;
  onCreated?: (requestId: string) => void;
}

const PTO_TYPES: Array<{ value: PTORequestType; label: string }> = [
  { value: 'vacation', label: 'Vacation' },
  { value: 'sick', label: 'Sick' },
  { value: 'personal', label: 'Personal' },
];

function todayYmd(): string {
  const d = new Date();
  const yyyy = d.getFullYear();
  const mm = String(d.getMonth() + 1).padStart(2, '0');
  const dd = String(d.getDate()).padStart(2, '0');
  return `${yyyy}-${mm}-${dd}`;
}

export function PTORequestModal({ isOpen, userId, onClose, onCreated }: PTORequestModalProps) {
  const createPtoRequest = useProfileStore((s) => s.createPtoRequest);

  const [type, setType] = useState<PTORequestType>('vacation');
  const [startDate, setStartDate] = useState<string>('');
  const [endDate, setEndDate] = useState<string>('');
  const [reason, setReason] = useState<string>('');
  const [error, setError] = useState<string | null>(null);

  const minDate = useMemo(() => todayYmd(), []);

  useEffect(() => {
    if (!isOpen) return;
    // Reset each time modal opens
    setType('vacation');
    setStartDate('');
    setEndDate('');
    setReason('');
    setError(null);
  }, [isOpen]);

  if (!isOpen) return null;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    const trimmedReason = reason.trim();

    const input: CreatePTORequestInput = {
      type,
      startDate,
      endDate,
      reason: trimmedReason.length > 0 ? trimmedReason : undefined,
    };

    const result = createPtoRequest({ userId, input });
    if (!result.ok) {
      setError(result.error);
      return;
    }

    onCreated?.(result.requestId);
    onClose();
  };

  return (
    <>
      <div className="fixed inset-0 z-50 bg-black/40" onClick={onClose} />
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
        <div className="w-full max-w-lg rounded-2xl border border-[var(--border)] bg-[var(--bg-surface)] shadow-2xl overflow-hidden">
          <header className="flex items-center justify-between px-6 py-4 border-b border-[var(--border)] bg-[var(--bg-tile)]">
            <div>
              <div className="text-sm font-semibold text-[var(--text-primary)]">Request time off</div>
              <div className="text-xs text-[var(--text-muted)]">Submit a PTO request for approval</div>
            </div>
            <button
              type="button"
              onClick={onClose}
              className="p-2 rounded-lg hover:bg-[var(--bg-tile-hover)] transition-colors"
              aria-label="Close"
            >
              <X className="w-4 h-4 text-[var(--text-muted)]" />
            </button>
          </header>

          <form onSubmit={handleSubmit} className="p-6 space-y-4">
            {error && (
              <div className="rounded-xl border border-rose-500/30 bg-rose-500/10 p-3 text-sm text-rose-300">
                {error}
              </div>
            )}

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label
                  htmlFor="pto-type"
                  className="block text-xs font-semibold text-[var(--text-muted)] mb-1"
                >
                  Type
                </label>
                <select
                  id="pto-type"
                  value={type}
                  onChange={(e) => setType(e.target.value as PTORequestType)}
                  className="w-full h-10 rounded-xl bg-[var(--bg-elevated)] border border-[var(--border)] px-3 text-sm text-[var(--text-primary)] focus:outline-none focus:ring-1 focus:ring-cyan-500/50"
                >
                  {PTO_TYPES.map((t) => (
                    <option key={t.value} value={t.value}>
                      {t.label}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label
                  htmlFor="pto-reason"
                  className="block text-xs font-semibold text-[var(--text-muted)] mb-1"
                >
                  Reason (optional)
                </label>
                <input
                  id="pto-reason"
                  value={reason}
                  onChange={(e) => setReason(e.target.value)}
                  placeholder="e.g., Vacation, appointment"
                  className="w-full h-10 rounded-xl bg-[var(--bg-elevated)] border border-[var(--border)] px-3 text-sm text-[var(--text-primary)] placeholder:text-[var(--text-muted)]/40 focus:outline-none focus:ring-1 focus:ring-cyan-500/50"
                />
              </div>
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label
                  htmlFor="pto-start-date"
                  className="block text-xs font-semibold text-[var(--text-muted)] mb-1"
                >
                  Start date
                </label>
                <input
                  id="pto-start-date"
                  type="date"
                  min={minDate}
                  value={startDate}
                  onChange={(e) => setStartDate(e.target.value)}
                  required
                  className="w-full h-10 rounded-xl bg-[var(--bg-elevated)] border border-[var(--border)] px-3 text-sm text-[var(--text-primary)] focus:outline-none focus:ring-1 focus:ring-cyan-500/50"
                />
              </div>
              <div>
                <label
                  htmlFor="pto-end-date"
                  className="block text-xs font-semibold text-[var(--text-muted)] mb-1"
                >
                  End date
                </label>
                <input
                  id="pto-end-date"
                  type="date"
                  min={startDate || minDate}
                  value={endDate}
                  onChange={(e) => setEndDate(e.target.value)}
                  required
                  className="w-full h-10 rounded-xl bg-[var(--bg-elevated)] border border-[var(--border)] px-3 text-sm text-[var(--text-primary)] focus:outline-none focus:ring-1 focus:ring-cyan-500/50"
                />
              </div>
            </div>

            <div className="flex items-center justify-end gap-2 pt-2">
              <button
                type="button"
                onClick={onClose}
                className="px-4 py-2 rounded-xl border border-[var(--border)] bg-[var(--bg-elevated)] text-sm font-semibold text-[var(--text-primary)] hover:bg-[var(--bg-tile-hover)] transition-colors"
              >
                Cancel
              </button>
              <button
                type="submit"
                className="px-4 py-2 rounded-xl bg-[var(--accent-cyan)] text-sm font-semibold text-white hover:opacity-90 transition-opacity"
              >
                Submit request
              </button>
            </div>
          </form>
        </div>
      </div>
    </>
  );
}
