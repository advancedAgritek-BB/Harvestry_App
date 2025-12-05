import React, { useState } from 'react';
import { AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend } from 'recharts';
import { cn } from '@/lib/utils';
import { X, AlertCircle, Palette } from 'lucide-react';

// Mock Data Types
interface TrendDataPoint {
  time: string;
  temp: number;
  rh: number;
  vpd: number;
  co2: number;
  ppfd: number;
  vwc: number; // Average or specific
  [key: string]: string | number; 
}

// Mock Data Generator
const generateData = (): TrendDataPoint[] => {
  const data: TrendDataPoint[] = [];
  for (let i = 0; i < 24; i++) {
    data.push({
      time: `${i}:00`,
      temp: 75 + Math.sin(i / 4) * 3, // Converted to F range
      rh: 55 + Math.cos(i / 4) * 5,
      vpd: 1.2 + Math.sin(i / 4) * 0.3,
      co2: 1000 + (Math.abs(Math.sin(i * 10)) * 200), // Deterministic CO2
      ppfd: i > 6 && i < 18 ? 900 : 0,
      vwc: 45 - (i % 4), // Dryback simulation
    });
  }
  return data;
};

interface SeriesConfig {
  key: string;
  name: string;
  color: string;
  yAxisId: string;
  unit: string;
  visible: boolean;
}

// Overlay Component for Overrides
function OverridesOverlay() {
  const [isExpanded, setIsExpanded] = useState(true); // Mock: expanded because issue exists
  
  // Mock Active Overrides
  const activeOverrides = [
    { id: 1, type: 'override', label: 'CO₂ Override', details: '1100ppm until 18:00', severity: 'critical' },
    { id: 2, type: 'recipe', label: 'Recipe: Flower v2', details: 'EC 2.5, pH 5.8, PPFD 900', severity: 'normal' },
  ];

  if (!activeOverrides.length && !isExpanded) return null;

  return (
    <div className="absolute top-4 right-4 z-10 flex flex-col items-end gap-2 pointer-events-none">
      {activeOverrides.map(item => (
        <div 
          key={item.id}
          className={cn(
            "pointer-events-auto flex items-center gap-3 px-3 py-2 rounded-lg backdrop-blur-md border shadow-lg transition-all cursor-pointer hover:scale-[1.02]",
            item.severity === 'critical' ? "bg-rose-500/10 border-rose-500 text-rose-200" : "bg-surface/80 border-border text-foreground"
          )}
          onClick={() => console.log("Open details for", item.label)}
        >
           <div className="flex flex-col">
             <span className="text-xs font-bold uppercase tracking-wider opacity-70">{item.severity === 'critical' ? 'Override Active' : 'Active Recipe'}</span>
             <span className="text-sm font-medium">{item.label}</span>
             <span className="text-[10px] opacity-60">{item.details}</span>
           </div>
           {item.severity === 'critical' && <AlertCircle className="w-4 h-4 text-rose-400 animate-pulse" />}
        </div>
      ))}
    </div>
  );
}

export function EnvironmentalTrendsWidget() {
  const [data] = useState(generateData());
  const [showVwcPicker, setShowVwcPicker] = useState(false);
  
  const [series, setSeries] = useState<SeriesConfig[]>([
    { key: 'temp', name: 'Temp', color: '#22d3ee', yAxisId: 'temp', unit: '°F', visible: true },
    { key: 'rh', name: 'RH', color: '#a78bfa', yAxisId: 'percent', unit: '%', visible: true },
    { key: 'vpd', name: 'VPD', color: '#34d399', yAxisId: 'vpd', unit: 'kPa', visible: false },
    { key: 'co2', name: 'CO₂', color: '#fbbf24', yAxisId: 'co2', unit: 'ppm', visible: false },
    { key: 'ppfd', name: 'PPFD', color: '#f472b6', yAxisId: 'ppfd', unit: 'µmol', visible: false },
    { key: 'vwc', name: 'VWC', color: '#60a5fa', yAxisId: 'percent', unit: '%', visible: true },
  ]);

  const toggleSeries = (key: string) => {
    setSeries(prev => prev.map(s => s.key === key ? { ...s, visible: !s.visible } : s));
  };

  const handleLegendClick = (e: any) => {
    const { dataKey } = e;
    if (dataKey === 'vwc') {
      // Special interaction for VWC legend
       setShowVwcPicker(true);
    } else {
      toggleSeries(dataKey);
    }
  };

  return (
    <div className="relative w-full h-full min-h-[350px] bg-surface/50 border border-border rounded-xl p-4 flex flex-col">
      <div className="flex items-center justify-between mb-4">
         <h3 className="text-sm font-medium text-muted-foreground uppercase tracking-wider">Environmental Trends</h3>
         <div className="flex items-center gap-2 text-xs">
           <span className="flex items-center gap-1.5 text-emerald-400 bg-emerald-500/10 px-2 py-0.5 rounded border border-emerald-500/20">
             <span className="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-pulse"/>
             Live
           </span>
         </div>
      </div>

      <OverridesOverlay />

      <div className="flex-1 min-h-0">
        <ResponsiveContainer width="100%" height="100%">
          <AreaChart data={data} margin={{ top: 10, right: 30, left: 0, bottom: 0 }}>
            <defs>
              {series.map(s => (
                <linearGradient key={s.key} id={`grad-${s.key}`} x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor={s.color} stopOpacity={0.2}/>
                  <stop offset="95%" stopColor={s.color} stopOpacity={0}/>
                </linearGradient>
              ))}
            </defs>
            
            <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" vertical={false} />
            <XAxis dataKey="time" stroke="var(--text-muted)" fontSize={12} tickLine={false} axisLine={false} />
            
            <YAxis yAxisId="temp" orientation="left" stroke="#22d3ee" fontSize={12} tickLine={false} axisLine={false} unit="°" width={30} />
            <YAxis yAxisId="percent" orientation="right" stroke="#a78bfa" fontSize={12} tickLine={false} axisLine={false} unit="%" width={30} />
            {/* Hidden Y-Axes for scaling other metrics properly */}
            <YAxis yAxisId="vpd" hide domain={[0, 3]} />
            <YAxis yAxisId="co2" hide domain={[0, 2000]} />
            <YAxis yAxisId="ppfd" hide domain={[0, 1500]} />

            <Tooltip 
              contentStyle={{ backgroundColor: 'var(--bg-surface)', borderColor: 'var(--border)', borderRadius: '8px' }}
              itemStyle={{ fontSize: '12px' }}
              labelStyle={{ color: 'var(--text-muted)', marginBottom: '4px' }}
            />
            
            <Legend 
              wrapperStyle={{ paddingTop: '20px' }} 
              onClick={handleLegendClick}
              formatter={(value, entry: any) => {
                 const s = series.find(item => item.key === entry.dataKey);
                 return <span className={cn("ml-1 text-xs font-medium transition-opacity", !s?.visible && "opacity-40 line-through decoration-muted-foreground")}>{value}</span>;
              }}
            />

            {series.map(s => (
              s.visible && (
                <Area 
                  key={s.key}
                  type="monotone" 
                  dataKey={s.key} 
                  name={s.name}
                  stroke={s.color} 
                  fill={`url(#grad-${s.key})`}
                  strokeWidth={2}
                  yAxisId={s.yAxisId}
                  animationDuration={1000}
                />
              )
            ))}
          </AreaChart>
        </ResponsiveContainer>
      </div>
      
      {/* VWC Color Picker Mock Modal */}
      {showVwcPicker && (
        <div className="absolute inset-0 z-50 flex items-center justify-center bg-background/60 backdrop-blur-sm rounded-xl">
           <div className="bg-surface border border-border p-6 rounded-xl shadow-2xl max-w-sm w-full">
             <div className="flex items-center justify-between mb-4">
               <h4 className="text-lg font-semibold text-foreground flex items-center gap-2">
                 <Palette className="w-4 h-4 text-cyan-400" />
                 Customize VWC Colors
               </h4>
               <button onClick={() => setShowVwcPicker(false)} className="text-muted-foreground hover:text-foreground">
                 <X className="w-5 h-5" />
               </button>
             </div>
             <p className="text-sm text-muted-foreground mb-4">Assign colors to individual sensor feeds. Changes will be saved to "My Colors".</p>
             
             <div className="space-y-3 mb-6">
                {['Sensor A1', 'Sensor A2', 'Sensor B1'].map((sensor, i) => (
                  <div key={sensor} className="flex items-center justify-between p-2 bg-muted rounded">
                    <span className="text-sm text-foreground/70">{sensor}</span>
                    <div className="flex gap-2">
                      <div className="w-6 h-6 rounded-full bg-blue-500 cursor-pointer hover:ring-2 ring-foreground" />
                      <div className="w-6 h-6 rounded-full bg-emerald-500 cursor-pointer hover:ring-2 ring-foreground" />
                      <div className="w-6 h-6 rounded-full bg-purple-500 cursor-pointer hover:ring-2 ring-foreground" />
                    </div>
                  </div>
                ))}
             </div>
             
             <div className="flex justify-end gap-2">
               <button onClick={() => setShowVwcPicker(false)} className="px-3 py-1.5 text-xs font-medium text-foreground/70 hover:text-foreground">Cancel</button>
               <button onClick={() => setShowVwcPicker(false)} className="px-3 py-1.5 text-xs font-medium bg-cyan-500 text-background rounded hover:bg-cyan-400">Save Changes</button>
             </div>
           </div>
        </div>
      )}
    </div>
  );
}
