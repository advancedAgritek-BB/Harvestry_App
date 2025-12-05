/**
 * useBlueprints Hook
 * React hook for task blueprint management
 */

import { useState, useCallback, useEffect } from 'react';
import type { TaskBlueprint, CreateTaskBlueprintRequest, UpdateTaskBlueprintRequest } from '../types/blueprint.types';
import * as blueprintService from '../services/blueprint.service';

export interface UseBlueprintsOptions {
  siteId: string;
  autoFetch?: boolean;
  activeOnly?: boolean;
}

export interface UseBlueprintsReturn {
  blueprints: TaskBlueprint[];
  isLoading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
  createBlueprint: (request: CreateTaskBlueprintRequest) => Promise<TaskBlueprint>;
  updateBlueprint: (blueprintId: string, request: UpdateTaskBlueprintRequest) => Promise<TaskBlueprint>;
  activateBlueprint: (blueprintId: string) => Promise<TaskBlueprint>;
  deactivateBlueprint: (blueprintId: string) => Promise<TaskBlueprint>;
  deleteBlueprint: (blueprintId: string) => Promise<void>;
  triggerGeneration: (request: { batchId: string; strainId?: string; phase: string; roomType: string }) => Promise<unknown[]>;
}

export function useBlueprints(options: UseBlueprintsOptions): UseBlueprintsReturn {
  const { siteId, autoFetch = true, activeOnly } = options;
  const [blueprints, setBlueprints] = useState<TaskBlueprint[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const refetch = useCallback(async () => {
    if (!siteId) return;
    
    setIsLoading(true);
    setError(null);
    try {
      const response = await blueprintService.getBlueprints(siteId, activeOnly);
      setBlueprints(response.blueprints);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch blueprints');
    } finally {
      setIsLoading(false);
    }
  }, [siteId, activeOnly]);

  useEffect(() => {
    if (autoFetch && siteId) {
      refetch();
    }
  }, [autoFetch, refetch, siteId]);

  const createBlueprint = useCallback(async (request: CreateTaskBlueprintRequest) => {
    const blueprint = await blueprintService.createBlueprint(siteId, request);
    setBlueprints(prev => [...prev, blueprint]);
    return blueprint;
  }, [siteId]);

  const updateBlueprint = useCallback(async (blueprintId: string, request: UpdateTaskBlueprintRequest) => {
    const blueprint = await blueprintService.updateBlueprint(siteId, blueprintId, request);
    setBlueprints(prev => prev.map(b => b.id === blueprintId ? blueprint : b));
    return blueprint;
  }, [siteId]);

  const activateBlueprint = useCallback(async (blueprintId: string) => {
    const blueprint = await blueprintService.activateBlueprint(siteId, blueprintId);
    setBlueprints(prev => prev.map(b => b.id === blueprintId ? blueprint : b));
    return blueprint;
  }, [siteId]);

  const deactivateBlueprint = useCallback(async (blueprintId: string) => {
    const blueprint = await blueprintService.deactivateBlueprint(siteId, blueprintId);
    setBlueprints(prev => prev.map(b => b.id === blueprintId ? blueprint : b));
    return blueprint;
  }, [siteId]);

  const deleteBlueprint = useCallback(async (blueprintId: string) => {
    await blueprintService.deleteBlueprint(siteId, blueprintId);
    setBlueprints(prev => prev.filter(b => b.id !== blueprintId));
  }, [siteId]);

  const triggerGeneration = useCallback(async (request: { batchId: string; strainId?: string; phase: string; roomType: string }) => {
    return blueprintService.triggerTaskGeneration(siteId, request);
  }, [siteId]);

  return {
    blueprints,
    isLoading,
    error,
    refetch,
    createBlueprint,
    updateBlueprint,
    activateBlueprint,
    deactivateBlueprint,
    deleteBlueprint,
    triggerGeneration,
  };
}

