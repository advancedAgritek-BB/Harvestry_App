import React, { useMemo } from 'react';
import { cn } from '@/lib/utils';
import { X, Droplets, Clock, MapPin, Filter, Hand, Zap } from 'lucide-react';
import type { IrrigationShotLog, IrrigationPeriod } from './types';
import { IRRIGATION_COLORS } from './types';

interface HistoryPanelProps {
  isOpen: boolean;
  onClose: () => void;
  logs: IrrigationShotLog[];
  filterPeriod?: IrrigationPeriod;
  filterZones?: string[];
}

// Mock shot logs for demonstration
const MOCK_SHOT_LOGS: IrrigationShotLog[] = [
  {
    id: '1',
    timestamp: new Date(Date.now() - 30 * 60 * 1000).toISOString(),
    volume: 100,
    type: 'automated',
    zones: ['A', 'B', 'C'],
    period: 'P1 - Ramp',
    endVwc: 48.2,
  },
  {
    id: '2',
    timestamp: new Date(Date.now() - 90 * 60 * 1000).toISOString(),
    volume: 75,
    type: 'manual',
    zones: ['D', 'E'],
    period: 'P1 - Ramp',
    triggeredBy: 'John Doe',
    endVwc: 52.1,
  },
  {
    id: '3',
    timestamp: new Date(Date.now() - 150 * 60 * 1000).toISOString(),
    volume: 125,
    type: 'automated',
    zones: ['A', 'B', 'C', 'D', 'E', 'F'],
    period: 'P2 - Maintenance',
    endVwc: 45.8,
  },
  {
    id: '4',
    timestamp: new Date(Date.now() - 240 * 60 * 1000).toISOString(),
    volume: 50,
    type: 'manual',
    zones: ['F'],
    period: 'P2 - Maintenance',
    triggeredBy: 'Jane Smith',
    endVwc: 38.5,
  },
  {
    id: '5',
    timestamp: new Date(Date.now() - 360 * 60 * 1000).toISOString(),
    volume: 100,
    type: 'automated',
    zones: ['A', 'B'],
    period: 'P3 - Dryback',
    endVwc: 32.1,
  },
];

function formatTimestamp(isoString: string): { time: string; date: string } {
  const date = new Date(isoString);
  return {
    time: date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
    date: date.toLocaleDateString([], { month: 'short', day: 'numeric' }),
  };
}

function formatTimeAgo(isoString: string): string {
  const now = Date.now();
  const then = new Date(isoString).getTime();
  const diffMinutes = Math.floor((now - then) / (1000 * 60));
  
  if (diffMinutes < 60) return `${diffMinutes}m ago`;
  if (diffMinutes < 1440) return `${Math.floor(diffMinutes / 60)}h ago`;
  return `${Math.floor(diffMinutes / 1440)}d ago`;
}

/**
 * Slide-over panel displaying irrigation shot history.
 * Filters by period and zones.
 */
export function HistoryPanel({
  isOpen,
  onClose,
  logs = MOCK_SHOT_LOGS,
  filterPeriod,
  filterZones,
}: HistoryPanelProps) {
  // Filter logs based on period and zones
  const filteredLogs = useMemo(() => {
    let result = logs;
    
    if (filterPeriod && filterPeriod !== 'All') {
      result = result.filter(log => log.period === filterPeriod);
    }
    
    if (filterZones && filterZones.length > 0) {
      result = result.filter(log => 
        log.zones.some(zone => filterZones.includes(zone))
      );
    }
    
    return result.sort((a, b) => 
      new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime()
    );
  }, [logs, filterPeriod, filterZones]);

  if (!isOpen) return null;

  return (
    <>
      {/* Backdrop */}
      <div 
        className="fixed inset-0 z-40 bg-background/60 backdrop-blur-sm animate-in fade-in duration-200"
        onClick={onClose}
      />
      
      {/* Panel */}
      <div className="fixed right-0 top-0 bottom-0 z-50 w-full max-w-md bg-surface border-l border-border shadow-2xl animate-in slide-in-from-right duration-300">
        {/* Header */}
        <div className="flex items-center justify-between p-4 border-b border-border">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-cyan-500/20 flex items-center justify-center">
              <Clock className="w-5 h-5 text-cyan-400" />
            </div>
            <div>
              <h2 className="text-lg font-semibold text-foreground">Shot History</h2>
              <p className="text-xs text-muted-foreground">
                {filteredLogs.length} shot{filteredLogs.length !== 1 ? 's' : ''}
                {filterPeriod && filterPeriod !== 'All' && ` in ${filterPeriod}`}
              </p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-2 text-muted-foreground hover:text-foreground rounded-lg hover:bg-muted transition-colors"
            title="Close history"
            aria-label="Close history panel"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Active Filters */}
        {(filterPeriod || (filterZones && filterZones.length > 0)) && (
          <div className="px-4 py-2 border-b border-border/50 flex items-center gap-2 flex-wrap">
            <Filter className="w-3.5 h-3.5 text-muted-foreground" />
            {filterPeriod && filterPeriod !== 'All' && (
              <span className="px-2 py-0.5 text-[10px] font-medium bg-cyan-500/20 text-cyan-300 rounded">
                {filterPeriod}
              </span>
            )}
            {filterZones && filterZones.length > 0 && (
              <span className="px-2 py-0.5 text-[10px] font-medium bg-blue-500/20 text-blue-300 rounded">
                Zones: {filterZones.slice(0, 4).join(', ')}{filterZones.length > 4 && '...'}
              </span>
            )}
          </div>
        )}

        {/* Log List */}
        <div className="flex-1 overflow-y-auto p-4 space-y-3" style={{ maxHeight: 'calc(100vh - 140px)' }}>
          {filteredLogs.length === 0 ? (
            <div className="text-center py-12">
              <Droplets className="w-10 h-10 text-muted-foreground/30 mx-auto mb-3" />
              <p className="text-sm text-muted-foreground">No shots found</p>
              <p className="text-xs text-muted-foreground/60 mt-1">
                Try adjusting your filters
              </p>
            </div>
          ) : (
            filteredLogs.map(log => {
              const isManual = log.type === 'manual';
              const { time, date } = formatTimestamp(log.timestamp);
              const timeAgo = formatTimeAgo(log.timestamp);
              
              return (
                <div
                  key={log.id}
                  className="bg-muted/30 border border-border/50 rounded-xl p-3 hover:bg-muted/50 transition-colors"
                >
                  {/* Header Row */}
                  <div className="flex items-start justify-between mb-2">
                    <div className="flex items-center gap-2">
                      <div className={cn(
                        "w-8 h-8 rounded-lg flex items-center justify-center",
                        isManual ? "bg-amber-500/20" : "bg-blue-500/20"
                      )}>
                        {isManual ? (
                          <Hand className="w-4 h-4 text-amber-400" />
                        ) : (
                          <Zap className="w-4 h-4 text-blue-400" />
                        )}
                      </div>
                      <div>
                        <div className="text-sm font-semibold text-foreground">
                          {log.volume} mL
                        </div>
                        <div className={cn(
                          "text-[10px] font-medium uppercase tracking-wider",
                          isManual ? "text-amber-400" : "text-blue-400"
                        )}>
                          {isManual ? 'Manual' : 'Automated'}
                        </div>
                      </div>
                    </div>
                    <div className="text-right">
                      <div className="text-xs font-medium text-foreground">{time}</div>
                      <div className="text-[10px] text-muted-foreground">{date} · {timeAgo}</div>
                    </div>
                  </div>

                  {/* Details */}
                  <div className="grid grid-cols-2 gap-2 text-xs">
                    <div className="flex items-center gap-1.5">
                      <MapPin className="w-3 h-3 text-muted-foreground" />
                      <span className="text-muted-foreground">Zones:</span>
                      <span className="text-foreground font-medium">
                        {log.zones.slice(0, 3).join(', ')}{log.zones.length > 3 && `+${log.zones.length - 3}`}
                      </span>
                    </div>
                    {log.endVwc !== undefined && (
                      <div className="flex items-center gap-1.5">
                        <div 
                          className="w-3 h-3 rounded-sm" 
                          style={{ backgroundColor: IRRIGATION_COLORS.vwc }}
                        />
                        <span className="text-muted-foreground">VWC:</span>
                        <span className="text-foreground font-medium">{log.endVwc.toFixed(1)}%</span>
                      </div>
                    )}
                  </div>

                  {/* Triggered By */}
                  {log.triggeredBy && (
                    <div className="mt-2 pt-2 border-t border-border/30 text-[10px] text-muted-foreground">
                      Triggered by: <span className="text-foreground/70">{log.triggeredBy}</span>
                    </div>
                  )}

                  {/* Period Badge */}
                  <div className="mt-2">
                    <span className="px-1.5 py-0.5 text-[9px] font-medium bg-surface border border-border rounded text-muted-foreground">
                      {log.period}
                    </span>
                  </div>
                </div>
              );
            })
          )}
        </div>
      </div>
    </>
  );
}



