'use client';

import React, { useState, useRef, useEffect, useCallback } from 'react';
import { Scan, Camera, X, CheckCircle, AlertCircle, Package, ArrowRight, RotateCcw } from 'lucide-react';
import { cn } from '@/lib/utils';
import { parseGS1Barcode, validateBarcodeFormat } from '../../services/scanning.service';
import type { ParsedGS1Barcode, ScanResult, ScanAction } from '../../services/scanning.service';

interface ScannerInputProps {
  onScan: (barcode: string) => Promise<ScanResult | void>;
  onAction?: (action: ScanAction, result: ScanResult) => void;
  placeholder?: string;
  autoFocus?: boolean;
  showCamera?: boolean;
  className?: string;
}

export function ScannerInput({
  onScan,
  onAction,
  placeholder = 'Scan or enter barcode...',
  autoFocus = true,
  showCamera = true,
  className,
}: ScannerInputProps) {
  const [inputValue, setInputValue] = useState('');
  const [isProcessing, setIsProcessing] = useState(false);
  const [lastResult, setLastResult] = useState<ScanResult | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [cameraActive, setCameraActive] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);
  const videoRef = useRef<HTMLVideoElement>(null);
  const streamRef = useRef<MediaStream | null>(null);

  // Auto-focus input on mount
  useEffect(() => {
    if (autoFocus && inputRef.current) {
      inputRef.current.focus();
    }
  }, [autoFocus]);

  // Cleanup camera on unmount
  useEffect(() => {
    return () => {
      if (streamRef.current) {
        streamRef.current.getTracks().forEach(track => track.stop());
      }
    };
  }, []);

  const handleSubmit = async (barcode: string) => {
    if (!barcode.trim() || isProcessing) return;

    setIsProcessing(true);
    setError(null);

    try {
      const result = await onScan(barcode.trim());
      if (result) {
        setLastResult(result);
        // Play success sound/haptic
        if ('vibrate' in navigator) {
          navigator.vibrate(50);
        }
      }
      setInputValue('');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Scan failed');
      setLastResult(null);
    } finally {
      setIsProcessing(false);
      inputRef.current?.focus();
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleSubmit(inputValue);
    }
  };

  const startCamera = async () => {
    try {
      const stream = await navigator.mediaDevices.getUserMedia({
        video: { facingMode: 'environment' }
      });
      streamRef.current = stream;
      if (videoRef.current) {
        videoRef.current.srcObject = stream;
      }
      setCameraActive(true);
    } catch (err) {
      setError('Camera access denied');
    }
  };

  const stopCamera = () => {
    if (streamRef.current) {
      streamRef.current.getTracks().forEach(track => track.stop());
      streamRef.current = null;
    }
    setCameraActive(false);
  };

  const handleActionClick = (action: ScanAction) => {
    if (lastResult && onAction) {
      onAction(action, lastResult);
    }
  };

  const clearResult = () => {
    setLastResult(null);
    setError(null);
    inputRef.current?.focus();
  };

  return (
    <div className={cn('space-y-4', className)}>
      {/* Input Area */}
      <div className="relative">
        <div className="absolute left-3 top-1/2 -translate-y-1/2">
          <Scan className={cn(
            'w-5 h-5 transition-colors',
            isProcessing ? 'text-cyan-400 animate-pulse' : 'text-muted-foreground'
          )} />
        </div>
        <input
          ref={inputRef}
          type="text"
          value={inputValue}
          onChange={(e) => setInputValue(e.target.value)}
          onKeyDown={handleKeyDown}
          placeholder={placeholder}
          disabled={isProcessing}
          className={cn(
            'w-full pl-11 pr-24 py-3 bg-muted/30 border rounded-xl text-sm text-foreground',
            'placeholder:text-muted-foreground focus:outline-none transition-colors',
            isProcessing 
              ? 'border-cyan-500/30 bg-cyan-500/5' 
              : 'border-border focus:border-cyan-500/30'
          )}
        />
        <div className="absolute right-2 top-1/2 -translate-y-1/2 flex items-center gap-1">
          {showCamera && (
            <button
              onClick={cameraActive ? stopCamera : startCamera}
              className={cn(
                'p-2 rounded-lg transition-colors',
                cameraActive 
                  ? 'bg-rose-500/10 text-rose-400' 
                  : 'hover:bg-white/5 text-muted-foreground hover:text-foreground'
              )}
            >
              <Camera className="w-4 h-4" />
            </button>
          )}
          {inputValue && (
            <button
              onClick={() => handleSubmit(inputValue)}
              disabled={isProcessing}
              className="px-3 py-1.5 rounded-lg bg-cyan-500/10 text-cyan-400 text-xs font-medium hover:bg-cyan-500/20 transition-colors"
            >
              Scan
            </button>
          )}
        </div>
      </div>

      {/* Camera Preview */}
      {cameraActive && (
        <div className="relative rounded-xl overflow-hidden border border-border">
          <video
            ref={videoRef}
            autoPlay
            playsInline
            className="w-full aspect-video object-cover"
          />
          <div className="absolute inset-0 flex items-center justify-center">
            <div className="w-64 h-16 border-2 border-cyan-400 rounded-lg" />
          </div>
          <button
            onClick={stopCamera}
            className="absolute top-2 right-2 p-2 rounded-lg bg-black/50 text-foreground hover:bg-black/70 transition-colors"
          >
            <X className="w-4 h-4" />
          </button>
        </div>
      )}

      {/* Error Message */}
      {error && (
        <div className="flex items-center gap-2 p-3 rounded-lg bg-rose-500/10 border border-rose-500/20">
          <AlertCircle className="w-4 h-4 text-rose-400 shrink-0" />
          <span className="text-sm text-rose-400">{error}</span>
          <button onClick={() => setError(null)} className="ml-auto">
            <X className="w-4 h-4 text-rose-400" />
          </button>
        </div>
      )}

      {/* Scan Result */}
      {lastResult && (
        <div className="rounded-xl bg-muted/30 border border-border overflow-hidden">
          {/* Header */}
          <div className={cn(
            'px-4 py-3 border-b border-border flex items-center justify-between',
            lastResult.parsedData.isValid 
              ? 'bg-emerald-500/5' 
              : 'bg-amber-500/5'
          )}>
            <div className="flex items-center gap-2">
              {lastResult.parsedData.isValid ? (
                <CheckCircle className="w-4 h-4 text-emerald-400" />
              ) : (
                <AlertCircle className="w-4 h-4 text-amber-400" />
              )}
              <span className="text-sm font-medium text-foreground">
                {lastResult.parsedData.isValid ? 'Barcode Recognized' : 'Partial Match'}
              </span>
            </div>
            <button onClick={clearResult} className="p-1 hover:bg-white/5 rounded">
              <X className="w-4 h-4 text-muted-foreground" />
            </button>
          </div>

          {/* Parsed Data */}
          <div className="p-4 space-y-3">
            {lastResult.lot ? (
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-lg bg-cyan-500/10 flex items-center justify-center">
                  <Package className="w-5 h-5 text-cyan-400" />
                </div>
                <div>
                  <div className="text-sm font-mono text-foreground">{lastResult.lot.lotNumber}</div>
                  <div className="text-xs text-muted-foreground">
                    {lastResult.lot.strainName} • {lastResult.lot.quantity} {lastResult.lot.uom}
                  </div>
                </div>
              </div>
            ) : (
              <div className="space-y-2">
                {lastResult.parsedData.lotNumber && (
                  <div className="flex justify-between text-sm">
                    <span className="text-muted-foreground">Lot Number</span>
                    <span className="font-mono text-foreground">{lastResult.parsedData.lotNumber}</span>
                  </div>
                )}
                {lastResult.parsedData.gtin && (
                  <div className="flex justify-between text-sm">
                    <span className="text-muted-foreground">GTIN</span>
                    <span className="font-mono text-foreground">{lastResult.parsedData.gtin}</span>
                  </div>
                )}
                {lastResult.parsedData.expirationDate && (
                  <div className="flex justify-between text-sm">
                    <span className="text-muted-foreground">Expiration</span>
                    <span className="text-foreground">{lastResult.parsedData.expirationDate}</span>
                  </div>
                )}
              </div>
            )}

            {/* Actions */}
            {lastResult.suggestedActions.length > 0 && (
              <div className="flex flex-wrap gap-2 pt-3 border-t border-border">
                {lastResult.suggestedActions.map((action, i) => (
                  <button
                    key={i}
                    onClick={() => handleActionClick(action)}
                    disabled={!action.enabled}
                    className={cn(
                      'flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-medium transition-colors',
                      action.enabled
                        ? 'bg-white/5 text-foreground hover:bg-white/10'
                        : 'bg-muted/30 text-muted-foreground cursor-not-allowed'
                    )}
                  >
                    {action.type === 'move' && <ArrowRight className="w-3 h-3" />}
                    {action.type === 'view' && <Package className="w-3 h-3" />}
                    {action.label}
                  </button>
                ))}
              </div>
            )}
          </div>

          {/* Scan Another */}
          <div className="px-4 py-3 border-t border-border bg-white/[0.01]">
            <button
              onClick={clearResult}
              className="flex items-center gap-2 text-xs text-cyan-400 hover:underline"
            >
              <RotateCcw className="w-3 h-3" />
              Scan Another
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

export default ScannerInput;
