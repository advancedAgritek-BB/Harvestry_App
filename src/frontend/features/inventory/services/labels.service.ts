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
  labelType: 'product' | 'package' | 'manifest' | 'batch' | 'location' | 'lot';
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

/**
 * Zebra Printer Configuration
 */
export type ZebraPrinterDensity = '6dpmm' | '8dpmm' | '12dpmm' | '24dpmm';

export interface ZebraPrinterConfig {
  id: string;
  name: string;
  ipAddress?: string;
  port?: number;
  density: ZebraPrinterDensity;
  darkness: number; // 0-30
  printSpeed: number; // inches per second
  isDefault: boolean;
}

export interface PrinterInfo {
  id: string;
  name: string;
  type: 'zebra' | 'dymo' | 'brother' | 'generic';
  isOnline: boolean;
  config?: ZebraPrinterConfig;
}

const PRINTER_STORAGE_KEY = 'harvestry_printer_config';

/**
 * Get saved printer configuration from localStorage
 */
export function getSavedPrinterConfig(): ZebraPrinterConfig | null {
  if (typeof window === 'undefined') return null;
  const saved = localStorage.getItem(PRINTER_STORAGE_KEY);
  return saved ? JSON.parse(saved) : null;
}

/**
 * Save printer configuration to localStorage
 */
export function savePrinterConfig(config: ZebraPrinterConfig): void {
  if (typeof window === 'undefined') return;
  localStorage.setItem(PRINTER_STORAGE_KEY, JSON.stringify(config));
}

/**
 * Clear saved printer configuration
 */
export function clearPrinterConfig(): void {
  if (typeof window === 'undefined') return;
  localStorage.removeItem(PRINTER_STORAGE_KEY);
}

/**
 * Barcode Generation Options
 */
export interface BarcodeGenerationOptions {
  width?: number;
  height?: number;
  scale?: number;
  includeText?: boolean;
  textSize?: number;
  backgroundColor?: string;
  barcodeColor?: string;
}

export type BarcodeFormat = 'gs1-128' | 'code128' | 'qr' | 'datamatrix';

/**
 * Generate barcode image as data URL using bwip-js
 * This is a client-side function for preview purposes
 */
export async function generateBarcodeImage(
  format: BarcodeFormat,
  data: string,
  options: BarcodeGenerationOptions = {}
): Promise<string> {
  // Dynamic import for client-side only
  const bwipjs = await import('bwip-js');
  
  const {
    width = 200,
    height = format === 'qr' || format === 'datamatrix' ? 200 : 80,
    scale = 2,
    includeText = format !== 'qr' && format !== 'datamatrix',
    textSize = 10,
    backgroundColor = 'ffffff',
    barcodeColor = '000000',
  } = options;

  // Map our format names to bwip-js encoder names
  const encoderMap: Record<BarcodeFormat, string> = {
    'gs1-128': 'gs1-128',
    'code128': 'code128',
    'qr': 'qrcode',
    'datamatrix': 'datamatrix',
  };

  try {
    const canvas = document.createElement('canvas');
    
    await bwipjs.toCanvas(canvas, {
      bcid: encoderMap[format],
      text: data,
      scale,
      height: format === 'qr' || format === 'datamatrix' ? undefined : height / 10,
      width: format === 'qr' || format === 'datamatrix' ? undefined : width / 10,
      includetext: includeText,
      textsize: textSize,
      backgroundcolor: backgroundColor,
      barcolor: barcodeColor,
    });

    return canvas.toDataURL('image/png');
  } catch (error) {
    console.error('Failed to generate barcode:', error);
    throw new Error(`Failed to generate ${format} barcode: ${error}`);
  }
}

/**
 * Generate a GS1-128 barcode string from lot data
 * Uses the existing generateGS1Barcode logic
 */
export function buildGS1BarcodeData(data: {
  gtin?: string;
  lotNumber: string;
  serial?: string;
  expirationDate?: Date;
  quantity?: number;
}): string {
  let barcode = '';

  // GTIN (AI 01)
  if (data.gtin) {
    barcode += `01${data.gtin.padStart(14, '0')}`;
  }

  // Lot number (AI 10)
  barcode += `10${data.lotNumber}`;

  // Serial (AI 21)
  if (data.serial) {
    barcode += `21${data.serial}`;
  }

  // Expiration date (AI 17 - YYMMDD)
  if (data.expirationDate) {
    const yy = String(data.expirationDate.getFullYear()).slice(-2);
    const mm = String(data.expirationDate.getMonth() + 1).padStart(2, '0');
    const dd = String(data.expirationDate.getDate()).padStart(2, '0');
    barcode += `17${yy}${mm}${dd}`;
  }

  // Quantity (AI 30)
  if (data.quantity !== undefined) {
    barcode += `30${data.quantity}`;
  }

  return barcode;
}

/**
 * Entity types that can have labels
 */
export type LabelEntityType = 'lot' | 'package' | 'batch' | 'location' | 'product' | 'manifest';

/**
 * Get the default template for an entity type
 */
export function getDefaultTemplateForEntity(
  templates: LabelTemplate[],
  entityType: LabelEntityType
): LabelTemplate | undefined {
  // First try to find a default template for this type
  const defaultForType = templates.find(
    t => t.labelType === entityType && t.isDefault && t.isActive
  );
  if (defaultForType) return defaultForType;

  // Fall back to any active template for this type
  return templates.find(t => t.labelType === entityType && t.isActive);
}
