'use client';

import React, { useState } from 'react';
import {
  Cpu,
  Link2,
  Wrench,
  Activity,
  Plus,
  Edit2,
  Trash2,
  Wifi,
  WifiOff,
  AlertTriangle,
  CheckCircle,
  Clock,
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
} from '@/components/admin';

const EQUIPMENT_TABS = [
  { id: 'devices', label: 'Devices', icon: Cpu },
  { id: 'links', label: 'Equipment Links', icon: Link2 },
  { id: 'calibration', label: 'Calibration', icon: Wrench },
  { id: 'health', label: 'Device Health', icon: Activity },
];

// Mock data
const MOCK_DEVICES = [
  { id: '1', code: 'CTRL-001', name: 'Main Controller F1', type: 'Controller', protocol: 'MQTT', location: 'Flower Room 1', status: 'online', lastSeen: '2 min ago' },
  { id: '2', code: 'SENSOR-001', name: 'Temp/RH Sensor F1-A', type: 'Sensor', protocol: 'Modbus', location: 'Zone A (F1)', status: 'online', lastSeen: '30 sec ago' },
  { id: '3', code: 'SENSOR-002', name: 'Temp/RH Sensor F1-B', type: 'Sensor', protocol: 'Modbus', location: 'Zone B (F1)', status: 'online', lastSeen: '30 sec ago' },
  { id: '4', code: 'PUMP-001', name: 'Main Irrigation Pump', type: 'Pump', protocol: 'Modbus', location: 'Pump Room', status: 'online', lastSeen: '1 min ago' },
  { id: '5', code: 'VALVE-001', name: 'Zone A Valve', type: 'Valve', protocol: 'BACnet', location: 'Zone A (F1)', status: 'online', lastSeen: '1 min ago' },
  { id: '6', code: 'SENSOR-003', name: 'CO2 Sensor F1', type: 'Sensor', protocol: 'SDI-12', location: 'Flower Room 1', status: 'offline', lastSeen: '2 hours ago' },
];

const MOCK_LINKS = [
  { id: '1', sensor: 'Temp/RH Sensor F1-A', zone: 'Zone A (F1)', metric: 'Temperature, Humidity', active: true },
  { id: '2', sensor: 'Temp/RH Sensor F1-B', zone: 'Zone B (F1)', metric: 'Temperature, Humidity', active: true },
  { id: '3', valve: 'Zone A Valve', zone: 'Zone A (F1)', controller: 'Main Controller F1', active: true },
  { id: '4', sensor: 'CO2 Sensor F1', zone: 'All Zones (F1)', metric: 'CO2', active: false },
];

const MOCK_CALIBRATION = [
  { id: '1', equipment: 'Temp/RH Sensor F1-A', lastCalibrated: '2025-10-15', nextDue: '2026-01-15', status: 'current', technician: 'Mike T.' },
  { id: '2', equipment: 'Temp/RH Sensor F1-B', lastCalibrated: '2025-10-15', nextDue: '2026-01-15', status: 'current', technician: 'Mike T.' },
  { id: '3', equipment: 'EC Probe Tank A', lastCalibrated: '2025-11-01', nextDue: '2025-12-01', status: 'due_soon', technician: 'Mike T.' },
  { id: '4', equipment: 'pH Probe Tank A', lastCalibrated: '2025-09-15', nextDue: '2025-11-15', status: 'overdue', technician: 'Mike T.' },
];

const MOCK_HEALTH = [
  { id: '1', device: 'Main Controller F1', uptime: '99.8%', lastRestart: '2025-11-01', faults: 0, status: 'healthy' },
  { id: '2', device: 'Temp/RH Sensor F1-A', uptime: '99.9%', lastRestart: '2025-10-15', faults: 0, status: 'healthy' },
  { id: '3', device: 'Main Irrigation Pump', uptime: '98.5%', lastRestart: '2025-11-20', faults: 2, status: 'warning' },
  { id: '4', device: 'CO2 Sensor F1', uptime: '45.2%', lastRestart: '2025-11-24', faults: 5, status: 'critical' },
];

const DEVICE_TYPES = [
  { value: 'controller', label: 'Controller' },
  { value: 'sensor', label: 'Sensor' },
  { value: 'pump', label: 'Pump' },
  { value: 'valve', label: 'Valve' },
  { value: 'injector', label: 'Injector' },
  { value: 'hvac', label: 'HVAC' },
  { value: 'lighting', label: 'Lighting' },
];

const PROTOCOLS = [
  { value: 'mqtt', label: 'MQTT' },
  { value: 'modbus', label: 'Modbus' },
  { value: 'bacnet', label: 'BACnet' },
  { value: 'sdi12', label: 'SDI-12' },
  { value: 'http', label: 'HTTP/REST' },
];

export default function EquipmentAdminPage() {
  const [activeTab, setActiveTab] = useState('devices');
  const [searchQuery, setSearchQuery] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);

  const deviceColumns = [
    {
      key: 'code',
      header: 'Code',
      width: '120px',
      render: (item: typeof MOCK_DEVICES[0]) => (
        <span className="font-mono text-xs bg-white/5 px-2 py-0.5 rounded">{item.code}</span>
      ),
    },
    {
      key: 'name',
      header: 'Device Name',
      sortable: true,
      render: (item: typeof MOCK_DEVICES[0]) => (
        <div className="font-medium text-foreground">{item.name}</div>
      ),
    },
    {
      key: 'type',
      header: 'Type',
      render: (item: typeof MOCK_DEVICES[0]) => (
        <span className="text-xs bg-white/5 px-2 py-0.5 rounded">{item.type}</span>
      ),
    },
    {
      key: 'protocol',
      header: 'Protocol',
      render: (item: typeof MOCK_DEVICES[0]) => (
        <span className="text-xs text-cyan-400">{item.protocol}</span>
      ),
    },
    {
      key: 'location',
      header: 'Location',
      render: (item: typeof MOCK_DEVICES[0]) => (
        <span className="text-sm text-muted-foreground">{item.location}</span>
      ),
    },
    {
      key: 'status',
      header: 'Status',
      render: (item: typeof MOCK_DEVICES[0]) => (
        <div className="flex items-center gap-2">
          {item.status === 'online' ? (
            <Wifi className="w-4 h-4 text-emerald-400" />
          ) : (
            <WifiOff className="w-4 h-4 text-rose-400" />
          )}
          <span className={item.status === 'online' ? 'text-emerald-400' : 'text-rose-400'}>
            {item.status}
          </span>
        </div>
      ),
    },
    {
      key: 'lastSeen',
      header: 'Last Seen',
      render: (item: typeof MOCK_DEVICES[0]) => (
        <span className="text-xs text-muted-foreground">{item.lastSeen}</span>
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

  const calibrationColumns = [
    {
      key: 'equipment',
      header: 'Equipment',
      sortable: true,
      render: (item: typeof MOCK_CALIBRATION[0]) => (
        <div className="font-medium text-foreground">{item.equipment}</div>
      ),
    },
    {
      key: 'lastCalibrated',
      header: 'Last Calibrated',
      render: (item: typeof MOCK_CALIBRATION[0]) => (
        <span className="text-sm text-muted-foreground">{item.lastCalibrated}</span>
      ),
    },
    {
      key: 'nextDue',
      header: 'Next Due',
      render: (item: typeof MOCK_CALIBRATION[0]) => (
        <span className={`text-sm ${
          item.status === 'overdue' ? 'text-rose-400' :
          item.status === 'due_soon' ? 'text-amber-400' :
          'text-muted-foreground'
        }`}>{item.nextDue}</span>
      ),
    },
    {
      key: 'status',
      header: 'Status',
      render: (item: typeof MOCK_CALIBRATION[0]) => (
        <StatusBadge 
          status={item.status === 'current' ? 'active' : item.status === 'due_soon' ? 'warning' : 'error'}
          label={item.status === 'current' ? 'Current' : item.status === 'due_soon' ? 'Due Soon' : 'Overdue'}
        />
      ),
    },
    {
      key: 'technician',
      header: 'Technician',
      render: (item: typeof MOCK_CALIBRATION[0]) => (
        <span className="text-sm text-muted-foreground">{item.technician}</span>
      ),
    },
    {
      key: 'actions',
      header: '',
      width: '100px',
      render: () => (
        <Button size="sm" variant="secondary">Log Calibration</Button>
      ),
    },
  ];

  const healthColumns = [
    {
      key: 'device',
      header: 'Device',
      sortable: true,
      render: (item: typeof MOCK_HEALTH[0]) => (
        <div className="font-medium text-foreground">{item.device}</div>
      ),
    },
    {
      key: 'uptime',
      header: 'Uptime',
      render: (item: typeof MOCK_HEALTH[0]) => (
        <span className={parseFloat(item.uptime) >= 99 ? 'text-emerald-400' : parseFloat(item.uptime) >= 95 ? 'text-amber-400' : 'text-rose-400'}>
          {item.uptime}
        </span>
      ),
    },
    {
      key: 'lastRestart',
      header: 'Last Restart',
      render: (item: typeof MOCK_HEALTH[0]) => (
        <span className="text-sm text-muted-foreground">{item.lastRestart}</span>
      ),
    },
    {
      key: 'faults',
      header: 'Faults',
      render: (item: typeof MOCK_HEALTH[0]) => (
        <span className={item.faults === 0 ? 'text-emerald-400' : item.faults < 3 ? 'text-amber-400' : 'text-rose-400'}>
          {item.faults}
        </span>
      ),
    },
    {
      key: 'status',
      header: 'Health',
      render: (item: typeof MOCK_HEALTH[0]) => (
        <div className="flex items-center gap-2">
          {item.status === 'healthy' ? (
            <CheckCircle className="w-4 h-4 text-emerald-400" />
          ) : item.status === 'warning' ? (
            <AlertTriangle className="w-4 h-4 text-amber-400" />
          ) : (
            <AlertTriangle className="w-4 h-4 text-rose-400" />
          )}
          <span className="capitalize">{item.status}</span>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <AdminTabs tabs={EQUIPMENT_TABS} activeTab={activeTab} onChange={setActiveTab} />

      <TabPanel id="devices" activeTab={activeTab}>
        <AdminSection title="Equipment Management" description="Register and manage devices">
          <AdminCard title="Device Registry" icon={Cpu} actions={
            <div className="flex items-center gap-3">
              <TableSearch value={searchQuery} onChange={setSearchQuery} placeholder="Search devices..." />
              <Button onClick={() => setIsModalOpen(true)}><Plus className="w-4 h-4" />Add Device</Button>
            </div>
          }>
            <AdminTable columns={deviceColumns} data={MOCK_DEVICES} keyField="id" />
          </AdminCard>
        </AdminSection>
      </TabPanel>

      <TabPanel id="links" activeTab={activeTab}>
        <AdminSection title="Equipment Links" description="Configure sensor-to-zone and controller mappings">
          <AdminCard title="Link Configuration" icon={Link2} actions={
            <Button><Plus className="w-4 h-4" />Add Link</Button>
          }>
            <div className="text-center py-8 text-muted-foreground">
              Link configuration coming soon
            </div>
          </AdminCard>
        </AdminSection>
      </TabPanel>

      <TabPanel id="calibration" activeTab={activeTab}>
        <AdminSection title="Calibration Logs" description="Track calibration history and upcoming due dates">
          <AdminCard title="Calibration Status" icon={Wrench}>
            <AdminTable columns={calibrationColumns} data={MOCK_CALIBRATION} keyField="id" />
          </AdminCard>
        </AdminSection>
      </TabPanel>

      <TabPanel id="health" activeTab={activeTab}>
        <AdminSection title="Device Health" description="Monitor device uptime and fault detection">
          <AdminCard title="Health Dashboard" icon={Activity}>
            <AdminTable columns={healthColumns} data={MOCK_HEALTH} keyField="id" />
          </AdminCard>
        </AdminSection>
      </TabPanel>

      <AdminModal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} title="Add Device" size="lg"
        footer={<><Button variant="ghost" onClick={() => setIsModalOpen(false)}>Cancel</Button><Button onClick={() => setIsModalOpen(false)}>Save</Button></>}>
        <div className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <FormField label="Device Code" required><Input placeholder="e.g., SENSOR-001" /></FormField>
            <FormField label="Device Name" required><Input placeholder="e.g., Temp/RH Sensor F1-A" /></FormField>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <FormField label="Device Type" required><Select options={DEVICE_TYPES} /></FormField>
            <FormField label="Protocol" required><Select options={PROTOCOLS} /></FormField>
          </div>
          <FormField label="Location"><Input placeholder="e.g., Zone A (F1)" /></FormField>
        </div>
      </AdminModal>
    </div>
  );
}

