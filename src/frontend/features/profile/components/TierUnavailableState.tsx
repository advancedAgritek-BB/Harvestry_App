'use client';

import React from 'react';

interface TierUnavailableStateProps {
  title?: string;
  description?: string;
}

export function TierUnavailableState({
  title = 'Unavailable',
  description = 'This section is not available in your current tier.',
}: TierUnavailableStateProps) {
  return (
    <div className="rounded-xl border border-[var(--border)] bg-[var(--bg-elevated)] p-4">
      <div className="text-sm font-semibold text-[var(--text-primary)]">{title}</div>
      <div className="mt-1 text-sm text-[var(--text-muted)]">{description}</div>
    </div>
  );
}
