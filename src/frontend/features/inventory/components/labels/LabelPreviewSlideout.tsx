'use client';

import React, { useState, useEffect } from 'react';
import {
  X,
  Printer,
  Download,
  ZoomIn,
  ZoomOut,
  ChevronDown,
  Settings,
  RefreshCw,
} from 'lucide-react';
import { QRCodeSVG } from 'qrcode.react';
import { cn } from '@/lib/utils';
import type { LabelTemplate } from '../../services/labels.service';
import type { LabelPreviewData } from '../../hooks/useLabelPreview';
import { generateBarcodeImage } from '../../services/labels.service';

interface LabelPreviewSlideoutProps {
  isOpen: boolean;
  onClose: () => void;
  template: LabelTemplate | null;
  availableTemplates: LabelTemplate[];
  onTemplateChange: (templateId: string) => void;
  entityData: LabelPreviewData | null;
  entityType: string;
  onPrint?: () => Promise<void>;
  onDownload?: (format: 'pdf' | 'png') => Promise<void>;
  onOpenSettings?: () => void;
  isLoading?: boolean;
}

export function LabelPreviewSlideout({
  isOpen,
  onClose,
  template,
  availableTemplates,
  onTemplateChange,
  entityData,
  entityType,
  onPrint,
  onDownload,
  onOpenSettings,
  isLoading = false,
}: LabelPreviewSlideoutProps) {
  const [zoom, setZoom] = useState(100);
  const [barcodeDataUrl, setBarcodeDataUrl] = useState<string | null>(null);
  const [isGeneratingBarcode, setIsGeneratingBarcode] = useState(false);
  const [showTemplateDropdown, setShowTemplateDropdown] = useState(false);

  // Generate barcode when template or data changes
  useEffect(() => {
    if (!template || !entityData) {
      setBarcodeDataUrl(null);
      return;
    }

    const generateBarcode = async () => {
      // Skip for QR codes - we use QRCodeSVG component
      if (template.barcodeFormat === 'qr') {
        setBarcodeDataUrl(null);
        return;
      }

      setIsGeneratingBarcode(true);
      try {
        const barcodeData = entityData.lotNumber || entityData.metrcTag || 'DEMO-BARCODE';
        const dataUrl = await generateBarcodeImage(
          template.barcodeFormat,
          barcodeData,
          {
            width: template.barcodePosition.width * 2,
            height: template.barcodePosition.height * 2,
            includeText: true,
          }
        );
        setBarcodeDataUrl(dataUrl);
      } catch (error) {
        console.error('Failed to generate barcode:', error);
        setBarcodeDataUrl(null);
      } finally {
        setIsGeneratingBarcode(false);
      }
    };

    generateBarcode();
  }, [template, entityData]);

  // Calculate label dimensions in pixels (96 DPI)
  const labelWidth = template ? template.widthInches * 96 : 192;
  const labelHeight = template ? template.heightInches * 96 : 96;

  // Get barcode data for QR code
  const qrData = entityData?.lotNumber || entityData?.metrcTag || 'DEMO-QR';

  if (!isOpen) return null;

  return (
    <>
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black/50 z-40 transition-opacity"
        onClick={onClose}
      />

      {/* Slideout Panel */}
      <div
        className={cn(
          'fixed right-0 top-0 h-full w-[480px] bg-surface border-l border-border z-50',
          'transform transition-transform duration-300 ease-out',
          isOpen ? 'translate-x-0' : 'translate-x-full'
        )}
      >
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-border">
          <div>
            <h2 className="text-lg font-semibold text-foreground">Label Preview</h2>
            <p className="text-sm text-muted-foreground capitalize">{entityType} Label</p>
          </div>
          <button
            onClick={onClose}
            title="Close preview"
            className="p-2 rounded-lg hover:bg-muted/50 text-muted-foreground hover:text-foreground transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Content */}
        <div className="flex flex-col h-[calc(100%-73px)]">
          {/* Template Selector */}
          <div className="px-6 py-4 border-b border-border">
            <label className="block text-xs font-medium text-muted-foreground uppercase tracking-wide mb-2">
              Template
            </label>
            <div className="relative">
              <button
                onClick={() => setShowTemplateDropdown(!showTemplateDropdown)}
                className="w-full flex items-center justify-between px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground hover:bg-muted/50 transition-colors"
              >
                <span>{template?.name || 'Select template...'}</span>
                <ChevronDown className="w-4 h-4 text-muted-foreground" />
              </button>

              {showTemplateDropdown && (
                <div className="absolute top-full left-0 right-0 mt-1 bg-elevated border border-border rounded-lg shadow-lg z-10 max-h-48 overflow-y-auto">
                  {availableTemplates.length === 0 ? (
                    <div className="px-3 py-2 text-sm text-muted-foreground">
                      No templates available for this type
                    </div>
                  ) : (
                    availableTemplates.map((t) => (
                      <button
                        key={t.id}
                        onClick={() => {
                          onTemplateChange(t.id);
                          setShowTemplateDropdown(false);
                        }}
                        className={cn(
                          'w-full text-left px-3 py-2 text-sm hover:bg-muted/50 transition-colors',
                          t.id === template?.id
                            ? 'bg-cyan-500/10 text-cyan-400'
                            : 'text-foreground'
                        )}
                      >
                        <div className="font-medium">{t.name}</div>
                        <div className="text-xs text-muted-foreground">
                          {t.widthInches}" × {t.heightInches}" • {t.barcodeFormat.toUpperCase()}
                        </div>
                      </button>
                    ))
                  )}
                </div>
              )}
            </div>
          </div>

          {/* Zoom Controls */}
          <div className="px-6 py-3 border-b border-border flex items-center justify-between">
            <div className="flex items-center gap-2">
              <button
                onClick={() => setZoom(Math.max(50, zoom - 25))}
                disabled={zoom <= 50}
                title="Zoom out"
                className="p-2 rounded-lg bg-muted/30 hover:bg-muted/50 text-muted-foreground hover:text-foreground transition-colors disabled:opacity-50"
              >
                <ZoomOut className="w-4 h-4" />
              </button>
              <span className="text-sm text-muted-foreground w-12 text-center">{zoom}%</span>
              <button
                onClick={() => setZoom(Math.min(200, zoom + 25))}
                disabled={zoom >= 200}
                title="Zoom in"
                className="p-2 rounded-lg bg-muted/30 hover:bg-muted/50 text-muted-foreground hover:text-foreground transition-colors disabled:opacity-50"
              >
                <ZoomIn className="w-4 h-4" />
              </button>
            </div>
            {onOpenSettings && (
              <button
                onClick={onOpenSettings}
                className="flex items-center gap-2 px-3 py-2 rounded-lg bg-muted/30 hover:bg-muted/50 text-sm text-muted-foreground hover:text-foreground transition-colors"
              >
                <Settings className="w-4 h-4" />
                Printer Settings
              </button>
            )}
          </div>

          {/* Preview Area */}
          <div className="flex-1 overflow-auto p-6 flex items-center justify-center bg-muted/20">
            {isLoading || isGeneratingBarcode ? (
              <div className="flex flex-col items-center gap-3">
                <RefreshCw className="w-8 h-8 text-cyan-400 animate-spin" />
                <span className="text-sm text-muted-foreground">Generating preview...</span>
              </div>
            ) : !template ? (
              <div className="text-center text-muted-foreground">
                <p>Select a template to preview</p>
              </div>
            ) : (
              <div
                style={{
                  transform: `scale(${zoom / 100})`,
                  transformOrigin: 'center',
                }}
                className="transition-transform"
              >
                {/* Rendered Label */}
                <div
                  style={{
                    width: labelWidth,
                    height: labelHeight,
                  }}
                  className="bg-white rounded shadow-lg p-4 text-black"
                >
                  <div className="h-full flex flex-col">
                    {/* Header */}
                    <div className="text-center mb-1">
                      <div className="text-[8px] uppercase tracking-wide text-gray-500">
                        {template.jurisdiction}
                      </div>
                    </div>

                    {/* Product Info */}
                    <div className="flex-1">
                      <div className="text-xs font-bold text-gray-900 mb-0.5 truncate">
                        {entityData?.productName || 'Product Name'}
                      </div>
                      {entityData?.strainName && (
                        <div className="text-[10px] text-gray-600 mb-1 truncate">
                          {entityData.strainName}
                        </div>
                      )}

                      {/* Barcode/QR Code */}
                      <div className="my-2 flex justify-center">
                        {template.barcodeFormat === 'qr' ? (
                          <QRCodeSVG
                            value={qrData}
                            size={Math.min(labelWidth - 32, labelHeight - 60)}
                            level="M"
                            includeMargin={false}
                          />
                        ) : barcodeDataUrl ? (
                          <img
                            src={barcodeDataUrl}
                            alt="Barcode"
                            className="max-w-full h-auto"
                            style={{ maxHeight: labelHeight * 0.35 }}
                          />
                        ) : (
                          // Placeholder barcode
                          <div className="h-10 bg-black flex items-center justify-center px-2">
                            <div className="flex items-end gap-px h-8">
                              {[...Array(30)].map((_, i) => (
                                <div
                                  key={i}
                                  className="bg-white"
                                  style={{
                                    width: i % 3 === 0 ? 2 : 1,
                                    height: `${60 + Math.random() * 40}%`,
                                  }}
                                />
                              ))}
                            </div>
                          </div>
                        )}
                      </div>

                      <div className="text-[7px] text-center font-mono text-gray-700">
                        {entityData?.lotNumber || 'LOT-2025-000000'}
                      </div>
                    </div>

                    {/* Footer */}
                    <div className="grid grid-cols-2 gap-1 text-[7px] text-gray-600 border-t border-gray-200 pt-1 mt-1">
                      <div>
                        <span className="font-medium">Net Wt:</span>{' '}
                        {entityData?.quantity ?? 0} {entityData?.uom || 'g'}
                      </div>
                      {entityData?.expirationDate && (
                        <div>
                          <span className="font-medium">Exp:</span>{' '}
                          {new Date(entityData.expirationDate).toLocaleDateString()}
                        </div>
                      )}
                      {entityData?.metrcTag && (
                        <div className="col-span-2 truncate">
                          <span className="font-medium">METRC:</span> {entityData.metrcTag}
                        </div>
                      )}
                    </div>
                  </div>
                </div>
              </div>
            )}
          </div>

          {/* Template Info */}
          {template && (
            <div className="px-6 py-2 border-t border-border">
              <div className="flex items-center justify-between text-xs text-muted-foreground">
                <span>
                  {template.widthInches}" × {template.heightInches}"
                </span>
                <span>{template.format.toUpperCase()}</span>
              </div>
            </div>
          )}

          {/* Actions */}
          <div className="px-6 py-4 border-t border-border flex items-center gap-3">
            <button
              onClick={() => onDownload?.('pdf')}
              disabled={!template || isLoading}
              className="flex-1 flex items-center justify-center gap-2 px-4 py-2.5 rounded-lg bg-muted/30 hover:bg-muted/50 text-foreground text-sm font-medium transition-colors disabled:opacity-50"
            >
              <Download className="w-4 h-4" />
              Download PDF
            </button>
            <button
              onClick={onPrint}
              disabled={!template || isLoading}
              className="flex-1 flex items-center justify-center gap-2 px-4 py-2.5 rounded-lg bg-cyan-500 hover:bg-cyan-400 text-black text-sm font-medium transition-colors disabled:opacity-50"
            >
              <Printer className="w-4 h-4" />
              Print
            </button>
          </div>
        </div>
      </div>
    </>
  );
}

export default LabelPreviewSlideout;
