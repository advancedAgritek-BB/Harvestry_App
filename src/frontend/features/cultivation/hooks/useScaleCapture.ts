/**
 * useScaleCapture Hook
 * Manages scale capture state machine with double-read prevention
 */

import { useState, useCallback, useRef, useEffect } from 'react';
import type {
  ScaleCaptureState,
  ScaleCaptureSettings,
  DEFAULT_SCALE_CAPTURE_SETTINGS,
} from '@/features/inventory/types/harvest-workflow.types';

interface UseScaleCaptureOptions {
  settings?: Partial<ScaleCaptureSettings>;
  onWeightCaptured?: (weight: number, isStable: boolean) => void;
  onStateChange?: (state: ScaleCaptureState) => void;
}

interface ScaleCaptureResult {
  state: ScaleCaptureState;
  currentWeight: number;
  isStable: boolean;
  stabilityDurationMs: number;
  lastCapturedWeight: number | null;
  lastCapturedAt: Date | null;
  recentWeights: number[];
  warningMessage: string | null;
  errorMessage: string | null;
  
  // Actions
  processScaleReading: (weight: number, isStable: boolean) => void;
  captureWeight: () => boolean;
  tare: () => void;
  reset: () => void;
  skipPlant: () => void;
}

const DEFAULT_SETTINGS: ScaleCaptureSettings = {
  plantDetectionThreshold: 10,
  scaleClearThreshold: 5,
  minimumWeightChange: 20,
  captureCooldownMs: 1500,
  stabilityDurationMs: 750,
  duplicateWarningPercent: 5,
  duplicateComparisonCount: 3,
  autoCaptureOnStable: true,
  audioEnabled: true,
};

export function useScaleCapture(options: UseScaleCaptureOptions = {}): ScaleCaptureResult {
  const settings = { ...DEFAULT_SETTINGS, ...options.settings };
  
  const [state, setState] = useState<ScaleCaptureState>('waiting_for_plant');
  const [currentWeight, setCurrentWeight] = useState(0);
  const [isStable, setIsStable] = useState(false);
  const [stabilityDurationMs, setStabilityDurationMs] = useState(0);
  const [lastCapturedWeight, setLastCapturedWeight] = useState<number | null>(null);
  const [lastCapturedAt, setLastCapturedAt] = useState<Date | null>(null);
  const [recentWeights, setRecentWeights] = useState<number[]>([]);
  const [warningMessage, setWarningMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  
  const stabilityStartRef = useRef<number | null>(null);
  const lastReadingRef = useRef<number>(0);
  const cooldownTimeoutRef = useRef<NodeJS.Timeout | null>(null);
  const isInCooldownRef = useRef(false);

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      if (cooldownTimeoutRef.current) {
        clearTimeout(cooldownTimeoutRef.current);
      }
    };
  }, []);

  // Play audio cue
  const playAudio = useCallback((type: 'success' | 'error' | 'warning' | 'stable') => {
    if (!settings.audioEnabled) return;
    
    // In a real implementation, these would play actual audio files
    // For now, we'll use the Web Audio API for simple beeps
    try {
      const audioContext = new (window.AudioContext || (window as any).webkitAudioContext)();
      const oscillator = audioContext.createOscillator();
      const gainNode = audioContext.createGain();
      
      oscillator.connect(gainNode);
      gainNode.connect(audioContext.destination);
      
      const frequencies: Record<string, number> = {
        success: 880,   // High A
        error: 220,     // Low A
        warning: 440,   // Middle A
        stable: 660,    // E
      };
      
      oscillator.frequency.value = frequencies[type] || 440;
      oscillator.type = 'sine';
      gainNode.gain.value = 0.1;
      
      oscillator.start();
      oscillator.stop(audioContext.currentTime + 0.1);
    } catch (e) {
      // Audio not available
    }
  }, [settings.audioEnabled]);

  // Check for duplicate weight
  const checkForDuplicate = useCallback((weight: number): string | null => {
    if (recentWeights.length === 0) return null;
    
    const threshold = weight * (settings.duplicateWarningPercent / 100);
    const compareCount = Math.min(settings.duplicateComparisonCount, recentWeights.length);
    
    for (let i = 0; i < compareCount; i++) {
      const diff = Math.abs(weight - recentWeights[i]);
      if (diff <= threshold) {
        return `Weight ${weight.toFixed(1)}g is within ${settings.duplicateWarningPercent}% of recent weight ${recentWeights[i].toFixed(1)}g`;
      }
    }
    
    return null;
  }, [recentWeights, settings.duplicateWarningPercent, settings.duplicateComparisonCount]);

  // Process incoming scale reading
  const processScaleReading = useCallback((weight: number, stable: boolean) => {
    setCurrentWeight(weight);
    setIsStable(stable);
    setErrorMessage(null);
    
    const now = Date.now();
    
    // Track stability duration
    if (stable && Math.abs(weight - lastReadingRef.current) < 0.5) {
      if (!stabilityStartRef.current) {
        stabilityStartRef.current = now;
      }
      setStabilityDurationMs(now - stabilityStartRef.current);
    } else {
      stabilityStartRef.current = null;
      setStabilityDurationMs(0);
    }
    
    lastReadingRef.current = weight;
    
    // State machine transitions
    switch (state) {
      case 'waiting_for_plant':
        if (weight >= settings.plantDetectionThreshold) {
          setState('plant_detected');
          options.onStateChange?.('plant_detected');
        }
        break;
        
      case 'plant_detected':
        if (weight < settings.scaleClearThreshold) {
          setState('waiting_for_plant');
          options.onStateChange?.('waiting_for_plant');
        } else if (stable) {
          setState('stabilizing');
          stabilityStartRef.current = now;
          options.onStateChange?.('stabilizing');
        }
        break;
        
      case 'stabilizing':
        if (weight < settings.scaleClearThreshold) {
          setState('waiting_for_plant');
          options.onStateChange?.('waiting_for_plant');
        } else if (!stable) {
          setState('plant_detected');
          stabilityStartRef.current = null;
          options.onStateChange?.('plant_detected');
        } else if (stabilityStartRef.current && (now - stabilityStartRef.current) >= settings.stabilityDurationMs) {
          // Weight is stable long enough
          playAudio('stable');
          
          if (settings.autoCaptureOnStable && !isInCooldownRef.current) {
            // Auto-capture
            const captured = performCapture(weight);
            if (captured) {
              setState('stable_captured');
              options.onStateChange?.('stable_captured');
            }
          }
        }
        break;
        
      case 'stable_captured':
        // Wait for plant removal
        if (weight < settings.scaleClearThreshold) {
          setState('scale_clear');
          options.onStateChange?.('scale_clear');
        }
        break;
        
      case 'waiting_for_removal':
        if (weight < settings.scaleClearThreshold) {
          setState('scale_clear');
          options.onStateChange?.('scale_clear');
        }
        break;
        
      case 'scale_clear':
        // Brief transition state, move to waiting
        setState('waiting_for_plant');
        setWarningMessage(null);
        options.onStateChange?.('waiting_for_plant');
        break;
    }
  }, [state, settings, playAudio, options]);

  // Perform the actual capture
  const performCapture = useCallback((weight: number): boolean => {
    // Check cooldown
    if (isInCooldownRef.current) {
      setWarningMessage('Please wait before capturing another weight');
      return false;
    }
    
    // Check minimum weight change from last capture
    if (lastCapturedWeight !== null) {
      const weightChange = Math.abs(weight - lastCapturedWeight);
      if (weightChange < settings.minimumWeightChange) {
        setWarningMessage(`Weight change (${weightChange.toFixed(1)}g) is below minimum (${settings.minimumWeightChange}g)`);
        return false;
      }
    }
    
    // Check for duplicates
    const duplicateWarning = checkForDuplicate(weight);
    if (duplicateWarning) {
      setWarningMessage(duplicateWarning);
      // Still allow capture but show warning
    }
    
    // Capture successful
    setLastCapturedWeight(weight);
    setLastCapturedAt(new Date());
    setRecentWeights(prev => [weight, ...prev.slice(0, settings.duplicateComparisonCount - 1)]);
    
    playAudio('success');
    options.onWeightCaptured?.(weight, true);
    
    // Start cooldown
    isInCooldownRef.current = true;
    cooldownTimeoutRef.current = setTimeout(() => {
      isInCooldownRef.current = false;
    }, settings.captureCooldownMs);
    
    return true;
  }, [lastCapturedWeight, settings, checkForDuplicate, playAudio, options]);

  // Manual capture action
  const captureWeight = useCallback((): boolean => {
    if (state !== 'stabilizing' && state !== 'plant_detected') {
      setErrorMessage('Cannot capture - no plant on scale or weight not stable');
      playAudio('error');
      return false;
    }
    
    if (currentWeight < settings.plantDetectionThreshold) {
      setErrorMessage('Weight too low to capture');
      playAudio('error');
      return false;
    }
    
    const captured = performCapture(currentWeight);
    if (captured) {
      setState('stable_captured');
      options.onStateChange?.('stable_captured');
    }
    return captured;
  }, [state, currentWeight, settings.plantDetectionThreshold, performCapture, playAudio, options]);

  // Tare action
  const tare = useCallback(() => {
    // In real implementation, this would send tare command to scale
    setCurrentWeight(0);
    setStabilityDurationMs(0);
    stabilityStartRef.current = null;
  }, []);

  // Reset state
  const reset = useCallback(() => {
    setState('waiting_for_plant');
    setCurrentWeight(0);
    setIsStable(false);
    setStabilityDurationMs(0);
    setWarningMessage(null);
    setErrorMessage(null);
    stabilityStartRef.current = null;
    isInCooldownRef.current = false;
    if (cooldownTimeoutRef.current) {
      clearTimeout(cooldownTimeoutRef.current);
    }
  }, []);

  // Skip current plant
  const skipPlant = useCallback(() => {
    setState('waiting_for_plant');
    setWarningMessage(null);
    options.onStateChange?.('waiting_for_plant');
  }, [options]);

  return {
    state,
    currentWeight,
    isStable,
    stabilityDurationMs,
    lastCapturedWeight,
    lastCapturedAt,
    recentWeights,
    warningMessage,
    errorMessage,
    processScaleReading,
    captureWeight,
    tare,
    reset,
    skipPlant,
  };
}





