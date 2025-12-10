'use client';

import React from 'react';
import { cn } from '@/lib/utils';

interface AvatarProps {
  name: string;
  size?: 'sm' | 'md' | 'lg';
  className?: string;
}

function getInitials(name: string): string {
  return name
    .split(' ')
    .map((part) => part[0])
    .join('')
    .toUpperCase()
    .slice(0, 2);
}

function getColorFromName(name: string): string {
  const colors = [
    'bg-emerald-500/20 text-emerald-400',
    'bg-cyan-500/20 text-cyan-400',
    'bg-violet-500/20 text-violet-400',
    'bg-amber-500/20 text-amber-400',
    'bg-rose-500/20 text-rose-400',
    'bg-sky-500/20 text-sky-400',
  ];
  const index = name.charCodeAt(0) % colors.length;
  return colors[index];
}

const SIZE_STYLES = {
  sm: 'w-7 h-7 text-[10px]',
  md: 'w-9 h-9 text-xs',
  lg: 'w-11 h-11 text-sm',
};

export function Avatar({ name, size = 'md', className }: AvatarProps) {
  const initials = getInitials(name);
  const colorClass = getColorFromName(name);

  return (
    <div
      className={cn(
        'inline-flex items-center justify-center rounded-full font-semibold',
        'ring-1 ring-white/10',
        SIZE_STYLES[size],
        colorClass,
        className
      )}
    >
      {initials}
    </div>
  );
}

export default Avatar;


