'use client';

/**
 * ScaleLiveDisplay Component
 * Real-time scale weight display with stability indicator and calibration status
 */

import { cn } from '@/lib/utils';
import type { ScaleCaptureState, ScaleDevice } from '@/features/inventory/types';
import { CAPTURE_STATE_CONFIG } from '@/features/inventory/types/harvest-workflow.types';

interface ScaleLiveDisplayProps {
  /** Current weight reading */
  weight: number;
  /** Unit of measurement */
  uom?: string;
  /** Whether weight is stable */
  isStable: boolean;
  /** Current capture state */
  captureState: ScaleCaptureState;
  /** Stability duration in milliseconds */
  stabilityDurationMs?: number;
  /** Required stability duration for capture */
  requiredStabilityMs?: number;
  /** Connected scale device */
  scaleDevice?: ScaleDevice | null;
  /** Warning message to display */
  warningMessage?: string | null;
  /** Error message to display */
  errorMessage?: string | null;
  /** Click handler for capture button */
  onCapture?: () => void;
  /** Click handler for tare button */
  onTare?: () => void;
  /** Click handler for skip button */
  onSkip?: () => void;
  /** Whether capture button is disabled */
  captureDisabled?: boolean;
  /** Additional class names */
  className?: string;
}

export function ScaleLiveDisplay({
  weight,
  uom = 'g',
  isStable,
  captureState,
  stabilityDurationMs = 0,
  requiredStabilityMs = 750,
  scaleDevice,
  warningMessage,
  errorMessage,
  onCapture,
  onTare,
  onSkip,
  captureDisabled = false,
  className,
}: ScaleLiveDisplayProps) {
  const stateConfig = CAPTURE_STATE_CONFIG[captureState];
  const stabilityPercent = Math.min(100, (stabilityDurationMs / requiredStabilityMs) * 100);
  
  // Determine display color based on state
  const getWeightColor = () => {
    if (captureState === 'stable_captured') return 'text-emerald-400';
    if (captureState === 'waiting_for_removal') return 'text-cyan-400';
    if (isStable && stabilityPercent >= 100) return 'text-emerald-400';
    if (captureState === 'stabilizing') return 'text-amber-400';
    if (weight > 0) return 'text-white';
    return 'text-muted-foreground';
  };

  return (
    <div className={cn('flex flex-col', className)}>
      {/* Main weight display */}
      <div className="bg-card/50 rounded-lg p-6 border border-border/50">
        {/* Weight value */}
        <div className="text-center mb-4">
          <div className={cn(
            'font-mono text-6xl font-bold tracking-tight transition-colors',
            getWeightColor()
          )}>
            {weight.toFixed(1)}
            <span className="text-3xl ml-2 text-muted-foreground">{uom}</span>
          </div>
          
          {/* Stability indicator */}
          <div className="mt-2 flex items-center justify-center gap-2">
            <div className={cn(
              'h-2 w-2 rounded-full',
              isStable ? 'bg-emerald-400' : 'bg-amber-400 animate-pulse'
            )} />
            <span className={cn('text-sm font-medium', stateConfig.color)}>
              {isStable ? 'STABLE' : 'UNSTABLE'}
            </span>
          </div>
          
          {/* Stability progress bar */}
          {captureState === 'stabilizing' && (
            <div className="mt-3 h-1.5 bg-muted rounded-full overflow-hidden">
              <div 
                className="h-full bg-amber-400 transition-all duration-100"
                style={{ width: `${stabilityPercent}%` }}
              />
            </div>
          )}
        </div>
        
        {/* State message */}
        <div className={cn(
          'text-center py-2 px-4 rounded-md',
          stateConfig.bgColor
        )}>
          <span className={cn('text-sm font-medium', stateConfig.color)}>
            {stateConfig.instruction}
          </span>
        </div>
        
        {/* Warning/Error messages */}
        {warningMessage && (
          <div className="mt-3 p-2 bg-amber-500/10 border border-amber-500/20 rounded-md">
            <p className="text-sm text-amber-400 text-center">{warningMessage}</p>
          </div>
        )}
        
        {errorMessage && (
          <div className="mt-3 p-2 bg-rose-500/10 border border-rose-500/20 rounded-md">
            <p className="text-sm text-rose-400 text-center">{errorMessage}</p>
          </div>
        )}
        
        {/* Action buttons */}
        <div className="mt-4 flex gap-2">
          <button
            onClick={onTare}
            className="flex-1 px-4 py-2 bg-muted hover:bg-muted/80 rounded-md text-sm font-medium transition-colors"
          >
            TARE
          </button>
          <button
            onClick={onCapture}
            disabled={captureDisabled || captureState === 'waiting_for_plant' || captureState === 'stable_captured'}
            className={cn(
              'flex-1 px-4 py-2 rounded-md text-sm font-medium transition-colors',
              captureDisabled || captureState === 'waiting_for_plant' || captureState === 'stable_captured'
                ? 'bg-muted text-muted-foreground cursor-not-allowed'
                : 'bg-emerald-600 hover:bg-emerald-500 text-white'
            )}
          >
            CAPTURE
          </button>
          <button
            onClick={onSkip}
            className="flex-1 px-4 py-2 bg-muted hover:bg-muted/80 rounded-md text-sm font-medium transition-colors"
          >
            SKIP
          </button>
        </div>
      </div>
      
      {/* Scale device info */}
      <div className="mt-3 flex items-center justify-between text-xs text-muted-foreground">
        <div className="flex items-center gap-2">
          <span>Scale:</span>
          <span className="font-medium text-foreground">
            {scaleDevice ? `${scaleDevice.manufacturer} ${scaleDevice.model}` : 'Manual Entry'}
          </span>
        </div>
        
        {scaleDevice && (
          <div className="flex items-center gap-2">
            <span>Cal:</span>
            {scaleDevice.isCalibrationValid ? (
              <span className="text-emerald-400 flex items-center gap-1">
                Valid
                {scaleDevice.currentCalibration && (
                  <span className="text-muted-foreground">
                    until {scaleDevice.currentCalibration.calibrationDueDate}
                  </span>
                )}
                <span className="text-emerald-400">✓</span>
              </span>
            ) : (
              <span className="text-rose-400 flex items-center gap-1">
                Expired <span>⚠</span>
              </span>
            )}
          </div>
        )}
      </div>
    </div>
  );
}

export default ScaleLiveDisplay;
