'use client';

import React from 'react';
import { useParams } from 'next/navigation';
import { ArrowLeft, Save } from 'lucide-react';
import Link from 'next/link';

export default function FertigationRecipeDetailPage() {
  const params = useParams();
  const isNew = params.id === 'new';

  return (
    <div className="max-w-4xl mx-auto">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center gap-4">
          <Link href="/dashboard/recipes/fertigation" className="p-2 hover:bg-muted rounded-full text-muted-foreground hover:text-foreground transition-colors">
            <ArrowLeft className="w-5 h-5" />
          </Link>
          <div>
            <h1 className="text-xl font-bold text-foreground">{isNew ? 'New Recipe' : 'Veg Week 3'}</h1>
            <p className="text-sm text-muted-foreground">Fertigation Recipe • v1.2</p>
          </div>
        </div>
        <button className="flex items-center gap-2 px-4 py-2 bg-purple-600 hover:bg-purple-500 text-foreground rounded-lg font-medium transition-colors">
          <Save className="w-4 h-4" />
          Save Changes
        </button>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Main Settings */}
        <div className="lg:col-span-2 space-y-6">
          {/* Basic Info */}
          <section className="bg-surface/50 border border-border rounded-xl p-5">
            <h3 className="text-sm font-bold text-foreground/70 uppercase tracking-wider mb-4">Basic Information</h3>
            <div className="grid grid-cols-2 gap-4">
              <div className="col-span-2">
                <label className="block text-xs font-medium text-muted-foreground mb-1">Recipe Name</label>
                <input type="text" defaultValue="Veg Week 3" className="w-full bg-elevated border border-border rounded-lg px-3 py-2 text-sm text-foreground focus:outline-none focus:border-purple-500/50" />
              </div>
              <div>
                <label className="block text-xs font-medium text-muted-foreground mb-1">Growth Phase</label>
                <select className="w-full bg-elevated border border-border rounded-lg px-3 py-2 text-sm text-foreground focus:outline-none focus:border-purple-500/50">
                  <option>Clone</option>
                  <option selected>Veg</option>
                  <option>Flower</option>
                </select>
              </div>
              <div>
                <label className="block text-xs font-medium text-muted-foreground mb-1">Version</label>
                <input type="text" defaultValue="1.2" className="w-full bg-elevated border border-border rounded-lg px-3 py-2 text-sm text-muted-foreground" disabled />
              </div>
            </div>
          </section>

          {/* Nutrient Mix */}
          <section className="bg-surface/50 border border-border rounded-xl p-5">
            <h3 className="text-sm font-bold text-foreground/70 uppercase tracking-wider mb-4">Nutrient Mix</h3>
            <div className="space-y-3">
              {[
                { name: 'Base A', amount: '5 mL/gal' },
                { name: 'Base B', amount: '5 mL/gal' },
                { name: 'CalMag', amount: '2 mL/gal' },
                { name: 'Silica', amount: '1 mL/gal' }
              ].map((nutrient, i) => (
                <div key={i} className="flex items-center gap-4 bg-elevated/50 p-3 rounded-lg border border-border/50">
                  <div className="flex-1">
                    <span className="text-sm font-medium text-foreground">{nutrient.name}</span>
                  </div>
                  <input 
                    type="text" 
                    defaultValue={nutrient.amount} 
                    className="w-24 bg-surface border border-border rounded px-2 py-1 text-sm text-right text-cyan-400 font-mono"
                  />
                </div>
              ))}
              <button className="w-full py-2 border border-dashed border-border rounded-lg text-xs font-medium text-muted-foreground hover:text-foreground/70 hover:border-border/80 transition-colors">
                + Add Nutrient Channel
              </button>
            </div>
          </section>
        </div>

        {/* Targets Sidebar */}
        <div className="space-y-6">
          <section className="bg-surface/50 border border-border rounded-xl p-5">
            <h3 className="text-sm font-bold text-foreground/70 uppercase tracking-wider mb-4">Target Parameters</h3>
            <div className="space-y-4">
              <div>
                <label className="flex justify-between text-xs font-medium text-muted-foreground mb-1">
                  <span>Target EC (mS/cm)</span>
                  <span className="text-cyan-400">2.4</span>
                </label>
                <input type="range" min="0" max="5" step="0.1" defaultValue="2.4" className="w-full accent-cyan-500 h-1 bg-muted rounded-lg appearance-none cursor-pointer" />
              </div>
              <div>
                <label className="flex justify-between text-xs font-medium text-muted-foreground mb-1">
                  <span>Target pH</span>
                  <span className="text-purple-400">5.8</span>
                </label>
                <input type="range" min="4" max="8" step="0.1" defaultValue="5.8" className="w-full accent-purple-500 h-1 bg-muted rounded-lg appearance-none cursor-pointer" />
              </div>
              <div>
                <label className="block text-xs font-medium text-muted-foreground mb-1">Water Temp (°F)</label>
                <input type="number" defaultValue="68" className="w-full bg-elevated border border-border rounded-lg px-3 py-2 text-sm text-foreground" />
              </div>
            </div>
          </section>
        </div>
      </div>
    </div>
  );
}
