'use client';

import React from 'react';
import { cn } from '@/lib/utils';

// Placeholder for SciChart or Recharts implementation
// Since we don't have the libraries installed yet, we'll build a visually representative CSS-only chart
// to maintain the "Premium" look without breaking the build.

const MOCK_DATA = [
  { label: 'Mon', val1: 40, val2: 20 },
  { label: 'Tue', val1: 30, val2: 40 },
  { label: 'Wed', val1: 55, val2: 30 },
  { label: 'Thu', val1: 45, val2: 50 },
  { label: 'Fri', val1: 70, val2: 45 },
  { label: 'Sat', val1: 65, val2: 60 },
  { label: 'Sun', val1: 85, val2: 55 },
];

export function PerformanceChartWidget() {
  const maxVal = 100;

  return (
    <div className="flex flex-col h-full w-full p-2">
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center gap-4">
           <div className="flex items-center gap-2">
              <div className="w-3 h-3 rounded-full bg-cyan-500"></div>
              <span className="text-xs text-muted-foreground">Actual Yield</span>
           </div>
           <div className="flex items-center gap-2">
              <div className="w-3 h-3 rounded-full bg-violet-500/50"></div>
              <span className="text-xs text-muted-foreground">Target</span>
           </div>
        </div>
        <select className="bg-background border border-border rounded-md text-xs px-2 py-1 outline-none focus:ring-1 focus:ring-cyan-500 text-muted-foreground">
           <option>Last 7 Days</option>
           <option>Last 30 Days</option>
           <option>This Run</option>
        </select>
      </div>

      <div className="flex-1 flex items-end justify-between gap-2 px-2 relative min-h-[150px]">
        {/* Y-Axis Grid Lines */}
        <div className="absolute inset-0 flex flex-col justify-between pointer-events-none">
           {[100, 75, 50, 25, 0].map((tick) => (
             <div key={tick} className="w-full border-t border-border/20 h-0 relative">
               <span className="absolute -left-8 -top-2 text-[10px] text-muted-foreground w-6 text-right">{tick}%</span>
             </div>
           ))}
        </div>

        {/* Bars */}
        {MOCK_DATA.map((d, i) => (
          <div key={i} className="flex-1 h-full flex items-end justify-center gap-1 group relative z-10">
             {/* Tooltip */}
             <div className="absolute -top-10 left-1/2 -translate-x-1/2 bg-popover text-popover-foreground text-xs py-1 px-2 rounded shadow-lg opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none whitespace-nowrap z-20 border border-border">
                {d.label}: {d.val1}%
             </div>

             {/* Bar 1 */}
             <div 
               className="w-full max-w-[24px] bg-cyan-500/80 hover:bg-cyan-400 rounded-t-sm transition-all duration-300 relative group-hover:shadow-[0_0_15px_rgba(6,182,212,0.3)]"
               style={{ height: `${(d.val1 / maxVal) * 100}%` }}
             />
             {/* Bar 2 (Target) */}
             <div 
               className="w-full max-w-[24px] bg-violet-500/30 hover:bg-violet-500/50 rounded-t-sm transition-all duration-300"
               style={{ height: `${(d.val2 / maxVal) * 100}%` }}
             />
          </div>
        ))}
      </div>
      
      {/* X-Axis Labels */}
      <div className="flex justify-between px-2 mt-2 border-t border-border">
        {MOCK_DATA.map((d, i) => (
          <span key={i} className="flex-1 text-center text-[10px] text-muted-foreground pt-2">
            {d.label}
          </span>
        ))}
      </div>
    </div>
  );
}

