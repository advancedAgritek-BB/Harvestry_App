// Pricing structure aligned with Harvestry_Pricing.md
// "Land and Expand" strategy: Free to Start → Scales with Success

export interface CompetitorEquivalent {
  total: number;
  breakdown: string;
  savingsPercent: number;
}

export interface PricingTier {
  id: string;
  name: string;
  tagline: string;
  description: string;
  monthlyPrice: number | null; // null = custom/contact sales
  annualPrice: number | null;
  priceNote: string;
  targetAudience: string;
  features: PricingFeature[];
  cta: string;
  ctaVariant: 'primary' | 'secondary' | 'outline';
  popular?: boolean;
  badge?: string;
  gradient: string;
  capacity?: string;
  hardwareLevel: string;
  complianceIncluded: boolean;
  financialsIncluded: boolean;
  competitorEquivalent?: CompetitorEquivalent;
}

export interface PricingFeature {
  text: string;
  category: 'core' | 'operations' | 'compliance' | 'hardware' | 'ai';
  highlight?: boolean;
}

export interface AddOn {
  id: string;
  name: string;
  description: string;
  monthlyPrice: number;
  annualPrice: number;
  includes: string[];
  includedInTiers?: string[]; // Tiers where this is bundled
}

export interface CapacityTier {
  range: string;
  addon: string;
}

export interface CompetitorComparison {
  category: string;
  competitor: string;
  competitorPrice: string;
  harvestryIncluded: boolean;
  harvestryTier: string;
}

// =============================================================================
// PRICING TIERS - Aligned with Harvestry_Pricing.md
// Monitor (Free) → Foundation → Growth → Enterprise
// =============================================================================

export const pricingTiers: PricingTier[] = [
  {
    id: 'monitor',
    name: 'Monitor',
    tagline: 'See your room. Anywhere.',
    description: 'The ultimate gateway. Purchase a Harvestry Controller, connect your sensors, and get a professional-grade dashboard for free.',
    monthlyPrice: 0,
    annualPrice: 0,
    priceNote: 'Free forever',
    targetAudience: 'Home Grow / Test Room',
    capacity: '1 Room / 1 Controller',
    hardwareLevel: 'Live Dashboard Only',
    complianceIncluded: false,
    financialsIncluded: false,
    features: [
      { text: 'Live environmental dashboard', category: 'core', highlight: true },
      { text: 'Connect any sensor (4-20mA, 0-10V, SDI-12)', category: 'hardware' },
      { text: 'Real-time readings & alerts', category: 'core' },
      { text: 'Mobile-friendly interface', category: 'core' },
      { text: 'Community support', category: 'core' },
    ],
    cta: 'Get Started Free',
    ctaVariant: 'outline',
    badge: 'Free Forever',
    gradient: 'from-cyan-500/10 to-cyan-600/5',
  },
  {
    id: 'foundation',
    name: 'Foundation',
    tagline: 'Professionalize your grow',
    description: 'For the craft cultivator who needs more than just a dashboard. Unlimited rooms, historical data, and professional tools.',
    monthlyPrice: 349,
    annualPrice: 314,
    priceNote: 'Up to 5,000 sq ft included',
    targetAudience: 'Craft / Micro-Business',
    capacity: 'Up to 5,000 sq ft',
    hardwareLevel: 'Read-Only (Historical Data)',
    complianceIncluded: false,
    financialsIncluded: false,
    competitorEquivalent: {
      total: 850,
      breakdown: 'Trym + Inventory + SOP tools',
      savingsPercent: 59,
    },
    features: [
      { text: 'Everything in Monitor, plus:', category: 'core' },
      { text: 'Unlimited rooms (within capacity)', category: 'core', highlight: true },
      { text: 'Historical data & trends', category: 'core', highlight: true },
      { text: 'Batch lifecycle tracking', category: 'core' },
      { text: 'SOP engine & training modules', category: 'operations', highlight: true },
      { text: 'Inventory & lot tracking', category: 'core' },
      { text: 'Task management with dependencies', category: 'operations' },
      { text: 'Standard support', category: 'core' },
    ],
    cta: 'Start Free Trial',
    ctaVariant: 'outline',
    badge: 'Professional',
    gradient: 'from-sky-500/10 to-sky-600/5',
  },
  {
    id: 'growth',
    name: 'Growth',
    tagline: 'The Commercial Standard',
    description: 'The complete operating system for licensed facilities. Replaces disjointed stacks (Trym + QBO + Controller Software).',
    monthlyPrice: 899,
    annualPrice: 809,
    priceNote: 'Up to 5,000 sq ft included',
    targetAudience: 'Licensed Commercial',
    capacity: 'Up to 5,000 sq ft',
    hardwareLevel: 'Control (Automation)',
    complianceIncluded: true,
    financialsIncluded: true,
    competitorEquivalent: {
      total: 2500,
      breakdown: 'Full commercial stack',
      savingsPercent: 64,
    },
    features: [
      { text: 'Everything in Foundation, plus:', category: 'core' },
      { text: 'Hardware control & automation', category: 'hardware', highlight: true },
      { text: 'METRC/BioTrack compliance sync', category: 'compliance', highlight: true },
      { text: 'QuickBooks Online integration', category: 'operations', highlight: true },
      { text: 'Production planning & scheduling', category: 'operations' },
      { text: 'GS1/UDI label printing', category: 'operations' },
      { text: 'Recipe & nutrient management', category: 'hardware' },
      { text: 'Priority support', category: 'core' },
    ],
    cta: 'Start Free Trial',
    ctaVariant: 'primary',
    popular: true,
    badge: 'Best Value',
    gradient: 'from-accent-emerald/10 to-accent-cyan/5',
  },
  {
    id: 'enterprise',
    name: 'Enterprise',
    tagline: 'AI-Driven Scale & MSO Management',
    description: 'For MSOs and large-scale facilities (100k+ sq ft) requiring custom logic, deep data warehousing, and AI optimization.',
    monthlyPrice: null, // Custom pricing
    annualPrice: null,
    priceNote: 'Custom pricing calculator',
    targetAudience: 'MSOs / Large Scale (100k+ sq ft)',
    capacity: 'MSO / 100k+ sq ft',
    hardwareLevel: 'AI Autosteer',
    complianceIncluded: true,
    financialsIncluded: true,
    features: [
      { text: 'Everything in Growth, plus:', category: 'core' },
      { text: 'AI Autosteer (MPC control)', category: 'ai', highlight: true },
      { text: 'AI-powered yield predictions', category: 'ai', highlight: true },
      { text: 'Copilot Ask-to-Act', category: 'ai' },
      { text: 'Multi-site rollups & dashboards', category: 'operations', highlight: true },
      { text: 'Custom BI & data warehousing', category: 'operations' },
      { text: 'SSO & dedicated infrastructure', category: 'operations' },
      { text: 'Dedicated success manager', category: 'core' },
      { text: '24/7 support with SLA guarantees', category: 'core' },
    ],
    cta: 'Contact Enterprise',
    ctaVariant: 'secondary',
    badge: 'Full Platform',
    gradient: 'from-accent-violet/10 to-accent-cyan/5',
  },
];

// =============================================================================
// CAPACITY SCALING - For Foundation & Growth tiers
// =============================================================================

export const capacityTiers: CapacityTier[] = [
  { range: '0 – 5,000 sq ft', addon: 'Included in Base Price' },
  { range: '5,001 – 10,000 sq ft', addon: '+ $199/mo' },
  { range: '10,001 – 20,000 sq ft', addon: '+ $399/mo' },
  { range: '20,001 – 50,000 sq ft', addon: '+ $899/mo' },
  { range: '50,001 – 100,000 sq ft', addon: '+ $1,499/mo' },
  { range: '100,000+ sq ft', addon: 'Enterprise Calculator' },
];

// =============================================================================
// ADD-ONS - For Foundation tier (included in Growth+)
// =============================================================================

export const addOns: AddOn[] = [
  {
    id: 'compliance',
    name: 'Regulatory Compliance',
    description: 'METRC & BioTrack integration for licensed markets',
    monthlyPrice: 200,
    annualPrice: 180,
    includes: [
      'Real-time METRC/BioTrack sync',
      'COA gating & hold management',
      'Automatic retry & reconciliation',
      'Regulator-ready audit exports',
    ],
    includedInTiers: ['growth', 'enterprise'],
  },
  {
    id: 'financials',
    name: 'Financial Integration',
    description: 'QuickBooks Online sync for complete cost visibility',
    monthlyPrice: 100,
    annualPrice: 90,
    includes: [
      'Item-level transaction sync',
      'GL summary journal entries',
      'WIP → COGS tracking',
      'Reconciliation reports',
    ],
    includedInTiers: ['growth', 'enterprise'],
  },
  {
    id: 'hardware-connect',
    name: 'Third-Party Hardware Connect',
    description: 'Integrate existing controllers if not using Harvestry Edge',
    monthlyPrice: 150,
    annualPrice: 135,
    includes: [
      'Growlink / Aroya data import',
      'TrolMaster / Agrowtek support',
      'MQTT & HTTP telemetry ingest',
      'Basic environmental dashboards',
    ],
  },
];

// =============================================================================
// IMPLEMENTATION & ONBOARDING
// =============================================================================

export interface OnboardingOption {
  id: string;
  name: string;
  price: number;
  description: string;
  includes: string[];
  note?: string;
}

export const onboardingOptions: OnboardingOption[] = [
  {
    id: 'self-guided',
    name: 'Self-Guided',
    price: 499,
    description: 'Get started on your own with resources and support',
    includes: [
      'Academy access',
      'Import templates',
      '1 Kickoff Call',
    ],
    note: 'Waived for Annual Contracts',
  },
  {
    id: 'managed',
    name: 'Managed Onboarding',
    price: 2500,
    description: 'White-glove setup with hands-on training',
    includes: [
      'Hardware commissioning validation',
      'Compliance mapping & METRC sync setup',
      '3 live training sessions for your team',
    ],
  },
];

// =============================================================================
// COMPETITOR COMPARISON - For TCO section
// =============================================================================

export const competitorComparisons: CompetitorComparison[] = [
  {
    category: 'Batch Lifecycle & Task Management',
    competitor: 'Trym',
    competitorPrice: '$400/mo',
    harvestryIncluded: true,
    harvestryTier: 'Foundation',
  },
  {
    category: 'SOP & Training Management',
    competitor: 'Custom / Spreadsheets',
    competitorPrice: '$200+/mo labor',
    harvestryIncluded: true,
    harvestryTier: 'Foundation',
  },
  {
    category: 'Inventory & Lot Tracking',
    competitor: 'Canix / Leaf Logix',
    competitorPrice: '$250–$500/mo',
    harvestryIncluded: true,
    harvestryTier: 'Foundation',
  },
  {
    category: 'Environmental Monitoring',
    competitor: 'Growlink / Aroya',
    competitorPrice: '$150–$400/mo',
    harvestryIncluded: true,
    harvestryTier: 'Growth',
  },
  {
    category: 'Irrigation Control',
    competitor: 'Controller software',
    competitorPrice: '$100–$300/mo',
    harvestryIncluded: true,
    harvestryTier: 'Growth',
  },
  {
    category: 'Compliance (METRC/BioTrack)',
    competitor: 'Flourish',
    competitorPrice: '$350–$1,000/mo',
    harvestryIncluded: true,
    harvestryTier: 'Growth (Included)',
  },
  {
    category: 'Financial Integration (QBO)',
    competitor: 'Manual / Custom',
    competitorPrice: '$200+/mo labor',
    harvestryIncluded: true,
    harvestryTier: 'Growth (Included)',
  },
];

// Calculate total competitor stack cost
export const competitorStackTotal = {
  low: 1350,  // Trym + basic inventory + basic environmental
  high: 2800, // Full stack with Flourish + premium environmental
  description: 'Typical multi-vendor stack for a licensed commercial facility',
};

// =============================================================================
// FEATURE CATEGORIES - For display grouping
// =============================================================================

export const featureCategories = {
  core: {
    label: 'Core Operations',
    description: 'Cultivation management fundamentals',
    color: 'emerald',
  },
  operations: {
    label: 'Workflow & Scale',
    description: 'Automation and multi-site capabilities',
    color: 'sky',
  },
  hardware: {
    label: 'Hardware Integration',
    description: 'Sensors, controllers, and telemetry',
    color: 'cyan',
  },
  compliance: {
    label: 'Compliance',
    description: 'Regulatory integration',
    color: 'amber',
  },
  ai: {
    label: 'AI & Automation',
    description: 'Intelligent optimization',
    color: 'violet',
  },
};

// =============================================================================
// HARDWARE INDEPENDENCE VALUE PROP
// =============================================================================

export const hardwareIndependence = {
  headline: 'Hardware Freedom',
  description: 'Our controllers work with *your* sensors. Don\'t pay $30k for a proprietary cabinet.',
  supportedProtocols: ['4-20mA', '0-10V', 'SDI-12'],
  benefits: [
    'Use off-the-shelf sensors',
    'Bring what you already have',
    'No vendor lock-in',
  ],
};
