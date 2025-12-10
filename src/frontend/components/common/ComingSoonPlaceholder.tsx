'use client';

import React, { useState } from 'react';
import { Rocket, Bell, CheckCircle, Sparkles } from 'lucide-react';
import { cn } from '@/lib/utils';

interface ComingSoonPlaceholderProps {
  /** Title for the upcoming feature */
  title: string;
  /** Description of what the feature will do */
  description: string;
  /** Icon to display (defaults to Rocket) */
  icon?: React.ComponentType<{ className?: string }>;
  /** Expected release timeframe (e.g., "Q1 2025") */
  releaseTimeframe?: string;
  /** List of feature highlights */
  highlights?: string[];
  /** Accent color variant */
  variant?: 'cyan' | 'violet' | 'emerald' | 'amber';
}

const VARIANT_STYLES = {
  cyan: {
    iconBg: 'bg-cyan-500/10',
    iconText: 'text-cyan-400',
    buttonBg: 'bg-cyan-600 hover:bg-cyan-500',
    badgeBg: 'bg-cyan-500/10',
    badgeText: 'text-cyan-400',
    badgeBorder: 'border-cyan-500/20',
    glow: 'from-cyan-500/20 to-transparent',
  },
  violet: {
    iconBg: 'bg-violet-500/10',
    iconText: 'text-violet-400',
    buttonBg: 'bg-violet-600 hover:bg-violet-500',
    badgeBg: 'bg-violet-500/10',
    badgeText: 'text-violet-400',
    badgeBorder: 'border-violet-500/20',
    glow: 'from-violet-500/20 to-transparent',
  },
  emerald: {
    iconBg: 'bg-emerald-500/10',
    iconText: 'text-emerald-400',
    buttonBg: 'bg-emerald-600 hover:bg-emerald-500',
    badgeBg: 'bg-emerald-500/10',
    badgeText: 'text-emerald-400',
    badgeBorder: 'border-emerald-500/20',
    glow: 'from-emerald-500/20 to-transparent',
  },
  amber: {
    iconBg: 'bg-amber-500/10',
    iconText: 'text-amber-400',
    buttonBg: 'bg-amber-600 hover:bg-amber-500',
    badgeBg: 'bg-amber-500/10',
    badgeText: 'text-amber-400',
    badgeBorder: 'border-amber-500/20',
    glow: 'from-amber-500/20 to-transparent',
  },
};

/**
 * ComingSoonPlaceholder
 * 
 * A polished placeholder for features that are not yet implemented.
 * Includes a "Notify Me" CTA to collect interest.
 */
export function ComingSoonPlaceholder({
  title,
  description,
  icon: Icon = Rocket,
  releaseTimeframe,
  highlights = [],
  variant = 'cyan',
}: ComingSoonPlaceholderProps) {
  const [isSubscribed, setIsSubscribed] = useState(false);
  const [email, setEmail] = useState('');
  const styles = VARIANT_STYLES[variant];

  const handleNotifyMe = () => {
    // In production, this would submit to an API
    setIsSubscribed(true);
    setTimeout(() => {
      // Reset after showing success for demo
    }, 5000);
  };

  return (
    <div className="flex items-center justify-center min-h-[60vh] p-6">
      <div className="max-w-lg w-full">
        {/* Glow effect */}
        <div className={cn(
          "absolute inset-0 -z-10 bg-gradient-radial opacity-30 blur-3xl",
          styles.glow
        )} />

        {/* Card */}
        <div className="bg-surface/50 border border-border/50 rounded-2xl p-8 backdrop-blur-sm text-center relative overflow-hidden">
          {/* Badge */}
          <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full border mb-6"
            style={{ 
              backgroundColor: `var(--${variant}-500-10, rgba(6, 182, 212, 0.1))` 
            }}
          >
            <Sparkles className={cn("w-3.5 h-3.5", styles.iconText)} />
            <span className={cn("text-xs font-semibold uppercase tracking-wider", styles.iconText)}>
              Coming Soon
            </span>
            {releaseTimeframe && (
              <>
                <span className="text-muted-foreground/50">•</span>
                <span className="text-xs text-muted-foreground">{releaseTimeframe}</span>
              </>
            )}
          </div>

          {/* Icon */}
          <div className={cn(
            "w-20 h-20 rounded-2xl mx-auto mb-6 flex items-center justify-center",
            styles.iconBg
          )}>
            <Icon className={cn("w-10 h-10", styles.iconText)} />
          </div>

          {/* Content */}
          <h2 className="text-2xl font-bold text-foreground mb-3">
            {title}
          </h2>
          <p className="text-muted-foreground mb-6 leading-relaxed">
            {description}
          </p>

          {/* Highlights */}
          {highlights.length > 0 && (
            <div className="space-y-2 mb-8">
              {highlights.map((highlight, index) => (
                <div 
                  key={index}
                  className="flex items-center gap-2 text-sm text-muted-foreground justify-center"
                >
                  <CheckCircle className={cn("w-4 h-4", styles.iconText)} />
                  <span>{highlight}</span>
                </div>
              ))}
            </div>
          )}

          {/* CTA */}
          {!isSubscribed ? (
            <div className="flex flex-col sm:flex-row gap-3 justify-center">
              <input
                type="email"
                placeholder="Enter your email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="px-4 py-2.5 bg-background border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-cyan-500/50 transition-colors"
              />
              <button
                onClick={handleNotifyMe}
                disabled={!email}
                className={cn(
                  "px-5 py-2.5 rounded-lg font-medium text-sm text-white transition-all flex items-center justify-center gap-2",
                  styles.buttonBg,
                  !email && "opacity-50 cursor-not-allowed"
                )}
              >
                <Bell className="w-4 h-4" />
                Notify Me
              </button>
            </div>
          ) : (
            <div className="flex items-center justify-center gap-2 text-emerald-400">
              <CheckCircle className="w-5 h-5" />
              <span className="font-medium">You're on the list!</span>
            </div>
          )}

          {/* Bottom decoration */}
          <div className="absolute bottom-0 left-0 right-0 h-1 bg-gradient-to-r from-transparent via-cyan-500/20 to-transparent" />
        </div>
      </div>
    </div>
  );
}




