'use client';

import { Plus, Check, Sparkles } from 'lucide-react';
import { addOns } from './pricingData';
import { useScrollAnimation } from '../hooks/useScrollAnimation';

interface AddOnsProps {
  isAnnual: boolean;
}

export function AddOns({ isAnnual }: AddOnsProps) {
  const { ref, isVisible } = useScrollAnimation({ threshold: 0.2 });

  return (
    <div 
      ref={ref}
      className={`mt-16 transition-all duration-700 ${isVisible ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'}`}
    >
      {/* Header */}
      <div className="text-center mb-8">
        <h3 className="text-xl font-bold mb-2">
          Enhance Foundation with Add-Ons
        </h3>
        <p className="text-sm text-muted-foreground max-w-xl mx-auto">
          Need compliance or financials on the Foundation tier? Add them à la carte.
          <br />
          <span className="text-accent-emerald font-medium">Growth & Enterprise include Compliance and Financials at no extra cost.</span>
        </p>
      </div>

      {/* Add-ons Grid */}
      <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4 max-w-4xl mx-auto">
        {addOns.map((addon) => {
          const price = isAnnual ? addon.annualPrice : addon.monthlyPrice;
          const savings = addon.monthlyPrice - addon.annualPrice;
          const isIncludedInHigherTiers = addon.includedInTiers && addon.includedInTiers.length > 0;

          return (
            <div
              key={addon.id}
              className="group relative p-5 rounded-xl border border-border/50 bg-surface/30 backdrop-blur-sm hover:border-accent-emerald/30 transition-all duration-300"
            >
              {/* Plus icon indicator */}
              <div className="absolute -top-2.5 -left-2.5 p-1.5 rounded-full bg-accent-emerald/10 border border-accent-emerald/30">
                <Plus className="h-3 w-3 text-accent-emerald" />
              </div>

              {/* Included in higher tiers badge */}
              {isIncludedInHigherTiers && (
                <div className="absolute -top-2.5 -right-2.5">
                  <div className="flex items-center gap-1 px-2 py-1 rounded-full bg-accent-emerald/10 border border-accent-emerald/30 text-xs">
                    <Sparkles className="h-3 w-3 text-accent-emerald" />
                    <span className="text-accent-emerald font-medium">Free in Growth+</span>
                  </div>
                </div>
              )}

              <div className="mb-3 mt-2">
                <h4 className="font-semibold text-sm group-hover:text-accent-emerald transition-colors">
                  {addon.name}
                </h4>
                <p className="text-xs text-muted-foreground mt-0.5">{addon.description}</p>
              </div>

              <div className="flex items-baseline gap-1 mb-4">
                <span className="text-2xl font-bold">+${price}</span>
                <span className="text-sm text-muted-foreground">/mo</span>
                {isAnnual && savings > 0 && (
                  <span className="ml-2 text-xs px-1.5 py-0.5 rounded bg-accent-emerald/10 text-accent-emerald">
                    Save ${savings}/mo
                  </span>
                )}
              </div>

              <ul className="space-y-1.5">
                {addon.includes.map((item, index) => (
                  <li key={index} className="flex items-start gap-2 text-xs text-muted-foreground">
                    <Check className="h-3 w-3 text-accent-emerald flex-shrink-0 mt-0.5" />
                    <span>{item}</span>
                  </li>
                ))}
              </ul>

              {/* For Foundation tier note */}
              <div className="mt-4 pt-3 border-t border-border/30">
                <p className="text-xs text-muted-foreground">
                  {isIncludedInHigherTiers 
                    ? 'Add to Foundation tier only'
                    : 'Available for any tier'
                  }
                </p>
              </div>
            </div>
          );
        })}
      </div>

      {/* Note */}
      <p className="text-center text-xs text-muted-foreground mt-6">
        <span className="inline-flex items-center gap-1">
          <Sparkles className="h-3 w-3 text-accent-emerald" />
          <span className="text-accent-emerald font-medium">Growth tier</span> includes Compliance ($200 value) + Financials ($100 value) at no additional cost.
        </span>
      </p>
    </div>
  );
}
