'use client';

/**
 * ThemeProvider Component
 * 
 * Manages the data-theme attribute on the HTML element based on user preference.
 * Handles hydration to prevent flash of wrong theme on initial load.
 */

import React, { useEffect, useState } from 'react';
import { useThemeStore, ThemeMode } from '@/stores/app/themeStore';

interface ThemeProviderProps {
  children: React.ReactNode;
  defaultTheme?: ThemeMode;
}

export function ThemeProvider({ children, defaultTheme = 'dark' }: ThemeProviderProps) {
  const theme = useThemeStore((state) => state.theme);
  const [mounted, setMounted] = useState(false);

  // Prevent hydration mismatch by only rendering after mount
  useEffect(() => {
    setMounted(true);
  }, []);

  // Apply theme to document
  useEffect(() => {
    if (!mounted) return;
    
    const root = document.documentElement;
    root.setAttribute('data-theme', theme);
    
    // Update color-scheme meta for browser UI
    root.style.colorScheme = theme;
  }, [theme, mounted]);

  // Script to run before React hydration to prevent flash
  // This is injected into the HTML to set initial theme
  const themeInitScript = `
    (function() {
      try {
        var stored = localStorage.getItem('harvestry-theme');
        if (stored) {
          var parsed = JSON.parse(stored);
          var theme = parsed.state?.theme || '${defaultTheme}';
          document.documentElement.setAttribute('data-theme', theme);
          document.documentElement.style.colorScheme = theme;
        } else {
          document.documentElement.setAttribute('data-theme', '${defaultTheme}');
          document.documentElement.style.colorScheme = '${defaultTheme}';
        }
      } catch (e) {
        document.documentElement.setAttribute('data-theme', '${defaultTheme}');
        document.documentElement.style.colorScheme = '${defaultTheme}';
      }
    })();
  `;

  return (
    <>
      <script
        dangerouslySetInnerHTML={{ __html: themeInitScript }}
        suppressHydrationWarning
      />
      {children}
    </>
  );
}

export default ThemeProvider;

