'use client';

import React, { useState } from 'react';
import {
  Shield,
  ChevronLeft,
  AlertTriangle,
  XCircle,
  Clock,
  CheckCircle,
  Filter,
  ChevronDown,
  ArrowRight,
  Camera,
  FileText,
  User,
  Trash2,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import type { Hold, HoldReasonCode } from '@/features/inventory/types';

const REASON_CONFIG: Record<HoldReasonCode, { label: string; color: string; bgColor: string }> = {
  coa_failed: { label: 'COA Failed', color: 'text-rose-400', bgColor: 'bg-rose-500/10' },
  coa_pending: { label: 'COA Pending', color: 'text-amber-400', bgColor: 'bg-amber-500/10' },
  contamination: { label: 'Contamination', color: 'text-rose-400', bgColor: 'bg-rose-500/10' },
  quality_issue: { label: 'Quality Issue', color: 'text-amber-400', bgColor: 'bg-amber-500/10' },
  regulatory: { label: 'Regulatory', color: 'text-violet-400', bgColor: 'bg-violet-500/10' },
  customer_return: { label: 'Customer Return', color: 'text-cyan-400', bgColor: 'bg-cyan-500/10' },
  investigation: { label: 'Investigation', color: 'text-amber-400', bgColor: 'bg-amber-500/10' },
  other: { label: 'Other', color: 'text-muted-foreground', bgColor: 'bg-muted/50' },
};

// Mock data
const MOCK_HOLDS: Hold[] = [
  {
    id: 'hold-1',
    siteId: 'site-1',
    lotId: 'lot-1',
    lotNumber: 'LOT-2025-0001',
    reasonCode: 'coa_failed',
    reasonNotes: 'Pesticide levels exceeded threshold',
    isActive: true,
    requiresTwoPersonApproval: true,
    labOrderId: 'lab-1',
    labResultId: 'result-1',
    syncStatus: 'synced',
    createdAt: new Date(Date.now() - 86400000).toISOString(),
    createdBy: 'John Smith',
    updatedAt: new Date(Date.now() - 86400000).toISOString(),
  },
  {
    id: 'hold-2',
    siteId: 'site-1',
    lotId: 'lot-2',
    lotNumber: 'LOT-2025-0005',
    reasonCode: 'coa_pending',
    isActive: true,
    requiresTwoPersonApproval: false,
    labOrderId: 'lab-2',
    syncStatus: 'synced',
    createdAt: new Date(Date.now() - 172800000).toISOString(),
    createdBy: 'Jane Doe',
    updatedAt: new Date(Date.now() - 172800000).toISOString(),
  },
  {
    id: 'hold-3',
    siteId: 'site-1',
    lotId: 'lot-3',
    lotNumber: 'LOT-2025-0012',
    reasonCode: 'customer_return',
    reasonNotes: 'Customer reported quality issue',
    isActive: true,
    requiresTwoPersonApproval: false,
    syncStatus: 'pending',
    createdAt: new Date(Date.now() - 43200000).toISOString(),
    createdBy: 'John Smith',
    updatedAt: new Date(Date.now() - 43200000).toISOString(),
  },
];

type Tab = 'active' | 'released' | 'destruction';

export default function HoldsPage() {
  const [activeTab, setActiveTab] = useState<Tab>('active');
  const [selectedHold, setSelectedHold] = useState<Hold | null>(null);

  const activeHolds = MOCK_HOLDS.filter((h) => h.isActive);
  const criticalHolds = activeHolds.filter(
    (h) => h.reasonCode === 'coa_failed' || h.reasonCode === 'contamination'
  );

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  };

  const formatTime = (dateString: string) => {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
    
    if (diffHours < 1) return 'Less than an hour ago';
    if (diffHours < 24) return `${diffHours} hours ago`;
    if (diffHours < 48) return 'Yesterday';
    return formatDate(dateString);
  };

  const tabs = [
    { id: 'active', label: 'Active Holds', count: activeHolds.length },
    { id: 'released', label: 'Released', count: 0 },
    { id: 'destruction', label: 'Pending Destruction', count: 0 },
  ];

  return (
    <div className="min-h-screen bg-background">
      {/* Header */}
      <header className="sticky top-0 z-40 glass-header">
        <div className="px-6 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <a href="/inventory" className="p-2 rounded-lg hover:bg-white/5 text-muted-foreground hover:text-foreground transition-colors">
                <ChevronLeft className="w-5 h-5" />
              </a>
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-xl bg-rose-500/10 flex items-center justify-center">
                  <Shield className="w-5 h-5 text-rose-400" />
                </div>
                <div>
                  <h1 className="text-xl font-semibold text-foreground">Holds & Quarantine</h1>
                  <p className="text-sm text-muted-foreground">{activeHolds.length} active holds</p>
                </div>
              </div>
            </div>

            {/* Actions */}
            <div className="flex items-center gap-3">
              <button className="flex items-center gap-2 px-3 py-2 rounded-lg bg-white/5 text-foreground hover:bg-white/10 transition-colors">
                <Filter className="w-4 h-4" />
                <span className="text-sm">Filter</span>
                <ChevronDown className="w-3 h-3" />
              </button>
            </div>
          </div>
        </div>
      </header>

      {/* Critical Alert */}
      {criticalHolds.length > 0 && (
        <div className="mx-6 mt-4 p-4 rounded-xl bg-rose-500/10 border border-rose-500/20">
          <div className="flex items-center gap-3">
            <AlertTriangle className="w-5 h-5 text-rose-400" />
            <div>
              <p className="text-sm font-medium text-rose-400">
                {criticalHolds.length} critical hold{criticalHolds.length > 1 ? 's' : ''} require immediate attention
              </p>
              <p className="text-xs text-muted-foreground mt-0.5">
                COA failures and contamination issues must be resolved before lots can be released
              </p>
            </div>
          </div>
        </div>
      )}

      {/* Tabs */}
      <div className="px-6 mt-4 border-b border-border">
        <div className="flex gap-1">
          {tabs.map((tab) => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id as Tab)}
              className={cn(
                'flex items-center gap-2 px-4 py-3 text-sm font-medium border-b-2 transition-colors',
                activeTab === tab.id
                  ? 'text-rose-400 border-rose-400'
                  : 'text-muted-foreground border-transparent hover:text-foreground'
              )}
            >
              {tab.label}
              {tab.count > 0 && (
                <span className={cn(
                  'px-1.5 py-0.5 rounded text-xs',
                  activeTab === tab.id ? 'bg-rose-500/20' : 'bg-white/5'
                )}>
                  {tab.count}
                </span>
              )}
            </button>
          ))}
        </div>
      </div>

      {/* Content */}
      <div className="px-6 py-6">
        <div className="grid grid-cols-12 gap-6">
          {/* Holds List */}
          <div className="col-span-12 lg:col-span-7 space-y-4">
            {activeHolds.length === 0 ? (
              <div className="bg-surface border border-border rounded-xl p-12 text-center">
                <CheckCircle className="w-12 h-12 text-emerald-400 mx-auto mb-3" />
                <p className="text-sm text-foreground">No active holds</p>
                <p className="text-xs text-muted-foreground mt-1">All lots are cleared for operations</p>
              </div>
            ) : (
              activeHolds.map((hold) => {
                const config = REASON_CONFIG[hold.reasonCode];
                return (
                  <button
                    key={hold.id}
                    onClick={() => setSelectedHold(hold)}
                    className={cn(
                      'w-full text-left bg-surface border rounded-xl p-4 transition-all',
                      selectedHold?.id === hold.id
                        ? 'border-rose-500/30 ring-1 ring-rose-500/20'
                        : 'border-border hover:border-border'
                    )}
                  >
                    <div className="flex items-start justify-between mb-3">
                      <div className="flex items-center gap-3">
                        <div className={cn('w-10 h-10 rounded-lg flex items-center justify-center', config.bgColor)}>
                          {hold.reasonCode === 'coa_failed' || hold.reasonCode === 'contamination' ? (
                            <XCircle className={cn('w-5 h-5', config.color)} />
                          ) : hold.reasonCode === 'coa_pending' ? (
                            <Clock className={cn('w-5 h-5', config.color)} />
                          ) : (
                            <AlertTriangle className={cn('w-5 h-5', config.color)} />
                          )}
                        </div>
                        <div>
                          <div className="flex items-center gap-2">
                            <span className="text-sm font-mono text-foreground">{hold.lotNumber}</span>
                            {hold.requiresTwoPersonApproval && (
                              <span className="px-1.5 py-0.5 rounded text-[10px] font-medium bg-violet-500/10 text-violet-400">
                                2-PERSON
                              </span>
                            )}
                          </div>
                          <span className={cn('text-xs', config.color)}>{config.label}</span>
                        </div>
                      </div>
                      <span className="text-xs text-muted-foreground">{formatTime(hold.createdAt)}</span>
                    </div>

                    {hold.reasonNotes && (
                      <p className="text-sm text-muted-foreground mb-3 line-clamp-2">
                        {hold.reasonNotes}
                      </p>
                    )}

                    <div className="flex items-center justify-between pt-3 border-t border-border">
                      <div className="flex items-center gap-2 text-xs text-muted-foreground">
                        <User className="w-3 h-3" />
                        {hold.createdBy}
                      </div>
                      <ArrowRight className="w-4 h-4 text-muted-foreground" />
                    </div>
                  </button>
                );
              })
            )}
          </div>

          {/* Detail Panel */}
          <div className="col-span-12 lg:col-span-5">
            {selectedHold ? (
              <div className="bg-surface border border-border rounded-xl overflow-hidden sticky top-24">
                <div className={cn(
                  'px-5 py-4 border-b border-border',
                  REASON_CONFIG[selectedHold.reasonCode].bgColor
                )}>
                  <div className="flex items-center justify-between">
                    <h3 className="text-sm font-semibold text-foreground">Hold Details</h3>
                    <button
                      onClick={() => setSelectedHold(null)}
                      className="p-1 rounded hover:bg-white/10 text-muted-foreground"
                    >
                      <XCircle className="w-4 h-4" />
                    </button>
                  </div>
                </div>

                <div className="p-5 space-y-4">
                  {/* Lot Info */}
                  <div className="space-y-2">
                    <div className="flex justify-between">
                      <span className="text-xs text-muted-foreground">Lot Number</span>
                      <span className="text-sm font-mono text-foreground">{selectedHold.lotNumber}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-xs text-muted-foreground">Reason</span>
                      <span className={cn('text-sm', REASON_CONFIG[selectedHold.reasonCode].color)}>
                        {REASON_CONFIG[selectedHold.reasonCode].label}
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-xs text-muted-foreground">Created</span>
                      <span className="text-sm text-foreground">{formatDate(selectedHold.createdAt)}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-xs text-muted-foreground">By</span>
                      <span className="text-sm text-foreground">{selectedHold.createdBy}</span>
                    </div>
                  </div>

                  {selectedHold.reasonNotes && (
                    <div className="p-3 rounded-lg bg-muted/30 border border-border">
                      <span className="text-xs text-muted-foreground block mb-1">Notes</span>
                      <p className="text-sm text-foreground">{selectedHold.reasonNotes}</p>
                    </div>
                  )}

                  {/* Lab Results Link */}
                  {selectedHold.labResultId && (
                    <button className="w-full flex items-center gap-2 p-3 rounded-lg bg-muted/30 border border-border text-sm text-cyan-400 hover:bg-white/[0.04] transition-colors">
                      <FileText className="w-4 h-4" />
                      View Lab Results
                      <ArrowRight className="w-4 h-4 ml-auto" />
                    </button>
                  )}

                  {/* Actions */}
                  <div className="pt-4 border-t border-border space-y-3">
                    {selectedHold.requiresTwoPersonApproval ? (
                      <div className="p-3 rounded-lg bg-violet-500/5 border border-violet-500/20">
                        <div className="flex items-center gap-2 mb-2">
                          <User className="w-4 h-4 text-violet-400" />
                          <span className="text-xs font-medium text-violet-400">Two-Person Approval Required</span>
                        </div>
                        <p className="text-xs text-muted-foreground">
                          This hold requires approval from two authorized users before release
                        </p>
                      </div>
                    ) : null}

                    <button className="w-full flex items-center justify-center gap-2 py-2.5 rounded-lg bg-emerald-500/10 text-emerald-400 font-medium text-sm hover:bg-emerald-500/20 transition-colors">
                      <CheckCircle className="w-4 h-4" />
                      Release Hold
                    </button>

                    <button className="w-full flex items-center justify-center gap-2 py-2.5 rounded-lg bg-rose-500/10 text-rose-400 font-medium text-sm hover:bg-rose-500/20 transition-colors">
                      <Trash2 className="w-4 h-4" />
                      Schedule Destruction
                    </button>
                  </div>
                </div>
              </div>
            ) : (
              <div className="bg-surface border border-border rounded-xl p-8 text-center">
                <Shield className="w-12 h-12 text-muted-foreground mx-auto mb-3" />
                <p className="text-sm text-muted-foreground">Select a hold to view details</p>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
