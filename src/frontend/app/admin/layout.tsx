'use client';

import React from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { cn } from '@/lib/utils';
import {
  Leaf,
  Users,
  MapPin,
  Cpu,
  Package,
  Shield,
  Link2,
  Flag,
  Bell,
  ChevronLeft,
  Settings,
  Play,
} from 'lucide-react';
import { useAuthStore } from '@/stores/auth';
import { AuthGuard } from '@/components/auth';

interface AdminTab {
  id: string;
  label: string;
  href: string;
  icon: React.ElementType;
  description: string;
  superAdminOnly?: boolean;
}

const ADMIN_TABS: AdminTab[] = [
  {
    id: 'cultivation',
    label: 'Cultivation',
    href: '/admin/cultivation',
    icon: Leaf,
    description: 'Environment, Irrigation, Fertigation, Alerts',
  },
  {
    id: 'identity',
    label: 'Identity & Access',
    href: '/admin/identity',
    icon: Users,
    description: 'Users, Roles, Badges, Training',
  },
  {
    id: 'spatial',
    label: 'Spatial Model',
    href: '/admin/spatial',
    icon: MapPin,
    description: 'Sites, Rooms, Zones, Racks',
  },
  {
    id: 'equipment',
    label: 'Equipment',
    href: '/admin/equipment',
    icon: Cpu,
    description: 'Devices, Calibration, Health',
  },
  {
    id: 'inventory',
    label: 'Inventory & Labels',
    href: '/admin/inventory',
    icon: Package,
    description: 'UoM, Label Templates, Barcodes',
  },
  {
    id: 'compliance',
    label: 'Compliance',
    href: '/admin/compliance',
    icon: Shield,
    description: 'METRC, BioTrack, COA, Jurisdiction',
  },
  {
    id: 'integrations',
    label: 'Integrations',
    href: '/admin/integrations',
    icon: Link2,
    description: 'Slack, QuickBooks',
  },
  {
    id: 'feature-flags',
    label: 'Feature Flags',
    href: '/admin/feature-flags',
    icon: Flag,
    description: 'Site-level flags',
  },
  {
    id: 'notifications',
    label: 'Notifications',
    href: '/admin/notifications',
    icon: Bell,
    description: 'Subscriptions, Escalations',
  },
  {
    id: 'simulation',
    label: 'Simulation',
    href: '/admin/simulation',
    icon: Play,
    description: 'Sensor Data Simulation',
    superAdminOnly: true,
  },
];

export default function AdminLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const pathname = usePathname();
  const { user, isSuperAdmin } = useAuthStore();
  const isSuperAdminUser = isSuperAdmin();

  // Filter tabs based on user role
  const visibleTabs = ADMIN_TABS.filter(tab => 
    !tab.superAdminOnly || isSuperAdminUser
  );

  // Find active tab based on current path
  const activeTab = ADMIN_TABS.find((tab) => pathname.startsWith(tab.href));

  return (
    <AuthGuard>
      <div className="flex h-screen bg-background overflow-hidden font-sans">
        {/* Admin Sidebar */}
        <aside className="w-72 h-screen flex flex-col bg-surface/50 border-r border-border z-50 flex-shrink-0">
        {/* Header */}
        <div className="h-[72px] px-6 flex items-center justify-between border-b border-border">
          <div className="flex items-center gap-3">
            <div className="w-9 h-9 rounded-lg bg-gradient-to-br from-violet-500/20 to-purple-600/20 flex items-center justify-center ring-1 ring-violet-500/30">
              <Settings className="w-5 h-5 text-violet-400" />
            </div>
            <div>
              <h1 className="text-sm font-semibold text-foreground">Administration</h1>
              <p className="text-[10px] text-muted-foreground">System Settings</p>
            </div>
          </div>
        </div>

        {/* Back to Dashboard */}
        <Link
          href="/dashboard"
          className="mx-4 mt-4 flex items-center gap-2 px-3 py-2 text-xs text-muted-foreground hover:text-foreground hover:bg-muted/50 rounded-lg transition-colors"
        >
          <ChevronLeft className="w-4 h-4" />
          Back to Dashboard
        </Link>

        {/* Navigation Tabs */}
        <nav className="flex-1 overflow-y-auto px-3 py-4">
          <div className="space-y-1">
            {visibleTabs.map((tab) => {
              const isActive = pathname.startsWith(tab.href);
              const Icon = tab.icon;

              return (
                <Link
                  key={tab.id}
                  href={tab.href}
                  className={cn(
                    'group flex items-start gap-3 px-3 py-2.5 rounded-lg transition-all duration-200 relative',
                    isActive
                      ? 'bg-violet-950/40 text-violet-300'
                      : 'text-muted-foreground hover:text-foreground hover:bg-muted/50'
                  )}
                >
                  {/* Active Indicator */}
                  {isActive && (
                    <div className="absolute left-0 top-1/2 -translate-y-1/2 w-1 h-6 bg-violet-500 rounded-r-full shadow-[0_0_12px_rgba(139,92,246,0.5)]" />
                  )}

                  <Icon
                    className={cn(
                      'w-5 h-5 mt-0.5 flex-shrink-0 transition-transform group-hover:scale-105',
                      isActive && 'text-violet-400'
                    )}
                    strokeWidth={isActive ? 2.5 : 2}
                  />

                  <div className="flex-1 min-w-0">
                    <div
                      className={cn(
                        'text-sm font-medium',
                        isActive ? 'text-violet-200' : 'text-foreground'
                      )}
                    >
                      {tab.label}
                    </div>
                    <div className="text-[11px] text-muted-foreground truncate">
                      {tab.description}
                    </div>
                  </div>
                </Link>
              );
            })}
          </div>
        </nav>

        {/* Footer */}
        <div className="p-4 border-t border-border">
          <div className="flex items-center gap-3 px-2">
            <div className="w-8 h-8 rounded-full bg-gradient-to-br from-violet-400 to-purple-600 ring-2 ring-surface flex items-center justify-center text-white text-xs font-bold">
              {user?.name?.split(' ').map(n => n[0]).join('') || '??'}
            </div>
            <div className="flex-1 min-w-0">
              <div className="text-sm font-medium text-foreground truncate">
                {user?.name || 'Unknown User'}
              </div>
              <div className="text-[11px] text-muted-foreground">
                {user?.role || 'Unknown Role'}
              </div>
            </div>
          </div>
        </div>
      </aside>

      {/* Main Content Area */}
      <div className="flex-1 flex flex-col h-full overflow-hidden relative">
        {/* Page Header */}
        <header className="h-[72px] px-8 flex items-center justify-between bg-surface/80 backdrop-blur-xl border-b border-border sticky top-0 z-40">
          <div className="flex items-center gap-4">
            {activeTab && (
              <>
                <activeTab.icon className="w-6 h-6 text-violet-400" />
                <div>
                  <h2 className="text-lg font-semibold text-foreground">{activeTab.label}</h2>
                  <p className="text-xs text-muted-foreground">{activeTab.description}</p>
                </div>
              </>
            )}
          </div>

          {/* Header Actions */}
          <div className="flex items-center gap-3">
            <div className="text-xs text-muted-foreground">
              Site: <span className="text-foreground font-medium">Evergreen</span>
            </div>
          </div>
        </header>

        {/* Scrollable Page Content */}
        <main className="flex-1 overflow-y-auto overflow-x-hidden relative">
          {/* Gradient Background */}
          <div className="absolute inset-0 pointer-events-none bg-[radial-gradient(ellipse_at_top,_var(--tw-gradient-stops))] from-violet-900/10 via-transparent to-transparent opacity-50" />

          {/* Content Container */}
          <div className="relative z-10 p-8">{children}</div>
        </main>
      </div>
      </div>
    </AuthGuard>
  );
}
