'use client';

import React, { useState } from 'react';
import { Brain, Leaf, TrendingUp, BarChart3 } from 'lucide-react';
import { cn } from '@/lib/utils';
import { AIInsightsPage } from '@/features/ai';
import { SustainabilityPage } from '@/features/sustainability';

type TabId = 'overview' | 'ai' | 'sustainability';

interface Tab {
  id: TabId;
  label: string;
  icon: React.ElementType;
  badge?: string;
}

const TABS: Tab[] = [
  { id: 'overview', label: 'Overview', icon: BarChart3 },
  { id: 'ai', label: 'AI Insights', icon: Brain, badge: 'Coming Soon' },
  { id: 'sustainability', label: 'ESG', icon: Leaf, badge: 'Coming Soon' },
];

export default function AnalyticsPage() {
  const [activeTab, setActiveTab] = useState<TabId>('overview');

  return (
    <div className="flex flex-col h-full">
      {/* Tab Navigation */}
      <div className="flex items-center gap-2 border-b border-border px-6 py-3 bg-surface/50">
        {TABS.map((tab) => {
          const Icon = tab.icon;
          const isActive = activeTab === tab.id;
          
          return (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={cn(
                "flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition-all",
                isActive 
                  ? "bg-cyan-500/10 text-cyan-400" 
                  : "text-muted-foreground hover:text-foreground hover:bg-muted/50"
              )}
            >
              <Icon className="w-4 h-4" />
              <span>{tab.label}</span>
              {tab.badge && (
                <span className="px-1.5 py-0.5 text-[10px] rounded bg-violet-500/10 text-violet-400 border border-violet-500/20">
                  {tab.badge}
                </span>
              )}
            </button>
          );
        })}
      </div>

      {/* Tab Content */}
      <div className="flex-1 overflow-auto p-6">
        {activeTab === 'overview' && <OverviewTab />}
        {activeTab === 'ai' && <AIInsightsPage />}
        {activeTab === 'sustainability' && <SustainabilityPage />}
      </div>
    </div>
  );
}

function OverviewTab() {
  return (
    <div className="max-w-4xl mx-auto">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-foreground mb-2">Analytics Overview</h1>
        <p className="text-muted-foreground">
          Monitor key performance indicators and track your cultivation metrics.
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
        <div className="bg-surface/50 border border-border rounded-xl p-6">
          <div className="flex items-center gap-3 mb-4">
            <div className="w-10 h-10 rounded-lg bg-emerald-500/10 flex items-center justify-center">
              <TrendingUp className="w-5 h-5 text-emerald-400" />
            </div>
            <div>
              <div className="text-2xl font-bold text-foreground">94%</div>
              <div className="text-xs text-muted-foreground">Avg. Environment Score</div>
            </div>
          </div>
          <div className="text-xs text-emerald-400">+2.3% from last week</div>
        </div>

        <div className="bg-surface/50 border border-border rounded-xl p-6">
          <div className="flex items-center gap-3 mb-4">
            <div className="w-10 h-10 rounded-lg bg-cyan-500/10 flex items-center justify-center">
              <BarChart3 className="w-5 h-5 text-cyan-400" />
            </div>
            <div>
              <div className="text-2xl font-bold text-foreground">1.2g/W</div>
              <div className="text-xs text-muted-foreground">Light Efficiency</div>
            </div>
          </div>
          <div className="text-xs text-cyan-400">On target</div>
        </div>

        <div className="bg-surface/50 border border-border rounded-xl p-6">
          <div className="flex items-center gap-3 mb-4">
            <div className="w-10 h-10 rounded-lg bg-amber-500/10 flex items-center justify-center">
              <Leaf className="w-5 h-5 text-amber-400" />
            </div>
            <div>
              <div className="text-2xl font-bold text-foreground">3.2L/g</div>
              <div className="text-xs text-muted-foreground">Water Usage</div>
            </div>
          </div>
          <div className="text-xs text-amber-400">-5% improvement</div>
        </div>
      </div>

      <div className="bg-surface/50 border border-border rounded-xl p-6">
        <h3 className="font-semibold text-foreground mb-4">Recent Trends</h3>
        <div className="h-64 flex items-center justify-center text-muted-foreground">
          <p className="text-sm">Chart visualization would appear here</p>
        </div>
      </div>
    </div>
  );
}
