'use client';

import React, { useState, useRef } from 'react';
import { cn } from '@/lib/utils';
import { Package, MapPin, ArrowRight, Clipboard, AlertTriangle } from 'lucide-react';

interface SwipeableLotCardProps {
  lot: {
    id: string;
    packageLabel: string;
    itemName: string;
    quantity: number;
    unitOfMeasure: string;
    locationName?: string;
    status: string;
    expirationDate?: string;
    holdReasonCode?: string;
  };
  onMove?: () => void;
  onAdjust?: () => void;
  onView?: () => void;
}

export function SwipeableLotCard({
  lot,
  onMove,
  onAdjust,
  onView,
}: SwipeableLotCardProps) {
  const [swipeOffset, setSwipeOffset] = useState(0);
  const [isRevealing, setIsRevealing] = useState(false);
  const startXRef = useRef(0);
  const cardRef = useRef<HTMLDivElement>(null);

  const handleTouchStart = (e: React.TouchEvent) => {
    startXRef.current = e.touches[0].clientX;
  };

  const handleTouchMove = (e: React.TouchEvent) => {
    const diff = startXRef.current - e.touches[0].clientX;
    if (diff > 0) {
      // Swiping left - reveal actions
      setSwipeOffset(Math.min(diff, 160));
      setIsRevealing(true);
    } else if (diff < 0 && isRevealing) {
      // Swiping right - hide actions
      setSwipeOffset(Math.max(0, swipeOffset + diff));
    }
  };

  const handleTouchEnd = () => {
    if (swipeOffset > 80) {
      setSwipeOffset(160);
    } else {
      setSwipeOffset(0);
      setIsRevealing(false);
    }
  };

  const isOnHold = lot.status === 'OnHold' || lot.holdReasonCode;
  const isExpiringSoon = lot.expirationDate && 
    new Date(lot.expirationDate) <= new Date(Date.now() + 14 * 24 * 60 * 60 * 1000);

  return (
    <div className="relative overflow-hidden rounded-lg">
      {/* Swipe actions */}
      <div className="absolute right-0 top-0 bottom-0 flex">
        <button
          className="w-20 h-full bg-cyan-500 text-white flex flex-col items-center justify-center gap-1"
          onClick={onMove}
        >
          <ArrowRight className="h-5 w-5" />
          <span className="text-xs">Move</span>
        </button>
        <button
          className="w-20 h-full bg-amber-500 text-white flex flex-col items-center justify-center gap-1"
          onClick={onAdjust}
        >
          <Clipboard className="h-5 w-5" />
          <span className="text-xs">Adjust</span>
        </button>
      </div>

      {/* Main card */}
      <div
        ref={cardRef}
        className={cn(
          'relative bg-muted/30 border border-border rounded-lg transition-transform touch-manipulation cursor-pointer',
          'active:scale-[0.99]'
        )}
        style={{ transform: `translateX(-${swipeOffset}px)` }}
        onTouchStart={handleTouchStart}
        onTouchMove={handleTouchMove}
        onTouchEnd={handleTouchEnd}
        onClick={() => !isRevealing && onView?.()}
      >
        <div className="p-4">
          <div className="flex items-start gap-3">
            <div className={cn(
              'w-10 h-10 rounded-lg flex items-center justify-center',
              isOnHold ? 'bg-rose-500/10 text-rose-500' : 'bg-primary/10 text-primary'
            )}>
              {isOnHold ? <AlertTriangle className="h-5 w-5" /> : <Package className="h-5 w-5" />}
            </div>

            <div className="flex-1 min-w-0">
              <div className="flex items-center justify-between gap-2 mb-1">
                <span className="font-mono text-sm font-medium text-foreground truncate">
                  {lot.packageLabel}
                </span>
                <span className={cn(
                  'text-xs px-2 py-0.5 rounded-full shrink-0',
                  isOnHold ? 'bg-rose-500/10 text-rose-500' : 'bg-muted text-muted-foreground'
                )}>
                  {lot.status}
                </span>
              </div>

              <div className="text-sm text-muted-foreground truncate mb-2">
                {lot.itemName}
              </div>

              <div className="flex items-center justify-between text-sm">
                <span className="font-semibold text-foreground">
                  {lot.quantity.toLocaleString()} {lot.unitOfMeasure}
                </span>
                {lot.locationName && (
                  <span className="flex items-center gap-1 text-muted-foreground">
                    <MapPin className="h-3 w-3" />
                    {lot.locationName}
                  </span>
                )}
              </div>

              {isExpiringSoon && (
                <div className="mt-2 text-xs text-amber-500 flex items-center gap-1">
                  <AlertTriangle className="h-3 w-3" />
                  Expires {new Date(lot.expirationDate!).toLocaleDateString()}
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default SwipeableLotCard;



