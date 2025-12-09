'use client';

import { ArrowRight, Layers, Zap } from 'lucide-react';
import { useScrollAnimation } from '../hooks/useScrollAnimation';

interface CompetitorTool {
  name: string;
  category: string;
  price: string;
}

const competitorTools: CompetitorTool[] = [
  { name: 'Trym', category: 'Batch & Tasks', price: '$400/mo' },
  { name: 'Canix', category: 'Inventory', price: '$350/mo' },
  { name: 'Growlink', category: 'Environmental', price: '$300/mo' },
  { name: 'Flourish', category: 'Compliance', price: '$500/mo' },
  { name: 'Custom', category: 'QBO Sync', price: '$200/mo' },
];

const totalCompetitorCost = 1750; // Conservative estimate
const harvestryPrice = 899;
const savingsPercent = Math.round(((totalCompetitorCost - harvestryPrice) / totalCompetitorCost) * 100);

export function StackComparisonHero() {
  const { ref, isVisible } = useScrollAnimation({ threshold: 0.2 });

  return (
    <div
      ref={ref}
      className={`mb-12 transition-all duration-700 ${
        isVisible ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'
      }`}
    >
      <div className="relative rounded-2xl border border-border/50 bg-gradient-to-br from-surface/80 via-surface/50 to-accent-amber/5 backdrop-blur-sm overflow-hidden">
        {/* Background decoration */}
        <div className="absolute top-0 right-0 w-64 h-64 bg-accent-emerald/10 rounded-full blur-3xl pointer-events-none" />
        <div className="absolute bottom-0 left-0 w-48 h-48 bg-accent-amber/10 rounded-full blur-3xl pointer-events-none" />

        <div className="relative p-6 sm:p-8 lg:p-10">
          {/* Header */}
          <div className="text-center mb-8">
            <h3 className="text-xl sm:text-2xl lg:text-3xl font-bold mb-3">
              Replace Your{' '}
              <span className="text-accent-amber">${totalCompetitorCost.toLocaleString()}+/Month</span>{' '}
              Software Stack
            </h3>
            <p className="text-muted-foreground max-w-2xl mx-auto">
              Most operators juggle 4–5 disconnected tools that don&apos;t talk to each other. 
              Harvestry unifies everything in one platform.
            </p>
          </div>

          {/* Visual Comparison */}
          <div className="grid lg:grid-cols-[1fr_auto_1fr] gap-6 lg:gap-8 items-center max-w-5xl mx-auto">
            {/* Competitor Stack (Left) */}
            <div className="space-y-3">
              <div className="flex items-center gap-2 text-sm font-medium text-muted-foreground mb-4">
                <Layers className="h-4 w-4 text-accent-amber" />
                <span>Typical Multi-Vendor Stack</span>
              </div>
              <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-2 gap-2">
                {competitorTools.map((tool) => (
                  <div
                    key={tool.name}
                    className="px-3 py-2 rounded-lg bg-surface/80 border border-border/50 text-center"
                  >
                    <div className="text-xs text-muted-foreground">{tool.category}</div>
                    <div className="text-sm font-medium">{tool.name}</div>
                    <div className="text-xs text-accent-amber font-medium">{tool.price}</div>
                  </div>
                ))}
              </div>
              <div className="pt-3 border-t border-border/30">
                <div className="flex items-center justify-between text-sm">
                  <span className="text-muted-foreground">Monthly Total:</span>
                  <span className="font-bold text-accent-amber text-lg">
                    ${totalCompetitorCost.toLocaleString()}+/mo
                  </span>
                </div>
                <p className="text-xs text-muted-foreground mt-1">
                  Plus integration headaches & multiple logins
                </p>
              </div>
            </div>

            {/* Arrow / Transition */}
            <div className="flex lg:flex-col items-center justify-center gap-2 py-4 lg:py-0">
              <div className="hidden lg:flex flex-col items-center gap-2">
                <ArrowRight className="h-8 w-8 text-accent-emerald animate-pulse" />
                <span className="text-xs font-medium text-accent-emerald whitespace-nowrap">
                  One Platform
                </span>
              </div>
              <div className="lg:hidden flex items-center gap-2">
                <div className="h-px w-8 bg-border/50" />
                <ArrowRight className="h-6 w-6 text-accent-emerald animate-pulse" />
                <div className="h-px w-8 bg-border/50" />
              </div>
            </div>

            {/* Harvestry (Right) */}
            <div className="p-6 rounded-xl bg-gradient-to-br from-accent-emerald/10 to-accent-emerald/5 border border-accent-emerald/30">
              <div className="flex items-center gap-2 text-sm font-medium text-accent-emerald mb-4">
                <Zap className="h-4 w-4" />
                <span>Harvestry Growth</span>
              </div>

              <div className="space-y-2 mb-4">
                <div className="flex items-center gap-2 text-sm">
                  <span className="w-2 h-2 rounded-full bg-accent-emerald" />
                  <span>Batch Lifecycle & Tasks</span>
                </div>
                <div className="flex items-center gap-2 text-sm">
                  <span className="w-2 h-2 rounded-full bg-accent-emerald" />
                  <span>Inventory & Lot Tracking</span>
                </div>
                <div className="flex items-center gap-2 text-sm">
                  <span className="w-2 h-2 rounded-full bg-accent-emerald" />
                  <span>Environmental Monitoring & Control</span>
                </div>
                <div className="flex items-center gap-2 text-sm">
                  <span className="w-2 h-2 rounded-full bg-accent-emerald" />
                  <span>METRC/BioTrack Compliance</span>
                </div>
                <div className="flex items-center gap-2 text-sm">
                  <span className="w-2 h-2 rounded-full bg-accent-emerald" />
                  <span>QuickBooks Integration</span>
                </div>
              </div>

              <div className="pt-4 border-t border-accent-emerald/20">
                <div className="flex items-center justify-between">
                  <span className="text-sm text-muted-foreground">All-in-one:</span>
                  <span className="font-bold text-accent-emerald text-2xl">
                    ${harvestryPrice}/mo
                  </span>
                </div>
              </div>
            </div>
          </div>

          {/* Savings Highlight */}
          <div className="mt-8 text-center">
            <div className="inline-flex items-center gap-3 px-6 py-3 rounded-full bg-accent-emerald/10 border border-accent-emerald/30">
              <span className="text-2xl sm:text-3xl font-bold text-accent-emerald">
                Save {savingsPercent}%
              </span>
              <span className="text-sm text-muted-foreground">
                vs. typical multi-vendor stack
              </span>
            </div>
            <p className="mt-3 text-xs text-muted-foreground">
              Plus eliminate integration headaches, reduce training time, and get one source of truth.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
