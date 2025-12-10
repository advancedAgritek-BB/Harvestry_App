'use client';

import React from 'react';
import Link from 'next/link';
import { AlertCircle, X, ChevronRight } from 'lucide-react';
import { cn } from '@/lib/utils';

export interface CriticalAlert {
  id: string;
  title: string;
  source: string;
  timestamp: string;
  href?: string;
}

interface AlertBannerProps {
  alerts: CriticalAlert[];
  onDismiss?: (alertId: string) => void;
  className?: string;
}

export function AlertBanner({ alerts, onDismiss, className }: AlertBannerProps) {
  if (alerts.length === 0) return null;

  const primaryAlert = alerts[0];
  const additionalCount = alerts.length - 1;

  return (
    <div
      className={cn(
        'relative flex items-center gap-4 px-5 py-3 rounded-xl',
        'bg-gradient-to-r from-rose-500/20 via-rose-500/10 to-transparent',
        'border border-rose-500/30',
        'animate-in slide-in-from-top-2 duration-300',
        className
      )}
    >
      {/* Pulsing indicator */}
      <div className="relative flex-shrink-0">
        <div className="absolute inset-0 rounded-full bg-rose-500/30 animate-ping" />
        <div className="relative p-2 rounded-full bg-rose-500/20">
          <AlertCircle className="w-5 h-5 text-rose-400" />
        </div>
      </div>

      {/* Alert content */}
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2">
          <span className="text-xs font-semibold uppercase tracking-wider text-rose-400">
            Critical Alert
          </span>
          {additionalCount > 0 && (
            <span className="px-2 py-0.5 text-[10px] font-bold rounded-full bg-rose-500/20 text-rose-300">
              +{additionalCount} more
            </span>
          )}
        </div>
        <p className="text-sm font-medium text-foreground truncate mt-0.5">
          {primaryAlert.title}
        </p>
        <p className="text-xs text-muted-foreground">
          {primaryAlert.source} • {formatTimeAgo(primaryAlert.timestamp)}
        </p>
      </div>

      {/* Actions */}
      <div className="flex items-center gap-2 flex-shrink-0">
        {primaryAlert.href ? (
          <Link
            href={primaryAlert.href}
            className="flex items-center gap-1 px-3 py-1.5 text-xs font-medium rounded-lg bg-rose-500/20 text-rose-300 hover:bg-rose-500/30 transition-colors"
          >
            View Details
            <ChevronRight className="w-3 h-3" />
          </Link>
        ) : (
          <button
            className="flex items-center gap-1 px-3 py-1.5 text-xs font-medium rounded-lg bg-rose-500/20 text-rose-300 hover:bg-rose-500/30 transition-colors"
          >
            Acknowledge
          </button>
        )}
        
        {onDismiss && (
          <button
            onClick={() => onDismiss(primaryAlert.id)}
            className="p-1.5 rounded-lg text-muted-foreground hover:text-foreground hover:bg-foreground/10 transition-colors"
            title="Dismiss"
          >
            <X className="w-4 h-4" />
          </button>
        )}
      </div>
    </div>
  );
}

function formatTimeAgo(timestamp: string): string {
  const now = new Date();
  const time = new Date(timestamp);
  const diffMs = now.getTime() - time.getTime();
  const diffMins = Math.floor(diffMs / 60000);
  
  if (diffMins < 1) return 'just now';
  if (diffMins < 60) return `${diffMins}m ago`;
  
  const diffHours = Math.floor(diffMins / 60);
  if (diffHours < 24) return `${diffHours}h ago`;
  
  const diffDays = Math.floor(diffHours / 24);
  return `${diffDays}d ago`;
}







