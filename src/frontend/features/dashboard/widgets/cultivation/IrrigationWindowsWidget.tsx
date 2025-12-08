import React, { useState } from 'react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Cell, TooltipProps, Brush } from 'recharts';
import { cn } from '@/lib/utils';
import { Plus, Droplets, AlertTriangle, Hand, Zap, Waves } from 'lucide-react';
import { useChartHorizontalScroll } from '@/hooks/useChartHorizontalScroll';

// Types
type IrrigationPeriod = 'P1 - Ramp' | 'P2 - Maintenance' | 'P3 - Dryback' | 'All';

interface IrrigationDataPoint {
  id: number;
  time: string;
  volume: number;
  endVwc: number;
  type: 'manual' | 'automated';
  zone: string;
}

// Color constants for consistency
const COLORS = {
  manual: '#fbbf24',    // Amber/Yellow - Manual irrigation
  automated: '#3b82f6', // Blue - Automated irrigation
  vwc: '#10b981',       // Emerald/Green - VWC percentage
} as const;

// Mock Data
const mockData: IrrigationDataPoint[] = Array.from({ length: 12 }).map((_, i) => ({
  id: i,
  time: `${8 + i}:00`,
  volume: Math.floor(Math.random() * 150) + 50, // mL
  endVwc: 45 + Math.random() * 5, // %
  type: i === 4 || i === 8 ? 'manual' : 'automated',
  zone: ['A', 'B', 'C'][Math.floor(Math.random() * 3)] as string
}));

// Custom Tooltip Component
function CustomTooltip({ active, payload, label }: TooltipProps<number, string>) {
  if (!active || !payload || !payload.length) return null;

  const dataPoint = payload[0]?.payload as IrrigationDataPoint;
  const volumeData = payload.find(p => p.dataKey === 'volume');
  const vwcData = payload.find(p => p.dataKey === 'endVwc');

  const isManual = dataPoint?.type === 'manual';

  return (
    <div className="bg-surface/95 backdrop-blur-sm border border-border rounded-xl p-3 shadow-2xl min-w-[180px]">
      {/* Header with time */}
      <div className="flex items-center gap-2 mb-3 pb-2 border-b border-border/50">
        <div className="w-8 h-8 rounded-lg bg-muted flex items-center justify-center">
          <Droplets className="w-4 h-4 text-cyan-400" />
        </div>
        <div>
          <div className="text-sm font-semibold text-foreground">{label}</div>
          <div className="text-[10px] text-muted-foreground uppercase tracking-wider">Irrigation Shot</div>
        </div>
      </div>

      {/* Volume row */}
      {volumeData && (
        <div className="flex items-center justify-between mb-2">
          <div className="flex items-center gap-2">
            <div 
              className="w-3 h-3 rounded-sm" 
              style={{ backgroundColor: isManual ? COLORS.manual : COLORS.automated }}
            />
            <span className="text-xs text-muted-foreground">Volume</span>
          </div>
          <div className="flex items-center gap-1.5">
            <span className="text-sm font-bold text-foreground">{volumeData.value} mL</span>
            <span 
              className={cn(
                "text-[9px] font-medium px-1.5 py-0.5 rounded uppercase tracking-wider",
                isManual 
                  ? "bg-amber-500/20 text-amber-400" 
                  : "bg-blue-500/20 text-blue-400"
              )}
            >
              {isManual ? 'Manual' : 'Auto'}
            </span>
          </div>
        </div>
      )}

      {/* VWC row */}
      {vwcData && (
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <div 
              className="w-3 h-3 rounded-sm" 
              style={{ backgroundColor: COLORS.vwc }}
            />
            <span className="text-xs text-muted-foreground">End VWC</span>
          </div>
          <span className="text-sm font-bold text-foreground">
            {typeof vwcData.value === 'number' ? vwcData.value.toFixed(1) : vwcData.value}%
          </span>
        </div>
      )}

      {/* Zone indicator */}
      {dataPoint?.zone && (
        <div className="mt-2 pt-2 border-t border-border/50">
          <span className="text-[10px] text-muted-foreground">Zone: </span>
          <span className="text-[10px] font-medium text-foreground/70">{dataPoint.zone}</span>
        </div>
      )}
    </div>
  );
}

// Legend Component
function ChartLegend() {
  const legendItems = [
    { color: COLORS.automated, label: 'Automated', icon: Zap },
    { color: COLORS.manual, label: 'Manual', icon: Hand },
    { color: COLORS.vwc, label: 'End VWC %', icon: Waves },
  ];

  return (
    <div className="flex items-center gap-4 px-2">
      {legendItems.map(({ color, label, icon: Icon }) => (
        <div key={label} className="flex items-center gap-1.5">
          <div 
            className="w-2.5 h-2.5 rounded-sm"
            style={{ backgroundColor: color }}
          />
          <Icon className="w-3 h-3" style={{ color }} />
          <span className="text-[10px] font-medium text-muted-foreground">{label}</span>
        </div>
      ))}
    </div>
  );
}

export function IrrigationWindowsWidget() {
  const [activeTab, setActiveTab] = useState<IrrigationPeriod>('P1 - Ramp');
  const [selectedZones, setSelectedZones] = useState<string[]>(['A', 'B', 'C', 'D', 'E', 'F']);
  const [showShotModal, setShowShotModal] = useState(false);
  const [pendingQuickShot, setPendingQuickShot] = useState<number | null>(null); // Volume for confirmation

  // Horizontal scroll hook for mouse wheel panning
  const { 
    containerRef: chartContainerRef, 
    startIndex, 
    endIndex, 
    onBrushChange,
  } = useChartHorizontalScroll({
    dataLength: mockData.length,
    scrollSensitivity: 1,
  });

  const toggleZone = (zone: string) => {
    setSelectedZones(prev => 
      prev.includes(zone) 
        ? prev.filter(z => z !== zone)
        : [...prev, zone].sort()
    );
  };

  const handleQuickPick = (volume: number) => {
    // Open confirmation instead of firing immediately
    setPendingQuickShot(volume);
  };

  const confirmQuickShot = () => {
    console.log(`Firing ${pendingQuickShot}mL shot on zones: ${selectedZones.join(', ')}`);
    setPendingQuickShot(null);
    // Toast success here
  };

  return (
    <div className="w-full h-full min-h-[300px] bg-surface/50 border border-border rounded-xl p-4 flex flex-col">
      
      {/* Header Controls */}
      <div className="flex flex-col gap-4 mb-4">
        <div className="flex items-center justify-between">
          <h3 className="text-sm font-medium text-muted-foreground uppercase tracking-wider flex items-center gap-2">
            <Droplets className="w-4 h-4 text-cyan-500" />
            Irrigation Windows
          </h3>
          
          {/* Tabs */}
          <div className="flex bg-muted/50 rounded-lg p-1 gap-1">
            {(['P1 - Ramp', 'P2 - Maintenance', 'P3 - Dryback', 'All'] as IrrigationPeriod[]).map(tab => (
              <button
                key={tab}
                onClick={() => setActiveTab(tab)}
                className={cn(
                  "px-3 py-1 text-xs font-medium rounded transition-colors",
                  activeTab === tab 
                    ? "bg-cyan-500/20 text-cyan-300 shadow-sm" 
                    : "text-muted-foreground hover:text-foreground hover:bg-muted"
                )}
              >
                {tab}
              </button>
            ))}
          </div>
        </div>

        {/* Zone Selectors */}
        <div className="flex items-center gap-2 overflow-x-auto pb-1">
          {['A', 'B', 'C', 'D', 'E', 'F'].map(zone => (
            <button
              key={zone}
              onClick={() => toggleZone(zone)}
              className={cn(
                "flex items-center justify-center min-w-[32px] h-8 text-xs font-bold rounded border transition-all",
                selectedZones.includes(zone)
                  ? "bg-blue-500/20 border-blue-500/50 text-blue-300"
                  : "bg-muted/50 border-border text-muted-foreground hover:border-border/80"
              )}
            >
              {zone}
            </button>
          ))}
          <div className="w-px h-6 bg-border mx-2" />
          <button 
            onClick={() => setShowShotModal(true)}
            className="flex items-center gap-1 px-3 py-1.5 text-xs font-medium text-foreground bg-muted hover:bg-muted/80 border border-border rounded transition-colors whitespace-nowrap"
          >
            <Plus className="w-3 h-3" />
            Add shot
          </button>
        </div>
      </div>

      {/* Legend */}
      <div className="mb-3">
        <ChartLegend />
      </div>

      {/* Dual Bar Chart - ref enables mouse wheel horizontal scrolling */}
      <div ref={chartContainerRef} className="flex-1 min-h-0 relative">
        <ResponsiveContainer width="100%" height="100%">
          <BarChart data={mockData} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
            <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" vertical={false} />
            <XAxis dataKey="time" stroke="var(--text-muted)" fontSize={11} tickLine={false} axisLine={false} />
            
            {/* Left Axis: Volume */}
            <YAxis yAxisId="vol" stroke={COLORS.automated} fontSize={11} tickLine={false} axisLine={false} unit="mL" width={35} />
            
            {/* Right Axis: VWC */}
            <YAxis yAxisId="vwc" orientation="right" stroke={COLORS.vwc} fontSize={11} tickLine={false} axisLine={false} unit="%" domain={[0, 100]} width={35} />

            <Tooltip 
               cursor={{fill: 'var(--bg-hover)', opacity: 0.4}}
               content={<CustomTooltip />}
            />

            {/* Bar 1: Volume (Color coded by manual vs auto) */}
            <Bar yAxisId="vol" dataKey="volume" name="Vol (mL)" radius={[4, 4, 0, 0]} barSize={20}>
              {mockData.map((entry, index) => (
                <Cell key={`cell-${index}`} fill={entry.type === 'manual' ? COLORS.manual : COLORS.automated} />
              ))}
            </Bar>

            {/* Bar 2: End VWC */}
            <Bar yAxisId="vwc" dataKey="endVwc" name="End VWC %" fill={COLORS.vwc} radius={[4, 4, 0, 0]} barSize={20} fillOpacity={0.6} />
            
            {/* Brush for scrolling - controlled by useChartHorizontalScroll hook */}
            <Brush 
              dataKey="time" 
              height={25} 
              stroke="hsl(var(--border))"
              fill="hsl(var(--surface))"
              tickFormatter={() => ''}
              startIndex={startIndex}
              endIndex={endIndex}
              onChange={onBrushChange}
            />
          </BarChart>
        </ResponsiveContainer>
        
        {/* Quick Pick Overlay (Bottom) */}
        <div className="absolute bottom-0 left-0 right-0 p-2 flex items-center justify-between bg-gradient-to-t from-surface via-surface/80 to-transparent pt-8 pointer-events-none">
           <div className="flex items-center gap-2 pointer-events-auto">
             <span className="text-[10px] font-medium text-muted-foreground uppercase tracking-wider">Quick Pick:</span>
             {[50, 75, 100, 125].map(vol => (
               <button
                 key={vol}
                 onClick={() => handleQuickPick(vol)}
                 className="px-2.5 py-1 text-xs font-mono font-medium text-cyan-300 bg-cyan-500/10 hover:bg-cyan-500/20 border border-cyan-500/30 rounded transition-colors active:scale-95"
               >
                 {vol} mL
               </button>
             ))}
           </div>
           
           <div className="text-[10px] text-muted-foreground flex flex-col items-end">
             <span>Leachate: <span className="text-foreground font-medium">12%</span></span>
             <span>Target: 8-15%</span>
           </div>
        </div>
      </div>

      {/* Confirmation Modal for Quick Shot */}
      {pendingQuickShot && (
        <div className="absolute inset-0 z-50 flex items-center justify-center bg-background/80 backdrop-blur-sm rounded-xl animate-in fade-in duration-200">
          <div className="bg-surface border border-border p-5 rounded-xl shadow-2xl max-w-xs w-full text-center">
            <div className="w-12 h-12 rounded-full bg-amber-500/20 text-amber-500 flex items-center justify-center mx-auto mb-4">
              <AlertTriangle className="w-6 h-6" />
            </div>
            <h4 className="text-lg font-semibold text-foreground mb-2">Confirm Irrigation</h4>
            <p className="text-sm text-muted-foreground mb-6">
              Are you sure you want to irrigate <span className="text-cyan-400 font-bold">{pendingQuickShot}mL</span> on <span className="text-foreground font-bold">{selectedZones.length} zones</span>?
            </p>
            <div className="grid grid-cols-2 gap-3">
              <button 
                onClick={() => setPendingQuickShot(null)}
                className="px-4 py-2 text-sm font-medium text-foreground/70 hover:text-foreground bg-muted hover:bg-muted/80 rounded-lg transition-colors"
              >
                Cancel
              </button>
              <button 
                onClick={confirmQuickShot}
                className="px-4 py-2 text-sm font-medium text-background bg-cyan-500 hover:bg-cyan-400 rounded-lg transition-colors shadow-[0_0_15px_-3px_rgba(6,182,212,0.5)]"
              >
                Confirm
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
