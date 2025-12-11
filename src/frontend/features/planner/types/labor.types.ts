// ============================================
// Critical Alert Types
// ============================================

export type AlertSeverity = 'critical' | 'warning' | 'info';

export type AlertCategory =
  | 'staffing_gap'
  | 'time_sensitive'
  | 'compliance'
  | 'cost_impact'
  | 'scheduling_conflict'
  | 'certification_expiring';

export type CriticalAlert = {
  id: string;
  severity: AlertSeverity;
  category: AlertCategory;
  title: string;
  description: string;
  actionLabel: string;
  actionHref: string;
  timestamp: string;
  dismissable?: boolean;
};

// ============================================
// Production Task Types
// ============================================

export type LaborStatus = 'fully_staffed' | 'needs_people' | 'unassigned';

export type ProductionTaskType =
  | 'harvest'
  | 'processing'
  | 'packaging'
  | 'planting'
  | 'maintenance'
  | 'quality_check'
  | 'other';

export type ProductionTask = {
  id: string;
  name: string;
  type: ProductionTaskType;
  date: string;
  startTime: string;
  endTime: string;
  location: string;
  laborStatus: LaborStatus;
  assignedCount: number;
  requiredCount: number;
  skills?: string[];
};

// ============================================
// Schedule Summary Types
// ============================================

export type ShiftType = 'morning' | 'afternoon' | 'night' | 'custom';

export type ScheduledEmployee = {
  id: string;
  name: string;
  role: string;
  shift: ShiftType;
  shiftStart: string;
  shiftEnd: string;
  status: 'scheduled' | 'clocked_in' | 'absent' | 'late';
  location?: string;
};

export type ScheduleSummary = {
  date: string;
  totalScheduled: number;
  totalPresent: number;
  absences: number;
  lateArrivals: number;
  employees: ScheduledEmployee[];
};

// ============================================
// Metrics Types
// ============================================

export type CoverageMetric = {
  filledPositions: number;
  requiredPositions: number;
  percentage: number;
  trend: 'up' | 'down' | 'flat';
  trendValue?: number;
};

export type LaborCostMetric = {
  actual: number;
  budget: number;
  variance: number;
  variancePercent: number;
  period: 'day' | 'week' | 'month';
};

export type OvertimeMetric = {
  hoursThisWeek: number;
  hoursLastWeek: number;
  trend: 'up' | 'down' | 'flat';
  cost: number;
};

export type LaborMetrics = {
  coverage: CoverageMetric;
  laborCost: LaborCostMetric;
  overtime: OvertimeMetric;
};

// ============================================
// Action Item Types
// ============================================

export type ActionItemType =
  | 'timecard_approval'
  | 'scheduling_conflict'
  | 'certification_expiring'
  | 'shift_swap_request'
  | 'overtime_approval';

export type ActionItem = {
  id: string;
  type: ActionItemType;
  title: string;
  description: string;
  priority: 'high' | 'medium' | 'low';
  actionHref: string;
  dueDate?: string;
  relatedEmployee?: string;
};

// ============================================
// Legacy Types (maintained for compatibility)
// ============================================

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


