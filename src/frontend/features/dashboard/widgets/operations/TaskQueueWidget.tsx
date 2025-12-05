'use client';

import React from 'react';
import { CheckCircle2, Clock, ArrowRight, UserCircle2 } from 'lucide-react';
import { cn } from '@/lib/utils';
import Link from 'next/link';

interface TaskAssignee {
  id: string;
  firstName: string;
  lastName: string;
  avatarUrl?: string;
}

interface Task {
  id: string;
  title: string;
  location: string;
  dueAt: string;
  priority: 'critical' | 'high' | 'normal';
  slaStatus: 'ok' | 'warning' | 'breached';
  assignee?: TaskAssignee;
}

const MOCK_TASKS: Task[] = [
  {
    id: '1',
    title: 'Inspect Mother Room A for PM',
    location: 'Room A (Mothers)',
    dueAt: '2023-11-26T10:00:00',
    priority: 'critical',
    slaStatus: 'breached',
    assignee: { id: 'u1', firstName: 'Marcus', lastName: 'Johnson' },
  },
  {
    id: '2',
    title: 'Transplant Batch B-203',
    location: 'Veg Room 2',
    dueAt: '2023-11-26T14:00:00',
    priority: 'high',
    slaStatus: 'warning',
    assignee: { id: 'u2', firstName: 'Sarah', lastName: 'Chen' },
  },
  {
    id: '3',
    title: 'Calibrate pH Sensors',
    location: 'Irrigation Zone 1',
    dueAt: '2023-11-27T09:00:00',
    priority: 'normal',
    slaStatus: 'ok',
    // No assignee - will show as unassigned
  },
  {
    id: '4',
    title: 'Refill Nutrient Tanks',
    location: 'Nutrient Room',
    dueAt: '2023-11-27T10:00:00',
    priority: 'normal',
    slaStatus: 'ok',
    assignee: { id: 'u3', firstName: 'David', lastName: 'Martinez' },
  },
];

function formatAssigneeName(assignee: TaskAssignee): string {
  return `${assignee.firstName} ${assignee.lastName.charAt(0)}.`;
}

function getInitials(assignee: TaskAssignee): string {
  return `${assignee.firstName.charAt(0)}${assignee.lastName.charAt(0)}`;
}

export function TaskQueueWidget() {
  // Show only first 3 tasks in compact view
  const displayTasks = MOCK_TASKS.slice(0, 3);
  const remainingCount = MOCK_TASKS.length - 3;
  const unassignedCount = MOCK_TASKS.filter(t => !t.assignee).length;

  return (
    <div className="flex flex-col">
      {/* Unassigned warning banner */}
      {unassignedCount > 0 && (
        <div className="flex items-center gap-2 px-3 py-1.5 mb-3 rounded-lg bg-gradient-to-r from-amber-500/15 to-transparent border border-amber-500/20">
          <div className="p-1 rounded bg-amber-500/15">
            <UserCircle2 className="w-2.5 h-2.5 text-amber-400" />
          </div>
          <span className="text-[10px] text-amber-300 font-medium">
            {unassignedCount} task{unassignedCount > 1 ? 's' : ''} need assignment
          </span>
        </div>
      )}

      <div className="space-y-2">
        {displayTasks.map((task) => (
          <div
            key={task.id}
            className="group flex items-center gap-2.5 p-2.5 rounded-xl bg-surface/20 hover:bg-surface/40 border border-border hover:border-border/80 cursor-pointer transition-all duration-200 hover:scale-[1.01]"
          >
            {/* Priority indicator */}
            <div
              className={cn(
                'w-2 h-2 rounded-full shrink-0',
                task.priority === 'critical' && 'bg-rose-400 shadow-[0_0_8px_rgba(251,113,133,0.5)]',
                task.priority === 'high' && 'bg-amber-400 shadow-[0_0_6px_rgba(251,191,36,0.3)]',
                task.priority === 'normal' && 'bg-cyan-400'
              )}
            />

            {/* Task details */}
            <div className="flex-1 min-w-0">
              <h4 className="text-xs font-medium text-foreground leading-tight truncate group-hover:text-cyan-300 transition-colors">
                {task.title}
              </h4>
              <div className="flex items-center gap-1.5 text-[10px] text-muted-foreground mt-0.5">
                <span className="flex items-center gap-1">
                  <Clock className="w-2.5 h-2.5 text-muted-foreground/70" />
                  {new Date(task.dueAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                </span>
                <span className="text-muted-foreground/50">•</span>
                <span className="truncate text-muted-foreground/70">{task.location}</span>
              </div>
            </div>

            {/* Assignee avatar - fixed width for alignment */}
            <div className="w-6 shrink-0 flex justify-center">
              {task.assignee ? (
                <div title={`${task.assignee.firstName} ${task.assignee.lastName}`}>
                  {task.assignee.avatarUrl ? (
                    <img 
                      src={task.assignee.avatarUrl} 
                      alt={formatAssigneeName(task.assignee)}
                      className="w-6 h-6 rounded-full object-cover ring-2 ring-border"
                    />
                  ) : (
                    <div className="w-6 h-6 rounded-full bg-gradient-to-br from-violet-500 to-fuchsia-500 flex items-center justify-center ring-2 ring-border shadow-lg shadow-violet-500/20">
                      <span className="text-[9px] font-bold text-foreground">
                        {getInitials(task.assignee)}
                      </span>
                    </div>
                  )}
                </div>
              ) : (
                <div 
                  className="w-6 h-6 rounded-full bg-muted/50 flex items-center justify-center ring-2 ring-amber-500/30"
                  title="Unassigned"
                >
                  <UserCircle2 className="w-3.5 h-3.5 text-amber-400/80" />
                </div>
              )}
            </div>
            
            {/* Complete button */}
            <button className="opacity-0 group-hover:opacity-100 p-1.5 rounded-lg hover:bg-emerald-500/20 text-muted-foreground hover:text-emerald-400 transition-all">
              <CheckCircle2 className="w-4 h-4" />
            </button>
          </div>
        ))}

        {MOCK_TASKS.length === 0 && (
          <div className="flex flex-col items-center justify-center py-6 text-center">
             <div className="p-3 rounded-full bg-emerald-500/10 mb-3">
               <CheckCircle2 className="w-6 h-6 text-emerald-400/50" />
             </div>
             <p className="text-xs text-muted-foreground">All caught up!</p>
          </div>
        )}
      </div>
      
      {remainingCount > 0 && (
        <div className="mt-3 pt-3 border-t border-border/20 flex justify-end">
          <Link 
            href="/dashboard/tasks"
            className="text-xs font-medium text-cyan-400 hover:text-cyan-300 flex items-center gap-1 transition-colors group px-2 py-1 rounded-lg hover:bg-cyan-500/10"
          >
            +{remainingCount} more <ArrowRight className="w-3 h-3 group-hover:translate-x-0.5 transition-transform" />
          </Link>
        </div>
      )}
    </div>
  );
}
