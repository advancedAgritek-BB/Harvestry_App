/**
 * Task Management Types
 * Defines types for tasks, recommendations, and blueprint-based scheduling
 */

import type { ApplicationType, ApplicationTaskDetails } from './application.types';

export type TaskPriority = 'critical' | 'high' | 'normal' | 'low';
export type TaskStatus = 'draft' | 'ready' | 'in_progress' | 'paused' | 'blocked' | 'completed' | 'verified' | 'closed';
export type SLAStatus = 'ok' | 'warning' | 'breached';
export type GrowthPhase = 'clone' | 'veg' | 'flower' | 'harvest' | 'cure';

/**
 * Task categories - determines what kind of task this is
 */
export type TaskCategory = 
  | 'general'       // Standard task (inspection, maintenance, etc.)
  | 'application'   // Application task (fertigation, IPM, etc.) - requires inventory
  | 'harvest'       // Harvest task
  | 'processing'    // Post-harvest processing
  | 'compliance';   // Compliance-related task

export interface TaskAssignee {
  id: string;
  firstName: string;
  lastName: string;
  avatarUrl?: string;
  role?: string;
  teamId?: string;
  permissions?: string[]; // User's permissions for validation
}

export interface Task {
  id: string;
  title: string;
  description?: string;
  location: string;
  roomId?: string;
  zoneId?: string;
  batchId?: string;
  dueAt: string;
  startedAt?: string;
  completedAt?: string;
  priority: TaskPriority;
  status: TaskStatus;
  slaStatus: SLAStatus;
  assignee?: TaskAssignee;
  blueprintTaskId?: string;
  phase?: GrowthPhase;
  
  // Task categorization
  category: TaskCategory;
  
  // Application task details (only present when category === 'application')
  applicationType?: ApplicationType;
  applicationDetails?: ApplicationTaskDetails;
  
  // Required permission to complete this task
  requiredPermission?: string;
}

export interface BlueprintTask {
  id: string;
  name: string;
  description?: string;
  phase: GrowthPhase;
  dayInPhase: number;
  estimatedDurationMinutes: number;
  priority: TaskPriority;
  requiredRole?: string;
  checklistItems?: string[];
  dependencies?: string[];
  slaHours?: number;
}

export interface StrainBlueprint {
  id: string;
  strainId: string;
  strainName: string;
  phases: {
    [K in GrowthPhase]?: {
      durationDays: number;
      tasks: BlueprintTask[];
    };
  };
}

export interface TaskRecommendation {
  id: string;
  blueprintTask: BlueprintTask;
  batchId: string;
  batchName: string;
  strainName: string;
  suggestedDate: Date;
  suggestedAssignees: TaskAssignee[];
  priority: TaskPriority;
  isOverdue: boolean;
  daysUntilDue: number;
  reason: string;
}

export interface UnassignedTaskAlert {
  taskId: string;
  taskTitle: string;
  batchName: string;
  dueAt: Date;
  hoursUntilDue: number;
  suggestedAssignees: TaskAssignee[];
  severity: 'info' | 'warning' | 'critical';
}

export interface TaskRecommendationSummary {
  recommendations: TaskRecommendation[];
  unassignedAlerts: UnassignedTaskAlert[];
  upcomingCount: number;
  overdueCount: number;
  lastUpdated: Date;
}

