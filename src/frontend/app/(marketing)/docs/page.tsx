'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Book, Lock } from 'lucide-react';
import Link from 'next/link';

export default function DocsPage() {
  const router = useRouter();

  return (
    <div className="flex items-center justify-center min-h-[60vh] p-6">
      <div className="max-w-lg w-full">
        <div className="bg-surface/50 border border-border/50 rounded-2xl p-8 backdrop-blur-sm text-center relative overflow-hidden">
          {/* Icon */}
          <div className="w-20 h-20 rounded-2xl mx-auto mb-6 flex items-center justify-center bg-cyan-500/10">
            <div className="relative">
              <Book className="w-10 h-10 text-cyan-400" />
              <Lock className="w-5 h-5 text-cyan-400 absolute -bottom-1 -right-1 bg-surface rounded-full p-0.5" />
            </div>
          </div>

          {/* Content */}
          <h2 className="text-2xl font-bold text-foreground mb-3">
            Documentation
          </h2>
          <p className="text-muted-foreground mb-6 leading-relaxed">
            Our comprehensive documentation is available to registered users. 
            Please sign in to access product guides, tutorials, and technical documentation.
          </p>

          {/* CTA */}
          <div className="flex flex-col sm:flex-row gap-3 justify-center">
            <Link
              href="/login?redirect=/docs"
              className="px-6 py-3 rounded-lg font-medium text-sm text-white bg-cyan-600 hover:bg-cyan-500 transition-all flex items-center justify-center gap-2"
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
          <div className="absolute bottom-0 left-0 right-0 h-1 bg-gradient-to-r from-transparent via-cyan-500/20 to-transparent" />
        </div>
      </div>
    </div>
  );
}




