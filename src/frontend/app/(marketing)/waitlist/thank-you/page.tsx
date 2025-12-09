'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { CheckCircle, Sparkles, ArrowRight } from 'lucide-react';

export default function ThankYouPage() {
  const router = useRouter();
  const [countdown, setCountdown] = useState(8);
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
    
    // Countdown timer
    const interval = setInterval(() => {
      setCountdown(prev => {
        if (prev <= 1) {
          clearInterval(interval);
          router.push('/');
          return 0;
        }
        return prev - 1;
      });
    }, 1000);

    return () => clearInterval(interval);
  }, [router]);

  return (
    <div className="min-h-screen bg-background relative overflow-hidden flex items-center justify-center">
      {/* Background Effects */}
      <div className="absolute inset-0">
        <div className="absolute top-1/3 left-1/2 -translate-x-1/2 w-[800px] h-[800px] bg-accent-emerald/15 rounded-full blur-[200px]" />
        <div className="absolute top-1/2 left-1/4 w-[400px] h-[400px] bg-accent-cyan/10 rounded-full blur-[100px]" />
        <div className="absolute bottom-1/4 right-1/4 w-[300px] h-[300px] bg-accent-violet/8 rounded-full blur-[80px]" />
      </div>

      {/* Confetti-like particles */}
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        {mounted && [...Array(20)].map((_, i) => (
          <div
            key={i}
            className="absolute w-2 h-2 rounded-full animate-float"
            style={{
              left: `${Math.random() * 100}%`,
              top: `${Math.random() * 100}%`,
              backgroundColor: ['#10b981', '#06b6d4', '#8b5cf6', '#f59e0b'][i % 4],
              opacity: 0.3,
              animationDelay: `${Math.random() * 3}s`,
              animationDuration: `${3 + Math.random() * 2}s`,
            }}
          />
        ))}
      </div>

      {/* Content */}
      <div className={`relative z-10 text-center px-4 max-w-xl mx-auto transition-all duration-700 ${mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'}`}>
        {/* Success Icon */}
        <div className="relative inline-flex items-center justify-center mb-8">
          <div className="absolute inset-0 w-24 h-24 bg-accent-emerald/20 rounded-full blur-xl animate-pulse" />
          <div className="relative w-24 h-24 rounded-full bg-gradient-to-br from-accent-emerald to-accent-cyan flex items-center justify-center">
            <CheckCircle className="h-12 w-12 text-white" />
          </div>
          <Sparkles className="absolute -top-2 -right-2 h-8 w-8 text-accent-amber animate-bounce" />
        </div>

        {/* Headline */}
        <h1 className="text-4xl sm:text-5xl font-bold mb-4">
          You&apos;re In!
        </h1>

        <p className="text-xl text-muted-foreground mb-8">
          Welcome to the future of cultivation management.
        </p>

        {/* What's Next */}
        <div className="bg-surface/50 backdrop-blur-sm border border-border/50 rounded-2xl p-6 mb-8 text-left">
          <h2 className="text-lg font-semibold mb-4 flex items-center gap-2">
            <span className="w-8 h-8 rounded-full bg-accent-emerald/10 flex items-center justify-center text-accent-emerald text-sm font-bold">1</span>
            What happens next?
          </h2>
          <ul className="space-y-3 text-muted-foreground">
            <li className="flex items-start gap-3">
              <span className="w-1.5 h-1.5 rounded-full bg-accent-emerald mt-2 flex-shrink-0" />
              <span>Check your inbox for a confirmation email</span>
            </li>
            <li className="flex items-start gap-3">
              <span className="w-1.5 h-1.5 rounded-full bg-accent-emerald mt-2 flex-shrink-0" />
              <span>We&apos;ll reach out when early access opens</span>
            </li>
            <li className="flex items-start gap-3">
              <span className="w-1.5 h-1.5 rounded-full bg-accent-emerald mt-2 flex-shrink-0" />
              <span>Founding members get exclusive pricing locked in</span>
            </li>
          </ul>
        </div>

        {/* Redirect Notice */}
        <div className="mb-6">
          <p className="text-sm text-muted-foreground mb-2">
            Returning to homepage in{' '}
            <span className="text-accent-emerald font-bold text-lg">{countdown}</span>
            {' '}seconds
          </p>
          <div className="w-48 h-1 mx-auto bg-surface rounded-full overflow-hidden">
            <div 
              className="h-full bg-accent-emerald transition-all duration-1000 ease-linear"
              style={{ width: `${(countdown / 8) * 100}%` }}
            />
          </div>
        </div>

        {/* Manual Return */}
        <Link 
          href="/"
          className="inline-flex items-center gap-2 text-accent-emerald hover:text-accent-emerald/80 font-medium transition-colors"
        >
          Return to homepage now
          <ArrowRight className="h-4 w-4" />
        </Link>
      </div>

      {/* Add keyframes for float animation */}
      <style jsx>{`
        @keyframes float {
          0%, 100% {
            transform: translateY(0) rotate(0deg);
          }
          50% {
            transform: translateY(-20px) rotate(180deg);
          }
        }
        .animate-float {
          animation: float 4s ease-in-out infinite;
        }
      `}</style>
    </div>
  );
}

