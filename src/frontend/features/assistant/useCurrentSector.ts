'use client';

import { usePathname } from 'next/navigation';
import { useMemo } from 'react';
import { AssistantSector } from './sectorConfig';

/**
 * Route patterns mapped to their corresponding sectors.
 * Order matters - more specific routes should come first.
 */
const ROUTE_TO_SECTOR: Array<{ pattern: RegExp; sector: AssistantSector }> = [
  // Specific sub-routes first
  { pattern: /^\/dashboard\/recipes/, sector: 'recipes' },
  { pattern: /^\/dashboard\/irrigation/, sector: 'irrigation' },
  { pattern: /^\/dashboard\/cultivation/, sector: 'cultivation' },
  { pattern: /^\/dashboard\/analytics/, sector: 'analytics' },
  { pattern: /^\/dashboard\/planner/, sector: 'planner' },
  { pattern: /^\/dashboard\/tasks/, sector: 'tasks' },
  { pattern: /^\/dashboard\/overview/, sector: 'overview' },
  { pattern: /^\/dashboard\/?$/, sector: 'overview' },
  
  // Top-level routes
  { pattern: /^\/library/, sector: 'library' },
  { pattern: /^\/inventory/, sector: 'inventory' },
  { pattern: /^\/admin/, sector: 'admin' },
];

/**
 * Hook to determine the current sector based on the active route.
 * Returns sector identifier for assistant context and UI customization.
 */
export function useCurrentSector(): AssistantSector {
  const pathname = usePathname();

  const sector = useMemo(() => {
    for (const { pattern, sector } of ROUTE_TO_SECTOR) {
      if (pattern.test(pathname)) {
        return sector;
      }
    }
    return 'general';
  }, [pathname]);

  return sector;
}

/**
 * Get sector from pathname string (for server-side or non-hook usage)
 */
export function getSectorFromPath(pathname: string): AssistantSector {
  for (const { pattern, sector } of ROUTE_TO_SECTOR) {
    if (pattern.test(pathname)) {
      return sector;
    }
  }
  return 'general';
}



