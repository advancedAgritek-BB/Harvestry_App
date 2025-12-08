'use client';

import React, { useState } from 'react';
import Link from 'next/link';
import { Droplets, Lightbulb, Sprout, ArrowRight } from 'lucide-react';
import { RoomCard } from '../components/RoomCard/RoomCard';
import { AlertBanner, CriticalAlert } from '../components/AlertBanner';
import { FacilityHealthBar } from '../components/FacilityHealthBar';
import { ActionPanel, ActionItem } from '../components/ActionPanel';
import { IrrigationStatusWidget } from '../widgets/operations/IrrigationStatusWidget';
import { ActiveBatchesWidget } from '../widgets/operations/ActiveBatchesWidget';
import { TaskRecommendationsWidget } from '../widgets/operations/TaskRecommendationsWidget';
import { useDashboardPermissions } from '@/stores/auth/authStore';

// Types
interface RoomData {
  id: string;
  name: string;
  stage: 'clone' | 'veg' | 'flower' | 'drying';
  temp: number;
  tempTarget: number;
  rh: number;
  rhTarget: number;
  ec?: number;
  vpd: number;
  status: 'healthy' | 'warning' | 'critical';
  lightCycle?: string;
  plantCount?: number;
}

// Mock Data
const MOCK_ROOMS: RoomData[] = [
  { 
    id: 'f1', name: 'Flower • F1', stage: 'flower',
    temp: 76.6, tempTarget: 77, rh: 55, rhTarget: 55,
    ec: 2.3, vpd: 1.32, status: 'warning',
    lightCycle: '12/12', plantCount: 450
  },
  { 
    id: 'f2', name: 'Flower • F2', stage: 'flower',
    temp: 77.2, tempTarget: 77, rh: 53, rhTarget: 55,
    ec: 2.4, vpd: 1.41, status: 'healthy',
    lightCycle: '12/12', plantCount: 380
  },
  { 
    id: 'v1', name: 'Veg • V1', stage: 'veg',
    temp: 73.8, tempTarget: 75, rh: 60, rhTarget: 60,
    ec: 1.8, vpd: 1.08, status: 'warning',
    lightCycle: '18/6', plantCount: 520
  },
  { 
    id: 'd1', name: 'Dry Room', stage: 'drying',
    temp: 65.3, tempTarget: 65, rh: 50, rhTarget: 50,
    vpd: 0.98, status: 'healthy'
  },
  { 
    id: 'f3', name: 'Flower • F3', stage: 'flower',
    temp: 76.8, tempTarget: 77, rh: 54, rhTarget: 55,
    ec: 2.4, vpd: 1.36, status: 'healthy',
    lightCycle: '12/12', plantCount: 410
  },
  { 
    id: 'v2', name: 'Veg • V2', stage: 'veg',
    temp: 74.1, tempTarget: 75, rh: 58, rhTarget: 60,
    ec: 1.9, vpd: 1.15, status: 'healthy',
    lightCycle: '18/6', plantCount: 340
  },
  { 
    id: 'c1', name: 'Clone Room', stage: 'clone',
    temp: 78.2, tempTarget: 78, rh: 75, rhTarget: 75,
    vpd: 0.72, status: 'healthy',
    plantCount: 800
  },
];

const MOCK_CRITICAL_ALERTS: CriticalAlert[] = [
  {
    id: 'crit-1',
    title: 'HVAC-01 Malfunction - Temperature Rising in F1',
    source: 'Environment Control',
    timestamp: new Date(Date.now() - 5 * 60000).toISOString(),
    href: '/dashboard/cultivation?room=f1',
  },
];

const MOCK_ACTION_ITEMS: ActionItem[] = [
  {
    id: 'alert-1',
    type: 'alert',
    title: 'HVAC-01 Malfunction',
    source: 'Environment',
    severity: 'critical',
    timestamp: new Date(Date.now() - 5 * 60000).toISOString(),
    href: '/dashboard/cultivation?room=f1',
  },
  {
    id: 'alert-2',
    type: 'alert',
    title: 'Humidity High (72%)',
    source: 'Room B',
    severity: 'warning',
    timestamp: new Date(Date.now() - 15 * 60000).toISOString(),
    href: '/dashboard/cultivation?room=v1',
  },
  {
    id: 'task-1',
    type: 'task',
    title: 'Inspect Mother Room A for PM',
    source: 'Room A (Mothers)',
    priority: 'critical',
    slaStatus: 'breached',
    timestamp: new Date(Date.now() - 120 * 60000).toISOString(),
    assignee: { id: 'u1', firstName: 'Marcus', lastName: 'Johnson' },
    href: '/dashboard/tasks',
  },
  {
    id: 'task-2',
    type: 'task',
    title: 'Transplant Batch B-203',
    source: 'Veg Room 2',
    priority: 'high',
    slaStatus: 'warning',
    timestamp: new Date(Date.now() - 60 * 60000).toISOString(),
    assignee: { id: 'u2', firstName: 'Sarah', lastName: 'Chen' },
    href: '/dashboard/tasks',
  },
  {
    id: 'task-3',
    type: 'task',
    title: 'Calibrate pH Sensors',
    source: 'Irrigation Zone 1',
    priority: 'normal',
    slaStatus: 'ok',
    timestamp: new Date(Date.now() - 180 * 60000).toISOString(),
    href: '/dashboard/tasks',
  },
  {
    id: 'alert-3',
    type: 'alert',
    title: 'Water Tank Refilled',
    source: 'Irrigation',
    severity: 'info',
    timestamp: new Date(Date.now() - 45 * 60000).toISOString(),
    href: '/dashboard/irrigation',
  },
  {
    id: 'task-4',
    type: 'task',
    title: 'Refill Nutrient Tanks',
    source: 'Nutrient Room',
    priority: 'normal',
    slaStatus: 'ok',
    timestamp: new Date(Date.now() - 240 * 60000).toISOString(),
    assignee: { id: 'u3', firstName: 'David', lastName: 'Martinez' },
    href: '/dashboard/tasks',
  },
];

export function OperationsOverview() {
  const [dismissedAlerts, setDismissedAlerts] = useState<Set<string>>(new Set());
  
  // Get permission-based visibility
  const { 
    canViewAlerts, 
    canViewTasks, 
    canViewIrrigation, 
    canViewAISuggestions 
  } = useDashboardPermissions();

  // Filter out dismissed critical alerts (only if user can view alerts)
  const activeCriticalAlerts = canViewAlerts 
    ? MOCK_CRITICAL_ALERTS.filter(alert => !dismissedAlerts.has(alert.id))
    : [];

  const handleDismissAlert = (alertId: string) => {
    setDismissedAlerts(prev => new Set([...prev, alertId]));
  };

  // Filter action items based on permissions
  const visibleActionItems = MOCK_ACTION_ITEMS.filter(item => {
    if (item.type === 'alert' && !canViewAlerts) return false;
    if (item.type === 'task' && !canViewTasks) return false;
    return true;
  });

  // Calculate stats (respecting permissions)
  const alertCount = canViewAlerts 
    ? MOCK_ACTION_ITEMS.filter(i => i.type === 'alert').length 
    : 0;
  const taskCount = canViewTasks 
    ? MOCK_ACTION_ITEMS.filter(i => i.type === 'task').length 
    : 0;
  const totalPlants = MOCK_ROOMS.reduce((acc, r) => acc + (r.plantCount || 0), 0);

  return (
    <div className="flex flex-col h-full bg-gradient-to-br from-background via-background to-surface/30">
      {/* Header - Refined with gradient accent */}
      <header className="h-14 flex items-center px-6 justify-between bg-gradient-to-r from-surface/60 via-surface/40 to-transparent backdrop-blur-xl shrink-0 relative">
        {/* Subtle bottom gradient line instead of border */}
        <div className="absolute bottom-0 left-0 right-0 h-px bg-gradient-to-r from-transparent via-white/10 to-transparent" />
        
        <div className="flex items-center gap-4">
          <h1 className="font-bold text-lg tracking-tight text-foreground">
            Operations Overview
          </h1>
          <span className="text-sm text-muted-foreground/70">Evergreen Facility</span>
        </div>
        <div className="flex items-center gap-3">
          <div className="flex items-center gap-2 px-3 py-1.5 rounded-full bg-emerald-500/15 ring-1 ring-emerald-400/20 shadow-sm shadow-emerald-500/10">
            <div className="w-2 h-2 rounded-full bg-emerald-400 animate-pulse shadow-[0_0_8px_rgba(52,211,153,0.6)]" />
            <span className="text-xs font-semibold text-emerald-400">Live</span>
          </div>
          <span className="text-xs text-muted-foreground/60">Updated just now</span>
        </div>
      </header>

      {/* Main Content - Command Center Layout */}
      <div className="flex-1 overflow-hidden flex">
        {/* Left Zone - Attention Area (~65%) */}
        <div className="flex-1 flex flex-col overflow-y-auto p-6 gap-5">
          {/* Critical Alert Banner - Only shows when critical issues exist */}
          {activeCriticalAlerts.length > 0 && (
            <AlertBanner 
              alerts={activeCriticalAlerts}
              onDismiss={handleDismissAlert}
            />
          )}

          {/* Facility Health Bar - Replaces KPI tiles */}
          <FacilityHealthBar
            rooms={MOCK_ROOMS.map(r => ({ id: r.id, name: r.name, status: r.status }))}
            alertCount={alertCount}
            taskCount={taskCount}
            systemHealth={98.2}
          />

          {/* Room Grid - Compact Cards */}
          <section>
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-sm font-semibold text-foreground">
                Cultivation Rooms
              </h2>
              <span className="text-xs text-muted-foreground">
                {MOCK_ROOMS.length} rooms • {totalPlants.toLocaleString()} plants
              </span>
            </div>
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
              {MOCK_ROOMS.map((room) => (
                <RoomCard key={room.id} room={room} />
              ))}
            </div>
          </section>

          {/* Widget Panels - Permission Gated */}
          <section className="mt-auto pt-4">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              {canViewIrrigation && (
                <WidgetPanel
                  icon={Droplets}
                  title="Irrigation Status"
                  href="/dashboard/irrigation"
                  accentColor="cyan"
                >
                  <IrrigationStatusWidget />
                </WidgetPanel>
              )}
              <WidgetPanel
                icon={Sprout}
                title="Active Batches"
                href="/inventory/batches"
                accentColor="emerald"
              >
                <ActiveBatchesWidget />
              </WidgetPanel>
              {canViewAISuggestions && (
                <WidgetPanel
                  icon={Lightbulb}
                  title="AI Suggestions"
                  href="/dashboard/recipes"
                  accentColor="amber"
                  badge="AI"
                >
                  <TaskRecommendationsWidget />
                </WidgetPanel>
              )}
            </div>
          </section>
        </div>

        {/* Right Zone - Action Panel (~35%) - Permission Gated */}
        {(canViewAlerts || canViewTasks) && (
          <aside className="w-[420px] relative p-6 overflow-hidden flex flex-col bg-gradient-to-br from-surface/40 via-surface/20 to-transparent">
            {/* Left edge gradient line */}
            <div className="absolute top-0 bottom-0 left-0 w-px bg-gradient-to-b from-transparent via-white/10 to-transparent" />
            
            <ActionPanel 
              items={visibleActionItems}
              showAlerts={canViewAlerts}
              showTasks={canViewTasks}
              maxAlerts={3}
              maxTasks={4}
              className="flex-1"
            />
          </aside>
        )}
      </div>
    </div>
  );
}

// Widget Panel Component
interface WidgetPanelProps {
  icon: React.ElementType;
  title: string;
  href: string;
  accentColor: 'cyan' | 'emerald' | 'amber' | 'violet';
  badge?: string;
  children: React.ReactNode;
}

const ACCENT_COLORS = {
  cyan: {
    bg: 'bg-cyan-500/20',
    iconBg: 'bg-gradient-to-br from-cyan-500/30 to-cyan-600/20',
    iconRing: 'ring-1 ring-cyan-400/30',
    iconShadow: 'shadow-lg shadow-cyan-500/20',
    text: 'text-cyan-400',
    cardGradient: 'from-cyan-500/10 via-surface/40 to-surface/20',
    cardShadow: 'shadow-lg shadow-cyan-500/5',
    hoverShadow: 'hover:shadow-xl hover:shadow-cyan-500/10',
    headerBg: 'bg-gradient-to-r from-cyan-500/10 to-transparent',
  },
  emerald: {
    bg: 'bg-emerald-500/20',
    iconBg: 'bg-gradient-to-br from-emerald-500/30 to-emerald-600/20',
    iconRing: 'ring-1 ring-emerald-400/30',
    iconShadow: 'shadow-lg shadow-emerald-500/20',
    text: 'text-emerald-400',
    cardGradient: 'from-emerald-500/10 via-surface/40 to-surface/20',
    cardShadow: 'shadow-lg shadow-emerald-500/5',
    hoverShadow: 'hover:shadow-xl hover:shadow-emerald-500/10',
    headerBg: 'bg-gradient-to-r from-emerald-500/10 to-transparent',
  },
  amber: {
    bg: 'bg-amber-500/20',
    iconBg: 'bg-gradient-to-br from-amber-500/30 to-amber-600/20',
    iconRing: 'ring-1 ring-amber-400/30',
    iconShadow: 'shadow-lg shadow-amber-500/20',
    text: 'text-amber-400',
    cardGradient: 'from-amber-500/10 via-surface/40 to-surface/20',
    cardShadow: 'shadow-lg shadow-amber-500/5',
    hoverShadow: 'hover:shadow-xl hover:shadow-amber-500/10',
    headerBg: 'bg-gradient-to-r from-amber-500/10 to-transparent',
  },
  violet: {
    bg: 'bg-violet-500/20',
    iconBg: 'bg-gradient-to-br from-violet-500/30 to-violet-600/20',
    iconRing: 'ring-1 ring-violet-400/30',
    iconShadow: 'shadow-lg shadow-violet-500/20',
    text: 'text-violet-400',
    cardGradient: 'from-violet-500/10 via-surface/40 to-surface/20',
    cardShadow: 'shadow-lg shadow-violet-500/5',
    hoverShadow: 'hover:shadow-xl hover:shadow-violet-500/10',
    headerBg: 'bg-gradient-to-r from-violet-500/10 to-transparent',
  },
};

function WidgetPanel({ icon: Icon, title, href, accentColor, badge, children }: WidgetPanelProps) {
  const colors = ACCENT_COLORS[accentColor];

  return (
    <div className={`
      group flex flex-col rounded-2xl overflow-hidden
      bg-gradient-to-br ${colors.cardGradient}
      backdrop-blur-sm ${colors.cardShadow}
      transition-all duration-300 ${colors.hoverShadow}
    `}>
      {/* Header with gradient accent */}
      <div className={`
        flex items-center justify-between p-4
        ${colors.headerBg}
      `}>
        <div className="flex items-center gap-3">
          <div className={`
            p-2.5 rounded-xl transition-transform group-hover:scale-105
            ${colors.iconBg} ${colors.iconRing} ${colors.iconShadow}
          `}>
            <Icon className={`w-5 h-5 ${colors.text}`} />
          </div>
          <h3 className="text-sm font-semibold text-foreground">
            {title}
          </h3>
          {badge && (
            <span className="px-2 py-0.5 text-[9px] font-bold uppercase rounded-full bg-gradient-to-r from-amber-500/25 to-orange-500/15 text-amber-300 ring-1 ring-amber-400/30 shadow-sm shadow-amber-500/20">
              {badge}
            </span>
          )}
        </div>
        <Link
          href={href}
          className={`
            flex items-center gap-1.5 text-sm font-medium
            ${colors.text} opacity-70 hover:opacity-100
            transition-all duration-200
            group-hover:gap-2
          `}
        >
          View <ArrowRight className="w-4 h-4 transition-transform group-hover:translate-x-0.5" />
        </Link>
      </div>
      
      {/* Content */}
      <div className="p-4 pt-2">
        {children}
      </div>
    </div>
  );
}
