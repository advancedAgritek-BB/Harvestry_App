'use client';

import { useState, useRef, useEffect } from 'react';
import Image from 'next/image';
import { ChevronLeft, ChevronRight } from 'lucide-react';
import { DASHBOARD_TABS, TRUST_METRICS, TRUST_COLORS } from './constants/dashboardData';

export function MobileDashboardCarousel() {
  const [activeIndex, setActiveIndex] = useState(0);
  const carouselRef = useRef<HTMLDivElement>(null);
  const [isAutoPlaying, setIsAutoPlaying] = useState(true);

  // Auto-advance slides every 4 seconds when auto-playing
  useEffect(() => {
    if (!isAutoPlaying) return;
    
    const interval = setInterval(() => {
      setActiveIndex((prev) => (prev + 1) % DASHBOARD_TABS.length);
    }, 4000);

    return () => clearInterval(interval);
  }, [isAutoPlaying]);

  // Scroll to active slide when activeIndex changes
  useEffect(() => {
    if (!carouselRef.current) return;
    
    const slideWidth = carouselRef.current.offsetWidth;
    carouselRef.current.scrollTo({
      left: activeIndex * slideWidth,
      behavior: 'smooth',
    });
  }, [activeIndex]);

  // Handle manual scroll detection
  const handleScroll = () => {
    if (!carouselRef.current) return;
    
    const slideWidth = carouselRef.current.offsetWidth;
    const scrollLeft = carouselRef.current.scrollLeft;
    const newIndex = Math.round(scrollLeft / slideWidth);
    
    if (newIndex !== activeIndex) {
      setActiveIndex(newIndex);
      setIsAutoPlaying(false); // Pause auto-play on manual interaction
    }
  };

  const goToSlide = (index: number) => {
    setActiveIndex(index);
    setIsAutoPlaying(false);
  };

  const goToPrevious = () => {
    setActiveIndex((prev) => (prev - 1 + DASHBOARD_TABS.length) % DASHBOARD_TABS.length);
    setIsAutoPlaying(false);
  };

  const goToNext = () => {
    setActiveIndex((prev) => (prev + 1) % DASHBOARD_TABS.length);
    setIsAutoPlaying(false);
  };

  return (
    <div className="w-full px-4">
      {/* Glow backdrop */}
      <div className="absolute -inset-4 bg-gradient-to-r from-accent-emerald/15 via-accent-cyan/15 to-accent-violet/15 blur-2xl opacity-40 rounded-2xl -z-10" />

      {/* Browser window container */}
      <div className="relative rounded-xl overflow-hidden border border-border/50 shadow-xl shadow-black/40 bg-surface">
        {/* Browser chrome */}
        <div className="h-8 bg-elevated/80 backdrop-blur flex items-center gap-2 px-3 border-b border-border/50">
          <div className="flex gap-1.5">
            <div className="w-2.5 h-2.5 rounded-full bg-accent-rose/60" />
            <div className="w-2.5 h-2.5 rounded-full bg-accent-amber/60" />
            <div className="w-2.5 h-2.5 rounded-full bg-accent-emerald/60" />
          </div>
          <div className="flex-1 flex justify-center">
            <div className="px-3 py-0.5 rounded-md bg-background/50 text-xs text-muted-foreground flex items-center gap-1.5">
              <div className="w-2 h-2 rounded-sm bg-accent-emerald/30" />
              app.harvestry.io
            </div>
          </div>
        </div>

        {/* Carousel container */}
        <div
          ref={carouselRef}
          onScroll={handleScroll}
          className="flex overflow-x-auto snap-x snap-mandatory scrollbar-hide"
          style={{ scrollbarWidth: 'none', msOverflowStyle: 'none' }}
        >
          {DASHBOARD_TABS.map((tab) => (
            <div
              key={tab.id}
              className="flex-shrink-0 w-full snap-center"
            >
              <div className="relative aspect-[16/10] bg-background">
                <Image
                  src={tab.image}
                  alt={`Harvestry ${tab.label} dashboard`}
                  fill
                  className="object-cover object-top"
                  priority={tab.id === 'cultivation'}
                />
              </div>
            </div>
          ))}
        </div>

        {/* Navigation arrows */}
        <button
          onClick={goToPrevious}
          className="absolute left-2 top-1/2 -translate-y-1/2 p-1.5 rounded-full bg-background/80 backdrop-blur border border-border/50 text-muted-foreground hover:text-foreground transition-colors"
          aria-label="Previous slide"
        >
          <ChevronLeft className="w-4 h-4" />
        </button>
        <button
          onClick={goToNext}
          className="absolute right-2 top-1/2 -translate-y-1/2 p-1.5 rounded-full bg-background/80 backdrop-blur border border-border/50 text-muted-foreground hover:text-foreground transition-colors"
          aria-label="Next slide"
        >
          <ChevronRight className="w-4 h-4" />
        </button>
      </div>

      {/* Tab label and description */}
      <div className="mt-4 text-center">
        <h3 className="text-lg font-semibold text-foreground">
          {DASHBOARD_TABS[activeIndex].label}
        </h3>
        <p className="text-sm text-muted-foreground mt-1">
          {DASHBOARD_TABS[activeIndex].description}
        </p>
      </div>

      {/* Dot indicators */}
      <div className="flex justify-center gap-2 mt-4">
        {DASHBOARD_TABS.map((tab, index) => (
          <button
            key={tab.id}
            onClick={() => goToSlide(index)}
            className={`w-2 h-2 rounded-full transition-all duration-300 ${
              index === activeIndex
                ? 'bg-accent-emerald w-6'
                : 'bg-border hover:bg-muted-foreground/50'
            }`}
            aria-label={`Go to ${tab.label} slide`}
          />
        ))}
      </div>

      {/* Swipe hint */}
      <p className="text-center text-xs text-muted-foreground mt-3">
        Swipe to explore dashboards
      </p>

      {/* Trust Badges - Mobile optimized (2 columns) */}
      <div className="mt-8">
        <p className="text-center text-sm font-medium text-muted-foreground mb-4">
          Enterprise-grade infrastructure
        </p>
        <div className="grid grid-cols-2 gap-2">
          {TRUST_METRICS.map((metric) => {
            const Icon = metric.icon;
            const colors = TRUST_COLORS[metric.color];
            return (
              <div
                key={metric.label}
                className="flex flex-col items-center text-center p-3 rounded-lg bg-surface/50 border border-border/50 backdrop-blur-sm"
              >
                <div className={`p-1.5 rounded-lg ${colors.bg} mb-1.5`}>
                  <Icon className={`h-3.5 w-3.5 ${colors.text}`} />
                </div>
                <div className={`text-base font-bold ${colors.text}`}>{metric.value}</div>
                <div className="text-xs text-muted-foreground">{metric.label}</div>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
}
