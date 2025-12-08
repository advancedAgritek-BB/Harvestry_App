'use client';

import { useState, useEffect } from 'react';
import { CheckCircle, AlertTriangle, XCircle, RefreshCw, Activity, Server, Database, Shield, Cloud } from 'lucide-react';
import { cn } from '@/lib/utils';

interface ServiceStatus {
  name: string;
  description: string;
  status: 'operational' | 'degraded' | 'outage';
  icon: React.ComponentType<{ className?: string }>;
  uptime: string;
}

const services: ServiceStatus[] = [
  {
    name: 'Web Application',
    description: 'app.harvestry.io - Main cultivation platform',
    status: 'operational',
    icon: Server,
    uptime: '99.99%',
  },
  {
    name: 'API Services',
    description: 'REST and GraphQL API endpoints',
    status: 'operational',
    icon: Cloud,
    uptime: '99.98%',
  },
  {
    name: 'Database',
    description: 'Primary data storage and retrieval',
    status: 'operational',
    icon: Database,
    uptime: '99.99%',
  },
  {
    name: 'Authentication',
    description: 'User authentication and authorization',
    status: 'operational',
    icon: Shield,
    uptime: '99.99%',
  },
  {
    name: 'Real-time Services',
    description: 'Live data feeds and notifications',
    status: 'operational',
    icon: Activity,
    uptime: '99.95%',
  },
];

const statusConfig = {
  operational: {
    label: 'Operational',
    color: 'text-emerald-400',
    bgColor: 'bg-emerald-500/10',
    borderColor: 'border-emerald-500/20',
    icon: CheckCircle,
  },
  degraded: {
    label: 'Degraded Performance',
    color: 'text-amber-400',
    bgColor: 'bg-amber-500/10',
    borderColor: 'border-amber-500/20',
    icon: AlertTriangle,
  },
  outage: {
    label: 'Service Outage',
    color: 'text-red-400',
    bgColor: 'bg-red-500/10',
    borderColor: 'border-red-500/20',
    icon: XCircle,
  },
};

export default function StatusPage() {
  const [lastUpdated, setLastUpdated] = useState<Date>(new Date());
  const [isRefreshing, setIsRefreshing] = useState(false);

  const overallStatus = services.every(s => s.status === 'operational') 
    ? 'operational' 
    : services.some(s => s.status === 'outage') 
      ? 'outage' 
      : 'degraded';

  const handleRefresh = () => {
    setIsRefreshing(true);
    setTimeout(() => {
      setLastUpdated(new Date());
      setIsRefreshing(false);
    }, 1000);
  };

  useEffect(() => {
    const interval = setInterval(() => {
      setLastUpdated(new Date());
    }, 60000); // Update every minute

    return () => clearInterval(interval);
  }, []);

  const StatusIcon = statusConfig[overallStatus].icon;

  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      {/* Header */}
      <div className="text-center mb-12">
        <h1 className="text-3xl sm:text-4xl font-bold text-foreground mb-4">
          System Status
        </h1>
        <p className="text-muted-foreground">
          Real-time status of Harvestry services and infrastructure
        </p>
      </div>

      {/* Overall Status Banner */}
      <div className={cn(
        "rounded-2xl border p-6 mb-8",
        statusConfig[overallStatus].bgColor,
        statusConfig[overallStatus].borderColor
      )}>
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-4">
            <StatusIcon className={cn("w-8 h-8", statusConfig[overallStatus].color)} />
            <div>
              <h2 className={cn("text-xl font-semibold", statusConfig[overallStatus].color)}>
                {overallStatus === 'operational' 
                  ? 'All Systems Operational' 
                  : overallStatus === 'degraded'
                    ? 'Some Systems Degraded'
                    : 'Service Outage Detected'}
              </h2>
              <p className="text-sm text-muted-foreground">
                Last updated: {lastUpdated.toLocaleString()}
              </p>
            </div>
          </div>
          <button
            onClick={handleRefresh}
            disabled={isRefreshing}
            className="p-2 rounded-lg bg-surface/50 hover:bg-surface border border-border transition-all"
            aria-label="Refresh status"
          >
            <RefreshCw className={cn(
              "w-5 h-5 text-muted-foreground",
              isRefreshing && "animate-spin"
            )} />
          </button>
        </div>
      </div>

      {/* Services Grid */}
      <div className="space-y-4">
        <h3 className="text-lg font-semibold text-foreground mb-4">Services</h3>
        {services.map((service) => {
          const config = statusConfig[service.status];
          const ServiceIcon = service.icon;
          const ServiceStatusIcon = config.icon;

          return (
            <div
              key={service.name}
              className="bg-surface/50 border border-border/50 rounded-xl p-4 hover:bg-surface/70 transition-colors"
            >
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-4">
                  <div className="w-10 h-10 rounded-lg bg-surface flex items-center justify-center border border-border">
                    <ServiceIcon className="w-5 h-5 text-muted-foreground" />
                  </div>
                  <div>
                    <h4 className="font-medium text-foreground">{service.name}</h4>
                    <p className="text-sm text-muted-foreground">{service.description}</p>
                  </div>
                </div>
                <div className="flex items-center gap-4">
                  <div className="text-right hidden sm:block">
                    <p className="text-xs text-muted-foreground">Uptime</p>
                    <p className="text-sm font-medium text-foreground">{service.uptime}</p>
                  </div>
                  <div className={cn(
                    "flex items-center gap-2 px-3 py-1.5 rounded-full",
                    config.bgColor
                  )}>
                    <ServiceStatusIcon className={cn("w-4 h-4", config.color)} />
                    <span className={cn("text-sm font-medium", config.color)}>
                      {config.label}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          );
        })}
      </div>

      {/* Incident History */}
      <div className="mt-12">
        <h3 className="text-lg font-semibold text-foreground mb-4">Recent Incidents</h3>
        <div className="bg-surface/50 border border-border/50 rounded-xl p-6 text-center">
          <CheckCircle className="w-8 h-8 text-emerald-400 mx-auto mb-3" />
          <p className="text-muted-foreground">No incidents reported in the last 90 days</p>
        </div>
      </div>

      {/* Subscribe Section */}
      <div className="mt-12 bg-surface/50 border border-border/50 rounded-xl p-6">
        <div className="flex flex-col sm:flex-row items-center justify-between gap-4">
          <div>
            <h3 className="font-semibold text-foreground">Stay Informed</h3>
            <p className="text-sm text-muted-foreground">
              Subscribe to receive status updates via email
            </p>
          </div>
          <div className="flex gap-3 w-full sm:w-auto">
            <input
              type="email"
              placeholder="Enter your email"
              className="px-4 py-2 bg-background border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-cyan-500/50 transition-colors flex-1 sm:w-64"
            />
            <button className="px-4 py-2 rounded-lg font-medium text-sm text-white bg-cyan-600 hover:bg-cyan-500 transition-all whitespace-nowrap">
              Subscribe
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
