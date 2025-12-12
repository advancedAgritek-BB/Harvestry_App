export type PTORequestType = 'vacation' | 'sick' | 'personal';
export type PTORequestStatus = 'pending' | 'approved' | 'denied' | 'cancelled';

export interface PTOBalance {
  availableDays: number;
  usedDays: number;
  pendingDays: number;
  totalAllowance: number;
}

export interface PTORequest {
  id: string;
  startDate: string; // YYYY-MM-DD
  endDate: string; // YYYY-MM-DD
  type: PTORequestType;
  status: PTORequestStatus;
  reason?: string;
  submittedAt: string; // ISO
  decidedAt?: string; // ISO
  decidedByName?: string;
}

export type ShiftStatus = 'scheduled' | 'completed' | 'missed' | 'cancelled';

/**
 * Frontend schedule item aligned to backend ShiftAssignment concepts.
 */
export interface ShiftScheduleItem {
  id: string;
  siteId: string;
  employeeId: string;
  shiftDate: string; // YYYY-MM-DD
  startTime: string; // HH:mm
  endTime: string; // HH:mm
  status: ShiftStatus;
  roomCode?: string;
  task?: string;
}

export interface ProfileStateSnapshot {
  ptoBalance: PTOBalance;
  ptoRequests: PTORequest[];
  weeklySchedule: ShiftScheduleItem[];
}

export interface CreatePTORequestInput {
  startDate: string;
  endDate: string;
  type: PTORequestType;
  reason?: string;
}
