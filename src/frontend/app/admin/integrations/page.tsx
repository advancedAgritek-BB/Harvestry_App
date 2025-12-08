'use client';

import React, { useState, useEffect } from 'react';
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
  Loader2,
  Leaf,
  Plug,
  Activity,
  Thermometer,
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
import { useIntegrationStore } from '@/stores/integrationStore';

const INTEGRATION_TABS = [
  { id: 'slack', label: 'Slack', icon: MessageSquare },
  { id: 'quickbooks', label: 'QuickBooks', icon: Receipt },
  { id: 'growlink', label: 'Growlink', icon: Leaf },
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

// Growlink mock data
const MOCK_GROWLINK_DEVICES = [
  { id: 'dev-1', name: 'Room A Controller', deviceType: 'GCX-Pro', isOnline: true, sensorCount: 8, lastSeen: '2 min ago' },
  { id: 'dev-2', name: 'Flower Room 1', deviceType: 'Growlink Hub', isOnline: true, sensorCount: 12, lastSeen: '1 min ago' },
  { id: 'dev-3', name: 'Clone Room', deviceType: 'Agrowtek GCX', isOnline: false, sensorCount: 6, lastSeen: '2 hours ago' },
];

const MOCK_GROWLINK_MAPPINGS = [
  { id: '1', deviceId: 'dev-1', sensorId: 'temp-1', sensorName: 'Room A Temperature', sensorType: 'temperature', harvestryStream: 'Room A - Air Temp', isActive: true, autoCreated: true },
  { id: '2', deviceId: 'dev-1', sensorId: 'rh-1', sensorName: 'Room A Humidity', sensorType: 'humidity', harvestryStream: 'Room A - RH', isActive: true, autoCreated: true },
  { id: '3', deviceId: 'dev-2', sensorId: 'co2-1', sensorName: 'Flower Room CO2', sensorType: 'co2', harvestryStream: 'Flower 1 - CO2', isActive: true, autoCreated: false },
  { id: '4', deviceId: 'dev-2', sensorId: 'vpd-1', sensorName: 'Flower Room VPD', sensorType: 'vpd', harvestryStream: 'Flower 1 - VPD', isActive: true, autoCreated: true },
];

export default function IntegrationsAdminPage() {
  const [activeTab, setActiveTab] = useState('slack');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isConnectingSlack, setIsConnectingSlack] = useState(false);
  const [isConnectingQbo, setIsConnectingQbo] = useState(false);
  const [isSyncing, setIsSyncing] = useState(false);
  
  // Growlink state
  const [growlinkConnected, setGrowlinkConnected] = useState(false);
  const [growlinkAccount, setGrowlinkAccount] = useState('');
  const [growlinkLastSync, setGrowlinkLastSync] = useState('Never');
  const [growlinkAutoMap, setGrowlinkAutoMap] = useState(true);
  const [isConnectingGrowlink, setIsConnectingGrowlink] = useState(false);
  const [isGrowlinkSyncing, setIsGrowlinkSyncing] = useState(false);

  const {
    slackConnected,
    slackWorkspace,
    slackMirrorMode,
    qboConnected,
    qboCompany,
    qboSyncMode,
    qboLastSync,
    setSlackConnected,
    setQboConnected,
    setQboLastSync,
  } = useIntegrationStore();
  
  // Check URL params for Growlink OAuth callback
  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    if (params.get('growlink') === 'connected') {
      setGrowlinkConnected(true);
      setGrowlinkAccount('My Growlink Account');
      setGrowlinkLastSync('Just now');
      setActiveTab('growlink');
      // Clean up URL
      window.history.replaceState({}, '', window.location.pathname);
    }
  }, []);

  const handleConnectSlack = () => {
    setIsConnectingSlack(true);
    setTimeout(() => {
      setSlackConnected(true);
      setIsConnectingSlack(false);
    }, 2000);
  };

  const handleDisconnectSlack = () => {
    if (confirm('Are you sure you want to disconnect Slack?')) {
      setSlackConnected(false);
    }
  };

  const handleConnectQbo = () => {
    setIsConnectingQbo(true);
    setTimeout(() => {
      setQboConnected(true);
      setIsConnectingQbo(false);
    }, 2000);
  };

  const handleDisconnectQbo = () => {
    if (confirm('Are you sure you want to disconnect QuickBooks?')) {
      setQboConnected(false);
    }
  };

  const handleSyncQbo = () => {
    setIsSyncing(true);
    setTimeout(() => {
      setQboLastSync('Just now');
      setIsSyncing(false);
    }, 1500);
  };

  const handleConnectGrowlink = () => {
    setIsConnectingGrowlink(true);
    // In production, this would redirect to the Growlink OAuth URL
    setTimeout(() => {
      setGrowlinkConnected(true);
      setGrowlinkAccount('demo@growlink.io');
      setGrowlinkLastSync('Just now');
      setIsConnectingGrowlink(false);
    }, 2000);
  };

  const handleDisconnectGrowlink = () => {
    if (confirm('Are you sure you want to disconnect Growlink? Sensor mappings will be preserved.')) {
      setGrowlinkConnected(false);
      setGrowlinkAccount('');
    }
  };

  const handleSyncGrowlink = () => {
    setIsGrowlinkSyncing(true);
    setTimeout(() => {
      setGrowlinkLastSync('Just now');
      setIsGrowlinkSyncing(false);
    }, 1500);
  };

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

  // Growlink device columns
  const growlinkDeviceColumns = [
    {
      key: 'name',
      header: 'Device',
      render: (item: typeof MOCK_GROWLINK_DEVICES[0]) => (
        <div className="flex items-center gap-2">
          <Plug className="w-4 h-4 text-muted-foreground" />
          <span className="font-medium text-foreground">{item.name}</span>
        </div>
      ),
    },
    {
      key: 'deviceType',
      header: 'Type',
      render: (item: typeof MOCK_GROWLINK_DEVICES[0]) => (
        <span className="text-sm text-muted-foreground">{item.deviceType}</span>
      ),
    },
    {
      key: 'sensorCount',
      header: 'Sensors',
      render: (item: typeof MOCK_GROWLINK_DEVICES[0]) => (
        <span className="text-sm text-cyan-400">{item.sensorCount}</span>
      ),
    },
    {
      key: 'isOnline',
      header: 'Status',
      render: (item: typeof MOCK_GROWLINK_DEVICES[0]) => (
        <StatusBadge status={item.isOnline ? 'active' : 'inactive'} />
      ),
    },
    {
      key: 'lastSeen',
      header: 'Last Seen',
      render: (item: typeof MOCK_GROWLINK_DEVICES[0]) => (
        <span className="text-xs text-muted-foreground">{item.lastSeen}</span>
      ),
    },
  ];

  // Growlink mapping columns
  const growlinkMappingColumns = [
    {
      key: 'sensorName',
      header: 'Growlink Sensor',
      render: (item: typeof MOCK_GROWLINK_MAPPINGS[0]) => (
        <div className="flex items-center gap-2">
          <Thermometer className="w-4 h-4 text-muted-foreground" />
          <span className="font-medium text-foreground">{item.sensorName}</span>
        </div>
      ),
    },
    {
      key: 'sensorType',
      header: 'Type',
      render: (item: typeof MOCK_GROWLINK_MAPPINGS[0]) => (
        <span className="text-xs bg-white/5 px-2 py-0.5 rounded capitalize">{item.sensorType}</span>
      ),
    },
    {
      key: 'harvestryStream',
      header: 'Harvestry Stream',
      render: (item: typeof MOCK_GROWLINK_MAPPINGS[0]) => (
        <span className="text-sm text-emerald-400">{item.harvestryStream}</span>
      ),
    },
    {
      key: 'autoCreated',
      header: 'Source',
      render: (item: typeof MOCK_GROWLINK_MAPPINGS[0]) => (
        <span className={`text-xs ${item.autoCreated ? 'text-muted-foreground' : 'text-cyan-400'}`}>
          {item.autoCreated ? 'Auto' : 'Manual'}
        </span>
      ),
    },
    {
      key: 'isActive',
      header: 'Status',
      render: (item: typeof MOCK_GROWLINK_MAPPINGS[0]) => (
        <StatusBadge status={item.isActive ? 'active' : 'inactive'} />
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
                      {slackConnected && (
                        <div className="text-xs text-muted-foreground">{slackWorkspace}</div>
                      )}
                    </div>
                  </div>
                  {slackConnected ? (
                    <Button size="sm" variant="secondary" onClick={handleDisconnectSlack}>Disconnect</Button>
                  ) : (
                    <Button size="sm" onClick={handleConnectSlack} disabled={isConnectingSlack}>
                      {isConnectingSlack ? <Loader2 className="w-4 h-4 animate-spin" /> : 'Connect Slack'}
                    </Button>
                  )}
                </div>

                <div className="flex items-center justify-between">
                  <div>
                    <div className="text-sm font-medium text-foreground">Mirror Mode</div>
                    <div className="text-xs text-muted-foreground">
                      Full two-way sync (edits, deletes, threads)
                    </div>
                  </div>
                  <Switch checked={slackMirrorMode} onChange={() => {}} disabled={!slackConnected} />
                </div>

                <div className="flex items-center justify-between">
                  <div>
                    <div className="text-sm font-medium text-foreground">Slash Commands</div>
                    <div className="text-xs text-muted-foreground">
                      Enable /harvestry commands
                    </div>
                  </div>
                  <Switch checked={true} onChange={() => {}} disabled={!slackConnected} />
                </div>
              </div>
            </AdminCard>

            {/* Quick Stats */}
            <AdminCard title="Activity" icon={Bell}>
              <div className="grid grid-cols-2 gap-4">
                <div className="p-4 bg-white/5 rounded-lg text-center">
                  <div className="text-2xl font-bold text-foreground">{slackConnected ? '247' : '-'}</div>
                  <div className="text-xs text-muted-foreground">Messages Today</div>
                </div>
                <div className="p-4 bg-white/5 rounded-lg text-center">
                  <div className="text-2xl font-bold text-foreground">{slackConnected ? '12' : '-'}</div>
                  <div className="text-xs text-muted-foreground">Alerts Sent</div>
                </div>
                <div className="p-4 bg-white/5 rounded-lg text-center">
                  <div className="text-2xl font-bold text-foreground">{slackConnected ? '4' : '-'}</div>
                  <div className="text-xs text-muted-foreground">Channels Active</div>
                </div>
                <div className="p-4 bg-white/5 rounded-lg text-center">
                  <div className={cn("text-2xl font-bold", slackConnected ? "text-emerald-400" : "text-muted-foreground")}>
                    {slackConnected ? '99.8%' : '-'}
                  </div>
                  <div className="text-xs text-muted-foreground">Delivery Rate</div>
                </div>
              </div>
            </AdminCard>
          </AdminGrid>

          {slackConnected && (
            <AdminCard title="Channel Mappings" icon={Hash} className="mt-6" actions={
              <Button onClick={() => setIsModalOpen(true)}>Add Channel</Button>
            }>
              <AdminTable columns={channelColumns} data={MOCK_SLACK_CHANNELS} keyField="id" />
            </AdminCard>
          )}
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
                      {qboConnected && (
                        <div className="text-xs text-muted-foreground">{qboCompany}</div>
                      )}
                    </div>
                  </div>
                  {qboConnected ? (
                    <Button size="sm" variant="secondary" onClick={handleDisconnectQbo}>Disconnect</Button>
                  ) : (
                    <Button size="sm" onClick={handleConnectQbo} disabled={isConnectingQbo}>
                      {isConnectingQbo ? <Loader2 className="w-4 h-4 animate-spin" /> : 'Connect QuickBooks'}
                    </Button>
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

                <Button 
                  variant="secondary" 
                  className="w-full" 
                  onClick={handleSyncQbo} 
                  disabled={!qboConnected || isSyncing}
                >
                  {isSyncing ? <Loader2 className="w-4 h-4 animate-spin" /> : <RefreshCw className="w-4 h-4 mr-2" />}
                  {isSyncing ? 'Syncing...' : 'Sync Now'}
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
                  <Switch checked={true} onChange={() => {}} disabled={!qboConnected} />
                </div>

                <div className="flex items-center justify-between">
                  <div>
                    <div className="text-sm font-medium text-foreground">GL Summary</div>
                    <div className="text-xs text-muted-foreground">
                      Monthly JEs for WIP → FG → COGS
                    </div>
                  </div>
                  <Switch checked={true} onChange={() => {}} disabled={!qboConnected} />
                </div>

                <FormField label="Reconciliation Schedule">
                  <Select options={[
                    { value: 'daily', label: 'Daily' },
                    { value: 'weekly', label: 'Weekly' },
                    { value: 'monthly', label: 'Monthly' },
                  ]} defaultValue="daily" disabled={!qboConnected} />
                </FormField>
              </div>
            </AdminCard>
          </AdminGrid>

          {qboConnected && (
            <AdminCard title="Account Mappings" icon={Receipt} className="mt-6" actions={
              <Button onClick={() => setIsModalOpen(true)}>Add Mapping</Button>
            }>
              <AdminTable columns={mappingColumns} data={MOCK_QBO_MAPPINGS} keyField="id" />
            </AdminCard>
          )}
        </AdminSection>
      </TabPanel>

      <TabPanel id="growlink" activeTab={activeTab}>
        <AdminSection title="Growlink Integration" description="Connect existing Growlink hardware to import sensor data into Harvestry">
          <AdminGrid columns={2}>
            {/* Connection Status */}
            <AdminCard title="Connection Status" icon={Link2}>
              <div className="space-y-4">
                <div className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
                  <div className="flex items-center gap-3">
                    {growlinkConnected ? (
                      <CheckCircle className="w-5 h-5 text-emerald-400" />
                    ) : (
                      <XCircle className="w-5 h-5 text-rose-400" />
                    )}
                    <div>
                      <div className="font-medium text-foreground">
                        {growlinkConnected ? 'Connected' : 'Not Connected'}
                      </div>
                      {growlinkConnected && (
                        <div className="text-xs text-muted-foreground">{growlinkAccount}</div>
                      )}
                    </div>
                  </div>
                  {growlinkConnected ? (
                    <Button size="sm" variant="secondary" onClick={handleDisconnectGrowlink}>Disconnect</Button>
                  ) : (
                    <Button size="sm" onClick={handleConnectGrowlink} disabled={isConnectingGrowlink}>
                      {isConnectingGrowlink ? <Loader2 className="w-4 h-4 animate-spin" /> : 'Connect Growlink'}
                    </Button>
                  )}
                </div>

                <div className="flex items-center justify-between">
                  <span className="text-sm text-muted-foreground">Last Sync</span>
                  <span className="text-sm text-foreground">{growlinkLastSync}</span>
                </div>

                <Button 
                  variant="secondary" 
                  className="w-full" 
                  onClick={handleSyncGrowlink} 
                  disabled={!growlinkConnected || isGrowlinkSyncing}
                >
                  {isGrowlinkSyncing ? <Loader2 className="w-4 h-4 animate-spin" /> : <RefreshCw className="w-4 h-4 mr-2" />}
                  {isGrowlinkSyncing ? 'Syncing...' : 'Sync Now'}
                </Button>
              </div>
            </AdminCard>

            {/* Sync Settings */}
            <AdminCard title="Sync Settings" icon={Settings}>
              <div className="space-y-4">
                <div className="flex items-center justify-between">
                  <div>
                    <div className="text-sm font-medium text-foreground">Auto-Map Sensors</div>
                    <div className="text-xs text-muted-foreground">
                      Automatically create streams for new sensors
                    </div>
                  </div>
                  <Switch checked={growlinkAutoMap} onChange={() => setGrowlinkAutoMap(!growlinkAutoMap)} disabled={!growlinkConnected} />
                </div>

                <FormField label="Sync Interval">
                  <Select options={[
                    { value: '30', label: '30 seconds' },
                    { value: '60', label: '1 minute' },
                    { value: '300', label: '5 minutes' },
                  ]} defaultValue="60" disabled={!growlinkConnected} />
                </FormField>

                <div className="p-3 bg-emerald-500/10 border border-emerald-500/20 rounded-lg">
                  <div className="flex items-start gap-2">
                    <Leaf className="w-4 h-4 text-emerald-400 mt-0.5" />
                    <div className="text-xs text-emerald-200">
                      Growlink integration supports Agrowtek GCX, TrolMaster, and Pulse Grow devices connected to your Growlink account.
                    </div>
                  </div>
                </div>
              </div>
            </AdminCard>
          </AdminGrid>

          {growlinkConnected && (
            <>
              {/* Devices */}
              <AdminCard title="Connected Devices" icon={Plug} className="mt-6">
                <AdminTable columns={growlinkDeviceColumns} data={MOCK_GROWLINK_DEVICES} keyField="id" />
              </AdminCard>

              {/* Activity Stats */}
              <AdminCard title="Sync Activity" icon={Activity} className="mt-6">
                <div className="grid grid-cols-4 gap-4">
                  <div className="p-4 bg-white/5 rounded-lg text-center">
                    <div className="text-2xl font-bold text-foreground">26</div>
                    <div className="text-xs text-muted-foreground">Active Sensors</div>
                  </div>
                  <div className="p-4 bg-white/5 rounded-lg text-center">
                    <div className="text-2xl font-bold text-emerald-400">1,247</div>
                    <div className="text-xs text-muted-foreground">Readings Today</div>
                  </div>
                  <div className="p-4 bg-white/5 rounded-lg text-center">
                    <div className="text-2xl font-bold text-foreground">99.9%</div>
                    <div className="text-xs text-muted-foreground">Sync Success</div>
                  </div>
                  <div className="p-4 bg-white/5 rounded-lg text-center">
                    <div className="text-2xl font-bold text-cyan-400">45ms</div>
                    <div className="text-xs text-muted-foreground">Avg Latency</div>
                  </div>
                </div>
              </AdminCard>

              {/* Stream Mappings */}
              <AdminCard title="Sensor Mappings" icon={Thermometer} className="mt-6" actions={
                <Button onClick={() => setIsModalOpen(true)}>Add Mapping</Button>
              }>
                <AdminTable columns={growlinkMappingColumns} data={MOCK_GROWLINK_MAPPINGS} keyField="id" />
              </AdminCard>
            </>
          )}
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
