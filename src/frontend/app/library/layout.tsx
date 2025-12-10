'use client';

import React from 'react';
import { cn } from '@/lib/utils';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { 
  Dna, 
  Layers, 
  Book, 
  Leaf, 
  Sun,
  Library as LibraryIcon
} from 'lucide-react';
import { Sidebar } from '@/components/navigation';
import { AuthGuard } from '@/components/auth';

interface LibraryLayoutProps {
  children: React.ReactNode;
}

const LIBRARY_TABS = [
  { 
    label: 'Genetics', 
    href: '/library/genetics', 
    icon: Dna,
    description: 'Strain genetics library'
  },
  { 
    label: 'Blueprints', 
    href: '/library/blueprints', 
    icon: Layers,
    description: 'Cultivation blueprints'
  },
  { 
    label: 'Fertigation', 
    href: '/library/fertigation', 
    icon: Book,
    description: 'Nutrient recipes'
  },
  { 
    label: 'Environment', 
    href: '/library/environment', 
    icon: Leaf,
    description: 'Climate profiles'
  },
  { 
    label: 'Lighting', 
    href: '/library/lighting', 
    icon: Sun,
    description: 'Light schedules'
  },
];

export default function LibraryLayout({ children }: LibraryLayoutProps) {
  const pathname = usePathname();

  // Determine active tab
  const activeTab = LIBRARY_TABS.find(tab => pathname.startsWith(tab.href));

  return (
    <AuthGuard>
      <div className="flex h-screen bg-background overflow-hidden font-sans">
        {/* Fixed Sidebar */}
        <Sidebar />
        
        {/* Main Content Area */}
        <div className="flex-1 flex flex-col h-full overflow-hidden relative">
          {/* Library Header */}
          <div className="flex items-center justify-between px-6 py-4 border-b border-border bg-surface/50 backdrop-blur shrink-0">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-xl bg-emerald-500/10 flex items-center justify-center ring-1 ring-emerald-500/30">
                <LibraryIcon className="w-5 h-5 text-emerald-400" />
              </div>
              <div>
                <h1 className="text-xl font-bold tracking-tight text-foreground">Library</h1>
                <p className="text-sm text-muted-foreground">
                  {activeTab?.description || 'Manage your cultivation knowledge base'}
                </p>
              </div>
            </div>
            
            {/* Navigation Tabs */}
            <nav className="flex bg-surface/50 rounded-lg p-1 gap-1">
              {LIBRARY_TABS.map((tab) => {
                const isActive = pathname.startsWith(tab.href);
                const Icon = tab.icon;
                return (
                  <Link
                    key={tab.href}
                    href={tab.href}
                    className={cn(
                      "flex items-center gap-2 px-4 py-2 text-sm font-medium rounded-md transition-all duration-200",
                      isActive 
                        ? "bg-emerald-500/20 text-emerald-300 shadow-sm ring-1 ring-emerald-500/30" 
                        : "text-muted-foreground hover:text-foreground hover:bg-muted/50"
                    )}
                  >
                    <Icon className={cn("w-4 h-4", isActive && "text-emerald-400")} />
                    {tab.label}
                  </Link>
                );
              })}
            </nav>
          </div>

          {/* Scrollable Page Content */}
          <main className="flex-1 overflow-y-auto overflow-x-hidden relative">
            {/* Gradient Spotlight Effect for Content Area */}
            <div className="absolute inset-0 pointer-events-none bg-[radial-gradient(ellipse_at_top,_var(--tw-gradient-stops))] from-primary/10 via-transparent to-transparent opacity-50" />
            
            {/* Content Container */}
            <div className="relative z-10">
              {children}
            </div>
          </main>
        </div>
      </div>
    </AuthGuard>
  );
}


