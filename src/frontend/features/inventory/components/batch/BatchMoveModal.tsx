'use client';

import React, { useState, useCallback } from 'react';
import { Package, MapPin, ArrowRight, Loader2, X, CheckCircle, Check } from 'lucide-react';
import { cn } from '@/lib/utils';

interface PackageToMove {
  id: string;
  label: string;
  itemName: string;
  quantity: number;
  unitOfMeasure: string;
  currentLocation?: string;
}

interface BatchMoveModalProps {
  isOpen: boolean;
  onClose: () => void;
  packages: PackageToMove[];
  onMove: (packageIds: string[], destinationLocationId: string, notes?: string) => Promise<void>;
  locations: { id: string; name: string; path: string }[];
}

export function BatchMoveModal({
  isOpen,
  onClose,
  packages,
  onMove,
  locations,
}: BatchMoveModalProps) {
  const [selectedPackages, setSelectedPackages] = useState<Set<string>>(new Set(packages.map(p => p.id)));
  const [destinationId, setDestinationId] = useState('');
  const [notes, setNotes] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [searchLocation, setSearchLocation] = useState('');

  const filteredLocations = locations.filter(loc =>
    loc.name.toLowerCase().includes(searchLocation.toLowerCase()) ||
    loc.path.toLowerCase().includes(searchLocation.toLowerCase())
  );

  const togglePackage = useCallback((id: string) => {
    setSelectedPackages(prev => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  }, []);

  const toggleAll = useCallback(() => {
    if (selectedPackages.size === packages.length) {
      setSelectedPackages(new Set());
    } else {
      setSelectedPackages(new Set(packages.map(p => p.id)));
    }
  }, [packages, selectedPackages.size]);

  const handleSubmit = useCallback(async () => {
    if (selectedPackages.size === 0 || !destinationId) return;

    setIsSubmitting(true);
    try {
      await onMove(Array.from(selectedPackages), destinationId, notes || undefined);
      onClose();
    } catch (error) {
      console.error('Batch move failed:', error);
    } finally {
      setIsSubmitting(false);
    }
  }, [selectedPackages, destinationId, notes, onMove, onClose]);

  const selectedDestination = locations.find(l => l.id === destinationId);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      {/* Backdrop */}
      <div className="absolute inset-0 bg-black/60 backdrop-blur-sm" onClick={onClose} />
      
      {/* Modal */}
      <div className="relative bg-background border border-border rounded-xl shadow-2xl w-full max-w-2xl mx-4 max-h-[90vh] overflow-hidden flex flex-col">
        {/* Header */}
        <div className="flex items-center justify-between p-4 border-b border-border shrink-0">
          <div className="flex items-center gap-2">
            <Package className="h-5 w-5 text-primary" />
            <h2 className="text-lg font-semibold text-foreground">Batch Move Packages</h2>
          </div>
          <button
            onClick={onClose}
            className="p-2 rounded-lg hover:bg-muted transition-colors"
          >
            <X className="h-5 w-5 text-muted-foreground" />
          </button>
        </div>

        <div className="flex-1 overflow-y-auto p-4 space-y-4">
          {/* Package selection */}
          <div>
            <div className="flex items-center justify-between mb-2">
              <label className="text-sm font-medium text-foreground">
                Select Packages to Move ({selectedPackages.size} of {packages.length})
              </label>
              <button
                className="text-sm text-primary hover:underline"
                onClick={toggleAll}
              >
                {selectedPackages.size === packages.length ? 'Deselect All' : 'Select All'}
              </button>
            </div>
            <div className="h-48 border border-border rounded-lg overflow-y-auto">
              <div className="p-2 space-y-1">
                {packages.map((pkg) => (
                  <div
                    key={pkg.id}
                    className={cn(
                      'flex items-center gap-3 p-2 rounded cursor-pointer hover:bg-muted transition-colors',
                      selectedPackages.has(pkg.id) && 'bg-muted'
                    )}
                    onClick={() => togglePackage(pkg.id)}
                  >
                    <div className={cn(
                      'w-5 h-5 rounded border-2 flex items-center justify-center transition-colors',
                      selectedPackages.has(pkg.id)
                        ? 'bg-primary border-primary'
                        : 'border-muted-foreground'
                    )}>
                      {selectedPackages.has(pkg.id) && (
                        <Check className="h-3 w-3 text-primary-foreground" />
                      )}
                    </div>
                    <Package className="h-4 w-4 text-muted-foreground" />
                    <div className="flex-1 min-w-0">
                      <div className="font-mono text-sm text-foreground">{pkg.label}</div>
                      <div className="text-xs text-muted-foreground truncate">
                        {pkg.itemName} • {pkg.quantity} {pkg.unitOfMeasure}
                      </div>
                    </div>
                    {pkg.currentLocation && (
                      <span className="text-xs px-2 py-0.5 rounded-full bg-muted text-muted-foreground">
                        {pkg.currentLocation}
                      </span>
                    )}
                  </div>
                ))}
              </div>
            </div>
          </div>

          {/* Destination selection */}
          <div>
            <label className="text-sm font-medium text-foreground mb-2 block">Destination Location</label>
            <input
              placeholder="Search locations..."
              value={searchLocation}
              onChange={(e) => setSearchLocation(e.target.value)}
              className="w-full px-3 py-2 mb-2 rounded-lg border border-border bg-background text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/50"
            />
            <div className="h-40 border border-border rounded-lg overflow-y-auto">
              <div className="p-2 space-y-1">
                {filteredLocations.map((loc) => (
                  <button
                    key={loc.id}
                    className={cn(
                      'w-full flex items-center gap-2 p-2 rounded text-left hover:bg-muted transition-colors',
                      destinationId === loc.id && 'bg-primary/10 border border-primary'
                    )}
                    onClick={() => setDestinationId(loc.id)}
                  >
                    <MapPin className="h-4 w-4 text-muted-foreground" />
                    <div className="flex-1">
                      <div className="font-medium text-foreground">{loc.name}</div>
                      <div className="text-xs text-muted-foreground">{loc.path}</div>
                    </div>
                    {destinationId === loc.id && (
                      <CheckCircle className="h-4 w-4 text-primary" />
                    )}
                  </button>
                ))}
                {filteredLocations.length === 0 && (
                  <div className="p-4 text-center text-muted-foreground text-sm">
                    No locations found
                  </div>
                )}
              </div>
            </div>
          </div>

          {/* Notes */}
          <div>
            <label htmlFor="notes" className="text-sm font-medium text-foreground mb-2 block">
              Notes (Optional)
            </label>
            <textarea
              id="notes"
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              placeholder="Add notes for this movement..."
              rows={2}
              className="w-full px-3 py-2 rounded-lg border border-border bg-background text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/50 resize-none"
            />
          </div>

          {/* Summary */}
          {selectedPackages.size > 0 && selectedDestination && (
            <div className="flex items-center gap-2 p-3 rounded-lg bg-muted text-sm">
              <span className="font-medium text-foreground">{selectedPackages.size} packages</span>
              <ArrowRight className="h-4 w-4 text-muted-foreground" />
              <span className="text-foreground">{selectedDestination.name}</span>
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="flex justify-end gap-2 p-4 border-t border-border shrink-0">
          <button
            onClick={onClose}
            disabled={isSubmitting}
            className="px-4 py-2 rounded-lg border border-border text-muted-foreground hover:bg-muted transition-colors disabled:opacity-50"
          >
            Cancel
          </button>
          <button
            onClick={handleSubmit}
            disabled={selectedPackages.size === 0 || !destinationId || isSubmitting}
            className={cn(
              'px-4 py-2 rounded-lg font-medium transition-colors',
              selectedPackages.size > 0 && destinationId && !isSubmitting
                ? 'bg-primary text-primary-foreground hover:bg-primary/90'
                : 'bg-muted text-muted-foreground cursor-not-allowed'
            )}
          >
            {isSubmitting ? (
              <>
                <Loader2 className="h-4 w-4 mr-2 animate-spin inline" />
                Moving...
              </>
            ) : (
              `Move ${selectedPackages.size} Package${selectedPackages.size !== 1 ? 's' : ''}`
            )}
          </button>
        </div>
      </div>
    </div>
  );
}

export default BatchMoveModal;



