/**
 * Compliance Service
 * METRC/BioTrack sync operations, holds, destructions, and audit exports
 */

import type {
  ComplianceProvider,
  ComplianceIntegration,
  SyncEvent,
  SyncQueueStatus,
  DLQItem,
  Hold,
  DestructionEvent,
  LabOrder,
  LabResult,
  ComplianceSummary,
  AuditExportRequest,
  HoldReasonCode,
  DestructionReasonCode,
} from '../types';

const API_BASE = '/api/compliance';

/**
 * Integration Management
 */
export async function getIntegrations(siteId?: string): Promise<ComplianceIntegration[]> {
  const params = siteId ? `?siteId=${siteId}` : '';
  const response = await fetch(`${API_BASE}/integrations${params}`);
  if (!response.ok) throw new Error('Failed to fetch integrations');
  return response.json();
}

export async function testConnection(
  siteId: string,
  provider: ComplianceProvider
): Promise<{ success: boolean; message: string; latencyMs: number }> {
  const response = await fetch(`${API_BASE}/integrations/test`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ siteId, provider }),
  });
  if (!response.ok) throw new Error('Failed to test connection');
  return response.json();
}

export async function updateIntegration(
  integrationId: string,
  updates: Partial<ComplianceIntegration>
): Promise<ComplianceIntegration> {
  const response = await fetch(`${API_BASE}/integrations/${integrationId}`, {
    method: 'PATCH',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(updates),
  });
  if (!response.ok) throw new Error('Failed to update integration');
  return response.json();
}

/**
 * Sync Operations
 */
export async function triggerSync(
  siteId: string,
  provider: ComplianceProvider,
  syncType?: 'plants' | 'packages' | 'transfers' | 'all'
): Promise<{ queuedEvents: number }> {
  const response = await fetch(`${API_BASE}/${provider}/sync`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ siteId, syncType: syncType ?? 'all' }),
  });
  if (!response.ok) throw new Error('Failed to trigger sync');
  return response.json();
}

export async function getSyncEvents(
  filters: {
    siteId?: string;
    provider?: ComplianceProvider;
    status?: string[];
    eventType?: string[];
    entityId?: string;
    startDate?: string;
    endDate?: string;
  } = {},
  page = 1,
  pageSize = 50
): Promise<{ items: SyncEvent[]; total: number }> {
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString(),
    ...Object.fromEntries(
      Object.entries(filters)
        .filter(([, v]) => v !== undefined)
        .map(([k, v]) => [k, Array.isArray(v) ? v.join(',') : String(v)])
    ),
  });

  const response = await fetch(`${API_BASE}/sync-events?${params}`);
  if (!response.ok) throw new Error('Failed to fetch sync events');
  return response.json();
}

export async function getSyncQueueStatus(siteId?: string): Promise<SyncQueueStatus[]> {
  const params = siteId ? `?siteId=${siteId}` : '';
  const response = await fetch(`${API_BASE}/sync-queue/status${params}`);
  if (!response.ok) throw new Error('Failed to fetch sync queue status');
  return response.json();
}

/**
 * Dead Letter Queue
 */
export async function getDLQItems(
  provider?: ComplianceProvider,
  siteId?: string,
  page = 1,
  pageSize = 50
): Promise<{ items: DLQItem[]; total: number }> {
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString(),
  });
  if (provider) params.set('provider', provider);
  if (siteId) params.set('siteId', siteId);

  const response = await fetch(`${API_BASE}/dlq?${params}`);
  if (!response.ok) throw new Error('Failed to fetch DLQ items');
  return response.json();
}

export async function retryDLQItem(dlqItemId: string): Promise<{ success: boolean }> {
  const response = await fetch(`${API_BASE}/dlq/${dlqItemId}/retry`, {
    method: 'POST',
  });
  if (!response.ok) throw new Error('Failed to retry DLQ item');
  return response.json();
}

export async function dismissDLQItem(dlqItemId: string, reason: string): Promise<void> {
  const response = await fetch(`${API_BASE}/dlq/${dlqItemId}/dismiss`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ reason }),
  });
  if (!response.ok) throw new Error('Failed to dismiss DLQ item');
}

export async function retryAllDLQ(
  provider?: ComplianceProvider,
  siteId?: string
): Promise<{ retried: number; failed: number }> {
  const response = await fetch(`${API_BASE}/dlq/retry-all`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ provider, siteId }),
  });
  if (!response.ok) throw new Error('Failed to retry all DLQ items');
  return response.json();
}

/**
 * Hold Management
 */
export async function getHolds(
  filters: {
    siteId?: string;
    lotId?: string;
    isActive?: boolean;
    reasonCode?: HoldReasonCode[];
  } = {},
  page = 1,
  pageSize = 50
): Promise<{ items: Hold[]; total: number }> {
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString(),
    ...Object.fromEntries(
      Object.entries(filters)
        .filter(([, v]) => v !== undefined)
        .map(([k, v]) => [k, Array.isArray(v) ? v.join(',') : String(v)])
    ),
  });

  const response = await fetch(`${API_BASE}/holds?${params}`);
  if (!response.ok) throw new Error('Failed to fetch holds');
  return response.json();
}

export async function createHold(request: {
  lotId: string;
  reasonCode: HoldReasonCode;
  reasonNotes?: string;
}): Promise<Hold> {
  const response = await fetch(`${API_BASE}/holds`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to create hold');
  return response.json();
}

export async function releaseHold(
  holdId: string,
  releaseNotes?: string
): Promise<Hold> {
  const response = await fetch(`${API_BASE}/holds/${holdId}/release`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ releaseNotes }),
  });
  if (!response.ok) throw new Error('Failed to release hold');
  return response.json();
}

export async function approveHoldRelease(
  holdId: string,
  approverRole: 'first' | 'second'
): Promise<Hold> {
  const response = await fetch(`${API_BASE}/holds/${holdId}/approve`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ approverRole }),
  });
  if (!response.ok) throw new Error('Failed to approve hold release');
  return response.json();
}

/**
 * Destruction Events
 */
export async function getDestructions(
  filters: {
    siteId?: string;
    lotId?: string;
    reasonCode?: DestructionReasonCode[];
    startDate?: string;
    endDate?: string;
  } = {},
  page = 1,
  pageSize = 50
): Promise<{ items: DestructionEvent[]; total: number }> {
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString(),
    ...Object.fromEntries(
      Object.entries(filters)
        .filter(([, v]) => v !== undefined)
        .map(([k, v]) => [k, Array.isArray(v) ? v.join(',') : String(v)])
    ),
  });

  const response = await fetch(`${API_BASE}/destructions?${params}`);
  if (!response.ok) throw new Error('Failed to fetch destructions');
  return response.json();
}

export async function createDestruction(request: {
  lotId: string;
  reasonCode: DestructionReasonCode;
  quantityDestroyed: number;
  method: string;
  notes?: string;
  photoUrls?: string[];
}): Promise<DestructionEvent> {
  const response = await fetch(`${API_BASE}/destructions`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to create destruction');
  return response.json();
}

export async function signDestruction(
  destructionId: string,
  witnessRole: 'first' | 'second'
): Promise<DestructionEvent> {
  const response = await fetch(`${API_BASE}/destructions/${destructionId}/sign`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ witnessRole }),
  });
  if (!response.ok) throw new Error('Failed to sign destruction');
  return response.json();
}

/**
 * Lab Orders & Results
 */
export async function getLabOrders(
  filters: {
    siteId?: string;
    lotId?: string;
    status?: string[];
  } = {},
  page = 1,
  pageSize = 50
): Promise<{ items: LabOrder[]; total: number }> {
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString(),
    ...Object.fromEntries(
      Object.entries(filters)
        .filter(([, v]) => v !== undefined)
        .map(([k, v]) => [k, Array.isArray(v) ? v.join(',') : String(v)])
    ),
  });

  const response = await fetch(`${API_BASE}/lab-orders?${params}`);
  if (!response.ok) throw new Error('Failed to fetch lab orders');
  return response.json();
}

export async function getLabResult(labResultId: string): Promise<LabResult> {
  const response = await fetch(`${API_BASE}/lab-results/${labResultId}`);
  if (!response.ok) throw new Error('Failed to fetch lab result');
  return response.json();
}

export async function uploadCOA(
  labOrderId: string,
  file: File
): Promise<LabResult> {
  const formData = new FormData();
  formData.append('file', file);
  formData.append('labOrderId', labOrderId);

  const response = await fetch(`${API_BASE}/coa/upload`, {
    method: 'POST',
    body: formData,
  });
  if (!response.ok) throw new Error('Failed to upload COA');
  return response.json();
}

/**
 * Audit Exports
 */
export async function requestAuditExport(
  request: AuditExportRequest
): Promise<{ exportId: string; status: 'queued' | 'processing' }> {
  const response = await fetch(`${API_BASE}/audit/export`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to request audit export');
  return response.json();
}

export async function getAuditExportStatus(
  exportId: string
): Promise<{ status: 'queued' | 'processing' | 'completed' | 'failed'; downloadUrl?: string }> {
  const response = await fetch(`${API_BASE}/audit/export/${exportId}`);
  if (!response.ok) throw new Error('Failed to get export status');
  return response.json();
}

/**
 * Summary & Dashboard
 */
export async function getComplianceSummary(siteId?: string): Promise<ComplianceSummary> {
  const params = siteId ? `?siteId=${siteId}` : '';
  const response = await fetch(`${API_BASE}/summary${params}`);
  if (!response.ok) throw new Error('Failed to fetch compliance summary');
  return response.json();
}

/**
 * Get last sync time for a specific entity
 */
export async function getLastSyncTime(
  entityType: 'lot' | 'movement' | 'location',
  entityId: string
): Promise<{ provider: ComplianceProvider; lastSyncAt: string; status: string } | null> {
  const response = await fetch(`${API_BASE}/sync-status/${entityType}/${entityId}`);
  if (!response.ok) return null;
  return response.json();
}
