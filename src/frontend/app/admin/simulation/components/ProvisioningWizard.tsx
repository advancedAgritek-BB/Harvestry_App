'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { 
  Building2, 
  DoorOpen, 
  Grid3X3, 
  Cpu, 
  Radio, 
  ChevronRight, 
  Check,
  Plus,
  Sparkles
} from 'lucide-react';
import { 
  provisioningService, 
  Site, 
  Room, 
  Zone, 
  Equipment,
  CoreEquipmentType,
  EquipmentTypeLabels,
  EquipmentTypeGroups,
  SensorStream
} from '@/features/telemetry/services/provisioning.service';
import {
  StreamType,
  StreamTypeLabels,
  StreamTypeGroups,
  Unit,
  UnitLabels,
  StreamTypeUnits,
  getDefaultUnit
} from '@/features/telemetry/services/simulation.service';

interface ProvisioningWizardProps {
  onStreamCreated?: (stream: SensorStream) => void;
}

type Step = 'site' | 'room' | 'zone' | 'equipment' | 'sensor';

const STEPS: { id: Step; label: string; icon: React.ElementType; description: string }[] = [
  { id: 'site', label: 'Site', icon: Building2, description: 'Create facility' },
  { id: 'room', label: 'Room', icon: DoorOpen, description: 'Add grow room' },
  { id: 'zone', label: 'Zone', icon: Grid3X3, description: 'Define zone' },
  { id: 'equipment', label: 'Equipment', icon: Cpu, description: 'Register device' },
  { id: 'sensor', label: 'Stream', icon: Radio, description: 'Create stream' },
];

// Inline Tailwind class names (styled-jsx with @apply doesn't work in Next.js without extra config)
const inputFieldClass = 'w-full px-3 py-2.5 rounded-lg border border-border bg-background/50 text-sm focus:outline-none focus:ring-2 focus:ring-cyan-500/30 focus:border-cyan-500/50 transition-all';
const btnPrimaryClass = 'w-full flex items-center justify-center gap-2 px-4 py-2.5 bg-gradient-to-r from-cyan-500 to-teal-500 text-white rounded-lg text-sm font-medium hover:from-cyan-600 hover:to-teal-600 disabled:opacity-50 disabled:cursor-not-allowed transition-all shadow-lg shadow-cyan-500/20';

export default function ProvisioningWizard({ onStreamCreated }: ProvisioningWizardProps) {
  // Data cache
  const [sites, setSites] = useState<Site[]>([]);
  const [rooms, setRooms] = useState<Room[]>([]);
  const [zones, setZones] = useState<Zone[]>([]);
  const [equipment, setEquipment] = useState<Equipment[]>([]);
  
  // Wizard state
  const [currentStep, setCurrentStep] = useState<Step>('site');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  // Selected context
  const [selectedSiteId, setSelectedSiteId] = useState('');
  const [selectedRoomId, setSelectedRoomId] = useState('');

  // Form states
  const [siteName, setSiteName] = useState('');
  const [roomName, setRoomName] = useState('');
  const [zoneName, setZoneName] = useState('');
  const [equipmentCode, setEquipmentCode] = useState('');
  const [equipmentType, setEquipmentType] = useState<CoreEquipmentType | ''>('');
  const [streamName, setStreamName] = useState('');
  const [streamType, setStreamType] = useState<StreamType>(StreamType.Temperature);
  const [streamUnit, setStreamUnit] = useState<Unit>(Unit.DegreesFahrenheit);
  const [selectedEquipmentId, setSelectedEquipmentId] = useState('');
  const [selectedZoneId, setSelectedZoneId] = useState('');

  // Fetch data
  const fetchData = useCallback(async () => {
    try {
      const [s, r] = await Promise.all([
        provisioningService.getSites(),
        provisioningService.getRooms()
      ]);
      setSites(s);
      setRooms(r);
    } catch (err) { 
      console.error('Error fetching data:', err); 
    }
  }, []);

  const fetchSiteData = useCallback(async (siteId: string) => {
    if (!siteId) {
      setZones([]);
      setEquipment([]);
      return;
    }
    try {
      const [z, e] = await Promise.all([
        provisioningService.getZones(siteId),
        provisioningService.getEquipment(siteId)
      ]);
      setZones(z);
      setEquipment(e);
    } catch (err) {
      console.error('Error fetching site data:', err);
    }
  }, []);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  useEffect(() => {
    if (selectedSiteId) {
      fetchSiteData(selectedSiteId);
    }
  }, [selectedSiteId, fetchSiteData]);

  // Handlers
  const showMessage = (type: 'success' | 'error', text: string) => {
    setMessage({ type, text });
    setTimeout(() => setMessage(null), 3000);
  };

  const handleCreateSite = async () => {
    if (!siteName.trim()) return;
    setIsSubmitting(true);
    try {
      const site = await provisioningService.createSite({ name: siteName });
      showMessage('success', `Created site: ${site.name}`);
      setSiteName('');
      setSelectedSiteId(site.id);
      await fetchData();
      setCurrentStep('room');
    } catch (err) {
      showMessage('error', 'Failed to create site');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCreateRoom = async () => {
    if (!selectedSiteId || !roomName.trim()) return;
    setIsSubmitting(true);
    try {
      const room = await provisioningService.createRoom({
        siteId: selectedSiteId,
        name: roomName,
        type: 'Indoor'
      });
      showMessage('success', `Created room: ${room.name}`);
      setRoomName('');
      setSelectedRoomId(room.id);
      await fetchData();
      setCurrentStep('zone');
    } catch (err) {
      showMessage('error', 'Failed to create room');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCreateZone = async () => {
    if (!selectedSiteId || !selectedRoomId || !zoneName.trim()) return;
    setIsSubmitting(true);
    try {
      const zone = await provisioningService.createZone({
        siteId: selectedSiteId,
        roomId: selectedRoomId,
        name: zoneName
      });
      showMessage('success', `Created zone: ${zone.name}`);
      setZoneName('');
      setSelectedZoneId(zone.id);
      await fetchSiteData(selectedSiteId);
      setCurrentStep('equipment');
    } catch (err) {
      showMessage('error', 'Failed to create zone');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCreateEquipment = async () => {
    if (!selectedSiteId || !selectedZoneId || !equipmentType || !equipmentCode.trim()) return;
    setIsSubmitting(true);
    try {
      const equip = await provisioningService.createEquipment({
        siteId: selectedSiteId,
        locationId: selectedZoneId,
        code: equipmentCode,
        typeCode: equipmentType,
        coreType: equipmentType as CoreEquipmentType
      });
      showMessage('success', `Created equipment: ${equip.code}`);
      setEquipmentCode('');
      setEquipmentType('');
      setSelectedEquipmentId(equip.id);
      await fetchSiteData(selectedSiteId);
      setCurrentStep('sensor');
    } catch (err) {
      showMessage('error', 'Failed to create equipment');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCreateStream = async () => {
    if (!selectedSiteId || !selectedEquipmentId || !streamName.trim()) return;
    setIsSubmitting(true);
    try {
      const stream = await provisioningService.createSensorStream({
        siteId: selectedSiteId,
        equipmentId: selectedEquipmentId,
        displayName: streamName,
        streamType: streamType,
        unit: streamUnit,
        zoneId: selectedZoneId || undefined
      });
      showMessage('success', `Created stream: ${stream.displayName}`);
      setStreamName('');
      onStreamCreated?.(stream);
    } catch (err) {
      showMessage('error', 'Failed to create stream');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleStreamTypeChange = (type: StreamType) => {
    setStreamType(type);
    setStreamUnit(getDefaultUnit(type));
  };

  const currentStepIndex = STEPS.findIndex(s => s.id === currentStep);
  const siteRooms = rooms.filter(r => r.siteId === selectedSiteId);

  return (
    <div className="rounded-2xl border border-border bg-gradient-to-br from-card to-card/50 overflow-hidden">
      {/* Header */}
      <div className="p-5 border-b border-border bg-gradient-to-r from-cyan-500/5 to-teal-500/5">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="p-2.5 rounded-xl bg-gradient-to-br from-cyan-500/20 to-teal-500/20 ring-1 ring-cyan-500/30">
              <Sparkles className="w-5 h-5 text-cyan-400" />
            </div>
            <div>
              <h2 className="font-semibold">Quick Setup</h2>
              <p className="text-xs text-muted-foreground">Create test data in seconds</p>
            </div>
          </div>
          {message && (
            <div className={`px-3 py-1.5 rounded-lg text-xs font-medium ${
              message.type === 'success' 
                ? 'bg-green-500/10 text-green-400 border border-green-500/20' 
                : 'bg-red-500/10 text-red-400 border border-red-500/20'
            }`}>
              {message.text}
            </div>
          )}
        </div>
      </div>

      {/* Step Indicators */}
      <div className="flex items-center justify-between px-5 py-3 bg-muted/20 border-b border-border overflow-x-auto">
        {STEPS.map((step, index) => {
          const isActive = currentStep === step.id;
          const isCompleted = index < currentStepIndex;
          const Icon = step.icon;
          
          return (
            <React.Fragment key={step.id}>
              <button
                onClick={() => setCurrentStep(step.id)}
                className={`flex items-center gap-2 px-3 py-1.5 rounded-lg transition-all whitespace-nowrap ${
                  isActive 
                    ? 'bg-cyan-500/10 text-cyan-400 ring-1 ring-cyan-500/30' 
                    : isCompleted
                      ? 'text-green-400 hover:bg-green-500/10'
                      : 'text-muted-foreground hover:bg-muted/50'
                }`}
              >
                {isCompleted ? (
                  <Check className="w-4 h-4" />
                ) : (
                  <Icon className="w-4 h-4" />
                )}
                <span className="text-xs font-medium hidden sm:inline">{step.label}</span>
              </button>
              {index < STEPS.length - 1 && (
                <ChevronRight className="w-4 h-4 text-muted-foreground/30 flex-shrink-0 mx-1" />
              )}
            </React.Fragment>
          );
        })}
      </div>

      {/* Step Content */}
      <div className="p-5">
        {/* Site Step */}
        {currentStep === 'site' && (
          <StepContent
            title="Create or Select Site"
            description="A site represents a physical facility or location"
          >
            <div className="space-y-4">
              {sites.length > 0 && (
                <div>
                  <label className="text-xs font-medium text-muted-foreground mb-2 block">Use Existing</label>
                  <select 
                    className={inputFieldClass}
                    value={selectedSiteId}
                    onChange={e => {
                      setSelectedSiteId(e.target.value);
                      if (e.target.value) setCurrentStep('room');
                    }}
                  >
                    <option value="">-- Select existing site --</option>
                    {sites.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
                  </select>
                </div>
              )}
              <div className="relative">
                {sites.length > 0 && (
                  <div className="absolute inset-x-0 top-0 flex items-center">
                    <div className="flex-1 border-t border-border" />
                    <span className="px-3 text-xs text-muted-foreground bg-card">or create new</span>
                    <div className="flex-1 border-t border-border" />
                  </div>
                )}
                <div className={sites.length > 0 ? 'pt-6' : ''}>
                  <input 
                    className={inputFieldClass}
                    value={siteName}
                    onChange={e => setSiteName(e.target.value)}
                    placeholder="Enter new site name..."
                    onKeyDown={e => e.key === 'Enter' && handleCreateSite()}
                  />
                </div>
              </div>
              <button 
                onClick={handleCreateSite} 
                disabled={isSubmitting || !siteName.trim()} 
                className={btnPrimaryClass}
              >
                <Plus className="w-4 h-4" /> Create Site
              </button>
            </div>
          </StepContent>
        )}

        {/* Room Step */}
        {currentStep === 'room' && (
          <StepContent
            title="Add a Room"
            description="Rooms are spaces within your site (e.g., Grow Room, Veg Room)"
          >
            <div className="space-y-4">
              <div>
                <label className="text-xs font-medium text-muted-foreground mb-2 block">Site</label>
                <select 
                  className={inputFieldClass}
                  value={selectedSiteId}
                  onChange={e => setSelectedSiteId(e.target.value)}
                >
                  <option value="">-- Select site --</option>
                  {sites.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
                </select>
              </div>
              <div>
                <label className="text-xs font-medium text-muted-foreground mb-2 block">Room Name</label>
                <input 
                  className={inputFieldClass}
                  value={roomName}
                  onChange={e => setRoomName(e.target.value)}
                  placeholder="e.g., Flower Room 1"
                  onKeyDown={e => e.key === 'Enter' && handleCreateRoom()}
                />
              </div>
              <button 
                onClick={handleCreateRoom} 
                disabled={isSubmitting || !selectedSiteId || !roomName.trim()} 
                className={btnPrimaryClass}
              >
                <Plus className="w-4 h-4" /> Create Room
              </button>
            </div>
          </StepContent>
        )}

        {/* Zone Step */}
        {currentStep === 'zone' && (
          <StepContent
            title="Define a Zone"
            description="Zones are areas within rooms where sensors are placed"
          >
            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="text-xs font-medium text-muted-foreground mb-2 block">Site</label>
                  <select className={inputFieldClass} value={selectedSiteId} onChange={e => setSelectedSiteId(e.target.value)}>
                    <option value="">-- Site --</option>
                    {sites.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
                  </select>
                </div>
                <div>
                  <label className="text-xs font-medium text-muted-foreground mb-2 block">Room</label>
                  <select className={inputFieldClass} value={selectedRoomId} onChange={e => setSelectedRoomId(e.target.value)}>
                    <option value="">-- Room --</option>
                    {siteRooms.map(r => <option key={r.id} value={r.id}>{r.name}</option>)}
                  </select>
                </div>
              </div>
              <div>
                <label className="text-xs font-medium text-muted-foreground mb-2 block">Zone Name</label>
                <input 
                  className={inputFieldClass}
                  value={zoneName}
                  onChange={e => setZoneName(e.target.value)}
                  placeholder="e.g., Zone A, Bench 1"
                />
              </div>
              <button 
                onClick={handleCreateZone} 
                disabled={isSubmitting || !selectedSiteId || !selectedRoomId || !zoneName.trim()} 
                className={btnPrimaryClass}
              >
                <Plus className="w-4 h-4" /> Create Zone
              </button>
            </div>
          </StepContent>
        )}

        {/* Equipment Step */}
        {currentStep === 'equipment' && (
          <StepContent
            title="Register Equipment"
            description="Equipment represents physical sensor devices"
          >
            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="text-xs font-medium text-muted-foreground mb-2 block">Site</label>
                  <select className={inputFieldClass} value={selectedSiteId} onChange={e => setSelectedSiteId(e.target.value)}>
                    <option value="">-- Site --</option>
                    {sites.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
                  </select>
                </div>
                <div>
                  <label className="text-xs font-medium text-muted-foreground mb-2 block">Zone</label>
                  <select className={inputFieldClass} value={selectedZoneId} onChange={e => setSelectedZoneId(e.target.value)}>
                    <option value="">-- Zone --</option>
                    {zones.map(z => <option key={z.id} value={z.id}>{z.name}</option>)}
                  </select>
                </div>
              </div>
              <div>
                <label className="text-xs font-medium text-muted-foreground mb-2 block">Equipment Type</label>
                <select 
                  className={inputFieldClass}
                  value={equipmentType}
                  onChange={e => setEquipmentType(e.target.value as CoreEquipmentType)}
                >
                  <option value="">-- Select type --</option>
                  {Object.entries(EquipmentTypeGroups).map(([group, types]) => (
                    <optgroup key={group} label={group}>
                      {types.map(type => (
                        <option key={type} value={type}>{EquipmentTypeLabels[type]}</option>
                      ))}
                    </optgroup>
                  ))}
                </select>
              </div>
              <div>
                <label className="text-xs font-medium text-muted-foreground mb-2 block">Equipment Code</label>
                <input 
                  className={inputFieldClass}
                  value={equipmentCode}
                  onChange={e => setEquipmentCode(e.target.value)}
                  placeholder="e.g., PH-INLINE-001"
                />
              </div>
              <button 
                onClick={handleCreateEquipment} 
                disabled={isSubmitting || !selectedSiteId || !selectedZoneId || !equipmentType || !equipmentCode.trim()} 
                className={btnPrimaryClass}
              >
                <Plus className="w-4 h-4" /> Create Equipment
              </button>
            </div>
          </StepContent>
        )}

        {/* Sensor Stream Step */}
        {currentStep === 'sensor' && (
          <StepContent
            title="Create Sensor Stream"
            description="Streams produce simulated time-series data"
          >
            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="text-xs font-medium text-muted-foreground mb-2 block">Site</label>
                  <select className={inputFieldClass} value={selectedSiteId} onChange={e => setSelectedSiteId(e.target.value)}>
                    <option value="">-- Site --</option>
                    {sites.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
                  </select>
                </div>
                <div>
                  <label className="text-xs font-medium text-muted-foreground mb-2 block">Equipment *</label>
                  <select className={inputFieldClass} value={selectedEquipmentId} onChange={e => setSelectedEquipmentId(e.target.value)}>
                    <option value="">-- Equipment --</option>
                    {equipment.map(e => <option key={e.id} value={e.id}>{e.code}</option>)}
                  </select>
                </div>
              </div>
              <div>
                <label className="text-xs font-medium text-muted-foreground mb-2 block">Stream Name</label>
                <input 
                  className={inputFieldClass}
                  value={streamName}
                  onChange={e => setStreamName(e.target.value)}
                  placeholder="e.g., Zone A Temperature"
                />
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="text-xs font-medium text-muted-foreground mb-2 block">Stream Type</label>
                  <select 
                    className={inputFieldClass}
                    value={streamType}
                    onChange={e => handleStreamTypeChange(Number(e.target.value) as StreamType)}
                  >
                    {Object.entries(StreamTypeGroups).map(([group, types]) => (
                      <optgroup key={group} label={group}>
                        {types.map(type => (
                          <option key={type} value={type}>{StreamTypeLabels[type]}</option>
                        ))}
                      </optgroup>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="text-xs font-medium text-muted-foreground mb-2 block">Unit</label>
                  <select 
                    className={inputFieldClass}
                    value={streamUnit}
                    onChange={e => setStreamUnit(Number(e.target.value) as Unit)}
                  >
                    {StreamTypeUnits[streamType]?.map(unit => (
                      <option key={unit} value={unit}>{UnitLabels[unit]}</option>
                    ))}
                  </select>
                </div>
              </div>
              <button 
                onClick={handleCreateStream} 
                disabled={isSubmitting || !selectedSiteId || !selectedEquipmentId || !streamName.trim()} 
                className={btnPrimaryClass}
              >
                <Radio className="w-4 h-4" /> Create Stream
              </button>
            </div>
          </StepContent>
        )}
      </div>
    </div>
  );
}

function StepContent({ 
  title, 
  description, 
  children 
}: { 
  title: string; 
  description: string; 
  children: React.ReactNode;
}) {
  return (
    <div>
      <div className="mb-4">
        <h3 className="font-medium text-foreground">{title}</h3>
        <p className="text-xs text-muted-foreground mt-1">{description}</p>
      </div>
      {children}
    </div>
  );
}

