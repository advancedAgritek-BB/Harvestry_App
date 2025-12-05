/**
 * Task Blueprint Types
 * Defines types for automated task generation blueprints
 */

export type GrowthPhaseType = 
  | 'any' 
  | 'immature' 
  | 'vegetative' 
  | 'flowering' 
  | 'mother' 
  | 'harvested'
  | 'drying'
  | 'curing';

export type BlueprintRoomType = 
  | 'any' 
  | 'veg' 
  | 'flower' 
  | 'mother' 
  | 'clone' 
  | 'dry' 
  | 'cure'
  | 'extraction'
  | 'manufacturing'
  | 'vault';

export type TaskPriorityType = 'low' | 'normal' | 'high' | 'critical';

export interface TaskBlueprint {
  id: string;
  siteId: string;
  title: string;
  description?: string;
  growthPhase: GrowthPhaseType;
  roomType: BlueprintRoomType;
  strainId?: string;
  priority: TaskPriorityType;
  timeOffsetHours: number;
  assignedToRole?: string;
  isActive: boolean;
  createdByUserId: string;
  createdAt: string;
  updatedAt: string;
  requiredSopIds: string[];
  requiredTrainingIds: string[];
}

export interface CreateTaskBlueprintRequest {
  title: string;
  description?: string;
  growthPhase?: GrowthPhaseType;
  roomType?: BlueprintRoomType;
  strainId?: string;
  priority?: TaskPriorityType;
  timeOffsetHours?: number;
  assignedToRole?: string;
  requiredSopIds?: string[];
  requiredTrainingIds?: string[];
}

export interface UpdateTaskBlueprintRequest extends CreateTaskBlueprintRequest {}

export interface BlueprintListResponse {
  blueprints: TaskBlueprint[];
  total: number;
}

