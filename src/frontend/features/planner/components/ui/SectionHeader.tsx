'use client';

import React from 'react';
import Link from 'next/link';
import { cn } from '@/lib/utils';
import { ChevronRight, LucideIcon } from 'lucide-react';

interface SectionHeaderProps {
  icon?: LucideIcon;
  title: string;
  subtitle?: string;
  actionLabel?: string;
  actionHref?: string;
  className?: string;
}

export function SectionHeader({
  icon: Icon,
  title,
  subtitle,
  actionLabel,
  actionHref,
  className,
}: SectionHeaderProps) {
  return (
    <div className={cn('flex items-center justify-between mb-4', className)}>
      <div className="flex items-center gap-3">
        {Icon && (
          <div className="p-2 rounded-xl bg-white/[0.04] ring-1 ring-white/[0.06]">
            <Icon className="w-4 h-4 text-muted-foreground" />
          </div>
        )}
        <div>
          <h3 className="text-sm font-semibold text-foreground">{title}</h3>
          {subtitle && (
            <p className="text-xs text-muted-foreground/70">{subtitle}</p>
          )}
        </div>
      </div>
      {actionLabel && actionHref && (
        <Link
          href={actionHref}
          className={cn(
            'group flex items-center gap-1 text-xs font-medium',
            'text-muted-foreground hover:text-foreground',
            'transition-colors duration-200'
          )}
        >
          {actionLabel}
          <ChevronRight className="w-3.5 h-3.5 transition-transform group-hover:translate-x-0.5" />
        </Link>
      )}
    </div>
  );
}

export default SectionHeader;



