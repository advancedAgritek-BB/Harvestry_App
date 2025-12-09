'use client';

// Skip static generation - requires QueryClient at runtime
export const dynamic = 'force-dynamic';

import { ReportBuilder } from '@/features/analytics/components/ReportBuilder/ReportBuilder';

export default function ReportBuilderPage() {
  return <ReportBuilder />;
}

