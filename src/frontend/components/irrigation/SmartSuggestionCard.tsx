'use client';

import React from 'react';
import {
  Clock,
  Layers,
  Gauge,
  AlertCircle,
  ArrowRight,
  X,
  TrendingUp,
} from 'lucide-react';
import { cn } from '@/lib/utils';

export type SuggestionType = 'time-shift' | 'sequential' | 'zone-grouping' | 'capacity';
export type SuggestionPriority = 'low' | 'medium' | 'high';

export interface ScheduleSuggestion {
  id: string;
  type: SuggestionType;
  title: string;
  description: string;
  currentValue: string;
  suggestedValue: string;
  estimatedImpactMinutes: number;
  priority: SuggestionPriority;
  affectedPrograms: string[];
}

interface SmartSuggestionCardProps {
  suggestions: ScheduleSuggestion[];
  onDismiss?: (id: string) => void;
  onApply?: (suggestion: ScheduleSuggestion) => void;
  className?: string;
}

interface SingleSuggestionProps {
  suggestion: ScheduleSuggestion;
  onDismiss?: () => void;
  onApply?: () => void;
}

const SUGGESTION_ICONS: Record<SuggestionType, typeof Clock> = {
  'time-shift': Clock,
  'sequential': Layers,
  'zone-grouping': Gauge,
  'capacity': AlertCircle,
};

const PRIORITY_COLORS: Record<SuggestionPriority, string> = {
  low: 'bg-muted border-border',
  medium: 'bg-amber-500/10 border-amber-500/20',
  high: 'bg-cyan-500/10 border-cyan-500/20',
};

const ICON_COLORS: Record<SuggestionPriority, string> = {
  low: 'bg-muted text-muted-foreground',
  medium: 'bg-amber-500/20 text-amber-400',
  high: 'bg-cyan-500/20 text-cyan-400',
};

function SingleSuggestion({ suggestion, onDismiss, onApply }: SingleSuggestionProps) {
  const Icon = SUGGESTION_ICONS[suggestion.type];

  return (
    <div className={cn(
      'p-4 rounded-lg border transition-all hover:bg-white/5',
      PRIORITY_COLORS[suggestion.priority]
    )}>
      <div className="flex items-start gap-3">
        <div className={cn(
          'w-8 h-8 rounded-lg flex items-center justify-center shrink-0',
          ICON_COLORS[suggestion.priority]
        )}>
          <Icon className="w-4 h-4" />
        </div>
        
        <div className="flex-1 min-w-0">
          <div className="flex items-start justify-between gap-2">
            <div className="font-medium text-foreground">{suggestion.title}</div>
            {onDismiss && (
              <button
                onClick={onDismiss}
                className="p-1 rounded-lg hover:bg-white/10 text-muted-foreground hover:text-foreground transition-colors"
                title="Dismiss"
              >
                <X className="w-4 h-4" />
              </button>
            )}
          </div>
          
          <p className="text-sm text-muted-foreground mt-1">
            {suggestion.description}
          </p>
          
          <div className="flex items-center flex-wrap gap-4 mt-3 text-xs">
            <span className="text-muted-foreground">
              Current: <span className="text-foreground">{suggestion.currentValue}</span>
            </span>
            <ArrowRight className="w-3 h-3 text-muted-foreground" />
            <span className="text-muted-foreground">
              Suggested: <span className="text-cyan-400">{suggestion.suggestedValue}</span>
            </span>
            <span className="text-emerald-400 flex items-center gap-1">
              <TrendingUp className="w-3 h-3" />
              ~{suggestion.estimatedImpactMinutes} min saved
            </span>
          </div>
          
          {suggestion.affectedPrograms.length > 0 && (
            <div className="mt-2 text-xs text-muted-foreground">
              Affects: {suggestion.affectedPrograms.slice(0, 3).join(', ')}
              {suggestion.affectedPrograms.length > 3 && ` +${suggestion.affectedPrograms.length - 3} more`}
            </div>
          )}
          
          {onApply && (
            <button
              onClick={onApply}
              className="mt-3 px-3 py-1.5 text-xs font-medium rounded-lg bg-cyan-600 hover:bg-cyan-500 text-white transition-colors"
            >
              Apply Suggestion
            </button>
          )}
        </div>
      </div>
    </div>
  );
}

export function SmartSuggestionCard({
  suggestions,
  onDismiss,
  onApply,
  className,
}: SmartSuggestionCardProps) {
  if (suggestions.length === 0) return null;

  return (
    <div className={cn(
      'rounded-xl border border-amber-500/20 bg-amber-500/5 overflow-hidden',
      className
    )}>
      <div className="px-4 py-3 border-b border-amber-500/20 bg-amber-500/10">
        <div className="flex items-center gap-2">
          <TrendingUp className="w-4 h-4 text-amber-400" />
          <span className="text-sm font-bold text-foreground">
            Schedule Optimization Suggestions
          </span>
          <span className="text-xs px-2 py-0.5 rounded-full bg-amber-500/20 text-amber-400">
            {suggestions.length} suggestion{suggestions.length !== 1 ? 's' : ''}
          </span>
        </div>
        <p className="text-xs text-muted-foreground mt-1">
          Based on recent queue patterns, here are ways to reduce irrigation delays
        </p>
      </div>
      
      <div className="p-4 space-y-3">
        {suggestions.map((suggestion) => (
          <SingleSuggestion
            key={suggestion.id}
            suggestion={suggestion}
            onDismiss={onDismiss ? () => onDismiss(suggestion.id) : undefined}
            onApply={onApply ? () => onApply(suggestion) : undefined}
          />
        ))}
      </div>
    </div>
  );
}

/**
 * Compact version for inline display
 */
interface CompactSuggestionBannerProps {
  suggestionCount: number;
  onClick: () => void;
  className?: string;
}

export function CompactSuggestionBanner({
  suggestionCount,
  onClick,
  className,
}: CompactSuggestionBannerProps) {
  if (suggestionCount === 0) return null;

  return (
    <button
      onClick={onClick}
      className={cn(
        'flex items-center gap-2 px-3 py-2 rounded-lg',
        'bg-amber-500/10 border border-amber-500/20',
        'hover:bg-amber-500/20 transition-colors',
        'text-sm text-amber-400',
        className
      )}
    >
      <TrendingUp className="w-4 h-4" />
      <span>
        {suggestionCount} schedule optimization{suggestionCount !== 1 ? 's' : ''} available
      </span>
      <ArrowRight className="w-4 h-4" />
    </button>
  );
}








