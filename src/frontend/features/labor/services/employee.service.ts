/**
 * Employee Service
 * API operations for employee profile and labor information management
 */

import type {
  EmployeeProfile,
  Certification,
  UpdateEmployeeRequest,
  ShiftSchedule,
  TimeOffRequest,
  SubmitTimeOffRequest,
} from '../types/employee.types';

const API_BASE = '/api/v1';
const USE_MOCK = process.env.NEXT_PUBLIC_USE_MOCK_AUTH === 'true';

// Helper to generate dates relative to today
const getRelativeDate = (daysFromToday: number): string => {
  const date = new Date();
  date.setDate(date.getDate() + daysFromToday);
  return date.toISOString().split('T')[0];
};

// Mock data for development
const mockEmployees: Record<string, EmployeeProfile> = {
  'u1': {
    id: 'u1',
    siteId: 'site-1',
    firstName: 'Marcus',
    lastName: 'Johnson',
    email: 'marcus.johnson@harvestry.io',
    role: 'Grower',
    payType: 'hourly',
    rate: 22.50,
    status: 'active',
    skills: ['Irrigation Management', 'IPM', 'Nutrient Mixing'],
    certifications: [
      { name: 'Pesticide Applicator License', expiresOn: '2026-03-15' },
      { name: 'OSHA Safety Certification', expiresOn: '2025-12-01' },
    ],
    preferredRooms: ['Flower Room A', 'Veg Room 1'],
    availabilityNotes: 'Available Mon-Fri, prefers morning shifts',
    teams: [{ id: 'team-1', name: 'Day Shift', isTeamLead: true }],
    birthday: '1988-05-22',
    hireDate: '2024-01-15',
    upcomingShifts: [],
    timeOffRequests: [],
    ptoBalance: { available: 80, used: 40, accrued: 120, pending: 0 },
    createdAt: '2024-01-15T08:00:00Z',
    updatedAt: '2025-11-20T14:30:00Z',
  },
  'u2': {
    id: 'u2',
    siteId: 'site-1',
    firstName: 'Sarah',
    lastName: 'Chen',
    email: 'sarah.chen@harvestry.io',
    role: 'Lead Grower',
    payType: 'salary',
    rate: 65000,
    status: 'active',
    skills: ['Crop Steering', 'Environmental Controls', 'Team Leadership', 'Quality Assurance'],
    certifications: [
      { name: 'Advanced Cultivation Certificate', expiresOn: '2026-06-30' },
    ],
    preferredRooms: ['Flower Room A', 'Flower Room B'],
    availabilityNotes: 'Flexible schedule, can cover evenings',
    teams: [{ id: 'team-1', name: 'Day Shift', isTeamLead: false }],
    birthday: '1992-11-08',
    hireDate: '2023-08-10',
    upcomingShifts: [],
    timeOffRequests: [],
    ptoBalance: { available: 96, used: 24, accrued: 120, pending: 16 },
    createdAt: '2023-08-10T08:00:00Z',
    updatedAt: '2025-10-15T09:00:00Z',
  },
  'u3': {
    id: 'u3',
    siteId: 'site-1',
    firstName: 'David',
    lastName: 'Martinez',
    email: 'david.martinez@harvestry.io',
    role: 'Technician',
    payType: 'hourly',
    rate: 28.00,
    status: 'active',
    skills: ['HVAC', 'Electrical', 'Plumbing', 'Equipment Calibration'],
    certifications: [
      { name: 'HVAC Certification', expiresOn: '2025-09-30' },
      { name: 'Electrical License', expiresOn: '2026-01-15' },
    ],
    preferredRooms: [],
    availabilityNotes: 'On-call for emergencies',
    teams: [{ id: 'team-1', name: 'Day Shift', isTeamLead: false }],
    birthday: '1985-03-14',
    hireDate: '2024-03-20',
    upcomingShifts: [],
    timeOffRequests: [],
    ptoBalance: { available: 56, used: 8, accrued: 64, pending: 0 },
    createdAt: '2024-03-20T08:00:00Z',
    updatedAt: '2025-11-01T16:00:00Z',
  },
  'u4': {
    id: 'u4',
    siteId: 'site-1',
    firstName: 'Emily',
    lastName: 'Rodriguez',
    email: 'emily.rodriguez@harvestry.io',
    role: 'Grower',
    payType: 'hourly',
    rate: 20.00,
    status: 'active',
    skills: ['Cloning', 'Transplanting', 'Plant Training'],
    certifications: [],
    preferredRooms: ['Propagation', 'Veg Room 2'],
    availabilityNotes: '',
    teams: [{ id: 'team-2', name: 'Flower Room Team', isTeamLead: false }],
    birthday: '1995-07-30',
    hireDate: '2024-06-01',
    upcomingShifts: [],
    timeOffRequests: [],
    ptoBalance: { available: 40, used: 8, accrued: 48, pending: 8 },
    createdAt: '2024-06-01T08:00:00Z',
    updatedAt: '2025-08-20T11:00:00Z',
  },
  'u5': {
    id: 'u5',
    siteId: 'site-1',
    firstName: 'James',
    lastName: 'Wilson',
    email: 'james.wilson@harvestry.io',
    role: 'Grower',
    payType: 'hourly',
    rate: 21.00,
    status: 'active',
    skills: ['Harvesting', 'Trimming', 'Drying'],
    certifications: [
      { name: 'Food Handler Certificate', expiresOn: '2025-08-15' },
    ],
    preferredRooms: ['Dry Room', 'Trim Room'],
    availabilityNotes: 'Prefers afternoon shifts',
    teams: [{ id: 'team-2', name: 'Flower Room Team', isTeamLead: false }],
    birthday: '1990-12-03',
    hireDate: '2024-04-15',
    upcomingShifts: [],
    timeOffRequests: [],
    ptoBalance: { available: 48, used: 16, accrued: 64, pending: 0 },
    createdAt: '2024-04-15T08:00:00Z',
    updatedAt: '2025-09-10T13:00:00Z',
  },
};

// Generate dynamic upcoming shifts based on current date
function generateUpcomingShifts(userId: string): ShiftSchedule[] {
  const locations: Record<string, string[]> = {
    'u1': ['Flower Room A', 'Veg Room 1'],
    'u2': ['Flower Room A', 'Flower Room B'],
    'u3': ['All Zones'],
    'u4': ['Propagation', 'Veg Room 2'],
    'u5': ['Dry Room', 'Trim Room'],
  };
  
  const userLocations = locations[userId] || ['Main Floor'];
  const shifts: ShiftSchedule[] = [];
  
  for (let i = 0; i < 7; i++) {
    // Skip weekends for most employees
    const date = new Date();
    date.setDate(date.getDate() + i);
    const dayOfWeek = date.getDay();
    
    if (dayOfWeek === 0 || dayOfWeek === 6) continue; // Skip Sunday and Saturday
    
    shifts.push({
      id: `shift-${userId}-${i}`,
      date: getRelativeDate(i),
      startTime: userId === 'u5' ? '14:00' : '06:00',
      endTime: userId === 'u5' ? '22:00' : '14:00',
      location: userLocations[i % userLocations.length],
      status: i === 0 ? 'scheduled' : 'scheduled',
    });
  }
  
  return shifts.slice(0, 5); // Return up to 5 shifts
}

// Generate time off requests
function generateTimeOffRequests(userId: string): TimeOffRequest[] {
  const requests: Record<string, TimeOffRequest[]> = {
    'u1': [],
    'u2': [
      {
        id: 'tor-u2-1',
        type: 'pto',
        startDate: getRelativeDate(14),
        endDate: getRelativeDate(15),
        status: 'pending',
        notes: 'Family event',
        requestedAt: getRelativeDate(-2) + 'T10:00:00Z',
      },
    ],
    'u3': [],
    'u4': [
      {
        id: 'tor-u4-1',
        type: 'pto',
        startDate: getRelativeDate(21),
        endDate: getRelativeDate(22),
        status: 'pending',
        notes: 'Vacation',
        requestedAt: getRelativeDate(-5) + 'T14:00:00Z',
      },
      {
        id: 'tor-u4-2',
        type: 'sick',
        startDate: getRelativeDate(-10),
        endDate: getRelativeDate(-10),
        status: 'approved',
        notes: 'Doctor appointment',
        requestedAt: getRelativeDate(-12) + 'T08:00:00Z',
      },
    ],
    'u5': [
      {
        id: 'tor-u5-1',
        type: 'pto',
        startDate: getRelativeDate(-30),
        endDate: getRelativeDate(-28),
        status: 'approved',
        notes: 'Holiday travel',
        requestedAt: getRelativeDate(-45) + 'T09:00:00Z',
      },
    ],
  };
  
  return requests[userId] || [];
}

/**
 * Get employee profile by user ID
 */
export async function getEmployee(siteId: string, userId: string): Promise<EmployeeProfile> {
  if (USE_MOCK) {
    await new Promise(resolve => setTimeout(resolve, 300));
    const employee = mockEmployees[userId];
    if (!employee) {
      throw new Error('Employee not found');
    }
    // Dynamically generate shifts and time off requests
    return {
      ...employee,
      upcomingShifts: generateUpcomingShifts(userId),
      timeOffRequests: generateTimeOffRequests(userId),
    };
  }

  const response = await fetch(`${API_BASE}/sites/${siteId}/employees/${userId}`);
  if (!response.ok) {
    if (response.status === 404) throw new Error('Employee not found');
    throw new Error('Failed to fetch employee');
  }
  return response.json();
}

/**
 * Update employee profile
 */
export async function updateEmployee(
  siteId: string,
  userId: string,
  data: UpdateEmployeeRequest
): Promise<EmployeeProfile> {
  if (USE_MOCK) {
    await new Promise(resolve => setTimeout(resolve, 300));
    const employee = mockEmployees[userId];
    if (!employee) {
      throw new Error('Employee not found');
    }
    
    const updated: EmployeeProfile = {
      ...employee,
      ...data,
      updatedAt: new Date().toISOString(),
    };
    mockEmployees[userId] = updated;
    return { ...updated };
  }

  const response = await fetch(`${API_BASE}/sites/${siteId}/employees/${userId}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data),
  });
  if (!response.ok) throw new Error('Failed to update employee');
  return response.json();
}

/**
 * Add a skill to an employee
 */
export async function addSkill(siteId: string, userId: string, skill: string): Promise<void> {
  if (USE_MOCK) {
    await new Promise(resolve => setTimeout(resolve, 200));
    const employee = mockEmployees[userId];
    if (!employee) throw new Error('Employee not found');
    
    if (!employee.skills.includes(skill)) {
      employee.skills.push(skill);
      employee.updatedAt = new Date().toISOString();
    }
    return;
  }

  const response = await fetch(`${API_BASE}/sites/${siteId}/employees/${userId}/skills`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ skill }),
  });
  if (!response.ok) throw new Error('Failed to add skill');
}

/**
 * Remove a skill from an employee
 */
export async function removeSkill(siteId: string, userId: string, skill: string): Promise<void> {
  if (USE_MOCK) {
    await new Promise(resolve => setTimeout(resolve, 200));
    const employee = mockEmployees[userId];
    if (!employee) throw new Error('Employee not found');
    
    employee.skills = employee.skills.filter(s => s !== skill);
    employee.updatedAt = new Date().toISOString();
    return;
  }

  const response = await fetch(
    `${API_BASE}/sites/${siteId}/employees/${userId}/skills/${encodeURIComponent(skill)}`,
    { method: 'DELETE' }
  );
  if (!response.ok) throw new Error('Failed to remove skill');
}

/**
 * Add a certification to an employee
 */
export async function addCertification(
  siteId: string,
  userId: string,
  name: string,
  expiresOn?: string
): Promise<void> {
  if (USE_MOCK) {
    await new Promise(resolve => setTimeout(resolve, 200));
    const employee = mockEmployees[userId];
    if (!employee) throw new Error('Employee not found');
    
    const existingIndex = employee.certifications.findIndex(c => c.name === name);
    if (existingIndex === -1) {
      employee.certifications.push({ name, expiresOn });
    } else {
      employee.certifications[existingIndex] = { name, expiresOn };
    }
    employee.updatedAt = new Date().toISOString();
    return;
  }

  const response = await fetch(`${API_BASE}/sites/${siteId}/employees/${userId}/certifications`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ name, expiresOn }),
  });
  if (!response.ok) throw new Error('Failed to add certification');
}

/**
 * Remove a certification from an employee
 */
export async function removeCertification(
  siteId: string,
  userId: string,
  name: string
): Promise<void> {
  if (USE_MOCK) {
    await new Promise(resolve => setTimeout(resolve, 200));
    const employee = mockEmployees[userId];
    if (!employee) throw new Error('Employee not found');
    
    employee.certifications = employee.certifications.filter(c => c.name !== name);
    employee.updatedAt = new Date().toISOString();
    return;
  }

  const response = await fetch(
    `${API_BASE}/sites/${siteId}/employees/${userId}/certifications/${encodeURIComponent(name)}`,
    { method: 'DELETE' }
  );
  if (!response.ok) throw new Error('Failed to remove certification');
}

/**
 * Get available rooms for the site (for preferred rooms selection)
 */
export async function getAvailableRooms(siteId: string): Promise<string[]> {
  if (USE_MOCK) {
    await new Promise(resolve => setTimeout(resolve, 100));
    return [
      'Flower Room A',
      'Flower Room B',
      'Veg Room 1',
      'Veg Room 2',
      'Propagation',
      'Dry Room',
      'Trim Room',
      'Processing',
      'Packaging',
    ];
  }

  const response = await fetch(`${API_BASE}/sites/${siteId}/rooms`);
  if (!response.ok) throw new Error('Failed to fetch rooms');
  const data = await response.json();
  return data.map((r: { name: string }) => r.name);
}

/**
 * Get employee schedule (upcoming shifts)
 */
export async function getEmployeeSchedule(
  siteId: string,
  userId: string,
  daysAhead: number = 7
): Promise<ShiftSchedule[]> {
  if (USE_MOCK) {
    await new Promise(resolve => setTimeout(resolve, 200));
    return generateUpcomingShifts(userId);
  }

  const response = await fetch(
    `${API_BASE}/sites/${siteId}/employees/${userId}/schedule?days=${daysAhead}`
  );
  if (!response.ok) throw new Error('Failed to fetch schedule');
  return response.json();
}

/**
 * Get employee time off requests
 */
export async function getTimeOffRequests(
  siteId: string,
  userId: string
): Promise<TimeOffRequest[]> {
  if (USE_MOCK) {
    await new Promise(resolve => setTimeout(resolve, 200));
    return generateTimeOffRequests(userId);
  }

  const response = await fetch(
    `${API_BASE}/sites/${siteId}/employees/${userId}/time-off`
  );
  if (!response.ok) throw new Error('Failed to fetch time off requests');
  return response.json();
}

/**
 * Submit a new time off request
 */
export async function submitTimeOffRequest(
  siteId: string,
  userId: string,
  request: SubmitTimeOffRequest
): Promise<TimeOffRequest> {
  if (USE_MOCK) {
    await new Promise(resolve => setTimeout(resolve, 300));
    const employee = mockEmployees[userId];
    if (!employee) throw new Error('Employee not found');
    
    const newRequest: TimeOffRequest = {
      id: `tor-${userId}-${Date.now()}`,
      type: request.type,
      startDate: request.startDate,
      endDate: request.endDate,
      status: 'pending',
      notes: request.notes,
      requestedAt: new Date().toISOString(),
    };
    
    // Calculate hours requested and update pending PTO
    const start = new Date(request.startDate);
    const end = new Date(request.endDate);
    const days = Math.ceil((end.getTime() - start.getTime()) / (1000 * 60 * 60 * 24)) + 1;
    const hours = days * 8;
    
    if (request.type === 'pto') {
      employee.ptoBalance.pending += hours;
    }
    
    employee.timeOffRequests.push(newRequest);
    return newRequest;
  }

  const response = await fetch(
    `${API_BASE}/sites/${siteId}/employees/${userId}/time-off`,
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
    }
  );
  if (!response.ok) throw new Error('Failed to submit time off request');
  return response.json();
}

/**
 * Cancel a time off request
 */
export async function cancelTimeOffRequest(
  siteId: string,
  userId: string,
  requestId: string
): Promise<void> {
  if (USE_MOCK) {
    await new Promise(resolve => setTimeout(resolve, 200));
    const employee = mockEmployees[userId];
    if (!employee) throw new Error('Employee not found');
    
    const requestIndex = employee.timeOffRequests.findIndex(r => r.id === requestId);
    if (requestIndex === -1) throw new Error('Time off request not found');
    
    const request = employee.timeOffRequests[requestIndex];
    if (request.status !== 'pending') {
      throw new Error('Can only cancel pending requests');
    }
    
    // Remove pending hours from PTO balance
    if (request.type === 'pto') {
      const start = new Date(request.startDate);
      const end = new Date(request.endDate);
      const days = Math.ceil((end.getTime() - start.getTime()) / (1000 * 60 * 60 * 24)) + 1;
      const hours = days * 8;
      employee.ptoBalance.pending = Math.max(0, employee.ptoBalance.pending - hours);
    }
    
    employee.timeOffRequests.splice(requestIndex, 1);
    return;
  }

  const response = await fetch(
    `${API_BASE}/sites/${siteId}/employees/${userId}/time-off/${requestId}`,
    { method: 'DELETE' }
  );
  if (!response.ok) throw new Error('Failed to cancel time off request');
}
