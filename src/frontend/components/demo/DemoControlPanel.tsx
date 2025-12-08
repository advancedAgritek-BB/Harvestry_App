'use client';

import React from 'react';
import { Settings2, ChevronDown, Check } from 'lucide-react';
import { useAuthStore, PricingTier, TIER_LABELS, TIER_FEATURES } from '@/stores/auth/authStore';
import { cn } from '@/lib/utils';

const TIERS: PricingTier[] = ['monitor', 'foundation', 'growth', 'enterprise'];

/**
 * DemoControlPanel
 * 
 * Admin-only floating panel for switching pricing tiers during demos.
 * Shows current tier and allows instant switching to demonstrate feature gating.
 */
export function DemoControlPanel() {
  const [isOpen, setIsOpen] = React.useState(false);
  const currentTier = useAuthStore((state) => state.currentTier);
  const setCurrentTier = useAuthStore((state) => state.setCurrentTier);
  const isSuperAdmin = useAuthStore((state) => state.isSuperAdmin());

  // Only show for SuperAdmin
  if (!isSuperAdmin) {
    return null;
  }

  return (
    <div className="fixed bottom-6 left-6 z-50">
      {/* Toggle Button */}
      <button
        onClick={() => setIsOpen(!isOpen)}
        className={cn(
          "flex items-center gap-2 px-3 py-2 rounded-lg shadow-lg transition-all",
          "bg-surface border border-border hover:border-cyan-500/50",
          isOpen && "border-cyan-500/50"
        )}
      >
        <Settings2 className="w-4 h-4 text-cyan-400" />
        <span className="text-sm font-medium text-foreground">Demo Mode</span>
        <span className="px-1.5 py-0.5 rounded text-[10px] font-bold uppercase bg-cyan-500/10 text-cyan-400">
          {currentTier}
        </span>
        <ChevronDown className={cn(
          "w-4 h-4 text-muted-foreground transition-transform",
          isOpen && "rotate-180"
        )} />
      </button>

      {/* Dropdown Panel */}
      {isOpen && (
        <div className="absolute bottom-full left-0 mb-2 w-64 bg-surface border border-border rounded-xl shadow-xl overflow-hidden">
          <div className="p-3 border-b border-border">
            <h4 className="text-sm font-semibold text-foreground">Pricing Tier</h4>
            <p className="text-xs text-muted-foreground">
              Switch tiers to demo feature gating
            </p>
          </div>
          
          <div className="p-2">
            {TIERS.map((tier) => (
              <button
                key={tier}
                onClick={() => {
                  setCurrentTier(tier);
                  setIsOpen(false);
                }}
                className={cn(
                  "w-full flex items-center justify-between px-3 py-2 rounded-lg text-left transition-colors",
                  currentTier === tier 
                    ? "bg-cyan-500/10 text-cyan-400" 
                    : "hover:bg-muted text-foreground"
                )}
              >
                <div>
                  <div className="text-sm font-medium">{TIER_LABELS[tier]}</div>
                  <div className="text-[10px] text-muted-foreground">
                    {TIER_FEATURES[tier].length} features
                  </div>
                </div>
                {currentTier === tier && (
                  <Check className="w-4 h-4" />
                )}
              </button>
            ))}
          </div>

          <div className="p-2 border-t border-border bg-muted/30">
            <p className="text-[10px] text-muted-foreground text-center">
              This panel is only visible to admins
            </p>
          </div>
        </div>
      )}
    </div>
  );
}
