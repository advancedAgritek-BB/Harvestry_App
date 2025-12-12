'use client';

import { Sidebar } from '@/components/navigation';
import { AuthGuard } from '@/components/auth';

export default function TransfersLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <AuthGuard>
      <div className="flex h-screen bg-background overflow-hidden font-sans">
        <Sidebar />
        <div className="flex-1 flex flex-col h-full overflow-hidden relative">
          <main className="flex-1 overflow-y-auto overflow-x-hidden relative">
            <div className="absolute inset-0 pointer-events-none bg-[radial-gradient(ellipse_at_top,_var(--tw-gradient-stops))] from-primary/10 via-transparent to-transparent opacity-50" />
            <div className="relative z-10">{children}</div>
          </main>
        </div>
      </div>
    </AuthGuard>
  );
}

