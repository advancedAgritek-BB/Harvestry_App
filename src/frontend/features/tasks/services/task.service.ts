/**
 * Task Service
 * CRUD operations for tasks, assignments, and task lifecycle management
 */

import type {
  Task,
  TaskPriority,
  TaskStatus,
  TaskAssignee,
} from '../types/task.types';

export interface TaskFilterOptions {
  status?: TaskStatus;
  assignedTo?: string;
  priority?: TaskPriority;
  dueAfter?: string;
  dueBefore?: string;
  search?: string;
}

export interface TaskListResponse {
  tasks: Task[];
  total: number;
}

export interface CreateTaskRequest {
  title: string;
  description?: string;
  priority?: TaskPriority;
  assignedToUserId?: string;
  assignedToRole?: string;
  dueDate?: string;
  requiredSopIds?: string[];
  requiredTrainingIds?: string[];
  relatedEntityType?: string;
  relatedEntityId?: string;
  taskLibraryItemId?: string; // Prefill from template
}

export interface UpdateTaskRequest {
  description?: string;
  priority?: TaskPriority;
  dueDate?: string;
  relatedEntityType?: string;
  relatedEntityId?: string;
}

export interface AssignTaskRequest {
  userId?: string;
  role?: string;
}

const API_BASE = '/api/v1';

/**
 * Get tasks for a site
 */
export async function getTasks(
  siteId: string,
  filters: TaskFilterOptions = {}
): Promise<TaskListResponse> {
  const params = new URLSearchParams(
    Object.fromEntries(
      Object.entries(filters)
        .filter(([, v]) => v !== undefined)
        .map(([k, v]) => [k, String(v)])
    )
  );

  const response = await fetch(`${API_BASE}/sites/${siteId}/tasks?${params}`);
  if (!response.ok) throw new Error('Failed to fetch tasks');
  
  const data = await response.json();
  return { tasks: data, total: data.length };
}

/**
 * Get a single task
 */
export async function getTask(siteId: string, taskId: string): Promise<Task> {
  const response = await fetch(`${API_BASE}/sites/${siteId}/tasks/${taskId}`);
  if (!response.ok) throw new Error('Failed to fetch task');
  return response.json();
}

/**
 * Create a new task
 */
export async function createTask(
  siteId: string,
  request: CreateTaskRequest
): Promise<Task> {
  const response = await fetch(`${API_BASE}/sites/${siteId}/tasks`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to create task');
  return response.json();
}

/**
 * Update a task
 */
export async function updateTask(
  siteId: string,
  taskId: string,
  request: UpdateTaskRequest
): Promise<Task> {
  const response = await fetch(`${API_BASE}/sites/${siteId}/tasks/${taskId}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to update task');
  return response.json();
}

/**
 * Assign a task
 */
export async function assignTask(
  siteId: string,
  taskId: string,
  request: AssignTaskRequest
): Promise<Task> {
  const response = await fetch(`${API_BASE}/sites/${siteId}/tasks/${taskId}/assign`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to assign task');
  return response.json();
}

/**
 * Start a task
 */
export async function startTask(siteId: string, taskId: string): Promise<Task> {
  const response = await fetch(`${API_BASE}/sites/${siteId}/tasks/${taskId}/start`, {
    method: 'POST',
  });
  if (!response.ok) throw new Error('Failed to start task');
  return response.json();
}

/**
 * Complete a task
 */
export async function completeTask(siteId: string, taskId: string): Promise<Task> {
  const response = await fetch(`${API_BASE}/sites/${siteId}/tasks/${taskId}/complete`, {
    method: 'POST',
  });
  if (!response.ok) throw new Error('Failed to complete task');
  return response.json();
}

/**
 * Cancel a task
 */
export async function cancelTask(
  siteId: string,
  taskId: string,
  reason?: string
): Promise<Task> {
  const response = await fetch(`${API_BASE}/sites/${siteId}/tasks/${taskId}/cancel`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ reason }),
  });
  if (!response.ok) throw new Error('Failed to cancel task');
  return response.json();
}

/**
 * Get overdue tasks
 */
export async function getOverdueTasks(siteId: string): Promise<Task[]> {
  const response = await fetch(`${API_BASE}/sites/${siteId}/tasks/overdue`);
  if (!response.ok) throw new Error('Failed to fetch overdue tasks');
  return response.json();
}

/**
 * Add watcher to task
 */
export async function addWatcher(
  siteId: string,
  taskId: string,
  userId: string
): Promise<Task> {
  const response = await fetch(`${API_BASE}/sites/${siteId}/tasks/${taskId}/watchers`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ userId }),
  });
  if (!response.ok) throw new Error('Failed to add watcher');
  return response.json();
}

/**
 * Remove watcher from task
 */
export async function removeWatcher(
  siteId: string,
  taskId: string,
  userId: string
): Promise<Task> {
  const response = await fetch(
    `${API_BASE}/sites/${siteId}/tasks/${taskId}/watchers/${userId}`,
    { method: 'DELETE' }
  );
  if (!response.ok) throw new Error('Failed to remove watcher');
  return response.json();
}

/**
 * Get task history
 */
export async function getTaskHistory(
  siteId: string,
  taskId: string
): Promise<Array<{
  previousStatus: TaskStatus;
  status: TaskStatus;
  changedBy: string;
  changedAt: string;
  reason?: string;
}>> {
  const response = await fetch(`${API_BASE}/sites/${siteId}/tasks/${taskId}/history`);
  if (!response.ok) throw new Error('Failed to fetch task history');
  return response.json();
}

/**
 * Get site users for assignee picker
 */
export async function getSiteUsers(siteId: string): Promise<TaskAssignee[]> {
  const response = await fetch(`${API_BASE}/sites/${siteId}/users`);
  if (!response.ok) throw new Error('Failed to fetch site users');
  return response.json();
}

