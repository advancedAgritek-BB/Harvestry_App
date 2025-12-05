# Frontend Component Audit

**Audit Date:** December 4, 2025  
**Auditor:** AI Agent  
**Status:** Complete

---

## Executive Summary

The frontend is a Next.js 14 application using Zustand for state management. Authentication is currently **mocked with a hardcoded dev user** - there is no real login/signup flow. Services are well-structured and ready to connect to backend APIs once authentication is implemented.

### Key Findings

| Finding | Severity | Impact |
|---------|----------|--------|
| Hardcoded mock user in authStore | Critical | No real authentication |
| No login/signup UI | Critical | Users can't actually log in |
| Services ready for real APIs | ✅ | Good foundation |
| Mock data in dashboard/tasks | Medium | Needs API integration |

---

## 1. Authentication System

### 1.1 Current State: Mock Authentication

**File:** `src/frontend/stores/auth/authStore.ts`

```typescript
// HARDCODED DEV USER - Lines 79-100
const DEFAULT_DEV_USER: User = {
  id: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
  email: 'brandon@harvestry.io',
  name: 'Brandon Burnette',
  role: UserRole.SuperAdmin,
  sitePermissions: [
    {
      siteId: 'site-1',
      siteName: 'Evergreen',
      canAccessSimulator: true,
      canConfigureSensors: true,
      enabledFeatureFlags: ['closed_loop_ecph_enabled', 'sms_critical_enabled']
    },
    // ...
  ]
};

// Store initializes with mock user - Line 110
user: DEFAULT_DEV_USER,
isAuthenticated: true,  // Always authenticated in dev!
```

### 1.2 What's Missing

- No Supabase client integration
- No login page component
- No signup flow
- No password reset
- No logout (API call)
- No session refresh
- No protected route wrapper

### 1.3 Required Implementation

```typescript
// Future authStore structure with Supabase
import { createClient } from '@supabase/supabase-js';

const supabase = createClient(
  process.env.NEXT_PUBLIC_SUPABASE_URL!,
  process.env.NEXT_PUBLIC_SUPABASE_ANON_KEY!
);

// Actions to add:
signIn: async (email, password) => {
  const { data, error } = await supabase.auth.signInWithPassword({
    email, password
  });
  // ...
},
signUp: async (email, password) => {
  // ...
},
signOut: async () => {
  await supabase.auth.signOut();
  set({ user: null, isAuthenticated: false });
},
```

---

## 2. Feature Services Analysis

### 2.1 Inventory Services

**Directory:** `src/frontend/features/inventory/services/`

| Service | API Base | Status | Mock Data |
|---------|----------|--------|-----------|
| `inventory.service.ts` | `/api/inventory` | ✅ Ready | None |
| `location.service.ts` | `/api/inventory` | ✅ Ready | None |
| `bom.service.ts` | `/api/inventory` | ✅ Ready | None |
| `compliance.service.ts` | `/api/compliance` | ✅ Ready | None |
| `labels.service.ts` | `/api/labels` | ✅ Ready | None |
| `product.service.ts` | `/api/products` | ✅ Ready | None |
| `production.service.ts` | `/api/production` | ✅ Ready | None |
| `scanning.service.ts` | `/api/scanning` | ✅ Ready | None |

**Status:** All services use proper API endpoints with no hardcoded mock data.

### 2.2 Tasks Services

**Directory:** `src/frontend/features/tasks/services/`

| Service | API Base | Status | Mock Data |
|---------|----------|--------|-----------|
| `task.service.ts` | `/api/v1/sites/{siteId}/tasks` | ✅ Ready | None |
| `sop.service.ts` | `/api/v1/sites/{siteId}/sops` | ✅ Ready | None |
| `blueprint.service.ts` | `/api/v1/sites/{siteId}/blueprints` | ✅ Ready | None |
| `notification.service.ts` | `/api/v1/sites/{siteId}/notifications` | ✅ Ready | None |
| `taskRecommendation.service.ts` | `/api/v1/sites/{siteId}/recommendations` | ✅ Ready | None |

**Status:** All services properly structured with site-scoped endpoints.

### 2.3 Issue: Services Don't Send Auth Headers

Current services don't include authentication headers:

```typescript
// Current (no auth):
const response = await fetch(`${API_BASE}/sites/${siteId}/tasks`);

// Required (with auth):
const response = await fetch(`${API_BASE}/sites/${siteId}/tasks`, {
  headers: {
    'Authorization': `Bearer ${session.access_token}`,
    'X-Site-Id': currentSiteId,
  },
});
```

---

## 3. Mock Data Inventory

### 3.1 Tasks Dashboard

**File:** `src/frontend/app/dashboard/tasks/mockData.ts`

Contains:
- `MOCK_TASKS[]` - 7 example tasks with full data
- `MOCK_SOPS{}` - 3 detailed SOPs with steps
- `MOCK_APPLICATION_DETAILS{}` - Fertigation/IPM recipes
- `MOCK_COMMENTS{}` - Task comments

**Usage:** Used by task dashboard page for development.

### 3.2 Auth Store

**File:** `src/frontend/stores/auth/authStore.ts`

Contains:
- `DEFAULT_DEV_USER` - Hardcoded super admin user
- Fake site permissions for 'Evergreen' and 'Oakdale'

**Usage:** Used globally - ALL authenticated state is mock.

### 3.3 Dashboard Widgets

Some widgets may contain embedded mock data for charts/visualizations. These should be audited individually:

| Widget | File | Data Source |
|--------|------|-------------|
| `AlertsWidget` | `operations/AlertsWidget.tsx` | Needs API |
| `TaskQueueWidget` | `operations/TaskQueueWidget.tsx` | Needs API |
| `IrrigationStatusWidget` | `operations/IrrigationStatusWidget.tsx` | Needs API |
| `ActiveBatchesWidget` | `operations/ActiveBatchesWidget.tsx` | Needs API |
| `EnvironmentalMetricsWidget` | `cultivation/EnvironmentalMetricsWidget.tsx` | Needs API |
| `ZoneHeatmapWidget` | `cultivation/ZoneHeatmapWidget.tsx` | Needs API |

---

## 4. State Management (Zustand)

### 4.1 Auth Store

**File:** `src/frontend/stores/auth/authStore.ts`

| State | Type | Persisted |
|-------|------|-----------|
| `user` | `User \| null` | ✅ |
| `isAuthenticated` | `boolean` | ✅ |
| `currentSiteId` | `string \| null` | ✅ |
| `isLoading` | `boolean` | ❌ |

**Persistence:** Uses `zustand/middleware/persist` with localStorage key `harvestry-auth`.

### 4.2 Permission Hooks

Properly implemented permission checks:

```typescript
export const useIsSuperAdmin = () => useAuthStore((state) => state.isSuperAdmin());
export const useIsAdmin = () => useAuthStore((state) => state.isAdmin());
export const useCanAccessSimulator = () => useAuthStore((state) => state.canAccessSimulator());
export const useCanConfigureSensors = (siteId?: string) => useAuthStore((state) => state.canConfigureSensors(siteId));
export const useCanManageFeatureFlags = () => useAuthStore((state) => state.canManageFeatureFlags());
```

These are well-designed and should continue to work after real auth is implemented.

---

## 5. Page Structure

### 5.1 Dashboard Pages

```
src/frontend/app/
├── dashboard/
│   ├── page.tsx                # Main dashboard
│   ├── tasks/
│   │   ├── page.tsx            # Task list
│   │   └── mockData.ts         # ⚠️ Mock data
│   ├── irrigation/
│   │   └── page.tsx            # Irrigation dashboard
│   └── cultivation/
│       └── page.tsx            # Cultivation dashboard
├── admin/
│   ├── feature-flags/
│   ├── sensors/
│   ├── simulator/
│   └── users/                  # ⚠️ Needs real user management
├── inventory/
│   ├── page.tsx
│   ├── lots/
│   ├── movements/
│   └── locations/
└── features/
    └── genetics/
        └── page.tsx
```

### 5.2 Missing Auth Pages

Need to create:
- `/login` - Sign in page
- `/signup` - Registration page
- `/forgot-password` - Password reset
- `/reset-password` - Password reset confirmation

---

## 6. API Integration Patterns

### 6.1 Current Pattern (No Auth)

```typescript
export async function getTasks(siteId: string): Promise<TaskListResponse> {
  const response = await fetch(`${API_BASE}/sites/${siteId}/tasks`);
  if (!response.ok) throw new Error('Failed to fetch tasks');
  return response.json();
}
```

### 6.2 Required Pattern (With Auth)

```typescript
import { getSession } from '@/lib/auth'; // Supabase session helper

export async function getTasks(siteId: string): Promise<TaskListResponse> {
  const session = await getSession();
  if (!session) throw new Error('Not authenticated');
  
  const response = await fetch(`${API_BASE}/sites/${siteId}/tasks`, {
    headers: {
      'Authorization': `Bearer ${session.access_token}`,
      'Content-Type': 'application/json',
    },
  });
  
  if (response.status === 401) {
    // Handle token expiry
    await refreshSession();
    // Retry...
  }
  
  if (!response.ok) throw new Error('Failed to fetch tasks');
  return response.json();
}
```

### 6.3 Recommended: Create API Client Wrapper

```typescript
// src/frontend/lib/api-client.ts
import { useAuthStore } from '@/stores/auth/authStore';

const apiClient = {
  async fetch(url: string, options: RequestInit = {}) {
    const session = await supabase.auth.getSession();
    const siteId = useAuthStore.getState().currentSiteId;
    
    const headers = new Headers(options.headers);
    headers.set('Content-Type', 'application/json');
    
    if (session?.data.session?.access_token) {
      headers.set('Authorization', `Bearer ${session.data.session.access_token}`);
    }
    
    if (siteId) {
      headers.set('X-Site-Id', siteId);
    }
    
    return fetch(url, { ...options, headers });
  }
};

export default apiClient;
```

---

## 7. Environment Configuration

### 7.1 Current (`.env.local`)

```env
# Likely missing or minimal
NEXT_PUBLIC_API_URL=http://localhost:5000
```

### 7.2 Required for Supabase Auth

```env
# Supabase Auth (public - safe for browser)
NEXT_PUBLIC_SUPABASE_URL=https://your-project.supabase.co
NEXT_PUBLIC_SUPABASE_ANON_KEY=your-anon-key

# API Backend
NEXT_PUBLIC_API_URL=https://api.harvestry.io

# Optional: Analytics, etc.
NEXT_PUBLIC_SENTRY_DSN=...
```

---

## 8. Migration Plan

### 8.1 Phase 1: Create Supabase Client

```typescript
// src/frontend/lib/supabase.ts
import { createBrowserClient } from '@supabase/ssr';

export function createClient() {
  return createBrowserClient(
    process.env.NEXT_PUBLIC_SUPABASE_URL!,
    process.env.NEXT_PUBLIC_SUPABASE_ANON_KEY!
  );
}
```

### 8.2 Phase 2: Create Auth Provider

```typescript
// src/frontend/components/providers/AuthProvider.tsx
'use client';

import { createContext, useContext, useEffect } from 'react';
import { createClient } from '@/lib/supabase';
import { useAuthStore } from '@/stores/auth/authStore';

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const setUser = useAuthStore((state) => state.setUser);
  const supabase = createClient();
  
  useEffect(() => {
    // Listen for auth changes
    const { data: { subscription } } = supabase.auth.onAuthStateChange(
      async (event, session) => {
        if (session?.user) {
          // Fetch full user profile from API
          const profile = await fetchUserProfile(session.user.id);
          setUser(profile);
        } else {
          setUser(null);
        }
      }
    );
    
    return () => subscription.unsubscribe();
  }, []);
  
  return <>{children}</>;
}
```

### 8.3 Phase 3: Update Auth Store

Remove `DEFAULT_DEV_USER` and initialize with null:

```typescript
// Initial state
user: null,
isAuthenticated: false,
currentSiteId: null,
isLoading: true,  // Start loading until auth state is resolved
```

### 8.4 Phase 4: Create Auth UI

- Login page with email/password
- Signup page with organization setup
- Protected route wrapper
- Site selector component

---

## 9. Files to Modify

| File | Change |
|------|--------|
| `src/frontend/stores/auth/authStore.ts` | Remove mock user, add Supabase integration |
| `src/frontend/app/layout.tsx` | Add AuthProvider |
| `src/frontend/lib/supabase.ts` | Create (new file) |
| `src/frontend/lib/api-client.ts` | Create (new file) |
| `src/frontend/app/login/page.tsx` | Create (new file) |
| `src/frontend/app/signup/page.tsx` | Create (new file) |
| `src/frontend/features/*/services/*.ts` | Add auth headers to all API calls |
| `src/frontend/middleware.ts` | Add route protection |
| `.env.local` | Add Supabase environment variables |

---

## 10. Testing Checklist

After implementation, verify:

- [ ] Can sign up new user
- [ ] Can sign in existing user
- [ ] Can sign out
- [ ] Token persists across page refreshes
- [ ] Token refreshes automatically
- [ ] Unauthorized users redirected to login
- [ ] API calls include auth headers
- [ ] Site context passed to backend
- [ ] Permission checks work correctly
- [ ] Multi-site users can switch sites

---

*End of Frontend Component Audit*



