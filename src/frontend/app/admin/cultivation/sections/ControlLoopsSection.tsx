'use client';

import React, { useState } from 'react';
import {
  GitBranch,
  Plus,
  Edit2,
  Settings,
  AlertCircle,
  CheckCircle,
  Clock,
  TrendingUp,
  Flag,
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

// Mock data for control loops
const MOCK_CONTROL_LOOPS = [
  {
    id: '1',
    name: 'EC Control - F1',
    type: 'ec',
    scope: 'Flower Room F1',
    mode: 'shadow',
    shadowDays: 12,
    medianDelta: 3.2,
    targetDelta: 5.0,
    promotionReady: false,
    enabled: true,
  },
  {
    id: '2',
    name: 'pH Control - F1',
    type: 'ph',
    scope: 'Flower Room F1',
    mode: 'shadow',
    shadowDays: 12,
    medianDelta: 2.8,
    targetDelta: 5.0,
    promotionReady: false,
    enabled: true,
  },
  {
    id: '3',
    name: 'EC Control - V1',
    type: 'ec',
    scope: 'Veg Room V1',
    mode: 'enabled',
    shadowDays: 14,
    medianDelta: 2.1,
    targetDelta: 5.0,
    promotionReady: true,
    enabled: true,
  },
  {
    id: '4',
    name: 'pH Control - V1',
    type: 'ph',
    scope: 'Veg Room V1',
    mode: 'disabled',
    shadowDays: 0,
    medianDelta: null,
    targetDelta: 5.0,
    promotionReady: false,
    enabled: false,
  },
];

const CONTROL_TYPES = [
  { value: 'ec', label: 'EC Control' },
  { value: 'ph', label: 'pH Control' },
];

const MODES = [
  { value: 'disabled', label: 'Disabled' },
  { value: 'shadow', label: 'Shadow Mode (Observe only)' },
  { value: 'staged', label: 'Staged (A/B Testing)' },
  { value: 'enabled', label: 'Enabled (Active control)' },
];

// Check if feature flag is enabled (mock)
const CLOSED_LOOP_ENABLED = false; // This would come from feature flag

export function ControlLoopsSection() {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingLoop, setEditingLoop] = useState<typeof MOCK_CONTROL_LOOPS[0] | null>(null);

  const handleEdit = (loop: typeof MOCK_CONTROL_LOOPS[0]) => {
    setEditingLoop(loop);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    setEditingLoop(null);
    setIsModalOpen(true);
  };

  const getModeStatus = (mode: string) => {
    switch (mode) {
      case 'enabled':
        return 'active';
      case 'shadow':
        return 'pending';
      case 'staged':
        return 'warning';
      default:
        return 'inactive';
    }
  };

  const columns = [
    {
      key: 'name',
      header: 'Control Loop',
      sortable: true,
      render: (item: typeof MOCK_CONTROL_LOOPS[0]) => (
        <div>
          <div className="font-medium text-foreground">{item.name}</div>
          <div className="text-xs text-muted-foreground">{item.scope}</div>
        </div>
      ),
    },
    {
      key: 'type',
      header: 'Type',
      render: (item: typeof MOCK_CONTROL_LOOPS[0]) => (
        <span className={`text-xs px-2 py-0.5 rounded ${
          item.type === 'ec' ? 'bg-emerald-500/10 text-emerald-400' : 'bg-violet-500/10 text-violet-400'
        }`}>
          {item.type.toUpperCase()}
        </span>
      ),
    },
    {
      key: 'mode',
      header: 'Mode',
      render: (item: typeof MOCK_CONTROL_LOOPS[0]) => (
        <StatusBadge
          status={getModeStatus(item.mode)}
          label={item.mode.charAt(0).toUpperCase() + item.mode.slice(1)}
        />
      ),
    },
    {
      key: 'shadow',
      header: 'Shadow Progress',
      render: (item: typeof MOCK_CONTROL_LOOPS[0]) => (
        <div className="flex items-center gap-2">
          <div className="w-16 h-2 bg-white/10 rounded-full overflow-hidden">
            <div 
              className={`h-full rounded-full ${
                item.shadowDays >= 14 ? 'bg-emerald-500' : 'bg-amber-500'
              }`}
              style={{ width: `${Math.min((item.shadowDays / 14) * 100, 100)}%` }}
            />
          </div>
          <span className="text-xs text-muted-foreground">
            {item.shadowDays}/14 days
          </span>
        </div>
      ),
    },
    {
      key: 'delta',
      header: 'Median Δ',
      render: (item: typeof MOCK_CONTROL_LOOPS[0]) => (
        item.medianDelta !== null ? (
          <span className={`text-sm ${
            item.medianDelta <= item.targetDelta ? 'text-emerald-400' : 'text-amber-400'
          }`}>
            {item.medianDelta.toFixed(1)}%
            <span className="text-muted-foreground text-xs ml-1">
              (≤{item.targetDelta}%)
            </span>
          </span>
        ) : (
          <span className="text-muted-foreground">—</span>
        )
      ),
    },
    {
      key: 'promotion',
      header: 'Promotion',
      render: (item: typeof MOCK_CONTROL_LOOPS[0]) => (
        item.promotionReady ? (
          <div className="flex items-center gap-1.5 text-emerald-400">
            <CheckCircle className="w-4 h-4" />
            <span className="text-xs">Ready</span>
          </div>
        ) : (
          <div className="flex items-center gap-1.5 text-muted-foreground">
            <Clock className="w-4 h-4" />
            <span className="text-xs">Not ready</span>
          </div>
        )
      ),
    },
    {
      key: 'actions',
      header: '',
      width: '80px',
      render: (item: typeof MOCK_CONTROL_LOOPS[0]) => (
        <TableActions>
          <TableActionButton onClick={() => handleEdit(item)}>
            <Settings className="w-4 h-4" />
          </TableActionButton>
        </TableActions>
      ),
    },
  ];

  if (!CLOSED_LOOP_ENABLED) {
    return (
      <AdminSection
        title="Control Loops (Closed-Loop EC/pH)"
        description="Configure closed-loop control with shadow mode and promotion gates"
      >
        <AdminCard
          title="Control Loop Configuration"
          icon={GitBranch}
        >
          <div className="flex flex-col items-center justify-center py-12 text-center">
            <div className="w-16 h-16 rounded-full bg-amber-500/10 flex items-center justify-center mb-4">
              <Flag className="w-8 h-8 text-amber-400" />
            </div>
            <h3 className="text-lg font-semibold text-foreground mb-2">
              Feature Not Enabled
            </h3>
            <p className="text-sm text-muted-foreground max-w-md mb-4">
              Closed-loop EC/pH control is available in Phase 2. Enable the 
              <code className="mx-1 px-1.5 py-0.5 bg-white/10 rounded text-xs">
                closed_loop_ecph_enabled
              </code> 
              feature flag to access this functionality.
            </p>
            <Button variant="secondary" onClick={() => window.location.href = '/admin/feature-flags'}>
              Go to Feature Flags
            </Button>
          </div>
        </AdminCard>
      </AdminSection>
    );
  }

  return (
    <AdminSection
      title="Control Loops (Closed-Loop EC/pH)"
      description="Configure closed-loop control with shadow mode and promotion gates"
    >
      <AdminCard
        title="Control Loop Configuration"
        icon={GitBranch}
        actions={
          <Button onClick={handleCreate}>
            <Plus className="w-4 h-4" />
            Add Control Loop
          </Button>
        }
      >
        <div className="flex items-center gap-2 p-3 bg-cyan-500/10 border border-cyan-500/20 rounded-lg mb-4">
          <TrendingUp className="w-4 h-4 text-cyan-400 flex-shrink-0" />
          <p className="text-xs text-cyan-200">
            <strong>Promotion Criteria:</strong> Shadow mode ≥14 days with median correction delta ≤5%. 
            Interlocks must be validated. Emergency stop drill must be passed.
          </p>
        </div>
        <AdminTable
          columns={columns}
          data={MOCK_CONTROL_LOOPS}
          keyField="id"
          onRowClick={handleEdit}
          emptyMessage="No control loops configured"
        />
      </AdminCard>

      {/* Create/Edit Modal */}
      <AdminModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingLoop ? 'Configure Control Loop' : 'Create Control Loop'}
        description="Set up closed-loop control with shadow mode"
        size="lg"
        footer={
          <>
            <Button variant="ghost" onClick={() => setIsModalOpen(false)}>
              Cancel
            </Button>
            <Button onClick={() => setIsModalOpen(false)}>
              {editingLoop ? 'Save Changes' : 'Create Loop'}
            </Button>
          </>
        }
      >
        <div className="space-y-6">
          <FormField label="Control Loop Name" required>
            <Input 
              placeholder="e.g., EC Control - F1" 
              defaultValue={editingLoop?.name} 
            />
          </FormField>

          <div className="grid grid-cols-2 gap-4">
            <FormField label="Control Type" required>
              <Select options={CONTROL_TYPES} defaultValue={editingLoop?.type || 'ec'} />
            </FormField>
            <FormField label="Mode" required>
              <Select options={MODES} defaultValue={editingLoop?.mode || 'shadow'} />
            </FormField>
          </div>

          <FormField label="Scope" required description="Which area this control loop covers">
            <Input 
              placeholder="e.g., Flower Room F1" 
              defaultValue={editingLoop?.scope} 
            />
          </FormField>

          <FormField label="Target Correction Delta (%)" description="Max acceptable correction percentage">
            <Input 
              type="number" 
              step="0.1" 
              placeholder="5.0" 
              defaultValue={editingLoop?.targetDelta} 
            />
          </FormField>

          <div className="p-4 bg-white/5 rounded-lg space-y-3">
            <h4 className="text-sm font-medium text-foreground">Promotion Checklist</h4>
            <div className="space-y-2 text-xs">
              <div className="flex items-center gap-2">
                <CheckCircle className="w-4 h-4 text-emerald-400" />
                <span>Shadow mode ≥14 days</span>
              </div>
              <div className="flex items-center gap-2">
                <CheckCircle className="w-4 h-4 text-emerald-400" />
                <span>Median correction delta ≤5%</span>
              </div>
              <div className="flex items-center gap-2">
                <Clock className="w-4 h-4 text-muted-foreground" />
                <span>Interlocks validated</span>
              </div>
              <div className="flex items-center gap-2">
                <Clock className="w-4 h-4 text-muted-foreground" />
                <span>Emergency stop drill passed</span>
              </div>
            </div>
          </div>

          <div className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
            <div>
              <div className="text-sm font-medium text-foreground">Enable Control Loop</div>
              <div className="text-xs text-muted-foreground">
                Disabled loops will not collect shadow data
              </div>
            </div>
            <Switch checked={editingLoop?.enabled ?? true} onChange={() => {}} />
          </div>
        </div>
      </AdminModal>
    </AdminSection>
  );
}

