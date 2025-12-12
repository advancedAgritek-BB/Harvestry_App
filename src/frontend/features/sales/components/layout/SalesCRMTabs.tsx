'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import {
  LayoutDashboard,
  Users,
  FileText,
  Package,
  Truck,
  BarChart3,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { usePermissions } from '@/providers/PermissionsProvider';

interface Tab {
  id: string;
  label: string;
  href: string;
  icon: React.ElementType;
  permission?: string;
}

const TABS: Tab[] = [
  {
    id: 'dashboard',
    label: 'Dashboard',
    href: '/sales/dashboard',
    icon: LayoutDashboard,
    permission: 'sales:dashboard:view',
  },
  {
    id: 'customers',
    label: 'Customers',
    href: '/sales/customers',
    icon: Users,
    permission: 'sales:customers:view',
  },
  {
    id: 'orders',
    label: 'Orders',
    href: '/sales/orders',
    icon: FileText,
    permission: 'sales:orders:view',
  },
  {
    id: 'shipments',
    label: 'Shipments',
    href: '/sales/shipments',
    icon: Package,
    permission: 'sales:shipments:create',
  },
  {
    id: 'transfers',
    label: 'Transfers',
    href: '/sales/transfers',
    icon: Truck,
    permission: 'sales:transfers:view',
  },
  {
    id: 'reports',
    label: 'Reports',
    href: '/sales/reports',
    icon: BarChart3,
    permission: 'sales:reports:view',
  },
];

export function SalesCRMTabs() {
  const pathname = usePathname();
  const permissions = usePermissions();

  // Determine which tab is active based on pathname
  function isTabActive(tab: Tab): boolean {
    if (tab.href === '/sales/dashboard') {
      return pathname === '/sales' || pathname === '/sales/dashboard';
    }
    return pathname.startsWith(tab.href);
  }

  return (
    <div className="border-b border-border bg-surface/50 backdrop-blur-sm">
      <div className="px-6">
        <nav className="flex items-center gap-1 -mb-px overflow-x-auto scrollbar-thin scrollbar-thumb-border">
          {TABS.map((tab) => {
            // Check permission - if no permission specified or user has it, show tab
            const hasPermission = !tab.permission || permissions.has(tab.permission);
            if (!hasPermission) return null;

            const active = isTabActive(tab);
            const Icon = tab.icon;

            return (
              <Link
                key={tab.id}
                href={tab.href}
                className={cn(
                  'flex items-center gap-2 px-4 py-3 text-sm font-medium border-b-2 transition-colors whitespace-nowrap',
                  active
                    ? 'border-amber-500 text-amber-400'
                    : 'border-transparent text-muted-foreground hover:text-foreground hover:border-border'
                )}
              >
                <Icon className="w-4 h-4" />
                {tab.label}
              </Link>
            );
          })}
        </nav>
      </div>
    </div>
  );
}
