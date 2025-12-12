'use client';

import React, { useEffect } from 'react';
import Link from 'next/link';
import { useAuthStore } from '@/stores/auth/authStore';
import { useProfileStore } from '@/stores/profile/profileStore';
import {
  AssignedTasksCard,
  PTOCard,
  ProfileHeader,
  WeeklyScheduleCard,
} from '@/features/profile/components';

export default function ProfilePage() {
  const user = useAuthStore((s) => s.user);
  const currentSiteId = useAuthStore((s) => s.currentSiteId);

  const seedProfileIfMissing = useProfileStore((s) => s.seedProfileIfMissing);

  useEffect(() => {
    if (!user) return;
    const siteId = currentSiteId || user.sitePermissions?.[0]?.siteId || 'site-1';
    seedProfileIfMissing({ userId: user.id, siteId });
  }, [currentSiteId, seedProfileIfMissing, user]);

  if (!user) {
    return (
      <div className="p-6">
        <div className="rounded-2xl border border-[var(--border)] bg-[var(--bg-surface)] p-6">
          <div className="text-lg font-semibold text-[var(--text-primary)]">My Profile</div>
          <div className="mt-1 text-sm text-[var(--text-muted)]">Please sign in to view your profile.</div>
          <Link
            href="/login"
            className="inline-flex mt-4 px-4 py-2 rounded-lg bg-[var(--accent-cyan)] text-white font-medium hover:opacity-90 transition-opacity"
          >
            Sign in
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6 p-6">
      <ProfileHeader />

      <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
        <AssignedTasksCard />
        <WeeklyScheduleCard />
      </div>

      <PTOCard />
    </div>
  );
}
