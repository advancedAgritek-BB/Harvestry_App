'use client';

import React, { useState, useCallback, useRef } from 'react';
import { cn } from '@/lib/utils';
import { 
  Plus, 
  Minus, 
  X, 
  Layers,
  Grid3X3,
  MousePointer2,
  BoxSelect,
  Check,
} from 'lucide-react';
import { Button, FormField, Input, Select } from '@/components/admin';
import {
  RoomLayoutConfig,
  CellAssignment,
  ZoneOption,
  SensorOption,
  getCellKey,
  getZoneColor,
} from '../types/roomLayout.types';

interface RoomGridConfiguratorProps {
  layoutConfig: RoomLayoutConfig;
  onLayoutChange: (config: RoomLayoutConfig) => void;
  availableZones: ZoneOption[];
  availableSensors: SensorOption[];
  roomName: string;
  onCreateZone?: (zone: { code: string; name: string; cells: string[] }) => void;
}

interface CellModalProps {
  isOpen: boolean;
  onClose: () => void;
  cellKey: string;
  currentAssignment?: CellAssignment;
  availableZones: ZoneOption[];
  availableSensors: SensorOption[];
  onSave: (cellKey: string, assignment: CellAssignment | null) => void;
}

interface CreateZoneModalProps {
  isOpen: boolean;
  onClose: () => void;
  selectedCells: string[];
  onCreateZone: (zone: { code: string; name: string }) => void;
}

function CreateZoneModal({ isOpen, onClose, selectedCells, onCreateZone }: CreateZoneModalProps) {
  const [zoneCode, setZoneCode] = useState('');
  const [zoneName, setZoneName] = useState('');

  if (!isOpen) return null;

  const handleCreate = () => {
    if (zoneCode && zoneName) {
      onCreateZone({ code: zoneCode, name: zoneName });
      setZoneCode('');
      setZoneName('');
      onClose();
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="absolute inset-0 bg-black/60 backdrop-blur-sm" onClick={onClose} />
      <div className="relative bg-surface border border-border rounded-xl w-full max-w-md shadow-2xl">
        <div className="px-5 py-4 border-b border-border flex items-center justify-between">
          <div>
            <h3 className="text-sm font-semibold text-foreground">Create New Zone</h3>
            <p className="text-xs text-muted-foreground mt-0.5">
              {selectedCells.length} cell{selectedCells.length !== 1 ? 's' : ''} selected
            </p>
          </div>
          <button
            onClick={onClose}
            className="p-1.5 hover:bg-white/10 rounded-lg transition-colors"
          >
            <X className="w-4 h-4 text-muted-foreground" />
          </button>
        </div>

        <div className="p-5 space-y-4">
          <FormField label="Zone Code" required description="Short identifier (e.g., Z-A, BENCH-1)">
            <Input
              placeholder="e.g., Z-A"
              value={zoneCode}
              onChange={(e) => setZoneCode(e.target.value.toUpperCase())}
              maxLength={10}
            />
          </FormField>

          <FormField label="Zone Name" required description="Descriptive name for this zone">
            <Input
              placeholder="e.g., Zone A, North Bench"
              value={zoneName}
              onChange={(e) => setZoneName(e.target.value)}
            />
          </FormField>

          <div className="p-3 bg-cyan-500/10 border border-cyan-500/20 rounded-lg">
            <p className="text-xs text-cyan-300">
              This zone will be created and automatically assigned to the {selectedCells.length} selected cell{selectedCells.length !== 1 ? 's' : ''}.
            </p>
          </div>
        </div>

        <div className="px-5 py-4 border-t border-border flex items-center justify-end gap-2">
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button onClick={handleCreate} disabled={!zoneCode || !zoneName}>
            <Plus className="w-4 h-4" />
            Create Zone
          </Button>
        </div>
      </div>
    </div>
  );
}

function CellAssignmentModal({
  isOpen,
  onClose,
  cellKey,
  currentAssignment,
  availableZones,
  availableSensors,
  onSave,
}: CellModalProps) {
  const [selectedZoneId, setSelectedZoneId] = useState(currentAssignment?.zoneId || '');
  const [selectedSensorIds, setSelectedSensorIds] = useState<string[]>(
    currentAssignment?.sensorIds || []
  );
  const [customLabel, setCustomLabel] = useState(currentAssignment?.label || '');

  if (!isOpen) return null;

  const [tier, row, col] = cellKey.split('-').map(Number);

  const handleSave = () => {
    if (!selectedZoneId) {
      onSave(cellKey, null);
    } else {
      const selectedZone = availableZones.find((z) => z.id === selectedZoneId);
      onSave(cellKey, {
        zoneId: selectedZoneId,
        zoneName: selectedZone?.name,
        zoneCode: selectedZone?.code,
        sensorIds: selectedSensorIds.length > 0 ? selectedSensorIds : undefined,
        label: customLabel || undefined,
      });
    }
    onClose();
  };

  const handleClear = () => {
    onSave(cellKey, null);
    onClose();
  };

  const zoneOptions = [
    { value: '', label: 'Unassigned' },
    ...availableZones.map((z) => ({ value: z.id, label: `${z.code} - ${z.name}` })),
  ];

  const filteredSensors = selectedZoneId
    ? availableSensors.filter((s) => !s.zoneId || s.zoneId === selectedZoneId)
    : availableSensors;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="absolute inset-0 bg-black/60 backdrop-blur-sm" onClick={onClose} />
      <div className="relative bg-surface border border-border rounded-xl w-full max-w-md shadow-2xl">
        <div className="px-5 py-4 border-b border-border flex items-center justify-between">
          <div>
            <h3 className="text-sm font-semibold text-foreground">
              Assign Cell ({row + 1}, {col + 1})
              {tier > 1 && <span className="text-muted-foreground ml-1">Tier {tier}</span>}
            </h3>
            <p className="text-xs text-muted-foreground mt-0.5">
              Assign a zone and optional sensors to this grid position
            </p>
          </div>
          <button
            onClick={onClose}
            className="p-1.5 hover:bg-white/10 rounded-lg transition-colors"
          >
            <X className="w-4 h-4 text-muted-foreground" />
          </button>
        </div>

        <div className="p-5 space-y-4">
          <FormField label="Zone Assignment" required>
            <Select
              options={zoneOptions}
              value={selectedZoneId}
              onChange={(e) => setSelectedZoneId(e.target.value)}
            />
          </FormField>

          <FormField 
            label="Custom Label" 
            description="Optional display label (e.g., 'Bench A1')"
          >
            <Input
              placeholder="e.g., Bench A1"
              value={customLabel}
              onChange={(e) => setCustomLabel(e.target.value)}
            />
          </FormField>

          {selectedZoneId && filteredSensors.length > 0 && (
            <FormField 
              label="Override Sensors" 
              description="Optional: assign specific sensors to this cell"
            >
              <div className="space-y-2 max-h-32 overflow-y-auto">
                {filteredSensors.map((sensor) => (
                  <label
                    key={sensor.id}
                    className="flex items-center gap-2 p-2 rounded-lg hover:bg-white/5 cursor-pointer"
                  >
                    <input
                      type="checkbox"
                      checked={selectedSensorIds.includes(sensor.id)}
                      onChange={(e) => {
                        if (e.target.checked) {
                          setSelectedSensorIds([...selectedSensorIds, sensor.id]);
                        } else {
                          setSelectedSensorIds(selectedSensorIds.filter((id) => id !== sensor.id));
                        }
                      }}
                      className="rounded border-border"
                    />
                    <span className="text-sm text-foreground">{sensor.name}</span>
                    <span className="text-xs text-muted-foreground">({sensor.type})</span>
                  </label>
                ))}
              </div>
            </FormField>
          )}
        </div>

        <div className="px-5 py-4 border-t border-border flex items-center justify-between">
          <Button variant="ghost" onClick={handleClear} className="text-rose-400">
            Clear Assignment
          </Button>
          <div className="flex items-center gap-2">
            <Button variant="ghost" onClick={onClose}>Cancel</Button>
            <Button onClick={handleSave}>Save</Button>
          </div>
        </div>
      </div>
    </div>
  );
}

type SelectionMode = 'assign' | 'select';

export function RoomGridConfigurator({
  layoutConfig,
  onLayoutChange,
  availableZones,
  availableSensors,
  roomName,
  onCreateZone,
}: RoomGridConfiguratorProps) {
  const [activeTier, setActiveTier] = useState(1);
  const [selectedCell, setSelectedCell] = useState<string | null>(null);
  const [selectionMode, setSelectionMode] = useState<SelectionMode>('assign');
  const [selectedCells, setSelectedCells] = useState<Set<string>>(new Set());
  const [isSelecting, setIsSelecting] = useState(false);
  const [showCreateZoneModal, setShowCreateZoneModal] = useState(false);
  const startCellRef = useRef<string | null>(null);

  const { gridRows, gridCols, tiers, cellAssignments } = layoutConfig;

  const updateDimension = useCallback(
    (dimension: 'gridRows' | 'gridCols' | 'tiers', delta: number) => {
      const newValue = Math.max(1, Math.min(12, layoutConfig[dimension] + delta));
      if (newValue !== layoutConfig[dimension]) {
        onLayoutChange({
          ...layoutConfig,
          [dimension]: newValue,
        });
      }
    },
    [layoutConfig, onLayoutChange]
  );

  const handleCellClick = useCallback((cellKey: string) => {
    if (selectionMode === 'assign') {
      setSelectedCell(cellKey);
    }
  }, [selectionMode]);

  const handleCellMouseDown = useCallback((cellKey: string, e: React.MouseEvent) => {
    if (selectionMode === 'select') {
      e.preventDefault();
      setIsSelecting(true);
      startCellRef.current = cellKey;
      
      if (e.shiftKey && selectedCells.size > 0) {
        // Shift-click: extend selection
        setSelectedCells(prev => new Set([...prev, cellKey]));
      } else if (e.ctrlKey || e.metaKey) {
        // Ctrl/Cmd-click: toggle selection
        setSelectedCells(prev => {
          const next = new Set(prev);
          if (next.has(cellKey)) {
            next.delete(cellKey);
          } else {
            next.add(cellKey);
          }
          return next;
        });
      } else {
        // Normal click: start new selection
        setSelectedCells(new Set([cellKey]));
      }
    }
  }, [selectionMode, selectedCells]);

  const handleCellMouseEnter = useCallback((cellKey: string) => {
    if (selectionMode === 'select' && isSelecting && startCellRef.current) {
      // Calculate rectangle selection
      const [, startRow, startCol] = startCellRef.current.split('-').map(Number);
      const [tier, endRow, endCol] = cellKey.split('-').map(Number);
      
      const minRow = Math.min(startRow, endRow);
      const maxRow = Math.max(startRow, endRow);
      const minCol = Math.min(startCol, endCol);
      const maxCol = Math.max(startCol, endCol);
      
      const newSelection = new Set<string>();
      for (let r = minRow; r <= maxRow; r++) {
        for (let c = minCol; c <= maxCol; c++) {
          newSelection.add(getCellKey(tier, r, c));
        }
      }
      setSelectedCells(newSelection);
    }
  }, [selectionMode, isSelecting]);

  const handleMouseUp = useCallback(() => {
    setIsSelecting(false);
    startCellRef.current = null;
  }, []);

  const handleCellSave = useCallback(
    (cellKey: string, assignment: CellAssignment | null) => {
      const newAssignments = { ...cellAssignments };
      if (assignment) {
        newAssignments[cellKey] = assignment;
      } else {
        delete newAssignments[cellKey];
      }
      onLayoutChange({
        ...layoutConfig,
        cellAssignments: newAssignments,
      });
    },
    [layoutConfig, cellAssignments, onLayoutChange]
  );

  const handleCreateZone = useCallback((zone: { code: string; name: string }) => {
    // Generate a temporary ID for the new zone
    const newZoneId = `new-${Date.now()}`;
    
    // Update all selected cells with the new zone
    const newAssignments = { ...cellAssignments };
    selectedCells.forEach(cellKey => {
      newAssignments[cellKey] = {
        zoneId: newZoneId,
        zoneName: zone.name,
        zoneCode: zone.code,
      };
    });
    
    onLayoutChange({
      ...layoutConfig,
      cellAssignments: newAssignments,
    });
    
    // Call the onCreateZone callback if provided
    if (onCreateZone) {
      onCreateZone({
        ...zone,
        cells: Array.from(selectedCells),
      });
    }
    
    // Clear selection
    setSelectedCells(new Set());
    setSelectionMode('assign');
  }, [cellAssignments, layoutConfig, onLayoutChange, onCreateZone, selectedCells]);

  const handleAssignZoneToSelected = useCallback((zoneId: string) => {
    const zone = availableZones.find(z => z.id === zoneId);
    if (!zone) return;
    
    const newAssignments = { ...cellAssignments };
    selectedCells.forEach(cellKey => {
      newAssignments[cellKey] = {
        zoneId: zone.id,
        zoneName: zone.name,
        zoneCode: zone.code,
      };
    });
    
    onLayoutChange({
      ...layoutConfig,
      cellAssignments: newAssignments,
    });
    
    setSelectedCells(new Set());
    setSelectionMode('assign');
  }, [availableZones, cellAssignments, layoutConfig, onLayoutChange, selectedCells]);

  // Build zone color map for consistent coloring
  const zoneColorMap = new Map<string, ReturnType<typeof getZoneColor>>();
  availableZones.forEach((zone, index) => {
    zoneColorMap.set(zone.id, getZoneColor(zone.id, index));
  });

  // Generate grid cells for current tier
  const renderGrid = () => {
    const cells: React.ReactNode[] = [];
    
    for (let row = 0; row < gridRows; row++) {
      for (let col = 0; col < gridCols; col++) {
        const cellKey = getCellKey(activeTier, row, col);
        const assignment = cellAssignments[cellKey];
        const zoneColor = assignment?.zoneId ? zoneColorMap.get(assignment.zoneId) : null;
        const isSelected = selectedCells.has(cellKey);

        cells.push(
          <button
            key={cellKey}
            onClick={() => handleCellClick(cellKey)}
            onMouseDown={(e) => handleCellMouseDown(cellKey, e)}
            onMouseEnter={() => handleCellMouseEnter(cellKey)}
            className={cn(
              'aspect-square rounded-lg border-2 transition-all select-none',
              'flex flex-col items-center justify-center gap-1 p-2',
              'hover:scale-105 hover:shadow-lg hover:z-10',
              selectionMode === 'select' && 'cursor-crosshair',
              isSelected && 'ring-2 ring-cyan-400 ring-offset-2 ring-offset-surface',
              assignment?.zoneId
                ? cn(zoneColor?.bg, zoneColor?.border, 'border-solid')
                : 'border-dashed border-border/50 hover:border-border bg-white/5'
            )}
          >
            {assignment ? (
              <>
                <span className={cn('text-xs font-medium', zoneColor?.text || 'text-foreground')}>
                  {assignment.label || assignment.zoneCode || 'Assigned'}
                </span>
                {assignment.zoneName && !assignment.label && (
                  <span className="text-[10px] text-muted-foreground truncate w-full text-center">
                    {assignment.zoneName}
                  </span>
                )}
              </>
            ) : (
              <Plus className="w-4 h-4 text-muted-foreground/50" />
            )}
            {isSelected && (
              <div className="absolute top-1 right-1">
                <Check className="w-3 h-3 text-cyan-400" />
              </div>
            )}
          </button>
        );
      }
    }
    
    return cells;
  };

  const assignedCount = Object.keys(cellAssignments).filter(
    (key) => key.startsWith(`${activeTier}-`)
  ).length;
  const totalCells = gridRows * gridCols;

  return (
    <div className="space-y-6" onMouseUp={handleMouseUp} onMouseLeave={handleMouseUp}>
      {/* Dimension Controls */}
      <div className="flex flex-wrap items-center gap-6">
        {/* Rows Control */}
        <div className="flex items-center gap-3">
          <div className="flex items-center gap-2 text-sm text-muted-foreground">
            <Grid3X3 className="w-4 h-4" />
            <span>Rows</span>
          </div>
          <div className="flex items-center gap-1">
            <button
              onClick={() => updateDimension('gridRows', -1)}
              disabled={gridRows <= 1}
              className="w-7 h-7 rounded-lg bg-white/5 hover:bg-white/10 disabled:opacity-50 flex items-center justify-center"
            >
              <Minus className="w-3 h-3" />
            </button>
            <span className="w-8 text-center font-mono text-sm">{gridRows}</span>
            <button
              onClick={() => updateDimension('gridRows', 1)}
              disabled={gridRows >= 12}
              className="w-7 h-7 rounded-lg bg-white/5 hover:bg-white/10 disabled:opacity-50 flex items-center justify-center"
            >
              <Plus className="w-3 h-3" />
            </button>
          </div>
        </div>

        {/* Columns Control */}
        <div className="flex items-center gap-3">
          <span className="text-sm text-muted-foreground">Columns</span>
          <div className="flex items-center gap-1">
            <button
              onClick={() => updateDimension('gridCols', -1)}
              disabled={gridCols <= 1}
              className="w-7 h-7 rounded-lg bg-white/5 hover:bg-white/10 disabled:opacity-50 flex items-center justify-center"
            >
              <Minus className="w-3 h-3" />
            </button>
            <span className="w-8 text-center font-mono text-sm">{gridCols}</span>
            <button
              onClick={() => updateDimension('gridCols', 1)}
              disabled={gridCols >= 12}
              className="w-7 h-7 rounded-lg bg-white/5 hover:bg-white/10 disabled:opacity-50 flex items-center justify-center"
            >
              <Plus className="w-3 h-3" />
            </button>
          </div>
        </div>

        {/* Tiers Control */}
        <div className="flex items-center gap-3">
          <div className="flex items-center gap-2 text-sm text-muted-foreground">
            <Layers className="w-4 h-4" />
            <span>Tiers</span>
          </div>
          <div className="flex items-center gap-1">
            <button
              onClick={() => updateDimension('tiers', -1)}
              disabled={tiers <= 1}
              className="w-7 h-7 rounded-lg bg-white/5 hover:bg-white/10 disabled:opacity-50 flex items-center justify-center"
            >
              <Minus className="w-3 h-3" />
            </button>
            <span className="w-8 text-center font-mono text-sm">{tiers}</span>
            <button
              onClick={() => updateDimension('tiers', 1)}
              disabled={tiers >= 5}
              className="w-7 h-7 rounded-lg bg-white/5 hover:bg-white/10 disabled:opacity-50 flex items-center justify-center"
            >
              <Plus className="w-3 h-3" />
            </button>
          </div>
        </div>

        {/* Stats */}
        <div className="ml-auto text-sm text-muted-foreground">
          {assignedCount} / {totalCells} cells assigned
        </div>
      </div>

      {/* Mode Toggle & Actions */}
      <div className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
        <div className="flex items-center gap-2">
          <span className="text-xs text-muted-foreground mr-2">Mode:</span>
          <button
            onClick={() => {
              setSelectionMode('assign');
              setSelectedCells(new Set());
            }}
            className={cn(
              'flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-medium transition-colors',
              selectionMode === 'assign'
                ? 'bg-violet-500/20 text-violet-300 ring-1 ring-violet-500/50'
                : 'bg-white/5 text-muted-foreground hover:bg-white/10'
            )}
          >
            <MousePointer2 className="w-3.5 h-3.5" />
            Assign
          </button>
          <button
            onClick={() => setSelectionMode('select')}
            className={cn(
              'flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-medium transition-colors',
              selectionMode === 'select'
                ? 'bg-cyan-500/20 text-cyan-300 ring-1 ring-cyan-500/50'
                : 'bg-white/5 text-muted-foreground hover:bg-white/10'
            )}
          >
            <BoxSelect className="w-3.5 h-3.5" />
            Select Cells
          </button>
        </div>

        {/* Selection Actions */}
        {selectionMode === 'select' && selectedCells.size > 0 && (
          <div className="flex items-center gap-2">
            <span className="text-xs text-cyan-400 mr-2">
              {selectedCells.size} selected
            </span>
            <Button
              size="sm"
              variant="secondary"
              onClick={() => setShowCreateZoneModal(true)}
            >
              <Plus className="w-3.5 h-3.5" />
              Create Zone
            </Button>
            {availableZones.length > 0 && (
              <select
                className="h-8 px-2 bg-muted border border-border rounded-lg text-xs text-foreground"
                onChange={(e) => {
                  if (e.target.value) {
                    handleAssignZoneToSelected(e.target.value);
                  }
                }}
                value=""
              >
                <option value="">Assign to Zone...</option>
                {availableZones.map(zone => (
                  <option key={zone.id} value={zone.id}>
                    {zone.code} - {zone.name}
                  </option>
                ))}
              </select>
            )}
            <Button
              size="sm"
              variant="ghost"
              onClick={() => setSelectedCells(new Set())}
            >
              Clear
            </Button>
          </div>
        )}
      </div>

      {/* Selection Help Text */}
      {selectionMode === 'select' && (
        <p className="text-xs text-muted-foreground">
          Click and drag to select multiple cells. Hold <kbd className="px-1.5 py-0.5 bg-white/10 rounded text-[10px]">Shift</kbd> to extend selection, 
          or <kbd className="px-1.5 py-0.5 bg-white/10 rounded text-[10px]">Ctrl/⌘</kbd> to toggle individual cells.
        </p>
      )}

      {/* Tier Selector (if multiple tiers) */}
      {tiers > 1 && (
        <div className="flex items-center gap-2">
          <span className="text-sm text-muted-foreground">Viewing Tier:</span>
          <div className="flex items-center gap-1">
            {Array.from({ length: tiers }, (_, i) => i + 1).map((tier) => (
              <button
                key={tier}
                onClick={() => setActiveTier(tier)}
                className={cn(
                  'px-3 py-1.5 rounded-lg text-sm font-medium transition-colors',
                  activeTier === tier
                    ? 'bg-cyan-500/20 text-cyan-300 ring-1 ring-cyan-500/50'
                    : 'bg-white/5 text-muted-foreground hover:bg-white/10'
                )}
              >
                Level {tier}
              </button>
            ))}
          </div>
        </div>
      )}

      {/* Grid Preview */}
      <div className="p-4 bg-black/20 rounded-xl border border-border">
        <div className="mb-3 flex items-center justify-between">
          <span className="text-xs font-medium text-muted-foreground uppercase tracking-wider">
            {roomName} - Grid Layout {tiers > 1 && `(Tier ${activeTier})`}
          </span>
          <span className="text-xs text-muted-foreground">
            {selectionMode === 'assign' ? 'Click a cell to assign a zone' : 'Click and drag to select cells'}
          </span>
        </div>
        
        <div
          className="grid gap-2"
          style={{
            gridTemplateColumns: `repeat(${gridCols}, minmax(60px, 1fr))`,
          }}
        >
          {renderGrid()}
        </div>
      </div>

      {/* Zone Legend */}
      {availableZones.length > 0 && (
        <div className="flex flex-wrap items-center gap-3">
          <span className="text-xs text-muted-foreground">Available Zones:</span>
          {availableZones.map((zone, index) => {
            const color = getZoneColor(zone.id, index);
            return (
              <div
                key={zone.id}
                className={cn(
                  'px-2 py-1 rounded text-xs font-medium',
                  color.bg,
                  color.text
                )}
              >
                {zone.code}
              </div>
            );
          })}
        </div>
      )}

      {/* Cell Assignment Modal */}
      {selectedCell && (
        <CellAssignmentModal
          isOpen={!!selectedCell}
          onClose={() => setSelectedCell(null)}
          cellKey={selectedCell}
          currentAssignment={cellAssignments[selectedCell]}
          availableZones={availableZones}
          availableSensors={availableSensors}
          onSave={handleCellSave}
        />
      )}

      {/* Create Zone Modal */}
      <CreateZoneModal
        isOpen={showCreateZoneModal}
        onClose={() => setShowCreateZoneModal(false)}
        selectedCells={Array.from(selectedCells)}
        onCreateZone={handleCreateZone}
      />
    </div>
  );
}

export default RoomGridConfigurator;
