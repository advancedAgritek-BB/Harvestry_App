/**
 * Growlink Integration Service
 * Client-side API for managing Growlink integration
 */

const API_BASE = '/api/integrations/growlink';

// Types
export interface GrowlinkConnectionStatus {
  siteId: string;
  isConnected: boolean;
  status: string;
  growlinkAccountId: string | null;
  lastSyncAt: string | null;
  lastSyncError: string | null;
  deviceCount: number;
  mappedSensorCount: number;
}

export interface GrowlinkDevice {
  deviceId: string;
  name: string;
  deviceType: string;
  location: string | null;
  isOnline: boolean;
  lastSeen: string | null;
  sensors: GrowlinkSensor[];
}

export interface GrowlinkSensor {
  sensorId: string;
  name: string;
  sensorType: string;
  unit: string;
  currentValue: number | null;
  lastUpdated: string | null;
}

export interface GrowlinkStreamMapping {
  id: string;
  growlinkDeviceId: string;
  growlinkSensorId: string;
  growlinkSensorName: string;
  growlinkSensorType: string;
  harvestryStreamId: string;
  isActive: boolean;
  autoCreated: boolean;
}

export interface GrowlinkSyncResult {
  siteId: string;
  status: string;
  readingsReceived: number;
  readingsIngested: number;
  readingsRejected: number;
  readingsDuplicate: number;
  processingTimeMs: number;
  errorMessage: string | null;
}

export interface CreateMappingRequest {
  growlinkDeviceId: string;
  growlinkSensorId: string;
  harvestryStreamId?: string;
  autoCreateStream: boolean;
}

/**
 * Initiates the OAuth connection flow.
 */
export async function initiateConnection(siteId: string): Promise<{ authorizationUrl: string; state: string }> {
  const response = await fetch(`${API_BASE}/connect?siteId=${siteId}`, {
    method: 'POST',
  });
  if (!response.ok) {
    throw new Error('Failed to initiate Growlink connection');
  }
  return response.json();
}

/**
 * Gets the connection status for a site.
 */
export async function getConnectionStatus(siteId: string): Promise<GrowlinkConnectionStatus> {
  const response = await fetch(`${API_BASE}/status?siteId=${siteId}`);
  if (!response.ok) {
    throw new Error('Failed to get connection status');
  }
  return response.json();
}

/**
 * Gets available devices from Growlink.
 */
export async function getDevices(siteId: string): Promise<GrowlinkDevice[]> {
  const response = await fetch(`${API_BASE}/devices?siteId=${siteId}`);
  if (!response.ok) {
    throw new Error('Failed to get Growlink devices');
  }
  return response.json();
}

/**
 * Gets stream mappings for a site.
 */
export async function getMappings(siteId: string): Promise<GrowlinkStreamMapping[]> {
  const response = await fetch(`${API_BASE}/mappings?siteId=${siteId}`);
  if (!response.ok) {
    throw new Error('Failed to get stream mappings');
  }
  return response.json();
}

/**
 * Creates a new stream mapping.
 */
export async function createMapping(
  siteId: string,
  request: CreateMappingRequest
): Promise<GrowlinkStreamMapping> {
  const response = await fetch(`${API_BASE}/mappings?siteId=${siteId}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) {
    throw new Error('Failed to create mapping');
  }
  return response.json();
}

/**
 * Deletes a stream mapping.
 */
export async function deleteMapping(mappingId: string): Promise<void> {
  const response = await fetch(`${API_BASE}/mappings/${mappingId}`, {
    method: 'DELETE',
  });
  if (!response.ok) {
    throw new Error('Failed to delete mapping');
  }
}

/**
 * Triggers a manual sync.
 */
export async function triggerSync(siteId: string): Promise<GrowlinkSyncResult> {
  const response = await fetch(`${API_BASE}/sync?siteId=${siteId}`, {
    method: 'POST',
  });
  if (!response.ok) {
    throw new Error('Failed to trigger sync');
  }
  return response.json();
}

/**
 * Disconnects the Growlink integration.
 */
export async function disconnect(siteId: string): Promise<void> {
  const response = await fetch(`${API_BASE}/disconnect?siteId=${siteId}`, {
    method: 'DELETE',
  });
  if (!response.ok) {
    throw new Error('Failed to disconnect Growlink');
  }
}

/**
 * Redirects to Growlink OAuth.
 */
export function redirectToOAuth(authorizationUrl: string): void {
  window.location.href = authorizationUrl;
}




