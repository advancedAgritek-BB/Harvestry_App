'use client';

import React, { useState, useEffect } from 'react';
import { cn } from '@/lib/utils';
import {
  X,
  Dna,
  FlaskConical,
  Leaf,
  Droplets,
  ChevronDown,
  HelpCircle,
  Eye,
  Building2,
} from 'lucide-react';
import type {
  Genetics,
  CreateGeneticsRequest,
  UpdateGeneticsRequest,
  GeneticType,
  YieldPotential,
  GeneticProfile,
  TerpeneProfile,
  VisualCharacteristics,
  AromaProfile,
  GrowthPattern,
  LeafShape,
  BudStructure,
  TrichomeDensity,
  AromaIntensity,
  ColaStructure,
  CanopyBehavior,
  TrainingResponse,
} from '../../types';
import {
  GENETIC_TYPE_CONFIG,
  YIELD_POTENTIAL_CONFIG,
  LEAF_SHAPE_CONFIG,
  BUD_STRUCTURE_CONFIG,
  TRICHOME_DENSITY_CONFIG,
  AROMA_INTENSITY_CONFIG,
  COLA_STRUCTURE_CONFIG,
  CANOPY_BEHAVIOR_CONFIG,
  TRAINING_RESPONSE_CONFIG,
  createEmptyGeneticProfile,
  createEmptyTerpeneProfile,
  createEmptyVisualCharacteristics,
  createEmptyAromaProfile,
  createEmptyGrowthPattern,
} from '../../types';

// =============================================================================
// TYPES
// =============================================================================

interface GeneticsModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: CreateGeneticsRequest | UpdateGeneticsRequest) => Promise<void>;
  genetics?: Genetics | null;
  isLoading?: boolean;
}

interface FormData {
  name: string;
  description: string;
  geneticType: GeneticType;
  thcMin: number;
  thcMax: number;
  cbdMin: number;
  cbdMax: number;
  floweringTimeDays: number | undefined;
  yieldPotential: YieldPotential;
  growthCharacteristics: GeneticProfile;
  terpeneProfile: TerpeneProfile;
  breedingNotes: string;
  // Phenotype fields
  expressionNotes: string;
  visualCharacteristics: VisualCharacteristics;
  aromaProfile: AromaProfile;
  growthPattern: GrowthPattern;
  // Source fields
  breeder: string;
  seedBank: string;
  cultivationNotes: string;
}

// =============================================================================
// HELPER COMPONENTS
// =============================================================================

function FormField({
  label,
  required,
  hint,
  error,
  children,
}: {
  label: string;
  required?: boolean;
  hint?: string;
  error?: string;
  children: React.ReactNode;
}) {
  return (
    <div className="space-y-1.5">
      <label className="flex items-center gap-1 text-sm font-medium text-foreground">
        {label}
        {required && <span className="text-rose-400">*</span>}
        {hint && (
          <span className="ml-1 text-muted-foreground" title={hint}>
            <HelpCircle className="w-3.5 h-3.5" />
          </span>
        )}
      </label>
      {children}
      {error && <p className="text-xs text-rose-400">{error}</p>}
    </div>
  );
}

function SelectField<T extends string>({
  value,
  onChange,
  options,
  placeholder,
}: {
  value: T;
  onChange: (value: T) => void;
  options: { value: T; label: string; color?: string }[];
  placeholder?: string;
}) {
  const [isOpen, setIsOpen] = useState(false);
  const selected = options.find((o) => o.value === value);

  return (
    <div className="relative">
      <button
        type="button"
        onClick={() => setIsOpen(!isOpen)}
        className="w-full flex items-center justify-between px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm hover:border-emerald-500/30 transition-colors"
      >
        <span className={cn("flex items-center gap-2", !selected && "text-muted-foreground")}>
          {selected?.color && (
            <span
              className="w-2 h-2 rounded-full"
              style={{ backgroundColor: selected.color }}
            />
          )}
          {selected?.label || placeholder || 'Select...'}
        </span>
        <ChevronDown className={cn("w-4 h-4 transition-transform", isOpen && "rotate-180")} />
      </button>

      {isOpen && (
        <>
          <div className="fixed inset-0 z-40" onClick={() => setIsOpen(false)} />
          <div className="absolute top-full left-0 right-0 mt-1 bg-surface border border-border rounded-lg shadow-xl z-50 py-1 max-h-48 overflow-y-auto">
            {options.map((option) => (
              <button
                key={option.value}
                type="button"
                onClick={() => {
                  onChange(option.value);
                  setIsOpen(false);
                }}
                className={cn(
                  "w-full flex items-center gap-2 px-3 py-2 text-sm text-left transition-colors",
                  option.value === value
                    ? "bg-emerald-500/10 text-emerald-400"
                    : "text-foreground hover:bg-muted/50"
                )}
              >
                {option.color && (
                  <span
                    className="w-2 h-2 rounded-full"
                    style={{ backgroundColor: option.color }}
                  />
                )}
                {option.label}
              </button>
            ))}
          </div>
        </>
      )}
    </div>
  );
}

function RangeInputs({
  minValue,
  maxValue,
  onMinChange,
  onMaxChange,
  unit,
  min = 0,
  max = 100,
  step = 0.1,
}: {
  minValue: number;
  maxValue: number;
  onMinChange: (value: number) => void;
  onMaxChange: (value: number) => void;
  unit: string;
  min?: number;
  max?: number;
  step?: number;
}) {
  return (
    <div className="flex items-center gap-2">
      <input
        type="number"
        value={minValue}
        onChange={(e) => onMinChange(parseFloat(e.target.value) || 0)}
        min={min}
        max={max}
        step={step}
        className="w-20 px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground focus:outline-none focus:border-emerald-500/50"
      />
      <span className="text-muted-foreground">to</span>
      <input
        type="number"
        value={maxValue}
        onChange={(e) => onMaxChange(parseFloat(e.target.value) || 0)}
        min={min}
        max={max}
        step={step}
        className="w-20 px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground focus:outline-none focus:border-emerald-500/50"
      />
      <span className="text-sm text-muted-foreground">{unit}</span>
    </div>
  );
}

function TerpeneTagInput({
  terpenes,
  onChange,
}: {
  terpenes: Record<string, number>;
  onChange: (terpenes: Record<string, number>) => void;
}) {
  const [inputValue, setInputValue] = useState('');

  const addTerpene = () => {
    const trimmed = inputValue.trim().toLowerCase();
    if (trimmed && !terpenes[trimmed]) {
      onChange({ ...terpenes, [trimmed]: 0.5 });
      setInputValue('');
    }
  };

  const removeTerpene = (key: string) => {
    const newTerpenes = { ...terpenes };
    delete newTerpenes[key];
    onChange(newTerpenes);
  };

  return (
    <div className="space-y-2">
      <div className="flex flex-wrap gap-2">
        {Object.entries(terpenes).map(([name, value]) => (
          <span
            key={name}
            className="inline-flex items-center gap-1 px-2 py-1 bg-emerald-500/10 text-emerald-400 rounded-lg text-xs capitalize"
          >
            {name}
            <button
              type="button"
              onClick={() => removeTerpene(name)}
              className="hover:text-rose-400 transition-colors"
            >
              <X className="w-3 h-3" />
            </button>
          </span>
        ))}
      </div>
      <div className="flex gap-2">
        <input
          type="text"
          value={inputValue}
          onChange={(e) => setInputValue(e.target.value)}
          onKeyDown={(e) => e.key === 'Enter' && (e.preventDefault(), addTerpene())}
          placeholder="Add terpene (e.g., myrcene)"
          className="flex-1 px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-emerald-500/50"
        />
        <button
          type="button"
          onClick={addTerpene}
          className="px-3 py-2 bg-emerald-500/10 text-emerald-400 rounded-lg text-sm hover:bg-emerald-500/20 transition-colors"
        >
          Add
        </button>
      </div>
    </div>
  );
}

// =============================================================================
// MAIN COMPONENT
// =============================================================================

export function GeneticsModal({
  isOpen,
  onClose,
  onSubmit,
  genetics,
  isLoading = false,
}: GeneticsModalProps) {
  const isEditing = !!genetics;

  // Form state
  const [formData, setFormData] = useState<FormData>({
    name: '',
    description: '',
    geneticType: 'hybrid',
    thcMin: 15,
    thcMax: 20,
    cbdMin: 0.1,
    cbdMax: 0.5,
    floweringTimeDays: 60,
    yieldPotential: 'medium',
    growthCharacteristics: createEmptyGeneticProfile(),
    terpeneProfile: createEmptyTerpeneProfile(),
    breedingNotes: '',
    // Phenotype fields
    expressionNotes: '',
    visualCharacteristics: createEmptyVisualCharacteristics(),
    aromaProfile: createEmptyAromaProfile(),
    growthPattern: createEmptyGrowthPattern(),
    // Source fields
    breeder: '',
    seedBank: '',
    cultivationNotes: '',
  });

  const [errors, setErrors] = useState<Record<string, string>>({});
  const [activeSection, setActiveSection] = useState<'basic' | 'cannabinoids' | 'growth' | 'phenotype' | 'terpenes' | 'source'>('basic');

  // Populate form when editing
  useEffect(() => {
    if (genetics) {
      setFormData({
        name: genetics.name,
        description: genetics.description,
        geneticType: genetics.geneticType,
        thcMin: genetics.thcMin,
        thcMax: genetics.thcMax,
        cbdMin: genetics.cbdMin,
        cbdMax: genetics.cbdMax,
        floweringTimeDays: genetics.floweringTimeDays,
        yieldPotential: genetics.yieldPotential,
        growthCharacteristics: genetics.growthCharacteristics || createEmptyGeneticProfile(),
        terpeneProfile: genetics.terpeneProfile || createEmptyTerpeneProfile(),
        breedingNotes: genetics.breedingNotes || '',
        // Phenotype fields
        expressionNotes: genetics.expressionNotes || '',
        visualCharacteristics: genetics.visualCharacteristics || createEmptyVisualCharacteristics(),
        aromaProfile: genetics.aromaProfile || createEmptyAromaProfile(),
        growthPattern: genetics.growthPattern || createEmptyGrowthPattern(),
        // Source fields
        breeder: genetics.breeder || '',
        seedBank: genetics.seedBank || '',
        cultivationNotes: genetics.cultivationNotes || '',
      });
    } else {
      // Reset to defaults for new genetics
      setFormData({
        name: '',
        description: '',
        geneticType: 'hybrid',
        thcMin: 15,
        thcMax: 20,
        cbdMin: 0.1,
        cbdMax: 0.5,
        floweringTimeDays: 60,
        yieldPotential: 'medium',
        growthCharacteristics: createEmptyGeneticProfile(),
        terpeneProfile: createEmptyTerpeneProfile(),
        breedingNotes: '',
        // Phenotype fields
        expressionNotes: '',
        visualCharacteristics: createEmptyVisualCharacteristics(),
        aromaProfile: createEmptyAromaProfile(),
        growthPattern: createEmptyGrowthPattern(),
        // Source fields
        breeder: '',
        seedBank: '',
        cultivationNotes: '',
      });
    }
    setErrors({});
  }, [genetics, isOpen]);

  // Handle escape key
  useEffect(() => {
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && isOpen) onClose();
    };

    document.addEventListener('keydown', handleEscape);
    return () => document.removeEventListener('keydown', handleEscape);
  }, [isOpen, onClose]);

  // Prevent body scroll when modal is open
  useEffect(() => {
    if (isOpen) {
      document.body.style.overflow = 'hidden';
    }
    return () => {
      document.body.style.overflow = 'unset';
    };
  }, [isOpen]);

  // Validation
  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.name.trim()) {
      newErrors.name = 'Name is required';
    } else if (formData.name.length > 200) {
      newErrors.name = 'Name must be 200 characters or less';
    }

    if (!formData.description.trim()) {
      newErrors.description = 'Description is required';
    } else if (formData.description.length > 2000) {
      newErrors.description = 'Description must be 2000 characters or less';
    }

    if (formData.thcMax < formData.thcMin) {
      newErrors.thcRange = 'Max THC must be greater than or equal to min';
    }

    if (formData.cbdMax < formData.cbdMin) {
      newErrors.cbdRange = 'Max CBD must be greater than or equal to min';
    }

    if (formData.floweringTimeDays !== undefined && 
        (formData.floweringTimeDays < 1 || formData.floweringTimeDays > 365)) {
      newErrors.floweringTime = 'Flowering time must be between 1 and 365 days';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  // Handle submit
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validate()) return;

    const commonFields = {
      description: formData.description,
      thcMin: formData.thcMin,
      thcMax: formData.thcMax,
      cbdMin: formData.cbdMin,
      cbdMax: formData.cbdMax,
      floweringTimeDays: formData.floweringTimeDays,
      yieldPotential: formData.yieldPotential,
      growthCharacteristics: formData.growthCharacteristics,
      terpeneProfile: formData.terpeneProfile,
      breedingNotes: formData.breedingNotes || undefined,
      // Phenotype fields
      expressionNotes: formData.expressionNotes || undefined,
      visualCharacteristics: formData.visualCharacteristics,
      aromaProfile: formData.aromaProfile,
      growthPattern: formData.growthPattern,
      // Source fields
      breeder: formData.breeder || undefined,
      seedBank: formData.seedBank || undefined,
      cultivationNotes: formData.cultivationNotes || undefined,
    };

    const data: CreateGeneticsRequest | UpdateGeneticsRequest = isEditing
      ? commonFields
      : {
          name: formData.name,
          geneticType: formData.geneticType,
          ...commonFields,
        };

    await onSubmit(data);
  };

  if (!isOpen) return null;

  // Type options
  const typeOptions = (Object.keys(GENETIC_TYPE_CONFIG) as GeneticType[]).map((type) => ({
    value: type,
    label: GENETIC_TYPE_CONFIG[type].label,
    color: GENETIC_TYPE_CONFIG[type].color,
  }));

  // Yield options
  const yieldOptions = (Object.keys(YIELD_POTENTIAL_CONFIG) as YieldPotential[]).map((y) => ({
    value: y,
    label: YIELD_POTENTIAL_CONFIG[y].label,
    color: YIELD_POTENTIAL_CONFIG[y].color,
  }));

  const sectionTabs = [
    { key: 'basic', label: 'Basic', icon: Dna },
    { key: 'cannabinoids', label: 'Cannabinoids', icon: FlaskConical },
    { key: 'growth', label: 'Growth', icon: Leaf },
    { key: 'phenotype', label: 'Phenotype', icon: Eye },
    { key: 'terpenes', label: 'Terpenes', icon: Droplets },
    { key: 'source', label: 'Source', icon: Building2 },
  ] as const;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      {/* Backdrop */}
      <div
        className="absolute inset-0 bg-background/80 backdrop-blur-sm"
        onClick={onClose}
      />

      {/* Modal */}
      <div className="relative w-full max-w-2xl max-h-[90vh] bg-surface border border-border rounded-xl shadow-2xl flex flex-col">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-border shrink-0">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-emerald-500/10 flex items-center justify-center">
              <Dna className="w-5 h-5 text-emerald-400" />
            </div>
            <div>
              <h2 className="text-lg font-semibold text-foreground">
                {isEditing ? 'Edit Genetics' : 'Add New Genetics'}
              </h2>
              <p className="text-sm text-muted-foreground">
                {isEditing ? `Editing ${genetics?.name}` : 'Create a new genetics entry for your library'}
              </p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-2 rounded-lg text-muted-foreground hover:text-foreground hover:bg-muted/50 transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Section Tabs */}
        <div className="flex items-center gap-1 px-6 py-2 border-b border-border bg-muted/20 shrink-0">
          {sectionTabs.map((tab) => {
            const Icon = tab.icon;
            return (
              <button
                key={tab.key}
                type="button"
                onClick={() => setActiveSection(tab.key)}
                className={cn(
                  "flex items-center gap-2 px-3 py-1.5 rounded-lg text-sm font-medium transition-colors",
                  activeSection === tab.key
                    ? "bg-emerald-500/20 text-emerald-400"
                    : "text-muted-foreground hover:text-foreground hover:bg-muted/50"
                )}
              >
                <Icon className="w-4 h-4" />
                {tab.label}
              </button>
            );
          })}
        </div>

        {/* Form Content */}
        <form onSubmit={handleSubmit} className="flex flex-col flex-1 min-h-0">
          <div className="flex-1 overflow-y-auto px-6 py-4">
            {/* Basic Info Section */}
            {activeSection === 'basic' && (
              <div className="space-y-4">
                <FormField label="Name" required error={errors.name}>
                  <input
                    type="text"
                    value={formData.name}
                    onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                    disabled={isEditing}
                    placeholder="e.g., Blue Dream"
                    className={cn(
                      "w-full px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-emerald-500/50 transition-colors",
                      isEditing && "opacity-50 cursor-not-allowed"
                    )}
                  />
                </FormField>

                <FormField label="Genetic Type" required>
                  <SelectField
                    value={formData.geneticType}
                    onChange={(value) => setFormData({ ...formData, geneticType: value })}
                    options={typeOptions}
                    placeholder="Select type..."
                  />
                </FormField>

                <FormField label="Description" required error={errors.description}>
                  <textarea
                    value={formData.description}
                    onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                    placeholder="Describe the genetics, effects, and characteristics..."
                    rows={4}
                    className="w-full px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-emerald-500/50 transition-colors resize-none"
                  />
                </FormField>

                <FormField label="Yield Potential" required>
                  <SelectField
                    value={formData.yieldPotential}
                    onChange={(value) => setFormData({ ...formData, yieldPotential: value })}
                    options={yieldOptions}
                    placeholder="Select yield..."
                  />
                </FormField>

                <FormField label="Breeding Notes" hint="Optional notes about lineage or breeding history">
                  <textarea
                    value={formData.breedingNotes}
                    onChange={(e) => setFormData({ ...formData, breedingNotes: e.target.value })}
                    placeholder="Parent strains, breeder info, etc."
                    rows={2}
                    className="w-full px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-emerald-500/50 transition-colors resize-none"
                  />
                </FormField>
              </div>
            )}

            {/* Cannabinoids Section */}
            {activeSection === 'cannabinoids' && (
              <div className="space-y-4">
                <FormField label="THC Range" required error={errors.thcRange}>
                  <RangeInputs
                    minValue={formData.thcMin}
                    maxValue={formData.thcMax}
                    onMinChange={(v) => setFormData({ ...formData, thcMin: v })}
                    onMaxChange={(v) => setFormData({ ...formData, thcMax: v })}
                    unit="%"
                    min={0}
                    max={35}
                    step={0.5}
                  />
                </FormField>

                <FormField label="CBD Range" required error={errors.cbdRange}>
                  <RangeInputs
                    minValue={formData.cbdMin}
                    maxValue={formData.cbdMax}
                    onMinChange={(v) => setFormData({ ...formData, cbdMin: v })}
                    onMaxChange={(v) => setFormData({ ...formData, cbdMax: v })}
                    unit="%"
                    min={0}
                    max={30}
                    step={0.1}
                  />
                </FormField>

                <FormField label="Flowering Time" error={errors.floweringTime} hint="Days from flip to harvest">
                  <div className="flex items-center gap-2">
                    <input
                      type="number"
                      value={formData.floweringTimeDays ?? ''}
                      onChange={(e) => setFormData({
                        ...formData,
                        floweringTimeDays: e.target.value ? parseInt(e.target.value) : undefined,
                      })}
                      min={1}
                      max={365}
                      placeholder="60"
                      className="w-24 px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground focus:outline-none focus:border-emerald-500/50"
                    />
                    <span className="text-sm text-muted-foreground">days</span>
                  </div>
                </FormField>
              </div>
            )}

            {/* Growth Section */}
            {activeSection === 'growth' && (
              <div className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <FormField label="Stretch Tendency">
                    <SelectField
                      value={formData.growthCharacteristics.stretchTendency || ''}
                      onChange={(v) => setFormData({
                        ...formData,
                        growthCharacteristics: { ...formData.growthCharacteristics, stretchTendency: v || undefined },
                      })}
                      options={[
                        { value: '', label: 'Not specified' },
                        { value: 'low', label: 'Low' },
                        { value: 'moderate', label: 'Moderate' },
                        { value: 'high', label: 'High' },
                        { value: 'very high', label: 'Very High' },
                      ]}
                    />
                  </FormField>

                  <FormField label="Branching Pattern">
                    <SelectField
                      value={formData.growthCharacteristics.branchingPattern || ''}
                      onChange={(v) => setFormData({
                        ...formData,
                        growthCharacteristics: { ...formData.growthCharacteristics, branchingPattern: v || undefined },
                      })}
                      options={[
                        { value: '', label: 'Not specified' },
                        { value: 'compact', label: 'Compact' },
                        { value: 'dense', label: 'Dense' },
                        { value: 'open', label: 'Open' },
                      ]}
                    />
                  </FormField>

                  <FormField label="Nutrient Sensitivity">
                    <SelectField
                      value={formData.growthCharacteristics.nutrientSensitivity || ''}
                      onChange={(v) => setFormData({
                        ...formData,
                        growthCharacteristics: { ...formData.growthCharacteristics, nutrientSensitivity: v || undefined },
                      })}
                      options={[
                        { value: '', label: 'Not specified' },
                        { value: 'low', label: 'Low' },
                        { value: 'moderate', label: 'Moderate' },
                        { value: 'high', label: 'High' },
                      ]}
                    />
                  </FormField>

                  <FormField label="Light Preference">
                    <SelectField
                      value={formData.growthCharacteristics.lightIntensityPreference || ''}
                      onChange={(v) => setFormData({
                        ...formData,
                        growthCharacteristics: { ...formData.growthCharacteristics, lightIntensityPreference: v || undefined },
                      })}
                      options={[
                        { value: '', label: 'Not specified' },
                        { value: 'low', label: 'Low' },
                        { value: 'medium', label: 'Medium' },
                        { value: 'medium-high', label: 'Medium-High' },
                        { value: 'high', label: 'High' },
                      ]}
                    />
                  </FormField>
                </div>

                <FormField label="Optimal Temperature Range (°F)">
                  <RangeInputs
                    minValue={formData.growthCharacteristics.optimalTemperatureMin ?? 65}
                    maxValue={formData.growthCharacteristics.optimalTemperatureMax ?? 80}
                    onMinChange={(v) => setFormData({
                      ...formData,
                      growthCharacteristics: { ...formData.growthCharacteristics, optimalTemperatureMin: v },
                    })}
                    onMaxChange={(v) => setFormData({
                      ...formData,
                      growthCharacteristics: { ...formData.growthCharacteristics, optimalTemperatureMax: v },
                    })}
                    unit="°F"
                    min={50}
                    max={100}
                    step={1}
                  />
                </FormField>

                <FormField label="Optimal Humidity Range">
                  <RangeInputs
                    minValue={formData.growthCharacteristics.optimalHumidityMin ?? 40}
                    maxValue={formData.growthCharacteristics.optimalHumidityMax ?? 60}
                    onMinChange={(v) => setFormData({
                      ...formData,
                      growthCharacteristics: { ...formData.growthCharacteristics, optimalHumidityMin: v },
                    })}
                    onMaxChange={(v) => setFormData({
                      ...formData,
                      growthCharacteristics: { ...formData.growthCharacteristics, optimalHumidityMax: v },
                    })}
                    unit="% RH"
                    min={20}
                    max={90}
                    step={5}
                  />
                </FormField>
              </div>
            )}

            {/* Phenotype Section */}
            {activeSection === 'phenotype' && (
              <div className="space-y-4">
                <FormField label="Expression Notes" hint="How this genetic typically expresses">
                  <textarea
                    value={formData.expressionNotes}
                    onChange={(e) => setFormData({ ...formData, expressionNotes: e.target.value })}
                    placeholder="Describe typical phenotype expression, variations observed..."
                    rows={3}
                    className="w-full px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-emerald-500/50 resize-none"
                  />
                </FormField>

                <div className="pt-2 pb-1">
                  <h4 className="text-sm font-medium text-foreground">Visual Characteristics</h4>
                  <p className="text-xs text-muted-foreground">Physical appearance traits</p>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <FormField label="Leaf Shape">
                    <SelectField
                      value={formData.visualCharacteristics.leafShape || ''}
                      onChange={(v) => setFormData({
                        ...formData,
                        visualCharacteristics: { ...formData.visualCharacteristics, leafShape: v as LeafShape || undefined },
                      })}
                      options={[
                        { value: '', label: 'Not specified' },
                        ...Object.entries(LEAF_SHAPE_CONFIG).map(([k, v]) => ({ value: k, label: v.label })),
                      ]}
                    />
                  </FormField>

                  <FormField label="Bud Structure">
                    <SelectField
                      value={formData.visualCharacteristics.budStructure || ''}
                      onChange={(v) => setFormData({
                        ...formData,
                        visualCharacteristics: { ...formData.visualCharacteristics, budStructure: v as BudStructure || undefined },
                      })}
                      options={[
                        { value: '', label: 'Not specified' },
                        ...Object.entries(BUD_STRUCTURE_CONFIG).map(([k, v]) => ({ value: k, label: v.label })),
                      ]}
                    />
                  </FormField>

                  <FormField label="Trichome Density">
                    <SelectField
                      value={formData.visualCharacteristics.trichomeDensity || ''}
                      onChange={(v) => setFormData({
                        ...formData,
                        visualCharacteristics: { ...formData.visualCharacteristics, trichomeDensity: v as TrichomeDensity || undefined },
                      })}
                      options={[
                        { value: '', label: 'Not specified' },
                        ...Object.entries(TRICHOME_DENSITY_CONFIG).map(([k, v]) => ({ value: k, label: v.label })),
                      ]}
                    />
                  </FormField>

                  <FormField label="Pistil Color">
                    <input
                      type="text"
                      value={formData.visualCharacteristics.pistilColor || ''}
                      onChange={(e) => setFormData({
                        ...formData,
                        visualCharacteristics: { ...formData.visualCharacteristics, pistilColor: e.target.value || undefined },
                      })}
                      placeholder="e.g., orange, red, amber"
                      className="w-full px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-emerald-500/50"
                    />
                  </FormField>
                </div>

                <FormField label="Primary Colors" hint="Comma-separated list">
                  <input
                    type="text"
                    value={formData.visualCharacteristics.primaryColors?.join(', ') || ''}
                    onChange={(e) => setFormData({
                      ...formData,
                      visualCharacteristics: {
                        ...formData.visualCharacteristics,
                        primaryColors: e.target.value.split(',').map((s) => s.trim()).filter(Boolean),
                      },
                    })}
                    placeholder="e.g., green, purple, orange"
                    className="w-full px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-emerald-500/50"
                  />
                </FormField>

                <div className="pt-2 pb-1">
                  <h4 className="text-sm font-medium text-foreground">Aroma Profile</h4>
                  <p className="text-xs text-muted-foreground">Distinct scent characteristics</p>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <FormField label="Primary Scents" hint="Comma-separated">
                    <input
                      type="text"
                      value={formData.aromaProfile.primaryScents?.join(', ') || ''}
                      onChange={(e) => setFormData({
                        ...formData,
                        aromaProfile: {
                          ...formData.aromaProfile,
                          primaryScents: e.target.value.split(',').map((s) => s.trim()).filter(Boolean),
                        },
                      })}
                      placeholder="e.g., diesel, citrus, pine"
                      className="w-full px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-emerald-500/50"
                    />
                  </FormField>

                  <FormField label="Aroma Intensity">
                    <SelectField
                      value={formData.aromaProfile.intensity || ''}
                      onChange={(v) => setFormData({
                        ...formData,
                        aromaProfile: { ...formData.aromaProfile, intensity: v as AromaIntensity || undefined },
                      })}
                      options={[
                        { value: '', label: 'Not specified' },
                        ...Object.entries(AROMA_INTENSITY_CONFIG).map(([k, v]) => ({ value: k, label: v.label })),
                      ]}
                    />
                  </FormField>
                </div>

                <div className="pt-2 pb-1">
                  <h4 className="text-sm font-medium text-foreground">Growth Pattern</h4>
                  <p className="text-xs text-muted-foreground">Structure and training behavior</p>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <FormField label="Cola Structure">
                    <SelectField
                      value={formData.growthPattern.colaStructure || ''}
                      onChange={(v) => setFormData({
                        ...formData,
                        growthPattern: { ...formData.growthPattern, colaStructure: v as ColaStructure || undefined },
                      })}
                      options={[
                        { value: '', label: 'Not specified' },
                        ...Object.entries(COLA_STRUCTURE_CONFIG).map(([k, v]) => ({ value: k, label: v.label })),
                      ]}
                    />
                  </FormField>

                  <FormField label="Canopy Behavior">
                    <SelectField
                      value={formData.growthPattern.canopyBehavior || ''}
                      onChange={(v) => setFormData({
                        ...formData,
                        growthPattern: { ...formData.growthPattern, canopyBehavior: v as CanopyBehavior || undefined },
                      })}
                      options={[
                        { value: '', label: 'Not specified' },
                        ...Object.entries(CANOPY_BEHAVIOR_CONFIG).map(([k, v]) => ({ value: k, label: v.label })),
                      ]}
                    />
                  </FormField>

                  <FormField label="Training Response">
                    <SelectField
                      value={formData.growthPattern.trainingResponse || ''}
                      onChange={(v) => setFormData({
                        ...formData,
                        growthPattern: { ...formData.growthPattern, trainingResponse: v as TrainingResponse || undefined },
                      })}
                      options={[
                        { value: '', label: 'Not specified' },
                        ...Object.entries(TRAINING_RESPONSE_CONFIG).map(([k, v]) => ({ value: k, label: v.label, color: v.color })),
                      ]}
                    />
                  </FormField>
                </div>

                <FormField label="Preferred Training Methods" hint="Comma-separated">
                  <input
                    type="text"
                    value={formData.growthPattern.preferredTrainingMethods?.join(', ') || ''}
                    onChange={(e) => setFormData({
                      ...formData,
                      growthPattern: {
                        ...formData.growthPattern,
                        preferredTrainingMethods: e.target.value.split(',').map((s) => s.trim()).filter(Boolean),
                      },
                    })}
                    placeholder="e.g., LST, topping, SCROG, mainlining"
                    className="w-full px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-emerald-500/50"
                  />
                </FormField>
              </div>
            )}

            {/* Terpenes Section */}
            {activeSection === 'terpenes' && (
              <div className="space-y-4">
                <FormField label="Dominant Terpenes" hint="Add the main terpenes found in this genetics">
                  <TerpeneTagInput
                    terpenes={formData.terpeneProfile.dominantTerpenes || {}}
                    onChange={(terpenes) => setFormData({
                      ...formData,
                      terpeneProfile: { ...formData.terpeneProfile, dominantTerpenes: terpenes },
                    })}
                  />
                </FormField>

                <FormField label="Aroma Descriptors" hint="Comma-separated list">
                  <input
                    type="text"
                    value={formData.terpeneProfile.aromaDescriptors?.join(', ') || ''}
                    onChange={(e) => setFormData({
                      ...formData,
                      terpeneProfile: {
                        ...formData.terpeneProfile,
                        aromaDescriptors: e.target.value.split(',').map((s) => s.trim()).filter(Boolean),
                      },
                    })}
                    placeholder="e.g., earthy, pine, citrus"
                    className="w-full px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-emerald-500/50"
                  />
                </FormField>

                <FormField label="Flavor Descriptors" hint="Comma-separated list">
                  <input
                    type="text"
                    value={formData.terpeneProfile.flavorDescriptors?.join(', ') || ''}
                    onChange={(e) => setFormData({
                      ...formData,
                      terpeneProfile: {
                        ...formData.terpeneProfile,
                        flavorDescriptors: e.target.value.split(',').map((s) => s.trim()).filter(Boolean),
                      },
                    })}
                    placeholder="e.g., sweet, herbal, spicy"
                    className="w-full px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-emerald-500/50"
                  />
                </FormField>

                <FormField label="Overall Profile">
                  <textarea
                    value={formData.terpeneProfile.overallProfile || ''}
                    onChange={(e) => setFormData({
                      ...formData,
                      terpeneProfile: { ...formData.terpeneProfile, overallProfile: e.target.value },
                    })}
                    placeholder="Describe the overall aroma and flavor profile..."
                    rows={2}
                    className="w-full px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-emerald-500/50 resize-none"
                  />
                </FormField>
              </div>
            )}

            {/* Source Section */}
            {activeSection === 'source' && (
              <div className="space-y-4">
                <FormField label="Breeder" hint="Original breeder or developer">
                  <input
                    type="text"
                    value={formData.breeder}
                    onChange={(e) => setFormData({ ...formData, breeder: e.target.value })}
                    placeholder="e.g., Exotic Genetix, Archive Seed Bank"
                    className="w-full px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-emerald-500/50"
                  />
                </FormField>

                <FormField label="Seed Bank" hint="Source or distributor">
                  <input
                    type="text"
                    value={formData.seedBank}
                    onChange={(e) => setFormData({ ...formData, seedBank: e.target.value })}
                    placeholder="e.g., North Atlantic Seed Co., Neptune Seed Bank"
                    className="w-full px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-emerald-500/50"
                  />
                </FormField>

                <FormField label="Cultivation Notes" hint="Specific growing tips and recommendations">
                  <textarea
                    value={formData.cultivationNotes}
                    onChange={(e) => setFormData({ ...formData, cultivationNotes: e.target.value })}
                    placeholder="Growing tips, preferred methods, known issues or sensitivities..."
                    rows={4}
                    className="w-full px-3 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-emerald-500/50 resize-none"
                  />
                </FormField>
              </div>
            )}
          </div>

          {/* Footer */}
          <div className="flex items-center justify-end gap-3 px-6 py-4 border-t border-border bg-muted/10 shrink-0">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-sm font-medium text-muted-foreground hover:text-foreground transition-colors"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={isLoading}
              className={cn(
                "px-6 py-2 rounded-lg text-sm font-medium transition-all",
                isLoading
                  ? "bg-emerald-500/50 text-emerald-200 cursor-not-allowed"
                  : "bg-emerald-500 text-black hover:bg-emerald-400 shadow-lg shadow-emerald-500/20"
              )}
            >
              {isLoading ? 'Saving...' : isEditing ? 'Save Changes' : 'Create Genetics'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default GeneticsModal;


