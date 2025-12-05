'use client';

import React, { useState } from 'react';
import {
  AlertTriangle,
  Plus,
  Edit2,
  Trash2,
  Bell,
  Mail,
  MessageSquare,
  Smartphone,
  Copy,
} from 'lucide-react';
import {
  AdminCard,
  AdminSection,
  AdminGrid,
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
  Checkbox,
} from '@/components/admin';

// Mock data for alert rules
const MOCK_ALERT_RULES = [
  {
    id: '1',
    name: 'High Temperature Alert',
    ruleType: 'threshold_above',
    scope: 'Flower Room F1',
    metric: 'Temperature',
    thresholdWarnHigh: '82°F',
    thresholdCriticalHigh: '88°F',
    thresholdWarnLow: '68°F',
    thresholdCriticalLow: '62°F',
    evaluationWindow: 5,
    dwellSeconds: 60,
    hysteresis: 2,
    channels: ['slack', 'email'],
    severity: 'critical',
    enabled: true,
  },
  {
    id: '2',
    name: 'VPD Out of Range',
    ruleType: 'threshold_range',
    scope: 'All Rooms',
    metric: 'VPD',
    thresholdWarnHigh: '1.4 kPa',
    thresholdCriticalHigh: '1.6 kPa',
    thresholdWarnLow: '0.6 kPa',
    thresholdCriticalLow: '0.4 kPa',
    evaluationWindow: 10,
    dwellSeconds: 120,
    hysteresis: 5,
    channels: ['slack'],
    severity: 'warning',
    enabled: true,
  },
  {
    id: '3',
    name: 'CO2 Deviation',
    ruleType: 'deviation',
    scope: 'Zone A',
    metric: 'CO2',
    thresholdWarnHigh: '±15%',
    thresholdCriticalHigh: '±25%',
    thresholdWarnLow: '-',
    thresholdCriticalLow: '-',
    evaluationWindow: 15,
    dwellSeconds: 180,
    hysteresis: 3,
    channels: ['slack', 'email', 'sms'],
    severity: 'critical',
    enabled: true,
  },
  {
    id: '4',
    name: 'Sensor Offline',
    ruleType: 'missing_data',
    scope: 'All Sensors',
    metric: 'Any',
    thresholdWarnHigh: '5 min',
    thresholdCriticalHigh: '15 min',
    thresholdWarnLow: '-',
    thresholdCriticalLow: '-',
    evaluationWindow: 1,
    dwellSeconds: 0,
    hysteresis: 0,
    channels: ['slack', 'email'],
    severity: 'warning',
    enabled: true,
  },
  {
    id: '5',
    name: 'Rapid Temperature Change',
    ruleType: 'rate_of_change',
    scope: 'All Rooms',
    metric: 'Temperature',
    thresholdWarnHigh: '3°F/5min',
    thresholdCriticalHigh: '5°F/5min',
    thresholdWarnLow: '-',
    thresholdCriticalLow: '-',
    evaluationWindow: 5,
    dwellSeconds: 30,
    hysteresis: 0,
    channels: ['slack', 'sms'],
    severity: 'critical',
    enabled: false,
  },
];

const RULE_TYPES = [
  { value: 'threshold_above', label: 'Threshold Above' },
  { value: 'threshold_below', label: 'Threshold Below' },
  { value: 'threshold_range', label: 'Threshold Range' },
  { value: 'deviation', label: 'Deviation from Target' },
  { value: 'rate_of_change', label: 'Rate of Change' },
  { value: 'missing_data', label: 'Missing Data' },
  { value: 'quality_degraded', label: 'Quality Degraded' },
];

const METRICS = [
  { value: 'temperature', label: 'Temperature' },
  { value: 'humidity', label: 'Humidity' },
  { value: 'vpd', label: 'VPD' },
  { value: 'co2', label: 'CO₂' },
  { value: 'ppfd', label: 'PPFD' },
  { value: 'substrate_ec', label: 'Substrate EC' },
  { value: 'substrate_ph', label: 'Substrate pH' },
  { value: 'vwc', label: 'VWC' },
];

const SEVERITIES = [
  { value: 'info', label: 'Info' },
  { value: 'warning', label: 'Warning' },
  { value: 'critical', label: 'Critical' },
];

export function AlertThresholdsSection() {
  const [searchQuery, setSearchQuery] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingRule, setEditingRule] = useState<typeof MOCK_ALERT_RULES[0] | null>(null);

  const filteredRules = MOCK_ALERT_RULES.filter(
    (rule) =>
      rule.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      rule.metric.toLowerCase().includes(searchQuery.toLowerCase()) ||
      rule.scope.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const handleEdit = (rule: typeof MOCK_ALERT_RULES[0]) => {
    setEditingRule(rule);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    setEditingRule(null);
    setIsModalOpen(true);
  };

  const getChannelIcons = (channels: string[]) => {
    return (
      <div className="flex items-center gap-1">
        {channels.includes('slack') && (
          <MessageSquare className="w-3.5 h-3.5 text-cyan-400" />
        )}
        {channels.includes('email') && (
          <Mail className="w-3.5 h-3.5 text-violet-400" />
        )}
        {channels.includes('sms') && (
          <Smartphone className="w-3.5 h-3.5 text-emerald-400" />
        )}
      </div>
    );
  };

  const columns = [
    {
      key: 'name',
      header: 'Rule Name',
      sortable: true,
      render: (item: typeof MOCK_ALERT_RULES[0]) => (
        <div>
          <div className="font-medium text-foreground">{item.name}</div>
          <div className="text-xs text-muted-foreground">{item.scope}</div>
        </div>
      ),
    },
    {
      key: 'ruleType',
      header: 'Type',
      render: (item: typeof MOCK_ALERT_RULES[0]) => (
        <span className="text-xs font-mono bg-white/5 px-2 py-0.5 rounded capitalize">
          {item.ruleType.replace(/_/g, ' ')}
        </span>
      ),
    },
    {
      key: 'metric',
      header: 'Metric',
    },
    {
      key: 'thresholds',
      header: 'Thresholds',
      render: (item: typeof MOCK_ALERT_RULES[0]) => (
        <div className="space-y-0.5 text-xs">
          <div className="flex items-center gap-2">
            <span className="w-12 text-amber-400">Warn:</span>
            <span>{item.thresholdWarnLow !== '-' && `${item.thresholdWarnLow} / `}{item.thresholdWarnHigh}</span>
          </div>
          <div className="flex items-center gap-2">
            <span className="w-12 text-rose-400">Crit:</span>
            <span>{item.thresholdCriticalLow !== '-' && `${item.thresholdCriticalLow} / `}{item.thresholdCriticalHigh}</span>
          </div>
        </div>
      ),
    },
    {
      key: 'timing',
      header: 'Timing',
      render: (item: typeof MOCK_ALERT_RULES[0]) => (
        <div className="text-xs text-muted-foreground">
          <div>Window: {item.evaluationWindow}m</div>
          <div>Dwell: {item.dwellSeconds}s</div>
        </div>
      ),
    },
    {
      key: 'channels',
      header: 'Notify',
      render: (item: typeof MOCK_ALERT_RULES[0]) => getChannelIcons(item.channels),
    },
    {
      key: 'severity',
      header: 'Severity',
      render: (item: typeof MOCK_ALERT_RULES[0]) => (
        <StatusBadge
          status={item.severity === 'critical' ? 'error' : item.severity === 'warning' ? 'warning' : 'active'}
          label={item.severity}
        />
      ),
    },
    {
      key: 'enabled',
      header: 'Status',
      render: (item: typeof MOCK_ALERT_RULES[0]) => (
        <StatusBadge status={item.enabled ? 'active' : 'inactive'} />
      ),
    },
    {
      key: 'actions',
      header: '',
      width: '100px',
      render: (item: typeof MOCK_ALERT_RULES[0]) => (
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
      title="Alert Thresholds"
      description="Configure alert rules with multi-level thresholds, evaluation windows, and notification channels"
    >
      <AdminCard
        title="Alert Rules"
        icon={AlertTriangle}
        actions={
          <div className="flex items-center gap-3">
            <TableSearch
              value={searchQuery}
              onChange={setSearchQuery}
              placeholder="Search rules..."
            />
            <Button onClick={handleCreate}>
              <Plus className="w-4 h-4" />
              Add Rule
            </Button>
          </div>
        }
      >
        <AdminTable
          columns={columns}
          data={filteredRules}
          keyField="id"
          onRowClick={handleEdit}
          emptyMessage="No alert rules configured"
        />
      </AdminCard>

      {/* Create/Edit Modal */}
      <AdminModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingRule ? 'Edit Alert Rule' : 'Create Alert Rule'}
        description="Configure thresholds, timing, and notification settings"
        size="xl"
        footer={
          <>
            <Button variant="ghost" onClick={() => setIsModalOpen(false)}>
              Cancel
            </Button>
            <Button onClick={() => setIsModalOpen(false)}>
              {editingRule ? 'Save Changes' : 'Create Rule'}
            </Button>
          </>
        }
      >
        <div className="space-y-6">
          <FormField label="Rule Name" required>
            <Input 
              placeholder="e.g., High Temperature Alert" 
              defaultValue={editingRule?.name} 
            />
          </FormField>

          <div className="grid grid-cols-3 gap-4">
            <FormField label="Rule Type" required>
              <Select options={RULE_TYPES} defaultValue={editingRule?.ruleType || 'threshold_range'} />
            </FormField>
            <FormField label="Metric" required>
              <Select options={METRICS} defaultValue="temperature" />
            </FormField>
            <FormField label="Severity" required>
              <Select options={SEVERITIES} defaultValue={editingRule?.severity || 'warning'} />
            </FormField>
          </div>

          <FormField label="Scope" required description="Which sensors/areas this rule applies to">
            <Input placeholder="e.g., All Rooms, Zone A, specific sensor" defaultValue={editingRule?.scope} />
          </FormField>

          <div className="p-4 bg-white/5 rounded-lg space-y-4">
            <h4 className="text-sm font-medium text-foreground">Warning Thresholds</h4>
            <div className="grid grid-cols-2 gap-4">
              <FormField label="Low Threshold">
                <Input placeholder="e.g., 68" />
              </FormField>
              <FormField label="High Threshold">
                <Input placeholder="e.g., 82" />
              </FormField>
            </div>
          </div>

          <div className="p-4 bg-rose-500/5 border border-rose-500/10 rounded-lg space-y-4">
            <h4 className="text-sm font-medium text-rose-400">Critical Thresholds</h4>
            <div className="grid grid-cols-2 gap-4">
              <FormField label="Low Threshold">
                <Input placeholder="e.g., 62" />
              </FormField>
              <FormField label="High Threshold">
                <Input placeholder="e.g., 88" />
              </FormField>
            </div>
          </div>

          <div className="grid grid-cols-3 gap-4">
            <FormField label="Evaluation Window (min)" description="Time window for rule evaluation">
              <Input type="number" placeholder="5" defaultValue={editingRule?.evaluationWindow} />
            </FormField>
            <FormField label="Dwell Time (sec)" description="Time before alert fires">
              <Input type="number" placeholder="60" defaultValue={editingRule?.dwellSeconds} />
            </FormField>
            <FormField label="Hysteresis (%)" description="Prevents alert flapping">
              <Input type="number" placeholder="2" defaultValue={editingRule?.hysteresis} />
            </FormField>
          </div>

          <div className="space-y-3">
            <h4 className="text-sm font-medium text-foreground">Notification Channels</h4>
            <div className="flex gap-6">
              <Checkbox 
                checked={editingRule?.channels.includes('slack') ?? true} 
                onChange={() => {}} 
                label="Slack" 
              />
              <Checkbox 
                checked={editingRule?.channels.includes('email') ?? true} 
                onChange={() => {}} 
                label="Email" 
              />
              <Checkbox 
                checked={editingRule?.channels.includes('sms') ?? false} 
                onChange={() => {}} 
                label="SMS (Critical only)" 
              />
            </div>
          </div>

          <div className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
            <div>
              <div className="text-sm font-medium text-foreground">Enable Rule</div>
              <div className="text-xs text-muted-foreground">
                Disabled rules will not trigger alerts
              </div>
            </div>
            <Switch checked={editingRule?.enabled ?? true} onChange={() => {}} />
          </div>
        </div>
      </AdminModal>
    </AdminSection>
  );
}

