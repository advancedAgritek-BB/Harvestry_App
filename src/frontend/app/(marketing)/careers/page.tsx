'use client';

import { Briefcase } from 'lucide-react';
import { ComingSoonPlaceholder } from '@/components/common';

export default function CareersPage() {
  return (
    <ComingSoonPlaceholder
      title="Join Our Team"
      description="We're building something special at Harvestry. Our careers page is coming soon with exciting opportunities to help shape the future of cultivation technology."
      icon={Briefcase}
      releaseTimeframe="Q1 2025"
      variant="violet"
      highlights={[
        'Remote-first culture',
        'Competitive compensation',
        'Growth opportunities',
        'Mission-driven work',
      ]}
    />
  );
}
