'use client';

import React, { useState } from 'react';
import { 
  Zap, 
  Play, 
  Square, 
  ChevronDown, 
  ChevronRight,
  Hash
} from 'lucide-react';
import {
  StreamType,
  StreamTypeLabels,
  simulationService
} from '@/features/telemetry/services/simulation.service';

interface GlobalControlsProps {
  onRefresh: () => void;
}

export default function GlobalControls({ onRefresh }: GlobalControlsProps) {
  const [streamIdInput, setStreamIdInput] = useState('');
  const [isToggling, setIsToggling] = useState(false);
  const [isExpanded, setIsExpanded] = useState(false);

  const handleToggleById = async () => {
    if (!streamIdInput.trim()) return;
    setIsToggling(true);
    try {
      await simulationService.toggle(streamIdInput.trim());
      setStreamIdInput('');
      onRefresh();
    } catch (err) {
      console.error('Error toggling stream:', err);
    } finally {
      setIsToggling(false);
    }
  };

  const handleStartType = async (type: StreamType) => {
    try {
      await simulationService.start(type);
      onRefresh();
    } catch (err) {
      console.error('Error starting simulation:', err);
    }
  };

  const handleStopType = async (type: StreamType) => {
    try {
      await simulationService.stop(type);
      onRefresh();
    } catch (err) {
      console.error('Error stopping simulation:', err);
    }
  };

  // Common stream types for quick access
  const quickTypes = [
    StreamType.Temperature,
    StreamType.Humidity,
    StreamType.Co2,
    StreamType.Ph,
    StreamType.Ec,
    StreamType.WaterLevel,
    StreamType.LightPpfd
  ];

  return (
    <div className="rounded-2xl border border-border bg-gradient-to-br from-card to-card/50 overflow-hidden">
      {/* Header */}
      <button
        onClick={() => setIsExpanded(!isExpanded)}
        className="w-full p-5 flex items-center justify-between hover:bg-muted/30 transition-colors border-b border-border bg-gradient-to-r from-amber-500/5 to-orange-500/5"
      >
        <div className="flex items-center gap-3">
          <div className="p-2.5 rounded-xl bg-gradient-to-br from-amber-500/20 to-orange-500/20 ring-1 ring-amber-500/30">
            <Zap className="w-5 h-5 text-amber-400" />
          </div>
          <div className="text-left">
            <h2 className="font-semibold">Global Controls</h2>
            <p className="text-xs text-muted-foreground">Bulk simulation controls</p>
          </div>
        </div>
        {isExpanded ? (
          <ChevronDown className="w-5 h-5 text-muted-foreground" />
        ) : (
          <ChevronRight className="w-5 h-5 text-muted-foreground" />
        )}
      </button>

      {isExpanded && (
        <div className="p-5 space-y-5">
          {/* Quick Type Controls */}
          <div>
            <label className="text-sm font-medium mb-3 block flex items-center gap-2">
              <Zap className="w-4 h-4 text-amber-400" />
              Quick Actions by Type
            </label>
            <p className="text-xs text-muted-foreground mb-4">
              Start or stop all simulations for a specific stream type
            </p>
            <div className="grid grid-cols-1 gap-2">
              {quickTypes.map((type) => (
                <TypeControlRow 
                  key={type}
                  type={type}
                  onStart={() => handleStartType(type)}
                  onStop={() => handleStopType(type)}
                />
              ))}
            </div>
          </div>

          {/* Divider */}
          <div className="border-t border-border" />

          {/* Direct Stream ID Toggle */}
          <div>
            <label className="text-sm font-medium mb-3 block flex items-center gap-2">
              <Hash className="w-4 h-4 text-amber-400" />
              Toggle by Stream ID
            </label>
            <p className="text-xs text-muted-foreground mb-3">
              Enter a specific stream UUID to toggle its simulation
            </p>
            <div className="flex gap-2">
              <input
                type="text"
                value={streamIdInput}
                onChange={(e) => setStreamIdInput(e.target.value)}
                placeholder="Enter stream UUID..."
                className="flex-1 px-3 py-2.5 rounded-lg border border-border bg-background/50 text-sm font-mono
                         focus:outline-none focus:ring-2 focus:ring-amber-500/30 focus:border-amber-500/50"
                onKeyDown={(e) => e.key === 'Enter' && handleToggleById()}
              />
              <button
                onClick={handleToggleById}
                disabled={!streamIdInput.trim() || isToggling}
                className="px-4 py-2.5 bg-gradient-to-r from-amber-500 to-orange-500 text-white rounded-lg 
                         text-sm font-medium hover:from-amber-600 hover:to-orange-600 
                         disabled:opacity-50 disabled:cursor-not-allowed transition-all
                         shadow-lg shadow-amber-500/20"
              >
                {isToggling ? 'Toggling...' : 'Toggle'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

interface TypeControlRowProps {
  type: StreamType;
  onStart: () => void;
  onStop: () => void;
}

function TypeControlRow({ type, onStart, onStop }: TypeControlRowProps) {
  return (
    <div className="flex items-center justify-between p-3 rounded-xl bg-muted/20 border border-border hover:border-amber-500/20 transition-colors">
      <span className="text-sm font-medium">{StreamTypeLabels[type]}</span>
      <div className="flex items-center gap-1">
        <button 
          onClick={onStart}
          className="p-2 rounded-lg text-green-400 hover:bg-green-500/10 hover:text-green-300 transition-colors"
          title={`Start all ${StreamTypeLabels[type]} simulations`}
        >
          <Play className="w-4 h-4" />
        </button>
        <button 
          onClick={onStop}
          className="p-2 rounded-lg text-red-400 hover:bg-red-500/10 hover:text-red-300 transition-colors"
          title={`Stop all ${StreamTypeLabels[type]} simulations`}
        >
          <Square className="w-4 h-4" />
        </button>
      </div>
    </div>
  );
}
