'use client';

import React from 'react';
import { RecipeCard } from '@/features/dashboard/widgets/recipes/RecipeCard';
import { Plus } from 'lucide-react';

export default function LightingRecipesPage() {
  const recipes = [
    {
      id: 'l1',
      name: '18/6 Vegetative Max',
      description: 'Standard vegetative photoperiod. High DLI target for rapid biomass production.',
      phase: 'Veg' as const,
      version: '1.0',
      lastModified: '3 months ago',
      metrics: [
        { label: 'Schedule', value: '18/6' },
        { label: 'DLI Target', value: '45 mol' },
        { label: 'Peak PPFD', value: '900 µmol' },
        { label: 'Sunrise', value: '30 min' },
      ],
      type: 'lighting' as const
    },
    {
      id: 'l2',
      name: '12/12 Flower Standard',
      description: 'Classic flowering photoperiod. Includes far-red wake up and sleep initiation.',
      phase: 'Flower' as const,
      version: '1.2',
      lastModified: '2 weeks ago',
      metrics: [
        { label: 'Schedule', value: '12/12' },
        { label: 'DLI Target', value: '35 mol' },
        { label: 'Peak PPFD', value: '1100 µmol' },
        { label: 'Spectrum', value: 'Full' },
      ],
      type: 'lighting' as const
    },
    {
      id: 'l3',
      name: 'Clone Acclimation',
      description: 'Low intensity 24h light for initial rooting, tapering to 18/6.',
      phase: 'Clone' as const,
      version: '1.1',
      lastModified: '1 month ago',
      metrics: [
        { label: 'Schedule', value: '24/0' },
        { label: 'DLI Target', value: '12 mol' },
        { label: 'Peak PPFD', value: '150 µmol' },
        { label: 'Dimming', value: '20%' },
      ],
      type: 'lighting' as const
    }
  ];

  return (
    <div className="max-w-6xl mx-auto">
      {/* Page Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h2 className="text-lg font-bold text-foreground">Lighting Schedules</h2>
          <p className="text-sm text-muted-foreground">Manage photoperiods, spectrum, and intensity</p>
        </div>
        <button className="flex items-center gap-2 px-4 py-2 bg-amber-600 hover:bg-amber-500 text-foreground rounded-lg font-medium transition-colors">
          <Plus className="w-4 h-4" />
          Create Schedule
        </button>
      </div>

      {/* Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {recipes.map(recipe => (
          <RecipeCard 
            key={recipe.id} 
            {...recipe} 
            onDuplicate={() => console.log('Duplicate', recipe.id)}
          />
        ))}
      </div>
    </div>
  );
}
