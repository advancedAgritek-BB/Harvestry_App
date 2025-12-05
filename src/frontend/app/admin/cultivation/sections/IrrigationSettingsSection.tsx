'use client';

import React, { useState } from 'react';
import {
  Droplets,
  Gauge,
  Edit2,
  Save,
  AlertCircle,
  Lightbulb,
  Clock,
  TrendingUp,
  AlertTriangle,
} from 'lucide-react';
import {
  AdminCard,
  AdminSection,
  AdminGrid,
  FormField,
  Input,
  Switch,
  Button,
} from '@/components/admin';

interface IrrigationSettings {
  maxSystemFlowRateLitersPerMinute: number;
  flowRateSafetyMarginPercent: number;
  enableFlowRateQueuing: boolean;
  enableSmartSuggestions: boolean;
  suggestionThresholdCount: number;
}

interface FlowRateStats {
  currentFlowRate: number;
  utilizationPercent: number;
  queuedEventsToday: number;
  avgDelayMinutes: number;
}

// Mock data
const MOCK_SETTINGS: IrrigationSettings = {
  maxSystemFlowRateLitersPerMinute: 120,
  flowRateSafetyMarginPercent: 5,
  enableFlowRateQueuing: true,
  enableSmartSuggestions: true,
  suggestionThresholdCount: 3,
};

const MOCK_STATS: FlowRateStats = {
  currentFlowRate: 45.2,
  utilizationPercent: 37.7,
  queuedEventsToday: 2,
  avgDelayMinutes: 8.5,
};

const COMMON_FLOW_RATES = [
  { value: 60, label: 'Small System (60 L/min)' },
  { value: 120, label: 'Medium System (120 L/min)' },
  { value: 240, label: 'Large System (240 L/min)' },
  { value: 480, label: 'Industrial (480 L/min)' },
];

export function IrrigationSettingsSection() {
  const [isEditing, setIsEditing] = useState(false);
  const [settings, setSettings] = useState<IrrigationSettings>(MOCK_SETTINGS);
  const [stats] = useState<FlowRateStats>(MOCK_STATS);

  const effectiveMaxFlow = settings.maxSystemFlowRateLitersPerMinute * 
    (1 - settings.flowRateSafetyMarginPercent / 100);

  const handleSave = () => {
    // In a real app, this would save to the API
    console.log('Saving settings:', settings);
    setIsEditing(false);
  };

  const getUtilizationColor = (percent: number) => {
    if (percent < 50) return 'text-emerald-400';
    if (percent < 80) return 'text-amber-400';
    return 'text-red-400';
  };

  return (
    <AdminSection
      title="Flow Rate Management"
      description="Configure system flow rate limits and intelligent queue management"
    >
      <AdminGrid columns={2}>
        {/* Flow Rate Configuration */}
        <AdminCard
          title="System Flow Rate"
          icon={Gauge}
          actions={
            isEditing ? (
              <div className="flex gap-2">
                <Button variant="ghost" size="sm" onClick={() => setIsEditing(false)}>
                  Cancel
                </Button>
                <Button size="sm" onClick={handleSave}>
                  <Save className="w-4 h-4" />
                  Save
                </Button>
              </div>
            ) : (
              <Button variant="secondary" size="sm" onClick={() => setIsEditing(true)}>
                <Edit2 className="w-4 h-4" />
                Edit
              </Button>
            )
          }
        >
          <div className="space-y-6">
            <FormField 
              label="Maximum System Flow Rate" 
              description="Total flow capacity of your irrigation system (pump, lines, etc.)"
            >
              <div className="flex gap-2">
                <Input 
                  type="number" 
                  min={1}
                  step={1}
                  value={settings.maxSystemFlowRateLitersPerMinute} 
                  onChange={(e) => setSettings({ 
                    ...settings, 
                    maxSystemFlowRateLitersPerMinute: parseFloat(e.target.value) || 0 
                  })}
                  disabled={!isEditing}
                  className="flex-1"
                />
                <span className="flex items-center px-3 text-sm text-muted-foreground bg-white/5 border border-border rounded-lg">
                  L/min
                </span>
              </div>
            </FormField>

            {/* Quick presets */}
            {isEditing && (
              <div className="flex flex-wrap gap-2">
                {COMMON_FLOW_RATES.map((rate) => (
                  <button
                    key={rate.value}
                    onClick={() => setSettings({ 
                      ...settings, 
                      maxSystemFlowRateLitersPerMinute: rate.value 
                    })}
                    className={`px-3 py-1.5 text-xs rounded-lg border transition-colors ${
                      settings.maxSystemFlowRateLitersPerMinute === rate.value
                        ? 'bg-cyan-500/20 border-cyan-500/40 text-cyan-400'
                        : 'bg-white/5 border-border text-muted-foreground hover:bg-white/10'
                    }`}
                  >
                    {rate.label}
                  </button>
                ))}
              </div>
            )}

            <FormField 
              label="Safety Margin" 
              description="Never exceed this percentage below max (e.g., 5% = max 95% utilization)"
            >
              <div className="flex gap-2">
                <Input 
                  type="number" 
                  min={0}
                  max={50}
                  step={1}
                  value={settings.flowRateSafetyMarginPercent} 
                  onChange={(e) => setSettings({ 
                    ...settings, 
                    flowRateSafetyMarginPercent: parseFloat(e.target.value) || 0 
                  })}
                  disabled={!isEditing}
                  className="flex-1"
                />
                <span className="flex items-center px-3 text-sm text-muted-foreground bg-white/5 border border-border rounded-lg">
                  %
                </span>
              </div>
            </FormField>

            {/* Effective max display */}
            <div className="p-4 bg-cyan-500/10 border border-cyan-500/20 rounded-lg">
              <div className="flex items-center justify-between">
                <div>
                  <div className="text-sm font-medium text-foreground">Effective Maximum</div>
                  <div className="text-xs text-muted-foreground">
                    After applying {settings.flowRateSafetyMarginPercent}% safety margin
                  </div>
                </div>
                <div className="text-2xl font-bold text-cyan-400">
                  {effectiveMaxFlow.toFixed(1)} <span className="text-sm">L/min</span>
                </div>
              </div>
            </div>
          </div>
        </AdminCard>

        {/* Queue Settings */}
        <AdminCard
          title="Intelligent Queuing"
          icon={Clock}
        >
          <div className="space-y-6">
            <div className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
              <div>
                <div className="text-sm font-medium text-foreground">Enable Flow Rate Queuing</div>
                <div className="text-xs text-muted-foreground">
                  Automatically queue events when flow rate limit would be exceeded
                </div>
              </div>
              <Switch 
                checked={settings.enableFlowRateQueuing} 
                onChange={(checked) => setSettings({ ...settings, enableFlowRateQueuing: checked })}
                disabled={!isEditing}
              />
            </div>

            <div className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
              <div>
                <div className="text-sm font-medium text-foreground">Smart Schedule Suggestions</div>
                <div className="text-xs text-muted-foreground">
                  Get recommendations when schedules frequently conflict
                </div>
              </div>
              <Switch 
                checked={settings.enableSmartSuggestions} 
                onChange={(checked) => setSettings({ ...settings, enableSmartSuggestions: checked })}
                disabled={!isEditing}
              />
            </div>

            <FormField 
              label="Suggestion Threshold" 
              description="Number of queue events before showing optimization suggestions"
            >
              <Input 
                type="number" 
                min={1}
                max={20}
                value={settings.suggestionThresholdCount} 
                onChange={(e) => setSettings({ 
                  ...settings, 
                  suggestionThresholdCount: parseInt(e.target.value) || 3 
                })}
                disabled={!isEditing || !settings.enableSmartSuggestions}
              />
            </FormField>

            {/* Info box */}
            <div className="p-3 bg-amber-500/10 border border-amber-500/20 rounded-lg">
              <div className="flex items-start gap-2">
                <Lightbulb className="w-4 h-4 text-amber-400 mt-0.5 flex-shrink-0" />
                <div className="text-xs text-amber-200">
                  <strong>How it works:</strong> When an irrigation event would exceed your 
                  flow rate limit, it's automatically queued with a visible <code className="px-1 py-0.5 bg-amber-500/20 rounded">+[delay]</code> indicator. 
                  After {settings.suggestionThresholdCount} queued events, you'll see suggestions to optimize your schedule.
                </div>
              </div>
            </div>
          </div>
        </AdminCard>
      </AdminGrid>

      {/* Current Status Stats */}
      <AdminCard
        title="Current Flow Status"
        icon={Droplets}
        className="mt-6"
      >
        <div className="grid grid-cols-4 gap-6">
          <div className="p-4 bg-white/5 rounded-lg text-center">
            <div className="text-2xl font-bold text-foreground mb-1">
              {stats.currentFlowRate.toFixed(1)}
            </div>
            <div className="text-xs text-muted-foreground">Current Flow (L/min)</div>
            <div className={`text-xs mt-1 ${getUtilizationColor(stats.utilizationPercent)}`}>
              {stats.utilizationPercent.toFixed(1)}% of max
            </div>
          </div>

          <div className="p-4 bg-white/5 rounded-lg text-center">
            <div className="text-2xl font-bold text-foreground mb-1">
              {effectiveMaxFlow.toFixed(1)}
            </div>
            <div className="text-xs text-muted-foreground">Effective Limit (L/min)</div>
            <div className="text-xs text-cyan-400 mt-1">
              {settings.flowRateSafetyMarginPercent}% safety margin
            </div>
          </div>

          <div className="p-4 bg-white/5 rounded-lg text-center">
            <div className="text-2xl font-bold text-foreground mb-1">
              {stats.queuedEventsToday}
            </div>
            <div className="text-xs text-muted-foreground">Queued Today</div>
            {stats.queuedEventsToday > 0 ? (
              <div className="text-xs text-amber-400 mt-1 flex items-center justify-center gap-1">
                <AlertTriangle className="w-3 h-3" />
                Review schedule
              </div>
            ) : (
              <div className="text-xs text-emerald-400 mt-1">No delays</div>
            )}
          </div>

          <div className="p-4 bg-white/5 rounded-lg text-center">
            <div className="text-2xl font-bold text-foreground mb-1">
              {stats.avgDelayMinutes.toFixed(1)}
            </div>
            <div className="text-xs text-muted-foreground">Avg Delay (min)</div>
            {stats.avgDelayMinutes > 10 ? (
              <div className="text-xs text-amber-400 mt-1">Consider optimization</div>
            ) : (
              <div className="text-xs text-emerald-400 mt-1">Within normal range</div>
            )}
          </div>
        </div>

        {/* Usage visualization */}
        <div className="mt-6 p-4 bg-white/5 rounded-lg">
          <div className="flex items-center justify-between mb-2">
            <span className="text-sm font-medium text-foreground">Flow Rate Utilization</span>
            <span className={`text-sm font-bold ${getUtilizationColor(stats.utilizationPercent)}`}>
              {stats.utilizationPercent.toFixed(1)}%
            </span>
          </div>
          <div className="h-3 bg-muted rounded-full overflow-hidden">
            <div 
              className={`h-full rounded-full transition-all duration-500 ${
                stats.utilizationPercent < 50 ? 'bg-emerald-500' :
                stats.utilizationPercent < 80 ? 'bg-amber-500' : 'bg-red-500'
              }`}
              style={{ width: `${Math.min(stats.utilizationPercent, 100)}%` }}
            />
          </div>
          <div className="flex justify-between mt-1 text-xs text-muted-foreground">
            <span>0 L/min</span>
            <span className="text-cyan-400">
              Safe limit: {effectiveMaxFlow.toFixed(0)} L/min
            </span>
            <span>Max: {settings.maxSystemFlowRateLitersPerMinute} L/min</span>
          </div>
        </div>
      </AdminCard>

      {/* Smart Suggestions Preview */}
      {settings.enableSmartSuggestions && stats.queuedEventsToday >= settings.suggestionThresholdCount && (
        <AdminCard
          title="Schedule Optimization Suggestions"
          icon={TrendingUp}
          className="mt-6"
        >
          <div className="space-y-3">
            <div className="p-4 bg-cyan-500/10 border border-cyan-500/20 rounded-lg">
              <div className="flex items-start gap-3">
                <div className="w-8 h-8 rounded-lg bg-cyan-500/20 flex items-center justify-center shrink-0">
                  <Clock className="w-4 h-4 text-cyan-400" />
                </div>
                <div className="flex-1">
                  <div className="font-medium text-foreground">Shift schedule to reduce congestion</div>
                  <p className="text-sm text-muted-foreground mt-1">
                    Schedules at 8:00 AM are frequently delayed. Consider moving some programs to 7:00 AM or 9:00 AM to reduce queue times.
                  </p>
                  <div className="flex items-center gap-4 mt-2 text-xs">
                    <span className="text-muted-foreground">Current: <span className="text-foreground">8:00 AM</span></span>
                    <span className="text-muted-foreground">Suggested: <span className="text-cyan-400">7:00 AM</span></span>
                    <span className="text-emerald-400">~5 min saved per event</span>
                  </div>
                </div>
              </div>
            </div>

            <div className="p-4 bg-white/5 border border-border rounded-lg">
              <div className="flex items-start gap-3">
                <div className="w-8 h-8 rounded-lg bg-amber-500/20 flex items-center justify-center shrink-0">
                  <AlertCircle className="w-4 h-4 text-amber-400" />
                </div>
                <div className="flex-1">
                  <div className="font-medium text-foreground">Use sequential zone scheduling</div>
                  <p className="text-sm text-muted-foreground mt-1">
                    Instead of scheduling multiple zones simultaneously, stagger zone starts by 5-10 minutes to stay within flow rate limits.
                  </p>
                  <div className="flex items-center gap-4 mt-2 text-xs">
                    <span className="text-muted-foreground">Current: <span className="text-foreground">Simultaneous</span></span>
                    <span className="text-muted-foreground">Suggested: <span className="text-cyan-400">5-10 min stagger</span></span>
                    <span className="text-emerald-400">~8 min avg delay reduction</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </AdminCard>
      )}
    </AdminSection>
  );
}



