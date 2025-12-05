import React from 'react';
import { cn } from '@/lib/utils';

interface CultivationLayoutProps {
  children: React.ReactNode;
  className?: string;
}

/**
 * A specialized layout for the Cultivation Dashboard.
 * Enforces a specific grid structure:
 * - Row 1: Metrics (Full Width)
 * - Row 2: Charts (9 Cols) | Sidebar (3 Cols)
 * - Row 3: Footer (Full Width)
 */
export function CultivationLayout({ children, className }: CultivationLayoutProps) {
  return (
    <div 
      className={cn(
        "grid grid-cols-12 gap-4 p-4 h-full overflow-y-auto custom-scrollbar",
        className
      )}
    >
      {children}
    </div>
  );
}

// Helper to wrap specific zones of the layout
export function CultivationSection({ 
  children, 
  span = 12, 
  className 
}: { 
  children: React.ReactNode, 
  span?: number,
  className?: string 
}) {
  // Explicit lookup to ensure Tailwind classes are generated
  const spanClasses: Record<number, string> = {
    2: 'lg:col-span-2',
    3: 'lg:col-span-3',
    4: 'lg:col-span-4',
    5: 'lg:col-span-5',
    6: 'lg:col-span-6',
    7: 'lg:col-span-7',
    8: 'lg:col-span-8',
    9: 'lg:col-span-9',
    10: 'lg:col-span-10',
    12: 'lg:col-span-12',
  };

  return (
    <div 
      className={cn(
        "col-span-12",
        spanClasses[span] || 'lg:col-span-12',
        className
      )}
    >
      {children}
    </div>
  );
}

