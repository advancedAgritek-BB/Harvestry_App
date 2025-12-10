import React, { useState, useMemo, useCallback } from 'react';
import { ComposedChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Cell, Brush, ReferenceArea } from 'recharts';
import { cn } from '@/lib/utils';
import { Plus, Droplets, AlertTriangle, Clock, Settings2, Bell, Lock, Target } from 'lucide-react';
import { useChartHorizontalScroll } from '@/hooks/useChartHorizontalScroll';
import { 
  ZoneSelector, 
  ShotDetailsPopover, 
  HistoryPanel, 
  PerformancePanel,
  WindowEditModal, 
  AlertsConfigModal,
  CustomTooltip,
  ChartLegend,
  PauseControlButton,
  PauseConfigModal,
  QuickPickConfigModal,
  DEFAULT_VWC_THRESHOLDS,
  DEFAULT_PAUSE_CONFIG,
  DEFAULT_QUICK_PICK_CONFIG,
  IRRIGATION_COLORS,
  DEFAULT_ZONES,
  DEFAULT_WINDOWS,
  generateMockData,
  createManualShot,
  PHASE_SHOT_VOLUMES,
} from './irrigation';
import type { 
  IrrigationPeriod, 
  IrrigationDataPoint, 
  IrrigationZone, 
  VwcThresholdConfig, 
  IrrigationWindow,
  ZonePauseState,
  PauseConfig,
  PauseBehaviorMode,
  QuickPickConfig,
} from './irrigation';

// Color constants alias
const COLORS = IRRIGATION_COLORS;

interface IrrigationWindowsWidgetProps {
  /** Available zones (defaults to A-F) */
  zones?: IrrigationZone[];
  /** Read-only mode disables all actions */
  readOnly?: boolean;
  /** VWC threshold configuration */
  vwcThresholds?: VwcThresholdConfig;
  /** Show VWC threshold bands on chart */
  showThresholdBands?: boolean;
  /** Whether user is an organization admin (enables pause config) */
  isAdmin?: boolean;
}

export function IrrigationWindowsWidget({
  zones = DEFAULT_ZONES,
  readOnly = false,
  vwcThresholds = DEFAULT_VWC_THRESHOLDS,
  showThresholdBands = true,
  isAdmin = false,
}: IrrigationWindowsWidgetProps) {
  const [activeTab, setActiveTab] = useState<IrrigationPeriod>('P1 - Ramp');
  const [selectedZones, setSelectedZones] = useState<string[]>(() => zones.map(z => z.id));
  const [showShotModal, setShowShotModal] = useState(false);
  const [pendingQuickShot, setPendingQuickShot] = useState<number | null>(null);
  const [selectedShot, setSelectedShot] = useState<{
    shot: IrrigationDataPoint;
    position: { x: number; y: number };
  } | null>(null);
  const [showHistory, setShowHistory] = useState(false);
  const [showWindowEdit, setShowWindowEdit] = useState(false);
  const [irrigationWindows, setIrrigationWindows] = useState<IrrigationWindow[]>([
    { id: 'p1', period: 'P1 - Ramp', startTime: '06:00', endTime: '10:00', isActive: true },
    { id: 'p2', period: 'P2 - Maintenance', startTime: '10:00', endTime: '16:00', isActive: true },
    { id: 'p3', period: 'P3 - Dryback', startTime: '16:00', endTime: '20:00', isActive: true },
  ]);
  const [showAlertsConfig, setShowAlertsConfig] = useState(false);
  const [alertsConfig, setAlertsConfig] = useState<VwcThresholdConfig>(vwcThresholds);
  const [pauseState, setPauseState] = useState<ZonePauseState>({
    pausedZones: [], pausedAt: null, pausedBy: null, isPaused: false,
  });
  const [pauseConfig, setPauseConfig] = useState<PauseConfig>(DEFAULT_PAUSE_CONFIG);
  const [showPauseConfig, setShowPauseConfig] = useState(false);
  const [quickPickConfig, setQuickPickConfig] = useState<QuickPickConfig>(DEFAULT_QUICK_PICK_CONFIG);
  const [showQuickPickConfig, setShowQuickPickConfig] = useState(false);
  const [showPerformance, setShowPerformance] = useState(false);

  // Chart data state (initialized with mock data, can be modified by manual shots)
  const [chartData, setChartData] = useState<IrrigationDataPoint[]>(() => 
    generateMockData(irrigationWindows)
  );

  // Filter data based on active period tab
  const filteredData = useMemo(() => {
    if (activeTab === 'All') return chartData;
    if (activeTab === 'P3 - Dryback') {
      const p2 = chartData.filter(d => d.period === 'P2 - Maintenance');
      const p3 = chartData.filter(d => d.period === 'P3 - Dryback');
      return p2.length > 0 ? [p2[p2.length - 1], ...p3] : p3;
    }
    return chartData.filter(d => d.period === activeTab);
  }, [chartData, activeTab]);

  const currentPeriod: Exclude<IrrigationPeriod, 'All'> = activeTab === 'All' ? 'P2 - Maintenance' : activeTab;
  // Find last non-null VWC reading (skip pending)
  const lastVwc = useMemo((): number => {
    for (let i = filteredData.length - 1; i >= 0; i--) {
      const vwc = filteredData[i].endVwc;
      if (vwc !== null) return vwc;
    }
    return 45;
  }, [filteredData]);

  // Horizontal scroll hook - use filtered data length
  const { 
    containerRef: chartContainerRef, 
    startIndex, 
    endIndex, 
    onBrushChange,
  } = useChartHorizontalScroll({
    dataLength: filteredData.length,
    scrollSensitivity: 1,
  });

  // Memoize selected zone names for display
  const selectedZoneNames = useMemo(() => {
    return selectedZones.slice(0, 3).join(', ') + (selectedZones.length > 3 ? '...' : '');
  }, [selectedZones]);

  // Handle bar click to show shot details
  const handleBarClick = useCallback((data: IrrigationDataPoint, event: React.MouseEvent) => {
    setSelectedShot({
      shot: data,
      position: { x: event.clientX, y: event.clientY },
    });
  }, []);

  // Handle shot edit
  const handleShotEdit = useCallback((shot: IrrigationDataPoint) => {
    console.log('Edit shot:', shot);
    setSelectedShot(null);
    // TODO: Open edit modal
  }, []);

  const handleQuickPick = (volume: number) => {
    // Open confirmation instead of firing immediately
    setPendingQuickShot(volume);
  };

  const confirmQuickShot = () => {
    if (!pendingQuickShot) return;
    const newShot = createManualShot(pendingQuickShot, selectedZones, currentPeriod, lastVwc, chartData.length);
    setChartData(prev => [...prev, newShot].sort((a, b) => a.time.localeCompare(b.time)));
    setPendingQuickShot(null);
  };

  const handlePauseToggle = useCallback((isPaused: boolean, zones: string[], _mode: PauseBehaviorMode) => {
    if (isPaused) setPauseState({ pausedZones: zones, pausedAt: new Date().toISOString(), pausedBy: 'Current User', isPaused: true });
    else setPauseState(prev => { const r = prev.pausedZones.filter(z => !zones.includes(z)); return { ...prev, pausedZones: r, isPaused: r.length > 0 }; });
  }, [pauseState.pausedZones]);

  return (
    <div className="w-full h-full min-h-[300px] bg-surface/50 border border-border rounded-xl p-4 flex flex-col">
      
      {/* Header Controls - Responsive: stacks on mobile */}
      <div className="flex flex-col gap-3 mb-4">
        {/* Title row with action buttons */}
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-2">
          <div className="flex items-center gap-2 flex-wrap">
            <h3 className="text-sm font-medium text-muted-foreground uppercase tracking-wider flex items-center gap-2">
              <Droplets className="w-4 h-4 text-cyan-500" />
              <span className="hidden xs:inline">Irrigation Windows</span>
              <span className="xs:hidden">Irrigation</span>
              {readOnly && (
                <span className="flex items-center gap-1 px-1.5 py-0.5 text-[9px] font-medium bg-muted text-muted-foreground rounded">
                  <Lock className="w-2.5 h-2.5" />
                  <span className="hidden sm:inline">View Only</span>
                </span>
              )}
            </h3>
            {/* Action buttons - 44px touch targets on mobile */}
            <div className="flex items-center gap-1">
              <button
                onClick={() => setShowHistory(true)}
                className="p-2.5 sm:p-1.5 text-muted-foreground hover:text-foreground hover:bg-muted rounded-lg transition-colors touch-manipulation"
                title="View shot history"
                aria-label="Open shot history"
              >
                <Clock className="w-4 h-4" />
              </button>
              <button
                onClick={() => setShowPerformance(true)}
                className="p-2.5 sm:p-1.5 text-purple-400 hover:text-purple-300 hover:bg-purple-500/10 rounded-lg transition-colors touch-manipulation"
                title="Schedule performance - Expected vs Actual"
                aria-label="View schedule performance analysis"
              >
                <Target className="w-4 h-4" />
              </button>
              {!readOnly && (
                <button
                  onClick={() => setShowWindowEdit(true)}
                  className="p-2.5 sm:p-1.5 text-muted-foreground hover:text-foreground hover:bg-muted rounded-lg transition-colors touch-manipulation"
                  title="Edit window times"
                  aria-label="Edit irrigation windows"
                >
                  <Settings2 className="w-4 h-4" />
                </button>
              )}
              <button
                onClick={() => setShowAlertsConfig(true)}
                className={cn(
                  "p-2.5 sm:p-1.5 rounded-lg transition-colors touch-manipulation",
                  alertsConfig.alertOnLow || alertsConfig.alertOnHigh
                    ? "text-amber-400 hover:text-amber-300 hover:bg-amber-500/10"
                    : "text-muted-foreground hover:text-foreground hover:bg-muted"
                )}
                title="Configure VWC alerts"
                aria-label="Configure VWC threshold alerts"
              >
                <Bell className="w-4 h-4" />
              </button>
            </div>
          </div>
          
          {/* Period Tabs - scrollable on mobile, wraps on tablet+ */}
          <div className="flex bg-muted/50 rounded-lg p-1 gap-1 overflow-x-auto scrollbar-none">
            {(['P1 - Ramp', 'P2 - Maintenance', 'P3 - Dryback', 'All'] as IrrigationPeriod[]).map(tab => (
              <button
                key={tab}
                onClick={() => setActiveTab(tab)}
                className={cn(
                  "px-3 py-2 sm:py-1 text-xs font-medium rounded transition-colors whitespace-nowrap min-h-[44px] sm:min-h-0 touch-manipulation",
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

        {/* Zone Selectors - responsive wrap */}
        <div className="flex items-center gap-2 flex-wrap">
          <ZoneSelector
            zones={zones}
            selectedZones={selectedZones}
            onSelectionChange={setSelectedZones}
            disabled={readOnly}
          />
          <div className="w-px h-6 bg-border mx-1 hidden sm:block" />
          <button 
            onClick={() => setShowShotModal(true)}
            disabled={readOnly}
            className={cn(
              "flex items-center gap-1 px-3 py-2 sm:py-1.5 text-xs font-medium text-foreground bg-muted hover:bg-muted/80 border border-border rounded transition-colors whitespace-nowrap min-h-[44px] sm:min-h-[32px] touch-manipulation",
              readOnly && "opacity-50 cursor-not-allowed"
            )}
          >
            <Plus className="w-3 h-3" />
            Add shot
          </button>
          {/* Pause Control */}
          {!readOnly && (
            <PauseControlButton
              pauseState={pauseState}
              selectedZones={selectedZones}
              pauseConfig={pauseConfig}
              onPauseToggle={handlePauseToggle}
              onOpenConfig={isAdmin ? () => setShowPauseConfig(true) : undefined}
              isAdmin={isAdmin}
              disabled={readOnly}
            />
          )}
        </div>
      </div>

      {/* Legend */}
      <div className="mb-3">
        <ChartLegend />
      </div>

      {/* Dual Bar Chart with VWC Trend Line - ref enables mouse wheel horizontal scrolling */}
      <div ref={chartContainerRef} className="flex-1 min-h-0 relative">
        <ResponsiveContainer width="100%" height="100%">
          <ComposedChart data={filteredData} margin={{ top: 10, right: 10, left: 5, bottom: 0 }}>
            <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" vertical={false} />
            <XAxis dataKey="time" stroke="var(--text-muted)" fontSize={11} tickLine={false} axisLine={false} />

            {/* Left Axis: Volume */}
            <YAxis yAxisId="vol" stroke={COLORS.automated} fontSize={11} tickLine={false} axisLine={false} unit="mL" width={45} />
            
            {/* Right Axis: VWC */}
            <YAxis yAxisId="vwc" orientation="right" stroke={COLORS.vwc} fontSize={11} tickLine={false} axisLine={false} unit="%" domain={[0, 100]} width={35} />

            <Tooltip 
               cursor={{fill: 'var(--bg-hover)', opacity: 0.4}}
               content={<CustomTooltip />}
            />

            {/* VWC Threshold Bands */}
            {showThresholdBands && (
              <>
                {/* Low threshold zone (0 to low) */}
                <ReferenceArea
                  yAxisId="vwc"
                  y1={0}
                  y2={alertsConfig.lowThreshold}
                  fill="#ef4444"
                  fillOpacity={alertsConfig.alertOnLow ? 0.08 : 0.03}
                  strokeOpacity={0}
                />
                {/* High threshold zone (high to 100) */}
                <ReferenceArea
                  yAxisId="vwc"
                  y1={alertsConfig.highThreshold}
                  y2={100}
                  fill="#ef4444"
                  fillOpacity={alertsConfig.alertOnHigh ? 0.08 : 0.03}
                  strokeOpacity={0}
                />
                {/* Optimal zone indicator (subtle green) */}
                <ReferenceArea
                  yAxisId="vwc"
                  y1={alertsConfig.lowThreshold}
                  y2={alertsConfig.highThreshold}
                  fill="#10b981"
                  fillOpacity={0.05}
                  strokeOpacity={0}
                />
              </>
            )}

            {/* Bar 1: Volume (Color coded by status and type) - Clickable */}
            <Bar 
              yAxisId="vol" 
              dataKey="volume" 
              name="Vol (mL)" 
              radius={[4, 4, 0, 0]} 
              barSize={20}
              className="cursor-pointer"
              onClick={(data, index, event) => {
                if (data && event) {
                  handleBarClick(data as unknown as IrrigationDataPoint, event as unknown as React.MouseEvent);
                }
              }}
            >
              {filteredData.map((entry) => {
                const isPlanned = entry.status === 'planned';
                const baseColor = isPlanned 
                  ? COLORS.plannedVolume 
                  : (entry.type === 'manual' ? COLORS.manual : COLORS.automated);
                return (
                  <Cell
                    key={`vol-${entry.id}`}
                    fill={baseColor}
                    fillOpacity={isPlanned ? 0.2 : 1}
                    className="hover:opacity-80 transition-opacity"
                  />
                );
              })}
            </Bar>

            {/* Bar 2: End VWC (Color coded by status) - Clickable */}
            <Bar 
              yAxisId="vwc" 
              dataKey="endVwc" 
              name="End VWC %" 
              radius={[4, 4, 0, 0]} 
              barSize={20} 
              className="cursor-pointer"
              onClick={(data, index, event) => {
                if (data && event) {
                  handleBarClick(data as unknown as IrrigationDataPoint, event as unknown as React.MouseEvent);
                }
              }}
            >
              {filteredData.map((entry) => {
                const isPlanned = entry.status === 'planned';
                return (
                  <Cell
                    key={`vwc-${entry.id}`}
                    fill={isPlanned ? COLORS.plannedVwc : COLORS.vwc}
                    fillOpacity={isPlanned ? 0.15 : 0.6}
                    className="hover:opacity-80 transition-opacity"
                  />
                );
              })}
            </Bar>
            
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
          </ComposedChart>
        </ResponsiveContainer>
        
        {/* Quick Pick Overlay (Bottom) - Responsive */}
        <div className={cn(
          "absolute bottom-0 left-0 right-0 p-2 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-2 bg-gradient-to-t from-surface via-surface/80 to-transparent pt-8 pointer-events-none",
          readOnly && "opacity-60"
        )}>
           <div className="flex items-center gap-2 pointer-events-auto flex-wrap">
             <div className="flex flex-col">
               <span className="text-[10px] font-medium text-muted-foreground uppercase tracking-wider">
                 Quick Pick{selectedZones.length > 0 && ':'}
               </span>
               {selectedZones.length > 0 && (
                 <span className="text-[9px] text-blue-400/80 hidden sm:block">
                   ({selectedZoneNames})
                 </span>
               )}
             </div>
             {selectedZones.length === 0 ? (
               <span className="text-[10px] text-muted-foreground/60 italic">Select zones first</span>
             ) : (
               <>
                 {[PHASE_SHOT_VOLUMES[currentPeriod], ...quickPickConfig.volumes]
                   .filter((v, i, a) => a.indexOf(v) === i).slice(0, 5)
                   .map((vol, idx) => {
                     const isPhaseVol = vol === PHASE_SHOT_VOLUMES[currentPeriod];
                     const disabled = readOnly || selectedZones.length === 0;
                     return (
                       <button key={vol} onClick={() => handleQuickPick(vol)} disabled={disabled}
                         className={cn(
                           "px-3 py-2 sm:px-2.5 sm:py-1 text-xs font-mono font-medium rounded transition-colors active:scale-95 min-h-[44px] sm:min-h-[28px] touch-manipulation",
                           idx >= 2 && "hidden sm:block",
                           disabled ? "text-muted-foreground/50 bg-muted/30 border border-border/50 cursor-not-allowed"
                             : isPhaseVol ? "text-cyan-200 bg-cyan-500/20 hover:bg-cyan-500/30 border border-cyan-400/50 ring-1 ring-cyan-400/30"
                             : "text-cyan-300 bg-cyan-500/10 hover:bg-cyan-500/20 border border-cyan-500/30"
                         )}
                         title={isPhaseVol ? `Recommended for ${currentPeriod}` : undefined}
                       >{vol} mL</button>
                     );
                   })}
                 {isAdmin && (
                   <button onClick={() => setShowQuickPickConfig(true)} title="Configure quick picks"
                     className="p-1.5 sm:p-1 text-muted-foreground hover:text-cyan-400 hover:bg-cyan-500/10 rounded transition-colors">
                     <Settings2 className="w-4 h-4 sm:w-3.5 sm:h-3.5" />
                   </button>
                 )}
               </>
             )}
           </div>
           
           <div className="text-[10px] text-muted-foreground flex flex-row sm:flex-col items-center sm:items-end gap-2 sm:gap-0 pointer-events-auto">
             <span>Leachate: <span className="text-foreground font-medium">12%</span></span>
             <span className="hidden sm:block">Target: 8-15%</span>
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

      {/* Shot Details Popover */}
      {selectedShot && <ShotDetailsPopover shot={selectedShot.shot} position={selectedShot.position} onClose={() => setSelectedShot(null)} onEdit={handleShotEdit} readOnly={readOnly} />}

      {/* Modals & Panels */}
      <HistoryPanel isOpen={showHistory} onClose={() => setShowHistory(false)} logs={[]} filterPeriod={activeTab} filterZones={selectedZones} />
      <PerformancePanel isOpen={showPerformance} onClose={() => setShowPerformance(false)} filterPeriod={activeTab} />
      <WindowEditModal isOpen={showWindowEdit} onClose={() => setShowWindowEdit(false)} windows={irrigationWindows} onSave={setIrrigationWindows} readOnly={readOnly} />
      <AlertsConfigModal isOpen={showAlertsConfig} onClose={() => setShowAlertsConfig(false)} config={alertsConfig} onSave={setAlertsConfig} readOnly={readOnly} />
      <PauseConfigModal isOpen={showPauseConfig} onClose={() => setShowPauseConfig(false)} config={pauseConfig} onSave={setPauseConfig} />
      <QuickPickConfigModal isOpen={showQuickPickConfig} onClose={() => setShowQuickPickConfig(false)} config={quickPickConfig} onSave={setQuickPickConfig} />
    </div>
  );
}
