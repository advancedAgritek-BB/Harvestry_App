'use client';

import React from 'react';
import { cn } from '@/lib/utils';

interface ProgressRingProps {
  value: number;
  max: number;
  size?: number;
  strokeWidth?: number;
  className?: string;
  colorClass?: string;
  trackClass?: string;
  showPercent?: boolean;
}

export function ProgressRing({
  value,
  max,
  size = 48,
  strokeWidth = 4,
  className,
  colorClass = 'stroke-emerald-400',
  trackClass = 'stroke-white/10',
  showPercent = true,
}: ProgressRingProps) {
  const radius = (size - strokeWidth) / 2;
  const circumference = radius * 2 * Math.PI;
  const percent = Math.min(100, Math.round((value / max) * 100));
  const strokeDashoffset = circumference - (percent / 100) * circumference;

  return (
    <div className={cn('relative inline-flex items-center justify-center', className)}>
      <svg width={size} height={size} className="-rotate-90">
        {/* Track */}
        <circle
          cx={size / 2}
          cy={size / 2}
          r={radius}
          fill="none"
          strokeWidth={strokeWidth}
          className={trackClass}
        />
        {/* Progress */}
        <circle
          cx={size / 2}
          cy={size / 2}
          r={radius}
          fill="none"
          strokeWidth={strokeWidth}
          strokeLinecap="round"
          strokeDasharray={circumference}
          strokeDashoffset={strokeDashoffset}
          className={cn(colorClass, 'transition-all duration-500 ease-out')}
        />
      </svg>
      {showPercent && (
        <span className="absolute text-xs font-bold text-foreground tabular-nums">
          {percent}%
        </span>
      )}
    </div>
  );
}

export default ProgressRing;


