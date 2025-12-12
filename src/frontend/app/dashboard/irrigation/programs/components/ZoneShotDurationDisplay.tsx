'use client';

import React, { useMemo } from 'react';
import { Timer, CheckCircle2, AlertTriangle } from 'lucide-react';
import { ZoneCalibration, calculateShotDuration } from '@/components/irrigation/types';

interface ZoneShotDurationDisplayProps {
  targetZones: string[];
  zoneCalibrations: Record<string, ZoneCalibration>;
  shotSizeMl: number;
}

/**
 * Displays calculated shot duration for each selected zone
 * Duration is zone-specific based on calibration data:
 * Duration = Shot Size ÷ (Flow Rate × Emitters)
 */
export function ZoneShotDurationDisplay({
  targetZones,
  zoneCalibrations,
  shotSizeMl,
}: ZoneShotDurationDisplayProps) {
  // Calculate shot durations for each zone
  const zoneDurations = useMemo(() => {
    return targetZones.map(zone => {
      const calibration = zoneCalibrations[zone];
      if (!calibration) {
        return { zone, duration: null, calibrated: false };
      }
      const duration = calculateShotDuration(
        shotSizeMl,
        calibration.emitterFlowMlPerSecond,
        calibration.emittersPerPlant
      );
      return { zone, duration, calibrated: true, calibration };
    });
  }, [targetZones, zoneCalibrations, shotSizeMl]);

  const calibratedZones = zoneDurations.filter(z => z.calibrated);
  const uncalibratedZones = zoneDurations.filter(z => !z.calibrated);

  const formatDuration = (seconds: number): string => {
    if (seconds < 60) {
      return `${seconds.toFixed(1)}s`;
    }
    const mins = Math.floor(seconds / 60);
    const secs = Math.round(seconds % 60);
    return secs > 0 ? `${mins}m ${secs}s` : `${mins}m`;
  };

  if (targetZones.length === 0) {
    return (
      <div className="p-3 bg-muted/50 border border-border rounded-lg text-sm">
        <div className="flex items-center gap-2 text-muted-foreground">
          <Timer className="w-4 h-4" />
          <span className="font-medium">Shot Duration</span>
        </div>
        <p className="text-muted-foreground text-xs mt-1">
          Select target zones to see calculated shot durations.
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-3">
      {/* Header with formula explanation */}
      <div className="flex items-center gap-2 text-sm">
        <Timer className="w-4 h-4 text-cyan-400" />
        <span className="font-medium text-foreground">Shot Duration by Zone</span>
        <span className="text-xs text-muted-foreground ml-auto">
          Duration = Shot Size ÷ (Flow Rate × Emitters)
        </span>
      </div>

      {/* Calibrated zones with durations */}
      {calibratedZones.length > 0 && (
        <div className="grid gap-2">
          {calibratedZones.map(({ zone, duration, calibration }) => (
            <div
              key={zone}
              className="flex items-center justify-between p-3 bg-emerald-500/10 border border-emerald-500/20 rounded-lg"
            >
              <div className="flex items-center gap-2">
                <CheckCircle2 className="w-4 h-4 text-emerald-400" />
                <span className="text-sm font-medium text-foreground">{zone}</span>
              </div>
              <div className="flex items-center gap-4 text-sm">
                <div className="text-right">
                  <span className="text-lg font-bold text-emerald-400">
                    {formatDuration(duration!)}
                  </span>
                </div>
                <div className="text-xs text-muted-foreground text-right min-w-[120px]">
                  <div>{calibration!.emitterFlowMlPerSecond.toFixed(2)} mL/s</div>
                  <div>{calibration!.emittersPerPlant} emitter{calibration!.emittersPerPlant > 1 ? 's' : ''}/plant</div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Uncalibrated zones - warning */}
      {uncalibratedZones.length > 0 && (
        <div className="p-3 bg-amber-500/10 border border-amber-500/30 rounded-lg">
          <div className="flex items-center gap-2 text-amber-400 mb-2">
            <AlertTriangle className="w-4 h-4" />
            <span className="font-medium text-sm">
              {uncalibratedZones.length === 1 ? 'Zone Requires Calibration' : 'Zones Require Calibration'}
            </span>
          </div>
          <div className="flex flex-wrap gap-2">
            {uncalibratedZones.map(({ zone }) => (
              <span
                key={zone}
                className="px-2 py-1 bg-amber-500/20 border border-amber-500/30 rounded text-xs text-amber-300"
              >
                {zone}
              </span>
            ))}
          </div>
          <p className="text-xs text-muted-foreground mt-2">
            Configure zone calibration in Zone Settings to calculate accurate shot duration.
          </p>
        </div>
      )}

      {/* All zones uncalibrated */}
      {calibratedZones.length === 0 && uncalibratedZones.length > 0 && (
        <p className="text-xs text-muted-foreground italic">
          No calibration data available. Shot duration cannot be calculated until zones are calibrated.
        </p>
      )}
    </div>
  );
}









