'use client';

import React, { Suspense } from 'react';
import { CultivationLayout, CultivationSection } from '@/features/dashboard/layouts/CultivationLayout';
import { EnvironmentalMetricsWidget } from '@/features/dashboard/widgets/cultivation/EnvironmentalMetricsWidget';
import { EnvironmentalTrendsWidget } from '@/features/dashboard/widgets/cultivation/EnvironmentalTrendsWidget';
import { IrrigationWindowsWidget } from '@/features/dashboard/widgets/cultivation/IrrigationWindowsWidget';
import { ZoneHeatmapWidget } from '@/features/dashboard/widgets/cultivation/ZoneHeatmapWidget';
import { RoomsStatusWidget } from '@/features/dashboard/widgets/cultivation/RoomsStatusWidget';
import { 
  ActiveAlertsListWidget, 
  TargetsVsCurrentWidget, 
  QuickActionsWidget 
} from '@/features/dashboard/widgets/cultivation/SidebarWidgets';
import { SiteRoomSelector } from '@/components/SiteRoomSelector';
import { useSelectedSite, useSelectedRoom } from '@/stores/siteRoomStore';

function CultivationHeader() {
  const selectedSite = useSelectedSite();
  const selectedRoom = useSelectedRoom();

  return (
    <div className="h-14 border-b border-border flex items-center px-6 justify-between bg-surface/50 backdrop-blur shrink-0">
      <h1 className="font-bold text-lg tracking-tight text-foreground">
        Harvestry{' '}
        <span className="text-muted-foreground/60 font-light">
          / Cultivation / {selectedRoom?.name || 'Select Room'}
        </span>
      </h1>
      <div className="flex items-center gap-4">
        <SiteRoomSelector />
        <span className="text-sm text-muted-foreground">Range: Last 24h</span>
      </div>
    </div>
  );
}

function HeaderFallback() {
  return (
    <div className="h-14 border-b border-border flex items-center px-6 justify-between bg-surface/50 backdrop-blur shrink-0">
      <h1 className="font-bold text-lg tracking-tight text-foreground">
        Harvestry <span className="text-muted-foreground/60 font-light">/ Cultivation / Loading...</span>
      </h1>
      <div className="flex gap-4 text-sm text-muted-foreground">
        <span>Loading...</span>
      </div>
    </div>
  );
}

export default function CultivationDashboardPage() {
  return (
    <div className="flex flex-col h-full">
      {/* Context Header / Toolbar */}
      <Suspense fallback={<HeaderFallback />}>
        <CultivationHeader />
      </Suspense>

      <CultivationLayout className="flex-1 overflow-y-auto p-4">
        {/* Left Main Column (10 cols) */}
        <CultivationSection span={10} className="flex flex-col gap-4">
           {/* 1. Metrics Row */}
           <div className="h-auto">
             <EnvironmentalMetricsWidget />
           </div>

           {/* 2. Trends Chart */}
           <div className="h-[500px]">
             <EnvironmentalTrendsWidget />
           </div>

           {/* 3. Irrigation + Heatmap Row */}
           <div className="grid grid-cols-12 gap-4 h-[350px]">
              <div className="col-span-12 lg:col-span-7 h-full">
                <IrrigationWindowsWidget />
              </div>
              <div className="col-span-12 lg:col-span-5 h-full">
                <ZoneHeatmapWidget />
              </div>
           </div>

           {/* 4. Rooms Footer */}
           <div className="h-auto">
              <RoomsStatusWidget />
           </div>
        </CultivationSection>

        {/* Right Sidebar Column (2 cols) */}
        <CultivationSection span={2} className="flex flex-col gap-4 h-full">
          <div className="flex-none">
            <ActiveAlertsListWidget />
          </div>
          <div className="flex-none">
            <TargetsVsCurrentWidget />
          </div>
          <div className="flex-none">
            <QuickActionsWidget />
          </div>
        </CultivationSection>
      </CultivationLayout>
    </div>
  );
}
