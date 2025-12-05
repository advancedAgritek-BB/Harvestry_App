'use client';

import React, { useState } from 'react';
import {
  Thermometer,
  Plus,
  Edit2,
  Trash2,
  Copy,
  ChevronDown,
  Sun,
  Moon,
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
} from '@/components/admin';

// Mock data for environment targets
const MOCK_TARGETS = [
  {
    id: '1',
    scope: 'Flower Room F1',
    scopeType: 'room',
    metric: 'Temperature',
    targetDay: '78°F',
    targetNight: '72°F',
    min: '70°F',
    max: '82°F',
    precedence: 3,
    enabled: true,
  },
  {
    id: '2',
    scope: 'Flower Room F1',
    scopeType: 'room',
    metric: 'Humidity',
    targetDay: '55%',
    targetNight: '60%',
    min: '45%',
    max: '65%',
    precedence: 3,
    enabled: true,
  },
  {
    id: '3',
    scope: 'Gorilla Glue #4',
    scopeType: 'strain',
    metric: 'VPD',
    targetDay: '1.2 kPa',
    targetNight: '1.0 kPa',
    min: '0.8 kPa',
    max: '1.5 kPa',
    precedence: 1,
    enabled: true,
  },
  {
    id: '4',
    scope: 'Zone A',
    scopeType: 'zone',
    metric: 'CO2',
    targetDay: '1200 ppm',
    targetNight: '800 ppm',
    min: '400 ppm',
    max: '1500 ppm',
    precedence: 4,
    enabled: false,
  },
  {
    id: '5',
    scope: 'Veg Phase',
    scopeType: 'phase',
    metric: 'PPFD',
    targetDay: '450 μmol',
    targetNight: '-',
    min: '300 μmol',
    max: '600 μmol',
    precedence: 2,
    enabled: true,
  },
];

const SCOPE_TYPES = [
  { value: 'strain', label: 'Strain' },
  { value: 'phase', label: 'Growth Phase' },
  { value: 'room', label: 'Room' },
  { value: 'zone', label: 'Zone' },
  { value: 'rack', label: 'Rack' },
];

const METRICS = [
  { value: 'temperature', label: 'Temperature (°F)' },
  { value: 'humidity', label: 'Relative Humidity (%)' },
  { value: 'vpd', label: 'VPD (kPa)' },
  { value: 'co2', label: 'CO₂ (ppm)' },
  { value: 'ppfd', label: 'PPFD (μmol/m²/s)' },
  { value: 'dli', label: 'DLI (mol/m²/day)' },
  { value: 'airflow', label: 'Airflow (CFM)' },
  { value: 'pressure', label: 'Pressure Differential (Pa)' },
];

export function EnvironmentTargetsSection() {
  const [searchQuery, setSearchQuery] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingTarget, setEditingTarget] = useState<typeof MOCK_TARGETS[0] | null>(null);

  const filteredTargets = MOCK_TARGETS.filter(
    (target) =>
      target.scope.toLowerCase().includes(searchQuery.toLowerCase()) ||
      target.metric.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const handleEdit = (target: typeof MOCK_TARGETS[0]) => {
    setEditingTarget(target);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    setEditingTarget(null);
    setIsModalOpen(true);
  };

  const columns = [
    {
      key: 'scope',
      header: 'Scope',
      sortable: true,
      render: (item: typeof MOCK_TARGETS[0]) => (
        <div className="flex items-center gap-2">
          <span
            className={`w-2 h-2 rounded-full ${
              item.scopeType === 'strain'
                ? 'bg-emerald-400'
                : item.scopeType === 'phase'
                ? 'bg-cyan-400'
                : item.scopeType === 'room'
                ? 'bg-violet-400'
                : item.scopeType === 'zone'
                ? 'bg-amber-400'
                : 'bg-rose-400'
            }`}
          />
          <div>
            <div className="font-medium text-foreground">{item.scope}</div>
            <div className="text-xs text-muted-foreground capitalize">{item.scopeType}</div>
          </div>
        </div>
      ),
    },
    {
      key: 'metric',
      header: 'Metric',
      sortable: true,
    },
    {
      key: 'targetDay',
      header: 'Day Target',
      render: (item: typeof MOCK_TARGETS[0]) => (
        <div className="flex items-center gap-1.5">
          <Sun className="w-3.5 h-3.5 text-amber-400" />
          <span>{item.targetDay}</span>
        </div>
      ),
    },
    {
      key: 'targetNight',
      header: 'Night Target',
      render: (item: typeof MOCK_TARGETS[0]) => (
        <div className="flex items-center gap-1.5">
          <Moon className="w-3.5 h-3.5 text-blue-400" />
          <span>{item.targetNight}</span>
        </div>
      ),
    },
    {
      key: 'range',
      header: 'Range',
      render: (item: typeof MOCK_TARGETS[0]) => (
        <span className="text-muted-foreground">
          {item.min} – {item.max}
        </span>
      ),
    },
    {
      key: 'precedence',
      header: 'Priority',
      render: (item: typeof MOCK_TARGETS[0]) => (
        <span className="text-xs font-mono bg-white/5 px-2 py-0.5 rounded">
          P{item.precedence}
        </span>
      ),
    },
    {
      key: 'enabled',
      header: 'Status',
      render: (item: typeof MOCK_TARGETS[0]) => (
        <StatusBadge status={item.enabled ? 'active' : 'inactive'} />
      ),
    },
    {
      key: 'actions',
      header: '',
      width: '100px',
      render: (item: typeof MOCK_TARGETS[0]) => (
        <TableActions>
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
      title="Environment Targets"
      description="Configure target environmental conditions with scope-based precedence (strain → phase → room → zone → rack)"
    >
      <AdminCard
        title="Target Configurations"
        icon={Thermometer}
        actions={
          <div className="flex items-center gap-3">
            <TableSearch
              value={searchQuery}
              onChange={setSearchQuery}
              placeholder="Search targets..."
            />
            <Button onClick={handleCreate}>
              <Plus className="w-4 h-4" />
              Add Target
            </Button>
          </div>
        }
      >
        <div className="text-xs text-muted-foreground mb-4 p-3 bg-white/5 rounded-lg">
          <strong>Precedence Order:</strong> Strain (P1) → Phase (P2) → Room (P3) → Zone (P4) → Rack (P5). 
          Lower priority numbers override higher ones for the same metric.
        </div>
        <AdminTable
          columns={columns}
          data={filteredTargets}
          keyField="id"
          onRowClick={handleEdit}
          emptyMessage="No environment targets configured"
        />
      </AdminCard>

      {/* Create/Edit Modal */}
      <AdminModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingTarget ? 'Edit Environment Target' : 'Create Environment Target'}
        description="Configure target values and acceptable ranges for environmental metrics"
        size="lg"
        footer={
          <>
            <Button variant="ghost" onClick={() => setIsModalOpen(false)}>
              Cancel
            </Button>
            <Button onClick={() => setIsModalOpen(false)}>
              {editingTarget ? 'Save Changes' : 'Create Target'}
            </Button>
          </>
        }
      >
        <div className="space-y-6">
          <div className="grid grid-cols-2 gap-4">
            <FormField label="Scope Type" required>
              <Select
                options={SCOPE_TYPES}
                defaultValue={editingTarget?.scopeType || 'room'}
              />
            </FormField>
            <FormField label="Scope" required description="Select the specific scope">
              <Input placeholder="e.g., Flower Room F1" defaultValue={editingTarget?.scope} />
            </FormField>
          </div>

          <FormField label="Metric" required>
            <Select options={METRICS} defaultValue="temperature" />
          </FormField>

          <div className="grid grid-cols-2 gap-4">
            <FormField label="Day Target" required>
              <Input type="number" placeholder="78" defaultValue={editingTarget?.targetDay?.replace(/[^0-9.]/g, '')} />
            </FormField>
            <FormField label="Night Target" required>
              <Input type="number" placeholder="72" defaultValue={editingTarget?.targetNight?.replace(/[^0-9.]/g, '')} />
            </FormField>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <FormField label="Minimum Acceptable" required>
              <Input type="number" placeholder="70" defaultValue={editingTarget?.min?.replace(/[^0-9.]/g, '')} />
            </FormField>
            <FormField label="Maximum Acceptable" required>
              <Input type="number" placeholder="82" defaultValue={editingTarget?.max?.replace(/[^0-9.]/g, '')} />
            </FormField>
          </div>

          <div className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
            <div>
              <div className="text-sm font-medium text-foreground">Enable Target</div>
              <div className="text-xs text-muted-foreground">
                Active targets will be used for monitoring and alerts
              </div>
            </div>
            <Switch checked={editingTarget?.enabled ?? true} onChange={() => {}} />
          </div>
        </div>
      </AdminModal>
    </AdminSection>
  );
}

