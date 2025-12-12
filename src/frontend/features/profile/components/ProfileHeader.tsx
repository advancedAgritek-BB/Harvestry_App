'use client';

import React from 'react';
import Link from 'next/link';
import { Mail } from 'lucide-react';
import { useAuthStore } from '@/stores/auth/authStore';

export function ProfileHeader() {
  const user = useAuthStore((s) => s.user);

  if (!user) {
    return (
      <div className="rounded-2xl border border-[var(--border)] bg-[var(--bg-surface)] p-6">
        <div className="text-lg font-semibold text-[var(--text-primary)]">My Profile</div>
        <div className="mt-1 text-sm text-[var(--text-muted)]">
          You’re not signed in.
        </div>
        <Link
          href="/login"
          className="inline-flex mt-4 px-4 py-2 rounded-lg bg-[var(--accent-cyan)] text-white font-medium hover:opacity-90 transition-opacity"
        >
          Sign in
        </Link>
      </div>
    );
  }

  return (
    <div className="rounded-2xl border border-[var(--border)] bg-[var(--bg-surface)] p-6">
      <div className="flex items-start gap-4">
        <img
          src={user.avatarUrl || '/images/user-avatar.png'}
          alt="User avatar"
          className="w-16 h-16 rounded-2xl object-cover ring-1 ring-[var(--border)]"
        />

        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-3">
            <h1 className="text-xl font-bold text-[var(--text-primary)] truncate">
              {user.name}
            </h1>
            <span className="text-xs font-semibold px-2 py-1 rounded-full bg-[var(--bg-tile)] border border-[var(--border)] text-[var(--text-muted)]">
              {user.role}
            </span>
          </div>

          <div className="mt-2 flex items-center gap-2 text-sm text-[var(--text-muted)]">
            <Mail className="w-4 h-4" />
            <span className="truncate">{user.email}</span>
          </div>

          {user.sitePermissions?.length > 0 && (
            <div className="mt-3 flex flex-wrap gap-2">
              {user.sitePermissions.map((sp) => (
                <span
                  key={sp.siteId}
                  className="text-xs px-2 py-1 rounded-full bg-[var(--bg-tile)] border border-[var(--border)] text-[var(--text-primary)]"
                >
                  {sp.siteName}
                </span>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
