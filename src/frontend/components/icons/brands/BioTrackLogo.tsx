'use client';

import Image from 'next/image';

interface BioTrackLogoProps {
  className?: string;
}

/**
 * BioTrack THC Compliance System Logo
 * Uses the official BioTrack logo image
 */
export function BioTrackLogo({ className }: BioTrackLogoProps) {
  return (
    <Image
      src="/images/biotrack-logo.png"
      alt="BioTrack"
      width={24}
      height={24}
      className={className}
    />
  );
}
