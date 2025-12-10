'use client';

import React from 'react';
import { cn } from '@/lib/utils';

export type BadgeStatus = 'success' | 'warning' | 'error' | 'neutral' | 'info';

interface StatusBadgeProps {
  status: BadgeStatus;
  label: string;
  size?: 'sm' | 'md';
  dot?: boolean;
  className?: string;
}

const STATUS_STYLES: Record<BadgeStatus, { bg: string; text: string; dot: string }> = {
  success: {
    bg: 'bg-emerald-500/15',
    text: 'text-emerald-400',
    dot: 'bg-emerald-400',
  },
  warning: {
    bg: 'bg-amber-500/15',
    text: 'text-amber-400',
    dot: 'bg-amber-400',
  },
  error: {
    bg: 'bg-rose-500/15',
    text: 'text-rose-400',
    dot: 'bg-rose-400',
  },
  neutral: {
    bg: 'bg-white/[0.06]',
    text: 'text-muted-foreground',
    dot: 'bg-muted-foreground',
  },
  info: {
    bg: 'bg-cyan-500/15',
    text: 'text-cyan-400',
    dot: 'bg-cyan-400',
  },
};

export function StatusBadge({
  status,
  label,
  size = 'sm',
  dot = false,
  className,
}: StatusBadgeProps) {
  const styles = STATUS_STYLES[status];

  return (
    <span
      className={cn(
        'inline-flex items-center gap-1.5 rounded-full font-medium',
        styles.bg,
        styles.text,
        size === 'sm' ? 'px-2 py-0.5 text-xs' : 'px-2.5 py-1 text-sm',
        className
      )}
    >
      {dot && (
        <span className={cn('w-1.5 h-1.5 rounded-full', styles.dot)} />
      )}
      {label}
    </span>
  );
}

export default StatusBadge;


