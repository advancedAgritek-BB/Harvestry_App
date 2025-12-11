'use client';

import React from 'react';
import Link from 'next/link';
import {
  Calendar,
  Clock,
  MapPin,
  AlertCircle,
  UserX,
  UserCheck,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { ScheduleSummary, ScheduledEmployee } from '../../types';
import { PlannerCard, StatusBadge, SectionHeader, Avatar } from '../ui';

interface TodayScheduleProps {
  schedule: ScheduleSummary;
  className?: string;
}

const STATUS_CONFIG: Record<
  ScheduledEmployee['status'],
  { label: string; badgeStatus: 'success' | 'warning' | 'error' | 'neutral' }
> = {
  clocked_in: {
    label: 'Working',
    badgeStatus: 'success',
  },
  scheduled: {
    label: 'Scheduled',
    badgeStatus: 'neutral',
  },
  absent: {
    label: 'Absent',
    badgeStatus: 'error',
  },
  late: {
    label: 'Late',
    badgeStatus: 'warning',
  },
};

function ScheduleStats({ schedule }: { schedule: ScheduleSummary }) {
  return (
    <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 mb-4">
      <div className="flex items-center gap-2 px-3 py-2 rounded-xl bg-white/[0.02] border border-white/[0.04]">
        <UserCheck className="w-4 h-4 text-emerald-400" />
        <div>
          <p className="text-lg font-bold tabular-nums text-foreground">
            {schedule.totalPresent}
          </p>
          <p className="text-xs text-muted-foreground">Present</p>
        </div>
      </div>
      <div className="flex items-center gap-2 px-3 py-2 rounded-xl bg-white/[0.02] border border-white/[0.04]">
        <Calendar className="w-4 h-4 text-cyan-400" />
        <div>
          <p className="text-lg font-bold tabular-nums text-foreground">
            {schedule.totalScheduled}
          </p>
          <p className="text-xs text-muted-foreground">Scheduled</p>
        </div>
      </div>
      <div className="flex items-center gap-2 px-3 py-2 rounded-xl bg-white/[0.02] border border-white/[0.04]">
        <UserX className="w-4 h-4 text-rose-400" />
        <div>
          <p className="text-lg font-bold tabular-nums text-foreground">
            {schedule.absences}
          </p>
          <p className="text-xs text-muted-foreground">Absent</p>
        </div>
      </div>
      <div className="flex items-center gap-2 px-3 py-2 rounded-xl bg-white/[0.02] border border-white/[0.04]">
        <Clock className="w-4 h-4 text-amber-400" />
        <div>
          <p className="text-lg font-bold tabular-nums text-foreground">
            {schedule.lateArrivals}
          </p>
          <p className="text-xs text-muted-foreground">Late</p>
        </div>
      </div>
    </div>
  );
}

function EmployeeRow({ employee }: { employee: ScheduledEmployee }) {
  const statusConfig = STATUS_CONFIG[employee.status];
  const isIssue = employee.status === 'absent' || employee.status === 'late';

  return (
    <div
      className={cn(
        'flex items-center gap-3 px-4 py-3 rounded-xl',
        'bg-white/[0.02] hover:bg-white/[0.04]',
        'border',
        isIssue ? 'border-amber-500/20' : 'border-white/[0.04]',
        'transition-colors duration-200'
      )}
    >
      <Avatar name={employee.name} size="md" />

      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2">
          <p className="text-sm font-semibold text-foreground truncate">
            {employee.name}
          </p>
          {isIssue && (
            <AlertCircle className="w-3.5 h-3.5 text-amber-400 flex-shrink-0" />
          )}
        </div>
        <p className="text-xs text-muted-foreground">{employee.role}</p>
      </div>

      <div className="hidden sm:flex items-center gap-1.5 text-xs text-muted-foreground">
        <Clock className="w-3 h-3" />
        <span>
          {employee.shiftStart} - {employee.shiftEnd}
        </span>
      </div>

      {employee.location && (
        <div className="hidden md:flex items-center gap-1.5 text-xs text-muted-foreground max-w-[120px]">
          <MapPin className="w-3 h-3 flex-shrink-0" />
          <span className="truncate">{employee.location}</span>
        </div>
      )}

      <StatusBadge
        status={statusConfig.badgeStatus}
        label={statusConfig.label}
        size="sm"
        dot
      />
    </div>
  );
}

function AbsenceAlert({
  absences,
}: {
  absences: ScheduledEmployee[];
}) {
  if (absences.length === 0) return null;

  return (
    <div
      className={cn(
        'flex items-start gap-3 px-4 py-3 rounded-xl mb-4',
        'bg-amber-500/10 border border-amber-500/20'
      )}
    >
      <AlertCircle className="w-5 h-5 text-amber-400 flex-shrink-0 mt-0.5" />
      <div className="flex-1 min-w-0">
        <p className="text-sm font-semibold text-amber-400">
          {absences.length} absence{absences.length !== 1 ? 's' : ''} today
        </p>
        <p className="text-xs text-muted-foreground mt-0.5">
          {absences.map((e) => e.name).join(', ')} - consider reassigning their
          tasks
        </p>
      </div>
      <Link
        href="/dashboard/planner/shift-board?filter=absences"
        className={cn(
          'px-3 py-1.5 rounded-lg text-xs font-medium',
          'bg-amber-500/20 text-amber-400',
          'hover:bg-amber-500/30 transition-colors'
        )}
      >
        Reassign
      </Link>
    </div>
  );
}

export function TodaySchedule({ schedule, className }: TodayScheduleProps) {
  const absences = schedule.employees.filter((e) => e.status === 'absent');
  const activeEmployees = schedule.employees.filter(
    (e) => e.status !== 'absent'
  );

  // Sort: late first, then clocked_in, then scheduled
  const sortedEmployees = [...activeEmployees].sort((a, b) => {
    const order: Record<ScheduledEmployee['status'], number> = {
      late: 0,
      clocked_in: 1,
      scheduled: 2,
      absent: 3,
    };
    return order[a.status] - order[b.status];
  });

  return (
    <section className={className}>
      <SectionHeader
        icon={Calendar}
        title="Today's Schedule"
        subtitle={`${schedule.totalPresent} of ${schedule.totalScheduled} present`}
        actionLabel="Full Schedule"
        actionHref="/dashboard/planner/shift-board"
      />

      <ScheduleStats schedule={schedule} />

      <AbsenceAlert absences={absences} />

      <div className="space-y-2">
        {sortedEmployees.slice(0, 6).map((employee) => (
          <EmployeeRow key={employee.id} employee={employee} />
        ))}

        {schedule.employees.length > 6 && (
          <Link
            href="/dashboard/planner/shift-board"
            className={cn(
              'flex items-center justify-center gap-2 px-4 py-3 rounded-xl',
              'text-sm font-medium text-muted-foreground',
              'bg-white/[0.02] hover:bg-white/[0.04]',
              'border border-white/[0.04]',
              'transition-colors duration-200'
            )}
          >
            View all {schedule.employees.length} employees
          </Link>
        )}
      </div>
    </section>
  );
}

export default TodaySchedule;
