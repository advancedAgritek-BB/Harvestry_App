'use client';

import { useState } from 'react';
import Image from 'next/image';
import { cn } from '@/lib/utils';

type ScreenshotTab = 'cultivation' | 'irrigation' | 'planner' | 'tasks';

export function Screenshots() {
  const [activeTab, setActiveTab] = useState<ScreenshotTab>('cultivation');

  const tabs = [
    { id: 'cultivation' as const, label: 'Cultivation', image: '/images/cultivation-dashboard.png' },
    { id: 'irrigation' as const, label: 'Irrigation', image: '/images/irrigation-dashboard.png' },
    { id: 'planner' as const, label: 'Planner', image: '/images/planner-dashboard.png' },
    { id: 'tasks' as const, label: 'Tasks', image: '/images/tasks-dashboard.png' },
  ];

  const activeImage = tabs.find(t => t.id === activeTab)?.image || tabs[0].image;

  return (
    <section id="screenshots" className="relative py-12 sm:py-16 lg:py-12 xl:py-16 overflow-hidden">
      {/* Background */}
      <div className="absolute inset-0 bg-gradient-to-b from-background via-surface/30 to-background" />
      
      <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="text-center mb-6 lg:mb-8">
          <span className="inline-block px-4 py-1 rounded-full bg-foreground/5 border border-foreground/10 text-muted-foreground text-sm font-medium mb-3">
            See It In Action
          </span>
          <h2 className="text-2xl sm:text-3xl lg:text-4xl xl:text-5xl font-bold mb-2 lg:mb-3">
            <span className="text-foreground">Powerful Dashboards,</span>{' '}
            <span className="text-accent-emerald">
              Beautiful Design
            </span>
          </h2>
          <p className="text-base lg:text-lg text-muted-foreground max-w-2xl mx-auto">
            Every screen is designed for operators who need real-time insights at a glance.
          </p>
        </div>

        {/* Tab Navigation */}
        <div className="flex justify-center mb-4 lg:mb-6">
          <div className="inline-flex bg-surface/50 backdrop-blur-sm rounded-xl p-1 lg:p-1.5 border border-border/50">
            {tabs.map((tab) => (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id)}
                className={cn(
                  "px-4 lg:px-6 py-2 lg:py-2.5 text-sm font-medium rounded-lg transition-all duration-300",
                  activeTab === tab.id
                    ? "bg-accent-emerald text-white shadow-lg shadow-accent-emerald/20"
                    : "text-muted-foreground hover:text-foreground hover:bg-surface"
                )}
              >
                {tab.label}
              </button>
            ))}
          </div>
        </div>

        {/* Screenshot Display */}
        <div className="relative">
          {/* Glow Effect */}
          <div className="absolute -inset-4 lg:-inset-6 bg-gradient-to-r from-accent-emerald/20 via-accent-cyan/20 to-accent-violet/20 blur-2xl lg:blur-3xl opacity-40 rounded-3xl" />
          
          {/* Browser Window */}
          <div className="relative rounded-xl lg:rounded-2xl overflow-hidden border border-border/50 shadow-2xl shadow-black/50 bg-background">
            {/* Browser Chrome */}
            <div className="h-8 lg:h-10 bg-elevated/80 backdrop-blur flex items-center gap-2 px-3 lg:px-4 border-b border-border/50">
              <div className="flex gap-1.5 lg:gap-2">
                <div className="w-2.5 h-2.5 lg:w-3 lg:h-3 rounded-full bg-accent-rose/60" />
                <div className="w-2.5 h-2.5 lg:w-3 lg:h-3 rounded-full bg-accent-amber/60" />
                <div className="w-2.5 h-2.5 lg:w-3 lg:h-3 rounded-full bg-accent-emerald/60" />
              </div>
              <div className="flex-1 flex justify-center">
                <div className="px-3 lg:px-4 py-0.5 lg:py-1 rounded-md bg-background/50 text-xs text-muted-foreground flex items-center gap-2">
                  <div className="w-2.5 h-2.5 lg:w-3 lg:h-3 rounded-sm bg-accent-emerald/30" />
                  app.harvestry.io/dashboard/{activeTab}
                </div>
              </div>
            </div>

            {/* Screenshot Image - more compact aspect ratio for desktop viewport fit */}
            <div className="relative aspect-[16/10] lg:aspect-[16/8] xl:aspect-[16/9] bg-background">
              <Image
                src={activeImage}
                alt={`Harvestry ${activeTab} dashboard`}
                fill
                className="object-cover object-top"
                priority
              />
            </div>
          </div>
        </div>

        {/* Feature highlights below screenshot */}
        <div className="mt-6 lg:mt-8 grid grid-cols-1 md:grid-cols-3 gap-4 lg:gap-6">
          {activeTab === 'cultivation' && (
            <>
              <FeatureHighlight
                title="Environmental Metrics"
                description="Real-time temperature, humidity, VPD, CO₂, and substrate readings at a glance."
              />
              <FeatureHighlight
                title="Trend Analysis"
                description="24-hour environmental trends with recipe overlays and override indicators."
              />
              <FeatureHighlight
                title="Zone Heatmaps"
                description="Visual sensor grid shows hot spots and environmental uniformity across zones."
              />
            </>
          )}
          
          {activeTab === 'irrigation' && (
            <>
              <FeatureHighlight
                title="Tank Management"
                description="Monitor all nutrient tanks with fill levels, EC, pH, and active recipes."
              />
              <FeatureHighlight
                title="Active Runs"
                description="Track irrigation programs in real-time with progress bars and zone assignments."
              />
              <FeatureHighlight
                title="Schedule Overview"
                description="View upcoming irrigation events and full schedule at a glance."
              />
            </>
          )}

          {activeTab === 'planner' && (
            <>
              <FeatureHighlight
                title="Visual Timeline"
                description="Drag-and-drop Gantt chart for scheduling batches across all rooms and zones."
              />
              <FeatureHighlight
                title="Capacity Planning"
                description="Automatically detect space conflicts and optimize room utilization efficiency."
              />
              <FeatureHighlight
                title="What-If Scenarios"
                description="Simulate schedule changes to see impact on harvest dates and projected yields."
              />
            </>
          )}

          {activeTab === 'tasks' && (
            <>
              <FeatureHighlight
                title="Kanban Board"
                description="Visualize workflow with customizable columns for pending, active, and completed tasks."
              />
              <FeatureHighlight
                title="SOP Integration"
                description="Tasks link directly to Standard Operating Procedures for consistent execution."
              />
              <FeatureHighlight
                title="Smart Assignments"
                description="Auto-assign tasks based on role, location, or specific employee availability."
              />
            </>
          )}
        </div>
      </div>
    </section>
  );
}

interface FeatureHighlightProps {
  title: string;
  description: string;
}

function FeatureHighlight({ title, description }: FeatureHighlightProps) {
  return (
    <div className="text-center p-4 lg:p-5 rounded-xl bg-surface/30 border border-border/50">
      <h3 className="text-base lg:text-lg font-semibold text-foreground mb-1.5">{title}</h3>
      <p className="text-sm text-muted-foreground leading-relaxed">{description}</p>
    </div>
  );
}

