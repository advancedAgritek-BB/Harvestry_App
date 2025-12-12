'use client';

import React from 'react';
import { FormField, Input } from '@/components/admin/AdminForm';
import { ShotConfiguration, ZoneCalibration } from '@/components/irrigation/types';
import { ZoneShotDurationDisplay } from './ZoneShotDurationDisplay';

interface ShotsTabContentProps {
  shotConfig: ShotConfiguration;
  targetZones: string[];
  zoneCalibrations: Record<string, ZoneCalibration>;
  onUpdateShotConfig: (updates: Partial<ShotConfiguration>) => void;
}

/**
 * Shots tab content for the program modal
 * Handles shot size, VWC increase, soak time, max shots, and zone durations
 */
export function ShotsTabContent({
  shotConfig,
  targetZones,
  zoneCalibrations,
  onUpdateShotConfig,
}: ShotsTabContentProps) {
  return (
    <div className="space-y-4">
      <div className="grid grid-cols-2 gap-4">
        <FormField label="Shot Size (mL)" description="Volume per plant per shot">
          <Input
            type="number"
            min={10}
            max={500}
            value={shotConfig.shotSizeMl}
            onChange={e => onUpdateShotConfig({
              shotSizeMl: parseInt(e.target.value) || 0,
            })}
          />
        </FormField>
        <FormField label="Expected VWC Increase" description="% increase per shot">
          <Input
            type="number"
            min={0.5}
            max={10}
            step={0.5}
            value={shotConfig.expectedVwcIncreasePercent}
            onChange={e => onUpdateShotConfig({
              expectedVwcIncreasePercent: parseFloat(e.target.value) || 0,
            })}
          />
        </FormField>
      </div>

      <div className="grid grid-cols-2 gap-4">
        <FormField label="Min Time Between Shots" description="Substrate absorption time (min)">
          <Input
            type="number"
            min={5}
            max={120}
            value={shotConfig.minSoakTimeMinutes}
            onChange={e => onUpdateShotConfig({
              minSoakTimeMinutes: parseInt(e.target.value) || 0,
            })}
          />
        </FormField>
        <FormField label="Max Shots / Day" description="Daily safety limit">
          <Input
            type="number"
            min={1}
            max={24}
            value={shotConfig.maxShotsPerDay}
            onChange={e => onUpdateShotConfig({
              maxShotsPerDay: parseInt(e.target.value) || 0,
            })}
          />
        </FormField>
      </div>

      {/* Shot Duration per Zone */}
      <ZoneShotDurationDisplay
        targetZones={targetZones}
        zoneCalibrations={zoneCalibrations}
        shotSizeMl={shotConfig.shotSizeMl}
      />
    </div>
  );
}









