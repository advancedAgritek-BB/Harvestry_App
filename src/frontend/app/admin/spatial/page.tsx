'use client';

import React, { useState } from 'react';
import {
  Building2,
  DoorOpen,
  Grid3X3,
  Layers,
  Box,
  Plus,
  Edit2,
  Trash2,
  ChevronRight,
  MapPin,
  Clock,
  Globe,
  LayoutGrid,
} from 'lucide-react';
import {
  AdminCard,
  AdminSection,
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
  Textarea,
} from '@/components/admin';
import { RoomLayoutSection } from './sections';

const SPATIAL_TABS = [
  { id: 'sites', label: 'Sites', icon: Building2 },
  { id: 'rooms', label: 'Rooms', icon: DoorOpen },
  { id: 'zones', label: 'Zones', icon: Grid3X3 },
  { id: 'room-layouts', label: 'Room Layouts', icon: LayoutGrid },
  { id: 'racks', label: 'Racks & Bins', icon: Layers },
];

// Mock data
const MOCK_SITES = [
  { id: '1', code: 'EVG', name: 'Evergreen Facility', address: '123 Green St, Denver, CO', timezone: 'America/Denver', rooms: 8, status: 'active' },
  { id: '2', code: 'OAK', name: 'Oakdale Campus', address: '456 Oak Ave, Boulder, CO', timezone: 'America/Denver', rooms: 5, status: 'active' },
  { id: '3', code: 'MTN', name: 'Mountain View', address: '789 Peak Rd, Aspen, CO', timezone: 'America/Denver', rooms: 0, status: 'inactive' },
];

const MOCK_ROOMS = [
  { id: '1', site: 'Evergreen', code: 'F1', name: 'Flower Room 1', type: 'Flower', sqft: 2500, zones: 4, status: 'active' },
  { id: '2', site: 'Evergreen', code: 'F2', name: 'Flower Room 2', type: 'Flower', sqft: 2500, zones: 4, status: 'active' },
  { id: '3', site: 'Evergreen', code: 'V1', name: 'Veg Room 1', type: 'Veg', sqft: 1500, zones: 2, status: 'active' },
  { id: '4', site: 'Evergreen', code: 'V2', name: 'Veg Room 2', type: 'Veg', sqft: 1500, zones: 2, status: 'active' },
  { id: '5', site: 'Evergreen', code: 'PROP', name: 'Propagation', type: 'Propagation', sqft: 800, zones: 1, status: 'active' },
  { id: '6', site: 'Evergreen', code: 'DRY', name: 'Dry Room', type: 'Dry/Cure', sqft: 1000, zones: 1, status: 'active' },
  { id: '7', site: 'Oakdale', code: 'F1', name: 'Flower Room 1', type: 'Flower', sqft: 3000, zones: 6, status: 'active' },
];

const MOCK_ZONES = [
  { id: '1', room: 'Flower Room 1', code: 'Z-A', name: 'Zone A', racks: 4, plants: 120, status: 'active' },
  { id: '2', room: 'Flower Room 1', code: 'Z-B', name: 'Zone B', racks: 4, plants: 120, status: 'active' },
  { id: '3', room: 'Flower Room 1', code: 'Z-C', name: 'Zone C', racks: 4, plants: 118, status: 'active' },
  { id: '4', room: 'Flower Room 1', code: 'Z-D', name: 'Zone D', racks: 4, plants: 115, status: 'active' },
  { id: '5', room: 'Veg Room 1', code: 'Z-A', name: 'Zone A', racks: 6, plants: 240, status: 'active' },
  { id: '6', room: 'Veg Room 1', code: 'Z-B', name: 'Zone B', racks: 6, plants: 235, status: 'active' },
];

const MOCK_RACKS = [
  { id: '1', zone: 'Zone A (F1)', code: 'R-01', name: 'Rack 01', bins: 30, capacity: 30, occupied: 28, isVault: false },
  { id: '2', zone: 'Zone A (F1)', code: 'R-02', name: 'Rack 02', bins: 30, capacity: 30, occupied: 30, isVault: false },
  { id: '3', zone: 'Zone A (F1)', code: 'R-03', name: 'Rack 03', bins: 30, capacity: 30, occupied: 29, isVault: false },
  { id: '4', zone: 'Zone A (F1)', code: 'R-04', name: 'Rack 04', bins: 30, capacity: 30, occupied: 25, isVault: false },
  { id: '5', zone: 'Vault', code: 'V-01', name: 'Vault Rack 01', bins: 50, capacity: 50, occupied: 42, isVault: true },
];

const ROOM_TYPES = [
  { value: 'flower', label: 'Flower' },
  { value: 'veg', label: 'Veg' },
  { value: 'propagation', label: 'Propagation' },
  { value: 'dry-cure', label: 'Dry/Cure' },
  { value: 'processing', label: 'Processing' },
  { value: 'storage', label: 'Storage' },
  { value: 'vault', label: 'Vault' },
];

const TIMEZONES = [
  { value: 'America/Denver', label: 'America/Denver (MT)' },
  { value: 'America/Los_Angeles', label: 'America/Los_Angeles (PT)' },
  { value: 'America/Chicago', label: 'America/Chicago (CT)' },
  { value: 'America/New_York', label: 'America/New_York (ET)' },
];

export default function SpatialAdminPage() {
  const [activeTab, setActiveTab] = useState('sites');
  const [searchQuery, setSearchQuery] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);

  // Sites columns
  const siteColumns = [
    {
      key: 'code',
      header: 'Code',
      width: '80px',
      render: (item: typeof MOCK_SITES[0]) => (
        <span className="font-mono text-xs bg-white/5 px-2 py-0.5 rounded">{item.code}</span>
      ),
    },
    {
      key: 'name',
      header: 'Site Name',
      sortable: true,
      render: (item: typeof MOCK_SITES[0]) => (
        <div>
          <div className="font-medium text-foreground">{item.name}</div>
          <div className="text-xs text-muted-foreground flex items-center gap-1">
            <MapPin className="w-3 h-3" />{item.address}
          </div>
        </div>
      ),
    },
    {
      key: 'timezone',
      header: 'Timezone',
      render: (item: typeof MOCK_SITES[0]) => (
        <div className="flex items-center gap-1.5 text-sm text-muted-foreground">
          <Globe className="w-3.5 h-3.5" />{item.timezone.split('/')[1]}
        </div>
      ),
    },
    {
      key: 'rooms',
      header: 'Rooms',
      render: (item: typeof MOCK_SITES[0]) => <span>{item.rooms}</span>,
    },
    {
      key: 'status',
      header: 'Status',
      render: (item: typeof MOCK_SITES[0]) => (
        <StatusBadge status={item.status === 'active' ? 'active' : 'inactive'} />
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

  // Rooms columns
  const roomColumns = [
    {
      key: 'site',
      header: 'Site',
      render: (item: typeof MOCK_ROOMS[0]) => (
        <span className="text-xs text-muted-foreground">{item.site}</span>
      ),
    },
    {
      key: 'code',
      header: 'Code',
      width: '80px',
      render: (item: typeof MOCK_ROOMS[0]) => (
        <span className="font-mono text-xs bg-white/5 px-2 py-0.5 rounded">{item.code}</span>
      ),
    },
    {
      key: 'name',
      header: 'Room Name',
      sortable: true,
      render: (item: typeof MOCK_ROOMS[0]) => (
        <div className="font-medium text-foreground">{item.name}</div>
      ),
    },
    {
      key: 'type',
      header: 'Type',
      render: (item: typeof MOCK_ROOMS[0]) => (
        <span className={`text-xs px-2 py-0.5 rounded ${
          item.type === 'Flower' ? 'bg-violet-500/10 text-violet-400' :
          item.type === 'Veg' ? 'bg-emerald-500/10 text-emerald-400' :
          'bg-amber-500/10 text-amber-400'
        }`}>{item.type}</span>
      ),
    },
    {
      key: 'sqft',
      header: 'Size',
      render: (item: typeof MOCK_ROOMS[0]) => (
        <span className="text-muted-foreground">{item.sqft.toLocaleString()} sqft</span>
      ),
    },
    {
      key: 'zones',
      header: 'Zones',
      render: (item: typeof MOCK_ROOMS[0]) => <span>{item.zones}</span>,
    },
    {
      key: 'status',
      header: 'Status',
      render: (item: typeof MOCK_ROOMS[0]) => (
        <StatusBadge status={item.status === 'active' ? 'active' : 'inactive'} />
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

  // Zones columns
  const zoneColumns = [
    {
      key: 'room',
      header: 'Room',
      render: (item: typeof MOCK_ZONES[0]) => (
        <span className="text-sm text-muted-foreground">{item.room}</span>
      ),
    },
    {
      key: 'code',
      header: 'Code',
      width: '80px',
      render: (item: typeof MOCK_ZONES[0]) => (
        <span className="font-mono text-xs bg-white/5 px-2 py-0.5 rounded">{item.code}</span>
      ),
    },
    {
      key: 'name',
      header: 'Zone Name',
      sortable: true,
      render: (item: typeof MOCK_ZONES[0]) => (
        <div className="font-medium text-foreground">{item.name}</div>
      ),
    },
    {
      key: 'racks',
      header: 'Racks',
      render: (item: typeof MOCK_ZONES[0]) => <span>{item.racks}</span>,
    },
    {
      key: 'plants',
      header: 'Plants',
      render: (item: typeof MOCK_ZONES[0]) => (
        <span className="text-emerald-400">{item.plants}</span>
      ),
    },
    {
      key: 'status',
      header: 'Status',
      render: (item: typeof MOCK_ZONES[0]) => (
        <StatusBadge status={item.status === 'active' ? 'active' : 'inactive'} />
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

  // Racks columns
  const rackColumns = [
    {
      key: 'zone',
      header: 'Zone',
      render: (item: typeof MOCK_RACKS[0]) => (
        <span className="text-sm text-muted-foreground">{item.zone}</span>
      ),
    },
    {
      key: 'code',
      header: 'Code',
      width: '80px',
      render: (item: typeof MOCK_RACKS[0]) => (
        <span className="font-mono text-xs bg-white/5 px-2 py-0.5 rounded">{item.code}</span>
      ),
    },
    {
      key: 'name',
      header: 'Rack Name',
      sortable: true,
      render: (item: typeof MOCK_RACKS[0]) => (
        <div className="flex items-center gap-2">
          <span className="font-medium text-foreground">{item.name}</span>
          {item.isVault && (
            <span className="text-xs bg-amber-500/10 text-amber-400 px-1.5 py-0.5 rounded">Vault</span>
          )}
        </div>
      ),
    },
    {
      key: 'bins',
      header: 'Bins',
      render: (item: typeof MOCK_RACKS[0]) => <span>{item.bins}</span>,
    },
    {
      key: 'occupancy',
      header: 'Occupancy',
      render: (item: typeof MOCK_RACKS[0]) => (
        <div className="flex items-center gap-2">
          <div className="w-16 h-2 bg-white/10 rounded-full overflow-hidden">
            <div 
              className={`h-full rounded-full ${
                item.occupied / item.capacity > 0.9 ? 'bg-amber-500' : 'bg-emerald-500'
              }`}
              style={{ width: `${(item.occupied / item.capacity) * 100}%` }}
            />
          </div>
          <span className="text-xs text-muted-foreground">{item.occupied}/{item.capacity}</span>
        </div>
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

  return (
    <div className="space-y-6">
      <AdminTabs tabs={SPATIAL_TABS} activeTab={activeTab} onChange={setActiveTab} />

      <TabPanel id="sites" activeTab={activeTab}>
        <AdminSection title="Sites" description="Configure sites with timezone and locale settings">
          <AdminCard title="Site Configuration" icon={Building2} actions={
            <div className="flex items-center gap-3">
              <TableSearch value={searchQuery} onChange={setSearchQuery} placeholder="Search sites..." />
              <Button onClick={() => setIsModalOpen(true)}><Plus className="w-4 h-4" />Add Site</Button>
            </div>
          }>
            <AdminTable columns={siteColumns} data={MOCK_SITES} keyField="id" />
          </AdminCard>
        </AdminSection>
      </TabPanel>

      <TabPanel id="rooms" activeTab={activeTab}>
        <AdminSection title="Rooms" description="Configure room types and classifications">
          <AdminCard title="Room Configuration" icon={DoorOpen} actions={
            <div className="flex items-center gap-3">
              <TableSearch value={searchQuery} onChange={setSearchQuery} placeholder="Search rooms..." />
              <Button onClick={() => setIsModalOpen(true)}><Plus className="w-4 h-4" />Add Room</Button>
            </div>
          }>
            <AdminTable columns={roomColumns} data={MOCK_ROOMS} keyField="id" />
          </AdminCard>
        </AdminSection>
      </TabPanel>

      <TabPanel id="zones" activeTab={activeTab}>
        <AdminSection title="Zones" description="Define zones within rooms with target overrides">
          <AdminCard title="Zone Configuration" icon={Grid3X3} actions={
            <div className="flex items-center gap-3">
              <TableSearch value={searchQuery} onChange={setSearchQuery} placeholder="Search zones..." />
              <Button onClick={() => setIsModalOpen(true)}><Plus className="w-4 h-4" />Add Zone</Button>
            </div>
          }>
            <AdminTable columns={zoneColumns} data={MOCK_ZONES} keyField="id" />
          </AdminCard>
        </AdminSection>
      </TabPanel>

      <TabPanel id="room-layouts" activeTab={activeTab}>
        <RoomLayoutSection />
      </TabPanel>

      <TabPanel id="racks" activeTab={activeTab}>
        <AdminSection title="Racks & Bins" description="Configure rack layouts and bin inventory locations">
          <AdminCard title="Rack Configuration" icon={Layers} actions={
            <div className="flex items-center gap-3">
              <TableSearch value={searchQuery} onChange={setSearchQuery} placeholder="Search racks..." />
              <Button onClick={() => setIsModalOpen(true)}><Plus className="w-4 h-4" />Add Rack</Button>
            </div>
          }>
            <AdminTable columns={rackColumns} data={MOCK_RACKS} keyField="id" />
          </AdminCard>
        </AdminSection>
      </TabPanel>

      <AdminModal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} title="Add Item" size="lg"
        footer={<><Button variant="ghost" onClick={() => setIsModalOpen(false)}>Cancel</Button><Button onClick={() => setIsModalOpen(false)}>Save</Button></>}>
        <div className="space-y-4">
          <FormField label="Code" required><Input placeholder="Enter code" /></FormField>
          <FormField label="Name" required><Input placeholder="Enter name" /></FormField>
        </div>
      </AdminModal>
    </div>
  );
}

