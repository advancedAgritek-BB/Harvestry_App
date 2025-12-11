'use client';

import React, { useRef } from 'react';
import Link from 'next/link';
import {
  Scissors,
  Package,
  Sprout,
  Wrench,
  ClipboardCheck,
  ChevronLeft,
  ChevronRight,
  Users,
  AlertCircle,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { ProductionTask, ProductionTaskType, LaborStatus } from '../../types';
import { PlannerCard, StatusBadge, SectionHeader } from '../ui';

interface ProductionTimelineProps {
  tasks: ProductionTask[];
  className?: string;
}

const TASK_TYPE_CONFIG: Record<
  ProductionTaskType,
  { icon: typeof Scissors; label: string; color: string }
> = {
  harvest: {
    icon: Scissors,
    label: 'Harvest',
    color: 'text-emerald-400',
  },
  processing: {
    icon: Package,
    label: 'Processing',
    color: 'text-cyan-400',
  },
  packaging: {
    icon: Package,
    label: 'Packaging',
    color: 'text-violet-400',
  },
  planting: {
    icon: Sprout,
    label: 'Planting',
    color: 'text-lime-400',
  },
  maintenance: {
    icon: Wrench,
    label: 'Maintenance',
    color: 'text-amber-400',
  },
  quality_check: {
    icon: ClipboardCheck,
    label: 'QA Check',
    color: 'text-blue-400',
  },
  other: {
    icon: ClipboardCheck,
    label: 'Other',
    color: 'text-muted-foreground',
  },
};

const LABOR_STATUS_CONFIG: Record<
  LaborStatus,
  { label: string; badgeStatus: 'success' | 'warning' | 'error' }
> = {
  fully_staffed: {
    label: 'Staffed',
    badgeStatus: 'success',
  },
  needs_people: {
    label: 'Needs Staff',
    badgeStatus: 'warning',
  },
  unassigned: {
    label: 'Unassigned',
    badgeStatus: 'error',
  },
};

function formatDate(dateString: string): { dayName: string; dayNum: string; isToday: boolean } {
  const date = new Date(dateString);
  const today = new Date();
  const isToday =
    date.getDate() === today.getDate() &&
    date.getMonth() === today.getMonth() &&
    date.getFullYear() === today.getFullYear();

  return {
    dayName: date.toLocaleDateString('en-US', { weekday: 'short' }),
    dayNum: date.getDate().toString(),
    isToday,
  };
}

function TaskCard({ task }: { task: ProductionTask }) {
  const typeConfig = TASK_TYPE_CONFIG[task.type];
  const statusConfig = LABOR_STATUS_CONFIG[task.laborStatus];
  const Icon = typeConfig.icon;
  const shortfall = task.requiredCount - task.assignedCount;

  return (
    <Link
      href={`/dashboard/planner/shift-board?task=${task.id}`}
      className="block"
    >
      <PlannerCard
        hoverable
        className={cn(
          'min-w-[240px] max-w-[280px]',
          'flex flex-col gap-3'
        )}
      >
        {/* Header */}
        <div className="flex items-start justify-between gap-2">
          <div className="flex items-center gap-2">
            <div className={cn('p-1.5 rounded-lg bg-white/[0.06]', typeConfig.color)}>
              <Icon className="w-4 h-4" />
            </div>
            <div className="min-w-0">
              <p className="text-sm font-semibold text-foreground truncate">
                {task.name}
              </p>
              <p className="text-xs text-muted-foreground">
                {task.startTime} - {task.endTime}
              </p>
            </div>
          </div>
        </div>

        {/* Location */}
        <p className="text-xs text-muted-foreground truncate">{task.location}</p>

        {/* Labor Status */}
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <Users className="w-4 h-4 text-muted-foreground" />
            <span className="text-sm tabular-nums text-foreground">
              {task.assignedCount}/{task.requiredCount}
            </span>
          </div>
          <StatusBadge
            status={statusConfig.badgeStatus}
            label={statusConfig.label}
            size="sm"
            dot
          />
        </div>

        {/* Shortfall Warning */}
        {shortfall > 0 && (
          <div className="flex items-center gap-1.5 text-xs text-amber-400">
            <AlertCircle className="w-3 h-3" />
            <span>Need {shortfall} more</span>
          </div>
        )}
      </PlannerCard>
    </Link>
  );
}

function DayColumn({
  date,
  tasks,
}: {
  date: string;
  tasks: ProductionTask[];
}) {
  const { dayName, dayNum, isToday } = formatDate(date);

  return (
    <div className="flex flex-col gap-3 min-w-[260px]">
      {/* Day Header */}
      <div
        className={cn(
          'flex items-center gap-2 px-3 py-2 rounded-xl',
          isToday ? 'bg-emerald-500/10 border border-emerald-500/30' : 'bg-white/[0.02]'
        )}
      >
        <div
          className={cn(
            'w-10 h-10 rounded-xl flex items-center justify-center',
            'text-lg font-bold',
            isToday ? 'bg-emerald-500 text-white' : 'bg-white/[0.06] text-foreground'
          )}
        >
          {dayNum}
        </div>
        <div>
          <p
            className={cn(
              'text-sm font-medium',
              isToday ? 'text-emerald-400' : 'text-foreground'
            )}
          >
            {isToday ? 'Today' : dayName}
          </p>
          {tasks.length > 0 && (
            <p className="text-xs text-muted-foreground">
              {tasks.length} task{tasks.length !== 1 ? 's' : ''}
            </p>
          )}
        </div>
      </div>

      {/* Tasks */}
      <div className="space-y-2">
        {tasks.length === 0 ? (
          <div className="px-3 py-6 text-center text-sm text-muted-foreground rounded-xl bg-white/[0.02] border border-dashed border-white/[0.06]">
            No tasks scheduled
          </div>
        ) : (
          tasks.map((task) => <TaskCard key={task.id} task={task} />)
        )}
      </div>
    </div>
  );
}

export function ProductionTimeline({
  tasks,
  className,
}: ProductionTimelineProps) {
  const scrollContainerRef = useRef<HTMLDivElement>(null);

  // Group tasks by date
  const tasksByDate = tasks.reduce(
    (acc, task) => {
      if (!acc[task.date]) {
        acc[task.date] = [];
      }
      acc[task.date].push(task);
      return acc;
    },
    {} as Record<string, ProductionTask[]>
  );

  // Get unique dates sorted
  const dates = Object.keys(tasksByDate).sort();

  // Count tasks needing attention
  const needsAttentionCount = tasks.filter(
    (t) => t.laborStatus !== 'fully_staffed'
  ).length;

  const scroll = (direction: 'left' | 'right') => {
    if (scrollContainerRef.current) {
      const scrollAmount = 300;
      scrollContainerRef.current.scrollBy({
        left: direction === 'left' ? -scrollAmount : scrollAmount,
        behavior: 'smooth',
      });
    }
  };

  return (
    <section className={className}>
      <SectionHeader
        icon={Package}
        title="This Week's Production"
        subtitle={
          needsAttentionCount > 0
            ? `${needsAttentionCount} task${needsAttentionCount !== 1 ? 's' : ''} need staffing`
            : 'All tasks staffed'
        }
        actionLabel="View All"
        actionHref="/dashboard/planner/batch-planning"
      />

      {/* Timeline with scroll controls */}
      <div className="relative">
        {/* Left scroll button */}
        <button
          onClick={() => scroll('left')}
          className={cn(
            'absolute left-0 top-1/2 -translate-y-1/2 z-10',
            'w-10 h-10 rounded-full',
            'bg-[var(--bg-surface)]/90 backdrop-blur-sm',
            'border border-white/[0.1]',
            'flex items-center justify-center',
            'text-muted-foreground hover:text-foreground',
            'transition-all duration-200',
            'hover:bg-white/[0.06]',
            'shadow-lg'
          )}
          aria-label="Scroll left"
        >
          <ChevronLeft className="w-5 h-5" />
        </button>

        {/* Scrollable container */}
        <div
          ref={scrollContainerRef}
          className={cn(
            'flex gap-4 overflow-x-auto pb-4 px-12',
            'scrollbar-thin scrollbar-track-transparent scrollbar-thumb-white/10',
            'scroll-smooth'
          )}
        >
          {dates.map((date) => (
            <DayColumn key={date} date={date} tasks={tasksByDate[date]} />
          ))}
        </div>

        {/* Right scroll button */}
        <button
          onClick={() => scroll('right')}
          className={cn(
            'absolute right-0 top-1/2 -translate-y-1/2 z-10',
            'w-10 h-10 rounded-full',
            'bg-[var(--bg-surface)]/90 backdrop-blur-sm',
            'border border-white/[0.1]',
            'flex items-center justify-center',
            'text-muted-foreground hover:text-foreground',
            'transition-all duration-200',
            'hover:bg-white/[0.06]',
            'shadow-lg'
          )}
          aria-label="Scroll right"
        >
          <ChevronRight className="w-5 h-5" />
        </button>
      </div>
    </section>
  );
}

export default ProductionTimeline;
