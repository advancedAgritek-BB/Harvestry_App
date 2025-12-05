'use client';

import React, { useState } from 'react';
import {
  Bell,
  Users,
  Clock,
  Moon,
  Plus,
  Edit2,
  Trash2,
  Mail,
  MessageSquare,
  Smartphone,
  AlertTriangle,
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
  Checkbox,
} from '@/components/admin';

const NOTIFICATION_TABS = [
  { id: 'subscriptions', label: 'Subscriptions', icon: Bell },
  { id: 'escalations', label: 'Escalation Chains', icon: Users },
  { id: 'quiet-hours', label: 'Quiet Hours', icon: Moon },
];

// Mock data
const MOCK_SUBSCRIPTIONS = [
  { id: '1', user: 'Brandon Burnette', role: 'Super Admin', modules: ['All'], severities: ['All'], channels: ['email', 'slack', 'sms'] },
  { id: '2', user: 'Sarah Mitchell', role: 'Cultivation Manager', modules: ['Cultivation', 'Irrigation'], severities: ['Critical', 'Warning'], channels: ['email', 'slack'] },
  { id: '3', user: 'Mike Thompson', role: 'Grower', modules: ['Cultivation'], severities: ['Critical'], channels: ['slack'] },
  { id: '4', user: 'Emily Chen', role: 'Compliance Officer', modules: ['Compliance', 'Inventory'], severities: ['All'], channels: ['email', 'slack'] },
];

const MOCK_ESCALATIONS = [
  { id: '1', name: 'Critical Equipment Failure', trigger: 'Critical alert unacknowledged', initialDelay: '5 min', escalationSteps: 3, finalAction: 'Page on-call manager' },
  { id: '2', name: 'Compliance Deadline', trigger: 'METRC sync failure > 2 hours', initialDelay: '30 min', escalationSteps: 2, finalAction: 'Email compliance team' },
  { id: '3', name: 'Environment Out of Range', trigger: 'Temperature critical > 10 min', initialDelay: '10 min', escalationSteps: 2, finalAction: 'SMS cultivation manager' },
];

const MOCK_QUIET_HOURS = [
  { id: '1', user: 'Brandon Burnette', schedule: 'Never (always available)', overrideFor: 'Critical', active: true },
  { id: '2', user: 'Sarah Mitchell', schedule: '10 PM - 6 AM', overrideFor: 'Critical', active: true },
  { id: '3', user: 'Mike Thompson', schedule: '8 PM - 7 AM', overrideFor: 'Critical', active: true },
  { id: '4', user: 'Emily Chen', schedule: '11 PM - 7 AM', overrideFor: 'All Critical', active: false },
];

const MODULES = [
  { value: 'all', label: 'All Modules' },
  { value: 'cultivation', label: 'Cultivation' },
  { value: 'irrigation', label: 'Irrigation' },
  { value: 'compliance', label: 'Compliance' },
  { value: 'inventory', label: 'Inventory' },
  { value: 'equipment', label: 'Equipment' },
];

const SEVERITIES = [
  { value: 'all', label: 'All Severities' },
  { value: 'critical', label: 'Critical' },
  { value: 'warning', label: 'Warning' },
  { value: 'info', label: 'Info' },
];

export default function NotificationsAdminPage() {
  const [activeTab, setActiveTab] = useState('subscriptions');
  const [isModalOpen, setIsModalOpen] = useState(false);

  const getChannelIcons = (channels: string[]) => (
    <div className="flex items-center gap-1">
      {channels.includes('email') && <Mail className="w-4 h-4 text-violet-400" />}
      {channels.includes('slack') && <MessageSquare className="w-4 h-4 text-cyan-400" />}
      {channels.includes('sms') && <Smartphone className="w-4 h-4 text-emerald-400" />}
    </div>
  );

  const subscriptionColumns = [
    {
      key: 'user',
      header: 'User',
      sortable: true,
      render: (item: typeof MOCK_SUBSCRIPTIONS[0]) => (
        <div>
          <div className="font-medium text-foreground">{item.user}</div>
          <div className="text-xs text-muted-foreground">{item.role}</div>
        </div>
      ),
    },
    {
      key: 'modules',
      header: 'Modules',
      render: (item: typeof MOCK_SUBSCRIPTIONS[0]) => (
        <div className="flex gap-1">
          {item.modules.map(m => (
            <span key={m} className="text-xs bg-white/5 px-2 py-0.5 rounded">{m}</span>
          ))}
        </div>
      ),
    },
    {
      key: 'severities',
      header: 'Severities',
      render: (item: typeof MOCK_SUBSCRIPTIONS[0]) => (
        <div className="flex gap-1">
          {item.severities.map(s => (
            <span key={s} className={`text-xs px-2 py-0.5 rounded ${
              s === 'Critical' ? 'bg-rose-500/10 text-rose-400' :
              s === 'Warning' ? 'bg-amber-500/10 text-amber-400' :
              'bg-white/5 text-muted-foreground'
            }`}>{s}</span>
          ))}
        </div>
      ),
    },
    {
      key: 'channels',
      header: 'Channels',
      render: (item: typeof MOCK_SUBSCRIPTIONS[0]) => getChannelIcons(item.channels),
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

  const escalationColumns = [
    {
      key: 'name',
      header: 'Policy Name',
      sortable: true,
      render: (item: typeof MOCK_ESCALATIONS[0]) => (
        <div className="font-medium text-foreground">{item.name}</div>
      ),
    },
    {
      key: 'trigger',
      header: 'Trigger',
      render: (item: typeof MOCK_ESCALATIONS[0]) => (
        <span className="text-sm text-muted-foreground">{item.trigger}</span>
      ),
    },
    {
      key: 'initialDelay',
      header: 'Initial Delay',
      render: (item: typeof MOCK_ESCALATIONS[0]) => (
        <span className="text-sm">{item.initialDelay}</span>
      ),
    },
    {
      key: 'steps',
      header: 'Steps',
      render: (item: typeof MOCK_ESCALATIONS[0]) => (
        <span>{item.escalationSteps}</span>
      ),
    },
    {
      key: 'finalAction',
      header: 'Final Action',
      render: (item: typeof MOCK_ESCALATIONS[0]) => (
        <span className="text-sm text-amber-400">{item.finalAction}</span>
      ),
    },
    {
      key: 'actions',
      header: '',
      width: '80px',
      render: () => (
        <TableActions>
          <TableActionButton onClick={() => setIsModalOpen(true)}><Edit2 className="w-4 h-4" /></TableActionButton>
          <TableActionButton onClick={() => {}} variant="danger"><Trash2 className="w-4 h-4" /></TableActionButton>
        </TableActions>
      ),
    },
  ];

  const quietHoursColumns = [
    {
      key: 'user',
      header: 'User',
      sortable: true,
      render: (item: typeof MOCK_QUIET_HOURS[0]) => (
        <div className="font-medium text-foreground">{item.user}</div>
      ),
    },
    {
      key: 'schedule',
      header: 'Quiet Hours',
      render: (item: typeof MOCK_QUIET_HOURS[0]) => (
        <div className="flex items-center gap-2">
          <Moon className="w-4 h-4 text-violet-400" />
          <span className="text-sm">{item.schedule}</span>
        </div>
      ),
    },
    {
      key: 'overrideFor',
      header: 'Override For',
      render: (item: typeof MOCK_QUIET_HOURS[0]) => (
        <span className="text-xs bg-rose-500/10 text-rose-400 px-2 py-0.5 rounded">
          {item.overrideFor}
        </span>
      ),
    },
    {
      key: 'active',
      header: 'Status',
      render: (item: typeof MOCK_QUIET_HOURS[0]) => (
        <StatusBadge status={item.active ? 'active' : 'inactive'} />
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
      <AdminTabs tabs={NOTIFICATION_TABS} activeTab={activeTab} onChange={setActiveTab} />

      <TabPanel id="subscriptions" activeTab={activeTab}>
        <AdminSection title="Subscription Management" description="Configure per-user notification preferences">
          <AdminCard title="User Subscriptions" icon={Bell} actions={
            <Button onClick={() => setIsModalOpen(true)}><Plus className="w-4 h-4" />Add Subscription</Button>
          }>
            <AdminTable columns={subscriptionColumns} data={MOCK_SUBSCRIPTIONS} keyField="id" />
          </AdminCard>
        </AdminSection>
      </TabPanel>

      <TabPanel id="escalations" activeTab={activeTab}>
        <AdminSection title="Escalation Chains" description="Define time-based escalation policies">
          <AdminCard title="Escalation Policies" icon={Users} actions={
            <Button onClick={() => setIsModalOpen(true)}><Plus className="w-4 h-4" />Create Policy</Button>
          }>
            <div className="flex items-center gap-2 p-3 bg-amber-500/10 border border-amber-500/20 rounded-lg mb-4">
              <AlertTriangle className="w-4 h-4 text-amber-400 flex-shrink-0" />
              <p className="text-xs text-amber-200">
                Escalation policies ensure critical alerts are addressed. Unacknowledged alerts 
                will escalate through the defined chain until resolved.
              </p>
            </div>
            <AdminTable columns={escalationColumns} data={MOCK_ESCALATIONS} keyField="id" />
          </AdminCard>
        </AdminSection>
      </TabPanel>

      <TabPanel id="quiet-hours" activeTab={activeTab}>
        <AdminSection title="Quiet Hours" description="Configure notification blackout periods">
          <AdminCard title="Quiet Hour Settings" icon={Moon} actions={
            <Button onClick={() => setIsModalOpen(true)}><Plus className="w-4 h-4" />Add Schedule</Button>
          }>
            <div className="flex items-center gap-2 p-3 bg-white/5 rounded-lg mb-4">
              <Clock className="w-4 h-4 text-muted-foreground flex-shrink-0" />
              <p className="text-xs text-muted-foreground">
                During quiet hours, non-critical notifications are held until the quiet period ends. 
                Critical alerts can be configured to override quiet hours.
              </p>
            </div>
            <AdminTable columns={quietHoursColumns} data={MOCK_QUIET_HOURS} keyField="id" />
          </AdminCard>
        </AdminSection>
      </TabPanel>

      <AdminModal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} title="Configure" size="lg"
        footer={<><Button variant="ghost" onClick={() => setIsModalOpen(false)}>Cancel</Button><Button onClick={() => setIsModalOpen(false)}>Save</Button></>}>
        <div className="space-y-4">
          <FormField label="User/Role"><Select options={[{ value: 'user', label: 'Select User' }]} /></FormField>
          <FormField label="Modules">
            <div className="space-y-2">
              <Checkbox checked={true} onChange={() => {}} label="All Modules" />
              <Checkbox checked={false} onChange={() => {}} label="Cultivation" />
              <Checkbox checked={false} onChange={() => {}} label="Compliance" />
            </div>
          </FormField>
          <FormField label="Channels">
            <div className="flex gap-4">
              <Checkbox checked={true} onChange={() => {}} label="Email" />
              <Checkbox checked={true} onChange={() => {}} label="Slack" />
              <Checkbox checked={false} onChange={() => {}} label="SMS" />
            </div>
          </FormField>
        </div>
      </AdminModal>
    </div>
  );
}

