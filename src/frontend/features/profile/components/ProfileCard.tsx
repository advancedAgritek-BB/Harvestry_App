'use client';

import React from 'react';

interface ProfileCardProps {
  title: string;
  rightSlot?: React.ReactNode;
  children: React.ReactNode;
}

export function ProfileCard({ title, rightSlot, children }: ProfileCardProps) {
  return (
    <section className="rounded-2xl border border-[var(--border)] bg-[var(--bg-surface)] overflow-hidden">
      <header className="flex items-center justify-between px-6 py-4 border-b border-[var(--border)] bg-[var(--bg-tile)]">
        <h2 className="text-sm font-semibold text-[var(--text-primary)]">{title}</h2>
        {rightSlot}
      </header>
      <div className="p-6">{children}</div>
    </section>
  );
}
