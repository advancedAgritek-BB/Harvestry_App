export interface ReportConfig {
  source: string;
  columns: ReportColumn[];
  filters: ReportFilter[];
  sorts: ReportSort[];
}

export interface ReportColumn {
  field: string;
  aggregation?: string | null;
  alias?: string | null;
  format?: string | null;
}

export interface ReportFilter {
  field: string;
  operator: string;
  value: any;
}

export interface ReportSort {
  field: string;
  direction: 'asc' | 'desc';
}

export interface Report {
  id: string;
  name: string;
  description?: string;
  config: ReportConfig;
  visualizationConfigJson: string;
  isPublic: boolean;
  ownerId: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateReportDto {
  name: string;
  description?: string;
  config: ReportConfig;
  visualizationConfigJson?: string;
  isPublic: boolean;
}

export interface UpdateReportDto {
  name: string;
  description?: string;
  config: ReportConfig;
  visualizationConfigJson?: string;
  isPublic: boolean;
}

export interface Dashboard {
  id: string;
  name: string;
  description?: string;
  layoutConfig: DashboardWidget[];
  isPublic: boolean;
  ownerId: string;
  createdAt: string;
  updatedAt: string;
}

export interface DashboardWidget {
  id: string;
  reportId: string;
  x: number;
  y: number;
  w: number;
  h: number;
  title: string;
  visualizationType: string;
}

export interface CreateDashboardDto {
  name: string;
  description?: string;
  layoutConfig: DashboardWidget[];
  isPublic: boolean;
}

export interface UpdateDashboardDto {
  name: string;
  description?: string;
  layoutConfig: DashboardWidget[];
  isPublic: boolean;
}






