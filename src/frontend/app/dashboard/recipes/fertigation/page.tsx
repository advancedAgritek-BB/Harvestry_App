'use client';

import React from 'react';
import { RecipeCard } from '@/features/dashboard/widgets/recipes/RecipeCard';
import { Plus } from 'lucide-react';

export default function FertigationRecipesPage() {
  const recipes = [
    {
      id: '1',
      name: 'Standard Veg - Week 1-2',
      description: 'Balanced NPK for early vegetative growth. High nitrogen for leaf development.',
      phase: 'Veg' as const,
      version: '1.2',
      lastModified: '2 days ago',
      metrics: [
        { label: 'Target EC', value: '1.2 mS' },
        { label: 'Target pH', value: '5.8' },
        { label: 'Base', value: 'A+B' },
        { label: 'Additives', value: '2' },
      ],
      type: 'fertigation' as const
    },
    {
      id: '2',
      name: 'Standard Veg - Week 3-4',
      description: 'Increased strength for late veg. Prepares plant for flower stretch.',
      phase: 'Veg' as const,
      version: '1.0',
      lastModified: '1 week ago',
      metrics: [
        { label: 'Target EC', value: '1.8 mS' },
        { label: 'Target pH', value: '5.8' },
        { label: 'Base', value: 'A+B' },
        { label: 'Additives', value: '3' },
      ],
      type: 'fertigation' as const
    },
    {
      id: '3',
      name: 'Flower Early - Stretch',
      description: 'Transition formula. Reduced N, increased P/K for initial flower set.',
      phase: 'Flower' as const,
      version: '2.1',
      lastModified: '3 days ago',
      metrics: [
        { label: 'Target EC', value: '2.2 mS' },
        { label: 'Target pH', value: '5.9' },
        { label: 'Base', value: 'Bloom A+B' },
        { label: 'Additives', value: '4' },
      ],
      type: 'fertigation' as const
    },
    {
      id: '4',
      name: 'Flower Late - Ripen',
      description: 'Finishing formula. Minimal N, high K for density. Includes flush taper.',
      phase: 'Flower' as const,
      version: '1.5',
      lastModified: 'Yesterday',
      metrics: [
        { label: 'Target EC', value: '1.5 mS' },
        { label: 'Target pH', value: '6.0' },
        { label: 'Base', value: 'Bloom A+B' },
        { label: 'Additives', value: '1' },
      ],
      type: 'fertigation' as const
    },
    {
      id: '5',
      name: 'Clone Rooting',
      description: 'Low strength starter mix with rooting hormone support.',
      phase: 'Clone' as const,
      version: '1.0',
      lastModified: '1 month ago',
      metrics: [
        { label: 'Target EC', value: '0.6 mS' },
        { label: 'Target pH', value: '5.8' },
        { label: 'Base', value: 'Start' },
        { label: 'Additives', value: '1' },
      ],
      type: 'fertigation' as const
    }
  ];

  return (
    <div className="max-w-6xl mx-auto">
      {/* Page Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h2 className="text-lg font-bold text-foreground">Fertigation Recipes</h2>
          <p className="text-sm text-muted-foreground">Manage nutrient mixes and target parameters</p>
        </div>
        <button className="flex items-center gap-2 px-4 py-2 bg-purple-600 hover:bg-purple-500 text-foreground rounded-lg font-medium transition-colors">
          <Plus className="w-4 h-4" />
          Create Recipe
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
