/**
 * SOP (Standard Operating Procedure) Service
 * Manages organization-wide SOPs and task library templates
 */

import type {
  StandardOperatingProcedure,
  SopSummary,
  CreateSopRequest,
  UpdateSopRequest,
  SopListResponse,
  TaskLibraryItem,
  CreateTaskLibraryItemRequest,
  UpdateTaskLibraryItemRequest,
  TaskLibraryListResponse,
} from '../types/sop.types';

const API_BASE = '/api/v1';

// ============= SOP Operations =============

/**
 * Get SOPs for an organization
 */
export async function getSops(
  orgId: string,
  activeOnly?: boolean,
  category?: string
): Promise<SopListResponse> {
  const params = new URLSearchParams();
  if (activeOnly !== undefined) params.set('activeOnly', String(activeOnly));
  if (category) params.set('category', category);
  
  const queryString = params.toString() ? `?${params.toString()}` : '';
  const response = await fetch(`${API_BASE}/orgs/${orgId}/sops${queryString}`);
  if (!response.ok) throw new Error('Failed to fetch SOPs');
  
  const data = await response.json();
  return { sops: data, total: data.length };
}

/**
 * Get a single SOP
 */
export async function getSop(orgId: string, sopId: string): Promise<StandardOperatingProcedure> {
  const response = await fetch(`${API_BASE}/orgs/${orgId}/sops/${sopId}`);
  if (!response.ok) throw new Error('Failed to fetch SOP');
  return response.json();
}

/**
 * Create a new SOP
 */
export async function createSop(
  orgId: string,
  request: CreateSopRequest
): Promise<StandardOperatingProcedure> {
  const response = await fetch(`${API_BASE}/orgs/${orgId}/sops`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to create SOP');
  return response.json();
}

/**
 * Update an SOP
 */
export async function updateSop(
  orgId: string,
  sopId: string,
  request: UpdateSopRequest
): Promise<StandardOperatingProcedure> {
  const response = await fetch(`${API_BASE}/orgs/${orgId}/sops/${sopId}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to update SOP');
  return response.json();
}

/**
 * Activate an SOP
 */
export async function activateSop(
  orgId: string,
  sopId: string
): Promise<StandardOperatingProcedure> {
  const response = await fetch(`${API_BASE}/orgs/${orgId}/sops/${sopId}/activate`, {
    method: 'POST',
  });
  if (!response.ok) throw new Error('Failed to activate SOP');
  return response.json();
}

/**
 * Deactivate an SOP
 */
export async function deactivateSop(
  orgId: string,
  sopId: string
): Promise<StandardOperatingProcedure> {
  const response = await fetch(`${API_BASE}/orgs/${orgId}/sops/${sopId}/deactivate`, {
    method: 'POST',
  });
  if (!response.ok) throw new Error('Failed to deactivate SOP');
  return response.json();
}

/**
 * Delete an SOP
 */
export async function deleteSop(orgId: string, sopId: string): Promise<void> {
  const response = await fetch(`${API_BASE}/orgs/${orgId}/sops/${sopId}`, {
    method: 'DELETE',
  });
  if (!response.ok) throw new Error('Failed to delete SOP');
}

// ============= Task Library (Templates) Operations =============

/**
 * Get task library items for an organization
 */
export async function getTaskLibraryItems(
  orgId: string,
  activeOnly?: boolean
): Promise<TaskLibraryListResponse> {
  const params = activeOnly !== undefined ? `?activeOnly=${activeOnly}` : '';
  const response = await fetch(`${API_BASE}/orgs/${orgId}/task-library${params}`);
  if (!response.ok) throw new Error('Failed to fetch task library');
  
  const data = await response.json();
  return { items: data, total: data.length };
}

/**
 * Get a single task library item
 */
export async function getTaskLibraryItem(
  orgId: string,
  itemId: string
): Promise<TaskLibraryItem> {
  const response = await fetch(`${API_BASE}/orgs/${orgId}/task-library/${itemId}`);
  if (!response.ok) throw new Error('Failed to fetch task library item');
  return response.json();
}

/**
 * Create a task library item
 */
export async function createTaskLibraryItem(
  orgId: string,
  request: CreateTaskLibraryItemRequest
): Promise<TaskLibraryItem> {
  const response = await fetch(`${API_BASE}/orgs/${orgId}/task-library`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to create task library item');
  return response.json();
}

/**
 * Update a task library item
 */
export async function updateTaskLibraryItem(
  orgId: string,
  itemId: string,
  request: UpdateTaskLibraryItemRequest
): Promise<TaskLibraryItem> {
  const response = await fetch(`${API_BASE}/orgs/${orgId}/task-library/${itemId}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to update task library item');
  return response.json();
}

/**
 * Activate a task library item
 */
export async function activateTaskLibraryItem(
  orgId: string,
  itemId: string
): Promise<TaskLibraryItem> {
  const response = await fetch(`${API_BASE}/orgs/${orgId}/task-library/${itemId}/activate`, {
    method: 'POST',
  });
  if (!response.ok) throw new Error('Failed to activate task library item');
  return response.json();
}

/**
 * Deactivate a task library item
 */
export async function deactivateTaskLibraryItem(
  orgId: string,
  itemId: string
): Promise<TaskLibraryItem> {
  const response = await fetch(`${API_BASE}/orgs/${orgId}/task-library/${itemId}/deactivate`, {
    method: 'POST',
  });
  if (!response.ok) throw new Error('Failed to deactivate task library item');
  return response.json();
}

/**
 * Delete a task library item
 */
export async function deleteTaskLibraryItem(orgId: string, itemId: string): Promise<void> {
  const response = await fetch(`${API_BASE}/orgs/${orgId}/task-library/${itemId}`, {
    method: 'DELETE',
  });
  if (!response.ok) throw new Error('Failed to delete task library item');
}

