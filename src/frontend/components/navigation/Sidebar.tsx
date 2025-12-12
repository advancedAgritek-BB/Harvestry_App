'use client';

import React from 'react';
import { 
  Home, 
  CalendarDays, 
  Flower2, 
  BarChart2, 
  Droplets,
  BookOpen,
  Cog,
  Package,
  ClipboardCheck,
  ShoppingCart,
  Truck
} from 'lucide-react';
import { cn } from '@/lib/utils';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { useAuthStore, TierFeature, TIER_FEATURES } from '@/stores/auth/authStore';
import { usePermissions } from '@/providers/PermissionsProvider';

interface NavItem {
  label: string;
  icon: React.ElementType;
  href: string;
  count?: number;
  accent?: 'cyan' | 'amber' | 'violet';
  /** Required tier feature - if not set, always visible */
  requiredFeature?: TierFeature;
}

const NAV_ITEMS: NavItem[] = [
  { label: 'Home', icon: Home, href: '/dashboard/overview' },
  { label: 'Planner', icon: CalendarDays, href: '/dashboard/planner', requiredFeature: 'production_planning' },
  { label: 'Tasks', icon: ClipboardCheck, href: '/dashboard/tasks', requiredFeature: 'task_management' },
  { label: 'Cultivation', icon: Flower2, href: '/dashboard/cultivation', requiredFeature: 'monitoring' },
  { label: 'Irrigation', icon: Droplets, href: '/dashboard/irrigation', requiredFeature: 'control' },
  { label: 'Library', icon: BookOpen, href: '/library', requiredFeature: 'sop_engine' },
  { label: 'Inventory', icon: Package, href: '/inventory', accent: 'amber', requiredFeature: 'inventory' },
  { label: 'Sales', icon: ShoppingCart, href: '/sales/dashboard', accent: 'amber', requiredFeature: 'inventory' },
  { label: 'Transfers', icon: Truck, href: '/transfers/outbound', accent: 'amber', requiredFeature: 'inventory' },
  { label: 'Analytics', icon: BarChart2, href: '/dashboard/analytics', accent: 'amber', requiredFeature: 'historical_data' },
  { label: 'Admin', icon: Cog, href: '/admin', accent: 'violet' },
];

const ACCENT_STYLES = {
  cyan: {
    text: 'text-cyan-400',
    bg: 'bg-cyan-950/30',
    indicator: 'bg-cyan-500 shadow-[0_0_12px_rgba(6,182,212,0.5)]',
  },
  amber: {
    text: 'text-amber-400',
    bg: 'bg-amber-950/30',
    indicator: 'bg-amber-500 shadow-[0_0_12px_rgba(245,158,11,0.5)]',
  },
  violet: {
    text: 'text-violet-400',
    bg: 'bg-violet-950/30',
    indicator: 'bg-violet-500 shadow-[0_0_12px_rgba(139,92,246,0.5)]',
  },
};

export function Sidebar() {
  const pathname = usePathname();
  // Subscribe to currentTier directly so component re-renders when tier changes
  const currentTier = useAuthStore((state) => state.currentTier);
  const permissions = usePermissions();
  
  // Check if a feature is available in the current tier
  const hasFeature = (feature: TierFeature) => TIER_FEATURES[currentTier].includes(feature);

  return (
    <aside className="w-20 h-screen flex flex-col items-center py-6 bg-surface border-r border-border z-50 flex-shrink-0 overflow-hidden">
      {/* Logo Placeholder / Home */}
      <div className="mb-4 flex-shrink-0">
         <div className="w-10 h-10 rounded-xl bg-cyan-500/10 flex items-center justify-center ring-1 ring-cyan-500/50">
            <Flower2 className="w-6 h-6 text-cyan-400" />
         </div>
      </div>

      {/* Main Navigation */}
      <nav className="flex-1 flex flex-col gap-4 w-full px-2 overflow-y-auto overflow-x-hidden scrollbar-thin scrollbar-thumb-border scrollbar-track-transparent min-h-0">
        {NAV_ITEMS.map((item) => {
          // Hide items that require a feature not available in the current tier
          if (item.requiredFeature && !hasFeature(item.requiredFeature)) {
            return null;
          }

          // Permission gating for new top-level modules.
          if (item.href.startsWith('/sales')) {
            // Show Sales if user has any sales-related permission
            if (!permissions.any([
              'sales:dashboard:view',
              'sales:customers:view',
              'sales:orders:view',
              'sales:orders:create',
              'sales:allocate',
              'sales:shipments:create',
              'sales:transfers:view',
              'sales:reports:view',
            ])) return null;
          }
          if (item.href.startsWith('/transfers')) {
            if (!permissions.any(['transfers:view', 'transfers:create'])) return null;
          }
          
          // Determine active state - Sales matches any /sales/* path
          let isActive = false;
          if (item.href === '/dashboard/overview') {
            isActive = pathname === '/dashboard/overview' || pathname === '/dashboard';
          } else if (item.href === '/sales/dashboard') {
            // Sales nav item is active for any /sales/* path
            isActive = pathname.startsWith('/sales');
          } else {
            isActive = pathname === item.href || (pathname.startsWith(item.href) && item.href !== '/dashboard/overview');
          }
          const Icon = item.icon;
          const accent = ACCENT_STYLES[item.accent ?? 'cyan'];
          
          return (
            <Link 
              key={item.href} 
              href={item.href}
              className={cn(
                "group flex flex-col items-center justify-center gap-1.5 p-2 rounded-xl transition-all duration-200 relative",
                isActive 
                  ? cn(accent.text, accent.bg)
                  : "text-muted-foreground hover:text-foreground hover:bg-muted/50"
              )}
            >
              <div className="relative">
                <Icon className={cn("w-6 h-6 transition-transform group-hover:scale-110", isActive && "fill-current")} strokeWidth={isActive ? 2.5 : 2} />
                {item.count && (
                  <span className="absolute -top-1.5 -right-1.5 min-w-[16px] h-4 flex items-center justify-center text-[9px] font-bold text-white bg-rose-500 rounded-full px-0.5 ring-2 ring-surface">
                    {item.count}
                  </span>
                )}
              </div>
              <span className="text-[10px] font-medium tracking-wide opacity-80 group-hover:opacity-100">
                {item.label}
              </span>
              
              {/* Active Indicator Dot */}
              {isActive && (
                <div className={cn("absolute left-0 top-1/2 -translate-y-1/2 w-1 h-8 rounded-r-full", accent.indicator)} />
              )}
            </Link>
          );
        })}
      </nav>

      {/* User Avatar */}
      <div className="w-full px-2 pt-4 flex-shrink-0 border-t border-border/50 mt-2">
        <img 
          src="/images/user-avatar.png" 
          alt="User avatar" 
          className="w-8 h-8 rounded-full mx-auto ring-2 ring-border object-cover"
        />
      </div>
    </aside>
  );
}
