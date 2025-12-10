import React, { useMemo, useState } from 'react';
import { cn } from '@/lib/utils';
import { 
  X, 
  TrendingUp, 
  TrendingDown, 
  Target, 
  Clock, 
  Droplets, 
  AlertTriangle,
  CheckCircle2,
  XCircle,
  ChevronDown,
  ChevronUp,
  Calendar
} from 'lucide-react';
import type { 
  IrrigationComparisonEvent, 
  IrrigationPerformanceSummary,
  IrrigationPeriod 
} from './types';
import { IRRIGATION_COLORS } from './types';

interface PerformancePanelProps {
  isOpen: boolean;
  onClose: () => void;
  filterPeriod?: IrrigationPeriod;
}

// Mock comparison data for demonstration
const MOCK_COMPARISON_EVENTS: IrrigationComparisonEvent[] = [
  {
    id: '1',
    time: '08:00',
    date: new Date().toLocaleDateString(),
    period: 'P1 - Ramp',
    zone: 'A',
    expected: { volume: 50, vwc: 35 },
    actual: { volume: 50, vwc: 36.2, executedAt: '08:02', type: 'automated' },
    variance: { volumeDelta: 0, volumePercent: 0, vwcDelta: 1.2, timingDelta: 2, missed: false },
  },
  {
    id: '2',
    time: '08:30',
    date: new Date().toLocaleDateString(),
    period: 'P1 - Ramp',
    zone: 'B',
    expected: { volume: 100, vwc: 42 },
    actual: { volume: 100, vwc: 40.5, executedAt: '08:31', type: 'automated' },
    variance: { volumeDelta: 0, volumePercent: 0, vwcDelta: -1.5, timingDelta: 1, missed: false },
  },
  {
    id: '3',
    time: '09:00',
    date: new Date().toLocaleDateString(),
    period: 'P1 - Ramp',
    zone: 'C',
    expected: { volume: 100, vwc: 48 },
    actual: { volume: 75, vwc: 44.8, executedAt: '09:05', type: 'manual' },
    variance: { volumeDelta: -25, volumePercent: -25, vwcDelta: -3.2, timingDelta: 5, missed: false },
  },
  {
    id: '4',
    time: '09:30',
    date: new Date().toLocaleDateString(),
    period: 'P1 - Ramp',
    zone: 'D',
    expected: { volume: 100, vwc: 52 },
    actual: { volume: null, vwc: null, executedAt: null, type: null },
    variance: { volumeDelta: null, volumePercent: null, vwcDelta: null, timingDelta: null, missed: true },
  },
  {
    id: '5',
    time: '12:00',
    date: new Date().toLocaleDateString(),
    period: 'P2 - Maintenance',
    zone: 'A',
    expected: { volume: 100, vwc: 60 },
    actual: { volume: 110, vwc: 62.1, executedAt: '11:58', type: 'automated' },
    variance: { volumeDelta: 10, volumePercent: 10, vwcDelta: 2.1, timingDelta: -2, missed: false },
  },
];

const MOCK_SUMMARY: IrrigationPerformanceSummary = {
  dateRange: { 
    start: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000).toLocaleDateString(), 
    end: new Date().toLocaleDateString() 
  },
  totalExpectedEvents: 84,
  totalActualEvents: 81,
  missedEvents: 3,
  extraEvents: 2,
  volume: {
    totalExpected: 8400,
    totalActual: 8150,
    averageVariance: -3.1,
    variancePercent: -2.98,
  },
  vwc: {
    averageExpected: 55.2,
    averageActual: 54.8,
    averageVariance: -0.4,
    withinTargetPercent: 94.2,
  },
  timing: {
    averageDelayMinutes: 1.8,
    onTimePercent: 96.3,
  },
  alignmentScore: 92,
};

function getVarianceColor(value: number | null, threshold: number = 5): string {
  if (value === null) return 'text-muted-foreground';
  const abs = Math.abs(value);
  if (abs <= threshold * 0.5) return 'text-emerald-400';
  if (abs <= threshold) return 'text-amber-400';
  return 'text-rose-400';
}

function getAlignmentScoreColor(score: number): string {
  if (score >= 90) return 'text-emerald-400';
  if (score >= 75) return 'text-amber-400';
  return 'text-rose-400';
}

function formatVariance(value: number | null, unit: string = '', showSign: boolean = true): string {
  if (value === null) return '—';
  const sign = showSign && value > 0 ? '+' : '';
  return `${sign}${value.toFixed(1)}${unit}`;
}

/**
 * Performance panel showing expected vs actual irrigation comparison.
 * Helps users identify alignment issues and make adjustments.
 */
export function PerformancePanel({
  isOpen,
  onClose,
  filterPeriod,
}: PerformancePanelProps) {
  const [expandedEvent, setExpandedEvent] = useState<string | null>(null);
  const [showAllEvents, setShowAllEvents] = useState(false);

  // Filter events based on period
  const filteredEvents = useMemo(() => {
    let events = MOCK_COMPARISON_EVENTS;
    if (filterPeriod && filterPeriod !== 'All') {
      events = events.filter(e => e.period === filterPeriod);
    }
    return showAllEvents ? events : events.slice(0, 5);
  }, [filterPeriod, showAllEvents]);

  const summary = MOCK_SUMMARY;

  if (!isOpen) return null;

  return (
    <>
      {/* Backdrop */}
      <div 
        className="fixed inset-0 z-40 bg-background/60 backdrop-blur-sm animate-in fade-in duration-200"
        onClick={onClose}
      />
      
      {/* Panel */}
      <div className="fixed right-0 top-0 bottom-0 z-50 w-full max-w-lg bg-surface border-l border-border shadow-2xl animate-in slide-in-from-right duration-300 flex flex-col">
        {/* Header */}
        <div className="flex items-center justify-between p-4 border-b border-border shrink-0">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-purple-500/20 flex items-center justify-center">
              <Target className="w-5 h-5 text-purple-400" />
            </div>
            <div>
              <h2 className="text-lg font-semibold text-foreground">Schedule Performance</h2>
              <p className="text-xs text-muted-foreground">
                Expected vs Actual Analysis
              </p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-2 text-muted-foreground hover:text-foreground rounded-lg hover:bg-muted transition-colors"
            aria-label="Close performance panel"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Content - Scrollable */}
        <div className="flex-1 overflow-y-auto p-4 space-y-4">
          {/* Alignment Score Card */}
          <div className="bg-muted/30 border border-border/50 rounded-xl p-4">
            <div className="flex items-center justify-between mb-3">
              <span className="text-xs font-medium text-muted-foreground uppercase tracking-wider">
                Alignment Score
              </span>
              <div className="flex items-center gap-1 text-[10px] text-muted-foreground">
                <Calendar className="w-3 h-3" />
                {summary.dateRange.start} - {summary.dateRange.end}
              </div>
            </div>
            <div className="flex items-center gap-4">
              <div className={cn(
                "text-4xl font-bold",
                getAlignmentScoreColor(summary.alignmentScore)
              )}>
                {summary.alignmentScore}
              </div>
              <div className="flex-1">
                <div className="h-2 bg-muted rounded-full overflow-hidden">
                  <div 
                    className={cn(
                      "h-full rounded-full transition-all",
                      summary.alignmentScore >= 90 ? "bg-emerald-500" :
                      summary.alignmentScore >= 75 ? "bg-amber-500" : "bg-rose-500"
                    )}
                    style={{ width: `${summary.alignmentScore}%` }}
                  />
                </div>
                <p className="text-[10px] text-muted-foreground mt-1">
                  {summary.alignmentScore >= 90 ? 'Excellent schedule alignment' :
                   summary.alignmentScore >= 75 ? 'Good alignment, minor adjustments recommended' :
                   'Schedule adjustments needed'}
                </p>
              </div>
            </div>
          </div>

          {/* Summary Metrics Grid */}
          <div className="grid grid-cols-2 gap-3">
            {/* Volume Variance */}
            <div className="bg-muted/30 border border-border/50 rounded-xl p-3">
              <div className="flex items-center gap-2 mb-2">
                <Droplets className="w-4 h-4 text-blue-400" />
                <span className="text-xs font-medium text-muted-foreground">Volume</span>
              </div>
              <div className={cn(
                "text-xl font-bold",
                getVarianceColor(summary.volume.variancePercent, 5)
              )}>
                {formatVariance(summary.volume.variancePercent, '%')}
              </div>
              <p className="text-[10px] text-muted-foreground mt-1">
                {summary.volume.totalActual.toLocaleString()} / {summary.volume.totalExpected.toLocaleString()} mL
              </p>
            </div>

            {/* VWC Variance */}
            <div className="bg-muted/30 border border-border/50 rounded-xl p-3">
              <div className="flex items-center gap-2 mb-2">
                <div className="w-4 h-4 rounded-sm" style={{ backgroundColor: IRRIGATION_COLORS.vwc }} />
                <span className="text-xs font-medium text-muted-foreground">VWC Accuracy</span>
              </div>
              <div className={cn(
                "text-xl font-bold",
                summary.vwc.withinTargetPercent >= 90 ? 'text-emerald-400' :
                summary.vwc.withinTargetPercent >= 75 ? 'text-amber-400' : 'text-rose-400'
              )}>
                {summary.vwc.withinTargetPercent.toFixed(0)}%
              </div>
              <p className="text-[10px] text-muted-foreground mt-1">
                Within ±2% of expected
              </p>
            </div>

            {/* Timing */}
            <div className="bg-muted/30 border border-border/50 rounded-xl p-3">
              <div className="flex items-center gap-2 mb-2">
                <Clock className="w-4 h-4 text-cyan-400" />
                <span className="text-xs font-medium text-muted-foreground">On-Time</span>
              </div>
              <div className={cn(
                "text-xl font-bold",
                summary.timing.onTimePercent >= 95 ? 'text-emerald-400' :
                summary.timing.onTimePercent >= 85 ? 'text-amber-400' : 'text-rose-400'
              )}>
                {summary.timing.onTimePercent.toFixed(0)}%
              </div>
              <p className="text-[10px] text-muted-foreground mt-1">
                Avg delay: {summary.timing.averageDelayMinutes.toFixed(1)} min
              </p>
            </div>

            {/* Missed Events */}
            <div className="bg-muted/30 border border-border/50 rounded-xl p-3">
              <div className="flex items-center gap-2 mb-2">
                <AlertTriangle className="w-4 h-4 text-amber-400" />
                <span className="text-xs font-medium text-muted-foreground">Events</span>
              </div>
              <div className="flex items-center gap-2">
                <span className={cn(
                  "text-xl font-bold",
                  summary.missedEvents === 0 ? 'text-emerald-400' : 'text-amber-400'
                )}>
                  {summary.missedEvents}
                </span>
                <span className="text-xs text-muted-foreground">missed</span>
              </div>
              <p className="text-[10px] text-muted-foreground mt-1">
                {summary.extraEvents} manual additions
              </p>
            </div>
          </div>

          {/* Event Details */}
          <div>
            <div className="flex items-center justify-between mb-3">
              <h3 className="text-sm font-medium text-foreground">Event Details</h3>
              <span className="text-[10px] text-muted-foreground">
                {filterPeriod && filterPeriod !== 'All' ? filterPeriod : 'All Periods'}
              </span>
            </div>
            
            <div className="space-y-2">
              {filteredEvents.map(event => (
                <div
                  key={event.id}
                  className="bg-muted/20 border border-border/50 rounded-lg overflow-hidden"
                >
                  {/* Event Header - Clickable */}
                  <button
                    onClick={() => setExpandedEvent(expandedEvent === event.id ? null : event.id)}
                    className="w-full p-3 flex items-center justify-between hover:bg-muted/30 transition-colors"
                  >
                    <div className="flex items-center gap-3">
                      {/* Status Icon */}
                      <div className={cn(
                        "w-8 h-8 rounded-lg flex items-center justify-center",
                        event.variance.missed ? "bg-rose-500/20" :
                        Math.abs(event.variance.vwcDelta || 0) <= 2 ? "bg-emerald-500/20" : "bg-amber-500/20"
                      )}>
                        {event.variance.missed ? (
                          <XCircle className="w-4 h-4 text-rose-400" />
                        ) : Math.abs(event.variance.vwcDelta || 0) <= 2 ? (
                          <CheckCircle2 className="w-4 h-4 text-emerald-400" />
                        ) : (
                          <AlertTriangle className="w-4 h-4 text-amber-400" />
                        )}
                      </div>
                      
                      <div className="text-left">
                        <div className="text-sm font-medium text-foreground">
                          {event.time} · Zone {event.zone}
                        </div>
                        <div className="text-[10px] text-muted-foreground">
                          {event.period}
                        </div>
                      </div>
                    </div>

                    <div className="flex items-center gap-3">
                      {/* Quick Variance Display */}
                      {!event.variance.missed && (
                        <div className="text-right">
                          <div className={cn(
                            "text-xs font-medium",
                            getVarianceColor(event.variance.vwcDelta, 2)
                          )}>
                            VWC {formatVariance(event.variance.vwcDelta, '%')}
                          </div>
                        </div>
                      )}
                      {event.variance.missed && (
                        <span className="text-xs font-medium text-rose-400">Missed</span>
                      )}
                      {expandedEvent === event.id ? (
                        <ChevronUp className="w-4 h-4 text-muted-foreground" />
                      ) : (
                        <ChevronDown className="w-4 h-4 text-muted-foreground" />
                      )}
                    </div>
                  </button>

                  {/* Expanded Details */}
                  {expandedEvent === event.id && (
                    <div className="px-3 pb-3 pt-0 border-t border-border/30">
                      <div className="grid grid-cols-3 gap-3 mt-3">
                        {/* Expected */}
                        <div>
                          <div className="text-[10px] font-medium text-muted-foreground uppercase tracking-wider mb-1">
                            Expected
                          </div>
                          <div className="text-xs text-foreground">
                            {event.expected.volume} mL
                          </div>
                          <div className="text-xs text-foreground">
                            {event.expected.vwc}% VWC
                          </div>
                        </div>

                        {/* Actual */}
                        <div>
                          <div className="text-[10px] font-medium text-muted-foreground uppercase tracking-wider mb-1">
                            Actual
                          </div>
                          {event.variance.missed ? (
                            <div className="text-xs text-rose-400">Not executed</div>
                          ) : (
                            <>
                              <div className="text-xs text-foreground">
                                {event.actual.volume} mL
                              </div>
                              <div className="text-xs text-foreground">
                                {event.actual.vwc?.toFixed(1)}% VWC
                              </div>
                            </>
                          )}
                        </div>

                        {/* Variance */}
                        <div>
                          <div className="text-[10px] font-medium text-muted-foreground uppercase tracking-wider mb-1">
                            Variance
                          </div>
                          {event.variance.missed ? (
                            <div className="text-xs text-rose-400">—</div>
                          ) : (
                            <>
                              <div className={cn(
                                "text-xs font-medium",
                                getVarianceColor(event.variance.volumePercent, 10)
                              )}>
                                {formatVariance(event.variance.volumeDelta, ' mL')}
                              </div>
                              <div className={cn(
                                "text-xs font-medium",
                                getVarianceColor(event.variance.vwcDelta, 2)
                              )}>
                                {formatVariance(event.variance.vwcDelta, '% VWC')}
                              </div>
                            </>
                          )}
                        </div>
                      </div>

                      {/* Timing Info */}
                      {!event.variance.missed && event.variance.timingDelta !== null && (
                        <div className="mt-2 pt-2 border-t border-border/30 flex items-center gap-2 text-[10px]">
                          <Clock className="w-3 h-3 text-muted-foreground" />
                          <span className="text-muted-foreground">Executed at {event.actual.executedAt}</span>
                          <span className={cn(
                            "font-medium",
                            Math.abs(event.variance.timingDelta) <= 2 ? "text-emerald-400" :
                            Math.abs(event.variance.timingDelta) <= 5 ? "text-amber-400" : "text-rose-400"
                          )}>
                            ({event.variance.timingDelta > 0 ? '+' : ''}{event.variance.timingDelta} min)
                          </span>
                        </div>
                      )}
                    </div>
                  )}
                </div>
              ))}
            </div>

            {/* Show More/Less Button */}
            {MOCK_COMPARISON_EVENTS.length > 5 && (
              <button
                onClick={() => setShowAllEvents(!showAllEvents)}
                className="w-full mt-3 py-2 text-xs font-medium text-muted-foreground hover:text-foreground transition-colors"
              >
                {showAllEvents ? 'Show Less' : `Show All (${MOCK_COMPARISON_EVENTS.length} events)`}
              </button>
            )}
          </div>

          {/* Recommendations */}
          <div className="bg-purple-500/10 border border-purple-500/20 rounded-xl p-4">
            <div className="flex items-center gap-2 mb-2">
              <TrendingUp className="w-4 h-4 text-purple-400" />
              <span className="text-xs font-medium text-purple-300">Recommendations</span>
            </div>
            <ul className="text-xs text-muted-foreground space-y-1.5">
              {summary.missedEvents > 0 && (
                <li className="flex items-start gap-2">
                  <span className="text-amber-400">•</span>
                  <span>Review {summary.missedEvents} missed events - consider schedule timing adjustments</span>
                </li>
              )}
              {Math.abs(summary.vwc.averageVariance) > 1 && (
                <li className="flex items-start gap-2">
                  <span className="text-amber-400">•</span>
                  <span>VWC running {summary.vwc.averageVariance < 0 ? 'lower' : 'higher'} than expected - adjust volume targets</span>
                </li>
              )}
              {summary.alignmentScore >= 90 && (
                <li className="flex items-start gap-2">
                  <span className="text-emerald-400">•</span>
                  <span>Schedule performing well - maintain current settings</span>
                </li>
              )}
            </ul>
          </div>
        </div>
      </div>
    </>
  );
}


