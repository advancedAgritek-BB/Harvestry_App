'use client';

import React from 'react';
import { AlertTriangle, AlertCircle, Info, X } from 'lucide-react';
import { cn } from '@/lib/utils';
import { useAlertsStore } from '@/stores/alertsStore';

const SEVERITY_CONFIG = {
  critical: { 
    icon: AlertCircle, 
    color: 'text-rose-400', 
    iconBg: 'bg-rose-500/15',
    border: 'border-rose-500/20',
    bg: 'bg-gradient-to-r from-rose-500/10 to-transparent'
  },
  warning: { 
    icon: AlertTriangle, 
    color: 'text-amber-400', 
    iconBg: 'bg-amber-500/15',
    border: 'border-amber-500/20',
    bg: 'bg-gradient-to-r from-amber-500/10 to-transparent'
  },
  info: { 
    icon: Info, 
    color: 'text-sky-400', 
    iconBg: 'bg-sky-500/15',
    border: 'border-sky-500/20',
    bg: 'bg-gradient-to-r from-sky-500/10 to-transparent'
  },
};

export function AlertsWidget() {
  const { alerts, dismissAlert } = useAlertsStore();
  
  // Show active (not dismissed) alerts, sorted by timestamp desc
  const activeAlerts = alerts
    .filter(a => !a.dismissed)
    .sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime())
    .slice(0, 5); // Limit to top 5

  if (activeAlerts.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center h-full min-h-[100px] text-muted-foreground/50">
        <Info className="w-8 h-8 mb-2 opacity-50" />
        <span className="text-sm">No active alerts</span>
      </div>
    );
  }

  return (
    <div className="flex flex-col space-y-2">
      {activeAlerts.map((alert) => {
        const config = SEVERITY_CONFIG[alert.severity];
        const Icon = config.icon;

        return (
          <div
            key={alert.id}
            className={cn(
              'group relative flex items-center gap-2.5 p-2.5 rounded-xl border transition-all duration-200 hover:scale-[1.01]',
              config.bg,
              config.border
            )}
          >
            <div className={cn('p-1.5 rounded-lg shrink-0', config.iconBg)}>
              <Icon className={cn('w-3 h-3', config.color, alert.severity === 'critical' && 'animate-pulse')} />
            </div>
            
            <div className="flex-1 min-w-0">
              <h4 className="text-xs font-medium text-foreground leading-tight truncate">
                {alert.title}
              </h4>
              <div className="flex items-center gap-1.5 text-[10px] text-muted-foreground">
                <span>{alert.source}</span>
                <span className="text-muted-foreground/50">•</span>
                <span className="font-mono text-muted-foreground/70">
                  {new Date(alert.timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                </span>
              </div>
            </div>

            <button 
              title="Dismiss"
              onClick={() => dismissAlert(alert.id)}
              className="opacity-0 group-hover:opacity-100 p-1.5 hover:bg-foreground/10 rounded-lg text-muted-foreground hover:text-foreground transition-all"
            >
              <X className="w-3 h-3" />
            </button>
          </div>
        );
      })}
    </div>
  );
}
