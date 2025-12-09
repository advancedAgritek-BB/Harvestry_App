'use client';

import { type SVGProps } from 'react';

interface SlackLogoProps extends SVGProps<SVGSVGElement> {
  className?: string;
}

/**
 * Slack Logo
 * Official brand colors: 
 * - Blue (#36C5F0)
 * - Green (#2EB67D)
 * - Yellow (#ECB22E)
 * - Red (#E01E5A)
 */
export function SlackLogo({ className, ...props }: SlackLogoProps) {
  return (
    <svg
      viewBox="0 0 24 24"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      className={className}
      {...props}
    >
      {/* Top left - Blue */}
      <path
        d="M6 10a2 2 0 1 1 0-4h2v2a2 2 0 0 1-2 2z"
        fill="#36C5F0"
      />
      <rect x="8" y="4" width="2" height="6" rx="1" fill="#36C5F0" />
      
      {/* Top right - Green */}
      <path
        d="M14 6a2 2 0 1 1 4 0v2h-2a2 2 0 0 1-2-2z"
        fill="#2EB67D"
      />
      <rect x="14" y="8" width="6" height="2" rx="1" fill="#2EB67D" />
      
      {/* Bottom right - Yellow */}
      <path
        d="M18 14a2 2 0 1 1 0 4h-2v-2a2 2 0 0 1 2-2z"
        fill="#ECB22E"
      />
      <rect x="12" y="14" width="2" height="6" rx="1" fill="#ECB22E" />
      
      {/* Bottom left - Red */}
      <path
        d="M10 18a2 2 0 1 1-4 0v-2h2a2 2 0 0 1 2 2z"
        fill="#E01E5A"
      />
      <rect x="4" y="14" width="6" height="2" rx="1" fill="#E01E5A" />
    </svg>
  );
}
