'use client';

import React, { useState } from 'react';
import {
  MessageSquare,
  Receipt,
  Link2,
  CheckCircle,
  XCircle,
  RefreshCw,
  ExternalLink,
  Settings,
  Hash,
  Bell,
} from 'lucide-react';
import {
  AdminCard,
  AdminSection,
  AdminGrid,
  AdminTabs,
  TabPanel,
  AdminTable,
  StatusBadge,
  Button,
  AdminModal,
  FormField,
  Input,
  Select,
  Switch,
} from '@/components/admin';

const INTEGRATION_TABS = [
  { id: 'slack', label: 'Slack', icon: MessageSquare },
  { id: 'quickbooks', label: 'QuickBooks', icon: Receipt },
];

// Mock data
const MOCK_SLACK_CHANNELS = [
  { id: '1', channel: '#cultivation-alerts', purpose: 'Critical Alerts', notifications: ['Critical', 'Warning'], active: true },
  { id: '2', channel: '#daily-tasks', purpose: 'Task Notifications', notifications: ['Tasks', 'Reminders'], active: true },
  { id: '3', channel: '#compliance', purpose: 'Compliance Alerts', notifications: ['METRC', 'COA'], active: true },
  { id: '4', channel: '#general', purpose: 'General Updates', notifications: ['Info'], active: false },
];

const MOCK_QBO_MAPPINGS = [
  { id: '1', harvestryType: 'Flower - Indoor', qboItem: 'Cannabis - Indoor Flower', category: 'Inventory', synced: true },
  { id: '2', harvestryType: 'Concentrate - Live Resin', qboItem: 'Cannabis - Live Resin', category: 'Inventory', synced: true },
  { id: '3', harvestryType: 'Nutrient Purchase', qboItem: 'Growing Supplies', category: 'Expense', synced: true },
  { id: '4', harvestryType: 'Labor - Cultivation', qboItem: 'Cultivation Labor', category: 'Expense', synced: false },
];

export default function IntegrationsAdminPage() {
  const [activeTab, setActiveTab] = useState('slack');
  const [isModalOpen, setIsModalOpen] = useState(false);

  // Slack Status
  const slackConnected = true;
  const slackWorkspace = 'Harvestry Team';
  const slackMirrorMode = false;

  // QuickBooks Status
  const qboConnected = true;
  const qboCompany = 'Harvestry LLC';
  const qboSyncMode = 'Item-level';
  const qboLastSync = '5 min ago';

  const channelColumns = [
    {
      key: 'channel',
      header: 'Channel',
      render: (item: typeof MOCK_SLACK_CHANNELS[0]) => (
        <div className="flex items-center gap-2">
          <Hash className="w-4 h-4 text-muted-foreground" />
          <span className="font-medium text-foreground">{item.channel}</span>
        </div>
      ),
    },
    {
      key: 'purpose',
      header: 'Purpose',
      render: (item: typeof MOCK_SLACK_CHANNELS[0]) => (
        <span className="text-sm text-muted-foreground">{item.purpose}</span>
      ),
    },
    {
      key: 'notifications',
      header: 'Notifications',
      render: (item: typeof MOCK_SLACK_CHANNELS[0]) => (
        <div className="flex gap-1">
          {item.notifications.map(n => (
            <span key={n} className="text-xs bg-white/5 px-2 py-0.5 rounded">{n}</span>
          ))}
        </div>
      ),
    },
    {
      key: 'active',
      header: 'Status',
      render: (item: typeof MOCK_SLACK_CHANNELS[0]) => (
        <StatusBadge status={item.active ? 'active' : 'inactive'} />
      ),
    },
    {
      key: 'actions',
      header: '',
      width: '80px',
      render: () => (
        <Button size="sm" variant="ghost"><Settings className="w-4 h-4" /></Button>
      ),
    },
  ];

  const mappingColumns = [
    {
      key: 'harvestryType',
      header: 'Harvestry Type',
      render: (item: typeof MOCK_QBO_MAPPINGS[0]) => (
        <span className="font-medium text-foreground">{item.harvestryType}</span>
      ),
    },
    {
      key: 'qboItem',
      header: 'QuickBooks Item',
      render: (item: typeof MOCK_QBO_MAPPINGS[0]) => (
        <span className="text-sm text-cyan-400">{item.qboItem}</span>
      ),
    },
    {
      key: 'category',
      header: 'Category',
      render: (item: typeof MOCK_QBO_MAPPINGS[0]) => (
        <span className="text-xs bg-white/5 px-2 py-0.5 rounded">{item.category}</span>
      ),
    },
    {
      key: 'synced',
      header: 'Synced',
      render: (item: typeof MOCK_QBO_MAPPINGS[0]) => (
        <div className="flex items-center gap-1.5">
          {item.synced ? (
            <CheckCircle className="w-4 h-4 text-emerald-400" />
          ) : (
            <XCircle className="w-4 h-4 text-amber-400" />
          )}
          <span className={item.synced ? 'text-emerald-400' : 'text-amber-400'}>
            {item.synced ? 'Yes' : 'Pending'}
          </span>
        </div>
      ),
    },
    {
      key: 'actions',
      header: '',
      width: '80px',
      render: () => (
        <Button size="sm" variant="ghost"><Settings className="w-4 h-4" /></Button>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <AdminTabs tabs={INTEGRATION_TABS} activeTab={activeTab} onChange={setActiveTab} />

      <TabPanel id="slack" activeTab={activeTab}>
        <AdminSection title="Slack Integration" description="Configure Slack workspace connection and channel mappings">
          <AdminGrid columns={2}>
            {/* Connection Status */}
            <AdminCard title="Connection Status" icon={Link2}>
              <div className="space-y-4">
                <div className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
                  <div className="flex items-center gap-3">
                    {slackConnected ? (
                      <CheckCircle className="w-5 h-5 text-emerald-400" />
                    ) : (
                      <XCircle className="w-5 h-5 text-rose-400" />
                    )}
                    <div>
                      <div className="font-medium text-foreground">
                        {slackConnected ? 'Connected' : 'Not Connected'}
                      </div>
                      <div className="text-xs text-muted-foreground">{slackWorkspace}</div>
                    </div>
                  </div>
                  {slackConnected ? (
                    <Button size="sm" variant="secondary">Disconnect</Button>
                  ) : (
                    <Button size="sm">Connect Slack</Button>
                  )}
                </div>

                <div className="flex items-center justify-between">
                  <div>
                    <div className="text-sm font-medium text-foreground">Mirror Mode</div>
                    <div className="text-xs text-muted-foreground">
                      Full two-way sync (edits, deletes, threads)
                    </div>
                  </div>
                  <Switch checked={slackMirrorMode} onChange={() => {}} />
                </div>

                <div className="flex items-center justify-between">
                  <div>
                    <div className="text-sm font-medium text-foreground">Slash Commands</div>
                    <div className="text-xs text-muted-foreground">
                      Enable /harvestry commands
                    </div>
                  </div>
                  <Switch checked={true} onChange={() => {}} />
                </div>
              </div>
            </AdminCard>

            {/* Quick Stats */}
            <AdminCard title="Activity" icon={Bell}>
              <div className="grid grid-cols-2 gap-4">
                <div className="p-4 bg-white/5 rounded-lg text-center">
                  <div className="text-2xl font-bold text-foreground">247</div>
                  <div className="text-xs text-muted-foreground">Messages Today</div>
                </div>
                <div className="p-4 bg-white/5 rounded-lg text-center">
                  <div className="text-2xl font-bold text-foreground">12</div>
                  <div className="text-xs text-muted-foreground">Alerts Sent</div>
                </div>
                <div className="p-4 bg-white/5 rounded-lg text-center">
                  <div className="text-2xl font-bold text-foreground">4</div>
                  <div className="text-xs text-muted-foreground">Channels Active</div>
                </div>
                <div className="p-4 bg-white/5 rounded-lg text-center">
                  <div className="text-2xl font-bold text-emerald-400">99.8%</div>
                  <div className="text-xs text-muted-foreground">Delivery Rate</div>
                </div>
              </div>
            </AdminCard>
          </AdminGrid>

          <AdminCard title="Channel Mappings" icon={Hash} className="mt-6" actions={
            <Button onClick={() => setIsModalOpen(true)}>Add Channel</Button>
          }>
            <AdminTable columns={channelColumns} data={MOCK_SLACK_CHANNELS} keyField="id" />
          </AdminCard>
        </AdminSection>
      </TabPanel>

      <TabPanel id="quickbooks" activeTab={activeTab}>
        <AdminSection title="QuickBooks Online" description="Configure QuickBooks connection and account mappings">
          <AdminGrid columns={2}>
            {/* Connection Status */}
            <AdminCard title="Connection Status" icon={Link2}>
              <div className="space-y-4">
                <div className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
                  <div className="flex items-center gap-3">
                    {qboConnected ? (
                      <CheckCircle className="w-5 h-5 text-emerald-400" />
                    ) : (
                      <XCircle className="w-5 h-5 text-rose-400" />
                    )}
                    <div>
                      <div className="font-medium text-foreground">
                        {qboConnected ? 'Connected' : 'Not Connected'}
                      </div>
                      <div className="text-xs text-muted-foreground">{qboCompany}</div>
                    </div>
                  </div>
                  {qboConnected ? (
                    <Button size="sm" variant="secondary">Reconnect</Button>
                  ) : (
                    <Button size="sm">Connect QuickBooks</Button>
                  )}
                </div>

                <div className="flex items-center justify-between">
                  <span className="text-sm text-muted-foreground">Sync Mode</span>
                  <span className="text-sm text-foreground">{qboSyncMode}</span>
                </div>

                <div className="flex items-center justify-between">
                  <span className="text-sm text-muted-foreground">Last Sync</span>
                  <span className="text-sm text-foreground">{qboLastSync}</span>
                </div>

                <Button variant="secondary" className="w-full">
                  <RefreshCw className="w-4 h-4" />
                  Sync Now
                </Button>
              </div>
            </AdminCard>

            {/* Sync Settings */}
            <AdminCard title="Sync Settings" icon={Settings}>
              <div className="space-y-4">
                <div className="flex items-center justify-between">
                  <div>
                    <div className="text-sm font-medium text-foreground">Item-Level Sync</div>
                    <div className="text-xs text-muted-foreground">
                      POs, Bills, Invoices, Payments
                    </div>
                  </div>
                  <Switch checked={true} onChange={() => {}} />
                </div>

                <div className="flex items-center justify-between">
                  <div>
                    <div className="text-sm font-medium text-foreground">GL Summary</div>
                    <div className="text-xs text-muted-foreground">
                      Monthly JEs for WIP → FG → COGS
                    </div>
                  </div>
                  <Switch checked={true} onChange={() => {}} />
                </div>

                <FormField label="Reconciliation Schedule">
                  <Select options={[
                    { value: 'daily', label: 'Daily' },
                    { value: 'weekly', label: 'Weekly' },
                    { value: 'monthly', label: 'Monthly' },
                  ]} defaultValue="daily" />
                </FormField>
              </div>
            </AdminCard>
          </AdminGrid>

          <AdminCard title="Account Mappings" icon={Receipt} className="mt-6" actions={
            <Button onClick={() => setIsModalOpen(true)}>Add Mapping</Button>
          }>
            <AdminTable columns={mappingColumns} data={MOCK_QBO_MAPPINGS} keyField="id" />
          </AdminCard>
        </AdminSection>
      </TabPanel>

      <AdminModal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} title="Add Configuration" size="lg"
        footer={<><Button variant="ghost" onClick={() => setIsModalOpen(false)}>Cancel</Button><Button onClick={() => setIsModalOpen(false)}>Save</Button></>}>
        <div className="space-y-4">
          <FormField label="Name" required><Input placeholder="Enter name" /></FormField>
        </div>
      </AdminModal>
    </div>
  );
}

