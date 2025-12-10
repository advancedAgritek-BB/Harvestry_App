'use client';

import { useState, useEffect } from 'react';

interface UseMobileBreakpointResult {
  isMobile: boolean;
  isDesktop: boolean;
}

/**
 * SSR-safe hook to detect mobile vs desktop viewport.
 * Uses matchMedia for efficient resize detection.
 * Defaults to desktop (false) during SSR to avoid layout shifts.
 * 
 * @param breakpoint - The pixel width threshold (default: 768px, Tailwind's md breakpoint)
 * @returns Object with isMobile and isDesktop states
 */
export function useMobileBreakpoint(breakpoint: number = 768): UseMobileBreakpointResult {
  // Default to desktop (false) during SSR - most landing page visitors are on desktop
  const [isMobile, setIsMobile] = useState<boolean>(false);

  useEffect(() => {
    // Create media query for the breakpoint
    const mediaQuery = window.matchMedia(`(max-width: ${breakpoint - 1}px)`);
    
    // Set initial value based on actual viewport
    setIsMobile(mediaQuery.matches);

    // Handler for media query changes
    const handleChange = (event: MediaQueryListEvent) => {
      setIsMobile(event.matches);
    };

    // Add listener for viewport changes
    mediaQuery.addEventListener('change', handleChange);

    return () => {
      mediaQuery.removeEventListener('change', handleChange);
    };
  }, [breakpoint]);

  return {
    isMobile,
    isDesktop: !isMobile,
  };
}




