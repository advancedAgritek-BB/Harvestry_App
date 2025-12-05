'use client';

import React, { useState } from 'react';
import {
  Shield,
  Key,
  RefreshCw,
  FileCheck,
  Scale,
  AlertTriangle,
  Plus,
  Edit2,
  CheckCircle,
  XCircle,
  Clock,
  ExternalLink,
} from 'lucide-react';
import {
  AdminCard,
  AdminSection,
  AdminGrid,
  AdminTabs,
  TabPanel,
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

const COMPLIANCE_TABS = [
  { id: 'credentials', label: 'Integration Credentials', icon: Key },
  { id: 'sync', label: 'Sync Configuration', icon: RefreshCw },
  { id: 'coa', label: 'COA Policies', icon: FileCheck },
  { id: 'jurisdiction', label: 'Jurisdiction Rules', icon: Scale },
  { id: 'destruction', label: 'Destruction Settings', icon: AlertTriangle },
];

// Mock data
const MOCK_CREDENTIALS = [
  { id: '1', provider: 'METRC', site: 'Evergreen', apiKey: '••••••••••••abc123', status: 'connected', lastSync: '2 min ago' },
  { id: '2', provider: 'METRC', site: 'Oakdale', apiKey: '••••••••••••def456', status: 'connected', lastSync: '5 min ago' },
  { id: '3', provider: 'BioTrack', site: 'Evergreen', apiKey: '••••••••••••ghi789', status: 'error', lastSync: '1 hour ago' },
];

const MOCK_SYNC_CONFIG = [
  { id: '1', integration: 'METRC', mode: 'Real-time', retryPolicy: '3 retries, exp backoff', dlqEnabled: true, reconcileTime: '02:00' },
  { id: '2', integration: 'BioTrack', mode: 'Scheduled', retryPolicy: '5 retries, linear backoff', dlqEnabled: true, reconcileTime: '03:00' },
];

const MOCK_COA_POLICIES = [
  { id: '1', site: 'Evergreen', holdOnFail: true, autoPass: false, requiredTests: ['Potency', 'Pesticides', 'Microbials'], gracePeriod: 48 },
  { id: '2', site: 'Oakdale', holdOnFail: true, autoPass: true, requiredTests: ['Potency', 'Pesticides'], gracePeriod: 72 },
];

const MOCK_JURISDICTION = [
  { id: '1', state: 'Colorado', escortRequired: false, timingWindow: '72 hours', labelPhrases: ['THC Warning', 'Keep Out of Reach'], lastUpdated: '2025-11-01' },
  { id: '2', state: 'California', escortRequired: true, timingWindow: '48 hours', labelPhrases: ['Prop 65 Warning', 'THC Warning', 'Keep Out of Reach'], lastUpdated: '2025-10-15' },
];

const MOCK_DESTRUCTION = [
  { id: '1', site: 'Evergreen', twoPersonRequired: true, reasonCodes: ['Contamination', 'Quality Failure', 'Expired', 'Regulatory'], photoRequired: true },
  { id: '2', site: 'Oakdale', twoPersonRequired: false, reasonCodes: ['Contamination', 'Quality Failure', 'Expired'], photoRequired: false },
];

const PROVIDERS = [
  { value: 'metrc', label: 'METRC' },
  { value: 'biotrack', label: 'BioTrack' },
];

const SYNC_MODES = [
  { value: 'realtime', label: 'Real-time' },
  { value: 'scheduled', label: 'Scheduled' },
  { value: 'manual', label: 'Manual Only' },
];

export default function ComplianceAdminPage() {
  const [activeTab, setActiveTab] = useState('credentials');
  const [isModalOpen, setIsModalOpen] = useState(false);

  const credentialColumns = [
    {
      key: 'provider',
      header: 'Provider',
      render: (item: typeof MOCK_CREDENTIALS[0]) => (
        <span className={`text-sm font-medium ${item.provider === 'METRC' ? 'text-emerald-400' : 'text-cyan-400'}`}>
          {item.provider}
        </span>
      ),
    },
    {
      key: 'site',
      header: 'Site',
    },
    {
      key: 'apiKey',
      header: 'API Key',
      render: (item: typeof MOCK_CREDENTIALS[0]) => (
        <span className="font-mono text-xs text-muted-foreground">{item.apiKey}</span>
      ),
    },
    {
      key: 'status',
      header: 'Status',
      render: (item: typeof MOCK_CREDENTIALS[0]) => (
        <div className="flex items-center gap-2">
          {item.status === 'connected' ? (
            <CheckCircle className="w-4 h-4 text-emerald-400" />
          ) : (
            <XCircle className="w-4 h-4 text-rose-400" />
          )}
          <span className={item.status === 'connected' ? 'text-emerald-400' : 'text-rose-400'}>
            {item.status}
          </span>
        </div>
      ),
    },
    {
      key: 'lastSync',
      header: 'Last Sync',
      render: (item: typeof MOCK_CREDENTIALS[0]) => (
        <span className="text-xs text-muted-foreground">{item.lastSync}</span>
      ),
    },
    {
      key: 'actions',
      header: '',
      width: '120px',
      render: () => (
        <TableActions>
          <Button size="sm" variant="secondary">Test Connection</Button>
        </TableActions>
      ),
    },
  ];

  const jurisdictionColumns = [
    {
      key: 'state',
      header: 'State',
      render: (item: typeof MOCK_JURISDICTION[0]) => (
        <span className="font-medium text-foreground">{item.state}</span>
      ),
    },
    {
      key: 'escortRequired',
      header: 'Escort Required',
      render: (item: typeof MOCK_JURISDICTION[0]) => (
        <span className={item.escortRequired ? 'text-amber-400' : 'text-muted-foreground'}>
          {item.escortRequired ? 'Yes' : 'No'}
        </span>
      ),
    },
    {
      key: 'timingWindow',
      header: 'Timing Window',
    },
    {
      key: 'labelPhrases',
      header: 'Label Requirements',
      render: (item: typeof MOCK_JURISDICTION[0]) => (
        <span className="text-xs text-muted-foreground">{item.labelPhrases.length} phrases</span>
      ),
    },
    {
      key: 'lastUpdated',
      header: 'Last Updated',
      render: (item: typeof MOCK_JURISDICTION[0]) => (
        <span className="text-xs text-muted-foreground">{item.lastUpdated}</span>
      ),
    },
    {
      key: 'actions',
      header: '',
      width: '80px',
      render: () => (
        <TableActions>
          <TableActionButton onClick={() => setIsModalOpen(true)}><Edit2 className="w-4 h-4" /></TableActionButton>
        </TableActions>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <AdminTabs tabs={COMPLIANCE_TABS} activeTab={activeTab} onChange={setActiveTab} />

      <TabPanel id="credentials" activeTab={activeTab}>
        <AdminSection title="Integration Credentials" description="Manage METRC/BioTrack API credentials per site">
          <AdminCard title="API Credentials" icon={Key} actions={
            <Button onClick={() => setIsModalOpen(true)}><Plus className="w-4 h-4" />Add Credential</Button>
          }>
            <AdminTable columns={credentialColumns} data={MOCK_CREDENTIALS} keyField="id" />
          </AdminCard>
        </AdminSection>
      </TabPanel>

      <TabPanel id="sync" activeTab={activeTab}>
        <AdminSection title="Sync Configuration" description="Configure real-time vs scheduled sync settings">
          <AdminGrid columns={2}>
            {MOCK_SYNC_CONFIG.map(config => (
              <AdminCard key={config.id} title={config.integration} icon={RefreshCw}>
                <div className="space-y-4">
                  <div className="flex justify-between">
                    <span className="text-sm text-muted-foreground">Mode</span>
                    <span className="text-sm text-foreground">{config.mode}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-sm text-muted-foreground">Retry Policy</span>
                    <span className="text-sm text-foreground">{config.retryPolicy}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-sm text-muted-foreground">DLQ Enabled</span>
                    <span className={config.dlqEnabled ? 'text-emerald-400' : 'text-muted-foreground'}>
                      {config.dlqEnabled ? 'Yes' : 'No'}
                    </span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-sm text-muted-foreground">Daily Reconcile</span>
                    <span className="text-sm text-foreground">{config.reconcileTime}</span>
                  </div>
                  <Button variant="secondary" className="w-full">Configure</Button>
                </div>
              </AdminCard>
            ))}
          </AdminGrid>
        </AdminSection>
      </TabPanel>

      <TabPanel id="coa" activeTab={activeTab}>
        <AdminSection title="COA Policies" description="Configure Certificate of Analysis hold and pass rules">
          <AdminGrid columns={2}>
            {MOCK_COA_POLICIES.map(policy => (
              <AdminCard key={policy.id} title={policy.site} icon={FileCheck}>
                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-muted-foreground">Hold on Fail</span>
                    <Switch checked={policy.holdOnFail} onChange={() => {}} />
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-muted-foreground">Auto-Pass Enabled</span>
                    <Switch checked={policy.autoPass} onChange={() => {}} />
                  </div>
                  <div>
                    <span className="text-sm text-muted-foreground">Required Tests</span>
                    <div className="flex flex-wrap gap-1 mt-2">
                      {policy.requiredTests.map(test => (
                        <span key={test} className="text-xs bg-white/5 px-2 py-0.5 rounded">{test}</span>
                      ))}
                    </div>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-sm text-muted-foreground">Grace Period</span>
                    <span className="text-sm text-foreground">{policy.gracePeriod} hours</span>
                  </div>
                </div>
              </AdminCard>
            ))}
          </AdminGrid>
        </AdminSection>
      </TabPanel>

      <TabPanel id="jurisdiction" activeTab={activeTab}>
        <AdminSection title="Jurisdiction Rules" description="State-by-state compliance requirements">
          <AdminCard title="Jurisdiction Matrix" icon={Scale} actions={
            <Button onClick={() => setIsModalOpen(true)}><Plus className="w-4 h-4" />Add State</Button>
          }>
            <AdminTable columns={jurisdictionColumns} data={MOCK_JURISDICTION} keyField="id" />
          </AdminCard>
        </AdminSection>
      </TabPanel>

      <TabPanel id="destruction" activeTab={activeTab}>
        <AdminSection title="Destruction Settings" description="Configure two-person signoff and reason codes">
          <AdminGrid columns={2}>
            {MOCK_DESTRUCTION.map(setting => (
              <AdminCard key={setting.id} title={setting.site} icon={AlertTriangle}>
                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-muted-foreground">Two-Person Required</span>
                    <Switch checked={setting.twoPersonRequired} onChange={() => {}} />
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-muted-foreground">Photo Required</span>
                    <Switch checked={setting.photoRequired} onChange={() => {}} />
                  </div>
                  <div>
                    <span className="text-sm text-muted-foreground">Reason Codes</span>
                    <div className="flex flex-wrap gap-1 mt-2">
                      {setting.reasonCodes.map(code => (
                        <span key={code} className="text-xs bg-white/5 px-2 py-0.5 rounded">{code}</span>
                      ))}
                    </div>
                  </div>
                </div>
              </AdminCard>
            ))}
          </AdminGrid>
        </AdminSection>
      </TabPanel>

      <AdminModal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} title="Add Configuration" size="lg"
        footer={<><Button variant="ghost" onClick={() => setIsModalOpen(false)}>Cancel</Button><Button onClick={() => setIsModalOpen(false)}>Save</Button></>}>
        <div className="space-y-4">
          <FormField label="Provider" required><Select options={PROVIDERS} /></FormField>
          <FormField label="API Key" required><Input type="password" placeholder="Enter API key" /></FormField>
        </div>
      </AdminModal>
    </div>
  );
}

