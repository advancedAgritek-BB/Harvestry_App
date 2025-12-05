'use client';

import React, { useState } from 'react';
import {
  Flag,
  ToggleLeft,
  Info,
  AlertTriangle,
  CheckCircle,
  Settings,
  History,
} from 'lucide-react';
import {
  AdminCard,
  AdminSection,
  AdminGrid,
  StatusBadge,
  Button,
  AdminModal,
  FormField,
  Input,
  Select,
  Switch,
  Textarea,
} from '@/components/admin';

// Feature flags from the PRD
const FEATURE_FLAGS = [
  {
    id: 'closed_loop_ecph_enabled',
    name: 'Closed-Loop EC/pH',
    description: 'Enable closed-loop EC/pH control for precision fertigation',
    category: 'Control',
    enabled: false,
    rollout: 0,
    phase: 'Phase 2',
    prerequisites: ['Shadow mode ≥14 days', 'Median delta ≤5%', 'Interlocks validated'],
  },
  {
    id: 'autosteer_mpc_enabled',
    name: 'Autosteer MPC',
    description: 'Enable Model Predictive Control for climate and lighting autosteer',
    category: 'AI',
    enabled: false,
    rollout: 0,
    phase: 'Phase 3',
    prerequisites: ['Closed-loop validated', 'A/B success ≥85%', '30+ days telemetry'],
  },
  {
    id: 'ai_auto_apply_enabled',
    name: 'AI Auto-Apply',
    description: 'Enable AI auto-apply for anomaly detection and yield predictions',
    category: 'AI',
    enabled: false,
    rollout: 0,
    phase: 'Phase 2',
    prerequisites: ['Confidence threshold met', 'Human review period passed'],
  },
  {
    id: 'et0_steering_enabled',
    name: 'ET₀ Steering',
    description: 'Enable ET₀-based irrigation recommendations',
    category: 'Control',
    enabled: false,
    rollout: 0,
    phase: 'Phase 2',
    prerequisites: ['Crop coefficients configured', 'Weather data source active'],
  },
  {
    id: 'sms_critical_enabled',
    name: 'SMS Critical Alerts',
    description: 'Enable SMS notifications for critical alerts (per site policy)',
    category: 'Notifications',
    enabled: true,
    rollout: 100,
    phase: 'MVP',
    prerequisites: ['SMS provider configured', 'Phone numbers verified'],
  },
  {
    id: 'slack_mirror_mode',
    name: 'Slack Mirror Mode',
    description: 'Enable full Slack two-way mirroring (edits, deletes, thread sync)',
    category: 'Integrations',
    enabled: false,
    rollout: 0,
    phase: 'Phase 2',
    prerequisites: ['Slack workspace connected', 'Bot permissions granted'],
  },
  {
    id: 'predictive_maintenance_auto_wo',
    name: 'PdM Auto Work Orders',
    description: 'Automatically create work orders from predictive maintenance alerts',
    category: 'AI',
    enabled: false,
    rollout: 0,
    phase: 'Phase 3',
    prerequisites: ['PdM models trained', 'AUC ≥0.75 validated'],
  },
  {
    id: 'clickhouse_olap_enabled',
    name: 'ClickHouse OLAP',
    description: 'Enable ClickHouse sidecar for OLAP queries when triggers trip',
    category: 'Infrastructure',
    enabled: false,
    rollout: 0,
    phase: 'Phase 2/3',
    prerequisites: ['ClickHouse provisioned', 'Triggers configured'],
  },
];

const getCategoryColor = (category: string) => {
  const colors: Record<string, string> = {
    Control: 'bg-cyan-500/10 text-cyan-400',
    AI: 'bg-violet-500/10 text-violet-400',
    Notifications: 'bg-amber-500/10 text-amber-400',
    Integrations: 'bg-emerald-500/10 text-emerald-400',
    Infrastructure: 'bg-rose-500/10 text-rose-400',
  };
  return colors[category] || 'bg-white/10 text-muted-foreground';
};

export default function FeatureFlagsAdminPage() {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedFlag, setSelectedFlag] = useState<typeof FEATURE_FLAGS[0] | null>(null);

  const handleFlagClick = (flag: typeof FEATURE_FLAGS[0]) => {
    setSelectedFlag(flag);
    setIsModalOpen(true);
  };

  const enabledCount = FEATURE_FLAGS.filter(f => f.enabled).length;
  const phaseBreakdown = FEATURE_FLAGS.reduce((acc, f) => {
    acc[f.phase] = (acc[f.phase] || 0) + 1;
    return acc;
  }, {} as Record<string, number>);

  return (
    <div className="space-y-6">
      <AdminSection
        title="Feature Flags"
        description="Manage site-level feature flags for phased rollouts"
      >
        {/* Stats */}
        <AdminGrid columns={4}>
          <div className="p-4 bg-surface border border-border rounded-xl text-center">
            <div className="text-2xl font-bold text-foreground">{FEATURE_FLAGS.length}</div>
            <div className="text-xs text-muted-foreground">Total Flags</div>
          </div>
          <div className="p-4 bg-surface border border-border rounded-xl text-center">
            <div className="text-2xl font-bold text-emerald-400">{enabledCount}</div>
            <div className="text-xs text-muted-foreground">Enabled</div>
          </div>
          <div className="p-4 bg-surface border border-border rounded-xl text-center">
            <div className="text-2xl font-bold text-amber-400">{FEATURE_FLAGS.length - enabledCount}</div>
            <div className="text-xs text-muted-foreground">Disabled</div>
          </div>
          <div className="p-4 bg-surface border border-border rounded-xl text-center">
            <div className="text-2xl font-bold text-cyan-400">{phaseBreakdown['MVP'] || 0}</div>
            <div className="text-xs text-muted-foreground">MVP Ready</div>
          </div>
        </AdminGrid>

        {/* Flag Grid */}
        <AdminGrid columns={2}>
          {FEATURE_FLAGS.map((flag) => (
            <AdminCard
              key={flag.id}
              title={flag.name}
              icon={Flag}
              className="cursor-pointer hover:border-violet-500/30 transition-colors"
              actions={
                <Switch
                  checked={flag.enabled}
                  onChange={() => {}}
                  disabled={!flag.enabled && flag.prerequisites.length > 0}
                />
              }
            >
              <div 
                className="space-y-4"
                onClick={() => handleFlagClick(flag)}
              >
                <p className="text-sm text-muted-foreground">{flag.description}</p>

                <div className="flex items-center gap-2">
                  <span className={`text-xs px-2 py-0.5 rounded ${getCategoryColor(flag.category)}`}>
                    {flag.category}
                  </span>
                  <span className="text-xs bg-white/5 px-2 py-0.5 rounded">{flag.phase}</span>
                </div>

                {flag.enabled ? (
                  <div className="flex items-center gap-2 text-xs">
                    <CheckCircle className="w-4 h-4 text-emerald-400" />
                    <span className="text-emerald-400">
                      {flag.rollout === 100 ? 'Fully enabled' : `${flag.rollout}% rollout`}
                    </span>
                  </div>
                ) : (
                  <div className="space-y-1">
                    <div className="flex items-center gap-2 text-xs text-muted-foreground">
                      <AlertTriangle className="w-4 h-4" />
                      <span>Prerequisites:</span>
                    </div>
                    <ul className="text-xs text-muted-foreground pl-6 space-y-0.5">
                      {flag.prerequisites.slice(0, 2).map((prereq, idx) => (
                        <li key={idx}>• {prereq}</li>
                      ))}
                      {flag.prerequisites.length > 2 && (
                        <li>• +{flag.prerequisites.length - 2} more</li>
                      )}
                    </ul>
                  </div>
                )}
              </div>
            </AdminCard>
          ))}
        </AdminGrid>
      </AdminSection>

      {/* Flag Detail Modal */}
      <AdminModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={selectedFlag?.name || 'Feature Flag'}
        description={selectedFlag?.description}
        size="lg"
        footer={
          <>
            <Button variant="ghost" onClick={() => setIsModalOpen(false)}>
              Cancel
            </Button>
            <Button onClick={() => setIsModalOpen(false)}>
              Save Changes
            </Button>
          </>
        }
      >
        {selectedFlag && (
          <div className="space-y-6">
            <div className="flex items-center gap-2">
              <span className={`text-xs px-2 py-0.5 rounded ${getCategoryColor(selectedFlag.category)}`}>
                {selectedFlag.category}
              </span>
              <span className="text-xs bg-white/5 px-2 py-0.5 rounded">{selectedFlag.phase}</span>
            </div>

            <div className="flex items-center justify-between p-4 bg-white/5 rounded-lg">
              <div>
                <div className="text-sm font-medium text-foreground">Enable Flag</div>
                <div className="text-xs text-muted-foreground">
                  {selectedFlag.enabled ? 'Flag is currently enabled' : 'Flag is currently disabled'}
                </div>
              </div>
              <Switch checked={selectedFlag.enabled} onChange={() => {}} />
            </div>

            <FormField label="Rollout Percentage" description="Percentage of users/sites to enable">
              <Input
                type="number"
                min={0}
                max={100}
                defaultValue={selectedFlag.rollout}
                disabled={!selectedFlag.enabled}
              />
            </FormField>

            <div className="p-4 bg-white/5 rounded-lg">
              <h4 className="text-sm font-medium text-foreground mb-3">Prerequisites</h4>
              <ul className="space-y-2">
                {selectedFlag.prerequisites.map((prereq, idx) => (
                  <li key={idx} className="flex items-center gap-2 text-sm">
                    <CheckCircle className="w-4 h-4 text-muted-foreground" />
                    <span className="text-muted-foreground">{prereq}</span>
                  </li>
                ))}
              </ul>
            </div>

            <div className="p-3 bg-amber-500/10 border border-amber-500/20 rounded-lg">
              <div className="flex items-start gap-2">
                <AlertTriangle className="w-4 h-4 text-amber-400 mt-0.5 flex-shrink-0" />
                <div className="text-xs text-amber-200">
                  <strong>Caution:</strong> Enabling this flag may affect production systems. 
                  Ensure all prerequisites are met before enabling. All changes are audited.
                </div>
              </div>
            </div>
          </div>
        )}
      </AdminModal>
    </div>
  );
}

