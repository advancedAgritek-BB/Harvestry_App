/**
 * Task Blueprint Service
 * Manages automated task generation blueprints
 */

import type {
  TaskBlueprint,
  CreateTaskBlueprintRequest,
  UpdateTaskBlueprintRequest,
  BlueprintListResponse,
} from '../types/blueprint.types';

const API_BASE = '/api/v1';

/**
 * Get blueprints for a site
 */
export async function getBlueprints(
  siteId: string,
  activeOnly?: boolean
): Promise<BlueprintListResponse> {
  const params = activeOnly !== undefined ? `?activeOnly=${activeOnly}` : '';
  const response = await fetch(`${API_BASE}/sites/${siteId}/task-blueprints${params}`);
  if (!response.ok) throw new Error('Failed to fetch blueprints');
  
  const data = await response.json();
  return { blueprints: data, total: data.length };
}

/**
 * Get a single blueprint
 */
export async function getBlueprint(
  siteId: string,
  blueprintId: string
): Promise<TaskBlueprint> {
  const response = await fetch(`${API_BASE}/sites/${siteId}/task-blueprints/${blueprintId}`);
  if (!response.ok) throw new Error('Failed to fetch blueprint');
  return response.json();
}

/**
 * Create a new blueprint
 */
export async function createBlueprint(
  siteId: string,
  request: CreateTaskBlueprintRequest
): Promise<TaskBlueprint> {
  const response = await fetch(`${API_BASE}/sites/${siteId}/task-blueprints`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to create blueprint');
  return response.json();
}

/**
 * Update a blueprint
 */
export async function updateBlueprint(
  siteId: string,
  blueprintId: string,
  request: UpdateTaskBlueprintRequest
): Promise<TaskBlueprint> {
  const response = await fetch(`${API_BASE}/sites/${siteId}/task-blueprints/${blueprintId}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to update blueprint');
  return response.json();
}

/**
 * Activate a blueprint
 */
export async function activateBlueprint(
  siteId: string,
  blueprintId: string
): Promise<TaskBlueprint> {
  const response = await fetch(
    `${API_BASE}/sites/${siteId}/task-blueprints/${blueprintId}/activate`,
    { method: 'POST' }
  );
  if (!response.ok) throw new Error('Failed to activate blueprint');
  return response.json();
}

/**
 * Deactivate a blueprint
 */
export async function deactivateBlueprint(
  siteId: string,
  blueprintId: string
): Promise<TaskBlueprint> {
  const response = await fetch(
    `${API_BASE}/sites/${siteId}/task-blueprints/${blueprintId}/deactivate`,
    { method: 'POST' }
  );
  if (!response.ok) throw new Error('Failed to deactivate blueprint');
  return response.json();
}

/**
 * Delete a blueprint
 */
export async function deleteBlueprint(siteId: string, blueprintId: string): Promise<void> {
  const response = await fetch(
    `${API_BASE}/sites/${siteId}/task-blueprints/${blueprintId}`,
    { method: 'DELETE' }
  );
  if (!response.ok) throw new Error('Failed to delete blueprint');
}

/**
 * Manually trigger task generation
 */
export async function triggerTaskGeneration(
  siteId: string,
  request: {
    batchId: string;
    strainId?: string;
    phase: string;
    roomType: string;
  }
): Promise<unknown[]> {
  const response = await fetch(`${API_BASE}/sites/${siteId}/task-generation/trigger`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to trigger task generation');
  return response.json();
}

