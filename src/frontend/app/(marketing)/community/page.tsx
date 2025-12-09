'use client';

import { Users } from 'lucide-react';
import { ComingSoonPlaceholder } from '@/components/common';

export default function CommunityPage() {
  return (
    <ComingSoonPlaceholder
      title="Harvestry Community"
      description="We're building an amazing community for cultivation professionals. Coming soon: forums, knowledge sharing, events, and more to connect you with fellow growers."
      icon={Users}
      releaseTimeframe="Q1 2025"
      variant="violet"
      highlights={[
        'Discussion forums',
        'Knowledge base',
        'Community events',
        'Expert networking',
      ]}
    />
  );
}
