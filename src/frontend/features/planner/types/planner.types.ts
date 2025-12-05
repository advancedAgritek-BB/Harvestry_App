/**
 * Planner Types - Gantt Chart Visual Planner
 * 
 * Type definitions for batch lifecycle planning with
 * Clone → Veg → Flower → Harvest → Cure phases
 */

// Phase types in lifecycle order
export type PhaseType = 'clone' | 'veg' | 'flower' | 'harvest' | 'cure';

// Room class types
export type RoomClass = 'propagation' | 'veg' | 'flower' | 'drying' | 'cure' | 'processing';

// Batch status
export type BatchStatus = 'planned' | 'active' | 'completed' | 'cancelled';

// Zoom levels for the Gantt view
export type ZoomLevel = 'day' | 'week' | 'month';

/**
 * Represents a single phase within a batch lifecycle
 */
export interface BatchPhase {
  id: string;
  phase: PhaseType;
  plannedStart: Date;
  plannedEnd: Date;
  actualStart?: Date;
  actualEnd?: Date;
  roomId: string;
  roomName?: string;
  notes?: string;
}

/**
 * Genetics information for a batch
 */
export interface Genetics {
  id: string;
  name: string;
  defaultVegDays: number;
  defaultFlowerDays: number;
  defaultCureDays?: number;
}

/**
 * A planned or active batch in the cultivation lifecycle
 */
export interface PlannedBatch {
  id: string;
  name: string;
  code: string;
  strain: string;
  genetics: Genetics;
  phenotypeId?: string;
  plantCount: number;
  phases: BatchPhase[];
  status: BatchStatus;
  createdAt: Date;
  updatedAt: Date;
}

/**
 * Room with capacity information
 */
export interface Room {
  id: string;
  name: string;
  code: string;
  roomClass: RoomClass;
  maxCapacity: number;
  siteId: string;
}

/**
 * Daily capacity snapshot for a room
 */
export interface DailyCapacity {
  date: Date;
  roomId: string;
  plantCount: number;
  batchIds: string[];
}

/**
 * Room capacity state with occupancy over time
 */
export interface RoomCapacity {
  roomId: string;
  roomName: string;
  roomClass: RoomClass;
  maxCapacity: number;
  dailyOccupancy: DailyCapacity[];
}

/**
 * Conflict types that can occur during planning
 */
export type ConflictType = 
  | 'capacity_exceeded'
  | 'room_unavailable'
  | 'phase_overlap'
  | 'genetics_violation'
  | 'scheduling_conflict';

/**
 * A detected conflict in the planner
 */
export interface PlannerConflict {
  id: string;
  type: ConflictType;
  severity: 'warning' | 'error';
  batchId: string;
  phaseId?: string;
  roomId?: string;
  date?: Date;
  message: string;
  affectedBatchIds?: string[];
}

/**
 * Impact of a change on downstream batches
 */
export interface ChangeImpact {
  batchId: string;
  batchName: string;
  phaseId: string;
  phase: PhaseType;
  originalStart: Date;
  newStart: Date;
  daysDelta: number;
  cascadeEffects: {
    phaseId: string;
    phase: PhaseType;
    daysDelta: number;
  }[];
}

/**
 * Dragging state for optimistic updates
 */
export interface DragState {
  isDragging: boolean;
  batchId: string | null;
  phaseId: string | null;
  originalStart: Date | null;
  currentStart: Date | null;
  dragType: 'move' | 'resize-start' | 'resize-end' | null;
}

/**
 * Date range for viewport
 */
export interface DateRange {
  start: Date;
  end: Date;
}

/**
 * Filter options for the planner
 */
export interface PlannerFilters {
  strains: string[];
  geneticsIds: string[];
  roomIds: string[];
  statuses: BatchStatus[];
  searchQuery: string;
}

/**
 * Configuration for phase appearance
 */
export interface PhaseConfig {
  phase: PhaseType;
  label: string;
  color: string;
  gradientFrom: string;
  gradientTo: string;
  icon: string;
  defaultDays: number;
}

/**
 * Planner view settings
 */
export interface PlannerSettings {
  zoomLevel: ZoomLevel;
  showCapacityLanes: boolean;
  showConflicts: boolean;
  showActualDates: boolean;
  snapToDay: boolean;
  whatIfMode: boolean;
}

