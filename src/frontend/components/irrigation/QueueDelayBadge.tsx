'use client';

import React from 'react';
import { Clock, AlertTriangle, HelpCircle } from 'lucide-react';
import { cn } from '@/lib/utils';

export interface QueueInfo {
  isQueued: boolean;
  originalTime: string;
  expectedTime: string;
  delayMinutes: number;
  queuePosition: number;
  reason?: string;
}

interface QueueDelayBadgeProps {
  queueInfo: QueueInfo;
  showTooltip?: boolean;
  size?: 'sm' | 'md' | 'lg';
  className?: string;
}

/**
 * Format delay duration for display
 * e.g., "+5min", "+1h 30min"
 */
function formatDelay(minutes: number): string {
  if (minutes < 1) return '+<1min';
  if (minutes < 60) return `+${Math.round(minutes)}min`;
  
  const hours = Math.floor(minutes / 60);
  const remainingMins = Math.round(minutes % 60);
  
  return remainingMins > 0 
    ? `+${hours}h ${remainingMins}min` 
    : `+${hours}h`;
}

/**
 * Get urgency level based on delay duration
 */
function getDelayUrgency(minutes: number): 'normal' | 'warning' | 'critical' {
  if (minutes < 10) return 'normal';
  if (minutes < 30) return 'warning';
  return 'critical';
}

const URGENCY_STYLES = {
  normal: {
    bg: 'bg-amber-500/20',
    border: 'border-amber-500/30',
    text: 'text-amber-400',
    icon: 'text-amber-400',
  },
  warning: {
    bg: 'bg-orange-500/20',
    border: 'border-orange-500/30',
    text: 'text-orange-400',
    icon: 'text-orange-400',
  },
  critical: {
    bg: 'bg-red-500/20',
    border: 'border-red-500/30',
    text: 'text-red-400',
    icon: 'text-red-400',
  },
};

const SIZE_STYLES = {
  sm: 'text-[10px] px-1.5 py-0.5 gap-1',
  md: 'text-xs px-2 py-1 gap-1.5',
  lg: 'text-sm px-3 py-1.5 gap-2',
};

const ICON_SIZES = {
  sm: 'w-3 h-3',
  md: 'w-3.5 h-3.5',
  lg: 'w-4 h-4',
};

export function QueueDelayBadge({
  queueInfo,
  showTooltip = true,
  size = 'md',
  className,
}: QueueDelayBadgeProps) {
  if (!queueInfo.isQueued) return null;

  const urgency = getDelayUrgency(queueInfo.delayMinutes);
  const styles = URGENCY_STYLES[urgency];
  const formattedDelay = formatDelay(queueInfo.delayMinutes);

  return (
    <div className={cn('relative group inline-flex', className)}>
      <span
        className={cn(
          'inline-flex items-center rounded-full border font-medium',
          styles.bg,
          styles.border,
          styles.text,
          SIZE_STYLES[size]
        )}
      >
        {urgency === 'critical' ? (
          <AlertTriangle className={cn(styles.icon, ICON_SIZES[size])} />
        ) : (
          <Clock className={cn(styles.icon, ICON_SIZES[size])} />
        )}
        <span>{formattedDelay}</span>
      </span>

      {/* Tooltip */}
      {showTooltip && (
        <div className="absolute bottom-full left-1/2 -translate-x-1/2 mb-2 hidden group-hover:block z-50">
          <div className="bg-surface border border-border rounded-lg shadow-lg p-3 min-w-[200px]">
            <div className="text-xs font-medium text-foreground mb-2">
              Queued Event
            </div>
            
            <div className="space-y-1.5 text-xs">
              <div className="flex justify-between">
                <span className="text-muted-foreground">Original time:</span>
                <span className="text-foreground">{queueInfo.originalTime}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Expected time:</span>
                <span className={styles.text}>{queueInfo.expectedTime}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Queue position:</span>
                <span className="text-foreground">#{queueInfo.queuePosition}</span>
              </div>
            </div>

            {queueInfo.reason && (
              <div className="mt-2 pt-2 border-t border-border">
                <div className="text-[10px] text-muted-foreground">
                  <span className="font-medium">Reason:</span> {queueInfo.reason}
                </div>
              </div>
            )}

            {/* Tooltip arrow */}
            <div className="absolute top-full left-1/2 -translate-x-1/2 -mt-[1px]">
              <div className="border-8 border-transparent border-t-border" />
              <div className="absolute top-0 left-1/2 -translate-x-1/2 border-[7px] border-transparent border-t-surface" />
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

/**
 * Inline version for use within schedule time displays
 */
interface InlineQueueDelayProps {
  delayMinutes: number;
  className?: string;
}

export function InlineQueueDelay({ delayMinutes, className }: InlineQueueDelayProps) {
  if (delayMinutes <= 0) return null;

  const urgency = getDelayUrgency(delayMinutes);
  const styles = URGENCY_STYLES[urgency];
  const formattedDelay = formatDelay(delayMinutes);

  return (
    <span className={cn('font-medium', styles.text, className)}>
      {formattedDelay}
    </span>
  );
}

/**
 * Queue status indicator for schedule list items
 */
interface QueueStatusIndicatorProps {
  queuedCount: number;
  totalEvents: number;
  className?: string;
}

export function QueueStatusIndicator({
  queuedCount,
  totalEvents,
  className,
}: QueueStatusIndicatorProps) {
  if (queuedCount === 0) {
    return (
      <span className={cn('text-xs text-emerald-400', className)}>
        All events on time
      </span>
    );
  }

  const percentage = (queuedCount / totalEvents) * 100;
  const urgency = percentage > 50 ? 'critical' : percentage > 20 ? 'warning' : 'normal';
  const styles = URGENCY_STYLES[urgency];

  return (
    <span className={cn('text-xs flex items-center gap-1', styles.text, className)}>
      <Clock className="w-3 h-3" />
      {queuedCount} of {totalEvents} queued
    </span>
  );
}




