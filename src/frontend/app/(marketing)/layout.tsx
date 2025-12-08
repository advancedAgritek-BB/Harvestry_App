'use client';

import { Navbar, Footer } from '@/components/landing';

interface MarketingLayoutProps {
  children: React.ReactNode;
}

export default function MarketingLayout({ children }: MarketingLayoutProps) {
  return (
    <div className="min-h-screen flex flex-col">
      <Navbar />
      <main className="flex-1 pt-20 lg:pt-24">
        {children}
      </main>
      <Footer />
    </div>
  );
}
