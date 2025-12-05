'use client';

import { useState } from 'react';
import { 
  Users, 
  FileCheck, 
  Calculator, 
  Wrench, 
  LineChart,
  ChevronRight,
  Check
} from 'lucide-react';
import { clsx } from 'clsx';
import { useScrollAnimation } from './hooks/useScrollAnimation';

const personas = [
  {
    id: 'cultivation',
    icon: Users,
    title: 'Heads of Cultivation',
    subtitle: 'Your single pane of glass',
    colorClass: {
      bg: 'bg-accent-emerald/10',
      bgHover: 'hover:bg-accent-emerald/5',
      border: 'border-accent-emerald/30',
      text: 'text-accent-emerald',
      glow: 'shadow-accent-emerald/20',
      gradient: 'from-accent-emerald/20 to-transparent',
    },
    benefits: [
      'Blueprint-driven task generation based on strain × phase × room',
      'Instant visibility into VPD, substrate conditions, and irrigation performance',
      'Anomaly detection that catches problems before they become disasters',
      'Real-time telemetry that actually means something',
    ],
  },
  {
    id: 'compliance',
    icon: FileCheck,
    title: 'Compliance Officers',
    subtitle: 'Sleep better at night',
    colorClass: {
      bg: 'bg-accent-violet/10',
      bgHover: 'hover:bg-accent-violet/5',
      border: 'border-accent-violet/30',
      text: 'text-accent-violet',
      glow: 'shadow-accent-violet/20',
      gradient: 'from-accent-violet/20 to-transparent',
    },
    benefits: [
      'Real-time compliance sync with automatic retry and reconciliation',
      'COA gating that blocks non-compliant product from moving',
      'Jurisdiction-specific label rules built right in',
      'Exportable audit trails that regulators actually want to see',
    ],
  },
  {
    id: 'finance',
    icon: Calculator,
    title: 'Finance Teams',
    subtitle: 'Cultivation that speaks accounting',
    colorClass: {
      bg: 'bg-accent-sky/10',
      bgHover: 'hover:bg-accent-sky/5',
      border: 'border-accent-sky/30',
      text: 'text-accent-sky',
      glow: 'shadow-accent-sky/20',
      gradient: 'from-accent-sky/20 to-transparent',
    },
    benefits: [
      'Automatic PO → Bill → Payment workflows',
      'Period-close journal entries with variance controls',
      'Full cost tracking: nutrients, labor, waste, yield',
      'Reconciliation reports you can actually trust',
    ],
  },
  {
    id: 'operators',
    icon: Wrench,
    title: 'Operators & Technicians',
    subtitle: 'Your day just got simpler',
    colorClass: {
      bg: 'bg-accent-amber/10',
      bgHover: 'hover:bg-accent-amber/5',
      border: 'border-accent-amber/30',
      text: 'text-accent-amber',
      glow: 'shadow-accent-amber/20',
      gradient: 'from-accent-amber/20 to-transparent',
    },
    benefits: [
      'Tasks assigned based on training certification',
      'Slack integration for real-time task notifications',
      'Evidence capture right from your device',
      'Clear escalation paths when you need help',
    ],
  },
  {
    id: 'executives',
    icon: LineChart,
    title: 'Executives & Owners',
    subtitle: 'Your entire operation at a glance',
    colorClass: {
      bg: 'bg-accent-cyan/10',
      bgHover: 'hover:bg-accent-cyan/5',
      border: 'border-accent-cyan/30',
      text: 'text-accent-cyan',
      glow: 'shadow-accent-cyan/20',
      gradient: 'from-accent-cyan/20 to-transparent',
    },
    benefits: [
      'Multi-site dashboards with drill-down capability',
      'KPIs that actually drive decisions',
      'Risk visibility before problems escalate',
      'Sustainability metrics (WUE, NUE, kWh/gram) for ESG reporting',
    ],
  },
];

export function PersonaBenefits() {
  const [activePersona, setActivePersona] = useState(personas[0]);
  const { ref: headerRef, isVisible: headerVisible } = useScrollAnimation({ threshold: 0.2 });
  const { ref: contentRef, isVisible: contentVisible } = useScrollAnimation({ threshold: 0.1 });

  return (
    <section id="solutions" className="py-32 relative overflow-hidden">
      {/* Background Effects */}
      <div className="absolute inset-0 pointer-events-none">
        <div className="absolute top-1/3 left-0 w-[600px] h-[600px] bg-accent-emerald/5 rounded-full blur-[120px]" />
        <div className="absolute bottom-1/3 right-0 w-[500px] h-[500px] bg-accent-violet/5 rounded-full blur-[100px]" />
        <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[800px] h-[400px] bg-accent-cyan/3 rounded-full blur-[150px]" />
      </div>
      
      <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div 
          ref={headerRef}
          className={clsx(
            'text-center mb-20 transition-all duration-1000',
            headerVisible ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'
          )}
        >
          <div className="inline-flex items-center gap-2 px-5 py-2.5 rounded-full bg-accent-violet/10 border border-accent-violet/20 text-accent-violet text-sm font-medium mb-8">
            <span className="relative flex h-2 w-2">
              <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-accent-violet opacity-75"></span>
              <span className="relative inline-flex rounded-full h-2 w-2 bg-accent-violet"></span>
            </span>
            Built for Every Role
          </div>
          <h2 className="text-4xl sm:text-5xl lg:text-6xl font-bold mb-6 tracking-tight">
            Tailored for{' '}
            <span className="bg-gradient-to-r from-accent-violet via-accent-cyan to-accent-emerald bg-clip-text text-transparent">
              Your Role
            </span>
          </h2>
          <p className="text-xl text-muted-foreground max-w-3xl mx-auto leading-relaxed">
            Whether you&apos;re managing grows, ensuring compliance, or tracking finances—
            <span className="text-foreground font-medium">Harvestry has you covered.</span>
          </p>
        </div>

        {/* Persona Tabs & Content */}
        <div 
          ref={contentRef}
          className={clsx(
            'grid lg:grid-cols-5 gap-8 lg:gap-12 transition-all duration-1000 delay-200',
            contentVisible ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'
          )}
        >
          {/* Tabs - Left Side */}
          <div className="lg:col-span-2 space-y-3">
            {personas.map((persona, index) => {
              const Icon = persona.icon;
              const isActive = activePersona.id === persona.id;
              return (
                <button
                  key={persona.id}
                  onClick={() => setActivePersona(persona)}
                  style={{ transitionDelay: `${index * 50}ms` }}
                  className={clsx(
                    'w-full group flex items-center gap-4 p-5 rounded-2xl text-left transition-all duration-300',
                    isActive
                      ? `${persona.colorClass.bg} border-2 ${persona.colorClass.border} shadow-lg ${persona.colorClass.glow}`
                      : 'bg-surface/50 border-2 border-transparent hover:bg-surface hover:border-border/50'
                  )}
                >
                  <div className={clsx(
                    'p-3 rounded-xl transition-all duration-300',
                    isActive 
                      ? `${persona.colorClass.bg} ${persona.colorClass.text}` 
                      : 'bg-elevated text-muted-foreground group-hover:text-foreground'
                  )}>
                    <Icon className="h-6 w-6" />
                  </div>
                  <div className="flex-1 min-w-0">
                    <div className={clsx(
                      'font-semibold text-base transition-colors duration-300',
                      isActive ? 'text-foreground' : 'text-muted-foreground group-hover:text-foreground'
                    )}>
                      {persona.title}
                    </div>
                    <div className={clsx(
                      'text-sm transition-colors duration-300 mt-0.5',
                      isActive ? persona.colorClass.text : 'text-muted-foreground/70'
                    )}>
                      {persona.subtitle}
                    </div>
                  </div>
                  <div className={clsx(
                    'w-8 h-8 rounded-full flex items-center justify-center transition-all duration-300',
                    isActive 
                      ? `${persona.colorClass.bg} ${persona.colorClass.text}` 
                      : 'bg-transparent text-muted-foreground group-hover:bg-surface'
                  )}>
                    <ChevronRight className={clsx(
                      'h-5 w-5 transition-transform duration-300',
                      isActive ? 'rotate-0' : '-rotate-90 group-hover:rotate-0'
                    )} />
                  </div>
                </button>
              );
            })}
          </div>

          {/* Content - Right Side */}
          <div className="lg:col-span-3">
            <div className={clsx(
              'relative p-8 lg:p-10 rounded-3xl bg-surface/80 backdrop-blur-sm border border-border/50',
              'shadow-2xl shadow-black/10 transition-all duration-500'
            )}>
              {/* Gradient accent */}
              <div className={clsx(
                'absolute top-0 left-0 right-0 h-1 rounded-t-3xl bg-gradient-to-r',
                activePersona.colorClass.gradient
              )} />
              
              {/* Header */}
              <div className="flex items-start gap-5 mb-8">
                <div className={clsx(
                  'p-4 rounded-2xl transition-colors duration-500',
                  activePersona.colorClass.bg
                )}>
                  {(() => {
                    const Icon = activePersona.icon;
                    return <Icon className={clsx('h-10 w-10', activePersona.colorClass.text)} />;
                  })()}
                </div>
                <div>
                  <h3 className="text-2xl lg:text-3xl font-bold tracking-tight mb-1">
                    {activePersona.title}
                  </h3>
                  <p className={clsx('text-lg', activePersona.colorClass.text)}>
                    {activePersona.subtitle}
                  </p>
                </div>
              </div>
              
              {/* Benefits List */}
              <div className="space-y-5">
                <h4 className="font-semibold text-muted-foreground uppercase tracking-wider text-xs">
                  Key Benefits
                </h4>
                <ul className="space-y-4">
                  {activePersona.benefits.map((benefit, index) => (
                    <li 
                      key={index} 
                      className="flex items-start gap-4 group/item"
                      style={{ animationDelay: `${index * 100}ms` }}
                    >
                      <div className={clsx(
                        'w-7 h-7 rounded-lg flex items-center justify-center flex-shrink-0 mt-0.5 transition-all duration-300',
                        activePersona.colorClass.bg,
                        'group-hover/item:scale-110'
                      )}>
                        <Check className={clsx('h-4 w-4', activePersona.colorClass.text)} />
                      </div>
                      <span className="text-foreground/90 text-base lg:text-lg leading-relaxed">
                        {benefit}
                      </span>
                    </li>
                  ))}
                </ul>
              </div>

              {/* CTA */}
              <div className="mt-10 pt-8 border-t border-border/50">
                <a 
                  href="#demo"
                  className={clsx(
                    'inline-flex items-center gap-3 px-6 py-3 rounded-xl font-semibold transition-all duration-300',
                    activePersona.colorClass.bg,
                    activePersona.colorClass.text,
                    'hover:gap-4 hover:shadow-lg',
                    activePersona.colorClass.glow
                  )}
                >
                  See Harvestry in action
                  <ChevronRight className="h-5 w-5" />
                </a>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
