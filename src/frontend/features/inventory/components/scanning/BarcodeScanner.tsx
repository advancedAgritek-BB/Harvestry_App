'use client';

import React, { useState, useRef, useCallback, useEffect } from 'react';
import { Camera, Keyboard, X, Loader2, CheckCircle, AlertCircle } from 'lucide-react';
import { cn } from '@/lib/utils';

interface BarcodeScannerProps {
  isOpen: boolean;
  onClose: () => void;
  onScan: (barcode: string) => void;
  title?: string;
  placeholder?: string;
  expectedFormat?: 'package' | 'item' | 'location' | 'any';
}

type ScanMode = 'camera' | 'keyboard';
type ScanStatus = 'idle' | 'scanning' | 'success' | 'error';

export function BarcodeScanner({
  isOpen,
  onClose,
  onScan,
  title = 'Scan Barcode',
  placeholder = 'Scan or enter barcode...',
  expectedFormat = 'any',
}: BarcodeScannerProps) {
  const [mode, setMode] = useState<ScanMode>('keyboard');
  const [status, setStatus] = useState<ScanStatus>('idle');
  const [manualInput, setManualInput] = useState('');
  const [error, setError] = useState<string | null>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const videoRef = useRef<HTMLVideoElement>(null);
  const streamRef = useRef<MediaStream | null>(null);

  // Focus input when opening in keyboard mode
  useEffect(() => {
    if (isOpen && mode === 'keyboard') {
      setTimeout(() => inputRef.current?.focus(), 100);
    }
  }, [isOpen, mode]);

  // Cleanup camera on close
  useEffect(() => {
    if (!isOpen && streamRef.current) {
      streamRef.current.getTracks().forEach(track => track.stop());
      streamRef.current = null;
    }
  }, [isOpen]);

  const validateBarcode = useCallback((barcode: string): boolean => {
    if (!barcode.trim()) return false;

    switch (expectedFormat) {
      case 'package':
        return barcode.length >= 10 && barcode.length <= 30;
      case 'item':
        return barcode.length >= 3 && barcode.length <= 100;
      case 'location':
        return barcode.length >= 1 && barcode.length <= 50;
      default:
        return barcode.length >= 1;
    }
  }, [expectedFormat]);

  const handleManualSubmit = useCallback(() => {
    const barcode = manualInput.trim().toUpperCase();
    
    if (!validateBarcode(barcode)) {
      setError('Invalid barcode format');
      setStatus('error');
      setTimeout(() => setStatus('idle'), 2000);
      return;
    }

    setStatus('success');
    setTimeout(() => {
      onScan(barcode);
      setManualInput('');
      setStatus('idle');
      onClose();
    }, 500);
  }, [manualInput, validateBarcode, onScan, onClose]);

  const handleKeyDown = useCallback((e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      handleManualSubmit();
    }
    if (e.key === 'Escape') {
      onClose();
    }
  }, [handleManualSubmit, onClose]);

  const startCamera = useCallback(async () => {
    try {
      setStatus('scanning');
      const stream = await navigator.mediaDevices.getUserMedia({
        video: { facingMode: 'environment' },
      });
      streamRef.current = stream;
      if (videoRef.current) {
        videoRef.current.srcObject = stream;
      }
      setMode('camera');
    } catch (err) {
      console.error('Camera access denied:', err);
      setError('Camera access denied. Please use manual entry.');
      setStatus('error');
      setTimeout(() => setStatus('idle'), 3000);
    }
  }, []);

  const stopCamera = useCallback(() => {
    if (streamRef.current) {
      streamRef.current.getTracks().forEach(track => track.stop());
      streamRef.current = null;
    }
    setMode('keyboard');
    setStatus('idle');
  }, []);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      {/* Backdrop */}
      <div className="absolute inset-0 bg-black/60 backdrop-blur-sm" onClick={onClose} />
      
      {/* Modal */}
      <div className="relative bg-background border border-border rounded-xl shadow-2xl w-full max-w-md mx-4 overflow-hidden">
        {/* Header */}
        <div className="flex items-center justify-between p-4 border-b border-border">
          <div className="flex items-center gap-2">
            {mode === 'camera' ? <Camera className="h-5 w-5" /> : <Keyboard className="h-5 w-5" />}
            <h2 className="text-lg font-semibold text-foreground">{title}</h2>
          </div>
          <button
            onClick={onClose}
            className="p-2 rounded-lg hover:bg-muted transition-colors"
          >
            <X className="h-5 w-5 text-muted-foreground" />
          </button>
        </div>

        <div className="p-4 space-y-4">
          {/* Mode toggle */}
          <div className="flex gap-2">
            <button
              className={cn(
                'flex-1 py-2 px-4 rounded-lg text-sm font-medium transition-colors',
                mode === 'keyboard'
                  ? 'bg-primary text-primary-foreground'
                  : 'bg-muted text-muted-foreground hover:bg-muted/80'
              )}
              onClick={() => { stopCamera(); setMode('keyboard'); }}
            >
              <Keyboard className="h-4 w-4 inline mr-2" />
              Manual Entry
            </button>
            <button
              className={cn(
                'flex-1 py-2 px-4 rounded-lg text-sm font-medium transition-colors',
                mode === 'camera'
                  ? 'bg-primary text-primary-foreground'
                  : 'bg-muted text-muted-foreground hover:bg-muted/80'
              )}
              onClick={startCamera}
            >
              <Camera className="h-4 w-4 inline mr-2" />
              Camera Scan
            </button>
          </div>

          {/* Camera view */}
          {mode === 'camera' && (
            <div className="relative aspect-video bg-black rounded-lg overflow-hidden">
              <video
                ref={videoRef}
                autoPlay
                playsInline
                className="w-full h-full object-cover"
              />
              <div className="absolute inset-0 border-2 border-dashed border-white/50 m-8 pointer-events-none" />
              {status === 'scanning' && (
                <div className="absolute bottom-4 left-1/2 -translate-x-1/2 bg-black/70 text-white px-4 py-2 rounded-full text-sm flex items-center gap-2">
                  <Loader2 className="h-4 w-4 animate-spin" />
                  Scanning...
                </div>
              )}
            </div>
          )}

          {/* Manual input */}
          {mode === 'keyboard' && (
            <div className="space-y-2">
              <div className="relative">
                <input
                  ref={inputRef}
                  value={manualInput}
                  onChange={(e) => {
                    setManualInput(e.target.value);
                    setError(null);
                    setStatus('idle');
                  }}
                  onKeyDown={handleKeyDown}
                  placeholder={placeholder}
                  className={cn(
                    'w-full px-4 py-3 pr-10 text-lg font-mono rounded-lg border bg-background text-foreground',
                    'focus:outline-none focus:ring-2 focus:ring-primary/50',
                    status === 'error' ? 'border-rose-500' :
                    status === 'success' ? 'border-emerald-500' : 'border-border'
                  )}
                  autoComplete="off"
                  autoFocus
                />
                <div className="absolute right-3 top-1/2 -translate-y-1/2">
                  {status === 'success' && <CheckCircle className="h-5 w-5 text-emerald-500" />}
                  {status === 'error' && <AlertCircle className="h-5 w-5 text-rose-500" />}
                </div>
              </div>
              {error && (
                <p className="text-sm text-rose-500">{error}</p>
              )}
              <p className="text-xs text-muted-foreground">
                Press Enter to submit or use a barcode scanner
              </p>
            </div>
          )}

          {/* Actions */}
          <div className="flex justify-end gap-2 pt-2">
            <button
              onClick={onClose}
              className="px-4 py-2 rounded-lg border border-border text-muted-foreground hover:bg-muted transition-colors"
            >
              Cancel
            </button>
            {mode === 'keyboard' && (
              <button
                onClick={handleManualSubmit}
                disabled={!manualInput.trim()}
                className={cn(
                  'px-4 py-2 rounded-lg font-medium transition-colors',
                  manualInput.trim()
                    ? 'bg-primary text-primary-foreground hover:bg-primary/90'
                    : 'bg-muted text-muted-foreground cursor-not-allowed'
                )}
              >
                Submit
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

export default BarcodeScanner;



