'use client';

import React, { useState } from 'react';
import {
  Syringe,
  Plus,
  Edit2,
  Trash2,
  Settings,
} from 'lucide-react';
import {
  AdminCard,
  AdminSection,
  AdminTable,
  StatusBadge,
  TableActions,
  TableActionButton,
  Button,
  AdminModal,
  FormField,
  Input,
  Select,
  Switch,
} from '@/components/admin';

// Mock data for injector channels
const MOCK_INJECTORS = [
  {
    id: '1',
    code: 'INJ-01',
    name: 'Bloom A Injector',
    driverType: 'peristaltic',
    ratioMlPerL: 3.0,
    maxRate: 50,
    calibrationCoeff: 1.02,
    mixTank: 'TANK-A',
    enabled: true,
  },
  {
    id: '2',
    code: 'INJ-02',
    name: 'Bloom B Injector',
    driverType: 'peristaltic',
    ratioMlPerL: 3.0,
    maxRate: 50,
    calibrationCoeff: 0.98,
    mixTank: 'TANK-A',
    enabled: true,
  },
  {
    id: '3',
    code: 'INJ-03',
    name: 'Cal-Mag Injector',
    driverType: 'dosatron',
    ratioMlPerL: 2.0,
    maxRate: 100,
    calibrationCoeff: 1.0,
    mixTank: 'TANK-A',
    enabled: true,
  },
  {
    id: '4',
    code: 'INJ-04',
    name: 'PK Boost Injector',
    driverType: 'venturi',
    ratioMlPerL: 1.0,
    maxRate: 25,
    calibrationCoeff: 1.05,
    mixTank: 'TANK-B',
    enabled: false,
  },
  {
    id: '5',
    code: 'INJ-05',
    name: 'pH Down Injector',
    driverType: 'peristaltic',
    ratioMlPerL: 0.5,
    maxRate: 10,
    calibrationCoeff: 1.0,
    mixTank: null,
    enabled: true,
  },
];

const DRIVER_TYPES = [
  { value: 'dosatron', label: 'Dosatron (Proportional)' },
  { value: 'peristaltic', label: 'Peristaltic Pump' },
  { value: 'venturi', label: 'Venturi Injector' },
  { value: 'inline', label: 'Inline Mixer' },
];

const TANKS = [
  { value: '', label: 'None (Inline)' },
  { value: 'tank-a', label: 'TANK-A - Main Mix Tank A' },
  { value: 'tank-b', label: 'TANK-B - Main Mix Tank B' },
  { value: 'tank-veg', label: 'TANK-VEG - Veg Room Tank' },
];

export function InjectorChannelsSection() {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingInjector, setEditingInjector] = useState<typeof MOCK_INJECTORS[0] | null>(null);

  const handleEdit = (injector: typeof MOCK_INJECTORS[0]) => {
    setEditingInjector(injector);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    setEditingInjector(null);
    setIsModalOpen(true);
  };

  const getDriverBadge = (type: string) => {
    const colors = {
      dosatron: 'bg-cyan-500/10 text-cyan-400',
      peristaltic: 'bg-violet-500/10 text-violet-400',
      venturi: 'bg-amber-500/10 text-amber-400',
      inline: 'bg-emerald-500/10 text-emerald-400',
    };
    return colors[type as keyof typeof colors] || 'bg-white/10 text-muted-foreground';
  };

  const columns = [
    {
      key: 'code',
      header: 'Code',
      width: '80px',
      render: (item: typeof MOCK_INJECTORS[0]) => (
        <span className="font-mono text-xs bg-white/5 px-2 py-0.5 rounded">
          {item.code}
        </span>
      ),
    },
    {
      key: 'name',
      header: 'Injector Name',
      sortable: true,
      render: (item: typeof MOCK_INJECTORS[0]) => (
        <div className="font-medium text-foreground">{item.name}</div>
      ),
    },
    {
      key: 'driverType',
      header: 'Driver Type',
      render: (item: typeof MOCK_INJECTORS[0]) => (
        <span className={`text-xs px-2 py-0.5 rounded capitalize ${getDriverBadge(item.driverType)}`}>
          {item.driverType}
        </span>
      ),
    },
    {
      key: 'ratio',
      header: 'Ratio (mL/L)',
      render: (item: typeof MOCK_INJECTORS[0]) => (
        <span>{item.ratioMlPerL}</span>
      ),
    },
    {
      key: 'maxRate',
      header: 'Max Rate',
      render: (item: typeof MOCK_INJECTORS[0]) => (
        <span className="text-muted-foreground">{item.maxRate} mL/min</span>
      ),
    },
    {
      key: 'calibration',
      header: 'Calibration',
      render: (item: typeof MOCK_INJECTORS[0]) => (
        <span className={item.calibrationCoeff !== 1.0 ? 'text-amber-400' : 'text-muted-foreground'}>
          ×{item.calibrationCoeff}
        </span>
      ),
    },
    {
      key: 'tank',
      header: 'Tank',
      render: (item: typeof MOCK_INJECTORS[0]) => (
        <span className="text-xs text-muted-foreground">
          {item.mixTank || 'Inline'}
        </span>
      ),
    },
    {
      key: 'enabled',
      header: 'Status',
      render: (item: typeof MOCK_INJECTORS[0]) => (
        <StatusBadge status={item.enabled ? 'active' : 'inactive'} />
      ),
    },
    {
      key: 'actions',
      header: '',
      width: '80px',
      render: (item: typeof MOCK_INJECTORS[0]) => (
        <TableActions>
          <TableActionButton onClick={() => handleEdit(item)}>
            <Edit2 className="w-4 h-4" />
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
      title="Injector Channels"
      description="Configure nutrient injectors with driver types, ratios, and calibration coefficients"
    >
      <AdminCard
        title="Injector Configuration"
        icon={Syringe}
        actions={
          <Button onClick={handleCreate}>
            <Plus className="w-4 h-4" />
            Add Injector
          </Button>
        }
      >
        <AdminTable
          columns={columns}
          data={MOCK_INJECTORS}
          keyField="id"
          onRowClick={handleEdit}
          emptyMessage="No injector channels configured"
        />
      </AdminCard>

      {/* Create/Edit Modal */}
      <AdminModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingInjector ? 'Edit Injector Channel' : 'Create Injector Channel'}
        description="Configure injector driver, ratio, and calibration"
        size="lg"
        footer={
          <>
            <Button variant="ghost" onClick={() => setIsModalOpen(false)}>
              Cancel
            </Button>
            <Button onClick={() => setIsModalOpen(false)}>
              {editingInjector ? 'Save Changes' : 'Create Injector'}
            </Button>
          </>
        }
      >
        <div className="space-y-6">
          <div className="grid grid-cols-2 gap-4">
            <FormField label="Injector Code" required>
              <Input placeholder="e.g., INJ-01" defaultValue={editingInjector?.code} />
            </FormField>
            <FormField label="Injector Name" required>
              <Input placeholder="e.g., Bloom A Injector" defaultValue={editingInjector?.name} />
            </FormField>
          </div>

          <FormField label="Driver Type" required>
            <Select options={DRIVER_TYPES} defaultValue={editingInjector?.driverType || 'peristaltic'} />
          </FormField>

          <div className="grid grid-cols-3 gap-4">
            <FormField label="Ratio (mL/L)" required>
              <Input type="number" step="0.1" placeholder="3.0" defaultValue={editingInjector?.ratioMlPerL} />
            </FormField>
            <FormField label="Max Rate (mL/min)" required>
              <Input type="number" placeholder="50" defaultValue={editingInjector?.maxRate} />
            </FormField>
            <FormField label="Calibration Coeff" required>
              <Input type="number" step="0.01" placeholder="1.00" defaultValue={editingInjector?.calibrationCoeff} />
            </FormField>
          </div>

          <FormField label="Mix Tank Assignment" description="Tank where nutrient is mixed (optional)">
            <Select options={TANKS} defaultValue={editingInjector?.mixTank || ''} />
          </FormField>

          <div className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
            <div>
              <div className="text-sm font-medium text-foreground">Enable Injector</div>
              <div className="text-xs text-muted-foreground">
                Disabled injectors will not be used in recipes
              </div>
            </div>
            <Switch checked={editingInjector?.enabled ?? true} onChange={() => {}} />
          </div>
        </div>
      </AdminModal>
    </AdminSection>
  );
}

