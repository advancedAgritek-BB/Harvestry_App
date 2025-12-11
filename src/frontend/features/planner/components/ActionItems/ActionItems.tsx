'use client';

import React from 'react';
import Link from 'next/link';
import {
  ClipboardList,
  Clock,
  AlertTriangle,
  Award,
  ArrowRightLeft,
  ChevronRight,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { ActionItem, ActionItemType } from '../../types';
import { PlannerCard, SectionHeader } from '../ui';

interface ActionItemsProps {
  items: ActionItem[];
  className?: string;
}

const ITEM_TYPE_CONFIG: Record<
  ActionItemType,
  { icon: typeof Clock; color: string; bgColor: string }
> = {
  timecard_approval: {
    icon: Clock,
    color: 'text-cyan-400',
    bgColor: 'bg-cyan-500/10',
  },
  overtime_approval: {
    icon: Clock,
    color: 'text-amber-400',
    bgColor: 'bg-amber-500/10',
  },
  scheduling_conflict: {
    icon: AlertTriangle,
    color: 'text-rose-400',
    bgColor: 'bg-rose-500/10',
  },
  certification_expiring: {
    icon: Award,
    color: 'text-violet-400',
    bgColor: 'bg-violet-500/10',
  },
  shift_swap_request: {
    icon: ArrowRightLeft,
    color: 'text-emerald-400',
    bgColor: 'bg-emerald-500/10',
  },
};

const PRIORITY_CONFIG: Record<
  ActionItem['priority'],
  { dotColor: string; order: number }
> = {
  high: { dotColor: 'bg-rose-400', order: 0 },
  medium: { dotColor: 'bg-amber-400', order: 1 },
  low: { dotColor: 'bg-muted-foreground', order: 2 },
};

function ActionItemRow({ item }: { item: ActionItem }) {
  const typeConfig = ITEM_TYPE_CONFIG[item.type];
  const priorityConfig = PRIORITY_CONFIG[item.priority];
  const Icon = typeConfig.icon;

  return (
    <Link href={item.actionHref} className="block">
      <div
        className={cn(
          'flex items-center gap-3 px-4 py-3 rounded-xl',
          'bg-white/[0.02] hover:bg-white/[0.04]',
          'border border-white/[0.04]',
          'transition-colors duration-200',
          'group'
        )}
      >
        {/* Priority dot */}
        <div
          className={cn(
            'w-2 h-2 rounded-full flex-shrink-0',
            priorityConfig.dotColor
          )}
        />

        {/* Icon */}
        <div className={cn('p-2 rounded-lg', typeConfig.bgColor)}>
          <Icon className={cn('w-4 h-4', typeConfig.color)} />
        </div>

        {/* Content */}
        <div className="flex-1 min-w-0">
          <p className="text-sm font-semibold text-foreground truncate">
            {item.title}
          </p>
          <p className="text-xs text-muted-foreground truncate">
            {item.description}
          </p>
        </div>

        {/* Due date if present */}
        {item.dueDate && (
          <span className="hidden sm:block text-xs text-muted-foreground">
            Due: {new Date(item.dueDate).toLocaleDateString('en-US', {
              month: 'short',
              day: 'numeric',
            })}
          </span>
        )}

        {/* Arrow */}
        <ChevronRight
          className={cn(
            'w-4 h-4 text-muted-foreground',
            'group-hover:text-foreground transition-colors'
          )}
        />
      </div>
    </Link>
  );
}

function EmptyState() {
  return (
    <div
      className={cn(
        'flex flex-col items-center justify-center py-8 px-4',
        'rounded-xl bg-white/[0.02] border border-dashed border-white/[0.06]'
      )}
    >
      <div className="p-3 rounded-full bg-emerald-500/10 mb-3">
        <ClipboardList className="w-6 h-6 text-emerald-400" />
      </div>
      <p className="text-sm font-medium text-foreground">All caught up!</p>
      <p className="text-xs text-muted-foreground mt-1">
        No pending action items
      </p>
    </div>
  );
}

function QuickStats({ items }: { items: ActionItem[] }) {
  const highPriority = items.filter((i) => i.priority === 'high').length;
  const timecards = items.filter((i) => i.type === 'timecard_approval').length;
  const conflicts = items.filter((i) => i.type === 'scheduling_conflict').length;

  return (
    <div className="flex flex-wrap gap-2 mb-4">
      {highPriority > 0 && (
        <div
          className={cn(
            'flex items-center gap-1.5 px-2.5 py-1 rounded-lg',
            'bg-rose-500/10 text-rose-400 text-xs font-medium'
          )}
        >
          <span className="w-1.5 h-1.5 rounded-full bg-rose-400" />
          {highPriority} urgent
        </div>
      )}
      {timecards > 0 && (
        <div
          className={cn(
            'flex items-center gap-1.5 px-2.5 py-1 rounded-lg',
            'bg-cyan-500/10 text-cyan-400 text-xs font-medium'
          )}
        >
          <Clock className="w-3 h-3" />
          {timecards} timecards
        </div>
      )}
      {conflicts > 0 && (
        <div
          className={cn(
            'flex items-center gap-1.5 px-2.5 py-1 rounded-lg',
            'bg-amber-500/10 text-amber-400 text-xs font-medium'
          )}
        >
          <AlertTriangle className="w-3 h-3" />
          {conflicts} conflict{conflicts !== 1 ? 's' : ''}
        </div>
      )}
    </div>
  );
}

export function ActionItems({ items, className }: ActionItemsProps) {
  // Sort by priority
  const sortedItems = [...items].sort(
    (a, b) => PRIORITY_CONFIG[a.priority].order - PRIORITY_CONFIG[b.priority].order
  );

  return (
    <section className={className}>
      <SectionHeader
        icon={ClipboardList}
        title="Action Items"
        subtitle={
          items.length > 0
            ? `${items.length} item${items.length !== 1 ? 's' : ''} need attention`
            : 'No pending items'
        }
        actionLabel={items.length > 0 ? 'View All' : undefined}
        actionHref="/dashboard/planner/time-approvals"
      />

      {items.length > 0 && <QuickStats items={items} />}

      {items.length === 0 ? (
        <EmptyState />
      ) : (
        <div className="space-y-2">
          {sortedItems.slice(0, 5).map((item) => (
            <ActionItemRow key={item.id} item={item} />
          ))}

          {items.length > 5 && (
            <Link
              href="/dashboard/planner/time-approvals"
              className={cn(
                'flex items-center justify-center gap-2 px-4 py-3 rounded-xl',
                'text-sm font-medium text-muted-foreground',
                'bg-white/[0.02] hover:bg-white/[0.04]',
                'border border-white/[0.04]',
                'transition-colors duration-200'
              )}
            >
              View all {items.length} action items
            </Link>
          )}
        </div>
      )}
    </section>
  );
}

export default ActionItems;
