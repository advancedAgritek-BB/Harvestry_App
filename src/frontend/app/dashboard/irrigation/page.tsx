'use client';

import React from 'react';
import { TankCard, TankData } from '@/features/dashboard/widgets/irrigation/TankCard';
import { NowPlayingWidget } from '@/features/dashboard/widgets/irrigation/NowPlayingWidget';
import { UpcomingScheduleWidget } from '@/features/dashboard/widgets/irrigation/UpcomingScheduleWidget';
import { SystemHealthWidget } from '@/features/dashboard/widgets/irrigation/SystemHealthWidget';

export default function IrrigationOverviewPage() {
  // Mock Data for Tanks
  const tanks: TankData[] = [
    {
      id: 'tank-a',
      name: 'Mix Tank A',
      fillPercentage: 45,
      capacityGallons: 100,
      currentGallons: 45,
      sensors: { ec: 2.4, ph: 5.8, temp: 68 },
      lastFill: '2h ago',
      estDepletion: '~4h',
      recipe: { id: 'r1', name: 'Veg Week 3' },
      zones: ['A', 'B', 'C'],
      status: 'feeding'
    },
    {
      id: 'tank-b',
      name: 'Mix Tank B',
      fillPercentage: 85,
      capacityGallons: 100,
      currentGallons: 85,
      sensors: { ec: 2.8, ph: 5.9, temp: 69 },
      lastFill: '30m ago',
      estDepletion: '~12h',
      recipe: { id: 'r2', name: 'Flower Week 6' },
      zones: ['D', 'E'],
      status: 'mixing'
    },
    {
      id: 'tank-c',
      name: 'Mix Tank C',
      fillPercentage: 12,
      capacityGallons: 50,
      currentGallons: 6,
      sensors: { ec: 0.2, ph: 7.0, temp: 70 },
      lastFill: '1d ago',
      estDepletion: 'Emptying',
      recipe: { id: 'r3', name: 'Flush' },
      zones: [],
      status: 'filling'
    },
    {
      id: 'tank-d',
      name: 'Mix Tank D',
      fillPercentage: 5,
      capacityGallons: 100,
      currentGallons: 5,
      sensors: { ec: 0.0, ph: 7.0, temp: 72 },
      lastFill: '2d ago',
      estDepletion: 'Empty',
      recipe: { id: 'r0', name: 'None' },
      zones: [],
      status: 'error' // Simulating low level alarm
    },
    {
      id: 'tank-e',
      name: 'Mix Tank E',
      fillPercentage: 65,
      capacityGallons: 200,
      currentGallons: 130,
      sensors: { ec: 1.8, ph: 6.0, temp: 67 },
      lastFill: '4h ago',
      estDepletion: '~8h',
      recipe: { id: 'r4', name: 'Veg Early' },
      zones: ['F', 'G'],
      status: 'idle'
    },
    {
      id: 'tank-f',
      name: 'Mix Tank F',
      fillPercentage: 95,
      capacityGallons: 200,
      currentGallons: 190,
      sensors: { ec: 3.0, ph: 5.7, temp: 68 },
      lastFill: '10m ago',
      estDepletion: '~24h',
      recipe: { id: 'r5', name: 'Flower Late' },
      zones: ['H'],
      status: 'idle'
    }
  ];

  return (
    <div className="flex flex-col gap-6 max-w-[1600px] mx-auto">
      
      {/* 1. Tank Hero Row (Grid Layout) */}
      <section className="w-full">
        <div className="flex items-center justify-between mb-3 px-1">
          <h2 className="text-sm font-bold text-muted-foreground uppercase tracking-wider">Nutrient Delivery Systems</h2>
          <span className="text-xs text-muted-foreground">6 Tanks Online</span>
        </div>
        
        {/* Grid Container */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6 gap-4">
          {tanks.map(tank => (
            <TankCard key={tank.id} tank={tank} className="h-full" />
          ))}
        </div>
      </section>

      {/* 2. Operations Row (Now Playing + Schedule) */}
      <section className="grid grid-cols-1 lg:grid-cols-2 gap-6 h-[220px]">
        <NowPlayingWidget />
        <UpcomingScheduleWidget />
      </section>

      {/* 3. System Health Footer */}
      <section className="mt-auto">
        <SystemHealthWidget />
      </section>

    </div>
  );
}

