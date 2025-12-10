import {
  BudgetVariance,
  EmployeeProfile,
  IntegrationStatus,
  LaborCoverageCard,
  ProductivitySnapshot,
  ShiftNeed,
  TimecardApproval,
} from '../types';

export const laborCoverage: LaborCoverageCard[] = [
  { id: 'veg', label: 'Veg Coverage', phase: 'Vegetative', room: 'Veg A/B', filled: 6, required: 8 },
  { id: 'flower', label: 'Flower Coverage', phase: 'Flower', room: 'FLR 1/2', filled: 10, required: 12 },
  { id: 'dry', label: 'Dry/Cure Coverage', phase: 'Dry & Cure', room: 'Dry/Cure', filled: 3, required: 3 },
];

export const shiftNeeds: ShiftNeed[] = [
  { id: 's1', role: 'Trimmer', room: 'FLR-1', start: '07:00', end: '15:00', required: 5, assigned: 4, skills: ['Trimming', 'QA'] },
  { id: 's2', role: 'Irrigation Tech', room: 'VEG-A', start: '08:00', end: '16:00', required: 2, assigned: 2, skills: ['Irrigation', 'IPM'] },
  { id: 's3', role: 'Harvest Crew', room: 'FLR-2', start: '09:00', end: '17:00', required: 4, assigned: 3, skills: ['Harvest', 'Food Safety'] },
];

export const timecards: TimecardApproval[] = [
  { id: 't1', employee: 'Avery Chen', role: 'Trimmer', status: 'pending', hours: 8.0, cost: 168, exceptions: ['Late clock-in'] },
  { id: 't2', employee: 'Jordan Lee', role: 'Irrigation Tech', status: 'approved', hours: 8.0, cost: 240 },
  { id: 't3', employee: 'Sam Patel', role: 'Harvest Lead', status: 'exception', hours: 10.5, cost: 325, exceptions: ['Overtime pending approval'] },
];

export const budgetVariances: BudgetVariance[] = [
  { id: 'b1', label: 'Week', budget: 12800, actual: 12150, variance: -650 },
  { id: 'b2', label: 'Month', budget: 52100, actual: 54800, variance: 2700 },
  { id: 'b3', label: 'Batch FLR-12', budget: 8600, actual: 8025, variance: -575 },
];

export const productivity: ProductivitySnapshot[] = [
  { id: 'p1', label: 'Trim Efficiency', metric: 'g/hr', value: 420, target: 400, unit: 'g/hr', trend: 'up' },
  { id: 'p2', label: 'Room Turnover', metric: 'days', value: 11.2, target: 12, unit: 'days', trend: 'down' },
  { id: 'p3', label: 'Units per Labor Hr', metric: 'units/hr', value: 38, target: 40, unit: 'units/hr', trend: 'flat' },
];

export const integrationStatuses: IntegrationStatus[] = [
  { id: 'i1', name: 'HRIS (ADP)', status: 'connected', description: 'Syncs employees, compensation, PTO' },
  { id: 'i2', name: 'Payroll Export', status: 'pending', description: 'CSV export configured, API pending' },
  { id: 'i3', name: 'Telemetry (Growlink)', status: 'connected', description: 'Room climate events mapped to tasks' },
];

export const employees: EmployeeProfile[] = [
  { id: 'e1', name: 'Avery Chen', role: 'Trimmer', skills: ['Trimming', 'QA'], certifications: [{ name: 'Food Safety', expiresOn: '2025-06-01' }], preferredRooms: ['FLR-1'], payType: 'hourly', rate: 21 },
  { id: 'e2', name: 'Jordan Lee', role: 'Irrigation Tech', skills: ['Irrigation', 'IPM'], certifications: [{ name: 'Pesticide Handler', expiresOn: '2025-03-15' }], preferredRooms: ['VEG-A', 'VEG-B'], payType: 'salary', rate: 72000 },
  { id: 'e3', name: 'Sam Patel', role: 'Harvest Lead', skills: ['Harvest', 'Team Lead'], certifications: [{ name: 'First Aid', expiresOn: '2026-01-10' }], preferredRooms: ['FLR-2'], payType: 'hourly', rate: 28 },
];


