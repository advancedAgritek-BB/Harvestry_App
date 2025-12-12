'use client';

import React, { useMemo } from 'react';
import { CalendarDays } from 'lucide-react';
import { useAuthStore } from '@/stores/auth/authStore';
import { useProfileStore } from '@/stores/profile/profileStore';
import type { ShiftScheduleItem } from '../types/profile.types';
import { ProfileCard } from './ProfileCard';
import { TierUnavailableState } from './TierUnavailableState';

function startOfWeekMonday(date: Date): Date {
  const d = new Date(date);
  const day = d.getDay(); // 0=Sun
  const diff = (day === 0 ? -6 : 1) - day;
  d.setDate(d.getDate() + diff);
  d.setHours(0, 0, 0, 0);
  return d;
}

function addDays(date: Date, days: number): Date {
  const d = new Date(date);
  d.setDate(d.getDate() + days);
  return d;
}

function toYmd(date: Date): string {
  const yyyy = date.getFullYear();
  const mm = String(date.getMonth() + 1).padStart(2, '0');
  const dd = String(date.getDate()).padStart(2, '0');
  return `${yyyy}-${mm}-${dd}`;
}

function formatDayLabel(date: Date): string {
  return date.toLocaleDateString(undefined, { weekday: 'short', month: 'short', day: 'numeric' });
}

function groupByDate(items: ShiftScheduleItem[]): Record<string, ShiftScheduleItem[]> {
  return items.reduce<Record<string, ShiftScheduleItem[]>>((acc, item) => {
    acc[item.shiftDate] = acc[item.shiftDate] || [];
    acc[item.shiftDate].push(item);
    return acc;
  }, {});
}

export function WeeklyScheduleCard() {
  const user = useAuthStore((s) => s.user);
  const canUseSchedule = useAuthStore((s) => s.hasFeature('production_planning'));

  const byUserId = useProfileStore((s) => s.byUserId);
  const snapshot = user ? byUserId[user.id] : undefined;

  const week = useMemo(() => {
    const monday = startOfWeekMonday(new Date());
    return Array.from({ length: 7 }).map((_, idx) => addDays(monday, idx));
  }, []);

  const grouped = useMemo(() => groupByDate(snapshot?.weeklySchedule ?? []), [snapshot?.weeklySchedule]);

  return (
    <ProfileCard title="Weekly Schedule">
      {!canUseSchedule ? (
        <TierUnavailableState
          title="Schedule"
          description="Scheduling is not available in your current tier."
        />
      ) : !user ? (
        <div className="text-sm text-[var(--text-muted)]">Sign in to see your schedule.</div>
      ) : !snapshot ? (
        <div className="text-sm text-[var(--text-muted)]">Loading schedule…</div>
      ) : (
        <div className="space-y-2">
          {week.map((date) => {
            const key = toYmd(date);
            const dayItems = grouped[key] ?? [];

            return (
              <div
                key={key}
                className="rounded-xl border border-[var(--border)] bg-[var(--bg-elevated)] p-4"
              >
                <div className="flex items-center gap-2">
                  <CalendarDays className="w-4 h-4 text-[var(--text-muted)]" />
                  <div className="text-sm font-semibold text-[var(--text-primary)]">
                    {formatDayLabel(date)}
                  </div>
                </div>

                {dayItems.length === 0 ? (
                  <div className="mt-2 text-sm text-[var(--text-muted)]">No shift assigned.</div>
                ) : (
                  <div className="mt-3 space-y-2">
                    {dayItems.map((shift) => (
                      <div key={shift.id} className="flex items-start justify-between gap-4">
                        <div className="min-w-0">
                          <div className="text-sm text-[var(--text-primary)]">
                            <span className="font-semibold">{shift.startTime}</span>–{shift.endTime}
                            {shift.roomCode ? (
                              <span className="ml-2 text-xs text-[var(--text-muted)]">• {shift.roomCode}</span>
                            ) : null}
                          </div>
                          {shift.task ? (
                            <div className="text-xs text-[var(--text-muted)] truncate">{shift.task}</div>
                          ) : null}
                        </div>
                        <span className="text-xs px-2 py-1 rounded-full border border-[var(--border)] bg-[var(--bg-tile)] text-[var(--text-muted)] capitalize">
                          {shift.status}
                        </span>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}
    </ProfileCard>
  );
}
