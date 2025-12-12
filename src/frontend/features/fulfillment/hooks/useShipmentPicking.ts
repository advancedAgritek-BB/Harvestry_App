/**
 * Shipment picking state machine (client-side)
 *
 * Backend persists only shipment-level status and packed timestamps.
 * This hook provides a scanner-first pick/pack UX layer with local state and exceptions.
 */

'use client';

import { useCallback, useMemo, useState } from 'react';
import type { ShipmentPackageDto } from '@/features/sales/types/shipments.types';

export type ShipmentPickExceptionType =
  | 'unknown_label'
  | 'duplicate_scan'
  | 'short_pick'
  | 'damaged'
  | 'manual_override';

export type ShipmentPickException = {
  type: ShipmentPickExceptionType;
  scannedValue: string;
  message: string;
  at: string;
};

export type ShipmentPickingState = {
  scannedPackageIds: Set<string>;
  exceptions: ShipmentPickException[];
};

export function useShipmentPicking(packages: ShipmentPackageDto[]) {
  const [state, setState] = useState<ShipmentPickingState>({
    scannedPackageIds: new Set<string>(),
    exceptions: [],
  });

  const packagesByLabel = useMemo(() => {
    const map = new Map<string, ShipmentPackageDto>();
    packages.forEach((p) => {
      if (p.packageLabel) map.set(p.packageLabel, p);
    });
    return map;
  }, [packages]);

  const packagesById = useMemo(() => {
    const map = new Map<string, ShipmentPackageDto>();
    packages.forEach((p) => map.set(p.packageId, p));
    return map;
  }, [packages]);

  const pickedCount = state.scannedPackageIds.size;
  const totalCount = packages.length;
  const isPickComplete = totalCount > 0 && pickedCount >= totalCount;

  const recordException = useCallback((ex: Omit<ShipmentPickException, 'at'>) => {
    setState((prev) => ({
      ...prev,
      exceptions: [
        ...prev.exceptions,
        { ...ex, at: new Date().toISOString() },
      ],
    }));
  }, []);

  const scan = useCallback(
    (valueRaw: string) => {
      const value = valueRaw.trim();
      if (!value) return;

      // Try label then packageId.
      const pkg =
        packagesByLabel.get(value) ??
        packagesById.get(value) ??
        undefined;

      if (!pkg) {
        recordException({
          type: 'unknown_label',
          scannedValue: value,
          message: `Scanned label not in shipment: ${value}`,
        });
        return;
      }

      setState((prev) => {
        if (prev.scannedPackageIds.has(pkg.packageId)) {
          return prev;
        }
        const next = new Set(prev.scannedPackageIds);
        next.add(pkg.packageId);
        return { ...prev, scannedPackageIds: next };
      });
    },
    [packagesByLabel, packagesById, recordException]
  );

  const clear = useCallback(() => {
    setState({ scannedPackageIds: new Set<string>(), exceptions: [] });
  }, []);

  const markManualOverride = useCallback((note: string) => {
    recordException({
      type: 'manual_override',
      scannedValue: '',
      message: note.trim() || 'Manual override',
    });
  }, [recordException]);

  return {
    scannedPackageIds: state.scannedPackageIds,
    exceptions: state.exceptions,
    pickedCount,
    totalCount,
    isPickComplete,
    scan,
    clear,
    markManualOverride,
  };
}

