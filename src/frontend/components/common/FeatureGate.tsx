'use client';

import React from 'react';
import { Lock } from 'lucide-react';
import { useAuthStore, TierFeature, TIER_LABELS } from '@/stores/auth/authStore';
import { cn } from '@/lib/utils';

interface FeatureGateProps {
  /** The feature to check access for */
  feature: TierFeature;
  /** Content to render if feature is available */
  children: React.ReactNode;
  /** Optional custom fallback when feature is not available */
  fallback?: React.ReactNode;
  /** If true, hide the content entirely instead of showing fallback */
  hideWhenLocked?: boolean;
}

/**
 * FeatureGate Component
 * 
 * Conditionally renders children based on the current pricing tier.
 * Shows a polished "Upgrade" prompt when the feature is not available.
 */
export function FeatureGate({ 
  feature, 
  children, 
  fallback,
  hideWhenLocked = false 
}: FeatureGateProps) {
  const hasFeature = useAuthStore((state) => state.hasFeature(feature));
  const currentTier = useAuthStore((state) => state.currentTier);

  if (hasFeature) {
    return <>{children}</>;
  }

  if (hideWhenLocked) {
    return null;
  }

  if (fallback) {
    return <>{fallback}</>;
  }

  // Default locked state UI
  return (
    <div className="flex flex-col items-center justify-center p-8 text-center min-h-[200px] bg-surface/30 border border-border/50 rounded-xl">
      <div className="w-12 h-12 rounded-full bg-amber-500/10 flex items-center justify-center mb-4">
        <Lock className="w-6 h-6 text-amber-400" />
      </div>
      <h3 className="text-lg font-semibold text-foreground mb-2">
        Feature Unavailable
      </h3>
      <p className="text-sm text-muted-foreground max-w-md mb-4">
        This feature requires a higher tier. You are currently on the{' '}
        <span className="text-foreground font-medium">{TIER_LABELS[currentTier]}</span> plan.
      </p>
      <button className="px-4 py-2 bg-gradient-to-r from-cyan-500 to-emerald-500 text-white rounded-lg font-medium text-sm hover:opacity-90 transition-opacity">
        Upgrade Plan
      </button>
    </div>
  );
}

interface FeatureGateInlineProps {
  feature: TierFeature;
  children: React.ReactNode;
}

/**
 * Inline version that just hides content without placeholder
 */
export function FeatureGateInline({ feature, children }: FeatureGateInlineProps) {
  const hasFeature = useAuthStore((state) => state.hasFeature(feature));
  
  if (!hasFeature) {
    return null;
  }

  return <>{children}</>;
}

interface FeatureGateBadgeProps {
  feature: TierFeature;
  className?: string;
}

/**
 * Shows a small "PRO" or upgrade badge if feature is locked
 */
export function FeatureGateBadge({ feature, className }: FeatureGateBadgeProps) {
  const hasFeature = useAuthStore((state) => state.hasFeature(feature));
  
  if (hasFeature) {
    return null;
  }

  return (
    <span className={cn(
      "inline-flex items-center gap-1 px-1.5 py-0.5 rounded text-[10px] font-bold uppercase",
      "bg-amber-500/10 text-amber-400 border border-amber-500/20",
      className
    )}>
      <Lock className="w-2.5 h-2.5" />
      PRO
    </span>
  );
}




