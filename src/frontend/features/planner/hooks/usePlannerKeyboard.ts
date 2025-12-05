/**
 * Planner Keyboard Shortcuts Hook
 * 
 * Handles keyboard navigation and shortcuts for the Gantt planner
 */

import { useEffect, useCallback } from 'react';
import { usePlannerStore } from '../stores/plannerStore';

interface KeyboardShortcut {
  key: string;
  ctrl?: boolean;
  shift?: boolean;
  alt?: boolean;
  action: () => void;
  description: string;
}

export function usePlannerKeyboard() {
  const {
    selectedBatchId,
    selectedPhaseId,
    batches,
    settings,
    selectBatch,
    selectPhase,
    navigateToToday,
    navigateBy,
    setZoomLevel,
    updateSettings,
    deleteBatch,
    duplicateBatch,
    dragState,
    endDrag,
  } = usePlannerStore();

  // Get currently selected batch
  const selectedBatch = batches.find((b) => b.id === selectedBatchId);

  // Navigate to next/previous batch
  const selectNextBatch = useCallback(() => {
    if (!selectedBatchId) {
      if (batches.length > 0) {
        selectBatch(batches[0].id);
      }
      return;
    }
    const currentIndex = batches.findIndex((b) => b.id === selectedBatchId);
    if (currentIndex < batches.length - 1) {
      selectBatch(batches[currentIndex + 1].id);
    }
  }, [selectedBatchId, batches, selectBatch]);

  const selectPrevBatch = useCallback(() => {
    if (!selectedBatchId) {
      if (batches.length > 0) {
        selectBatch(batches[batches.length - 1].id);
      }
      return;
    }
    const currentIndex = batches.findIndex((b) => b.id === selectedBatchId);
    if (currentIndex > 0) {
      selectBatch(batches[currentIndex - 1].id);
    }
  }, [selectedBatchId, batches, selectBatch]);

  // Navigate phases within a batch
  const selectNextPhase = useCallback(() => {
    if (!selectedBatch) return;
    
    const phases = selectedBatch.phases;
    if (!selectedPhaseId) {
      if (phases.length > 0) {
        selectPhase(selectedBatch.id, phases[0].id);
      }
      return;
    }
    
    const currentIndex = phases.findIndex((p) => p.id === selectedPhaseId);
    if (currentIndex < phases.length - 1) {
      selectPhase(selectedBatch.id, phases[currentIndex + 1].id);
    }
  }, [selectedBatch, selectedPhaseId, selectPhase]);

  const selectPrevPhase = useCallback(() => {
    if (!selectedBatch) return;
    
    const phases = selectedBatch.phases;
    if (!selectedPhaseId) {
      if (phases.length > 0) {
        selectPhase(selectedBatch.id, phases[phases.length - 1].id);
      }
      return;
    }
    
    const currentIndex = phases.findIndex((p) => p.id === selectedPhaseId);
    if (currentIndex > 0) {
      selectPhase(selectedBatch.id, phases[currentIndex - 1].id);
    }
  }, [selectedBatch, selectedPhaseId, selectPhase]);

  // Clear selection
  const clearSelection = useCallback(() => {
    selectBatch(null);
  }, [selectBatch]);

  // Define shortcuts
  const shortcuts: KeyboardShortcut[] = [
    // Navigation
    { key: 't', action: navigateToToday, description: 'Go to today' },
    { key: 'ArrowLeft', alt: true, action: () => navigateBy(-7), description: 'Navigate back' },
    { key: 'ArrowRight', alt: true, action: () => navigateBy(7), description: 'Navigate forward' },
    
    // Zoom
    { key: '1', action: () => setZoomLevel('day'), description: 'Day view' },
    { key: '2', action: () => setZoomLevel('week'), description: 'Week view' },
    { key: '3', action: () => setZoomLevel('month'), description: 'Month view' },
    
    // Selection
    { key: 'ArrowDown', action: selectNextBatch, description: 'Select next batch' },
    { key: 'ArrowUp', action: selectPrevBatch, description: 'Select previous batch' },
    { key: 'ArrowRight', action: selectNextPhase, description: 'Select next phase' },
    { key: 'ArrowLeft', action: selectPrevPhase, description: 'Select previous phase' },
    { key: 'Escape', action: clearSelection, description: 'Clear selection' },
    
    // Actions
    { 
      key: 'd', 
      ctrl: true, 
      action: () => {
        if (selectedBatchId) duplicateBatch(selectedBatchId);
      }, 
      description: 'Duplicate batch' 
    },
    { 
      key: 'Delete', 
      action: () => {
        if (selectedBatchId) deleteBatch(selectedBatchId);
      }, 
      description: 'Delete batch' 
    },
    { 
      key: 'Backspace', 
      action: () => {
        if (selectedBatchId) deleteBatch(selectedBatchId);
      }, 
      description: 'Delete batch' 
    },
    
    // View toggles
    { 
      key: 'c', 
      action: () => updateSettings({ showCapacityLanes: !settings.showCapacityLanes }), 
      description: 'Toggle capacity lanes' 
    },
    { 
      key: 'w', 
      action: () => updateSettings({ whatIfMode: !settings.whatIfMode }), 
      description: 'Toggle what-if mode' 
    },
  ];

  // Handle keyboard events
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      // Don't handle shortcuts when typing in inputs
      if (
        e.target instanceof HTMLInputElement ||
        e.target instanceof HTMLTextAreaElement
      ) {
        return;
      }

      // Cancel drag on Escape
      if (e.key === 'Escape' && dragState.isDragging) {
        endDrag(false);
        return;
      }

      // Find matching shortcut
      const matchingShortcut = shortcuts.find((s) => {
        if (s.key !== e.key) return false;
        if (s.ctrl && !e.ctrlKey && !e.metaKey) return false;
        if (s.shift && !e.shiftKey) return false;
        if (s.alt && !e.altKey) return false;
        if (!s.ctrl && (e.ctrlKey || e.metaKey)) return false;
        if (!s.shift && e.shiftKey) return false;
        if (!s.alt && e.altKey && s.key.startsWith('Arrow')) return false;
        return true;
      });

      if (matchingShortcut) {
        e.preventDefault();
        matchingShortcut.action();
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [shortcuts, dragState.isDragging, endDrag]);

  return {
    shortcuts,
  };
}

/**
 * Keyboard shortcuts help component data
 */
export const KEYBOARD_SHORTCUTS_HELP = [
  { category: 'Navigation', shortcuts: [
    { keys: ['T'], description: 'Go to today' },
    { keys: ['Alt', '←/→'], description: 'Navigate timeline' },
    { keys: ['↑/↓'], description: 'Select batch' },
    { keys: ['←/→'], description: 'Select phase' },
  ]},
  { category: 'Zoom', shortcuts: [
    { keys: ['1'], description: 'Day view' },
    { keys: ['2'], description: 'Week view' },
    { keys: ['3'], description: 'Month view' },
  ]},
  { category: 'Actions', shortcuts: [
    { keys: ['Ctrl', 'D'], description: 'Duplicate batch' },
    { keys: ['Delete'], description: 'Delete batch' },
    { keys: ['Esc'], description: 'Clear selection / Cancel' },
  ]},
  { category: 'View', shortcuts: [
    { keys: ['C'], description: 'Toggle capacity lanes' },
    { keys: ['W'], description: 'Toggle what-if mode' },
  ]},
];

