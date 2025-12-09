'use client';

// Skip static generation - requires QueryClient at runtime
export const dynamic = 'force-dynamic';

import { DashboardBuilder } from '@/features/analytics/components/DashboardBuilder/DashboardBuilder';

export default function DashboardBuilderPage() {
  return <DashboardBuilder />;
}

