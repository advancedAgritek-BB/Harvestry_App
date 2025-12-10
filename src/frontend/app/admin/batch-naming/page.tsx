'use client';

import React, { useState, useEffect, useMemo } from 'react';
import {
  Tag,
  Hash,
  FileText,
  RefreshCw,
  Info,
  ChevronDown,
  ChevronUp,
  MapPin,
  Copy,
  Check,
  Plus,
} from 'lucide-react';
import {
  AdminCard,
  AdminSection,
  AdminGrid,
  Button,
  FormField,
  Input,
  Select,
} from '@/components/admin';
import {
  BatchNamingConfig,
  NamingMode,
  ResetFrequency,
  TEMPLATE_TOKENS,
  STATE_COMPLIANCE_HINTS,
  DEFAULT_TEMPLATE,
  DEFAULT_PREFIX,
  DEFAULT_DIGIT_COUNT,
  TokenInfo,
} from '@/features/planner/types/batchNaming.types';
import { BatchNamingService } from '@/features/planner/services/batchNaming.service';

// =============================================================================
// TOKEN PALETTE COMPONENT
// =============================================================================

interface TokenPaletteProps {
  onInsert: (token: string) => void;
}

function TokenPalette({ onInsert }: TokenPaletteProps) {
  const [expandedCategory, setExpandedCategory] = useState<string | null>('strain');

  const categories = useMemo(() => {
    const grouped: Record<string, TokenInfo[]> = {};
    TEMPLATE_TOKENS.forEach((token) => {
      if (!grouped[token.category]) {
        grouped[token.category] = [];
      }
      grouped[token.category].push(token);
    });
    return grouped;
  }, []);

  const categoryLabels: Record<string, string> = {
    strain: 'Strain/Genetics',
    date: 'Date & Time',
    sequence: 'Sequence Numbers',
    location: 'Location',
    other: 'Other',
  };

  return (
    <div className="border border-border rounded-lg overflow-hidden">
      <div className="px-3 py-2 bg-surface/50 border-b border-border">
        <div className="text-xs font-medium text-foreground">Insert Token</div>
        <div className="text-[10px] text-muted-foreground">Click to insert at cursor</div>
      </div>
      <div className="divide-y divide-border">
        {Object.entries(categories).map(([category, tokens]) => (
          <div key={category}>
            <button
              onClick={() => setExpandedCategory(expandedCategory === category ? null : category)}
              className="w-full flex items-center justify-between px-3 py-2 hover:bg-muted/30 transition-colors"
            >
              <span className="text-xs font-medium text-foreground">
                {categoryLabels[category] || category}
              </span>
              {expandedCategory === category ? (
                <ChevronUp className="w-3 h-3 text-muted-foreground" />
              ) : (
                <ChevronDown className="w-3 h-3 text-muted-foreground" />
              )}
            </button>
            {expandedCategory === category && (
              <div className="px-2 pb-2 grid grid-cols-2 gap-1">
                {tokens.map((token) => (
                  <button
                    key={token.token}
                    onClick={() => onInsert(token.token)}
                    className="text-left p-2 rounded bg-surface hover:bg-violet-500/10 border border-border hover:border-violet-500/30 transition-colors group"
                    title={token.description}
                  >
                    <div className="text-xs font-mono text-violet-400 group-hover:text-violet-300">
                      {token.token}
                    </div>
                    <div className="text-[10px] text-muted-foreground truncate">
                      Ex: {token.example}
                    </div>
                  </button>
                ))}
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}

// =============================================================================
// PREVIEW PANEL COMPONENT
// =============================================================================

interface PreviewPanelProps {
  config: BatchNamingConfig;
}

function PreviewPanel({ config }: PreviewPanelProps) {
  const samples = useMemo(() => {
    return BatchNamingService.getSampleBatchNames(config, 3);
  }, [config]);

  const [copied, setCopied] = useState<number | null>(null);

  const handleCopy = (text: string, index: number) => {
    navigator.clipboard.writeText(text);
    setCopied(index);
    setTimeout(() => setCopied(null), 2000);
  };

  return (
    <div className="p-4 bg-surface border border-border rounded-lg">
      <div className="flex items-center gap-2 mb-3">
        <Hash className="w-4 h-4 text-violet-400" />
        <span className="text-sm font-medium text-foreground">Preview</span>
      </div>
      <div className="space-y-2">
        {samples.map((name, idx) => (
          <div
            key={idx}
            className="flex items-center justify-between p-2 bg-background/50 rounded border border-border/50"
          >
            <span className="font-mono text-sm text-cyan-400">{name}</span>
            <button
              onClick={() => handleCopy(name, idx)}
              className="p-1 hover:bg-muted/50 rounded transition-colors"
            >
              {copied === idx ? (
                <Check className="w-3 h-3 text-emerald-400" />
              ) : (
                <Copy className="w-3 h-3 text-muted-foreground" />
              )}
            </button>
          </div>
        ))}
      </div>
      <div className="mt-3 text-[10px] text-muted-foreground">
        Sample names using: Blue Dream, OG Kush, Girl Scout Cookies
      </div>
    </div>
  );
}

// =============================================================================
// STATE COMPLIANCE HINTS COMPONENT
// =============================================================================

function ComplianceHints({
  onSelectTemplate,
}: {
  onSelectTemplate: (template: string) => void;
}) {
  const [expanded, setExpanded] = useState(false);

  return (
    <div className="border border-border rounded-lg overflow-hidden">
      <button
        onClick={() => setExpanded(!expanded)}
        className="w-full flex items-center justify-between px-4 py-3 bg-surface/50 hover:bg-surface transition-colors"
      >
        <div className="flex items-center gap-2">
          <MapPin className="w-4 h-4 text-amber-400" />
          <span className="text-sm font-medium text-foreground">
            State Compliance Guidelines
          </span>
        </div>
        {expanded ? (
          <ChevronUp className="w-4 h-4 text-muted-foreground" />
        ) : (
          <ChevronDown className="w-4 h-4 text-muted-foreground" />
        )}
      </button>
      {expanded && (
        <div className="p-4 space-y-3 max-h-80 overflow-y-auto">
          {STATE_COMPLIANCE_HINTS.map((hint) => (
            <div
              key={hint.stateCode}
              className="p-3 bg-background/50 rounded-lg border border-border/50"
            >
              <div className="flex items-center justify-between mb-2">
                <span className="text-sm font-medium text-foreground">
                  {hint.stateName} ({hint.stateCode})
                </span>
                <button
                  onClick={() => onSelectTemplate(hint.suggestedTemplate)}
                  className="text-[10px] px-2 py-1 bg-violet-500/10 text-violet-400 hover:bg-violet-500/20 rounded transition-colors"
                >
                  Use Template
                </button>
              </div>
              <p className="text-xs text-muted-foreground mb-2">{hint.requirement}</p>
              <div className="flex items-center gap-2">
                <span className="text-[10px] text-muted-foreground">Example:</span>
                <code className="text-xs font-mono text-cyan-400 bg-cyan-500/10 px-2 py-0.5 rounded">
                  {hint.example}
                </code>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

// =============================================================================
// MAIN PAGE COMPONENT
// =============================================================================

export default function BatchNamingAdminPage() {
  const [config, setConfig] = useState<BatchNamingConfig | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [hasChanges, setHasChanges] = useState(false);

  // Form state
  const [mode, setMode] = useState<NamingMode>('template');
  const [template, setTemplate] = useState(DEFAULT_TEMPLATE);
  const [prefix, setPrefix] = useState(DEFAULT_PREFIX);
  const [suffix, setSuffix] = useState('');
  const [digitCount, setDigitCount] = useState(DEFAULT_DIGIT_COUNT);
  const [resetFrequency, setResetFrequency] = useState<ResetFrequency>('yearly');

  // Validation
  const [templateValidation, setTemplateValidation] = useState<{
    valid: boolean;
    errors: string[];
  }>({ valid: true, errors: [] });

  // Load config on mount
  useEffect(() => {
    const loadConfig = async () => {
      try {
        const loadedConfig = await BatchNamingService.getBatchNamingConfig('site-1');
        setConfig(loadedConfig);
        setMode(loadedConfig.mode);
        setTemplate(loadedConfig.template || DEFAULT_TEMPLATE);
        setPrefix(loadedConfig.prefix || DEFAULT_PREFIX);
        setSuffix(loadedConfig.suffix || '');
        setDigitCount(loadedConfig.digitCount || DEFAULT_DIGIT_COUNT);
        setResetFrequency(loadedConfig.resetFrequency || 'yearly');
      } catch (error) {
        console.error('Failed to load batch naming config:', error);
      } finally {
        setIsLoading(false);
      }
    };
    loadConfig();
  }, []);

  // Validate template when it changes
  useEffect(() => {
    if (mode === 'template') {
      const validation = BatchNamingService.validateTemplate(template);
      setTemplateValidation(validation);
    }
  }, [template, mode]);

  // Build preview config
  const previewConfig = useMemo<BatchNamingConfig>(() => {
    return {
      id: config?.id || 'preview',
      siteId: config?.siteId || 'site-1',
      mode,
      template,
      prefix,
      suffix,
      digitCount,
      currentNumber: config?.currentNumber || 0,
      resetFrequency,
      isActive: true,
      createdAt: config?.createdAt || new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };
  }, [config, mode, template, prefix, suffix, digitCount, resetFrequency]);

  // Handle token insertion
  const handleInsertToken = (token: string) => {
    setTemplate((prev) => prev + token);
    setHasChanges(true);
  };

  // Handle save
  const handleSave = async () => {
    setIsSaving(true);
    try {
      await BatchNamingService.updateBatchNamingConfig('site-1', {
        mode,
        template,
        prefix,
        suffix,
        digitCount,
        resetFrequency,
      });
      setHasChanges(false);
    } catch (error) {
      console.error('Failed to save config:', error);
    } finally {
      setIsSaving(false);
    }
  };

  // Handle counter reset
  const handleResetCounter = async () => {
    if (!confirm('Are you sure you want to reset the counter to 0?')) return;
    try {
      const updated = await BatchNamingService.resetCounter('site-1');
      setConfig(updated);
    } catch (error) {
      console.error('Failed to reset counter:', error);
    }
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-violet-500" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header Actions */}
      <div className="flex items-center justify-between">
        <div>
          <h3 className="text-lg font-semibold text-foreground">Batch Naming Rules</h3>
          <p className="text-sm text-muted-foreground">
            Configure how batch names are automatically generated
          </p>
        </div>
        <Button onClick={handleSave} disabled={!hasChanges || isSaving}>
          {isSaving ? 'Saving...' : 'Save Changes'}
        </Button>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Left Column - Configuration */}
        <div className="lg:col-span-2 space-y-6">
          {/* Mode Selection */}
          <AdminCard title="Naming Mode" icon={Tag}>
            <div className="grid grid-cols-2 gap-4">
              <button
                onClick={() => {
                  setMode('template');
                  setHasChanges(true);
                }}
                className={`p-4 rounded-lg border-2 transition-all ${
                  mode === 'template'
                    ? 'border-violet-500 bg-violet-500/10'
                    : 'border-border hover:border-violet-500/30'
                }`}
              >
                <FileText
                  className={`w-6 h-6 mb-2 ${
                    mode === 'template' ? 'text-violet-400' : 'text-muted-foreground'
                  }`}
                />
                <div
                  className={`text-sm font-medium ${
                    mode === 'template' ? 'text-violet-300' : 'text-foreground'
                  }`}
                >
                  Template-Based
                </div>
                <div className="text-xs text-muted-foreground mt-1">
                  Flexible naming with tokens like strain, date, sequence
                </div>
              </button>
              <button
                onClick={() => {
                  setMode('sequential');
                  setHasChanges(true);
                }}
                className={`p-4 rounded-lg border-2 transition-all ${
                  mode === 'sequential'
                    ? 'border-violet-500 bg-violet-500/10'
                    : 'border-border hover:border-violet-500/30'
                }`}
              >
                <Hash
                  className={`w-6 h-6 mb-2 ${
                    mode === 'sequential' ? 'text-violet-400' : 'text-muted-foreground'
                  }`}
                />
                <div
                  className={`text-sm font-medium ${
                    mode === 'sequential' ? 'text-violet-300' : 'text-foreground'
                  }`}
                >
                  Sequential
                </div>
                <div className="text-xs text-muted-foreground mt-1">
                  Simple prefix + auto-incrementing number
                </div>
              </button>
            </div>
          </AdminCard>

          {/* Template Mode Configuration */}
          {mode === 'template' && (
            <AdminCard title="Template Configuration" icon={FileText}>
              <div className="space-y-4">
                <FormField
                  label="Naming Template"
                  description="Use tokens to build your batch naming pattern"
                  error={
                    !templateValidation.valid ? templateValidation.errors.join(', ') : undefined
                  }
                >
                  <Input
                    value={template}
                    onChange={(e) => {
                      setTemplate(e.target.value);
                      setHasChanges(true);
                    }}
                    placeholder="{STRAIN_CODE}-{YYYY}-{###}"
                    className="font-mono"
                  />
                </FormField>

                <TokenPalette onInsert={handleInsertToken} />
              </div>
            </AdminCard>
          )}

          {/* Sequential Mode Configuration */}
          {mode === 'sequential' && (
            <AdminCard title="Sequential Configuration" icon={Hash}>
              <div className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <FormField label="Prefix" description="Text before the number">
                    <Input
                      value={prefix}
                      onChange={(e) => {
                        setPrefix(e.target.value);
                        setHasChanges(true);
                      }}
                      placeholder="B-"
                    />
                  </FormField>
                  <FormField label="Suffix" description="Text after the number (optional)">
                    <Input
                      value={suffix}
                      onChange={(e) => {
                        setSuffix(e.target.value);
                        setHasChanges(true);
                      }}
                      placeholder="-2024"
                    />
                  </FormField>
                </div>

                <FormField
                  label="Number of Digits"
                  description="Pad with zeros (e.g., 4 digits = 0001)"
                >
                  <Select
                    value={String(digitCount)}
                    onChange={(e) => {
                      setDigitCount(Number(e.target.value));
                      setHasChanges(true);
                    }}
                    options={[
                      { value: '3', label: '3 digits (001)' },
                      { value: '4', label: '4 digits (0001)' },
                      { value: '5', label: '5 digits (00001)' },
                      { value: '6', label: '6 digits (000001)' },
                    ]}
                  />
                </FormField>
              </div>
            </AdminCard>
          )}

          {/* Counter Settings */}
          <AdminCard title="Counter Settings" icon={RefreshCw}>
            <div className="space-y-4">
              <div className="flex items-center justify-between p-4 bg-surface/50 rounded-lg border border-border">
                <div>
                  <div className="text-sm font-medium text-foreground">Current Counter</div>
                  <div className="text-xs text-muted-foreground">
                    Next batch will use number {(config?.currentNumber || 0) + 1}
                  </div>
                </div>
                <div className="flex items-center gap-3">
                  <span className="text-2xl font-mono font-bold text-cyan-400">
                    {config?.currentNumber || 0}
                  </span>
                  <Button variant="ghost" size="sm" onClick={handleResetCounter}>
                    Reset
                  </Button>
                </div>
              </div>

              <FormField
                label="Counter Reset Frequency"
                description="When to automatically reset the counter"
              >
                <Select
                  value={resetFrequency}
                  onChange={(e) => {
                    setResetFrequency(e.target.value as ResetFrequency);
                    setHasChanges(true);
                  }}
                  options={[
                    { value: 'never', label: 'Never (continuous)' },
                    { value: 'yearly', label: 'Yearly (reset Jan 1)' },
                    { value: 'quarterly', label: 'Quarterly' },
                    { value: 'monthly', label: 'Monthly' },
                  ]}
                />
              </FormField>
            </div>
          </AdminCard>
        </div>

        {/* Right Column - Preview & Compliance */}
        <div className="space-y-6">
          <PreviewPanel config={previewConfig} />
          <ComplianceHints
            onSelectTemplate={(t) => {
              setTemplate(t);
              setMode('template');
              setHasChanges(true);
            }}
          />

          {/* Info Card */}
          <div className="p-4 bg-violet-500/10 border border-violet-500/20 rounded-lg">
            <div className="flex items-start gap-3">
              <Info className="w-4 h-4 text-violet-400 mt-0.5 flex-shrink-0" />
              <div>
                <div className="text-sm font-medium text-violet-300 mb-1">
                  Batch Naming Best Practices
                </div>
                <ul className="text-xs text-violet-200/80 space-y-1">
                  <li>• Always include a sequence number for uniqueness</li>
                  <li>• Include date tokens for easy chronological sorting</li>
                  <li>• Consider state compliance requirements</li>
                  <li>• Keep names readable but compact</li>
                </ul>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}




