'use client';

import React from 'react';
import { cn } from '@/lib/utils';

export type CardVariant = 'default' | 'emerald' | 'amber' | 'rose' | 'cyan' | 'violet';

interface PlannerCardProps {
  children: React.ReactNode;
  variant?: CardVariant;
  className?: string;
  hoverable?: boolean;
  onClick?: () => void;
}

const VARIANT_STYLES: Record<CardVariant, string> = {
  default: 'from-white/5 to-transparent',
  emerald: 'from-emerald-500/10 to-transparent',
  amber: 'from-amber-500/10 to-transparent',
  rose: 'from-rose-500/10 to-transparent',
  cyan: 'from-cyan-500/10 to-transparent',
  violet: 'from-violet-500/10 to-transparent',
};

export function PlannerCard({
  children,
  variant = 'default',
  className,
  hoverable = false,
  onClick,
}: PlannerCardProps) {
  const Component = onClick ? 'button' : 'div';

  return (
    <Component
      onClick={onClick}
      className={cn(
        'relative overflow-hidden rounded-2xl p-5',
        'bg-[var(--bg-surface)]/60 backdrop-blur-sm',
        'bg-gradient-to-br',
        VARIANT_STYLES[variant],
        'border border-white/[0.06]',
        'shadow-lg shadow-black/10',
        hoverable && [
          'transition-all duration-300 cursor-pointer',
          'hover:shadow-xl hover:-translate-y-0.5',
          'hover:border-white/10',
        ],
        onClick && 'text-left w-full',
        className
      )}
    >
      {children}
    </Component>
  );
}

export default PlannerCard;


