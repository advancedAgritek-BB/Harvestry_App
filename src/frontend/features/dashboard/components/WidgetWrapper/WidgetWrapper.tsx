'use client';

import React, { Suspense, Component, ErrorInfo, ReactNode } from 'react';
import { cn } from '@/lib/utils';
import { Loader2, AlertCircle, RefreshCw, MoreHorizontal } from 'lucide-react';
import { WidgetConfig } from '../../types/widget.types';

// ... (ErrorBoundary remains the same) ...
interface ErrorBoundaryProps {
  children: ReactNode;
  fallback?: ReactNode;
  onReset?: () => void;
}

interface ErrorBoundaryState {
  hasError: boolean;
  error?: Error;
}

class WidgetErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('Widget Error:', error, errorInfo);
  }

  reset = () => {
    this.setState({ hasError: false, error: undefined });
    this.props.onReset?.();
  };

  render() {
    if (this.state.hasError) {
      if (this.props.fallback) {
        return this.props.fallback;
      }
      return (
        <div className="flex flex-col items-center justify-center h-full p-4 text-center bg-rose-500/5 rounded-lg border border-rose-500/10">
          <AlertCircle className="w-6 h-6 text-rose-500 mb-2" />
          <h3 className="text-xs font-medium text-rose-500">Widget Error</h3>
          <button
            onClick={this.reset}
            className="mt-2 flex items-center gap-1 px-2 py-1 text-[10px] font-medium text-rose-400 hover:bg-rose-500/10 rounded transition-colors"
          >
            <RefreshCw className="w-3 h-3" />
            Retry
          </button>
        </div>
      );
    }

    return this.props.children;
  }
}

interface WidgetWrapperProps {
  title: string;
  children: ReactNode;
  className?: string;
  headerActions?: ReactNode;
  isGlass?: boolean;
}

export function WidgetWrapper({
  title,
  children,
  className,
  headerActions,
  isGlass = false,
}: WidgetWrapperProps) {
  return (
    <div
      className={cn(
        'flex flex-col h-full overflow-hidden rounded-xl transition-all duration-200 card-premium',
        isGlass && 'glass-panel',
        className
      )}
    >
      {/* Premium Header: Subtle separator, lighter text for hierarchy */}
      <div className="flex items-center justify-between px-4 py-3 border-b border-border/40">
        <h3 className="font-semibold text-xs uppercase tracking-wider text-muted-foreground">
          {title}
        </h3>
        <div className="flex items-center gap-2">
           {headerActions}
           {/* Default 'More' menu placeholder for consistent look */}
           {!headerActions && (
             <button className="text-border hover:text-foreground transition-colors">
               <MoreHorizontal className="w-4 h-4" />
             </button>
           )}
        </div>
      </div>
      
      <div className="flex-1 p-4 overflow-auto relative">
        <WidgetErrorBoundary>
          <Suspense
            fallback={
              <div className="flex items-center justify-center h-full w-full text-muted-foreground/30">
                <Loader2 className="w-5 h-5 animate-spin" />
              </div>
            }
          >
            {children}
          </Suspense>
        </WidgetErrorBoundary>
      </div>
    </div>
  );
}
