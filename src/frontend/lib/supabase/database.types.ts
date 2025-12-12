/**
 * Database Types for Supabase
 * 
 * Note: Since we're using Supabase for AUTH ONLY (not as the database),
 * this file contains minimal type definitions for the auth schema.
 * All application data types are defined elsewhere and queried via the .NET backend.
 */

export type Json =
  | string
  | number
  | boolean
  | null
  | { [key: string]: Json | undefined }
  | Json[];

/**
 * Minimal database type definition for Supabase auth-only usage.
 * The actual database schema lives in AWS RDS.
 */
export interface Database {
  public: {
    Tables: Record<string, never>; // No public tables in Supabase
    Views: Record<string, never>;
    Functions: Record<string, never>;
    Enums: Record<string, never>;
  };
  auth: {
    Tables: {
      users: {
        Row: {
          id: string;
          email: string | null;
          encrypted_password: string | null;
          email_confirmed_at: string | null;
          phone: string | null;
          confirmed_at: string | null;
          created_at: string;
          updated_at: string;
          raw_app_meta_data: Json | null;
          raw_user_meta_data: Json | null;
        };
      };
    };
    Views: Record<string, never>;
    Functions: Record<string, never>;
    Enums: Record<string, never>;
  };
}








