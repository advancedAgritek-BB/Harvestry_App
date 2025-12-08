'use client';

import React from 'react';
import Link from 'next/link';
import { 
  AlertTriangle, 
  AlertCircle, 
  Info, 
  ClipboardList, 
  Clock, 
  ChevronRight,
  CheckCircle2,
  UserCircle2,
  ArrowRight,
  Bell
} from 'lucide-react';
import { cn } from '@/lib/utils';

// Types for unified action items
type AlertSeverity = 'critical' | 'warning' | 'info';
type TaskPriority = 'critical' | 'high' | 'normal';
type SlaStatus = 'breached' | 'warning' | 'ok';

interface BaseActionItem {
  id: string;
  title: string;
  source: string;
  timestamp: string;
  href?: string;
}

interface AlertActionItem extends BaseActionItem {
  type: 'alert';
  severity: AlertSeverity;
}

interface TaskActionItem extends BaseActionItem {
  type: 'task';
  priority: TaskPriority;
  slaStatus: SlaStatus;
  assignee?: {
    id: string;
    firstName: string;
    lastName: string;
  };
}

type ActionItem = AlertActionItem | TaskActionItem;

interface ActionPanelProps {
  items: ActionItem[];
  showAlerts?: boolean;
  showTasks?: boolean;
  maxAlerts?: number;
  maxTasks?: number;
  className?: string;
}

// Priority scoring for sorting within each list
function getAlertPriority(alert: AlertActionItem): number {
  switch (alert.severity) {
    case 'critical': return 100;
    case 'warning': return 50;
    case 'info': return 10;
  }
}

function getTaskPriority(task: TaskActionItem): number {
  if (task.slaStatus === 'breached') return 90;
  if (task.slaStatus === 'warning') return 60;
  switch (task.priority) {
    case 'critical': return 80;
    case 'high': return 40;
    case 'normal': return 20;
  }
}

const ALERT_CONFIG = {
  critical: { 
    icon: AlertCircle, 
    color: 'text-rose-400', 
    iconBg: 'bg-gradient-to-br from-rose-500/30 to-rose-600/20 ring-1 ring-rose-400/30',
    cardBg: 'bg-gradient-to-r from-rose-500/10 via-rose-500/5 to-transparent',
    accentBar: 'bg-rose-400',
    shadow: 'shadow-lg shadow-rose-500/5 hover:shadow-rose-500/10',
    label: 'Critical'
  },
  warning: { 
    icon: AlertTriangle, 
    color: 'text-amber-400', 
    iconBg: 'bg-gradient-to-br from-amber-500/30 to-amber-600/20 ring-1 ring-amber-400/30',
    cardBg: 'bg-gradient-to-r from-amber-500/10 via-amber-500/5 to-transparent',
    accentBar: 'bg-amber-400',
    shadow: 'shadow-lg shadow-amber-500/5 hover:shadow-amber-500/10',
    label: 'Warning'
  },
  info: { 
    icon: Info, 
    color: 'text-sky-400', 
    iconBg: 'bg-gradient-to-br from-sky-500/30 to-sky-600/20 ring-1 ring-sky-400/30',
    cardBg: 'bg-gradient-to-r from-sky-500/10 via-sky-500/5 to-transparent',
    accentBar: 'bg-sky-400',
    shadow: 'shadow-lg shadow-sky-500/5 hover:shadow-sky-500/10',
    label: 'Info'
  },
};

const TASK_CONFIG = {
  critical: { 
    accentBar: 'bg-rose-400', 
    cardBg: 'bg-gradient-to-r from-rose-500/10 via-rose-500/5 to-transparent',
    shadow: 'shadow-lg shadow-rose-500/5 hover:shadow-rose-500/10',
    glow: 'shadow-[0_0_8px_rgba(251,113,133,0.5)]' 
  },
  high: { 
    accentBar: 'bg-amber-400', 
    cardBg: 'bg-gradient-to-r from-amber-500/10 via-amber-500/5 to-transparent',
    shadow: 'shadow-lg shadow-amber-500/5 hover:shadow-amber-500/10',
    glow: 'shadow-[0_0_6px_rgba(251,191,36,0.4)]' 
  },
  normal: { 
    accentBar: 'bg-cyan-400', 
    cardBg: 'bg-gradient-to-r from-surface/40 to-transparent',
    shadow: 'shadow-md shadow-black/10 hover:shadow-lg',
    glow: '' 
  },
};

export function ActionPanel({ 
  items, 
  showAlerts = true, 
  showTasks = true, 
  maxAlerts = 3,
  maxTasks = 4,
  className 
}: ActionPanelProps) {
  // Separate and sort alerts and tasks
  const alerts = items
    .filter((item): item is AlertActionItem => item.type === 'alert')
    .sort((a, b) => getAlertPriority(b) - getAlertPriority(a))
    .slice(0, maxAlerts);

  const tasks = items
    .filter((item): item is TaskActionItem => item.type === 'task')
    .sort((a, b) => getTaskPriority(b) - getTaskPriority(a))
    .slice(0, maxTasks);

  const totalAlerts = items.filter(i => i.type === 'alert').length;
  const totalTasks = items.filter(i => i.type === 'task').length;
  const hasMoreAlerts = totalAlerts > maxAlerts;
  const hasMoreTasks = totalTasks > maxTasks;

  return (
    <div className={cn('flex flex-col h-full', className)}>
      {/* Alerts Section */}
      {showAlerts && (
        <div className="mb-6">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-base font-semibold text-foreground flex items-center gap-3">
              <div className="p-2.5 rounded-xl bg-gradient-to-br from-amber-500/25 to-amber-600/15 ring-1 ring-amber-400/20 shadow-lg shadow-amber-500/10">
                <Bell className="w-5 h-5 text-amber-400" />
              </div>
              Alerts
            </h2>
            <span className={cn(
              'px-3 py-1.5 rounded-full text-sm font-semibold tabular-nums',
              totalAlerts > 0 
                ? 'bg-gradient-to-r from-amber-500/20 to-amber-600/10 text-amber-400 ring-1 ring-amber-400/20' 
                : 'bg-white/5 text-muted-foreground'
            )}>
              {totalAlerts}
            </span>
          </div>

          {alerts.length === 0 ? (
            <div className="flex items-center gap-3 p-4 rounded-xl bg-emerald-500/10 shadow-sm">
              <div className="p-1.5 rounded-lg bg-emerald-500/20">
                <CheckCircle2 className="w-4 h-4 text-emerald-400" />
              </div>
              <span className="text-sm text-emerald-400/80 font-medium">No active alerts</span>
            </div>
          ) : (
            <div className="space-y-2.5">
              {alerts.map((alert) => (
                <AlertRow key={alert.id} alert={alert} />
              ))}
            </div>
          )}

          {hasMoreAlerts && (
            <Link 
              href="/dashboard/cultivation"
              className="group flex items-center justify-center gap-1.5 mt-4 py-2.5 text-sm font-medium text-amber-400/80 hover:text-amber-400 transition-all"
            >
              View all {totalAlerts} alerts 
              <ArrowRight className="w-4 h-4 transition-transform group-hover:translate-x-0.5" />
            </Link>
          )}
        </div>
      )}

      {/* Divider - Gradient */}
      {showAlerts && showTasks && (
        <div className="relative h-px mb-6">
          <div className="absolute inset-0 bg-gradient-to-r from-transparent via-white/10 to-transparent" />
        </div>
      )}

      {/* Tasks Section */}
      {showTasks && (
        <div className="flex-1 flex flex-col min-h-0">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-base font-semibold text-foreground flex items-center gap-3">
              <div className="p-2.5 rounded-xl bg-gradient-to-br from-violet-500/25 to-violet-600/15 ring-1 ring-violet-400/20 shadow-lg shadow-violet-500/10">
                <ClipboardList className="w-5 h-5 text-violet-400" />
              </div>
              Tasks
            </h2>
            <span className={cn(
              'px-3 py-1.5 rounded-full text-sm font-semibold tabular-nums',
              totalTasks > 0 
                ? 'bg-gradient-to-r from-violet-500/20 to-violet-600/10 text-violet-400 ring-1 ring-violet-400/20' 
                : 'bg-white/5 text-muted-foreground'
            )}>
              {totalTasks}
            </span>
          </div>

          {tasks.length === 0 ? (
            <div className="flex items-center gap-3 p-4 rounded-xl bg-emerald-500/10 shadow-sm">
              <div className="p-1.5 rounded-lg bg-emerald-500/20">
                <CheckCircle2 className="w-4 h-4 text-emerald-400" />
              </div>
              <span className="text-sm text-emerald-400/80 font-medium">No pending tasks</span>
            </div>
          ) : (
            <div className="space-y-2.5 flex-1 overflow-y-auto">
              {tasks.map((task) => (
                <TaskRow key={task.id} task={task} />
              ))}
            </div>
          )}

          {hasMoreTasks && (
            <Link 
              href="/dashboard/tasks"
              className="group flex items-center justify-center gap-1.5 mt-4 py-2.5 text-sm font-medium text-violet-400/80 hover:text-violet-400 transition-all"
            >
              View all {totalTasks} tasks 
              <ArrowRight className="w-4 h-4 transition-transform group-hover:translate-x-0.5" />
            </Link>
          )}
        </div>
      )}
    </div>
  );
}

function AlertRow({ alert }: { alert: AlertActionItem }) {
  const config = ALERT_CONFIG[alert.severity];
  const Icon = config.icon;

  const content = (
    <div
      className={cn(
        'group relative flex items-center gap-3.5 p-3.5 pl-4 rounded-xl',
        'transition-all duration-300 cursor-pointer',
        config.cardBg,
        config.shadow,
        'hover:-translate-y-0.5'
      )}
    >
      {/* Left accent bar */}
      <div className={cn(
        'absolute left-0 top-2 bottom-2 w-1 rounded-full',
        config.accentBar,
        alert.severity === 'critical' && 'animate-pulse'
      )} />
      
      <div className={cn(
        'p-2 rounded-lg shrink-0 transition-transform group-hover:scale-105',
        config.iconBg
      )}>
        <Icon className={cn('w-4 h-4', config.color, alert.severity === 'critical' && 'animate-pulse')} />
      </div>
      
      <div className="flex-1 min-w-0">
        <h4 className="text-sm font-semibold text-foreground leading-tight truncate group-hover:text-white transition-colors">
          {alert.title}
        </h4>
        <p className="text-xs text-muted-foreground/80 mt-1">
          {alert.source} • {formatTimeAgo(alert.timestamp)}
        </p>
      </div>

      <ChevronRight className="w-4 h-4 text-muted-foreground/50 group-hover:text-foreground group-hover:translate-x-0.5 transition-all shrink-0" />
    </div>
  );

  if (alert.href) {
    return <Link href={alert.href}>{content}</Link>;
  }
  return content;
}

function TaskRow({ task }: { task: TaskActionItem }) {
  const priorityConfig = TASK_CONFIG[task.priority];
  const isSlaIssue = task.slaStatus === 'breached' || task.slaStatus === 'warning';

  const content = (
    <div
      className={cn(
        'group relative flex items-center gap-3.5 p-3.5 pl-4 rounded-xl',
        'transition-all duration-300 cursor-pointer',
        isSlaIssue ? TASK_CONFIG.critical.cardBg : priorityConfig.cardBg,
        isSlaIssue ? TASK_CONFIG.critical.shadow : priorityConfig.shadow,
        'hover:-translate-y-0.5'
      )}
    >
      {/* Left accent bar */}
      <div className={cn(
        'absolute left-0 top-2 bottom-2 w-1 rounded-full',
        priorityConfig.accentBar,
        priorityConfig.glow
      )} />

      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2 mb-1">
          {task.slaStatus === 'breached' && (
            <span className="px-2 py-0.5 text-[10px] font-bold uppercase rounded-full bg-gradient-to-r from-rose-500/25 to-rose-600/15 text-rose-400 ring-1 ring-rose-400/30">
              Overdue
            </span>
          )}
        </div>
        <h4 className="text-sm font-semibold text-foreground leading-tight truncate group-hover:text-white transition-colors">
          {task.title}
        </h4>
        <div className="flex items-center gap-1.5 text-xs text-muted-foreground/70 mt-1">
          <Clock className="w-3 h-3" />
          <span>{formatTimeAgo(task.timestamp)}</span>
          <span className="text-muted-foreground/40">•</span>
          <span className="truncate">{task.source}</span>
        </div>
      </div>

      {/* Assignee */}
      <div className="shrink-0 flex justify-center">
        {task.assignee ? (
          <div 
            className="w-8 h-8 rounded-full bg-gradient-to-br from-violet-500 to-fuchsia-500 flex items-center justify-center ring-2 ring-white/10 shadow-lg shadow-violet-500/20"
            title={`${task.assignee.firstName} ${task.assignee.lastName}`}
          >
            <span className="text-xs font-bold text-white">
              {task.assignee.firstName.charAt(0)}{task.assignee.lastName.charAt(0)}
            </span>
          </div>
        ) : (
          <div 
            className="w-8 h-8 rounded-full bg-gradient-to-br from-amber-500/20 to-amber-600/10 flex items-center justify-center ring-1 ring-amber-400/30"
            title="Unassigned"
          >
            <UserCircle2 className="w-4 h-4 text-amber-400" />
          </div>
        )}
      </div>

      <ChevronRight className="w-4 h-4 text-muted-foreground/50 group-hover:text-foreground group-hover:translate-x-0.5 transition-all shrink-0" />
    </div>
  );

  if (task.href) {
    return <Link href={task.href}>{content}</Link>;
  }
  return content;
}

function formatTimeAgo(timestamp: string): string {
  const now = new Date();
  const time = new Date(timestamp);
  const diffMs = now.getTime() - time.getTime();
  const diffMins = Math.floor(diffMs / 60000);
  
  if (diffMins < 1) return 'just now';
  if (diffMins < 60) return `${diffMins}m ago`;
  
  const diffHours = Math.floor(diffMins / 60);
  if (diffHours < 24) return `${diffHours}h ago`;
  
  const diffDays = Math.floor(diffHours / 24);
  return `${diffDays}d ago`;
}

export type { ActionItem, AlertActionItem, TaskActionItem };
