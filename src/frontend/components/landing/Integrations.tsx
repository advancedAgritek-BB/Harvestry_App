'use client';

import { 
  MessageSquare, 
  Mail, 
  Smartphone,
  Wifi,
  Cloud,
  Server
} from 'lucide-react';

const integrationCategories = [
  {
    title: 'Compliance Systems',
    description: 'Stay compliant automatically',
    integrations: [
      { name: 'METRC', description: 'Multi-state compliance sync', logo: '🏛️' },
      { name: 'BioTrack', description: 'Alternative state reporting', logo: '📊' },
    ],
  },
  {
    title: 'Accounting',
    description: 'Financial data that flows',
    integrations: [
      { name: 'QuickBooks Online', description: 'Item-level & GL Summary', logo: '📒' },
    ],
  },
  {
    title: 'Communication',
    description: 'Stay connected',
    integrations: [
      { name: 'Slack', icon: MessageSquare, description: 'Two-way task mirroring' },
      { name: 'Email', icon: Mail, description: 'All alert severities' },
      { name: 'SMS', icon: Smartphone, description: 'Critical alerts only' },
    ],
  },
  {
    title: 'IoT & Hardware',
    description: 'Connect your equipment',
    integrations: [
      { name: 'MQTT/HTTP', icon: Wifi, description: 'Sensor adapters' },
      { name: 'SDI-12', icon: Server, description: 'Agricultural sensors' },
      { name: 'Edge Controllers', icon: Cloud, description: 'Major brands supported' },
    ],
  },
];

export function Integrations() {
  return (
    <section id="integrations" className="py-24 relative">
      {/* Background */}
      <div className="absolute inset-0 bg-surface/30" />
      
      <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="text-center mb-16">
          <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-foreground/5 border border-foreground/10 text-muted-foreground text-sm font-medium mb-6">
            Integration Ecosystem
          </div>
          <h2 className="text-3xl sm:text-4xl font-bold mb-4">
            Connects to{' '}
            <span className="text-accent-emerald">
              Your Existing Stack
            </span>
          </h2>
          <p className="text-lg text-muted-foreground max-w-2xl mx-auto">
            Harvestry integrates with the tools you already use—compliance systems, 
            accounting software, communication platforms, and hardware.
          </p>
        </div>

        {/* Integration Grid */}
        <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-6">
          {integrationCategories.map((category) => (
            <div 
              key={category.title}
              className="p-6 rounded-xl bg-surface border border-border"
            >
              <h3 className="font-semibold text-lg mb-1">{category.title}</h3>
              <p className="text-sm text-muted-foreground mb-6">{category.description}</p>
              
              <div className="space-y-4">
                {category.integrations.map((integration) => (
                  <div 
                    key={integration.name}
                    className="flex items-center gap-3 p-3 rounded-lg bg-background/50 border border-border/50"
                  >
                    <div className="w-10 h-10 rounded-lg bg-surface flex items-center justify-center text-xl flex-shrink-0">
                      {integration.logo ? (
                        <span>{integration.logo}</span>
                      ) : integration.icon ? (
                        <integration.icon className="h-5 w-5 text-muted-foreground" />
                      ) : null}
                    </div>
                    <div className="min-w-0">
                      <div className="font-medium text-sm truncate">
                        {integration.name}
                      </div>
                      <div className="text-xs text-muted-foreground truncate">
                        {integration.description}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          ))}
        </div>

        {/* API Note */}
        <div className="mt-12 text-center">
          <p className="text-sm text-muted-foreground">
            Need a custom integration?{' '}
            <a href="#demo" className="text-accent-emerald hover:text-accent-emerald/80 hover:underline transition-colors">
              Talk to our team
            </a>{' '}
            about our API access.
          </p>
        </div>
      </div>
    </section>
  );
}


