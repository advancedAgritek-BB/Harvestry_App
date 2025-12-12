/**
 * useLabelPreview Hook
 * Manages label preview slideout state and template fetching
 */

import { useState, useCallback, useEffect } from 'react';
import type { LabelTemplate, LabelEntityType } from '../services/labels.service';
import { getDefaultTemplateForEntity } from '../services/labels.service';

/** Data structure for label preview */
export interface LabelPreviewData {
  lotNumber?: string;
  productName?: string;
  strainName?: string;
  quantity?: number;
  uom?: string;
  expirationDate?: string;
  metrcTag?: string;
  locationName?: string;
  batchName?: string;
  thcPercent?: string;
  cbdPercent?: string;
}

/** Entity being previewed */
export interface PreviewEntity {
  id: string;
  type: LabelEntityType;
  data: LabelPreviewData;
}

interface UseLabelPreviewOptions {
  /** Available templates to choose from */
  templates?: LabelTemplate[];
  /** Called when print is requested */
  onPrint?: (templateId: string, entityId: string) => Promise<void>;
  /** Called when download is requested */
  onDownload?: (templateId: string, entityId: string, format: 'pdf' | 'png') => Promise<void>;
}

interface UseLabelPreviewReturn {
  /** Whether the slideout is open */
  isOpen: boolean;
  /** The entity being previewed */
  entity: PreviewEntity | null;
  /** Currently selected template */
  selectedTemplate: LabelTemplate | null;
  /** Available templates for the entity type */
  availableTemplates: LabelTemplate[];
  /** Open the preview for an entity */
  openPreview: (entity: PreviewEntity) => void;
  /** Close the preview */
  closePreview: () => void;
  /** Select a different template */
  selectTemplate: (templateId: string) => void;
  /** Print the label */
  print: () => Promise<void>;
  /** Download the label */
  download: (format: 'pdf' | 'png') => Promise<void>;
  /** Whether a print/download operation is in progress */
  isLoading: boolean;
}

// Mock templates for demo purposes
const MOCK_TEMPLATES: LabelTemplate[] = [
  {
    id: 'tpl-1',
    siteId: 'site-1',
    name: 'Colorado Product Label',
    jurisdiction: 'CO',
    labelType: 'product',
    format: 'zpl',
    widthInches: 2,
    heightInches: 1,
    barcodeFormat: 'gs1-128',
    barcodePosition: { x: 10, y: 40, width: 180, height: 30 },
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
    widthInches: 3,
    heightInches: 2,
    barcodeFormat: 'qr',
    barcodePosition: { x: 10, y: 10, width: 60, height: 60 },
    fields: [],
    requiredPhrases: [],
    jurisdictionRules: {},
    isActive: true,
    isDefault: true,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: 'tpl-3',
    siteId: 'site-1',
    name: 'Location QR Code',
    jurisdiction: 'ALL',
    labelType: 'location',
    format: 'zpl',
    widthInches: 2,
    heightInches: 2,
    barcodeFormat: 'qr',
    barcodePosition: { x: 20, y: 20, width: 160, height: 160 },
    fields: [],
    requiredPhrases: [],
    jurisdictionRules: {},
    isActive: true,
    isDefault: true,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: 'tpl-4',
    siteId: 'site-1',
    name: 'Lot Label - Standard',
    jurisdiction: 'ALL',
    labelType: 'lot',
    format: 'zpl',
    widthInches: 2,
    heightInches: 1,
    barcodeFormat: 'gs1-128',
    barcodePosition: { x: 10, y: 40, width: 180, height: 30 },
    fields: [],
    requiredPhrases: [],
    jurisdictionRules: {},
    isActive: true,
    isDefault: true,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: 'tpl-5',
    siteId: 'site-1',
    name: 'Package Label',
    jurisdiction: 'ALL',
    labelType: 'package',
    format: 'zpl',
    widthInches: 2.5,
    heightInches: 1.5,
    barcodeFormat: 'gs1-128',
    barcodePosition: { x: 10, y: 50, width: 220, height: 40 },
    fields: [],
    requiredPhrases: [],
    jurisdictionRules: {},
    isActive: true,
    isDefault: true,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
];

export function useLabelPreview(options: UseLabelPreviewOptions = {}): UseLabelPreviewReturn {
  const { templates = MOCK_TEMPLATES, onPrint, onDownload } = options;

  const [isOpen, setIsOpen] = useState(false);
  const [entity, setEntity] = useState<PreviewEntity | null>(null);
  const [selectedTemplateId, setSelectedTemplateId] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  // Get templates available for the current entity type
  const availableTemplates = entity
    ? templates.filter(t => t.labelType === entity.type && t.isActive)
    : [];

  // Get the selected template
  const selectedTemplate = selectedTemplateId
    ? templates.find(t => t.id === selectedTemplateId) ?? null
    : null;

  // Auto-select default template when entity changes
  useEffect(() => {
    if (entity && availableTemplates.length > 0) {
      const defaultTemplate = getDefaultTemplateForEntity(templates, entity.type);
      if (defaultTemplate) {
        setSelectedTemplateId(defaultTemplate.id);
      } else if (availableTemplates.length > 0) {
        setSelectedTemplateId(availableTemplates[0].id);
      }
    }
  }, [entity, availableTemplates, templates]);

  const openPreview = useCallback((newEntity: PreviewEntity) => {
    setEntity(newEntity);
    setIsOpen(true);
  }, []);

  const closePreview = useCallback(() => {
    setIsOpen(false);
    // Delay clearing entity to allow animation
    setTimeout(() => {
      setEntity(null);
      setSelectedTemplateId(null);
    }, 300);
  }, []);

  const selectTemplate = useCallback((templateId: string) => {
    setSelectedTemplateId(templateId);
  }, []);

  const print = useCallback(async () => {
    if (!entity || !selectedTemplateId || !onPrint) return;
    
    setIsLoading(true);
    try {
      await onPrint(selectedTemplateId, entity.id);
    } finally {
      setIsLoading(false);
    }
  }, [entity, selectedTemplateId, onPrint]);

  const download = useCallback(async (format: 'pdf' | 'png') => {
    if (!entity || !selectedTemplateId || !onDownload) return;
    
    setIsLoading(true);
    try {
      await onDownload(selectedTemplateId, entity.id, format);
    } finally {
      setIsLoading(false);
    }
  }, [entity, selectedTemplateId, onDownload]);

  return {
    isOpen,
    entity,
    selectedTemplate,
    availableTemplates,
    openPreview,
    closePreview,
    selectTemplate,
    print,
    download,
    isLoading,
  };
}

export default useLabelPreview;
