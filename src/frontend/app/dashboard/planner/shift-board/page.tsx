'use client';

import React from 'react';
import Link from 'next/link';
import { Clock, Users, Zap, CalendarRange } from 'lucide-react';
import { shiftNeeds } from '@/features/planner/constants/laborMockData';
import { ShiftNeed } from '@/features/planner/types';
import { PlannerCard, SectionHeader, StatusBadge } from '@/features/planner/components/ui';
import { cn } from '@/lib/utils';

function ShiftCard({ shift }: { shift: ShiftNeed }) {
  const shortfall = shift.required - shift.assigned;
  const isFilled = shortfall <= 0;

  return (
    <PlannerCard
      variant={isFilled ? 'emerald' : shortfall > 1 ? 'rose' : 'amber'}
      className="flex flex-col gap-4"
    >
      {/* Header */}
      <div className="flex items-start justify-between">
        <div>
          <p className="text-xs text-muted-foreground/70 uppercase tracking-wide">{shift.room}</p>
          <h3 className="text-lg font-semibold text-foreground">{shift.role}</h3>
          <p className="text-sm text-muted-foreground flex items-center gap-1.5 mt-1">
            <Clock className="w-3.5 h-3.5" />
            {shift.start} – {shift.end}
          </p>
        </div>
        <div className="text-right">
          <p className="text-2xl font-bold tabular-nums text-foreground">
            {shift.assigned}<span className="text-muted-foreground/50">/{shift.required}</span>
          </p>
          {shortfall > 0 ? (
            <StatusBadge status="warning" label={`${shortfall} open`} dot />
          ) : (
            <StatusBadge status="success" label="Filled" dot />
          )}
        </div>
      </div>

      {/* Skills */}
      <div className="flex flex-wrap gap-1.5">
        {shift.skills.map((skill) => (
          <span
            key={skill}
            className="px-2.5 py-1 rounded-lg bg-white/[0.04] text-xs font-medium text-muted-foreground"
          >
            {skill}
          </span>
        ))}
      </div>

      {/* Actions */}
      <div className="flex gap-2 pt-2 border-t border-white/[0.04]">
        <button
          className={cn(
            'flex-1 flex items-center justify-center gap-2 px-3 py-2 rounded-xl',
            'bg-emerald-500/10 text-emerald-400 text-sm font-medium',
            'hover:bg-emerald-500/20 transition-colors duration-200'
          )}
        >
          <Users className="w-4 h-4" />
          Assign
        </button>
        <button
          className={cn(
            'flex items-center justify-center gap-2 px-3 py-2 rounded-xl',
            'bg-white/[0.04] text-foreground text-sm font-medium',
            'hover:bg-white/[0.06] transition-colors duration-200'
          )}
        >
          <Zap className="w-4 h-4" />
          Auto-fill
        </button>
      </div>
    </PlannerCard>
  );
}

export default function ShiftBoardPage() {
  const openShifts = shiftNeeds.filter((s) => s.assigned < s.required);
  const filledShifts = shiftNeeds.filter((s) => s.assigned >= s.required);

  return (
    <div className="flex flex-col gap-6 p-6 max-w-[1600px] mx-auto">
      {/* Page Header */}
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-foreground">Shift Board</h1>
          <p className="text-sm text-muted-foreground mt-1">
            Manage shift assignments and coverage across the facility
          </p>
        </div>
        <Link
          href="/dashboard/planner/batch-planning"
          className={cn(
            'flex items-center gap-2 px-4 py-2 rounded-xl',
            'bg-white/[0.04] text-foreground text-sm font-medium',
            'hover:bg-white/[0.06] transition-colors duration-200'
          )}
        >
          <CalendarRange className="w-4 h-4" />
          View in Batch Planning
        </Link>
      </div>

      {/* Open Shifts */}
      {openShifts.length > 0 && (
        <section>
          <SectionHeader
            icon={Users}
            title="Needs Coverage"
            subtitle={`${openShifts.length} shifts with open positions`}
          />
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            {openShifts.map((shift) => (
              <ShiftCard key={shift.id} shift={shift} />
            ))}
          </div>
        </section>
      )}

      {/* Filled Shifts */}
      {filledShifts.length > 0 && (
        <section>
          <SectionHeader
            icon={Users}
            title="Fully Staffed"
            subtitle={`${filledShifts.length} shifts with complete coverage`}
          />
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            {filledShifts.map((shift) => (
              <ShiftCard key={shift.id} shift={shift} />
            ))}
          </div>
        </section>
      )}
    </div>
  );
}


