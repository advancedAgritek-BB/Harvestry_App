'use client';

import React, { useState, useEffect } from 'react';
import Link from 'next/link';
import { formatDistanceToNow, format, differenceInDays } from 'date-fns';
import {
  Leaf,
  Plus,
  Search,
  Filter,
  ChevronDown,
  ChevronLeft,
  Scissors,
  Sprout,
  Flower2,
  Timer,
  Wind,
  CheckCircle,
  Calendar,
  MapPin,
  TrendingUp,
  Sparkles,
  Printer,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import {
  PHASE_CONFIG,
  type CultivationBatch,
  type CultivationPhase,
  type BatchStatus,
} from '@/features/inventory/types';
import { useBatchStore } from '@/stores/batchStore';
import { SEED_BATCHES } from '@/stores/batchSeedData';
import { LabelPreviewSlideout, PrinterSettings } from '@/features/inventory/components/labels';
import type { LabelTemplate } from '@/features/inventory/services/labels.service';

// Batch label templates
const BATCH_LABEL_TEMPLATES: LabelTemplate[] = [
  {
    id: 'batch-tpl-1',
    siteId: 'site-1',
    name: 'Batch Label - Standard',
    jurisdiction: 'ALL',
    labelType: 'batch',
    format: 'zpl',
    barcodeFormat: 'qr',
    barcodePosition: { x: 10, y: 10, width: 80, height: 80 },
    widthInches: 3,
    heightInches: 2,
    fields: [],
    requiredPhrases: [],
    jurisdictionRules: {},
    isActive: true,
    isDefault: true,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
];

// Phase icons
const PHASE_ICONS: Record<CultivationPhase, React.ElementType> = {
  germination: Sparkles,
  propagation: Sprout,
  vegetative: Leaf,
  flowering: Flower2,
  harvest: Scissors,
  drying: Wind,
  curing: Timer,
  complete: CheckCircle,
};

// Phase badge component
function PhaseBadge({ phase }: { phase: CultivationPhase }) {
  const config = PHASE_CONFIG[phase];
  const Icon = PHASE_ICONS[phase];

  return (
    <span className={cn('inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-medium', config.color, config.bgColor)}>
      <Icon className="w-3.5 h-3.5" />
      {config.label}
    </span>
  );
}

// Status badge
function StatusBadge({ status }: { status: BatchStatus }) {
  const config: Record<BatchStatus, { label: string; color: string; bg: string }> = {
    planned: { label: 'Planned', color: 'text-muted-foreground', bg: 'bg-muted/50' },
    active: { label: 'Active', color: 'text-emerald-400', bg: 'bg-emerald-500/10' },
    harvested: { label: 'Harvested', color: 'text-amber-400', bg: 'bg-amber-500/10' },
    processing: { label: 'Processing', color: 'text-violet-400', bg: 'bg-violet-500/10' },
    complete: { label: 'Complete', color: 'text-cyan-400', bg: 'bg-cyan-500/10' },
    destroyed: { label: 'Destroyed', color: 'text-rose-400', bg: 'bg-rose-500/10' },
    cancelled: { label: 'Cancelled', color: 'text-muted-foreground', bg: 'bg-muted/50' },
  };
  const { label, color, bg } = config[status];

  return (
    <span className={cn('px-2 py-0.5 rounded text-xs font-medium', color, bg)}>
      {label}
    </span>
  );
}

// Phase filter tabs
function PhaseTabs({
  selected,
  onChange,
  counts,
}: {
  selected: CultivationPhase | 'all';
  onChange: (phase: CultivationPhase | 'all') => void;
  counts: Record<string, number>;
}) {
  const phases: (CultivationPhase | 'all')[] = [
    'all',
    'propagation',
    'vegetative',
    'flowering',
    'harvest',
    'drying',
    'curing',
  ];

  return (
    <div className="flex items-center gap-1 p-1 bg-muted/30 rounded-lg overflow-x-auto">
      {phases.map((phase) => {
        const isSelected = selected === phase;
        const Icon = phase === 'all' ? Leaf : PHASE_ICONS[phase];
        const label = phase === 'all' ? 'All' : PHASE_CONFIG[phase].label;
        const count = phase === 'all'
          ? Object.values(counts).reduce((a, b) => a + b, 0)
          : counts[phase] || 0;

        return (
          <button
            key={phase}
            onClick={() => onChange(phase)}
            className={cn(
              'flex items-center gap-2 px-3 py-1.5 rounded-md text-sm font-medium transition-all whitespace-nowrap',
              isSelected
                ? 'bg-emerald-500/10 text-emerald-400'
                : 'text-muted-foreground hover:text-foreground hover:bg-white/5'
            )}
          >
            <Icon className="w-4 h-4" />
            <span>{label}</span>
            <span className={cn(
              'px-1.5 py-0.5 rounded text-xs',
              isSelected ? 'bg-emerald-500/20' : 'bg-white/5'
            )}>
              {count}
            </span>
          </button>
        );
      })}
    </div>
  );
}

// Batch card component
function BatchCard({ batch, onClick, onPrintLabel }: { batch: CultivationBatch; onClick: () => void; onPrintLabel: () => void }) {
  const phaseConfig = PHASE_CONFIG[batch.currentPhase];
  const PhaseIcon = PHASE_ICONS[batch.currentPhase];
  const daysToHarvest = batch.expectedHarvestDate
    ? differenceInDays(new Date(batch.expectedHarvestDate), new Date())
    : null;

  return (
    <div
      onClick={onClick}
      className="group p-5 bg-surface border border-border rounded-xl hover:border-emerald-500/30 transition-all cursor-pointer"
    >
      {/* Header */}
      <div className="flex items-start justify-between mb-4">
        <div className="flex items-center gap-3">
          <div className={cn('w-12 h-12 rounded-xl flex items-center justify-center', phaseConfig.bgColor)}>
            <PhaseIcon className={cn('w-6 h-6', phaseConfig.color)} />
          </div>
          <div>
            <div className="font-mono text-sm text-emerald-400">{batch.batchNumber}</div>
            <div className="text-sm font-medium text-foreground">{batch.strainName}</div>
          </div>
        </div>
        <div className="flex flex-col items-end gap-1">
          <PhaseBadge phase={batch.currentPhase} />
          <StatusBadge status={batch.status} />
        </div>
      </div>

      {/* Plant Count & Survival */}
      <div className="mb-4 p-3 bg-muted/30 rounded-lg">
        <div className="flex items-center justify-between text-sm">
          <span className="text-muted-foreground">Plants</span>
          <div className="flex items-center gap-2">
            <span className="text-foreground font-medium">{batch.currentPlantCount}</span>
            <span className="text-muted-foreground">/</span>
            <span className="text-muted-foreground">{batch.initialPlantCount}</span>
            <span className={cn(
              'px-1.5 py-0.5 rounded text-xs',
              batch.survivalRate >= 95 ? 'bg-emerald-500/10 text-emerald-400' :
              batch.survivalRate >= 85 ? 'bg-amber-500/10 text-amber-400' :
              'bg-rose-500/10 text-rose-400'
            )}>
              {batch.survivalRate}%
            </span>
          </div>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-3 gap-2 text-xs mb-4">
        <div className="p-2 bg-muted/30 rounded text-center">
          <Calendar className="w-3.5 h-3.5 mx-auto mb-1 text-muted-foreground" />
          <div className="text-muted-foreground">Started</div>
          <div className="text-foreground">{format(new Date(batch.startDate), 'MMM d')}</div>
        </div>
        <div className="p-2 bg-muted/30 rounded text-center">
          <MapPin className="w-3.5 h-3.5 mx-auto mb-1 text-muted-foreground" />
          <div className="text-muted-foreground">Room</div>
          <div className="text-foreground truncate">{batch.currentRoomName}</div>
        </div>
        <div className="p-2 bg-muted/30 rounded text-center">
          <TrendingUp className="w-3.5 h-3.5 mx-auto mb-1 text-muted-foreground" />
          <div className="text-muted-foreground">Gen</div>
          <div className="text-foreground">#{batch.generationNumber}</div>
        </div>
      </div>

      {/* Harvest countdown or yield */}
      {batch.status === 'active' && daysToHarvest !== null && (
        <div className="mb-4 p-3 bg-emerald-500/5 border border-emerald-500/10 rounded-lg">
          <div className="flex items-center justify-between text-sm">
            <span className="text-emerald-400">Expected Harvest</span>
            <span className="text-foreground font-medium">
              {daysToHarvest > 0 ? `${daysToHarvest} days` : 'Ready'}
            </span>
          </div>
          {batch.projectedYieldGrams && (
            <div className="flex items-center justify-between text-xs mt-1">
              <span className="text-muted-foreground">Projected Yield</span>
              <span className="text-foreground">
                {(batch.projectedYieldGrams / 1000).toFixed(1)} kg
              </span>
            </div>
          )}
        </div>
      )}

      {/* Harvested info */}
      {(batch.status === 'harvested' || batch.status === 'processing') && batch.actualWetWeightGrams && (
        <div className="mb-4 p-3 bg-amber-500/5 border border-amber-500/10 rounded-lg">
          <div className="flex items-center justify-between text-sm">
            <span className="text-amber-400">Wet Weight</span>
            <span className="text-foreground font-medium">
              {(batch.actualWetWeightGrams / 1000).toFixed(1)} kg
            </span>
          </div>
          {batch.outputLotIds.length > 0 && (
            <div className="flex items-center justify-between text-xs mt-1">
              <span className="text-muted-foreground">Output Lots</span>
              <span className="text-foreground">{batch.outputLotIds.length}</span>
            </div>
          )}
        </div>
      )}

      {/* Footer */}
      <div className="flex items-center justify-between pt-3 border-t border-border">
        <span className="text-xs text-muted-foreground capitalize">
          {batch.originType}
        </span>
        <div className="flex items-center gap-2">
          <button
            onClick={(e) => {
              e.stopPropagation();
              onPrintLabel();
            }}
            title="Print Label"
            className="p-1.5 rounded-md hover:bg-muted text-muted-foreground hover:text-cyan-400 transition-colors"
          >
            <Printer className="w-4 h-4" />
          </button>
          <span className="text-xs text-muted-foreground">
            Updated {formatDistanceToNow(new Date(batch.updatedAt), { addSuffix: true })}
          </span>
        </div>
      </div>
    </div>
  );
}

// KPI card
function KPICard({
  label,
  value,
  subValue,
  icon: Icon,
  accent,
}: {
  label: string;
  value: string | number;
  subValue?: string;
  icon: React.ElementType;
  accent: string;
}) {
  return (
    <div className="p-4 bg-surface border border-border rounded-xl">
      <div className="flex items-start justify-between">
        <div>
          <div className="text-xs text-muted-foreground mb-1">{label}</div>
          <div className="text-2xl font-bold text-foreground">{value}</div>
          {subValue && <div className="text-xs text-muted-foreground mt-1">{subValue}</div>}
        </div>
        <div className={cn('w-10 h-10 rounded-lg flex items-center justify-center', `bg-${accent}-500/10`)}>
          <Icon className={cn('w-5 h-5', `text-${accent}-400`)} />
        </div>
      </div>
    </div>
  );
}

export default function CultivationBatchesPage() {
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedPhase, setSelectedPhase] = useState<CultivationPhase | 'all'>('all');
  
  // Label preview state
  const [isPreviewOpen, setIsPreviewOpen] = useState(false);
  const [previewBatch, setPreviewBatch] = useState<CultivationBatch | null>(null);
  const [selectedTemplate, setSelectedTemplate] = useState<LabelTemplate | null>(BATCH_LABEL_TEMPLATES[0]);
  const [isPrinterSettingsOpen, setIsPrinterSettingsOpen] = useState(false);
  
  // Use shared batch store (single source of truth)
  const { batches, setBatches } = useBatchStore();

  // Initialize with seed data if empty
  useEffect(() => {
    if (batches.length === 0) {
      setBatches(SEED_BATCHES);
    }
  }, [batches.length, setBatches]);

  // Calculate phase counts
  const phaseCounts = batches.reduce((acc, batch) => {
    acc[batch.currentPhase] = (acc[batch.currentPhase] || 0) + 1;
    return acc;
  }, {} as Record<string, number>);

  // Filter batches
  let filteredBatches = batches;

  if (selectedPhase !== 'all') {
    filteredBatches = filteredBatches.filter((b) => b.currentPhase === selectedPhase);
  }

  if (searchQuery) {
    const query = searchQuery.toLowerCase();
    filteredBatches = filteredBatches.filter(
      (b) =>
        b.batchNumber.toLowerCase().includes(query) ||
        b.strainName.toLowerCase().includes(query) ||
        b.currentRoomName.toLowerCase().includes(query)
    );
  }

  // Calculate KPIs
  const activeBatches = batches.filter((b) => b.status === 'active').length;
  const totalPlants = batches.reduce((sum, b) => sum + b.currentPlantCount, 0);
  const avgSurvival = batches.length > 0
    ? (batches.reduce((sum, b) => sum + b.survivalRate, 0) / batches.length).toFixed(1)
    : 0;
  const upcomingHarvests = batches.filter((b) => {
    if (b.status !== 'active' || !b.expectedHarvestDate) return false;
    const days = differenceInDays(new Date(b.expectedHarvestDate), new Date());
    return days >= 0 && days <= 14;
  }).length;

  return (
    <div className="min-h-screen bg-background">
      {/* Header */}
      <header className="sticky top-0 z-40 glass-header">
        <div className="px-6 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <Link
                href="/inventory"
                className="p-2 rounded-lg hover:bg-white/5 text-muted-foreground hover:text-foreground transition-colors"
              >
                <ChevronLeft className="w-5 h-5" />
              </Link>
              <div className="w-10 h-10 rounded-xl bg-lime-500/10 flex items-center justify-center">
                <Leaf className="w-5 h-5 text-lime-400" />
              </div>
              <div>
                <h1 className="text-xl font-semibold text-foreground">Cultivation Batches</h1>
                <p className="text-sm text-muted-foreground">
                  Track plants from seed to harvest
                </p>
              </div>
            </div>

            {/* Actions */}
            <div className="flex items-center gap-3">
              <Link
                href="/inventory/batches/new"
                className="flex items-center gap-2 px-4 py-2 rounded-lg bg-emerald-500 text-black font-medium hover:bg-emerald-400 transition-colors"
              >
                <Plus className="w-4 h-4" />
                <span className="text-sm">New Batch</span>
              </Link>
            </div>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="px-6 py-6 space-y-6">
        {/* KPI Strip */}
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
          <KPICard
            label="Active Batches"
            value={activeBatches}
            subValue="In cultivation"
            icon={Leaf}
            accent="emerald"
          />
          <KPICard
            label="Total Plants"
            value={totalPlants.toLocaleString()}
            subValue="Across all batches"
            icon={Sprout}
            accent="lime"
          />
          <KPICard
            label="Avg Survival"
            value={`${avgSurvival}%`}
            subValue="Plant survival rate"
            icon={TrendingUp}
            accent="cyan"
          />
          <KPICard
            label="Upcoming Harvests"
            value={upcomingHarvests}
            subValue="Within 14 days"
            icon={Scissors}
            accent="amber"
          />
        </div>

        {/* Phase Tabs */}
        <PhaseTabs
          selected={selectedPhase}
          onChange={setSelectedPhase}
          counts={phaseCounts}
        />

        {/* Toolbar */}
        <div className="flex items-center justify-between gap-4">
          {/* Search */}
          <div className="relative flex-1 max-w-md">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
            <input
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              placeholder="Search batches by number, strain, or room..."
              className="w-full pl-10 pr-4 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-emerald-500/30"
            />
          </div>

          {/* Filter */}
          <button className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-muted/30 text-muted-foreground hover:text-foreground transition-colors">
            <Filter className="w-3.5 h-3.5" />
            <span className="text-xs">Filter</span>
            <ChevronDown className="w-3 h-3" />
          </button>
        </div>

        {/* Results Count */}
        <div className="text-sm text-muted-foreground">
          Showing {filteredBatches.length} batches
        </div>

        {/* Batch Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
          {filteredBatches.map((batch) => (
            <BatchCard
              key={batch.id}
              batch={batch}
              onClick={() => {
                window.location.href = `/inventory/batches/${batch.id}`;
              }}
              onPrintLabel={() => {
                setPreviewBatch(batch);
                setIsPreviewOpen(true);
              }}
            />
          ))}
        </div>

        {filteredBatches.length === 0 && (
          <div className="text-center py-12">
            <Leaf className="w-12 h-12 text-muted-foreground mx-auto mb-3" />
            <p className="text-muted-foreground">No batches found</p>
            <Link
              href="/inventory/batches/new"
              className="mt-4 inline-flex items-center gap-2 px-4 py-2 rounded-lg bg-emerald-500/10 text-emerald-400 hover:bg-emerald-500/20 transition-colors"
            >
              <Plus className="w-4 h-4" />
              Start New Batch
            </Link>
          </div>
        )}
      </main>

      {/* Label Preview Slideout */}
      <LabelPreviewSlideout
        isOpen={isPreviewOpen}
        onClose={() => setIsPreviewOpen(false)}
        template={selectedTemplate}
        availableTemplates={BATCH_LABEL_TEMPLATES}
        onTemplateChange={(id) => {
          const t = BATCH_LABEL_TEMPLATES.find(tpl => tpl.id === id);
          if (t) setSelectedTemplate(t);
        }}
        entityData={previewBatch ? {
          lotNumber: previewBatch.batchNumber,
          productName: previewBatch.strainName,
          strainName: previewBatch.strainName,
          batchName: previewBatch.batchNumber,
        } : null}
        entityType="batch"
        onPrint={async () => console.log('Printing batch label:', previewBatch?.batchNumber)}
        onDownload={async (format) => console.log('Downloading as:', format)}
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

