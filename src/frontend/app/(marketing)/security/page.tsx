import { Metadata } from 'next';
import { Shield } from 'lucide-react';
import { ComingSoonPlaceholder } from '@/components/common';

export const metadata: Metadata = {
  title: 'Security | Harvestry',
  description: 'Learn about Harvestry\'s enterprise-grade security measures and compliance certifications.',
};

export default function SecurityPage() {
  return (
    <ComingSoonPlaceholder
      title="Enterprise Security"
      description="We're building comprehensive documentation about our security practices, certifications, and compliance measures. Check back soon for detailed information about how we protect your data."
      icon={Shield}
      releaseTimeframe="Q1 2025"
      variant="cyan"
      highlights={[
        'SOC 2 Type II Compliance',
        'End-to-end encryption',
        'Role-based access control',
        'Regular security audits',
      ]}
    />
  );
}
