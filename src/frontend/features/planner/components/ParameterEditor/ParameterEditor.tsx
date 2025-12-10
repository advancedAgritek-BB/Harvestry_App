'use client';

import React, { useState } from 'react';
import { cn } from '@/lib/utils';
import { 
  Thermometer, 
  Droplets, 
  Wind, 
  Sun,
  Zap,
  Clock,
  TrendingUp,
  Gauge,
  ChevronDown,
  ChevronRight,
  Save,
  RotateCcw,
} from 'lucide-react';
import { 
  PhaseBlueprint,
  EnvironmentalParams,
  LightingParams,
  IrrigationParams,
} from '../../types/blueprint.types';
import { PhaseType } from '../../types/planner.types';

interface ParameterEditorProps {
  blueprint: PhaseBlueprint | null;
  phase: PhaseType;
  isEditing: boolean;
  onChange?: (updates: Partial<PhaseBlueprint>) => void;
  onSaveAsNew?: (name: string) => void;
  onReset?: () => void;
}

type ParamSection = 'environmental' | 'lighting' | 'irrigation';

const SECTION_ICONS: Record<ParamSection, React.ElementType> = {
  environmental: Thermometer,
  lighting: Sun,
  irrigation: Droplets,
};

const SECTION_LABELS: Record<ParamSection, string> = {
  environmental: 'Environmental',
  lighting: 'Lighting',
  irrigation: 'Irrigation',
};

interface ParamInputProps {
  label: string;
  value: number | string | undefined;
  unit?: string;
  onChange: (value: number | string) => void;
  disabled?: boolean;
  type?: 'number' | 'text' | 'time';
  min?: number;
  max?: number;
  step?: number;
}

function ParamInput({ 
  label, 
  value, 
  unit, 
  onChange, 
  disabled,
  type = 'number',
  min,
  max,
  step = 0.1,
}: ParamInputProps) {
  return (
    <div className="flex items-center justify-between py-1.5">
      <span className="text-xs text-muted-foreground">{label}</span>
      <div className="flex items-center gap-1.5">
        <input
          type={type}
          value={value ?? ''}
          onChange={(e) => onChange(type === 'number' ? parseFloat(e.target.value) : e.target.value)}
          disabled={disabled}
          min={min}
          max={max}
          step={step}
          className={cn(
            'w-16 px-2 py-1 text-xs text-right rounded bg-muted border border-border',
            'focus:outline-none focus:border-cyan-500',
            disabled && 'opacity-50 cursor-not-allowed'
          )}
        />
        {unit && <span className="text-xs text-muted-foreground w-8">{unit}</span>}
      </div>
    </div>
  );
}

function EnvironmentalSection({ 
  params, 
  onChange, 
  disabled 
}: { 
  params: EnvironmentalParams; 
  onChange: (key: keyof EnvironmentalParams, value: number) => void;
  disabled: boolean;
}) {
  return (
    <div className="space-y-3">
      {/* Temperature */}
      <div className="space-y-1">
        <div className="flex items-center gap-1.5 text-xs text-foreground/70 font-medium">
          <Thermometer className="w-3 h-3" />
          Temperature
        </div>
        <div className="pl-4 border-l border-border/50">
          <ParamInput label="Day Target" value={params.tempDayTarget} unit="°F" onChange={(v) => onChange('tempDayTarget', v as number)} disabled={disabled} />
          <ParamInput label="Night Target" value={params.tempNightTarget} unit="°F" onChange={(v) => onChange('tempNightTarget', v as number)} disabled={disabled} />
          <ParamInput label="Tolerance" value={params.tempTolerance} unit="±°F" onChange={(v) => onChange('tempTolerance', v as number)} disabled={disabled} />
        </div>
      </div>

      {/* Humidity */}
      <div className="space-y-1">
        <div className="flex items-center gap-1.5 text-xs text-foreground/70 font-medium">
          <Droplets className="w-3 h-3" />
          Relative Humidity
        </div>
        <div className="pl-4 border-l border-border/50">
          <ParamInput label="Day Target" value={params.rhDayTarget} unit="%" onChange={(v) => onChange('rhDayTarget', v as number)} disabled={disabled} />
          <ParamInput label="Night Target" value={params.rhNightTarget} unit="%" onChange={(v) => onChange('rhNightTarget', v as number)} disabled={disabled} />
          <ParamInput label="Tolerance" value={params.rhTolerance} unit="±%" onChange={(v) => onChange('rhTolerance', v as number)} disabled={disabled} />
        </div>
      </div>

      {/* VPD */}
      <div className="space-y-1">
        <div className="flex items-center gap-1.5 text-xs text-foreground/70 font-medium">
          <Wind className="w-3 h-3" />
          VPD
        </div>
        <div className="pl-4 border-l border-border/50">
          <ParamInput label="Day Target" value={params.vpdDayTarget} unit="kPa" onChange={(v) => onChange('vpdDayTarget', v as number)} disabled={disabled} />
          <ParamInput label="Night Target" value={params.vpdNightTarget} unit="kPa" onChange={(v) => onChange('vpdNightTarget', v as number)} disabled={disabled} />
          <ParamInput label="Tolerance" value={params.vpdTolerance} unit="±kPa" onChange={(v) => onChange('vpdTolerance', v as number)} disabled={disabled} />
        </div>
      </div>

      {/* CO2 */}
      <div className="space-y-1">
        <div className="flex items-center gap-1.5 text-xs text-foreground/70 font-medium">
          <Gauge className="w-3 h-3" />
          CO₂
        </div>
        <div className="pl-4 border-l border-border/50">
          <ParamInput label="Day Target" value={params.co2DayTarget} unit="ppm" onChange={(v) => onChange('co2DayTarget', v as number)} disabled={disabled} step={50} />
          <ParamInput label="Night Target" value={params.co2NightTarget} unit="ppm" onChange={(v) => onChange('co2NightTarget', v as number)} disabled={disabled} step={50} />
          <ParamInput label="Tolerance" value={params.co2Tolerance} unit="±ppm" onChange={(v) => onChange('co2Tolerance', v as number)} disabled={disabled} step={25} />
        </div>
      </div>
    </div>
  );
}

function LightingSection({ 
  params, 
  onChange, 
  disabled 
}: { 
  params: LightingParams; 
  onChange: (key: keyof LightingParams, value: number | string) => void;
  disabled: boolean;
}) {
  return (
    <div className="space-y-3">
      <div className="space-y-1">
        <div className="flex items-center gap-1.5 text-xs text-foreground/70 font-medium">
          <Clock className="w-3 h-3" />
          Schedule
        </div>
        <div className="pl-4 border-l border-border/50">
          <ParamInput label="Photoperiod" value={params.photoperiod} unit="hrs" onChange={(v) => onChange('photoperiod', v as number)} disabled={disabled} step={1} min={0} max={24} />
        </div>
      </div>

      <div className="space-y-1">
        <div className="flex items-center gap-1.5 text-xs text-foreground/70 font-medium">
          <Sun className="w-3 h-3" />
          Intensity
        </div>
        <div className="pl-4 border-l border-border/50">
          <ParamInput label="PPFD Target" value={params.ppfdTarget} unit="µmol" onChange={(v) => onChange('ppfdTarget', v as number)} disabled={disabled} step={25} />
          <ParamInput label="DLI Target" value={params.dliTarget} unit="mol" onChange={(v) => onChange('dliTarget', v as number)} disabled={disabled} />
          <ParamInput label="Ramp Up" value={params.ppfdRampUp} unit="min" onChange={(v) => onChange('ppfdRampUp', v as number)} disabled={disabled} step={5} />
          <ParamInput label="Ramp Down" value={params.ppfdRampDown} unit="min" onChange={(v) => onChange('ppfdRampDown', v as number)} disabled={disabled} step={5} />
        </div>
      </div>
    </div>
  );
}

function IrrigationSection({ 
  params, 
  onChange, 
  disabled 
}: { 
  params: IrrigationParams; 
  onChange: (key: keyof IrrigationParams, value: number | string) => void;
  disabled: boolean;
}) {
  return (
    <div className="space-y-3">
      <div className="space-y-1">
        <div className="flex items-center gap-1.5 text-xs text-foreground/70 font-medium">
          <TrendingUp className="w-3 h-3" />
          Feed Targets
        </div>
        <div className="pl-4 border-l border-border/50">
          <ParamInput label="EC Target" value={params.ecTarget} unit="mS" onChange={(v) => onChange('ecTarget', v as number)} disabled={disabled} />
          <ParamInput label="EC Tolerance" value={params.ecTolerance} unit="±mS" onChange={(v) => onChange('ecTolerance', v as number)} disabled={disabled} />
          <ParamInput label="pH Target" value={params.phTarget} unit="" onChange={(v) => onChange('phTarget', v as number)} disabled={disabled} />
          <ParamInput label="pH Tolerance" value={params.phTolerance} unit="±" onChange={(v) => onChange('phTolerance', v as number)} disabled={disabled} />
        </div>
      </div>

      <div className="space-y-1">
        <div className="flex items-center gap-1.5 text-xs text-foreground/70 font-medium">
          <Zap className="w-3 h-3" />
          Mode: {params.irrigationMode}
        </div>
        <div className="pl-4 border-l border-border/50">
          {params.irrigationMode === 'time-based' && (
            <>
              <ParamInput label="Shots/Day" value={params.shotsPerDay} unit="" onChange={(v) => onChange('shotsPerDay', v as number)} disabled={disabled} step={1} />
              <ParamInput label="Shot Volume" value={params.shotVolumeMl} unit="mL" onChange={(v) => onChange('shotVolumeMl', v as number)} disabled={disabled} step={10} />
            </>
          )}
          {params.irrigationMode === 'sensor-based' && (
            <>
              <ParamInput label="Dryback Target" value={params.drybackTargetPercent} unit="%" onChange={(v) => onChange('drybackTargetPercent', v as number)} disabled={disabled} />
              <ParamInput label="Min Soak" value={params.minSoakMinutes} unit="min" onChange={(v) => onChange('minSoakMinutes', v as number)} disabled={disabled} step={5} />
              <ParamInput label="Max Shots/Day" value={params.maxShotsPerDay} unit="" onChange={(v) => onChange('maxShotsPerDay', v as number)} disabled={disabled} step={1} />
            </>
          )}
          {params.vwcDayTarget !== undefined && (
            <>
              <ParamInput label="VWC Day" value={params.vwcDayTarget} unit="%" onChange={(v) => onChange('vwcDayTarget', v as number)} disabled={disabled} />
              <ParamInput label="VWC Night" value={params.vwcNightTarget} unit="%" onChange={(v) => onChange('vwcNightTarget', v as number)} disabled={disabled} />
            </>
          )}
          {params.runoffTargetPercent !== undefined && (
            <ParamInput label="Runoff Target" value={params.runoffTargetPercent} unit="%" onChange={(v) => onChange('runoffTargetPercent', v as number)} disabled={disabled} />
          )}
        </div>
      </div>
    </div>
  );
}

export function ParameterEditor({
  blueprint,
  phase,
  isEditing,
  onChange,
  onSaveAsNew,
  onReset,
}: ParameterEditorProps) {
  const [expandedSection, setExpandedSection] = useState<ParamSection | null>('environmental');
  const [newBlueprintName, setNewBlueprintName] = useState('');
  const [showSaveDialog, setShowSaveDialog] = useState(false);

  if (!blueprint) {
    return (
      <div className="p-4 text-center text-sm text-muted-foreground">
        No blueprint selected
      </div>
    );
  }

  const sections: ParamSection[] = ['environmental', 'lighting', 'irrigation'];

  const handleParamChange = (section: ParamSection, key: string, value: number | string) => {
    if (!onChange) return;
    
    const currentSection = blueprint[section];
    onChange({
      [section]: {
        ...currentSection,
        [key]: value,
      },
    });
  };

  return (
    <div className="space-y-2">
      {/* Blueprint Header */}
      <div className="flex items-center justify-between px-1 py-2 border-b border-border/50">
        <div>
          <h4 className="text-sm font-medium text-foreground">{blueprint.name}</h4>
          <p className="text-xs text-muted-foreground">
            {blueprint.isDefault ? 'System Default' : 'Custom'}
          </p>
        </div>
        {isEditing && (
          <div className="flex items-center gap-1">
            <button
              onClick={onReset}
              className="p-1.5 text-muted-foreground hover:text-foreground hover:bg-hover rounded transition-colors"
              title="Reset changes"
            >
              <RotateCcw className="w-3.5 h-3.5" />
            </button>
            <button
              onClick={() => setShowSaveDialog(true)}
              className="p-1.5 text-cyan-400 hover:text-cyan-300 hover:bg-cyan-500/10 rounded transition-colors"
              title="Save as new blueprint"
            >
              <Save className="w-3.5 h-3.5" />
            </button>
          </div>
        )}
      </div>

      {/* Save as New Dialog */}
      {showSaveDialog && (
        <div className="p-3 bg-muted/50 rounded-lg border border-cyan-500/30">
          <p className="text-xs text-foreground/70 mb-2">Save as new blueprint:</p>
          <div className="flex gap-2">
            <input
              type="text"
              value={newBlueprintName}
              onChange={(e) => setNewBlueprintName(e.target.value)}
              placeholder="Blueprint name..."
              className="flex-1 px-2 py-1.5 text-xs bg-hover border border-border rounded focus:outline-none focus:border-cyan-500"
            />
            <button
              onClick={() => {
                if (newBlueprintName.trim() && onSaveAsNew) {
                  onSaveAsNew(newBlueprintName.trim());
                  setNewBlueprintName('');
                  setShowSaveDialog(false);
                }
              }}
              disabled={!newBlueprintName.trim()}
              className="px-3 py-1.5 text-xs font-medium text-foreground bg-cyan-500 hover:bg-cyan-400 rounded disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              Save
            </button>
            <button
              onClick={() => setShowSaveDialog(false)}
              className="px-2 py-1.5 text-xs text-muted-foreground hover:text-foreground transition-colors"
            >
              Cancel
            </button>
          </div>
        </div>
      )}

      {/* Parameter Sections */}
      {sections.map((section) => {
        const Icon = SECTION_ICONS[section];
        const isExpanded = expandedSection === section;

        return (
          <div key={section} className="bg-muted/30 rounded-lg overflow-hidden">
            <button
              onClick={() => setExpandedSection(isExpanded ? null : section)}
              className="w-full flex items-center justify-between px-3 py-2.5 hover:bg-hover/30 transition-colors"
            >
              <div className="flex items-center gap-2">
                <Icon className="w-4 h-4 text-muted-foreground" />
                <span className="text-sm font-medium text-foreground">
                  {SECTION_LABELS[section]}
                </span>
              </div>
              {isExpanded ? (
                <ChevronDown className="w-4 h-4 text-muted-foreground" />
              ) : (
                <ChevronRight className="w-4 h-4 text-muted-foreground" />
              )}
            </button>

            {isExpanded && (
              <div className="px-3 pb-3 border-t border-border/30">
                {section === 'environmental' && (
                  <EnvironmentalSection
                    params={blueprint.environmental}
                    onChange={(key, value) => handleParamChange('environmental', key, value)}
                    disabled={!isEditing}
                  />
                )}
                {section === 'lighting' && (
                  <LightingSection
                    params={blueprint.lighting}
                    onChange={(key, value) => handleParamChange('lighting', key, value)}
                    disabled={!isEditing}
                  />
                )}
                {section === 'irrigation' && (
                  <IrrigationSection
                    params={blueprint.irrigation}
                    onChange={(key, value) => handleParamChange('irrigation', key, value)}
                    disabled={!isEditing}
                  />
                )}
              </div>
            )}
          </div>
        );
      })}
    </div>
  );
}










