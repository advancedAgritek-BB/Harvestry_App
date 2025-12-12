'use client';

/**
 * HarvestWeighingStation Page
 * Primary interface for weighing plants during harvest with scale integration
 */

import { useState, useCallback, useEffect } from 'react';
import { cn } from '@/lib/utils';
import { useScaleCapture } from '../hooks/useScaleCapture';
import { ScaleLiveDisplay } from '../components/harvest/ScaleLiveDisplay';
import { PlantQueueList } from '../components/harvest/PlantQueueList';
import { PinOverrideModal } from '../components/harvest/PinOverrideModal';
import { HarvestMetricsCard } from '../components/harvest/HarvestMetricsCard';
import type {
  HarvestWorkflowState,
  HarvestPlantWeighing,
  HarvestMetrics,
  ScaleDevice,
  WeightAdjustmentReasonCode,
} from '@/features/inventory/types';

interface HarvestWeighingStationProps {
  /** Harvest ID */
  harvestId: string;
  /** Initial harvest data */
  initialData?: HarvestWorkflowState;
  /** Available scale devices */
  scaleDevices?: ScaleDevice[];
  /** Callback when weight is captured */
  onWeightCaptured?: (plantId: string, weight: number) => Promise<void>;
  /** Callback when session is finished */
  onFinishSession?: () => void;
  /** Callback when weight adjustment is requested */
  onAdjustWeight?: (
    plantId: string | null,
    newWeight: number,
    reasonCode: WeightAdjustmentReasonCode,
    pin: string,
    notes?: string
  ) => Promise<void>;
}

// Mock data for development
const mockPlants: HarvestPlantWeighing[] = [
  { id: '1', harvestId: 'h1', plantId: 'p1', plantTag: 'BD-001', wetWeight: 234.5, unitOfWeight: 'g', harvestedAt: '', weightSource: 'scale', isWeightLocked: true },
  { id: '2', harvestId: 'h1', plantId: 'p2', plantTag: 'BD-002', wetWeight: 189.2, unitOfWeight: 'g', harvestedAt: '', weightSource: 'scale', isWeightLocked: true },
  { id: '3', harvestId: 'h1', plantId: 'p3', plantTag: 'BD-003', wetWeight: 312.8, unitOfWeight: 'g', harvestedAt: '', weightSource: 'scale', isWeightLocked: false },
  { id: '4', harvestId: 'h1', plantId: 'p4', plantTag: 'BD-004', wetWeight: 0, unitOfWeight: 'g', harvestedAt: '', weightSource: 'manual', isWeightLocked: false, status: 'pending' },
  { id: '5', harvestId: 'h1', plantId: 'p5', plantTag: 'BD-005', wetWeight: 0, unitOfWeight: 'g', harvestedAt: '', weightSource: 'manual', isWeightLocked: false, status: 'pending' },
  { id: '6', harvestId: 'h1', plantId: 'p6', plantTag: 'BD-006', wetWeight: 0, unitOfWeight: 'g', harvestedAt: '', weightSource: 'manual', isWeightLocked: false, status: 'pending' },
];

const mockMetrics: HarvestMetrics = {
  wetWeight: 736.5,
  dryWeight: 0,
  buckedFlowerWeight: 0,
  totalWasteWeight: 0,
  stemWaste: 0,
  leafWaste: 0,
  otherWaste: 0,
  moistureLossPercent: 0,
  dryToWetRatio: 0,
  usableFlowerPercent: 0,
  wastePercent: 0,
  yieldPerPlant: 245.5,
  plantsHarvested: 6,
  plantsWeighed: 3,
};

export function HarvestWeighingStation({
  harvestId,
  initialData,
  scaleDevices = [],
  onWeightCaptured,
  onFinishSession,
  onAdjustWeight,
}: HarvestWeighingStationProps) {
  // State
  const [plants, setPlants] = useState<HarvestPlantWeighing[]>(initialData?.plants || mockPlants);
  const [metrics, setMetrics] = useState<HarvestMetrics>(initialData?.metrics || mockMetrics);
  const [selectedPlant, setSelectedPlant] = useState<HarvestPlantWeighing | null>(null);
  const [weighingPlant, setWeighingPlant] = useState<HarvestPlantWeighing | null>(null);
  const [selectedScaleDevice, setSelectedScaleDevice] = useState<ScaleDevice | null>(
    scaleDevices.find(d => d.isActive && d.isCalibrationValid) || null
  );
  const [isLoading, setIsLoading] = useState(false);
  
  // PIN override modal state
  const [pinModalOpen, setPinModalOpen] = useState(false);
  const [pinModalError, setPinModalError] = useState<string | null>(null);
  const [adjustmentTarget, setAdjustmentTarget] = useState<{
    plantId: string | null;
    currentWeight: number;
    newWeight: number;
    weightType: string;
    plantTag?: string;
  } | null>(null);

  // Scale capture hook
  const scaleCapture = useScaleCapture({
    settings: {
      autoCaptureOnStable: true,
      audioEnabled: true,
    },
    onWeightCaptured: async (weight) => {
      if (weighingPlant) {
        await handleWeightCaptured(weighingPlant, weight);
      }
    },
    onStateChange: (newState) => {
      // Handle state changes if needed
    },
  });

  // Auto-select first pending plant
  useEffect(() => {
    if (!weighingPlant) {
      const firstPending = plants.find(p => p.wetWeight === 0);
      if (firstPending) {
        setWeighingPlant(firstPending);
        setSelectedPlant(firstPending);
      }
    }
  }, [plants, weighingPlant]);

  // Simulate scale readings (in real implementation, this would come from actual scale)
  useEffect(() => {
    if (!selectedScaleDevice) return;
    
    // Simulate periodic scale readings
    const interval = setInterval(() => {
      // This would be replaced with actual scale reading
      const simulatedWeight = Math.random() > 0.5 ? 250 + Math.random() * 100 : 0;
      const isStable = Math.random() > 0.3;
      scaleCapture.processScaleReading(simulatedWeight, isStable);
    }, 100);
    
    return () => clearInterval(interval);
  }, [selectedScaleDevice, scaleCapture]);

  // Handle weight captured
  const handleWeightCaptured = useCallback(async (plant: HarvestPlantWeighing, weight: number) => {
    setIsLoading(true);
    try {
      await onWeightCaptured?.(plant.plantId, weight);
      
      // Update local state
      setPlants(prev => prev.map(p => 
        p.id === plant.id 
          ? { ...p, wetWeight: weight, status: 'weighed' as const, weightSource: 'scale' }
          : p
      ));
      
      // Update metrics
      const totalWeight = plants.reduce((sum, p) => 
        p.id === plant.id ? sum + weight : sum + p.wetWeight, 0
      );
      const weighedCount = plants.filter(p => p.id === plant.id || p.wetWeight > 0).length;
      
      setMetrics(prev => ({
        ...prev,
        wetWeight: totalWeight,
        plantsWeighed: weighedCount,
        yieldPerPlant: weighedCount > 0 ? totalWeight / weighedCount : 0,
      }));
      
      // Move to next plant
      const currentIndex = plants.findIndex(p => p.id === plant.id);
      const nextPending = plants.slice(currentIndex + 1).find(p => p.wetWeight === 0);
      if (nextPending) {
        setWeighingPlant(nextPending);
        setSelectedPlant(nextPending);
      } else {
        setWeighingPlant(null);
      }
      
      scaleCapture.reset();
    } catch (error) {
      console.error('Failed to capture weight:', error);
    } finally {
      setIsLoading(false);
    }
  }, [plants, onWeightCaptured, scaleCapture]);

  // Handle plant selection
  const handleSelectPlant = useCallback((plant: HarvestPlantWeighing) => {
    setSelectedPlant(plant);
    if (plant.wetWeight === 0) {
      setWeighingPlant(plant);
      scaleCapture.reset();
    }
  }, [scaleCapture]);

  // Handle skip
  const handleSkip = useCallback(() => {
    if (weighingPlant) {
      const currentIndex = plants.findIndex(p => p.id === weighingPlant.id);
      const nextPending = plants.slice(currentIndex + 1).find(p => p.wetWeight === 0);
      if (nextPending) {
        setWeighingPlant(nextPending);
        setSelectedPlant(nextPending);
      }
      scaleCapture.skipPlant();
    }
  }, [weighingPlant, plants, scaleCapture]);

  // Handle weight adjustment request
  const handleAdjustmentRequest = useCallback((plant: HarvestPlantWeighing, newWeight: number) => {
    setAdjustmentTarget({
      plantId: plant.id,
      currentWeight: plant.wetWeight,
      newWeight,
      weightType: 'Wet Weight',
      plantTag: plant.plantTag,
    });
    setPinModalOpen(true);
    setPinModalError(null);
  }, []);

  // Handle PIN override submission
  const handlePinSubmit = useCallback(async (
    pin: string,
    reasonCode: WeightAdjustmentReasonCode,
    notes?: string
  ) => {
    if (!adjustmentTarget) return;
    
    try {
      await onAdjustWeight?.(
        adjustmentTarget.plantId,
        adjustmentTarget.newWeight,
        reasonCode,
        pin,
        notes
      );
      
      // Update local state
      if (adjustmentTarget.plantId) {
        setPlants(prev => prev.map(p =>
          p.id === adjustmentTarget.plantId
            ? { ...p, wetWeight: adjustmentTarget.newWeight }
            : p
        ));
      }
      
      setPinModalOpen(false);
      setAdjustmentTarget(null);
    } catch (error) {
      setPinModalError('Invalid PIN or adjustment failed');
    }
  }, [adjustmentTarget, onAdjustWeight]);

  // Calculate progress
  const weighedCount = plants.filter(p => p.wetWeight > 0).length;
  const totalCount = plants.length;
  const progressPercent = totalCount > 0 ? Math.round((weighedCount / totalCount) * 100) : 0;

  return (
    <div className="h-full flex flex-col bg-background">
      {/* Header */}
      <div className="flex items-center justify-between px-6 py-4 border-b border-border">
        <div>
          <h1 className="text-xl font-semibold">
            Harvest: {initialData?.harvestName || 'Blue Dream #BD-2024-042'}
          </h1>
          <p className="text-sm text-muted-foreground">
            {initialData?.strainName || 'Blue Dream'} • Wet Weight Capture
          </p>
        </div>
        
        {/* Progress */}
        <div className="flex items-center gap-4">
          <div className="text-right">
            <div className="text-sm text-muted-foreground">Progress</div>
            <div className="text-lg font-semibold">
              {weighedCount}/{totalCount} plants ({progressPercent}%)
            </div>
          </div>
          <div className="w-32 h-2 bg-muted rounded-full overflow-hidden">
            <div 
              className="h-full bg-emerald-500 transition-all"
              style={{ width: `${progressPercent}%` }}
            />
          </div>
        </div>
      </div>
      
      {/* Main content */}
      <div className="flex-1 flex overflow-hidden">
        {/* Left panel - Plant queue */}
        <div className="w-80 border-r border-border flex flex-col">
          <PlantQueueList
            plants={plants}
            selectedPlantId={selectedPlant?.id}
            weighingPlantId={weighingPlant?.id}
            onSelectPlant={handleSelectPlant}
            className="flex-1"
          />
        </div>
        
        {/* Center panel - Scale display */}
        <div className="flex-1 flex flex-col p-6">
          {/* Current plant info */}
          {weighingPlant && (
            <div className="mb-4 p-4 bg-card/50 rounded-lg border border-border/50">
              <div className="flex items-center justify-between">
                <div>
                  <div className="text-sm text-muted-foreground">Now Weighing</div>
                  <div className="text-2xl font-mono font-semibold">{weighingPlant.plantTag}</div>
                </div>
                <div className="text-right">
                  <div className="text-sm text-muted-foreground">Plant ID</div>
                  <div className="text-sm font-mono">{weighingPlant.plantId.slice(0, 8)}...</div>
                </div>
              </div>
            </div>
          )}
          
          {/* Scale display */}
          <ScaleLiveDisplay
            weight={scaleCapture.currentWeight}
            isStable={scaleCapture.isStable}
            captureState={scaleCapture.state}
            stabilityDurationMs={scaleCapture.stabilityDurationMs}
            scaleDevice={selectedScaleDevice}
            warningMessage={scaleCapture.warningMessage}
            errorMessage={scaleCapture.errorMessage}
            onCapture={scaleCapture.captureWeight}
            onTare={scaleCapture.tare}
            onSkip={handleSkip}
            captureDisabled={isLoading || !weighingPlant}
            className="flex-1 max-w-lg mx-auto"
          />
          
          {/* Keyboard shortcuts hint */}
          <div className="mt-4 text-center text-xs text-muted-foreground">
            <span className="px-2 py-1 bg-muted rounded">Enter</span> Capture •
            <span className="px-2 py-1 bg-muted rounded ml-2">T</span> Tare •
            <span className="px-2 py-1 bg-muted rounded ml-2">Tab</span> Skip •
            <span className="px-2 py-1 bg-muted rounded ml-2">Esc</span> Back
          </div>
        </div>
        
        {/* Right panel - Metrics and info */}
        <div className="w-80 border-l border-border p-4 space-y-4 overflow-y-auto">
          {/* Metrics card */}
          <HarvestMetricsCard
            metrics={metrics}
            showDetails={false}
          />
          
          {/* Scale selector */}
          <div className="bg-card/50 rounded-lg border border-border/50 p-4">
            <h3 className="text-sm font-medium mb-3">Scale Device</h3>
            <select
              value={selectedScaleDevice?.id || ''}
              onChange={(e) => {
                const device = scaleDevices.find(d => d.id === e.target.value);
                setSelectedScaleDevice(device || null);
              }}
              className="w-full px-3 py-2 bg-background border border-border rounded-md text-sm"
            >
              <option value="">Manual Entry</option>
              {scaleDevices.map(device => (
                <option 
                  key={device.id} 
                  value={device.id}
                  disabled={!device.isActive}
                >
                  {device.deviceName} 
                  {!device.isCalibrationValid && ' ⚠️'}
                </option>
              ))}
            </select>
          </div>
          
          {/* Session actions */}
          <div className="space-y-2">
            <button
              onClick={() => {
                // Lock all weights
              }}
              disabled={weighedCount === 0}
              className="w-full px-4 py-2 bg-amber-600 hover:bg-amber-500 text-white rounded-md text-sm font-medium transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Lock All Weights
            </button>
            <button
              onClick={onFinishSession}
              disabled={weighedCount < totalCount}
              className="w-full px-4 py-2 bg-emerald-600 hover:bg-emerald-500 text-white rounded-md text-sm font-medium transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Finish Weighing Session
            </button>
          </div>
        </div>
      </div>
      
      {/* PIN Override Modal */}
      <PinOverrideModal
        isOpen={pinModalOpen}
        onClose={() => {
          setPinModalOpen(false);
          setAdjustmentTarget(null);
        }}
        onSubmit={handlePinSubmit}
        currentWeight={adjustmentTarget?.currentWeight || 0}
        newWeight={adjustmentTarget?.newWeight || 0}
        weightType={adjustmentTarget?.weightType || 'Weight'}
        plantTag={adjustmentTarget?.plantTag}
        error={pinModalError}
      />
    </div>
  );
}

export default HarvestWeighingStation;





