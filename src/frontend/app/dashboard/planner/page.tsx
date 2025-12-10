'use client';

import Link from 'next/link';
import React from 'react';
import {
  Users,
  Calendar,
  Clock,
  TrendingUp,
  Link2,
  ArrowUpRight,
  ArrowDownRight,
  Minus,
  DollarSign,
  AlertCircle,
} from 'lucide-react';
import {
  budgetVariances,
  employees,
  integrationStatuses,
  laborCoverage,
  productivity,
  shiftNeeds,
  timecards,
} from '@/features/planner/constants/laborMockData';
import {
  BudgetVariance,
  LaborCoverageCard,
  ProductivitySnapshot,
  ShiftNeed,
  TimecardApproval,
} from '@/features/planner/types';
import {
  PlannerCard,
  SectionHeader,
  StatusBadge,
  ProgressRing,
  Avatar,
} from '@/features/planner/components/ui';
import { cn } from '@/lib/utils';

function CoverageCard({ card }: { card: LaborCoverageCard }) {
  const isGap = card.filled < card.required;
  const percent = Math.round((card.filled / card.required) * 100);

  return (
    <PlannerCard variant={isGap ? 'amber' : 'emerald'} className="flex items-center gap-4">
      <ProgressRing
        value={card.filled}
        max={card.required}
        size={56}
        strokeWidth={5}
        colorClass={isGap ? 'stroke-amber-400' : 'stroke-emerald-400'}
      />
      <div className="flex-1 min-w-0">
        <p className="text-xs text-muted-foreground/70 uppercase tracking-wide">{card.phase}</p>
        <h3 className="text-base font-semibold text-foreground truncate">{card.label}</h3>
        <p className="text-sm text-muted-foreground">{card.room}</p>
      </div>
      <div className="text-right">
        <p className="text-2xl font-bold tabular-nums text-foreground">
          {card.filled}<span className="text-muted-foreground/50">/{card.required}</span>
        </p>
        {isGap && (
          <p className="text-xs font-medium text-amber-400 flex items-center gap-1 justify-end">
            <AlertCircle className="w-3 h-3" />
            Gap
          </p>
        )}
      </div>
    </PlannerCard>
  );
}

function ShiftNeedRow({ need }: { need: ShiftNeed }) {
  const shortfall = need.required - need.assigned;
  
  return (
    <div className={cn(
      'flex items-center gap-4 px-4 py-3 rounded-xl',
      'bg-white/[0.02] hover:bg-white/[0.04]',
      'border border-white/[0.04]',
      'transition-colors duration-200'
    )}>
      <div className="flex-1 min-w-0">
        <p className="text-sm font-semibold text-foreground">{need.role}</p>
        <p className="text-xs text-muted-foreground flex items-center gap-1.5">
          <Clock className="w-3 h-3" />
          {need.room} · {need.start}–{need.end}
        </p>
      </div>
      <div className="flex items-center gap-2">
        <div className="flex -space-x-1.5">
          {Array.from({ length: Math.min(need.assigned, 3) }).map((_, i) => (
            <div
              key={i}
              className="w-6 h-6 rounded-full bg-emerald-500/20 ring-2 ring-[var(--bg-surface)] flex items-center justify-center"
            >
              <span className="text-[9px] font-bold text-emerald-400">{i + 1}</span>
            </div>
          ))}
          {need.assigned > 3 && (
            <div className="w-6 h-6 rounded-full bg-white/10 ring-2 ring-[var(--bg-surface)] flex items-center justify-center">
              <span className="text-[9px] font-medium text-muted-foreground">+{need.assigned - 3}</span>
            </div>
          )}
        </div>
        <span className="text-sm tabular-nums text-muted-foreground">
          {need.assigned}/{need.required}
        </span>
      </div>
      {shortfall > 0 && (
        <StatusBadge status="warning" label={`${shortfall} open`} dot />
      )}
    </div>
  );
}

function BudgetCard({ item }: { item: BudgetVariance }) {
  const isOver = item.variance > 0;
  
  return (
    <PlannerCard className="space-y-2">
      <div className="flex items-center justify-between">
        <span className="text-xs text-muted-foreground/70 uppercase tracking-wide">{item.label}</span>
        <div className={cn(
          'flex items-center gap-1 text-sm font-semibold',
          isOver ? 'text-amber-400' : 'text-emerald-400'
        )}>
          {isOver ? <ArrowUpRight className="w-4 h-4" /> : <ArrowDownRight className="w-4 h-4" />}
          ${Math.abs(item.variance).toLocaleString()}
        </div>
      </div>
      <div className="flex items-baseline gap-2">
        <span className="text-xl font-bold tabular-nums text-foreground">
          ${item.actual.toLocaleString()}
        </span>
        <span className="text-xs text-muted-foreground">
          of ${item.budget.toLocaleString()}
        </span>
      </div>
    </PlannerCard>
  );
}

function ProductivityCard({ metric }: { metric: ProductivitySnapshot }) {
  const TrendIcon = metric.trend === 'up' ? ArrowUpRight : metric.trend === 'down' ? ArrowDownRight : Minus;
  const trendColor = metric.trend === 'up' ? 'text-emerald-400' : metric.trend === 'down' ? 'text-rose-400' : 'text-muted-foreground';
  
  return (
    <PlannerCard className="space-y-1">
      <p className="text-xs text-muted-foreground/70 uppercase tracking-wide">{metric.label}</p>
      <div className="flex items-baseline gap-2">
        <span className="text-2xl font-bold tabular-nums text-foreground">{metric.value}</span>
        <span className="text-sm text-muted-foreground">{metric.unit}</span>
        <TrendIcon className={cn('w-4 h-4 ml-auto', trendColor)} />
      </div>
      {metric.target && (
        <p className="text-xs text-muted-foreground">Target: {metric.target} {metric.unit}</p>
      )}
    </PlannerCard>
  );
}

function TimecardRow({ card }: { card: TimecardApproval }) {
  const statusMap: Record<string, 'success' | 'warning' | 'error'> = {
    approved: 'success',
    pending: 'warning',
    exception: 'error',
  };

  return (
    <div className={cn(
      'flex items-center gap-4 px-4 py-3 rounded-xl',
      'bg-white/[0.02] hover:bg-white/[0.04]',
      'border border-white/[0.04]',
      'transition-colors duration-200'
    )}>
      <Avatar name={card.employee} size="sm" />
      <div className="flex-1 min-w-0">
        <p className="text-sm font-semibold text-foreground truncate">{card.employee}</p>
        <p className="text-xs text-muted-foreground">{card.role}</p>
      </div>
      <div className="text-right">
        <p className="text-sm font-semibold tabular-nums text-foreground">{card.hours.toFixed(1)} hrs</p>
        <p className="text-xs text-muted-foreground">${card.cost.toFixed(0)}</p>
      </div>
      <StatusBadge status={statusMap[card.status]} label={card.status} />
    </div>
  );
}

function EmployeeRow({ employee }: { employee: typeof employees[0] }) {
  return (
    <div className={cn(
      'flex items-center gap-4 px-4 py-3 rounded-xl',
      'bg-white/[0.02] hover:bg-white/[0.04]',
      'border border-white/[0.04]',
      'transition-colors duration-200'
    )}>
      <Avatar name={employee.name} size="md" />
      <div className="flex-1 min-w-0">
        <p className="text-sm font-semibold text-foreground">{employee.name}</p>
        <p className="text-xs text-muted-foreground">{employee.role}</p>
      </div>
      <div className="flex flex-wrap gap-1 max-w-[200px]">
        {employee.skills.slice(0, 2).map((skill) => (
          <span
            key={skill}
            className="px-2 py-0.5 rounded-full bg-white/[0.04] text-xs text-muted-foreground"
          >
            {skill}
          </span>
        ))}
        {employee.skills.length > 2 && (
          <span className="px-2 py-0.5 rounded-full bg-white/[0.04] text-xs text-muted-foreground">
            +{employee.skills.length - 2}
          </span>
        )}
      </div>
    </div>
  );
}

function IntegrationRow({ integration }: { integration: typeof integrationStatuses[0] }) {
  const statusMap: Record<string, 'success' | 'warning' | 'error' | 'neutral'> = {
    connected: 'success',
    pending: 'warning',
    error: 'error',
    disabled: 'neutral',
  };

  return (
    <div className={cn(
      'flex items-center gap-4 px-4 py-3 rounded-xl',
      'bg-white/[0.02] hover:bg-white/[0.04]',
      'border border-white/[0.04]',
      'transition-colors duration-200'
    )}>
      <div className="p-2 rounded-lg bg-white/[0.04]">
        <Link2 className="w-4 h-4 text-muted-foreground" />
      </div>
      <div className="flex-1 min-w-0">
        <p className="text-sm font-semibold text-foreground">{integration.name}</p>
        <p className="text-xs text-muted-foreground truncate">{integration.description}</p>
      </div>
      <StatusBadge status={statusMap[integration.status]} label={integration.status} dot />
    </div>
  );
}

export default function PlannerHomePage() {
  return (
    <div className="flex flex-col gap-6 p-6 max-w-[1600px] mx-auto">
      {/* Page Header */}
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-foreground">Labor Overview</h1>
          <p className="text-sm text-muted-foreground mt-1">
            Coverage, scheduling, and workforce performance at a glance
          </p>
        </div>
        <div className="flex gap-2">
          <Link
            href="/dashboard/planner/batch-planning"
            className={cn(
              'flex items-center gap-2 px-4 py-2 rounded-xl',
              'bg-emerald-500/10 text-emerald-400 text-sm font-medium',
              'hover:bg-emerald-500/20 transition-colors duration-200'
            )}
          >
            <Calendar className="w-4 h-4" />
            Batch Planning
          </Link>
          <Link
            href="/dashboard/planner/time-approvals"
            className={cn(
              'flex items-center gap-2 px-4 py-2 rounded-xl',
              'bg-white/[0.04] text-foreground text-sm font-medium',
              'hover:bg-white/[0.06] transition-colors duration-200'
            )}
          >
            <Clock className="w-4 h-4" />
            Review Timecards
          </Link>
        </div>
      </div>

      {/* Coverage Cards */}
      <section>
        <SectionHeader
          icon={Users}
          title="Staff Coverage"
          subtitle="Current shift fill rates by area"
        />
        <div className="grid gap-4 md:grid-cols-3">
          {laborCoverage.map((card) => (
            <CoverageCard key={card.id} card={card} />
          ))}
        </div>
      </section>

      {/* Two Column Layout */}
      <div className="grid gap-6 lg:grid-cols-3">
        {/* Shift Needs */}
        <div className="lg:col-span-2">
          <SectionHeader
            icon={Calendar}
            title="Open Shifts"
            subtitle="Positions needing assignment"
            actionLabel="Shift Board"
            actionHref="/dashboard/planner/shift-board"
          />
          <div className="space-y-2">
            {shiftNeeds.map((need) => (
              <ShiftNeedRow key={need.id} need={need} />
            ))}
          </div>
        </div>

        {/* Budget */}
        <div>
          <SectionHeader
            icon={DollarSign}
            title="Labor Cost"
            subtitle="Budget vs actual spend"
          />
          <div className="space-y-3">
            {budgetVariances.map((item) => (
              <BudgetCard key={item.id} item={item} />
            ))}
          </div>
        </div>
      </div>

      {/* Two Column Layout */}
      <div className="grid gap-6 lg:grid-cols-3">
        {/* Timecards */}
        <div className="lg:col-span-2">
          <SectionHeader
            icon={Clock}
            title="Timecards"
            subtitle="Entries needing review"
            actionLabel="Approve All"
            actionHref="/dashboard/planner/time-approvals"
          />
          <div className="space-y-2">
            {timecards.map((card) => (
              <TimecardRow key={card.id} card={card} />
            ))}
          </div>
        </div>

        {/* Productivity */}
        <div>
          <SectionHeader
            icon={TrendingUp}
            title="Productivity"
            subtitle="Key efficiency metrics"
            actionLabel="Details"
            actionHref="/dashboard/planner/productivity"
          />
          <div className="space-y-3">
            {productivity.map((metric) => (
              <ProductivityCard key={metric.id} metric={metric} />
            ))}
          </div>
        </div>
      </div>

      {/* Two Column Layout */}
      <div className="grid gap-6 lg:grid-cols-3">
        {/* Employees */}
        <div className="lg:col-span-2">
          <SectionHeader
            icon={Users}
            title="Team"
            subtitle="Skills and certifications"
          />
          <div className="space-y-2">
            {employees.map((employee) => (
              <EmployeeRow key={employee.id} employee={employee} />
            ))}
          </div>
        </div>

        {/* Integrations */}
        <div>
          <SectionHeader
            icon={Link2}
            title="Integrations"
            subtitle="Connected systems"
            actionLabel="Settings"
            actionHref="/dashboard/planner/settings"
          />
          <div className="space-y-2">
            {integrationStatuses.map((integration) => (
              <IntegrationRow key={integration.id} integration={integration} />
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
