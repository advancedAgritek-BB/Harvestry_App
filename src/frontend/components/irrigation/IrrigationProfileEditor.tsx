'use client';

import React, { useState, useCallback, useMemo } from 'react';
import { 
  Settings2, 
  Droplets, 
  Target, 
  Sun, 
  Moon, 
  Shield,
  Calculator,
  AlertTriangle,
  Gauge
} from 'lucide-react';
import { FormField, Input, Switch } from '@/components/admin/AdminForm';
import { 
  ShotConfiguration,
  PhaseTargets,
  DayProfile,
  NightProfile,
  SafetyPolicy,
  ZoneCalibration,
  calculateShotDuration,
  DEFAULT_SHOT_CONFIG,
  DEFAULT_PHASE_TARGETS,
  DEFAULT_SAFETY_POLICY,
} from './types';
import { ZoneShotCalibrationModal } from './ZoneShotCalibrationModal';

interface IrrigationProfileEditorProps {
  dayProfile?: Partial<DayProfile>;
  nightProfile?: Partial<NightProfile>;
  safetyPolicy?: Partial<SafetyPolicy>;
  zoneCalibration?: ZoneCalibration | null;
  zones?: Array<{ id: string; name: string }>;
  onDayProfileChange?: (profile: Partial<DayProfile>) => void;
  onNightProfileChange?: (profile: Partial<NightProfile>) => void;
  onSafetyPolicyChange?: (policy: Partial<SafetyPolicy>) => void;
  onCalibrationChange?: (calibration: ZoneCalibration) => void;
  disabled?: boolean;
}

interface SectionHeaderProps {
  icon: React.ReactNode;
  title: string;
  color: string;
  children?: React.ReactNode;
}

function SectionHeader({ icon, title, color, children }: SectionHeaderProps) {
  return (
    <div className="flex items-center justify-between mb-4">
      <h4 className={`text-sm font-medium text-foreground flex items-center gap-2`}>
        <span className={`w-2 h-2 rounded-full ${color}`} />
        {icon}
        {title}
      </h4>
      {children}
    </div>
  );
}

export function IrrigationProfileEditor({
  dayProfile: initialDayProfile,
  nightProfile: initialNightProfile,
  safetyPolicy: initialSafetyPolicy,
  zoneCalibration: initialCalibration,
  zones = [],
  onDayProfileChange,
  onNightProfileChange,
  onSafetyPolicyChange,
  onCalibrationChange,
  disabled = false,
}: IrrigationProfileEditorProps) {
  // State for shot configuration
  const [shotConfig, setShotConfig] = useState<ShotConfiguration>({
    ...DEFAULT_SHOT_CONFIG,
    ...(initialDayProfile?.shotConfig || {}),
  });

  // State for phase targets
  const [phaseTargets, setPhaseTargets] = useState<PhaseTargets>({
    ...DEFAULT_PHASE_TARGETS,
    ...(initialDayProfile?.phaseTargets || {}),
  });

  // State for night profile
  const [nightSettings, setNightSettings] = useState({
    allowIrrigation: initialNightProfile?.allowIrrigation ?? false,
    maintainVwcPercent: initialNightProfile?.maintainVwcPercent ?? 45,
    maxNightShots: initialNightProfile?.maxNightShots ?? 2,
    description: initialNightProfile?.description ?? 'No irrigation, maintain dryback',
  });

  // State for safety policy
  const [safety, setSafety] = useState<SafetyPolicy>({
    ...DEFAULT_SAFETY_POLICY,
    ...(initialSafetyPolicy || {}),
  });

  // State for calibration
  const [calibration, setCalibration] = useState<ZoneCalibration | null>(
    initialCalibration || null
  );
  const [showCalibrationModal, setShowCalibrationModal] = useState(false);

  // Calculate shot duration from calibration and shot size
  const calculatedShotDuration = useMemo(() => {
    if (!calibration || calibration.emitterFlowMlPerSecond <= 0) return 0;
    return calculateShotDuration(
      shotConfig.shotSizeMl,
      calibration.emitterFlowMlPerSecond,
      calibration.emittersPerPlant
    );
  }, [calibration, shotConfig.shotSizeMl]);

  // Update handlers
  const handleShotConfigChange = useCallback((key: keyof ShotConfiguration, value: number) => {
    setShotConfig(prev => {
      const updated = { ...prev, [key]: value };
      onDayProfileChange?.({ shotConfig: updated, phaseTargets });
      return updated;
    });
  }, [phaseTargets, onDayProfileChange]);

  const handlePhaseTargetChange = useCallback((key: keyof PhaseTargets, value: number | boolean) => {
    setPhaseTargets(prev => {
      const updated = { ...prev, [key]: value };
      onDayProfileChange?.({ shotConfig, phaseTargets: updated });
      return updated;
    });
  }, [shotConfig, onDayProfileChange]);

  const handleNightSettingChange = useCallback((key: string, value: number | boolean | string) => {
    setNightSettings(prev => {
      const updated = { ...prev, [key]: value };
      onNightProfileChange?.(updated);
      return updated;
    });
  }, [onNightProfileChange]);

  const handleSafetyChange = useCallback((key: keyof SafetyPolicy, value: number | boolean) => {
    setSafety(prev => {
      const updated = { ...prev, [key]: value };
      onSafetyPolicyChange?.(updated);
      return updated;
    });
  }, [onSafetyPolicyChange]);

  const handleCalibrationComplete = useCallback((newCalibration: ZoneCalibration) => {
    setCalibration(newCalibration);
    onCalibrationChange?.(newCalibration);
  }, [onCalibrationChange]);

  return (
    <div className="space-y-6">
      {/* Shot Configuration Section */}
      <div className="p-4 bg-white/5 rounded-lg space-y-4">
        <SectionHeader 
          icon={<Droplets className="w-4 h-4 text-cyan-400" />}
          title="Shot Configuration"
          color="bg-cyan-400"
        />

        <div className="grid grid-cols-2 gap-4">
          <FormField 
            label="Shot Size" 
            required
            description="Volume per plant (mL)"
          >
            <Input
              type="number"
              min={10}
              max={500}
              step={5}
              value={shotConfig.shotSizeMl}
              onChange={(e) => handleShotConfigChange('shotSizeMl', parseFloat(e.target.value) || 0)}
              disabled={disabled}
            />
          </FormField>

          <FormField 
            label="Calculated Duration"
            description="Based on calibration"
          >
            <div className="flex items-center gap-2">
              <Input
                type="text"
                value={calculatedShotDuration > 0 ? `${calculatedShotDuration.toFixed(1)}s` : 'Not calibrated'}
                disabled
                className="bg-white/5"
              />
              <button
                onClick={() => setShowCalibrationModal(true)}
                disabled={disabled || zones.length === 0}
                className="h-10 px-3 bg-violet-600 hover:bg-violet-500 text-white rounded-lg
                  text-sm font-medium transition-colors flex items-center gap-2
                  disabled:opacity-50 disabled:cursor-not-allowed"
              >
                <Calculator className="w-4 h-4" />
                Calibrate
              </button>
            </div>
          </FormField>
        </div>

        {calibration && (
          <div className="p-3 bg-emerald-500/10 border border-emerald-500/30 rounded-lg text-sm">
            <div className="flex items-center gap-2 text-emerald-400 mb-2">
              <Gauge className="w-4 h-4" />
              <span className="font-medium">Calibration Active</span>
            </div>
            <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 text-muted-foreground">
              <div>
                <span className="text-xs">Zone:</span>
                <span className="text-foreground ml-1">{calibration.zoneName}</span>
              </div>
              <div>
                <span className="text-xs">Media:</span>
                <span className="text-foreground ml-1">
                  {calibration.mediaVolume} {calibration.mediaVolumeUnit}
                </span>
              </div>
              <div>
                <span className="text-xs">Flow Rate:</span>
                <span className="text-cyan-400 ml-1">{calibration.emitterFlowMlPerSecond.toFixed(2)} mL/s</span>
              </div>
              <div>
                <span className="text-xs">Calibrated:</span>
                <span className="text-foreground ml-1">
                  {new Date(calibration.calibratedAt).toLocaleDateString()}
                </span>
              </div>
            </div>
          </div>
        )}

        {!calibration && (
          <div className="p-3 bg-amber-500/10 border border-amber-500/30 rounded-lg text-sm">
            <div className="flex items-center gap-2 text-amber-400">
              <AlertTriangle className="w-4 h-4" />
              <span>No calibration data. Shot duration cannot be calculated.</span>
            </div>
          </div>
        )}

        <div className="grid grid-cols-3 gap-4">
          <FormField 
            label="VWC Increase/Shot"
            description="Expected % increase"
          >
            <Input
              type="number"
              min={0.5}
              max={10}
              step={0.5}
              value={shotConfig.expectedVwcIncreasePercent}
              onChange={(e) => handleShotConfigChange('expectedVwcIncreasePercent', parseFloat(e.target.value) || 0)}
              disabled={disabled}
            />
          </FormField>

          <FormField 
            label="Min Soak Time"
            description="Minutes between shots"
          >
            <Input
              type="number"
              min={5}
              max={120}
              step={5}
              value={shotConfig.minSoakTimeMinutes}
              onChange={(e) => handleShotConfigChange('minSoakTimeMinutes', parseInt(e.target.value) || 0)}
              disabled={disabled}
            />
          </FormField>

          <FormField 
            label="Max Shots/Day"
            description="Daily limit"
          >
            <Input
              type="number"
              min={1}
              max={24}
              step={1}
              value={shotConfig.maxShotsPerDay}
              onChange={(e) => handleShotConfigChange('maxShotsPerDay', parseInt(e.target.value) || 0)}
              disabled={disabled}
            />
          </FormField>
        </div>
      </div>

      {/* Day Profile - Phase Targets */}
      <div className="p-4 bg-white/5 rounded-lg space-y-4">
        <SectionHeader 
          icon={<Sun className="w-4 h-4 text-amber-400" />}
          title="Day Profile - Phase Targets"
          color="bg-amber-400"
        />

        {/* P1 - Ramp Phase */}
        <div className="p-3 bg-white/5 rounded-lg space-y-3">
          <div className="flex items-center gap-2">
            <span className="text-xs font-medium text-violet-400 bg-violet-400/10 px-2 py-0.5 rounded">P1</span>
            <span className="text-sm font-medium text-foreground">Ramp Phase</span>
            <span className="text-xs text-muted-foreground ml-auto">Morning saturation</span>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <FormField label="Target VWC" description="Peak saturation %">
              <Input
                type="number"
                min={40}
                max={80}
                step={1}
                value={phaseTargets.p1TargetVwcPercent}
                onChange={(e) => handlePhaseTargetChange('p1TargetVwcPercent', parseFloat(e.target.value) || 0)}
                disabled={disabled}
              />
            </FormField>
            <FormField label="Shot Count" description="Shots in P1">
              <Input
                type="number"
                min={1}
                max={12}
                step={1}
                value={phaseTargets.p1ShotCount}
                onChange={(e) => handlePhaseTargetChange('p1ShotCount', parseInt(e.target.value) || 0)}
                disabled={disabled}
              />
            </FormField>
          </div>
        </div>

        {/* P2 - Maintenance Phase */}
        <div className="p-3 bg-white/5 rounded-lg space-y-3">
          <div className="flex items-center gap-2">
            <span className="text-xs font-medium text-cyan-400 bg-cyan-400/10 px-2 py-0.5 rounded">P2</span>
            <span className="text-sm font-medium text-foreground">Maintenance Phase</span>
            <span className="text-xs text-muted-foreground ml-auto">Midday stability</span>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <FormField label="Target VWC" description="Maintain at %">
              <Input
                type="number"
                min={30}
                max={70}
                step={1}
                value={phaseTargets.p2TargetVwcPercent}
                onChange={(e) => handlePhaseTargetChange('p2TargetVwcPercent', parseFloat(e.target.value) || 0)}
                disabled={disabled}
              />
            </FormField>
            <FormField label="Shot Count" description="Shots in P2">
              <Input
                type="number"
                min={1}
                max={12}
                step={1}
                value={phaseTargets.p2ShotCount}
                onChange={(e) => handlePhaseTargetChange('p2ShotCount', parseInt(e.target.value) || 0)}
                disabled={disabled}
              />
            </FormField>
          </div>
        </div>

        {/* P3 - Dryback Phase */}
        <div className="p-3 bg-white/5 rounded-lg space-y-3">
          <div className="flex items-center gap-2">
            <span className="text-xs font-medium text-rose-400 bg-rose-400/10 px-2 py-0.5 rounded">P3</span>
            <span className="text-sm font-medium text-foreground">Dryback Phase</span>
            <span className="text-xs text-muted-foreground ml-auto">Evening dry-down</span>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <FormField label="Target Dryback" description="% to dry back">
              <Input
                type="number"
                min={10}
                max={50}
                step={1}
                value={phaseTargets.p3TargetDrybackPercent}
                onChange={(e) => handlePhaseTargetChange('p3TargetDrybackPercent', parseFloat(e.target.value) || 0)}
                disabled={disabled}
              />
            </FormField>
            <FormField label="Emergency Shots" description="Allow if critical">
              <div className="flex items-center h-10">
                <Switch
                  checked={phaseTargets.p3AllowEmergencyShots}
                  onChange={(checked) => handlePhaseTargetChange('p3AllowEmergencyShots', checked)}
                  disabled={disabled}
                />
                <span className="ml-2 text-sm text-muted-foreground">
                  {phaseTargets.p3AllowEmergencyShots ? 'Enabled' : 'Disabled'}
                </span>
              </div>
            </FormField>
          </div>
        </div>
      </div>

      {/* Night Profile */}
      <div className="p-4 bg-white/5 rounded-lg space-y-4">
        <SectionHeader 
          icon={<Moon className="w-4 h-4 text-blue-400" />}
          title="Night Profile"
          color="bg-blue-400"
        />

        <div className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
          <div>
            <div className="text-sm font-medium text-foreground">Allow Night Irrigation</div>
            <div className="text-xs text-muted-foreground">
              Enable shots during dark period
            </div>
          </div>
          <Switch
            checked={nightSettings.allowIrrigation}
            onChange={(checked) => handleNightSettingChange('allowIrrigation', checked)}
            disabled={disabled}
          />
        </div>

        {nightSettings.allowIrrigation && (
          <div className="grid grid-cols-2 gap-4">
            <FormField label="Maintain VWC" description="Target % at night">
              <Input
                type="number"
                min={20}
                max={60}
                step={1}
                value={nightSettings.maintainVwcPercent}
                onChange={(e) => handleNightSettingChange('maintainVwcPercent', parseFloat(e.target.value) || 0)}
                disabled={disabled}
              />
            </FormField>
            <FormField label="Max Night Shots" description="Limit overnight">
              <Input
                type="number"
                min={0}
                max={6}
                step={1}
                value={nightSettings.maxNightShots}
                onChange={(e) => handleNightSettingChange('maxNightShots', parseInt(e.target.value) || 0)}
                disabled={disabled}
              />
            </FormField>
          </div>
        )}

        {!nightSettings.allowIrrigation && (
          <div className="text-sm text-muted-foreground p-3 bg-blue-500/10 border border-blue-500/30 rounded-lg">
            No irrigation during night. Substrate will dryback naturally.
          </div>
        )}
      </div>

      {/* Safety Policy */}
      <div className="p-4 bg-white/5 rounded-lg space-y-4">
        <SectionHeader 
          icon={<Shield className="w-4 h-4 text-rose-400" />}
          title="Safety Policy"
          color="bg-rose-400"
        >
          <span className="text-xs text-muted-foreground">Guardrails and limits</span>
        </SectionHeader>

        <div className="grid grid-cols-3 gap-4">
          <FormField label="Max Volume/Plant/Day" description="Daily limit (mL)">
            <Input
              type="number"
              min={50}
              max={500}
              step={10}
              value={safety.maxVolumeMlPerPlantPerDay}
              onChange={(e) => handleSafetyChange('maxVolumeMlPerPlantPerDay', parseInt(e.target.value) || 0)}
              disabled={disabled}
            />
          </FormField>
          <FormField label="EC Range" description="Min - Max">
            <div className="flex items-center gap-2">
              <Input
                type="number"
                min={0}
                max={5}
                step={0.1}
                value={safety.minEc}
                onChange={(e) => handleSafetyChange('minEc', parseFloat(e.target.value) || 0)}
                disabled={disabled}
                className="w-20"
              />
              <span className="text-muted-foreground">-</span>
              <Input
                type="number"
                min={0}
                max={5}
                step={0.1}
                value={safety.maxEc}
                onChange={(e) => handleSafetyChange('maxEc', parseFloat(e.target.value) || 0)}
                disabled={disabled}
                className="w-20"
              />
            </div>
          </FormField>
          <FormField label="pH Range" description="Min - Max">
            <div className="flex items-center gap-2">
              <Input
                type="number"
                min={4}
                max={8}
                step={0.1}
                value={safety.minPh}
                onChange={(e) => handleSafetyChange('minPh', parseFloat(e.target.value) || 0)}
                disabled={disabled}
                className="w-20"
              />
              <span className="text-muted-foreground">-</span>
              <Input
                type="number"
                min={4}
                max={8}
                step={0.1}
                value={safety.maxPh}
                onChange={(e) => handleSafetyChange('maxPh', parseFloat(e.target.value) || 0)}
                disabled={disabled}
                className="w-20"
              />
            </div>
          </FormField>
        </div>

        <div className="flex gap-6">
          <label className="flex items-center gap-2 cursor-pointer">
            <Switch
              checked={safety.requireFlowVerification}
              onChange={(checked) => handleSafetyChange('requireFlowVerification', checked)}
              disabled={disabled}
            />
            <span className="text-sm text-foreground">Require Flow Verification</span>
          </label>
          <label className="flex items-center gap-2 cursor-pointer">
            <Switch
              checked={safety.requirePressureVerification}
              onChange={(checked) => handleSafetyChange('requirePressureVerification', checked)}
              disabled={disabled}
            />
            <span className="text-sm text-foreground">Require Pressure Verification</span>
          </label>
        </div>
      </div>

      {/* Calibration Modal */}
      <ZoneShotCalibrationModal
        isOpen={showCalibrationModal}
        onClose={() => setShowCalibrationModal(false)}
        onCalibrationComplete={handleCalibrationComplete}
        zones={zones}
        existingCalibration={calibration}
      />
    </div>
  );
}

