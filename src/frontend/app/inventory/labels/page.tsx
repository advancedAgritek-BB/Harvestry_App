'use client';

import React, { useState } from 'react';
import {
  Tag,
  ChevronLeft,
  Plus,
  Printer,
  Edit2,
  Trash2,
  QrCode,
  Search,
  Filter,
  ChevronDown,
  Eye,
  CheckCircle,
  Settings,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { LabelPreviewSlideout, PrinterSettings } from '@/features/inventory/components/labels';
import type { LabelTemplate } from '@/features/inventory/services/labels.service';

// Full template data for preview
const MOCK_TEMPLATES: LabelTemplate[] = [
  {
    id: 'tpl-1',
    siteId: 'site-1',
    name: 'Colorado Product Label',
    jurisdiction: 'CO',
    labelType: 'product',
    format: 'zpl',
    barcodeFormat: 'gs1-128',
    barcodePosition: { x: 10, y: 40, width: 180, height: 30 },
    widthInches: 2,
    heightInches: 1,
    fields: [],
    requiredPhrases: [],
    jurisdictionRules: {},
    isActive: true,
    isDefault: true,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: 'tpl-2',
    siteId: 'site-1',
    name: 'California Batch Label',
    jurisdiction: 'CA',
    labelType: 'batch',
    format: 'zpl',
    barcodeFormat: 'qr',
    barcodePosition: { x: 10, y: 10, width: 60, height: 60 },
    widthInches: 3,
    heightInches: 2,
    fields: [],
    requiredPhrases: [],
    jurisdictionRules: {},
    isActive: true,
    isDefault: false,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: 'tpl-3',
    siteId: 'site-1',
    name: 'Manifest Label',
    jurisdiction: 'ALL',
    labelType: 'manifest',
    format: 'zpl',
    barcodeFormat: 'gs1-128',
    barcodePosition: { x: 10, y: 50, width: 280, height: 40 },
    widthInches: 4,
    heightInches: 2,
    fields: [],
    requiredPhrases: [],
    jurisdictionRules: {},
    isActive: true,
    isDefault: false,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: 'tpl-4',
    siteId: 'site-1',
    name: 'Location QR Code',
    jurisdiction: 'ALL',
    labelType: 'location',
    format: 'zpl',
    barcodeFormat: 'qr',
    barcodePosition: { x: 20, y: 20, width: 160, height: 160 },
    widthInches: 2,
    heightInches: 2,
    fields: [],
    requiredPhrases: [],
    jurisdictionRules: {},
    isActive: true,
    isDefault: false,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
];

const MOCK_PRINT_QUEUE = [
  { id: 'job-1', templateName: 'Colorado Product Label', lotNumber: 'LOT-2025-0001', quantity: 10, status: 'completed', createdAt: new Date(Date.now() - 3600000).toISOString() },
  { id: 'job-2', templateName: 'California Batch Label', lotNumber: 'LOT-2025-0005', quantity: 25, status: 'printing', createdAt: new Date(Date.now() - 1800000).toISOString() },
  { id: 'job-3', templateName: 'Colorado Product Label', lotNumber: 'LOT-2025-0012', quantity: 5, status: 'queued', createdAt: new Date(Date.now() - 600000).toISOString() },
];

// Sample data for template preview
const SAMPLE_LABEL_DATA = {
  lotNumber: 'LOT-2025-001234',
  productName: 'Blue Dream Flower',
  strainName: 'Blue Dream',
  quantity: 3.5,
  uom: 'g',
  metrcTag: '1A40500000001234567',
};

type Tab = 'templates' | 'print-queue';

export default function LabelsPage() {
  const [activeTab, setActiveTab] = useState<Tab>('templates');
  const [searchQuery, setSearchQuery] = useState('');
  
  // Preview slideout state
  const [isPreviewOpen, setIsPreviewOpen] = useState(false);
  const [previewTemplate, setPreviewTemplate] = useState<LabelTemplate | null>(null);
  
  // Printer settings state
  const [isPrinterSettingsOpen, setIsPrinterSettingsOpen] = useState(false);

  const filteredTemplates = MOCK_TEMPLATES.filter((t) =>
    t.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    t.jurisdiction.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const formatTime = (dateString: string) => {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    
    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  };

  const handlePreviewTemplate = (template: LabelTemplate) => {
    setPreviewTemplate(template);
    setIsPreviewOpen(true);
  };

  const handleTemplateChange = (templateId: string) => {
    const template = MOCK_TEMPLATES.find(t => t.id === templateId);
    if (template) {
      setPreviewTemplate(template);
    }
  };

  const handlePrint = async () => {
    // TODO: Implement actual printing
    console.log('Printing template:', previewTemplate?.id);
  };

  const handleDownload = async (format: 'pdf' | 'png') => {
    // TODO: Implement actual download
    console.log('Downloading as:', format);
  };

  const tabs = [
    { id: 'templates', label: 'Templates', count: MOCK_TEMPLATES.length },
    { id: 'print-queue', label: 'Print Queue', count: MOCK_PRINT_QUEUE.filter(j => j.status !== 'completed').length },
  ];

  return (
    <div className="min-h-screen bg-background">
      {/* Header */}
      <header className="sticky top-0 z-40 glass-header">
        <div className="px-6 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <a href="/inventory" className="p-2 rounded-lg hover:bg-white/5 text-muted-foreground hover:text-foreground transition-colors">
                <ChevronLeft className="w-5 h-5" />
              </a>
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-xl bg-indigo-500/10 flex items-center justify-center">
                  <Tag className="w-5 h-5 text-indigo-400" />
                </div>
                <div>
                  <h1 className="text-xl font-semibold text-foreground">Labels</h1>
                  <p className="text-sm text-muted-foreground">GS1 label templates and printing</p>
                </div>
              </div>
            </div>

            <div className="flex items-center gap-3">
              <button 
                onClick={() => setIsPrinterSettingsOpen(true)}
                className="flex items-center gap-2 px-3 py-2 rounded-lg bg-white/5 text-foreground hover:bg-white/10 transition-colors"
              >
                <Settings className="w-4 h-4" />
                <span className="text-sm">Printer Settings</span>
              </button>
              <button className="flex items-center gap-2 px-3 py-2 rounded-lg bg-white/5 text-foreground hover:bg-white/10 transition-colors">
                <Printer className="w-4 h-4" />
                <span className="text-sm">Quick Print</span>
              </button>
              <button className="flex items-center gap-2 px-4 py-2 rounded-lg bg-indigo-500/10 text-indigo-400 hover:bg-indigo-500/20 transition-colors">
                <Plus className="w-4 h-4" />
                <span className="text-sm font-medium">New Template</span>
              </button>
            </div>
          </div>
        </div>
      </header>

      {/* Tabs */}
      <div className="px-6 border-b border-border">
        <div className="flex gap-1">
          {tabs.map((tab) => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id as Tab)}
              className={cn(
                'flex items-center gap-2 px-4 py-3 text-sm font-medium border-b-2 transition-colors',
                activeTab === tab.id
                  ? 'text-indigo-400 border-indigo-400'
                  : 'text-muted-foreground border-transparent hover:text-foreground'
              )}
            >
              {tab.label}
              {tab.count > 0 && (
                <span className={cn(
                  'px-1.5 py-0.5 rounded text-xs',
                  activeTab === tab.id ? 'bg-indigo-500/20' : 'bg-white/5'
                )}>
                  {tab.count}
                </span>
              )}
            </button>
          ))}
        </div>
      </div>

      {/* Content */}
      <div className="px-6 py-6">
        {activeTab === 'templates' && (
          <div className="space-y-4">
            {/* Search */}
            <div className="flex items-center gap-4">
              <div className="relative flex-1 max-w-md">
                <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
                <input
                  type="text"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  placeholder="Search templates..."
                  className="w-full pl-10 pr-4 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-indigo-500/30"
                />
              </div>
              <button className="flex items-center gap-2 px-3 py-2 rounded-lg bg-muted/30 border border-border text-sm text-muted-foreground hover:text-foreground">
                <Filter className="w-4 h-4" />
                Filter
                <ChevronDown className="w-3 h-3" />
              </button>
            </div>

            {/* Templates Grid */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {filteredTemplates.map((template) => (
                <div
                  key={template.id}
                  className="bg-surface border border-border rounded-xl overflow-hidden transition-all hover:border-indigo-500/30"
                >
                  {/* Preview Area */}
                  <div className="aspect-video bg-muted/30 flex items-center justify-center border-b border-border">
                    <div 
                      className="bg-white rounded shadow-lg p-2 flex flex-col items-center justify-center"
                      style={{
                        width: `${template.widthInches * 40}px`,
                        height: `${template.heightInches * 40}px`,
                      }}
                    >
                      {template.barcodeFormat === 'qr' ? (
                        <QrCode className="w-6 h-6 text-gray-800" />
                      ) : (
                        <div className="flex flex-col items-center">
                          <div className="flex gap-px">
                            {[...Array(20)].map((_, i) => (
                              <div
                                key={i}
                                className="bg-gray-800"
                                style={{
                                  width: Math.random() > 0.5 ? '2px' : '1px',
                                  height: '16px',
                                }}
                              />
                            ))}
                          </div>
                          <span className="text-[6px] text-gray-600 mt-0.5 font-mono">*LOT-2025-0001*</span>
                        </div>
                      )}
                    </div>
                  </div>

                  {/* Info */}
                  <div className="p-4">
                    <div className="flex items-start justify-between mb-2">
                      <div>
                        <h3 className="text-sm font-medium text-foreground">{template.name}</h3>
                        <p className="text-xs text-muted-foreground capitalize">{template.labelType} label</p>
                      </div>
                      {template.isDefault && (
                        <span className="px-2 py-0.5 rounded text-[10px] font-medium bg-indigo-500/10 text-indigo-400">
                          DEFAULT
                        </span>
                      )}
                    </div>

                    <div className="flex items-center gap-3 text-xs text-muted-foreground mb-3">
                      <span>{template.jurisdiction}</span>
                      <span>•</span>
                      <span>{template.widthInches}" × {template.heightInches}"</span>
                      <span>•</span>
                      <span className="uppercase">{template.barcodeFormat}</span>
                    </div>

                    <div className="flex items-center gap-2">
                      <button 
                        onClick={() => handlePreviewTemplate(template)}
                        className="flex-1 flex items-center justify-center gap-1.5 py-2 rounded-lg bg-white/5 text-sm text-foreground hover:bg-white/10 transition-colors"
                      >
                        <Eye className="w-3.5 h-3.5" />
                        Preview
                      </button>
                      <button className="flex-1 flex items-center justify-center gap-1.5 py-2 rounded-lg bg-indigo-500/10 text-sm text-indigo-400 hover:bg-indigo-500/20 transition-colors">
                        <Printer className="w-3.5 h-3.5" />
                        Print
                      </button>
                      <button className="p-2 rounded-lg hover:bg-white/5 text-muted-foreground hover:text-foreground transition-colors">
                        <Edit2 className="w-4 h-4" />
                      </button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {activeTab === 'print-queue' && (
          <div className="bg-surface border border-border rounded-xl overflow-hidden">
            <table className="w-full">
              <thead>
                <tr className="border-b border-border bg-muted/30">
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">Template</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">Lot</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">Quantity</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">Status</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">Time</th>
                  <th className="px-4 py-3 w-12"></th>
                </tr>
              </thead>
              <tbody>
                {MOCK_PRINT_QUEUE.map((job) => (
                  <tr key={job.id} className="border-b border-border hover:bg-muted/30">
                    <td className="px-4 py-3">
                      <span className="text-sm text-foreground">{job.templateName}</span>
                    </td>
                    <td className="px-4 py-3">
                      <span className="text-sm font-mono text-muted-foreground">{job.lotNumber}</span>
                    </td>
                    <td className="px-4 py-3">
                      <span className="text-sm text-foreground tabular-nums">{job.quantity}</span>
                    </td>
                    <td className="px-4 py-3">
                      <span className={cn(
                        'inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium',
                        job.status === 'completed' && 'bg-emerald-500/10 text-emerald-400',
                        job.status === 'printing' && 'bg-blue-500/10 text-blue-400',
                        job.status === 'queued' && 'bg-amber-500/10 text-amber-400',
                        job.status === 'failed' && 'bg-rose-500/10 text-rose-400'
                      )}>
                        {job.status === 'completed' && <CheckCircle className="w-3 h-3" />}
                        {job.status === 'printing' && <Printer className="w-3 h-3 animate-pulse" />}
                        {job.status}
                      </span>
                    </td>
                    <td className="px-4 py-3">
                      <span className="text-sm text-muted-foreground">{formatTime(job.createdAt)}</span>
                    </td>
                    <td className="px-4 py-3">
                      {job.status === 'queued' && (
                        <button className="p-1.5 rounded hover:bg-white/5 text-muted-foreground hover:text-rose-400">
                          <Trash2 className="w-4 h-4" />
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>

            {MOCK_PRINT_QUEUE.length === 0 && (
              <div className="p-12 text-center">
                <Printer className="w-12 h-12 text-muted-foreground mx-auto mb-3" />
                <p className="text-sm text-muted-foreground">No print jobs in queue</p>
              </div>
            )}
          </div>
        )}
      </div>

      {/* Label Preview Slideout */}
      <LabelPreviewSlideout
        isOpen={isPreviewOpen}
        onClose={() => setIsPreviewOpen(false)}
        template={previewTemplate}
        availableTemplates={MOCK_TEMPLATES}
        onTemplateChange={handleTemplateChange}
        entityData={SAMPLE_LABEL_DATA}
        entityType={previewTemplate?.labelType || 'product'}
        onPrint={handlePrint}
        onDownload={handleDownload}
        onOpenSettings={() => {
          setIsPreviewOpen(false);
          setIsPrinterSettingsOpen(true);
        }}
      />

      {/* Printer Settings Modal */}
      <PrinterSettings
        isOpen={isPrinterSettingsOpen}
        onClose={() => setIsPrinterSettingsOpen(false)}
      />
    </div>
  );
}
