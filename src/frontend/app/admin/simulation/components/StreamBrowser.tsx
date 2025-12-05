'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { 
  Database, 
  Play, 
  Square, 
  RefreshCw, 
  ChevronDown, 
  ChevronRight, 
  Search,
  Radio
} from 'lucide-react';
import { 
  provisioningService, 
  Site, 
  SensorStream 
} from '@/features/telemetry/services/provisioning.service';
import {
  StreamType,
  StreamTypeLabels,
  UnitLabels,
  Unit,
  simulationService,
  SimulationState
} from '@/features/telemetry/services/simulation.service';

interface StreamBrowserProps {
  activeSimulations: SimulationState[];
  onRefresh: () => void;
}

export default function StreamBrowser({ activeSimulations, onRefresh }: StreamBrowserProps) {
  const [sites, setSites] = useState<Site[]>([]);
  const [selectedSiteId, setSelectedSiteId] = useState('');
  const [streams, setStreams] = useState<SensorStream[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isExpanded, setIsExpanded] = useState(true);
  const [filterType, setFilterType] = useState<StreamType | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [togglingStreams, setTogglingStreams] = useState<Set<string>>(new Set());

  // Fetch sites on mount
  useEffect(() => {
    const fetchSites = async () => {
      try {
        const s = await provisioningService.getSites();
        setSites(s);
      } catch (err) {
        console.error('Error fetching sites:', err);
      }
    };
    fetchSites();
  }, []);

  // Fetch streams when site changes
  const fetchStreams = useCallback(async () => {
    if (!selectedSiteId) {
      setStreams([]);
      return;
    }
    setIsLoading(true);
    try {
      const s = await provisioningService.getSensorStreams(selectedSiteId);
      setStreams(s);
    } catch (err) {
      console.error('Error fetching streams:', err);
      setStreams([]);
    } finally {
      setIsLoading(false);
    }
  }, [selectedSiteId]);

  useEffect(() => {
    fetchStreams();
  }, [fetchStreams]);

  // Check if a stream is actively simulating
  const isSimulating = (streamId: string) => {
    return activeSimulations.some(s => s.streamId === streamId);
  };

  // Toggle simulation for a stream
  const handleToggle = async (streamId: string) => {
    setTogglingStreams(prev => new Set(prev).add(streamId));
    try {
      await simulationService.toggle(streamId);
      onRefresh();
    } catch (err) {
      console.error('Error toggling simulation:', err);
    } finally {
      setTogglingStreams(prev => {
        const next = new Set(prev);
        next.delete(streamId);
        return next;
      });
    }
  };

  // Filter and search streams
  const filteredStreams = streams
    .filter(s => filterType === null || s.streamType === filterType)
    .filter(s => !searchQuery || s.displayName.toLowerCase().includes(searchQuery.toLowerCase()));

  // Get unique stream types from current streams
  const availableTypes = Array.from(new Set(streams.map(s => s.streamType)));

  return (
    <div className="rounded-2xl border border-border bg-gradient-to-br from-card to-card/50 overflow-hidden">
      {/* Header */}
      <button
        onClick={() => setIsExpanded(!isExpanded)}
        className="w-full p-5 flex items-center justify-between hover:bg-muted/30 transition-colors border-b border-border bg-gradient-to-r from-blue-500/5 to-indigo-500/5"
      >
        <div className="flex items-center gap-3">
          <div className="p-2.5 rounded-xl bg-gradient-to-br from-blue-500/20 to-indigo-500/20 ring-1 ring-blue-500/30">
            <Database className="w-5 h-5 text-blue-400" />
          </div>
          <div className="text-left">
            <h2 className="font-semibold flex items-center gap-2">
              Stream Browser
              {streams.length > 0 && (
                <span className="text-xs bg-blue-500/10 text-blue-400 px-2 py-0.5 rounded-full">
                  {filteredStreams.length}
                </span>
              )}
            </h2>
            <p className="text-xs text-muted-foreground">Browse and toggle sensor streams</p>
          </div>
        </div>
        {isExpanded ? (
          <ChevronDown className="w-5 h-5 text-muted-foreground" />
        ) : (
          <ChevronRight className="w-5 h-5 text-muted-foreground" />
        )}
      </button>

      {isExpanded && (
        <div className="p-5 space-y-4">
          {/* Site Selector */}
          <div className="flex gap-2">
            <select
              className="flex-1 px-3 py-2.5 rounded-lg border border-border bg-background/50 text-sm 
                       focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-500/50"
              value={selectedSiteId}
              onChange={e => setSelectedSiteId(e.target.value)}
            >
              <option value="">Select a site...</option>
              {sites.map(s => (
                <option key={s.id} value={s.id}>{s.name}</option>
              ))}
            </select>
            <button
              onClick={fetchStreams}
              disabled={!selectedSiteId || isLoading}
              className="p-2.5 rounded-lg border border-border bg-background/50 hover:bg-muted/50 
                       transition-colors disabled:opacity-50"
            >
              <RefreshCw className={`w-4 h-4 ${isLoading ? 'animate-spin text-blue-400' : ''}`} />
            </button>
          </div>

          {/* Search and Filter */}
          {streams.length > 0 && (
            <div className="space-y-3">
              {/* Search */}
              <div className="relative">
                <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
                <input
                  type="text"
                  placeholder="Search streams..."
                  value={searchQuery}
                  onChange={e => setSearchQuery(e.target.value)}
                  className="w-full pl-10 pr-4 py-2 rounded-lg border border-border bg-background/50 text-sm 
                           focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-500/50"
                />
              </div>
              
              {/* Type Filters */}
              <div className="flex items-center gap-2 flex-wrap">
                <button
                  onClick={() => setFilterType(null)}
                  className={`px-2.5 py-1 text-xs rounded-lg transition-all ${
                    filterType === null 
                      ? 'bg-blue-500 text-white shadow-lg shadow-blue-500/20' 
                      : 'bg-muted/50 hover:bg-muted text-muted-foreground'
                  }`}
                >
                  All
                </button>
                {availableTypes.map(type => (
                  <button
                    key={type}
                    onClick={() => setFilterType(type)}
                    className={`px-2.5 py-1 text-xs rounded-lg transition-all ${
                      filterType === type 
                        ? 'bg-blue-500 text-white shadow-lg shadow-blue-500/20' 
                        : 'bg-muted/50 hover:bg-muted text-muted-foreground'
                    }`}
                  >
                    {StreamTypeLabels[type as StreamType] || StreamType[type]}
                  </button>
                ))}
              </div>
            </div>
          )}

          {/* Stream List */}
          {!selectedSiteId ? (
            <EmptyState message="Select a site to browse streams" />
          ) : isLoading ? (
            <div className="text-center py-8">
              <RefreshCw className="w-6 h-6 animate-spin text-blue-400 mx-auto mb-2" />
              <p className="text-sm text-muted-foreground">Loading streams...</p>
            </div>
          ) : filteredStreams.length === 0 ? (
            <EmptyState 
              message={streams.length === 0 
                ? 'No streams found for this site' 
                : 'No streams match your search'
              } 
            />
          ) : (
            <div className="space-y-2 max-h-[280px] overflow-y-auto pr-1">
              {filteredStreams.map(stream => {
                const simulating = isSimulating(stream.id);
                const toggling = togglingStreams.has(stream.id);
                const activeState = activeSimulations.find(s => s.streamId === stream.id);
                
                return (
                  <StreamCard
                    key={stream.id}
                    stream={stream}
                    simulating={simulating}
                    toggling={toggling}
                    activeState={activeState}
                    onToggle={() => handleToggle(stream.id)}
                  />
                );
              })}
            </div>
          )}
        </div>
      )}
    </div>
  );
}

function EmptyState({ message }: { message: string }) {
  return (
    <div className="text-center py-8 border-2 border-dashed border-border/50 rounded-xl">
      <Radio className="w-8 h-8 text-muted-foreground/30 mx-auto mb-2" />
      <p className="text-sm text-muted-foreground">{message}</p>
    </div>
  );
}

interface StreamCardProps {
  stream: SensorStream;
  simulating: boolean;
  toggling: boolean;
  activeState?: SimulationState;
  onToggle: () => void;
}

function StreamCard({ stream, simulating, toggling, activeState, onToggle }: StreamCardProps) {
  return (
    <div 
      className={`flex items-center justify-between p-3 rounded-xl border transition-all ${
        simulating 
          ? 'border-green-500/30 bg-gradient-to-r from-green-500/5 to-emerald-500/5' 
          : 'border-border bg-muted/20 hover:bg-muted/30'
      }`}
    >
      <div className="min-w-0 flex-1">
        <div className="flex items-center gap-2 mb-1">
          <span className={`text-xs px-2 py-0.5 rounded-md font-medium ${
            simulating 
              ? 'bg-green-500/20 text-green-400' 
              : 'bg-blue-500/10 text-blue-400'
          }`}>
            {StreamTypeLabels[stream.streamType as StreamType] || StreamType[stream.streamType]}
          </span>
        </div>
        <div className="text-sm font-medium truncate">{stream.displayName}</div>
        <div className="text-xs text-muted-foreground flex items-center gap-2 mt-0.5">
          <span>{UnitLabels[stream.unit as Unit] || stream.unit}</span>
          {simulating && activeState && (
            <>
              <span className="text-muted-foreground/50">•</span>
              <span className="text-green-400 font-mono font-medium">
                {activeState.lastValue.toFixed(2)}
              </span>
            </>
          )}
        </div>
      </div>
      
      <button
        onClick={onToggle}
        disabled={toggling}
        className={`p-2.5 rounded-lg transition-all disabled:opacity-50 ${
          simulating
            ? 'text-red-400 hover:bg-red-500/10 hover:text-red-300'
            : 'text-green-400 hover:bg-green-500/10 hover:text-green-300'
        }`}
      >
        {toggling ? (
          <RefreshCw className="w-4 h-4 animate-spin" />
        ) : simulating ? (
          <Square className="w-4 h-4" />
        ) : (
          <Play className="w-4 h-4" />
        )}
      </button>
    </div>
  );
}
