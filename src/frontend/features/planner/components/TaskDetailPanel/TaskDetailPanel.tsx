'use client';

import React from 'react';
import { cn } from '@/lib/utils';
import {
  X,
  Clock,
  MapPin,
  Users,
  ChevronRight,
  Edit3,
  Calendar,
  AlertCircle,
  Scissors,
  Package,
  Sprout,
  Wrench,
  ClipboardCheck,
  CheckCircle,
  UserPlus,
  ExternalLink,
} from 'lucide-react';
import { ProductionTask, ProductionTaskType, LaborStatus } from '../../types';
import { StatusBadge } from '../ui';

interface TaskDetailPanelProps {
  task: ProductionTask | null;
  isOpen: boolean;
  onClose: () => void;
  onEdit?: (task: ProductionTask) => void;
  onAssignStaff?: (task: ProductionTask) => void;
  onViewShiftBoard?: (task: ProductionTask) => void;
}

const TASK_TYPE_CONFIG: Record<
  ProductionTaskType,
  { icon: typeof Scissors; label: string; color: string; bgColor: string }
> = {
  harvest: {
    icon: Scissors,
    label: 'Harvest',
    color: 'text-emerald-400',
    bgColor: 'bg-emerald-500/10',
  },
  processing: {
    icon: Package,
    label: 'Processing',
    color: 'text-cyan-400',
    bgColor: 'bg-cyan-500/10',
  },
  packaging: {
    icon: Package,
    label: 'Packaging',
    color: 'text-violet-400',
    bgColor: 'bg-violet-500/10',
  },
  planting: {
    icon: Sprout,
    label: 'Planting',
    color: 'text-lime-400',
    bgColor: 'bg-lime-500/10',
  },
  maintenance: {
    icon: Wrench,
    label: 'Maintenance',
    color: 'text-amber-400',
    bgColor: 'bg-amber-500/10',
  },
  quality_check: {
    icon: ClipboardCheck,
    label: 'Quality Check',
    color: 'text-blue-400',
    bgColor: 'bg-blue-500/10',
  },
  other: {
    icon: ClipboardCheck,
    label: 'Other',
    color: 'text-muted-foreground',
    bgColor: 'bg-muted/10',
  },
};

const LABOR_STATUS_CONFIG: Record<
  LaborStatus,
  { label: string; badgeStatus: 'success' | 'warning' | 'error'; description: string }
> = {
  fully_staffed: {
    label: 'Fully Staffed',
    badgeStatus: 'success',
    description: 'All required positions are filled',
  },
  needs_people: {
    label: 'Needs Staff',
    badgeStatus: 'warning',
    description: 'Additional staff members required',
  },
  unassigned: {
    label: 'Unassigned',
    badgeStatus: 'error',
    description: 'No staff assigned yet',
  },
};

function formatDate(dateString: string): string {
  const date = new Date(dateString);
  return date.toLocaleDateString('en-US', {
    weekday: 'long',
    month: 'short',
    day: 'numeric',
  });
}

export function TaskDetailPanel({
  task,
  isOpen,
  onClose,
  onEdit,
  onAssignStaff,
  onViewShiftBoard,
}: TaskDetailPanelProps) {
  if (!task) return null;

  const typeConfig = TASK_TYPE_CONFIG[task.type];
  const statusConfig = LABOR_STATUS_CONFIG[task.laborStatus];
  const Icon = typeConfig.icon;
  const shortfall = task.requiredCount - task.assignedCount;
  const staffingPercentage = Math.round((task.assignedCount / task.requiredCount) * 100);

  return (
    <>
      {/* Backdrop for click-outside-to-close */}
      {isOpen && (
        <div
          className="fixed inset-0 top-[72px] z-30"
          onClick={onClose}
          aria-hidden="true"
        />
      )}

      <div
        className={cn(
          'fixed top-[72px] right-0 h-[calc(100vh-72px)] w-[420px] bg-surface border-l border-border shadow-2xl z-40',
          'transform transition-transform duration-300 ease-out',
          isOpen ? 'translate-x-0' : 'translate-x-full'
        )}
      >
        {/* Collapse Button - Left Edge */}
        <button
          onClick={onClose}
          className={cn(
            'absolute -left-8 top-1/2 -translate-y-1/2 z-50',
            'w-7 h-14 rounded-l-lg',
            'bg-surface border border-border border-r-0',
            'flex items-center justify-center',
            'text-muted-foreground hover:text-foreground hover:bg-muted/50',
            'transition-colors shadow-lg'
          )}
          aria-label="Collapse panel"
        >
          <ChevronRight className="w-4 h-4" />
        </button>

        {/* Header */}
        <div className="flex items-center justify-between px-4 py-3 border-b border-border">
          <div className="flex items-center gap-2">
            <div className={cn('p-1.5 rounded-lg', typeConfig.bgColor, typeConfig.color)}>
              <Icon className="w-4 h-4" />
            </div>
            <span className="text-xs text-muted-foreground uppercase tracking-wider">
              {typeConfig.label}
            </span>
          </div>
          <button
            onClick={onClose}
            className="p-1.5 text-muted-foreground hover:text-foreground hover:bg-muted/80 rounded-md transition-colors border border-transparent hover:border-border"
            aria-label="Close panel"
          >
            <X className="w-4 h-4" />
          </button>
        </div>

        {/* Content */}
        <div className="flex flex-col h-[calc(100%-140px)] overflow-y-auto">
          {/* Task Info */}
          <div className="p-4 border-b border-border/50">
            <h2 className="text-xl font-semibold text-foreground mb-1">{task.name}</h2>
            <p className="text-sm text-muted-foreground">{task.location}</p>

            <div className="mt-4 space-y-3">
              {/* Date */}
              <div className="flex items-center gap-2 text-sm">
                <Calendar className="w-4 h-4 text-blue-400" />
                <span className="text-muted-foreground">Date:</span>
                <span className="text-foreground">{formatDate(task.date)}</span>
              </div>

              {/* Time */}
              <div className="flex items-center gap-2 text-sm">
                <Clock className="w-4 h-4 text-violet-400" />
                <span className="text-muted-foreground">Time:</span>
                <span className="text-foreground">
                  {task.startTime} - {task.endTime}
                </span>
              </div>

              {/* Location */}
              <div className="flex items-center gap-2 text-sm">
                <MapPin className="w-4 h-4 text-emerald-400" />
                <span className="text-muted-foreground">Location:</span>
                <span className="text-foreground">{task.location}</span>
              </div>
            </div>
          </div>

          {/* Labor Status Section */}
          <div className="p-4 border-b border-border/50">
            <h3 className="text-xs text-muted-foreground uppercase tracking-wider mb-3">
              Staffing Status
            </h3>

            <div className="space-y-4">
              {/* Status Badge */}
              <div className="flex items-center justify-between">
                <StatusBadge
                  status={statusConfig.badgeStatus}
                  label={statusConfig.label}
                  size="sm"
                  dot
                />
                <span className="text-sm text-muted-foreground">
                  {staffingPercentage}% filled
                </span>
              </div>

              {/* Progress Bar */}
              <div className="space-y-1.5">
                <div className="flex items-center justify-between text-sm">
                  <span className="text-muted-foreground">Staff Assigned</span>
                  <span className="text-foreground tabular-nums">
                    {task.assignedCount} / {task.requiredCount}
                  </span>
                </div>
                <div className="h-2 bg-muted/30 rounded-full overflow-hidden">
                  <div
                    className={cn(
                      'h-full rounded-full transition-all duration-300',
                      task.laborStatus === 'fully_staffed' && 'bg-emerald-500',
                      task.laborStatus === 'needs_people' && 'bg-amber-500',
                      task.laborStatus === 'unassigned' && 'bg-rose-500'
                    )}
                    style={{ width: `${staffingPercentage}%` }}
                  />
                </div>
              </div>

              {/* Shortfall Warning */}
              {shortfall > 0 && (
                <div className="flex items-center gap-2 p-3 rounded-lg bg-amber-500/10 border border-amber-500/20">
                  <AlertCircle className="w-4 h-4 text-amber-400 flex-shrink-0" />
                  <div className="flex-1">
                    <p className="text-sm font-medium text-amber-400">
                      {shortfall} more {shortfall === 1 ? 'person' : 'people'} needed
                    </p>
                    <p className="text-xs text-amber-400/70">
                      {statusConfig.description}
                    </p>
                  </div>
                </div>
              )}

              {/* Fully Staffed Message */}
              {task.laborStatus === 'fully_staffed' && (
                <div className="flex items-center gap-2 p-3 rounded-lg bg-emerald-500/10 border border-emerald-500/20">
                  <CheckCircle className="w-4 h-4 text-emerald-400 flex-shrink-0" />
                  <p className="text-sm text-emerald-400">{statusConfig.description}</p>
                </div>
              )}
            </div>
          </div>

          {/* Required Skills */}
          {task.skills && task.skills.length > 0 && (
            <div className="p-4">
              <h3 className="text-xs text-muted-foreground uppercase tracking-wider mb-3">
                Required Skills
              </h3>
              <div className="flex flex-wrap gap-2">
                {task.skills.map((skill) => (
                  <span
                    key={skill}
                    className={cn(
                      'px-2.5 py-1 text-xs font-medium rounded-lg',
                      'bg-white/[0.04] text-foreground/80 border border-white/[0.06]'
                    )}
                  >
                    {skill}
                  </span>
                ))}
              </div>
            </div>
          )}
        </div>

        {/* Actions Footer */}
        <div className="absolute bottom-0 left-0 right-0 p-4 border-t border-border bg-surface">
          <div className="flex flex-col gap-2">
            {/* Primary Action */}
            {shortfall > 0 ? (
              <button
                onClick={() => onAssignStaff?.(task)}
                className="w-full flex items-center justify-center gap-2 px-4 py-2.5 text-sm font-medium text-white bg-emerald-500 hover:bg-emerald-400 rounded-lg transition-colors"
              >
                <UserPlus className="w-4 h-4" />
                Assign Staff
              </button>
            ) : (
              <button
                onClick={() => onEdit?.(task)}
                className="w-full flex items-center justify-center gap-2 px-4 py-2.5 text-sm font-medium text-foreground bg-cyan-500 hover:bg-cyan-400 rounded-lg transition-colors"
              >
                <Edit3 className="w-4 h-4" />
                Edit Task
              </button>
            )}

            {/* Secondary Action */}
            <button
              onClick={() => onViewShiftBoard?.(task)}
              className="w-full flex items-center justify-center gap-2 px-4 py-2 text-sm font-medium text-muted-foreground hover:text-foreground bg-white/[0.04] hover:bg-white/[0.06] border border-white/[0.06] rounded-lg transition-colors"
            >
              <ExternalLink className="w-4 h-4" />
              View in Shift Board
            </button>
          </div>
        </div>
      </div>
    </>
  );
}

export default TaskDetailPanel;
