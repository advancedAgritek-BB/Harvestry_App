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
    bg: 'bg-rose-500/15',
    border: 'border-rose-500/30',
    label: 'Critical'
  },
  warning: { 
    icon: AlertTriangle, 
    color: 'text-amber-400', 
    bg: 'bg-amber-500/15',
    border: 'border-amber-500/30',
    label: 'Warning'
  },
  info: { 
    icon: Info, 
    color: 'text-sky-400', 
    bg: 'bg-sky-500/15',
    border: 'border-sky-500/30',
    label: 'Info'
  },
};

const TASK_CONFIG = {
  critical: { color: 'bg-rose-400', glow: 'shadow-[0_0_8px_rgba(251,113,133,0.6)]' },
  high: { color: 'bg-amber-400', glow: 'shadow-[0_0_6px_rgba(251,191,36,0.4)]' },
  normal: { color: 'bg-cyan-400', glow: '' },
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
              <div className="p-2 rounded-xl bg-amber-500/15">
                <Bell className="w-5 h-5 text-amber-400" />
              </div>
              Alerts
            </h2>
            <span className={cn(
              'px-2.5 py-1 rounded-lg text-sm font-medium',
              totalAlerts > 0 ? 'bg-amber-500/15 text-amber-400' : 'bg-muted/50 text-muted-foreground'
            )}>
              {totalAlerts}
            </span>
          </div>

          {alerts.length === 0 ? (
            <div className="flex items-center gap-3 p-4 rounded-xl bg-surface/30 border border-border">
              <CheckCircle2 className="w-5 h-5 text-emerald-400" />
              <span className="text-sm text-muted-foreground">No active alerts</span>
            </div>
          ) : (
            <div className="space-y-3">
              {alerts.map((alert) => (
                <AlertRow key={alert.id} alert={alert} />
              ))}
            </div>
          )}

          {hasMoreAlerts && (
            <Link 
              href="/dashboard/cultivation"
              className="flex items-center justify-center gap-1.5 mt-3 py-2 text-sm font-medium text-cyan-400 hover:text-cyan-300 transition-colors"
            >
              View all {totalAlerts} alerts <ArrowRight className="w-4 h-4" />
            </Link>
          )}
        </div>
      )}

      {/* Divider */}
      {showAlerts && showTasks && (
        <div className="border-t border-border/30 mb-6" />
      )}

      {/* Tasks Section */}
      {showTasks && (
        <div className="flex-1 flex flex-col min-h-0">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-base font-semibold text-foreground flex items-center gap-3">
              <div className="p-2 rounded-xl bg-violet-500/15">
                <ClipboardList className="w-5 h-5 text-violet-400" />
              </div>
              Tasks
            </h2>
            <span className={cn(
              'px-2.5 py-1 rounded-lg text-sm font-medium',
              totalTasks > 0 ? 'bg-violet-500/15 text-violet-400' : 'bg-muted/50 text-muted-foreground'
            )}>
              {totalTasks}
            </span>
          </div>

          {tasks.length === 0 ? (
            <div className="flex items-center gap-3 p-4 rounded-xl bg-surface/30 border border-border">
              <CheckCircle2 className="w-5 h-5 text-emerald-400" />
              <span className="text-sm text-muted-foreground">No pending tasks</span>
            </div>
          ) : (
            <div className="space-y-3 flex-1 overflow-y-auto">
              {tasks.map((task) => (
                <TaskRow key={task.id} task={task} />
              ))}
            </div>
          )}

          {hasMoreTasks && (
            <Link 
              href="/dashboard/tasks"
              className="flex items-center justify-center gap-1.5 mt-3 py-2 text-sm font-medium text-cyan-400 hover:text-cyan-300 transition-colors"
            >
              View all {totalTasks} tasks <ArrowRight className="w-4 h-4" />
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
        'group flex items-center gap-4 p-4 rounded-xl border transition-all duration-200',
        'hover:scale-[1.01] cursor-pointer',
        'bg-gradient-to-r from-surface/50 to-transparent',
        config.border
      )}
    >
      <div className={cn('p-2.5 rounded-xl shrink-0', config.bg)}>
        <Icon className={cn('w-5 h-5', config.color, alert.severity === 'critical' && 'animate-pulse')} />
      </div>
      
      <div className="flex-1 min-w-0">
        <h4 className="text-sm font-semibold text-foreground leading-tight truncate group-hover:text-cyan-300 transition-colors">
          {alert.title}
        </h4>
        <p className="text-sm text-muted-foreground mt-0.5">
          {alert.source} • {formatTimeAgo(alert.timestamp)}
        </p>
      </div>

      <ChevronRight className="w-5 h-5 text-muted-foreground group-hover:text-foreground transition-colors shrink-0" />
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
        'group flex items-center gap-4 p-4 rounded-xl border transition-all duration-200',
        'hover:scale-[1.01] cursor-pointer',
        'bg-gradient-to-r from-surface/50 to-transparent',
        isSlaIssue ? 'border-amber-500/30' : 'border-border'
      )}
    >
      {/* Priority indicator */}
      <div className={cn(
        'w-3 h-3 rounded-full shrink-0',
        priorityConfig.color,
        priorityConfig.glow
      )} />

      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2 mb-0.5">
          {task.slaStatus === 'breached' && (
            <span className="px-2 py-0.5 text-xs font-bold uppercase rounded-md bg-rose-500/20 text-rose-400">
              Overdue
            </span>
          )}
        </div>
        <h4 className="text-sm font-semibold text-foreground leading-tight truncate group-hover:text-cyan-300 transition-colors">
          {task.title}
        </h4>
        <div className="flex items-center gap-2 text-sm text-muted-foreground mt-0.5">
          <Clock className="w-3.5 h-3.5" />
          <span>{formatTimeAgo(task.timestamp)}</span>
          <span className="text-muted-foreground/50">•</span>
          <span className="truncate">{task.source}</span>
        </div>
      </div>

      {/* Assignee */}
      <div className="w-8 shrink-0 flex justify-center">
        {task.assignee ? (
          <div 
            className="w-8 h-8 rounded-full bg-gradient-to-br from-violet-500 to-fuchsia-500 flex items-center justify-center ring-2 ring-border"
            title={`${task.assignee.firstName} ${task.assignee.lastName}`}
          >
            <span className="text-xs font-bold text-foreground">
              {task.assignee.firstName.charAt(0)}{task.assignee.lastName.charAt(0)}
            </span>
          </div>
        ) : (
          <div 
            className="w-8 h-8 rounded-full bg-muted/50 flex items-center justify-center ring-2 ring-amber-500/30"
            title="Unassigned"
          >
            <UserCircle2 className="w-4 h-4 text-amber-400/80" />
          </div>
        )}
      </div>

      <ChevronRight className="w-5 h-5 text-muted-foreground group-hover:text-foreground transition-colors shrink-0" />
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
