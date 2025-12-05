'use client';

import React, { useState } from 'react';
import {
  Shield,
  Plus,
  Edit2,
  Trash2,
  AlertOctagon,
  Droplets,
  Thermometer,
  Wind,
  Clock,
  Power,
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
  Textarea,
} from '@/components/admin';

// Mock data for interlocks
const MOCK_INTERLOCKS = [
  {
    id: '1',
    name: 'Low Tank Level Stop',
    type: 'tank_level',
    scope: 'All Mix Tanks',
    condition: 'Level < 50L',
    action: 'Stop irrigation, alert',
    priority: 'critical',
    enabled: true,
  },
  {
    id: '2',
    name: 'EC Bounds Check',
    type: 'ec_bounds',
    scope: 'All Irrigation Groups',
    condition: 'EC > 4.0 or EC < 0.5',
    action: 'Stop dosing, flush, alert',
    priority: 'critical',
    enabled: true,
  },
  {
    id: '3',
    name: 'pH Safety Bounds',
    type: 'ph_bounds',
    scope: 'All Irrigation Groups',
    condition: 'pH < 4.5 or pH > 7.5',
    action: 'Stop dosing, alert',
    priority: 'critical',
    enabled: true,
  },
  {
    id: '4',
    name: 'CO2 Exhaust Lockout',
    type: 'co2_exhaust',
    scope: 'All Rooms',
    condition: 'CO2 > 1500 ppm',
    action: 'Force exhaust fans ON, lock CO2 valve',
    priority: 'high',
    enabled: true,
  },
  {
    id: '5',
    name: 'Night Curfew',
    type: 'curfew',
    scope: 'Flower Rooms',
    condition: '20:00 - 06:00',
    action: 'No irrigation during dark period',
    priority: 'medium',
    enabled: true,
  },
  {
    id: '6',
    name: 'Emergency Stop',
    type: 'estop',
    scope: 'Site-wide',
    condition: 'E-Stop pressed',
    action: 'All equipment OFF, all valves CLOSED',
    priority: 'critical',
    enabled: true,
  },
  {
    id: '7',
    name: 'Max Runtime Protection',
    type: 'max_runtime',
    scope: 'All Pumps',
    condition: 'Runtime > 30 min continuous',
    action: 'Stop pump, alert',
    priority: 'high',
    enabled: false,
  },
];

const INTERLOCK_TYPES = [
  { value: 'tank_level', label: 'Tank Level' },
  { value: 'ec_bounds', label: 'EC Bounds' },
  { value: 'ph_bounds', label: 'pH Bounds' },
  { value: 'co2_exhaust', label: 'CO2 Exhaust Lockout' },
  { value: 'curfew', label: 'Curfew/Quiet Hours' },
  { value: 'estop', label: 'Emergency Stop' },
  { value: 'max_runtime', label: 'Max Runtime' },
  { value: 'temperature', label: 'Temperature Bounds' },
  { value: 'pressure', label: 'Pressure Bounds' },
];

const PRIORITIES = [
  { value: 'critical', label: 'Critical (Immediate action)' },
  { value: 'high', label: 'High (Fast response)' },
  { value: 'medium', label: 'Medium (Standard response)' },
  { value: 'low', label: 'Low (Advisory)' },
];

export function InterlocksSection() {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingInterlock, setEditingInterlock] = useState<typeof MOCK_INTERLOCKS[0] | null>(null);

  const handleEdit = (interlock: typeof MOCK_INTERLOCKS[0]) => {
    setEditingInterlock(interlock);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    setEditingInterlock(null);
    setIsModalOpen(true);
  };

  const getTypeIcon = (type: string) => {
    switch (type) {
      case 'tank_level':
        return <Droplets className="w-4 h-4 text-cyan-400" />;
      case 'ec_bounds':
      case 'ph_bounds':
        return <Thermometer className="w-4 h-4 text-emerald-400" />;
      case 'co2_exhaust':
        return <Wind className="w-4 h-4 text-amber-400" />;
      case 'curfew':
        return <Clock className="w-4 h-4 text-violet-400" />;
      case 'estop':
        return <Power className="w-4 h-4 text-rose-400" />;
      default:
        return <Shield className="w-4 h-4 text-muted-foreground" />;
    }
  };

  const columns = [
    {
      key: 'name',
      header: 'Interlock Name',
      sortable: true,
      render: (item: typeof MOCK_INTERLOCKS[0]) => (
        <div className="flex items-center gap-3">
          {getTypeIcon(item.type)}
          <div>
            <div className="font-medium text-foreground">{item.name}</div>
            <div className="text-xs text-muted-foreground capitalize">
              {item.type.replace(/_/g, ' ')}
            </div>
          </div>
        </div>
      ),
    },
    {
      key: 'scope',
      header: 'Scope',
      render: (item: typeof MOCK_INTERLOCKS[0]) => (
        <span className="text-sm text-muted-foreground">{item.scope}</span>
      ),
    },
    {
      key: 'condition',
      header: 'Condition',
      render: (item: typeof MOCK_INTERLOCKS[0]) => (
        <span className="text-xs font-mono bg-white/5 px-2 py-0.5 rounded">
          {item.condition}
        </span>
      ),
    },
    {
      key: 'action',
      header: 'Action',
      render: (item: typeof MOCK_INTERLOCKS[0]) => (
        <span className="text-xs text-muted-foreground">{item.action}</span>
      ),
    },
    {
      key: 'priority',
      header: 'Priority',
      render: (item: typeof MOCK_INTERLOCKS[0]) => (
        <StatusBadge
          status={
            item.priority === 'critical' ? 'error' :
            item.priority === 'high' ? 'warning' :
            'active'
          }
          label={item.priority.charAt(0).toUpperCase() + item.priority.slice(1)}
        />
      ),
    },
    {
      key: 'enabled',
      header: 'Status',
      render: (item: typeof MOCK_INTERLOCKS[0]) => (
        <StatusBadge status={item.enabled ? 'active' : 'inactive'} />
      ),
    },
    {
      key: 'actions',
      header: '',
      width: '80px',
      render: (item: typeof MOCK_INTERLOCKS[0]) => (
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
      title="Interlocks"
      description="Configure safety interlocks for fail-safe operation"
    >
      <AdminCard
        title="Safety Interlock Configuration"
        icon={Shield}
        actions={
          <Button onClick={handleCreate}>
            <Plus className="w-4 h-4" />
            Add Interlock
          </Button>
        }
      >
        <div className="flex items-center gap-2 p-3 bg-rose-500/10 border border-rose-500/20 rounded-lg mb-4">
          <AlertOctagon className="w-4 h-4 text-rose-400 flex-shrink-0" />
          <p className="text-xs text-rose-200">
            <strong>Critical Safety System:</strong> Interlocks provide fail-safe protection. 
            Disabling critical interlocks may result in equipment damage or crop loss. 
            All changes are logged for audit.
          </p>
        </div>
        <AdminTable
          columns={columns}
          data={MOCK_INTERLOCKS}
          keyField="id"
          onRowClick={handleEdit}
          emptyMessage="No interlocks configured"
        />
      </AdminCard>

      {/* Create/Edit Modal */}
      <AdminModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingInterlock ? 'Edit Interlock' : 'Create Interlock'}
        description="Configure safety interlock conditions and actions"
        size="lg"
        footer={
          <>
            <Button variant="ghost" onClick={() => setIsModalOpen(false)}>
              Cancel
            </Button>
            <Button onClick={() => setIsModalOpen(false)}>
              {editingInterlock ? 'Save Changes' : 'Create Interlock'}
            </Button>
          </>
        }
      >
        <div className="space-y-6">
          <FormField label="Interlock Name" required>
            <Input 
              placeholder="e.g., Low Tank Level Stop" 
              defaultValue={editingInterlock?.name} 
            />
          </FormField>

          <div className="grid grid-cols-2 gap-4">
            <FormField label="Interlock Type" required>
              <Select options={INTERLOCK_TYPES} defaultValue={editingInterlock?.type || 'tank_level'} />
            </FormField>
            <FormField label="Priority" required>
              <Select options={PRIORITIES} defaultValue={editingInterlock?.priority || 'high'} />
            </FormField>
          </div>

          <FormField label="Scope" required description="Which equipment/areas this interlock applies to">
            <Input 
              placeholder="e.g., All Mix Tanks, Flower Rooms" 
              defaultValue={editingInterlock?.scope} 
            />
          </FormField>

          <FormField label="Condition" required description="When the interlock triggers">
            <Textarea 
              rows={2}
              placeholder="e.g., Level < 50L, EC > 4.0"
              defaultValue={editingInterlock?.condition} 
            />
          </FormField>

          <FormField label="Action" required description="What happens when triggered">
            <Textarea 
              rows={2}
              placeholder="e.g., Stop irrigation, alert"
              defaultValue={editingInterlock?.action} 
            />
          </FormField>

          <div className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
            <div>
              <div className="text-sm font-medium text-foreground">Enable Interlock</div>
              <div className="text-xs text-muted-foreground">
                Disabled interlocks will not trigger safety actions
              </div>
            </div>
            <Switch checked={editingInterlock?.enabled ?? true} onChange={() => {}} />
          </div>
        </div>
      </AdminModal>
    </AdminSection>
  );
}

