'use client';

import React from 'react';
import { BlueprintMatrix } from '@/features/dashboard/widgets/recipes/BlueprintMatrix';

export default function BlueprintsPage() {
  return (
    <div className="max-w-full">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h2 className="text-lg font-bold text-foreground">Master Blueprint Matrix</h2>
          <p className="text-sm text-muted-foreground">Map recipes to Strain × Phase combinations</p>
        </div>
        <div className="flex gap-2">
           <button className="px-3 py-1.5 text-xs font-medium bg-muted hover:bg-hover text-foreground rounded border border-border">
             Export Matrix
           </button>
        </div>
      </div>

      <BlueprintMatrix />
    </div>
  );
}






