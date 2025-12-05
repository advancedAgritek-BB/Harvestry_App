import React from 'react';
import { cn } from '@/lib/utils';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { Book, Leaf, Sun, Table } from 'lucide-react';

interface RecipeLayoutProps {
  children: React.ReactNode;
}

export default function RecipeLayout({ children }: RecipeLayoutProps) {
  const pathname = usePathname();

  const tabs = [
    { label: 'Fertigation', href: '/dashboard/recipes/fertigation', icon: Book },
    { label: 'Environment', href: '/dashboard/recipes/environment', icon: Leaf },
    { label: 'Lighting', href: '/dashboard/recipes/lighting', icon: Sun },
    { label: 'Blueprints', href: '/dashboard/recipes/blueprints', icon: Table },
  ];

  return (
    <div className="flex flex-col h-full bg-background text-foreground">
      {/* Header */}
      <div className="flex items-center justify-between px-6 py-4 border-b border-border bg-surface/50 backdrop-blur shrink-0">
        <div>
          <h1 className="text-xl font-bold tracking-tight text-foreground">Recipe Library</h1>
          <div className="flex gap-2 text-sm text-muted-foreground">
            <span>Manage grow templates & schedules</span>
          </div>
        </div>
        
        {/* Navigation Tabs */}
        <div className="flex bg-surface/50 rounded-lg p-1 gap-1">
          {tabs.map((tab) => {
             const isActive = pathname.startsWith(tab.href);
             const Icon = tab.icon;
             return (
               <Link
                 key={tab.href}
                 href={tab.href}
                 className={cn(
                   "flex items-center gap-2 px-4 py-1.5 text-sm font-medium rounded transition-colors",
                   isActive 
                     ? "bg-purple-500/20 text-purple-300 shadow-sm" 
                     : "text-muted-foreground hover:text-foreground hover:bg-muted/50"
                 )}
               >
                 <Icon className="w-4 h-4" />
                 {tab.label}
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
