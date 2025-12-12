'use client';

import { useState } from 'react';
import {
  FileText,
  Users,
  Package,
  Truck,
  AlertTriangle,
  CheckCircle,
  Clock,
} from 'lucide-react';
import Link from 'next/link';
import { Card, CardHeader, KPICard, DemoModeBanner } from '@/features/sales/components/shared';
import {
  DEMO_DASHBOARD_KPIS,
  DEMO_PIPELINE,
  DEMO_RECENT_ACTIVITY,
  DEMO_COMPLIANCE_SUMMARY,
} from '@/features/sales/demo';

function formatRelativeTime(timestamp: string): string {
  const diff = Date.now() - new Date(timestamp).getTime();
  const minutes = Math.floor(diff / 60000);
  if (minutes < 60) return `${minutes}m ago`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  return `${days}d ago`;
}

export default function SalesDashboardPage() {
  const [isDemoMode] = useState(true); // Will be replaced with actual backend check
  const kpis = DEMO_DASHBOARD_KPIS;
  const compliance = DEMO_COMPLIANCE_SUMMARY;

  return (
    <div className="p-6 space-y-6">
      {/* Demo Mode Banner */}
      {isDemoMode && <DemoModeBanner />}

      {/* KPI Strip */}
      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4">
        <KPICard
          label="Open Orders"
          value={kpis.openOrders}
          icon={FileText}
          iconColor="text-blue-400"
        />
        <KPICard
          label="Active Customers"
          value={kpis.customersActive}
          icon={Users}
          iconColor="text-violet-400"
        />
        <KPICard
          label="Shipped (7d)"
          value={kpis.shipmentsThisWeek}
          icon={Package}
          iconColor="text-emerald-400"
        />
        <KPICard
          label="Pending Transfers"
          value={kpis.pendingTransfers}
          icon={Truck}
          iconColor="text-amber-400"
        />
        <KPICard
          label="METRC Pending"
          value={kpis.metrcPending}
          icon={Clock}
          iconColor="text-cyan-400"
        />
        <KPICard
          label="METRC Failed"
          value={kpis.metrcFailed}
          icon={kpis.metrcFailed > 0 ? AlertTriangle : CheckCircle}
          iconColor={kpis.metrcFailed > 0 ? 'text-rose-400' : 'text-emerald-400'}
        />
      </div>

      {/* Main Grid */}
      <div className="grid grid-cols-12 gap-6">
        {/* Left Column - Pipeline + Quick Navigation */}
        <div className="col-span-12 lg:col-span-8 space-y-6">
          {/* Order Pipeline */}
          <Card>
            <CardHeader
              title="Order Pipeline"
              subtitle="Current order distribution by status"
              action={
                <Link
                  href="/sales/orders"
                  className="text-xs text-amber-400 hover:text-amber-300"
                >
                  View All Orders →
                </Link>
              }
            />
            <div className="space-y-3">
              {DEMO_PIPELINE.map((stage) => (
                <div key={stage.stage} className="flex items-center gap-3">
                  <div className="w-24 text-sm text-muted-foreground">{stage.stage}</div>
                  <div className="flex-1 h-8 bg-muted/30 rounded-lg overflow-hidden">
                    <div
                      className={`h-full ${stage.color} rounded-lg transition-all`}
                      style={{ width: `${(stage.count / 12) * 100}%` }}
                    />
                  </div>
                  <div className="w-8 text-sm font-medium text-foreground tabular-nums text-right">
                    {stage.count}
                  </div>
                </div>
              ))}
            </div>
          </Card>

          {/* Quick Navigation */}
          <Card>
            <CardHeader title="Quick Actions" />
            <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
              <Link
                href="/sales/customers"
                className="flex items-center gap-3 p-4 bg-muted/30 border border-border rounded-xl hover:border-amber-500/30 transition-all group"
              >
                <div className="w-10 h-10 rounded-lg bg-violet-500/10 flex items-center justify-center">
                  <Users className="w-5 h-5 text-violet-400" />
                </div>
                <div>
                  <div className="text-sm font-medium text-foreground group-hover:text-amber-400">
                    Customers
                  </div>
                  <div className="text-xs text-muted-foreground">Manage accounts</div>
                </div>
              </Link>
              <Link
                href="/sales/orders"
                className="flex items-center gap-3 p-4 bg-muted/30 border border-border rounded-xl hover:border-amber-500/30 transition-all group"
              >
                <div className="w-10 h-10 rounded-lg bg-blue-500/10 flex items-center justify-center">
                  <FileText className="w-5 h-5 text-blue-400" />
                </div>
                <div>
                  <div className="text-sm font-medium text-foreground group-hover:text-amber-400">
                    Orders
                  </div>
                  <div className="text-xs text-muted-foreground">Create & manage</div>
                </div>
              </Link>
              <Link
                href="/sales/shipments"
                className="flex items-center gap-3 p-4 bg-muted/30 border border-border rounded-xl hover:border-amber-500/30 transition-all group"
              >
                <div className="w-10 h-10 rounded-lg bg-emerald-500/10 flex items-center justify-center">
                  <Package className="w-5 h-5 text-emerald-400" />
                </div>
                <div>
                  <div className="text-sm font-medium text-foreground group-hover:text-amber-400">
                    Shipments
                  </div>
                  <div className="text-xs text-muted-foreground">Pick & pack</div>
                </div>
              </Link>
              <Link
                href="/sales/transfers"
                className="flex items-center gap-3 p-4 bg-muted/30 border border-border rounded-xl hover:border-amber-500/30 transition-all group"
              >
                <div className="w-10 h-10 rounded-lg bg-amber-500/10 flex items-center justify-center">
                  <Truck className="w-5 h-5 text-amber-400" />
                </div>
                <div>
                  <div className="text-sm font-medium text-foreground group-hover:text-amber-400">
                    Transfers
                  </div>
                  <div className="text-xs text-muted-foreground">METRC compliance</div>
                </div>
              </Link>
            </div>
          </Card>
        </div>

        {/* Right Column - Activity + Compliance */}
        <div className="col-span-12 lg:col-span-4 space-y-6">
          {/* Compliance Status */}
          <Card>
            <CardHeader
              title="Compliance Status"
              subtitle="METRC sync health"
            />
            <div className="space-y-3">
              <div className="flex items-center justify-between p-3 rounded-lg bg-emerald-500/10 border border-emerald-500/30">
                <div className="flex items-center gap-2">
                  <CheckCircle className="w-4 h-4 text-emerald-400" />
                  <span className="text-sm text-emerald-300">Synced</span>
                </div>
                <span className="text-sm font-medium text-emerald-400">{compliance.synced}</span>
              </div>
              <div className="flex items-center justify-between p-3 rounded-lg bg-amber-500/10 border border-amber-500/30">
                <div className="flex items-center gap-2">
                  <Clock className="w-4 h-4 text-amber-400" />
                  <span className="text-sm text-amber-300">Pending</span>
                </div>
                <span className="text-sm font-medium text-amber-400">
                  {compliance.pending}
                </span>
              </div>
              {compliance.failed > 0 && (
                <div className="flex items-center justify-between p-3 rounded-lg bg-rose-500/10 border border-rose-500/30">
                  <div className="flex items-center gap-2">
                    <AlertTriangle className="w-4 h-4 text-rose-400" />
                    <span className="text-sm text-rose-300">Failed</span>
                  </div>
                  <span className="text-sm font-medium text-rose-400">
                    {compliance.failed}
                  </span>
                </div>
              )}
            </div>
          </Card>

          {/* Recent Activity */}
          <Card>
            <CardHeader title="Recent Activity" />
            <div className="space-y-3">
              {DEMO_RECENT_ACTIVITY.map((activity) => (
                <div
                  key={activity.id}
                  className="flex items-start gap-3 p-2 rounded-lg hover:bg-muted/30 transition-colors"
                >
                  <div
                    className={`w-8 h-8 rounded-lg flex items-center justify-center flex-shrink-0 ${
                      activity.type === 'order'
                        ? 'bg-blue-500/10'
                        : activity.type === 'shipment'
                          ? 'bg-emerald-500/10'
                          : 'bg-amber-500/10'
                    }`}
                  >
                    {activity.type === 'order' && (
                      <FileText className="w-4 h-4 text-blue-400" />
                    )}
                    {activity.type === 'shipment' && (
                      <Package className="w-4 h-4 text-emerald-400" />
                    )}
                    {activity.type === 'transfer' && (
                      <Truck className="w-4 h-4 text-amber-400" />
                    )}
                  </div>
                  <div className="flex-1 min-w-0">
                    <div className="text-sm text-foreground truncate">
                      {activity.title}
                    </div>
                    <div className="text-xs text-muted-foreground">
                      {activity.customer} • {formatRelativeTime(activity.timestamp)}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </Card>
        </div>
      </div>
    </div>
  );
}
