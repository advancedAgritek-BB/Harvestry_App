/**
 * Hook for accessing task recommendations from blueprints
 */

import { useState, useEffect, useCallback } from 'react';
import { getTaskRecommendationService } from '../services/taskRecommendation.service';
import { TaskRecommendationSummary } from '../types/task.types';

interface UseTaskRecommendationsOptions {
  daysAhead?: number;
  refreshIntervalMs?: number;
}

interface UseTaskRecommendationsResult {
  summary: TaskRecommendationSummary | null;
  isLoading: boolean;
  error: Error | null;
  refresh: () => void;
}

export function useTaskRecommendations(
  options: UseTaskRecommendationsOptions = {}
): UseTaskRecommendationsResult {
  const { daysAhead = 2, refreshIntervalMs = 60000 } = options;
  
  const [summary, setSummary] = useState<TaskRecommendationSummary | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  const fetchRecommendations = useCallback(() => {
    try {
      setIsLoading(true);
      const service = getTaskRecommendationService();
      const result = service.getRecommendations(daysAhead);
      setSummary(result);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Failed to fetch recommendations'));
    } finally {
      setIsLoading(false);
    }
  }, [daysAhead]);

  useEffect(() => {
    fetchRecommendations();
    
    // Set up refresh interval
    const intervalId = setInterval(fetchRecommendations, refreshIntervalMs);
    
    return () => clearInterval(intervalId);
  }, [fetchRecommendations, refreshIntervalMs]);

  return {
    summary,
    isLoading,
    error,
    refresh: fetchRecommendations,
  };
}

