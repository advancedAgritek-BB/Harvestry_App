'use client';

import React from 'react';
import Link from 'next/link';
import { Clock, CheckCircle, AlertTriangle, Settings, Check, X } from 'lucide-react';
import { timecards } from '@/features/planner/constants/laborMockData';
import { TimecardApproval } from '@/features/planner/types';
import { SectionHeader, StatusBadge, Avatar } from '@/features/planner/components/ui';
import { cn } from '@/lib/utils';

function TimecardCard({ card }: { card: TimecardApproval }) {
  const statusMap: Record<string, 'success' | 'warning' | 'error'> = {
    approved: 'success',
    pending: 'warning',
    exception: 'error',
  };

  const isPending = card.status === 'pending';
  const isException = card.status === 'exception';

  return (
    <div
      className={cn(
        'flex items-center gap-4 px-5 py-4 rounded-2xl',
        'bg-[var(--bg-surface)]/60 backdrop-blur-sm',
        'border border-white/[0.06]',
        isException && 'border-rose-500/20 bg-rose-500/5'
      )}
    >
      <Avatar name={card.employee} size="lg" />
      
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2">
          <p className="text-base font-semibold text-foreground">{card.employee}</p>
          <StatusBadge status={statusMap[card.status]} label={card.status} />
        </div>
        <p className="text-sm text-muted-foreground">{card.role}</p>
        {card.exceptions && card.exceptions.length > 0 && (
          <p className="text-xs text-rose-400 mt-1 flex items-center gap-1">
            <AlertTriangle className="w-3 h-3" />
            {card.exceptions.join(' · ')}
          </p>
        )}
      </div>

      <div className="text-right">
        <p className="text-xl font-bold tabular-nums text-foreground">{card.hours.toFixed(1)}</p>
        <p className="text-xs text-muted-foreground">hours</p>
      </div>

      <div className="text-right min-w-[80px]">
        <p className="text-lg font-semibold tabular-nums text-foreground">${card.cost.toFixed(0)}</p>
        <p className="text-xs text-muted-foreground">cost</p>
      </div>

      {(isPending || isException) && (
        <div className="flex gap-2 ml-2">
          <button
            className={cn(
              'p-2 rounded-xl',
              'bg-emerald-500/10 text-emerald-400',
              'hover:bg-emerald-500/20 transition-colors duration-200'
            )}
            title="Approve"
          >
            <Check className="w-5 h-5" />
          </button>
          <button
            className={cn(
              'p-2 rounded-xl',
              'bg-white/[0.04] text-muted-foreground',
              'hover:bg-white/[0.06] transition-colors duration-200'
            )}
            title="Request edit"
          >
            <X className="w-5 h-5" />
          </button>
        </div>
      )}
    </div>
  );
}

export default function TimeApprovalsPage() {
  const pending = timecards.filter((c) => c.status === 'pending');
  const exceptions = timecards.filter((c) => c.status === 'exception');
  const approved = timecards.filter((c) => c.status === 'approved');

  const totalHours = timecards.reduce((sum, c) => sum + c.hours, 0);
  const totalCost = timecards.reduce((sum, c) => sum + c.cost, 0);

  return (
    <div className="flex flex-col gap-6 p-6 max-w-[1200px] mx-auto">
      {/* Page Header */}
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-foreground">Time Approvals</h1>
          <p className="text-sm text-muted-foreground mt-1">
            Review and approve employee time entries
          </p>
        </div>
        <Link
          href="/dashboard/planner/settings"
          className={cn(
            'flex items-center gap-2 px-4 py-2 rounded-xl',
            'bg-white/[0.04] text-foreground text-sm font-medium',
            'hover:bg-white/[0.06] transition-colors duration-200'
          )}
        >
          <Settings className="w-4 h-4" />
          Configure Rules
        </Link>
      </div>

      {/* Summary Stats */}
      <div className="grid grid-cols-4 gap-4">
        <div className="px-5 py-4 rounded-2xl bg-[var(--bg-surface)]/60 border border-white/[0.06]">
          <p className="text-xs text-muted-foreground/70 uppercase tracking-wide">Total Entries</p>
          <p className="text-2xl font-bold text-foreground">{timecards.length}</p>
        </div>
        <div className="px-5 py-4 rounded-2xl bg-amber-500/10 border border-amber-500/20">
          <p className="text-xs text-amber-400/70 uppercase tracking-wide">Pending</p>
          <p className="text-2xl font-bold text-amber-400">{pending.length}</p>
        </div>
        <div className="px-5 py-4 rounded-2xl bg-[var(--bg-surface)]/60 border border-white/[0.06]">
          <p className="text-xs text-muted-foreground/70 uppercase tracking-wide">Total Hours</p>
          <p className="text-2xl font-bold text-foreground tabular-nums">{totalHours.toFixed(1)}</p>
        </div>
        <div className="px-5 py-4 rounded-2xl bg-[var(--bg-surface)]/60 border border-white/[0.06]">
          <p className="text-xs text-muted-foreground/70 uppercase tracking-wide">Total Cost</p>
          <p className="text-2xl font-bold text-foreground tabular-nums">${totalCost.toLocaleString()}</p>
        </div>
      </div>

      {/* Exceptions */}
      {exceptions.length > 0 && (
        <section>
          <SectionHeader
            icon={AlertTriangle}
            title="Exceptions"
            subtitle="Entries with flags or issues"
          />
          <div className="space-y-3">
            {exceptions.map((card) => (
              <TimecardCard key={card.id} card={card} />
            ))}
          </div>
        </section>
      )}

      {/* Pending */}
      {pending.length > 0 && (
        <section>
          <SectionHeader
            icon={Clock}
            title="Pending Approval"
            subtitle="Ready for review"
          />
          <div className="space-y-3">
            {pending.map((card) => (
              <TimecardCard key={card.id} card={card} />
            ))}
          </div>
        </section>
      )}

      {/* Approved */}
      {approved.length > 0 && (
        <section>
          <SectionHeader
            icon={CheckCircle}
            title="Approved"
            subtitle="Processed entries"
          />
          <div className="space-y-3">
            {approved.map((card) => (
              <TimecardCard key={card.id} card={card} />
            ))}
          </div>
        </section>
      )}
    </div>
  );
}



