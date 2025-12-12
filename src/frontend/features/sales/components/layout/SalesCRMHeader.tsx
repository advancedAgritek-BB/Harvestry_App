'use client';

import { ShoppingCart, Search, Plus, Users, RefreshCw } from 'lucide-react';
import { usePermissions } from '@/providers/PermissionsProvider';
import Link from 'next/link';

interface SalesCRMHeaderProps {
  onRefresh?: () => void;
  isRefreshing?: boolean;
}

export function SalesCRMHeader({ onRefresh, isRefreshing }: SalesCRMHeaderProps) {
  const permissions = usePermissions();
  const canCreateOrder = permissions.has('sales:orders:create');
  const canCreateCustomer = permissions.has('sales:customers:edit');

  return (
    <header className="sticky top-0 z-40 glass-header">
      <div className="px-6 py-4">
        <div className="flex items-center justify-between">
          {/* Title Section */}
          <div className="flex items-center gap-4">
            <div className="w-10 h-10 rounded-xl bg-amber-500/10 flex items-center justify-center">
              <ShoppingCart className="w-5 h-5 text-amber-400" />
            </div>
            <div>
              <h1 className="text-xl font-semibold text-foreground">Sales CRM</h1>
              <p className="text-sm text-muted-foreground">
                Manage customers, orders, and compliance
              </p>
            </div>
          </div>

          {/* Search + Actions */}
          <div className="flex items-center gap-3">
            {/* Global Search */}
            <div className="relative hidden md:block">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
              <input
                type="text"
                placeholder="Search customers, orders..."
                className="w-64 pl-10 pr-4 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-amber-500/30 focus:ring-2 focus:ring-amber-500/10"
              />
            </div>

            {/* Quick Actions */}
            {canCreateCustomer && (
              <Link
                href="/sales/customers/new"
                className="flex items-center gap-2 px-4 py-2 rounded-lg bg-muted/50 text-foreground hover:bg-muted transition-colors"
              >
                <Users className="w-4 h-4" />
                <span className="text-sm font-medium hidden lg:inline">New Customer</span>
              </Link>
            )}

            {canCreateOrder && (
              <Link
                href="/sales/orders/new"
                className="flex items-center gap-2 px-4 py-2 rounded-lg bg-amber-500/10 text-amber-400 hover:bg-amber-500/20 transition-colors"
              >
                <Plus className="w-4 h-4" />
                <span className="text-sm font-medium">New Order</span>
              </Link>
            )}

            {onRefresh && (
              <button
                onClick={onRefresh}
                disabled={isRefreshing}
                className="p-2 rounded-lg hover:bg-muted/50 text-muted-foreground hover:text-foreground transition-colors disabled:opacity-50"
                aria-label="Refresh data"
              >
                <RefreshCw className={`w-5 h-5 ${isRefreshing ? 'animate-spin' : ''}`} />
              </button>
            )}
          </div>
        </div>
      </div>
    </header>
  );
}
