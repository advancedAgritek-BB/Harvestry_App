import React from 'react';
import { cn } from '@/lib/utils';
import Link from 'next/link';
import { Calendar, ChevronRight } from 'lucide-react';

interface ScheduledEvent {
  id: string;
  time: string; // "08:00"
  relativeTime: string; // "In 45m"
  programName: string;
  target: string; // "Tank A -> Zones A,B"
  type: 'program' | 'maintenance';
}

export function UpcomingScheduleWidget() {
  const events: ScheduledEvent[] = [
    { id: '1', time: '08:00', relativeTime: 'In 45m', programName: 'P2 - Maintenance', target: 'Tank B → Zones D,E', type: 'program' },
    { id: '2', time: '09:30', relativeTime: 'In 2h 15m', programName: 'P1 - Ramp', target: 'Tank A → Zone A', type: 'program' },
    { id: '3', time: '10:00', relativeTime: 'In 2h 45m', programName: 'P3 - Dryback', target: 'Tank C → All Zones', type: 'program' },
    { id: '4', time: '12:00', relativeTime: 'In 4h 45m', programName: 'Filter Backwash', target: 'System Maintenance', type: 'maintenance' },
  ];

  return (
    <div className="h-full min-h-[160px] bg-surface/50 border border-border rounded-xl p-5 flex flex-col">
      {/* Header */}
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center gap-2">
           <Calendar className="w-4 h-4 text-cyan-400" />
           <h3 className="text-sm font-bold text-foreground uppercase tracking-wide">Upcoming</h3>
        </div>
        <Link 
          href="/dashboard/irrigation/schedules"
          className="text-xs text-cyan-400 hover:text-cyan-300 font-medium flex items-center gap-1 transition-colors"
        >
          Full Schedule <ChevronRight className="w-3 h-3" />
        </Link>
      </div>

      {/* List */}
      <div className="flex-1 space-y-2 overflow-y-auto custom-scrollbar pr-1">
        {events.map((event, idx) => (
          <div 
            key={event.id}
            className="flex items-center gap-3 p-2 rounded-lg hover:bg-muted/50 transition-colors group"
          >
            {/* Time Column */}
            <div className="flex flex-col items-end min-w-[60px]">
              <span className="text-sm font-mono font-bold text-foreground">{event.time}</span>
              <span className="text-[10px] text-muted-foreground">{event.relativeTime}</span>
            </div>

            {/* Timeline Line */}
            <div className="h-8 w-px bg-border relative">
               <div className={cn(
                 "absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-2 h-2 rounded-full border-2",
                 idx === 0 ? "border-cyan-500 bg-cyan-950 animate-pulse" : "border-border bg-surface"
               )} />
            </div>

            {/* Content */}
            <div className="flex-1 min-w-0">
               <div className="text-sm font-medium text-foreground/70 truncate group-hover:text-cyan-400 transition-colors">
                 {event.programName}
               </div>
               <div className="text-xs text-muted-foreground truncate flex items-center gap-1">
                 {event.type === 'maintenance' ? (
                   <span className="text-amber-500/80">System Op</span>
                 ) : (
                   <span>{event.target}</span>
                 )}
               </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
