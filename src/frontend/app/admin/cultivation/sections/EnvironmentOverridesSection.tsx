'use client';

import React, { useState } from 'react';
import {
  Calendar,
  Plus,
  Edit2,
  Trash2,
  Clock,
  User,
  AlertCircle,
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
  Textarea,
} from '@/components/admin';

// Mock data for environment overrides
const MOCK_OVERRIDES = [
  {
    id: '1',
    scope: 'Flower Room F1',
    scopeType: 'room',
    metric: 'Temperature',
    originalTarget: '78°F',
    overrideValue: '75°F',
    reason: 'Heat stress prevention during heat wave',
    owner: 'Brandon B.',
    startsAt: '2025-11-25T08:00:00',
    endsAt: '2025-11-28T20:00:00',
    status: 'active',
  },
  {
    id: '2',
    scope: 'Zone A',
    scopeType: 'zone',
    metric: 'CO2',
    originalTarget: '1200 ppm',
    overrideValue: '1400 ppm',
    reason: 'Increased CO2 for flowering boost',
    owner: 'Sarah M.',
    startsAt: '2025-11-20T00:00:00',
    endsAt: '2025-12-01T23:59:59',
    status: 'active',
  },
  {
    id: '3',
    scope: 'Veg Room V2',
    scopeType: 'room',
    metric: 'Humidity',
    originalTarget: '65%',
    overrideValue: '55%',
    reason: 'Powdery mildew prevention',
    owner: 'Mike T.',
    startsAt: '2025-11-18T06:00:00',
    endsAt: '2025-11-24T18:00:00',
    status: 'expired',
  },
  {
    id: '4',
    scope: 'All Rooms',
    scopeType: 'site',
    metric: 'PPFD',
    originalTarget: '600 μmol',
    overrideValue: '450 μmol',
    reason: 'Scheduled maintenance on lighting system',
    owner: 'Brandon B.',
    startsAt: '2025-11-30T10:00:00',
    endsAt: '2025-11-30T14:00:00',
    status: 'pending',
  },
];

const SCOPE_TYPES = [
  { value: 'site', label: 'Entire Site' },
  { value: 'room', label: 'Room' },
  { value: 'zone', label: 'Zone' },
  { value: 'rack', label: 'Rack' },
];

const METRICS = [
  { value: 'temperature', label: 'Temperature' },
  { value: 'humidity', label: 'Humidity' },
  { value: 'vpd', label: 'VPD' },
  { value: 'co2', label: 'CO₂' },
  { value: 'ppfd', label: 'PPFD' },
  { value: 'dli', label: 'DLI' },
];

export function EnvironmentOverridesSection() {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingOverride, setEditingOverride] = useState<typeof MOCK_OVERRIDES[0] | null>(null);

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const getStatus = (override: typeof MOCK_OVERRIDES[0]) => {
    const now = new Date();
    const start = new Date(override.startsAt);
    const end = new Date(override.endsAt);
    
    if (now < start) return 'pending';
    if (now > end) return 'inactive';
    return 'active';
  };

  const handleEdit = (override: typeof MOCK_OVERRIDES[0]) => {
    setEditingOverride(override);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    setEditingOverride(null);
    setIsModalOpen(true);
  };

  const columns = [
    {
      key: 'scope',
      header: 'Scope',
      render: (item: typeof MOCK_OVERRIDES[0]) => (
        <div>
          <div className="font-medium text-foreground">{item.scope}</div>
          <div className="text-xs text-muted-foreground capitalize">{item.scopeType}</div>
        </div>
      ),
    },
    {
      key: 'metric',
      header: 'Metric',
    },
    {
      key: 'override',
      header: 'Override',
      render: (item: typeof MOCK_OVERRIDES[0]) => (
        <div className="flex items-center gap-2">
          <span className="text-muted-foreground line-through text-xs">
            {item.originalTarget}
          </span>
          <span className="text-amber-400 font-medium">{item.overrideValue}</span>
        </div>
      ),
    },
    {
      key: 'duration',
      header: 'Duration',
      render: (item: typeof MOCK_OVERRIDES[0]) => (
        <div className="flex items-center gap-1.5 text-xs">
          <Clock className="w-3.5 h-3.5 text-muted-foreground" />
          <span>
            {formatDate(item.startsAt)} → {formatDate(item.endsAt)}
          </span>
        </div>
      ),
    },
    {
      key: 'owner',
      header: 'Owner',
      render: (item: typeof MOCK_OVERRIDES[0]) => (
        <div className="flex items-center gap-1.5">
          <User className="w-3.5 h-3.5 text-muted-foreground" />
          <span className="text-sm">{item.owner}</span>
        </div>
      ),
    },
    {
      key: 'status',
      header: 'Status',
      render: (item: typeof MOCK_OVERRIDES[0]) => {
        const status = getStatus(item);
        return (
          <StatusBadge
            status={status === 'active' ? 'warning' : status === 'pending' ? 'pending' : 'inactive'}
            label={status === 'active' ? 'Active' : status === 'pending' ? 'Scheduled' : 'Expired'}
          />
        );
      },
    },
    {
      key: 'actions',
      header: '',
      width: '80px',
      render: (item: typeof MOCK_OVERRIDES[0]) => (
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
      title="Environment Overrides"
      description="Time-boxed temporary overrides to environment targets with reason tracking and audit trail"
    >
      <AdminCard
        title="Active & Scheduled Overrides"
        icon={Calendar}
        actions={
          <Button onClick={handleCreate}>
            <Plus className="w-4 h-4" />
            Create Override
          </Button>
        }
      >
        <div className="flex items-center gap-2 p-3 bg-amber-500/10 border border-amber-500/20 rounded-lg mb-4">
          <AlertCircle className="w-4 h-4 text-amber-400 flex-shrink-0" />
          <p className="text-xs text-amber-200">
            <strong>Note:</strong> Overrides temporarily replace target values. They do not 
            modify the permanent recipe. All overrides are logged for audit purposes.
          </p>
        </div>
        <AdminTable
          columns={columns}
          data={MOCK_OVERRIDES}
          keyField="id"
          onRowClick={handleEdit}
          emptyMessage="No active or scheduled overrides"
        />
      </AdminCard>

      {/* Create/Edit Modal */}
      <AdminModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingOverride ? 'Edit Override' : 'Create Override'}
        description="Create a time-boxed temporary override for an environment target"
        size="lg"
        footer={
          <>
            <Button variant="ghost" onClick={() => setIsModalOpen(false)}>
              Cancel
            </Button>
            <Button onClick={() => setIsModalOpen(false)}>
              {editingOverride ? 'Save Changes' : 'Create Override'}
            </Button>
          </>
        }
      >
        <div className="space-y-6">
          <div className="grid grid-cols-2 gap-4">
            <FormField label="Scope Type" required>
              <Select
                options={SCOPE_TYPES}
                defaultValue={editingOverride?.scopeType || 'room'}
              />
            </FormField>
            <FormField label="Scope" required>
              <Input placeholder="e.g., Flower Room F1" defaultValue={editingOverride?.scope} />
            </FormField>
          </div>

          <FormField label="Metric" required>
            <Select options={METRICS} defaultValue="temperature" />
          </FormField>

          <FormField label="Override Value" required description="The temporary target value">
            <Input 
              placeholder="e.g., 75" 
              defaultValue={editingOverride?.overrideValue?.replace(/[^0-9.]/g, '')} 
            />
          </FormField>

          <div className="grid grid-cols-2 gap-4">
            <FormField label="Start Time" required>
              <Input 
                type="datetime-local" 
                defaultValue={editingOverride?.startsAt?.slice(0, 16)} 
              />
            </FormField>
            <FormField label="End Time" required>
              <Input 
                type="datetime-local" 
                defaultValue={editingOverride?.endsAt?.slice(0, 16)} 
              />
            </FormField>
          </div>

          <FormField label="Reason" required description="Explain why this override is needed">
            <Textarea
              rows={3}
              placeholder="e.g., Heat stress prevention during heat wave"
              defaultValue={editingOverride?.reason}
            />
          </FormField>
        </div>
      </AdminModal>
    </AdminSection>
  );
}

