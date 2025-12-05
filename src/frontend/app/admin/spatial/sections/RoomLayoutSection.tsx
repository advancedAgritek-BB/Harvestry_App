'use client';

import React, { useState, useCallback, useMemo } from 'react';
import {
  LayoutGrid,
  Save,
  RotateCcw,
  CheckCircle,
} from 'lucide-react';
import {
  AdminCard,
  AdminSection,
  Button,
  Select,
} from '@/components/admin';
import { RoomGridConfigurator } from '../components/RoomGridConfigurator';
import {
  RoomLayoutConfig,
  RoomWithLayout,
  ZoneOption,
  SensorOption,
  createDefaultLayoutConfig,
} from '../types/roomLayout.types';

// Mock data for rooms (will be replaced with API call)
const MOCK_ROOMS_WITH_LAYOUT: RoomWithLayout[] = [
  {
    id: '1',
    siteId: 's1',
    site: 'Evergreen',
    code: 'F1',
    name: 'Flower Room 1',
    type: 'Flower',
    sqft: 2500,
    zones: 4,
    status: 'active',
    layoutConfig: {
      gridRows: 2,
      gridCols: 2,
      tiers: 1,
      cellAssignments: {
        '1-0-0': { zoneId: '1', zoneName: 'Zone A', zoneCode: 'Z-A' },
        '1-0-1': { zoneId: '2', zoneName: 'Zone B', zoneCode: 'Z-B' },
        '1-1-0': { zoneId: '3', zoneName: 'Zone C', zoneCode: 'Z-C' },
        '1-1-1': { zoneId: '4', zoneName: 'Zone D', zoneCode: 'Z-D' },
      },
    },
  },
  {
    id: '2',
    siteId: 's1',
    site: 'Evergreen',
    code: 'F2',
    name: 'Flower Room 2',
    type: 'Flower',
    sqft: 2500,
    zones: 4,
    status: 'active',
  },
  {
    id: '3',
    siteId: 's1',
    site: 'Evergreen',
    code: 'V1',
    name: 'Veg Room 1',
    type: 'Veg',
    sqft: 1500,
    zones: 2,
    status: 'active',
    layoutConfig: {
      gridRows: 3,
      gridCols: 4,
      tiers: 2,
      cellAssignments: {
        '1-0-0': { zoneId: '5', zoneName: 'Zone A', zoneCode: 'Z-A' },
        '1-0-1': { zoneId: '5', zoneName: 'Zone A', zoneCode: 'Z-A' },
        '1-0-2': { zoneId: '6', zoneName: 'Zone B', zoneCode: 'Z-B' },
        '1-0-3': { zoneId: '6', zoneName: 'Zone B', zoneCode: 'Z-B' },
      },
    },
  },
  {
    id: '4',
    siteId: 's1',
    site: 'Evergreen',
    code: 'V2',
    name: 'Veg Room 2',
    type: 'Veg',
    sqft: 1500,
    zones: 2,
    status: 'active',
  },
  {
    id: '5',
    siteId: 's1',
    site: 'Evergreen',
    code: 'PROP',
    name: 'Propagation',
    type: 'Propagation',
    sqft: 800,
    zones: 1,
    status: 'active',
  },
  {
    id: '7',
    siteId: 's2',
    site: 'Oakdale',
    code: 'F1',
    name: 'Flower Room 1',
    type: 'Flower',
    sqft: 3000,
    zones: 6,
    status: 'active',
  },
];

// Mock zones (will be replaced with API call)
const MOCK_ZONES: ZoneOption[] = [
  { id: '1', code: 'Z-A', name: 'Zone A', roomId: '1' },
  { id: '2', code: 'Z-B', name: 'Zone B', roomId: '1' },
  { id: '3', code: 'Z-C', name: 'Zone C', roomId: '1' },
  { id: '4', code: 'Z-D', name: 'Zone D', roomId: '1' },
  { id: '5', code: 'Z-A', name: 'Zone A', roomId: '3' },
  { id: '6', code: 'Z-B', name: 'Zone B', roomId: '3' },
  { id: '7', code: 'Z-A', name: 'Zone A', roomId: '7' },
  { id: '8', code: 'Z-B', name: 'Zone B', roomId: '7' },
  { id: '9', code: 'Z-C', name: 'Zone C', roomId: '7' },
  { id: '10', code: 'Z-D', name: 'Zone D', roomId: '7' },
  { id: '11', code: 'Z-E', name: 'Zone E', roomId: '7' },
  { id: '12', code: 'Z-F', name: 'Zone F', roomId: '7' },
];

// Mock sensors (will be replaced with API call)
const MOCK_SENSORS: SensorOption[] = [
  { id: 's1', code: 'SENSOR-001', name: 'Temp/RH Sensor F1-A', type: 'Temp/RH', zoneId: '1' },
  { id: 's2', code: 'SENSOR-002', name: 'Temp/RH Sensor F1-B', type: 'Temp/RH', zoneId: '2' },
  { id: 's3', code: 'SENSOR-003', name: 'VWC Probe F1-A', type: 'VWC', zoneId: '1' },
  { id: 's4', code: 'SENSOR-004', name: 'VWC Probe F1-B', type: 'VWC', zoneId: '2' },
  { id: 's5', code: 'SENSOR-005', name: 'EC Sensor F1', type: 'EC' },
  { id: 's6', code: 'SENSOR-006', name: 'PPFD Sensor F1', type: 'PPFD' },
];

export function RoomLayoutSection() {
  const [selectedRoomId, setSelectedRoomId] = useState<string>('');
  const [layoutConfig, setLayoutConfig] = useState<RoomLayoutConfig | null>(null);
  const [originalConfig, setOriginalConfig] = useState<RoomLayoutConfig | null>(null);
  const [isSaving, setIsSaving] = useState(false);
  const [saveSuccess, setSaveSuccess] = useState(false);

  const selectedRoom = useMemo(
    () => MOCK_ROOMS_WITH_LAYOUT.find((r) => r.id === selectedRoomId),
    [selectedRoomId]
  );

  const availableZones = useMemo(
    () => MOCK_ZONES.filter((z) => z.roomId === selectedRoomId),
    [selectedRoomId]
  );

  const roomOptions = useMemo(
    () => [
      { value: '', label: 'Select a room...' },
      ...MOCK_ROOMS_WITH_LAYOUT.map((r) => ({
        value: r.id,
        label: `${r.site} / ${r.code} - ${r.name}`,
      })),
    ],
    []
  );

  const handleRoomSelect = useCallback((roomId: string) => {
    setSelectedRoomId(roomId);
    setSaveSuccess(false);
    
    if (roomId) {
      const room = MOCK_ROOMS_WITH_LAYOUT.find((r) => r.id === roomId);
      const config = room?.layoutConfig || createDefaultLayoutConfig();
      setLayoutConfig(config);
      setOriginalConfig(JSON.parse(JSON.stringify(config)));
    } else {
      setLayoutConfig(null);
      setOriginalConfig(null);
    }
  }, []);

  const handleLayoutChange = useCallback((config: RoomLayoutConfig) => {
    setLayoutConfig(config);
    setSaveSuccess(false);
  }, []);

  const handleSave = useCallback(async () => {
    if (!layoutConfig || !selectedRoomId) return;
    
    setIsSaving(true);
    setSaveSuccess(false);
    
    // Simulate API call
    await new Promise((resolve) => setTimeout(resolve, 800));
    
    // In real implementation, this would call an API to save the layout config
    console.log('Saving layout config for room:', selectedRoomId, layoutConfig);
    
    setOriginalConfig(JSON.parse(JSON.stringify(layoutConfig)));
    setIsSaving(false);
    setSaveSuccess(true);
    
    // Clear success message after 3 seconds
    setTimeout(() => setSaveSuccess(false), 3000);
  }, [layoutConfig, selectedRoomId]);

  const handleReset = useCallback(() => {
    if (originalConfig) {
      setLayoutConfig(JSON.parse(JSON.stringify(originalConfig)));
      setSaveSuccess(false);
    }
  }, [originalConfig]);

  const hasChanges = useMemo(
    () => JSON.stringify(layoutConfig) !== JSON.stringify(originalConfig),
    [layoutConfig, originalConfig]
  );

  return (
    <AdminSection
      title="Room Layouts"
      description="Configure the visual layout grid for each room to create a digital twin representation"
    >
      <AdminCard
        title="Room Layout Configuration"
        icon={LayoutGrid}
        actions={
          <div className="flex items-center gap-3">
            {saveSuccess && (
              <div className="flex items-center gap-1.5 text-emerald-400 text-sm">
                <CheckCircle className="w-4 h-4" />
                <span>Saved</span>
              </div>
            )}
            {selectedRoomId && (
              <>
                <Button
                  variant="ghost"
                  onClick={handleReset}
                  disabled={!hasChanges || isSaving}
                >
                  <RotateCcw className="w-4 h-4" />
                  Reset
                </Button>
                <Button
                  onClick={handleSave}
                  disabled={!hasChanges || isSaving}
                >
                  <Save className="w-4 h-4" />
                  {isSaving ? 'Saving...' : 'Save Layout'}
                </Button>
              </>
            )}
          </div>
        }
      >
        {/* Room Selector */}
        <div className="mb-6">
          <label className="block text-sm font-medium text-foreground mb-2">
            Select Room to Configure
          </label>
          <div className="max-w-md">
            <Select
              options={roomOptions}
              value={selectedRoomId}
              onChange={(e) => handleRoomSelect(e.target.value)}
            />
          </div>
        </div>

        {/* Room Info & Grid Configurator */}
        {selectedRoom && layoutConfig ? (
          <div className="space-y-6">
            {/* Room Info Banner */}
            <div className="flex items-center gap-6 p-4 bg-white/5 rounded-lg">
              <div>
                <div className="text-xs text-muted-foreground uppercase tracking-wider">Room</div>
                <div className="text-lg font-semibold text-foreground">{selectedRoom.name}</div>
              </div>
              <div className="w-px h-10 bg-border" />
              <div>
                <div className="text-xs text-muted-foreground uppercase tracking-wider">Type</div>
                <div className="text-sm text-foreground">{selectedRoom.type}</div>
              </div>
              <div className="w-px h-10 bg-border" />
              <div>
                <div className="text-xs text-muted-foreground uppercase tracking-wider">Size</div>
                <div className="text-sm text-foreground">{selectedRoom.sqft.toLocaleString()} sqft</div>
              </div>
              <div className="w-px h-10 bg-border" />
              <div>
                <div className="text-xs text-muted-foreground uppercase tracking-wider">Zones</div>
                <div className="text-sm text-foreground">{availableZones.length} available</div>
              </div>
              {!selectedRoom.layoutConfig && (
                <div className="ml-auto px-3 py-1.5 bg-amber-500/10 text-amber-400 rounded-lg text-xs font-medium">
                  New Layout
                </div>
              )}
            </div>

            {/* Grid Configurator */}
            <RoomGridConfigurator
              layoutConfig={layoutConfig}
              onLayoutChange={handleLayoutChange}
              availableZones={availableZones}
              availableSensors={MOCK_SENSORS}
              roomName={selectedRoom.name}
            />
          </div>
        ) : (
          <div className="text-center py-12 text-muted-foreground">
            <LayoutGrid className="w-12 h-12 mx-auto mb-4 opacity-30" />
            <p className="text-sm">Select a room above to configure its layout grid</p>
            <p className="text-xs mt-1 text-muted-foreground/70">
              The layout will be used to display environmental data as a digital twin heatmap
            </p>
          </div>
        )}
      </AdminCard>
    </AdminSection>
  );
}

export default RoomLayoutSection;

