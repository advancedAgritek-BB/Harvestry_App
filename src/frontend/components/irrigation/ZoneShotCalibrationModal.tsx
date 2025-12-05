'use client';

import React, { useState, useCallback, useEffect, useRef } from 'react';
import { 
  Play, 
  Square, 
  Droplets, 
  Timer, 
  CheckCircle2, 
  AlertTriangle,
  ChevronRight,
  RotateCcw,
  Beaker
} from 'lucide-react';
import { AdminModal } from '@/components/admin';
import { Button, FormField, Input, Select } from '@/components/admin/AdminForm';
import { 
  CalibrationSession, 
  CalibrationStep,
  calculateEmitterFlowRate,
  ZoneCalibration,
  VolumeUnit,
  convertToMl,
  calculateRecommendedShotSize,
} from './types';

interface ZoneShotCalibrationModalProps {
  isOpen: boolean;
  onClose: () => void;
  onCalibrationComplete: (calibration: ZoneCalibration) => void;
  zones: Array<{ id: string; name: string }>;
  existingCalibration?: ZoneCalibration | null;
}

const STEP_LABELS: Record<CalibrationStep, string> = {
  select_zone: 'Select Zone',
  prepare: 'Prepare Container',
  running: 'Capturing Flow',
  measure: 'Enter Measurement',
  confirm: 'Confirm Calibration',
};

const VOLUME_UNIT_OPTIONS = [
  { value: 'gallons', label: 'Gallons' },
  { value: 'liters', label: 'Liters' },
];

export function ZoneShotCalibrationModal({
  isOpen,
  onClose,
  onCalibrationComplete,
  zones,
  existingCalibration,
}: ZoneShotCalibrationModalProps) {
  const [session, setSession] = useState<CalibrationSession>({
    step: 'select_zone',
    zoneId: '',
    zoneName: '',
    targetVolumeMl: 500,
    emittersPerPlant: 1,
    isValveOpen: false,
  });

  // Media volume state (separate for cleaner UI handling)
  const [mediaVolume, setMediaVolume] = useState(1);
  const [mediaVolumeUnit, setMediaVolumeUnit] = useState<VolumeUnit>('gallons');

  const [elapsedSeconds, setElapsedSeconds] = useState(0);
  const timerRef = useRef<NodeJS.Timeout | null>(null);
  const startTimeRef = useRef<number | null>(null);

  // Calculate media volume in mL
  const mediaVolumeMl = convertToMl(mediaVolume, mediaVolumeUnit);
  
  // Calculate recommended shot size based on media volume
  const recommendedShotSize = calculateRecommendedShotSize(mediaVolumeMl, 3);

  // Reset session when modal opens
  useEffect(() => {
    if (isOpen) {
      setSession({
        step: 'select_zone',
        zoneId: existingCalibration?.zoneId || '',
        zoneName: existingCalibration?.zoneName || '',
        targetVolumeMl: existingCalibration?.targetVolumeMl || 500,
        emittersPerPlant: existingCalibration?.emittersPerPlant || 1,
        isValveOpen: false,
      });
      setMediaVolume(existingCalibration?.mediaVolume || 1);
      setMediaVolumeUnit(existingCalibration?.mediaVolumeUnit || 'gallons');
      setElapsedSeconds(0);
    }
  }, [isOpen, existingCalibration]);

  // Clean up timer on unmount
  useEffect(() => {
    return () => {
      if (timerRef.current) {
        clearInterval(timerRef.current);
      }
    };
  }, []);

  const startTimer = useCallback(() => {
    startTimeRef.current = Date.now();
    timerRef.current = setInterval(() => {
      if (startTimeRef.current) {
        setElapsedSeconds(Math.floor((Date.now() - startTimeRef.current) / 1000));
      }
    }, 100);
  }, []);

  const stopTimer = useCallback(() => {
    if (timerRef.current) {
      clearInterval(timerRef.current);
      timerRef.current = null;
    }
    if (startTimeRef.current) {
      const finalTime = (Date.now() - startTimeRef.current) / 1000;
      setSession(prev => ({
        ...prev,
        measuredTimeSeconds: Math.round(finalTime * 10) / 10,
        stoppedAt: new Date().toISOString(),
        isValveOpen: false,
      }));
      setElapsedSeconds(Math.round(finalTime));
    }
  }, []);

  const handleZoneSelect = (zoneId: string) => {
    const zone = zones.find(z => z.id === zoneId);
    setSession(prev => ({
      ...prev,
      zoneId,
      zoneName: zone?.name || '',
    }));
  };

  const handleStartCalibration = () => {
    setSession(prev => ({
      ...prev,
      step: 'running',
      startedAt: new Date().toISOString(),
      isValveOpen: true,
    }));
    setElapsedSeconds(0);
    startTimer();
    // In real implementation, this would call the API to open the valve
    // POST /zones/{id}/calibrations (start)
  };

  const handleStopCalibration = () => {
    stopTimer();
    setSession(prev => ({
      ...prev,
      step: 'measure',
      isValveOpen: false,
    }));
    // In real implementation, this would call the API to close the valve
    // POST /zones/{id}/calibrations (stop)
  };

  const handleConfirm = () => {
    if (!session.measuredTimeSeconds || session.measuredTimeSeconds <= 0) return;

    const flowRate = calculateEmitterFlowRate(
      session.targetVolumeMl,
      session.measuredTimeSeconds,
      session.emittersPerPlant
    );

    const calibration: ZoneCalibration = {
      zoneId: session.zoneId,
      zoneName: session.zoneName,
      method: 'container',
      targetVolumeMl: session.targetVolumeMl,
      measuredTimeSeconds: session.measuredTimeSeconds,
      runsCount: 1,
      emitterFlowMlPerSecond: flowRate,
      emittersPerPlant: session.emittersPerPlant,
      mediaVolume: mediaVolume,
      mediaVolumeUnit: mediaVolumeUnit,
      calibratedByUserId: 'current-user', // Would come from auth context
      calibratedAt: new Date().toISOString(),
    };

    onCalibrationComplete(calibration);
    onClose();
  };

  const handleReset = () => {
    if (timerRef.current) {
      clearInterval(timerRef.current);
    }
    setSession(prev => ({
      ...prev,
      step: 'prepare',
      startedAt: undefined,
      stoppedAt: undefined,
      measuredTimeSeconds: undefined,
      calculatedFlowRate: undefined,
      isValveOpen: false,
    }));
    setElapsedSeconds(0);
  };

  const formatTime = (seconds: number) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  };

  const calculatedFlowRate = session.measuredTimeSeconds 
    ? calculateEmitterFlowRate(session.targetVolumeMl, session.measuredTimeSeconds, session.emittersPerPlant)
    : 0;

  const renderStepIndicator = () => (
    <div className="flex items-center justify-center gap-2 mb-6">
      {(['select_zone', 'prepare', 'running', 'measure', 'confirm'] as CalibrationStep[]).map((step, index) => {
        const isActive = session.step === step;
        const isPast = ['select_zone', 'prepare', 'running', 'measure', 'confirm'].indexOf(session.step) > index;
        
        return (
          <React.Fragment key={step}>
            <div 
              className={`
                w-8 h-8 rounded-full flex items-center justify-center text-xs font-medium
                transition-all duration-200
                ${isActive 
                  ? 'bg-violet-600 text-white ring-4 ring-violet-600/20' 
                  : isPast 
                    ? 'bg-emerald-600 text-white' 
                    : 'bg-white/10 text-muted-foreground'
                }
              `}
            >
              {isPast ? <CheckCircle2 className="w-4 h-4" /> : index + 1}
            </div>
            {index < 4 && (
              <div className={`w-8 h-0.5 ${isPast ? 'bg-emerald-600' : 'bg-white/10'}`} />
            )}
          </React.Fragment>
        );
      })}
    </div>
  );

  const renderContent = () => {
    switch (session.step) {
      case 'select_zone':
        return (
          <div className="space-y-6">
            <div className="text-center mb-6">
              <Droplets className="w-12 h-12 mx-auto text-cyan-400 mb-3" />
              <h3 className="text-lg font-semibold text-foreground">Zone Shot Time Calibration</h3>
              <p className="text-sm text-muted-foreground mt-1">
                Calibrate valve timing to convert mL dosing to seconds
              </p>
            </div>

            <FormField label="Select Zone to Calibrate" required>
              <Select
                options={zones.map(z => ({ value: z.id, label: z.name }))}
                value={session.zoneId}
                onChange={(e) => handleZoneSelect(e.target.value)}
              />
            </FormField>

            {/* Media Volume Section */}
            <div className="p-4 bg-white/5 rounded-lg space-y-4">
              <h4 className="text-sm font-medium text-foreground flex items-center gap-2">
                <Beaker className="w-4 h-4 text-violet-400" />
                Media Volume Per Plant
              </h4>
              
              <div className="grid grid-cols-2 gap-4">
                <FormField 
                  label="Volume" 
                  required
                  description="Container/pot size"
                >
                  <Input
                    type="number"
                    min={0.1}
                    max={100}
                    step={0.5}
                    value={mediaVolume}
                    onChange={(e) => setMediaVolume(parseFloat(e.target.value) || 1)}
                  />
                </FormField>

                <FormField 
                  label="Unit" 
                  required
                >
                  <Select
                    options={VOLUME_UNIT_OPTIONS}
                    value={mediaVolumeUnit}
                    onChange={(e) => setMediaVolumeUnit(e.target.value as VolumeUnit)}
                  />
                </FormField>
              </div>

              {/* Calculated info */}
              <div className="p-3 bg-cyan-500/10 border border-cyan-500/30 rounded-lg">
                <div className="grid grid-cols-2 gap-4 text-sm">
                  <div>
                    <span className="text-muted-foreground">Media Volume:</span>
                    <span className="text-cyan-400 ml-2 font-medium">
                      {mediaVolumeMl.toLocaleString()} mL
                    </span>
                  </div>
                  <div>
                    <span className="text-muted-foreground">Rec. Shot Size:</span>
                    <span className="text-emerald-400 ml-2 font-medium">
                      ~{recommendedShotSize} mL
                    </span>
                  </div>
                </div>
                <p className="text-xs text-muted-foreground mt-2">
                  Based on ~3% VWC increase per shot
                </p>
              </div>
            </div>

            <FormField 
              label="Target Container Volume" 
              required
              description="Volume to fill during calibration (mL)"
            >
              <Input
                type="number"
                min={100}
                max={2000}
                step={50}
                value={session.targetVolumeMl}
                onChange={(e) => setSession(prev => ({ 
                  ...prev, 
                  targetVolumeMl: parseInt(e.target.value) || 500 
                }))}
              />
            </FormField>

            <FormField 
              label="Emitters Per Plant" 
              required
              description="Number of drip emitters per plant in this zone"
            >
              <Input
                type="number"
                min={1}
                max={8}
                step={1}
                value={session.emittersPerPlant}
                onChange={(e) => setSession(prev => ({ 
                  ...prev, 
                  emittersPerPlant: parseInt(e.target.value) || 1 
                }))}
              />
            </FormField>
          </div>
        );

      case 'prepare':
        return (
          <div className="space-y-6">
            <div className="text-center mb-6">
              <Beaker className="w-12 h-12 mx-auto text-amber-400 mb-3" />
              <h3 className="text-lg font-semibold text-foreground">Prepare Your Container</h3>
              <p className="text-sm text-muted-foreground mt-1">
                Zone: <span className="text-cyan-400">{session.zoneName}</span>
              </p>
            </div>

            <div className="bg-amber-500/10 border border-amber-500/30 rounded-lg p-4">
              <div className="flex gap-3">
                <AlertTriangle className="w-5 h-5 text-amber-400 flex-shrink-0 mt-0.5" />
                <div className="space-y-2 text-sm">
                  <p className="font-medium text-amber-400">Before Starting:</p>
                  <ol className="list-decimal list-inside space-y-1 text-muted-foreground">
                    <li>Place a measuring container ({session.targetVolumeMl}mL capacity) under the emitter</li>
                    <li>Ensure the container is empty and positioned correctly</li>
                    <li>Make sure line pressure is at normal operating level</li>
                    <li>Clear the area of obstacles</li>
                  </ol>
                </div>
              </div>
            </div>

            <div className="p-4 bg-white/5 rounded-lg">
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div>
                  <span className="text-muted-foreground">Media Volume:</span>
                  <span className="text-foreground ml-2 font-medium">{mediaVolume} {mediaVolumeUnit}</span>
                </div>
                <div>
                  <span className="text-muted-foreground">Target Calibration:</span>
                  <span className="text-foreground ml-2 font-medium">{session.targetVolumeMl} mL</span>
                </div>
                <div>
                  <span className="text-muted-foreground">Emitters:</span>
                  <span className="text-foreground ml-2 font-medium">{session.emittersPerPlant} per plant</span>
                </div>
                <div>
                  <span className="text-muted-foreground">Rec. Shot:</span>
                  <span className="text-emerald-400 ml-2 font-medium">~{recommendedShotSize} mL</span>
                </div>
              </div>
            </div>
          </div>
        );

      case 'running':
        return (
          <div className="space-y-6">
            <div className="text-center">
              <div className="relative w-32 h-32 mx-auto mb-4">
                <div className="absolute inset-0 rounded-full border-4 border-white/10" />
                <div 
                  className="absolute inset-0 rounded-full border-4 border-cyan-400 animate-pulse"
                  style={{ 
                    clipPath: 'polygon(50% 50%, 50% 0%, 100% 0%, 100% 100%, 0% 100%, 0% 0%, 50% 0%)',
                  }}
                />
                <div className="absolute inset-0 flex items-center justify-center">
                  <div>
                    <Timer className="w-8 h-8 text-cyan-400 mx-auto mb-1" />
                    <span className="text-2xl font-mono font-bold text-foreground">
                      {formatTime(elapsedSeconds)}
                    </span>
                  </div>
                </div>
              </div>
              <h3 className="text-lg font-semibold text-foreground">Valve Open - Filling Container</h3>
              <p className="text-sm text-cyan-400 mt-1 animate-pulse">
                Zone: {session.zoneName}
              </p>
            </div>

            <div className="bg-cyan-500/10 border border-cyan-500/30 rounded-lg p-4">
              <p className="text-sm text-center text-muted-foreground">
                Stop the timer when the container reaches <span className="text-cyan-400 font-medium">{session.targetVolumeMl}mL</span>
              </p>
            </div>

            <div className="flex justify-center">
              <Button
                variant="danger"
                size="lg"
                onClick={handleStopCalibration}
                className="px-8"
              >
                <Square className="w-5 h-5" />
                Stop Calibration
              </Button>
            </div>
          </div>
        );

      case 'measure':
        return (
          <div className="space-y-6">
            <div className="text-center mb-4">
              <CheckCircle2 className="w-12 h-12 mx-auto text-emerald-400 mb-3" />
              <h3 className="text-lg font-semibold text-foreground">Calibration Complete</h3>
              <p className="text-sm text-muted-foreground mt-1">
                Verify and adjust the measured time if needed
              </p>
            </div>

            <FormField 
              label="Measured Time" 
              required
              description="Time to fill container (seconds)"
            >
              <Input
                type="number"
                min={1}
                max={300}
                step={0.1}
                value={session.measuredTimeSeconds || elapsedSeconds}
                onChange={(e) => setSession(prev => ({ 
                  ...prev, 
                  measuredTimeSeconds: parseFloat(e.target.value) || 0 
                }))}
              />
            </FormField>

            <div className="p-4 bg-white/5 rounded-lg space-y-3">
              <h4 className="text-sm font-medium text-foreground">Calculated Results</h4>
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div>
                  <span className="text-muted-foreground">Total Flow Rate:</span>
                  <span className="text-cyan-400 ml-2 font-medium">
                    {(calculatedFlowRate * session.emittersPerPlant).toFixed(2)} mL/s
                  </span>
                </div>
                <div>
                  <span className="text-muted-foreground">Per Emitter:</span>
                  <span className="text-cyan-400 ml-2 font-medium">
                    {calculatedFlowRate.toFixed(2)} mL/s
                  </span>
                </div>
              </div>
            </div>

            <div className="flex justify-center">
              <Button
                variant="ghost"
                onClick={handleReset}
                className="text-muted-foreground"
              >
                <RotateCcw className="w-4 h-4" />
                Run Again
              </Button>
            </div>
          </div>
        );

      case 'confirm':
        return (
          <div className="space-y-6">
            <div className="text-center mb-4">
              <CheckCircle2 className="w-12 h-12 mx-auto text-emerald-400 mb-3" />
              <h3 className="text-lg font-semibold text-foreground">Confirm Calibration</h3>
            </div>

            <div className="p-4 bg-white/5 rounded-lg space-y-4">
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div>
                  <span className="text-muted-foreground">Zone:</span>
                  <span className="text-foreground ml-2">{session.zoneName}</span>
                </div>
                <div>
                  <span className="text-muted-foreground">Method:</span>
                  <span className="text-foreground ml-2">Container</span>
                </div>
                <div>
                  <span className="text-muted-foreground">Media Volume:</span>
                  <span className="text-foreground ml-2">
                    {mediaVolume} {mediaVolumeUnit} ({mediaVolumeMl.toLocaleString()} mL)
                  </span>
                </div>
                <div>
                  <span className="text-muted-foreground">Emitters/Plant:</span>
                  <span className="text-foreground ml-2">{session.emittersPerPlant}</span>
                </div>
                <div>
                  <span className="text-muted-foreground">Calibration Volume:</span>
                  <span className="text-foreground ml-2">{session.targetVolumeMl} mL</span>
                </div>
                <div>
                  <span className="text-muted-foreground">Measured Time:</span>
                  <span className="text-foreground ml-2">{session.measuredTimeSeconds?.toFixed(1)}s</span>
                </div>
              </div>
              
              <div className="border-t border-white/10 pt-4 mt-4">
                <div className="text-sm">
                  <span className="text-muted-foreground">Flow Rate:</span>
                  <span className="text-cyan-400 ml-2 font-medium text-lg">
                    {calculatedFlowRate.toFixed(2)} mL/s per emitter
                  </span>
                </div>
              </div>
            </div>

            <div className="p-4 bg-emerald-500/10 border border-emerald-500/30 rounded-lg space-y-2">
              <p className="text-sm text-muted-foreground">
                <span className="text-emerald-400 font-medium">Recommended Shot:</span> ~{recommendedShotSize}mL will take{' '}
                <span className="text-emerald-400 font-medium">
                  {calculatedFlowRate > 0 ? (recommendedShotSize / (calculatedFlowRate * session.emittersPerPlant)).toFixed(1) : '—'}s
                </span>
              </p>
              <p className="text-xs text-muted-foreground">
                This represents ~3% of your {mediaVolume} {mediaVolumeUnit} media volume
              </p>
            </div>
          </div>
        );

      default:
        return null;
    }
  };

  const canProceed = () => {
    switch (session.step) {
      case 'select_zone':
        return session.zoneId && session.targetVolumeMl > 0 && session.emittersPerPlant > 0;
      case 'prepare':
        return true;
      case 'running':
        return false;
      case 'measure':
        return (session.measuredTimeSeconds || 0) > 0;
      case 'confirm':
        return true;
      default:
        return false;
    }
  };

  const handleNext = () => {
    const steps: CalibrationStep[] = ['select_zone', 'prepare', 'running', 'measure', 'confirm'];
    const currentIndex = steps.indexOf(session.step);
    
    if (session.step === 'prepare') {
      handleStartCalibration();
    } else if (session.step === 'confirm') {
      handleConfirm();
    } else if (currentIndex < steps.length - 1) {
      setSession(prev => ({ ...prev, step: steps[currentIndex + 1] }));
    }
  };

  const handleBack = () => {
    const steps: CalibrationStep[] = ['select_zone', 'prepare', 'running', 'measure', 'confirm'];
    const currentIndex = steps.indexOf(session.step);
    
    if (currentIndex > 0) {
      setSession(prev => ({ ...prev, step: steps[currentIndex - 1] }));
    }
  };

  return (
    <AdminModal
      isOpen={isOpen}
      onClose={onClose}
      title="Zone Shot Time Calibration"
      description={STEP_LABELS[session.step]}
      size="md"
      footer={
        <div className="flex justify-between w-full">
          <div>
            {session.step !== 'select_zone' && session.step !== 'running' && (
              <Button variant="ghost" onClick={handleBack}>
                Back
              </Button>
            )}
          </div>
          <div className="flex gap-2">
            <Button variant="ghost" onClick={onClose}>
              Cancel
            </Button>
            {session.step !== 'running' && (
              <Button onClick={handleNext} disabled={!canProceed()}>
                {session.step === 'prepare' && (
                  <>
                    <Play className="w-4 h-4" />
                    Start Calibration
                  </>
                )}
                {session.step === 'confirm' && 'Save Calibration'}
                {session.step !== 'prepare' && session.step !== 'confirm' && (
                  <>
                    Next
                    <ChevronRight className="w-4 h-4" />
                  </>
                )}
              </Button>
            )}
          </div>
        </div>
      }
    >
      {renderStepIndicator()}
      {renderContent()}
    </AdminModal>
  );
}

