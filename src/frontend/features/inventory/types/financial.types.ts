/**
 * Financial types for inventory WMS dashboard
 */

import type { InventoryCategory } from './product.types';

export type { InventoryCategory };
export type TrendDirection = 'up' | 'down' | 'stable';
export type AlertSeverity = 'critical' | 'warning' | 'info';
export type AlertType = 'low_stock' | 'expiring' | 'hold' | 'sync' | 'variance' | 'lab_test';

/**
 * Financial summary for dashboard
 */
export interface FinancialSummary {
  valueByCategory: Record<InventoryCategory, number>;
  totalInventoryValue: number;
  cogsLast30Days: number;
  cogsLast90Days: number;
  cogsTrend: CogsTrendPoint[];
  grossMarginPercent: number;
  grossMarginTrend: TrendDirection;
  valueAtRisk: ValueAtRisk;
}

/**
 * COGS trend data point
 */
export interface CogsTrendPoint {
  date: string;
  value: number;
}

/**
 * Value at risk breakdown
 */
export interface ValueAtRisk {
  expiring7Days: number;
  expiring30Days: number;
  expired: number;
  quarantined: number;
  coaFailed: number;
  onHold: number;
  pendingLab: number;
  total: number;
}

/**
 * Category value summary
 */
export interface CategoryValue {
  category: InventoryCategory;
  packageCount: number;
  totalQuantity: number;
  totalValue: number;
  averageUnitCost: number;
  totalReserved: number;
  totalAvailable: number;
}

/**
 * Inventory aging bucket
 */
export interface AgingBucket {
  value0To30: number;
  value31To60: number;
  value61To90: number;
  value91To180: number;
  value180Plus: number;
  count0To30: number;
  count31To60: number;
  count61To90: number;
  count91To180: number;
  count180Plus: number;
}

/**
 * Inventory aging analysis
 */
export interface AgingAnalysis {
  byCategory: Record<string, AgingBucket>;
  total: AgingBucket;
}

/**
 * Turnover metrics
 */
export interface TurnoverMetrics {
  averageInventoryValue: number;
  cogsLast30Days: number;
  cogsLast90Days: number;
  cogsLastYear: number;
  turnoverRateAnnualized: number;
  daysOnHand: number | null;
  turnoverByCategory: Record<string, number>;
}

/**
 * Alert item
 */
export interface AlertItem {
  type: AlertType;
  severity: AlertSeverity;
  title: string;
  description: string;
  relatedId?: string;
  relatedLabel?: string;
  valueImpact?: number;
  createdAt: string;
}

/**
 * Alert summary
 */
export interface AlertSummary {
  lowStockCount: number;
  expiringCount: number;
  onHoldCount: number;
  pendingSyncCount: number;
  failedLabTestCount: number;
  varianceCount: number;
  totalAlertValue: number;
  topAlerts: AlertItem[];
}

/**
 * Dashboard KPIs
 */
export interface DashboardKpis {
  totalInventoryValue: number;
  totalInventoryValueChange: number;
  totalPackages: number;
  totalPackagesChange: number;
  activeHolds: number;
  valueOnHold: number;
  pendingSyncs: number;
  daysOnHandAverage: number;
  cogsLast30Days: number;
  cogsChange: number;
  valueAtRisk: number;
  lowStockItems: number;
  expiringPackages: number;
}



