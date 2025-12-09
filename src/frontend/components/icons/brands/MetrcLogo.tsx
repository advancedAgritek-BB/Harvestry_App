'use client';

import Image from 'next/image';

interface MetrcLogoProps {
  className?: string;
}

/**
 * METRC (Marijuana Enforcement Tracking Reporting Compliance) Logo
 * Uses the official METRC logo image
 */
export function MetrcLogo({ className }: MetrcLogoProps) {
  return (
    <Image
      src="/images/metrc-logo.png"
      alt="METRC"
      width={24}
      height={24}
      className={className}
    />
  );
}
