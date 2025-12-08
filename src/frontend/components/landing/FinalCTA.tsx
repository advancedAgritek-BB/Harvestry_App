'use client';

import Link from 'next/link';
import { ArrowRight, Calendar, Leaf, Check, Sparkles } from 'lucide-react';
import { useScrollAnimation } from './hooks/useScrollAnimation';
import { MagneticButton } from './MagneticButton';

export function FinalCTA() {
  const { ref, isVisible } = useScrollAnimation({ threshold: 0.2 });

  return (
    <section id="demo" className="py-28 relative overflow-hidden">
      {/* Animated background */}
      <div className="absolute inset-0">
        <div className="absolute inset-0 bg-gradient-to-b from-background via-accent-emerald/5 to-background" />
        <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[1000px] h-[1000px] bg-accent-emerald/10 rounded-full blur-3xl animate-pulse-slow" />
        <div className="absolute top-1/4 left-1/4 w-[400px] h-[400px] bg-accent-cyan/5 rounded-full blur-3xl animate-pulse-slow delay-1000" />
        <div className="absolute bottom-1/4 right-1/4 w-[400px] h-[400px] bg-accent-violet/5 rounded-full blur-3xl animate-pulse-slow delay-500" />
      </div>
      
      <div 
        ref={ref}
        className={`relative max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 text-center transition-all duration-700 ${isVisible ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'}`}
      >
        {/* Floating icon */}
        <div className="inline-flex p-5 rounded-2xl bg-accent-emerald/10 border border-accent-emerald/20 mb-8 animate-float-slow">
          <div className="absolute inset-0 rounded-2xl bg-accent-emerald/20 blur-xl opacity-50" />
          <Leaf className="relative h-12 w-12 text-accent-emerald" />
        </div>

        {/* Headline */}
        <h2 className="text-3xl sm:text-4xl lg:text-5xl font-bold mb-6">
          Ready to Transform{' '}
          <span className="text-accent-emerald">
            Your Cultivation?
          </span>
        </h2>

        {/* Subheadline */}
        <p className="text-lg sm:text-xl text-muted-foreground max-w-2xl mx-auto mb-10">
          Join the leading cultivators who have unified their operations with Harvestry. 
          See how the platform fits your specific needs with a personalized demo.
        </p>

        {/* CTAs */}
        <div className="flex flex-col sm:flex-row items-center justify-center gap-4 mb-12">
          <MagneticButton as="a" href="https://calendly.com/bburnette-advancedagritek/harvestry-io-demo" target="_blank" rel="noopener noreferrer" strength={0.2}>
            <span className="group relative inline-flex items-center justify-center gap-3 px-8 py-4 text-base font-semibold text-white bg-accent-emerald rounded-xl overflow-hidden">
              {/* Animated gradient */}
              <span className="absolute inset-0 bg-gradient-to-r from-accent-emerald via-emerald-400 to-accent-emerald bg-[length:200%_auto] animate-gradient opacity-0 group-hover:opacity-100 transition-opacity duration-500" />
              {/* Glow */}
              <span className="absolute inset-0 opacity-0 group-hover:opacity-100 transition-opacity duration-500 blur-xl bg-accent-emerald/50" />
              <Calendar className="relative h-5 w-5" />
              <span className="relative">Book a Demo</span>
              <ArrowRight className="relative h-5 w-5 group-hover:translate-x-1 transition-transform duration-300" />
            </span>
          </MagneticButton>
          
          <MagneticButton as="a" href="mailto:sales@harvestry.io?subject=Contact Sales" strength={0.2}>
            <span className="group inline-flex items-center justify-center gap-2 px-8 py-4 text-base font-semibold text-foreground bg-surface border border-border rounded-xl transition-all duration-300 hover:border-foreground/20 hover:bg-elevated">
              <span className="absolute inset-0 bg-foreground/[0.02] opacity-0 group-hover:opacity-100 transition-opacity duration-300 rounded-xl" />
              <span className="relative">Contact Sales</span>
            </span>
          </MagneticButton>
        </div>

        {/* Trust Indicators */}
        <div className="flex flex-col sm:flex-row items-center justify-center gap-6 sm:gap-8">
          {[
            { text: 'No credit card required', icon: Check },
            { text: 'Personalized walkthrough', icon: Sparkles },
            { text: 'Response within 24 hours', icon: Check },
          ].map((item, index) => (
            <div 
              key={index}
              className="group flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors duration-300 cursor-default"
            >
              <div className="w-5 h-5 rounded-full bg-accent-emerald/10 flex items-center justify-center group-hover:bg-accent-emerald/20 transition-colors duration-300">
                <item.icon className="h-3 w-3 text-accent-emerald" />
              </div>
              <span>{item.text}</span>
            </div>
          ))}
        </div>

        {/* Decorative elements */}
        <div className="absolute -bottom-20 left-1/2 -translate-x-1/2 w-full max-w-2xl h-px bg-gradient-to-r from-transparent via-accent-emerald/30 to-transparent" />
      </div>
    </section>
  );
}


