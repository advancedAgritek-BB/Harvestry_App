'use client';

import React, { useEffect, useState } from 'react';
import Link from 'next/link';
import { formatDistanceToNow, format } from 'date-fns';
import {
  Factory,
  Plus,
  Search,
  Filter,
  ChevronDown,
  ChevronLeft,
  MoreHorizontal,
  Edit2,
  Trash2,
  Eye,
  Play,
  Pause,
  CheckCircle2,
  XCircle,
  Clock,
  Package,
  AlertTriangle,
  FileEdit,
  ClipboardCheck,
  BarChart3,
  Calendar,
  Users,
  ArrowRight,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { useManufacturingStore } from '@/features/inventory/stores/manufacturingStore';
import { ProductionService } from '@/features/inventory/services/production.service';
import {
  PRODUCTION_STATUS_CONFIG,
  PRIORITY_CONFIG,
  type ProductionOrder,
  type ProductionOrderStatus,
  type ProductionPriority,
} from '@/features/inventory/types';

// Status icons
const STATUS_ICONS: Record<ProductionOrderStatus, React.ElementType> = {
  draft: FileEdit,
  pending_materials: Package,
  pending_approval: Clock,
  ready: CheckCircle2,
  in_progress: Play,
  on_hold: Pause,
  pending_qa: ClipboardCheck,
  completed: CheckCircle2,
  cancelled: XCircle,
  failed: AlertTriangle,
};

// Status badge
function StatusBadge({ status }: { status: ProductionOrderStatus }) {
  const config = PRODUCTION_STATUS_CONFIG[status];
  const Icon = STATUS_ICONS[status];

  return (
    <span className={cn('inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-medium', config.color, config.bgColor)}>
      <Icon className="w-3.5 h-3.5" />
      {config.label}
    </span>
  );
}

// Priority badge
function PriorityBadge({ priority }: { priority: ProductionPriority }) {
  const config = PRIORITY_CONFIG[priority];

  return (
    <span className={cn('px-2 py-0.5 rounded text-xs font-medium', config.color, config.bgColor)}>
      {config.label}
    </span>
  );
}

// Status filter tabs
function StatusTabs({
  selected,
  onChange,
  counts,
}: {
  selected: ProductionOrderStatus | 'all' | 'active';
  onChange: (status: ProductionOrderStatus | 'all' | 'active') => void;
  counts: Record<string, number>;
}) {
  const statuses: (ProductionOrderStatus | 'all' | 'active')[] = [
    'all',
    'active',
    'draft',
    'pending_materials',
    'ready',
    'in_progress',
    'pending_qa',
    'completed',
  ];

  return (
    <div className="flex items-center gap-1 p-1 bg-muted/30 rounded-lg overflow-x-auto">
      {statuses.map((status) => {
        const isSelected = selected === status;
        let label = '';
        let count = 0;

        if (status === 'all') {
          label = 'All';
          count = Object.values(counts).reduce((a, b) => a + b, 0);
        } else if (status === 'active') {
          label = 'Active';
          count = (counts['in_progress'] || 0) + (counts['ready'] || 0);
        } else {
          label = PRODUCTION_STATUS_CONFIG[status].label;
          count = counts[status] || 0;
        }

        return (
          <button
            key={status}
            onClick={() => onChange(status)}
            className={cn(
              'flex items-center gap-2 px-3 py-1.5 rounded-md text-sm font-medium transition-all whitespace-nowrap',
              isSelected
                ? 'bg-cyan-500/10 text-cyan-400'
                : 'text-muted-foreground hover:text-foreground hover:bg-white/5'
            )}
          >
            <span>{label}</span>
            <span className={cn(
              'px-1.5 py-0.5 rounded text-xs',
              isSelected ? 'bg-cyan-500/20' : 'bg-white/5'
            )}>
              {count}
            </span>
          </button>
        );
      })}
    </div>
  );
}

// KPI card component
function KPICard({
  label,
  value,
  subValue,
  icon: Icon,
  accent,
}: {
  label: string;
  value: string | number;
  subValue?: string;
  icon: React.ElementType;
  accent: string;
}) {
  return (
    <div className="p-4 bg-surface border border-border rounded-xl">
      <div className="flex items-start justify-between">
        <div>
          <div className="text-xs text-muted-foreground mb-1">{label}</div>
          <div className="text-2xl font-bold text-foreground">{value}</div>
          {subValue && <div className="text-xs text-muted-foreground mt-1">{subValue}</div>}
        </div>
        <div className={cn('w-10 h-10 rounded-lg flex items-center justify-center', `bg-${accent}-500/10`)}>
          <Icon className={cn('w-5 h-5', `text-${accent}-400`)} />
        </div>
      </div>
    </div>
  );
}

// Order card component
function OrderCard({
  order,
  onView,
}: {
  order: ProductionOrder;
  onView: () => void;
}) {
  const [menuOpen, setMenuOpen] = useState(false);

  return (
    <div
      className="group p-5 bg-surface border border-border rounded-xl hover:border-cyan-500/30 transition-all cursor-pointer"
      onClick={onView}
    >
      {/* Header */}
      <div className="flex items-start justify-between mb-4">
        <div>
          <div className="font-mono text-sm text-cyan-400">{order.orderNumber}</div>
          <div className="text-sm text-muted-foreground mt-0.5">{order.description}</div>
        </div>
        <div className="flex items-center gap-2">
          <PriorityBadge priority={order.priority} />
          <StatusBadge status={order.status} />
        </div>
      </div>

      {/* Output */}
      <div className="mb-4 p-3 bg-muted/30 rounded-lg">
        <div className="text-xs text-muted-foreground mb-1">Output</div>
        <div className="flex items-center justify-between">
          <span className="text-sm text-foreground">
            {order.outputProduct?.name || 'Unknown Product'}
          </span>
          <span className="text-sm font-mono text-foreground">
            {order.plannedQuantity} {order.plannedUom}
          </span>
        </div>
      </div>

      {/* Progress */}
      {order.status === 'in_progress' && (
        <div className="mb-4">
          <div className="flex items-center justify-between text-xs mb-1">
            <span className="text-muted-foreground">Progress</span>
            <span className="text-foreground">{order.progressPercent}%</span>
          </div>
          <div className="h-1.5 bg-white/5 rounded-full overflow-hidden">
            <div
              className="h-full bg-cyan-500 rounded-full transition-all"
              style={{ width: `${order.progressPercent}%` }}
            />
          </div>
        </div>
      )}

      {/* Stats */}
      <div className="grid grid-cols-3 gap-2 text-xs">
        <div className="p-2 bg-muted/30 rounded text-center">
          <Calendar className="w-3.5 h-3.5 mx-auto mb-1 text-muted-foreground" />
          <div className="text-muted-foreground">Planned</div>
          <div className="text-foreground">
            {format(new Date(order.plannedStartDate), 'MMM d')}
          </div>
        </div>
        <div className="p-2 bg-muted/30 rounded text-center">
          <Users className="w-3.5 h-3.5 mx-auto mb-1 text-muted-foreground" />
          <div className="text-muted-foreground">Labor</div>
          <div className="text-foreground">{order.estimatedLaborHours}h</div>
        </div>
        <div className="p-2 bg-muted/30 rounded text-center">
          <BarChart3 className="w-3.5 h-3.5 mx-auto mb-1 text-muted-foreground" />
          <div className="text-muted-foreground">Yield</div>
          <div className="text-foreground">{order.expectedYieldPercent}%</div>
        </div>
      </div>

      {/* Footer */}
      <div className="flex items-center justify-between mt-4 pt-3 border-t border-border">
        <span className="text-xs text-muted-foreground">
          {order.workCenterName || 'No work center'}
        </span>
        <span className="text-xs text-muted-foreground">
          Updated {formatDistanceToNow(new Date(order.updatedAt), { addSuffix: true })}
        </span>
      </div>
    </div>
  );
}

// Order row component
function OrderRow({
  order,
  onView,
}: {
  order: ProductionOrder;
  onView: () => void;
}) {
  return (
    <tr
      className="group border-b border-border hover:bg-muted/30 transition-colors cursor-pointer"
      onClick={onView}
    >
      <td className="px-4 py-3">
        <div className="font-mono text-sm text-cyan-400">{order.orderNumber}</div>
        <div className="text-xs text-muted-foreground truncate max-w-[200px]">
          {order.description}
        </div>
      </td>
      <td className="px-4 py-3 text-sm text-foreground">
        {order.outputProduct?.name || 'Unknown'}
      </td>
      <td className="px-4 py-3 text-sm font-mono text-foreground">
        {order.plannedQuantity} {order.plannedUom}
      </td>
      <td className="px-4 py-3">
        <StatusBadge status={order.status} />
      </td>
      <td className="px-4 py-3">
        <PriorityBadge priority={order.priority} />
      </td>
      <td className="px-4 py-3 text-sm text-foreground">
        {format(new Date(order.plannedStartDate), 'MMM d, yyyy')}
      </td>
      <td className="px-4 py-3">
        {order.status === 'in_progress' ? (
          <div className="flex items-center gap-2">
            <div className="w-16 h-1.5 bg-white/5 rounded-full overflow-hidden">
              <div
                className="h-full bg-cyan-500 rounded-full"
                style={{ width: `${order.progressPercent}%` }}
              />
            </div>
            <span className="text-xs text-foreground">{order.progressPercent}%</span>
          </div>
        ) : (
          <span className="text-xs text-muted-foreground">—</span>
        )}
      </td>
      <td className="px-4 py-3 text-sm text-muted-foreground">
        {order.workCenterName || '—'}
      </td>
      <td className="px-4 py-3">
        <button className="p-1.5 rounded-lg hover:bg-white/5 text-muted-foreground hover:text-foreground opacity-0 group-hover:opacity-100 transition-all">
          <ArrowRight className="w-4 h-4" />
        </button>
      </td>
    </tr>
  );
}

export default function ProductionOrdersPage() {
  const store = useManufacturingStore();
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedStatus, setSelectedStatus] = useState<ProductionOrderStatus | 'all' | 'active'>('all');
  const [viewMode, setViewMode] = useState<'grid' | 'table'>('grid');

  // Load orders on mount
  useEffect(() => {
    const loadOrders = async () => {
      store.setOrdersLoading(true);
      try {
        let statusFilter: ProductionOrderStatus[] | undefined;

        if (selectedStatus === 'active') {
          statusFilter = ['in_progress', 'ready'];
        } else if (selectedStatus !== 'all') {
          statusFilter = [selectedStatus];
        }

        const response = await ProductionService.getProductionOrders({
          status: statusFilter,
          search: searchQuery || undefined,
        });
        store.setProductionOrders(response.items);

        const summary = await ProductionService.getDashboardSummary();
        store.setProductionSummary(summary);
      } catch (error) {
        store.setOrdersError('Failed to load production orders');
      } finally {
        store.setOrdersLoading(false);
      }
    };

    loadOrders();
  }, [selectedStatus, searchQuery]);

  // Calculate status counts
  const statusCounts = store.productionOrders.reduce((acc, order) => {
    acc[order.status] = (acc[order.status] || 0) + 1;
    return acc;
  }, {} as Record<string, number>);

  // Filter orders
  let filteredOrders = store.productionOrders;

  if (selectedStatus === 'active') {
    filteredOrders = filteredOrders.filter(
      (o) => o.status === 'in_progress' || o.status === 'ready'
    );
  } else if (selectedStatus !== 'all') {
    filteredOrders = filteredOrders.filter((o) => o.status === selectedStatus);
  }

  if (searchQuery) {
    const query = searchQuery.toLowerCase();
    filteredOrders = filteredOrders.filter(
      (o) =>
        o.orderNumber.toLowerCase().includes(query) ||
        o.description?.toLowerCase().includes(query)
    );
  }

  const summary = store.productionSummary;

  return (
    <div className="min-h-screen bg-background">
      {/* Header */}
      <header className="sticky top-0 z-40 glass-header">
        <div className="px-6 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <Link
                href="/inventory"
                className="p-2 rounded-lg hover:bg-white/5 text-muted-foreground hover:text-foreground transition-colors"
              >
                <ChevronLeft className="w-5 h-5" />
              </Link>
              <div className="w-10 h-10 rounded-xl bg-violet-500/10 flex items-center justify-center">
                <Factory className="w-5 h-5 text-violet-400" />
              </div>
              <div>
                <h1 className="text-xl font-semibold text-foreground">Production Orders</h1>
                <p className="text-sm text-muted-foreground">
                  Manage manufacturing work orders
                </p>
              </div>
            </div>

            {/* Actions */}
            <div className="flex items-center gap-3">
              <Link
                href="/inventory/production/new"
                className="flex items-center gap-2 px-4 py-2 rounded-lg bg-cyan-500 text-black font-medium hover:bg-cyan-400 transition-colors"
              >
                <Plus className="w-4 h-4" />
                <span className="text-sm">New Order</span>
              </Link>
            </div>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="px-6 py-6 space-y-6">
        {/* KPI Strip */}
        {summary && (
          <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
            <KPICard
              label="Today's Planned"
              value={summary.todaySchedule.plannedOrders}
              subValue={`${summary.todaySchedule.completedOrders} completed`}
              icon={Calendar}
              accent="cyan"
            />
            <KPICard
              label="In Progress"
              value={statusCounts['in_progress'] || 0}
              subValue="Active orders"
              icon={Play}
              accent="blue"
            />
            <KPICard
              label="Pending Materials"
              value={summary.materialStatus.pendingAllocation}
              subValue={`${summary.materialStatus.lowStockAlerts} low stock alerts`}
              icon={Package}
              accent="amber"
            />
            <KPICard
              label="Pending QA"
              value={summary.qaStatus.pendingRelease}
              subValue="Awaiting release"
              icon={ClipboardCheck}
              accent="violet"
            />
          </div>
        )}

        {/* Status Tabs */}
        <StatusTabs
          selected={selectedStatus}
          onChange={setSelectedStatus}
          counts={statusCounts}
        />

        {/* Toolbar */}
        <div className="flex items-center justify-between gap-4">
          {/* Search */}
          <div className="relative flex-1 max-w-md">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
            <input
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              placeholder="Search orders by number..."
              className="w-full pl-10 pr-4 py-2 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-cyan-500/30"
            />
          </div>

          {/* View Toggle & Filter */}
          <div className="flex items-center gap-2">
            <div className="flex items-center bg-muted/30 rounded-lg p-1">
              <button
                onClick={() => setViewMode('grid')}
                className={cn(
                  'p-1.5 rounded transition-colors',
                  viewMode === 'grid'
                    ? 'bg-cyan-500/10 text-cyan-400'
                    : 'text-muted-foreground hover:text-foreground'
                )}
              >
                <Factory className="w-4 h-4" />
              </button>
              <button
                onClick={() => setViewMode('table')}
                className={cn(
                  'p-1.5 rounded transition-colors',
                  viewMode === 'table'
                    ? 'bg-cyan-500/10 text-cyan-400'
                    : 'text-muted-foreground hover:text-foreground'
                )}
              >
                <Filter className="w-4 h-4" />
              </button>
            </div>

            <button className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-muted/30 text-muted-foreground hover:text-foreground transition-colors">
              <Filter className="w-3.5 h-3.5" />
              <span className="text-xs">Filter</span>
              <ChevronDown className="w-3 h-3" />
            </button>
          </div>
        </div>

        {/* Results Count */}
        <div className="text-sm text-muted-foreground">
          Showing {filteredOrders.length} orders
        </div>

        {/* Loading State */}
        {store.ordersLoading ? (
          <div className="flex items-center justify-center py-12">
            <div className="animate-spin w-8 h-8 border-2 border-cyan-500 border-t-transparent rounded-full" />
          </div>
        ) : viewMode === 'grid' ? (
          /* Grid View */
          <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
            {filteredOrders.map((order) => (
              <OrderCard
                key={order.id}
                order={order}
                onView={() => {
                  window.location.href = `/inventory/production/${order.id}`;
                }}
              />
            ))}
          </div>
        ) : (
          /* Table View */
          <div className="bg-surface border border-border rounded-xl overflow-hidden">
            <table className="w-full">
              <thead>
                <tr className="border-b border-border bg-muted/30">
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                    Order
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                    Output
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                    Qty
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                    Status
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                    Priority
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                    Planned Date
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                    Progress
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                    Work Center
                  </th>
                  <th className="px-4 py-3 w-12"></th>
                </tr>
              </thead>
              <tbody>
                {filteredOrders.map((order) => (
                  <OrderRow
                    key={order.id}
                    order={order}
                    onView={() => {
                      window.location.href = `/inventory/production/${order.id}`;
                    }}
                  />
                ))}
              </tbody>
            </table>

            {filteredOrders.length === 0 && (
              <div className="py-12 text-center">
                <Factory className="w-12 h-12 text-muted-foreground mx-auto mb-3" />
                <p className="text-muted-foreground">No production orders found</p>
              </div>
            )}
          </div>
        )}

        {filteredOrders.length === 0 && !store.ordersLoading && (
          <div className="text-center py-8">
            <Link
              href="/inventory/production/new"
              className="inline-flex items-center gap-2 px-4 py-2 rounded-lg bg-cyan-500/10 text-cyan-400 hover:bg-cyan-500/20 transition-colors"
            >
              <Plus className="w-4 h-4" />
              Create Production Order
            </Link>
          </div>
        )}
      </main>
    </div>
  );
}

