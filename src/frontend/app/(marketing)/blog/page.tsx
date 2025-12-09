'use client';

import { FileText } from 'lucide-react';
import { ComingSoonPlaceholder } from '@/components/common';

export default function BlogPage() {
  return (
    <ComingSoonPlaceholder
      title="Harvestry Blog"
      description="Our blog is coming soon! We'll be sharing industry insights, cultivation best practices, product updates, and expert tips to help you grow smarter."
      icon={FileText}
      releaseTimeframe="Q1 2025"
      variant="amber"
      highlights={[
        'Industry trends & insights',
        'Cultivation best practices',
        'Product updates & features',
        'Customer success stories',
      ]}
    />
  );
}
