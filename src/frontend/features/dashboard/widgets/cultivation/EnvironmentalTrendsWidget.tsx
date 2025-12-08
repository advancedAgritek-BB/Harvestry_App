'use client';

import React, { useState, useEffect } from 'react';
import { 
  AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, 
  ResponsiveContainer, Legend, Brush, ReferenceArea 
} from 'recharts';
import { cn } from '@/lib/utils';
import { X, AlertCircle, Palette, XCircle, Clock, RotateCcw } from 'lucide-react';
import { useChartHorizontalScroll } from '@/hooks/useChartHorizontalScroll';
import { simulationService, StreamType } from '@/features/telemetry/services/simulation.service';
import { useOverridesStore } from '@/stores/overridesStore';
import { 
  getCropSteeringData, 
  TIME_SCALES, 
  type TimeScale, 
  type CropSteeringDataPoint 
} from '@/features/telemetry/services/crop-steering-data.service';

// Data Types - extend CropSteeringDataPoint for chart compatibility
interface TrendDataPoint extends CropSteeringDataPoint {
  [key: string]: string | number | boolean | undefined;
}

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
  const overrides = useOverridesStore((state) => state.overrides);
  const cancelOverride = useOverridesStore((state) => state.cancelOverride);
  
  const activeOverrides = overrides.filter((o) => {
    if (!o.isActive) return false;
    if (o.expiresAt && new Date(o.expiresAt) < new Date()) return false;
    return true;
  });

  if (!activeOverrides.length) return null;

  const formatExpiry = (expiresAt?: string) => {
    if (!expiresAt) return null;
    const expiry = new Date(expiresAt);
    const now = new Date();
    const diffMs = expiry.getTime() - now.getTime();
    if (diffMs < 0) return 'Expired';
    const diffMins = Math.floor(diffMs / 60000);
    if (diffMins < 60) return `${diffMins}m remaining`;
    const diffHours = Math.floor(diffMins / 60);
    return `${diffHours}h ${diffMins % 60}m remaining`;
  };

  return (
    <div className="absolute top-4 right-4 z-10 flex flex-col items-end gap-2">
      {activeOverrides.map((item) => (
        <div 
          key={item.id}
          className={cn(
            "flex items-start gap-3 px-3 py-2 rounded-lg backdrop-blur-md border shadow-lg transition-all",
            item.severity === 'critical' ? "bg-rose-500/10 border-rose-500/50 text-rose-200" : 
            item.severity === 'warning' ? "bg-amber-500/10 border-amber-500/50 text-amber-200" :
            "bg-surface/80 border-border text-foreground"
          )}
        >
          <div className="flex flex-col flex-1 min-w-0">
            <div className="flex items-center gap-2">
              <span className="text-xs font-bold uppercase tracking-wider opacity-70">
                {item.type === 'manual' ? 'Override Active' : 
                 item.type === 'recipe' ? 'Active Recipe' :
                 item.type === 'emergency' ? 'Emergency Override' : 'Scheduled'}
              </span>
              {item.severity === 'critical' && (
                <AlertCircle className="w-3 h-3 text-rose-400 animate-pulse" />
              )}
            </div>
            <span className="text-sm font-medium">{item.label}</span>
            <span className="text-[10px] opacity-60">{item.details}</span>
            {item.expiresAt && (
              <span className="text-[10px] opacity-50 flex items-center gap-1 mt-1">
                <Clock className="w-2.5 h-2.5" />
                {formatExpiry(item.expiresAt)}
              </span>
            )}
          </div>
          <button
            onClick={() => cancelOverride(item.id)}
            className={cn(
              "p-1 rounded hover:bg-white/10 transition-colors flex-shrink-0",
              item.severity === 'critical' ? "text-rose-300 hover:text-rose-100" : "text-muted-foreground hover:text-foreground"
            )}
            title="Cancel override"
            aria-label={`Cancel ${item.label}`}
          >
            <XCircle className="w-4 h-4" />
          </button>
        </div>
      ))}
    </div>
  );
}

export function EnvironmentalTrendsWidget() {
  const [timeScale, setTimeScale] = useState<TimeScale>('1h');
  const [data, setData] = useState<TrendDataPoint[]>(() => getCropSteeringData('1h') as TrendDataPoint[]);
  const [showVwcPicker, setShowVwcPicker] = useState(false);
  
  // Horizontal scroll hook for mouse wheel panning
  const { 
    containerRef: chartContainerRef, 
    startIndex, 
    endIndex, 
    onBrushChange,
    resetViewport,
    isPanned 
  } = useChartHorizontalScroll({
    dataLength: data.length,
    scrollSensitivity: 2, // Move 2 data points per scroll step
  });
  
  // Zoom state
  const [zoomLeft, setZoomLeft] = useState<string | null>(null);
  const [zoomRight, setZoomRight] = useState<string | null>(null);
  const [refAreaLeft, setRefAreaLeft] = useState<string>('');
  const [refAreaRight, setRefAreaRight] = useState<string>('');
  const [isSelecting, setIsSelecting] = useState(false);
  
  const [series, setSeries] = useState<SeriesConfig[]>([
    { key: 'temp', name: 'Temp', color: '#22d3ee', yAxisId: 'temp', unit: '°F', visible: true },
    { key: 'rh', name: 'RH', color: '#a78bfa', yAxisId: 'percent', unit: '%', visible: true },
    { key: 'vpd', name: 'VPD', color: '#34d399', yAxisId: 'vpd', unit: 'kPa', visible: false },
    { key: 'co2', name: 'CO₂', color: '#fbbf24', yAxisId: 'co2', unit: 'ppm', visible: false },
    { key: 'ppfd', name: 'PPFD', color: '#f472b6', yAxisId: 'ppfd', unit: 'µmol', visible: false },
    { key: 'vwc', name: 'VWC', color: '#60a5fa', yAxisId: 'percent', unit: '%', visible: true },
  ]);

  // Get data from the consistent crop steering data service when time scale changes
  useEffect(() => {
    setData(getCropSteeringData(timeScale) as TrendDataPoint[]);
    setZoomLeft(null);
    setZoomRight(null);
  }, [timeScale]);

  // Periodic refresh to keep data current (uses same underlying cached data)
  useEffect(() => {
    const config = TIME_SCALES[timeScale];
    
    const refreshData = () => {
      // Refresh from the crop steering data service
      // This maintains consistency since the service uses cached base data
      setData(getCropSteeringData(timeScale) as TrendDataPoint[]);
    };

    // Also check for any active simulations to overlay real-time values
    const pollSimulation = async () => {
      try {
        const activeSims = await simulationService.getActive();
        if (activeSims.length === 0) {
          refreshData();
          return;
        }

        // Get base data from crop steering service
        const baseData = getCropSteeringData(timeScale) as TrendDataPoint[];
        
        // Update the last point with live simulation values if available
        if (baseData.length > 0) {
          const lastPoint = { ...baseData[baseData.length - 1] };
          
          activeSims.forEach(sim => {
            switch(sim.stream.streamType) {
              case StreamType.Temperature: lastPoint.temp = sim.lastValue; break;
              case StreamType.Humidity: lastPoint.rh = sim.lastValue; break;
              case StreamType.Vpd: lastPoint.vpd = sim.lastValue; break;
              case StreamType.Co2: lastPoint.co2 = sim.lastValue; break;
              case StreamType.LightPpfd: lastPoint.ppfd = sim.lastValue; break;
              case StreamType.SoilMoisture: lastPoint.vwc = sim.lastValue; break;
            }
          });

          baseData[baseData.length - 1] = lastPoint;
        }
        
        setData(baseData);
      } catch {
        // Silently fail and use base data
        refreshData();
      }
    };

    pollSimulation();
    const interval = setInterval(pollSimulation, Math.min(config.intervalMs, 2000));
    return () => clearInterval(interval);
  }, [timeScale]);

  // Get displayed data (zoomed or full)
  const displayData = React.useMemo(() => {
    if (zoomLeft && zoomRight) {
      const leftIdx = data.findIndex(d => d.time === zoomLeft);
      const rightIdx = data.findIndex(d => d.time === zoomRight);
      if (leftIdx >= 0 && rightIdx >= 0) {
        return data.slice(Math.min(leftIdx, rightIdx), Math.max(leftIdx, rightIdx) + 1);
      }
    }
    return data;
  }, [data, zoomLeft, zoomRight]);

  // Zoom handlers
  const handleMouseDown = (e: any) => {
    if (e?.activeLabel) {
      setRefAreaLeft(e.activeLabel);
      setIsSelecting(true);
    }
  };

  const handleMouseMove = (e: any) => {
    if (isSelecting && e?.activeLabel) {
      setRefAreaRight(e.activeLabel);
    }
  };

  const handleMouseUp = () => {
    if (refAreaLeft && refAreaRight && refAreaLeft !== refAreaRight) {
      setZoomLeft(refAreaLeft);
      setZoomRight(refAreaRight);
    }
    setRefAreaLeft('');
    setRefAreaRight('');
    setIsSelecting(false);
  };

  const resetZoom = () => {
    setZoomLeft(null);
    setZoomRight(null);
    resetViewport(); // Also reset scroll position
  };

  const toggleSeries = (key: string) => {
    setSeries(prev => prev.map(s => s.key === key ? { ...s, visible: !s.visible } : s));
  };

  const handleLegendClick = (e: any) => {
    const { dataKey } = e;
    if (dataKey === 'vwc') {
      setShowVwcPicker(true);
    } else {
      toggleSeries(dataKey);
    }
  };

  const isZoomed = zoomLeft !== null && zoomRight !== null;
  const hasViewportChange = isZoomed || isPanned;

  return (
    <div className="relative w-full h-full min-h-[350px] bg-surface/50 border border-border rounded-xl p-4 flex flex-col">
      {/* Header with controls */}
      <div className="flex items-center justify-between mb-4 gap-4 flex-wrap">
        <h3 className="text-sm font-medium text-muted-foreground uppercase tracking-wider">Environmental Trends</h3>
        
        <div className="flex items-center gap-3">
          {/* Time Scale Buttons */}
          <div className="flex items-center bg-muted/50 rounded-lg p-0.5">
            {(Object.keys(TIME_SCALES) as TimeScale[]).map((scale) => (
              <button
                key={scale}
                onClick={() => setTimeScale(scale)}
                className={cn(
                  "px-2.5 py-1 text-xs font-medium rounded-md transition-all",
                  timeScale === scale 
                    ? "bg-cyan-500 text-white shadow-sm" 
                    : "text-muted-foreground hover:text-foreground"
                )}
              >
                {TIME_SCALES[scale].label}
              </button>
            ))}
          </div>

          {/* Zoom/Pan Controls */}
          <div className="flex items-center gap-1">
            {hasViewportChange && (
              <button
                onClick={resetZoom}
                className="p-1.5 rounded-md bg-muted/50 text-muted-foreground hover:text-foreground hover:bg-muted transition-colors"
                title="Reset view"
              >
                <RotateCcw className="w-4 h-4" />
              </button>
            )}
          </div>

          {/* Live indicator */}
          <span className="flex items-center gap-1.5 text-emerald-400 bg-emerald-500/10 px-2 py-0.5 rounded border border-emerald-500/20 text-xs">
            <span className="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-pulse"/>
            Live
          </span>
        </div>
      </div>

      {/* Interaction hints */}
      {!hasViewportChange && (
        <div className="absolute top-14 left-4 text-[10px] text-muted-foreground/50">
          Scroll to pan • Drag to zoom • Click legend to toggle
        </div>
      )}

      <OverridesOverlay />

      {/* Chart - ref enables mouse wheel horizontal scrolling */}
      <div ref={chartContainerRef} className="flex-1 min-h-0">
        <ResponsiveContainer width="100%" height="100%">
          <AreaChart 
            data={displayData} 
            margin={{ top: 10, right: 30, left: 0, bottom: 0 }}
            onMouseDown={handleMouseDown}
            onMouseMove={handleMouseMove}
            onMouseUp={handleMouseUp}
            onMouseLeave={() => {
              if (isSelecting) {
                setIsSelecting(false);
                setRefAreaLeft('');
                setRefAreaRight('');
              }
            }}
          >
            <defs>
              {series.map(s => (
                <linearGradient key={s.key} id={`grad-${s.key}`} x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor={s.color} stopOpacity={0.3}/>
                  <stop offset="95%" stopColor={s.color} stopOpacity={0}/>
                </linearGradient>
              ))}
            </defs>
            
            <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" vertical={false} />
            <XAxis 
              dataKey="time" 
              stroke="var(--text-muted)" 
              fontSize={10} 
              tickLine={false} 
              axisLine={false}
              interval="preserveStartEnd"
              minTickGap={30}
            />
            
            <YAxis yAxisId="temp" orientation="left" stroke="#22d3ee" fontSize={10} tickLine={false} axisLine={false} unit="°" width={35} domain={['auto', 'auto']} />
            <YAxis yAxisId="percent" orientation="right" stroke="#a78bfa" fontSize={10} tickLine={false} axisLine={false} unit="%" width={35} domain={[0, 100]} />
            <YAxis yAxisId="vpd" hide domain={[0, 3]} />
            <YAxis yAxisId="co2" hide domain={[0, 2000]} />
            <YAxis yAxisId="ppfd" hide domain={[0, 1500]} />

            <Tooltip 
              contentStyle={{ 
                backgroundColor: 'hsl(var(--surface) / 0.85)', 
                backdropFilter: 'blur(12px)',
                WebkitBackdropFilter: 'blur(12px)',
                borderColor: 'hsl(var(--border) / 0.5)', 
                borderRadius: '12px',
                fontSize: '12px',
                boxShadow: '0 8px 32px rgba(0, 0, 0, 0.3)',
                padding: '12px 16px',
              }}
              labelStyle={{ color: 'hsl(var(--muted-foreground))', marginBottom: '8px', fontWeight: 500 }}
              formatter={(value: number, name: string) => {
                const s = series.find(item => item.name === name);
                return [`${value.toFixed(1)}${s?.unit || ''}`, name];
              }}
            />
            
            <Legend 
              wrapperStyle={{ paddingTop: '10px' }} 
              onClick={handleLegendClick}
              formatter={(value, entry: any) => {
                const s = series.find(item => item.key === entry.dataKey);
                return (
                  <span className={cn(
                    "ml-1 text-xs font-medium transition-opacity cursor-pointer",
                    !s?.visible && "opacity-40 line-through decoration-muted-foreground"
                  )}>
                    {value}
                  </span>
                );
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
                  strokeWidth={1.5}
                  yAxisId={s.yAxisId}
                  animationDuration={300}
                  isAnimationActive={true}
                  dot={false}
                />
              )
            ))}

            {/* Zoom selection area */}
            {refAreaLeft && refAreaRight && (
              <ReferenceArea
                yAxisId="temp"
                x1={refAreaLeft}
                x2={refAreaRight}
                strokeOpacity={0.3}
                fill="hsl(var(--cyan-500))"
                fillOpacity={0.1}
              />
            )}

            {/* Brush for scrolling - controlled by useChartHorizontalScroll hook */}
            <Brush 
              dataKey="time" 
              height={30} 
              stroke="hsl(var(--border))"
              fill="hsl(var(--surface))"
              tickFormatter={() => ''}
              startIndex={startIndex}
              endIndex={endIndex}
              onChange={onBrushChange}
            />
          </AreaChart>
        </ResponsiveContainer>
      </div>
      
      {/* VWC Color Picker Modal */}
      {showVwcPicker && (
        <div className="absolute inset-0 z-50 flex items-center justify-center bg-background/60 backdrop-blur-sm rounded-xl">
           <div className="bg-surface border border-border p-6 rounded-xl shadow-2xl max-w-sm w-full">
             <div className="flex items-center justify-between mb-4">
               <h4 className="text-lg font-semibold text-foreground flex items-center gap-2">
                 <Palette className="w-4 h-4 text-cyan-400" />
                 Customize VWC Colors
               </h4>
               <button 
                 onClick={() => setShowVwcPicker(false)} 
                 className="text-muted-foreground hover:text-foreground"
                 aria-label="Close color picker"
               >
                 <X className="w-5 h-5" />
               </button>
             </div>
             <p className="text-sm text-muted-foreground mb-4">Assign colors to individual sensor feeds.</p>
             
             <div className="space-y-3 mb-6">
                {['Sensor A1', 'Sensor A2', 'Sensor B1'].map((sensor) => (
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
               <button onClick={() => setShowVwcPicker(false)} className="px-3 py-1.5 text-xs font-medium bg-cyan-500 text-background rounded hover:bg-cyan-400">Save</button>
             </div>
           </div>
        </div>
      )}
    </div>
  );
}
