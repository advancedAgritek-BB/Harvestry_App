'use client';

import { cn } from '@/lib/utils';
import {
  CheckCircle,
  Clock,
  AlertTriangle,
  XCircle,
  Circle,
  Loader2,
  Shield,
  ShieldCheck,
  ShieldX,
  ShieldAlert,
} from 'lucide-react';

// Order/Shipment/Transfer status badge
export type EntityStatus =
  | 'Draft'
  | 'Submitted'
  | 'Allocated'
  | 'Picking'
  | 'Packed'
  | 'Shipped'
  | 'Ready'
  | 'Cancelled'
  | 'Voided';

const STATUS_CONFIG: Record<
  EntityStatus,
  { color: string; bgColor: string; borderColor: string; icon: React.ElementType }
> = {
  Draft: {
    color: 'text-slate-400',
    bgColor: 'bg-slate-500/10',
    borderColor: 'border-slate-500/30',
    icon: Circle,
  },
  Submitted: {
    color: 'text-blue-400',
    bgColor: 'bg-blue-500/10',
    borderColor: 'border-blue-500/30',
    icon: Clock,
  },
  Allocated: {
    color: 'text-violet-400',
    bgColor: 'bg-violet-500/10',
    borderColor: 'border-violet-500/30',
    icon: CheckCircle,
  },
  Picking: {
    color: 'text-cyan-400',
    bgColor: 'bg-cyan-500/10',
    borderColor: 'border-cyan-500/30',
    icon: Loader2,
  },
  Packed: {
    color: 'text-amber-400',
    bgColor: 'bg-amber-500/10',
    borderColor: 'border-amber-500/30',
    icon: CheckCircle,
  },
  Shipped: {
    color: 'text-emerald-400',
    bgColor: 'bg-emerald-500/10',
    borderColor: 'border-emerald-500/30',
    icon: CheckCircle,
  },
  Ready: {
    color: 'text-emerald-400',
    bgColor: 'bg-emerald-500/10',
    borderColor: 'border-emerald-500/30',
    icon: CheckCircle,
  },
  Cancelled: {
    color: 'text-rose-400',
    bgColor: 'bg-rose-500/10',
    borderColor: 'border-rose-500/30',
    icon: XCircle,
  },
  Voided: {
    color: 'text-rose-400',
    bgColor: 'bg-rose-500/10',
    borderColor: 'border-rose-500/30',
    icon: XCircle,
  },
};

interface StatusBadgeProps {
  status: EntityStatus | string;
  size?: 'sm' | 'md';
  showIcon?: boolean;
}

export function StatusBadge({ status, size = 'sm', showIcon = true }: StatusBadgeProps) {
  const config = STATUS_CONFIG[status as EntityStatus] ?? {
    color: 'text-slate-400',
    bgColor: 'bg-slate-500/10',
    borderColor: 'border-slate-500/30',
    icon: Circle,
  };

  const Icon = config.icon;

  return (
    <span
      className={cn(
        'inline-flex items-center gap-1.5 rounded-full border font-medium',
        config.color,
        config.bgColor,
        config.borderColor,
        size === 'sm' ? 'px-2 py-0.5 text-xs' : 'px-3 py-1 text-sm'
      )}
    >
      {showIcon && (
        <Icon
          className={cn(
            size === 'sm' ? 'w-3 h-3' : 'w-4 h-4',
            status === 'Picking' && 'animate-spin'
          )}
        />
      )}
      {status}
    </span>
  );
}

// Compliance verification badge
export type ComplianceStatus = 'Unknown' | 'Verified' | 'Pending' | 'Failed';

const COMPLIANCE_CONFIG: Record<
  ComplianceStatus,
  { color: string; bgColor: string; borderColor: string; icon: React.ElementType; label: string }
> = {
  Unknown: {
    color: 'text-slate-400',
    bgColor: 'bg-slate-500/10',
    borderColor: 'border-slate-500/30',
    icon: Shield,
    label: 'Not Verified',
  },
  Verified: {
    color: 'text-emerald-400',
    bgColor: 'bg-emerald-500/10',
    borderColor: 'border-emerald-500/30',
    icon: ShieldCheck,
    label: 'Verified',
  },
  Pending: {
    color: 'text-amber-400',
    bgColor: 'bg-amber-500/10',
    borderColor: 'border-amber-500/30',
    icon: ShieldAlert,
    label: 'Pending',
  },
  Failed: {
    color: 'text-rose-400',
    bgColor: 'bg-rose-500/10',
    borderColor: 'border-rose-500/30',
    icon: ShieldX,
    label: 'Failed',
  },
};

interface ComplianceBadgeProps {
  status: ComplianceStatus;
  size?: 'sm' | 'md';
  showLabel?: boolean;
}

export function ComplianceBadge({ status, size = 'sm', showLabel = true }: ComplianceBadgeProps) {
  const config = COMPLIANCE_CONFIG[status];
  const Icon = config.icon;

  return (
    <span
      className={cn(
        'inline-flex items-center gap-1.5 rounded-full border font-medium',
        config.color,
        config.bgColor,
        config.borderColor,
        size === 'sm' ? 'px-2 py-0.5 text-xs' : 'px-3 py-1 text-sm'
      )}
      title={`License ${config.label}`}
    >
      <Icon className={size === 'sm' ? 'w-3 h-3' : 'w-4 h-4'} />
      {showLabel && config.label}
    </span>
  );
}

// METRC sync status badge
export type MetrcSyncStatus = 'Synced' | 'Pending' | 'Failed' | 'NotRequired';

const METRC_CONFIG: Record<
  MetrcSyncStatus,
  { color: string; bgColor: string; label: string }
> = {
  Synced: {
    color: 'text-emerald-400',
    bgColor: 'bg-emerald-500/10',
    label: 'Synced',
  },
  Pending: {
    color: 'text-amber-400',
    bgColor: 'bg-amber-500/10',
    label: 'Pending',
  },
  Failed: {
    color: 'text-rose-400',
    bgColor: 'bg-rose-500/10',
    label: 'Failed',
  },
  NotRequired: {
    color: 'text-slate-400',
    bgColor: 'bg-slate-500/10',
    label: '—',
  },
};

interface MetrcBadgeProps {
  status: MetrcSyncStatus | string | null;
}

export function MetrcBadge({ status }: MetrcBadgeProps) {
  const config = METRC_CONFIG[(status as MetrcSyncStatus) ?? 'NotRequired'] ?? METRC_CONFIG.NotRequired;

  return (
    <span
      className={cn(
        'inline-flex items-center px-2 py-0.5 rounded text-xs font-medium',
        config.color,
        config.bgColor
      )}
    >
      {config.label}
    </span>
  );
}
