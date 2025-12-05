'use client';

/**
 * TaskList Component
 * Table view of tasks with filtering and sorting
 */

import { useState, useMemo } from 'react';
import { ChevronUp, ChevronDown, Play, CheckCircle2 } from 'lucide-react';
import type { Task, TaskStatus, TaskPriority } from '../../types/task.types';

interface TaskListProps {
  tasks: Task[];
  isLoading?: boolean;
  onTaskClick?: (task: Task) => void;
  onStartTask?: (taskId: string) => void;
  onCompleteTask?: (taskId: string) => void;
}

const statusConfig: Record<TaskStatus, { bg: string; text: string; label: string }> = {
  draft: { bg: 'bg-slate-500/10', text: 'text-slate-400', label: 'Draft' },
  ready: { bg: 'bg-sky-500/10', text: 'text-sky-400', label: 'Ready' },
  in_progress: { bg: 'bg-amber-500/10', text: 'text-amber-400', label: 'In Progress' },
  paused: { bg: 'bg-orange-500/10', text: 'text-orange-400', label: 'Paused' },
  blocked: { bg: 'bg-rose-500/10', text: 'text-rose-400', label: 'Blocked' },
  completed: { bg: 'bg-emerald-500/10', text: 'text-emerald-400', label: 'Completed' },
  verified: { bg: 'bg-cyan-500/10', text: 'text-cyan-400', label: 'Verified' },
  closed: { bg: 'bg-slate-500/10', text: 'text-slate-500', label: 'Closed' },
};

const priorityConfig: Record<TaskPriority, { dot: string; label: string }> = {
  critical: { dot: 'bg-rose-500', label: 'Critical' },
  high: { dot: 'bg-amber-500', label: 'High' },
  normal: { dot: 'bg-emerald-500', label: 'Normal' },
  low: { dot: 'bg-slate-400', label: 'Low' },
};

export function TaskList({ 
  tasks, 
  isLoading, 
  onTaskClick, 
  onStartTask, 
  onCompleteTask 
}: TaskListProps) {
  const [sortField, setSortField] = useState<'dueAt' | 'priority' | 'status'>('dueAt');
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc');
  const [statusFilter, setStatusFilter] = useState<TaskStatus | 'all'>('all');

  const filteredTasks = useMemo(() => {
    let filtered = tasks;
    if (statusFilter !== 'all') {
      filtered = tasks.filter(t => t.status === statusFilter);
    }
    return filtered;
  }, [tasks, statusFilter]);

  const sortedTasks = useMemo(() => {
    return [...filteredTasks].sort((a, b) => {
      let comparison = 0;
      
      if (sortField === 'dueAt') {
        comparison = new Date(a.dueAt).getTime() - new Date(b.dueAt).getTime();
      } else if (sortField === 'priority') {
        const priorityOrder = { critical: 0, high: 1, normal: 2, low: 3 };
        comparison = priorityOrder[a.priority] - priorityOrder[b.priority];
      } else if (sortField === 'status') {
        comparison = a.status.localeCompare(b.status);
      }
      
      return sortDirection === 'asc' ? comparison : -comparison;
    });
  }, [filteredTasks, sortField, sortDirection]);

  const handleSort = (field: typeof sortField) => {
    if (sortField === field) {
      setSortDirection(prev => prev === 'asc' ? 'desc' : 'asc');
    } else {
      setSortField(field);
      setSortDirection('asc');
    }
  };

  const formatDate = (dateStr: string) => {
    const date = new Date(dateStr);
    return date.toLocaleDateString(undefined, { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' });
  };

  const isOverdue = (task: Task) => {
    return new Date(task.dueAt) < new Date() && task.status !== 'completed' && task.status !== 'closed';
  };

  const SortIcon = ({ field }: { field: typeof sortField }) => {
    if (sortField !== field) return null;
    return sortDirection === 'asc' 
      ? <ChevronUp className="w-3 h-3 inline ml-1" />
      : <ChevronDown className="w-3 h-3 inline ml-1" />;
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-[var(--accent-cyan)]" />
      </div>
    );
  }

  return (
    <div className="task-list-container rounded-xl overflow-hidden">
      {/* Filters */}
      <div className="p-4 border-b border-[var(--border)] flex items-center gap-4">
        <label className="text-sm font-medium text-[var(--text-muted)]">Status:</label>
        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value as TaskStatus | 'all')}
          className="rounded-lg border border-[var(--border)] bg-[var(--bg-elevated)] text-[var(--text-primary)] text-sm px-3 py-1.5 focus:outline-none focus:border-[var(--accent-cyan)]"
        >
          <option value="all">All</option>
          <option value="ready">Ready</option>
          <option value="in_progress">In Progress</option>
          <option value="blocked">Blocked</option>
          <option value="completed">Completed</option>
        </select>
      </div>

      {/* Table */}
      <div className="overflow-x-auto scrollbar-thin">
        <table className="w-full">
          <thead>
            <tr className="bg-[var(--bg-surface)]">
              <th className="px-4 py-3 text-left text-xs font-medium text-[var(--text-muted)] uppercase tracking-wider">
                Task
              </th>
              <th 
                className="px-4 py-3 text-left text-xs font-medium text-[var(--text-muted)] uppercase tracking-wider cursor-pointer hover:text-[var(--text-primary)] transition-colors"
                onClick={() => handleSort('status')}
              >
                Status <SortIcon field="status" />
              </th>
              <th 
                className="px-4 py-3 text-left text-xs font-medium text-[var(--text-muted)] uppercase tracking-wider cursor-pointer hover:text-[var(--text-primary)] transition-colors"
                onClick={() => handleSort('priority')}
              >
                Priority <SortIcon field="priority" />
              </th>
              <th className="px-4 py-3 text-left text-xs font-medium text-[var(--text-muted)] uppercase tracking-wider">
                Assignee
              </th>
              <th 
                className="px-4 py-3 text-left text-xs font-medium text-[var(--text-muted)] uppercase tracking-wider cursor-pointer hover:text-[var(--text-primary)] transition-colors"
                onClick={() => handleSort('dueAt')}
              >
                Due <SortIcon field="dueAt" />
              </th>
              <th className="px-4 py-3 text-right text-xs font-medium text-[var(--text-muted)] uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-[var(--border)]">
            {sortedTasks.map((task) => (
              <tr 
                key={task.id}
                className="hover:bg-[var(--bg-tile-hover)] cursor-pointer transition-colors"
                onClick={() => onTaskClick?.(task)}
              >
                <td className="px-4 py-4">
                  <div className="flex flex-col">
                    <span className="font-medium text-[var(--text-primary)]">{task.title}</span>
                    <span className="text-sm text-[var(--text-subtle)]">{task.location}</span>
                  </div>
                </td>
                <td className="px-4 py-4">
                  <span className={`px-2.5 py-1 text-xs rounded-full font-medium ${statusConfig[task.status].bg} ${statusConfig[task.status].text}`}>
                    {statusConfig[task.status].label}
                  </span>
                </td>
                <td className="px-4 py-4">
                  <div className="flex items-center gap-2">
                    <div className={`w-2 h-2 rounded-full ${priorityConfig[task.priority].dot}`} />
                    <span className="text-sm text-[var(--text-muted)]">
                      {priorityConfig[task.priority].label}
                    </span>
                  </div>
                </td>
                <td className="px-4 py-4">
                  {task.assignee ? (
                    <div className="flex items-center gap-2">
                      {task.assignee.avatarUrl ? (
                        <img 
                          src={task.assignee.avatarUrl} 
                          alt=""
                          className="w-6 h-6 rounded-full"
                        />
                      ) : (
                        <div className="w-6 h-6 rounded-full bg-[var(--accent-cyan)]/20 flex items-center justify-center">
                          <span className="text-xs text-[var(--accent-cyan)] font-medium">
                            {task.assignee.firstName[0]}
                          </span>
                        </div>
                      )}
                      <span className="text-sm text-[var(--text-muted)]">
                        {task.assignee.firstName} {task.assignee.lastName}
                      </span>
                    </div>
                  ) : (
                    <span className="text-sm text-[var(--text-subtle)] italic">Unassigned</span>
                  )}
                </td>
                <td className="px-4 py-4">
                  <span className={`text-sm ${isOverdue(task) ? 'text-rose-400 font-medium' : 'text-[var(--text-muted)]'}`}>
                    {formatDate(task.dueAt)}
                    {isOverdue(task) && ' (Overdue)'}
                  </span>
                </td>
                <td className="px-4 py-4 text-right">
                  <div className="flex items-center justify-end gap-2" onClick={(e) => e.stopPropagation()}>
                    {task.status === 'ready' && (
                      <button
                        onClick={() => onStartTask?.(task.id)}
                        className="flex items-center gap-1.5 px-3 py-1.5 text-xs bg-[var(--accent-emerald)] text-white rounded-lg hover:opacity-90 transition-all font-medium"
                      >
                        <Play className="w-3 h-3" />
                        Start
                      </button>
                    )}
                    {task.status === 'in_progress' && (
                      <button
                        onClick={() => onCompleteTask?.(task.id)}
                        className="flex items-center gap-1.5 px-3 py-1.5 text-xs bg-[var(--accent-cyan)] text-white rounded-lg hover:opacity-90 transition-all font-medium"
                      >
                        <CheckCircle2 className="w-3 h-3" />
                        Complete
                      </button>
                    )}
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        {sortedTasks.length === 0 && (
          <div className="text-center py-12 text-[var(--text-subtle)]">
            No tasks found
          </div>
        )}
      </div>
    </div>
  );
}
