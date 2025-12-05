/**
 * useMovements Hook
 * Data fetching and operations for inventory movements
 */

import { useCallback, useEffect, useState } from 'react';
import { useInventoryStore } from '../stores/inventoryStore';
import * as inventoryService from '../services/inventory.service';
import type {
  InventoryMovement,
  MovementFilterOptions,
  CreateMovementRequest,
  BatchMovementRequest,
} from '../types';

interface UseMovementsOptions {
  autoFetch?: boolean;
  pageSize?: number;
  lotId?: string;
  locationId?: string;
}

export function useMovements(options: UseMovementsOptions = {}) {
  const { autoFetch = true, pageSize = 50, lotId, locationId } = options;
  const store = useInventoryStore();
  
  const [movements, setMovements] = useState<InventoryMovement[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [filters, setFilters] = useState<MovementFilterOptions>({
    lotId,
    locationId,
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  const fetchMovements = useCallback(async (
    customFilters?: MovementFilterOptions,
    customPage?: number
  ) => {
    setLoading(true);
    setError(null);
    
    try {
      const response = await inventoryService.getMovements(
        customFilters ?? filters,
        customPage ?? page,
        pageSize
      );
      setMovements(response.items);
      setTotal(response.total);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch movements');
    } finally {
      setLoading(false);
    }
  }, [filters, page, pageSize]);
  
  const fetchSummary = useCallback(async () => {
    store.setMovementsLoading(true);
    try {
      const summary = await inventoryService.getMovementSummary(store.currentSiteId ?? undefined);
      store.setMovementsSummary(summary);
    } catch (err) {
      console.error('Failed to fetch movement summary:', err);
    } finally {
      store.setMovementsLoading(false);
    }
  }, [store]);
  
  const fetchRecentMovements = useCallback(async () => {
    try {
      const response = await inventoryService.getMovements({}, 1, 10);
      store.setRecentMovements(response.items);
    } catch (err) {
      console.error('Failed to fetch recent movements:', err);
    }
  }, [store]);
  
  // Auto-fetch on mount
  useEffect(() => {
    if (autoFetch) {
      fetchMovements();
      fetchSummary();
      fetchRecentMovements();
    }
  }, [autoFetch, fetchMovements, fetchSummary, fetchRecentMovements]);
  
  const updateFilters = useCallback((newFilters: MovementFilterOptions) => {
    setFilters(newFilters);
    setPage(1);
  }, []);
  
  const updatePage = useCallback((newPage: number) => {
    setPage(newPage);
  }, []);
  
  const createMovement = useCallback(async (request: CreateMovementRequest) => {
    const movement = await inventoryService.createMovement(request);
    store.addMovement(movement);
    // Refresh the list
    fetchMovements();
    return movement;
  }, [store, fetchMovements]);
  
  const createBatchMovement = useCallback(async (request: BatchMovementRequest) => {
    const createdMovements = await inventoryService.createBatchMovement(request);
    createdMovements.forEach((m) => store.addMovement(m));
    fetchMovements();
    return createdMovements;
  }, [store, fetchMovements]);
  
  const cancelMovement = useCallback(async (movementId: string) => {
    await inventoryService.cancelMovement(movementId);
    fetchMovements();
  }, [fetchMovements]);
  
  const refresh = useCallback(() => {
    fetchMovements();
    fetchSummary();
    fetchRecentMovements();
  }, [fetchMovements, fetchSummary, fetchRecentMovements]);
  
  return {
    // State
    movements,
    total,
    page,
    filters,
    loading,
    error,
    summary: store.movementsSummary,
    recentMovements: store.recentMovements,
    
    // Actions
    fetchMovements,
    fetchSummary,
    fetchRecentMovements,
    setFilters: updateFilters,
    setPage: updatePage,
    createMovement,
    createBatchMovement,
    cancelMovement,
    refresh,
  };
}

/**
 * useMovement Hook
 * Single movement fetching
 */
export function useMovement(movementId: string | null) {
  const [movement, setMovement] = useState<InventoryMovement | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  const fetchMovement = useCallback(async (id: string) => {
    setLoading(true);
    setError(null);
    try {
      const fetched = await inventoryService.getMovement(id);
      setMovement(fetched);
      return fetched;
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch movement');
      setMovement(null);
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);
  
  useEffect(() => {
    if (movementId) {
      fetchMovement(movementId);
    } else {
      setMovement(null);
    }
  }, [movementId, fetchMovement]);
  
  const refresh = useCallback(() => {
    if (movementId) {
      fetchMovement(movementId);
    }
  }, [movementId, fetchMovement]);
  
  return {
    movement,
    loading,
    error,
    refresh,
  };
}
