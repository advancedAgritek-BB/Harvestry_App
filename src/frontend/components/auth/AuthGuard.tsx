'use client';

/**
 * Auth Guard Component
 * 
 * Wraps protected pages to ensure user is authenticated.
 * Shows loading state while checking authentication.
 * Redirects to login if not authenticated.
 * 
 * In mock auth mode, uses the authStore to check authentication.
 */

import { useEffect, type ReactNode } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/providers/AuthProvider';
import { useAuthStore } from '@/stores/auth/authStore';

interface AuthGuardProps {
  children: ReactNode;
  fallback?: ReactNode;
}

export function AuthGuard({ children, fallback }: AuthGuardProps) {
  const router = useRouter();
  const { session, isLoading, isMockAuth } = useAuth();
  const storeUser = useAuthStore((state) => state.user);
  const storeIsAuthenticated = useAuthStore((state) => state.isAuthenticated);

  // In mock auth mode, check the store directly
  const isAuthenticated = isMockAuth ? storeIsAuthenticated : !!session;
  const hasUser = isMockAuth ? !!storeUser : !!session;

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.push('/login');
    }
  }, [isAuthenticated, isLoading, router]);

  // Show loading state or custom fallback
  if (isLoading) {
    return fallback || <AuthLoadingScreen />;
  }

  // Not authenticated - don't render children while redirecting
  if (!hasUser) {
    return fallback || <AuthLoadingScreen />;
  }

  return <>{children}</>;
}

/**
 * Default loading screen shown while checking authentication.
 */
function AuthLoadingScreen() {
  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-slate-900 via-emerald-950 to-slate-900">
      <div className="absolute inset-0 bg-[url('/grid-pattern.svg')] opacity-5" />
      
      <div className="relative flex flex-col items-center gap-4">
        {/* Logo */}
        <div className="inline-flex items-center justify-center w-16 h-16 rounded-2xl bg-emerald-500/10 border border-emerald-500/20">
          <svg
            className="w-8 h-8 text-emerald-400"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253"
            />
          </svg>
        </div>
        
        {/* Loading spinner */}
        <div className="flex items-center gap-3">
          <svg
            className="animate-spin h-5 w-5 text-emerald-400"
            fill="none"
            viewBox="0 0 24 24"
          >
            <circle
              className="opacity-25"
              cx="12"
              cy="12"
              r="10"
              stroke="currentColor"
              strokeWidth="4"
            />
            <path
              className="opacity-75"
              fill="currentColor"
              d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
            />
          </svg>
          <span className="text-slate-400 text-sm">Loading...</span>
        </div>
      </div>
    </div>
  );
}

