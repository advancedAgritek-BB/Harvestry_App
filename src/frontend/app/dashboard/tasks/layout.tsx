'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { cn } from '@/lib/utils';

const TABS = [
  { label: 'My Tasks', href: '/dashboard/tasks' },
  { label: 'All Tasks', href: '/dashboard/tasks/all' },
  { label: 'Blueprints', href: '/dashboard/tasks/blueprints' },
  { label: 'SOPs', href: '/dashboard/tasks/sops' },
  { label: 'Templates', href: '/dashboard/tasks/templates' },
];

export default function TasksLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const pathname = usePathname();

  return (
    <div className="h-full flex flex-col">
      {/* Page Header */}
      <div className="px-6 pt-6 pb-4 border-b border-[var(--border)] bg-[var(--bg-surface)]">
        <div className="flex items-center justify-between mb-4">
          <div>
            <h1 className="text-2xl font-bold text-[var(--text-primary)]">Task Management</h1>
            <p className="text-sm text-[var(--text-muted)] mt-1">
              Manage tasks, blueprints, and SOPs for your cultivation operations
            </p>
          </div>
        </div>

        {/* Tab Navigation */}
        <nav className="flex gap-1">
          {TABS.map((tab) => {
            const isActive = 
              tab.href === '/dashboard/tasks' 
                ? pathname === '/dashboard/tasks'
                : pathname.startsWith(tab.href);
            
            return (
              <Link
                key={tab.href}
                href={tab.href}
                className={cn(
                  'px-4 py-2 text-sm font-medium rounded-lg transition-all',
                  isActive
                    ? 'bg-[var(--accent-cyan)]/15 text-[var(--accent-cyan)]'
                    : 'text-[var(--text-muted)] hover:text-[var(--text-primary)] hover:bg-[var(--bg-tile)]'
                )}
              >
                {tab.label}
              </Link>
            );
          })}
        </nav>
      </div>

      {/* Page Content */}
      <div className="flex-1 overflow-y-auto p-6">
        {children}
      </div>
    </div>
  );
}
