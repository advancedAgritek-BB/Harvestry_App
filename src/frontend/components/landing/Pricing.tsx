'use client';

import { Check, ArrowRight, Sparkles } from 'lucide-react';
import Link from 'next/link';
import { useScrollAnimation } from './hooks/useScrollAnimation';
import { MagneticButton } from './MagneticButton';

const plans = [
  {
    name: 'Core',
    description: 'Essential cultivation management',
    price: 'Custom',
    priceNote: 'Based on canopy size',
    features: [
      'Identity & access control (RLS)',
      'Spatial model & equipment registry',
      'Batch lifecycle & blueprint tasks',
      'Telemetry ingest & rollups',
      'Open-loop irrigation programs',
      'Inventory & movement tracking',
      'Basic reporting & dashboards',
      'Email & Slack notifications',
    ],
    cta: 'Get Started',
    popular: false,
    gradient: 'from-slate-500/10 to-slate-600/5',
  },
  {
    name: 'Professional',
    description: 'Full operational control',
    price: 'Custom',
    priceNote: 'Per licensed facility',
    features: [
      'Everything in Core, plus:',
      'METRC/BioTrack compliance sync',
      'COA gating & hold management',
      'QuickBooks Online integration',
      'Closed-loop EC/pH control',
      'Advanced task workflows',
      'Multi-site dashboards',
      'SMS critical alerts',
      'Priority support',
    ],
    cta: 'Talk to Sales',
    popular: true,
    gradient: 'from-accent-emerald/10 to-accent-cyan/5',
  },
  {
    name: 'Enterprise',
    description: 'For scale and sophistication',
    price: 'Custom',
    priceNote: 'Multi-site pricing',
    features: [
      'Everything in Professional, plus:',
      'Autosteer MPC (climate/irrigation)',
      'AI-powered insights & predictions',
      'Copilot Ask-to-Act',
      'Sustainability & ESG reporting',
      'Predictive maintenance',
      'Custom integrations & API',
      'Dedicated success manager',
      'SLA guarantees',
    ],
    cta: 'Contact Enterprise',
    popular: false,
    gradient: 'from-accent-violet/10 to-accent-cyan/5',
  },
];

export function Pricing() {
  const { ref: headerRef, isVisible: headerVisible } = useScrollAnimation({ threshold: 0.2 });
  const { ref: cardsRef, isVisible: cardsVisible } = useScrollAnimation({ threshold: 0.1 });

  return (
    <section id="pricing" className="py-28 relative overflow-hidden">
      {/* Background */}
      <div className="absolute inset-0 bg-surface/20" />
      <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[800px] h-[800px] bg-accent-emerald/5 rounded-full blur-3xl" />
      
      <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div 
          ref={headerRef}
          className={`text-center mb-16 transition-all duration-700 ${headerVisible ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'}`}
        >
          <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-accent-sky/10 border border-accent-sky/20 text-accent-sky text-sm font-medium mb-6">
            <Sparkles className="h-4 w-4" />
            Transparent Pricing
          </div>
          <h2 className="text-3xl sm:text-4xl lg:text-5xl font-bold mb-5">
            Plans That{' '}
            <span className="bg-gradient-to-r from-accent-sky via-accent-emerald to-accent-sky bg-[length:200%_auto] bg-clip-text text-transparent animate-gradient">
              Scale With You
            </span>
          </h2>
          <p className="text-lg text-muted-foreground max-w-2xl mx-auto">
            From single facilities to multi-state operations. 
            Pay for what you need, grow into what you want.
          </p>
        </div>

        {/* Pricing Grid */}
        <div 
          ref={cardsRef}
          className="grid md:grid-cols-3 gap-6 lg:gap-8"
          data-visible={cardsVisible}
          style={{ '--stagger-delay': '150ms' } as React.CSSProperties}
        >
          {plans.map((plan, index) => (
            <div 
              key={plan.name}
              className={`group relative p-8 rounded-2xl border backdrop-blur-sm transition-all duration-500 hover:-translate-y-2 ${
                plan.popular 
                  ? 'bg-gradient-to-b from-accent-emerald/10 via-surface/80 to-surface border-accent-emerald/30 shadow-xl shadow-accent-emerald/5' 
                  : 'bg-surface/50 border-border/50 hover:border-border'
              }`}
            >
              {/* Popular badge */}
              {plan.popular && (
                <div className="absolute -top-4 left-1/2 -translate-x-1/2">
                  <div className="relative">
                    <div className="absolute inset-0 bg-accent-emerald blur-lg opacity-50" />
                    <div className="relative px-4 py-1.5 rounded-full bg-accent-emerald text-white text-sm font-semibold">
                      Most Popular
                    </div>
                  </div>
                </div>
              )}

              {/* Gradient overlay on hover */}
              <div className={`absolute inset-0 rounded-2xl bg-gradient-to-br ${plan.gradient} opacity-0 group-hover:opacity-100 transition-opacity duration-500 pointer-events-none`} />

              <div className="relative">
                {/* Plan name & description */}
                <div className="mb-6">
                  <h3 className={`text-xl font-bold mb-2 transition-colors duration-300 ${plan.popular ? 'text-accent-emerald' : 'group-hover:text-accent-emerald'}`}>
                    {plan.name}
                  </h3>
                  <p className="text-sm text-muted-foreground">{plan.description}</p>
                </div>

                {/* Price */}
                <div className="mb-8">
                  <div className="text-4xl font-bold group-hover:scale-105 transition-transform duration-300 inline-block">
                    {plan.price}
                  </div>
                  <div className="text-sm text-muted-foreground mt-1">{plan.priceNote}</div>
                </div>

                {/* Features */}
                <ul className="space-y-3.5 mb-8">
                  {plan.features.map((feature, featureIndex) => (
                    <li 
                      key={featureIndex} 
                      className="flex items-start gap-3 group/item"
                    >
                      <div className={`flex-shrink-0 mt-0.5 transition-transform duration-300 group-hover/item:scale-110 ${
                        plan.popular ? 'text-accent-emerald' : 'text-muted-foreground group-hover:text-accent-emerald'
                      }`}>
                        <Check className="h-5 w-5" />
                      </div>
                      <span className={`text-sm transition-colors duration-300 ${
                        feature.startsWith('Everything') 
                          ? 'font-medium text-foreground' 
                          : 'text-muted-foreground group-hover/item:text-foreground'
                      }`}>
                        {feature}
                      </span>
                    </li>
                  ))}
                </ul>

                {/* CTA Button */}
                <MagneticButton as="a" href="#demo" strength={0.15} className="block">
                  <span className={`w-full inline-flex items-center justify-center gap-2 px-6 py-3.5 rounded-xl font-semibold transition-all duration-300 ${
                    plan.popular
                      ? 'bg-accent-emerald text-white hover:bg-accent-emerald/90 shadow-lg shadow-accent-emerald/20 hover:shadow-xl hover:shadow-accent-emerald/30'
                      : 'bg-surface border border-border text-foreground hover:bg-elevated hover:border-accent-emerald/30'
                  }`}>
                    {plan.cta}
                    <ArrowRight className="h-4 w-4 group-hover:translate-x-1 transition-transform duration-300" />
                  </span>
                </MagneticButton>
              </div>

              {/* Corner accent for popular plan */}
              {plan.popular && (
                <div className="absolute top-0 right-0 w-32 h-32 bg-accent-emerald/10 rounded-bl-full blur-2xl pointer-events-none" />
              )}
            </div>
          ))}
        </div>

        {/* Enterprise Note */}
        <div className={`mt-12 text-center transition-all duration-700 delay-300 ${cardsVisible ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}`}>
          <p className="text-muted-foreground">
            Need something specific?{' '}
            <a 
              href="#demo" 
              className="text-accent-emerald hover:text-accent-emerald/80 font-medium transition-colors duration-300 hover:underline underline-offset-4"
            >
              Let&apos;s build a custom package
            </a>
          </p>
        </div>
      </div>
    </section>
  );
}

