'use client';

import { useCallback, useEffect, useRef, useState, RefObject } from 'react';

interface UseChartHorizontalScrollOptions {
  /** Total number of data points */
  dataLength: number;
  /** Initial number of visible points (viewport size). Defaults to showing all data. */
  initialVisibleCount?: number;
  /** Sensitivity of scroll - points to move per wheel delta. Default: 1 */
  scrollSensitivity?: number;
  /** Whether scrolling is enabled. Default: true */
  enabled?: boolean;
}

interface UseChartHorizontalScrollReturn {
  /** Ref to attach to the chart container div */
  containerRef: RefObject<HTMLDivElement>;
  /** Start index of visible data range */
  startIndex: number;
  /** End index of visible data range */
  endIndex: number;
  /** Handler to pass to Recharts Brush onChange */
  onBrushChange: (brushState: { startIndex?: number; endIndex?: number }) => void;
  /** Reset viewport to show all data */
  resetViewport: () => void;
  /** Whether the chart is currently zoomed/panned */
  isPanned: boolean;
  /** Scroll to a specific position (0-1 normalized) */
  scrollTo: (position: number) => void;
}

/**
 * Hook to enable horizontal mouse wheel scrolling on Recharts charts.
 * 
 * Usage:
 * 1. Attach containerRef to the wrapper div around ResponsiveContainer
 * 2. Use startIndex/endIndex to control Brush component
 * 3. Pass onBrushChange to Brush onChange prop
 * 
 * @example
 * ```tsx
 * const { containerRef, startIndex, endIndex, onBrushChange } = useChartHorizontalScroll({
 *   dataLength: data.length,
 *   initialVisibleCount: 20,
 * });
 * 
 * return (
 *   <div ref={containerRef}>
 *     <ResponsiveContainer>
 *       <AreaChart>
 *         <Brush startIndex={startIndex} endIndex={endIndex} onChange={onBrushChange} />
 *       </AreaChart>
 *     </ResponsiveContainer>
 *   </div>
 * );
 * ```
 */
export function useChartHorizontalScroll({
  dataLength,
  initialVisibleCount,
  scrollSensitivity = 1,
  enabled = true,
}: UseChartHorizontalScrollOptions): UseChartHorizontalScrollReturn {
  const containerRef = useRef<HTMLDivElement>(null);
  
  // Default to showing all data
  const effectiveVisibleCount = initialVisibleCount ?? dataLength;
  
  const [startIndex, setStartIndex] = useState(0);
  const [endIndex, setEndIndex] = useState(Math.min(effectiveVisibleCount - 1, dataLength - 1));
  
  // Track the viewport size (can be changed via Brush drag)
  const viewportSize = useRef(effectiveVisibleCount);

  // Track previous data length to detect actual data changes
  const prevDataLength = useRef(dataLength);

  // Update viewport only when data length changes (not when indices change)
  useEffect(() => {
    if (dataLength <= 0) return;
    
    const previousLength = prevDataLength.current;
    const dataLengthChanged = previousLength !== dataLength;
    
    // Only reset if data length actually changed
    if (!dataLengthChanged) return;
    
    prevDataLength.current = dataLength;
    
    // If we have an initialVisibleCount, maintain that viewport size
    if (initialVisibleCount && initialVisibleCount < dataLength) {
      const newViewportSize = Math.min(initialVisibleCount, dataLength);
      viewportSize.current = newViewportSize;
      
      // Keep end at max (show latest data)
      setStartIndex(Math.max(0, dataLength - newViewportSize));
      setEndIndex(dataLength - 1);
    } else if (viewportSize.current >= previousLength || viewportSize.current >= dataLength) {
      // Was showing all data, continue to show all
      viewportSize.current = dataLength;
      setStartIndex(0);
      setEndIndex(dataLength - 1);
    } else {
      // Maintain current viewport size, shift to show latest
      const currentSize = viewportSize.current;
      setStartIndex(Math.max(0, dataLength - currentSize));
      setEndIndex(dataLength - 1);
    }
  }, [dataLength, initialVisibleCount]);

  const handleWheel = useCallback((event: WheelEvent) => {
    if (!enabled || dataLength <= 1) return;
    
    // Use deltaX for horizontal scroll, fall back to deltaY for vertical scroll wheels
    // Negative deltaX = scroll left (show earlier data), Positive = scroll right (show later data)
    const delta = event.deltaX !== 0 ? event.deltaX : event.deltaY;
    
    if (Math.abs(delta) < 1) return;
    
    // Prevent default only if we're actually scrolling the chart
    const scrollAmount = Math.sign(delta) * scrollSensitivity;
    const currentViewportSize = viewportSize.current;
    
    setStartIndex(prev => {
      const newStart = Math.max(0, Math.min(prev + scrollAmount, dataLength - currentViewportSize));
      
      // Only prevent default if we actually moved
      if (newStart !== prev) {
        event.preventDefault();
      }
      
      return newStart;
    });
    
    setEndIndex(prev => {
      const newEnd = Math.max(currentViewportSize - 1, Math.min(prev + scrollAmount, dataLength - 1));
      return newEnd;
    });
  }, [dataLength, enabled, scrollSensitivity]);

  // Attach wheel event listener
  useEffect(() => {
    const container = containerRef.current;
    if (!container || !enabled) return;

    // Use passive: false to allow preventDefault
    container.addEventListener('wheel', handleWheel, { passive: false });
    
    return () => {
      container.removeEventListener('wheel', handleWheel);
    };
  }, [handleWheel, enabled]);

  const onBrushChange = useCallback((brushState: { startIndex?: number; endIndex?: number }) => {
    if (brushState.startIndex !== undefined && brushState.endIndex !== undefined) {
      setStartIndex(brushState.startIndex);
      setEndIndex(brushState.endIndex);
      viewportSize.current = brushState.endIndex - brushState.startIndex + 1;
    }
  }, []);

  const resetViewport = useCallback(() => {
    viewportSize.current = dataLength;
    setStartIndex(0);
    setEndIndex(dataLength - 1);
  }, [dataLength]);

  const scrollTo = useCallback((position: number) => {
    if (dataLength <= 1) return;
    
    const currentViewportSize = viewportSize.current;
    const maxStart = dataLength - currentViewportSize;
    const newStart = Math.round(position * maxStart);
    
    setStartIndex(Math.max(0, Math.min(newStart, maxStart)));
    setEndIndex(Math.max(currentViewportSize - 1, Math.min(newStart + currentViewportSize - 1, dataLength - 1)));
  }, [dataLength]);

  const isPanned = startIndex > 0 || endIndex < dataLength - 1;

  return {
    containerRef,
    startIndex,
    endIndex,
    onBrushChange,
    resetViewport,
    isPanned,
    scrollTo,
  };
}
