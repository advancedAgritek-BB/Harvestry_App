'use client';

import React, { useState, useCallback } from 'react';
import { cn } from '@/lib/utils';
import {
  Package,
  QrCode,
  ArrowLeftRight,
  Home,
  AlertCircle,
  Search,
  MoreHorizontal,
  ChevronLeft,
} from 'lucide-react';

interface MobileInventoryShellProps {
  children: React.ReactNode;
  title?: string;
  showBackButton?: boolean;
  onBack?: () => void;
  currentTab?: 'home' | 'scan' | 'move' | 'holds' | 'search';
  onTabChange?: (tab: 'home' | 'scan' | 'move' | 'holds' | 'search') => void;
}

export function MobileInventoryShell({
  children,
  title = 'Inventory',
  showBackButton = false,
  onBack,
  currentTab = 'home',
  onTabChange,
}: MobileInventoryShellProps) {
  const [isMenuOpen, setIsMenuOpen] = useState(false);

  const handleTabChange = useCallback((tab: 'home' | 'scan' | 'move' | 'holds' | 'search') => {
    onTabChange?.(tab);
  }, [onTabChange]);

  const tabs = [
    { id: 'home' as const, label: 'Home', icon: Home },
    { id: 'scan' as const, label: 'Scan', icon: QrCode },
    { id: 'move' as const, label: 'Move', icon: ArrowLeftRight },
    { id: 'holds' as const, label: 'Holds', icon: AlertCircle },
    { id: 'search' as const, label: 'Search', icon: Search },
  ];

  return (
    <div className="min-h-screen flex flex-col bg-background">
      {/* Header */}
      <header className="sticky top-0 z-40 bg-background border-b border-border">
        <div className="flex items-center h-14 px-4">
          {showBackButton ? (
            <button
              className="mr-2 -ml-2 h-10 w-10 flex items-center justify-center rounded-lg hover:bg-muted transition-colors"
              onClick={onBack}
            >
              <ChevronLeft className="h-6 w-6 text-foreground" />
            </button>
          ) : (
            <Package className="h-6 w-6 text-primary mr-3" />
          )}
          <h1 className="font-semibold text-lg text-foreground flex-1">{title}</h1>
          <button
            className="h-10 w-10 flex items-center justify-center rounded-lg hover:bg-muted transition-colors"
            onClick={() => setIsMenuOpen(!isMenuOpen)}
          >
            <MoreHorizontal className="h-5 w-5 text-muted-foreground" />
          </button>
        </div>
      </header>

      {/* Main content area with touch-friendly padding */}
      <main className="flex-1 overflow-y-auto pb-20">
        <div className="p-4">
          {children}
        </div>
      </main>

      {/* Bottom navigation bar - fixed, large touch targets */}
      <nav className="fixed bottom-0 left-0 right-0 z-50 bg-background border-t border-border pb-safe">
        <div className="flex items-center justify-around h-16 px-2">
          {tabs.map((tab) => {
            const Icon = tab.icon;
            const isActive = currentTab === tab.id;
            return (
              <button
                key={tab.id}
                onClick={() => handleTabChange(tab.id)}
                className={cn(
                  'flex flex-col items-center justify-center gap-1 w-16 h-14 rounded-lg transition-colors',
                  'active:bg-muted touch-manipulation',
                  isActive
                    ? 'text-primary'
                    : 'text-muted-foreground hover:text-foreground'
                )}
              >
                <Icon className={cn('h-6 w-6', isActive && 'text-primary')} />
                <span className="text-xs font-medium">{tab.label}</span>
              </button>
            );
          })}
        </div>
      </nav>
    </div>
  );
}

export default MobileInventoryShell;




