'use client';

import React from 'react';
import {
  Leaf,
  Flower2,
  Droplets,
  Thermometer,
  Wind,
  Zap,
  ArrowRight,
  ArrowUp,
  ArrowDown,
  X,
  Target,
  Clock,
  Info,
} from 'lucide-react';
import { cn } from '@/lib/utils';

// Types matching backend SteeringSuggestion
export type SteeringSuggestionType = 
  | 'lever-adjustment' 
  | 'irrigation-adjustment' 
  | 'phase-transition' 
  | 'profile-recommendation';

export type SteeringSuggestionPriority = 'low' | 'medium' | 'high' | 'critical';

export type SteeringMode = 'vegetative' | 'generative' | 'balanced';

export type DailyPhase = 'night' | 'p1-ramp' | 'p2-maintenance' | 'p3-dryback';

export interface SteeringSuggestion {
  id: string;
  type: SteeringSuggestionType;
  metricName: string;
  title: string;
  description: string;
  currentValue: string;
  targetRange: string;
  suggestedAction: string;
  priority: SteeringSuggestionPriority;
  impactScore: number;
  phase: DailyPhase;
  relatedStreamType?: number;
}

export interface SteeringProfileSummary {
  profileId: string;
  profileName: string;
  targetMode: SteeringMode;
  isSiteDefault: boolean;
  strainName?: string;
}

interface SteeringRecommendationCardProps {
  suggestions: SteeringSuggestion[];
  currentPhase: DailyPhase;
  profile?: SteeringProfileSummary;
  onDismiss?: (id: string) => void;
  onApply?: (suggestion: SteeringSuggestion) => void;
  onViewProfile?: () => void;
  className?: string;
}

interface SingleRecommendationProps {
  suggestion: SteeringSuggestion;
  onDismiss?: () => void;
  onApply?: () => void;
}

// Icon mapping for metrics
const METRIC_ICONS: Record<string, typeof Thermometer> = {
  'SubstrateEC': Zap,
  'VWC': Droplets,
  'VPD': Wind,
  'Temperature': Thermometer,
  'DailyDryback': Droplets,
  'IntershotDryback': Droplets,
};

const PRIORITY_STYLES: Record<SteeringSuggestionPriority, { bg: string; border: string; icon: string }> = {
  low: {
    bg: 'bg-muted',
    border: 'border-border',
    icon: 'bg-muted text-muted-foreground',
  },
  medium: {
    bg: 'bg-amber-500/10',
    border: 'border-amber-500/20',
    icon: 'bg-amber-500/20 text-amber-400',
  },
  high: {
    bg: 'bg-cyan-500/10',
    border: 'border-cyan-500/20',
    icon: 'bg-cyan-500/20 text-cyan-400',
  },
  critical: {
    bg: 'bg-red-500/10',
    border: 'border-red-500/20',
    icon: 'bg-red-500/20 text-red-400',
  },
};

const MODE_STYLES: Record<SteeringMode, { bg: string; text: string; icon: typeof Leaf }> = {
  vegetative: {
    bg: 'bg-emerald-500/20',
    text: 'text-emerald-400',
    icon: Leaf,
  },
  generative: {
    bg: 'bg-purple-500/20',
    text: 'text-purple-400',
    icon: Flower2,
  },
  balanced: {
    bg: 'bg-blue-500/20',
    text: 'text-blue-400',
    icon: Target,
  },
};

const PHASE_LABELS: Record<DailyPhase, { label: string; description: string }> = {
  'night': { label: 'Night', description: 'Lights off period' },
  'p1-ramp': { label: 'P1 Ramp', description: 'Saturating substrate' },
  'p2-maintenance': { label: 'P2 Maintenance', description: 'Maintaining VWC' },
  'p3-dryback': { label: 'P3 Dryback', description: 'Controlled drying' },
};

function getTrendIcon(action: string) {
  const lowerAction = action.toLowerCase();
  if (lowerAction.includes('higher') || lowerAction.includes('increase') || lowerAction.includes('more')) {
    return <ArrowUp className="w-3 h-3 text-emerald-400" />;
  }
  if (lowerAction.includes('lower') || lowerAction.includes('decrease') || lowerAction.includes('less') || lowerAction.includes('reduce')) {
    return <ArrowDown className="w-3 h-3 text-amber-400" />;
  }
  return <ArrowRight className="w-3 h-3 text-muted-foreground" />;
}

function SingleRecommendation({ suggestion, onDismiss, onApply }: SingleRecommendationProps) {
  const Icon = METRIC_ICONS[suggestion.metricName] || Target;
  const styles = PRIORITY_STYLES[suggestion.priority];

  return (
    <div className={cn(
      'p-4 rounded-lg border transition-all hover:bg-white/5',
      styles.bg,
      styles.border
    )}>
      <div className="flex items-start gap-3">
        <div className={cn(
          'w-8 h-8 rounded-lg flex items-center justify-center shrink-0',
          styles.icon
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
              Current: <span className="text-foreground font-medium">{suggestion.currentValue}</span>
            </span>
            {getTrendIcon(suggestion.suggestedAction)}
            <span className="text-muted-foreground">
              Target: <span className="text-cyan-400 font-medium">{suggestion.targetRange}</span>
            </span>
          </div>
          
          <div className="flex items-center gap-4 mt-3">
            <span className="text-xs px-2 py-0.5 rounded-full bg-muted text-muted-foreground flex items-center gap-1">
              <Clock className="w-3 h-3" />
              {PHASE_LABELS[suggestion.phase].label}
            </span>
            {suggestion.impactScore > 50 && (
              <span className="text-xs text-amber-400">
                High impact adjustment
              </span>
            )}
          </div>
          
          {onApply && (
            <button
              onClick={onApply}
              className="mt-3 px-3 py-1.5 text-xs font-medium rounded-lg bg-cyan-600 hover:bg-cyan-500 text-white transition-colors"
            >
              View Details
            </button>
          )}
        </div>
      </div>
    </div>
  );
}

export function SteeringRecommendationCard({
  suggestions,
  currentPhase,
  profile,
  onDismiss,
  onApply,
  onViewProfile,
  className,
}: SteeringRecommendationCardProps) {
  const modeStyles = profile ? MODE_STYLES[profile.targetMode] : MODE_STYLES.balanced;
  const ModeIcon = modeStyles.icon;
  const phaseInfo = PHASE_LABELS[currentPhase];

  return (
    <div className={cn(
      'rounded-xl border border-cyan-500/20 bg-card overflow-hidden',
      className
    )}>
      {/* Header */}
      <div className="px-4 py-3 border-b border-border bg-muted/50">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <Target className="w-4 h-4 text-cyan-400" />
            <span className="text-sm font-bold text-foreground">
              Crop Steering Recommendations
            </span>
            {suggestions.length > 0 && (
              <span className="text-xs px-2 py-0.5 rounded-full bg-cyan-500/20 text-cyan-400">
                {suggestions.length} suggestion{suggestions.length !== 1 ? 's' : ''}
              </span>
            )}
          </div>
          
          {/* Current Phase Badge */}
          <div className="flex items-center gap-2">
            <span className="text-xs px-2 py-1 rounded-lg bg-muted text-muted-foreground">
              {phaseInfo.label}
            </span>
          </div>
        </div>
        
        {/* Profile Info */}
        {profile && (
          <div className="flex items-center gap-2 mt-2">
            <div className={cn(
              'flex items-center gap-1.5 px-2 py-1 rounded-lg text-xs',
              modeStyles.bg,
              modeStyles.text
            )}>
              <ModeIcon className="w-3 h-3" />
              <span className="capitalize">{profile.targetMode}</span>
            </div>
            <span className="text-xs text-muted-foreground">
              Profile: {profile.profileName}
              {profile.strainName && ` (${profile.strainName})`}
              {profile.isSiteDefault && ' • Site Default'}
            </span>
            {onViewProfile && (
              <button
                onClick={onViewProfile}
                className="text-xs text-cyan-400 hover:text-cyan-300 transition-colors ml-auto"
              >
                Edit Profile
              </button>
            )}
          </div>
        )}
      </div>
      
      {/* Suggestions */}
      {suggestions.length > 0 ? (
        <div className="p-4 space-y-3">
          {suggestions.map((suggestion) => (
            <SingleRecommendation
              key={suggestion.id}
              suggestion={suggestion}
              onDismiss={onDismiss ? () => onDismiss(suggestion.id) : undefined}
              onApply={onApply ? () => onApply(suggestion) : undefined}
            />
          ))}
        </div>
      ) : (
        <div className="p-6 text-center">
          <div className="w-12 h-12 rounded-full bg-emerald-500/20 flex items-center justify-center mx-auto mb-3">
            <Target className="w-6 h-6 text-emerald-400" />
          </div>
          <p className="text-sm font-medium text-foreground">
            All metrics within target range
          </p>
          <p className="text-xs text-muted-foreground mt-1">
            Current conditions align with your {profile?.targetMode || 'selected'} steering profile
          </p>
        </div>
      )}
    </div>
  );
}

/**
 * Compact phase indicator for inline display
 */
interface PhaseIndicatorProps {
  currentPhase: DailyPhase;
  targetMode?: SteeringMode;
  className?: string;
}

export function PhaseIndicator({ currentPhase, targetMode, className }: PhaseIndicatorProps) {
  const phaseInfo = PHASE_LABELS[currentPhase];
  const modeStyles = targetMode ? MODE_STYLES[targetMode] : null;
  const ModeIcon = modeStyles?.icon || Target;

  return (
    <div className={cn(
      'flex items-center gap-2 px-3 py-2 rounded-lg bg-muted/50 border border-border',
      className
    )}>
      <Clock className="w-4 h-4 text-muted-foreground" />
      <div className="flex flex-col">
        <span className="text-xs font-medium text-foreground">{phaseInfo.label}</span>
        <span className="text-[10px] text-muted-foreground">{phaseInfo.description}</span>
      </div>
      {targetMode && (
        <>
          <div className="w-px h-6 bg-border mx-1" />
          <div className={cn(
            'flex items-center gap-1 px-2 py-0.5 rounded text-xs',
            modeStyles?.bg,
            modeStyles?.text
          )}>
            <ModeIcon className="w-3 h-3" />
            <span className="capitalize">{targetMode}</span>
          </div>
        </>
      )}
    </div>
  );
}

/**
 * Compact steering mode selector
 */
interface SteeringModeSelectorProps {
  value: SteeringMode;
  onChange: (mode: SteeringMode) => void;
  disabled?: boolean;
  className?: string;
}

export function SteeringModeSelector({ 
  value, 
  onChange, 
  disabled,
  className 
}: SteeringModeSelectorProps) {
  const modes: SteeringMode[] = ['vegetative', 'generative', 'balanced'];

  return (
    <div className={cn('flex rounded-lg border border-border overflow-hidden', className)}>
      {modes.map((mode) => {
        const styles = MODE_STYLES[mode];
        const Icon = styles.icon;
        const isSelected = value === mode;
        
        return (
          <button
            key={mode}
            onClick={() => onChange(mode)}
            disabled={disabled}
            className={cn(
              'flex items-center gap-1.5 px-3 py-2 text-xs font-medium transition-colors',
              'border-r border-border last:border-r-0',
              isSelected 
                ? cn(styles.bg, styles.text)
                : 'bg-muted/30 text-muted-foreground hover:bg-muted/50',
              disabled && 'opacity-50 cursor-not-allowed'
            )}
          >
            <Icon className="w-3.5 h-3.5" />
            <span className="capitalize">{mode}</span>
          </button>
        );
      })}
    </div>
  );
}

/**
 * Info tooltip for steering education
 */
interface SteeringInfoTooltipProps {
  metric: string;
  className?: string;
}

export function SteeringInfoTooltip({ metric, className }: SteeringInfoTooltipProps) {
  const info: Record<string, { veg: string; gen: string }> = {
    'SubstrateEC': {
      veg: '↓ Lower EC (1.5-2.5 mS/cm) promotes leaf/stem growth',
      gen: '↑ Higher EC (2.5-4.0 mS/cm) stresses toward flowering',
    },
    'VWC': {
      veg: '↑ Higher VWC (55-70%) keeps plants hydrated for growth',
      gen: '↓ Lower VWC (40-55%) triggers flowering hormones',
    },
    'VPD': {
      veg: '↓ Lower VPD (0.8-1.0 kPa) reduces stress',
      gen: '↑ Higher VPD (1.2-1.5 kPa) increases transpiration',
    },
    'Temperature': {
      veg: '↑ Higher temps (78-84°F) accelerate growth',
      gen: '↓ Lower temps (72-78°F) promote terpene retention',
    },
  };

  const metricInfo = info[metric];
  if (!metricInfo) return null;

  return (
    <div className={cn(
      'p-3 rounded-lg bg-muted/50 border border-border text-xs',
      className
    )}>
      <div className="flex items-center gap-1.5 mb-2 text-muted-foreground">
        <Info className="w-3 h-3" />
        <span>Steering Guide: {metric}</span>
      </div>
      <div className="space-y-1">
        <div className="flex items-start gap-2">
          <Leaf className="w-3 h-3 text-emerald-400 mt-0.5 shrink-0" />
          <span className="text-muted-foreground">{metricInfo.veg}</span>
        </div>
        <div className="flex items-start gap-2">
          <Flower2 className="w-3 h-3 text-purple-400 mt-0.5 shrink-0" />
          <span className="text-muted-foreground">{metricInfo.gen}</span>
        </div>
      </div>
    </div>
  );
}
