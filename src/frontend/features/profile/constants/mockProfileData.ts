import type { ProfileStateSnapshot, ShiftScheduleItem } from '../types/profile.types';

function toDateString(date: Date): string {
  const yyyy = date.getFullYear();
  const mm = String(date.getMonth() + 1).padStart(2, '0');
  const dd = String(date.getDate()).padStart(2, '0');
  return `${yyyy}-${mm}-${dd}`;
}

function startOfWeekMonday(date: Date): Date {
  const d = new Date(date);
  const day = d.getDay(); // 0=Sun
  const diff = (day === 0 ? -6 : 1) - day;
  d.setDate(d.getDate() + diff);
  d.setHours(0, 0, 0, 0);
  return d;
}

function addDays(date: Date, days: number): Date {
  const d = new Date(date);
  d.setDate(d.getDate() + days);
  return d;
}

function buildMockSchedule(params: {
  userId: string;
  siteId: string;
}): ShiftScheduleItem[] {
  const monday = startOfWeekMonday(new Date());

  return [
    {
      id: `shift-${params.userId}-mon`,
      siteId: params.siteId,
      employeeId: params.userId,
      shiftDate: toDateString(monday),
      startTime: '08:00',
      endTime: '16:00',
      status: 'scheduled',
      roomCode: 'F1',
      task: 'Defoliation / canopy maintenance',
    },
    {
      id: `shift-${params.userId}-tue`,
      siteId: params.siteId,
      employeeId: params.userId,
      shiftDate: toDateString(addDays(monday, 1)),
      startTime: '08:00',
      endTime: '16:00',
      status: 'scheduled',
      roomCode: 'VEG2',
      task: 'Transplant + IPM scouting',
    },
    {
      id: `shift-${params.userId}-wed`,
      siteId: params.siteId,
      employeeId: params.userId,
      shiftDate: toDateString(addDays(monday, 2)),
      startTime: '10:00',
      endTime: '18:00',
      status: 'scheduled',
      roomCode: 'IRR1',
      task: 'Irrigation maintenance',
    },
  ];
}

export function createMockProfileSnapshot(params: {
  userId: string;
  siteId: string;
}): ProfileStateSnapshot {
  return {
    ptoBalance: {
      totalAllowance: 15,
      availableDays: 10.5,
      usedDays: 4,
      pendingDays: 0.5,
    },
    ptoRequests: [
      {
        id: `pto-${params.userId}-1`,
        startDate: toDateString(addDays(new Date(), 7)),
        endDate: toDateString(addDays(new Date(), 7)),
        type: 'personal',
        status: 'pending',
        reason: 'Family appointment',
        submittedAt: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
      },
      {
        id: `pto-${params.userId}-2`,
        startDate: toDateString(addDays(new Date(), -30)),
        endDate: toDateString(addDays(new Date(), -29)),
        type: 'vacation',
        status: 'approved',
        reason: 'Weekend trip',
        submittedAt: new Date(Date.now() - 45 * 24 * 60 * 60 * 1000).toISOString(),
        decidedAt: new Date(Date.now() - 44 * 24 * 60 * 60 * 1000).toISOString(),
        decidedByName: 'Manager',
      },
    ],
    weeklySchedule: buildMockSchedule({ userId: params.userId, siteId: params.siteId }),
  };
}
