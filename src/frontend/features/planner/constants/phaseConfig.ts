/**
 * Phase Configuration
 * 
 * Visual and default settings for each lifecycle phase
 */

import { PhaseConfig, PhaseType } from '../types/planner.types';

export const PHASE_CONFIGS: Record<PhaseType, PhaseConfig> = {
  clone: {
    phase: 'clone',
    label: 'Clone',
    color: '#22d3ee', // cyan-400
    gradientFrom: '#06b6d4', // cyan-500
    gradientTo: '#0891b2', // cyan-600
    icon: 'Sprout',
    defaultDays: 14,
  },
  veg: {
    phase: 'veg',
    label: 'Vegetative',
    color: '#34d399', // emerald-400
    gradientFrom: '#10b981', // emerald-500
    gradientTo: '#059669', // emerald-600
    icon: 'Leaf',
    defaultDays: 21,
  },
  flower: {
    phase: 'flower',
    label: 'Flower',
    color: '#fb7185', // rose-400
    gradientFrom: '#f43f5e', // rose-500
    gradientTo: '#e11d48', // rose-600
    icon: 'Flower2',
    defaultDays: 56,
  },
  harvest: {
    phase: 'harvest',
    label: 'Harvest',
    color: '#fbbf24', // amber-400
    gradientFrom: '#f59e0b', // amber-500
    gradientTo: '#d97706', // amber-600
    icon: 'Scissors',
    defaultDays: 3,
  },
  cure: {
    phase: 'cure',
    label: 'Cure',
    color: '#a78bfa', // violet-400
    gradientFrom: '#8b5cf6', // violet-500
    gradientTo: '#7c3aed', // violet-600
    icon: 'Package',
    defaultDays: 14,
  },
};

// Phase order for lifecycle progression
export const PHASE_ORDER: PhaseType[] = ['clone', 'veg', 'flower', 'harvest', 'cure'];

// Get next phase in lifecycle
export function getNextPhase(currentPhase: PhaseType): PhaseType | null {
  const currentIndex = PHASE_ORDER.indexOf(currentPhase);
  if (currentIndex === -1 || currentIndex === PHASE_ORDER.length - 1) {
    return null;
  }
  return PHASE_ORDER[currentIndex + 1];
}

// Get previous phase in lifecycle
export function getPreviousPhase(currentPhase: PhaseType): PhaseType | null {
  const currentIndex = PHASE_ORDER.indexOf(currentPhase);
  if (currentIndex <= 0) {
    return null;
  }
  return PHASE_ORDER[currentIndex - 1];
}

// Capacity thresholds for color coding
export const CAPACITY_THRESHOLDS = {
  low: 0.7,    // Under 70% - green
  medium: 0.9, // 70-90% - yellow
  high: 1.0,   // 90%+ - red
};

// Timeline constants
export const TIMELINE_CONFIG = {
  dayWidth: {
    day: 60,   // px per day in day view - detailed view
    week: 20,  // px per day in week view - standard view
    month: 6,  // px per day in month view - overview
  },
  rowHeight: 56,
  capacityLaneHeight: 24,
  headerHeight: 64,
  minBatchWidth: 20,
};

