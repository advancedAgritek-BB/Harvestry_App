/**
 * Room Layout Configuration Types
 * Defines the data structures for room layout digital twin configuration
 */

export interface CellAssignment {
  zoneId: string;
  zoneName?: string;
  zoneCode?: string;
  sensorIds?: string[];
  label?: string;
}

export interface RoomLayoutConfig {
  gridRows: number;
  gridCols: number;
  tiers: number;
  cellAssignments: Record<string, CellAssignment>; // key = "tier-row-col" e.g., "1-0-2"
}

export interface RoomWithLayout {
  id: string;
  siteId: string;
  site: string;
  code: string;
  name: string;
  type: string;
  sqft: number;
  zones: number;
  status: string;
  layoutConfig?: RoomLayoutConfig;
}

export interface ZoneOption {
  id: string;
  code: string;
  name: string;
  roomId: string;
}

export interface SensorOption {
  id: string;
  code: string;
  name: string;
  type: string;
  zoneId?: string;
}

/**
 * Generates a cell key from tier, row, and column
 */
export function getCellKey(tier: number, row: number, col: number): string {
  return `${tier}-${row}-${col}`;
}

/**
 * Parses a cell key into tier, row, and column
 */
export function parseCellKey(key: string): { tier: number; row: number; col: number } {
  const [tier, row, col] = key.split('-').map(Number);
  return { tier, row, col };
}

/**
 * Creates a default empty layout config
 */
export function createDefaultLayoutConfig(
  rows: number = 3,
  cols: number = 4,
  tiers: number = 1
): RoomLayoutConfig {
  return {
    gridRows: rows,
    gridCols: cols,
    tiers,
    cellAssignments: {},
  };
}

/**
 * Zone color palette for visual distinction
 */
export const ZONE_COLORS = [
  { bg: 'bg-violet-500/30', border: 'border-violet-500/50', text: 'text-violet-300' },
  { bg: 'bg-cyan-500/30', border: 'border-cyan-500/50', text: 'text-cyan-300' },
  { bg: 'bg-emerald-500/30', border: 'border-emerald-500/50', text: 'text-emerald-300' },
  { bg: 'bg-amber-500/30', border: 'border-amber-500/50', text: 'text-amber-300' },
  { bg: 'bg-rose-500/30', border: 'border-rose-500/50', text: 'text-rose-300' },
  { bg: 'bg-blue-500/30', border: 'border-blue-500/50', text: 'text-blue-300' },
  { bg: 'bg-pink-500/30', border: 'border-pink-500/50', text: 'text-pink-300' },
  { bg: 'bg-indigo-500/30', border: 'border-indigo-500/50', text: 'text-indigo-300' },
];

/**
 * Gets a consistent color for a zone based on its ID
 */
export function getZoneColor(zoneId: string, zoneIndex: number = 0): typeof ZONE_COLORS[0] {
  // Use a simple hash of the zone ID to get a consistent color
  const hash = zoneId.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0);
  return ZONE_COLORS[(hash + zoneIndex) % ZONE_COLORS.length];
}







