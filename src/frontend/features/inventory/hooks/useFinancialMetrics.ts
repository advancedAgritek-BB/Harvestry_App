import { useState, useEffect, useCallback } from 'react';
import type {
  FinancialSummary,
  CategoryValue,
  ValueAtRisk,
  CogsTrendPoint,
  AgingAnalysis,
  TurnoverMetrics,
  DashboardKpis,
} from '../types/financial.types';

const API_BASE = '/api/v1/sites';

interface UseFinancialMetricsOptions {
  siteId: string;
  autoRefresh?: boolean;
  refreshInterval?: number;
}

interface FinancialMetricsState {
  financialSummary: FinancialSummary | null;
  categoryValues: CategoryValue[];
  valueAtRisk: ValueAtRisk | null;
  cogsTrend: CogsTrendPoint[];
  agingAnalysis: AgingAnalysis | null;
  turnoverMetrics: TurnoverMetrics | null;
  dashboardKpis: DashboardKpis | null;
  isLoading: boolean;
  error: string | null;
}

export function useFinancialMetrics(options: UseFinancialMetricsOptions) {
  const { siteId, autoRefresh = false, refreshInterval = 60000 } = options;

  const [state, setState] = useState<FinancialMetricsState>({
    financialSummary: null,
    categoryValues: [],
    valueAtRisk: null,
    cogsTrend: [],
    agingAnalysis: null,
    turnoverMetrics: null,
    dashboardKpis: null,
    isLoading: true,
    error: null,
  });

  const fetchFinancialSummary = useCallback(async () => {
    try {
      const response = await fetch(`${API_BASE}/${siteId}/inventory/dashboard/financial-summary`);
      if (!response.ok) throw new Error('Failed to fetch financial summary');
      const data = await response.json();
      setState(prev => ({ ...prev, financialSummary: data }));
    } catch (error) {
      console.error('Error fetching financial summary:', error);
      throw error;
    }
  }, [siteId]);

  const fetchCategoryValues = useCallback(async () => {
    try {
      const response = await fetch(`${API_BASE}/${siteId}/inventory/dashboard/value-by-category`);
      if (!response.ok) throw new Error('Failed to fetch category values');
      const data = await response.json();
      setState(prev => ({ ...prev, categoryValues: data }));
    } catch (error) {
      console.error('Error fetching category values:', error);
      throw error;
    }
  }, [siteId]);

  const fetchValueAtRisk = useCallback(async () => {
    try {
      const response = await fetch(`${API_BASE}/${siteId}/inventory/dashboard/value-at-risk`);
      if (!response.ok) throw new Error('Failed to fetch value at risk');
      const data = await response.json();
      setState(prev => ({ ...prev, valueAtRisk: data }));
    } catch (error) {
      console.error('Error fetching value at risk:', error);
      throw error;
    }
  }, [siteId]);

  const fetchCogsTrend = useCallback(async (days: number = 90) => {
    try {
      const response = await fetch(`${API_BASE}/${siteId}/inventory/dashboard/cogs-trend?days=${days}`);
      if (!response.ok) throw new Error('Failed to fetch COGS trend');
      const data = await response.json();
      setState(prev => ({ ...prev, cogsTrend: data }));
    } catch (error) {
      console.error('Error fetching COGS trend:', error);
      throw error;
    }
  }, [siteId]);

  const fetchAgingAnalysis = useCallback(async () => {
    try {
      const response = await fetch(`${API_BASE}/${siteId}/inventory/dashboard/aging-analysis`);
      if (!response.ok) throw new Error('Failed to fetch aging analysis');
      const data = await response.json();
      setState(prev => ({ ...prev, agingAnalysis: data }));
    } catch (error) {
      console.error('Error fetching aging analysis:', error);
      throw error;
    }
  }, [siteId]);

  const fetchTurnoverMetrics = useCallback(async () => {
    try {
      const response = await fetch(`${API_BASE}/${siteId}/inventory/dashboard/turnover-metrics`);
      if (!response.ok) throw new Error('Failed to fetch turnover metrics');
      const data = await response.json();
      setState(prev => ({ ...prev, turnoverMetrics: data }));
    } catch (error) {
      console.error('Error fetching turnover metrics:', error);
      throw error;
    }
  }, [siteId]);

  const fetchDashboardKpis = useCallback(async () => {
    try {
      const response = await fetch(`${API_BASE}/${siteId}/inventory/dashboard/kpis`);
      if (!response.ok) throw new Error('Failed to fetch dashboard KPIs');
      const data = await response.json();
      setState(prev => ({ ...prev, dashboardKpis: data }));
    } catch (error) {
      console.error('Error fetching dashboard KPIs:', error);
      throw error;
    }
  }, [siteId]);

  const refreshAll = useCallback(async () => {
    setState(prev => ({ ...prev, isLoading: true, error: null }));
    try {
      await Promise.all([
        fetchFinancialSummary(),
        fetchCategoryValues(),
        fetchValueAtRisk(),
        fetchCogsTrend(),
        fetchAgingAnalysis(),
        fetchTurnoverMetrics(),
        fetchDashboardKpis(),
      ]);
    } catch (error) {
      setState(prev => ({ ...prev, error: 'Failed to load financial data' }));
    } finally {
      setState(prev => ({ ...prev, isLoading: false }));
    }
  }, [
    fetchFinancialSummary,
    fetchCategoryValues,
    fetchValueAtRisk,
    fetchCogsTrend,
    fetchAgingAnalysis,
    fetchTurnoverMetrics,
    fetchDashboardKpis,
  ]);

  useEffect(() => {
    refreshAll();
  }, [refreshAll]);

  useEffect(() => {
    if (!autoRefresh) return;
    const interval = setInterval(refreshAll, refreshInterval);
    return () => clearInterval(interval);
  }, [autoRefresh, refreshInterval, refreshAll]);

  return {
    ...state,
    refreshAll,
    fetchFinancialSummary,
    fetchCategoryValues,
    fetchValueAtRisk,
    fetchCogsTrend,
    fetchAgingAnalysis,
    fetchTurnoverMetrics,
    fetchDashboardKpis,
  };
}




