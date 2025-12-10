'use client';

import React, { useState, useEffect } from 'react';
import Link from 'next/link';
import { useParams } from 'next/navigation';
import {
  GitBranch,
  ChevronLeft,
  Package,
  Leaf,
  Factory,
  AlertTriangle,
  Download,
  Share2,
  Printer,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { LineageGraph } from '@/features/inventory/components/lineage';
import type { LineageGraph as LineageGraphType, LineageNode } from '@/features/inventory/types/lineage.types';

// Mock lineage data - simulating a complete seed-to-sale journey
const MOCK_LINEAGE_GRAPH: LineageGraphType = {
  rootNodeId: 'seed-001',
  nodes: [
    // Generation 0: Origin
    {
      id: 'seed-001',
      type: 'seed',
      label: 'SEED-BD-001',
      sublabel: 'Blue Dream Seeds',
      entityId: 'lot-seed-001',
      entityType: 'lot',
      quantity: 50,
      uom: 'ea',
      date: '2024-10-01T00:00:00Z',
      status: 'depleted',
      generation: 0,
    },
    // Generation 1: Cultivation Batch
    {
      id: 'batch-001',
      type: 'batch',
      label: 'BATCH-2024-042',
      sublabel: 'Blue Dream #1',
      entityId: 'batch-001',
      entityType: 'batch',
      quantity: 48,
      uom: 'plants',
      date: '2024-10-15T00:00:00Z',
      status: 'complete',
      generation: 1,
    },
    // Generation 2: Harvest
    {
      id: 'harvest-001',
      type: 'harvest',
      label: 'HRV-2024-089',
      sublabel: 'Blue Dream Harvest',
      entityId: 'harvest-001',
      entityType: 'harvest',
      quantity: 48000,
      uom: 'g wet',
      date: '2024-12-20T00:00:00Z',
      status: 'complete',
      generation: 2,
    },
    // Generation 3: Drying/Curing lots
    {
      id: 'lot-dry-001',
      type: 'lot',
      label: 'LOT-2024-1201',
      sublabel: 'Dried Flower',
      entityId: 'lot-001',
      entityType: 'lot',
      quantity: 12000,
      uom: 'g',
      date: '2025-01-05T00:00:00Z',
      status: 'available',
      generation: 3,
    },
    {
      id: 'lot-trim-001',
      type: 'lot',
      label: 'LOT-2024-1202',
      sublabel: 'Trim',
      entityId: 'lot-002',
      entityType: 'lot',
      quantity: 2400,
      uom: 'g',
      date: '2025-01-05T00:00:00Z',
      status: 'available',
      generation: 3,
    },
    // Generation 4: Production - Packaging
    {
      id: 'prod-001',
      type: 'production',
      label: 'PO-2025-0012',
      sublabel: 'Package 3.5g Jars',
      entityId: 'po-001',
      entityType: 'production_order',
      quantity: 100,
      uom: 'ea',
      date: '2025-01-15T00:00:00Z',
      status: 'complete',
      generation: 4,
    },
    {
      id: 'prod-002',
      type: 'production',
      label: 'PO-2025-0013',
      sublabel: 'Roll Pre-Rolls',
      entityId: 'po-002',
      entityType: 'production_order',
      quantity: 200,
      uom: 'ea',
      date: '2025-01-16T00:00:00Z',
      status: 'complete',
      generation: 4,
    },
    {
      id: 'prod-003',
      type: 'production',
      label: 'PO-2025-0018',
      sublabel: 'Extract Distillate',
      entityId: 'po-003',
      entityType: 'production_order',
      quantity: 360,
      uom: 'g',
      date: '2025-01-20T00:00:00Z',
      status: 'complete',
      generation: 4,
    },
    // Generation 5: Finished Goods
    {
      id: 'lot-pkg-001',
      type: 'package',
      label: 'LOT-2025-0101',
      sublabel: 'BD 3.5g Jars',
      entityId: 'lot-pkg-001',
      entityType: 'lot',
      quantity: 98,
      uom: 'ea',
      date: '2025-01-15T00:00:00Z',
      status: 'available',
      generation: 5,
    },
    {
      id: 'lot-pkg-002',
      type: 'package',
      label: 'LOT-2025-0102',
      sublabel: 'BD Pre-Rolls 1g',
      entityId: 'lot-pkg-002',
      entityType: 'lot',
      quantity: 195,
      uom: 'ea',
      date: '2025-01-16T00:00:00Z',
      status: 'available',
      generation: 5,
    },
    {
      id: 'lot-dist-001',
      type: 'lot',
      label: 'LOT-2025-0115',
      sublabel: 'THC Distillate',
      entityId: 'lot-dist-001',
      entityType: 'lot',
      quantity: 350,
      uom: 'g',
      date: '2025-01-20T00:00:00Z',
      status: 'available',
      generation: 5,
    },
    // Generation 6: Sales
    {
      id: 'sale-001',
      type: 'sale',
      label: 'SALE-2025-0088',
      sublabel: 'Dispensary A',
      entityId: 'sale-001',
      entityType: 'sale',
      quantity: 50,
      uom: 'ea',
      date: '2025-01-25T00:00:00Z',
      status: 'complete',
      generation: 6,
    },
  ],
  edges: [
    // Seed → Batch
    {
      id: 'edge-001',
      sourceId: 'seed-001',
      targetId: 'batch-001',
      type: 'planted_from',
      quantityIn: 50,
      quantityOut: 48,
    },
    // Batch → Harvest
    {
      id: 'edge-002',
      sourceId: 'batch-001',
      targetId: 'harvest-001',
      type: 'harvested_from',
      quantityIn: 48,
      quantityOut: 48000,
      uomIn: 'plants',
      uomOut: 'g wet',
    },
    // Harvest → Dried lots
    {
      id: 'edge-003',
      sourceId: 'harvest-001',
      targetId: 'lot-dry-001',
      type: 'processed_from',
      quantityIn: 48000,
      quantityOut: 12000,
    },
    {
      id: 'edge-004',
      sourceId: 'harvest-001',
      targetId: 'lot-trim-001',
      type: 'processed_from',
      quantityIn: 48000,
      quantityOut: 2400,
    },
    // Dried flower → Productions
    {
      id: 'edge-005',
      sourceId: 'lot-dry-001',
      targetId: 'prod-001',
      type: 'processed_from',
      quantityIn: 350,
      quantityOut: 100,
    },
    {
      id: 'edge-006',
      sourceId: 'lot-dry-001',
      targetId: 'prod-002',
      type: 'processed_from',
      quantityIn: 220,
      quantityOut: 200,
    },
    // Trim → Extraction
    {
      id: 'edge-007',
      sourceId: 'lot-trim-001',
      targetId: 'prod-003',
      type: 'processed_from',
      quantityIn: 2400,
      quantityOut: 360,
    },
    // Productions → Finished goods
    {
      id: 'edge-008',
      sourceId: 'prod-001',
      targetId: 'lot-pkg-001',
      type: 'packaged_from',
      quantityIn: 100,
      quantityOut: 98,
    },
    {
      id: 'edge-009',
      sourceId: 'prod-002',
      targetId: 'lot-pkg-002',
      type: 'packaged_from',
      quantityIn: 200,
      quantityOut: 195,
    },
    {
      id: 'edge-010',
      sourceId: 'prod-003',
      targetId: 'lot-dist-001',
      type: 'processed_from',
      quantityIn: 360,
      quantityOut: 350,
    },
    // Finished goods → Sales
    {
      id: 'edge-011',
      sourceId: 'lot-pkg-001',
      targetId: 'sale-001',
      type: 'sold_to',
      quantityIn: 50,
      quantityOut: 50,
    },
  ],
  totalGenerations: 7,
  nodeCount: 12,
  edgeCount: 11,
  longestPath: ['seed-001', 'batch-001', 'harvest-001', 'lot-dry-001', 'prod-001', 'lot-pkg-001', 'sale-001'],
  allPaths: [],
};

// Node detail panel
function NodeDetailPanel({ node }: { node: LineageNode | null }) {
  if (!node) {
    return (
      <div className="p-6 text-center text-muted-foreground">
        <GitBranch className="w-12 h-12 mx-auto mb-3 opacity-50" />
        <p>Select a node to view details</p>
      </div>
    );
  }

  return (
    <div className="p-4 space-y-4">
      <div className="flex items-center gap-3">
        <div className="w-12 h-12 rounded-xl bg-cyan-500/10 flex items-center justify-center">
          {node.type === 'lot' || node.type === 'package' ? (
            <Package className="w-6 h-6 text-cyan-400" />
          ) : node.type === 'batch' ? (
            <Leaf className="w-6 h-6 text-emerald-400" />
          ) : (
            <Factory className="w-6 h-6 text-violet-400" />
          )}
        </div>
        <div>
          <div className="font-mono text-sm text-cyan-400">{node.label}</div>
          <div className="text-sm text-foreground">{node.sublabel}</div>
        </div>
      </div>

      <div className="space-y-2 text-sm">
        <div className="flex justify-between py-2 border-b border-border">
          <span className="text-muted-foreground">Type</span>
          <span className="text-foreground capitalize">{node.type}</span>
        </div>
        <div className="flex justify-between py-2 border-b border-border">
          <span className="text-muted-foreground">Generation</span>
          <span className="text-foreground">G{node.generation}</span>
        </div>
        {node.quantity && (
          <div className="flex justify-between py-2 border-b border-border">
            <span className="text-muted-foreground">Quantity</span>
            <span className="text-foreground font-mono">
              {node.quantity.toLocaleString()} {node.uom}
            </span>
          </div>
        )}
        <div className="flex justify-between py-2 border-b border-border">
          <span className="text-muted-foreground">Date</span>
          <span className="text-foreground">
            {new Date(node.date).toLocaleDateString()}
          </span>
        </div>
        {node.status && (
          <div className="flex justify-between py-2 border-b border-border">
            <span className="text-muted-foreground">Status</span>
            <span className="text-foreground capitalize">{node.status}</span>
          </div>
        )}
      </div>

      <Link
        href={`/inventory/lots/${node.entityId}`}
        className="block w-full text-center py-2 rounded-lg bg-cyan-500/10 text-cyan-400 hover:bg-cyan-500/20 transition-colors text-sm"
      >
        View Full Details →
      </Link>
    </div>
  );
}

export default function LineageViewerPage() {
  const params = useParams();
  const lotId = params.lotId as string;

  const [selectedNode, setSelectedNode] = useState<LineageNode | null>(null);
  const [hoveredNode, setHoveredNode] = useState<LineageNode | null>(null);
  const [loading, setLoading] = useState(true);
  const [graph, setGraph] = useState<LineageGraphType | null>(null);

  // Load lineage data
  useEffect(() => {
    const loadLineage = async () => {
      setLoading(true);
      // Simulate API call
      await new Promise((resolve) => setTimeout(resolve, 500));
      setGraph(MOCK_LINEAGE_GRAPH);
      setLoading(false);
    };

    loadLineage();
  }, [lotId]);

  const handleNodeClick = (node: LineageNode) => {
    setSelectedNode(node);
  };

  const handleNodeHover = (node: LineageNode | null) => {
    setHoveredNode(node);
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin w-8 h-8 border-2 border-cyan-500 border-t-transparent rounded-full mx-auto mb-4" />
          <p className="text-muted-foreground">Loading lineage data...</p>
        </div>
      </div>
    );
  }

  if (!graph) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center">
        <div className="text-center">
          <AlertTriangle className="w-12 h-12 text-amber-400 mx-auto mb-4" />
          <p className="text-foreground mb-2">Lineage data not found</p>
          <Link href="/inventory/lots" className="text-cyan-400 hover:underline">
            ← Back to Lots
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="h-screen flex flex-col bg-background">
      {/* Header */}
      <header className="flex-shrink-0 glass-header">
        <div className="px-6 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <Link
                href="/inventory/lots"
                className="p-2 rounded-lg hover:bg-white/5 text-muted-foreground hover:text-foreground transition-colors"
              >
                <ChevronLeft className="w-5 h-5" />
              </Link>
              <div className="w-10 h-10 rounded-xl bg-violet-500/10 flex items-center justify-center">
                <GitBranch className="w-5 h-5 text-violet-400" />
              </div>
              <div>
                <h1 className="text-xl font-semibold text-foreground">Lineage Viewer</h1>
                <p className="text-sm text-muted-foreground">
                  Trace product from seed to sale • {graph.nodeCount} nodes • {graph.totalGenerations} generations
                </p>
              </div>
            </div>

            {/* Actions */}
            <div className="flex items-center gap-2">
              <button className="flex items-center gap-2 px-3 py-2 rounded-lg bg-white/5 text-foreground hover:bg-white/10 transition-colors">
                <Download className="w-4 h-4" />
                <span className="text-sm">Export</span>
              </button>
              <button className="flex items-center gap-2 px-3 py-2 rounded-lg bg-white/5 text-foreground hover:bg-white/10 transition-colors">
                <Printer className="w-4 h-4" />
                <span className="text-sm">Print</span>
              </button>
              <button className="flex items-center gap-2 px-3 py-2 rounded-lg bg-white/5 text-foreground hover:bg-white/10 transition-colors">
                <Share2 className="w-4 h-4" />
                <span className="text-sm">Share</span>
              </button>
            </div>
          </div>
        </div>
      </header>

      {/* Main content */}
      <div className="flex-1 flex overflow-hidden">
        {/* Graph */}
        <div className="flex-1">
          <LineageGraph
            graph={graph}
            selectedNodeId={selectedNode?.id}
            onNodeClick={handleNodeClick}
            onNodeHover={handleNodeHover}
            className="w-full h-full"
          />
        </div>

        {/* Detail panel */}
        <div className="w-80 bg-surface border-l border-border overflow-y-auto">
          <div className="p-4 border-b border-border">
            <h3 className="text-sm font-semibold text-foreground">Node Details</h3>
          </div>
          <NodeDetailPanel node={selectedNode || hoveredNode} />
        </div>
      </div>
    </div>
  );
}










