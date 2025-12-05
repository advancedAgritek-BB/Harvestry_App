'use client';

import React, { useState, useEffect } from 'react';
import { WifiOff, Cloud, CloudOff, RefreshCw } from 'lucide-react';
import { cn } from '@/lib/utils';

interface OfflineIndicatorProps {
  queuedActions?: number;
  lastSyncAt?: string;
  onRetry?: () => void;
  className?: string;
}

export function OfflineIndicator({ 
  queuedActions = 0, 
  lastSyncAt,
  onRetry,
  className 
}: OfflineIndicatorProps) {
  const [isOnline, setIsOnline] = useState(true);
  const [showBanner, setShowBanner] = useState(false);

  useEffect(() => {
    // Check initial state
    setIsOnline(navigator.onLine);

    const handleOnline = () => {
      setIsOnline(true);
      // Show banner briefly when coming back online
      setShowBanner(true);
      setTimeout(() => setShowBanner(false), 3000);
    };

    const handleOffline = () => {
      setIsOnline(false);
      setShowBanner(true);
    };

    window.addEventListener('online', handleOnline);
    window.addEventListener('offline', handleOffline);

    return () => {
      window.removeEventListener('online', handleOnline);
      window.removeEventListener('offline', handleOffline);
    };
  }, []);

  // Don't show anything if online and no queued actions
  if (isOnline && queuedActions === 0 && !showBanner) {
    return null;
  }

  const formatLastSync = () => {
    if (!lastSyncAt) return 'Never';
    const date = new Date(lastSyncAt);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    
    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    if (diffMins < 1440) return `${Math.floor(diffMins / 60)}h ago`;
    return date.toLocaleDateString();
  };

  return (
    <div className={cn('fixed bottom-20 inset-x-4 z-40', className)}>
      <div className={cn(
        'rounded-2xl p-4 transition-all',
        isOnline 
          ? 'bg-emerald-500/10 border border-emerald-500/20' 
          : 'bg-amber-500/10 border border-amber-500/20'
      )}>
        <div className="flex items-center gap-3">
          <div className={cn(
            'w-10 h-10 rounded-xl flex items-center justify-center',
            isOnline ? 'bg-emerald-500/20' : 'bg-amber-500/20'
          )}>
            {isOnline ? (
              <Cloud className="w-5 h-5 text-emerald-400" />
            ) : (
              <WifiOff className="w-5 h-5 text-amber-400" />
            )}
          </div>

          <div className="flex-1">
            <div className={cn(
              'text-sm font-medium',
              isOnline ? 'text-emerald-400' : 'text-amber-400'
            )}>
              {isOnline ? 'Back Online' : 'You\'re Offline'}
            </div>
            <div className="text-xs text-muted-foreground">
              {isOnline && queuedActions > 0 
                ? `Syncing ${queuedActions} action${queuedActions !== 1 ? 's' : ''}...`
                : isOnline 
                  ? 'All changes synced'
                  : queuedActions > 0 
                    ? `${queuedActions} action${queuedActions !== 1 ? 's' : ''} queued`
                    : 'Changes will sync when connected'
              }
            </div>
          </div>

          {!isOnline && queuedActions > 0 && (
            <div className="flex items-center gap-2">
              <span className="text-xs text-muted-foreground">
                Last sync: {formatLastSync()}
              </span>
            </div>
          )}

          {isOnline && queuedActions > 0 && (
            <RefreshCw className="w-4 h-4 text-emerald-400 animate-spin" />
          )}
        </div>

        {/* Offline Queue Details */}
        {!isOnline && queuedActions > 0 && (
          <div className="mt-3 pt-3 border-t border-border">
            <div className="flex items-center justify-between">
              <span className="text-xs text-muted-foreground">
                Your changes are saved locally and will sync automatically
              </span>
              {onRetry && (
                <button
                  onClick={onRetry}
                  className="text-xs text-amber-400 underline"
                >
                  Retry Now
                </button>
              )}
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

export default OfflineIndicator;

