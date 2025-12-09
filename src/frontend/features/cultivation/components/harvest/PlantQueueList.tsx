'use client';

/**
 * PlantQueueList Component
 * Displays list of plants to weigh with status indicators
 */

import { useRef, useEffect } from 'react';
import { cn } from '@/lib/utils';
import type { HarvestPlantWeighing } from '@/features/inventory/types';

interface PlantQueueListProps {
  /** List of plants */
  plants: HarvestPlantWeighing[];
  /** Currently selected plant ID */
  selectedPlantId?: string;
  /** Plant currently being weighed */
  weighingPlantId?: string;
  /** Callback when plant is selected */
  onSelectPlant?: (plant: HarvestPlantWeighing) => void;
  /** Unit of measurement */
  uom?: string;
  /** Additional class names */
  className?: string;
}

export function PlantQueueList({
  plants,
  selectedPlantId,
  weighingPlantId,
  onSelectPlant,
  uom = 'g',
  className,
}: PlantQueueListProps) {
  const listRef = useRef<HTMLDivElement>(null);
  const selectedRef = useRef<HTMLButtonElement>(null);

  // Scroll to selected/weighing plant
  useEffect(() => {
    if (selectedRef.current) {
      selectedRef.current.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }
  }, [selectedPlantId, weighingPlantId]);

  // Group plants by status
  const weighedPlants = plants.filter(p => p.wetWeight > 0);
  const pendingPlants = plants.filter(p => p.wetWeight === 0);
  
  const getStatusIcon = (plant: HarvestPlantWeighing) => {
    if (plant.id === weighingPlantId) return '→';
    if (plant.wetWeight > 0) return '✓';
    if (plant.status === 'error') return '✗';
    return '○';
  };

  const getStatusColor = (plant: HarvestPlantWeighing) => {
    if (plant.id === weighingPlantId) return 'text-cyan-400';
    if (plant.wetWeight > 0) return 'text-emerald-400';
    if (plant.status === 'error') return 'text-rose-400';
    return 'text-muted-foreground';
  };

  const renderPlantItem = (plant: HarvestPlantWeighing) => {
    const isSelected = plant.id === selectedPlantId;
    const isWeighing = plant.id === weighingPlantId;
    const hasWeight = plant.wetWeight > 0;

    return (
      <button
        key={plant.id}
        ref={isSelected || isWeighing ? selectedRef : undefined}
        onClick={() => onSelectPlant?.(plant)}
        className={cn(
          'w-full flex items-center gap-3 px-3 py-2 rounded-md text-left transition-colors',
          'hover:bg-muted/50',
          isSelected && 'bg-primary/10 border border-primary/20',
          isWeighing && 'bg-cyan-500/10 border border-cyan-500/20',
          !isSelected && !isWeighing && 'border border-transparent'
        )}
      >
        {/* Status icon */}
        <span className={cn('w-4 text-center font-mono', getStatusColor(plant))}>
          {getStatusIcon(plant)}
        </span>
        
        {/* Plant tag */}
        <span className={cn(
          'flex-1 font-mono text-sm',
          hasWeight ? 'text-foreground' : 'text-muted-foreground'
        )}>
          {plant.plantTag}
        </span>
        
        {/* Weight or status */}
        <span className={cn(
          'text-sm font-mono tabular-nums',
          hasWeight ? 'text-foreground' : 'text-muted-foreground'
        )}>
          {hasWeight ? `${plant.wetWeight.toFixed(1)}${uom}` : '---'}
        </span>
        
        {/* Lock indicator */}
        {plant.isWeightLocked && (
          <span className="text-amber-400 text-xs" title="Weight locked">
            🔒
          </span>
        )}
      </button>
    );
  };

  return (
    <div className={cn('flex flex-col h-full', className)}>
      {/* Header */}
      <div className="flex items-center justify-between px-3 py-2 border-b border-border/50">
        <h3 className="text-sm font-medium">Plant Queue</h3>
        <span className="text-xs text-muted-foreground">
          {weighedPlants.length}/{plants.length} weighed
        </span>
      </div>
      
      {/* Plant list */}
      <div 
        ref={listRef}
        className="flex-1 overflow-y-auto divide-y divide-border/30"
      >
        {/* Pending plants first */}
        {pendingPlants.length > 0 && (
          <div className="p-1">
            <div className="px-2 py-1 text-xs text-muted-foreground font-medium">
              Pending ({pendingPlants.length})
            </div>
            {pendingPlants.map(renderPlantItem)}
          </div>
        )}
        
        {/* Weighed plants */}
        {weighedPlants.length > 0 && (
          <div className="p-1">
            <div className="px-2 py-1 text-xs text-muted-foreground font-medium">
              Completed ({weighedPlants.length})
            </div>
            {weighedPlants.map(renderPlantItem)}
          </div>
        )}
        
        {/* Empty state */}
        {plants.length === 0 && (
          <div className="flex flex-col items-center justify-center h-full text-muted-foreground">
            <span className="text-2xl mb-2">🌱</span>
            <span className="text-sm">No plants in harvest</span>
          </div>
        )}
      </div>
      
      {/* Footer with totals */}
      <div className="border-t border-border/50 px-3 py-2 bg-muted/30">
        <div className="flex justify-between text-xs">
          <span className="text-muted-foreground">Total Weight:</span>
          <span className="font-mono font-medium">
            {weighedPlants.reduce((sum, p) => sum + p.wetWeight, 0).toFixed(1)}{uom}
          </span>
        </div>
        <div className="flex justify-between text-xs mt-1">
          <span className="text-muted-foreground">Avg per Plant:</span>
          <span className="font-mono">
            {weighedPlants.length > 0
              ? (weighedPlants.reduce((sum, p) => sum + p.wetWeight, 0) / weighedPlants.length).toFixed(1)
              : '---'
            }{weighedPlants.length > 0 ? uom : ''}
          </span>
        </div>
      </div>
    </div>
  );
}

export default PlantQueueList;
