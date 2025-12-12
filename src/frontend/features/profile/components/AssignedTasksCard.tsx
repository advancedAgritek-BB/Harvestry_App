'use client';

import React, { useMemo } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { ArrowRight, ClipboardCheck } from 'lucide-react';
import { useAuthStore } from '@/stores/auth/authStore';
import { MOCK_TASKS } from '@/app/dashboard/tasks/mockData';
import type { Task } from '@/features/tasks/types/task.types';
import { ProfileCard } from './ProfileCard';
import { TierUnavailableState } from './TierUnavailableState';

function formatDue(dateIso: string): string {
  const d = new Date(dateIso);
  if (Number.isNaN(d.getTime())) return '—';
  return d.toLocaleString(undefined, { month: 'short', day: 'numeric', hour: 'numeric', minute: '2-digit' });
}

function nameToAssignee(name: string): { firstName: string; lastName: string } {
  const parts = name.trim().split(/\s+/);
  if (parts.length <= 1) return { firstName: name.trim(), lastName: '' };
  return { firstName: parts[0] || '', lastName: parts.slice(1).join(' ') };
}

function normalizeAssignedTasks(params: { userId: string; userName: string }): Task[] {
  const direct = MOCK_TASKS.filter((t) => t.assignee?.id === params.userId);
  if (direct.length > 0) return direct;

  // Mock fallback: if task mock IDs don't match auth IDs, remap "u1" tasks to the current user.
  const fallback = MOCK_TASKS.filter((t) => t.assignee?.id === 'u1');
  if (fallback.length === 0) return [];

  const { firstName, lastName } = nameToAssignee(params.userName);

  return fallback.map((t) => ({
    ...t,
    assignee: t.assignee
      ? {
          ...t.assignee,
          id: params.userId,
          firstName,
          lastName,
        }
      : undefined,
  }));
}

export function AssignedTasksCard() {
  const router = useRouter();
  const user = useAuthStore((s) => s.user);
  const canUseTasks = useAuthStore((s) => s.hasFeature('task_management'));

  const assignedTasks = useMemo(() => {
    if (!user) return [];
    return normalizeAssignedTasks({ userId: user.id, userName: user.name });
  }, [user]);

  const topTasks = assignedTasks.slice(0, 4);

  return (
    <ProfileCard
      title="My Tasks"
      rightSlot={
        <Link
          href="/dashboard/tasks"
          className="text-xs font-semibold text-[var(--accent-cyan)] hover:underline inline-flex items-center gap-1"
        >
          View all <ArrowRight className="w-3 h-3" />
        </Link>
      }
    >
      {!canUseTasks ? (
        <TierUnavailableState title="Tasks" description="Task management is not available in your current tier." />
      ) : !user ? (
        <div className="text-sm text-[var(--text-muted)]">Sign in to see tasks assigned to you.</div>
      ) : topTasks.length === 0 ? (
        <div className="text-sm text-[var(--text-muted)]">No tasks assigned.</div>
      ) : (
        <div className="space-y-2">
          {topTasks.map((task) => (
            <button
              key={task.id}
              type="button"
              onClick={() => router.push(`/dashboard/tasks?taskId=${task.id}`)}
              className="w-full text-left rounded-xl border border-[var(--border)] bg-[var(--bg-elevated)] hover:bg-[var(--bg-tile-hover)] transition-colors p-4"
            >
              <div className="flex items-start gap-3">
                <div className="mt-0.5 text-[var(--accent-cyan)]">
                  <ClipboardCheck className="w-4 h-4" />
                </div>
                <div className="flex-1 min-w-0">
                  <div className="text-sm font-semibold text-[var(--text-primary)] truncate">
                    {task.title}
                  </div>
                  <div className="mt-1 flex items-center gap-3 text-xs text-[var(--text-muted)]">
                    <span className="capitalize">{task.status.replace(/_/g, ' ')}</span>
                    <span>•</span>
                    <span>Due {formatDue(task.dueAt)}</span>
                  </div>
                </div>
              </div>
            </button>
          ))}
        </div>
      )}
    </ProfileCard>
  );
}
