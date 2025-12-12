'use client';

import React, { useState } from 'react';
import {
  MapPin,
  ChevronLeft,
  Plus,
  Search,
  ChevronRight,
  ChevronDown,
  Warehouse,
  Grid3X3,
  Layers,
  Box,
  QrCode,
  Edit2,
  Package,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import type { LocationType, LocationStatus } from '@/features/inventory/types';
import { LabelPreviewSlideout, PrinterSettings } from '@/features/inventory/components/labels';
import type { LabelTemplate } from '@/features/inventory/services/labels.service';

// Location QR label templates
const LOCATION_LABEL_TEMPLATES: LabelTemplate[] = [
  {
    id: 'loc-tpl-1',
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
    isDefault: true,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
];

const LOCATION_ICONS: Record<LocationType, React.ElementType> = {
  room: Warehouse,
  zone: Grid3X3,
  sub_zone: Layers,
  row: Layers,
  position: MapPin,
  rack: Box,
  shelf: Layers,
  bin: Box,
  vault: Warehouse,
};

const LOCATION_COLORS: Record<LocationType, string> = {
  room: 'text-violet-400 bg-violet-500/10',
  zone: 'text-cyan-400 bg-cyan-500/10',
  sub_zone: 'text-blue-400 bg-blue-500/10',
  row: 'text-indigo-400 bg-indigo-500/10',
  position: 'text-emerald-400 bg-emerald-500/10',
  rack: 'text-amber-400 bg-amber-500/10',
  shelf: 'text-orange-400 bg-orange-500/10',
  bin: 'text-rose-400 bg-rose-500/10',
  vault: 'text-violet-400 bg-violet-500/10',
};

// Mock data with nested structure
const MOCK_LOCATIONS = [
  {
    id: 'vault-a',
    name: 'Vault A',
    code: 'VA',
    locationType: 'vault' as LocationType,
    status: 'active' as LocationStatus,
    capacityPercent: 65,
    lotCount: 42,
    children: [
      {
        id: 'rack-1',
        name: 'Rack 1',
        code: 'R1',
        locationType: 'rack' as LocationType,
        status: 'active' as LocationStatus,
        capacityPercent: 85,
        lotCount: 15,
        children: [
          { id: 'shelf-1a', name: 'Shelf A', code: 'SA', locationType: 'shelf' as LocationType, status: 'active' as LocationStatus, capacityPercent: 100, lotCount: 8, children: [] },
          { id: 'shelf-1b', name: 'Shelf B', code: 'SB', locationType: 'shelf' as LocationType, status: 'active' as LocationStatus, capacityPercent: 70, lotCount: 7, children: [] },
        ],
      },
      {
        id: 'rack-2',
        name: 'Rack 2',
        code: 'R2',
        locationType: 'rack' as LocationType,
        status: 'active' as LocationStatus,
        capacityPercent: 45,
        lotCount: 12,
        children: [],
      },
    ],
  },
  {
    id: 'warehouse-b',
    name: 'Warehouse B',
    code: 'WB',
    locationType: 'room' as LocationType,
    status: 'active' as LocationStatus,
    capacityPercent: 35,
    lotCount: 28,
    children: [
      {
        id: 'zone-1',
        name: 'Zone 1',
        code: 'Z1',
        locationType: 'zone' as LocationType,
        status: 'active' as LocationStatus,
        capacityPercent: 50,
        lotCount: 15,
        children: [],
      },
    ],
  },
];

interface LocationNode {
  id: string;
  name: string;
  code: string;
  locationType: LocationType;
  status: LocationStatus;
  capacityPercent: number;
  lotCount: number;
  children: LocationNode[];
}

function LocationTreeItem({
  node,
  depth,
  expanded,
  onToggle,
  onSelect,
  selected,
}: {
  node: LocationNode;
  depth: number;
  expanded: Set<string>;
  onToggle: (id: string) => void;
  onSelect: (node: LocationNode) => void;
  selected: string | null;
}) {
  const Icon = LOCATION_ICONS[node.locationType];
  const colorClass = LOCATION_COLORS[node.locationType];
  const isExpanded = expanded.has(node.id);
  const hasChildren = node.children.length > 0;

  const getCapacityColor = (percent: number) => {
    if (percent >= 90) return 'bg-rose-500';
    if (percent >= 70) return 'bg-amber-500';
    return 'bg-emerald-500';
  };

  return (
    <div>
      <div
        className={cn(
          'group flex items-center gap-2 py-2.5 px-3 rounded-lg cursor-pointer transition-all',
          selected === node.id
            ? 'bg-amber-500/10 border border-amber-500/20'
            : 'hover:bg-muted/40 border border-transparent'
        )}
        style={{ marginLeft: `${depth * 20}px` }}
        onClick={() => onSelect(node)}
      >
        <button
          onClick={(e) => {
            e.stopPropagation();
            onToggle(node.id);
          }}
          className={cn(
            'w-5 h-5 flex items-center justify-center rounded transition-colors',
            hasChildren ? 'hover:bg-white/10' : 'invisible'
          )}
        >
          {hasChildren && (
            isExpanded
              ? <ChevronDown className="w-3.5 h-3.5 text-muted-foreground" />
              : <ChevronRight className="w-3.5 h-3.5 text-muted-foreground" />
          )}
        </button>

        <div className={cn('w-8 h-8 rounded-lg flex items-center justify-center', colorClass.split(' ')[1])}>
          <Icon className={cn('w-4 h-4', colorClass.split(' ')[0])} />
        </div>

        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2">
            <span className="text-sm font-medium text-foreground truncate">{node.name}</span>
            <span className="text-xs text-muted-foreground font-mono">{node.code}</span>
          </div>
        </div>

        <span className="text-xs text-muted-foreground tabular-nums">
          {node.lotCount} lots
        </span>

        <div className="w-16 h-1.5 bg-white/5 rounded-full overflow-hidden">
          <div
            className={cn('h-full rounded-full', getCapacityColor(node.capacityPercent))}
            style={{ width: `${node.capacityPercent}%` }}
          />
        </div>

        <span className="text-xs text-muted-foreground tabular-nums w-10 text-right">
          {node.capacityPercent}%
        </span>
      </div>

      {hasChildren && isExpanded && (
        <div>
          {node.children.map((child) => (
            <LocationTreeItem
              key={child.id}
              node={child}
              depth={depth + 1}
              expanded={expanded}
              onToggle={onToggle}
              onSelect={onSelect}
              selected={selected}
            />
          ))}
        </div>
      )}
    </div>
  );
}

export default function LocationsPage() {
  const [searchQuery, setSearchQuery] = useState('');
  const [expanded, setExpanded] = useState<Set<string>>(new Set(['vault-a']));
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [selectedNode, setSelectedNode] = useState<LocationNode | null>(null);
  
  // Label preview state
  const [isPreviewOpen, setIsPreviewOpen] = useState(false);
  const [selectedTemplate, setSelectedTemplate] = useState<LabelTemplate | null>(LOCATION_LABEL_TEMPLATES[0]);
  const [isPrinterSettingsOpen, setIsPrinterSettingsOpen] = useState(false);

  const handleToggle = (id: string) => {
    const next = new Set(expanded);
    if (next.has(id)) {
      next.delete(id);
    } else {
      next.add(id);
    }
    setExpanded(next);
  };

  const handleSelect = (node: LocationNode) => {
    setSelectedId(node.id);
    setSelectedNode(node);
  };

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
                <div className="w-10 h-10 rounded-xl bg-cyan-500/10 flex items-center justify-center">
                  <MapPin className="w-5 h-5 text-cyan-400" />
                </div>
                <div>
                  <h1 className="text-xl font-semibold text-foreground">Locations</h1>
                  <p className="text-sm text-muted-foreground">Manage inventory locations and hierarchy</p>
                </div>
              </div>
            </div>

            <div className="flex items-center gap-3">
              <button className="flex items-center gap-2 px-4 py-2 rounded-lg bg-cyan-500/10 text-cyan-400 hover:bg-cyan-500/20 transition-colors">
                <Plus className="w-4 h-4" />
                <span className="text-sm font-medium">Add Location</span>
              </button>
            </div>
          </div>
        </div>
      </header>

      {/* Content */}
      <div className="px-6 py-6">
        <div className="grid grid-cols-12 gap-6">
          {/* Tree View */}
          <div className="col-span-12 lg:col-span-7">
            <div className="bg-surface border border-border rounded-xl overflow-hidden">
              {/* Search */}
              <div className="p-4 border-b border-border">
                <div className="relative">
                  <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
                  <input
                    type="text"
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    placeholder="Search locations..."
                    className="w-full pl-10 pr-4 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-cyan-500/30"
                  />
                </div>
              </div>

              {/* Tree */}
              <div className="p-4 max-h-[600px] overflow-y-auto">
                {MOCK_LOCATIONS.map((node) => (
                  <LocationTreeItem
                    key={node.id}
                    node={node}
                    depth={0}
                    expanded={expanded}
                    onToggle={handleToggle}
                    onSelect={handleSelect}
                    selected={selectedId}
                  />
                ))}
              </div>

              {/* Legend */}
              <div className="px-4 py-3 border-t border-border flex items-center gap-4">
                <span className="text-xs text-muted-foreground">Capacity:</span>
                <div className="flex items-center gap-1.5">
                  <div className="w-2 h-2 rounded-full bg-emerald-500" />
                  <span className="text-[10px] text-muted-foreground">&lt;70%</span>
                </div>
                <div className="flex items-center gap-1.5">
                  <div className="w-2 h-2 rounded-full bg-amber-500" />
                  <span className="text-[10px] text-muted-foreground">70-90%</span>
                </div>
                <div className="flex items-center gap-1.5">
                  <div className="w-2 h-2 rounded-full bg-rose-500" />
                  <span className="text-[10px] text-muted-foreground">&gt;90%</span>
                </div>
              </div>
            </div>
          </div>

          {/* Detail Panel */}
          <div className="col-span-12 lg:col-span-5">
            {selectedNode ? (
              <div className="bg-surface border border-border rounded-xl overflow-hidden sticky top-24">
                <div className="px-5 py-4 border-b border-border flex items-center justify-between">
                  <h3 className="text-sm font-semibold text-foreground">Location Details</h3>
                  <button className="p-1.5 rounded-lg hover:bg-white/5 text-muted-foreground hover:text-foreground">
                    <Edit2 className="w-4 h-4" />
                  </button>
                </div>

                <div className="p-5 space-y-4">
                  {/* Info */}
                  <div className="flex items-center gap-4">
                    <div className={cn(
                      'w-12 h-12 rounded-xl flex items-center justify-center',
                      LOCATION_COLORS[selectedNode.locationType].split(' ')[1]
                    )}>
                      {React.createElement(LOCATION_ICONS[selectedNode.locationType], {
                        className: cn('w-6 h-6', LOCATION_COLORS[selectedNode.locationType].split(' ')[0])
                      })}
                    </div>
                    <div>
                      <h4 className="text-lg font-semibold text-foreground">{selectedNode.name}</h4>
                      <p className="text-sm text-muted-foreground capitalize">{selectedNode.locationType}</p>
                    </div>
                  </div>

                  {/* Stats */}
                  <div className="grid grid-cols-2 gap-3">
                    <div className="p-3 rounded-lg bg-muted/30 border border-border">
                      <div className="text-2xl font-bold text-foreground tabular-nums">{selectedNode.lotCount}</div>
                      <div className="text-xs text-muted-foreground">Lots</div>
                    </div>
                    <div className="p-3 rounded-lg bg-muted/30 border border-border">
                      <div className="text-2xl font-bold text-foreground tabular-nums">{selectedNode.capacityPercent}%</div>
                      <div className="text-xs text-muted-foreground">Capacity Used</div>
                    </div>
                  </div>

                  {/* Capacity Bar */}
                  <div>
                    <div className="flex justify-between text-xs mb-1">
                      <span className="text-muted-foreground">Capacity</span>
                      <span className="text-foreground">{selectedNode.capacityPercent}%</span>
                    </div>
                    <div className="h-2 bg-white/5 rounded-full overflow-hidden">
                      <div
                        className={cn(
                          'h-full rounded-full transition-all',
                          selectedNode.capacityPercent >= 90 ? 'bg-rose-500' :
                          selectedNode.capacityPercent >= 70 ? 'bg-amber-500' : 'bg-emerald-500'
                        )}
                        style={{ width: `${selectedNode.capacityPercent}%` }}
                      />
                    </div>
                  </div>

                  {/* Details */}
                  <div className="space-y-2">
                    <div className="flex justify-between py-2 border-b border-border">
                      <span className="text-sm text-muted-foreground">Code</span>
                      <span className="text-sm font-mono text-foreground">{selectedNode.code}</span>
                    </div>
                    <div className="flex justify-between py-2 border-b border-border">
                      <span className="text-sm text-muted-foreground">Status</span>
                      <span className="text-sm text-emerald-400 capitalize">{selectedNode.status}</span>
                    </div>
                    <div className="flex justify-between py-2 border-b border-border">
                      <span className="text-sm text-muted-foreground">Children</span>
                      <span className="text-sm text-foreground">{selectedNode.children.length}</span>
                    </div>
                  </div>

                  {/* Actions */}
                  <div className="pt-4 space-y-2">
                    <button className="w-full flex items-center justify-center gap-2 py-2.5 rounded-lg bg-white/5 text-foreground text-sm hover:bg-white/10 transition-colors">
                      <Package className="w-4 h-4" />
                      View Lots
                    </button>
                    <button 
                      onClick={() => setIsPreviewOpen(true)}
                      className="w-full flex items-center justify-center gap-2 py-2.5 rounded-lg bg-white/5 text-foreground text-sm hover:bg-white/10 transition-colors"
                    >
                      <QrCode className="w-4 h-4" />
                      Print Label
                    </button>
                  </div>
                </div>
              </div>
            ) : (
              <div className="bg-surface border border-border rounded-xl p-8 text-center">
                <MapPin className="w-12 h-12 text-muted-foreground mx-auto mb-3" />
                <p className="text-sm text-muted-foreground">Select a location to view details</p>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Label Preview Slideout */}
      <LabelPreviewSlideout
        isOpen={isPreviewOpen}
        onClose={() => setIsPreviewOpen(false)}
        template={selectedTemplate}
        availableTemplates={LOCATION_LABEL_TEMPLATES}
        onTemplateChange={(id) => {
          const t = LOCATION_LABEL_TEMPLATES.find(tpl => tpl.id === id);
          if (t) setSelectedTemplate(t);
        }}
        entityData={selectedNode ? {
          lotNumber: selectedNode.code,
          productName: selectedNode.name,
          locationName: selectedNode.name,
        } : null}
        entityType="location"
        onPrint={async () => console.log('Printing location label:', selectedNode?.name)}
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
