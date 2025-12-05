'use client';

import React, { useState } from 'react';
import {
  Target,
  Plus,
  Edit2,
  Trash2,
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
  Button,
  AdminModal,
  FormField,
  Input,
  Select,
  Switch,
} from '@/components/admin';

// Mock data for feed targets
const MOCK_TARGETS = [
  {
    id: '1',
    recipeVersion: 'Flower Week 4-6 v2.1',
    targetEcDay: '2.4',
    targetEcNight: '2.2',
    toleranceEc: '0.2',
    targetPhDay: '5.9',
    targetPhNight: '6.0',
    tolerancePh: '0.2',
    targetTempC: '22',
    enabled: true,
  },
  {
    id: '2',
    recipeVersion: 'Flower Week 7-9 v2.0',
    targetEcDay: '2.0',
    targetEcNight: '1.8',
    toleranceEc: '0.2',
    targetPhDay: '6.0',
    targetPhNight: '6.1',
    tolerancePh: '0.2',
    targetTempC: '21',
    enabled: true,
  },
  {
    id: '3',
    recipeVersion: 'Veg Standard v1.3',
    targetEcDay: '1.8',
    targetEcNight: '1.6',
    toleranceEc: '0.3',
    targetPhDay: '5.8',
    targetPhNight: '5.9',
    tolerancePh: '0.3',
    targetTempC: '23',
    enabled: true,
  },
];

const RECIPE_VERSIONS = [
  { value: 'flower-4-6-v2.1', label: 'Flower Week 4-6 v2.1' },
  { value: 'flower-7-9-v2.0', label: 'Flower Week 7-9 v2.0' },
  { value: 'veg-std-v1.3', label: 'Veg Standard v1.3' },
  { value: 'flush-v1.0', label: 'Flush Only v1.0' },
];

export function FeedTargetsSection() {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingTarget, setEditingTarget] = useState<typeof MOCK_TARGETS[0] | null>(null);

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
      key: 'recipeVersion',
      header: 'Recipe Version',
      sortable: true,
      render: (item: typeof MOCK_TARGETS[0]) => (
        <span className="text-cyan-400 font-medium">{item.recipeVersion}</span>
      ),
    },
    {
      key: 'ec',
      header: 'EC Target (mS/cm)',
      render: (item: typeof MOCK_TARGETS[0]) => (
        <div className="space-y-0.5 text-sm">
          <div className="flex items-center gap-2">
            <Sun className="w-3.5 h-3.5 text-amber-400" />
            <span>{item.targetEcDay} ±{item.toleranceEc}</span>
          </div>
          <div className="flex items-center gap-2">
            <Moon className="w-3.5 h-3.5 text-blue-400" />
            <span>{item.targetEcNight} ±{item.toleranceEc}</span>
          </div>
        </div>
      ),
    },
    {
      key: 'ph',
      header: 'pH Target',
      render: (item: typeof MOCK_TARGETS[0]) => (
        <div className="space-y-0.5 text-sm">
          <div className="flex items-center gap-2">
            <Sun className="w-3.5 h-3.5 text-amber-400" />
            <span>{item.targetPhDay} ±{item.tolerancePh}</span>
          </div>
          <div className="flex items-center gap-2">
            <Moon className="w-3.5 h-3.5 text-blue-400" />
            <span>{item.targetPhNight} ±{item.tolerancePh}</span>
          </div>
        </div>
      ),
    },
    {
      key: 'temp',
      header: 'Solution Temp',
      render: (item: typeof MOCK_TARGETS[0]) => (
        <span>{item.targetTempC}°C</span>
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
      width: '80px',
      render: (item: typeof MOCK_TARGETS[0]) => (
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
      title="Feed Targets"
      description="Configure EC/pH targets with tolerances and day/night variants for each recipe"
    >
      <AdminCard
        title="Target Configuration"
        icon={Target}
        actions={
          <Button onClick={handleCreate}>
            <Plus className="w-4 h-4" />
            Add Target
          </Button>
        }
      >
        <AdminTable
          columns={columns}
          data={MOCK_TARGETS}
          keyField="id"
          onRowClick={handleEdit}
          emptyMessage="No feed targets configured"
        />
      </AdminCard>

      {/* Create/Edit Modal */}
      <AdminModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingTarget ? 'Edit Feed Target' : 'Create Feed Target'}
        description="Configure EC/pH targets with tolerances"
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
          <FormField label="Recipe Version" required>
            <Select options={RECIPE_VERSIONS} defaultValue="flower-4-6-v2.1" />
          </FormField>

          <div className="p-4 bg-white/5 rounded-lg space-y-4">
            <h4 className="text-sm font-medium text-foreground">EC Targets (mS/cm)</h4>
            <div className="grid grid-cols-3 gap-4">
              <FormField label="Day Target" required>
                <Input type="number" step="0.1" placeholder="2.4" defaultValue={editingTarget?.targetEcDay} />
              </FormField>
              <FormField label="Night Target" required>
                <Input type="number" step="0.1" placeholder="2.2" defaultValue={editingTarget?.targetEcNight} />
              </FormField>
              <FormField label="Tolerance (±)" required>
                <Input type="number" step="0.1" placeholder="0.2" defaultValue={editingTarget?.toleranceEc} />
              </FormField>
            </div>
          </div>

          <div className="p-4 bg-white/5 rounded-lg space-y-4">
            <h4 className="text-sm font-medium text-foreground">pH Targets</h4>
            <div className="grid grid-cols-3 gap-4">
              <FormField label="Day Target" required>
                <Input type="number" step="0.1" placeholder="5.9" defaultValue={editingTarget?.targetPhDay} />
              </FormField>
              <FormField label="Night Target" required>
                <Input type="number" step="0.1" placeholder="6.0" defaultValue={editingTarget?.targetPhNight} />
              </FormField>
              <FormField label="Tolerance (±)" required>
                <Input type="number" step="0.1" placeholder="0.2" defaultValue={editingTarget?.tolerancePh} />
              </FormField>
            </div>
          </div>

          <FormField label="Solution Temperature (°C)" description="Optional target temperature">
            <Input type="number" placeholder="22" defaultValue={editingTarget?.targetTempC} />
          </FormField>

          <div className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
            <div>
              <div className="text-sm font-medium text-foreground">Enable Target</div>
              <div className="text-xs text-muted-foreground">
                Active targets will be used for closed-loop control
              </div>
            </div>
            <Switch checked={editingTarget?.enabled ?? true} onChange={() => {}} />
          </div>
        </div>
      </AdminModal>
    </AdminSection>
  );
}

