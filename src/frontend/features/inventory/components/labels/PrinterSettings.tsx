'use client';

import React, { useState, useEffect } from 'react';
import {
  X,
  Printer,
  Save,
  RefreshCw,
  Wifi,
  WifiOff,
  CheckCircle,
  AlertTriangle,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import type { ZebraPrinterConfig, ZebraPrinterDensity, PrinterInfo } from '../../services/labels.service';
import {
  getSavedPrinterConfig,
  savePrinterConfig,
  clearPrinterConfig,
} from '../../services/labels.service';

interface PrinterSettingsProps {
  isOpen: boolean;
  onClose: () => void;
  onSave?: (config: ZebraPrinterConfig) => void;
}

// Mock available printers for demo
const MOCK_PRINTERS: PrinterInfo[] = [
  { id: 'zebra-1', name: 'Zebra ZD420', type: 'zebra', isOnline: true },
  { id: 'zebra-2', name: 'Zebra ZT230', type: 'zebra', isOnline: true },
  { id: 'zebra-3', name: 'Zebra GK420d', type: 'zebra', isOnline: false },
  { id: 'dymo-1', name: 'DYMO LabelWriter 450', type: 'dymo', isOnline: true },
];

const DENSITY_OPTIONS: { value: ZebraPrinterDensity; label: string; dpi: number }[] = [
  { value: '6dpmm', label: '6 dpmm (152 DPI)', dpi: 152 },
  { value: '8dpmm', label: '8 dpmm (203 DPI)', dpi: 203 },
  { value: '12dpmm', label: '12 dpmm (300 DPI)', dpi: 300 },
  { value: '24dpmm', label: '24 dpmm (600 DPI)', dpi: 600 },
];

const DEFAULT_CONFIG: Omit<ZebraPrinterConfig, 'id' | 'name'> = {
  density: '8dpmm',
  darkness: 15,
  printSpeed: 4,
  isDefault: true,
};

export function PrinterSettings({
  isOpen,
  onClose,
  onSave,
}: PrinterSettingsProps) {
  const [printers] = useState<PrinterInfo[]>(MOCK_PRINTERS);
  const [selectedPrinterId, setSelectedPrinterId] = useState<string | null>(null);
  const [config, setConfig] = useState<Partial<ZebraPrinterConfig>>({});
  const [isLoading, setIsLoading] = useState(false);
  const [isSaved, setIsSaved] = useState(false);

  // Load saved config on mount
  useEffect(() => {
    const savedConfig = getSavedPrinterConfig();
    if (savedConfig) {
      setSelectedPrinterId(savedConfig.id);
      setConfig(savedConfig);
    }
  }, []);

  // Get selected printer info
  const selectedPrinter = printers.find(p => p.id === selectedPrinterId);

  const handlePrinterSelect = (printerId: string) => {
    const printer = printers.find(p => p.id === printerId);
    if (!printer) return;

    setSelectedPrinterId(printerId);
    setConfig({
      id: printer.id,
      name: printer.name,
      ...DEFAULT_CONFIG,
    });
    setIsSaved(false);
  };

  const handleSave = () => {
    if (!selectedPrinterId || !config.id) return;

    const fullConfig: ZebraPrinterConfig = {
      id: config.id!,
      name: config.name!,
      ipAddress: config.ipAddress,
      port: config.port,
      density: config.density || '8dpmm',
      darkness: config.darkness || 15,
      printSpeed: config.printSpeed || 4,
      isDefault: config.isDefault ?? true,
    };

    savePrinterConfig(fullConfig);
    onSave?.(fullConfig);
    setIsSaved(true);

    // Hide saved indicator after 2 seconds
    setTimeout(() => setIsSaved(false), 2000);
  };

  const handleTestPrint = async () => {
    setIsLoading(true);
    // Simulate test print
    await new Promise(resolve => setTimeout(resolve, 1500));
    setIsLoading(false);
  };

  if (!isOpen) return null;

  return (
    <>
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black/50 z-50 transition-opacity"
        onClick={onClose}
      />

      {/* Modal */}
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
        <div
          className="w-full max-w-lg bg-surface border border-border rounded-xl shadow-2xl"
          onClick={e => e.stopPropagation()}
        >
          {/* Header */}
          <div className="flex items-center justify-between px-6 py-4 border-b border-border">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-xl bg-cyan-500/10 flex items-center justify-center">
                <Printer className="w-5 h-5 text-cyan-400" />
              </div>
              <div>
                <h2 className="text-lg font-semibold text-foreground">Printer Settings</h2>
                <p className="text-sm text-muted-foreground">Configure Zebra label printer</p>
              </div>
            </div>
            <button
              onClick={onClose}
              className="p-2 rounded-lg hover:bg-muted/50 text-muted-foreground hover:text-foreground transition-colors"
            >
              <X className="w-5 h-5" />
            </button>
          </div>

          {/* Content */}
          <div className="p-6 space-y-6">
            {/* Printer Selection */}
            <div>
              <label className="block text-sm font-medium text-foreground mb-2">
                Select Printer
              </label>
              <div className="space-y-2">
                {printers.map((printer) => (
                  <button
                    key={printer.id}
                    onClick={() => handlePrinterSelect(printer.id)}
                    disabled={!printer.isOnline}
                    className={cn(
                      'w-full flex items-center justify-between px-4 py-3 rounded-lg border transition-colors',
                      selectedPrinterId === printer.id
                        ? 'border-cyan-500/50 bg-cyan-500/10'
                        : 'border-border hover:bg-muted/30',
                      !printer.isOnline && 'opacity-50 cursor-not-allowed'
                    )}
                  >
                    <div className="flex items-center gap-3">
                      <Printer className="w-5 h-5 text-muted-foreground" />
                      <div className="text-left">
                        <div className="text-sm font-medium text-foreground">{printer.name}</div>
                        <div className="text-xs text-muted-foreground capitalize">{printer.type}</div>
                      </div>
                    </div>
                    <div className="flex items-center gap-2">
                      {printer.isOnline ? (
                        <span className="flex items-center gap-1 text-xs text-emerald-400">
                          <Wifi className="w-3 h-3" />
                          Online
                        </span>
                      ) : (
                        <span className="flex items-center gap-1 text-xs text-rose-400">
                          <WifiOff className="w-3 h-3" />
                          Offline
                        </span>
                      )}
                      {selectedPrinterId === printer.id && (
                        <CheckCircle className="w-4 h-4 text-cyan-400" />
                      )}
                    </div>
                  </button>
                ))}
              </div>
            </div>

            {/* Zebra Settings (only show when Zebra printer selected) */}
            {selectedPrinter?.type === 'zebra' && (
              <>
                {/* Network Settings */}
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-foreground mb-2">
                      IP Address (optional)
                    </label>
                    <input
                      type="text"
                      value={config.ipAddress || ''}
                      onChange={(e) => setConfig({ ...config, ipAddress: e.target.value })}
                      placeholder="192.168.1.100"
                      className="w-full px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground/50 focus:outline-none focus:border-cyan-500/50"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-foreground mb-2">
                      Port
                    </label>
                    <input
                      type="number"
                      value={config.port || 9100}
                      onChange={(e) => setConfig({ ...config, port: parseInt(e.target.value) })}
                      className="w-full px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground focus:outline-none focus:border-cyan-500/50"
                    />
                  </div>
                </div>

                {/* Print Density */}
                <div>
                  <label className="block text-sm font-medium text-foreground mb-2">
                    Print Density
                  </label>
                  <select
                    value={config.density || '8dpmm'}
                    onChange={(e) => setConfig({ ...config, density: e.target.value as ZebraPrinterDensity })}
                    className="w-full px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground focus:outline-none focus:border-cyan-500/50"
                  >
                    {DENSITY_OPTIONS.map((opt) => (
                      <option key={opt.value} value={opt.value}>
                        {opt.label}
                      </option>
                    ))}
                  </select>
                </div>

                {/* Darkness */}
                <div>
                  <label className="block text-sm font-medium text-foreground mb-2">
                    Print Darkness: {config.darkness || 15}
                  </label>
                  <input
                    type="range"
                    min={0}
                    max={30}
                    value={config.darkness || 15}
                    onChange={(e) => setConfig({ ...config, darkness: parseInt(e.target.value) })}
                    className="w-full accent-cyan-500"
                  />
                  <div className="flex justify-between text-xs text-muted-foreground mt-1">
                    <span>Lighter</span>
                    <span>Darker</span>
                  </div>
                </div>

                {/* Print Speed */}
                <div>
                  <label className="block text-sm font-medium text-foreground mb-2">
                    Print Speed: {config.printSpeed || 4} in/sec
                  </label>
                  <input
                    type="range"
                    min={1}
                    max={8}
                    value={config.printSpeed || 4}
                    onChange={(e) => setConfig({ ...config, printSpeed: parseInt(e.target.value) })}
                    className="w-full accent-cyan-500"
                  />
                  <div className="flex justify-between text-xs text-muted-foreground mt-1">
                    <span>Slower (Higher Quality)</span>
                    <span>Faster</span>
                  </div>
                </div>
              </>
            )}

            {/* Non-Zebra Warning */}
            {selectedPrinter && selectedPrinter.type !== 'zebra' && (
              <div className="flex items-start gap-3 px-4 py-3 bg-amber-500/10 border border-amber-500/20 rounded-lg">
                <AlertTriangle className="w-5 h-5 text-amber-400 flex-shrink-0 mt-0.5" />
                <div className="text-sm">
                  <p className="font-medium text-amber-400">Limited Settings</p>
                  <p className="text-muted-foreground mt-1">
                    Advanced settings are only available for Zebra printers. This printer will use default settings.
                  </p>
                </div>
              </div>
            )}
          </div>

          {/* Footer */}
          <div className="flex items-center justify-between px-6 py-4 border-t border-border">
            <button
              onClick={handleTestPrint}
              disabled={!selectedPrinterId || isLoading}
              className="flex items-center gap-2 px-4 py-2 rounded-lg bg-muted/30 hover:bg-muted/50 text-sm text-foreground transition-colors disabled:opacity-50"
            >
              {isLoading ? (
                <RefreshCw className="w-4 h-4 animate-spin" />
              ) : (
                <Printer className="w-4 h-4" />
              )}
              Test Print
            </button>

            <div className="flex items-center gap-3">
              <button
                onClick={onClose}
                className="px-4 py-2 rounded-lg text-sm text-muted-foreground hover:text-foreground transition-colors"
              >
                Cancel
              </button>
              <button
                onClick={handleSave}
                disabled={!selectedPrinterId}
                className={cn(
                  'flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition-colors disabled:opacity-50',
                  isSaved
                    ? 'bg-emerald-500 text-black'
                    : 'bg-cyan-500 hover:bg-cyan-400 text-black'
                )}
              >
                {isSaved ? (
                  <>
                    <CheckCircle className="w-4 h-4" />
                    Saved
                  </>
                ) : (
                  <>
                    <Save className="w-4 h-4" />
                    Save Settings
                  </>
                )}
              </button>
            </div>
          </div>
        </div>
      </div>
    </>
  );
}

export default PrinterSettings;
