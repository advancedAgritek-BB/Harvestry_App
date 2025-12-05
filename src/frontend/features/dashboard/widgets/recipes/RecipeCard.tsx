import React from 'react';
import { cn } from '@/lib/utils';
import { Edit2, MoreVertical, Copy } from 'lucide-react';
import Link from 'next/link';

interface RecipeCardProps {
  id: string;
  name: string;
  description: string;
  phase: 'Clone' | 'Veg' | 'Flower' | 'Cure';
  version: string;
  lastModified: string;
  metrics: { label: string; value: string }[];
  type: 'fertigation' | 'environment' | 'lighting';
  onDuplicate?: () => void;
}

export function RecipeCard({ 
  id, 
  name, 
  description, 
  phase, 
  version, 
  lastModified, 
  metrics, 
  type,
  onDuplicate
}: RecipeCardProps) {
  
  const phaseColors = {
    'Clone': 'bg-blue-500/10 text-blue-400 border-blue-500/20',
    'Veg': 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20',
    'Flower': 'bg-purple-500/10 text-purple-400 border-purple-500/20',
    'Cure': 'bg-amber-500/10 text-amber-400 border-amber-500/20',
  };

  const typeHrefMap = {
    'fertigation': '/dashboard/recipes/fertigation',
    'environment': '/dashboard/recipes/environment',
    'lighting': '/dashboard/recipes/lighting',
  };

  return (
    <div className="group bg-surface/50 border border-border rounded-xl p-4 hover:border-border/80 transition-all hover:shadow-lg hover:shadow-background/20">
      {/* Header */}
      <div className="flex items-start justify-between mb-3">
        <div>
          <div className="flex items-center gap-2 mb-1">
            <span className={cn("text-[10px] uppercase font-bold px-1.5 py-0.5 rounded border", phaseColors[phase])}>
              {phase}
            </span>
            <span className="text-[10px] text-muted-foreground font-mono">v{version}</span>
          </div>
          <h3 className="font-bold text-foreground group-hover:text-foreground transition-colors">{name}</h3>
        </div>
        <button className="text-muted-foreground hover:text-foreground/70 p-1 rounded hover:bg-muted">
          <MoreVertical className="w-4 h-4" />
        </button>
      </div>

      <p className="text-xs text-muted-foreground mb-4 line-clamp-2 min-h-[2.5em]">
        {description}
      </p>

      {/* Metrics Grid */}
      <div className="grid grid-cols-2 gap-2 mb-4">
        {metrics.map((m, i) => (
          <div key={i} className="bg-muted/50 rounded p-2 border border-border">
            <div className="text-[10px] text-muted-foreground uppercase">{m.label}</div>
            <div className="text-sm font-mono font-medium text-foreground">{m.value}</div>
          </div>
        ))}
      </div>

      {/* Footer Actions */}
      <div className="flex items-center justify-between pt-3 border-t border-border/50">
        <span className="text-[10px] text-muted-foreground/60">Updated {lastModified}</span>
        <div className="flex gap-2">
          {onDuplicate && (
            <button 
              onClick={onDuplicate}
              className="p-1.5 text-muted-foreground hover:text-foreground hover:bg-muted rounded transition-colors"
              title="Duplicate"
            >
              <Copy className="w-3.5 h-3.5" />
            </button>
          )}
          <Link
            href={`${typeHrefMap[type]}/${id}`}
            className="flex items-center gap-1.5 px-3 py-1.5 bg-muted hover:bg-muted/80 text-xs font-medium text-foreground rounded transition-colors"
          >
            <Edit2 className="w-3.5 h-3.5" />
            Edit
          </Link>
        </div>
      </div>
    </div>
  );
}
