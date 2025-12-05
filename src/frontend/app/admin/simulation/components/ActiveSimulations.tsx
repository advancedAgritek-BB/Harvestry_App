'use client';

import React from 'react';
import { 
  Activity, 
  Square, 
  Waves, 
  TrendingUp,
  Gauge,
  Radio
} from 'lucide-react';
import {
  StreamType,
  StreamTypeLabels,
  SimulationBehavior,
  SimulationBehaviorLabels,
  UnitLabels,
  Unit,
  simulationService,
  SimulationState
} from '@/features/telemetry/services/simulation.service';

interface ActiveSimulationsProps {
  simulations: SimulationState[];
  onRefresh: () => void;
  readOnly?: boolean;
}

export default function ActiveSimulations({ simulations, onRefresh, readOnly = false }: ActiveSimulationsProps) {
  const handleStop = async (streamId: string) => {
    if (readOnly) return;
    try {
      await simulationService.toggle(streamId);
      onRefresh();
    } catch (err) {
      console.error('Error stopping simulation:', err);
    }
  };

  return (
    <div className="rounded-2xl border border-border bg-gradient-to-br from-card to-card/50 h-full">
      {/* Header */}
      <div className="p-5 border-b border-border bg-gradient-to-r from-green-500/5 to-emerald-500/5">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="p-2.5 rounded-xl bg-gradient-to-br from-green-500/20 to-emerald-500/20 ring-1 ring-green-500/30">
              <Activity className="w-5 h-5 text-green-400" />
            </div>
            <div>
              <h2 className="font-semibold flex items-center gap-2">
                Active Simulations
                {simulations.length > 0 && (
                  <span className="text-xs bg-green-500/10 text-green-400 px-2 py-0.5 rounded-full ring-1 ring-green-500/20">
                    {simulations.length} running
                  </span>
                )}
              </h2>
              <p className="text-xs text-muted-foreground">Real-time data generation</p>
            </div>
          </div>
          
          {simulations.length > 0 && (
            <div className="flex items-center gap-2">
              <span className="relative flex h-2.5 w-2.5">
                <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-green-400 opacity-75"></span>
                <span className="relative inline-flex rounded-full h-2.5 w-2.5 bg-green-500"></span>
              </span>
              <span className="text-xs text-green-400">Live</span>
            </div>
          )}
        </div>
      </div>
      
      {/* Content */}
      <div className="p-5">
        {simulations.length === 0 ? (
          <EmptyState readOnly={readOnly} />
        ) : (
          <div className="space-y-4 max-h-[600px] overflow-y-auto pr-2">
            {simulations.map((sim) => (
              <SimulationCard 
                key={sim.streamId} 
                simulation={sim} 
                onStop={() => handleStop(sim.streamId)}
                readOnly={readOnly}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

function EmptyState({ readOnly }: { readOnly: boolean }) {
  return (
    <div className="text-center py-16">
      <div className="w-16 h-16 rounded-2xl bg-gradient-to-br from-muted/50 to-muted/30 flex items-center justify-center mx-auto mb-4">
        <Radio className="w-8 h-8 text-muted-foreground/50" />
      </div>
      <h3 className="font-medium text-muted-foreground mb-2">No Active Simulations</h3>
      {!readOnly && (
        <p className="text-sm text-muted-foreground/70 max-w-xs mx-auto">
          Create a sensor stream and click the play button to start generating simulated data.
        </p>
      )}
    </div>
  );
}

interface SimulationCardProps {
  simulation: SimulationState;
  onStop: () => void;
  readOnly?: boolean;
}

function SimulationCard({ simulation, onStop, readOnly = false }: SimulationCardProps) {
  const { stream, profile, lastValue, streamId } = simulation;
  const streamTypeLabel = StreamTypeLabels[stream.streamType as StreamType] || StreamType[stream.streamType];
  const unitLabel = UnitLabels[stream.unit as Unit] || '';
  const behaviorLabel = SimulationBehaviorLabels[profile.behavior] || SimulationBehavior[profile.behavior];

  // Calculate percentage within range for visual indicator
  const range = profile.max - profile.min;
  const percentage = range > 0 ? ((lastValue - profile.min) / range) * 100 : 50;

  return (
    <div className="group relative overflow-hidden rounded-xl border border-green-500/20 bg-gradient-to-br from-green-500/5 to-emerald-500/5 hover:border-green-500/30 transition-all">
      {/* Animated Background Glow */}
      <div className="absolute inset-0 bg-gradient-to-r from-green-500/0 via-green-500/5 to-green-500/0 animate-pulse" />
      
      <div className="relative p-4">
        <div className="flex items-start justify-between gap-4">
          {/* Left: Stream Info */}
          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2 mb-2">
              <span className="inline-flex items-center gap-1.5 px-2 py-1 rounded-md bg-green-500/10 text-green-400 text-xs font-medium ring-1 ring-green-500/20">
                <Waves className="w-3 h-3" />
                {streamTypeLabel}
              </span>
              <span className="text-xs text-muted-foreground font-mono">
                {streamId.slice(0, 8)}...
              </span>
            </div>
            
            <h3 className="font-medium text-foreground truncate mb-3">
              {stream.displayName}
            </h3>
            
            {/* Profile Details */}
            <div className="grid grid-cols-4 gap-2 text-xs">
              <DetailItem label="Min" value={`${profile.min}${unitLabel}`} />
              <DetailItem label="Max" value={`${profile.max}${unitLabel}`} />
              <DetailItem label="Noise" value={`±${profile.noise}`} />
              <DetailItem label="Pattern" value={behaviorLabel} />
            </div>
          </div>
          
          {/* Right: Value Display */}
          <div className="flex items-center gap-4">
            <div className="text-right">
              <div className="flex items-center gap-2 justify-end mb-1">
                <TrendingUp className="w-4 h-4 text-green-400" />
                <span className="text-2xl font-bold font-mono text-green-400">
                  {lastValue.toFixed(2)}
                </span>
              </div>
              <div className="text-xs text-muted-foreground">{unitLabel || 'value'}</div>
              
              {/* Visual Range Indicator */}
              <div className="mt-2 w-24 h-1.5 rounded-full bg-muted/50 overflow-hidden">
                <div 
                  className="h-full bg-gradient-to-r from-green-500 to-emerald-400 rounded-full transition-all duration-500"
                  style={{ width: `${Math.min(100, Math.max(0, percentage))}%` }}
                />
              </div>
            </div>
            
            {!readOnly && (
              <button
                onClick={onStop}
                className="p-2.5 rounded-lg text-red-400 hover:bg-red-500/10 hover:text-red-300 
                         border border-transparent hover:border-red-500/20 transition-all"
                title="Stop simulation"
              >
                <Square className="w-5 h-5" />
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

function DetailItem({ label, value }: { label: string; value: string }) {
  return (
    <div className="bg-muted/30 rounded-md px-2 py-1.5">
      <div className="text-muted-foreground text-[10px] uppercase tracking-wider">{label}</div>
      <div className="text-foreground font-medium truncate">{value}</div>
    </div>
  );
}
