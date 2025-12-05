import { Metadata } from 'next';
import {
  Navbar,
  Hero,
  ProblemSolution,
  Features,
  Screenshots,
  PersonaBenefits,
  Integrations,
  Testimonials,
  Pricing,
  FinalCTA,
  Footer,
} from '@/components/landing';

export const metadata: Metadata = {
  title: 'Harvestry | The Modern Cultivation Operating System',
  description: 'Enterprise-grade Cultivation OS unifying ERP, compliance, and prescriptive control. Grow Smarter. Stay Compliant. Scale Confidently.',
  keywords: ['cannabis', 'cultivation', 'ERP', 'compliance', 'METRC', 'BioTrack', 'irrigation', 'automation'],
  openGraph: {
    title: 'Harvestry | The Modern Cultivation Operating System',
    description: 'Enterprise-grade Cultivation OS unifying ERP, compliance, and prescriptive control.',
    url: 'https://harvestry.io',
    siteName: 'Harvestry',
    type: 'website',
  },
  twitter: {
    card: 'summary_large_image',
    title: 'Harvestry | The Modern Cultivation Operating System',
    description: 'Enterprise-grade Cultivation OS unifying ERP, compliance, and prescriptive control.',
  },
};

export default function LandingPage() {
  return (
    <main className="min-h-screen">
      <Navbar />
      <Hero />
      <ProblemSolution />
      <Features />
      <Screenshots />
      <PersonaBenefits />
      <Integrations />
      <Testimonials />
      <Pricing />
      <FinalCTA />
      <Footer />
    </main>
  );
}
