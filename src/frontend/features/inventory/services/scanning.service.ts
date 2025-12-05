/**
 * Scanning Service
 * Barcode parsing, GS1 decoding, and scan-to-movement flows
 */

import type { InventoryLot, InventoryMovement } from '../types';

const API_BASE = '/api/inventory';

/** GS1 Application Identifiers */
export const GS1_AI = {
  GTIN: '01',
  LOT_NUMBER: '10',
  SERIAL: '21',
  EXPIRATION: '17',
  PRODUCTION_DATE: '11',
  BEST_BY: '15',
  QUANTITY: '30',
  WEIGHT_KG: '310',
  WEIGHT_LB: '320',
} as const;

/** Parsed GS1 barcode data */
export interface ParsedGS1Barcode {
  raw: string;
  gtin?: string;
  lotNumber?: string;
  serial?: string;
  expirationDate?: string;
  productionDate?: string;
  quantity?: number;
  weight?: number;
  weightUnit?: 'kg' | 'lb';
  isValid: boolean;
  errors: string[];
}

/** Scan result from API */
export interface ScanResult {
  barcode: string;
  parsedData: ParsedGS1Barcode;
  lot?: InventoryLot;
  suggestedActions: ScanAction[];
}

/** Suggested action after scan */
export interface ScanAction {
  type: 'move' | 'adjust' | 'view' | 'hold' | 'release';
  label: string;
  icon: string;
  enabled: boolean;
  reason?: string;
}

/** Quick move request via scan */
export interface ScanMoveRequest {
  sourceBarcode: string;
  destinationBarcode: string;
  quantity?: number;
  notes?: string;
}

/**
 * Parse a GS1-128 barcode string locally
 */
export function parseGS1Barcode(barcode: string): ParsedGS1Barcode {
  const result: ParsedGS1Barcode = {
    raw: barcode,
    isValid: true,
    errors: [],
  };

  try {
    // Remove any FNC1 characters (often represented as ]C1 or \x1D)
    let data = barcode.replace(/\]C1|\x1D/g, '');
    
    // Process each AI
    while (data.length > 0) {
      let matched = false;

      // GTIN (01) - 14 digits
      if (data.startsWith('01') && data.length >= 16) {
        result.gtin = data.slice(2, 16);
        data = data.slice(16);
        matched = true;
      }
      // Lot Number (10) - variable length, ends at GS or end
      else if (data.startsWith('10')) {
        const remaining = data.slice(2);
        const gsIndex = remaining.indexOf('\x1D');
        const lotEnd = gsIndex > -1 ? gsIndex : Math.min(20, remaining.length);
        result.lotNumber = remaining.slice(0, lotEnd);
        data = remaining.slice(lotEnd + (gsIndex > -1 ? 1 : 0));
        matched = true;
      }
      // Serial (21) - variable length
      else if (data.startsWith('21')) {
        const remaining = data.slice(2);
        const gsIndex = remaining.indexOf('\x1D');
        const serialEnd = gsIndex > -1 ? gsIndex : Math.min(20, remaining.length);
        result.serial = remaining.slice(0, serialEnd);
        data = remaining.slice(serialEnd + (gsIndex > -1 ? 1 : 0));
        matched = true;
      }
      // Expiration Date (17) - YYMMDD
      else if (data.startsWith('17') && data.length >= 8) {
        const dateStr = data.slice(2, 8);
        result.expirationDate = parseGS1Date(dateStr);
        data = data.slice(8);
        matched = true;
      }
      // Production Date (11) - YYMMDD
      else if (data.startsWith('11') && data.length >= 8) {
        const dateStr = data.slice(2, 8);
        result.productionDate = parseGS1Date(dateStr);
        data = data.slice(8);
        matched = true;
      }
      // Count (30) - variable length numeric
      else if (data.startsWith('30')) {
        const remaining = data.slice(2);
        const numMatch = remaining.match(/^(\d+)/);
        if (numMatch) {
          result.quantity = parseInt(numMatch[1], 10);
          data = remaining.slice(numMatch[1].length);
          matched = true;
        }
      }
      // Weight in kg (310X) - 6 digits with X decimal places
      else if (data.match(/^310[0-9]/) && data.length >= 10) {
        const decimals = parseInt(data[3], 10);
        const value = parseInt(data.slice(4, 10), 10);
        result.weight = value / Math.pow(10, decimals);
        result.weightUnit = 'kg';
        data = data.slice(10);
        matched = true;
      }
      // Weight in lb (320X) - 6 digits with X decimal places
      else if (data.match(/^320[0-9]/) && data.length >= 10) {
        const decimals = parseInt(data[3], 10);
        const value = parseInt(data.slice(4, 10), 10);
        result.weight = value / Math.pow(10, decimals);
        result.weightUnit = 'lb';
        data = data.slice(10);
        matched = true;
      }

      if (!matched) {
        // Unknown AI or invalid format - skip one character
        result.errors.push(`Unknown AI or format at: ${data.slice(0, 10)}...`);
        data = data.slice(1);
      }
    }

    // Validate we got at least a lot number
    if (!result.lotNumber) {
      result.isValid = false;
      result.errors.push('No lot number found in barcode');
    }
  } catch (error) {
    result.isValid = false;
    result.errors.push(`Parse error: ${error instanceof Error ? error.message : 'Unknown'}`);
  }

  return result;
}

/**
 * Parse GS1 date format (YYMMDD) to ISO date string
 */
function parseGS1Date(dateStr: string): string {
  if (dateStr.length !== 6) return '';
  
  const yy = parseInt(dateStr.slice(0, 2), 10);
  const mm = dateStr.slice(2, 4);
  const dd = dateStr.slice(4, 6);
  
  // Assume 20xx for years 00-49, 19xx for 50-99
  const year = yy < 50 ? 2000 + yy : 1900 + yy;
  
  return `${year}-${mm}-${dd}`;
}

/**
 * Scan a barcode and get lot info + suggested actions
 */
export async function scanBarcode(barcode: string): Promise<ScanResult> {
  const response = await fetch(`${API_BASE}/scan`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ barcode }),
  });
  
  if (!response.ok) {
    // If API fails, try local parsing
    const parsed = parseGS1Barcode(barcode);
    return {
      barcode,
      parsedData: parsed,
      suggestedActions: parsed.isValid
        ? [
            { type: 'view', label: 'View Lot', icon: 'eye', enabled: true },
            { type: 'move', label: 'Move', icon: 'move', enabled: true },
          ]
        : [],
    };
  }
  
  return response.json();
}

/**
 * Execute a quick move via barcode scans
 */
export async function quickMove(request: ScanMoveRequest): Promise<InventoryMovement> {
  const response = await fetch(`${API_BASE}/scan/quick-move`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  
  if (!response.ok) throw new Error('Failed to execute quick move');
  return response.json();
}

/**
 * Validate a barcode format
 */
export function validateBarcodeFormat(barcode: string): {
  isValid: boolean;
  format: 'gs1-128' | 'qr' | 'code128' | 'datamatrix' | 'unknown';
  message?: string;
} {
  // GS1-128 typically starts with ]C1 or has AI patterns
  if (barcode.startsWith(']C1') || /^0[1-9]/.test(barcode)) {
    return { isValid: true, format: 'gs1-128' };
  }
  
  // Check for common AI patterns
  if (/^\d{14}/.test(barcode) || barcode.includes('10') || barcode.includes('21')) {
    return { isValid: true, format: 'gs1-128' };
  }
  
  // QR codes often contain URLs or structured data
  if (barcode.startsWith('http') || barcode.includes('://')) {
    return { isValid: true, format: 'qr' };
  }
  
  // Alphanumeric only - likely Code 128
  if (/^[A-Z0-9-]+$/i.test(barcode)) {
    return { isValid: true, format: 'code128' };
  }
  
  return { isValid: false, format: 'unknown', message: 'Unrecognized barcode format' };
}

/**
 * Generate a GS1-128 barcode string from lot data
 */
export function generateGS1Barcode(data: {
  gtin?: string;
  lotNumber: string;
  serial?: string;
  expirationDate?: string;
  quantity?: number;
}): string {
  let barcode = '';
  
  if (data.gtin) {
    barcode += `01${data.gtin.padStart(14, '0')}`;
  }
  
  barcode += `10${data.lotNumber}`;
  
  if (data.serial) {
    barcode += `\x1D21${data.serial}`;
  }
  
  if (data.expirationDate) {
    const date = new Date(data.expirationDate);
    const yy = String(date.getFullYear()).slice(-2);
    const mm = String(date.getMonth() + 1).padStart(2, '0');
    const dd = String(date.getDate()).padStart(2, '0');
    barcode += `17${yy}${mm}${dd}`;
  }
  
  if (data.quantity) {
    barcode += `30${data.quantity}`;
  }
  
  return barcode;
}

/** Scan history entry */
export interface ScanHistoryEntry {
  id: string;
  barcode: string;
  parsedLotNumber?: string;
  action?: string;
  success: boolean;
  timestamp: string;
}

/** Local scan history (persisted to localStorage) */
const SCAN_HISTORY_KEY = 'harvestry_scan_history';
const MAX_HISTORY = 100;

export function getScanHistory(): ScanHistoryEntry[] {
  if (typeof window === 'undefined') return [];
  const stored = localStorage.getItem(SCAN_HISTORY_KEY);
  return stored ? JSON.parse(stored) : [];
}

export function addToScanHistory(entry: Omit<ScanHistoryEntry, 'id' | 'timestamp'>): void {
  if (typeof window === 'undefined') return;
  
  const history = getScanHistory();
  history.unshift({
    ...entry,
    id: crypto.randomUUID(),
    timestamp: new Date().toISOString(),
  });
  
  // Keep only recent entries
  localStorage.setItem(SCAN_HISTORY_KEY, JSON.stringify(history.slice(0, MAX_HISTORY)));
}

export function clearScanHistory(): void {
  if (typeof window === 'undefined') return;
  localStorage.removeItem(SCAN_HISTORY_KEY);
}
