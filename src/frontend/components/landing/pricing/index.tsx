'use client';

import { useState } from 'react';
import { Sparkles, ArrowRight } from 'lucide-react';
import { pricingTiers, capacityTiers } from './pricingData';
import { PricingCard } from './PricingCard';
import { TCOComparison } from './TCOComparison';
import { AddOns } from './AddOns';
import { useScrollAnimation } from '../hooks/useScrollAnimation';

export function Pricing() {
  const [isAnnual, setIsAnnual] = useState(true);
  const { ref: headerRef, isVisible: headerVisible } = useScrollAnimation({ threshold: 0.2 });
  const { ref: cardsRef, isVisible: cardsVisible } = useScrollAnimation({ threshold: 0.1 });
  const { ref: capacityRef, isVisible: capacityVisible } = useScrollAnimation({ threshold: 0.2 });

  // Get Growth tier price for TCO comparison (most common commercial tier)
  const growthTier = pricingTiers.find((t) => t.id === 'growth');
  const comparisonPrice = growthTier
    ? isAnnual
      ? growthTier.annualPrice ?? 899
      : growthTier.monthlyPrice ?? 899
    : 899;

  return (
    <section id="pricing" className="py-28 relative overflow-hidden">
      {/* Background */}
      <div className="absolute inset-0 bg-surface/20" />
      <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[800px] h-[800px] bg-accent-emerald/5 rounded-full blur-3xl" />

      <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div
          ref={headerRef}
          className={`text-center mb-12 transition-all duration-700 ${
            headerVisible ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'
          }`}
        >
          <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-foreground/5 border border-foreground/10 text-muted-foreground text-sm font-medium mb-6">
            <Sparkles className="h-4 w-4 text-accent-emerald" />
            Transparent Pricing
          </div>
          <h2 className="text-3xl sm:text-4xl lg:text-5xl font-bold mb-5">
            One Platform.{' '}
            <span className="text-accent-emerald">
              Replace Your Entire Stack.
            </span>
          </h2>
          <p className="text-lg text-muted-foreground max-w-2xl mx-auto mb-8">
            Stop paying for 4–5 disconnected tools. Harvestry unifies batch tracking, inventory, compliance, 
            environmental monitoring, and financials—at a fraction of the cost.
          </p>

          {/* Billing Toggle */}
          <div className="inline-flex items-center justify-center gap-3 bg-surface/50 border border-border/50 rounded-full px-4 py-2">
            <span
              className={`text-sm font-medium transition-colors ${
                !isAnnual ? 'text-foreground' : 'text-muted-foreground'
              }`}
            >
              Monthly
            </span>
            <button
              onClick={() => setIsAnnual(!isAnnual)}
              className={`relative w-12 h-6 rounded-full transition-colors duration-300 flex-shrink-0 ${
                isAnnual ? 'bg-accent-emerald' : 'bg-muted'
              }`}
              aria-label="Toggle annual billing"
            >
              <span
                className={`absolute top-1 left-1 w-4 h-4 rounded-full bg-white shadow-md transition-transform duration-300 ${
                  isAnnual ? 'translate-x-6' : 'translate-x-0'
                }`}
              />
            </button>
            <span
              className={`text-sm font-medium transition-colors ${
                isAnnual ? 'text-foreground' : 'text-muted-foreground'
              }`}
            >
              Annual
            </span>
            {isAnnual && (
              <span className="px-2 py-0.5 rounded-full bg-accent-emerald/10 text-accent-emerald text-xs font-semibold whitespace-nowrap">
                Save 10%
              </span>
            )}
          </div>
        </div>


        {/* Pricing Grid - 4 tiers (responsive: 1 → 2 → 4 columns) */}
        <div
          ref={cardsRef}
          className={`grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-6 transition-all duration-700 ${
            cardsVisible ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'
          }`}
        >
          {pricingTiers.map((tier, index) => (
            <PricingCard key={tier.id} tier={tier} isAnnual={isAnnual} index={index} />
          ))}
        </div>

        {/* Hardware Independence Callout */}
        <div className={`mt-12 p-6 rounded-2xl bg-gradient-to-r from-accent-cyan/5 via-surface/50 to-accent-emerald/5 border border-border/50 transition-all duration-700 ${
          cardsVisible ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'
        }`}>
          <div className="flex flex-col sm:flex-row items-center justify-between gap-4">
            <div>
              <h4 className="font-bold text-lg mb-1">Hardware Freedom</h4>
              <p className="text-sm text-muted-foreground">
                Our controllers work with <strong>your</strong> sensors. 4-20mA, 0-10V, SDI-12—bring what you have or buy off-the-shelf.
                <br className="hidden sm:block" />
                Don't pay $30k for a proprietary cabinet.
              </p>
            </div>
            <a 
              href="#demo"
              className="inline-flex items-center gap-2 px-5 py-2.5 rounded-lg bg-surface border border-border text-sm font-medium hover:border-accent-emerald/50 transition-colors whitespace-nowrap"
            >
              Learn More
              <ArrowRight className="h-4 w-4" />
            </a>
          </div>
        </div>

        {/* Capacity Scaling Table */}
        <div
          ref={capacityRef}
          className={`mt-16 transition-all duration-700 ${
            capacityVisible ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'
          }`}
        >
          <div className="text-center mb-8">
            <h3 className="text-xl font-bold mb-2">
              Capacity-Based Scaling
            </h3>
            <p className="text-sm text-muted-foreground">
              Foundation & Growth tiers include up to 5,000 sq ft. Add capacity as you grow.
            </p>
          </div>

          <div className="max-w-2xl mx-auto rounded-xl border border-border/50 bg-surface/30 overflow-hidden">
            <div className="grid grid-cols-2 gap-4 p-4 bg-surface/50 border-b border-border/50 text-sm font-medium">
              <div>Facility Canopy Size</div>
              <div className="text-right">Monthly Add-On</div>
            </div>
            {capacityTiers.map((tier, index) => (
              <div
                key={tier.range}
                className={`grid grid-cols-2 gap-4 p-4 text-sm ${
                  index !== capacityTiers.length - 1 ? 'border-b border-border/30' : ''
                } ${index === 0 ? 'bg-accent-emerald/5' : ''}`}
              >
                <div className="font-medium">{tier.range}</div>
                <div className={`text-right ${index === 0 ? 'text-accent-emerald font-semibold' : 'text-muted-foreground'}`}>
                  {tier.addon}
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Add-Ons Section */}
        <AddOns isAnnual={isAnnual} />

        {/* TCO Comparison */}
        <TCOComparison harvestryPrice={comparisonPrice} isAnnual={isAnnual} />

        {/* Custom Package CTA */}
        <div
          className={`mt-12 text-center transition-all duration-700 delay-300 ${
            cardsVisible ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'
          }`}
        >
          <p className="text-muted-foreground">
            Need something specific?{' '}
            <a
              href="mailto:sales@harvestry.io?subject=Custom Package Inquiry"
              className="text-accent-emerald hover:text-accent-emerald/80 font-medium transition-colors duration-300 hover:underline underline-offset-4"
            >
              Let&apos;s build a custom package
            </a>
          </p>
          <p className="text-xs text-muted-foreground mt-2">
            All paid plans include 14-day free trial • No credit card required • Cancel anytime
          </p>
        </div>
      </div>
    </section>
  );
}

// Re-export for convenience
export { pricingTiers, addOns, competitorComparisons, capacityTiers } from './pricingData';
