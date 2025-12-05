/**
 * Standard Operating Procedure (SOP) Types
 */

/**
 * A single step in an SOP with support for images and sub-steps
 */
export interface SopStep {
  id: string;
  order: number;
  title: string;
  description?: string;
  imageUrl?: string;
  imageCaption?: string;
  warningText?: string;
  tipText?: string;
  estimatedMinutes?: number;
  subSteps?: SopSubStep[];
}

export interface SopSubStep {
  id: string;
  order: number;
  text: string;
  imageUrl?: string;
}

/**
 * SOP with structured steps for interactive checklist display
 */
export interface StandardOperatingProcedure {
  id: string;
  orgId: string;
  title: string;
  description?: string;
  content?: string; // Legacy HTML content for backwards compatibility
  steps?: SopStep[]; // New structured steps format
  category?: string;
  version: number;
  isActive: boolean;
  createdByUserId: string;
  createdAt: string;
  updatedAt: string;
  estimatedTotalMinutes?: number;
  requiredEquipment?: string[];
  safetyNotes?: string[];
}

export interface SopSummary {
  id: string;
  title: string;
  description?: string;
  category?: string;
  version: number;
  isActive: boolean;
  updatedAt: string;
  stepCount?: number;
  estimatedMinutes?: number;
}

/**
 * Tracks completion progress of an SOP for a specific task execution
 */
export interface SopProgress {
  sopId: string;
  taskId: string;
  completedStepIds: string[];
  completedSubStepIds: string[];
  startedAt?: string;
  completedAt?: string;
  notes?: string;
}

export interface CreateSopRequest {
  title: string;
  content?: string;
  category?: string;
}

export interface UpdateSopRequest {
  title: string;
  content?: string;
  category?: string;
}

export interface SopListResponse {
  sops: SopSummary[];
  total: number;
}

/**
 * Task Library (Template) Types
 */
export type TaskType = 
  | 'cultivation'
  | 'irrigation'
  | 'harvest'
  | 'processing'
  | 'maintenance'
  | 'compliance'
  | 'quality'
  | 'custom';

export interface TaskLibraryItem {
  id: string;
  orgId: string;
  title: string;
  description?: string;
  defaultPriority: 'low' | 'normal' | 'high' | 'critical';
  taskType: TaskType;
  customTaskType?: string;
  defaultAssignedToRole?: string;
  defaultDueDaysOffset?: number;
  isActive: boolean;
  createdByUserId: string;
  createdAt: string;
  updatedAt: string;
  defaultSopIds: string[];
}

export interface CreateTaskLibraryItemRequest {
  title: string;
  description?: string;
  defaultPriority?: 'low' | 'normal' | 'high' | 'critical';
  taskType?: TaskType;
  customTaskType?: string;
  defaultAssignedToRole?: string;
  defaultDueDaysOffset?: number;
  defaultSopIds?: string[];
}

export interface UpdateTaskLibraryItemRequest extends CreateTaskLibraryItemRequest {}

export interface TaskLibraryListResponse {
  items: TaskLibraryItem[];
  total: number;
}

