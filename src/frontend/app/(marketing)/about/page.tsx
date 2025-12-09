'use client';

import { Building2 } from 'lucide-react';
import { ComingSoonPlaceholder } from '@/components/common';

export default function AboutPage() {
  return (
    <ComingSoonPlaceholder
      title="About Harvestry"
      description="We're preparing our story. Learn about our mission to transform the cultivation industry with modern technology and data-driven insights."
      icon={Building2}
      releaseTimeframe="Q1 2025"
      variant="emerald"
      highlights={[
        'Our mission and vision',
        'Leadership team',
        'Company milestones',
        'Industry partnerships',
      ]}
    />
  );
}
