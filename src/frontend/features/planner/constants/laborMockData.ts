import {
  ActionItem,
  BudgetVariance,
  CriticalAlert,
  EmployeeProfile,
  IntegrationStatus,
  LaborMetrics,
  ProductionTask,
  ProductivitySnapshot,
  ScheduleSummary,
  ShiftNeed,
  TimecardApproval,
} from '../types';

// Helper to generate dates relative to today
const getRelativeDate = (daysFromToday: number): string => {
  const date = new Date();
  date.setDate(date.getDate() + daysFromToday);
  return date.toISOString().split('T')[0];
};

// ============================================
// Critical Alerts
// ============================================
export const criticalAlerts: CriticalAlert[] = [
  {
    id: 'alert-1',
    severity: 'critical',
    category: 'staffing_gap',
    title: 'Harvest crew short 3 people',
    description: 'Tomorrow\'s harvest deadline requires 8 workers, only 5 assigned',
    actionLabel: 'Assign Staff',
    actionHref: '/dashboard/planner/shift-board?task=harvest-tomorrow',
    timestamp: new Date().toISOString(),
    dismissable: false,
  },
  {
    id: 'alert-2',
    severity: 'warning',
    category: 'certification_expiring',
    title: 'Certification expiring',
    description: 'Jordan Lee\'s Pesticide Handler cert expires in 5 days',
    actionLabel: 'View Details',
    actionHref: '/dashboard/planner/settings?tab=compliance',
    timestamp: new Date().toISOString(),
    dismissable: true,
  },
];

// ============================================
// Labor Metrics (Top KPIs)
// ============================================
export const laborMetrics: LaborMetrics = {
  coverage: {
    filledPositions: 42,
    requiredPositions: 48,
    percentage: 87.5,
    trend: 'up',
    trendValue: 3.2,
  },
  laborCost: {
    actual: 12150,
    budget: 12800,
    variance: -650,
    variancePercent: -5.1,
    period: 'week',
  },
  overtime: {
    hoursThisWeek: 24.5,
    hoursLastWeek: 18.0,
    trend: 'up',
    cost: 892,
  },
};

// ============================================
// Production Tasks (Week Timeline)
// ============================================
export const productionTasks: ProductionTask[] = [
  {
    id: 'task-1',
    name: 'Zone A Harvest',
    type: 'harvest',
    date: getRelativeDate(0),
    startTime: '06:00',
    endTime: '14:00',
    location: 'Zone A',
    laborStatus: 'fully_staffed',
    assignedCount: 6,
    requiredCount: 6,
    skills: ['Harvest', 'Food Safety'],
  },
  {
    id: 'task-2',
    name: 'Zone B Harvest',
    type: 'harvest',
    date: getRelativeDate(1),
    startTime: '06:00',
    endTime: '14:00',
    location: 'Zone B',
    laborStatus: 'needs_people',
    assignedCount: 5,
    requiredCount: 8,
    skills: ['Harvest', 'Food Safety'],
  },
  {
    id: 'task-3',
    name: 'Processing Run #47',
    type: 'processing',
    date: getRelativeDate(1),
    startTime: '08:00',
    endTime: '16:00',
    location: 'Processing Facility',
    laborStatus: 'fully_staffed',
    assignedCount: 4,
    requiredCount: 4,
    skills: ['Processing', 'QA'],
  },
  {
    id: 'task-4',
    name: 'Packaging - Batch 2024-47',
    type: 'packaging',
    date: getRelativeDate(2),
    startTime: '07:00',
    endTime: '15:00',
    location: 'Packaging Area',
    laborStatus: 'needs_people',
    assignedCount: 3,
    requiredCount: 5,
    skills: ['Packaging', 'Labeling'],
  },
  {
    id: 'task-5',
    name: 'Planting - Greenhouse 3',
    type: 'planting',
    date: getRelativeDate(2),
    startTime: '06:00',
    endTime: '12:00',
    location: 'Greenhouse 3',
    laborStatus: 'fully_staffed',
    assignedCount: 4,
    requiredCount: 4,
    skills: ['Planting', 'Irrigation'],
  },
  {
    id: 'task-6',
    name: 'Equipment Maintenance',
    type: 'maintenance',
    date: getRelativeDate(3),
    startTime: '08:00',
    endTime: '12:00',
    location: 'All Zones',
    laborStatus: 'unassigned',
    assignedCount: 0,
    requiredCount: 2,
    skills: ['Maintenance', 'Equipment'],
  },
  {
    id: 'task-7',
    name: 'Quality Inspection',
    type: 'quality_check',
    date: getRelativeDate(3),
    startTime: '09:00',
    endTime: '11:00',
    location: 'Processing Facility',
    laborStatus: 'fully_staffed',
    assignedCount: 2,
    requiredCount: 2,
    skills: ['QA', 'Compliance'],
  },
  {
    id: 'task-8',
    name: 'Zone C Harvest',
    type: 'harvest',
    date: getRelativeDate(4),
    startTime: '06:00',
    endTime: '14:00',
    location: 'Zone C',
    laborStatus: 'needs_people',
    assignedCount: 4,
    requiredCount: 7,
    skills: ['Harvest', 'Food Safety'],
  },
  {
    id: 'task-9',
    name: 'Processing Run #48',
    type: 'processing',
    date: getRelativeDate(5),
    startTime: '08:00',
    endTime: '16:00',
    location: 'Processing Facility',
    laborStatus: 'unassigned',
    assignedCount: 0,
    requiredCount: 4,
    skills: ['Processing', 'QA'],
  },
  {
    id: 'task-10',
    name: 'Packaging - Batch 2024-48',
    type: 'packaging',
    date: getRelativeDate(6),
    startTime: '07:00',
    endTime: '15:00',
    location: 'Packaging Area',
    laborStatus: 'unassigned',
    assignedCount: 0,
    requiredCount: 5,
    skills: ['Packaging', 'Labeling'],
  },
];

// ============================================
// Today's Schedule
// ============================================
export const todaySchedule: ScheduleSummary = {
  date: getRelativeDate(0),
  totalScheduled: 24,
  totalPresent: 22,
  absences: 2,
  lateArrivals: 1,
  employees: [
    {
      id: 'emp-1',
      name: 'Maria Garcia',
      role: 'Harvest Lead',
      shift: 'morning',
      shiftStart: '06:00',
      shiftEnd: '14:00',
      status: 'clocked_in',
      location: 'Zone A',
    },
    {
      id: 'emp-2',
      name: 'David Kim',
      role: 'Processing Tech',
      shift: 'morning',
      shiftStart: '06:00',
      shiftEnd: '14:00',
      status: 'clocked_in',
      location: 'Processing Facility',
    },
    {
      id: 'emp-3',
      name: 'Sarah Johnson',
      role: 'QA Specialist',
      shift: 'morning',
      shiftStart: '07:00',
      shiftEnd: '15:00',
      status: 'clocked_in',
      location: 'All Zones',
    },
    {
      id: 'emp-4',
      name: 'James Wilson',
      role: 'Harvest Crew',
      shift: 'morning',
      shiftStart: '06:00',
      shiftEnd: '14:00',
      status: 'absent',
      location: 'Zone A',
    },
    {
      id: 'emp-5',
      name: 'Emily Davis',
      role: 'Packaging Lead',
      shift: 'afternoon',
      shiftStart: '14:00',
      shiftEnd: '22:00',
      status: 'scheduled',
      location: 'Packaging Area',
    },
    {
      id: 'emp-6',
      name: 'Michael Brown',
      role: 'Irrigation Tech',
      shift: 'morning',
      shiftStart: '06:00',
      shiftEnd: '14:00',
      status: 'late',
      location: 'Greenhouse 1-3',
    },
    {
      id: 'emp-7',
      name: 'Lisa Chen',
      role: 'Harvest Crew',
      shift: 'morning',
      shiftStart: '06:00',
      shiftEnd: '14:00',
      status: 'absent',
      location: 'Zone A',
    },
    {
      id: 'emp-8',
      name: 'Robert Martinez',
      role: 'Maintenance Tech',
      shift: 'morning',
      shiftStart: '07:00',
      shiftEnd: '15:00',
      status: 'clocked_in',
      location: 'All Zones',
    },
  ],
};

// ============================================
// Action Items
// ============================================
export const actionItems: ActionItem[] = [
  {
    id: 'action-1',
    type: 'timecard_approval',
    title: '3 timecards pending approval',
    description: 'From yesterday\'s shift',
    priority: 'high',
    actionHref: '/dashboard/planner/time-approvals',
    dueDate: getRelativeDate(0),
  },
  {
    id: 'action-2',
    type: 'overtime_approval',
    title: 'Overtime request',
    description: 'Sam Patel - 2.5 hours overtime approval needed',
    priority: 'high',
    actionHref: '/dashboard/planner/time-approvals?filter=overtime',
    relatedEmployee: 'Sam Patel',
  },
  {
    id: 'action-3',
    type: 'scheduling_conflict',
    title: 'Scheduling conflict',
    description: 'Double-booking: Maria Garcia on Zone A & Processing',
    priority: 'medium',
    actionHref: '/dashboard/planner/shift-board?conflict=emp-1',
    relatedEmployee: 'Maria Garcia',
  },
  {
    id: 'action-4',
    type: 'certification_expiring',
    title: 'Certification renewal due',
    description: 'Jordan Lee - Pesticide Handler expires Mar 15',
    priority: 'medium',
    actionHref: '/dashboard/planner/settings?tab=compliance&employee=e2',
    dueDate: '2025-03-15',
    relatedEmployee: 'Jordan Lee',
  },
  {
    id: 'action-5',
    type: 'shift_swap_request',
    title: 'Shift swap request',
    description: 'Emily Davis requests swap with Michael Brown (Dec 15)',
    priority: 'low',
    actionHref: '/dashboard/planner/shift-board?swap=pending',
    relatedEmployee: 'Emily Davis',
  },
];

// ============================================
// Legacy Data (maintained for compatibility)
// ============================================

export const shiftNeeds: ShiftNeed[] = [
  {
    id: 's1',
    role: 'Harvest Crew',
    room: 'Zone B',
    start: '06:00',
    end: '14:00',
    required: 8,
    assigned: 5,
    skills: ['Harvest', 'Food Safety'],
  },
  {
    id: 's2',
    role: 'Processing Tech',
    room: 'Processing Facility',
    start: '08:00',
    end: '16:00',
    required: 4,
    assigned: 4,
    skills: ['Processing', 'QA'],
  },
  {
    id: 's3',
    role: 'Packaging Crew',
    room: 'Packaging Area',
    start: '07:00',
    end: '15:00',
    required: 5,
    assigned: 3,
    skills: ['Packaging', 'Labeling'],
  },
];

export const timecards: TimecardApproval[] = [
  {
    id: 't1',
    employee: 'Avery Chen',
    role: 'Processing Tech',
    status: 'pending',
    hours: 8.0,
    cost: 168,
    exceptions: ['Late clock-in'],
  },
  {
    id: 't2',
    employee: 'Jordan Lee',
    role: 'Irrigation Tech',
    status: 'approved',
    hours: 8.0,
    cost: 240,
  },
  {
    id: 't3',
    employee: 'Sam Patel',
    role: 'Harvest Lead',
    status: 'exception',
    hours: 10.5,
    cost: 325,
    exceptions: ['Overtime pending approval'],
  },
];

export const budgetVariances: BudgetVariance[] = [
  { id: 'b1', label: 'This Week', budget: 12800, actual: 12150, variance: -650 },
  { id: 'b2', label: 'Month to Date', budget: 52100, actual: 54800, variance: 2700 },
];

export const productivity: ProductivitySnapshot[] = [
  {
    id: 'p1',
    label: 'Processing Efficiency',
    metric: 'units/hr',
    value: 420,
    target: 400,
    unit: 'units/hr',
    trend: 'up',
  },
  {
    id: 'p2',
    label: 'Cycle Time',
    metric: 'days',
    value: 11.2,
    target: 12,
    unit: 'days',
    trend: 'down',
  },
  {
    id: 'p3',
    label: 'Units per Labor Hr',
    metric: 'units/hr',
    value: 38,
    target: 40,
    unit: 'units/hr',
    trend: 'flat',
  },
];

export const integrationStatuses: IntegrationStatus[] = [
  {
    id: 'i1',
    name: 'HRIS (ADP)',
    status: 'connected',
    description: 'Syncs employees, compensation, PTO',
  },
  {
    id: 'i2',
    name: 'Payroll Export',
    status: 'pending',
    description: 'CSV export configured, API pending',
  },
  {
    id: 'i3',
    name: 'Equipment Sensors',
    status: 'connected',
    description: 'Real-time equipment status monitoring',
  },
];

export const employees: EmployeeProfile[] = [
  {
    id: 'e1',
    name: 'Avery Chen',
    role: 'Processing Tech',
    skills: ['Processing', 'QA'],
    certifications: [{ name: 'Food Safety', expiresOn: '2025-06-01' }],
    preferredRooms: ['Processing Facility'],
    payType: 'hourly',
    rate: 21,
  },
  {
    id: 'e2',
    name: 'Jordan Lee',
    role: 'Irrigation Tech',
    skills: ['Irrigation', 'IPM'],
    certifications: [{ name: 'Pesticide Handler', expiresOn: '2025-03-15' }],
    preferredRooms: ['Greenhouse 1', 'Greenhouse 2'],
    payType: 'salary',
    rate: 72000,
  },
  {
    id: 'e3',
    name: 'Sam Patel',
    role: 'Harvest Lead',
    skills: ['Harvest', 'Team Lead'],
    certifications: [{ name: 'First Aid', expiresOn: '2026-01-10' }],
    preferredRooms: ['Zone A', 'Zone B'],
    payType: 'hourly',
    rate: 28,
  },
];
