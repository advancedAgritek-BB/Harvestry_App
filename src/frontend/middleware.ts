/**
 * Next.js Middleware for Authentication
 * 
 * Handles route protection by checking Supabase session cookies.
 * Redirects unauthenticated users to login for protected routes.
 * 
 * In development with mock auth enabled, middleware is bypassed.
 */

import { createServerClient, type CookieOptions } from '@supabase/ssr';
import { NextResponse, type NextRequest } from 'next/server';

// Routes that require authentication
const PROTECTED_ROUTES = [
  '/dashboard',
  '/admin',
  '/inventory',
  '/sales',
  '/transfers',
  '/settings',
];

// Routes that should redirect to dashboard if already authenticated
const AUTH_ROUTES = [
  '/login',
  '/signup',
  '/forgot-password',
];

export async function middleware(request: NextRequest) {
  const response = NextResponse.next({
    request: {
      headers: request.headers,
    },
  });

  const supabaseUrl = process.env.NEXT_PUBLIC_SUPABASE_URL;
  const supabaseAnonKey = process.env.NEXT_PUBLIC_SUPABASE_ANON_KEY;
  const mockAuthEnabled = process.env.NEXT_PUBLIC_USE_MOCK_AUTH === 'true';

  // Skip middleware if mock auth is enabled (development mode)
  if (mockAuthEnabled) {
    return response;
  }

  // Skip middleware if Supabase is not configured
  if (!supabaseUrl || !supabaseAnonKey) {
    // In development, allow access without Supabase
    if (process.env.NODE_ENV === 'development') {
      return response;
    }
    // In production, this is a configuration error - allow through but log
    console.error('Supabase environment variables not configured in production');
    return response;
  }

  const supabase = createServerClient(
    supabaseUrl,
    supabaseAnonKey,
    {
      cookies: {
        get(name: string) {
          return request.cookies.get(name)?.value;
        },
        set(name: string, value: string, options: CookieOptions) {
          response.cookies.set({
            name,
            value,
            ...options,
          });
        },
        remove(name: string, options: CookieOptions) {
          response.cookies.set({
            name,
            value: '',
            ...options,
          });
        },
      },
    }
  );

  // Refresh session if expired
  const { data: { session } } = await supabase.auth.getSession();
  
  const pathname = request.nextUrl.pathname;

  // Check if the route is protected
  const isProtectedRoute = PROTECTED_ROUTES.some(route => 
    pathname.startsWith(route)
  );
  
  // Check if the route is an auth route
  const isAuthRoute = AUTH_ROUTES.some(route => 
    pathname === route || pathname.startsWith(route + '/')
  );

  // Redirect to login if accessing protected route without session
  if (isProtectedRoute && !session) {
    const redirectUrl = new URL('/login', request.url);
    redirectUrl.searchParams.set('redirect', pathname);
    return NextResponse.redirect(redirectUrl);
  }

  // Redirect to dashboard if accessing auth route with session
  if (isAuthRoute && session) {
    // Don't redirect from reset-password even with session
    // (user might be resetting password while logged in)
    if (!pathname.includes('reset-password')) {
      return NextResponse.redirect(new URL('/dashboard', request.url));
    }
  }

  return response;
}

export const config = {
  matcher: [
    /*
     * Match all request paths except:
     * - _next/static (static files)
     * - _next/image (image optimization files)
     * - favicon.ico (favicon file)
     * - public folder
     * - api routes (handled separately)
     */
    '/((?!_next/static|_next/image|favicon.ico|public|api).*)',
  ],
};

