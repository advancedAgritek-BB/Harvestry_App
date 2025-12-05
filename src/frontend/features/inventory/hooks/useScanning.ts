/**
 * useScanning Hook
 * Barcode scanning operations and state management
 */

import { useCallback, useState, useEffect, useRef } from 'react';
import { useInventoryStore } from '../stores/inventoryStore';
import * as scanningService from '../services/scanning.service';
import type { ParsedGS1Barcode, ScanResult, ScanHistoryEntry } from '../services/scanning.service';
import type { InventoryMovement } from '../types';

interface UseScanningOptions {
  onScanSuccess?: (result: ScanResult) => void;
  onScanError?: (error: string) => void;
  onMovementComplete?: (movement: InventoryMovement) => void;
}

export function useScanning(options: UseScanningOptions = {}) {
  const { onScanSuccess, onScanError, onMovementComplete } = options;
  const store = useInventoryStore();
  
  const [lastScan, setLastScan] = useState<ScanResult | null>(null);
  const [scanHistory, setScanHistory] = useState<ScanHistoryEntry[]>([]);
  const [isProcessing, setIsProcessing] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  // Quick move state
  const [quickMoveSource, setQuickMoveSource] = useState<string | null>(null);
  const [quickMoveMode, setQuickMoveMode] = useState(false);
  
  // Keyboard input buffer for hardware scanners
  const inputBuffer = useRef<string>('');
  const inputTimeout = useRef<NodeJS.Timeout | null>(null);
  
  // Load scan history on mount
  useEffect(() => {
    setScanHistory(scanningService.getScanHistory());
  }, []);
  
  /**
   * Process a barcode scan
   */
  const processScan = useCallback(async (barcode: string) => {
    if (!barcode.trim()) return;
    
    setIsProcessing(true);
    setError(null);
    
    try {
      const result = await scanningService.scanBarcode(barcode);
      setLastScan(result);
      
      // Add to history
      scanningService.addToScanHistory({
        barcode,
        parsedLotNumber: result.parsedData.lotNumber,
        success: result.parsedData.isValid,
      });
      setScanHistory(scanningService.getScanHistory());
      
      if (result.parsedData.isValid) {
        onScanSuccess?.(result);
        
        // Handle quick move mode
        if (quickMoveMode && quickMoveSource) {
          // This is the destination scan
          await executeQuickMove(quickMoveSource, barcode);
          setQuickMoveSource(null);
          setQuickMoveMode(false);
        }
      } else {
        const errorMsg = result.parsedData.errors.join(', ') || 'Invalid barcode';
        setError(errorMsg);
        onScanError?.(errorMsg);
      }
      
      return result;
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Scan failed';
      setError(errorMsg);
      onScanError?.(errorMsg);
      
      scanningService.addToScanHistory({
        barcode,
        success: false,
      });
      setScanHistory(scanningService.getScanHistory());
      
      throw err;
    } finally {
      setIsProcessing(false);
    }
  }, [quickMoveMode, quickMoveSource, onScanSuccess, onScanError]);
  
  /**
   * Execute a quick move between two scanned barcodes
   */
  const executeQuickMove = useCallback(async (
    sourceBarcode: string,
    destinationBarcode: string,
    quantity?: number
  ) => {
    setIsProcessing(true);
    try {
      const movement = await scanningService.quickMove({
        sourceBarcode,
        destinationBarcode,
        quantity,
      });
      
      store.addMovement(movement);
      onMovementComplete?.(movement);
      
      // Update history with action
      scanningService.addToScanHistory({
        barcode: destinationBarcode,
        action: 'quick_move',
        success: true,
      });
      setScanHistory(scanningService.getScanHistory());
      
      return movement;
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Quick move failed';
      setError(errorMsg);
      onScanError?.(errorMsg);
      throw err;
    } finally {
      setIsProcessing(false);
    }
  }, [store, onMovementComplete, onScanError]);
  
  /**
   * Start quick move mode
   */
  const startQuickMove = useCallback((sourceBarcode?: string) => {
    setQuickMoveMode(true);
    if (sourceBarcode) {
      setQuickMoveSource(sourceBarcode);
    }
  }, []);
  
  /**
   * Cancel quick move mode
   */
  const cancelQuickMove = useCallback(() => {
    setQuickMoveMode(false);
    setQuickMoveSource(null);
  }, []);
  
  /**
   * Parse a barcode locally (without API call)
   */
  const parseBarcode = useCallback((barcode: string): ParsedGS1Barcode => {
    return scanningService.parseGS1Barcode(barcode);
  }, []);
  
  /**
   * Validate barcode format
   */
  const validateBarcode = useCallback((barcode: string) => {
    return scanningService.validateBarcodeFormat(barcode);
  }, []);
  
  /**
   * Handle keyboard input from hardware scanners
   * Hardware scanners typically type quickly and end with Enter
   */
  const handleKeyboardInput = useCallback((event: KeyboardEvent) => {
    // Only process if scanner mode is active
    if (!store.scannerActive) return;
    
    // Clear previous timeout
    if (inputTimeout.current) {
      clearTimeout(inputTimeout.current);
    }
    
    if (event.key === 'Enter') {
      // Process the buffered input
      if (inputBuffer.current.length > 0) {
        processScan(inputBuffer.current);
        inputBuffer.current = '';
      }
    } else if (event.key.length === 1) {
      // Add character to buffer
      inputBuffer.current += event.key;
      
      // Clear buffer after 100ms of no input (human typing is slower)
      inputTimeout.current = setTimeout(() => {
        inputBuffer.current = '';
      }, 100);
    }
  }, [store.scannerActive, processScan]);
  
  // Set up keyboard listener for hardware scanners
  useEffect(() => {
    if (store.scannerActive) {
      window.addEventListener('keypress', handleKeyboardInput);
      return () => {
        window.removeEventListener('keypress', handleKeyboardInput);
      };
    }
  }, [store.scannerActive, handleKeyboardInput]);
  
  /**
   * Clear scan history
   */
  const clearHistory = useCallback(() => {
    scanningService.clearScanHistory();
    setScanHistory([]);
  }, []);
  
  /**
   * Toggle scanner mode
   */
  const toggleScanner = useCallback(() => {
    store.setScannerActive(!store.scannerActive);
  }, [store]);
  
  /**
   * Retry a previous scan
   */
  const retryScan = useCallback((entry: ScanHistoryEntry) => {
    return processScan(entry.barcode);
  }, [processScan]);
  
  return {
    // State
    lastScan,
    scanHistory,
    isProcessing,
    error,
    scannerActive: store.scannerActive,
    quickMoveMode,
    quickMoveSource,
    
    // Actions
    processScan,
    parseBarcode,
    validateBarcode,
    executeQuickMove,
    startQuickMove,
    cancelQuickMove,
    toggleScanner,
    clearHistory,
    retryScan,
    clearError: () => setError(null),
  };
}
