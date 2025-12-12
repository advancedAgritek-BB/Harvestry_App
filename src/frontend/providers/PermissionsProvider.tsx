'use client';

import React, { createContext, useContext, useMemo } from 'react';
import { useAuthStore, UserRole } from '@/stores/auth/authStore';

type PermissionsContextValue = {
  has: (permissionKey: string) => boolean;
  any: (permissionKeys: string[]) => boolean;
};

const PermissionsContext = createContext<PermissionsContextValue | null>(null);

const ROLE_DEFAULT_PERMISSIONS: Record<UserRole, string[]> = {
  [UserRole.SuperAdmin]: ['*'],
  [UserRole.Admin]: ['*'],
  [UserRole.CultivationManager]: [
    'dashboard:view',
    'inventory:view',
    'sales:dashboard:view',
    'sales:orders:view',
  ],
  [UserRole.Grower]: ['dashboard:view', 'inventory:view'],
  [UserRole.ComplianceOfficer]: [
    'dashboard:view',
    'inventory:view',
    'sales:dashboard:view',
    'sales:customers:view',
    'sales:orders:view',
    'sales:shipments:create',
    'sales:transfers:view',
    'sales:reports:view',
    'transfers:view',
    'transfers:create',
    'compliance:metrc-submit',
  ],
  [UserRole.Technician]: ['dashboard:view', 'inventory:view'],
  [UserRole.Viewer]: [
    'dashboard:view',
    'inventory:view',
    'sales:dashboard:view',
    'sales:customers:view',
    'sales:orders:view',
    'sales:transfers:view',
    'transfers:view',
  ],
};

export function PermissionsProvider({ children }: { children: React.ReactNode }) {
  const user = useAuthStore((s) => s.user);

  const value = useMemo<PermissionsContextValue>(() => {
    const role = user?.role ?? UserRole.Viewer;
    const defaults = ROLE_DEFAULT_PERMISSIONS[role] ?? [];
    const isWildcard = defaults.includes('*');
    const set = new Set(defaults);

    const has = (permissionKey: string) => {
      if (isWildcard) return true;
      return set.has(permissionKey);
    };

    const any = (permissionKeys: string[]) => {
      if (isWildcard) return true;
      return permissionKeys.some((k) => set.has(k));
    };

    return { has, any };
  }, [user?.role]);

  return <PermissionsContext.Provider value={value}>{children}</PermissionsContext.Provider>;
}

export function usePermissions(): PermissionsContextValue {
  const ctx = useContext(PermissionsContext);
  if (!ctx) {
    // Safe default: deny.
    return {
      has: () => false,
      any: () => false,
    };
  }
  return ctx;
}

