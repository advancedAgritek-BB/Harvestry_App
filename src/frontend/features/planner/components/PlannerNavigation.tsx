'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import React from 'react';
import { cn } from '@/lib/utils';
import {
  LayoutDashboard,
  CalendarRange,
  Users,
  ClipboardCheck,
  TrendingUp,
  Settings,
  LucideIcon,
} from 'lucide-react';

type PlannerNavItem = {
  href: string;
  label: string;
  icon: LucideIcon;
};

const NAV_ITEMS: PlannerNavItem[] = [
  { href: '/dashboard/planner', label: 'Overview', icon: LayoutDashboard },
  { href: '/dashboard/planner/batch-planning', label: 'Batch Planning', icon: CalendarRange },
  { href: '/dashboard/planner/shift-board', label: 'Shifts', icon: Users },
  { href: '/dashboard/planner/time-approvals', label: 'Timecards', icon: ClipboardCheck },
  { href: '/dashboard/planner/productivity', label: 'Productivity', icon: TrendingUp },
  { href: '/dashboard/planner/settings', label: 'Settings', icon: Settings },
];

export function PlannerNavigation() {
  const pathname = usePathname();

  return (
    <nav className="flex items-center gap-1 px-6 py-3 border-b border-white/[0.04] bg-[var(--bg-surface)]/30">
      {NAV_ITEMS.map((item) => {
        const isActive = pathname === item.href;
        const Icon = item.icon;

        return (
          <Link
            key={item.href}
            href={item.href}
            className={cn(
              'relative flex items-center gap-2 px-4 py-2 rounded-lg',
              'text-sm font-medium transition-all duration-200',
              isActive
                ? 'text-foreground bg-white/[0.06]'
                : 'text-muted-foreground hover:text-foreground hover:bg-white/[0.03]'
            )}
          >
            <Icon className={cn('w-4 h-4', isActive && 'text-emerald-400')} />
            <span>{item.label}</span>
            {isActive && (
              <span className="absolute bottom-0 left-4 right-4 h-0.5 bg-emerald-400 rounded-full" />
            )}
          </Link>
        );
      })}
    </nav>
  );
}

export default PlannerNavigation;


