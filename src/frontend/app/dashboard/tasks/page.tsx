'use client';

/**
 * My Tasks Page
 * Shows tasks assigned to the current user with board and list views
 */

import { useState, useCallback, useEffect } from 'react';
import { useSearchParams, useRouter } from 'next/navigation';
import { Plus, LayoutGrid, List } from 'lucide-react';
import { TaskList, TaskBoard, CreateTaskModal, TaskDetailModal } from '@/features/tasks/components';
import type { Task, TaskStatus } from '@/features/tasks/types/task.types';
import type { SopProgress } from '@/features/tasks/types/sop.types';
import { 
  MOCK_TASKS, 
  MOCK_SOPS, 
  MOCK_COMMENTS, 
  INITIAL_SOP_PROGRESS,
  type TaskComment 
} from './mockData';

type ViewMode = 'board' | 'list';

export default function MyTasksPage() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const taskIdFromUrl = searchParams.get('taskId');

  const [viewMode, setViewMode] = useState<ViewMode>('board');
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [selectedTask, setSelectedTask] = useState<Task | null>(null);
  const [tasks, setTasks] = useState<Task[]>(MOCK_TASKS);
  const [comments, setComments] = useState<Record<string, TaskComment[]>>(MOCK_COMMENTS);
  const [sopProgress, setSopProgress] = useState<Record<string, SopProgress>>(INITIAL_SOP_PROGRESS);

  // Handle opening task from URL parameter (e.g., from notification click)
  useEffect(() => {
    if (taskIdFromUrl) {
      const taskToOpen = tasks.find(t => t.id === taskIdFromUrl);
      if (taskToOpen) {
        setSelectedTask(taskToOpen);
      }
    }
  }, [taskIdFromUrl, tasks]);

  const handleTaskClick = (task: Task) => {
    setSelectedTask(task);
    // Update URL to reflect the selected task (for bookmarking/sharing)
    router.push(`/dashboard/tasks?taskId=${task.id}`, { scroll: false });
  };

  const handleCloseDetailModal = () => {
    setSelectedTask(null);
    // Clear the taskId from URL when closing modal
    router.push('/dashboard/tasks', { scroll: false });
  };

  const handleStartTask = async (taskId: string) => {
    setTasks(prev => 
      prev.map(t => t.id === taskId ? { 
        ...t, 
        status: 'in_progress' as TaskStatus,
        startedAt: new Date().toISOString()
      } : t)
    );
    if (selectedTask?.id === taskId) {
      setSelectedTask(prev => prev ? { 
        ...prev, 
        status: 'in_progress' as TaskStatus,
        startedAt: new Date().toISOString()
      } : null);
    }
  };

  const handleCompleteTask = async (taskId: string) => {
    setTasks(prev => 
      prev.map(t => t.id === taskId ? { 
        ...t, 
        status: 'completed' as TaskStatus,
        completedAt: new Date().toISOString()
      } : t)
    );
    if (selectedTask?.id === taskId) {
      setSelectedTask(prev => prev ? { 
        ...prev, 
        status: 'completed' as TaskStatus,
        completedAt: new Date().toISOString()
      } : null);
    }
  };

  const handleBlockTask = async (taskId: string, reason: string) => {
    setTasks(prev => 
      prev.map(t => t.id === taskId ? { ...t, status: 'blocked' as TaskStatus } : t)
    );
    setComments(prev => ({
      ...prev,
      [taskId]: [
        ...(prev[taskId] || []),
        {
          id: `c${Date.now()}`,
          userId: 'u1',
          userName: 'Brandon Burnette',
          content: `🚫 Task blocked: ${reason}`,
          createdAt: new Date().toISOString(),
        }
      ]
    }));
    if (selectedTask?.id === taskId) {
      setSelectedTask(prev => prev ? { ...prev, status: 'blocked' as TaskStatus } : null);
    }
  };

  const handleUnblockTask = async (taskId: string) => {
    setTasks(prev => 
      prev.map(t => t.id === taskId ? { ...t, status: 'in_progress' as TaskStatus } : t)
    );
    setComments(prev => ({
      ...prev,
      [taskId]: [
        ...(prev[taskId] || []),
        {
          id: `c${Date.now()}`,
          userId: 'u1',
          userName: 'Brandon Burnette',
          content: '✅ Task unblocked and resumed',
          createdAt: new Date().toISOString(),
        }
      ]
    }));
    if (selectedTask?.id === taskId) {
      setSelectedTask(prev => prev ? { ...prev, status: 'in_progress' as TaskStatus } : null);
    }
  };

  const handleAddComment = async (taskId: string, content: string) => {
    setComments(prev => ({
      ...prev,
      [taskId]: [
        ...(prev[taskId] || []),
        {
          id: `c${Date.now()}`,
          userId: 'u1',
          userName: 'Brandon Burnette',
          content,
          createdAt: new Date().toISOString(),
        }
      ]
    }));
  };

  const handleSopStepComplete = useCallback((sopId: string, stepId: string) => {
    setSopProgress(prev => {
      const existing = prev[sopId] || {
        sopId,
        taskId: selectedTask?.id || '',
        completedStepIds: [],
        completedSubStepIds: [],
      };
      
      const isCompleted = existing.completedStepIds.includes(stepId);
      
      return {
        ...prev,
        [sopId]: {
          ...existing,
          completedStepIds: isCompleted
            ? existing.completedStepIds.filter(id => id !== stepId)
            : [...existing.completedStepIds, stepId],
        }
      };
    });
  }, [selectedTask?.id]);

  const handleSopSubStepComplete = useCallback((sopId: string, _stepId: string, subStepId: string) => {
    setSopProgress(prev => {
      const existing = prev[sopId] || {
        sopId,
        taskId: selectedTask?.id || '',
        completedStepIds: [],
        completedSubStepIds: [],
      };
      
      const isCompleted = existing.completedSubStepIds.includes(subStepId);
      
      return {
        ...prev,
        [sopId]: {
          ...existing,
          completedSubStepIds: isCompleted
            ? existing.completedSubStepIds.filter(id => id !== subStepId)
            : [...existing.completedSubStepIds, subStepId],
        }
      };
    });
  }, [selectedTask?.id]);

  const handleCreateTask = async (data: unknown) => {
    console.log('Create task:', data);
    setIsCreateModalOpen(false);
  };

  const pendingCount = tasks.filter(t => t.status === 'ready').length;
  const inProgressCount = tasks.filter(t => t.status === 'in_progress').length;
  const blockedCount = tasks.filter(t => t.status === 'blocked').length;

  return (
    <div className="space-y-6 p-6">
      {/* Stats Bar */}
      <div className="flex items-center gap-6">
        <div className="flex items-center gap-2">
          <div className="w-2 h-2 rounded-full bg-sky-400" />
          <span className="text-sm text-[var(--text-muted)]">
            <span className="font-semibold text-[var(--text-primary)]">{pendingCount}</span> Ready
          </span>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-2 h-2 rounded-full bg-amber-400" />
          <span className="text-sm text-[var(--text-muted)]">
            <span className="font-semibold text-[var(--text-primary)]">{inProgressCount}</span> In Progress
          </span>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-2 h-2 rounded-full bg-rose-400" />
          <span className="text-sm text-[var(--text-muted)]">
            <span className="font-semibold text-[var(--text-primary)]">{blockedCount}</span> Blocked
          </span>
        </div>

        <div className="flex-1" />

        {/* View Toggle */}
        <div className="flex items-center bg-[var(--bg-tile)] rounded-lg p-1">
          <button
            onClick={() => setViewMode('board')}
            className={`p-2 rounded-md transition-colors ${
              viewMode === 'board' 
                ? 'bg-[var(--bg-surface)] shadow-sm text-[var(--accent-cyan)]' 
                : 'text-[var(--text-muted)] hover:text-[var(--text-primary)] hover:bg-[var(--bg-tile-hover)]'
            }`}
            title="Board View"
          >
            <LayoutGrid className="w-4 h-4" />
          </button>
          <button
            onClick={() => setViewMode('list')}
            className={`p-2 rounded-md transition-colors ${
              viewMode === 'list' 
                ? 'bg-[var(--bg-surface)] shadow-sm text-[var(--accent-cyan)]' 
                : 'text-[var(--text-muted)] hover:text-[var(--text-primary)] hover:bg-[var(--bg-tile-hover)]'
            }`}
            title="List View"
          >
            <List className="w-4 h-4" />
          </button>
        </div>

        {/* Create Button */}
        <button
          onClick={() => setIsCreateModalOpen(true)}
          className="flex items-center gap-2 px-4 py-2 bg-[var(--accent-cyan)] text-white rounded-lg hover:opacity-90 transition-all font-medium"
        >
          <Plus className="w-4 h-4" />
          New Task
        </button>
      </div>

      {/* Task Views */}
      {viewMode === 'board' ? (
        <TaskBoard
          tasks={tasks}
          onTaskClick={handleTaskClick}
        />
      ) : (
        <TaskList
          tasks={tasks}
          onTaskClick={handleTaskClick}
          onStartTask={handleStartTask}
          onCompleteTask={handleCompleteTask}
        />
      )}

      {/* Create Task Modal */}
      <CreateTaskModal
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
        onSubmit={handleCreateTask}
        templates={[]}
        sops={[]}
        assignees={[
          { id: 'u1', firstName: 'Brandon', lastName: 'Burnette', role: 'Admin' },
          { id: 'u2', firstName: 'Sarah', lastName: 'Chen', role: 'Cultivator' },
          { id: 'u3', firstName: 'Marcus', lastName: 'Johnson', role: 'Cultivator' },
        ]}
      />

      {/* Task Detail Modal */}
      <TaskDetailModal
        isOpen={selectedTask !== null}
        onClose={handleCloseDetailModal}
        task={selectedTask}
        sops={selectedTask ? MOCK_SOPS[selectedTask.id] || [] : []}
        sopProgress={sopProgress}
        comments={selectedTask ? comments[selectedTask.id] || [] : []}
        onStartTask={handleStartTask}
        onCompleteTask={handleCompleteTask}
        onBlockTask={handleBlockTask}
        onUnblockTask={handleUnblockTask}
        onAddComment={handleAddComment}
        onSopStepComplete={handleSopStepComplete}
        onSopSubStepComplete={handleSopSubStepComplete}
      />
    </div>
  );
}
