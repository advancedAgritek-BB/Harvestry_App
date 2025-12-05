/**
 * Compliance Type Definitions
 * Types for METRC/BioTrack integration, sync status, and regulatory workflows
 */

/** Compliance provider */
export type ComplianceProvider = 'metrc' | 'biotrack';

/** Sync status for individual items */
export type SyncStatus = 'synced' | 'pending' | 'error' | 'stale';

/** Sync event type */
export type SyncEventType =
  | 'lot_create'
  | 'lot_update'
  | 'movement'
  | 'adjustment'
  | 'destruction'
  | 'lab_result'
  | 'package'
  | 'transfer'
  | 'manifest';

/** Hold reason codes */
export type HoldReasonCode =
  | 'coa_failed'
  | 'coa_pending'
  | 'contamination'
  | 'quality_issue'
  | 'regulatory'
  | 'customer_return'
  | 'investigation'
  | 'other';

/** Destruction reason codes */
export type DestructionReasonCode =
  | 'contamination'
  | 'quality_failure'
  | 'expired'
  | 'regulatory'
  | 'damage'
  | 'pest_issue'
  | 'mold'
  | 'other';

/** Compliance integration configuration */
export interface ComplianceIntegration {
  id: string;
  siteId: string;
  provider: ComplianceProvider;
  
  // Connection
  apiKeyMasked: string;
  isConnected: boolean;
  lastConnectionTest?: string;
  
  // Sync settings
  syncMode: 'realtime' | 'scheduled' | 'manual';
  syncIntervalMinutes?: number;
  retryPolicy: string;
  dlqEnabled: boolean;
  
  // Status
  lastSyncAt?: string;
  lastSyncStatus?: 'success' | 'partial' | 'failed';
  pendingCount: number;
  errorCount: number;
  
  // Audit
  createdAt: string;
  updatedAt: string;
}

/** Sync event record */
export interface SyncEvent {
  id: string;
  siteId: string;
  provider: ComplianceProvider;
  
  // Event details
  eventType: SyncEventType;
  entityType: string;
  entityId: string;
  
  // Status
  status: 'pending' | 'processing' | 'success' | 'failed' | 'dlq';
  retryCount: number;
  maxRetries: number;
  
  // Error info
  errorMessage?: string;
  errorCode?: string;
  
  // External IDs
  externalId?: string;
  externalResponse?: Record<string, unknown>;
  
  // Timing
  createdAt: string;
  processedAt?: string;
  nextRetryAt?: string;
}

/** Hold record */
export interface Hold {
  id: string;
  siteId: string;
  lotId: string;
  lotNumber: string;
  
  // Hold details
  reasonCode: HoldReasonCode;
  reasonNotes?: string;
  
  // Status
  isActive: boolean;
  releasedAt?: string;
  releasedBy?: string;
  releaseNotes?: string;
  
  // Lab reference
  labOrderId?: string;
  labResultId?: string;
  
  // Two-person approval (if required)
  requiresTwoPersonApproval: boolean;
  firstApprover?: string;
  firstApprovedAt?: string;
  secondApprover?: string;
  secondApprovedAt?: string;
  
  // Compliance sync
  syncStatus: SyncStatus;
  
  // Audit
  createdAt: string;
  createdBy: string;
  updatedAt: string;
}

/** Destruction event */
export interface DestructionEvent {
  id: string;
  siteId: string;
  lotId: string;
  lotNumber: string;
  
  // Destruction details
  reasonCode: DestructionReasonCode;
  quantityDestroyed: number;
  uom: string;
  method: string;
  
  // Evidence
  notes?: string;
  photoUrls?: string[];
  videoUrl?: string;
  
  // Witnesses (two-person signoff)
  requiresTwoPersonSignoff: boolean;
  witnessOne: string;
  witnessOneSignedAt: string;
  witnessTwo?: string;
  witnessTwoSignedAt?: string;
  
  // Compliance
  metrcDestructionId?: string;
  biotrackDestructionId?: string;
  syncStatus: SyncStatus;
  
  // Audit
  createdAt: string;
  completedAt?: string;
}

/** Lab order for COA */
export interface LabOrder {
  id: string;
  siteId: string;
  lotId: string;
  lotNumber: string;
  
  // Lab info
  labId: string;
  labName: string;
  
  // Order details
  testTypes: string[];
  status: 'pending' | 'submitted' | 'in_progress' | 'completed' | 'cancelled';
  
  // Results
  resultId?: string;
  overallResult?: 'pass' | 'fail';
  
  // Dates
  orderedAt: string;
  submittedAt?: string;
  resultsReceivedAt?: string;
  
  // Audit
  createdBy: string;
}

/** Lab result (COA) */
export interface LabResult {
  id: string;
  labOrderId: string;
  lotId: string;
  
  // Results
  overallResult: 'pass' | 'fail';
  testResults: TestResult[];
  
  // Document
  coaUrl?: string;
  coaFilename?: string;
  
  // Dates
  testedAt: string;
  receivedAt: string;
  expiresAt?: string;
  
  // Compliance
  metrcLabResultId?: string;
  syncStatus: SyncStatus;
}

/** Individual test result */
export interface TestResult {
  testType: string;
  result: 'pass' | 'fail' | 'pending';
  value?: number;
  unit?: string;
  limit?: number;
  notes?: string;
}

/** Sync queue status */
export interface SyncQueueStatus {
  provider: ComplianceProvider;
  siteId: string;
  
  // Counts
  pendingCount: number;
  processingCount: number;
  failedCount: number;
  dlqCount: number;
  
  // Rates
  successRate: number;
  avgProcessingTimeMs: number;
  
  // Last activity
  lastSuccessAt?: string;
  lastFailureAt?: string;
  oldestPendingAt?: string;
}

/** Dead letter queue item */
export interface DLQItem {
  id: string;
  syncEventId: string;
  provider: ComplianceProvider;
  eventType: SyncEventType;
  entityId: string;
  errorMessage: string;
  errorCode?: string;
  failedAt: string;
  retryCount: number;
  payload: Record<string, unknown>;
}

/** Compliance summary for dashboard */
export interface ComplianceSummary {
  integrations: {
    provider: ComplianceProvider;
    siteId: string;
    siteName: string;
    isConnected: boolean;
    lastSyncAt?: string;
    pendingCount: number;
    errorCount: number;
  }[];
  
  activeHolds: number;
  pendingDestructions: number;
  pendingLabOrders: number;
  dlqTotal: number;
  
  syncHealth: 'healthy' | 'degraded' | 'critical';
}

/** Audit export request */
export interface AuditExportRequest {
  siteId: string;
  startDate: string;
  endDate: string;
  entityTypes?: string[];
  format: 'csv' | 'xlsx' | 'pdf';
}
