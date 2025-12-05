'use client';

import React, { useState } from 'react';
import {
  Shield,
  ChevronLeft,
  RefreshCw,
  CheckCircle,
  AlertCircle,
  XCircle,
  Clock,
  Play,
  RotateCcw,
  Download,
  Filter,
  ChevronDown,
  ExternalLink,
  Trash2,
} from 'lucide-react';
import { cn } from '@/lib/utils';

// Note: A user will only ever have either METRC or BIOTRACK, never both
const MOCK_INTEGRATIONS = [
  {
    id: 'int-1',
    siteId: 'site-1',
    siteName: 'Evergreen Facility',
    provider: 'metrc',
    isConnected: true,
    lastSyncAt: new Date(Date.now() - 300000).toISOString(),
    pendingCount: 3,
    errorCount: 0,
    successRate: 99.8,
    syncMode: 'realtime',
  },
];

const MOCK_SYNC_EVENTS = Array.from({ length: 20 }, (_, i) => ({
  id: `event-${i + 1}`,
  provider: 'metrc',
  eventType: ['lot_create', 'movement', 'adjustment', 'package'][i % 4],
  entityId: `entity-${i + 1}`,
  entityName: `LOT-2025-${String(i + 1).padStart(4, '0')}`,
  status: ['success', 'success', 'success', 'pending', 'failed'][i % 5],
  retryCount: i % 5 === 4 ? 2 : 0,
  createdAt: new Date(Date.now() - i * 600000).toISOString(),
  processedAt: i % 5 !== 3 ? new Date(Date.now() - i * 600000 + 5000).toISOString() : undefined,
  errorMessage: i % 5 === 4 ? 'Rate limit exceeded' : undefined,
}));

const MOCK_DLQ_ITEMS = [
  {
    id: 'dlq-1',
    provider: 'metrc',
    eventType: 'movement',
    entityId: 'mov-123',
    errorMessage: 'API rate limit exceeded (429)',
    failedAt: new Date(Date.now() - 3600000).toISOString(),
    retryCount: 3,
  },
];

type Tab = 'overview' | 'events' | 'dlq';

export default function ComplianceDashboardPage() {
  const [activeTab, setActiveTab] = useState<Tab>('overview');
  const [syncing, setSyncing] = useState<string | null>(null);

  const handleSync = async (provider: string) => {
    setSyncing(provider);
    // Simulate sync
    await new Promise((resolve) => setTimeout(resolve, 2000));
    setSyncing(null);
  };

  const formatTime = (dateString: string) => {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    
    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    if (diffMins < 1440) return `${Math.floor(diffMins / 60)}h ago`;
    return date.toLocaleDateString();
  };

  const tabs = [
    { id: 'overview', label: 'Overview' },
    { id: 'events', label: 'Sync Events', count: MOCK_SYNC_EVENTS.filter(e => e.status === 'pending').length },
    { id: 'dlq', label: 'Dead Letter Queue', count: MOCK_DLQ_ITEMS.length },
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
                <div className="w-10 h-10 rounded-xl bg-violet-500/10 flex items-center justify-center">
                  <Shield className="w-5 h-5 text-violet-400" />
                </div>
                <div>
                  <h1 className="text-xl font-semibold text-foreground">Compliance Sync</h1>
                  <p className="text-sm text-muted-foreground">METRC & BioTrack Integration Status</p>
                </div>
              </div>
            </div>

            {/* Actions */}
            <div className="flex items-center gap-3">
              <button className="flex items-center gap-2 px-3 py-2 rounded-lg bg-white/5 text-foreground hover:bg-white/10 transition-colors">
                <Download className="w-4 h-4" />
                <span className="text-sm">Export Audit</span>
              </button>
              <button className="flex items-center gap-2 px-4 py-2 rounded-lg bg-violet-500/10 text-violet-400 hover:bg-violet-500/20 transition-colors">
                <RefreshCw className="w-4 h-4" />
                <span className="text-sm font-medium">Sync All</span>
              </button>
            </div>
          </div>
        </div>
      </header>

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
                  ? 'text-violet-400 border-violet-400'
                  : 'text-muted-foreground border-transparent hover:text-foreground'
              )}
            >
              {tab.label}
              {tab.count !== undefined && tab.count > 0 && (
                <span className={cn(
                  'px-1.5 py-0.5 rounded text-xs',
                  activeTab === tab.id ? 'bg-violet-500/20' : 'bg-white/5'
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
        {activeTab === 'overview' && (
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {MOCK_INTEGRATIONS.map((integration) => (
              <div
                key={integration.id}
                className="bg-surface border border-border rounded-xl overflow-hidden"
              >
                {/* Header */}
                <div className={cn(
                  'px-5 py-4 border-b border-border',
                  integration.provider === 'metrc' ? 'bg-emerald-500/5' : 'bg-blue-500/5'
                )}>
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3">
                      <div className={cn(
                        'w-10 h-10 rounded-xl flex items-center justify-center',
                        integration.provider === 'metrc' ? 'bg-emerald-500/10' : 'bg-blue-500/10'
                      )}>
                        <span className={cn(
                          'text-sm font-bold uppercase',
                          integration.provider === 'metrc' ? 'text-emerald-400' : 'text-blue-400'
                        )}>
                          {integration.provider === 'metrc' ? 'M' : 'BT'}
                        </span>
                      </div>
                      <div>
                        <h3 className={cn(
                          'text-lg font-semibold',
                          integration.provider === 'metrc' ? 'text-emerald-400' : 'text-blue-400'
                        )}>
                          {integration.provider.toUpperCase()}
                        </h3>
                        <p className="text-sm text-muted-foreground">{integration.siteName}</p>
                      </div>
                    </div>
                    <div className="flex items-center gap-2">
                      {integration.isConnected ? (
                        <span className="flex items-center gap-1.5 px-2 py-1 rounded-full text-xs bg-emerald-500/10 text-emerald-400">
                          <CheckCircle className="w-3 h-3" />
                          Connected
                        </span>
                      ) : (
                        <span className="flex items-center gap-1.5 px-2 py-1 rounded-full text-xs bg-rose-500/10 text-rose-400">
                          <XCircle className="w-3 h-3" />
                          Disconnected
                        </span>
                      )}
                    </div>
                  </div>
                </div>

                {/* Stats */}
                <div className="p-5">
                  <div className="grid grid-cols-4 gap-4 mb-4">
                    <div className="text-center">
                      <div className="text-2xl font-bold text-foreground tabular-nums">
                        {integration.pendingCount}
                      </div>
                      <div className="text-xs text-muted-foreground">Pending</div>
                    </div>
                    <div className="text-center">
                      <div className={cn(
                        'text-2xl font-bold tabular-nums',
                        integration.errorCount > 0 ? 'text-rose-400' : 'text-foreground'
                      )}>
                        {integration.errorCount}
                      </div>
                      <div className="text-xs text-muted-foreground">Errors</div>
                    </div>
                    <div className="text-center">
                      <div className={cn(
                        'text-2xl font-bold tabular-nums',
                        integration.successRate >= 99 ? 'text-emerald-400' : 
                        integration.successRate >= 95 ? 'text-amber-400' : 'text-rose-400'
                      )}>
                        {integration.successRate}%
                      </div>
                      <div className="text-xs text-muted-foreground">Success Rate</div>
                    </div>
                    <div className="text-center">
                      <div className="text-sm font-medium text-foreground capitalize">
                        {integration.syncMode}
                      </div>
                      <div className="text-xs text-muted-foreground">Mode</div>
                    </div>
                  </div>

                  <div className="flex items-center justify-between pt-4 border-t border-border">
                    <span className="text-xs text-muted-foreground">
                      Last sync: {formatTime(integration.lastSyncAt)}
                    </span>
                    <button
                      onClick={() => handleSync(integration.provider)}
                      disabled={syncing === integration.provider}
                      className={cn(
                        'flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-medium transition-all',
                        integration.provider === 'metrc' 
                          ? 'bg-emerald-500/10 text-emerald-400 hover:bg-emerald-500/20' 
                          : 'bg-blue-500/10 text-blue-400 hover:bg-blue-500/20',
                        syncing === integration.provider && 'opacity-50 cursor-not-allowed'
                      )}
                    >
                      <RefreshCw className={cn('w-3 h-3', syncing === integration.provider && 'animate-spin')} />
                      {syncing === integration.provider ? 'Syncing...' : 'Sync Now'}
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}

        {activeTab === 'events' && (
          <div className="bg-surface border border-border rounded-xl overflow-hidden">
            <div className="px-4 py-3 border-b border-border flex items-center justify-between">
              <div className="flex items-center gap-3">
                <button className="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-muted/30 border border-border text-sm text-muted-foreground hover:text-foreground">
                  <Filter className="w-3.5 h-3.5" />
                  Filter
                  <ChevronDown className="w-3 h-3" />
                </button>
              </div>
              <button className="p-2 rounded-lg hover:bg-white/5 text-muted-foreground hover:text-foreground">
                <RefreshCw className="w-4 h-4" />
              </button>
            </div>

            <table className="w-full">
              <thead>
                <tr className="border-b border-border bg-muted/30">
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">Provider</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">Event</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">Entity</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">Status</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">Time</th>
                  <th className="px-4 py-3 w-12"></th>
                </tr>
              </thead>
              <tbody>
                {MOCK_SYNC_EVENTS.map((event) => (
                  <tr key={event.id} className="border-b border-border hover:bg-muted/30">
                    <td className="px-4 py-3">
                      <span className={cn(
                        'text-sm font-medium',
                        event.provider === 'metrc' ? 'text-emerald-400' : 'text-blue-400'
                      )}>
                        {event.provider.toUpperCase()}
                      </span>
                    </td>
                    <td className="px-4 py-3">
                      <span className="text-sm text-foreground capitalize">
                        {event.eventType.replace('_', ' ')}
                      </span>
                    </td>
                    <td className="px-4 py-3">
                      <span className="text-sm font-mono text-muted-foreground">
                        {event.entityName}
                      </span>
                    </td>
                    <td className="px-4 py-3">
                      <span className={cn(
                        'inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium',
                        event.status === 'success' && 'bg-emerald-500/10 text-emerald-400',
                        event.status === 'pending' && 'bg-amber-500/10 text-amber-400',
                        event.status === 'failed' && 'bg-rose-500/10 text-rose-400'
                      )}>
                        {event.status === 'success' && <CheckCircle className="w-3 h-3" />}
                        {event.status === 'pending' && <Clock className="w-3 h-3" />}
                        {event.status === 'failed' && <XCircle className="w-3 h-3" />}
                        {event.status}
                      </span>
                    </td>
                    <td className="px-4 py-3">
                      <span className="text-sm text-muted-foreground">
                        {formatTime(event.createdAt)}
                      </span>
                    </td>
                    <td className="px-4 py-3">
                      {event.status === 'failed' && (
                        <button className="p-1.5 rounded hover:bg-white/5 text-muted-foreground hover:text-foreground">
                          <RotateCcw className="w-4 h-4" />
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {activeTab === 'dlq' && (
          <div className="space-y-4">
            {MOCK_DLQ_ITEMS.length === 0 ? (
              <div className="bg-surface border border-border rounded-xl p-12 text-center">
                <CheckCircle className="w-12 h-12 text-emerald-400 mx-auto mb-3" />
                <p className="text-sm text-foreground">Dead Letter Queue is empty</p>
                <p className="text-xs text-muted-foreground mt-1">All sync events are processing successfully</p>
              </div>
            ) : (
              <>
                <div className="flex items-center justify-between">
                  <p className="text-sm text-muted-foreground">
                    {MOCK_DLQ_ITEMS.length} items requiring attention
                  </p>
                  <button className="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-amber-500/10 text-amber-400 text-sm hover:bg-amber-500/20">
                    <Play className="w-3.5 h-3.5" />
                    Retry All
                  </button>
                </div>

                {MOCK_DLQ_ITEMS.map((item) => (
                  <div
                    key={item.id}
                    className="bg-surface border border-rose-500/20 rounded-xl p-4"
                  >
                    <div className="flex items-start justify-between mb-3">
                      <div className="flex items-center gap-3">
                        <AlertCircle className="w-5 h-5 text-rose-400" />
                        <div>
                          <div className="text-sm font-medium text-foreground">
                            {item.eventType.replace('_', ' ')} failed
                          </div>
                          <div className="text-xs text-muted-foreground">
                            {item.provider.toUpperCase()} • Entity: {item.entityId}
                          </div>
                        </div>
                      </div>
                      <span className="text-xs text-muted-foreground">
                        {formatTime(item.failedAt)}
                      </span>
                    </div>

                    <div className="p-3 rounded-lg bg-rose-500/5 border border-rose-500/10 mb-3">
                      <code className="text-xs text-rose-400">{item.errorMessage}</code>
                    </div>

                    <div className="flex items-center justify-between">
                      <span className="text-xs text-muted-foreground">
                        Retry attempts: {item.retryCount}
                      </span>
                      <div className="flex items-center gap-2">
                        <button className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-white/5 text-sm text-foreground hover:bg-white/10">
                          <RotateCcw className="w-3.5 h-3.5" />
                          Retry
                        </button>
                        <button className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-white/5 text-sm text-muted-foreground hover:text-rose-400 hover:bg-rose-500/10">
                          <Trash2 className="w-3.5 h-3.5" />
                          Dismiss
                        </button>
                      </div>
                    </div>
                  </div>
                ))}
              </>
            )}
          </div>
        )}
      </div>
    </div>
  );
}
