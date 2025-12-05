'use client';

/**
 * TaskBoard Component
 * Kanban-style board view for tasks
 */

import { useMemo } from 'react';
import { Clock, AlertCircle, CheckCircle2, PlayCircle } from 'lucide-react';
import type { Task, TaskStatus, TaskPriority } from '../../types/task.types';

interface TaskBoardProps {
  tasks: Task[];
  isLoading?: boolean;
  onTaskClick?: (task: Task) => void;
  onTaskMove?: (taskId: string, newStatus: TaskStatus) => void;
}

const columns: { status: TaskStatus; title: string; icon: React.ReactNode; accentClass: string }[] = [
  { 
    status: 'ready', 
    title: 'Ready', 
    icon: <Clock className="w-4 h-4" />,
    accentClass: 'text-sky-400'
  },
  { 
    status: 'in_progress', 
    title: 'In Progress', 
    icon: <PlayCircle className="w-4 h-4" />,
    accentClass: 'text-amber-400'
  },
  { 
    status: 'blocked', 
    title: 'Blocked', 
    icon: <AlertCircle className="w-4 h-4" />,
    accentClass: 'text-rose-400'
  },
  { 
    status: 'completed', 
    title: 'Completed', 
    icon: <CheckCircle2 className="w-4 h-4" />,
    accentClass: 'text-emerald-400'
  },
];

const priorityConfig: Record<TaskPriority, { dot: string; label: string }> = {
  critical: { dot: 'bg-rose-500', label: 'Critical' },
  high: { dot: 'bg-amber-500', label: 'High' },
  normal: { dot: 'bg-emerald-500', label: 'Normal' },
  low: { dot: 'bg-slate-400', label: 'Low' },
};

function TaskCard({ 
  task, 
  onClick 
}: { 
  task: Task; 
  onClick?: () => void;
}) {
  const isOverdue = new Date(task.dueAt) < new Date() && task.status !== 'completed';
  const priorityInfo = priorityConfig[task.priority];

  return (
    <button
      type="button"
      onClick={onClick}
      className="task-card p-4 rounded-lg cursor-pointer group w-full text-left"
      aria-label={`Task: ${task.title}`}
    >
      {/* Header: Title + Priority */}
      <div className="flex items-start gap-3 mb-2">
        <div className={`w-2 h-2 rounded-full mt-1.5 flex-shrink-0 ${priorityInfo.dot}`} />
        <h4 className="font-medium text-[var(--text-primary)] text-sm leading-tight flex-1 group-hover:text-[var(--accent-cyan)] transition-colors">
          {task.title}
        </h4>
      </div>
      
      {/* Description */}
      {task.description && (
        <p className="text-xs text-[var(--text-muted)] mb-3 line-clamp-2 pl-5">
          {task.description}
        </p>
      )}
      
      {/* Footer: Location + Due Date */}
      <div className="flex items-center justify-between text-xs pl-5">
        <span className="text-[var(--text-subtle)] truncate max-w-[120px]">{task.location}</span>
        <span className={isOverdue ? 'text-rose-400 font-medium' : 'text-[var(--text-subtle)]'}>
          {new Date(task.dueAt).toLocaleDateString(undefined, { month: 'short', day: 'numeric' })}
        </span>
      </div>
      
      {/* Assignee */}
      {task.assignee && (
        <div className="flex items-center gap-2 mt-3 pt-3 border-t border-[var(--border)] pl-5">
          {task.assignee.avatarUrl ? (
            <img src={task.assignee.avatarUrl} alt="" className="w-5 h-5 rounded-full" />
          ) : (
            <div className="w-5 h-5 rounded-full bg-[var(--accent-cyan)]/20 flex items-center justify-center">
              <span className="text-[10px] text-[var(--accent-cyan)] font-medium">
                {task.assignee.firstName[0]}
              </span>
            </div>
          )}
          <span className="text-xs text-[var(--text-muted)]">
            {task.assignee.firstName}
          </span>
        </div>
      )}
    </button>
  );
}

export function TaskBoard({ tasks, isLoading, onTaskClick }: TaskBoardProps) {
  const tasksByStatus = useMemo(() => {
    const grouped: Record<TaskStatus, Task[]> = {
      draft: [],
      ready: [],
      in_progress: [],
      paused: [],
      blocked: [],
      completed: [],
      verified: [],
      closed: [],
    };
    
    tasks.forEach(task => {
      grouped[task.status].push(task);
    });
    
    // Sort by priority within each column
    const priorityOrder = { critical: 0, high: 1, normal: 2, low: 3 };
    Object.keys(grouped).forEach(status => {
      grouped[status as TaskStatus].sort((a, b) => 
        priorityOrder[a.priority] - priorityOrder[b.priority]
      );
    });
    
    return grouped;
  }, [tasks]);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-[var(--accent-cyan)]" />
      </div>
    );
  }

  return (
    <div className="flex gap-4 overflow-x-auto pb-4 scrollbar-thin">
      {columns.map(({ status, title, icon, accentClass }) => (
        <div
          key={status}
          className="flex-shrink-0 w-72"
        >
          {/* Column Header */}
          <div className="flex items-center gap-2 mb-4 px-1">
            <span className={accentClass}>{icon}</span>
            <h3 className="font-semibold text-[var(--text-primary)] text-sm">{title}</h3>
            <span className="text-xs bg-[var(--bg-tile)] px-2 py-0.5 rounded-full text-[var(--text-muted)] ml-auto">
              {tasksByStatus[status].length}
            </span>
          </div>
          
          {/* Column Content */}
          <div className="space-y-3 min-h-[200px]">
            {tasksByStatus[status].map(task => (
              <TaskCard
                key={task.id}
                task={task}
                onClick={() => onTaskClick?.(task)}
              />
            ))}
            
            {tasksByStatus[status].length === 0 && (
              <div className="text-center py-8 text-[var(--text-subtle)] text-sm rounded-lg border border-dashed border-[var(--border)]">
                No tasks
              </div>
            )}
          </div>
        </div>
      ))}
    </div>
  );
}
