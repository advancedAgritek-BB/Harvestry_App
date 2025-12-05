/**
 * useLots Hook
 * Data fetching and operations for inventory lots
 */

import { useCallback, useEffect } from 'react';
import { useInventoryStore, useLotsState } from '../stores/inventoryStore';
import * as inventoryService from '../services/inventory.service';
import type { LotFilterOptions, CreateLotRequest, SplitLotRequest, MergeLotRequest, LotAdjustmentRequest } from '../types';

interface UseLotsOptions {
  autoFetch?: boolean;
  pageSize?: number;
}

export function useLots(options: UseLotsOptions = {}) {
  const { autoFetch = true, pageSize = 50 } = options;
  const state = useLotsState();
  const store = useInventoryStore();
  
  const fetchLots = useCallback(async (
    filters?: LotFilterOptions,
    page?: number
  ) => {
    store.setLotsLoading(true);
    store.setLotsError(null);
    
    try {
      const response = await inventoryService.getLots(
        filters ?? state.filters,
        page ?? state.page,
        pageSize
      );
      store.setLots(response.items, response.total);
    } catch (error) {
      store.setLotsError(error instanceof Error ? error.message : 'Failed to fetch lots');
    } finally {
      store.setLotsLoading(false);
    }
  }, [store, state.filters, state.page, pageSize]);
  
  const fetchSummary = useCallback(async () => {
    try {
      const summary = await inventoryService.getLotSummary(store.currentSiteId ?? undefined);
      store.setLotsSummary(summary);
    } catch (error) {
      console.error('Failed to fetch lot summary:', error);
    }
  }, [store]);
  
  // Auto-fetch on mount and when filters/page change
  useEffect(() => {
    if (autoFetch) {
      fetchLots();
      fetchSummary();
    }
  }, [autoFetch, fetchLots, fetchSummary]);
  
  const setFilters = useCallback((filters: LotFilterOptions) => {
    store.setLotsFilters(filters);
  }, [store]);
  
  const setPage = useCallback((page: number) => {
    store.setLotsPage(page);
  }, [store]);
  
  const createLot = useCallback(async (request: CreateLotRequest) => {
    const lot = await inventoryService.createLot(request);
    store.addLot(lot);
    return lot;
  }, [store]);
  
  const updateLot = useCallback(async (lotId: string, updates: Partial<CreateLotRequest>) => {
    const lot = await inventoryService.updateLot(lotId, updates);
    store.updateLot(lotId, lot);
    return lot;
  }, [store]);
  
  const splitLot = useCallback(async (request: SplitLotRequest) => {
    const response = await inventoryService.splitLot(request);
    // Remove parent lot and add children
    store.removeLot(request.sourceLotId);
    response.childLots.forEach((lot) => store.addLot(lot));
    return response;
  }, [store]);
  
  const mergeLots = useCallback(async (request: MergeLotRequest) => {
    const lot = await inventoryService.mergeLots(request);
    // Remove source lots and add merged lot
    request.sourceLotIds.forEach((id) => store.removeLot(id));
    store.addLot(lot);
    return lot;
  }, [store]);
  
  const adjustLot = useCallback(async (request: LotAdjustmentRequest) => {
    const lot = await inventoryService.adjustLot(request);
    store.updateLot(request.lotId, lot);
    return lot;
  }, [store]);
  
  const selectLot = useCallback((lotId: string, selected: boolean) => {
    store.selectLot(lotId, selected);
  }, [store]);
  
  const selectAll = useCallback((selected: boolean) => {
    store.selectAllLots(selected);
  }, [store]);
  
  const clearSelection = useCallback(() => {
    store.clearLotSelection();
  }, [store]);
  
  const refresh = useCallback(() => {
    fetchLots();
    fetchSummary();
  }, [fetchLots, fetchSummary]);
  
  return {
    // State
    lots: state.lots,
    total: state.total,
    page: state.page,
    filters: state.filters,
    loading: state.loading,
    error: state.error,
    selectedIds: state.selectedIds,
    summary: store.lotsSummary,
    
    // Actions
    fetchLots,
    fetchSummary,
    setFilters,
    setPage,
    createLot,
    updateLot,
    splitLot,
    mergeLots,
    adjustLot,
    selectLot,
    selectAll,
    clearSelection,
    refresh,
  };
}

/**
 * useLot Hook
 * Single lot fetching and operations
 */
export function useLot(lotId: string | null) {
  const store = useInventoryStore();
  const { lot, loading } = useInventoryStore((state) => ({
    lot: state.currentLot,
    loading: state.currentLotLoading,
  }));
  
  const fetchLot = useCallback(async (id: string) => {
    store.setCurrentLotLoading(true);
    try {
      const fetchedLot = await inventoryService.getLot(id);
      store.setCurrentLot(fetchedLot);
      return fetchedLot;
    } catch (error) {
      console.error('Failed to fetch lot:', error);
      store.setCurrentLot(null);
      throw error;
    } finally {
      store.setCurrentLotLoading(false);
    }
  }, [store]);
  
  useEffect(() => {
    if (lotId) {
      fetchLot(lotId);
    } else {
      store.setCurrentLot(null);
    }
    
    return () => {
      store.setCurrentLot(null);
    };
  }, [lotId, fetchLot, store]);
  
  const refresh = useCallback(() => {
    if (lotId) {
      fetchLot(lotId);
    }
  }, [lotId, fetchLot]);
  
  return {
    lot,
    loading,
    refresh,
  };
}
