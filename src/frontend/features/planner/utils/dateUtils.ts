/**
 * Date Utilities for Planner
 * 
 * Helper functions for date calculations and formatting
 */

import {
  addDays,
  differenceInDays,
  startOfDay,
  endOfDay,
  format,
  isWeekend,
  isSameDay,
  isWithinInterval,
  eachDayOfInterval,
  startOfWeek,
  endOfWeek,
  startOfMonth,
  endOfMonth,
} from 'date-fns';
import { ZoomLevel, DateRange } from '../types/planner.types';
import { TIMELINE_CONFIG } from '../constants/phaseConfig';

/**
 * Get the width in pixels for a day based on zoom level
 */
export function getDayWidth(zoomLevel: ZoomLevel): number {
  return TIMELINE_CONFIG.dayWidth[zoomLevel];
}

/**
 * Calculate the x position of a date within the viewport
 */
export function getDatePosition(date: Date, viewportStart: Date, zoomLevel: ZoomLevel): number {
  const daysDiff = differenceInDays(startOfDay(date), startOfDay(viewportStart));
  return daysDiff * getDayWidth(zoomLevel);
}

/**
 * Calculate the width of a date range in pixels
 */
export function getDateRangeWidth(start: Date, end: Date, zoomLevel: ZoomLevel): number {
  const days = differenceInDays(endOfDay(end), startOfDay(start)) + 1;
  return Math.max(days * getDayWidth(zoomLevel), TIMELINE_CONFIG.minBatchWidth);
}

/**
 * Convert a pixel position to a date
 */
export function positionToDate(x: number, viewportStart: Date, zoomLevel: ZoomLevel): Date {
  const dayWidth = getDayWidth(zoomLevel);
  const days = Math.round(x / dayWidth);
  return addDays(startOfDay(viewportStart), days);
}

/**
 * Snap a date to the nearest day boundary
 */
export function snapToDay(date: Date): Date {
  return startOfDay(date);
}

/**
 * Get all days in a date range
 */
export function getDaysInRange(range: DateRange): Date[] {
  return eachDayOfInterval({
    start: startOfDay(range.start),
    end: startOfDay(range.end),
  });
}

/**
 * Check if a date is within a range
 */
export function isDateInRange(date: Date, range: DateRange): boolean {
  return isWithinInterval(date, {
    start: startOfDay(range.start),
    end: endOfDay(range.end),
  });
}

/**
 * Check if two date ranges overlap
 */
export function doRangesOverlap(
  range1: { start: Date; end: Date },
  range2: { start: Date; end: Date }
): boolean {
  return (
    startOfDay(range1.start) <= endOfDay(range2.end) &&
    startOfDay(range2.start) <= endOfDay(range1.end)
  );
}

/**
 * Format date for display based on zoom level
 */
export function formatDateForZoom(date: Date, zoomLevel: ZoomLevel): string {
  switch (zoomLevel) {
    case 'day':
      return format(date, 'EEE d');
    case 'week':
      return format(date, 'MMM d');
    case 'month':
      return format(date, 'MMM');
    default:
      return format(date, 'MMM d');
  }
}

/**
 * Get header labels for the timeline
 */
export function getTimelineHeaders(range: DateRange, zoomLevel: ZoomLevel): { date: Date; label: string; isWeekend: boolean }[] {
  const days = getDaysInRange(range);
  
  return days.map((date) => ({
    date,
    label: formatDateForZoom(date, zoomLevel),
    isWeekend: isWeekend(date),
  }));
}

/**
 * Get the total width of the timeline in pixels
 */
export function getTimelineWidth(range: DateRange, zoomLevel: ZoomLevel): number {
  const days = differenceInDays(range.end, range.start) + 1;
  return days * getDayWidth(zoomLevel);
}

/**
 * Calculate the date range visible in the viewport
 */
export function getVisibleDateRange(
  scrollLeft: number,
  viewportWidth: number,
  startDate: Date,
  zoomLevel: ZoomLevel
): DateRange {
  const dayWidth = getDayWidth(zoomLevel);
  const startDays = Math.floor(scrollLeft / dayWidth);
  const endDays = Math.ceil((scrollLeft + viewportWidth) / dayWidth);
  
  return {
    start: addDays(startDate, startDays),
    end: addDays(startDate, endDays),
  };
}

/**
 * Calculate today marker position
 */
export function getTodayPosition(viewportStart: Date, zoomLevel: ZoomLevel): number | null {
  const today = startOfDay(new Date());
  const position = getDatePosition(today, viewportStart, zoomLevel);
  return position >= 0 ? position : null;
}

/**
 * Get week boundaries for a date range
 */
export function getWeekBoundaries(range: DateRange): Date[] {
  const boundaries: Date[] = [];
  let current = startOfWeek(range.start, { weekStartsOn: 1 });
  
  while (current <= range.end) {
    if (current >= range.start) {
      boundaries.push(current);
    }
    current = addDays(current, 7);
  }
  
  return boundaries;
}

/**
 * Get month boundaries for a date range
 */
export function getMonthBoundaries(range: DateRange): Date[] {
  const boundaries: Date[] = [];
  let current = startOfMonth(range.start);
  
  while (current <= range.end) {
    if (current >= range.start) {
      boundaries.push(current);
    }
    current = startOfMonth(addDays(endOfMonth(current), 1));
  }
  
  return boundaries;
}

/**
 * Format duration in days to human-readable string
 */
export function formatDuration(days: number): string {
  if (days === 1) return '1 day';
  if (days < 7) return `${days} days`;
  const weeks = Math.floor(days / 7);
  const remainingDays = days % 7;
  if (remainingDays === 0) {
    return weeks === 1 ? '1 week' : `${weeks} weeks`;
  }
  return `${weeks}w ${remainingDays}d`;
}

/**
 * Check if a date is today
 */
export function isToday(date: Date): boolean {
  return isSameDay(date, new Date());
}

