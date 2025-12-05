'use client';

import React, { useState } from 'react';
import {
  Leaf,
  Edit2,
  Save,
  Users,
  Calendar,
  AlertCircle,
} from 'lucide-react';
import {
  AdminCard,
  AdminSection,
  AdminGrid,
  FormField,
  Input,
  Select,
  Switch,
  Button,
} from '@/components/admin';

// Mock data for propagation settings
const MOCK_SETTINGS = {
  dailyLimit: 500,
  weeklyLimit: 2000,
  motherPropagationLimit: 50,
  requiresOverrideApproval: true,
  approverRole: 'Cultivation Manager',
};

const ROLES = [
  { value: 'manager', label: 'Cultivation Manager' },
  { value: 'lead', label: 'Cultivation Lead' },
  { value: 'supervisor', label: 'Supervisor' },
  { value: 'admin', label: 'Admin' },
];

export function PropagationSettingsSection() {
  const [isEditing, setIsEditing] = useState(false);
  const [settings, setSettings] = useState(MOCK_SETTINGS);

  const handleSave = () => {
    // In a real app, this would save to the API
    setIsEditing(false);
  };

  return (
    <AdminSection
      title="Propagation Settings"
      description="Configure site-wide propagation limits and override approval workflows"
    >
      <AdminGrid columns={2}>
        <AdminCard
          title="Propagation Limits"
          icon={Leaf}
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
              label="Daily Limit" 
              description="Maximum clones per day across all mothers (leave empty for unlimited)"
            >
              <Input 
                type="number" 
                placeholder="Unlimited" 
                value={settings.dailyLimit || ''} 
                onChange={(e) => setSettings({ ...settings, dailyLimit: parseInt(e.target.value) || 0 })}
                disabled={!isEditing}
              />
            </FormField>

            <FormField 
              label="Weekly Limit" 
              description="Maximum clones per week across all mothers (leave empty for unlimited)"
            >
              <Input 
                type="number" 
                placeholder="Unlimited" 
                value={settings.weeklyLimit || ''} 
                onChange={(e) => setSettings({ ...settings, weeklyLimit: parseInt(e.target.value) || 0 })}
                disabled={!isEditing}
              />
            </FormField>

            <FormField 
              label="Per-Mother Limit" 
              description="Maximum clones per mother plant (leave empty for unlimited)"
            >
              <Input 
                type="number" 
                placeholder="Unlimited" 
                value={settings.motherPropagationLimit || ''} 
                onChange={(e) => setSettings({ ...settings, motherPropagationLimit: parseInt(e.target.value) || 0 })}
                disabled={!isEditing}
              />
            </FormField>
          </div>
        </AdminCard>

        <AdminCard
          title="Override Approval"
          icon={Users}
        >
          <div className="space-y-6">
            <div className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
              <div>
                <div className="text-sm font-medium text-foreground">Require Approval for Overrides</div>
                <div className="text-xs text-muted-foreground">
                  When limits are exceeded, approval is required to proceed
                </div>
              </div>
              <Switch 
                checked={settings.requiresOverrideApproval} 
                onChange={(checked) => setSettings({ ...settings, requiresOverrideApproval: checked })}
                disabled={!isEditing}
              />
            </div>

            <FormField 
              label="Approver Role" 
              description="Role required to approve override requests"
            >
              <Select 
                options={ROLES} 
                value={settings.approverRole === 'Cultivation Manager' ? 'manager' : 'lead'}
                disabled={!isEditing || !settings.requiresOverrideApproval}
              />
            </FormField>

            <div className="p-3 bg-amber-500/10 border border-amber-500/20 rounded-lg">
              <div className="flex items-start gap-2">
                <AlertCircle className="w-4 h-4 text-amber-400 mt-0.5 flex-shrink-0" />
                <div className="text-xs text-amber-200">
                  <strong>Audit Trail:</strong> All override requests and approvals are 
                  logged for compliance. Repeated limit breaches will trigger alerts 
                  to management.
                </div>
              </div>
            </div>
          </div>
        </AdminCard>
      </AdminGrid>

      {/* Current Usage Stats */}
      <AdminCard
        title="Current Period Usage"
        icon={Calendar}
        className="mt-6"
      >
        <div className="grid grid-cols-4 gap-6">
          <div className="p-4 bg-white/5 rounded-lg text-center">
            <div className="text-2xl font-bold text-foreground mb-1">347</div>
            <div className="text-xs text-muted-foreground">Today's Clones</div>
            <div className="text-xs text-emerald-400 mt-1">
              {settings.dailyLimit ? `${((347 / settings.dailyLimit) * 100).toFixed(0)}% of limit` : 'No limit'}
            </div>
          </div>
          <div className="p-4 bg-white/5 rounded-lg text-center">
            <div className="text-2xl font-bold text-foreground mb-1">1,284</div>
            <div className="text-xs text-muted-foreground">This Week</div>
            <div className="text-xs text-emerald-400 mt-1">
              {settings.weeklyLimit ? `${((1284 / settings.weeklyLimit) * 100).toFixed(0)}% of limit` : 'No limit'}
            </div>
          </div>
          <div className="p-4 bg-white/5 rounded-lg text-center">
            <div className="text-2xl font-bold text-foreground mb-1">23</div>
            <div className="text-xs text-muted-foreground">Active Mothers</div>
            <div className="text-xs text-cyan-400 mt-1">Avg 56 clones/mother</div>
          </div>
          <div className="p-4 bg-white/5 rounded-lg text-center">
            <div className="text-2xl font-bold text-foreground mb-1">2</div>
            <div className="text-xs text-muted-foreground">Pending Overrides</div>
            <div className="text-xs text-amber-400 mt-1">Awaiting approval</div>
          </div>
        </div>
      </AdminCard>
    </AdminSection>
  );
}

