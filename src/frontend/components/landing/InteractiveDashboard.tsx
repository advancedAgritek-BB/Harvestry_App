'use client';

import { useEffect, useState, useRef } from 'react';
import { Shield, Leaf, CheckSquare, ListTodo, Activity, Droplets, Server, Clock, Lock, Database, FileSpreadsheet, AlertTriangle, RefreshCw, ArrowRight, Check, X } from 'lucide-react';
import { useMobileBreakpoint } from '@/hooks/device';
import { MobileDashboardCarousel } from './MobileDashboardCarousel';
import { MobileProblemSolution } from './MobileProblemSolution';

// Problem/Solution data
const PROBLEMS = [
  { icon: FileSpreadsheet, title: 'Disconnected Tools', description: 'Spreadsheets for scheduling, separate apps for climate monitoring, manual processes for compliance.' },
  { icon: AlertTriangle, title: 'Compliance Anxiety', description: 'Manual data entry into METRC/BioTrack, scrambling before inspections, hoping nothing was missed.' },
  { icon: RefreshCw, title: 'Reconciliation Chaos', description: 'Financial data that never quite matches cultivation records, endless manual adjustments.' },
];

const SOLUTIONS = [
  'Single unified platform for all operations',
  'Automatic compliance sync with retry & reconciliation',
  'Blueprint-driven, SLA-monitored workflows',
  'Real-time financial integration with QuickBooks',
  'Safety-first automation with complete explainability',
  'Predictive equipment health monitoring',
  'Out-of-the-box KPIs and dashboards',
];

// Trust badge metrics
const TRUST_METRICS = [
  { icon: Activity, value: '99.9%', label: 'Uptime SLA', color: 'emerald' },
  { icon: Clock, value: '<1s', label: 'Telemetry Ingest', color: 'cyan' },
  { icon: Shield, value: 'SOC 2', label: 'Type II Compliant', color: 'violet' },
  { icon: Lock, value: 'RLS', label: 'Row-Level Security', color: 'amber' },
  { icon: Database, value: '5 min', label: 'Max Data Loss (RPO)', color: 'sky' },
  { icon: Server, value: '30 min', label: 'Recovery Time (RTO)', color: 'rose' },
];

const TRUST_COLORS: Record<string, { bg: string; text: string }> = {
  emerald: { bg: 'bg-accent-emerald/10', text: 'text-accent-emerald' },
  cyan: { bg: 'bg-accent-cyan/10', text: 'text-accent-cyan' },
  violet: { bg: 'bg-accent-violet/10', text: 'text-accent-violet' },
  amber: { bg: 'bg-accent-amber/10', text: 'text-accent-amber' },
  sky: { bg: 'bg-accent-sky/10', text: 'text-accent-sky' },
  rose: { bg: 'bg-accent-rose/10', text: 'text-accent-rose' },
};

// Widget configuration with descriptions
const WIDGET_INFO = {
  rooms: {
    label: 'Spatial Management',
    description: 'Track every room, zone, rack, and bin in real-time',
    icon: Shield,
    color: 'accent-emerald',
  },
  plants: {
    label: 'Plant Lifecycle',
    description: 'Track every plant from seed to sale with full traceability',
    icon: Leaf,
    color: 'accent-cyan',
  },
  compliance: {
    label: 'Compliance Engine',
    description: 'Automated METRC sync keeps you audit-ready 24/7',
    icon: CheckSquare,
    color: 'accent-amber',
  },
  tasks: {
    label: 'Task Orchestration',
    description: 'AI-prioritized workflows for your cultivation team',
    icon: ListTodo,
    color: 'accent-violet',
  },
  chart: {
    label: 'Analytics Dashboard',
    description: 'Real-time insights across your entire operation',
    icon: Activity,
    color: 'accent-emerald',
  },
  environment: {
    label: 'Environmental Control',
    description: 'Optimize climate, CO₂, lighting, and fertigation for peak yields',
    icon: Droplets,
    color: 'accent-cyan',
  },
};

type WidgetKey = keyof typeof WIDGET_INFO;

const WIDGET_SEQUENCE: WidgetKey[] = ['rooms', 'plants', 'compliance', 'tasks', 'chart', 'environment'];

interface InfoTooltipProps {
  widgetKey: WidgetKey;
  position: 'top' | 'bottom' | 'left' | 'right';
}

function InfoTooltip({ widgetKey, position }: InfoTooltipProps) {
  const info = WIDGET_INFO[widgetKey];
  const Icon = info.icon;
  
  const positionClasses = {
    top: '-top-2 left-1/2 -translate-x-1/2 -translate-y-full',
    bottom: '-bottom-2 left-1/2 -translate-x-1/2 translate-y-full',
    left: '-left-2 top-1/2 -translate-y-1/2 -translate-x-full',
    right: '-right-2 top-1/2 -translate-y-1/2 translate-x-full',
  };

  const arrowClasses = {
    top: '-bottom-2 left-1/2 -translate-x-1/2 rotate-45 border-r border-b',
    bottom: '-top-2 left-1/2 -translate-x-1/2 rotate-45 border-l border-t',
    left: '-right-2 top-1/2 -translate-y-1/2 rotate-45 border-t border-r',
    right: '-left-2 top-1/2 -translate-y-1/2 rotate-45 border-b border-l',
  };

  return (
    <div className={`absolute ${positionClasses[position]} bg-elevated/95 backdrop-blur-md rounded-xl p-3 border border-${info.color}/30 shadow-xl min-w-[200px] max-w-[240px] z-50 animate-fade-in`}>
      <div className="flex items-center gap-2 mb-1.5">
        <div className={`p-1.5 rounded-lg bg-${info.color}/20`}>
          <Icon className={`w-3.5 h-3.5 text-${info.color}`} />
        </div>
        <span className={`font-semibold text-sm text-${info.color}`}>{info.label}</span>
      </div>
      <p className="text-xs text-muted-foreground leading-relaxed">{info.description}</p>
      <div className={`absolute ${arrowClasses[position]} w-3 h-3 bg-elevated/95 border-${info.color}/30`} />
    </div>
  );
}

export function InteractiveDashboard() {
  const { isMobile } = useMobileBreakpoint();
  
  // On mobile, show the carousel + static problem/solution instead of scroll-hijack
  if (isMobile) {
    return (
      <div className="relative">
        <MobileDashboardCarousel />
        <MobileProblemSolution />
      </div>
    );
  }

  // Desktop: Full scroll-hijack interactive experience
  return <DesktopInteractiveDashboard />;
}

function DesktopInteractiveDashboard() {
  const containerRef = useRef<HTMLDivElement>(null);
  const [activeWidget, setActiveWidget] = useState<WidgetKey | null>(null);
  const [scrollProgress, setScrollProgress] = useState(0);
  const [dashboardFadeOut, setDashboardFadeOut] = useState(0);
  const [headlineFadeIn, setHeadlineFadeIn] = useState(0);
  const [subheadlineFadeIn, setSubheadlineFadeIn] = useState(0);
  const [contentFadeIn, setContentFadeIn] = useState(0);
  const [isFixed, setIsFixed] = useState(false);

  useEffect(() => {
    const handleScroll = () => {
      if (!containerRef.current) return;
      
      const rect = containerRef.current.getBoundingClientRect();
      const containerTop = rect.top;
      const containerHeight = rect.height;
      const windowHeight = window.innerHeight;
      
      const triggerPoint = windowHeight * 0.15;
      const dashboardVisualHeight = windowHeight * 0.82;
      // Split: 80% widgets, 7% headline, 6% subheadline, 7% content
      const totalScrollable = containerHeight - dashboardVisualHeight;
      const widgetAnimationHeight = totalScrollable * 0.80;
      const headlineFadeHeight = totalScrollable * 0.07;
      const subheadlineFadeHeight = totalScrollable * 0.06;
      const contentFadeHeight = totalScrollable * 0.07;
      
      if (containerTop > triggerPoint) {
        // Haven't reached the section yet
        setIsFixed(false);
        setScrollProgress(0);
        setDashboardFadeOut(0);
        setHeadlineFadeIn(0);
        setSubheadlineFadeIn(0);
        setContentFadeIn(0);
        setActiveWidget(null);
      } else if (containerTop + containerHeight - dashboardVisualHeight < 0) {
        // Scrolled past the section - unfix
        setIsFixed(false);
        setScrollProgress(1);
        setDashboardFadeOut(1);
        setHeadlineFadeIn(1);
        setSubheadlineFadeIn(1);
        setContentFadeIn(1);
        setActiveWidget(WIDGET_SEQUENCE[WIDGET_SEQUENCE.length - 1]);
      } else {
        // In the scroll hijack zone - fix the dashboard
        setIsFixed(true);
        
        const scrolled = Math.abs(containerTop - triggerPoint);
        
        // Phase 1: Widget animations (0-80%)
        const widgetProgress = Math.min(1, Math.max(0, scrolled / widgetAnimationHeight));
        setScrollProgress(widgetProgress);
        
        // Phase 2: Headline fade in (80-87%) - dashboard fades out, headline fades in
        if (scrolled > widgetAnimationHeight) {
          const headlineScrolled = scrolled - widgetAnimationHeight;
          const headlineProgress = Math.min(1, Math.max(0, headlineScrolled / headlineFadeHeight));
          setDashboardFadeOut(headlineProgress);
          setHeadlineFadeIn(headlineProgress);
          
          // Phase 3: Subheadline fade in (87-93%)
          if (headlineScrolled > headlineFadeHeight) {
            const subheadlineScrolled = headlineScrolled - headlineFadeHeight;
            const subheadlineProgress = Math.min(1, Math.max(0, subheadlineScrolled / subheadlineFadeHeight));
            setSubheadlineFadeIn(subheadlineProgress);
            
            // Phase 4: Content fade in (93-100%)
            if (subheadlineScrolled > subheadlineFadeHeight) {
              const contentScrolled = subheadlineScrolled - subheadlineFadeHeight;
              const contentProgress = Math.min(1, Math.max(0, contentScrolled / contentFadeHeight));
              setContentFadeIn(contentProgress);
            } else {
              setContentFadeIn(0);
            }
          } else {
            setSubheadlineFadeIn(0);
            setContentFadeIn(0);
          }
        } else {
          setDashboardFadeOut(0);
          setHeadlineFadeIn(0);
          setSubheadlineFadeIn(0);
          setContentFadeIn(0);
        }
        
        // Determine active widget based on widget progress
        // Each widget gets equal scroll time: (1.0 - 0.05) / 6 = ~15.83% each
        if (widgetProgress < 0.05) {
          setActiveWidget(null);
        } else {
          const adjustedProgress = (widgetProgress - 0.05) / 0.95;
          const widgetIndex = Math.floor(adjustedProgress * WIDGET_SEQUENCE.length);
          const clampedIndex = Math.min(Math.max(0, widgetIndex), WIDGET_SEQUENCE.length - 1);
          setActiveWidget(WIDGET_SEQUENCE[clampedIndex]);
        }
      }
    };

    window.addEventListener('scroll', handleScroll, { passive: true });
    handleScroll();
    
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  const isActive = (key: WidgetKey) => activeWidget === key;
  
  const getWidgetStyle = (key: WidgetKey) => {
    const active = isActive(key);
    return {
      transform: active ? 'perspective(800px) translateZ(25px) scale(1.05)' : 'perspective(800px) translateZ(0) scale(1)',
      zIndex: active ? 20 : 1,
      boxShadow: active ? '0 25px 50px -12px rgba(0, 0, 0, 0.5)' : undefined,
    };
  };

  // Dashboard opacity - only fades during phase 3
  const dashboardOpacity = 1 - dashboardFadeOut;
  
  return (
    <div 
      ref={containerRef} 
      className="relative"
      style={{ height: '300vh' }} // Extended scroll space for all phases - gives ~25% viewport per widget
    >
      {/* Backdrop - fades with dashboard */}
      {isFixed && dashboardOpacity > 0 && (
        <div 
          className="fixed inset-0 z-20"
          style={{ 
            background: 'hsl(var(--background))',
            opacity: dashboardOpacity,
          }}
        />
      )}

      {/* Headline - fades in first (fixed during scroll hijack) */}
      {isFixed && headlineFadeIn > 0 && (
        <div 
          className="fixed inset-x-0 z-25 flex flex-col items-center text-center px-4"
          style={{ 
            top: '10vh',
          }}
        >
          <h2 
            className="text-3xl sm:text-4xl lg:text-5xl font-bold mb-6"
            style={{ 
              opacity: headlineFadeIn,
              transform: `translateY(${(1 - headlineFadeIn) * 20}px)`,
            }}
          >
            <span className="text-foreground">Cultivation Has Evolved. </span>
            <span className="text-accent-amber">Your Tools Haven&apos;t.</span>
          </h2>
          {/* Subheadline - fades in after headline */}
          <p 
            className="text-lg text-muted-foreground max-w-2xl"
            style={{ 
              opacity: subheadlineFadeIn,
              transform: `translateY(${(1 - subheadlineFadeIn) * 15}px)`,
            }}
          >
            It&apos;s no longer just about growing—it&apos;s about precision, compliance, traceability, and operational excellence at scale.
          </p>
        </div>
      )}

      {/* Problem/Solution content - fades in after subheadline (fixed during scroll hijack) */}
      {isFixed && contentFadeIn > 0 && (
        <div 
          className="fixed inset-x-0 z-25 px-4 overflow-y-auto"
          style={{ 
            top: '28vh',
            bottom: '5vh',
            opacity: contentFadeIn,
            transform: `translateY(${(1 - contentFadeIn) * 20}px)`,
          }}
        >
          <div className="max-w-7xl mx-auto grid lg:grid-cols-2 gap-8">
            {/* Problems - The Old Way */}
            <div>
              <div className="flex items-center gap-3 mb-6">
                <div className="p-2 rounded-lg bg-accent-rose/10">
                  <X className="h-5 w-5 text-accent-rose" />
                </div>
                <h3 className="text-xl font-semibold">The Old Way</h3>
              </div>
              <div className="space-y-4">
                {PROBLEMS.map((problem) => {
                  const Icon = problem.icon;
                  return (
                    <div 
                      key={problem.title}
                      className="p-5 rounded-xl bg-accent-rose/5 border border-accent-rose/10"
                    >
                      <div className="flex items-start gap-4">
                        <div className="p-2 rounded-lg bg-accent-rose/10 flex-shrink-0">
                          <Icon className="h-5 w-5 text-accent-rose" />
                        </div>
                        <div>
                          <h4 className="font-semibold text-foreground mb-1">{problem.title}</h4>
                          <p className="text-sm text-muted-foreground">{problem.description}</p>
                        </div>
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>

            {/* Solutions - The Harvestry Way */}
            <div>
              <div className="flex items-center gap-3 mb-6">
                <div className="p-2 rounded-lg bg-accent-emerald/10">
                  <Check className="h-5 w-5 text-accent-emerald" />
                </div>
                <h3 className="text-xl font-semibold">The Harvestry Way</h3>
              </div>
              <div className="p-6 rounded-xl bg-accent-emerald/5 border border-accent-emerald/10">
                <ul className="space-y-4">
                  {SOLUTIONS.map((solution, index) => (
                    <li key={index} className="flex items-start gap-3">
                      <Check className="h-5 w-5 text-accent-emerald flex-shrink-0 mt-0.5" />
                      <span className="text-foreground">{solution}</span>
                    </li>
                  ))}
                </ul>
                <div className="mt-6 pt-6 border-t border-accent-emerald/10">
                  <a 
                    href="#features"
                    className="inline-flex items-center gap-2 text-accent-emerald font-medium hover:gap-3 transition-all"
                  >
                    See all features
                    <ArrowRight className="h-4 w-4" />
                  </a>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Statically positioned content at bottom of container - visible after scroll hijack ends */}
      <div 
        className="absolute bottom-0 left-0 right-0 px-4 pb-8"
        style={{ 
          opacity: !isFixed && contentFadeIn === 1 ? 1 : 0,
          pointerEvents: !isFixed && contentFadeIn === 1 ? 'auto' : 'none',
        }}
      >
        <div className="max-w-7xl mx-auto">
          {/* Header */}
          <div className="text-center mb-12">
            <h2 className="text-3xl sm:text-4xl lg:text-5xl font-bold mb-6">
              <span className="text-foreground">Cultivation Has Evolved. </span>
              <span className="text-accent-amber">Your Tools Haven&apos;t.</span>
            </h2>
            <p className="text-lg text-muted-foreground max-w-2xl mx-auto">
              It&apos;s no longer just about growing—it&apos;s about precision, compliance, traceability, and operational excellence at scale.
            </p>
          </div>
          
          {/* Content grid */}
          <div className="grid lg:grid-cols-2 gap-8">
            {/* Problems - The Old Way */}
            <div>
              <div className="flex items-center gap-3 mb-6">
                <div className="p-2 rounded-lg bg-accent-rose/10">
                  <X className="h-5 w-5 text-accent-rose" />
                </div>
                <h3 className="text-xl font-semibold">The Old Way</h3>
              </div>
              <div className="space-y-4">
                {PROBLEMS.map((problem) => {
                  const Icon = problem.icon;
                  return (
                    <div 
                      key={problem.title}
                      className="p-5 rounded-xl bg-accent-rose/5 border border-accent-rose/10"
                    >
                      <div className="flex items-start gap-4">
                        <div className="p-2 rounded-lg bg-accent-rose/10 flex-shrink-0">
                          <Icon className="h-5 w-5 text-accent-rose" />
                        </div>
                        <div>
                          <h4 className="font-semibold text-foreground mb-1">{problem.title}</h4>
                          <p className="text-sm text-muted-foreground">{problem.description}</p>
                        </div>
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>

            {/* Solutions - The Harvestry Way */}
            <div>
              <div className="flex items-center gap-3 mb-6">
                <div className="p-2 rounded-lg bg-accent-emerald/10">
                  <Check className="h-5 w-5 text-accent-emerald" />
                </div>
                <h3 className="text-xl font-semibold">The Harvestry Way</h3>
              </div>
              <div className="p-6 rounded-xl bg-accent-emerald/5 border border-accent-emerald/10">
                <ul className="space-y-4">
                  {SOLUTIONS.map((solution, index) => (
                    <li key={index} className="flex items-start gap-3">
                      <Check className="h-5 w-5 text-accent-emerald flex-shrink-0 mt-0.5" />
                      <span className="text-foreground">{solution}</span>
                    </li>
                  ))}
                </ul>
                <div className="mt-6 pt-6 border-t border-accent-emerald/10">
                  <a 
                    href="#features"
                    className="inline-flex items-center gap-2 text-accent-emerald font-medium hover:gap-3 transition-all"
                  >
                    See all features
                    <ArrowRight className="h-4 w-4" />
                  </a>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>


      
      {/* Fixed Dashboard */}
      <div 
        className="w-full max-w-4xl mx-auto px-4"
        style={{
          position: isFixed ? 'fixed' : 'relative',
          top: isFixed ? '14vh' : 0,
          left: isFixed ? '50%' : 'auto',
          transform: isFixed ? 'translateX(-50%)' : 'none',
          zIndex: 30,
          opacity: dashboardOpacity,
        }}
      >
        {/* Progress indicator */}
        <div 
          className="mb-3 flex items-center justify-center gap-3 text-xs text-muted-foreground transition-opacity duration-300"
          style={{ opacity: scrollProgress > 0.05 ? 1 : 0 }}
        >
          <span className="uppercase tracking-wider font-medium">Exploring Platform</span>
          <div className="w-32 h-1 bg-border rounded-full overflow-hidden">
            <div 
              className="h-full bg-gradient-to-r from-accent-emerald via-accent-cyan to-accent-violet rounded-full transition-all duration-200"
              style={{ width: `${scrollProgress * 100}%` }}
            />
          </div>
          <span className="tabular-nums w-8 font-medium">{Math.round(scrollProgress * 100)}%</span>
        </div>

        {/* Glow backdrop */}
        <div className="absolute -inset-6 bg-gradient-to-r from-accent-emerald/15 via-accent-cyan/15 to-accent-violet/15 blur-3xl opacity-50 rounded-3xl -z-10" />

        <div className="relative">
          {/* Main dashboard container */}
          <div className="relative rounded-2xl overflow-visible border border-border/50 shadow-2xl shadow-black/50 bg-surface">
            {/* Browser chrome */}
            <div className="relative h-10 bg-elevated/80 backdrop-blur flex items-center gap-2 px-4 border-b border-border/50">
              <div className="flex gap-2">
                <div className="w-3 h-3 rounded-full bg-accent-rose/60" />
                <div className="w-3 h-3 rounded-full bg-accent-amber/60" />
                <div className="w-3 h-3 rounded-full bg-accent-emerald/60" />
              </div>
              <div className="flex-1 flex justify-center">
                <div className="px-4 py-1 rounded-md bg-background/50 text-xs text-muted-foreground flex items-center gap-2">
                  <div className="w-3 h-3 rounded-sm bg-accent-emerald/30" />
                  app.harvestry.io
                </div>
              </div>
            </div>
            
{/* Dashboard content */}
                            <div className="aspect-[16/8] relative bg-gradient-to-br from-background via-surface to-elevated overflow-visible">
              <div className="absolute inset-0 p-6">
                <div className="h-full flex gap-4">
                  {/* Sidebar */}
                  <div className="hidden sm:flex flex-col w-16 bg-elevated/50 rounded-lg p-2 gap-2">
                    {[...Array(6)].map((_, i) => (
                      <div key={i} className={`w-full aspect-square rounded-lg ${i === 0 ? 'bg-accent-emerald/20' : 'bg-background/50'}`} />
                    ))}
                  </div>
                  
                  {/* Main content */}
                  <div className="flex-1 space-y-4">
                    {/* Header */}
                    <div className="h-12 bg-elevated/50 rounded-lg flex items-center px-4 gap-4">
                      <div className="w-32 h-4 bg-border/50 rounded-full" />
                      <div className="flex-1" />
                      <div className="w-8 h-8 rounded-full bg-accent-cyan/20" />
                    </div>
                    
                    {/* Stats cards */}
                    <div className="grid grid-cols-4 gap-3">
                      {(['rooms', 'plants', 'compliance', 'tasks'] as const).map((key) => {
                        const colors = { rooms: 'emerald', plants: 'cyan', compliance: 'amber', tasks: 'violet' };
                        const values = { rooms: '24', plants: '847', compliance: '98.2%', tasks: '12' };
                        const labels = { rooms: 'Rooms', plants: 'Active Plants', compliance: 'Compliance', tasks: 'Tasks Due' };
                        const color = colors[key];
                        
                        return (
                          <div 
                            key={key}
                            className={`relative bg-elevated/50 rounded-lg p-3 space-y-2 transition-all duration-500 border ${isActive(key) ? `border-accent-${color}/50 bg-elevated/80` : 'border-transparent'}`}
                            style={getWidgetStyle(key)}
                          >
                            <div className={`w-8 h-8 rounded-lg bg-accent-${color}/20 flex items-center justify-center`}>
                              <div className={`w-4 h-4 rounded bg-accent-${color}/40`} />
                            </div>
                            <div className={`text-lg font-bold text-accent-${color}`}>{values[key]}</div>
                            <div className="text-xs text-muted-foreground">{labels[key]}</div>
                            {isActive(key) && <InfoTooltip widgetKey={key} position="top" />}
                          </div>
                        );
                      })}
                    </div>
                    
                    {/* Chart area */}
                                    <div className="flex-1 grid grid-cols-3 gap-4 min-h-0">
                      {/* Main Chart */}
                                      <div 
                                        className={`relative col-span-2 bg-elevated/50 rounded-lg p-4 transition-all duration-500 border h-full min-h-0 flex flex-col ${isActive('chart') ? 'border-accent-emerald/50 bg-elevated/80' : 'border-transparent'}`}
                                        style={getWidgetStyle('chart')}
                                      >
                                        <div className="w-24 h-3 bg-border/50 rounded mb-3 flex-shrink-0" />
                                        <div className="flex items-end gap-2 flex-1 min-h-0">
                                          {[40, 65, 45, 80, 55, 90, 70, 85, 60, 75, 95, 50].map((h, i) => (
                                            <div key={i} className="flex-1 bg-accent-emerald/30 rounded-t" style={{ height: `${h}%` }} />
                                          ))}
                                        </div>
                                        {isActive('chart') && <InfoTooltip widgetKey="chart" position="top" />}
                                      </div>
                      
                      {/* Side widgets */}
                                      <div className="flex flex-col gap-4 min-h-0 h-full">
                                        <div 
                                          className={`relative bg-elevated/50 rounded-lg p-3 transition-all duration-500 border flex-1 min-h-0 flex flex-col items-center justify-center ${isActive('environment') ? 'border-accent-cyan/50 bg-elevated/80' : 'border-transparent'}`}
                                          style={getWidgetStyle('environment')}
                                        >
                                          <div className="w-12 h-12 rounded-full border-4 border-accent-cyan/20 border-t-accent-cyan animate-spin-slow" />
                                          <div className="mt-2 text-center text-xs text-muted-foreground">VPD Target</div>
                                          {isActive('environment') && <InfoTooltip widgetKey="environment" position="left" />}
                                        </div>
                                        
                                        <div className="bg-elevated/50 rounded-lg p-3 flex-1 min-h-0 flex flex-col justify-center">
                                          <div className="space-y-2">
                                            {[80, 60, 40].map((w, i) => (
                                              <div key={i} className="flex items-center gap-2">
                                                <div className="h-2 bg-accent-amber/30 rounded" style={{ width: `${w}%` }} />
                                                <div className="text-xs text-muted-foreground">{w}%</div>
                                              </div>
                                            ))}
                                          </div>
                                        </div>
                                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
          
          {/* Floating accents */}
          <div className="absolute -top-4 -right-4 w-24 h-24 bg-accent-cyan/10 rounded-2xl blur-2xl animate-pulse-slow" />
          <div className="absolute -bottom-4 -left-4 w-32 h-32 bg-accent-emerald/10 rounded-2xl blur-2xl animate-pulse-slow" />
        </div>

        {/* Trust Badges - always visible with dashboard */}
        <div className="mt-4">
          <p className="text-center text-sm font-medium text-muted-foreground mb-4">
            Enterprise-grade infrastructure trusted by leading cultivators
          </p>
          <div className="grid grid-cols-3 lg:grid-cols-6 gap-2">
            {TRUST_METRICS.map((metric) => {
              const Icon = metric.icon;
              const colors = TRUST_COLORS[metric.color];
              return (
                <div 
                  key={metric.label}
                  className="group flex flex-col items-center text-center p-3 rounded-lg bg-surface/50 border border-border/50 backdrop-blur-sm hover:bg-surface/70 transition-all duration-300"
                >
                  <div className={`p-2 rounded-lg ${colors.bg} mb-2`}>
                    <Icon className={`h-4 w-4 ${colors.text}`} />
                  </div>
                  <div className={`text-lg font-bold ${colors.text}`}>{metric.value}</div>
                  <div className="text-xs text-muted-foreground">{metric.label}</div>
                </div>
              );
            })}
          </div>
        </div>

        {/* Completion message */}
        <div 
          className="mt-4 text-center transition-all duration-500"
          style={{ opacity: scrollProgress > 0.92 ? 1 : 0, transform: scrollProgress > 0.92 ? 'translateY(0)' : 'translateY(10px)' }}
        >
          <p className="text-base font-medium text-foreground">All modules connected. One unified platform.</p>
        </div>
      </div>
    </div>
  );
}

