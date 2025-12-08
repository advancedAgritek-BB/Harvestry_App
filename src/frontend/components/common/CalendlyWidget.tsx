'use client';

import { useEffect, useState, useCallback } from 'react';
import { Calendar } from 'lucide-react';

declare global {
  interface Window {
    Calendly?: {
      initPopupWidget: (options: { url: string }) => void;
      closePopupWidget: () => void;
    };
  }
}

const CALENDLY_URL = 'https://calendly.com/bburnette-advancedagritek/harvestry-io-demo';

export function CalendlyWidget() {
  const [isLoaded, setIsLoaded] = useState(false);
  const [isVisible, setIsVisible] = useState(false);

  useEffect(() => {
    // Load Calendly CSS
    const link = document.createElement('link');
    link.href = 'https://assets.calendly.com/assets/external/widget.css';
    link.rel = 'stylesheet';
    document.head.appendChild(link);

    // Load Calendly JS
    const script = document.createElement('script');
    script.src = 'https://assets.calendly.com/assets/external/widget.js';
    script.async = true;
    script.onload = () => setIsLoaded(true);
    document.body.appendChild(script);

    // Show button after a short delay for smooth appearance
    const showTimeout = setTimeout(() => setIsVisible(true), 1000);

    return () => {
      clearTimeout(showTimeout);
      // Cleanup is optional since these are cacheable resources
    };
  }, []);

  const openCalendly = useCallback(() => {
    if (window.Calendly) {
      window.Calendly.initPopupWidget({ url: CALENDLY_URL });
    }
  }, []);

  return (
    <>
      {/* Floating Schedule Button */}
      <button
        onClick={openCalendly}
        disabled={!isLoaded}
        aria-label="Schedule a demo"
        className={`
          fixed bottom-4 right-4 z-50
          flex items-center gap-2 px-5 py-3
          bg-accent-emerald text-white font-semibold
          rounded-full shadow-lg
          transition-all duration-500 ease-out
          hover:shadow-xl hover:shadow-accent-emerald/25
          hover:scale-105 hover:bg-emerald-500
          active:scale-95
          disabled:opacity-50 disabled:cursor-not-allowed
          ${isVisible ? 'translate-y-0 opacity-100' : 'translate-y-4 opacity-0'}
        `}
      >
        <Calendar className="h-5 w-5" />
        <span className="hidden sm:inline">Schedule Demo</span>
      </button>

      {/* Custom styles to theme the Calendly popup */}
      <style jsx global>{`
        .calendly-overlay {
          z-index: 9999 !important;
        }
        .calendly-popup {
          z-index: 10000 !important;
        }
        .calendly-popup-close {
          z-index: 10001 !important;
        }
      `}</style>
    </>
  );
}
