'use client';

import { Check, Sparkles } from 'lucide-react';
import { PricingTier } from './pricingData';
import { MagneticButton } from '../MagneticButton';

interface PricingCardProps {
  tier: PricingTier;
  isAnnual: boolean;
  index: number;
}

// Use consistent emerald green for all highlights
const highlightColor = 'text-accent-emerald';

export function PricingCard({ tier, isAnnual, index }: PricingCardProps) {
  const isFree = tier.monthlyPrice === 0;
  const isCustom = tier.monthlyPrice === null;
  const price = isCustom ? null : (isAnnual ? tier.annualPrice : tier.monthlyPrice);
  const monthlyPrice = tier.monthlyPrice ?? 0;
  const annualPrice = tier.annualPrice ?? 0;
  const monthlySavings = !isCustom && !isFree ? monthlyPrice - annualPrice : 0;

  // Sort features: inheritance first, then highlighted, then by category
  const sortedFeatures = [...tier.features].sort((a, b) => {
    const aInheritance = a.text.startsWith('Everything in');
    const bInheritance = b.text.startsWith('Everything in');
    if (aInheritance && !bInheritance) return -1;
    if (!aInheritance && bInheritance) return 1;
    if (a.highlight && !b.highlight) return -1;
    if (!a.highlight && b.highlight) return 1;
    return 0;
  });

  const isEnterprise = tier.id === 'enterprise';
  const isMonitor = tier.id === 'monitor';
  const isFoundation = tier.id === 'foundation';

  return (
    <div
      className={`group relative p-6 lg:p-8 rounded-2xl border backdrop-blur-sm transition-all duration-500 hover:-translate-y-2 flex flex-col ${
        tier.popular
          ? 'bg-gradient-to-b from-accent-emerald/10 via-surface/80 to-surface border-accent-emerald/30 shadow-xl shadow-accent-emerald/5'
          : isEnterprise
          ? 'bg-gradient-to-b from-accent-violet/5 via-surface/80 to-surface border-accent-violet/30 shadow-lg shadow-accent-violet/5'
          : isFoundation
          ? 'bg-gradient-to-b from-accent-sky/5 via-surface/80 to-surface border-accent-sky/30 shadow-lg shadow-accent-sky/5'
          : isFree
          ? 'bg-gradient-to-b from-accent-cyan/5 via-surface/80 to-surface border-accent-cyan/30'
          : 'bg-surface/50 border-border/50 hover:border-border'
      }`}
      style={{ transitionDelay: `${index * 100}ms` }}
    >
      {/* Badge */}
      {tier.badge && (
        <div className="absolute -top-3.5 left-1/2 -translate-x-1/2">
          <div className="relative">
            {tier.popular && (
              <div className="absolute inset-0 bg-accent-emerald blur-lg opacity-50" />
            )}
            <div
              className={`relative px-4 py-1.5 rounded-full text-sm font-semibold ${
                tier.popular
                  ? 'bg-accent-emerald text-white shadow-lg shadow-accent-emerald/30'
                  : isFree
                  ? 'bg-background/95 backdrop-blur-md text-accent-cyan border border-accent-cyan/40 shadow-md'
                  : isFoundation
                  ? 'bg-background/95 backdrop-blur-md text-accent-sky border border-accent-sky/40 shadow-md'
                  : 'bg-background/95 backdrop-blur-md text-accent-violet border border-accent-violet/40 shadow-md'
              }`}
            >
              {tier.badge}
            </div>
          </div>
        </div>
      )}


      {/* Gradient overlay on hover */}
      <div
        className={`absolute inset-0 rounded-2xl bg-gradient-to-br ${tier.gradient} opacity-0 group-hover:opacity-100 transition-opacity duration-500 pointer-events-none`}
      />

      <div className="relative flex flex-col flex-1">
        {/* Header */}
        <div className="mb-4">
          <h3
            className={`text-xl font-bold transition-colors duration-300 ${
              tier.popular 
                ? 'text-accent-emerald' 
                : isEnterprise 
                ? 'text-accent-violet' 
                : isFoundation
                ? 'text-accent-sky'
                : isFree
                ? 'text-accent-cyan'
                : 'group-hover:text-accent-emerald'
            }`}
          >
            {tier.name}
          </h3>
          <p className="text-sm font-medium text-muted-foreground">{tier.tagline}</p>
        </div>

        {/* Price */}
        <div className="mb-4">
          <div className="flex items-baseline gap-1">
            {isCustom ? (
              <span className="text-2xl font-bold">Custom Pricing</span>
            ) : isFree ? (
              <span className="text-4xl font-bold text-accent-cyan">$0</span>
            ) : (
              <>
                <span className="text-4xl font-bold">${price?.toLocaleString()}</span>
                <span className="text-lg text-muted-foreground">/mo</span>
              </>
            )}
            {isAnnual && monthlySavings > 0 && !isCustom && !isFree && (
              <span className="ml-2 text-xs px-2 py-0.5 rounded-full bg-accent-emerald/10 text-accent-emerald font-medium whitespace-nowrap">
                Save ${monthlySavings}/mo
              </span>
            )}
          </div>
          <p className="text-sm text-muted-foreground mt-1">{tier.priceNote}</p>
        </div>

        {/* Capacity & Hardware Level */}
        {tier.capacity && (
          <div className="mb-4 pb-4 border-b border-border/50">
            <div className="flex flex-wrap gap-2">
              <span className="text-xs px-2 py-1 rounded-md bg-surface border border-border/50 text-muted-foreground">
                {tier.capacity}
              </span>
              <span className="text-xs px-2 py-1 rounded-md bg-surface border border-border/50 text-muted-foreground">
                {tier.hardwareLevel}
              </span>
            </div>
          </div>
        )}

        {/* Bundled Compliance/Financials indicator */}
        {(tier.complianceIncluded || tier.financialsIncluded) && (
          <div className="mb-4 p-3 rounded-lg bg-accent-emerald/5 border border-accent-emerald/20">
            <p className="text-xs font-medium text-accent-emerald mb-1">Included in this tier:</p>
            <div className="flex flex-wrap gap-2">
              {tier.complianceIncluded && (
                <span className="text-xs px-2 py-0.5 rounded bg-accent-emerald/10 text-accent-emerald">
                  ✓ Compliance
                </span>
              )}
              {tier.financialsIncluded && (
                <span className="text-xs px-2 py-0.5 rounded bg-accent-emerald/10 text-accent-emerald">
                  ✓ Financials
                </span>
              )}
            </div>
          </div>
        )}

        {/* Description */}
        <div className="mb-6">
          <p className="text-sm text-muted-foreground">{tier.description}</p>
          <p className="text-xs text-muted-foreground/70 mt-1">
            Best for: {tier.targetAudience}
          </p>
        </div>

        {/* Features */}
        <ul className="space-y-2.5 mb-8 flex-1">
          {sortedFeatures.map((feature, featureIndex) => {
            const isInheritance = feature.text.startsWith('Everything in');

            return (
              <li key={featureIndex} className="flex items-start gap-2.5 group/item">
                <div
                  className={`flex-shrink-0 mt-0.5 transition-transform duration-300 group-hover/item:scale-110 ${
                    feature.highlight || tier.popular
                      ? highlightColor
                      : 'text-muted-foreground group-hover:text-accent-emerald'
                  }`}
                >
                  {feature.highlight ? (
                    <Sparkles className="h-4 w-4" />
                  ) : (
                    <Check className="h-4 w-4" />
                  )}
                </div>
                <span
                  className={`text-sm transition-colors duration-300 ${
                    isInheritance
                      ? 'font-medium text-foreground'
                      : feature.highlight
                      ? `font-medium ${highlightColor}`
                      : 'text-muted-foreground group-hover/item:text-foreground'
                  }`}
                >
                  {feature.text}
                </span>
              </li>
            );
          })}
        </ul>

        {/* CTA Button */}
        <MagneticButton as="a" href={isCustom ? "mailto:enterprise@harvestry.io?subject=Enterprise Inquiry" : "#demo"} strength={0.15} className="block mt-auto">
          <span
            className={`w-full inline-flex items-center justify-center gap-2 px-6 py-3.5 rounded-xl font-semibold transition-all duration-300 ${
              tier.ctaVariant === 'primary'
                ? 'bg-accent-emerald text-white hover:bg-accent-emerald/90 shadow-lg shadow-accent-emerald/20 hover:shadow-xl hover:shadow-accent-emerald/30'
                : tier.ctaVariant === 'secondary'
                ? 'bg-surface border border-border text-foreground hover:bg-elevated hover:border-accent-emerald/30'
                : isFree
                ? 'bg-accent-cyan/10 border border-accent-cyan/30 text-accent-cyan hover:bg-accent-cyan/20'
                : 'bg-transparent border border-border/50 text-muted-foreground hover:text-foreground hover:border-border'
            }`}
          >
            {tier.cta}
          </span>
        </MagneticButton>
      </div>

      {/* Corner accent for popular plan */}
      {tier.popular && (
        <div className="absolute top-0 right-0 w-32 h-32 bg-accent-emerald/10 rounded-bl-full blur-2xl pointer-events-none" />
      )}
      {/* Corner accent for enterprise plan */}
      {isEnterprise && (
        <div className="absolute top-0 right-0 w-32 h-32 bg-accent-violet/10 rounded-bl-full blur-2xl pointer-events-none" />
      )}
      {/* Corner accent for foundation plan */}
      {isFoundation && (
        <div className="absolute top-0 right-0 w-32 h-32 bg-accent-sky/10 rounded-bl-full blur-2xl pointer-events-none" />
      )}
      {/* Corner accent for free plan */}
      {isFree && (
        <div className="absolute top-0 right-0 w-32 h-32 bg-accent-cyan/10 rounded-bl-full blur-2xl pointer-events-none" />
      )}
    </div>
  );
}
