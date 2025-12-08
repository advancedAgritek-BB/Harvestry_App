'use client';

import { Star, Quote } from 'lucide-react';
import { useScrollAnimation } from './hooks/useScrollAnimation';

const testimonials = [
  {
    quote: "Harvestry transformed how we manage compliance. What used to take hours of manual entry now happens automatically. Our audit prep time dropped by 80%.",
    author: "Sarah M.",
    role: "Compliance Director",
    company: "Multi-State Operator",
    rating: 5,
    avatar: "SM",
  },
  {
    quote: "The real-time telemetry changed everything. We caught an HVAC drift issue before it affected the crop. That single save paid for a year of the platform.",
    author: "Mike T.",
    role: "Head of Cultivation",
    company: "Premium Craft Cultivator",
    rating: 5,
    avatar: "MT",
  },
  {
    quote: "Finally, a platform where our financial data matches our cultivation records. The QuickBooks integration is seamless and our month-end close is now pain-free.",
    author: "Jennifer K.",
    role: "CFO",
    company: "Vertically Integrated Operator",
    rating: 5,
    avatar: "JK",
  },
];

const stats = [
  { value: '80%', label: 'Less audit prep time', color: 'accent-emerald' },
  { value: '35%', label: 'Planning time reduction', color: 'accent-cyan' },
  { value: '95%', label: 'Irrigation on-time rate', color: 'accent-amber' },
  { value: '30%', label: 'Error rate reduction', color: 'accent-violet' },
];

export function Testimonials() {
  const { ref: headerRef, isVisible: headerVisible } = useScrollAnimation({ threshold: 0.2 });
  const { ref: cardsRef, isVisible: cardsVisible } = useScrollAnimation({ threshold: 0.1 });
  const { ref: statsRef, isVisible: statsVisible } = useScrollAnimation({ threshold: 0.2 });

  return (
    <section className="py-28 relative overflow-hidden">
      {/* Background effects */}
      <div className="absolute inset-0">
        <div className="absolute top-0 left-1/4 w-[500px] h-[500px] bg-accent-violet/5 rounded-full blur-3xl" />
        <div className="absolute bottom-0 right-1/4 w-[500px] h-[500px] bg-accent-emerald/5 rounded-full blur-3xl" />
      </div>
      
      <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div 
          ref={headerRef}
          className={`text-center mb-16 transition-all duration-700 ${headerVisible ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'}`}
        >
          <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-foreground/5 border border-foreground/10 text-muted-foreground text-sm font-medium mb-6">
            <span className="relative flex h-2 w-2">
              <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-accent-emerald opacity-75"></span>
              <span className="relative inline-flex rounded-full h-2 w-2 bg-accent-emerald"></span>
            </span>
            Customer Success
          </div>
          <h2 className="text-3xl sm:text-4xl lg:text-5xl font-bold mb-5">
            Trusted by{' '}
            <span className="text-accent-emerald">
              Leading Cultivators
            </span>
          </h2>
          <p className="text-lg text-muted-foreground max-w-2xl mx-auto">
            See why cultivation teams are switching to Harvestry for unified operations.
          </p>
        </div>

        {/* Testimonials Grid */}
        <div 
          ref={cardsRef}
          className="grid md:grid-cols-3 gap-6"
          data-visible={cardsVisible}
          style={{ '--stagger-delay': '150ms' } as React.CSSProperties}
        >
          {testimonials.map((testimonial, index) => (
            <div 
              key={index}
              className="testimonial-card group relative p-8 rounded-2xl bg-surface/50 border border-border/50 backdrop-blur-sm hover:border-accent-emerald/30 transition-all duration-500"
            >
              {/* Quote Icon with glow */}
              <div className="absolute -top-4 -left-4 p-3 rounded-xl bg-accent-emerald/10 border border-accent-emerald/20 group-hover:bg-accent-emerald/20 transition-colors duration-300">
                <div className="absolute inset-0 rounded-xl bg-accent-emerald/20 blur-lg opacity-0 group-hover:opacity-100 transition-opacity duration-300" />
                <Quote className="relative h-5 w-5 text-accent-emerald" />
              </div>

              {/* Rating with animation */}
              <div className="flex gap-1 mb-5 mt-2">
                {[...Array(testimonial.rating)].map((_, i) => (
                  <Star 
                    key={i} 
                    className="h-4 w-4 fill-accent-amber text-accent-amber transition-transform duration-300 group-hover:scale-110"
                    style={{ transitionDelay: `${i * 50}ms` }}
                  />
                ))}
              </div>

              {/* Quote */}
              <blockquote className="text-foreground mb-6 leading-relaxed group-hover:text-foreground/90 transition-colors duration-300">
                &ldquo;{testimonial.quote}&rdquo;
              </blockquote>

              {/* Author */}
              <div className="flex items-center gap-4">
                <div className="relative w-12 h-12 rounded-full bg-gradient-to-br from-accent-emerald/30 to-accent-cyan/30 flex items-center justify-center overflow-hidden group-hover:scale-105 transition-transform duration-300">
                  <span className="text-lg font-semibold text-foreground">
                    {testimonial.avatar}
                  </span>
                  {/* Animated ring */}
                  <div className="absolute inset-0 rounded-full border-2 border-accent-emerald/0 group-hover:border-accent-emerald/50 transition-all duration-500" />
                </div>
                <div>
                  <div className="font-semibold group-hover:text-accent-emerald transition-colors duration-300">
                    {testimonial.author}
                  </div>
                  <div className="text-sm text-muted-foreground">
                    {testimonial.role}
                  </div>
                  <div className="text-xs text-muted-foreground/70">
                    {testimonial.company}
                  </div>
                </div>
              </div>

              {/* Hover gradient overlay */}
              <div className="absolute inset-0 rounded-2xl bg-gradient-to-br from-accent-emerald/0 via-transparent to-accent-cyan/0 group-hover:from-accent-emerald/5 group-hover:to-accent-cyan/5 transition-all duration-500 pointer-events-none" />
            </div>
          ))}
        </div>

        {/* Stats with animated counters */}
        <div 
          ref={statsRef}
          className={`mt-20 grid grid-cols-2 md:grid-cols-4 gap-8 transition-all duration-700 delay-200 ${statsVisible ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'}`}
        >
          {stats.map((stat, index) => (
            <div 
              key={index} 
              className="group text-center cursor-default"
              style={{ transitionDelay: `${index * 100 + 200}ms` }}
            >
              <div className={`text-4xl sm:text-5xl font-bold text-${stat.color} mb-2 group-hover:scale-110 transition-transform duration-300`}>
                {stat.value}
              </div>
              <div className="text-sm text-muted-foreground group-hover:text-foreground transition-colors duration-300">
                {stat.label}
              </div>
              {/* Underline animation */}
              <div className={`h-0.5 mx-auto mt-2 bg-${stat.color} rounded-full transition-all duration-300 w-0 group-hover:w-12`} />
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}


