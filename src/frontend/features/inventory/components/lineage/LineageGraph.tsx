'use client';

import React, { useState, useRef, useEffect, useMemo, useCallback } from 'react';
import {
  Sparkles,
  Sprout,
  Leaf,
  Scissors,
  Package,
  Factory,
  PackageCheck,
  ShoppingCart,
  ZoomIn,
  ZoomOut,
  Maximize2,
  RefreshCw,
  ChevronRight,
  Info,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import {
  NODE_TYPE_CONFIG,
  type LineageNode,
  type LineageEdge,
  type LineageGraph as LineageGraphType,
  type LineageNodeType,
} from '../../types/lineage.types';

// Icon mapping
const NODE_ICONS: Record<LineageNodeType, React.ElementType> = {
  seed: Sparkles,
  clone: Sprout,
  batch: Leaf,
  harvest: Scissors,
  lot: Package,
  production: Factory,
  package: PackageCheck,
  sale: ShoppingCart,
};

// Graph layout constants
const NODE_WIDTH = 180;
const NODE_HEIGHT = 80;
const HORIZONTAL_GAP = 80;
const VERTICAL_GAP = 40;
const PADDING = 60;

interface LineageGraphProps {
  graph: LineageGraphType;
  selectedNodeId?: string;
  onNodeClick?: (node: LineageNode) => void;
  onNodeHover?: (node: LineageNode | null) => void;
  className?: string;
}

// Individual node component
function GraphNode({
  node,
  isSelected,
  isHighlighted,
  onClick,
  onMouseEnter,
  onMouseLeave,
}: {
  node: LineageNode;
  isSelected: boolean;
  isHighlighted: boolean;
  onClick: () => void;
  onMouseEnter: () => void;
  onMouseLeave: () => void;
}) {
  const config = NODE_TYPE_CONFIG[node.type];
  const Icon = NODE_ICONS[node.type];

  return (
    <g
      transform={`translate(${node.x}, ${node.y})`}
      onClick={onClick}
      onMouseEnter={onMouseEnter}
      onMouseLeave={onMouseLeave}
      style={{ cursor: 'pointer' }}
      className="transition-transform duration-200 hover:scale-105"
    >
      {/* Background */}
      <rect
        x={0}
        y={0}
        width={NODE_WIDTH}
        height={NODE_HEIGHT}
        rx={12}
        className={cn(
          'transition-all duration-200',
          isSelected
            ? 'fill-cyan-500/20 stroke-cyan-400 stroke-2'
            : isHighlighted
            ? 'fill-white/10 stroke-white/30 stroke-1'
            : 'fill-surface stroke-border stroke-1 hover:stroke-foreground/20'
        )}
      />

      {/* Icon container */}
      <rect
        x={12}
        y={16}
        width={40}
        height={40}
        rx={8}
        className={config.bgColor.replace('bg-', 'fill-').replace('/10', '/20')}
      />

      {/* Icon - using foreignObject for React icons */}
      <foreignObject x={12} y={16} width={40} height={40}>
        <div className="w-full h-full flex items-center justify-center">
          <Icon className={cn('w-5 h-5', config.color)} />
        </div>
      </foreignObject>

      {/* Label */}
      <foreignObject x={60} y={12} width={NODE_WIDTH - 72} height={24}>
        <div className="text-sm font-medium text-foreground truncate">
          {node.label}
        </div>
      </foreignObject>

      {/* Sublabel */}
      <foreignObject x={60} y={32} width={NODE_WIDTH - 72} height={18}>
        <div className="text-xs text-muted-foreground truncate">
          {node.sublabel}
        </div>
      </foreignObject>

      {/* Quantity badge */}
      {node.quantity && (
        <foreignObject x={60} y={52} width={NODE_WIDTH - 72} height={20}>
          <div className="text-xs font-mono text-cyan-400">
            {node.quantity.toLocaleString()} {node.uom}
          </div>
        </foreignObject>
      )}

      {/* Generation indicator */}
      <circle
        cx={NODE_WIDTH - 16}
        cy={16}
        r={10}
        className="fill-white/5 stroke-white/10"
      />
      <text
        x={NODE_WIDTH - 16}
        y={20}
        textAnchor="middle"
        className="fill-muted-foreground text-[10px]"
      >
        G{node.generation}
      </text>
    </g>
  );
}

// Edge path component
function GraphEdge({
  edge,
  sourceNode,
  targetNode,
  isHighlighted,
}: {
  edge: LineageEdge;
  sourceNode: LineageNode;
  targetNode: LineageNode;
  isHighlighted: boolean;
}) {
  // Calculate path points
  const sourceX = (sourceNode.x || 0) + NODE_WIDTH;
  const sourceY = (sourceNode.y || 0) + NODE_HEIGHT / 2;
  const targetX = targetNode.x || 0;
  const targetY = (targetNode.y || 0) + NODE_HEIGHT / 2;

  // Create curved path
  const midX = (sourceX + targetX) / 2;
  const path = `M ${sourceX} ${sourceY} C ${midX} ${sourceY}, ${midX} ${targetY}, ${targetX} ${targetY}`;

  return (
    <g className="transition-opacity duration-200">
      {/* Shadow/glow for highlighted */}
      {isHighlighted && (
        <path
          d={path}
          fill="none"
          strokeWidth={6}
          className="stroke-cyan-500/30"
        />
      )}

      {/* Main path */}
      <path
        d={path}
        fill="none"
        strokeWidth={isHighlighted ? 2 : 1}
        strokeDasharray={edge.type.includes('split') || edge.type.includes('merge') ? '4 4' : undefined}
        className={cn(
          'transition-all duration-200',
          isHighlighted ? 'stroke-cyan-400' : 'stroke-white/20'
        )}
        markerEnd="url(#arrowhead)"
      />

      {/* Edge label */}
      {isHighlighted && edge.quantityIn && (
        <foreignObject
          x={midX - 40}
          y={(sourceY + targetY) / 2 - 12}
          width={80}
          height={24}
        >
          <div className="text-xs bg-surface border border-border rounded px-2 py-0.5 text-center text-muted-foreground">
            {edge.quantityIn} → {edge.quantityOut || edge.quantityIn}
          </div>
        </foreignObject>
      )}
    </g>
  );
}

// Timeline panel component
function TimelinePanel({
  nodes,
  selectedNodeId,
  onNodeClick,
}: {
  nodes: LineageNode[];
  selectedNodeId?: string;
  onNodeClick: (node: LineageNode) => void;
}) {
  const sortedNodes = [...nodes].sort(
    (a, b) => new Date(a.date).getTime() - new Date(b.date).getTime()
  );

  return (
    <div className="w-64 bg-surface border-l border-border p-4 overflow-y-auto">
      <h3 className="text-sm font-semibold text-foreground mb-4 flex items-center gap-2">
        <Info className="w-4 h-4 text-cyan-400" />
        Lineage Timeline
      </h3>

      <div className="space-y-2">
        {sortedNodes.map((node, index) => {
          const config = NODE_TYPE_CONFIG[node.type];
          const Icon = NODE_ICONS[node.type];
          const isSelected = node.id === selectedNodeId;

          return (
            <button
              key={node.id}
              onClick={() => onNodeClick(node)}
              className={cn(
                'w-full text-left p-3 rounded-lg border transition-all',
                isSelected
                  ? 'bg-cyan-500/10 border-cyan-500/30'
                  : 'bg-muted/30 border-border hover:border-border'
              )}
            >
              <div className="flex items-start gap-2">
                <div className={cn('w-8 h-8 rounded-lg flex items-center justify-center flex-shrink-0', config.bgColor)}>
                  <Icon className={cn('w-4 h-4', config.color)} />
                </div>
                <div className="flex-1 min-w-0">
                  <div className="text-xs text-muted-foreground">
                    {new Date(node.date).toLocaleDateString()}
                  </div>
                  <div className="text-sm font-medium text-foreground truncate">
                    {node.label}
                  </div>
                  {node.quantity && (
                    <div className="text-xs text-cyan-400 font-mono">
                      {node.quantity} {node.uom}
                    </div>
                  )}
                </div>
                <ChevronRight className="w-4 h-4 text-muted-foreground flex-shrink-0" />
              </div>
            </button>
          );
        })}
      </div>
    </div>
  );
}

export function LineageGraph({
  graph,
  selectedNodeId,
  onNodeClick,
  onNodeHover,
  className,
}: LineageGraphProps) {
  const containerRef = useRef<HTMLDivElement>(null);
  const [zoom, setZoom] = useState(1);
  const [pan, setPan] = useState({ x: 0, y: 0 });
  const [isDragging, setIsDragging] = useState(false);
  const [dragStart, setDragStart] = useState({ x: 0, y: 0 });
  const [hoveredNodeId, setHoveredNodeId] = useState<string | null>(null);
  const [showTimeline, setShowTimeline] = useState(true);

  // Calculate node positions
  const layoutedNodes = useMemo(() => {
    const nodesByGeneration: Map<number, LineageNode[]> = new Map();

    // Group nodes by generation
    graph.nodes.forEach((node) => {
      const gen = node.generation;
      if (!nodesByGeneration.has(gen)) {
        nodesByGeneration.set(gen, []);
      }
      nodesByGeneration.get(gen)!.push(node);
    });

    // Position nodes
    const positioned: LineageNode[] = [];

    nodesByGeneration.forEach((nodes, gen) => {
      const totalHeight = nodes.length * NODE_HEIGHT + (nodes.length - 1) * VERTICAL_GAP;
      const startY = (graph.nodes.length * (NODE_HEIGHT + VERTICAL_GAP)) / 2 - totalHeight / 2;

      nodes.forEach((node, index) => {
        positioned.push({
          ...node,
          x: PADDING + gen * (NODE_WIDTH + HORIZONTAL_GAP),
          y: startY + index * (NODE_HEIGHT + VERTICAL_GAP) + PADDING,
        });
      });
    });

    return positioned;
  }, [graph.nodes]);

  // Calculate SVG dimensions
  const svgDimensions = useMemo(() => {
    const maxX = Math.max(...layoutedNodes.map((n) => (n.x || 0) + NODE_WIDTH));
    const maxY = Math.max(...layoutedNodes.map((n) => (n.y || 0) + NODE_HEIGHT));
    return {
      width: maxX + PADDING * 2,
      height: maxY + PADDING * 2,
    };
  }, [layoutedNodes]);

  // Get highlighted path (ancestors and descendants of hovered/selected node)
  const highlightedNodeIds = useMemo(() => {
    const targetId = hoveredNodeId || selectedNodeId;
    if (!targetId) return new Set<string>();

    const highlighted = new Set<string>([targetId]);

    // Find ancestors
    const findAncestors = (nodeId: string) => {
      graph.edges.forEach((edge) => {
        if (edge.targetId === nodeId && !highlighted.has(edge.sourceId)) {
          highlighted.add(edge.sourceId);
          findAncestors(edge.sourceId);
        }
      });
    };

    // Find descendants
    const findDescendants = (nodeId: string) => {
      graph.edges.forEach((edge) => {
        if (edge.sourceId === nodeId && !highlighted.has(edge.targetId)) {
          highlighted.add(edge.targetId);
          findDescendants(edge.targetId);
        }
      });
    };

    findAncestors(targetId);
    findDescendants(targetId);

    return highlighted;
  }, [hoveredNodeId, selectedNodeId, graph.edges]);

  // Zoom controls
  const handleZoomIn = useCallback(() => {
    setZoom((z) => Math.min(z + 0.2, 2));
  }, []);

  const handleZoomOut = useCallback(() => {
    setZoom((z) => Math.max(z - 0.2, 0.5));
  }, []);

  const handleFitView = useCallback(() => {
    setZoom(1);
    setPan({ x: 0, y: 0 });
  }, []);

  // Pan handling
  const handleMouseDown = useCallback((e: React.MouseEvent) => {
    if (e.button === 0) {
      setIsDragging(true);
      setDragStart({ x: e.clientX - pan.x, y: e.clientY - pan.y });
    }
  }, [pan]);

  const handleMouseMove = useCallback((e: React.MouseEvent) => {
    if (isDragging) {
      setPan({
        x: e.clientX - dragStart.x,
        y: e.clientY - dragStart.y,
      });
    }
  }, [isDragging, dragStart]);

  const handleMouseUp = useCallback(() => {
    setIsDragging(false);
  }, []);

  // Node click handler
  const handleNodeClick = useCallback((node: LineageNode) => {
    onNodeClick?.(node);
  }, [onNodeClick]);

  // Node hover handlers
  const handleNodeMouseEnter = useCallback((node: LineageNode) => {
    setHoveredNodeId(node.id);
    onNodeHover?.(node);
  }, [onNodeHover]);

  const handleNodeMouseLeave = useCallback(() => {
    setHoveredNodeId(null);
    onNodeHover?.(null);
  }, [onNodeHover]);

  return (
    <div className={cn('flex h-full bg-background', className)}>
      {/* Main graph area */}
      <div className="flex-1 relative overflow-hidden">
        {/* Controls */}
        <div className="absolute top-4 left-4 z-10 flex items-center gap-2">
          <button
            onClick={handleZoomIn}
            className="p-2 rounded-lg bg-surface border border-border text-foreground hover:bg-white/5 transition-colors"
            title="Zoom in"
          >
            <ZoomIn className="w-4 h-4" />
          </button>
          <button
            onClick={handleZoomOut}
            className="p-2 rounded-lg bg-surface border border-border text-foreground hover:bg-white/5 transition-colors"
            title="Zoom out"
          >
            <ZoomOut className="w-4 h-4" />
          </button>
          <button
            onClick={handleFitView}
            className="p-2 rounded-lg bg-surface border border-border text-foreground hover:bg-white/5 transition-colors"
            title="Fit view"
          >
            <Maximize2 className="w-4 h-4" />
          </button>
          <div className="px-3 py-1.5 rounded-lg bg-surface border border-border text-xs text-muted-foreground">
            {Math.round(zoom * 100)}%
          </div>
        </div>

        {/* Legend */}
        <div className="absolute bottom-4 left-4 z-10 p-3 rounded-lg bg-surface border border-border">
          <div className="text-xs text-muted-foreground mb-2">Legend</div>
          <div className="flex flex-wrap gap-3">
            {Object.entries(NODE_TYPE_CONFIG).slice(0, 5).map(([type, config]) => {
              const Icon = NODE_ICONS[type as LineageNodeType];
              return (
                <div key={type} className="flex items-center gap-1.5">
                  <div className={cn('w-5 h-5 rounded flex items-center justify-center', config.bgColor)}>
                    <Icon className={cn('w-3 h-3', config.color)} />
                  </div>
                  <span className="text-xs text-foreground">{config.label}</span>
                </div>
              );
            })}
          </div>
        </div>

        {/* Timeline toggle */}
        <button
          onClick={() => setShowTimeline(!showTimeline)}
          className="absolute top-4 right-4 z-10 p-2 rounded-lg bg-surface border border-border text-foreground hover:bg-white/5 transition-colors"
        >
          <Info className="w-4 h-4" />
        </button>

        {/* SVG Canvas */}
        <div
          ref={containerRef}
          className="w-full h-full cursor-grab active:cursor-grabbing"
          onMouseDown={handleMouseDown}
          onMouseMove={handleMouseMove}
          onMouseUp={handleMouseUp}
          onMouseLeave={handleMouseUp}
        >
          <svg
            width="100%"
            height="100%"
            viewBox={`0 0 ${svgDimensions.width} ${svgDimensions.height}`}
            style={{
              transform: `scale(${zoom}) translate(${pan.x / zoom}px, ${pan.y / zoom}px)`,
              transformOrigin: 'center center',
            }}
          >
            {/* Definitions */}
            <defs>
              <marker
                id="arrowhead"
                markerWidth="10"
                markerHeight="7"
                refX="9"
                refY="3.5"
                orient="auto"
              >
                <polygon
                  points="0 0, 10 3.5, 0 7"
                  className="fill-white/30"
                />
              </marker>

              {/* Grid pattern */}
              <pattern
                id="grid"
                width="40"
                height="40"
                patternUnits="userSpaceOnUse"
              >
                <path
                  d="M 40 0 L 0 0 0 40"
                  fill="none"
                  stroke="rgba(255,255,255,0.03)"
                  strokeWidth="1"
                />
              </pattern>
            </defs>

            {/* Background grid */}
            <rect width="100%" height="100%" fill="url(#grid)" />

            {/* Edges */}
            <g>
              {graph.edges.map((edge) => {
                const sourceNode = layoutedNodes.find((n) => n.id === edge.sourceId);
                const targetNode = layoutedNodes.find((n) => n.id === edge.targetId);
                if (!sourceNode || !targetNode) return null;

                const isHighlighted =
                  highlightedNodeIds.has(edge.sourceId) &&
                  highlightedNodeIds.has(edge.targetId);

                return (
                  <GraphEdge
                    key={edge.id}
                    edge={edge}
                    sourceNode={sourceNode}
                    targetNode={targetNode}
                    isHighlighted={isHighlighted}
                  />
                );
              })}
            </g>

            {/* Nodes */}
            <g>
              {layoutedNodes.map((node) => (
                <GraphNode
                  key={node.id}
                  node={node}
                  isSelected={node.id === selectedNodeId}
                  isHighlighted={highlightedNodeIds.has(node.id)}
                  onClick={() => handleNodeClick(node)}
                  onMouseEnter={() => handleNodeMouseEnter(node)}
                  onMouseLeave={handleNodeMouseLeave}
                />
              ))}
            </g>
          </svg>
        </div>
      </div>

      {/* Timeline panel */}
      {showTimeline && (
        <TimelinePanel
          nodes={layoutedNodes}
          selectedNodeId={selectedNodeId}
          onNodeClick={handleNodeClick}
        />
      )}
    </div>
  );
}

export default LineageGraph;




