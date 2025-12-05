import React from 'react';
import { cn } from '@/lib/utils';

interface DashboardGridProps {
  children: React.ReactNode;
  className?: string;
}

export function DashboardGrid({ children, className }: DashboardGridProps) {
  return (
    <div 
      className={cn(
        "grid grid-cols-1 md:grid-cols-2 lg:grid-cols-12 auto-rows-[minmax(180px,auto)] gap-4 p-4 md:p-6",
        className
      )}
    >
      {children}
    </div>
  );
}

