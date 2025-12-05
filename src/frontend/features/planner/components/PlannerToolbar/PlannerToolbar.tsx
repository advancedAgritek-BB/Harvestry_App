'use client';

import React, { useState } from 'react';
import { cn } from '@/lib/utils';
import { 
  Calendar,
  ChevronLeft, 
  ChevronRight, 
  Plus, 
  Search,
  SlidersHorizontal,
  Layers,
  AlertTriangle,
  Eye,
  EyeOff,
  RotateCcw,
  FlaskConical,
} from 'lucide-react';
import { usePlannerStore } from '../../stores/plannerStore';
import { ZoomLevel } from '../../types/planner.types';
import { CapacityLegend } from '../CapacityLane';
import { format } from 'date-fns';

interface PlannerToolbarProps {
  onNewBatch?: () => void;
  conflictCount?: number;
}

const ZOOM_OPTIONS: { value: ZoomLevel; label: string }[] = [
  { value: 'day', label: 'Day' },
  { value: 'week', label: 'Week' },
  { value: 'month', label: 'Month' },
];

export function PlannerToolbar({ onNewBatch, conflictCount = 0 }: PlannerToolbarProps) {
  const [showFilters, setShowFilters] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  
  const {
    dateRange,
    settings,
    filters,
    setZoomLevel,
    navigateToToday,
    navigateBy,
    updateSettings,
    setFilters,
    clearFilters,
  } = usePlannerStore();

  const handleZoomChange = (level: ZoomLevel) => {
    setZoomLevel(level);
  };

  const handleNavigateBack = () => {
    const days = settings.zoomLevel === 'day' ? -7 : settings.zoomLevel === 'week' ? -14 : -30;
    navigateBy(days);
  };

  const handleNavigateForward = () => {
    const days = settings.zoomLevel === 'day' ? 7 : settings.zoomLevel === 'week' ? 14 : 30;
    navigateBy(days);
  };

  const handleSearch = (e: React.ChangeEvent<HTMLInputElement>) => {
    const query = e.target.value;
    setSearchQuery(query);
    setFilters({ searchQuery: query });
  };

  return (
    <div className="flex flex-col gap-3 p-4 bg-surface/50 border-b border-border">
      {/* Main Toolbar Row */}
      <div className="flex items-center justify-between gap-4">
        {/* Left Section - Navigation & Date Range */}
        <div className="flex items-center gap-3">
          {/* New Batch Button */}
          <button
            onClick={onNewBatch}
            className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-background bg-cyan-500 hover:bg-cyan-400 rounded-lg transition-colors shadow-lg shadow-cyan-500/20 whitespace-nowrap"
          >
            <Plus className="w-4 h-4" />
            New Batch
          </button>

          {/* Date Navigation */}
          <div className="flex items-center gap-1 bg-muted/50 rounded-lg p-1">
            <button
              onClick={handleNavigateBack}
              className="p-2 text-muted-foreground hover:text-foreground hover:bg-muted rounded-md transition-colors"
              aria-label="Previous"
            >
              <ChevronLeft className="w-4 h-4" />
            </button>
            
            <button
              onClick={navigateToToday}
              className="px-3 py-1.5 text-sm font-medium text-foreground/70 hover:text-foreground hover:bg-muted rounded-md transition-colors"
            >
              Today
            </button>
            
            <button
              onClick={handleNavigateForward}
              className="p-2 text-muted-foreground hover:text-foreground hover:bg-muted rounded-md transition-colors"
              aria-label="Next"
            >
              <ChevronRight className="w-4 h-4" />
            </button>
          </div>

          {/* Current Date Range Display */}
          <div className="flex items-center gap-2 text-sm text-muted-foreground">
            <Calendar className="w-4 h-4" />
            <span>
              {format(dateRange.start, 'MMM d')} - {format(dateRange.end, 'MMM d, yyyy')}
            </span>
          </div>
        </div>

        {/* Center Section - Search */}
        <div className="flex-1 max-w-md">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
            <input
              type="text"
              placeholder="Search batches, strains..."
              value={searchQuery}
              onChange={handleSearch}
              className="w-full pl-10 pr-4 py-2 text-sm bg-muted/50 border border-border rounded-lg text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-cyan-500/50 focus:border-cyan-500"
            />
          </div>
        </div>

        {/* Right Section - Zoom & Settings */}
        <div className="flex items-center gap-3">
          {/* Conflict Warning */}
          {conflictCount > 0 && (
            <div className="flex items-center gap-2 px-3 py-1.5 bg-red-500/10 border border-red-500/30 rounded-lg">
              <AlertTriangle className="w-4 h-4 text-red-400" />
              <span className="text-sm text-red-400">
                {conflictCount} {conflictCount === 1 ? 'conflict' : 'conflicts'}
              </span>
            </div>
          )}

          {/* Zoom Controls */}
          <div className="flex items-center bg-muted/50 rounded-lg p-1">
            {ZOOM_OPTIONS.map((option) => (
              <button
                key={option.value}
                onClick={() => handleZoomChange(option.value)}
                className={cn(
                  'px-3 py-1.5 text-sm font-medium rounded-md transition-colors',
                  settings.zoomLevel === option.value
                    ? 'bg-cyan-500 text-foreground'
                    : 'text-muted-foreground hover:text-foreground hover:bg-muted'
                )}
              >
                {option.label}
              </button>
            ))}
          </div>

          {/* View Settings */}
          <div className="flex items-center gap-1">
            <button
              onClick={() => updateSettings({ showCapacityLanes: !settings.showCapacityLanes })}
              className={cn(
                'p-2 rounded-lg transition-colors',
                settings.showCapacityLanes
                  ? 'text-cyan-400 bg-cyan-500/10'
                  : 'text-muted-foreground hover:text-foreground hover:bg-muted'
              )}
              title="Toggle capacity lanes"
            >
              <Layers className="w-4 h-4" />
            </button>
            
            <button
              onClick={() => updateSettings({ showConflicts: !settings.showConflicts })}
              className={cn(
                'p-2 rounded-lg transition-colors',
                settings.showConflicts
                  ? 'text-cyan-400 bg-cyan-500/10'
                  : 'text-muted-foreground hover:text-foreground hover:bg-muted'
              )}
              title="Toggle conflict highlighting"
            >
              <AlertTriangle className="w-4 h-4" />
            </button>
            
            <button
              onClick={() => updateSettings({ showActualDates: !settings.showActualDates })}
              className={cn(
                'p-2 rounded-lg transition-colors',
                settings.showActualDates
                  ? 'text-cyan-400 bg-cyan-500/10'
                  : 'text-muted-foreground hover:text-foreground hover:bg-muted'
              )}
              title="Toggle actual dates overlay"
            >
              {settings.showActualDates ? (
                <Eye className="w-4 h-4" />
              ) : (
                <EyeOff className="w-4 h-4" />
              )}
            </button>

            <button
              onClick={() => setShowFilters(!showFilters)}
              className={cn(
                'p-2 rounded-lg transition-colors',
                showFilters
                  ? 'text-cyan-400 bg-cyan-500/10'
                  : 'text-muted-foreground hover:text-foreground hover:bg-muted'
              )}
              title="Show filters"
            >
              <SlidersHorizontal className="w-4 h-4" />
            </button>
          </div>

          {/* What-If Mode */}
          <button
            onClick={() => updateSettings({ whatIfMode: !settings.whatIfMode })}
            className={cn(
              'flex items-center gap-2 px-3 py-2 text-sm font-medium rounded-lg transition-colors',
              settings.whatIfMode
                ? 'bg-violet-500/20 text-violet-400 border border-violet-500/30'
                : 'text-muted-foreground hover:text-foreground bg-muted/50 hover:bg-muted'
            )}
          >
            <FlaskConical className="w-4 h-4" />
            What-If
          </button>
        </div>
      </div>

      {/* Filters Row (Collapsible) */}
      {showFilters && (
        <div className="flex items-center gap-4 pt-3 border-t border-border/50">
          <div className="flex items-center gap-2">
            <span className="text-xs text-muted-foreground uppercase tracking-wider">Filters:</span>
          </div>

          {/* Status Filter */}
          <div className="flex items-center gap-2">
            <span className="text-xs text-muted-foreground">Status:</span>
            <div className="flex gap-1">
              {['planned', 'active', 'completed'].map((status) => (
                <button
                  key={status}
                  onClick={() => {
                    const current = filters.statuses;
                    const newStatuses = current.includes(status as any)
                      ? current.filter((s) => s !== status)
                      : [...current, status as any];
                    setFilters({ statuses: newStatuses });
                  }}
                  className={cn(
                    'px-2 py-1 text-xs rounded-md capitalize transition-colors',
                    filters.statuses.includes(status as any)
                      ? 'bg-cyan-500/20 text-cyan-400'
                      : 'bg-muted text-muted-foreground hover:text-foreground'
                  )}
                >
                  {status}
                </button>
              ))}
            </div>
          </div>

          {/* Spacer */}
          <div className="flex-1" />

          {/* Capacity Legend */}
          <CapacityLegend />

          {/* Clear Filters */}
          {(filters.statuses.length > 0 || filters.searchQuery) && (
            <button
              onClick={clearFilters}
              className="flex items-center gap-1 px-2 py-1 text-xs text-muted-foreground hover:text-foreground transition-colors"
            >
              <RotateCcw className="w-3 h-3" />
              Clear
            </button>
          )}
        </div>
      )}
    </div>
  );
}
