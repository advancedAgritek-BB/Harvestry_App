/**
 * Supabase Browser Client
 * 
 * Creates a Supabase client for browser-side authentication.
 * Uses the @supabase/ssr package for proper cookie handling with Next.js.
 */

import { createBrowserClient } from '@supabase/ssr';
import type { SupabaseClient } from '@supabase/supabase-js';
import type { Database } from './database.types';

/**
 * Check if Supabase is configured.
 */
export function isSupabaseConfigured(): boolean {
  return !!(
    process.env.NEXT_PUBLIC_SUPABASE_URL && 
    process.env.NEXT_PUBLIC_SUPABASE_ANON_KEY
  );
}

/**
 * Check if mock auth is enabled (for development without Supabase).
 * 
 * Mock auth is enabled when:
 * 1. NEXT_PUBLIC_USE_MOCK_AUTH=true is explicitly set, OR
 * 2. Supabase is not configured (auto-fallback for development)
 */
export function isMockAuthEnabled(): boolean {
  return process.env.NEXT_PUBLIC_USE_MOCK_AUTH === 'true' || !isSupabaseConfigured();
}

/**
 * Creates a Supabase client for browser usage.
 * This client is used for authentication and direct Supabase calls.
 * 
 * Note: For this architecture, we only use Supabase for authentication.
 * All data operations go through the .NET backend to AWS RDS.
 * 
 * Returns null if Supabase is not configured and mock auth is enabled.
 */
export function createClient(): SupabaseClient<Database> | null {
  const supabaseUrl = process.env.NEXT_PUBLIC_SUPABASE_URL;
  const supabaseAnonKey = process.env.NEXT_PUBLIC_SUPABASE_ANON_KEY;

  if (!supabaseUrl || !supabaseAnonKey) {
    if (isMockAuthEnabled()) {
      // Mock auth mode - Supabase not required
      console.info('[Auth] Mock auth enabled - Supabase client not initialized');
      return null;
    }
    
    console.error(
      '[Auth] Missing Supabase environment variables. ' +
      'Set NEXT_PUBLIC_SUPABASE_URL and NEXT_PUBLIC_SUPABASE_ANON_KEY, ' +
      'or enable mock auth with NEXT_PUBLIC_USE_MOCK_AUTH=true'
    );
    return null;
  }

  return createBrowserClient<Database>(supabaseUrl, supabaseAnonKey);
}

/**
 * Singleton instance for convenience.
 * Use createClient() directly if you need a fresh instance.
 */
let clientInstance: SupabaseClient<Database> | null | undefined = undefined;

export function getSupabaseClient(): SupabaseClient<Database> | null {
  if (clientInstance === undefined) {
    clientInstance = createClient();
  }
  return clientInstance;
}

