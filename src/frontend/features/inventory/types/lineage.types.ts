/**
 * Lineage Graph Type Definitions
 * Types for visualizing product transformations from seed to final good
 */

/** Node type in the lineage graph */
export type LineageNodeType =
  | 'seed'
  | 'clone'
  | 'batch'
  | 'harvest'
  | 'lot'
  | 'production'
  | 'package'
  | 'sale';

/** Relationship type between nodes */
export type LineageEdgeType =
  | 'planted_from'
  | 'cloned_from'
  | 'harvested_from'
  | 'processed_from'
  | 'split_from'
  | 'merged_from'
  | 'packaged_from'
  | 'sold_to';

/** A node in the lineage graph */
export interface LineageNode {
  id: string;
  type: LineageNodeType;
  
  // Display
  label: string;
  sublabel?: string;
  
  // Reference
  entityId: string;
  entityType: 'lot' | 'batch' | 'production_order' | 'harvest' | 'sale';
  
  // Metadata
  quantity?: number;
  uom?: string;
  date: string;
  status?: string;
  
  // Position (for layout)
  generation: number;  // 0 = origin, increases with each transformation
  column?: number;
  x?: number;
  y?: number;
  
  // Visual
  color?: string;
  icon?: string;
  highlighted?: boolean;
  selected?: boolean;
}

/** An edge connecting two nodes */
export interface LineageEdge {
  id: string;
  sourceId: string;
  targetId: string;
  type: LineageEdgeType;
  
  // Transformation details
  quantityIn?: number;
  quantityOut?: number;
  uomIn?: string;
  uomOut?: string;
  conversionRatio?: number;
  
  // Context
  transformationDate?: string;
  productionOrderId?: string;
  notes?: string;
  
  // Visual
  highlighted?: boolean;
  animated?: boolean;
}

/** Complete lineage graph data */
export interface LineageGraph {
  rootNodeId: string;
  nodes: LineageNode[];
  edges: LineageEdge[];
  
  // Metadata
  totalGenerations: number;
  nodeCount: number;
  edgeCount: number;
  
  // Computed paths
  longestPath: string[];
  allPaths: string[][];
}

/** Lineage query options */
export interface LineageQueryOptions {
  lotId?: string;
  batchId?: string;
  productionOrderId?: string;
  direction: 'ancestors' | 'descendants' | 'both';
  maxDepth?: number;
  includeRelatedLots?: boolean;
}

/** Lineage summary for dashboard */
export interface LineageSummary {
  originType: 'seed' | 'clone' | 'purchase';
  originDate: string;
  originLotNumber?: string;
  originBatchNumber?: string;
  
  generationsCount: number;
  totalTransformations: number;
  
  timeline: {
    event: string;
    date: string;
    entityType: string;
    entityId: string;
    quantity?: number;
    uom?: string;
  }[];
}

/** Node type configuration for display */
export const NODE_TYPE_CONFIG: Record<LineageNodeType, {
  label: string;
  color: string;
  bgColor: string;
  icon: string;
}> = {
  seed: {
    label: 'Seed',
    color: 'text-amber-400',
    bgColor: 'bg-amber-500/10',
    icon: 'Sparkles',
  },
  clone: {
    label: 'Clone',
    color: 'text-lime-400',
    bgColor: 'bg-lime-500/10',
    icon: 'Sprout',
  },
  batch: {
    label: 'Batch',
    color: 'text-emerald-400',
    bgColor: 'bg-emerald-500/10',
    icon: 'Leaf',
  },
  harvest: {
    label: 'Harvest',
    color: 'text-orange-400',
    bgColor: 'bg-orange-500/10',
    icon: 'Scissors',
  },
  lot: {
    label: 'Lot',
    color: 'text-cyan-400',
    bgColor: 'bg-cyan-500/10',
    icon: 'Package',
  },
  production: {
    label: 'Production',
    color: 'text-violet-400',
    bgColor: 'bg-violet-500/10',
    icon: 'Factory',
  },
  package: {
    label: 'Package',
    color: 'text-blue-400',
    bgColor: 'bg-blue-500/10',
    icon: 'PackageCheck',
  },
  sale: {
    label: 'Sale',
    color: 'text-rose-400',
    bgColor: 'bg-rose-500/10',
    icon: 'ShoppingCart',
  },
};

/** Edge type configuration for display */
export const EDGE_TYPE_CONFIG: Record<LineageEdgeType, {
  label: string;
  color: string;
  dashed?: boolean;
}> = {
  planted_from: { label: 'Planted from', color: 'stroke-amber-400' },
  cloned_from: { label: 'Cloned from', color: 'stroke-lime-400' },
  harvested_from: { label: 'Harvested from', color: 'stroke-orange-400' },
  processed_from: { label: 'Processed from', color: 'stroke-violet-400' },
  split_from: { label: 'Split from', color: 'stroke-cyan-400', dashed: true },
  merged_from: { label: 'Merged from', color: 'stroke-cyan-400', dashed: true },
  packaged_from: { label: 'Packaged from', color: 'stroke-blue-400' },
  sold_to: { label: 'Sold to', color: 'stroke-rose-400' },
};





