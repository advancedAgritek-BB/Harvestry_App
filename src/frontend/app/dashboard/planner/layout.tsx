import React from 'react';
import { PlannerNavigation } from '@/features/planner/components';

export default function PlannerLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex h-full flex-col bg-[var(--bg-primary)]">
      <PlannerNavigation />
      <main className="flex-1 overflow-auto">
        {children}
      </main>
    </div>
  );
}



