'use client';

import React from 'react';
import { useParams } from 'next/navigation';
import { ArrowLeft, Save } from 'lucide-react';
import Link from 'next/link';

export default function EnvironmentRecipeDetailPage() {
  const params = useParams();
  
  return (
    <div className="max-w-4xl mx-auto">
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center gap-4">
          <Link href="/library/environment" className="p-2 hover:bg-muted rounded-full text-muted-foreground hover:text-foreground transition-colors">
            <ArrowLeft className="w-5 h-5" />
          </Link>
          <div>
            <h1 className="text-xl font-bold text-foreground">High VPD Veg</h1>
            <p className="text-sm text-muted-foreground">Environmental Blueprint • v3.0</p>
          </div>
        </div>
        <button className="flex items-center gap-2 px-4 py-2 bg-emerald-600 hover:bg-emerald-500 text-foreground rounded-lg font-medium transition-colors">
          <Save className="w-4 h-4" />
          Save Changes
        </button>
      </div>

      <div className="bg-surface/50 border border-border rounded-xl p-6 flex items-center justify-center h-64 text-muted-foreground">
        [Environment Editor Placeholder - Chart + Form]
      </div>
    </div>
  );
}
