import { useState, useEffect, useCallback } from 'react';
import type { AlertSummary, AlertItem } from '../types/financial.types';

const API_BASE = '/api/v1/sites';

interface UseAlertsOptions {
  siteId: string;
  autoRefresh?: boolean;
  refreshInterval?: number;
}

interface AlertsState {
  alertSummary: AlertSummary | null;
  isLoading: boolean;
  error: string | null;
}

export function useAlerts(options: UseAlertsOptions) {
  const { siteId, autoRefresh = false, refreshInterval = 30000 } = options;

  const [state, setState] = useState<AlertsState>({
    alertSummary: null,
    isLoading: true,
    error: null,
  });

  const fetchAlerts = useCallback(async () => {
    setState(prev => ({ ...prev, isLoading: true, error: null }));
    try {
      const response = await fetch(`${API_BASE}/${siteId}/inventory/dashboard/alerts`);
      if (!response.ok) throw new Error('Failed to fetch alerts');
      const data = await response.json();
      setState({ alertSummary: data, isLoading: false, error: null });
    } catch (error) {
      console.error('Error fetching alerts:', error);
      setState(prev => ({ ...prev, isLoading: false, error: 'Failed to load alerts' }));
    }
  }, [siteId]);

  useEffect(() => {
    fetchAlerts();
  }, [fetchAlerts]);

  useEffect(() => {
    if (!autoRefresh) return;
    const interval = setInterval(fetchAlerts, refreshInterval);
    return () => clearInterval(interval);
  }, [autoRefresh, refreshInterval, fetchAlerts]);

  const criticalAlerts = state.alertSummary?.topAlerts.filter(a => a.severity === 'critical') ?? [];
  const warningAlerts = state.alertSummary?.topAlerts.filter(a => a.severity === 'warning') ?? [];
  const infoAlerts = state.alertSummary?.topAlerts.filter(a => a.severity === 'info') ?? [];

  const totalAlertCount = state.alertSummary
    ? state.alertSummary.lowStockCount +
      state.alertSummary.expiringCount +
      state.alertSummary.onHoldCount +
      state.alertSummary.pendingSyncCount +
      state.alertSummary.failedLabTestCount +
      state.alertSummary.varianceCount
    : 0;

  return {
    ...state,
    criticalAlerts,
    warningAlerts,
    infoAlerts,
    totalAlertCount,
    refresh: fetchAlerts,
  };
}



