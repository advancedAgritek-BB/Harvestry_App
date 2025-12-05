'use client';

import React, { useState } from 'react';
import {
  Container,
  Plus,
  Edit2,
  Trash2,
  Droplets,
  Thermometer,
  Activity,
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

// Mock data for mix tanks
const MOCK_TANKS = [
  {
    id: '1',
    code: 'TANK-A',
    name: 'Main Mix Tank A',
    capacityL: 500,
    currentLevelL: 420,
    lowLevelThresholdL: 100,
    tempC: 22.5,
    ecMscm: 2.35,
    ph: 5.92,
    probeEquipment: 'Probe-001',
    enabled: true,
  },
  {
    id: '2',
    code: 'TANK-B',
    name: 'Main Mix Tank B',
    capacityL: 500,
    currentLevelL: 380,
    lowLevelThresholdL: 100,
    tempC: 22.1,
    ecMscm: 2.41,
    ph: 5.88,
    probeEquipment: 'Probe-002',
    enabled: true,
  },
  {
    id: '3',
    code: 'TANK-VEG',
    name: 'Veg Room Tank',
    capacityL: 200,
    currentLevelL: 45,
    lowLevelThresholdL: 50,
    tempC: 23.0,
    ecMscm: 1.82,
    ph: 5.78,
    probeEquipment: 'Probe-003',
    enabled: true,
  },
  {
    id: '4',
    code: 'TANK-FLUSH',
    name: 'Flush Water Tank',
    capacityL: 300,
    currentLevelL: 290,
    lowLevelThresholdL: 50,
    tempC: 21.0,
    ecMscm: 0.12,
    ph: 6.0,
    probeEquipment: null,
    enabled: false,
  },
];

export function MixTanksSection() {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingTank, setEditingTank] = useState<typeof MOCK_TANKS[0] | null>(null);

  const handleEdit = (tank: typeof MOCK_TANKS[0]) => {
    setEditingTank(tank);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    setEditingTank(null);
    setIsModalOpen(true);
  };

  const getLevelPercent = (current: number, capacity: number) => (current / capacity) * 100;
  const isLowLevel = (current: number, threshold: number) => current <= threshold;

  const columns = [
    {
      key: 'code',
      header: 'Code',
      width: '100px',
      render: (item: typeof MOCK_TANKS[0]) => (
        <span className="font-mono text-xs bg-white/5 px-2 py-0.5 rounded">
          {item.code}
        </span>
      ),
    },
    {
      key: 'name',
      header: 'Tank Name',
      sortable: true,
      render: (item: typeof MOCK_TANKS[0]) => (
        <div className="font-medium text-foreground">{item.name}</div>
      ),
    },
    {
      key: 'level',
      header: 'Level',
      render: (item: typeof MOCK_TANKS[0]) => {
        const percent = getLevelPercent(item.currentLevelL, item.capacityL);
        const low = isLowLevel(item.currentLevelL, item.lowLevelThresholdL);
        return (
          <div className="flex items-center gap-2">
            <div className="w-16 h-2 bg-white/10 rounded-full overflow-hidden">
              <div 
                className={`h-full rounded-full ${low ? 'bg-rose-500' : percent < 30 ? 'bg-amber-500' : 'bg-cyan-500'}`}
                style={{ width: `${percent}%` }}
              />
            </div>
            <span className={`text-xs ${low ? 'text-rose-400' : 'text-muted-foreground'}`}>
              {item.currentLevelL}L / {item.capacityL}L
            </span>
          </div>
        );
      },
    },
    {
      key: 'readings',
      header: 'Current Readings',
      render: (item: typeof MOCK_TANKS[0]) => (
        <div className="flex gap-3 text-xs">
          <div className="flex items-center gap-1">
            <Thermometer className="w-3 h-3 text-amber-400" />
            <span>{item.tempC}°C</span>
          </div>
          <div className="flex items-center gap-1">
            <Activity className="w-3 h-3 text-emerald-400" />
            <span>{item.ecMscm}</span>
          </div>
          <div className="flex items-center gap-1">
            <Droplets className="w-3 h-3 text-violet-400" />
            <span>{item.ph}</span>
          </div>
        </div>
      ),
    },
    {
      key: 'probe',
      header: 'Probe',
      render: (item: typeof MOCK_TANKS[0]) => (
        <span className="text-xs text-muted-foreground">
          {item.probeEquipment || '—'}
        </span>
      ),
    },
    {
      key: 'enabled',
      header: 'Status',
      render: (item: typeof MOCK_TANKS[0]) => (
        <StatusBadge 
          status={
            !item.enabled ? 'inactive' : 
            isLowLevel(item.currentLevelL, item.lowLevelThresholdL) ? 'warning' : 
            'active'
          }
          label={
            !item.enabled ? 'Disabled' : 
            isLowLevel(item.currentLevelL, item.lowLevelThresholdL) ? 'Low Level' : 
            'OK'
          }
        />
      ),
    },
    {
      key: 'actions',
      header: '',
      width: '80px',
      render: (item: typeof MOCK_TANKS[0]) => (
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
      title="Mix Tanks"
      description="Configure mix tanks with capacity, probes, and level thresholds"
    >
      <AdminCard
        title="Tank Configuration"
        icon={Container}
        actions={
          <Button onClick={handleCreate}>
            <Plus className="w-4 h-4" />
            Add Tank
          </Button>
        }
      >
        <AdminTable
          columns={columns}
          data={MOCK_TANKS}
          keyField="id"
          onRowClick={handleEdit}
          emptyMessage="No mix tanks configured"
        />
      </AdminCard>

      {/* Create/Edit Modal */}
      <AdminModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingTank ? 'Edit Mix Tank' : 'Create Mix Tank'}
        description="Configure tank capacity, probes, and alert thresholds"
        size="lg"
        footer={
          <>
            <Button variant="ghost" onClick={() => setIsModalOpen(false)}>
              Cancel
            </Button>
            <Button onClick={() => setIsModalOpen(false)}>
              {editingTank ? 'Save Changes' : 'Create Tank'}
            </Button>
          </>
        }
      >
        <div className="space-y-6">
          <div className="grid grid-cols-2 gap-4">
            <FormField label="Tank Code" required>
              <Input placeholder="e.g., TANK-A" defaultValue={editingTank?.code} />
            </FormField>
            <FormField label="Tank Name" required>
              <Input placeholder="e.g., Main Mix Tank A" defaultValue={editingTank?.name} />
            </FormField>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <FormField label="Capacity (L)" required>
              <Input type="number" placeholder="500" defaultValue={editingTank?.capacityL} />
            </FormField>
            <FormField label="Low Level Threshold (L)" required>
              <Input type="number" placeholder="100" defaultValue={editingTank?.lowLevelThresholdL} />
            </FormField>
          </div>

          <FormField label="Probe Equipment" description="EC/pH/Temp probe for this tank">
            <Select
              options={[
                { value: '', label: 'None' },
                { value: 'probe-001', label: 'Probe-001' },
                { value: 'probe-002', label: 'Probe-002' },
                { value: 'probe-003', label: 'Probe-003' },
              ]}
              defaultValue={editingTank?.probeEquipment || ''}
            />
          </FormField>

          <div className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
            <div>
              <div className="text-sm font-medium text-foreground">Enable Tank</div>
              <div className="text-xs text-muted-foreground">
                Disabled tanks will not be used for irrigation
              </div>
            </div>
            <Switch checked={editingTank?.enabled ?? true} onChange={() => {}} />
          </div>
        </div>
      </AdminModal>
    </AdminSection>
  );
}

