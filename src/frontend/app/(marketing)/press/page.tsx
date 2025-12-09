'use client';

import { Newspaper } from 'lucide-react';
import { ComingSoonPlaceholder } from '@/components/common';

export default function PressPage() {
  return (
    <ComingSoonPlaceholder
      title="Press & Media"
      description="Our press room is being prepared. Soon you'll find press releases, media assets, and news coverage about Harvestry's journey."
      icon={Newspaper}
      releaseTimeframe="Q1 2025"
      variant="cyan"
      highlights={[
        'Press releases',
        'Media kit & brand assets',
        'News coverage',
        'Press contact information',
      ]}
    />
  );
}
