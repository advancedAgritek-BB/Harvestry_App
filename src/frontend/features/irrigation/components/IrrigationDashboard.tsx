'use client';

import React, { useState, useEffect } from 'react';
import { Droplets, Play, Square, Activity, Gauge } from 'lucide-react';
import { cn } from '@/lib/utils';
import { simulationService, StreamType } from '@/features/telemetry/services/simulation.service';

interface Zone {
  id: string;
  name: string;
  status: 'idle' | 'watering' | 'soaking';
  flowRate: number; // GPM
  pressure: number; // PSI
  lastRun: string;
  moisture: number; // %
}

const INITIAL_ZONES: Zone[] = [
  { id: 'z1', name: 'Flower Room 1 (Left)', status: 'idle', flowRate: 0, pressure: 45, lastRun: '2h ago', moisture: 45 },
  { id: 'z2', name: 'Flower Room 1 (Right)', status: 'idle', flowRate: 0, pressure: 44, lastRun: '2h ago', moisture: 42 },
  { id: 'z3', name: 'Veg Room 1', status: 'idle', flowRate: 0, pressure: 42, lastRun: '4h ago', moisture: 55 },
  { id: 'z4', name: 'Mother Room', status: 'idle', flowRate: 0, pressure: 45, lastRun: '6h ago', moisture: 60 },
];

export function IrrigationDashboard() {
  const [zones, setZones] = useState<Zone[]>(INITIAL_ZONES);
  const [simulatedData, setSimulatedData] = useState<Record<string, number>>({});

  // Poll simulation service for "live" feel
  useEffect(() => {
    // Start simulations if not running
    simulationService.start(StreamType.FlowRate).catch(() => {});
    simulationService.start(StreamType.Pressure).catch(() => {});
    simulationService.start(StreamType.SoilMoisture).catch(() => {});

    const interval = setInterval(async () => {
      try {
        const active = await simulationService.getActive();
        const newData: Record<string, number> = {};
        active.forEach(s => {
          // Map some streams to our local data just for visual vitality
          if (s.stream.streamType === StreamType.FlowRate) newData.flow = s.lastValue;
          if (s.stream.streamType === StreamType.Pressure) newData.pressure = s.lastValue;
          if (s.stream.streamType === StreamType.SoilMoisture) newData.moisture = s.lastValue;
        });
        setSimulatedData(newData);
      } catch (e) {
        // ignore
      }
    }, 1000);

    return () => clearInterval(interval);
  }, []);

  const toggleZone = (id: string) => {
    setZones(prev => prev.map(z => {
      if (z.id !== id) return z;
      const newStatus = z.status === 'idle' ? 'watering' : 'idle';
      return { ...z, status: newStatus };
    }));
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-lg font-bold text-foreground">Zone Control</h2>
          <p className="text-sm text-muted-foreground">Manual override and real-time monitoring</p>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {zones.map(zone => {
          const isWatering = zone.status === 'watering';
          // Use simulated data if watering, otherwise 0 or static
          const currentFlow = isWatering ? (simulatedData.flow || 2.5) : 0;
          const currentPressure = isWatering ? (simulatedData.pressure || 45) : (zone.pressure);
          // Moisture drifts slowly, use sim data as base variance
          const currentMoisture = zone.moisture + ((simulatedData.moisture || 50) - 50) * 0.05;

          return (
            <div key={zone.id} className={cn(
              "bg-surface/50 border rounded-xl p-4 transition-all",
              isWatering ? "border-cyan-500/50 shadow-[0_0_15px_rgba(6,182,212,0.15)]" : "border-border"
            )}>
              <div className="flex justify-between items-start mb-4">
                <div>
                  <h3 className="font-medium text-foreground">{zone.name}</h3>
                  <div className="flex items-center gap-2 mt-1">
                    <span className={cn(
                      "flex h-2 w-2 rounded-full",
                      isWatering ? "bg-cyan-400 animate-pulse" : "bg-muted-foreground/30"
                    )} />
                    <span className="text-xs text-muted-foreground capitalize">{zone.status}</span>
                  </div>
                </div>
                <button
                  onClick={() => toggleZone(zone.id)}
                  className={cn(
                    "p-2 rounded-lg transition-colors",
                    isWatering 
                      ? "bg-rose-500/10 text-rose-400 hover:bg-rose-500/20" 
                      : "bg-cyan-500/10 text-cyan-400 hover:bg-cyan-500/20"
                  )}
                >
                  {isWatering ? <Square className="w-4 h-4 fill-current" /> : <Play className="w-4 h-4 fill-current" />}
                </button>
              </div>

              <div className="grid grid-cols-2 gap-3 mb-3">
                <div className="bg-background/50 rounded-lg p-2">
                  <div className="flex items-center gap-1.5 text-muted-foreground mb-1">
                    <Droplets className="w-3 h-3" />
                    <span className="text-[10px] uppercase tracking-wider">Flow</span>
                  </div>
                  <div className="text-lg font-mono font-medium">
                    {currentFlow.toFixed(1)} <span className="text-xs text-muted-foreground">GPM</span>
                  </div>
                </div>
                <div className="bg-background/50 rounded-lg p-2">
                  <div className="flex items-center gap-1.5 text-muted-foreground mb-1">
                    <Gauge className="w-3 h-3" />
                    <span className="text-[10px] uppercase tracking-wider">PSI</span>
                  </div>
                  <div className="text-lg font-mono font-medium">
                    {currentPressure.toFixed(0)} <span className="text-xs text-muted-foreground">PSI</span>
                  </div>
                </div>
              </div>

              <div className="flex items-center justify-between text-xs text-muted-foreground border-t border-border/50 pt-3">
                <div className="flex items-center gap-1.5">
                  <Activity className="w-3 h-3 text-emerald-400" />
                  <span>VWC {currentMoisture.toFixed(1)}%</span>
                </div>
                <span>Last run: {zone.lastRun}</span>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
