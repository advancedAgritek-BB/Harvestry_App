'use client';

import React, { useState } from 'react';
import {
  Thermometer,
  Droplets,
  AlertTriangle,
  Waves,
  FlaskConical,
  Beaker,
  Syringe,
  Container,
  Shield,
  GitBranch,
  Leaf,
} from 'lucide-react';
import { AdminTabs, TabPanel } from '@/components/admin';
import { EnvironmentTargetsSection } from './sections/EnvironmentTargetsSection';
import { EnvironmentOverridesSection } from './sections/EnvironmentOverridesSection';
import { AlertThresholdsSection } from './sections/AlertThresholdsSection';
import { IrrigationGroupsSection } from './sections/IrrigationGroupsSection';
import { IrrigationProgramsSection } from './sections/IrrigationProgramsSection';
import { IrrigationSchedulesSection } from './sections/IrrigationSchedulesSection';
import { FeedRecipesSection } from './sections/FeedRecipesSection';
import { FeedTargetsSection } from './sections/FeedTargetsSection';
import { MixTanksSection } from './sections/MixTanksSection';
import { InjectorChannelsSection } from './sections/InjectorChannelsSection';
import { NutrientProductsSection } from './sections/NutrientProductsSection';
import { StockSolutionLotsSection } from './sections/StockSolutionLotsSection';
import { InterlocksSection } from './sections/InterlocksSection';
import { ControlLoopsSection } from './sections/ControlLoopsSection';
import { PropagationSettingsSection } from './sections/PropagationSettingsSection';

const CULTIVATION_TABS = [
  { id: 'environment', label: 'Environment', icon: Thermometer },
  { id: 'alerts', label: 'Alerts', icon: AlertTriangle },
  { id: 'irrigation', label: 'Irrigation', icon: Droplets },
  { id: 'fertigation', label: 'Fertigation', icon: FlaskConical },
  { id: 'safety', label: 'Safety & Control', icon: Shield },
  { id: 'propagation', label: 'Propagation', icon: Leaf },
];

export default function CultivationAdminPage() {
  const [activeTab, setActiveTab] = useState('environment');

  return (
    <div className="space-y-6">
      <AdminTabs
        tabs={CULTIVATION_TABS}
        activeTab={activeTab}
        onChange={setActiveTab}
      />

      {/* Environment Tab */}
      <TabPanel id="environment" activeTab={activeTab}>
        <div className="space-y-8">
          <EnvironmentTargetsSection />
          <EnvironmentOverridesSection />
        </div>
      </TabPanel>

      {/* Alerts Tab */}
      <TabPanel id="alerts" activeTab={activeTab}>
        <AlertThresholdsSection />
      </TabPanel>

      {/* Irrigation Tab */}
      <TabPanel id="irrigation" activeTab={activeTab}>
        <div className="space-y-8">
          <IrrigationGroupsSection />
          <IrrigationProgramsSection />
          <IrrigationSchedulesSection />
        </div>
      </TabPanel>

      {/* Fertigation Tab */}
      <TabPanel id="fertigation" activeTab={activeTab}>
        <div className="space-y-8">
          <FeedRecipesSection />
          <FeedTargetsSection />
          <MixTanksSection />
          <InjectorChannelsSection />
          <NutrientProductsSection />
          <StockSolutionLotsSection />
        </div>
      </TabPanel>

      {/* Safety & Control Tab */}
      <TabPanel id="safety" activeTab={activeTab}>
        <div className="space-y-8">
          <InterlocksSection />
          <ControlLoopsSection />
        </div>
      </TabPanel>

      {/* Propagation Tab */}
      <TabPanel id="propagation" activeTab={activeTab}>
        <PropagationSettingsSection />
      </TabPanel>
    </div>
  );
}

