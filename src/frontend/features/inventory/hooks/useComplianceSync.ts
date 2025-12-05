/**
 * useComplianceSync Hook
 * Compliance integration status, sync operations, and hold management
 */

import { useCallback, useEffect, useState } from 'react';
import { useInventoryStore, useComplianceState } from '../stores/inventoryStore';
import * as complianceService from '../services/compliance.service';
import type {
  ComplianceProvider,
  ComplianceIntegration,
  SyncEvent,
  DLQItem,
  Hold,
  DestructionEvent,
  HoldReasonCode,
  DestructionReasonCode,
  AuditExportRequest,
} from '../types';

interface UseComplianceSyncOptions {
  autoFetch?: boolean;
  pollInterval?: number; // ms, for sync status polling
}

export function useComplianceSync(options: UseComplianceSyncOptions = {}) {
  const { autoFetch = true, pollInterval = 30000 } = options;
  const store = useInventoryStore();
  const state = useComplianceState();
  
  const [integrations, setIntegrations] = useState<ComplianceIntegration[]>([]);
  const [syncEvents, setSyncEvents] = useState<SyncEvent[]>([]);
  const [dlqItems, setDlqItems] = useState<DLQItem[]>([]);
  const [integrationsLoading, setIntegrationsLoading] = useState(false);
  
  /**
   * Fetch integrations list
   */
  const fetchIntegrations = useCallback(async () => {
    setIntegrationsLoading(true);
    try {
      const data = await complianceService.getIntegrations(store.currentSiteId ?? undefined);
      setIntegrations(data);
    } catch (err) {
      console.error('Failed to fetch integrations:', err);
    } finally {
      setIntegrationsLoading(false);
    }
  }, [store.currentSiteId]);
  
  /**
   * Fetch sync queue status
   */
  const fetchSyncStatus = useCallback(async () => {
    try {
      const status = await complianceService.getSyncQueueStatus(store.currentSiteId ?? undefined);
      store.setSyncQueueStatus(status);
    } catch (err) {
      console.error('Failed to fetch sync status:', err);
    }
  }, [store]);
  
  /**
   * Fetch compliance summary
   */
  const fetchSummary = useCallback(async () => {
    store.setComplianceLoading(true);
    try {
      const summary = await complianceService.getComplianceSummary(store.currentSiteId ?? undefined);
      store.setComplianceSummary(summary);
    } catch (err) {
      console.error('Failed to fetch compliance summary:', err);
    } finally {
      store.setComplianceLoading(false);
    }
  }, [store]);
  
  /**
   * Fetch active holds
   */
  const fetchHolds = useCallback(async () => {
    try {
      const { items } = await complianceService.getHolds({
        siteId: store.currentSiteId ?? undefined,
        isActive: true,
      });
      store.setActiveHolds(items);
    } catch (err) {
      console.error('Failed to fetch holds:', err);
    }
  }, [store]);
  
  /**
   * Fetch DLQ items
   */
  const fetchDLQ = useCallback(async (provider?: ComplianceProvider) => {
    try {
      const { items } = await complianceService.getDLQItems(
        provider,
        store.currentSiteId ?? undefined
      );
      setDlqItems(items);
    } catch (err) {
      console.error('Failed to fetch DLQ:', err);
    }
  }, [store.currentSiteId]);
  
  /**
   * Fetch sync events
   */
  const fetchSyncEvents = useCallback(async (filters: {
    provider?: ComplianceProvider;
    status?: string[];
    entityId?: string;
  } = {}) => {
    try {
      const { items } = await complianceService.getSyncEvents({
        siteId: store.currentSiteId ?? undefined,
        ...filters,
      });
      setSyncEvents(items);
    } catch (err) {
      console.error('Failed to fetch sync events:', err);
    }
  }, [store.currentSiteId]);
  
  // Auto-fetch on mount
  useEffect(() => {
    if (autoFetch) {
      fetchIntegrations();
      fetchSummary();
      fetchSyncStatus();
      fetchHolds();
    }
  }, [autoFetch, fetchIntegrations, fetchSummary, fetchSyncStatus, fetchHolds]);
  
  // Polling for sync status
  useEffect(() => {
    if (pollInterval > 0) {
      const interval = setInterval(fetchSyncStatus, pollInterval);
      return () => clearInterval(interval);
    }
  }, [pollInterval, fetchSyncStatus]);
  
  /**
   * Test integration connection
   */
  const testConnection = useCallback(async (
    siteId: string,
    provider: ComplianceProvider
  ) => {
    return complianceService.testConnection(siteId, provider);
  }, []);
  
  /**
   * Trigger manual sync
   */
  const triggerSync = useCallback(async (
    provider: ComplianceProvider,
    syncType?: 'plants' | 'packages' | 'transfers' | 'all'
  ) => {
    if (!store.currentSiteId) throw new Error('No site selected');
    
    const result = await complianceService.triggerSync(
      store.currentSiteId,
      provider,
      syncType
    );
    
    // Refresh status after triggering
    await fetchSyncStatus();
    
    return result;
  }, [store.currentSiteId, fetchSyncStatus]);
  
  /**
   * Retry DLQ item
   */
  const retryDLQItem = useCallback(async (dlqItemId: string) => {
    const result = await complianceService.retryDLQItem(dlqItemId);
    await fetchDLQ();
    await fetchSyncStatus();
    return result;
  }, [fetchDLQ, fetchSyncStatus]);
  
  /**
   * Dismiss DLQ item
   */
  const dismissDLQItem = useCallback(async (dlqItemId: string, reason: string) => {
    await complianceService.dismissDLQItem(dlqItemId, reason);
    await fetchDLQ();
  }, [fetchDLQ]);
  
  /**
   * Retry all DLQ items
   */
  const retryAllDLQ = useCallback(async (provider?: ComplianceProvider) => {
    const result = await complianceService.retryAllDLQ(provider, store.currentSiteId ?? undefined);
    await fetchDLQ();
    await fetchSyncStatus();
    return result;
  }, [store.currentSiteId, fetchDLQ, fetchSyncStatus]);
  
  /**
   * Create a hold
   */
  const createHold = useCallback(async (
    lotId: string,
    reasonCode: HoldReasonCode,
    reasonNotes?: string
  ) => {
    const hold = await complianceService.createHold({ lotId, reasonCode, reasonNotes });
    store.setActiveHolds([hold, ...state.holds]);
    return hold;
  }, [store, state.holds]);
  
  /**
   * Release a hold
   */
  const releaseHold = useCallback(async (holdId: string, releaseNotes?: string) => {
    const hold = await complianceService.releaseHold(holdId, releaseNotes);
    store.setActiveHolds(state.holds.filter((h) => h.id !== holdId));
    return hold;
  }, [store, state.holds]);
  
  /**
   * Approve hold release (for two-person approval)
   */
  const approveHoldRelease = useCallback(async (
    holdId: string,
    approverRole: 'first' | 'second'
  ) => {
    const hold = await complianceService.approveHoldRelease(holdId, approverRole);
    store.setActiveHolds(
      state.holds.map((h) => (h.id === holdId ? hold : h))
    );
    return hold;
  }, [store, state.holds]);
  
  /**
   * Create destruction event
   */
  const createDestruction = useCallback(async (request: {
    lotId: string;
    reasonCode: DestructionReasonCode;
    quantityDestroyed: number;
    method: string;
    notes?: string;
    photoUrls?: string[];
  }) => {
    return complianceService.createDestruction(request);
  }, []);
  
  /**
   * Sign destruction (witness)
   */
  const signDestruction = useCallback(async (
    destructionId: string,
    witnessRole: 'first' | 'second'
  ) => {
    return complianceService.signDestruction(destructionId, witnessRole);
  }, []);
  
  /**
   * Request audit export
   */
  const requestAuditExport = useCallback(async (request: AuditExportRequest) => {
    return complianceService.requestAuditExport(request);
  }, []);
  
  /**
   * Check audit export status
   */
  const checkExportStatus = useCallback(async (exportId: string) => {
    return complianceService.getAuditExportStatus(exportId);
  }, []);
  
  const refresh = useCallback(() => {
    fetchIntegrations();
    fetchSummary();
    fetchSyncStatus();
    fetchHolds();
    fetchDLQ();
  }, [fetchIntegrations, fetchSummary, fetchSyncStatus, fetchHolds, fetchDLQ]);
  
  return {
    // State
    integrations,
    integrationsLoading,
    summary: state.summary,
    syncQueueStatus: state.syncStatus,
    activeHolds: state.holds,
    dlqItems,
    syncEvents,
    loading: state.loading,
    
    // Fetch actions
    fetchIntegrations,
    fetchSyncStatus,
    fetchSummary,
    fetchHolds,
    fetchDLQ,
    fetchSyncEvents,
    
    // Integration actions
    testConnection,
    triggerSync,
    
    // DLQ actions
    retryDLQItem,
    dismissDLQItem,
    retryAllDLQ,
    
    // Hold actions
    createHold,
    releaseHold,
    approveHoldRelease,
    
    // Destruction actions
    createDestruction,
    signDestruction,
    
    // Audit actions
    requestAuditExport,
    checkExportStatus,
    
    // Utility
    refresh,
  };
}
