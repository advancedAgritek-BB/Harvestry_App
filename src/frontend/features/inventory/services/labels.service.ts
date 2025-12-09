/**
 * Labels Service
 * GS1 label generation, template management, and print queue
 */

const API_BASE = '/api/inventory/labels';

/** Label template definition */
export interface LabelTemplate {
  id: string;
  siteId: string;
  name: string;
  jurisdiction: string;
  labelType: 'product' | 'package' | 'manifest' | 'batch' | 'location';
  format: 'zpl' | 'pdf' | 'png';
  
  // Dimensions
  widthInches: number;
  heightInches: number;
  
  // Barcode settings
  barcodeFormat: 'gs1-128' | 'qr' | 'code128' | 'datamatrix';
  barcodePosition: { x: number; y: number; width: number; height: number };
  
  // Content layout
  fields: LabelField[];
  
  // Compliance
  requiredPhrases: string[];
  jurisdictionRules: Record<string, unknown>;
  
  // Status
  isActive: boolean;
  isDefault: boolean;
  
  // Audit
  createdAt: string;
  updatedAt: string;
}

/** Label field configuration */
export interface LabelField {
  id: string;
  fieldType: 'text' | 'variable' | 'barcode' | 'logo' | 'qr';
  
  // Position
  x: number;
  y: number;
  width: number;
  height: number;
  
  // Content
  variableName?: string; // e.g., 'lotNumber', 'strainName', 'thcPercent'
  staticText?: string;
  
  // Styling
  fontSize?: number;
  fontWeight?: 'normal' | 'bold';
  textAlign?: 'left' | 'center' | 'right';
  
  // Formatting
  prefix?: string;
  suffix?: string;
  dateFormat?: string;
  numberFormat?: string;
}

/** Label variable for template preview */
export interface LabelVariable {
  name: string;
  displayName: string;
  category: 'lot' | 'product' | 'compliance' | 'location' | 'date';
  sampleValue: string;
  format?: string;
}

/** Available label variables */
export const LABEL_VARIABLES: LabelVariable[] = [
  // Lot info
  { name: 'lotNumber', displayName: 'Lot Number', category: 'lot', sampleValue: 'LOT-2025-001234' },
  { name: 'lotBarcode', displayName: 'Lot Barcode', category: 'lot', sampleValue: '0100012345678901101234567' },
  { name: 'quantity', displayName: 'Quantity', category: 'lot', sampleValue: '100.0' },
  { name: 'uom', displayName: 'Unit of Measure', category: 'lot', sampleValue: 'g' },
  
  // Product info
  { name: 'strainName', displayName: 'Strain Name', category: 'product', sampleValue: 'Blue Dream' },
  { name: 'productType', displayName: 'Product Type', category: 'product', sampleValue: 'Flower' },
  { name: 'batchName', displayName: 'Batch Name', category: 'product', sampleValue: 'BD-2025-F01' },
  
  // Compliance
  { name: 'thcPercent', displayName: 'THC %', category: 'compliance', sampleValue: '22.5%' },
  { name: 'cbdPercent', displayName: 'CBD %', category: 'compliance', sampleValue: '0.5%' },
  { name: 'metrcId', displayName: 'METRC ID', category: 'compliance', sampleValue: '1A40500000001234567' },
  { name: 'biotrackId', displayName: 'BioTrack ID', category: 'compliance', sampleValue: 'BT1234567890' },
  { name: 'coaStatus', displayName: 'COA Status', category: 'compliance', sampleValue: 'PASSED' },
  
  // Location
  { name: 'locationName', displayName: 'Location', category: 'location', sampleValue: 'Vault A - Shelf 1' },
  { name: 'locationPath', displayName: 'Location Path', category: 'location', sampleValue: 'Warehouse > Vault A > Shelf 1' },
  { name: 'locationBarcode', displayName: 'Location Barcode', category: 'location', sampleValue: 'LOC-WH-VA-S1' },
  
  // Dates
  { name: 'harvestDate', displayName: 'Harvest Date', category: 'date', sampleValue: '2025-01-15', format: 'MM/DD/YYYY' },
  { name: 'packageDate', displayName: 'Package Date', category: 'date', sampleValue: '2025-01-20', format: 'MM/DD/YYYY' },
  { name: 'expirationDate', displayName: 'Expiration Date', category: 'date', sampleValue: '2026-01-20', format: 'MM/DD/YYYY' },
  { name: 'printDate', displayName: 'Print Date', category: 'date', sampleValue: 'today', format: 'MM/DD/YYYY' },
];

/** Print job */
export interface PrintJob {
  id: string;
  templateId: string;
  lotIds: string[];
  quantity: number;
  printer?: string;
  status: 'queued' | 'printing' | 'completed' | 'failed';
  createdAt: string;
  completedAt?: string;
  error?: string;
}

/** Label preview data */
export interface LabelPreview {
  templateId: string;
  lotId?: string;
  previewUrl: string;
  width: number;
  height: number;
  variables: Record<string, string>;
}

/**
 * Template Management
 */
export async function getTemplates(
  filters: {
    siteId?: string;
    labelType?: string;
    jurisdiction?: string;
    isActive?: boolean;
  } = {}
): Promise<LabelTemplate[]> {
  const params = new URLSearchParams(
    Object.fromEntries(
      Object.entries(filters)
        .filter(([, v]) => v !== undefined)
        .map(([k, v]) => [k, String(v)])
    )
  );

  const response = await fetch(`${API_BASE}/templates?${params}`);
  if (!response.ok) throw new Error('Failed to fetch templates');
  return response.json();
}

export async function getTemplate(templateId: string): Promise<LabelTemplate> {
  const response = await fetch(`${API_BASE}/templates/${templateId}`);
  if (!response.ok) throw new Error('Failed to fetch template');
  return response.json();
}

export async function createTemplate(
  template: Omit<LabelTemplate, 'id' | 'createdAt' | 'updatedAt'>
): Promise<LabelTemplate> {
  const response = await fetch(`${API_BASE}/templates`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(template),
  });
  if (!response.ok) throw new Error('Failed to create template');
  return response.json();
}

export async function updateTemplate(
  templateId: string,
  updates: Partial<LabelTemplate>
): Promise<LabelTemplate> {
  const response = await fetch(`${API_BASE}/templates/${templateId}`, {
    method: 'PATCH',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(updates),
  });
  if (!response.ok) throw new Error('Failed to update template');
  return response.json();
}

export async function deleteTemplate(templateId: string): Promise<void> {
  const response = await fetch(`${API_BASE}/templates/${templateId}`, {
    method: 'DELETE',
  });
  if (!response.ok) throw new Error('Failed to delete template');
}

export async function duplicateTemplate(templateId: string, newName: string): Promise<LabelTemplate> {
  const response = await fetch(`${API_BASE}/templates/${templateId}/duplicate`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ name: newName }),
  });
  if (!response.ok) throw new Error('Failed to duplicate template');
  return response.json();
}

/**
 * Label Generation
 */
export async function generatePreview(
  templateId: string,
  lotId?: string,
  variables?: Record<string, string>
): Promise<LabelPreview> {
  const response = await fetch(`${API_BASE}/preview`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ templateId, lotId, variables }),
  });
  if (!response.ok) throw new Error('Failed to generate preview');
  return response.json();
}

export async function generateLabels(
  templateId: string,
  lotIds: string[],
  format: 'pdf' | 'zpl' | 'png' = 'pdf'
): Promise<Blob> {
  const response = await fetch(`${API_BASE}/generate`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ templateId, lotIds, format }),
  });
  if (!response.ok) throw new Error('Failed to generate labels');
  return response.blob();
}

/**
 * Print Queue
 */
export async function getPrintJobs(
  status?: 'queued' | 'printing' | 'completed' | 'failed'
): Promise<PrintJob[]> {
  const params = status ? `?status=${status}` : '';
  const response = await fetch(`${API_BASE}/print-jobs${params}`);
  if (!response.ok) throw new Error('Failed to fetch print jobs');
  return response.json();
}

export async function createPrintJob(request: {
  templateId: string;
  lotIds: string[];
  quantity?: number;
  printer?: string;
}): Promise<PrintJob> {
  const response = await fetch(`${API_BASE}/print-jobs`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to create print job');
  return response.json();
}

export async function cancelPrintJob(jobId: string): Promise<void> {
  const response = await fetch(`${API_BASE}/print-jobs/${jobId}/cancel`, {
    method: 'POST',
  });
  if (!response.ok) throw new Error('Failed to cancel print job');
}

export async function retryPrintJob(jobId: string): Promise<PrintJob> {
  const response = await fetch(`${API_BASE}/print-jobs/${jobId}/retry`, {
    method: 'POST',
  });
  if (!response.ok) throw new Error('Failed to retry print job');
  return response.json();
}

/**
 * Available Printers
 */
export async function getAvailablePrinters(): Promise<{ id: string; name: string; type: string; isOnline: boolean }[]> {
  const response = await fetch(`${API_BASE}/printers`);
  if (!response.ok) throw new Error('Failed to fetch printers');
  return response.json();
}

/**
 * Jurisdiction Rules
 */
export async function getJurisdictionRules(
  jurisdiction: string
): Promise<{ phrases: string[]; requirements: string[]; warnings: string[] }> {
  const response = await fetch(`${API_BASE}/jurisdictions/${jurisdiction}/rules`);
  if (!response.ok) throw new Error('Failed to fetch jurisdiction rules');
  return response.json();
}

export async function validateLabel(
  templateId: string,
  jurisdiction: string
): Promise<{ isValid: boolean; errors: string[]; warnings: string[] }> {
  const response = await fetch(`${API_BASE}/templates/${templateId}/validate`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ jurisdiction }),
  });
  if (!response.ok) throw new Error('Failed to validate label');
  return response.json();
}
