'use client';

import React, { useState } from 'react';
import {
  Clock,
  Plus,
  Edit2,
  Trash2,
  Calendar,
  Gauge,
  Zap,
} from 'lucide-react';
import {
  AdminCard,
  AdminSection,
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

// Mock data for irrigation schedules
const MOCK_SCHEDULES = [
  {
    id: '1',
    name: 'F1 Morning Ramp',
    program: 'Flower Generative Push',
    scheduleType: 'time',
    triggers: '06:00, 08:00, 10:00, 12:00',
    predicates: 'VWC < 75%',
    quietHours: '18:00 - 06:00',
    enabled: true,
  },
  {
    id: '2',
    name: 'F1 Sensor-Based',
    program: 'Flower Generative Push',
    scheduleType: 'sensor',
    triggers: 'VWC drops below 55%',
    predicates: 'Min soak: 30min, Max shots: 8/day',
    quietHours: '20:00 - 06:00',
    enabled: true,
  },
  {
    id: '3',
    name: 'V1 Hybrid Schedule',
    program: 'Veg Maintenance',
    scheduleType: 'hybrid',
    triggers: 'Every 2h OR VWC < 60%',
    predicates: 'EC drift < 0.3, Temp > 65°F',
    quietHours: '22:00 - 06:00',
    enabled: true,
  },
  {
    id: '4',
    name: 'Prop Mist Cycle',
    program: 'Propagation Mist',
    scheduleType: 'time',
    triggers: 'Every 5min (day), Every 15min (night)',
    predicates: 'RH < 95%',
    quietHours: 'None',
    enabled: false,
  },
];

const SCHEDULE_TYPES = [
  { value: 'time', label: 'Time-based (Cron/Fixed times)' },
  { value: 'sensor', label: 'Sensor-triggered (VWC/EC/pH)' },
  { value: 'hybrid', label: 'Hybrid (Time OR Sensor)' },
];

const PROGRAMS = [
  { value: 'flower-gen', label: 'Flower Generative Push' },
  { value: 'veg-maint', label: 'Veg Maintenance' },
  { value: 'flower-flush', label: 'Flower Flush' },
  { value: 'prop-mist', label: 'Propagation Mist' },
];

export function IrrigationSchedulesSection() {
  const [searchQuery, setSearchQuery] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingSchedule, setEditingSchedule] = useState<typeof MOCK_SCHEDULES[0] | null>(null);

  const filteredSchedules = MOCK_SCHEDULES.filter(
    (schedule) =>
      schedule.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      schedule.program.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const handleEdit = (schedule: typeof MOCK_SCHEDULES[0]) => {
    setEditingSchedule(schedule);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    setEditingSchedule(null);
    setIsModalOpen(true);
  };

  const getTypeIcon = (type: string) => {
    switch (type) {
      case 'time':
        return <Clock className="w-4 h-4 text-cyan-400" />;
      case 'sensor':
        return <Gauge className="w-4 h-4 text-emerald-400" />;
      case 'hybrid':
        return <Zap className="w-4 h-4 text-amber-400" />;
      default:
        return null;
    }
  };

  const columns = [
    {
      key: 'name',
      header: 'Schedule Name',
      sortable: true,
      render: (item: typeof MOCK_SCHEDULES[0]) => (
        <div className="font-medium text-foreground">{item.name}</div>
      ),
    },
    {
      key: 'program',
      header: 'Program',
      render: (item: typeof MOCK_SCHEDULES[0]) => (
        <span className="text-sm text-cyan-400">{item.program}</span>
      ),
    },
    {
      key: 'type',
      header: 'Type',
      render: (item: typeof MOCK_SCHEDULES[0]) => (
        <div className="flex items-center gap-2">
          {getTypeIcon(item.scheduleType)}
          <span className="text-xs capitalize">{item.scheduleType}</span>
        </div>
      ),
    },
    {
      key: 'triggers',
      header: 'Triggers',
      render: (item: typeof MOCK_SCHEDULES[0]) => (
        <span className="text-xs text-muted-foreground">{item.triggers}</span>
      ),
    },
    {
      key: 'predicates',
      header: 'Predicates',
      render: (item: typeof MOCK_SCHEDULES[0]) => (
        <span className="text-xs text-muted-foreground">{item.predicates}</span>
      ),
    },
    {
      key: 'quietHours',
      header: 'Quiet Hours',
      render: (item: typeof MOCK_SCHEDULES[0]) => (
        <span className="text-xs text-muted-foreground">{item.quietHours}</span>
      ),
    },
    {
      key: 'enabled',
      header: 'Status',
      render: (item: typeof MOCK_SCHEDULES[0]) => (
        <StatusBadge status={item.enabled ? 'active' : 'inactive'} />
      ),
    },
    {
      key: 'actions',
      header: '',
      width: '80px',
      render: (item: typeof MOCK_SCHEDULES[0]) => (
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
      title="Irrigation Schedules"
      description="Configure time-based, sensor-triggered, or hybrid irrigation schedules"
    >
      <AdminCard
        title="Schedule Configuration"
        icon={Calendar}
        actions={
          <div className="flex items-center gap-3">
            <TableSearch
              value={searchQuery}
              onChange={setSearchQuery}
              placeholder="Search schedules..."
            />
            <Button onClick={handleCreate}>
              <Plus className="w-4 h-4" />
              Add Schedule
            </Button>
          </div>
        }
      >
        <AdminTable
          columns={columns}
          data={filteredSchedules}
          keyField="id"
          onRowClick={handleEdit}
          emptyMessage="No irrigation schedules configured"
        />
      </AdminCard>

      {/* Create/Edit Modal */}
      <AdminModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingSchedule ? 'Edit Schedule' : 'Create Schedule'}
        description="Configure triggers, predicates, and quiet hours"
        size="lg"
        footer={
          <>
            <Button variant="ghost" onClick={() => setIsModalOpen(false)}>
              Cancel
            </Button>
            <Button onClick={() => setIsModalOpen(false)}>
              {editingSchedule ? 'Save Changes' : 'Create Schedule'}
            </Button>
          </>
        }
      >
        <div className="space-y-6">
          <FormField label="Schedule Name" required>
            <Input 
              placeholder="e.g., F1 Morning Ramp" 
              defaultValue={editingSchedule?.name} 
            />
          </FormField>

          <div className="grid grid-cols-2 gap-4">
            <FormField label="Program" required>
              <Select options={PROGRAMS} defaultValue="flower-gen" />
            </FormField>
            <FormField label="Schedule Type" required>
              <Select options={SCHEDULE_TYPES} defaultValue={editingSchedule?.scheduleType || 'time'} />
            </FormField>
          </div>

          <FormField 
            label="Trigger Configuration" 
            required 
            description="Define when irrigation should occur"
          >
            <Textarea 
              rows={3}
              placeholder="e.g., 06:00, 08:00, 10:00 OR VWC < 55%"
              defaultValue={editingSchedule?.triggers} 
            />
          </FormField>

          <FormField 
            label="Predicates" 
            description="Additional conditions that must be met"
          >
            <Textarea 
              rows={2}
              placeholder="e.g., Min soak: 30min, EC drift < 0.3"
              defaultValue={editingSchedule?.predicates} 
            />
          </FormField>

          <div className="grid grid-cols-2 gap-4">
            <FormField label="Quiet Hours Start">
              <Input 
                type="time" 
                defaultValue="18:00" 
              />
            </FormField>
            <FormField label="Quiet Hours End">
              <Input 
                type="time" 
                defaultValue="06:00" 
              />
            </FormField>
          </div>

          <div className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
            <div>
              <div className="text-sm font-medium text-foreground">Enable Schedule</div>
              <div className="text-xs text-muted-foreground">
                Disabled schedules will not trigger irrigation
              </div>
            </div>
            <Switch checked={editingSchedule?.enabled ?? true} onChange={() => {}} />
          </div>
        </div>
      </AdminModal>
    </AdminSection>
  );
}

