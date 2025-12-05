'use client';

/**
 * All Tasks Page
 * Shows all tasks across the site with advanced filtering
 */

import { useState } from 'react';
import { Plus, Filter, Download } from 'lucide-react';
import { TaskList } from '@/features/tasks/components';
import type { Task, TaskStatus } from '@/features/tasks/types/task.types';

// Extended mock data showing tasks from all users
const ALL_TASKS: Task[] = [
  {
    id: '1',
    title: 'Inspect Mother Room A for PM',
    location: 'Room A (Mothers)',
    dueAt: new Date(Date.now() - 2 * 60 * 60 * 1000).toISOString(),
    priority: 'critical',
    status: 'ready',
    slaStatus: 'breached',
    assignee: { id: 'u2', firstName: 'Sarah', lastName: 'Chen' },
  },
  {
    id: '2',
    title: 'Transplant Batch B-203',
    location: 'Veg Room 2',
    dueAt: new Date(Date.now() + 4 * 60 * 60 * 1000).toISOString(),
    priority: 'high',
    status: 'in_progress',
    slaStatus: 'warning',
    assignee: { id: 'u3', firstName: 'Marcus', lastName: 'Johnson' },
  },
  {
    id: '3',
    title: 'Calibrate pH Sensors',
    location: 'Irrigation Zone 1',
    dueAt: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString(),
    priority: 'normal',
    status: 'ready',
    slaStatus: 'ok',
    // Unassigned
  },
  {
    id: '4',
    title: 'Week 3 Defoliation - F1',
    location: 'Flower Room 1',
    dueAt: new Date(Date.now() + 2 * 24 * 60 * 60 * 1000).toISOString(),
    priority: 'high',
    status: 'blocked',
    slaStatus: 'ok',
    assignee: { id: 'u1', firstName: 'Brandon', lastName: 'Burnette' },
  },
  {
    id: '5',
    title: 'Nutrient Tank Refill',
    location: 'Nutrient Room',
    dueAt: new Date(Date.now() + 1 * 60 * 60 * 1000).toISOString(),
    priority: 'high',
    status: 'completed',
    slaStatus: 'ok',
    assignee: { id: 'u3', firstName: 'Marcus', lastName: 'Johnson' },
  },
  {
    id: '6',
    title: 'Clone Room Humidity Check',
    location: 'Clone Room',
    dueAt: new Date(Date.now() + 6 * 60 * 60 * 1000).toISOString(),
    priority: 'normal',
    status: 'ready',
    slaStatus: 'ok',
    assignee: { id: 'u2', firstName: 'Sarah', lastName: 'Chen' },
  },
];

export default function AllTasksPage() {
  const [tasks] = useState<Task[]>(ALL_TASKS);
  const [statusFilter, setStatusFilter] = useState<TaskStatus | 'all'>('all');
  const [assigneeFilter, setAssigneeFilter] = useState<string>('all');

  const filteredTasks = tasks.filter(task => {
    if (statusFilter !== 'all' && task.status !== statusFilter) return false;
    if (assigneeFilter === 'unassigned' && task.assignee) return false;
    if (assigneeFilter !== 'all' && assigneeFilter !== 'unassigned' && task.assignee?.id !== assigneeFilter) return false;
    return true;
  });

  const uniqueAssignees = Array.from(
    new Map(
      tasks
        .filter(t => t.assignee)
        .map(t => [t.assignee!.id, t.assignee!])
    ).values()
  );

  return (
    <div className="space-y-6">
      {/* Filters Bar */}
      <div className="flex items-center gap-4 p-4 tile-premium rounded-lg">
        <Filter className="w-4 h-4 text-[var(--text-muted)]" />
        
        <div className="flex items-center gap-2">
          <label className="text-sm text-[var(--text-muted)]">Status:</label>
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value as TaskStatus | 'all')}
            className="text-sm rounded-md border border-[var(--border)] bg-[var(--bg-elevated)] text-[var(--text-primary)] px-2 py-1 focus:outline-none focus:border-[var(--accent-cyan)]"
          >
            <option value="all">All</option>
            <option value="ready">Ready</option>
            <option value="in_progress">In Progress</option>
            <option value="blocked">Blocked</option>
            <option value="completed">Completed</option>
          </select>
        </div>

        <div className="flex items-center gap-2">
          <label className="text-sm text-[var(--text-muted)]">Assignee:</label>
          <select
            value={assigneeFilter}
            onChange={(e) => setAssigneeFilter(e.target.value)}
            className="text-sm rounded-md border border-[var(--border)] bg-[var(--bg-elevated)] text-[var(--text-primary)] px-2 py-1 focus:outline-none focus:border-[var(--accent-cyan)]"
          >
            <option value="all">All</option>
            <option value="unassigned">Unassigned</option>
            {uniqueAssignees.map(assignee => (
              <option key={assignee.id} value={assignee.id}>
                {assignee.firstName} {assignee.lastName}
              </option>
            ))}
          </select>
        </div>

        <div className="flex-1" />

        <button className="flex items-center gap-2 px-3 py-1.5 text-sm text-[var(--text-muted)] hover:text-[var(--text-primary)] border border-[var(--border)] rounded-md hover:bg-[var(--bg-tile)] transition-colors">
          <Download className="w-4 h-4" />
          Export
        </button>

        <button className="flex items-center gap-2 px-4 py-2 bg-[var(--accent-cyan)] text-white rounded-lg hover:opacity-90 transition-all font-medium text-sm">
          <Plus className="w-4 h-4" />
          New Task
        </button>
      </div>

      {/* Results Count */}
      <p className="text-sm text-[var(--text-muted)]">
        Showing {filteredTasks.length} of {tasks.length} tasks
      </p>

      {/* Task List */}
      <TaskList
        tasks={filteredTasks}
        onTaskClick={(task) => console.log('Task clicked:', task)}
      />
    </div>
  );
}
