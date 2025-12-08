'use client';

/**
 * Login Page
 * 
 * Premium authentication page with animated background effects,
 * glass morphism styling, and smooth entrance animations.
 */

import { useState, useEffect } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { Leaf, Eye, EyeOff, ArrowRight, Mail, Lock, AlertCircle } from 'lucide-react';
import { useAuth } from '@/providers/AuthProvider';

export default function LoginPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { signIn, isLoading: authLoading } = useAuth();
  
  const [mounted, setMounted] = useState(false);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [rememberMe, setRememberMe] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  const expired = searchParams.get('expired') === 'true';
  const redirectTo = searchParams.get('redirect') || '/dashboard';

  useEffect(() => {
    setMounted(true);
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsLoading(true);

    try {
      const { error } = await signIn(email, password);
      
      if (error) {
        setError(getErrorMessage(error.message));
        return;
      }
      
      router.push(redirectTo);
    } catch {
      setError('An unexpected error occurred. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  const loading = isLoading || authLoading;

  return (
    <div className="min-h-screen flex items-center justify-center relative overflow-hidden">
      {/* Animated Background */}
      <div className="absolute inset-0 bg-background">
        {/* Gradient Orbs */}
        <div 
          className="absolute top-1/4 -left-20 w-[500px] h-[500px] bg-accent-emerald/20 rounded-full blur-[120px] animate-pulse-slow"
        />
        <div 
          className="absolute bottom-1/4 -right-20 w-[600px] h-[600px] bg-accent-cyan/15 rounded-full blur-[140px] animate-pulse-slow"
          style={{ animationDelay: '1s' }}
        />
        <div 
          className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[800px] h-[800px] bg-accent-violet/5 rounded-full blur-[150px]"
        />
        
        {/* Grid Pattern */}
        <div 
          className="absolute inset-0 opacity-[0.02]"
          style={{
            backgroundImage: `linear-gradient(to right, rgba(255,255,255,0.1) 1px, transparent 1px),
                              linear-gradient(to bottom, rgba(255,255,255,0.1) 1px, transparent 1px)`,
            backgroundSize: '60px 60px',
          }}
        />

        {/* Floating Particles */}
        <div className="absolute inset-0 overflow-hidden pointer-events-none">
          {mounted && [...Array(15)].map((_, i) => (
            <div
              key={i}
              className="absolute w-1 h-1 bg-accent-emerald/40 rounded-full animate-float"
              style={{
                left: `${Math.random() * 100}%`,
                top: `${Math.random() * 100}%`,
                animationDelay: `${Math.random() * 5}s`,
                animationDuration: `${10 + Math.random() * 5}s`,
              }}
            />
          ))}
        </div>
      </div>

      {/* Login Card */}
      <div 
        className={`relative w-full max-w-md mx-4 transition-all duration-700 ${
          mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'
        }`}
      >
        {/* Glass Card */}
        <div className="glass-card rounded-2xl p-8 sm:p-10">
          {/* Logo & Header */}
          <div 
            className={`text-center mb-8 transition-all duration-700 delay-100 ${
              mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'
            }`}
          >
            <Link href="/" className="inline-flex items-center gap-3 group mb-6">
              <div className="relative">
                <div className="absolute inset-0 bg-accent-emerald/20 blur-xl rounded-full scale-150 opacity-0 group-hover:opacity-100 transition-opacity duration-500" />
                <Leaf className="relative h-10 w-10 text-accent-emerald group-hover:scale-110 transition-transform duration-300" />
              </div>
              <span className="text-2xl font-bold tracking-tight">
                <span className="text-foreground">Harvestry</span>
                <span className="text-accent-emerald">.io</span>
              </span>
            </Link>
            <h1 className="text-2xl font-bold text-foreground mb-2">
              Welcome back
            </h1>
            <p className="text-muted-foreground">
              Sign in to your account to continue
            </p>
          </div>

          {/* Session Expired Notice */}
          {expired && (
            <div 
              className={`mb-6 p-4 rounded-xl bg-accent-amber/10 border border-accent-amber/20 flex items-center gap-3 transition-all duration-700 delay-150 ${
                mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'
              }`}
            >
              <AlertCircle className="h-5 w-5 text-accent-amber flex-shrink-0" />
              <p className="text-sm text-accent-amber">
                Your session has expired. Please sign in again.
              </p>
            </div>
          )}

          {/* Error Message */}
          {error && (
            <div className="mb-6 p-4 rounded-xl bg-accent-rose/10 border border-accent-rose/20 flex items-center gap-3 animate-fade-in">
              <AlertCircle className="h-5 w-5 text-accent-rose flex-shrink-0" />
              <p className="text-sm text-accent-rose">{error}</p>
            </div>
          )}

          {/* Login Form */}
          <form onSubmit={handleSubmit} className="space-y-5">
            {/* Email Field */}
            <div 
              className={`transition-all duration-700 delay-200 ${
                mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'
              }`}
            >
              <label
                htmlFor="email"
                className="block text-sm font-medium text-muted-foreground mb-2"
              >
                Email address
              </label>
              <div className="relative">
                <Mail className="absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 text-muted-foreground/50" />
                <input
                  id="email"
                  name="email"
                  type="email"
                  autoComplete="email"
                  required
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  disabled={loading}
                  className="w-full pl-12 pr-4 py-3.5 rounded-xl bg-surface/50 border border-border text-foreground placeholder-muted-foreground/50 focus:outline-none focus:ring-2 focus:ring-accent-emerald/50 focus:border-accent-emerald transition-all duration-300 disabled:opacity-50 disabled:cursor-not-allowed"
                  placeholder="you@company.com"
                />
              </div>
            </div>

            {/* Password Field */}
            <div 
              className={`transition-all duration-700 delay-300 ${
                mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'
              }`}
            >
              <div className="flex items-center justify-between mb-2">
                <label
                  htmlFor="password"
                  className="block text-sm font-medium text-muted-foreground"
                >
                  Password
                </label>
                <Link
                  href="/forgot-password"
                  className="text-sm text-accent-emerald hover:text-accent-emerald/80 transition-colors font-medium"
                >
                  Forgot password?
                </Link>
              </div>
              <div className="relative">
                <Lock className="absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 text-muted-foreground/50" />
                <input
                  id="password"
                  name="password"
                  type={showPassword ? 'text' : 'password'}
                  autoComplete="current-password"
                  required
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  disabled={loading}
                  className="w-full pl-12 pr-12 py-3.5 rounded-xl bg-surface/50 border border-border text-foreground placeholder-muted-foreground/50 focus:outline-none focus:ring-2 focus:ring-accent-emerald/50 focus:border-accent-emerald transition-all duration-300 disabled:opacity-50 disabled:cursor-not-allowed"
                  placeholder="••••••••"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-4 top-1/2 -translate-y-1/2 text-muted-foreground/50 hover:text-muted-foreground transition-colors"
                  tabIndex={-1}
                >
                  {showPassword ? (
                    <EyeOff className="h-5 w-5" />
                  ) : (
                    <Eye className="h-5 w-5" />
                  )}
                </button>
              </div>
            </div>

            {/* Remember Me */}
            <div 
              className={`flex items-center gap-3 transition-all duration-700 delay-400 ${
                mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'
              }`}
            >
              <div className="relative">
                <input
                  type="checkbox"
                  id="remember-me"
                  checked={rememberMe}
                  onChange={(e) => setRememberMe(e.target.checked)}
                  className="peer sr-only"
                />
                <label
                  htmlFor="remember-me"
                  className={`flex items-center justify-center w-5 h-5 rounded-md border-2 cursor-pointer transition-all duration-200 ${
                    rememberMe 
                      ? 'bg-accent-emerald border-accent-emerald' 
                      : 'border-border hover:border-muted-foreground'
                  }`}
                >
                  {rememberMe && (
                    <svg className="w-3 h-3 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={3}>
                      <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
                    </svg>
                  )}
                </label>
              </div>
              <label 
                htmlFor="remember-me"
                className="text-sm text-muted-foreground cursor-pointer select-none"
              >
                Remember me for 30 days
              </label>
            </div>

            {/* Submit Button */}
            <div 
              className={`pt-2 transition-all duration-700 delay-500 ${
                mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'
              }`}
            >
              <button
                type="submit"
                disabled={loading}
                className="group relative w-full py-4 px-6 rounded-xl bg-accent-emerald text-white font-semibold overflow-hidden transition-all duration-300 hover:shadow-lg hover:shadow-accent-emerald/20 disabled:opacity-50 disabled:cursor-not-allowed focus:outline-none focus:ring-2 focus:ring-accent-emerald/50 focus:ring-offset-2 focus:ring-offset-background"
              >
                {/* Animated gradient on hover */}
                <span className="absolute inset-0 bg-gradient-to-r from-accent-emerald via-emerald-400 to-accent-emerald bg-[length:200%_auto] opacity-0 group-hover:opacity-100 animate-gradient transition-opacity duration-500" />
                
                {/* Button content */}
                <span className="relative flex items-center justify-center gap-2">
                  {loading ? (
                    <>
                      <svg
                        className="animate-spin h-5 w-5"
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
                      Signing in...
                    </>
                  ) : (
                    <>
                      Sign in
                      <ArrowRight className="h-5 w-5 group-hover:translate-x-1 transition-transform duration-300" />
                    </>
                  )}
                </span>
              </button>
            </div>
          </form>

          {/* Divider */}
          <div 
            className={`relative my-8 transition-all duration-700 delay-500 ${
              mounted ? 'opacity-100' : 'opacity-0'
            }`}
          >
            <div className="absolute inset-0 flex items-center">
              <div className="w-full border-t border-border" />
            </div>
            <div className="relative flex justify-center text-sm">
              <span className="px-4 bg-surface/80 text-muted-foreground">
                New to Harvestry?
              </span>
            </div>
          </div>

          {/* Sign Up Link */}
          <div 
            className={`transition-all duration-700 delay-700 ${
              mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'
            }`}
          >
            <Link
              href="/signup"
              className="group flex items-center justify-center gap-2 w-full py-4 px-6 rounded-xl border border-border bg-surface/30 text-foreground font-semibold hover:border-accent-emerald/50 hover:bg-surface/50 transition-all duration-300"
            >
              Create an account
              <ArrowRight className="h-5 w-5 text-accent-emerald group-hover:translate-x-1 transition-transform duration-300" />
            </Link>
          </div>
        </div>

        {/* Footer */}
        <p 
          className={`mt-8 text-center text-sm text-muted-foreground transition-all duration-700 delay-700 ${
            mounted ? 'opacity-100' : 'opacity-0'
          }`}
        >
          By signing in, you agree to our{' '}
          <Link href="/terms" className="text-foreground/80 hover:text-foreground transition-colors underline-offset-4 hover:underline">
            Terms of Service
          </Link>{' '}
          and{' '}
          <Link href="/privacy" className="text-foreground/80 hover:text-foreground transition-colors underline-offset-4 hover:underline">
            Privacy Policy
          </Link>
        </p>
      </div>
    </div>
  );
}

/**
 * Convert Supabase error messages to user-friendly messages.
 */
function getErrorMessage(message: string): string {
  const errorMap: Record<string, string> = {
    'Invalid login credentials': 'Invalid email or password. Please try again.',
    'Email not confirmed': 'Please verify your email address before signing in.',
    'User not found': 'No account found with this email address.',
    'Too many requests': 'Too many login attempts. Please wait a moment and try again.',
  };
  
  return errorMap[message] || message;
}
