'use client';

import React, { useMemo, useState } from 'react';
import { CalendarClock, Plus } from 'lucide-react';
import { useAuthStore } from '@/stores/auth/authStore';
import { useProfileStore } from '@/stores/profile/profileStore';
import type { PTORequest } from '../types/profile.types';
import { ProfileCard } from './ProfileCard';
import { PTORequestModal } from './PTORequestModal';

function statusBadgeClasses(status: PTORequest['status']): string {
  switch (status) {
    case 'approved':
      return 'border-emerald-500/30 bg-emerald-500/10 text-emerald-300';
    case 'denied':
      return 'border-rose-500/30 bg-rose-500/10 text-rose-300';
    case 'cancelled':
      return 'border-[var(--border)] bg-[var(--bg-tile)] text-[var(--text-muted)]';
    case 'pending':
    default:
      return 'border-amber-500/30 bg-amber-500/10 text-amber-300';
  }
}

function formatRange(start: string, end: string): string {
  if (start === end) return start;
  return `${start} → ${end}`;
}

export function PTOCard() {
  const user = useAuthStore((s) => s.user);

  const byUserId = useProfileStore((s) => s.byUserId);
  const cancelPtoRequest = useProfileStore((s) => s.cancelPtoRequest);

  const snapshot = user ? byUserId[user.id] : undefined;

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [feedback, setFeedback] = useState<string | null>(null);

  const requests = useMemo(() => snapshot?.ptoRequests ?? [], [snapshot?.ptoRequests]);

  if (!user) {
    return (
      <ProfileCard title="PTO & Time Off">
        <div className="text-sm text-[var(--text-muted)]">Sign in to manage your PTO.</div>
      </ProfileCard>
    );
  }

  if (!snapshot) {
    return (
      <ProfileCard title="PTO & Time Off">
        <div className="text-sm text-[var(--text-muted)]">Loading PTO…</div>
      </ProfileCard>
    );
  }

  const { ptoBalance } = snapshot;

  const handleCancel = (requestId: string) => {
    setFeedback(null);
    const result = cancelPtoRequest({ userId: user.id, requestId });
    if (!result.ok) {
      setFeedback(result.error);
      return;
    }
    setFeedback('Request cancelled.');
  };

  return (
    <>
      <ProfileCard
        title="PTO & Time Off"
        rightSlot={
          <button
            type="button"
            onClick={() => {
              setFeedback(null);
              setIsModalOpen(true);
            }}
            className="inline-flex items-center gap-2 px-3 py-1.5 rounded-lg bg-[var(--accent-cyan)] text-white text-xs font-semibold hover:opacity-90 transition-opacity"
          >
            <Plus className="w-3.5 h-3.5" />
            Request time off
          </button>
        }
      >
        {feedback && (
          <div className="mb-4 rounded-xl border border-[var(--border)] bg-[var(--bg-elevated)] p-3 text-sm text-[var(--text-primary)]">
            {feedback}
          </div>
        )}

        <div className="grid grid-cols-2 lg:grid-cols-4 gap-3">
          <div className="rounded-xl border border-[var(--border)] bg-[var(--bg-elevated)] p-4">
            <div className="text-xs text-[var(--text-muted)]">Available</div>
            <div className="mt-1 text-lg font-bold text-[var(--text-primary)]">{ptoBalance.availableDays}</div>
          </div>
          <div className="rounded-xl border border-[var(--border)] bg-[var(--bg-elevated)] p-4">
            <div className="text-xs text-[var(--text-muted)]">Pending</div>
            <div className="mt-1 text-lg font-bold text-[var(--text-primary)]">{ptoBalance.pendingDays}</div>
          </div>
          <div className="rounded-xl border border-[var(--border)] bg-[var(--bg-elevated)] p-4">
            <div className="text-xs text-[var(--text-muted)]">Used</div>
            <div className="mt-1 text-lg font-bold text-[var(--text-primary)]">{ptoBalance.usedDays}</div>
          </div>
          <div className="rounded-xl border border-[var(--border)] bg-[var(--bg-elevated)] p-4">
            <div className="text-xs text-[var(--text-muted)]">Allowance</div>
            <div className="mt-1 text-lg font-bold text-[var(--text-primary)]">{ptoBalance.totalAllowance}</div>
          </div>
        </div>

        <div className="mt-6">
          <div className="text-sm font-semibold text-[var(--text-primary)] mb-3">Requests</div>

          {requests.length === 0 ? (
            <div className="text-sm text-[var(--text-muted)]">No PTO requests yet.</div>
          ) : (
            <div className="space-y-2">
              {requests.slice(0, 6).map((r) => (
                <div
                  key={r.id}
                  className="rounded-xl border border-[var(--border)] bg-[var(--bg-elevated)] p-4"
                >
                  <div className="flex items-start justify-between gap-4">
                    <div className="min-w-0">
                      <div className="flex items-center gap-2">
                        <CalendarClock className="w-4 h-4 text-[var(--text-muted)]" />
                        <div className="text-sm font-semibold text-[var(--text-primary)]">
                          {formatRange(r.startDate, r.endDate)}
                        </div>
                      </div>
                      <div className="mt-1 text-xs text-[var(--text-muted)] capitalize">
                        {r.type}{r.reason ? ` • ${r.reason}` : ''}
                      </div>
                    </div>

                    <div className="flex items-center gap-2">
                      <span
                        className={`text-xs px-2 py-1 rounded-full border ${statusBadgeClasses(r.status)} capitalize`}
                      >
                        {r.status}
                      </span>

                      {r.status === 'pending' ? (
                        <button
                          type="button"
                          onClick={() => handleCancel(r.id)}
                          className="text-xs font-semibold text-rose-300 hover:underline"
                        >
                          Cancel
                        </button>
                      ) : null}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </ProfileCard>

      <PTORequestModal
        isOpen={isModalOpen}
        userId={user.id}
        onClose={() => setIsModalOpen(false)}
        onCreated={() => setFeedback('Request submitted for approval.')}
      />
    </>
  );
}
