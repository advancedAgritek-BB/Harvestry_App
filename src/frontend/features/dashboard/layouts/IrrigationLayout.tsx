'use client';

import React from 'react';
import { cn } from '@/lib/utils';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { RoomSelectorCompact } from '@/components/SiteRoomSelector';

interface IrrigationLayoutProps {
  children: React.ReactNode;
}

export default function IrrigationLayout({ children }: IrrigationLayoutProps) {
  const pathname = usePathname();

  const tabs = [
    { label: 'Overview', href: '/dashboard/irrigation' },
    { label: 'Programs', href: '/dashboard/irrigation/programs' },
    { label: 'Schedules', href: '/dashboard/irrigation/schedules' },
    { label: 'History', href: '/dashboard/irrigation/history' },
    { label: 'Recipes', href: '/dashboard/recipes', isExternal: true },
  ];

  return (
    <div className="flex flex-col h-full bg-background text-foreground">
      {/* Header */}
      <div className="flex items-center justify-between px-6 py-4 border-b border-border bg-surface/50 backdrop-blur shrink-0">
        <div>
          <h1 className="text-xl font-bold tracking-tight text-foreground">Irrigation Control</h1>
          <RoomSelectorCompact />
        </div>
        
        {/* Navigation Tabs */}
        <div className="flex bg-surface/50 rounded-lg p-1 gap-1">
          {tabs.map((tab) => {
             const isActive = !tab.isExternal && pathname === tab.href;
             return (
               <Link
                 key={tab.href}
                 href={tab.href}
                 className={cn(
                   "px-4 py-1.5 text-sm font-medium rounded transition-colors",
                   isActive 
                     ? "bg-cyan-500/20 text-cyan-300 shadow-sm" 
                     : "text-muted-foreground hover:text-foreground hover:bg-muted/50"
                 )}
               >
                 {tab.label}
                 {tab.isExternal && <span className="ml-1 opacity-50 text-[10px]">↗</span>}
               </Link>
             );
          })}
        </div>
      </div>

      {/* Main Content Area */}
      <div className="flex-1 overflow-auto p-6">
        {children}
      </div>
    </div>
  );
}
