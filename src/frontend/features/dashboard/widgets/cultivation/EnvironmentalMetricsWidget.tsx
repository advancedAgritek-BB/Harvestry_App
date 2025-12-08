'use client';

import React, { useState, useEffect } from 'react';
import { cn } from '@/lib/utils';
import { Thermometer, Droplets, Sun, Wind, Activity, Wifi, WifiOff, Settings2 } from 'lucide-react';
import { SensorSelectorModal } from './SensorSelectorModal';
import { useCanConfigureSensors } from '@/stores/auth/authStore';
import { simulationService, StreamType } from '@/features/telemetry/services/simulation.service';
import {
  SensorAssignment,
  SensorMetricType,
  getMetricReading,
  METRIC_CONFIGS,
} from './types';

interface MetricAssignments {
  Temperature: SensorAssignment | null;
  Humidity: SensorAssignment | null;
  PPFD: SensorAssignment | null;
  DLI: SensorAssignment | null;
  CO2: SensorAssignment | null;
  EC: SensorAssignment | null;
  pH: SensorAssignment | null;
  VPD: SensorAssignment | null;
  VWC: SensorAssignment | null;
}

const defaultAssignments: MetricAssignments = {
  Temperature: null,
  Humidity: null,
  PPFD: null,
  DLI: null,
  CO2: null,
  EC: null,
  pH: null,
  VPD: null,
  VWC: null,
};

interface ClickableMetricCardProps {
  title: string;
  metric: SensorMetricType;
  assignment: SensorAssignment | null;
  fallbackValue: number;
  unit: string;
  icon: React.ReactNode;
  iconColor: string;
  status?: 'normal' | 'warning' | 'critical';
  subValue?: React.ReactNode;
  onConfigureClick: () => void;
  /** Whether user has permission to configure sensors */
  canConfigure?: boolean;
}

function ClickableMetricCard({
  title,
  metric,
  assignment,
  fallbackValue,
  unit,
  icon,
  iconColor,
  status = 'normal',
  subValue,
  onConfigureClick,
  canConfigure = true,
}: ClickableMetricCardProps) {
  const reading = getMetricReading(metric, assignment, fallbackValue);
  const config = METRIC_CONFIGS[metric];

  return (
    <div 
      className={cn(
        "flex flex-col p-4 bg-surface/50 border rounded-xl backdrop-blur-sm transition-all duration-200",
        status === 'normal' && "border-border hover:border-border/80",
        status === 'warning' && "border-amber-500/50 bg-amber-500/5",
        status === 'critical' && "border-rose-500/50 bg-rose-500/5"
      )}
    >
      <div className="flex items-center justify-between mb-2">
        <span className="text-base font-bold text-muted-foreground uppercase tracking-wider">{title}</span>
        <div className={cn(
          "w-4 h-4",
          status === 'normal' && "",
          status === 'warning' && "text-amber-500",
          status === 'critical' && "text-rose-500"
        )} style={{ color: status === 'normal' ? iconColor : undefined }}>
          {icon}
        </div>
      </div>
      
      {/* Value - Clickable only if user has permission */}
      {canConfigure ? (
        <button
          type="button"
          onClick={(e) => {
            e.preventDefault();
            e.stopPropagation();
            onConfigureClick();
          }}
          className="flex items-baseline gap-1 group text-left hover:bg-cyan-500/10 rounded px-2 -mx-2 py-1 transition-all cursor-pointer"
          title={`Click to configure ${title} sensor`}
        >
          <span className="text-2xl font-bold text-foreground tracking-tight group-hover:text-cyan-400 transition-colors">
            {reading.value.toFixed(config.decimals)}
          </span>
          <span className="text-xs font-medium text-muted-foreground">{unit}</span>
          
          {/* Source label */}
          {reading.label && (
            <span className="text-[9px] text-muted-foreground opacity-70 ml-1">
              ({reading.label})
            </span>
          )}
          
          {/* Live indicator */}
          {assignment && assignment.sensorIds.length > 0 && (
            reading.isLive ? (
              <Wifi className="w-2.5 h-2.5 text-emerald-400 opacity-60 ml-1" />
            ) : (
              <WifiOff className="w-2.5 h-2.5 text-amber-400 opacity-60 ml-1" />
            )
          )}
          
          {/* Configure hint */}
          <Settings2 className="w-3 h-3 text-cyan-400 opacity-0 group-hover:opacity-60 ml-auto transition-opacity" />
        </button>
      ) : (
        <div className="flex items-baseline gap-1 px-2 -mx-2 py-1">
          <span className="text-2xl font-bold text-foreground tracking-tight">
            {reading.value.toFixed(config.decimals)}
          </span>
          <span className="text-xs font-medium text-muted-foreground">{unit}</span>
          
          {/* Source label */}
          {reading.label && (
            <span className="text-[9px] text-muted-foreground opacity-70 ml-1">
              ({reading.label})
            </span>
          )}
          
          {/* Live indicator */}
          {assignment && assignment.sensorIds.length > 0 && (
            reading.isLive ? (
              <Wifi className="w-2.5 h-2.5 text-emerald-400 opacity-60 ml-1" />
            ) : (
              <WifiOff className="w-2.5 h-2.5 text-amber-400 opacity-60 ml-1" />
            )
          )}
        </div>
      )}
      
      {subValue && (
        <div className="mt-auto pt-2">
          {subValue}
        </div>
      )}
    </div>
  );
}

export function EnvironmentalMetricsWidget() {
  const [lightMode, setLightMode] = useState<'DLI' | 'PPFD'>('DLI');
  const [assignments, setAssignments] = useState<MetricAssignments>(defaultAssignments);
  const [simulatedValues, setSimulatedValues] = useState<Record<string, number>>({});
  
  // Permission check for sensor configuration
  const canConfigureSensors = useCanConfigureSensors();
  
  // Modal state
  const [activeModal, setActiveModal] = useState<SensorMetricType | null>(null);
  
  // Default fallback data
  const fallbackMetrics = {
    Temperature: simulatedValues.Temperature ?? 75.4,
    Humidity: simulatedValues.Humidity ?? 57,
    VPD: simulatedValues.VPD ?? 1.4,
    DLI: simulatedValues.DLI ?? 42,
    PPFD: simulatedValues.PPFD ?? 950,
    CO2: simulatedValues.CO2 ?? 1050,
    EC: simulatedValues.EC ?? 2.2,
    pH: simulatedValues.pH ?? 5.8,
    VWC: simulatedValues.VWC ?? 55,
  };

  // Start simulations on mount
  useEffect(() => {
    const startSimulations = async () => {
      try {
        // Start all relevant simulation types
        await Promise.all([
          simulationService.start(StreamType.Temperature),
          simulationService.start(StreamType.Humidity),
          simulationService.start(StreamType.Vpd),
          simulationService.start(StreamType.LightPpfd),
          simulationService.start(StreamType.Co2),
          simulationService.start(StreamType.Ec),
          simulationService.start(StreamType.Ph),
          simulationService.start(StreamType.SoilMoisture),
        ]);
      } catch (err) {
        console.error('Failed to start simulations:', err);
      }
    };

    startSimulations();

    // Poll for updates every second for smooth visuals
    const interval = setInterval(async () => {
      try {
        const activeSims = await simulationService.getActive();
        const newValues: Record<string, number> = {};
        
        activeSims.forEach(sim => {
          // Map stream types to our metric keys
          switch(sim.stream.streamType) {
            case StreamType.Temperature: newValues.Temperature = sim.lastValue; break;
            case StreamType.Humidity: newValues.Humidity = sim.lastValue; break;
            case StreamType.Vpd: newValues.VPD = sim.lastValue; break;
            case StreamType.LightPpfd: 
              newValues.PPFD = sim.lastValue;
              // Calculate DLI from PPFD: DLI = PPFD × hours × 3600 / 1,000,000
              // Assuming 12-hour photoperiod for flowering
              newValues.DLI = (sim.lastValue * 12 * 3600) / 1000000;
              break;
            case StreamType.Co2: newValues.CO2 = sim.lastValue; break;
            case StreamType.Ec: newValues.EC = sim.lastValue; break;
            case StreamType.Ph: newValues.pH = sim.lastValue; break;
            case StreamType.SoilMoisture: newValues.VWC = sim.lastValue; break;
          }
        });
        
        setSimulatedValues(prev => ({ ...prev, ...newValues }));
      } catch (err) {
        console.error('Failed to poll active simulations:', err);
      }
    }, 1000);

    return () => clearInterval(interval);
  }, []);

  const co2Reading = getMetricReading('CO2', assignments.CO2, fallbackMetrics.CO2);
  const isCo2Warning = co2Reading.value < 900 || co2Reading.value > 1200;

  const updateAssignment = (metric: SensorMetricType, assignment: SensorAssignment | null) => {
    setAssignments(prev => ({ ...prev, [metric]: assignment }));
  };

  const openModal = (metric: SensorMetricType) => {
    if (!canConfigureSensors) return;
    setActiveModal(metric);
  };

  const closeModal = () => {
    setActiveModal(null);
  };

  // Get VPD reading for temperature subvalue
  const vpdReading = getMetricReading('VPD', assignments.VPD, fallbackMetrics.VPD);
  
  // Get DL (Dewpoint Leaf?) reading for humidity - using VPD as proxy
  const dlValue = (vpdReading.value * 0.3).toFixed(2);

  return (
    <>
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4 h-full min-h-[120px]">
        {/* Temperature */}
        <ClickableMetricCard
          title="Temp"
          metric="Temperature"
          assignment={assignments.Temperature}
          fallbackValue={fallbackMetrics.Temperature}
          unit="°F"
          icon={<Thermometer />}
          iconColor="#22d3ee"
          onConfigureClick={() => openModal('Temperature')}
          canConfigure={canConfigureSensors}
          subValue={
            canConfigureSensors ? (
              <button
                type="button"
                onClick={(e) => {
                  e.stopPropagation();
                  openModal('VPD');
                }}
                className="text-xs text-muted-foreground hover:text-foreground transition-colors cursor-pointer"
              >
                VPD <span className="text-foreground">{vpdReading.value.toFixed(2)}</span>
                {assignments.VPD && <Settings2 className="w-2.5 h-2.5 inline ml-1 opacity-50" />}
              </button>
            ) : (
              <div className="text-xs text-muted-foreground">
                VPD <span className="text-foreground">{vpdReading.value.toFixed(2)}</span>
              </div>
            )
          }
        />

        {/* RH */}
        <ClickableMetricCard
          title="RH"
          metric="Humidity"
          assignment={assignments.Humidity}
          fallbackValue={fallbackMetrics.Humidity}
          unit="%"
          icon={<Droplets />}
          iconColor="#a78bfa"
          onConfigureClick={() => openModal('Humidity')}
          canConfigure={canConfigureSensors}
          subValue={
            <div className="text-xs text-muted-foreground">
              DL: <span className="text-foreground">{dlValue}</span>
            </div>
          }
        />

        {/* Light (Toggleable DLI/PPFD) */}
        <div 
          className={cn(
            "flex flex-col p-4 bg-surface/50 border rounded-xl backdrop-blur-sm transition-all duration-200",
            "border-border hover:border-border/80"
          )}
        >
          <div className="flex items-center justify-between mb-2">
            <span className="text-base font-bold text-muted-foreground uppercase tracking-wider">
              {lightMode}
            </span>
            <div className="w-4 h-4" style={{ color: "#fbbf24" }}>
              <Sun />
            </div>
          </div>
          
          {/* Value - Clickable only if user has permission */}
          {canConfigureSensors ? (
            <button
              type="button"
              onClick={(e) => {
                e.preventDefault();
                e.stopPropagation();
                openModal(lightMode);
              }}
              className="flex items-baseline gap-1 group text-left hover:bg-cyan-500/10 rounded px-2 -mx-2 py-1 transition-all cursor-pointer"
              title={`Click to configure ${lightMode} sensor`}
            >
              {(() => {
                const reading = getMetricReading(
                  lightMode, 
                  lightMode === 'DLI' ? assignments.DLI : assignments.PPFD, 
                  lightMode === 'DLI' ? fallbackMetrics.DLI : fallbackMetrics.PPFD
                );
                const config = METRIC_CONFIGS[lightMode];
                return (
                  <>
                    <span className="text-2xl font-bold text-foreground tracking-tight group-hover:text-cyan-400 transition-colors">
                      {reading.value.toFixed(config.decimals)}
                    </span>
                    <span className="text-xs font-medium text-muted-foreground">{config.unit}</span>
                    {reading.label && (
                      <span className="text-[9px] text-muted-foreground opacity-70 ml-1">
                        ({reading.label})
                      </span>
                    )}
                    {(lightMode === 'DLI' ? assignments.DLI : assignments.PPFD) && (
                      reading.isLive ? (
                        <Wifi className="w-2.5 h-2.5 text-emerald-400 opacity-60 ml-1" />
                      ) : (
                        <WifiOff className="w-2.5 h-2.5 text-amber-400 opacity-60 ml-1" />
                      )
                    )}
                    <Settings2 className="w-3 h-3 text-cyan-400 opacity-0 group-hover:opacity-60 ml-auto transition-opacity" />
                  </>
                );
              })()}
            </button>
          ) : (
            <div className="flex items-baseline gap-1 px-2 -mx-2 py-1">
              {(() => {
                const reading = getMetricReading(
                  lightMode, 
                  lightMode === 'DLI' ? assignments.DLI : assignments.PPFD, 
                  lightMode === 'DLI' ? fallbackMetrics.DLI : fallbackMetrics.PPFD
                );
                const config = METRIC_CONFIGS[lightMode];
                return (
                  <>
                    <span className="text-2xl font-bold text-foreground tracking-tight">
                      {reading.value.toFixed(config.decimals)}
                    </span>
                    <span className="text-xs font-medium text-muted-foreground">{config.unit}</span>
                    {reading.label && (
                      <span className="text-[9px] text-muted-foreground opacity-70 ml-1">
                        ({reading.label})
                      </span>
                    )}
                    {(lightMode === 'DLI' ? assignments.DLI : assignments.PPFD) && (
                      reading.isLive ? (
                        <Wifi className="w-2.5 h-2.5 text-emerald-400 opacity-60 ml-1" />
                      ) : (
                        <WifiOff className="w-2.5 h-2.5 text-amber-400 opacity-60 ml-1" />
                      )
                    )}
                  </>
                );
              })()}
            </div>
          )}
          
          <div className="mt-auto pt-2">
            <div className="flex items-center justify-between text-xs">
              <span className="text-muted-foreground">Tap to view {lightMode === 'DLI' ? 'PPFD' : 'DLI'}</span>
              <button 
                type="button"
                className="px-1.5 py-0.5 rounded bg-muted text-[10px] text-cyan-400 hover:bg-muted/80 transition-colors"
                onClick={(e) => {
                  e.stopPropagation();
                  setLightMode(prev => prev === 'DLI' ? 'PPFD' : 'DLI');
                }}
              >
                Toggle
              </button>
            </div>
          </div>
        </div>

        {/* CO2 */}
        <ClickableMetricCard
          title="CO₂"
          metric="CO2"
          assignment={assignments.CO2}
          fallbackValue={fallbackMetrics.CO2}
          unit="ppm"
          icon={<Wind />}
          iconColor="#f472b6"
          status={isCo2Warning ? 'warning' : 'normal'}
          onConfigureClick={() => openModal('CO2')}
          canConfigure={canConfigureSensors}
          subValue={
            <div className={cn(
              "text-xs",
              isCo2Warning ? "text-amber-500" : "text-muted-foreground"
            )}>
              Band <span className="text-foreground">900–1200</span>
            </div>
          }
        />

        {/* Substrate EC with pH */}
        <div 
          className={cn(
            "flex flex-col p-4 bg-surface/50 border rounded-xl backdrop-blur-sm transition-all duration-200",
            "border-border hover:border-border/80"
          )}
        >
          <div className="flex items-center justify-between mb-2">
            <span className="text-base font-bold text-muted-foreground uppercase tracking-wider">Substrate EC</span>
            <div className="w-4 h-4" style={{ color: "#60a5fa" }}>
              <Activity />
            </div>
          </div>
          
          {/* EC Value - Clickable only with permission */}
          {(() => {
            const reading = getMetricReading('EC', assignments.EC, fallbackMetrics.EC);
            return canConfigureSensors ? (
              <button
                type="button"
                onClick={(e) => {
                  e.preventDefault();
                  e.stopPropagation();
                  openModal('EC');
                }}
                className="flex items-baseline gap-1 group text-left hover:bg-cyan-500/10 rounded px-2 -mx-2 py-1 transition-all cursor-pointer"
                title="Click to configure EC sensor"
              >
                <span className="text-2xl font-bold text-foreground tracking-tight group-hover:text-cyan-400 transition-colors">
                  {reading.value.toFixed(1)}
                </span>
                <span className="text-xs font-medium text-muted-foreground">mS</span>
                {reading.label && (
                  <span className="text-[9px] text-muted-foreground opacity-70 ml-1">
                    ({reading.label})
                  </span>
                )}
                {assignments.EC && (
                  reading.isLive ? (
                    <Wifi className="w-2.5 h-2.5 text-emerald-400 opacity-60 ml-1" />
                  ) : (
                    <WifiOff className="w-2.5 h-2.5 text-amber-400 opacity-60 ml-1" />
                  )
                )}
                <Settings2 className="w-3 h-3 text-cyan-400 opacity-0 group-hover:opacity-60 ml-auto transition-opacity" />
              </button>
            ) : (
              <div className="flex items-baseline gap-1 px-2 -mx-2 py-1">
                <span className="text-2xl font-bold text-foreground tracking-tight">
                  {reading.value.toFixed(1)}
                </span>
                <span className="text-xs font-medium text-muted-foreground">mS</span>
                {reading.label && (
                  <span className="text-[9px] text-muted-foreground opacity-70 ml-1">
                    ({reading.label})
                  </span>
                )}
                {assignments.EC && (
                  reading.isLive ? (
                    <Wifi className="w-2.5 h-2.5 text-emerald-400 opacity-60 ml-1" />
                  ) : (
                    <WifiOff className="w-2.5 h-2.5 text-amber-400 opacity-60 ml-1" />
                  )
                )}
              </div>
            );
          })()}
          
          {/* pH Sub-value - Clickable only with permission */}
          <div className="mt-auto pt-2">
            {(() => {
              const phReading = getMetricReading('pH', assignments.pH, fallbackMetrics.pH);
              return canConfigureSensors ? (
                <button
                  type="button"
                  onClick={(e) => {
                    e.stopPropagation();
                    openModal('pH');
                  }}
                  className="text-xs text-muted-foreground hover:text-foreground flex items-center gap-1 hover:bg-cyan-500/10 rounded px-1 -mx-1 py-0.5 transition-all cursor-pointer"
                >
                  <span>pH</span>
                  <span className="text-foreground font-mono">{phReading.value.toFixed(1)}</span>
                  {phReading.label && (
                    <span className="text-[9px] opacity-70">({phReading.label})</span>
                  )}
                  {assignments.pH && (
                    phReading.isLive ? (
                      <Wifi className="w-2 h-2 text-emerald-400 opacity-60" />
                    ) : (
                      <WifiOff className="w-2 h-2 text-amber-400 opacity-60" />
                    )
                  )}
                  <Settings2 className="w-2.5 h-2.5 text-cyan-400 opacity-0 group-hover:opacity-60 transition-opacity" />
                </button>
              ) : (
                <div className="text-xs text-muted-foreground flex items-center gap-1">
                  <span>pH</span>
                  <span className="text-foreground font-mono">{phReading.value.toFixed(1)}</span>
                  {phReading.label && (
                    <span className="text-[9px] opacity-70">({phReading.label})</span>
                  )}
                  {assignments.pH && (
                    phReading.isLive ? (
                      <Wifi className="w-2 h-2 text-emerald-400 opacity-60" />
                    ) : (
                      <WifiOff className="w-2 h-2 text-amber-400 opacity-60" />
                    )
                  )}
                </div>
              );
            })()}
          </div>
        </div>
      </div>

      {/* Sensor Selection Modals */}
      {activeModal && (
        <SensorSelectorModal
          isOpen={true}
          onClose={closeModal}
          title={`Configure ${METRIC_CONFIGS[activeModal].label} Sensor`}
          description={`Select one or more sensors to display ${METRIC_CONFIGS[activeModal].label} readings. Multiple sensors can report average, high, or low values.`}
          metric={activeModal}
          currentAssignment={assignments[activeModal]}
          onSave={(assignment) => {
            updateAssignment(activeModal, assignment);
            closeModal();
          }}
          allowMultiple={true}
        />
      )}
    </>
  );
}
