'use client';

import Image from 'next/image';

interface QuickBooksLogoProps {
  className?: string;
}

/**
 * QuickBooks Logo
 * Uses the official QuickBooks logo image
 */
export function QuickBooksLogo({ className }: QuickBooksLogoProps) {
  return (
    <Image
      src="/images/quickbooks_logo.png"
      alt="QuickBooks"
      width={24}
      height={24}
      className={className}
    />
  );
}
