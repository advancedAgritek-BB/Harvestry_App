'use client';

import React, { useState, useCallback } from 'react';
import {
  Workflow,
  Plus,
  Edit2,
  Trash2,
  Copy,
  Play,
  Pause,
} from 'lucide-react';
import {
  AdminCard,
  AdminSection,
  AdminTable,
  StatusBadge,
  TableActions,
  TableActionButton,
  TableSearch,
  Button,
  AdminModal,
  FormField,
  Input,
  Select,
  Switch,
  Textarea,
} from '@/components/admin';
import { 
  IrrigationProfileEditor, 
  ZoneCalibration,
  DayProfile,
  NightProfile,
  SafetyPolicy,
} from '@/components/irrigation';

// Mock data for irrigation programs with extended profile data
const MOCK_PROGRAMS = [
  {
    id: '1',
    name: 'Flower Generative Push',
    group: 'GRP-F1',
    groupId: 'grp-f1',
    recipe: 'Flower Week 4-6 v2.1',
    recipeId: 'flower-4-6',
    description: 'Aggressive P1 ramp with controlled dryback',
    dayProfile: {
      shotConfig: { shotSizeMl: 50, expectedVwcIncreasePercent: 2, minSoakTimeMinutes: 30, maxShotsPerDay: 10 },
      phaseTargets: { p1TargetVwcPercent: 65, p1ShotCount: 6, p2TargetVwcPercent: 55, p2ShotCount: 4, p3TargetDrybackPercent: 25, p3AllowEmergencyShots: false },
    },
    nightProfile: { allowIrrigation: false, description: 'No irrigation, maintain dryback' },
    safetyPolicy: { maxVolumeMlPerPlantPerDay: 120, maxEc: 3.5, minEc: 1.5, maxPh: 6.5, minPh: 5.5 },
    dayProfileSummary: 'P1: 6 shots → 65% VWC, P2: 4 shots → 55% VWC, P3: dryback to 25%',
    nightProfileSummary: 'No irrigation, maintain dryback',
    safetyPolicySummary: 'Max 120mL/plant, EC < 3.5',
    enabled: true,
    zoneCalibration: { 
      zoneId: 'zone-f1-a', 
      zoneName: 'F1-Zone A', 
      emitterFlowMlPerSecond: 1.5, 
      emittersPerPlant: 2,
      mediaVolume: 3,
      mediaVolumeUnit: 'gallons' as const,
      method: 'container' as const,
      targetVolumeMl: 500,
      measuredTimeSeconds: 45,
      runsCount: 1,
      calibratedByUserId: 'user-1',
      calibratedAt: new Date().toISOString(),
    },
  },
  {
    id: '2',
    name: 'Veg Maintenance',
    group: 'GRP-V1',
    groupId: 'grp-v1',
    recipe: 'Veg Standard v1.3',
    recipeId: 'veg-standard',
    description: 'Standard veg maintenance with VWC targeting',
    dayProfile: {
      shotConfig: { shotSizeMl: 40, expectedVwcIncreasePercent: 3, minSoakTimeMinutes: 45, maxShotsPerDay: 8 },
      phaseTargets: { p1TargetVwcPercent: 70, p1ShotCount: 4, p2TargetVwcPercent: 65, p2ShotCount: 4, p3TargetDrybackPercent: 20, p3AllowEmergencyShots: true },
    },
    nightProfile: { allowIrrigation: true, maintainVwcPercent: 50, maxNightShots: 1, description: 'Single shot at lights off' },
    safetyPolicy: { maxVolumeMlPerPlantPerDay: 150, maxEc: 2.2, minEc: 1.8, maxPh: 6.2, minPh: 5.8 },
    dayProfileSummary: 'Target VWC 65-70%, shots as needed',
    nightProfileSummary: 'Single shot at lights off',
    safetyPolicySummary: 'EC 1.8-2.2, pH 5.8-6.2',
    enabled: true,
    zoneCalibration: null,
  },
  {
    id: '3',
    name: 'Flower Flush',
    group: 'GRP-F1',
    groupId: 'grp-f1',
    recipe: 'Flush Only',
    recipeId: 'flush',
    description: 'Pre-harvest flush program',
    dayProfile: {
      shotConfig: { shotSizeMl: 100, expectedVwcIncreasePercent: 5, minSoakTimeMinutes: 60, maxShotsPerDay: 6 },
      phaseTargets: { p1TargetVwcPercent: 75, p1ShotCount: 3, p2TargetVwcPercent: 70, p2ShotCount: 3, p3TargetDrybackPercent: 30, p3AllowEmergencyShots: false },
    },
    nightProfile: { allowIrrigation: false, description: 'Extended dryback' },
    safetyPolicy: { maxVolumeMlPerPlantPerDay: 300, maxEc: 0.5, minEc: 0, maxPh: 7.0, minPh: 5.5 },
    dayProfileSummary: 'High volume, low EC flush cycles',
    nightProfileSummary: 'Extended dryback',
    safetyPolicySummary: 'EC < 0.5, high runoff',
    enabled: false,
    zoneCalibration: null,
  },
  {
    id: '4',
    name: 'Propagation Mist',
    group: 'GRP-PROP',
    groupId: 'grp-prop',
    recipe: 'Clone Mist v1.0',
    recipeId: 'clone-mist',
    description: 'High humidity misting for propagation',
    dayProfile: {
      shotConfig: { shotSizeMl: 10, expectedVwcIncreasePercent: 1, minSoakTimeMinutes: 5, maxShotsPerDay: 50 },
      phaseTargets: { p1TargetVwcPercent: 85, p1ShotCount: 20, p2TargetVwcPercent: 80, p2ShotCount: 20, p3TargetDrybackPercent: 5, p3AllowEmergencyShots: true },
    },
    nightProfile: { allowIrrigation: true, maintainVwcPercent: 75, maxNightShots: 10, description: '30s on / 15min off cycles' },
    safetyPolicy: { maxVolumeMlPerPlantPerDay: 200, maxEc: 1.5, minEc: 0.5, maxPh: 6.5, minPh: 5.5 },
    dayProfileSummary: '30s on / 5min off cycles',
    nightProfileSummary: '30s on / 15min off cycles',
    safetyPolicySummary: 'RH > 85%, no pooling',
    enabled: true,
    zoneCalibration: null,
  },
];

const GROUPS = [
  { value: 'grp-f1', label: 'GRP-F1 - Flower Room F1' },
  { value: 'grp-f2', label: 'GRP-F2 - Flower Room F2' },
  { value: 'grp-v1', label: 'GRP-V1 - Veg Room V1' },
  { value: 'grp-prop', label: 'GRP-PROP - Propagation' },
];

const RECIPES = [
  { value: 'flower-4-6', label: 'Flower Week 4-6 v2.1' },
  { value: 'flower-7-9', label: 'Flower Week 7-9 v2.0' },
  { value: 'veg-standard', label: 'Veg Standard v1.3' },
  { value: 'flush', label: 'Flush Only' },
  { value: 'clone-mist', label: 'Clone Mist v1.0' },
];

// Mock zones for calibration
const MOCK_ZONES = [
  { id: 'zone-f1-a', name: 'F1-Zone A' },
  { id: 'zone-f1-b', name: 'F1-Zone B' },
  { id: 'zone-f1-c', name: 'F1-Zone C' },
  { id: 'zone-v1-a', name: 'V1-Zone A' },
  { id: 'zone-v1-b', name: 'V1-Zone B' },
  { id: 'zone-prop-1', name: 'Prop-Zone 1' },
];

type ProgramType = typeof MOCK_PROGRAMS[0];

export function IrrigationProgramsSection() {
  const [searchQuery, setSearchQuery] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingProgram, setEditingProgram] = useState<ProgramType | null>(null);
  
  // Profile editor state
  const [dayProfileState, setDayProfileState] = useState<Partial<DayProfile>>({});
  const [nightProfileState, setNightProfileState] = useState<Partial<NightProfile>>({});
  const [safetyPolicyState, setSafetyPolicyState] = useState<Partial<SafetyPolicy>>({});
  const [calibrationState, setCalibrationState] = useState<ZoneCalibration | null>(null);

  const filteredPrograms = MOCK_PROGRAMS.filter(
    (program) =>
      program.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      program.group.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const handleEdit = useCallback((program: ProgramType) => {
    setEditingProgram(program);
    // Initialize profile editor state from program data
    setDayProfileState(program.dayProfile || {});
    setNightProfileState(program.nightProfile || {});
    setSafetyPolicyState(program.safetyPolicy || {});
    setCalibrationState(program.zoneCalibration as ZoneCalibration | null);
    setIsModalOpen(true);
  }, []);

  const handleCreate = useCallback(() => {
    setEditingProgram(null);
    // Reset profile editor state
    setDayProfileState({});
    setNightProfileState({});
    setSafetyPolicyState({});
    setCalibrationState(null);
    setIsModalOpen(true);
  }, []);

  const handleDayProfileChange = useCallback((profile: Partial<DayProfile>) => {
    setDayProfileState(prev => ({ ...prev, ...profile }));
  }, []);

  const handleNightProfileChange = useCallback((profile: Partial<NightProfile>) => {
    setNightProfileState(prev => ({ ...prev, ...profile }));
  }, []);

  const handleSafetyPolicyChange = useCallback((policy: Partial<SafetyPolicy>) => {
    setSafetyPolicyState(prev => ({ ...prev, ...policy }));
  }, []);

  const handleCalibrationChange = useCallback((calibration: ZoneCalibration) => {
    setCalibrationState(calibration);
  }, []);

  const columns = [
    {
      key: 'name',
      header: 'Program Name',
      sortable: true,
      render: (item: ProgramType) => (
        <div>
          <div className="font-medium text-foreground">{item.name}</div>
          <div className="text-xs text-muted-foreground">{item.description}</div>
        </div>
      ),
    },
    {
      key: 'group',
      header: 'Group',
      render: (item: ProgramType) => (
        <span className="font-mono text-xs bg-white/5 px-2 py-0.5 rounded">
          {item.group}
        </span>
      ),
    },
    {
      key: 'recipe',
      header: 'Recipe',
      render: (item: ProgramType) => (
        <span className="text-sm text-cyan-400">{item.recipe}</span>
      ),
    },
    {
      key: 'dayProfile',
      header: 'Day Profile',
      render: (item: ProgramType) => (
        <span className="text-xs text-muted-foreground">{item.dayProfileSummary}</span>
      ),
    },
    {
      key: 'nightProfile',
      header: 'Night Profile',
      render: (item: ProgramType) => (
        <span className="text-xs text-muted-foreground">{item.nightProfileSummary}</span>
      ),
    },
    {
      key: 'enabled',
      header: 'Status',
      render: (item: ProgramType) => (
        <StatusBadge status={item.enabled ? 'active' : 'inactive'} />
      ),
    },
    {
      key: 'actions',
      header: '',
      width: '120px',
      render: (item: ProgramType) => (
        <TableActions>
          <TableActionButton onClick={() => {}}>
            {item.enabled ? <Pause className="w-4 h-4" /> : <Play className="w-4 h-4" />}
          </TableActionButton>
          <TableActionButton onClick={() => handleEdit(item)}>
            <Edit2 className="w-4 h-4" />
          </TableActionButton>
          <TableActionButton onClick={() => {}}>
            <Copy className="w-4 h-4" />
          </TableActionButton>
          <TableActionButton onClick={() => {}} variant="danger">
            <Trash2 className="w-4 h-4" />
          </TableActionButton>
        </TableActions>
      ),
    },
  ];

  return (
    <AdminSection
      title="Irrigation Programs"
      description="Define irrigation programs with day/night profiles, recipes, and safety policies"
    >
      <AdminCard
        title="Program Configuration"
        icon={Workflow}
        actions={
          <div className="flex items-center gap-3">
            <TableSearch
              value={searchQuery}
              onChange={setSearchQuery}
              placeholder="Search programs..."
            />
            <Button onClick={handleCreate}>
              <Plus className="w-4 h-4" />
              Add Program
            </Button>
          </div>
        }
      >
        <AdminTable
          columns={columns}
          data={filteredPrograms}
          keyField="id"
          onRowClick={handleEdit}
          emptyMessage="No irrigation programs configured"
        />
      </AdminCard>

      {/* Create/Edit Modal */}
      <AdminModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingProgram ? 'Edit Irrigation Program' : 'Create Irrigation Program'}
        description="Configure program settings, profiles, and safety policies"
        size="xl"
        footer={
          <>
            <Button variant="ghost" onClick={() => setIsModalOpen(false)}>
              Cancel
            </Button>
            <Button onClick={() => setIsModalOpen(false)}>
              {editingProgram ? 'Save Changes' : 'Create Program'}
            </Button>
          </>
        }
      >
        <div className="space-y-6 max-h-[70vh] overflow-y-auto pr-2">
          {/* Basic Info Section */}
          <div className="space-y-4">
            <FormField label="Program Name" required>
              <Input 
                placeholder="e.g., Flower Generative Push" 
                defaultValue={editingProgram?.name} 
              />
            </FormField>

            <FormField label="Description">
              <Textarea 
                rows={2}
                placeholder="Brief description of the program's purpose"
                defaultValue={editingProgram?.description} 
              />
            </FormField>

            <div className="grid grid-cols-2 gap-4">
              <FormField label="Irrigation Group" required>
                <Select 
                  options={GROUPS} 
                  defaultValue={editingProgram?.groupId || 'grp-f1'} 
                />
              </FormField>
              <FormField label="Feed Recipe" required>
                <Select 
                  options={RECIPES} 
                  defaultValue={editingProgram?.recipeId || 'flower-4-6'} 
                />
              </FormField>
            </div>
          </div>

          {/* Divider */}
          <div className="border-t border-border pt-4">
            <h3 className="text-sm font-semibold text-foreground uppercase tracking-wide mb-4">
              Irrigation Profile Configuration
            </h3>
          </div>

          {/* Irrigation Profile Editor */}
          <IrrigationProfileEditor
            dayProfile={dayProfileState}
            nightProfile={nightProfileState}
            safetyPolicy={safetyPolicyState}
            zoneCalibration={calibrationState}
            zones={MOCK_ZONES}
            onDayProfileChange={handleDayProfileChange}
            onNightProfileChange={handleNightProfileChange}
            onSafetyPolicyChange={handleSafetyPolicyChange}
            onCalibrationChange={handleCalibrationChange}
          />

          {/* Enable Toggle */}
          <div className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
            <div>
              <div className="text-sm font-medium text-foreground">Enable Program</div>
              <div className="text-xs text-muted-foreground">
                Disabled programs will not be scheduled
              </div>
            </div>
            <Switch checked={editingProgram?.enabled ?? true} onChange={() => {}} />
          </div>
        </div>
      </AdminModal>
    </AdminSection>
  );
}

