import React, { useState } from 'react';
import { cn } from '@/lib/utils';
import Link from 'next/link';
import { 
  AlertOctagon, // E-Stop
  DoorOpen,     // Door
  Waves,        // Tank Level
  Activity,     // EC
  Droplets,     // pH
  Wind,         // CO2
  Clock,        // Runtime
  ChevronDown,
  ChevronUp,
  CheckCircle2,
  AlertTriangle,
  XCircle
} from 'lucide-react';

interface Interlock {
  id: string;
  label: string;
  status: 'ok' | 'warning' | 'tripped';
  icon: React.ElementType;
  message?: string;
  href: string;
}

export function SystemHealthWidget() {
  const [isExpanded, setIsExpanded] = useState(true);

  const interlocks: Interlock[] = [
    { id: 'estop', label: 'E-Stop', status: 'ok', icon: AlertOctagon, href: '/settings/safety/estop' },
    { id: 'door', label: 'Door Safety', status: 'ok', icon: DoorOpen, href: '/settings/safety/interlocks' },
    { id: 'tank', label: 'Tank Levels', status: 'warning', icon: Waves, message: 'Tank D Low Level Warning', href: '/settings/equipment/tanks' },
    { id: 'ec', label: 'EC Bounds', status: 'ok', icon: Activity, href: '/dashboard/recipes/fertigation' },
    { id: 'ph', label: 'pH Bounds', status: 'ok', icon: Droplets, href: '/dashboard/recipes/fertigation' },
    { id: 'co2', label: 'CO₂ Lockout', status: 'ok', icon: Wind, href: '/dashboard/recipes/environment' },
    { id: 'runtime', label: 'Max Runtime', status: 'ok', icon: Clock, href: '/settings/irrigation/safety' },
  ];

  const overallStatus = interlocks.some(i => i.status === 'tripped') 
    ? 'critical' 
    : interlocks.some(i => i.status === 'warning') 
      ? 'warning' 
      : 'ok';

  return (
    <div className="w-full bg-surface/50 border border-border rounded-xl overflow-hidden transition-all duration-300">
      
      {/* Header Bar - Always Visible */}
      <div 
        className="flex items-center justify-between px-4 py-3 cursor-pointer hover:bg-muted/30 transition-colors"
        onClick={() => setIsExpanded(!isExpanded)}
      >
        <div className="flex items-center gap-3">
          <div className="flex items-center gap-2">
            <div className={cn(
              "w-2.5 h-2.5 rounded-full animate-pulse",
              overallStatus === 'ok' ? "bg-emerald-500" : (overallStatus === 'warning' ? "bg-amber-500" : "bg-rose-500")
            )} />
            <h3 className="text-sm font-bold text-foreground uppercase tracking-wide">System Safety Interlocks</h3>
          </div>
          
          {/* Collapsed Summary */}
          {!isExpanded && (
            <div className="flex items-center gap-2 ml-4">
              {interlocks.map(lock => (
                <div 
                  key={lock.id} 
                  className={cn(
                    "w-1.5 h-1.5 rounded-full",
                    lock.status === 'ok' ? "bg-emerald-500/30" : (lock.status === 'warning' ? "bg-amber-500" : "bg-rose-500")
                  )} 
                />
              ))}
            </div>
          )}
        </div>

        <button className="text-muted-foreground hover:text-foreground">
          {isExpanded ? <ChevronUp className="w-4 h-4" /> : <ChevronDown className="w-4 h-4" />}
        </button>
      </div>

      {/* Expanded Details Grid */}
      <div className={cn(
        "grid grid-cols-2 md:grid-cols-4 lg:grid-cols-7 gap-px bg-muted/50 border-t border-border transition-all duration-300",
        isExpanded ? "opacity-100" : "opacity-0 h-0 overflow-hidden"
      )}>
        {interlocks.map((lock) => {
          const Icon = lock.icon;
          const isOk = lock.status === 'ok';
          const isWarn = lock.status === 'warning';
          const isTrip = lock.status === 'tripped';

          return (
            <Link 
              key={lock.id} 
              href={lock.href}
              className={cn(
                "flex flex-col items-center justify-center p-4 gap-2 transition-all duration-200 hover:brightness-110 group relative outline-none focus:ring-2 focus:ring-inset focus:ring-cyan-500/50",
                isOk ? "bg-surface/30 text-muted-foreground hover:bg-muted/50" : (isWarn ? "bg-amber-500/5 text-amber-500 hover:bg-amber-500/10" : "bg-rose-500/10 text-rose-500 hover:bg-rose-500/20 animate-pulse-soft")
              )}
            >
              <div className="relative">
                <Icon className={cn("w-5 h-5 transition-transform group-hover:scale-110", isOk ? "opacity-50" : "opacity-100")} />
                <div className="absolute -bottom-1 -right-1 bg-surface rounded-full p-0.5">
                  {isOk && <CheckCircle2 className="w-3 h-3 text-emerald-500" />}
                  {isWarn && <AlertTriangle className="w-3 h-3 text-amber-500" />}
                  {isTrip && <XCircle className="w-3 h-3 text-rose-500" />}
                </div>
              </div>
              
              <div className="text-center">
                <div className="text-[10px] font-bold uppercase tracking-wider opacity-80 group-hover:text-foreground transition-colors">{lock.label}</div>
                {lock.message && (
                  <div className="text-[9px] mt-1 font-medium opacity-100 bg-background/20 px-1.5 py-0.5 rounded">
                    {lock.message}
                  </div>
                )}
              </div>
            </Link>
          );
        })}
      </div>
    </div>
  );
}
