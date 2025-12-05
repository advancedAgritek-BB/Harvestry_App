'use client';

import React, { useState } from 'react';
import {
  Users,
  Shield,
  CreditCard,
  GraduationCap,
  Plus,
  Edit2,
  Trash2,
  MoreHorizontal,
  Mail,
  Phone,
  MapPin,
  CheckCircle,
  XCircle,
  Clock,
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
  TableSearch,
  Button,
  AdminModal,
  FormField,
  Input,
  Select,
  Switch,
  Checkbox,
} from '@/components/admin';

const IDENTITY_TABS = [
  { id: 'users', label: 'Users', icon: Users },
  { id: 'roles', label: 'Roles & Permissions', icon: Shield },
  { id: 'badges', label: 'Badge Credentials', icon: CreditCard },
  { id: 'training', label: 'Training & SOPs', icon: GraduationCap },
];

// Mock data
const MOCK_USERS = [
  { id: '1', name: 'Brandon Burnette', email: 'brandon@harvestry.io', role: 'Super Admin', sites: ['Evergreen', 'Oakdale'], status: 'active', lastLogin: '2025-11-26 09:15' },
  { id: '2', name: 'Sarah Mitchell', email: 'sarah@harvestry.io', role: 'Cultivation Manager', sites: ['Evergreen'], status: 'active', lastLogin: '2025-11-26 08:45' },
  { id: '3', name: 'Mike Thompson', email: 'mike@harvestry.io', role: 'Grower', sites: ['Evergreen'], status: 'active', lastLogin: '2025-11-25 16:30' },
  { id: '4', name: 'Emily Chen', email: 'emily@harvestry.io', role: 'Compliance Officer', sites: ['Evergreen', 'Oakdale'], status: 'active', lastLogin: '2025-11-26 07:00' },
  { id: '5', name: 'David Park', email: 'david@harvestry.io', role: 'Technician', sites: ['Oakdale'], status: 'inactive', lastLogin: '2025-11-20 12:00' },
];

const MOCK_ROLES = [
  { id: '1', name: 'Super Admin', description: 'Full system access', users: 1, permissions: 45 },
  { id: '2', name: 'Cultivation Manager', description: 'Manage cultivation operations', users: 2, permissions: 32 },
  { id: '3', name: 'Grower', description: 'Day-to-day cultivation tasks', users: 8, permissions: 18 },
  { id: '4', name: 'Compliance Officer', description: 'Compliance and regulatory access', users: 2, permissions: 24 },
  { id: '5', name: 'Technician', description: 'Equipment and maintenance', users: 4, permissions: 15 },
  { id: '6', name: 'Viewer', description: 'Read-only access', users: 3, permissions: 8 },
];

const MOCK_BADGES = [
  { id: '1', user: 'Brandon Burnette', badgeId: 'BDG-001', type: 'NFC', issuedAt: '2025-01-15', status: 'active', lastUsed: '2025-11-26 09:15' },
  { id: '2', user: 'Sarah Mitchell', badgeId: 'BDG-002', type: 'NFC', issuedAt: '2025-02-20', status: 'active', lastUsed: '2025-11-26 08:45' },
  { id: '3', user: 'Mike Thompson', badgeId: 'BDG-003', type: 'Barcode', issuedAt: '2025-03-10', status: 'active', lastUsed: '2025-11-25 16:30' },
  { id: '4', user: 'David Park', badgeId: 'BDG-004', type: 'NFC', issuedAt: '2025-04-05', status: 'revoked', lastUsed: '2025-11-20 12:00' },
];

const MOCK_TRAINING = [
  { id: '1', name: 'Cultivation Basics', type: 'SOP', assignedTo: 'All Growers', completionRate: 92, dueDate: null },
  { id: '2', name: 'Safety Protocols', type: 'Training', assignedTo: 'All Users', completionRate: 100, dueDate: null },
  { id: '3', name: 'Equipment Calibration', type: 'SOP', assignedTo: 'Technicians', completionRate: 75, dueDate: '2025-12-01' },
  { id: '4', name: 'Compliance Procedures', type: 'Training', assignedTo: 'Compliance Officers', completionRate: 100, dueDate: null },
  { id: '5', name: 'Emergency Response', type: 'Training', assignedTo: 'All Users', completionRate: 85, dueDate: '2025-12-15' },
];

const ROLES_OPTIONS = [
  { value: 'super-admin', label: 'Super Admin' },
  { value: 'cultivation-manager', label: 'Cultivation Manager' },
  { value: 'grower', label: 'Grower' },
  { value: 'compliance', label: 'Compliance Officer' },
  { value: 'technician', label: 'Technician' },
  { value: 'viewer', label: 'Viewer' },
];

const SITES_OPTIONS = [
  { value: 'evergreen', label: 'Evergreen' },
  { value: 'oakdale', label: 'Oakdale' },
];

export default function IdentityAdminPage() {
  const [activeTab, setActiveTab] = useState('users');
  const [searchQuery, setSearchQuery] = useState('');
  const [isUserModalOpen, setIsUserModalOpen] = useState(false);
  const [isRoleModalOpen, setIsRoleModalOpen] = useState(false);

  // Users columns
  const userColumns = [
    {
      key: 'name',
      header: 'User',
      sortable: true,
      render: (item: typeof MOCK_USERS[0]) => (
        <div className="flex items-center gap-3">
          <div className="w-8 h-8 rounded-full bg-gradient-to-br from-violet-400 to-purple-600 flex items-center justify-center text-white text-xs font-bold">
            {item.name.split(' ').map(n => n[0]).join('')}
          </div>
          <div>
            <div className="font-medium text-foreground">{item.name}</div>
            <div className="text-xs text-muted-foreground">{item.email}</div>
          </div>
        </div>
      ),
    },
    {
      key: 'role',
      header: 'Role',
      render: (item: typeof MOCK_USERS[0]) => (
        <span className="text-sm text-cyan-400">{item.role}</span>
      ),
    },
    {
      key: 'sites',
      header: 'Sites',
      render: (item: typeof MOCK_USERS[0]) => (
        <div className="flex gap-1">
          {item.sites.map(site => (
            <span key={site} className="text-xs bg-white/5 px-2 py-0.5 rounded">
              {site}
            </span>
          ))}
        </div>
      ),
    },
    {
      key: 'lastLogin',
      header: 'Last Login',
      render: (item: typeof MOCK_USERS[0]) => (
        <span className="text-sm text-muted-foreground">{item.lastLogin}</span>
      ),
    },
    {
      key: 'status',
      header: 'Status',
      render: (item: typeof MOCK_USERS[0]) => (
        <StatusBadge status={item.status === 'active' ? 'active' : 'inactive'} />
      ),
    },
    {
      key: 'actions',
      header: '',
      width: '80px',
      render: (item: typeof MOCK_USERS[0]) => (
        <TableActions>
          <TableActionButton onClick={() => setIsUserModalOpen(true)}>
            <Edit2 className="w-4 h-4" />
          </TableActionButton>
          <TableActionButton onClick={() => {}} variant="danger">
            <Trash2 className="w-4 h-4" />
          </TableActionButton>
        </TableActions>
      ),
    },
  ];

  // Roles columns
  const roleColumns = [
    {
      key: 'name',
      header: 'Role Name',
      sortable: true,
      render: (item: typeof MOCK_ROLES[0]) => (
        <div>
          <div className="font-medium text-foreground">{item.name}</div>
          <div className="text-xs text-muted-foreground">{item.description}</div>
        </div>
      ),
    },
    {
      key: 'users',
      header: 'Users',
      render: (item: typeof MOCK_ROLES[0]) => (
        <span>{item.users}</span>
      ),
    },
    {
      key: 'permissions',
      header: 'Permissions',
      render: (item: typeof MOCK_ROLES[0]) => (
        <span className="text-muted-foreground">{item.permissions} permissions</span>
      ),
    },
    {
      key: 'actions',
      header: '',
      width: '80px',
      render: (item: typeof MOCK_ROLES[0]) => (
        <TableActions>
          <TableActionButton onClick={() => setIsRoleModalOpen(true)}>
            <Edit2 className="w-4 h-4" />
          </TableActionButton>
        </TableActions>
      ),
    },
  ];

  // Badges columns
  const badgeColumns = [
    {
      key: 'user',
      header: 'User',
      sortable: true,
    },
    {
      key: 'badgeId',
      header: 'Badge ID',
      render: (item: typeof MOCK_BADGES[0]) => (
        <span className="font-mono text-xs bg-white/5 px-2 py-0.5 rounded">
          {item.badgeId}
        </span>
      ),
    },
    {
      key: 'type',
      header: 'Type',
      render: (item: typeof MOCK_BADGES[0]) => (
        <span className={`text-xs px-2 py-0.5 rounded ${
          item.type === 'NFC' ? 'bg-cyan-500/10 text-cyan-400' : 'bg-amber-500/10 text-amber-400'
        }`}>
          {item.type}
        </span>
      ),
    },
    {
      key: 'issuedAt',
      header: 'Issued',
      render: (item: typeof MOCK_BADGES[0]) => (
        <span className="text-sm text-muted-foreground">{item.issuedAt}</span>
      ),
    },
    {
      key: 'lastUsed',
      header: 'Last Used',
      render: (item: typeof MOCK_BADGES[0]) => (
        <span className="text-sm text-muted-foreground">{item.lastUsed}</span>
      ),
    },
    {
      key: 'status',
      header: 'Status',
      render: (item: typeof MOCK_BADGES[0]) => (
        <StatusBadge status={item.status === 'active' ? 'active' : 'error'} label={item.status} />
      ),
    },
    {
      key: 'actions',
      header: '',
      width: '80px',
      render: (item: typeof MOCK_BADGES[0]) => (
        <TableActions>
          <TableActionButton onClick={() => {}} variant="danger">
            {item.status === 'active' ? 'Revoke' : 'Delete'}
          </TableActionButton>
        </TableActions>
      ),
    },
  ];

  // Training columns
  const trainingColumns = [
    {
      key: 'name',
      header: 'Training/SOP',
      sortable: true,
      render: (item: typeof MOCK_TRAINING[0]) => (
        <div>
          <div className="font-medium text-foreground">{item.name}</div>
          <div className="text-xs text-muted-foreground">{item.type}</div>
        </div>
      ),
    },
    {
      key: 'assignedTo',
      header: 'Assigned To',
    },
    {
      key: 'completion',
      header: 'Completion',
      render: (item: typeof MOCK_TRAINING[0]) => (
        <div className="flex items-center gap-2">
          <div className="w-16 h-2 bg-white/10 rounded-full overflow-hidden">
            <div 
              className={`h-full rounded-full ${
                item.completionRate === 100 ? 'bg-emerald-500' : 
                item.completionRate >= 75 ? 'bg-cyan-500' : 
                'bg-amber-500'
              }`}
              style={{ width: `${item.completionRate}%` }}
            />
          </div>
          <span className="text-xs text-muted-foreground">{item.completionRate}%</span>
        </div>
      ),
    },
    {
      key: 'dueDate',
      header: 'Due Date',
      render: (item: typeof MOCK_TRAINING[0]) => (
        <span className={`text-sm ${item.dueDate ? 'text-amber-400' : 'text-muted-foreground'}`}>
          {item.dueDate || 'No deadline'}
        </span>
      ),
    },
    {
      key: 'actions',
      header: '',
      width: '80px',
      render: (item: typeof MOCK_TRAINING[0]) => (
        <TableActions>
          <TableActionButton onClick={() => {}}>
            <Edit2 className="w-4 h-4" />
          </TableActionButton>
        </TableActions>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <AdminTabs tabs={IDENTITY_TABS} activeTab={activeTab} onChange={setActiveTab} />

      {/* Users Tab */}
      <TabPanel id="users" activeTab={activeTab}>
        <AdminSection title="User Management" description="Manage users, site assignments, and role mappings">
          <AdminCard
            title="Users"
            icon={Users}
            actions={
              <div className="flex items-center gap-3">
                <TableSearch value={searchQuery} onChange={setSearchQuery} placeholder="Search users..." />
                <Button onClick={() => setIsUserModalOpen(true)}>
                  <Plus className="w-4 h-4" />
                  Invite User
                </Button>
              </div>
            }
          >
            <AdminTable columns={userColumns} data={MOCK_USERS} keyField="id" />
          </AdminCard>
        </AdminSection>
      </TabPanel>

      {/* Roles Tab */}
      <TabPanel id="roles" activeTab={activeTab}>
        <AdminSection title="Roles & Permissions" description="Configure role definitions and permission matrices">
          <AdminCard
            title="Role Definitions"
            icon={Shield}
            actions={
              <Button onClick={() => setIsRoleModalOpen(true)}>
                <Plus className="w-4 h-4" />
                Create Role
              </Button>
            }
          >
            <AdminTable columns={roleColumns} data={MOCK_ROLES} keyField="id" />
          </AdminCard>
        </AdminSection>
      </TabPanel>

      {/* Badges Tab */}
      <TabPanel id="badges" activeTab={activeTab}>
        <AdminSection title="Badge Credentials" description="Manage badge enrollment, credentials, and revocation">
          <AdminCard
            title="Badge Registry"
            icon={CreditCard}
            actions={
              <Button>
                <Plus className="w-4 h-4" />
                Enroll Badge
              </Button>
            }
          >
            <AdminTable columns={badgeColumns} data={MOCK_BADGES} keyField="id" />
          </AdminCard>
        </AdminSection>
      </TabPanel>

      {/* Training Tab */}
      <TabPanel id="training" activeTab={activeTab}>
        <AdminSection title="Training & SOPs" description="Manage training assignments, quizzes, and digital sign-offs">
          <AdminCard
            title="Training Programs"
            icon={GraduationCap}
            actions={
              <Button>
                <Plus className="w-4 h-4" />
                Create Training
              </Button>
            }
          >
            <AdminTable columns={trainingColumns} data={MOCK_TRAINING} keyField="id" />
          </AdminCard>
        </AdminSection>
      </TabPanel>

      {/* User Modal */}
      <AdminModal
        isOpen={isUserModalOpen}
        onClose={() => setIsUserModalOpen(false)}
        title="Invite User"
        description="Add a new user to the system"
        size="lg"
        footer={
          <>
            <Button variant="ghost" onClick={() => setIsUserModalOpen(false)}>Cancel</Button>
            <Button onClick={() => setIsUserModalOpen(false)}>Send Invitation</Button>
          </>
        }
      >
        <div className="space-y-6">
          <div className="grid grid-cols-2 gap-4">
            <FormField label="First Name" required><Input placeholder="John" /></FormField>
            <FormField label="Last Name" required><Input placeholder="Doe" /></FormField>
          </div>
          <FormField label="Email" required><Input type="email" placeholder="john@example.com" /></FormField>
          <FormField label="Role" required><Select options={ROLES_OPTIONS} /></FormField>
          <FormField label="Site Access" required>
            <div className="space-y-2">
              {SITES_OPTIONS.map(site => (
                <Checkbox key={site.value} checked={false} onChange={() => {}} label={site.label} />
              ))}
            </div>
          </FormField>
        </div>
      </AdminModal>

      {/* Role Modal */}
      <AdminModal
        isOpen={isRoleModalOpen}
        onClose={() => setIsRoleModalOpen(false)}
        title="Create Role"
        description="Define a new role with permissions"
        size="lg"
        footer={
          <>
            <Button variant="ghost" onClick={() => setIsRoleModalOpen(false)}>Cancel</Button>
            <Button onClick={() => setIsRoleModalOpen(false)}>Create Role</Button>
          </>
        }
      >
        <div className="space-y-6">
          <FormField label="Role Name" required><Input placeholder="e.g., Shift Supervisor" /></FormField>
          <FormField label="Description"><Input placeholder="Brief description of the role" /></FormField>
          <div className="p-4 bg-white/5 rounded-lg">
            <h4 className="text-sm font-medium text-foreground mb-3">Permissions</h4>
            <div className="grid grid-cols-2 gap-3">
              <Checkbox checked={true} onChange={() => {}} label="View Dashboard" />
              <Checkbox checked={true} onChange={() => {}} label="View Cultivation" />
              <Checkbox checked={false} onChange={() => {}} label="Edit Recipes" />
              <Checkbox checked={false} onChange={() => {}} label="Configure Sensors" />
              <Checkbox checked={false} onChange={() => {}} label="Manage Users" />
              <Checkbox checked={false} onChange={() => {}} label="Access Compliance" />
              <Checkbox checked={false} onChange={() => {}} label="Admin Settings" />
              <Checkbox checked={false} onChange={() => {}} label="Access Simulator" />
            </div>
          </div>
        </div>
      </AdminModal>
    </div>
  );
}

