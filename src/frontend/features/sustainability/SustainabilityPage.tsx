'use client';

import { ComingSoonPlaceholder } from '@/components/common/ComingSoonPlaceholder';
import { Leaf } from 'lucide-react';

export function SustainabilityPage() {
  return (
    <ComingSoonPlaceholder
      title="Sustainability & ESG"
      description="Track and report on your environmental impact. Monitor water usage efficiency, energy consumption, and carbon footprint to meet regulatory requirements and stakeholder expectations."
      icon={Leaf}
      releaseTimeframe="Q2 2025"
      variant="emerald"
      highlights={[
        "Water Use Efficiency (WUE) tracking",
        "Nutrient Use Efficiency (NUE) metrics",
        "kWh per gram calculations",
        "Automated ESG compliance reports",
      ]}
    />
  );
}

export default SustainabilityPage;




