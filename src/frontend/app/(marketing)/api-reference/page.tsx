'use client';

import Link from 'next/link';
import { Code, Lock } from 'lucide-react';

export default function ApiReferencePage() {
  return (
    <div className="flex items-center justify-center min-h-[60vh] p-6">
      <div className="max-w-lg w-full">
        <div className="bg-surface/50 border border-border/50 rounded-2xl p-8 backdrop-blur-sm text-center relative overflow-hidden">
          {/* Icon */}
          <div className="w-20 h-20 rounded-2xl mx-auto mb-6 flex items-center justify-center bg-violet-500/10">
            <div className="relative">
              <Code className="w-10 h-10 text-violet-400" />
              <Lock className="w-5 h-5 text-violet-400 absolute -bottom-1 -right-1 bg-surface rounded-full p-0.5" />
            </div>
          </div>

          {/* Content */}
          <h2 className="text-2xl font-bold text-foreground mb-3">
            API Reference
          </h2>
          <p className="text-muted-foreground mb-6 leading-relaxed">
            Access our complete API documentation including endpoints, authentication, 
            and code examples. Sign in to view the API reference.
          </p>

          {/* CTA */}
          <div className="flex flex-col sm:flex-row gap-3 justify-center">
            <Link
              href="/login?redirect=/api-reference"
              className="px-6 py-3 rounded-lg font-medium text-sm text-white bg-violet-600 hover:bg-violet-500 transition-all flex items-center justify-center gap-2"
            >
              Sign In to Access
            </Link>
            <Link
              href="/signup"
              className="px-6 py-3 rounded-lg font-medium text-sm text-foreground bg-surface border border-border hover:bg-elevated transition-all flex items-center justify-center gap-2"
            >
              Create Account
            </Link>
          </div>

          {/* Bottom decoration */}
          <div className="absolute bottom-0 left-0 right-0 h-1 bg-gradient-to-r from-transparent via-violet-500/20 to-transparent" />
        </div>
      </div>
    </div>
  );
}




