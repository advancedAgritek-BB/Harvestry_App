'use client';

import { useState, useCallback } from 'react';

/**
 * Hook to manage demo mode state for Sales CRM pages.
 * Enables demo mode when backend is unavailable (403/network errors).
 */
export function useDemoMode() {
  const [isDemoMode, setIsDemoMode] = useState(false);

  const enableDemoMode = useCallback(() => {
    setIsDemoMode(true);
  }, []);

  const disableDemoMode = useCallback(() => {
    setIsDemoMode(false);
  }, []);

  /**
   * Check if an error indicates backend unavailability.
   */
  const isBackendUnavailableError = useCallback((error: unknown): boolean => {
    if (error instanceof Error) {
      const message = error.message.toLowerCase();
      return (
        message.includes('403') ||
        message.includes('forbidden') ||
        message.includes('failed to fetch') ||
        message.includes('network error') ||
        message.includes('econnrefused')
      );
    }
    return false;
  }, []);

  /**
   * Handle an API error - enable demo mode if backend is unavailable.
   * Returns true if demo mode was enabled.
   */
  const handleApiError = useCallback(
    (error: unknown): boolean => {
      if (isBackendUnavailableError(error)) {
        enableDemoMode();
        return true;
      }
      return false;
    },
    [isBackendUnavailableError, enableDemoMode]
  );

  return {
    isDemoMode,
    enableDemoMode,
    disableDemoMode,
    handleApiError,
    isBackendUnavailableError,
  };
}
