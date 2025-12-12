/**
 * App Route Client
 *
 * Calls same-origin Next.js route handlers (e.g. /api/v1/...).
 * Automatically includes JWT token, site context headers, and optional X-User-Id.
 */

'use client';

import { getSupabaseClient } from '@/lib/supabase/client';
import { useAuthStore, UserRole } from '@/stores/auth/authStore';

/**
 * Maps frontend UserRole to backend-expected role strings.
 * Backend accepts: operator, supervisor, manager, admin, service_account
 */
function mapRoleToBackendRole(role: UserRole): string | null {
  switch (role) {
    case UserRole.SuperAdmin:
    case UserRole.Admin:
      return 'admin';
    case UserRole.CultivationManager:
      return 'manager';
    case UserRole.ComplianceOfficer:
      return 'supervisor';
    case UserRole.Grower:
    case UserRole.Technician:
      return 'operator';
    case UserRole.Viewer:
      return 'operator'; // Read-only access
    default:
      return null;
  }
}

export interface AppRouteClientOptions extends RequestInit {
  siteId?: string;
  userId?: string;
  skipAuth?: boolean;
}

export async function appRouteClient<T = unknown>(
  endpoint: string,
  options: AppRouteClientOptions = {}
): Promise<T> {
  const { siteId, userId, skipAuth = false, ...fetchOptions } = options;

  const headers = new Headers(fetchOptions.headers);

  if (!headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json');
  }

  if (!skipAuth) {
    const supabase = getSupabaseClient();
    if (supabase) {
      const {
        data: { session },
      } = await supabase.auth.getSession();
      if (session?.access_token) {
        headers.set('Authorization', `Bearer ${session.access_token}`);
      }
    }
  }

  const resolvedSiteId =
    siteId ??
    (typeof window !== 'undefined'
      ? localStorage.getItem('harvestry-current-site-id') ?? undefined
      : undefined);
  if (resolvedSiteId) {
    headers.set('X-Site-Id', resolvedSiteId);
  }

  const storeUser = useAuthStore.getState().user;
  const resolvedUserId = userId ?? storeUser?.id ?? undefined;
  if (resolvedUserId) {
    headers.set('X-User-Id', resolvedUserId);
  }

  // For development/mock auth mode, send role header for backend authentication
  const userRole = storeUser?.role;
  if (userRole) {
    // Map frontend roles to backend expected roles
    const backendRole = mapRoleToBackendRole(userRole);
    if (backendRole) {
      headers.set('X-User-Role', backendRole);
    }
  }

  const url = endpoint.startsWith('/') ? endpoint : `/${endpoint}`;

  const response = await fetch(url, {
    ...fetchOptions,
    headers,
  });

  if (!response.ok) {
    let body: unknown;
    try {
      body = await response.json();
    } catch {
      body = await response.text();
    }

    const message =
      typeof body === 'object' && body !== null && 'error' in body
        ? String((body as { error: unknown }).error)
        : `Request failed: ${response.status} ${response.statusText}`;
    throw new Error(message);
  }

  if (response.status === 204) return undefined as T;

  const contentType = response.headers.get('Content-Type') ?? '';
  if (!contentType.includes('application/json')) {
    return (await response.text()) as unknown as T;
  }

  return response.json();
}

export const appApi = {
  get: <T = unknown>(endpoint: string, options?: AppRouteClientOptions) =>
    appRouteClient<T>(endpoint, { ...options, method: 'GET' }),

  post: <T = unknown>(endpoint: string, data?: unknown, options?: AppRouteClientOptions) =>
    appRouteClient<T>(endpoint, {
      ...options,
      method: 'POST',
      body: data ? JSON.stringify(data) : undefined,
    }),

  put: <T = unknown>(endpoint: string, data?: unknown, options?: AppRouteClientOptions) =>
    appRouteClient<T>(endpoint, {
      ...options,
      method: 'PUT',
      body: data ? JSON.stringify(data) : undefined,
    }),
};

