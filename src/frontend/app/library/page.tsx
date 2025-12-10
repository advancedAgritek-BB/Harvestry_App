'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';

/**
 * Library Landing Page
 * 
 * Redirects to the Genetics tab by default.
 */
export default function LibraryPage() {
  const router = useRouter();

  useEffect(() => {
    router.replace('/library/genetics');
  }, [router]);

  // Show nothing while redirecting
  return null;
}


