'use client';

import React, { useState, useEffect } from 'react';
import { 
  AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, 
  ResponsiveContainer, Legend, Brush, ReferenceArea 
} from 'recharts';
import { cn } from '@/lib/utils';
import { X, AlertCircle, Palette, XCircle, Clock, RotateCcw, ZoomIn, ZoomOut } from 'lucide-react';
import { useChartHorizontalScroll } from '@/hooks/useChartHorizontalScroll';
import { simulationService, StreamType } from '@/features/telemetry/services/simulation.service';
import { useOverridesStore } from '@/stores/overridesStore';
import { 
  getCropSteeringData, 
  TIME_SCALES, 
  type TimeScale, 
  type CropSteeringDataPoint 
} from '@/features/telemetry/services/crop-steering-data.service';
import { TrendSensorConfigModal, type TrendSensorConfig } from './TrendSensorConfigModal';
import type { SensorMetricType } from './types';

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
    { key: 'rh', name: 'RH', color: '#a78bfa', yAxisId: 'percent', unit: '%', visible: true },
    { key: 'vwc', name: 'VWC', color: '#60a5fa', yAxisId: 'percent', unit: '%', visible: true },
    { key: 'temp', name: 'Temp', color: '#22d3ee', yAxisId: 'temp', unit: '°F', visible: true },
    { key: 'vpd', name: 'VPD', color: '#34d399', yAxisId: 'vpd', unit: 'kPa', visible: false },
    { key: 'co2', name: 'CO₂', color: '#fbbf24', yAxisId: 'co2', unit: 'ppm', visible: false },
    { key: 'ppfd', name: 'PPFD', color: '#f472b6', yAxisId: 'ppfd', unit: 'µmol', visible: false },
  ]);

  // Sensor configuration modal state
  const [configModalOpen, setConfigModalOpen] = useState(false);
  const [configModalSeries, setConfigModalSeries] = useState<SeriesConfig | null>(null);
  const [sensorConfigs, setSensorConfigs] = useState<Record<string, TrendSensorConfig[]>>({});

  // Map series keys to SensorMetricType
  const seriesKeyToMetricType: Record<string, SensorMetricType> = {
    rh: 'Humidity',
    vwc: 'VWC',
    temp: 'Temperature',
    vpd: 'VPD',
    co2: 'CO2',
    ppfd: 'PPFD',
  };

  // Calculate which axes need to be visible based on active series
  const visibleAxes = React.useMemo(() => {
    const axes = new Set<string>();
    series.forEach(s => {
      if (s.visible) axes.add(s.yAxisId);
    });
    return axes;
  }, [series]);

  // Small right margin - axes handle their own width
  const rightMargin = 5;

  // Get data from the consistent crop steering data service when time scale changes
  useEffect(() => {
    setData(getCropSteeringData(timeScale) as TrendDataPoint[]);
    setZoomLeft(null);
    setZoomRight(null);
  }, [timeScale]);

  // Periodic refresh to keep data current (only mutate the latest tick, slide window)
  useEffect(() => {
    const config = TIME_SCALES[timeScale];
    const maxPoints = config.dataPoints;

    // Append or replace only the latest tick and slide the window forward
    const applyNextPoint = (nextPoint: TrendDataPoint) => {
      setData(prev => {
        if (!prev.length) return [nextPoint];

        const last = prev[prev.length - 1];

        // If the timestamp matches or is behind, replace the last tick
        if (nextPoint.timestamp <= last.timestamp) {
          const updated = [...prev];
          updated[updated.length - 1] = nextPoint;
          return updated;
        }

        // Otherwise push and keep a sliding window
        const updated = [...prev, nextPoint];
        if (updated.length > maxPoints) {
          updated.shift();
        }
        return updated;
      });
    };

    const pollSimulation = async () => {
      try {
        // Base point for "now" from the deterministic crop steering service
        const baseData = getCropSteeringData(timeScale) as TrendDataPoint[];
        if (baseData.length === 0) return;
        const nextPoint = { ...baseData[baseData.length - 1] };

        // Overlay live simulation values onto just the latest tick
        const activeSims = await simulationService.getActive();
        activeSims.forEach(sim => {
          switch(sim.stream.streamType) {
            case StreamType.Temperature: nextPoint.temp = sim.lastValue; break;
            case StreamType.Humidity: nextPoint.rh = sim.lastValue; break;
            case StreamType.Vpd: nextPoint.vpd = sim.lastValue; break;
            case StreamType.Co2: nextPoint.co2 = sim.lastValue; break;
            case StreamType.LightPpfd: nextPoint.ppfd = sim.lastValue; break;
            case StreamType.SoilMoisture: nextPoint.vwc = sim.lastValue; break;
          }
        });

        applyNextPoint(nextPoint);
      } catch {
        // Best-effort: ignore and keep existing data
      }
    };

    pollSimulation();

    // Use the time scale's intended cadence so each tick advances the window
    const interval = setInterval(pollSimulation, config.intervalMs);
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

  // Zoom in - reduce viewport to show fewer data points (more detail)
  const zoomIn = () => {
    const currentViewportSize = endIndex - startIndex + 1;
    const minViewportSize = 10; // Don't zoom in past 10 data points
    if (currentViewportSize <= minViewportSize) return;
    
    const newViewportSize = Math.max(minViewportSize, Math.floor(currentViewportSize * 0.7));
    const reduction = currentViewportSize - newViewportSize;
    const halfReduction = Math.floor(reduction / 2);
    
    const newStartIndex = Math.min(startIndex + halfReduction, data.length - newViewportSize);
    const newEndIndex = newStartIndex + newViewportSize - 1;
    
    onBrushChange({ startIndex: newStartIndex, endIndex: newEndIndex });
  };

  // Zoom out - increase viewport to show more data points (less detail)
  const zoomOut = () => {
    const currentViewportSize = endIndex - startIndex + 1;
    if (currentViewportSize >= data.length) return;
    
    const newViewportSize = Math.min(data.length, Math.ceil(currentViewportSize * 1.4));
    const expansion = newViewportSize - currentViewportSize;
    const halfExpansion = Math.floor(expansion / 2);
    
    const newStartIndex = Math.max(0, startIndex - halfExpansion);
    const newEndIndex = Math.min(data.length - 1, newStartIndex + newViewportSize - 1);
    
    // Adjust start if we hit the end
    const adjustedStartIndex = Math.max(0, newEndIndex - newViewportSize + 1);
    
    onBrushChange({ startIndex: adjustedStartIndex, endIndex: newEndIndex });
  };

  const toggleSeries = (key: string) => {
    setSeries(prev => prev.map(s => s.key === key ? { ...s, visible: !s.visible } : s));
  };

  const handleLegendClick = (e: any) => {
    const { dataKey } = e;
    const seriesItem = series.find(s => s.key === dataKey);
    if (seriesItem) {
      setConfigModalSeries(seriesItem);
      setConfigModalOpen(true);
    }
  };

  const handleSensorConfigSave = (configs: TrendSensorConfig[]) => {
    if (!configModalSeries) return;
    
    // Save sensor configs for this series
    setSensorConfigs(prev => ({
      ...prev,
      [configModalSeries.key]: configs,
    }));

    // Update the series color if sensors are configured, but keep visible either way
    // Empty configs = show default simulated data; populated configs = show with custom color
    setSeries(prev => prev.map(s => {
      if (s.key === configModalSeries.key) {
        const newColor = configs.length > 0 ? configs[0].color : s.color;
        return { ...s, visible: true, color: newColor };
      }
      return s;
    }));
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
            <button
              onClick={zoomOut}
              disabled={endIndex - startIndex + 1 >= data.length}
              className="p-1.5 rounded-md bg-muted/50 text-muted-foreground hover:text-foreground hover:bg-muted disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
              title="Zoom out"
            >
              <ZoomOut className="w-4 h-4" />
            </button>
            <button
              onClick={zoomIn}
              disabled={endIndex - startIndex + 1 <= 10}
              className="p-1.5 rounded-md bg-muted/50 text-muted-foreground hover:text-foreground hover:bg-muted disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
              title="Zoom in"
            >
              <ZoomIn className="w-4 h-4" />
            </button>
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
            margin={{ top: 10, right: rightMargin, left: 0, bottom: 0 }}
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
                <linearGradient key={`${s.key}-${s.color}`} id={`grad-${s.key}`} x1="0" y1="0" x2="0" y2="1">
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
            
            {/* Dynamic Y-Axes - only render visible ones */}
            {visibleAxes.has('temp') && (
              <YAxis 
                yAxisId="temp" 
                orientation="right" 
                stroke="#22d3ee"
                tick={{ fill: '#22d3ee', fontSize: 10 }}
                tickLine={false}
                axisLine={false}
                width={40}
                tickFormatter={(value: number) => `${Math.round(value)}°`}
                domain={[70, 85]}
                tickCount={4}
              />
            )}
            {visibleAxes.has('percent') && (
              <YAxis 
                yAxisId="percent" 
                orientation="right" 
                stroke="#a78bfa" 
                tick={{ fill: '#a78bfa', fontSize: 10 }}
                tickLine={false} 
                axisLine={false} 
                width={40}
                domain={[0, 100]} 
                tickCount={5}
                tickFormatter={(v: number) => `${v}%`}
              />
            )}
            {visibleAxes.has('vpd') && (
              <YAxis 
                yAxisId="vpd" 
                orientation="right" 
                stroke="#34d399"
                tick={{ fill: '#34d399', fontSize: 10 }}
                tickLine={false}
                axisLine={false}
                width={40}
                domain={[0, 2]}
                tickCount={5}
                tickFormatter={(v: number) => v.toFixed(1)}
              />
            )}
            {visibleAxes.has('co2') && (
              <YAxis 
                yAxisId="co2" 
                orientation="right" 
                stroke="#fbbf24"
                tick={{ fill: '#fbbf24', fontSize: 10 }}
                tickLine={false}
                axisLine={false}
                width={40}
                domain={[0, 2000]}
                tickCount={5}
                tickFormatter={(v: number) => `${v}`}
              />
            )}
            {visibleAxes.has('ppfd') && (
              <YAxis 
                yAxisId="ppfd" 
                orientation="right" 
                stroke="#f472b6"
                tick={{ fill: '#f472b6', fontSize: 10 }}
                tickLine={false}
                axisLine={false}
                width={40}
                domain={[0, 1500]}
                tickCount={5}
                tickFormatter={(v: number) => `${v}`}
              />
            )}
            {/* Hidden axes for series that might get toggled - provides scale reference */}
            {!visibleAxes.has('temp') && <YAxis yAxisId="temp" hide domain={[70, 85]} />}
            {!visibleAxes.has('percent') && <YAxis yAxisId="percent" hide domain={[0, 100]} />}
            {!visibleAxes.has('vpd') && <YAxis yAxisId="vpd" hide domain={[0, 2]} />}
            {!visibleAxes.has('co2') && <YAxis yAxisId="co2" hide domain={[0, 2000]} />}
            {!visibleAxes.has('ppfd') && <YAxis yAxisId="ppfd" hide domain={[0, 1500]} />}

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
              content={() => {
                const legendOrder = ['rh', 'vwc', 'temp', 'vpd', 'co2', 'ppfd'];
                return (
                  <div className="flex flex-wrap items-center gap-3 text-xs">
                    {legendOrder.map((key) => {
                      const item = series.find((s) => s.key === key);
                      if (!item) return null;
                      return (
                        <div
                          key={item.key}
                          className={cn(
                            "flex items-center gap-1 px-2 py-1 rounded-md transition-colors",
                            "border border-border/70 bg-muted/30",
                            !item.visible && "opacity-50"
                          )}
                        >
                          {/* Color dot - click to configure */}
                          <button
                            type="button"
                            onClick={() => handleLegendClick({ dataKey: item.key })}
                            title="Click to configure sensors & colors"
                            className="w-3 h-3 rounded-full hover:ring-2 ring-white/50 transition-all"
                            style={{ backgroundColor: item.color }}
                            aria-label={`Configure ${item.name} sensors`}
                          />
                          {/* Name - click to toggle visibility */}
                          <button
                            type="button"
                            onClick={() => toggleSeries(item.key)}
                            className={cn(
                              "font-medium hover:text-foreground transition-colors",
                              item.visible ? "text-foreground/80" : "text-muted-foreground line-through"
                            )}
                          >
                            {item.name}
                          </button>
                        </div>
                      );
                    })}
                  </div>
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
      
      {/* Sensor Configuration Modal */}
      {configModalSeries && (
        <TrendSensorConfigModal
          isOpen={configModalOpen}
          onClose={() => {
            setConfigModalOpen(false);
            setConfigModalSeries(null);
          }}
          metricKey={configModalSeries.key}
          metricName={configModalSeries.name}
          metricType={seriesKeyToMetricType[configModalSeries.key]}
          defaultColor={configModalSeries.color}
          currentConfig={sensorConfigs[configModalSeries.key] || []}
          onSave={handleSensorConfigSave}
        />
      )}
    </div>
  );
}
