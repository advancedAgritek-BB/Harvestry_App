'use client';

import React, { useState } from 'react';
import { cn } from '@/lib/utils';
import { Layers, X } from 'lucide-react';

// Types
type MetricType = 'Temp' | 'RH' | 'VWC' | 'EC' | 'PPFD';

// Mock Heatmap Data (6x3 Grid for Zone Visualization)
const GRID_ROWS = 3;
const GRID_COLS = 6; // 18 Sensors total
// Deterministic mock data generation to prevent hydration mismatches
const MOCK_SENSOR_DATA = Array.from({ length: GRID_ROWS * GRID_COLS }).map((_, i) => ({
  id: i + 1,
  values: {
    Temp: 73 + (Math.abs(Math.sin(i + 1)) * 5),
    RH: 50 + (Math.abs(Math.cos(i + 1)) * 10),
    VWC: 40 + (Math.abs(Math.sin(i * 2)) * 15),
    EC: 2.0 + (Math.abs(Math.cos(i * 2)) * 0.8),
    PPFD: 850 + (Math.abs(Math.sin(i * 3)) * 100),
  },
  hasAlert: (i % 13) === 0 // Deterministic alert pattern
}));

// Color Scales
const getColor = (metric: MetricType, value: number): string => {
  // Simplified scale logic
  if (metric === 'Temp') {
    if (value > 78) return 'bg-rose-500 text-foreground';
    if (value > 76) return 'bg-amber-400 text-black';
    return 'bg-emerald-500 text-foreground'; // Good
  }
  // Default
  return 'bg-emerald-500/80 text-foreground';
};

export function ZoneHeatmapWidget() {
  const [activeMetric, setActiveMetric] = useState<MetricType>('Temp');
  const [selectedSensor, setSelectedSensor] = useState<number | null>(null);

  return (
    <div className="w-full h-full min-h-[300px] bg-surface/50 border border-border rounded-xl p-4 flex flex-col relative">
      
      {/* Header */}
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-sm font-medium text-muted-foreground uppercase tracking-wider flex items-center gap-2">
          <Layers className="w-4 h-4 text-cyan-500" />
          Zone Heatmap
        </h3>
        
        <select 
          value={activeMetric}
          onChange={(e) => setActiveMetric(e.target.value as MetricType)}
          className="bg-muted text-foreground text-xs font-medium px-2 py-1 rounded border border-border focus:outline-none focus:border-cyan-500"
        >
          <option value="Temp">Temperature</option>
          <option value="RH">Humidity</option>
          <option value="VWC">VWC</option>
          <option value="EC">EC</option>
          <option value="PPFD">PPFD</option>
        </select>
      </div>

      {/* Grid */}
      <div className="flex-1 grid grid-cols-6 gap-2">
        {MOCK_SENSOR_DATA.map((sensor) => {
          const val = sensor.values[activeMetric];
          const colorClass = getColor(activeMetric, val);
          
          return (
            <button
              key={sensor.id}
              onClick={() => setSelectedSensor(sensor.id)}
              className={cn(
                "relative flex items-center justify-center rounded-lg transition-all hover:scale-105 hover:z-10 hover:shadow-lg",
                colorClass
              )}
            >
              <span className="font-bold text-sm">{val.toFixed(1)}</span>
              
              {/* Alert Dot */}
              {sensor.hasAlert && (
                <span className="absolute top-1 right-1 w-2 h-2 rounded-full bg-red-600 ring-1 ring-white animate-pulse" />
              )}
            </button>
          );
        })}
      </div>

      {/* Sensor Detail Drawer (Overlay) */}
      {selectedSensor !== null && (
        <div className="absolute inset-0 bg-surface/95 backdrop-blur-sm rounded-xl p-4 z-20 animate-in slide-in-from-bottom-4">
           <div className="flex items-center justify-between mb-4 border-b border-border pb-2">
             <h4 className="font-bold text-foreground">Sensor #{selectedSensor} Details</h4>
             <button onClick={() => setSelectedSensor(null)} className="text-muted-foreground hover:text-foreground p-1">
               <X className="w-5 h-5" />
             </button>
           </div>
           
           <div className="space-y-3">
              {Object.entries(MOCK_SENSOR_DATA[selectedSensor - 1].values).map(([key, val]) => (
                <div key={key} className="flex items-center justify-between text-sm">
                   <span className="text-muted-foreground">{key}</span>
                   <span className="font-mono font-medium text-foreground">{val.toFixed(2)}</span>
                </div>
              ))}
              
              {MOCK_SENSOR_DATA[selectedSensor - 1].hasAlert && (
                 <div className="mt-4 p-3 bg-rose-500/10 border border-rose-500/20 rounded-lg text-xs text-rose-300">
                   Active Alert: Signal Timeout (Last seen 15m ago)
                 </div>
              )}
           </div>
        </div>
      )}
    </div>
  );
}
