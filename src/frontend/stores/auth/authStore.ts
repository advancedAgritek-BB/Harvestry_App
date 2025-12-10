/**
 * Auth Store
 * 
 * Zustand store for managing user authentication and authorization.
 * Handles user roles, site permissions, feature flag access, and pricing tiers.
 */

import { create } from 'zustand';
import { persist } from 'zustand/middleware';

// ============================================================================
// TYPES
// ============================================================================

export enum UserRole {
  SuperAdmin = 'SuperAdmin',
  Admin = 'Admin',
  CultivationManager = 'CultivationManager',
  Grower = 'Grower',
  ComplianceOfficer = 'ComplianceOfficer',
  Technician = 'Technician',
  Viewer = 'Viewer'
}

export type PricingTier = 'monitor' | 'foundation' | 'growth' | 'enterprise';

export interface SitePermissions {
  siteId: string;
  siteName: string;
  canAccessSimulator: boolean;
  canConfigureSensors: boolean;
  enabledFeatureFlags: string[];
}

export interface User {
  id: string;
  email: string;
  name: string;
  role: UserRole;
  avatarUrl?: string;
  sitePermissions: SitePermissions[];
}

// ============================================================================
// TIER FEATURE MAP
// ============================================================================

export type TierFeature = 
  | 'dashboard'
  | 'monitoring'
  | 'historical_data'
  | 'batch_tracking'
  | 'sop_engine'
  | 'inventory'
  | 'task_management'
  | 'control'
  | 'automation'
  | 'compliance'
  | 'financials'
  | 'production_planning'
  | 'labels'
  | 'ai_autosteer'
  | 'ai_insights'
  | 'multi_site'
  | 'custom_bi'
  | 'sso';

/**
 * Map of pricing tiers to enabled features.
 * Each tier includes all features from lower tiers.
 */
export const TIER_FEATURES: Record<PricingTier, TierFeature[]> = {
  monitor: [
    'dashboard',
    'monitoring',
  ],
  foundation: [
    'dashboard',
    'monitoring',
    'historical_data',
    'batch_tracking',
    'sop_engine',
    'inventory',
    'task_management',
  ],
  growth: [
    'dashboard',
    'monitoring',
    'historical_data',
    'batch_tracking',
    'sop_engine',
    'inventory',
    'task_management',
    'control',
    'automation',
    'compliance',
    'financials',
    'production_planning',
    'labels',
  ],
  enterprise: [
    'dashboard',
    'monitoring',
    'historical_data',
    'batch_tracking',
    'sop_engine',
    'inventory',
    'task_management',
    'control',
    'automation',
    'compliance',
    'financials',
    'production_planning',
    'labels',
    'ai_autosteer',
    'ai_insights',
    'multi_site',
    'custom_bi',
    'sso',
  ],
};

/**
 * Human-readable tier labels
 */
export const TIER_LABELS: Record<PricingTier, string> = {
  monitor: 'Monitor (Free)',
  foundation: 'Foundation',
  growth: 'Growth',
  enterprise: 'Enterprise',
};

export interface AuthState {
  // Current user
  user: User | null;
  isAuthenticated: boolean;
  
  // Pricing tier (for demo purposes)
  currentTier: PricingTier;
  
  // Selected site context
  currentSiteId: string | null;
  
  // Loading state
  isLoading: boolean;
  
  // Actions
  setUser: (user: User | null) => void;
  setCurrentSite: (siteId: string | null) => void;
  setLoading: (loading: boolean) => void;
  logout: () => void;
  
  // Tier management (for demo)
  setCurrentTier: (tier: PricingTier) => void;
  hasFeature: (feature: TierFeature) => boolean;
  
  // Permission checks
  isSuperAdmin: () => boolean;
  isAdmin: () => boolean;
  canAccessAdminPanel: () => boolean;
  canAccessSimulator: () => boolean;
  canConfigureSensors: (siteId?: string) => boolean;
  canManageFeatureFlags: () => boolean;
  canAssignSiteFeatures: () => boolean;
  getSitePermissions: (siteId: string) => SitePermissions | undefined;
  hasFeatureFlag: (siteId: string, flagId: string) => boolean;
  
  // Site permissions management (SuperAdmin only)
  updateSiteSimulatorAccess: (siteId: string, canAccess: boolean) => void;
  updateSiteSensorConfigAccess: (siteId: string, canAccess: boolean) => void;
  updateSiteFeatureFlags: (siteId: string, flags: string[]) => void;
}

// ============================================================================
// ENVIRONMENT
// ============================================================================

/**
 * Check if Supabase is configured.
 */
const SUPABASE_CONFIGURED = !!(
  process.env.NEXT_PUBLIC_SUPABASE_URL && 
  process.env.NEXT_PUBLIC_SUPABASE_ANON_KEY
);

/**
 * Check if mock auth is explicitly enabled or if we should auto-enable it.
 * 
 * Mock auth is enabled when:
 * 1. NEXT_PUBLIC_USE_MOCK_AUTH=true is explicitly set, OR
 * 2. Supabase is not configured (auto-fallback for development)
 */
const USE_MOCK_AUTH = 
  process.env.NEXT_PUBLIC_USE_MOCK_AUTH === 'true' || 
  !SUPABASE_CONFIGURED;

/**
 * Mock user for development only.
 * This is used when mock auth is enabled (either explicitly or auto-fallback).
 */
const DEV_MOCK_USER: User | null = USE_MOCK_AUTH ? {
  id: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
  email: 'dev@harvestry.io',
  name: 'Dev User',
  role: UserRole.SuperAdmin,
  avatarUrl: '/images/user-avatar.png',
  sitePermissions: [
    {
      siteId: 'site-1',
      siteName: 'Dev Site',
      canAccessSimulator: true,
      canConfigureSensors: true,
      enabledFeatureFlags: ['closed_loop_ecph_enabled', 'sms_critical_enabled']
    }
  ]
} : null;

// ============================================================================
// STORE
// ============================================================================

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      // Initial state - starts unauthenticated unless mock auth is enabled
      user: DEV_MOCK_USER,
      isAuthenticated: !!DEV_MOCK_USER,
      currentSiteId: DEV_MOCK_USER?.sitePermissions[0]?.siteId || null,
      currentTier: 'growth', // Default to Growth for demo
      isLoading: !DEV_MOCK_USER, // Loading until auth provider initializes
      
      // Actions
      setUser: (user) => {
        const currentSiteId = user?.sitePermissions[0]?.siteId || null;
        // Persist site ID for API client
        if (typeof window !== 'undefined') {
          if (currentSiteId) {
            localStorage.setItem('harvestry-current-site-id', currentSiteId);
          } else {
            localStorage.removeItem('harvestry-current-site-id');
          }
        }
        set({ 
          user, 
          isAuthenticated: !!user,
          currentSiteId,
          isLoading: false
        });
      },
      
      setCurrentSite: (siteId) => {
        // Persist site ID for API client
        if (typeof window !== 'undefined') {
          if (siteId) {
            localStorage.setItem('harvestry-current-site-id', siteId);
          } else {
            localStorage.removeItem('harvestry-current-site-id');
          }
        }
        set({ currentSiteId: siteId });
      },
      
      setLoading: (loading) => set({ isLoading: loading }),
      
      logout: () => {
        // Clear persisted site ID
        if (typeof window !== 'undefined') {
          localStorage.removeItem('harvestry-current-site-id');
        }
        set({ 
          user: null, 
          isAuthenticated: false, 
          currentSiteId: null,
          isLoading: false
        });
      },
      
      // Tier management
      setCurrentTier: (tier) => set({ currentTier: tier }),
      
      hasFeature: (feature) => {
        const { currentTier } = get();
        return TIER_FEATURES[currentTier].includes(feature);
      },
      
      // Permission checks
      isSuperAdmin: () => {
        const { user } = get();
        return user?.role === UserRole.SuperAdmin;
      },
      
      isAdmin: () => {
        const { user } = get();
        return user?.role === UserRole.SuperAdmin || user?.role === UserRole.Admin;
      },
      
      canAccessAdminPanel: () => {
        const { user } = get();
        if (!user) return false;
        // All admin roles can view the admin panel
        return [
          UserRole.SuperAdmin,
          UserRole.Admin,
          UserRole.CultivationManager,
          UserRole.ComplianceOfficer
        ].includes(user.role);
      },
      
      canAccessSimulator: () => {
        // Simulator is Super Admin only
        return get().isSuperAdmin();
      },
      
      canConfigureSensors: (siteId?: string) => {
        const { user, currentSiteId, isSuperAdmin, isAdmin } = get();
        if (!user) return false;
        
        // Super admin and admin always have sensor config access
        if (isSuperAdmin() || isAdmin()) return true;
        
        // Cultivation Manager and Technician roles can configure by default
        if ([UserRole.CultivationManager, UserRole.Technician].includes(user.role)) {
          // But check site-specific override if exists
          const targetSiteId = siteId || currentSiteId;
          if (targetSiteId) {
            const sitePerms = user.sitePermissions.find(sp => sp.siteId === targetSiteId);
            // If site permissions exist and explicitly deny, respect that
            if (sitePerms && sitePerms.canConfigureSensors === false) return false;
          }
          return true;
        }
        
        // For other roles (Grower, ComplianceOfficer, Viewer), check site-specific permissions
        const targetSiteId = siteId || currentSiteId;
        if (!targetSiteId) return false;
        
        const sitePerms = user.sitePermissions.find(sp => sp.siteId === targetSiteId);
        return sitePerms?.canConfigureSensors ?? false;
      },
      
      canManageFeatureFlags: () => {
        return get().isSuperAdmin();
      },
      
      canAssignSiteFeatures: () => {
        return get().isSuperAdmin();
      },
      
      getSitePermissions: (siteId: string) => {
        const { user } = get();
        return user?.sitePermissions.find(sp => sp.siteId === siteId);
      },
      
      hasFeatureFlag: (siteId: string, flagId: string) => {
        const { user, isSuperAdmin } = get();
        if (!user) return false;
        
        // Super admin has all flags
        if (isSuperAdmin()) return true;
        
        const sitePerms = user.sitePermissions.find(sp => sp.siteId === siteId);
        return sitePerms?.enabledFeatureFlags.includes(flagId) ?? false;
      },
      
      // Site permissions management (SuperAdmin only)
      updateSiteSimulatorAccess: (siteId: string, canAccess: boolean) => {
        const { user, isSuperAdmin } = get();
        if (!user || !isSuperAdmin()) return;
        
        const updatedPermissions = user.sitePermissions.map(sp => 
          sp.siteId === siteId 
            ? { ...sp, canAccessSimulator: canAccess }
            : sp
        );
        
        set({ user: { ...user, sitePermissions: updatedPermissions } });
      },
      
      updateSiteSensorConfigAccess: (siteId: string, canAccess: boolean) => {
        const { user, isSuperAdmin } = get();
        if (!user || !isSuperAdmin()) return;
        
        const updatedPermissions = user.sitePermissions.map(sp => 
          sp.siteId === siteId 
            ? { ...sp, canConfigureSensors: canAccess }
            : sp
        );
        
        set({ user: { ...user, sitePermissions: updatedPermissions } });
      },
      
      updateSiteFeatureFlags: (siteId: string, flags: string[]) => {
        const { user, isSuperAdmin } = get();
        if (!user || !isSuperAdmin()) return;
        
        const updatedPermissions = user.sitePermissions.map(sp => 
          sp.siteId === siteId 
            ? { ...sp, enabledFeatureFlags: flags }
            : sp
        );
        
        set({ user: { ...user, sitePermissions: updatedPermissions } });
      }
    }),
    {
      name: 'harvestry-auth',
      partialize: (state) => ({
        user: state.user,
        isAuthenticated: state.isAuthenticated,
        currentSiteId: state.currentSiteId,
        currentTier: state.currentTier,
      })
    }
  )
);

// ============================================================================
// HOOKS
// ============================================================================

/** Hook to check if current user is a super admin */
export const useIsSuperAdmin = () => useAuthStore((state) => state.isSuperAdmin());

/** Hook to check if current user is any admin role */
export const useIsAdmin = () => useAuthStore((state) => state.isAdmin());

/** Hook to check simulator access (Super Admin only) */
export const useCanAccessSimulator = () => 
  useAuthStore((state) => state.canAccessSimulator());

/** Hook to check sensor configuration permission for current/specified site */
export const useCanConfigureSensors = (siteId?: string) => 
  useAuthStore((state) => state.canConfigureSensors(siteId));

/** Hook to check feature flag management permission */
export const useCanManageFeatureFlags = () => 
  useAuthStore((state) => state.canManageFeatureFlags());

/** Hook to get current pricing tier */
export const useCurrentTier = () => 
  useAuthStore((state) => state.currentTier);

/** Hook to check if a feature is available in current tier */
export const useHasFeature = (feature: TierFeature) => 
  useAuthStore((state) => state.hasFeature(feature));

// ============================================================================
// DASHBOARD PERMISSION HOOKS
// ============================================================================

/** Roles that can view alerts on the dashboard */
const ALERT_VIEWER_ROLES = [
  UserRole.SuperAdmin,
  UserRole.Admin,
  UserRole.CultivationManager,
  UserRole.Grower,
  UserRole.ComplianceOfficer,
  UserRole.Technician,
];

/** Roles that can view tasks on the dashboard */
const TASK_VIEWER_ROLES = [
  UserRole.SuperAdmin,
  UserRole.Admin,
  UserRole.CultivationManager,
  UserRole.Grower,
  UserRole.Technician,
];

/** Roles that can view irrigation data on the dashboard */
const IRRIGATION_VIEWER_ROLES = [
  UserRole.SuperAdmin,
  UserRole.Admin,
  UserRole.CultivationManager,
  UserRole.Grower,
  UserRole.Technician,
];

/** Roles that can view AI suggestions on the dashboard */
const AI_SUGGESTIONS_ROLES = [
  UserRole.SuperAdmin,
  UserRole.Admin,
  UserRole.CultivationManager,
];

/** Hook to check if user can view alerts */
export const useCanViewAlerts = () => {
  const user = useAuthStore((state) => state.user);
  return user ? ALERT_VIEWER_ROLES.includes(user.role) : false;
};

/** Hook to check if user can view tasks */
export const useCanViewTasks = () => {
  const user = useAuthStore((state) => state.user);
  return user ? TASK_VIEWER_ROLES.includes(user.role) : false;
};

/** Hook to check if user can view irrigation data */
export const useCanViewIrrigation = () => {
  const user = useAuthStore((state) => state.user);
  return user ? IRRIGATION_VIEWER_ROLES.includes(user.role) : false;
};

/** Hook to check if user can view AI suggestions */
export const useCanViewAISuggestions = () => {
  const user = useAuthStore((state) => state.user);
  return user ? AI_SUGGESTIONS_ROLES.includes(user.role) : false;
};

/** Combined hook for all dashboard permissions */
export const useDashboardPermissions = () => {
  const user = useAuthStore((state) => state.user);
  
  return {
    canViewAlerts: user ? ALERT_VIEWER_ROLES.includes(user.role) : false,
    canViewTasks: user ? TASK_VIEWER_ROLES.includes(user.role) : false,
    canViewIrrigation: user ? IRRIGATION_VIEWER_ROLES.includes(user.role) : false,
    canViewAISuggestions: user ? AI_SUGGESTIONS_ROLES.includes(user.role) : false,
    isAuthenticated: !!user,
    userRole: user?.role ?? null,
  };
};
