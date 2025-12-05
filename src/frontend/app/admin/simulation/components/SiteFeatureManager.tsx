'use client';

import React, { useState } from 'react';
import { 
  Settings2, 
  ChevronDown, 
  ChevronRight, 
  Play, 
  Flag, 
  Check, 
  X,
  Building2,
  Sparkles
} from 'lucide-react';
import { useAuthStore, SitePermissions } from '@/stores/auth';

// Feature flags that can be assigned to sites
const AVAILABLE_FEATURE_FLAGS = [
  { id: 'closed_loop_ecph_enabled', name: 'Closed-Loop EC/pH', category: 'Control' },
  { id: 'autosteer_mpc_enabled', name: 'Autosteer MPC', category: 'AI' },
  { id: 'ai_auto_apply_enabled', name: 'AI Auto-Apply', category: 'AI' },
  { id: 'et0_steering_enabled', name: 'ET₀ Steering', category: 'Control' },
  { id: 'sms_critical_enabled', name: 'SMS Critical Alerts', category: 'Notifications' },
  { id: 'slack_mirror_mode', name: 'Slack Mirror Mode', category: 'Integrations' },
  { id: 'predictive_maintenance_auto_wo', name: 'PdM Auto Work Orders', category: 'AI' },
  { id: 'clickhouse_olap_enabled', name: 'ClickHouse OLAP', category: 'Infrastructure' },
];

export default function SiteFeatureManager() {
  const [isExpanded, setIsExpanded] = useState(false);
  const [editingSiteId, setEditingSiteId] = useState<string | null>(null);
  
  const { 
    user, 
    updateSiteSimulatorAccess,
    updateSiteSensorConfigAccess,
    updateSiteFeatureFlags 
  } = useAuthStore();
  
  const sites = user?.sitePermissions || [];

  const handleSimulatorToggle = (siteId: string, currentValue: boolean) => {
    updateSiteSimulatorAccess(siteId, !currentValue);
  };

  const handleSensorConfigToggle = (siteId: string, currentValue: boolean) => {
    updateSiteSensorConfigAccess(siteId, !currentValue);
  };

  const handleFeatureFlagToggle = (siteId: string, flagId: string, currentFlags: string[]) => {
    const newFlags = currentFlags.includes(flagId)
      ? currentFlags.filter(f => f !== flagId)
      : [...currentFlags, flagId];
    updateSiteFeatureFlags(siteId, newFlags);
  };

  return (
    <div className="rounded-2xl border border-violet-500/20 bg-gradient-to-br from-violet-500/5 to-purple-500/5 overflow-hidden">
      {/* Header */}
      <button
        onClick={() => setIsExpanded(!isExpanded)}
        className="w-full p-5 flex items-center justify-between hover:bg-violet-500/5 transition-colors"
      >
        <div className="flex items-center gap-4">
          <div className="p-3 rounded-xl bg-gradient-to-br from-violet-500/20 to-purple-500/20 ring-1 ring-violet-500/30">
            <Settings2 className="w-6 h-6 text-violet-400" />
          </div>
          <div className="text-left">
            <h2 className="text-lg font-semibold flex items-center gap-2">
              Site Feature Management
              <span className="text-xs bg-violet-500/10 text-violet-400 px-2 py-0.5 rounded-full ring-1 ring-violet-500/20">
                Super Admin
              </span>
            </h2>
            <p className="text-sm text-muted-foreground mt-0.5">
              Configure simulator access and feature flags per site
            </p>
          </div>
        </div>
        <div className="p-2 rounded-lg hover:bg-violet-500/10 transition-colors">
          {isExpanded ? (
            <ChevronDown className="w-5 h-5 text-violet-400" />
          ) : (
            <ChevronRight className="w-5 h-5 text-violet-400" />
          )}
        </div>
      </button>

      {isExpanded && (
        <div className="border-t border-violet-500/20 p-5">
          {sites.length > 0 ? (
            <div className="grid gap-4 md:grid-cols-2">
              {sites.map((site) => (
                <SiteCard
                  key={site.siteId}
                  site={site}
                  isEditing={editingSiteId === site.siteId}
                  onEdit={() => setEditingSiteId(editingSiteId === site.siteId ? null : site.siteId)}
                  onSimulatorToggle={() => handleSimulatorToggle(site.siteId, site.canAccessSimulator)}
                  onFeatureFlagToggle={(flagId) => 
                    handleFeatureFlagToggle(site.siteId, flagId, site.enabledFeatureFlags)
                  }
                />
              ))}
            </div>
          ) : (
            <div className="text-center py-12">
              <div className="w-16 h-16 rounded-2xl bg-violet-500/10 flex items-center justify-center mx-auto mb-4">
                <Building2 className="w-8 h-8 text-violet-400/50" />
              </div>
              <p className="text-muted-foreground">No sites configured</p>
              <p className="text-xs text-muted-foreground/70 mt-1">Add sites through the Spatial Model admin</p>
            </div>
          )}
        </div>
      )}
    </div>
  );
}

interface SiteCardProps {
  site: SitePermissions;
  isEditing: boolean;
  onEdit: () => void;
  onSimulatorToggle: () => void;
  onFeatureFlagToggle: (flagId: string) => void;
}

function SiteCard({ 
  site, 
  isEditing, 
  onEdit, 
  onSimulatorToggle, 
  onFeatureFlagToggle 
}: SiteCardProps) {
  return (
    <div className={`rounded-xl border transition-all ${
      isEditing 
        ? 'border-violet-500/30 bg-violet-500/5' 
        : 'border-border bg-card hover:border-violet-500/20'
    }`}>
      {/* Site Header */}
      <div className="p-4 border-b border-border">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className={`p-2.5 rounded-xl ${
              isEditing 
                ? 'bg-violet-500/20 ring-1 ring-violet-500/30' 
                : 'bg-muted/50'
            }`}>
              <Building2 className={`w-5 h-5 ${isEditing ? 'text-violet-400' : 'text-muted-foreground'}`} />
            </div>
            <div>
              <h3 className="font-semibold text-foreground">{site.siteName}</h3>
              <p className="text-xs text-muted-foreground font-mono">{site.siteId.slice(0, 12)}...</p>
            </div>
          </div>
          <button
            onClick={onEdit}
            className={`px-3 py-1.5 text-sm rounded-lg font-medium transition-all ${
              isEditing 
                ? 'bg-violet-500 text-white shadow-lg shadow-violet-500/20' 
                : 'bg-muted hover:bg-muted/80 text-muted-foreground'
            }`}
          >
            {isEditing ? 'Done' : 'Edit'}
          </button>
        </div>
      </div>

      <div className="p-4 space-y-4">
        {/* Simulator Access Toggle */}
        <div className="flex items-center justify-between p-3 rounded-xl bg-muted/30">
          <div className="flex items-center gap-2.5">
            <Play className="w-4 h-4 text-muted-foreground" />
            <span className="text-sm font-medium">Simulator Access</span>
          </div>
          <ToggleSwitch
            enabled={site.canAccessSimulator}
            disabled={!isEditing}
            onToggle={onSimulatorToggle}
          />
        </div>

        {/* Feature Flags */}
        <div>
          <div className="flex items-center gap-2 mb-3">
            <Flag className="w-4 h-4 text-muted-foreground" />
            <span className="text-sm font-medium">Feature Flags</span>
            <span className="text-xs text-muted-foreground bg-muted/50 px-2 py-0.5 rounded-full">
              {site.enabledFeatureFlags.length} enabled
            </span>
          </div>
          
          {isEditing ? (
            <div className="grid grid-cols-2 gap-2">
              {AVAILABLE_FEATURE_FLAGS.map((flag) => {
                const isEnabled = site.enabledFeatureFlags.includes(flag.id);
                return (
                  <button
                    key={flag.id}
                    onClick={() => onFeatureFlagToggle(flag.id)}
                    className={`flex items-center gap-2 p-2.5 rounded-lg text-left text-xs transition-all ${
                      isEnabled
                        ? 'bg-green-500/10 border border-green-500/30 text-green-400'
                        : 'bg-muted/30 border border-transparent hover:bg-muted/50 text-muted-foreground'
                    }`}
                  >
                    {isEnabled ? (
                      <Check className="w-3.5 h-3.5 flex-shrink-0" />
                    ) : (
                      <X className="w-3.5 h-3.5 flex-shrink-0" />
                    )}
                    <span className="truncate font-medium">{flag.name}</span>
                  </button>
                );
              })}
            </div>
          ) : (
            <div className="flex flex-wrap gap-1.5">
              {site.enabledFeatureFlags.length > 0 ? (
                site.enabledFeatureFlags.map((flagId) => {
                  const flag = AVAILABLE_FEATURE_FLAGS.find(f => f.id === flagId);
                  return (
                    <span 
                      key={flagId}
                      className="text-xs px-2.5 py-1 rounded-lg bg-green-500/10 text-green-400 ring-1 ring-green-500/20"
                    >
                      {flag?.name || flagId}
                    </span>
                  );
                })
              ) : (
                <span className="text-xs text-muted-foreground italic">No feature flags enabled</span>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

interface ToggleSwitchProps {
  enabled: boolean;
  disabled: boolean;
  onToggle: () => void;
}

function ToggleSwitch({ enabled, disabled, onToggle }: ToggleSwitchProps) {
  return (
    <button
      onClick={onToggle}
      disabled={disabled}
      className={`relative w-12 h-6 rounded-full transition-all ${
        enabled 
          ? 'bg-green-500 shadow-lg shadow-green-500/20' 
          : 'bg-muted-foreground/30'
      } ${disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer hover:opacity-90'}`}
    >
      <span 
        className={`absolute top-1 left-1 w-4 h-4 rounded-full bg-white shadow transition-transform ${
          enabled ? 'translate-x-6' : 'translate-x-0'
        }`}
      />
    </button>
  );
}
