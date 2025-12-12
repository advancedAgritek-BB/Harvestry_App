'use client';

import { useRef, useEffect } from 'react';
import { 
  Thermometer, 
  Droplets, 
  Shield, 
  Leaf, 
  CheckSquare, 
  Package,
  Factory,
  FileCheck,
  Wallet,
  Bell,
  Brain,
  Gauge,
  Users,
  CalendarDays,
  Dna,
  Scale,
  Truck,
  MapPin,
  Network,
  Wrench,
  Cpu
} from 'lucide-react';
import { AnimatedSection, StaggerContainer } from './AnimatedSection';
import { useScrollAnimation } from './hooks/useScrollAnimation';

const features = [
  {
    icon: Thermometer,
    title: 'Real-Time Environmental Monitoring',
    description: 'Air, canopy, substrate sensors with VPD, CO₂, PPFD, DLI tracking. Smart rollups with conformance views.',
    color: 'cyan',
  },
  {
    icon: Droplets,
    title: 'Precision Irrigation & Fertigation',
    description: 'Flexible programs with cycle/soak, time-based, or volume triggers. Recipe management with EC/pH targeting.',
    color: 'sky',
  },
  {
    icon: Shield,
    title: 'Safety-First Automation',
    description: 'Interlocks, e-stop integration, promotion checklists. One-click revert with complete audit trails.',
    color: 'amber',
  },
  {
    icon: Leaf,
    title: 'Complete Batch Lifecycle',
    description: 'Clone → Veg → Flower → Harvest → Cure with rule-based transitions and blueprint matrix automation.',
    color: 'emerald',
  },
  {
    icon: CheckSquare,
    title: 'Intelligent Task Management',
    description: 'Dependency chains, SLA tracking, training gating, multi-person approvals. Slack integration built-in.',
    color: 'violet',
  },
  {
    icon: Package,
    title: 'Unified Inventory & Labeling',
    description: 'Universal locations, lot tracking, mobile scanning, lineage graphs. GS1/UDI labels with jurisdiction-specific rules.',
    color: 'cyan',
  },
  {
    icon: Factory,
    title: 'Processing & Manufacturing',
    description: 'Process templates, WIP tracking, yield analysis, labor logging. Complete waste events with lineage.',
    color: 'amber',
  },
  {
    icon: FileCheck,
    title: 'Compliance That Works',
    description: 'METRC & BioTrack sync with automatic retry. COA gating, destruction controls, regulator-ready exports.',
    color: 'emerald',
  },
  {
    icon: Wallet,
    title: 'Financial Integration',
    description: 'QuickBooks Online sync for POs, Bills, Invoices. GL summary JEs with WIP→FG and COGS tracking.',
    color: 'sky',
  },
  {
    icon: Dna,
    title: 'Genetics & Strain Library',
    description: 'Strain profiles with breeder info, phenotype tracking, per-strain crop steering targets, and harvest performance history.',
    color: 'violet',
  },
  {
    icon: Scale,
    title: 'Harvest Workflow & Scale Integration',
    description: 'Industrial scale integration (OHAUS & more), real-time weight capture, tare calibration, and tracked adjustments with dual approval.',
    color: 'amber',
  },
  {
    icon: Truck,
    title: 'Sales & Transfers',
    description: 'Full order lifecycle from quote to invoice. Manifest generation, transfer verification with signature capture, and RMA workflow.',
    color: 'sky',
  },
  {
    icon: MapPin,
    title: 'Spatial Management',
    description: 'Model your facility: Site → Building → Room → Zone → Rack. Capacity planning, emitter specs per zone, and optimal pick paths.',
    color: 'cyan',
  },
  {
    icon: Brain,
    title: 'Harvey — Cultivation Copilot',
    description: "The industry's most advanced cultivation AI. Harvey learns your operation, spots problems before you do, and delivers expert advice—all trained on your data in a private, secure environment.",
    color: 'violet',
  },
  {
    icon: Gauge,
    title: 'Sustainability & ESG',
    description: 'Track WUE, NUE, kWh/gram, CO₂ intensity. Automated compliance reports for stakeholders.',
    color: 'emerald',
  },
  {
    icon: Bell,
    title: 'Notifications & Escalations',
    description: 'In-app, email, Slack for all severities. SMS for critical alerts. Role-based subscriptions.',
    color: 'rose',
  },
  {
    icon: Users,
    title: 'Labor & Workforce Management',
    description: 'Shift scheduling, timecard approvals, certification tracking. Real-time coverage metrics and productivity dashboards.',
    color: 'sky',
  },
  {
    icon: CalendarDays,
    title: 'Visual Production Planning',
    description: 'Drag-and-drop Gantt charts for batch scheduling. What-if modeling and capacity optimization built-in.',
    color: 'cyan',
  },
];

const colorClasses = {
  cyan: { bg: 'bg-accent-cyan/10', text: 'text-accent-cyan', glow: 'group-hover:shadow-accent-cyan/20' },
  sky: { bg: 'bg-accent-sky/10', text: 'text-accent-sky', glow: 'group-hover:shadow-accent-sky/20' },
  amber: { bg: 'bg-accent-amber/10', text: 'text-accent-amber', glow: 'group-hover:shadow-accent-amber/20' },
  emerald: { bg: 'bg-accent-emerald/10', text: 'text-accent-emerald', glow: 'group-hover:shadow-accent-emerald/20' },
  violet: { bg: 'bg-accent-violet/10', text: 'text-accent-violet', glow: 'group-hover:shadow-accent-violet/20' },
  rose: { bg: 'bg-accent-rose/10', text: 'text-accent-rose', glow: 'group-hover:shadow-accent-rose/20' },
};

function FeatureCard({ feature, index }: { feature: typeof features[0]; index: number }) {
  const cardRef = useRef<HTMLDivElement>(null);
  const colors = colorClasses[feature.color as keyof typeof colorClasses];

  useEffect(() => {
    const card = cardRef.current;
    if (!card) return;

    const handleMouseMove = (e: MouseEvent) => {
      const rect = card.getBoundingClientRect();
      const x = ((e.clientX - rect.left) / rect.width) * 100;
      const y = ((e.clientY - rect.top) / rect.height) * 100;
      card.style.setProperty('--mouse-x', `${x}%`);
      card.style.setProperty('--mouse-y', `${y}%`);
    };

    card.addEventListener('mousemove', handleMouseMove);
    return () => card.removeEventListener('mousemove', handleMouseMove);
  }, []);

  const Icon = feature.icon;

  return (
    <div 
      ref={cardRef}
      className={`feature-card card-shine group relative p-6 rounded-2xl bg-surface/50 border border-border/50 backdrop-blur-sm cursor-default ${colors.glow}`}
      style={{ 
        transitionDelay: `${index * 50}ms`,
      }}
    >
      {/* Hover gradient overlay */}
      <div className="absolute inset-0 rounded-2xl bg-gradient-to-br from-transparent via-transparent to-transparent group-hover:from-accent-emerald/5 group-hover:via-transparent group-hover:to-accent-cyan/5 transition-all duration-500 pointer-events-none" />
      
      {/* Icon container with glow effect */}
      <div className={`relative inline-flex p-3.5 rounded-xl ${colors.bg} mb-5 transition-all duration-300 group-hover:scale-110`}>
        <div className={`absolute inset-0 rounded-xl ${colors.bg} blur-lg opacity-0 group-hover:opacity-100 transition-opacity duration-300`} />
        <Icon className={`relative h-6 w-6 ${colors.text}`} />
      </div>
      
      {/* Content */}
      <h3 className={`text-lg font-semibold mb-2.5 transition-colors duration-300 group-hover:${colors.text}`}>
        {feature.title}
      </h3>
      <p className="text-sm text-muted-foreground leading-relaxed group-hover:text-muted-foreground/80 transition-colors duration-300">
        {feature.description}
      </p>
      
      {/* Bottom highlight line */}
      <div className={`absolute bottom-0 left-6 right-6 h-px bg-gradient-to-r from-transparent ${colors.text.replace('text-', 'via-')}/30 to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300`} />
    </div>
  );
}

export function Features() {
  const { ref: headerRef, isVisible: headerVisible } = useScrollAnimation({ threshold: 0.2 });
  const { ref: gridRef, isVisible: gridVisible } = useScrollAnimation({ threshold: 0.1 });

  return (
    <section id="features" className="py-28 relative overflow-hidden">
      {/* Animated background elements */}
      <div className="absolute inset-0 bg-surface/20" />
      <div className="absolute top-0 left-1/4 w-96 h-96 bg-accent-emerald/5 rounded-full blur-3xl" />
      <div className="absolute bottom-0 right-1/4 w-96 h-96 bg-accent-cyan/5 rounded-full blur-3xl" />
      
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
            Comprehensive Feature Set
          </div>
          <h2 className="text-3xl sm:text-4xl lg:text-5xl font-bold mb-5">
            Everything You Need to{' '}
            <span className="text-accent-emerald">
              Run Your Facility
            </span>
          </h2>
          <p className="text-lg text-muted-foreground max-w-2xl mx-auto">
            From seed to sale, from sensors to spreadsheets—one platform for complete operational control.
          </p>
        </div>

        {/* Features Grid with stagger animation */}
        <div 
          ref={gridRef}
          className="grid sm:grid-cols-2 lg:grid-cols-3 gap-5"
          data-visible={gridVisible}
          style={{ '--stagger-delay': '80ms' } as React.CSSProperties}
        >
          {features.map((feature, index) => (
            <FeatureCard key={feature.title} feature={feature} index={index} />
          ))}
        </div>

        {/* Advanced Capabilities Section */}
        <div className="mt-20">
          <div className="text-center mb-10">
            <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-accent-violet/10 border border-accent-violet/20 text-accent-violet text-sm font-medium mb-4">
              <Cpu className="h-4 w-4" />
              Advanced Capabilities
            </div>
            <h3 className="text-2xl sm:text-3xl font-bold mb-3">
              Next-Generation{' '}
              <span className="text-accent-violet">Intelligence & Automation</span>
            </h3>
            <p className="text-muted-foreground max-w-xl mx-auto">
              For operators ready to push the boundaries of what cultivation software can do.
            </p>
          </div>

          <div className="grid sm:grid-cols-3 gap-5">
            {/* Graph Intelligence */}
            <div className="group p-6 rounded-2xl bg-gradient-to-br from-accent-violet/5 to-accent-cyan/5 border border-accent-violet/20 hover:border-accent-violet/40 transition-all duration-300">
              <div className="inline-flex p-3 rounded-xl bg-accent-violet/10 mb-4 group-hover:scale-110 transition-transform duration-300">
                <Network className="h-6 w-6 text-accent-violet" />
              </div>
              <h4 className="text-lg font-semibold mb-2">Graph Intelligence</h4>
              <p className="text-sm text-muted-foreground leading-relaxed">
                Graph ML engine detects traceability anomalies, irrigation response issues, and unusual patterns. Natural language knowledge graph queries.
              </p>
            </div>

            {/* Predictive Maintenance */}
            <div className="group p-6 rounded-2xl bg-gradient-to-br from-accent-amber/5 to-accent-rose/5 border border-accent-amber/20 hover:border-accent-amber/40 transition-all duration-300">
              <div className="inline-flex p-3 rounded-xl bg-accent-amber/10 mb-4 group-hover:scale-110 transition-transform duration-300">
                <Wrench className="h-6 w-6 text-accent-amber" />
              </div>
              <h4 className="text-lg font-semibold mb-2">Predictive Maintenance</h4>
              <p className="text-sm text-muted-foreground leading-relaxed">
                Equipment health scoring for pumps, valves, sensors. Drift detection, failure probability, and automatic work order generation.
              </p>
            </div>

            {/* Autosteer MPC */}
            <div className="group p-6 rounded-2xl bg-gradient-to-br from-accent-emerald/5 to-accent-cyan/5 border border-accent-emerald/20 hover:border-accent-emerald/40 transition-all duration-300">
              <div className="inline-flex p-3 rounded-xl bg-accent-emerald/10 mb-4 group-hover:scale-110 transition-transform duration-300">
                <Cpu className="h-6 w-6 text-accent-emerald" />
              </div>
              <h4 className="text-lg font-semibold mb-2">Autosteer MPC</h4>
              <p className="text-sm text-muted-foreground leading-relaxed">
                Model Predictive Control for joint optimization of irrigation, climate, and lighting. A/B block testing with full explainability.
              </p>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}


