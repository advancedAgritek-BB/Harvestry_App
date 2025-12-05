import React from 'react';
import { cn } from '@/lib/utils';

interface RoomSummary {
  id: string;
  name: string;
  temp: number;
  rh: number;
  ec: number;
  status: 'healthy' | 'warning' | 'critical';
}

const MOCK_ROOMS: RoomSummary[] = [
  { id: 'f1', name: 'Flower • F1', temp: 76.6, rh: 55, ec: 2.3, status: 'warning' },
  { id: 'f2', name: 'Flower • F2', temp: 77.2, rh: 53, ec: 2.4, status: 'healthy' },
  { id: 'v1', name: 'Veg', temp: 73.8, rh: 60, ec: 1.8, status: 'warning' },
  { id: 'd1', name: 'Dry', temp: 65.3, rh: 50, ec: 0, status: 'healthy' },
  { id: 'f3', name: 'Flower • F3', temp: 76.8, rh: 54, ec: 2.4, status: 'healthy' },
  { id: 'v2', name: 'Veg • V2', temp: 74.1, rh: 58, ec: 1.9, status: 'healthy' },
];

export function RoomsStatusWidget() {
  return (
    // Increased grid columns for wider screens to prevent stretching
    <div className="w-full grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-6 2xl:grid-cols-8 gap-3">
      {MOCK_ROOMS.map((room) => (
        <button
          key={room.id}
          className="group flex flex-col p-3 bg-surface/30 border border-border rounded-xl hover:bg-muted/50 hover:border-border/80 transition-all text-left active:scale-[0.98] h-full"
          onClick={() => console.log('Navigate to room', room.id)}
        >
          <div className="flex items-center justify-between w-full mb-3">
            <span className="text-sm font-bold text-foreground group-hover:text-cyan-400 transition-colors line-clamp-1">
              {room.name}
            </span>
             {/* Status Dot */}
             <div className={cn("w-2 h-2 flex-shrink-0 rounded-full ml-2", 
                room.status === 'healthy' && "bg-emerald-500 shadow-[0_0_6px_rgba(16,185,129,0.4)]",
                room.status === 'warning' && "bg-amber-500 shadow-[0_0_6px_rgba(245,158,11,0.4)]",
                room.status === 'critical' && "bg-rose-500 shadow-[0_0_6px_rgba(244,63,94,0.4)]"
             )} />
          </div>

          {/* Compact Metric Grid: Keeps labels and values close together */}
          <div className="grid grid-cols-2 gap-x-2 gap-y-2 text-muted-foreground">
            <div className="flex flex-col">
               <span className="text-[10px] font-semibold uppercase tracking-wider opacity-80">Temp</span>
               <span className="text-foreground font-bold font-mono text-lg">{room.temp}°F</span>
            </div>
            <div className="flex flex-col">
               <span className="text-[10px] font-semibold uppercase tracking-wider opacity-80">RH</span>
               <span className="text-foreground font-bold font-mono text-lg">{room.rh}%</span>
            </div>
            {room.ec > 0 && (
              <div className="flex flex-col">
                 <span className="text-[10px] font-semibold uppercase tracking-wider opacity-80">EC</span>
                 <span className="text-foreground font-bold font-mono text-lg">{room.ec}</span>
              </div>
            )}
          </div>
        </button>
      ))}
    </div>
  );
}
