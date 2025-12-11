'use client';

import React, { useState } from 'react';
import Link from 'next/link';
import { AlertTriangle, AlertCircle, Info, X, ChevronRight } from 'lucide-react';
import { cn } from '@/lib/utils';
import { CriticalAlert, AlertSeverity } from '../../types';

interface CriticalAlertBannerProps {
  alerts: CriticalAlert[];
  className?: string;
  onDismiss?: (alertId: string) => void;
}

const SEVERITY_CONFIG: Record<
  AlertSeverity,
  {
    icon: typeof AlertTriangle;
    bgClass: string;
    borderClass: string;
    textClass: string;
    iconClass: string;
  }
> = {
  critical: {
    icon: AlertTriangle,
    bgClass: 'bg-rose-500/10',
    borderClass: 'border-rose-500/30',
    textClass: 'text-rose-400',
    iconClass: 'text-rose-400',
  },
  warning: {
    icon: AlertCircle,
    bgClass: 'bg-amber-500/10',
    borderClass: 'border-amber-500/30',
    textClass: 'text-amber-400',
    iconClass: 'text-amber-400',
  },
  info: {
    icon: Info,
    bgClass: 'bg-cyan-500/10',
    borderClass: 'border-cyan-500/30',
    textClass: 'text-cyan-400',
    iconClass: 'text-cyan-400',
  },
};

function AlertItem({
  alert,
  onDismiss,
  isOnly,
}: {
  alert: CriticalAlert;
  onDismiss?: (alertId: string) => void;
  isOnly: boolean;
}) {
  const config = SEVERITY_CONFIG[alert.severity];
  const Icon = config.icon;

  return (
    <div
      className={cn(
        'flex items-center gap-3 px-4 py-3 rounded-xl',
        'transition-all duration-200',
        config.bgClass,
        'border',
        config.borderClass
      )}
    >
      <Icon className={cn('w-5 h-5 flex-shrink-0', config.iconClass)} />

      <div className="flex-1 min-w-0">
        <p className={cn('text-sm font-semibold', config.textClass)}>
          {alert.title}
        </p>
        <p className="text-xs text-muted-foreground truncate">
          {alert.description}
        </p>
      </div>

      <Link
        href={alert.actionHref}
        className={cn(
          'flex items-center gap-1 px-3 py-1.5 rounded-lg',
          'text-sm font-medium',
          'bg-white/[0.06] hover:bg-white/[0.1]',
          'transition-colors duration-200',
          config.textClass
        )}
      >
        {alert.actionLabel}
        <ChevronRight className="w-4 h-4" />
      </Link>

      {alert.dismissable && onDismiss && (
        <button
          onClick={() => onDismiss(alert.id)}
          className={cn(
            'p-1.5 rounded-lg',
            'text-muted-foreground hover:text-foreground',
            'hover:bg-white/[0.06]',
            'transition-colors duration-200'
          )}
          aria-label="Dismiss alert"
        >
          <X className="w-4 h-4" />
        </button>
      )}
    </div>
  );
}

export function CriticalAlertBanner({
  alerts,
  className,
  onDismiss,
}: CriticalAlertBannerProps) {
  const [dismissedIds, setDismissedIds] = useState<Set<string>>(new Set());

  const visibleAlerts = alerts.filter((alert) => !dismissedIds.has(alert.id));

  const handleDismiss = (alertId: string) => {
    setDismissedIds((prev) => new Set([...prev, alertId]));
    onDismiss?.(alertId);
  };

  if (visibleAlerts.length === 0) {
    return null;
  }

  // Sort by severity: critical first, then warning, then info
  const sortedAlerts = [...visibleAlerts].sort((a, b) => {
    const severityOrder: Record<AlertSeverity, number> = {
      critical: 0,
      warning: 1,
      info: 2,
    };
    return severityOrder[a.severity] - severityOrder[b.severity];
  });

  const criticalCount = sortedAlerts.filter(
    (a) => a.severity === 'critical'
  ).length;
  const warningCount = sortedAlerts.filter(
    (a) => a.severity === 'warning'
  ).length;

  return (
    <div className={cn('space-y-3', className)}>
      {/* Summary header when multiple alerts */}
      {sortedAlerts.length > 1 && (
        <div className="flex items-center gap-3 text-sm">
          {criticalCount > 0 && (
            <span className="flex items-center gap-1.5 text-rose-400">
              <AlertTriangle className="w-4 h-4" />
              {criticalCount} Critical
            </span>
          )}
          {warningCount > 0 && (
            <span className="flex items-center gap-1.5 text-amber-400">
              <AlertCircle className="w-4 h-4" />
              {warningCount} Warning
            </span>
          )}
        </div>
      )}

      {/* Alert items */}
      <div className="space-y-2">
        {sortedAlerts.map((alert) => (
          <AlertItem
            key={alert.id}
            alert={alert}
            onDismiss={handleDismiss}
            isOnly={sortedAlerts.length === 1}
          />
        ))}
      </div>
    </div>
  );
}

export default CriticalAlertBanner;
