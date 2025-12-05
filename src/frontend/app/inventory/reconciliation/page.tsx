'use client';

import React, { useState } from 'react';
import {
  Scale,
  ChevronLeft,
  RefreshCw,
  CheckCircle,
  AlertTriangle,
  ArrowUp,
  ArrowDown,
  Download,
  Calendar,
  Filter,
  ChevronDown,
  Play,
  FileText,
  Clock,
} from 'lucide-react';
import { cn } from '@/lib/utils';

// Mock discrepancies
const MOCK_DISCREPANCIES = [
  {
    id: 'disc-1',
    lotNumber: 'LOT-2025-0001',
    locationPath: 'Vault A > Rack 1 > Shelf A',
    expectedQuantity: 500,
    actualQuantity: 485,
    variance: -15,
    variancePercent: -3.0,
    uom: 'g',
    status: 'open',
    detectedAt: new Date(Date.now() - 86400000).toISOString(),
  },
  {
    id: 'disc-2',
    lotNumber: 'LOT-2025-0012',
    locationPath: 'Warehouse B > Zone 1',
    expectedQuantity: 1000,
    actualQuantity: 1025,
    variance: 25,
    variancePercent: 2.5,
    uom: 'g',
    status: 'investigating',
    detectedAt: new Date(Date.now() - 172800000).toISOString(),
  },
];

// Mock cycle counts
const MOCK_CYCLE_COUNTS = [
  { id: 'cc-1', locationPath: 'Vault A', scheduledAt: new Date(Date.now() + 86400000 * 2).toISOString(), status: 'scheduled', lotCount: 42 },
  { id: 'cc-2', locationPath: 'Warehouse B', scheduledAt: new Date(Date.now() + 86400000 * 7).toISOString(), status: 'scheduled', lotCount: 28 },
  { id: 'cc-3', locationPath: 'Vault A > Rack 1', scheduledAt: new Date(Date.now() - 86400000 * 3).toISOString(), status: 'completed', lotCount: 15, varianceFound: 1 },
];

type Tab = 'discrepancies' | 'cycle-counts' | 'history';

export default function ReconciliationPage() {
  const [activeTab, setActiveTab] = useState<Tab>('discrepancies');
  const [reconciling, setReconciling] = useState(false);

  const handleReconcile = async () => {
    setReconciling(true);
    await new Promise((resolve) => setTimeout(resolve, 3000));
    setReconciling(false);
  };

  const openDiscrepancies = MOCK_DISCREPANCIES.filter((d) => d.status === 'open');
  const overallVariance = MOCK_DISCREPANCIES.reduce((sum, d) => sum + d.variance, 0);

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  };

  const tabs = [
    { id: 'discrepancies', label: 'Discrepancies', count: openDiscrepancies.length },
    { id: 'cycle-counts', label: 'Cycle Counts', count: MOCK_CYCLE_COUNTS.filter(c => c.status === 'scheduled').length },
    { id: 'history', label: 'History' },
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
                <div className="w-10 h-10 rounded-xl bg-emerald-500/10 flex items-center justify-center">
                  <Scale className="w-5 h-5 text-emerald-400" />
                </div>
                <div>
                  <h1 className="text-xl font-semibold text-foreground">Reconciliation</h1>
                  <p className="text-sm text-muted-foreground">Balance verification and cycle counts</p>
                </div>
              </div>
            </div>

            <div className="flex items-center gap-3">
              <button className="flex items-center gap-2 px-3 py-2 rounded-lg bg-white/5 text-foreground hover:bg-white/10 transition-colors">
                <Download className="w-4 h-4" />
                <span className="text-sm">Export Report</span>
              </button>
              <button
                onClick={handleReconcile}
                disabled={reconciling}
                className="flex items-center gap-2 px-4 py-2 rounded-lg bg-emerald-500/10 text-emerald-400 hover:bg-emerald-500/20 disabled:opacity-50 transition-colors"
              >
                <RefreshCw className={cn('w-4 h-4', reconciling && 'animate-spin')} />
                <span className="text-sm font-medium">{reconciling ? 'Reconciling...' : 'Run Reconciliation'}</span>
              </button>
            </div>
          </div>
        </div>
      </header>

      {/* Summary Cards */}
      <div className="px-6 py-4">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          {/* Overall Status */}
          <div className={cn(
            'p-4 rounded-xl border',
            openDiscrepancies.length === 0
              ? 'bg-emerald-500/5 border-emerald-500/20'
              : 'bg-amber-500/5 border-amber-500/20'
          )}>
            <div className="flex items-center gap-3 mb-2">
              {openDiscrepancies.length === 0 ? (
                <CheckCircle className="w-5 h-5 text-emerald-400" />
              ) : (
                <AlertTriangle className="w-5 h-5 text-amber-400" />
              )}
              <span className={cn(
                'text-sm font-medium',
                openDiscrepancies.length === 0 ? 'text-emerald-400' : 'text-amber-400'
              )}>
                {openDiscrepancies.length === 0 ? 'All Balanced' : 'Discrepancies Found'}
              </span>
            </div>
            <div className="text-2xl font-bold text-foreground tabular-nums">
              {openDiscrepancies.length}
            </div>
            <div className="text-xs text-muted-foreground">Open issues</div>
          </div>

          {/* Overall Variance */}
          <div className="bg-surface border border-border rounded-xl p-4">
            <div className="flex items-center gap-2 mb-2">
              {overallVariance > 0 ? (
                <ArrowUp className="w-4 h-4 text-emerald-400" />
              ) : overallVariance < 0 ? (
                <ArrowDown className="w-4 h-4 text-rose-400" />
              ) : null}
              <span className="text-xs text-muted-foreground">Net Variance</span>
            </div>
            <div className={cn(
              'text-2xl font-bold tabular-nums',
              overallVariance > 0 ? 'text-emerald-400' : overallVariance < 0 ? 'text-rose-400' : 'text-foreground'
            )}>
              {overallVariance > 0 ? '+' : ''}{overallVariance} g
            </div>
            <div className="text-xs text-muted-foreground">Across all locations</div>
          </div>

          {/* Last Reconciliation */}
          <div className="bg-surface border border-border rounded-xl p-4">
            <div className="flex items-center gap-2 mb-2">
              <Clock className="w-4 h-4 text-muted-foreground" />
              <span className="text-xs text-muted-foreground">Last Reconciliation</span>
            </div>
            <div className="text-lg font-medium text-foreground">2 hours ago</div>
            <div className="text-xs text-muted-foreground">By John Smith</div>
          </div>

          {/* Next Cycle Count */}
          <div className="bg-surface border border-border rounded-xl p-4">
            <div className="flex items-center gap-2 mb-2">
              <Calendar className="w-4 h-4 text-muted-foreground" />
              <span className="text-xs text-muted-foreground">Next Cycle Count</span>
            </div>
            <div className="text-lg font-medium text-foreground">In 2 days</div>
            <div className="text-xs text-muted-foreground">Vault A</div>
          </div>
        </div>
      </div>

      {/* Tabs */}
      <div className="px-6 border-b border-border">
        <div className="flex gap-1">
          {tabs.map((tab) => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id as Tab)}
              className={cn(
                'flex items-center gap-2 px-4 py-3 text-sm font-medium border-b-2 transition-colors',
                activeTab === tab.id
                  ? 'text-emerald-400 border-emerald-400'
                  : 'text-muted-foreground border-transparent hover:text-foreground'
              )}
            >
              {tab.label}
              {tab.count !== undefined && tab.count > 0 && (
                <span className={cn(
                  'px-1.5 py-0.5 rounded text-xs',
                  activeTab === tab.id ? 'bg-emerald-500/20' : 'bg-white/5'
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
        {activeTab === 'discrepancies' && (
          <div className="space-y-4">
            {/* Filters */}
            <div className="flex items-center gap-4">
              <button className="flex items-center gap-2 px-3 py-2 rounded-lg bg-muted/30 border border-border text-sm text-muted-foreground hover:text-foreground">
                <Filter className="w-4 h-4" />
                Status
                <ChevronDown className="w-3 h-3" />
              </button>
              <button className="flex items-center gap-2 px-3 py-2 rounded-lg bg-muted/30 border border-border text-sm text-muted-foreground hover:text-foreground">
                <Calendar className="w-4 h-4" />
                Date Range
                <ChevronDown className="w-3 h-3" />
              </button>
            </div>

            {/* Discrepancies List */}
            {MOCK_DISCREPANCIES.length === 0 ? (
              <div className="bg-surface border border-border rounded-xl p-12 text-center">
                <CheckCircle className="w-12 h-12 text-emerald-400 mx-auto mb-3" />
                <p className="text-sm text-foreground">No discrepancies found</p>
                <p className="text-xs text-muted-foreground mt-1">All balances are reconciled</p>
              </div>
            ) : (
              <div className="bg-surface border border-border rounded-xl overflow-hidden">
                <table className="w-full">
                  <thead>
                    <tr className="border-b border-border bg-muted/30">
                      <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">Lot</th>
                      <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">Location</th>
                      <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">Expected</th>
                      <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">Actual</th>
                      <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">Variance</th>
                      <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">Status</th>
                      <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">Detected</th>
                    </tr>
                  </thead>
                  <tbody>
                    {MOCK_DISCREPANCIES.map((disc) => (
                      <tr key={disc.id} className="border-b border-border hover:bg-muted/30">
                        <td className="px-4 py-3">
                          <span className="text-sm font-mono text-foreground">{disc.lotNumber}</span>
                        </td>
                        <td className="px-4 py-3">
                          <span className="text-sm text-muted-foreground">{disc.locationPath}</span>
                        </td>
                        <td className="px-4 py-3">
                          <span className="text-sm text-foreground tabular-nums">{disc.expectedQuantity} {disc.uom}</span>
                        </td>
                        <td className="px-4 py-3">
                          <span className="text-sm text-foreground tabular-nums">{disc.actualQuantity} {disc.uom}</span>
                        </td>
                        <td className="px-4 py-3">
                          <span className={cn(
                            'text-sm font-medium tabular-nums',
                            disc.variance > 0 ? 'text-emerald-400' : 'text-rose-400'
                          )}>
                            {disc.variance > 0 ? '+' : ''}{disc.variance} {disc.uom}
                            <span className="text-xs ml-1 opacity-60">({disc.variancePercent}%)</span>
                          </span>
                        </td>
                        <td className="px-4 py-3">
                          <span className={cn(
                            'px-2 py-0.5 rounded-full text-xs font-medium',
                            disc.status === 'open' && 'bg-amber-500/10 text-amber-400',
                            disc.status === 'investigating' && 'bg-blue-500/10 text-blue-400',
                            disc.status === 'resolved' && 'bg-emerald-500/10 text-emerald-400'
                          )}>
                            {disc.status}
                          </span>
                        </td>
                        <td className="px-4 py-3">
                          <span className="text-sm text-muted-foreground">{formatDate(disc.detectedAt)}</span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        )}

        {activeTab === 'cycle-counts' && (
          <div className="space-y-4">
            <div className="flex justify-end">
              <button className="flex items-center gap-2 px-4 py-2 rounded-lg bg-emerald-500/10 text-emerald-400 hover:bg-emerald-500/20 transition-colors">
                <Calendar className="w-4 h-4" />
                <span className="text-sm font-medium">Schedule Count</span>
              </button>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {MOCK_CYCLE_COUNTS.map((count) => (
                <div
                  key={count.id}
                  className={cn(
                    'bg-surface border rounded-xl p-4',
                    count.status === 'scheduled' ? 'border-border' : 'border-emerald-500/20'
                  )}
                >
                  <div className="flex items-center justify-between mb-3">
                    <span className={cn(
                      'px-2 py-0.5 rounded-full text-xs font-medium',
                      count.status === 'scheduled' && 'bg-blue-500/10 text-blue-400',
                      count.status === 'completed' && 'bg-emerald-500/10 text-emerald-400'
                    )}>
                      {count.status}
                    </span>
                    {count.status === 'completed' && count.varianceFound !== undefined && (
                      <span className={cn(
                        'text-xs',
                        count.varianceFound > 0 ? 'text-amber-400' : 'text-emerald-400'
                      )}>
                        {count.varianceFound} variance{count.varianceFound !== 1 ? 's' : ''}
                      </span>
                    )}
                  </div>

                  <h3 className="text-sm font-medium text-foreground mb-1">{count.locationPath}</h3>
                  <p className="text-xs text-muted-foreground mb-3">
                    {count.lotCount} lots to count
                  </p>

                  <div className="flex items-center justify-between pt-3 border-t border-border">
                    <span className="text-xs text-muted-foreground">
                      {count.status === 'scheduled' ? 'Scheduled:' : 'Completed:'} {formatDate(count.scheduledAt)}
                    </span>
                    {count.status === 'scheduled' && (
                      <button className="flex items-center gap-1 text-xs text-cyan-400 hover:underline">
                        <Play className="w-3 h-3" />
                        Start
                      </button>
                    )}
                    {count.status === 'completed' && (
                      <button className="flex items-center gap-1 text-xs text-cyan-400 hover:underline">
                        <FileText className="w-3 h-3" />
                        Report
                      </button>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {activeTab === 'history' && (
          <div className="bg-surface border border-border rounded-xl p-8 text-center">
            <Scale className="w-12 h-12 text-muted-foreground mx-auto mb-3" />
            <p className="text-sm text-muted-foreground">Reconciliation history will be displayed here</p>
          </div>
        )}
      </div>
    </div>
  );
}

