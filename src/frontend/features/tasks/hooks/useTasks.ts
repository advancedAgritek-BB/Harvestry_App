/**
 * useTasks Hook
 * React hook for task management operations
 */

import { useState, useCallback, useEffect } from 'react';
import type { Task, TaskStatus, TaskPriority } from '../types/task.types';
import * as taskService from '../services/task.service';

export interface UseTasksOptions {
  siteId: string;
  autoFetch?: boolean;
  filters?: {
    status?: TaskStatus;
    assignedTo?: string;
    priority?: TaskPriority;
  };
}

export interface UseTasksReturn {
  tasks: Task[];
  isLoading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
  createTask: (request: taskService.CreateTaskRequest) => Promise<Task>;
  updateTask: (taskId: string, request: taskService.UpdateTaskRequest) => Promise<Task>;
  assignTask: (taskId: string, request: taskService.AssignTaskRequest) => Promise<Task>;
  startTask: (taskId: string) => Promise<Task>;
  completeTask: (taskId: string) => Promise<Task>;
  cancelTask: (taskId: string, reason?: string) => Promise<Task>;
}

export function useTasks(options: UseTasksOptions): UseTasksReturn {
  const { siteId, autoFetch = true, filters = {} } = options;
  const [tasks, setTasks] = useState<Task[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const refetch = useCallback(async () => {
    if (!siteId) return;
    
    setIsLoading(true);
    setError(null);
    try {
      const response = await taskService.getTasks(siteId, filters);
      setTasks(response.tasks);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch tasks');
    } finally {
      setIsLoading(false);
    }
  }, [siteId, JSON.stringify(filters)]);

  useEffect(() => {
    if (autoFetch && siteId) {
      refetch();
    }
  }, [autoFetch, refetch, siteId]);

  const createTask = useCallback(async (request: taskService.CreateTaskRequest) => {
    const task = await taskService.createTask(siteId, request);
    setTasks(prev => [task, ...prev]);
    return task;
  }, [siteId]);

  const updateTask = useCallback(async (taskId: string, request: taskService.UpdateTaskRequest) => {
    const task = await taskService.updateTask(siteId, taskId, request);
    setTasks(prev => prev.map(t => t.id === taskId ? task : t));
    return task;
  }, [siteId]);

  const assignTask = useCallback(async (taskId: string, request: taskService.AssignTaskRequest) => {
    const task = await taskService.assignTask(siteId, taskId, request);
    setTasks(prev => prev.map(t => t.id === taskId ? task : t));
    return task;
  }, [siteId]);

  const startTask = useCallback(async (taskId: string) => {
    const task = await taskService.startTask(siteId, taskId);
    setTasks(prev => prev.map(t => t.id === taskId ? task : t));
    return task;
  }, [siteId]);

  const completeTask = useCallback(async (taskId: string) => {
    const task = await taskService.completeTask(siteId, taskId);
    setTasks(prev => prev.map(t => t.id === taskId ? task : t));
    return task;
  }, [siteId]);

  const cancelTask = useCallback(async (taskId: string, reason?: string) => {
    const task = await taskService.cancelTask(siteId, taskId, reason);
    setTasks(prev => prev.map(t => t.id === taskId ? task : t));
    return task;
  }, [siteId]);

  return {
    tasks,
    isLoading,
    error,
    refetch,
    createTask,
    updateTask,
    assignTask,
    startTask,
    completeTask,
    cancelTask,
  };
}

