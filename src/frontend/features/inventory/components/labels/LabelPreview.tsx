'use client';

import React from 'react';
import { 
  ZoomIn,
  ZoomOut,
  Printer,
  Download,
  RefreshCw,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import type { LabelTemplate } from '../../services/labels.service';

interface LabelPreviewProps {
  template: LabelTemplate | null;
  lotData?: {
    lotNumber: string;
    productName: string;
    strainName?: string;
    quantity: number;
    uom: string;
    expirationDate?: string;
    metrcTag?: string;
  };
  isLoading?: boolean;
  onPrint?: () => void;
  onDownload?: () => void;
}

export function LabelPreview({
  template,
  lotData,
  isLoading,
  onPrint,
  onDownload,
}: LabelPreviewProps) {
  const [zoom, setZoom] = React.useState(100);
  
  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64 rounded-xl bg-muted/30 border border-border">
        <RefreshCw className="w-8 h-8 text-cyan-400 animate-spin" />
      </div>
    );
  }
  
  if (!template) {
    return (
      <div className="flex flex-col items-center justify-center h-64 rounded-xl bg-muted/30 border border-border">
        <div className="text-muted-foreground text-sm">Select a template to preview</div>
      </div>
    );
  }
  
  // Simulated label dimensions
  const labelWidth = template.widthInches * 96; // 96 DPI
  const labelHeight = template.heightInches * 96;
  
  return (
    <div className="space-y-4">
      {/* Toolbar */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <button
            onClick={() => setZoom(Math.max(50, zoom - 25))}
            disabled={zoom <= 50}
            className="p-2 rounded-lg bg-white/5 hover:bg-white/10 text-muted-foreground hover:text-foreground transition-colors disabled:opacity-50"
          >
            <ZoomOut className="w-4 h-4" />
          </button>
          <span className="text-sm text-muted-foreground w-12 text-center">{zoom}%</span>
          <button
            onClick={() => setZoom(Math.min(200, zoom + 25))}
            disabled={zoom >= 200}
            className="p-2 rounded-lg bg-white/5 hover:bg-white/10 text-muted-foreground hover:text-foreground transition-colors disabled:opacity-50"
          >
            <ZoomIn className="w-4 h-4" />
          </button>
        </div>
        
        <div className="flex items-center gap-2">
          <button
            onClick={onDownload}
            className="flex items-center gap-2 px-3 py-2 rounded-lg bg-white/5 hover:bg-white/10 text-foreground text-sm transition-colors"
          >
            <Download className="w-4 h-4" />
            Download PDF
          </button>
          <button
            onClick={onPrint}
            className="flex items-center gap-2 px-3 py-2 rounded-lg bg-cyan-500 hover:bg-cyan-400 text-black text-sm font-medium transition-colors"
          >
            <Printer className="w-4 h-4" />
            Print
          </button>
        </div>
      </div>
      
      {/* Preview Area */}
      <div className="overflow-auto p-8 rounded-xl bg-muted/30 border border-border min-h-[400px] flex items-center justify-center">
        <div 
          style={{ 
            transform: `scale(${zoom / 100})`,
            transformOrigin: 'center',
          }}
          className="transition-transform"
        >
          {/* Label */}
          <div 
            style={{
              width: labelWidth,
              height: labelHeight,
            }}
            className="bg-white rounded shadow-lg p-4 text-black"
          >
            {/* Simulated label content */}
            <div className="h-full flex flex-col">
              {/* Header */}
              <div className="text-center mb-2">
                <div className="text-[10px] uppercase tracking-wide text-gray-500">
                  {template.jurisdiction}
                </div>
              </div>
              
              {/* Product Info */}
              <div className="flex-1">
                <div className="text-sm font-bold text-gray-900 mb-1">
                  {lotData?.productName || 'Product Name'}
                </div>
                {lotData?.strainName && (
                  <div className="text-xs text-gray-600 mb-2">
                    {lotData.strainName}
                  </div>
                )}
                
                {/* Barcode placeholder */}
                <div className="my-3 h-12 bg-black flex items-center justify-center">
                  <div className="flex items-end gap-px h-10">
                    {[...Array(40)].map((_, i) => (
                      <div 
                        key={i}
                        className="bg-white"
                        style={{ 
                          width: i % 3 === 0 ? 2 : 1,
                          height: `${60 + Math.random() * 40}%`
                        }}
                      />
                    ))}
                  </div>
                </div>
                
                <div className="text-[8px] text-center font-mono text-gray-700 mb-2">
                  {lotData?.lotNumber || 'LOT-2025-000000'}
                </div>
              </div>
              
              {/* Footer */}
              <div className="grid grid-cols-2 gap-2 text-[8px] text-gray-600 border-t border-gray-200 pt-2">
                <div>
                  <span className="font-medium">Net Wt:</span> {lotData?.quantity || 0} {lotData?.uom || 'g'}
                </div>
                {lotData?.expirationDate && (
                  <div>
                    <span className="font-medium">Exp:</span> {new Date(lotData.expirationDate).toLocaleDateString()}
                  </div>
                )}
                {lotData?.metrcTag && (
                  <div className="col-span-2">
                    <span className="font-medium">METRC:</span> {lotData.metrcTag}
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>
      
      {/* Template Info */}
      <div className="flex items-center justify-between text-xs text-muted-foreground">
        <span>{template.name} • {template.widthInches}" × {template.heightInches}"</span>
        <span>{template.format.toUpperCase()}</span>
      </div>
    </div>
  );
}

export default LabelPreview;

