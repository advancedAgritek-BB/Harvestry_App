'use client';

import { ComingSoonPlaceholder } from '@/components/common/ComingSoonPlaceholder';
import { Brain } from 'lucide-react';

export function AIInsightsPage() {
  return (
    <ComingSoonPlaceholder
      title="AI-Powered Insights"
      description="Get intelligent recommendations for optimizing your cultivation. Our AI analyzes environmental data, yield history, and industry best practices to help you grow better."
      icon={Brain}
      releaseTimeframe="Q1 2025"
      variant="violet"
      highlights={[
        "Anomaly detection and early warnings",
        "Yield prediction based on current conditions",
        "ET₀-aware irrigation recommendations",
        "Auto-apply suggestions with feature flags",
      ]}
    />
  );
}

export default AIInsightsPage;
