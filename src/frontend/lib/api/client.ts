/**
 * API Client
 * 
 * Provides authenticated fetch requests to the backend API.
 * Automatically includes JWT token and site context headers.
 */

import { getSupabaseClient } from '@/lib/supabase/client';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || '';

export interface ApiClientOptions extends RequestInit {
  /** Override the site ID header */
  siteId?: string;
  /** Skip authentication (for public endpoints) */
  skipAuth?: boolean;
}

export interface ApiError extends Error {
  status: number;
  statusText: string;
  body?: unknown;
}

/**
 * Creates an authenticated API request.
 * Automatically injects authorization header and site context.
 */
export async function apiClient<T = unknown>(
  endpoint: string,
  options: ApiClientOptions = {}
): Promise<T> {
  const { siteId, skipAuth = false, ...fetchOptions } = options;

  const headers = new Headers(fetchOptions.headers);
  
  // Set content type if not already set
  if (!headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json');
  }

  // Add authentication header
  if (!skipAuth) {
    const supabase = getSupabaseClient();
    const { data: { session } } = await supabase.auth.getSession();

    if (session?.access_token) {
      headers.set('Authorization', `Bearer ${session.access_token}`);
    }
  }

  // Add site context header
  if (siteId) {
    headers.set('X-Site-Id', siteId);
  } else {
    // Try to get from localStorage (set by authStore)
    const storedSiteId = typeof window !== 'undefined' 
      ? localStorage.getItem('harvestry-current-site-id')
      : null;
    if (storedSiteId) {
      headers.set('X-Site-Id', storedSiteId);
    }
  }

  const url = endpoint.startsWith('http') 
    ? endpoint 
    : `${API_BASE_URL}${endpoint}`;

  const response = await fetch(url, {
    ...fetchOptions,
    headers,
  });

  // Handle token expiry
  if (response.status === 401) {
    const tokenExpired = response.headers.get('Token-Expired');
    
    if (tokenExpired === 'true') {
      // Try to refresh the session
      const supabase = getSupabaseClient();
      const { error } = await supabase.auth.refreshSession();
      
      if (!error) {
        // Retry the request with the new token
        return apiClient<T>(endpoint, options);
      }
    }
    
    // Redirect to login if unable to refresh
    if (typeof window !== 'undefined') {
      window.location.href = '/login?expired=true';
    }
    
    throw createApiError(response, 'Session expired. Please log in again.');
  }

  // Handle other error responses
  if (!response.ok) {
    let errorBody: unknown;
    try {
      errorBody = await response.json();
    } catch {
      errorBody = await response.text();
    }
    
    const message = typeof errorBody === 'object' && errorBody !== null && 'error' in errorBody
      ? String((errorBody as { error: unknown }).error)
      : `Request failed: ${response.statusText}`;
    
    throw createApiError(response, message, errorBody);
  }

  // Handle empty responses
  const contentType = response.headers.get('Content-Type');
  if (response.status === 204 || !contentType?.includes('application/json')) {
    return undefined as T;
  }

  return response.json();
}

function createApiError(
  response: Response,
  message: string,
  body?: unknown
): ApiError {
  const error = new Error(message) as ApiError;
  error.status = response.status;
  error.statusText = response.statusText;
  error.body = body;
  return error;
}

// Convenience methods
export const api = {
  get: <T = unknown>(endpoint: string, options?: ApiClientOptions) =>
    apiClient<T>(endpoint, { ...options, method: 'GET' }),
    
  post: <T = unknown>(endpoint: string, data?: unknown, options?: ApiClientOptions) =>
    apiClient<T>(endpoint, {
      ...options,
      method: 'POST',
      body: data ? JSON.stringify(data) : undefined,
    }),
    
  put: <T = unknown>(endpoint: string, data?: unknown, options?: ApiClientOptions) =>
    apiClient<T>(endpoint, {
      ...options,
      method: 'PUT',
      body: data ? JSON.stringify(data) : undefined,
    }),
    
  patch: <T = unknown>(endpoint: string, data?: unknown, options?: ApiClientOptions) =>
    apiClient<T>(endpoint, {
      ...options,
      method: 'PATCH',
      body: data ? JSON.stringify(data) : undefined,
    }),
    
  delete: <T = unknown>(endpoint: string, options?: ApiClientOptions) =>
    apiClient<T>(endpoint, { ...options, method: 'DELETE' }),
};



