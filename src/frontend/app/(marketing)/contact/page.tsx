import { Metadata } from 'next';
import { Mail } from 'lucide-react';
import { ComingSoonPlaceholder } from '@/components/common';

export const metadata: Metadata = {
  title: 'Contact Us | Harvestry',
  description: 'Get in touch with the Harvestry team.',
};

export default function ContactPage() {
  return (
    <ComingSoonPlaceholder
      title="Contact Us"
      description="Our contact page is coming soon. In the meantime, you can reach us at support@harvestry.io or book a demo through our homepage."
      icon={Mail}
      releaseTimeframe="Q1 2025"
      variant="emerald"
      highlights={[
        'General inquiries',
        'Sales & partnerships',
        'Technical support',
        'Office locations',
      ]}
    />
  );
}
