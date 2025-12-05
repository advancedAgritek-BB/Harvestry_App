'use client';

import React, { useState } from 'react';
import {
  Droplets,
  Plus,
  Edit2,
  Trash2,
  Settings,
  Layers,
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

// Mock data for irrigation groups
const MOCK_GROUPS = [
  {
    id: '1',
    code: 'GRP-F1',
    name: 'Flower Room F1 - Main',
    pump: 'Pump-001 (2HP Main)',
    maxConcurrentValves: 4,
    sequenceMode: 'parallel',
    pressureSensor: 'PSensor-001',
    flowSensor: 'FSensor-001',
    zoneCount: 8,
    enabled: true,
  },
  {
    id: '2',
    code: 'GRP-F2',
    name: 'Flower Room F2 - Main',
    pump: 'Pump-002 (2HP Main)',
    maxConcurrentValves: 4,
    sequenceMode: 'parallel',
    pressureSensor: 'PSensor-002',
    flowSensor: 'FSensor-002',
    zoneCount: 6,
    enabled: true,
  },
  {
    id: '3',
    code: 'GRP-V1',
    name: 'Veg Room V1 - Drip',
    pump: 'Pump-003 (1HP Drip)',
    maxConcurrentValves: 2,
    sequenceMode: 'serial',
    pressureSensor: null,
    flowSensor: 'FSensor-003',
    zoneCount: 4,
    enabled: true,
  },
  {
    id: '4',
    code: 'GRP-PROP',
    name: 'Propagation - Mist',
    pump: 'Pump-004 (0.5HP Mist)',
    maxConcurrentValves: 1,
    sequenceMode: 'serial',
    pressureSensor: null,
    flowSensor: null,
    zoneCount: 2,
    enabled: false,
  },
];

const SEQUENCE_MODES = [
  { value: 'parallel', label: 'Parallel (Multiple zones at once)' },
  { value: 'serial', label: 'Serial (One zone at a time)' },
];

export function IrrigationGroupsSection() {
  const [searchQuery, setSearchQuery] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingGroup, setEditingGroup] = useState<typeof MOCK_GROUPS[0] | null>(null);

  const filteredGroups = MOCK_GROUPS.filter(
    (group) =>
      group.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      group.code.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const handleEdit = (group: typeof MOCK_GROUPS[0]) => {
    setEditingGroup(group);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    setEditingGroup(null);
    setIsModalOpen(true);
  };

  const columns = [
    {
      key: 'code',
      header: 'Code',
      width: '100px',
      render: (item: typeof MOCK_GROUPS[0]) => (
        <span className="font-mono text-xs bg-white/5 px-2 py-0.5 rounded">
          {item.code}
        </span>
      ),
    },
    {
      key: 'name',
      header: 'Group Name',
      sortable: true,
      render: (item: typeof MOCK_GROUPS[0]) => (
        <div className="font-medium text-foreground">{item.name}</div>
      ),
    },
    {
      key: 'pump',
      header: 'Pump',
      render: (item: typeof MOCK_GROUPS[0]) => (
        <span className="text-sm text-muted-foreground">{item.pump}</span>
      ),
    },
    {
      key: 'sequence',
      header: 'Mode',
      render: (item: typeof MOCK_GROUPS[0]) => (
        <span className={`text-xs px-2 py-0.5 rounded ${
          item.sequenceMode === 'parallel' 
            ? 'bg-cyan-500/10 text-cyan-400' 
            : 'bg-amber-500/10 text-amber-400'
        }`}>
          {item.sequenceMode === 'parallel' ? 'Parallel' : 'Serial'}
        </span>
      ),
    },
    {
      key: 'maxValves',
      header: 'Max Valves',
      render: (item: typeof MOCK_GROUPS[0]) => (
        <span>{item.maxConcurrentValves}</span>
      ),
    },
    {
      key: 'zones',
      header: 'Zones',
      render: (item: typeof MOCK_GROUPS[0]) => (
        <div className="flex items-center gap-1.5">
          <Layers className="w-3.5 h-3.5 text-muted-foreground" />
          <span>{item.zoneCount}</span>
        </div>
      ),
    },
    {
      key: 'sensors',
      header: 'Sensors',
      render: (item: typeof MOCK_GROUPS[0]) => (
        <div className="flex gap-2 text-xs">
          <span className={item.pressureSensor ? 'text-emerald-400' : 'text-muted-foreground/50'}>
            P: {item.pressureSensor ? '✓' : '—'}
          </span>
          <span className={item.flowSensor ? 'text-emerald-400' : 'text-muted-foreground/50'}>
            F: {item.flowSensor ? '✓' : '—'}
          </span>
        </div>
      ),
    },
    {
      key: 'enabled',
      header: 'Status',
      render: (item: typeof MOCK_GROUPS[0]) => (
        <StatusBadge status={item.enabled ? 'active' : 'inactive'} />
      ),
    },
    {
      key: 'actions',
      header: '',
      width: '80px',
      render: (item: typeof MOCK_GROUPS[0]) => (
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
      title="Irrigation Groups"
      description="Configure pump-to-zone groupings with valve sequencing and sensor assignments"
    >
      <AdminCard
        title="Group Configuration"
        icon={Droplets}
        actions={
          <div className="flex items-center gap-3">
            <TableSearch
              value={searchQuery}
              onChange={setSearchQuery}
              placeholder="Search groups..."
            />
            <Button onClick={handleCreate}>
              <Plus className="w-4 h-4" />
              Add Group
            </Button>
          </div>
        }
      >
        <AdminTable
          columns={columns}
          data={filteredGroups}
          keyField="id"
          onRowClick={handleEdit}
          emptyMessage="No irrigation groups configured"
        />
      </AdminCard>

      {/* Create/Edit Modal */}
      <AdminModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingGroup ? 'Edit Irrigation Group' : 'Create Irrigation Group'}
        description="Configure zone grouping, pump assignment, and valve sequencing"
        size="lg"
        footer={
          <>
            <Button variant="ghost" onClick={() => setIsModalOpen(false)}>
              Cancel
            </Button>
            <Button onClick={() => setIsModalOpen(false)}>
              {editingGroup ? 'Save Changes' : 'Create Group'}
            </Button>
          </>
        }
      >
        <div className="space-y-6">
          <div className="grid grid-cols-2 gap-4">
            <FormField label="Group Code" required>
              <Input 
                placeholder="e.g., GRP-F1" 
                defaultValue={editingGroup?.code} 
              />
            </FormField>
            <FormField label="Group Name" required>
              <Input 
                placeholder="e.g., Flower Room F1 - Main" 
                defaultValue={editingGroup?.name} 
              />
            </FormField>
          </div>

          <FormField label="Pump Equipment" required description="Select the pump for this group">
            <Select
              options={[
                { value: 'pump-001', label: 'Pump-001 (2HP Main)' },
                { value: 'pump-002', label: 'Pump-002 (2HP Main)' },
                { value: 'pump-003', label: 'Pump-003 (1HP Drip)' },
                { value: 'pump-004', label: 'Pump-004 (0.5HP Mist)' },
              ]}
              defaultValue="pump-001"
            />
          </FormField>

          <div className="grid grid-cols-2 gap-4">
            <FormField label="Sequence Mode" required>
              <Select options={SEQUENCE_MODES} defaultValue={editingGroup?.sequenceMode || 'parallel'} />
            </FormField>
            <FormField label="Max Concurrent Valves" required>
              <Input 
                type="number" 
                placeholder="4" 
                defaultValue={editingGroup?.maxConcurrentValves} 
              />
            </FormField>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <FormField label="Pressure Sensor" description="Optional">
              <Select
                options={[
                  { value: '', label: 'None' },
                  { value: 'psensor-001', label: 'PSensor-001' },
                  { value: 'psensor-002', label: 'PSensor-002' },
                ]}
                defaultValue=""
              />
            </FormField>
            <FormField label="Flow Sensor" description="Optional">
              <Select
                options={[
                  { value: '', label: 'None' },
                  { value: 'fsensor-001', label: 'FSensor-001' },
                  { value: 'fsensor-002', label: 'FSensor-002' },
                  { value: 'fsensor-003', label: 'FSensor-003' },
                ]}
                defaultValue=""
              />
            </FormField>
          </div>

          <div className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
            <div>
              <div className="text-sm font-medium text-foreground">Enable Group</div>
              <div className="text-xs text-muted-foreground">
                Disabled groups will not receive irrigation commands
              </div>
            </div>
            <Switch checked={editingGroup?.enabled ?? true} onChange={() => {}} />
          </div>
        </div>
      </AdminModal>
    </AdminSection>
  );
}

