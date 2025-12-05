'use client';

/**
 * Auth Provider
 * 
 * Provides authentication context to the application.
 * Supports two modes:
 * 1. Supabase Auth - Real authentication via Supabase
 * 2. Mock Auth - Development mode using mock user from authStore
 */

import { createContext, useContext, useEffect, useState, useCallback, type ReactNode } from 'react';
import type { Session, User, AuthError } from '@supabase/supabase-js';
import { getSupabaseClient, isMockAuthEnabled, isSupabaseConfigured } from '@/lib/supabase/client';
import { useAuthStore } from '@/stores/auth/authStore';

// ============================================================================
// TYPES
// ============================================================================

interface AuthContextType {
  /** Current Supabase session */
  session: Session | null;
  /** Current Supabase user */
  user: User | null;
  /** Whether auth state is being loaded */
  isLoading: boolean;
  /** Whether mock auth is active */
  isMockAuth: boolean;
  /** Sign in with email and password */
  signIn: (email: string, password: string) => Promise<{ error: AuthError | null }>;
  /** Sign up with email and password */
  signUp: (email: string, password: string, metadata?: UserMetadata) => Promise<{ error: AuthError | null }>;
  /** Sign out the current user */
  signOut: () => Promise<{ error: AuthError | null }>;
  /** Send password reset email */
  resetPassword: (email: string) => Promise<{ error: AuthError | null }>;
  /** Update password */
  updatePassword: (newPassword: string) => Promise<{ error: AuthError | null }>;
}

interface UserMetadata {
  firstName?: string;
  lastName?: string;
  organizationName?: string;
}

// ============================================================================
// CONTEXT
// ============================================================================

const AuthContext = createContext<AuthContextType | undefined>(undefined);

// ============================================================================
// MOCK AUTH ERROR
// ============================================================================

const createMockError = (message: string): AuthError => ({
  message,
  status: 400,
  name: 'AuthError',
} as AuthError);

// ============================================================================
// PROVIDER
// ============================================================================

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [session, setSession] = useState<Session | null>(null);
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isMockAuth, setIsMockAuth] = useState(false);
  
  const storeUser = useAuthStore((state) => state.user);
  const clearStoreUser = useAuthStore((state) => state.logout);

  /**
   * Initialize auth state on mount.
   */
  useEffect(() => {
    const mockAuthEnabled = isMockAuthEnabled();
    const supabaseConfigured = isSupabaseConfigured();
    
    // If mock auth is enabled or Supabase isn't configured, use mock mode
    if (mockAuthEnabled || !supabaseConfigured) {
      setIsMockAuth(true);
      
      if (!supabaseConfigured && !mockAuthEnabled) {
        console.warn(
          '[Auth] Supabase not configured and mock auth not enabled. ' +
          'Set NEXT_PUBLIC_USE_MOCK_AUTH=true for development.'
        );
      }
      
      // In mock mode, the authStore already has the mock user
      // Just mark loading as complete
      setIsLoading(false);
      return;
    }
    
    // Real Supabase auth mode
    const supabase = getSupabaseClient();
    if (!supabase) {
      setIsLoading(false);
      return;
    }
    
    // Get initial session
    supabase.auth.getSession().then(({ data: { session } }) => {
      setSession(session);
      setUser(session?.user ?? null);
      setIsLoading(false);
    });

    // Listen for auth changes
    const { data: { subscription } } = supabase.auth.onAuthStateChange(
      async (_event, session) => {
        setSession(session);
        setUser(session?.user ?? null);
      }
    );

    return () => {
      subscription.unsubscribe();
    };
  }, []);

  /**
   * Sign in with email and password.
   */
  const signIn = useCallback(async (email: string, password: string) => {
    if (isMockAuth) {
      // Mock auth - just mark as "signed in" (store already has user)
      console.info('[Mock Auth] Sign in:', email);
      return { error: null };
    }
    
    setIsLoading(true);
    const supabase = getSupabaseClient();
    
    if (!supabase) {
      setIsLoading(false);
      return { error: createMockError('Supabase not configured') };
    }
    
    const { error } = await supabase.auth.signInWithPassword({
      email,
      password,
    });
    
    if (error) {
      setIsLoading(false);
    }
    
    return { error };
  }, [isMockAuth]);

  /**
   * Sign up with email and password.
   */
  const signUp = useCallback(async (
    email: string,
    password: string,
    metadata?: UserMetadata
  ) => {
    if (isMockAuth) {
      console.info('[Mock Auth] Sign up:', email, metadata);
      return { error: null };
    }
    
    setIsLoading(true);
    const supabase = getSupabaseClient();
    
    if (!supabase) {
      setIsLoading(false);
      return { error: createMockError('Supabase not configured') };
    }
    
    const { error } = await supabase.auth.signUp({
      email,
      password,
      options: {
        data: {
          first_name: metadata?.firstName,
          last_name: metadata?.lastName,
          organization_name: metadata?.organizationName,
        },
      },
    });
    
    if (error) {
      setIsLoading(false);
    }
    
    return { error };
  }, [isMockAuth]);

  /**
   * Sign out the current user.
   */
  const signOut = useCallback(async () => {
    if (isMockAuth) {
      console.info('[Mock Auth] Sign out');
      clearStoreUser();
      return { error: null };
    }
    
    const supabase = getSupabaseClient();
    
    if (!supabase) {
      clearStoreUser();
      return { error: null };
    }
    
    const { error } = await supabase.auth.signOut();
    
    if (!error) {
      clearStoreUser();
    }
    
    return { error };
  }, [isMockAuth, clearStoreUser]);

  /**
   * Send password reset email.
   */
  const resetPassword = useCallback(async (email: string) => {
    if (isMockAuth) {
      console.info('[Mock Auth] Reset password:', email);
      return { error: null };
    }
    
    const supabase = getSupabaseClient();
    
    if (!supabase) {
      return { error: createMockError('Supabase not configured') };
    }
    
    const { error } = await supabase.auth.resetPasswordForEmail(email, {
      redirectTo: `${window.location.origin}/reset-password`,
    });
    
    return { error };
  }, [isMockAuth]);

  /**
   * Update password (for logged-in users or after reset).
   */
  const updatePassword = useCallback(async (newPassword: string) => {
    if (isMockAuth) {
      console.info('[Mock Auth] Update password');
      return { error: null };
    }
    
    const supabase = getSupabaseClient();
    
    if (!supabase) {
      return { error: createMockError('Supabase not configured') };
    }
    
    const { error } = await supabase.auth.updateUser({
      password: newPassword,
    });
    
    return { error };
  }, [isMockAuth]);

  // In mock auth mode, consider user authenticated if store has user
  const effectiveSession = isMockAuth && storeUser ? {} as Session : session;
  const effectiveUser = isMockAuth && storeUser ? { id: storeUser.id, email: storeUser.email } as User : user;

  const value: AuthContextType = {
    session: effectiveSession,
    user: effectiveUser,
    isLoading,
    isMockAuth,
    signIn,
    signUp,
    signOut,
    resetPassword,
    updatePassword,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}

// ============================================================================
// HOOK
// ============================================================================

/**
 * Hook to access the auth context.
 * Must be used within an AuthProvider.
 */
export function useAuth() {
  const context = useContext(AuthContext);
  
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  
  return context;
}

