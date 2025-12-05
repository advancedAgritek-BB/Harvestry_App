'use client';

import React, { useState, useRef, useEffect } from 'react';
import { X, Camera, Flashlight, RotateCcw, CheckCircle, Package, ArrowRight, MapPin } from 'lucide-react';
import { cn } from '@/lib/utils';
import type { ScanResult } from '../../services/scanning.service';

interface MobileScannerSheetProps {
  isOpen: boolean;
  onClose: () => void;
  onScan: (barcode: string) => Promise<ScanResult | void>;
  mode?: 'scan' | 'quick-move';
}

export function MobileScannerSheet({ isOpen, onClose, onScan, mode = 'scan' }: MobileScannerSheetProps) {
  const [hasPermission, setHasPermission] = useState<boolean | null>(null);
  const [flashEnabled, setFlashEnabled] = useState(false);
  const [processing, setProcessing] = useState(false);
  const [lastResult, setLastResult] = useState<ScanResult | null>(null);
  const [error, setError] = useState<string | null>(null);
  const videoRef = useRef<HTMLVideoElement>(null);
  const streamRef = useRef<MediaStream | null>(null);

  useEffect(() => {
    if (isOpen) {
      startCamera();
    } else {
      stopCamera();
    }

    return () => {
      stopCamera();
    };
  }, [isOpen]);

  const startCamera = async () => {
    try {
      const stream = await navigator.mediaDevices.getUserMedia({
        video: {
          facingMode: 'environment',
          width: { ideal: 1280 },
          height: { ideal: 720 },
        },
      });
      streamRef.current = stream;
      if (videoRef.current) {
        videoRef.current.srcObject = stream;
      }
      setHasPermission(true);
    } catch (err) {
      setHasPermission(false);
      setError('Camera permission denied');
    }
  };

  const stopCamera = () => {
    if (streamRef.current) {
      streamRef.current.getTracks().forEach((track) => track.stop());
      streamRef.current = null;
    }
  };

  const toggleFlash = async () => {
    if (!streamRef.current) return;

    const track = streamRef.current.getVideoTracks()[0];
    const capabilities = track.getCapabilities() as MediaTrackCapabilities & { torch?: boolean };
    
    if (capabilities.torch) {
      try {
        await track.applyConstraints({
          advanced: [{ torch: !flashEnabled } as MediaTrackConstraintSet],
        });
        setFlashEnabled(!flashEnabled);
      } catch (err) {
        console.error('Flash not supported');
      }
    }
  };

  const handleManualInput = () => {
    const barcode = prompt('Enter barcode manually:');
    if (barcode) {
      handleScan(barcode);
    }
  };

  const handleScan = async (barcode: string) => {
    setProcessing(true);
    setError(null);

    try {
      const result = await onScan(barcode);
      if (result) {
        setLastResult(result);
        // Haptic feedback
        if ('vibrate' in navigator) {
          navigator.vibrate(50);
        }
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Scan failed');
    } finally {
      setProcessing(false);
    }
  };

  const clearResult = () => {
    setLastResult(null);
    setError(null);
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 bg-black">
      {/* Header */}
      <div className="absolute top-0 left-0 right-0 z-10 p-4 flex items-center justify-between bg-gradient-to-b from-black/80 to-transparent">
        <button
          onClick={onClose}
          className="p-2 rounded-full bg-black/50 text-foreground"
        >
          <X className="w-6 h-6" />
        </button>
        <span className="text-foreground font-medium">
          {mode === 'quick-move' ? 'Quick Move' : 'Scan Barcode'}
        </span>
        <div className="flex items-center gap-2">
          <button
            onClick={toggleFlash}
            className={cn(
              'p-2 rounded-full',
              flashEnabled ? 'bg-amber-500 text-black' : 'bg-black/50 text-foreground'
            )}
          >
            <Flashlight className="w-5 h-5" />
          </button>
        </div>
      </div>

      {/* Camera View */}
      <div className="relative w-full h-full">
        {hasPermission === false ? (
          <div className="absolute inset-0 flex items-center justify-center bg-black">
            <div className="text-center p-6">
              <Camera className="w-16 h-16 text-muted-foreground mx-auto mb-4" />
              <p className="text-foreground mb-4">Camera access is required for scanning</p>
              <button
                onClick={startCamera}
                className="px-4 py-2 rounded-lg bg-white text-black font-medium"
              >
                Enable Camera
              </button>
            </div>
          </div>
        ) : (
          <video
            ref={videoRef}
            autoPlay
            playsInline
            className="w-full h-full object-cover"
          />
        )}

        {/* Scan Area Overlay */}
        {hasPermission !== false && !lastResult && (
          <div className="absolute inset-0 flex items-center justify-center">
            {/* Darkened areas */}
            <div className="absolute inset-0 bg-black/50" />
            
            {/* Clear scan area */}
            <div className="relative w-72 h-48 border-2 border-cyan-400 rounded-xl bg-transparent z-10">
              {/* Corner markers */}
              <div className="absolute -top-1 -left-1 w-8 h-8 border-t-4 border-l-4 border-cyan-400 rounded-tl-lg" />
              <div className="absolute -top-1 -right-1 w-8 h-8 border-t-4 border-r-4 border-cyan-400 rounded-tr-lg" />
              <div className="absolute -bottom-1 -left-1 w-8 h-8 border-b-4 border-l-4 border-cyan-400 rounded-bl-lg" />
              <div className="absolute -bottom-1 -right-1 w-8 h-8 border-b-4 border-r-4 border-cyan-400 rounded-br-lg" />
              
              {/* Scan line animation */}
              {processing && (
                <div className="absolute inset-x-4 h-0.5 bg-cyan-400 animate-scan" />
              )}
            </div>
          </div>
        )}

        {/* Result Card */}
        {lastResult && (
          <div className="absolute inset-x-4 bottom-24 bg-surface rounded-2xl border border-border overflow-hidden">
            <div className={cn(
              'px-4 py-3 border-b border-border',
              lastResult.parsedData.isValid ? 'bg-emerald-500/10' : 'bg-amber-500/10'
            )}>
              <div className="flex items-center gap-2">
                <CheckCircle className={cn(
                  'w-5 h-5',
                  lastResult.parsedData.isValid ? 'text-emerald-400' : 'text-amber-400'
                )} />
                <span className="text-sm font-medium text-foreground">
                  {lastResult.parsedData.isValid ? 'Barcode Recognized' : 'Partial Match'}
                </span>
              </div>
            </div>

            <div className="p-4">
              {lastResult.lot ? (
                <div className="flex items-center gap-3 mb-4">
                  <div className="w-12 h-12 rounded-xl bg-amber-500/10 flex items-center justify-center">
                    <Package className="w-6 h-6 text-amber-400" />
                  </div>
                  <div>
                    <div className="text-base font-mono text-foreground">{lastResult.lot.lotNumber}</div>
                    <div className="text-sm text-muted-foreground">
                      {lastResult.lot.strainName} • {lastResult.lot.quantity} {lastResult.lot.uom}
                    </div>
                  </div>
                </div>
              ) : (
                <div className="mb-4">
                  <div className="text-sm text-muted-foreground mb-1">Lot Number</div>
                  <div className="text-lg font-mono text-foreground">{lastResult.parsedData.lotNumber || 'Unknown'}</div>
                </div>
              )}

              {/* Quick Actions */}
              <div className="grid grid-cols-2 gap-2">
                <button className="flex items-center justify-center gap-2 py-3 rounded-xl bg-white/5 text-foreground">
                  <Package className="w-4 h-4" />
                  <span className="text-sm font-medium">View Details</span>
                </button>
                <button className="flex items-center justify-center gap-2 py-3 rounded-xl bg-cyan-500/10 text-cyan-400">
                  <ArrowRight className="w-4 h-4" />
                  <span className="text-sm font-medium">Move</span>
                </button>
              </div>
            </div>

            <div className="px-4 py-3 border-t border-border">
              <button
                onClick={clearResult}
                className="flex items-center justify-center gap-2 w-full text-sm text-cyan-400"
              >
                <RotateCcw className="w-4 h-4" />
                Scan Another
              </button>
            </div>
          </div>
        )}

        {/* Error Message */}
        {error && (
          <div className="absolute inset-x-4 bottom-24 p-4 bg-rose-500/10 border border-rose-500/30 rounded-xl">
            <p className="text-sm text-rose-400 text-center">{error}</p>
            <button
              onClick={() => setError(null)}
              className="mt-2 w-full text-xs text-rose-400 underline"
            >
              Dismiss
            </button>
          </div>
        )}
      </div>

      {/* Bottom Controls */}
      <div className="absolute bottom-0 left-0 right-0 p-6 bg-gradient-to-t from-black to-transparent">
        <div className="flex items-center justify-center gap-4">
          <button
            onClick={handleManualInput}
            className="px-6 py-3 rounded-full bg-white/10 text-foreground text-sm font-medium"
          >
            Enter Manually
          </button>
        </div>
        
        {/* Instructions */}
        {!lastResult && (
          <p className="text-center text-foreground/60 text-xs mt-4">
            Position barcode within the frame
          </p>
        )}
      </div>

      <style jsx>{`
        @keyframes scan {
          0% { top: 0; }
          50% { top: calc(100% - 2px); }
          100% { top: 0; }
        }
        .animate-scan {
          animation: scan 2s ease-in-out infinite;
        }
      `}</style>
    </div>
  );
}

export default MobileScannerSheet;

