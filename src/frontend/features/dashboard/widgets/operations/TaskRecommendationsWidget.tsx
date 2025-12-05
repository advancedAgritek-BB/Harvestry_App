'use client';

import React from 'react';
import { Lightbulb, AlertCircle, ChevronRight, Sparkles } from 'lucide-react';
import { cn } from '@/lib/utils';
import { useTaskRecommendations } from '@/features/tasks';
import type { TaskRecommendation } from '@/features/tasks';

function formatDate(date: Date): string {
  const today = new Date();
  const tomorrow = new Date(today);
  tomorrow.setDate(tomorrow.getDate() + 1);
  
  if (date.toDateString() === today.toDateString()) {
    return 'Today';
  }
  if (date.toDateString() === tomorrow.toDateString()) {
    return 'Tomorrow';
  }
  return date.toLocaleDateString([], { weekday: 'short', month: 'short', day: 'numeric' });
}

function RecommendationItem({ recommendation }: { recommendation: TaskRecommendation }) {
  return (
    <div
      className={cn(
        'group flex items-center gap-3 p-3 rounded-xl cursor-pointer transition-all duration-200',
        recommendation.isOverdue 
          ? 'bg-gradient-to-r from-rose-500/15 to-transparent border border-rose-500/30 hover:border-rose-500/50'
          : 'bg-surface/30 border border-border hover:bg-surface/50 hover:border-border/80'
      )}
    >
      {/* Priority/Status indicator */}
      <div className={cn(
        'p-2 rounded-lg shrink-0',
        recommendation.isOverdue ? 'bg-rose-500/20' : 'bg-amber-500/20'
      )}>
        {recommendation.isOverdue ? (
          <AlertCircle className="w-4 h-4 text-rose-400 animate-pulse" />
        ) : (
          <Lightbulb className="w-4 h-4 text-amber-400" />
        )}
      </div>

      <div className="flex-1 min-w-0">
        <h4 className="text-sm font-semibold text-foreground leading-tight truncate group-hover:text-cyan-300 transition-colors">
          {recommendation.blueprintTask.name}
        </h4>
        <div className="flex items-center gap-2 text-sm text-muted-foreground mt-0.5">
          <span className="truncate">{recommendation.batchName}</span>
          <span className="text-muted-foreground/50">•</span>
          <span className={cn(
            recommendation.isOverdue ? 'text-rose-400' : 
            recommendation.daysUntilDue === 0 ? 'text-amber-300' : 'text-muted-foreground'
          )}>
            {formatDate(recommendation.suggestedDate)}
          </span>
        </div>
      </div>

      {/* Suggested assignee */}
      {recommendation.suggestedAssignees.length > 0 && (
        <div 
          className="shrink-0"
          title={`Suggested: ${recommendation.suggestedAssignees[0].firstName}`}
        >
          <div className="w-8 h-8 rounded-full bg-gradient-to-br from-violet-500 to-fuchsia-500 flex items-center justify-center ring-2 ring-border">
            <span className="text-xs font-bold text-foreground">
              {recommendation.suggestedAssignees[0].firstName.charAt(0)}
              {recommendation.suggestedAssignees[0].lastName.charAt(0)}
            </span>
          </div>
        </div>
      )}
    </div>
  );
}

export function TaskRecommendationsWidget() {
  const { summary, isLoading } = useTaskRecommendations({ daysAhead: 2 });

  if (isLoading || !summary) {
    return (
      <div className="flex items-center justify-center py-8">
        <div className="w-6 h-6 border-2 border-amber-400 border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  const { recommendations, overdueCount } = summary;
  const displayRecs = recommendations.slice(0, 3);
  const remainingCount = recommendations.length - 3;

  if (recommendations.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-8 text-center">
        <div className="p-3 rounded-full bg-emerald-500/15 mb-3">
          <Sparkles className="w-6 h-6 text-emerald-400" />
        </div>
        <p className="text-sm text-muted-foreground">All blueprint tasks assigned!</p>
      </div>
    );
  }

  return (
    <div className="flex flex-col">
      {/* Overdue banner */}
      {overdueCount > 0 && (
        <div className="flex items-center gap-2 px-3 py-2 mb-3 rounded-lg bg-gradient-to-r from-rose-500/15 to-transparent border border-rose-500/20">
          <div className="p-1.5 rounded bg-rose-500/20">
            <AlertCircle className="w-4 h-4 text-rose-400" />
          </div>
          <span className="text-sm text-rose-300 font-medium">
            {overdueCount} overdue from blueprint
          </span>
        </div>
      )}

      <div className="space-y-3">
        {displayRecs.map((rec) => (
          <RecommendationItem key={rec.id} recommendation={rec} />
        ))}
      </div>

      {remainingCount > 0 && (
        <div className="mt-4 pt-4 border-t border-border/30 flex justify-end">
          <button className="text-sm font-medium text-amber-400 hover:text-amber-300 flex items-center gap-1.5 transition-colors group px-3 py-1.5 rounded-lg hover:bg-amber-500/10">
            +{remainingCount} more <ChevronRight className="w-4 h-4 group-hover:translate-x-0.5 transition-transform" />
          </button>
        </div>
      )}
    </div>
  );
}
