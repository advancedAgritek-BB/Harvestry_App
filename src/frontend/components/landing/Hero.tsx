'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import Image from 'next/image';
import { ArrowRight, Play, Shield, Zap, BarChart3, Sparkles } from 'lucide-react';
import { MagneticButton } from './MagneticButton';
import { InteractiveDashboard } from './InteractiveDashboard';
import { useMousePosition } from './hooks/useScrollAnimation';

export function Hero() {
  const [mounted, setMounted] = useState(false);
  const mousePosition = useMousePosition();
  
  useEffect(() => {
    setMounted(true);
  }, []);

  // Calculate subtle parallax for background orbs based on mouse position
  const getOrbOffset = (factor: number) => {
    if (!mounted) return { x: 0, y: 0 };
    const x = (mousePosition.x - window.innerWidth / 2) * factor * 0.02;
    const y = (mousePosition.y - window.innerHeight / 2) * factor * 0.02;
    return { x, y };
  };

  const orb1 = getOrbOffset(1);
  const orb2 = getOrbOffset(-0.5);
  const orb3 = getOrbOffset(0.3);

  return (
    <>
    <section className="relative flex flex-col items-center justify-center pt-32 pb-4">
      {/* Animated Background */}
      <div className="absolute inset-0 overflow-hidden">
        {/* Mouse-reactive Gradient Orbs */}
        <div 
          className="absolute top-1/4 left-1/4 w-[500px] h-[500px] bg-accent-emerald/15 rounded-full blur-[100px] transition-transform duration-1000 ease-out"
          style={{ transform: `translate(${orb1.x}px, ${orb1.y}px)` }}
        />
        <div 
          className="absolute bottom-1/4 right-1/4 w-[600px] h-[600px] bg-accent-cyan/10 rounded-full blur-[120px] transition-transform duration-1000 ease-out"
          style={{ transform: `translate(${orb2.x}px, ${orb2.y}px)` }}
        />
        <div 
          className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[800px] h-[800px] bg-accent-violet/5 rounded-full blur-[150px] transition-transform duration-1000 ease-out"
          style={{ transform: `translate(calc(-50% + ${orb3.x}px), calc(-50% + ${orb3.y}px))` }}
        />
        
        {/* Animated Grid Pattern */}
        <div 
          className="absolute inset-0 opacity-[0.03]"
          style={{
            backgroundImage: `linear-gradient(to right, rgba(255,255,255,0.1) 1px, transparent 1px),
                              linear-gradient(to bottom, rgba(255,255,255,0.1) 1px, transparent 1px)`,
            backgroundSize: '80px 80px',
          }}
        />

        {/* Floating particles */}
        <div className="absolute inset-0 overflow-hidden pointer-events-none">
          {mounted && [...Array(20)].map((_, i) => (
            <div
              key={i}
              className="absolute w-1 h-1 bg-accent-emerald/30 rounded-full animate-float"
              style={{
                left: `${Math.random() * 100}%`,
                top: `${Math.random() * 100}%`,
                animationDelay: `${Math.random() * 5}s`,
                animationDuration: `${8 + Math.random() * 4}s`,
              }}
            />
          ))}
        </div>
      </div>

      <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
        <div className="text-center max-w-4xl mx-auto">
          {/* Badge with shimmer effect */}
          <div 
            className={`inline-flex items-center gap-2 px-4 py-2 rounded-full bg-accent-emerald/10 border border-accent-emerald/20 text-accent-emerald text-sm font-medium mb-8 overflow-hidden relative transition-all duration-700 ${mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}`}
          >
            <div className="absolute inset-0 bg-gradient-to-r from-transparent via-accent-emerald/20 to-transparent animate-shimmer" />
            <Sparkles className="h-4 w-4 relative" />
            <span className="relative">Now Available for Cultivators</span>
          </div>

          {/* Headline with staggered reveal */}
          <h1 
            className={`text-4xl sm:text-5xl lg:text-7xl font-bold tracking-tight mb-6 transition-all duration-700 delay-100 ${mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}`}
          >
            <span className="text-foreground inline-block hover:scale-[1.02] transition-transform cursor-default">The Modern</span>
            <br />
            <span className="bg-gradient-to-r from-accent-emerald via-accent-cyan to-accent-emerald bg-[length:200%_auto] bg-clip-text text-transparent animate-gradient inline-block">
              Cultivation Operating System
            </span>
          </h1>

          {/* Subheadline */}
          <p 
            className={`text-lg sm:text-xl text-muted-foreground max-w-2xl mx-auto mb-10 transition-all duration-700 delay-200 ${mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}`}
          >
            Unify your grow. Simplify compliance. Optimize everything.
            <br className="hidden sm:block" />
            <span className="text-foreground/80">One platform</span> for ERP, telemetry, and prescriptive control.
          </p>

          {/* CTAs with magnetic effect */}
          <div 
            className={`flex flex-col sm:flex-row items-center justify-center gap-4 mb-10 transition-all duration-700 delay-300 ${mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}`}
          >
            <MagneticButton as="a" href="#demo" strength={0.2}>
              <span className="group relative inline-flex items-center justify-center gap-2 px-8 py-4 text-base font-semibold text-white bg-accent-emerald rounded-xl overflow-hidden">
                {/* Animated gradient background */}
                <span className="absolute inset-0 bg-gradient-to-r from-accent-emerald via-emerald-400 to-accent-emerald bg-[length:200%_auto] animate-gradient opacity-0 group-hover:opacity-100 transition-opacity duration-500" />
                {/* Glow effect */}
                <span className="absolute inset-0 opacity-0 group-hover:opacity-100 transition-opacity duration-500 blur-xl bg-accent-emerald/50" />
                <span className="relative flex items-center gap-2">
                  Book a Demo
                  <ArrowRight className="h-5 w-5 group-hover:translate-x-1 transition-transform duration-300" />
                </span>
              </span>
            </MagneticButton>
            
            <MagneticButton as="button" strength={0.2}>
              <span className="group relative inline-flex items-center justify-center gap-2 px-8 py-4 text-base font-semibold text-foreground bg-surface border border-border rounded-xl overflow-hidden hover:border-accent-cyan/50 transition-colors duration-300">
                <span className="absolute inset-0 bg-accent-cyan/5 opacity-0 group-hover:opacity-100 transition-opacity duration-300" />
                <Play className="h-5 w-5 text-accent-cyan group-hover:scale-110 transition-transform duration-300" />
                <span className="relative">Watch Overview</span>
              </span>
            </MagneticButton>
          </div>

          {/* Value Props with hover effects */}
          <div 
            className={`grid grid-cols-1 sm:grid-cols-3 gap-4 max-w-3xl mx-auto transition-all duration-700 delay-400 ${mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}`}
          >
            {[
              { icon: Shield, text: 'Compliance-Ready', color: 'accent-emerald' },
              { icon: Zap, text: 'Real-Time Telemetry', color: 'accent-amber' },
              { icon: BarChart3, text: 'Enterprise-Grade', color: 'accent-cyan' },
            ].map((item, index) => (
              <div 
                key={item.text}
                className="group relative flex items-center justify-center gap-3 px-5 py-4 rounded-xl bg-surface/30 border border-border/50 backdrop-blur-sm hover:border-border hover:bg-surface/50 transition-all duration-300 cursor-default overflow-hidden"
                style={{ transitionDelay: `${index * 50}ms` }}
              >
                {/* Hover glow */}
                <div className={`absolute inset-0 bg-${item.color}/5 opacity-0 group-hover:opacity-100 transition-opacity duration-300`} />
                <item.icon className={`h-5 w-5 text-${item.color} flex-shrink-0 group-hover:scale-110 transition-transform duration-300`} />
                <span className="text-sm font-medium text-muted-foreground group-hover:text-foreground transition-colors duration-300">
                  {item.text}
                </span>
              </div>
            ))}
          </div>
        </div>

        {/* Scroll Indicator */}
        <div className={`mt-3 flex flex-col items-center gap-1 transition-all duration-700 delay-700 ${mounted ? 'opacity-100' : 'opacity-0'}`}>
          <span className="text-xs text-muted-foreground uppercase tracking-wider">Scroll to explore</span>
          <div className="w-6 h-10 rounded-full border-2 border-muted-foreground/30 flex items-start justify-center p-1.5">
            <div className="w-1.5 h-3 bg-muted-foreground/50 rounded-full animate-scroll" />
          </div>
        </div>
      </div>
    </section>

    {/* Interactive Dashboard Section - positioned below scroll indicator */}
    <section className={`relative mt-2 transition-all duration-1000 ${mounted ? 'opacity-100' : 'opacity-0'}`}>
      <InteractiveDashboard />
    </section>
    </>
  );
}
