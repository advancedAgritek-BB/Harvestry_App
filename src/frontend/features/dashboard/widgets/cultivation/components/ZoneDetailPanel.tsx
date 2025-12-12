'use client';

import React from 'react';
import { cn } from '@/lib/utils';
import { 
  X, 
  Thermometer, 
  Droplets, 
  Waves, 
  Zap, 
  Sun,
  AlertTriangle,
  Activity,
  Leaf,
} from 'lucide-react';
import { MetricType, CellData } from './DigitalTwinGrid';

interface SensorReading {
  id: string;
  name: string;
  type: MetricType;
  value: number;
  status: 'online' | 'offline' | 'warning';
  lastUpdate: string;
}

interface ZoneDetailPanelProps {
  isOpen: boolean;
  onClose: () => void;
  cellData: CellData | null;
  cellKey: string | null;
  sensorReadings?: SensorReading[];
  plantCount?: number;
  alerts?: Array<{ id: string; message: string; severity: 'warning' | 'critical' }>;
}

const METRIC_ICONS: Record<MetricType, typeof Thermometer> = {
  Temp: Thermometer,
  RH: Droplets,
  VWC: Waves,
  EC: Zap,
  PPFD: Sun,
};

const METRIC_LABELS: Record<MetricType, string> = {
  Temp: 'Temperature',
  RH: 'Relative Humidity',
  VWC: 'Water Content',
  EC: 'EC',
  PPFD: 'Light Intensity',
};

const METRIC_UNITS: Record<MetricType, string> = {
  Temp: '°F',
  RH: '%',
  VWC: '%',
  EC: 'mS/cm',
  PPFD: 'μmol/m²/s',
};

// Mock sensor data generator based on zone
function generateMockSensorReadings(zoneId: string): SensorReading[] {
  const baseValue = parseInt(zoneId, 10) || 1;
  const readings: SensorReading[] = [
    {
      id: `${zoneId}-temp`,
      name: 'Temp/RH Sensor',
      type: 'Temp',
      value: 73 + (baseValue * 0.8),
      status: 'online',
      lastUpdate: '30s ago',
    },
    {
      id: `${zoneId}-rh`,
      name: 'Temp/RH Sensor',
      type: 'RH',
      value: 52 + (baseValue * 1.2),
      status: 'online',
      lastUpdate: '30s ago',
    },
    {
      id: `${zoneId}-vwc`,
      name: 'VWC Probe',
      type: 'VWC',
      value: 42 + (baseValue * 2),
      status: 'online',
      lastUpdate: '1m ago',
    },
    {
      id: `${zoneId}-ec`,
      name: 'EC Sensor',
      type: 'EC',
      value: 2.1 + (baseValue * 0.1),
      status: 'online',
      lastUpdate: '1m ago',
    },
    {
      id: `${zoneId}-ppfd`,
      name: 'PPFD Sensor',
      type: 'PPFD',
      value: 850 + (baseValue * 15),
      status: 'online',
      lastUpdate: '5s ago',
    },
  ];
  return readings;
}

export function ZoneDetailPanel({
  isOpen,
  onClose,
  cellData,
  cellKey,
  plantCount = 32,
  alerts = [],
}: ZoneDetailPanelProps) {
  if (!isOpen || !cellData || !cellKey) return null;

  const [, row, col] = cellKey.split('-').map(Number);
  const sensorReadings = generateMockSensorReadings(cellData.zoneId);
  const zoneName = cellData.zoneName || cellData.zoneCode || `Zone ${cellData.zoneId}`;

  return (
    <div className="absolute inset-0 bg-surface/98 backdrop-blur-sm rounded-xl z-20 animate-in slide-in-from-bottom-4 duration-200">
      {/* Header */}
      <div className="flex items-center justify-between px-4 py-3 border-b border-border">
        <div className="flex items-center gap-3">
          <div className="w-8 h-8 rounded-lg bg-cyan-500/20 flex items-center justify-center">
            <Activity className="w-4 h-4 text-cyan-400" />
          </div>
          <div>
            <h4 className="font-semibold text-foreground">{zoneName}</h4>
            <p className="text-xs text-muted-foreground">
              Grid Position: Row {row + 1}, Col {col + 1}
              {cellData.label && <span className="ml-2">• {cellData.label}</span>}
            </p>
          </div>
        </div>
        <button
          onClick={onClose}
          className="p-1.5 hover:bg-white/10 rounded-lg transition-colors"
        >
          <X className="w-5 h-5 text-muted-foreground" />
        </button>
      </div>

      {/* Content */}
      <div className="p-4 space-y-4 max-h-[calc(100%-60px)] overflow-y-auto">
        {/* Quick Stats */}
        <div className="grid grid-cols-3 gap-3">
          <div className="p-3 bg-white/5 rounded-lg">
            <div className="text-xs text-muted-foreground">Sensors</div>
            <div className="text-lg font-bold text-foreground">{sensorReadings.length}</div>
          </div>
          <div className="p-3 bg-white/5 rounded-lg">
            <div className="text-xs text-muted-foreground flex items-center gap-1">
              <Leaf className="w-3 h-3" /> Plants
            </div>
            <div className="text-lg font-bold text-emerald-400">{plantCount}</div>
          </div>
          <div className="p-3 bg-white/5 rounded-lg">
            <div className="text-xs text-muted-foreground">Alerts</div>
            <div className={cn(
              'text-lg font-bold',
              cellData.hasAlert ? 'text-rose-400' : 'text-emerald-400'
            )}>
              {cellData.hasAlert ? alerts.length || 1 : 0}
            </div>
          </div>
        </div>

        {/* Active Alerts */}
        {cellData.hasAlert && (
          <div className="p-3 bg-rose-500/10 border border-rose-500/20 rounded-lg">
            <div className="flex items-center gap-2 text-rose-400 mb-2">
              <AlertTriangle className="w-4 h-4" />
              <span className="text-sm font-medium">Active Alert</span>
            </div>
            <p className="text-xs text-rose-300">
              Sensor timeout detected - last reading received 15 minutes ago
            </p>
          </div>
        )}

        {/* Sensor Readings */}
        <div>
          <h5 className="text-xs font-medium text-muted-foreground uppercase tracking-wider mb-3">
            Current Readings
          </h5>
          <div className="space-y-2">
            {sensorReadings.map((reading) => {
              const Icon = METRIC_ICONS[reading.type];
              return (
                <div
                  key={reading.id}
                  className="flex items-center justify-between p-2 bg-white/5 rounded-lg"
                >
                  <div className="flex items-center gap-2">
                    <Icon className="w-4 h-4 text-muted-foreground" />
                    <div>
                      <div className="text-sm text-foreground">
                        {METRIC_LABELS[reading.type]}
                      </div>
                      <div className="text-xs text-muted-foreground">
                        {reading.name} • {reading.lastUpdate}
                      </div>
                    </div>
                  </div>
                  <div className="text-right">
                    <div className="text-sm font-mono font-bold text-foreground">
                      {reading.value.toFixed(reading.type === 'EC' ? 2 : 1)}
                      <span className="text-xs text-muted-foreground ml-0.5">
                        {METRIC_UNITS[reading.type]}
                      </span>
                    </div>
                    <div className={cn(
                      'text-xs',
                      reading.status === 'online' ? 'text-emerald-400' : 
                      reading.status === 'warning' ? 'text-amber-400' : 'text-rose-400'
                    )}>
                      {reading.status}
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>

        {/* View Plants Link (placeholder) */}
        <button className="w-full py-2.5 px-4 bg-cyan-500/10 hover:bg-cyan-500/20 text-cyan-400 rounded-lg text-sm font-medium transition-colors flex items-center justify-center gap-2">
          <Leaf className="w-4 h-4" />
          View Plants in This Zone
        </button>
      </div>
    </div>
  );
}

export default ZoneDetailPanel;








