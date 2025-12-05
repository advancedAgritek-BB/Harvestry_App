'use client';

import React, { Suspense } from 'react';
import { useSearchParams } from 'next/navigation';
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

// Room data lookup - in production this would come from an API/store
const ROOMS_DATA: Record<string, { name: string; stage: string }> = {
  'f1': { name: 'Flower • F1', stage: 'Flowering' },
  'f2': { name: 'Flower • F2', stage: 'Flowering' },
  'f3': { name: 'Flower • F3', stage: 'Flowering' },
  'v1': { name: 'Veg • V1', stage: 'Vegetative' },
  'v2': { name: 'Veg • V2', stage: 'Vegetative' },
  'c1': { name: 'Clone Room', stage: 'Clone' },
  'd1': { name: 'Dry Room', stage: 'Drying' },
};

function CultivationHeader() {
  const searchParams = useSearchParams();
  const roomId = searchParams.get('room') || 'f1';
  const roomData = ROOMS_DATA[roomId] || { name: 'Unknown Room', stage: 'Unknown' };

  return (
    <div className="h-14 border-b border-border flex items-center px-6 justify-between bg-surface/50 backdrop-blur shrink-0">
      <h1 className="font-bold text-lg tracking-tight text-foreground">
        Harvestry <span className="text-muted-foreground/60 font-light">/ Cultivation / {roomData.name}</span>
      </h1>
      <div className="flex gap-4 text-sm text-muted-foreground">
        <span>Site: Evergreen</span>
        <span>Range: Last 24h</span>
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
        <span>Site: Evergreen</span>
        <span>Range: Last 24h</span>
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
          <div className="flex-1 min-h-[200px]">
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
