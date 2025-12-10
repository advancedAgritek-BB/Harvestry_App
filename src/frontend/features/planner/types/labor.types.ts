export type LaborCoverageCard = {
  id: string;
  label: string;
  filled: number;
  required: number;
  phase?: string;
  room?: string;
};

export type ShiftNeed = {
  id: string;
  role: string;
  room: string;
  start: string;
  end: string;
  required: number;
  assigned: number;
  skills: string[];
};

export type TimecardApproval = {
  id: string;
  employee: string;
  role: string;
  status: 'pending' | 'approved' | 'exception';
  hours: number;
  cost: number;
  exceptions?: string[];
};

export type BudgetVariance = {
  id: string;
  label: string;
  budget: number;
  actual: number;
  variance: number;
};

export type ProductivitySnapshot = {
  id: string;
  label: string;
  metric: string;
  value: number;
  target?: number;
  unit: string;
  trend: 'up' | 'down' | 'flat';
};

export type IntegrationStatus = {
  id: string;
  name: string;
  status: 'connected' | 'pending' | 'error' | 'disabled';
  description?: string;
};

export type EmployeeProfile = {
  id: string;
  name: string;
  role: string;
  skills: string[];
  certifications: { name: string; expiresOn?: string }[];
  preferredRooms?: string[];
  payType: 'hourly' | 'salary';
  rate: number;
};


