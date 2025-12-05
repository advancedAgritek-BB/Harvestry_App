'use client';

import { 
  Shield, 
  Server, 
  Clock, 
  Lock,
  Activity,
  Database 
} from 'lucide-react';
import { useScrollAnimation } from './hooks/useScrollAnimation';

const metrics = [
  {
    icon: Activity,
    value: '99.9%',
    label: 'Uptime SLA',
    color: 'emerald',
  },
  {
    icon: Clock,
    value: '<1s',
    label: 'Telemetry Ingest',
    color: 'cyan',
  },
  {
    icon: Shield,
    value: 'SOC 2',
    label: 'Type II Compliant',
    color: 'violet',
  },
  {
    icon: Lock,
    value: 'RLS',
    label: 'Row-Level Security',
    color: 'amber',
  },
  {
    icon: Database,
    value: '5 min',
    label: 'Max Data Loss (RPO)',
    color: 'sky',
  },
  {
    icon: Server,
    value: '30 min',
    label: 'Recovery Time (RTO)',
    color: 'rose',
  },
];

const colorClasses = {
  emerald: { bg: 'bg-accent-emerald/10', text: 'text-accent-emerald', glow: 'group-hover:shadow-accent-emerald/20' },
  cyan: { bg: 'bg-accent-cyan/10', text: 'text-accent-cyan', glow: 'group-hover:shadow-accent-cyan/20' },
  violet: { bg: 'bg-accent-violet/10', text: 'text-accent-violet', glow: 'group-hover:shadow-accent-violet/20' },
  amber: { bg: 'bg-accent-amber/10', text: 'text-accent-amber', glow: 'group-hover:shadow-accent-amber/20' },
  sky: { bg: 'bg-accent-sky/10', text: 'text-accent-sky', glow: 'group-hover:shadow-accent-sky/20' },
  rose: { bg: 'bg-accent-rose/10', text: 'text-accent-rose', glow: 'group-hover:shadow-accent-rose/20' },
};

export function TrustBadges() {
  const { ref, isVisible } = useScrollAnimation({ threshold: 0.2 });

  return (
    <section className="pt-4 pb-16 border-y border-border/50 bg-surface/20 backdrop-blur-sm relative overflow-hidden">
      {/* Subtle animated background */}
      <div className="absolute inset-0 bg-gradient-to-r from-accent-emerald/5 via-transparent to-accent-cyan/5 opacity-50" />
      
      <div 
        ref={ref}
        className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8"
      >
        <p className={`text-center text-sm font-medium text-muted-foreground mb-10 transition-all duration-700 ${isVisible ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}`}>
          Enterprise-grade infrastructure trusted by leading cultivators
        </p>
        
        <div 
          className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-4 sm:gap-6"
          data-visible={isVisible}
          style={{ '--stagger-delay': '100ms' } as React.CSSProperties}
        >
          {metrics.map((metric, index) => {
            const Icon = metric.icon;
            const colors = colorClasses[metric.color as keyof typeof colorClasses];
            
            return (
              <div 
                key={metric.label}
                className={`group relative flex flex-col items-center text-center p-5 rounded-xl bg-surface/50 border border-border/50 backdrop-blur-sm hover:border-border hover:bg-surface/70 transition-all duration-300 cursor-default ${colors.glow} hover:shadow-lg`}
                style={{ transitionDelay: `${index * 50}ms` }}
              >
                {/* Hover glow effect */}
                <div className={`absolute inset-0 rounded-xl ${colors.bg} opacity-0 group-hover:opacity-50 transition-opacity duration-300 blur-xl`} />
                
                {/* Icon */}
                <div className={`relative p-2.5 rounded-lg ${colors.bg} mb-3 group-hover:scale-110 transition-transform duration-300`}>
                  <Icon className={`h-5 w-5 ${colors.text}`} />
                </div>
                
                {/* Value */}
                <div className={`relative text-2xl sm:text-3xl font-bold ${colors.text} group-hover:scale-105 transition-transform duration-300`}>
                  {metric.value}
                </div>
                
                {/* Label */}
                <div className="relative text-xs text-muted-foreground mt-1.5 group-hover:text-foreground/80 transition-colors duration-300">
                  {metric.label}
                </div>

                {/* Bottom accent line */}
                <div className={`absolute bottom-0 left-1/2 -translate-x-1/2 h-0.5 ${colors.text.replace('text-', 'bg-')} rounded-full transition-all duration-300 w-0 group-hover:w-8 opacity-50`} />
              </div>
            );
          })}
        </div>
      </div>
    </section>
  );
}

