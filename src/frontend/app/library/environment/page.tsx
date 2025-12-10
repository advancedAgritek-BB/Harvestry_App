'use client';

import React from 'react';
import { RecipeCard } from '@/features/dashboard/widgets/recipes/RecipeCard';
import { Plus } from 'lucide-react';

export default function EnvironmentRecipesPage() {
  const recipes = [
    {
      id: 'e1',
      name: 'High VPD Veg',
      description: 'Aggressive vegetative growth targets. Higher temp and humidity to drive transpiration.',
      phase: 'Veg' as const,
      version: '3.0',
      lastModified: '5 days ago',
      metrics: [
        { label: 'Day Temp', value: '82°F' },
        { label: 'Day RH', value: '70%' },
        { label: 'Target VPD', value: '1.0 kPa' },
        { label: 'CO2', value: '800 ppm' },
      ],
      type: 'environment' as const
    },
    {
      id: 'e2',
      name: 'Standard Flower - Early',
      description: 'Moderate conditions for transition. Lower humidity to prevent issues as canopy fills.',
      phase: 'Flower' as const,
      version: '2.2',
      lastModified: '2 weeks ago',
      metrics: [
        { label: 'Day Temp', value: '78°F' },
        { label: 'Day RH', value: '60%' },
        { label: 'Target VPD', value: '1.2 kPa' },
        { label: 'CO2', value: '1000 ppm' },
      ],
      type: 'environment' as const
    },
    {
      id: 'e3',
      name: 'Standard Flower - Late',
      description: 'Cooler temps and low humidity for ripening and terpene preservation.',
      phase: 'Flower' as const,
      version: '1.4',
      lastModified: 'Yesterday',
      metrics: [
        { label: 'Day Temp', value: '72°F' },
        { label: 'Day RH', value: '45%' },
        { label: 'Target VPD', value: '1.4 kPa' },
        { label: 'CO2', value: '400 ppm' },
      ],
      type: 'environment' as const
    },
    {
      id: 'e4',
      name: 'Cure - Initial',
      description: 'Slow dry settings. Controlled humidity step-down.',
      phase: 'Cure' as const,
      version: '1.0',
      lastModified: '1 month ago',
      metrics: [
        { label: 'Temp', value: '60°F' },
        { label: 'RH', value: '60%' },
        { label: 'Airflow', value: 'Low' },
        { label: 'Duration', value: '14 Days' },
      ],
      type: 'environment' as const
    }
  ];

  return (
    <div className="max-w-6xl mx-auto">
      {/* Page Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h2 className="text-lg font-bold text-foreground">Environmental Blueprints</h2>
          <p className="text-sm text-muted-foreground">Manage climate targets and setpoints</p>
        </div>
        <button className="flex items-center gap-2 px-4 py-2 bg-emerald-600 hover:bg-emerald-500 text-foreground rounded-lg font-medium transition-colors">
          <Plus className="w-4 h-4" />
          Create Blueprint
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
