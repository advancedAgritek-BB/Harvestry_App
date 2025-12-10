'use client';

import { Check, X, ArrowRight } from 'lucide-react';
import { competitorComparisons, competitorStackTotal } from './pricingData';
import { useScrollAnimation } from '../hooks/useScrollAnimation';

interface TCOComparisonProps {
  harvestryPrice: number;
  isAnnual: boolean;
}

export function TCOComparison({ harvestryPrice, isAnnual }: TCOComparisonProps) {
  const { ref, isVisible } = useScrollAnimation({ threshold: 0.2 });
  
  const savings = competitorStackTotal.high - harvestryPrice;
  const savingsPercent = Math.round((savings / competitorStackTotal.high) * 100);

  return (
    <div 
      ref={ref}
      className={`mt-20 transition-all duration-700 ${isVisible ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'}`}
    >
      {/* Header */}
      <div className="text-center mb-10">
        <h3 className="text-2xl sm:text-3xl font-bold mb-3">
          Replace Your{' '}
          <span className="text-accent-amber">Fragmented Stack</span>
        </h3>
        <p className="text-muted-foreground max-w-2xl mx-auto">
          Most operators juggle 4–5 disconnected tools. Harvestry unifies batch tracking, task management, 
          inventory, telemetry, and compliance in one platform.
        </p>
      </div>

      {/* Comparison Table */}
      <div className="max-w-4xl mx-auto">
        <div className="rounded-2xl border border-border/50 bg-surface/30 backdrop-blur-sm overflow-hidden">
          {/* Table Header */}
          <div className="grid grid-cols-12 gap-4 p-4 bg-surface/50 border-b border-border/50 text-sm font-medium">
            <div className="col-span-5">What You Need</div>
            <div className="col-span-4 text-center">Typical Vendor</div>
            <div className="col-span-3 text-center text-accent-emerald">Harvestry</div>
          </div>

          {/* Table Rows */}
          {competitorComparisons.map((item, index) => (
            <div 
              key={item.category}
              className={`grid grid-cols-12 gap-4 p-4 items-center transition-colors hover:bg-surface/50 ${
                index !== competitorComparisons.length - 1 ? 'border-b border-border/30' : ''
              }`}
            >
              <div className="col-span-5">
                <div className="font-medium text-sm">{item.category}</div>
              </div>
              <div className="col-span-4 text-center">
                <div className="text-sm text-muted-foreground">{item.competitor}</div>
                <div className="text-sm font-medium text-accent-amber">{item.competitorPrice}</div>
              </div>
              <div className="col-span-3 flex justify-center">
                {item.harvestryIncluded ? (
                  <div className="flex items-center gap-1.5">
                    <div className="p-1 rounded-full bg-accent-emerald/20">
                      <Check className="h-3.5 w-3.5 text-accent-emerald" />
                    </div>
                    <span className="text-xs text-muted-foreground">{item.harvestryTier}</span>
                  </div>
                ) : (
                  <div className="p-1 rounded-full bg-muted/20">
                    <X className="h-3.5 w-3.5 text-muted-foreground" />
                  </div>
                )}
              </div>
            </div>
          ))}

          {/* Total Row */}
          <div className="grid grid-cols-12 gap-4 p-4 bg-gradient-to-r from-surface/80 to-accent-emerald/5 border-t border-border/50">
            <div className="col-span-5">
              <div className="font-bold">Typical Monthly Total</div>
              <div className="text-xs text-muted-foreground">Plus integration headaches</div>
            </div>
            <div className="col-span-4 text-center">
              <div className="text-lg font-bold text-accent-amber">
                ${competitorStackTotal.low.toLocaleString()}–${competitorStackTotal.high.toLocaleString()}/mo
              </div>
              <div className="text-xs text-muted-foreground">4–5 separate logins</div>
            </div>
            <div className="col-span-3 text-center">
              <div className="text-lg font-bold text-accent-emerald">
                ${harvestryPrice.toLocaleString()}/mo
              </div>
              <div className="text-xs text-muted-foreground">One platform</div>
            </div>
          </div>
        </div>

        {/* Savings Callout */}
        <div className="mt-6 p-6 rounded-xl bg-gradient-to-r from-accent-emerald/10 via-accent-emerald/5 to-transparent border border-accent-emerald/20">
          <div className="flex flex-col sm:flex-row items-center justify-between gap-4">
            <div>
              <div className="text-2xl font-bold text-accent-emerald">
                Save up to ${savings.toLocaleString()}/mo
              </div>
              <div className="text-sm text-muted-foreground">
                That's <span className="font-semibold text-foreground">{savingsPercent}% less</span> than a typical multi-vendor stack—with zero integration tax.
              </div>
            </div>
            <a 
              href="#demo"
              className="inline-flex items-center gap-2 px-6 py-3 rounded-xl bg-accent-emerald text-white font-semibold hover:bg-accent-emerald/90 transition-colors shadow-lg shadow-accent-emerald/20"
            >
              See It In Action
              <ArrowRight className="h-4 w-4" />
            </a>
          </div>
        </div>

        {/* Fine Print */}
        <p className="mt-4 text-xs text-center text-muted-foreground">
          Competitor pricing based on publicly available information as of Dec 2025. 
          Your actual costs may vary based on facility size and requirements.
        </p>
      </div>
    </div>
  );
}




