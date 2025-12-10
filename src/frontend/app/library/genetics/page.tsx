'use client';

import React, { useEffect, useState, useCallback } from 'react';
import { Plus, Dna, AlertCircle } from 'lucide-react';
import { useGeneticsStore } from '@/features/genetics/stores';
import { GeneticsList, GeneticsModal } from '@/features/genetics/components';
import type { Genetics, CreateGeneticsRequest, UpdateGeneticsRequest } from '@/features/genetics/types';

/**
 * Genetics Library Page
 * 
 * Main page for browsing, searching, and managing genetics in the library.
 * Route: /library/genetics
 */
export default function GeneticsLibraryPage() {
  // Store
  const {
    genetics,
    geneticsLoading,
    geneticsError,
    viewMode,
    isModalOpen,
    editingGeneticsId,
    setViewMode,
    openModal,
    closeModal,
    loadGenetics,
    createGenetics,
    updateGenetics,
    deleteGenetics,
    getGeneticsById,
    setSiteId,
  } = useGeneticsStore();

  // Local state for submit loading
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Initialize with demo site ID and load genetics
  useEffect(() => {
    // Set a default site ID for demo purposes
    // In production, this would come from auth context or route params
    setSiteId('site-1');
  }, [setSiteId]);

  // Get the genetics being edited
  const editingGenetics = editingGeneticsId 
    ? getGeneticsById(editingGeneticsId) 
    : null;

  // Handle modal submit
  const handleModalSubmit = useCallback(async (data: CreateGeneticsRequest | UpdateGeneticsRequest) => {
    setIsSubmitting(true);
    try {
      if (editingGeneticsId) {
        await updateGenetics(editingGeneticsId, data as UpdateGeneticsRequest);
      } else {
        await createGenetics(data as CreateGeneticsRequest);
      }
      closeModal();
    } catch (error) {
      console.error('Failed to save genetics:', error);
      // In production, show a toast notification
    } finally {
      setIsSubmitting(false);
    }
  }, [editingGeneticsId, updateGenetics, createGenetics, closeModal]);

  // Handle delete
  const handleDelete = useCallback(async (g: Genetics) => {
    if (confirm(`Are you sure you want to delete "${g.name}"? This action cannot be undone.`)) {
      try {
        await deleteGenetics(g.id);
      } catch (error) {
        console.error('Failed to delete genetics:', error);
      }
    }
  }, [deleteGenetics]);

  // Handle edit
  const handleEdit = useCallback((g: Genetics) => {
    openModal(g.id);
  }, [openModal]);

  return (
    <div className="flex flex-col h-full">
      {/* Page Header */}
      <div className="px-6 py-4 border-b border-border bg-surface/30">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-emerald-500/10 flex items-center justify-center ring-1 ring-emerald-500/30">
              <Dna className="w-5 h-5 text-emerald-400" />
            </div>
            <div>
              <h2 className="text-lg font-semibold text-foreground">Genetics Library</h2>
              <p className="text-sm text-muted-foreground">
                {genetics.length} genetics in your library
              </p>
            </div>
          </div>

          {/* Add Button */}
          <button
            onClick={() => openModal()}
            className="flex items-center gap-2 px-4 py-2 rounded-lg bg-emerald-500 text-black font-medium hover:bg-emerald-400 transition-all shadow-lg shadow-emerald-500/20"
          >
            <Plus className="w-4 h-4" />
            Add Genetics
          </button>
        </div>
      </div>

      {/* Content */}
      <div className="flex-1 overflow-auto p-6">
        {/* Error State */}
        {geneticsError && (
          <div className="mb-4 flex items-center gap-3 px-4 py-3 bg-rose-500/10 border border-rose-500/30 rounded-lg text-rose-400">
            <AlertCircle className="w-5 h-5 shrink-0" />
            <div className="flex-1">
              <p className="font-medium">Error loading genetics</p>
              <p className="text-sm opacity-80">{geneticsError}</p>
            </div>
            <button
              onClick={() => loadGenetics()}
              className="px-3 py-1 text-sm bg-rose-500/20 hover:bg-rose-500/30 rounded-lg transition-colors"
            >
              Retry
            </button>
          </div>
        )}

        {/* Genetics List */}
        <GeneticsList
          genetics={genetics}
          isLoading={geneticsLoading}
          viewMode={viewMode}
          onViewModeChange={setViewMode}
          onEdit={handleEdit}
          onDelete={handleDelete}
        />
      </div>

      {/* Modal */}
      <GeneticsModal
        isOpen={isModalOpen}
        onClose={closeModal}
        onSubmit={handleModalSubmit}
        genetics={editingGenetics}
        isLoading={isSubmitting}
      />
    </div>
  );
}


