/**
 * Employee Types
 * Types for employee profile and labor information management
 */

export type PayType = 'hourly' | 'salary';
export type EmployeeStatus = 'active' | 'inactive' | 'terminated';
export type ShiftStatus = 'scheduled' | 'completed' | 'cancelled';
export type TimeOffType = 'pto' | 'sick' | 'unpaid' | 'other';
export type TimeOffRequestStatus = 'pending' | 'approved' | 'denied';

export interface Certification {
  name: string;
  expiresOn?: string;
}

export interface TeamMembership {
  id: string;
  name: string;
  isTeamLead: boolean;
}

export interface ShiftSchedule {
  id: string;
  date: string;
  startTime: string;
  endTime: string;
  location?: string;
  status: ShiftStatus;
}

export interface TimeOffRequest {
  id: string;
  type: TimeOffType;
  startDate: string;
  endDate: string;
  status: TimeOffRequestStatus;
  notes?: string;
  requestedAt: string;
}

export interface PTOBalance {
  available: number;
  used: number;
  accrued: number;
  pending: number;
}

export interface EmployeeProfile {
  id: string;
  siteId: string;
  firstName: string;
  lastName: string;
  email?: string;
  role: string;
  payType: PayType;
  rate: number;
  status: EmployeeStatus;
  skills: string[];
  certifications: Certification[];
  preferredRooms?: string[];
  availabilityNotes?: string;
  teams: TeamMembership[];
  // HR fields
  birthday?: string;
  hireDate?: string;
  upcomingShifts: ShiftSchedule[];
  timeOffRequests: TimeOffRequest[];
  ptoBalance: PTOBalance;
  createdAt: string;
  updatedAt: string;
}

export interface UpdateEmployeeRequest {
  firstName?: string;
  lastName?: string;
  email?: string;
  role?: string;
  payType?: PayType;
  rate?: number;
  status?: EmployeeStatus;
  preferredRooms?: string[];
  availabilityNotes?: string;
  birthday?: string;
}

export interface SubmitTimeOffRequest {
  type: TimeOffType;
  startDate: string;
  endDate: string;
  notes?: string;
}
