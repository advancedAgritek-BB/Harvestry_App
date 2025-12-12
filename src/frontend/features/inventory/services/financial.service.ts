/**
 * Financial Service
 * Connects to the Inventory Dashboard API for financial metrics
 */

import type {
  FinancialSummary,
  CategoryValue,
  ValueAtRisk,
  CogsTrendPoint,
  AgingAnalysis,
  TurnoverMetrics,
  AlertSummary,
  DashboardKpis,
} from '../types/financial.types';

const getApiBase = (siteId: string) => `/api/v1/sites/${siteId}/inventory/dashboard`;

/**
 * Get complete financial summary
 */
export async function getFinancialSummary(siteId: string): Promise<FinancialSummary> {
  const response = await fetch(`${getApiBase(siteId)}/financial-summary`);
  if (!response.ok) throw new Error('Failed to fetch financial summary');
  return response.json();
}

/**
 * Get inventory value by category
 */
export async function getValueByCategory(siteId: string): Promise<CategoryValue[]> {
  const response = await fetch(`${getApiBase(siteId)}/value-by-category`);
  if (!response.ok) throw new Error('Failed to fetch category values');
  return response.json();
}

/**
 * Get value at risk breakdown
 */
export async function getValueAtRisk(siteId: string): Promise<ValueAtRisk> {
  const response = await fetch(`${getApiBase(siteId)}/value-at-risk`);
  if (!response.ok) throw new Error('Failed to fetch value at risk');
  return response.json();
}

/**
 * Get COGS trend data
 */
export async function getCogsTrend(siteId: string, days = 90): Promise<CogsTrendPoint[]> {
  const response = await fetch(`${getApiBase(siteId)}/cogs-trend?days=${days}`);
  if (!response.ok) throw new Error('Failed to fetch COGS trend');
  return response.json();
}

/**
 * Get inventory aging analysis
 */
export async function getAgingAnalysis(siteId: string): Promise<AgingAnalysis> {
  const response = await fetch(`${getApiBase(siteId)}/aging-analysis`);
  if (!response.ok) throw new Error('Failed to fetch aging analysis');
  return response.json();
}

/**
 * Get turnover metrics
 */
export async function getTurnoverMetrics(siteId: string): Promise<TurnoverMetrics> {
  const response = await fetch(`${getApiBase(siteId)}/turnover-metrics`);
  if (!response.ok) throw new Error('Failed to fetch turnover metrics');
  return response.json();
}

/**
 * Get alert summary
 */
export async function getAlertSummary(siteId: string): Promise<AlertSummary> {
  const response = await fetch(`${getApiBase(siteId)}/alerts`);
  if (!response.ok) throw new Error('Failed to fetch alerts');
  return response.json();
}

/**
 * Get dashboard KPIs
 */
export async function getDashboardKpis(siteId: string): Promise<DashboardKpis> {
  const response = await fetch(`${getApiBase(siteId)}/kpis`);
  if (!response.ok) throw new Error('Failed to fetch KPIs');
  return response.json();
}




