'use client';

import React from 'react';
import { cn } from '@/lib/utils';

interface TabItem {
  id: string;
  label: string;
  icon?: React.ElementType;
  badge?: number | string;
}

interface AdminTabsProps {
  tabs: TabItem[];
  activeTab: string;
  onChange: (tabId: string) => void;
  className?: string;
}

export function AdminTabs({ tabs, activeTab, onChange, className }: AdminTabsProps) {
  return (
    <div className={cn('border-b border-border', className)}>
      <nav className="flex gap-1 -mb-px" aria-label="Tabs">
        {tabs.map((tab) => {
          const isActive = tab.id === activeTab;
          const Icon = tab.icon;

          return (
            <button
              key={tab.id}
              onClick={() => onChange(tab.id)}
              className={cn(
                'flex items-center gap-2 px-4 py-3 text-sm font-medium border-b-2 transition-all duration-200',
                isActive
                  ? 'text-violet-400 border-violet-500'
                  : 'text-muted-foreground border-transparent hover:text-foreground hover:border-white/20'
              )}
            >
              {Icon && <Icon className="w-4 h-4" />}
              {tab.label}
              {tab.badge !== undefined && (
                <span
                  className={cn(
                    'ml-1 px-1.5 py-0.5 text-[10px] font-semibold rounded-full',
                    isActive
                      ? 'bg-violet-500/20 text-violet-300'
                      : 'bg-white/10 text-muted-foreground'
                  )}
                >
                  {tab.badge}
                </span>
              )}
            </button>
          );
        })}
      </nav>
    </div>
  );
}

interface TabPanelProps {
  id: string;
  activeTab: string;
  children: React.ReactNode;
  className?: string;
}

export function TabPanel({ id, activeTab, children, className }: TabPanelProps) {
  if (id !== activeTab) return null;

  return (
    <div className={cn('py-6', className)} role="tabpanel" aria-labelledby={`tab-${id}`}>
      {children}
    </div>
  );
}

