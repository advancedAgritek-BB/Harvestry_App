'use client';

import React from 'react';
import { Settings, Link2, Clock, Shield, Save } from 'lucide-react';
import { integrationStatuses } from '@/features/planner/constants/laborMockData';
import { PlannerCard, SectionHeader, StatusBadge } from '@/features/planner/components/ui';
import { cn } from '@/lib/utils';

function IntegrationCard({ integration }: { integration: typeof integrationStatuses[0] }) {
  const statusMap: Record<string, 'success' | 'warning' | 'error' | 'neutral'> = {
    connected: 'success',
    pending: 'warning',
    error: 'error',
    disabled: 'neutral',
  };

  return (
    <PlannerCard className="flex items-center gap-4">
      <div className={cn(
        'p-3 rounded-xl',
        integration.status === 'connected' ? 'bg-emerald-500/10' : 'bg-white/[0.04]'
      )}>
        <Link2 className={cn(
          'w-5 h-5',
          integration.status === 'connected' ? 'text-emerald-400' : 'text-muted-foreground'
        )} />
      </div>
      <div className="flex-1 min-w-0">
        <p className="text-base font-semibold text-foreground">{integration.name}</p>
        <p className="text-sm text-muted-foreground">{integration.description}</p>
      </div>
      <StatusBadge status={statusMap[integration.status]} label={integration.status} dot />
      <button
        className={cn(
          'px-4 py-2 rounded-xl text-sm font-medium',
          'bg-white/[0.04] text-foreground',
          'hover:bg-white/[0.06] transition-colors duration-200'
        )}
      >
        Configure
      </button>
    </PlannerCard>
  );
}

function SettingInput({
  label,
  description,
  type = 'number',
  defaultValue,
  suffix,
}: {
  label: string;
  description: string;
  type?: string;
  defaultValue: string | number;
  suffix?: string;
}) {
  return (
    <div className="space-y-2">
      <label className="block">
        <span className="text-sm font-medium text-foreground">{label}</span>
        <p className="text-xs text-muted-foreground">{description}</p>
      </label>
      <div className="flex items-center gap-2">
        <input
          type={type}
          defaultValue={defaultValue}
          className={cn(
            'w-full px-4 py-2.5 rounded-xl',
            'bg-white/[0.04] border border-white/[0.06]',
            'text-foreground placeholder:text-muted-foreground/50',
            'focus:outline-none focus:ring-2 focus:ring-emerald-500/30 focus:border-emerald-500/50',
            'transition-all duration-200'
          )}
          aria-label={label}
        />
        {suffix && <span className="text-sm text-muted-foreground">{suffix}</span>}
      </div>
    </div>
  );
}

function SettingSelect({
  label,
  description,
  options,
  defaultValue,
}: {
  label: string;
  description: string;
  options: { value: string; label: string }[];
  defaultValue: string;
}) {
  return (
    <div className="space-y-2">
      <label className="block">
        <span className="text-sm font-medium text-foreground">{label}</span>
        <p className="text-xs text-muted-foreground">{description}</p>
      </label>
      <select
        defaultValue={defaultValue}
        className={cn(
          'w-full px-4 py-2.5 rounded-xl',
          'bg-white/[0.04] border border-white/[0.06]',
          'text-foreground',
          'focus:outline-none focus:ring-2 focus:ring-emerald-500/30 focus:border-emerald-500/50',
          'transition-all duration-200'
        )}
        aria-label={label}
      >
        {options.map((opt) => (
          <option key={opt.value} value={opt.value} className="bg-[var(--bg-surface)]">
            {opt.label}
          </option>
        ))}
      </select>
    </div>
  );
}

export default function PlannerSettingsPage() {
  return (
    <div className="flex flex-col gap-6 p-6 max-w-[1000px] mx-auto">
      {/* Page Header */}
      <div>
        <h1 className="text-2xl font-semibold text-foreground">Planner Settings</h1>
        <p className="text-sm text-muted-foreground mt-1">
          Configure labor rules, integrations, and compliance settings
        </p>
      </div>

      {/* Integrations */}
      <section>
        <SectionHeader
          icon={Link2}
          title="Integrations"
          subtitle="Connect external systems for data sync"
        />
        <div className="space-y-3">
          {integrationStatuses.map((integration) => (
            <IntegrationCard key={integration.id} integration={integration} />
          ))}
        </div>
      </section>

      {/* Compliance Rules */}
      <section>
        <SectionHeader
          icon={Shield}
          title="Compliance Rules"
          subtitle="Labor law and policy enforcement"
        />
        <PlannerCard className="space-y-6">
          <div className="grid gap-6 md:grid-cols-2">
            <SettingInput
              label="Overtime Threshold"
              description="Weekly hours before overtime applies"
              defaultValue={40}
              suffix="hours"
            />
            <SettingInput
              label="Meal Break Length"
              description="Required meal break duration"
              defaultValue={30}
              suffix="minutes"
            />
            <SettingInput
              label="Rest Break Frequency"
              description="Hours between required rest breaks"
              defaultValue={4}
              suffix="hours"
            />
            <SettingSelect
              label="Certification Enforcement"
              description="Block scheduling for expired certifications"
              defaultValue="yes"
              options={[
                { value: 'yes', label: 'Block assignment' },
                { value: 'warn', label: 'Warn only' },
                { value: 'no', label: 'No enforcement' },
              ]}
            />
          </div>
        </PlannerCard>
      </section>

      {/* Time Tracking */}
      <section>
        <SectionHeader
          icon={Clock}
          title="Time Tracking"
          subtitle="Clock in/out behavior and validation"
        />
        <PlannerCard className="space-y-6">
          <div className="grid gap-6 md:grid-cols-2">
            <SettingInput
              label="Early Clock-in Window"
              description="Minutes before shift start allowed"
              defaultValue={15}
              suffix="minutes"
            />
            <SettingInput
              label="Late Clock-out Grace"
              description="Minutes after shift end before alert"
              defaultValue={10}
              suffix="minutes"
            />
            <SettingSelect
              label="Geofencing"
              description="Require location for mobile clock-in"
              defaultValue="optional"
              options={[
                { value: 'required', label: 'Required' },
                { value: 'optional', label: 'Optional' },
                { value: 'disabled', label: 'Disabled' },
              ]}
            />
            <SettingSelect
              label="Auto-approve Timecards"
              description="Automatically approve entries without exceptions"
              defaultValue="no"
              options={[
                { value: 'yes', label: 'Yes' },
                { value: 'no', label: 'No' },
              ]}
            />
          </div>
        </PlannerCard>
      </section>

      {/* Save Button */}
      <div className="flex justify-end">
        <button
          className={cn(
            'flex items-center gap-2 px-6 py-3 rounded-xl',
            'bg-emerald-500 text-white text-sm font-semibold',
            'hover:bg-emerald-600 transition-colors duration-200',
            'shadow-lg shadow-emerald-500/20'
          )}
        >
          <Save className="w-4 h-4" />
          Save Settings
        </button>
      </div>
    </div>
  );
}


