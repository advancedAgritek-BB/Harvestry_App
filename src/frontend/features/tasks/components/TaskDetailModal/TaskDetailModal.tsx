'use client';

/**
 * TaskDetailModal Component
 * Modal for viewing task details, SOPs, and communication log
 */

import { useState } from 'react';
import { 
  X, Clock, AlertCircle, CheckCircle2, PlayCircle, 
  Pause, FileText, MessageSquare, User, MapPin,
  Calendar, Flag, Send, AlertTriangle
} from 'lucide-react';
import type { Task, TaskStatus, TaskPriority } from '../../types/task.types';
import type { StandardOperatingProcedure, SopProgress } from '../../types/sop.types';
import { SopViewer } from '../SopViewer';

interface TaskComment {
  id: string;
  userId: string;
  userName: string;
  userAvatar?: string;
  content: string;
  createdAt: string;
}

interface TaskDetailModalProps {
  isOpen: boolean;
  onClose: () => void;
  task: Task | null;
  sops?: StandardOperatingProcedure[];
  sopProgress?: Record<string, SopProgress>;
  comments?: TaskComment[];
  onStartTask?: (taskId: string) => Promise<void>;
  onCompleteTask?: (taskId: string) => Promise<void>;
  onBlockTask?: (taskId: string, reason: string) => Promise<void>;
  onUnblockTask?: (taskId: string) => Promise<void>;
  onAddComment?: (taskId: string, content: string) => Promise<void>;
  onSopStepComplete?: (sopId: string, stepId: string) => void;
  onSopSubStepComplete?: (sopId: string, stepId: string, subStepId: string) => void;
}

const statusConfig: Record<TaskStatus, { icon: React.ReactNode; label: string; color: string }> = {
  draft: { icon: <FileText className="w-4 h-4" />, label: 'Draft', color: 'text-slate-400' },
  ready: { icon: <Clock className="w-4 h-4" />, label: 'Ready', color: 'text-sky-400' },
  in_progress: { icon: <PlayCircle className="w-4 h-4" />, label: 'In Progress', color: 'text-amber-400' },
  paused: { icon: <Pause className="w-4 h-4" />, label: 'Paused', color: 'text-orange-400' },
  blocked: { icon: <AlertCircle className="w-4 h-4" />, label: 'Blocked', color: 'text-rose-400' },
  completed: { icon: <CheckCircle2 className="w-4 h-4" />, label: 'Completed', color: 'text-emerald-400' },
  verified: { icon: <CheckCircle2 className="w-4 h-4" />, label: 'Verified', color: 'text-cyan-400' },
  closed: { icon: <CheckCircle2 className="w-4 h-4" />, label: 'Closed', color: 'text-slate-500' },
};

const priorityConfig: Record<TaskPriority, { label: string; color: string; bg: string }> = {
  critical: { label: 'Critical', color: 'text-rose-400', bg: 'bg-rose-500/10' },
  high: { label: 'High', color: 'text-amber-400', bg: 'bg-amber-500/10' },
  normal: { label: 'Normal', color: 'text-emerald-400', bg: 'bg-emerald-500/10' },
  low: { label: 'Low', color: 'text-slate-400', bg: 'bg-slate-500/10' },
};

type TabType = 'details' | 'sops' | 'activity';

export function TaskDetailModal({
  isOpen,
  onClose,
  task,
  sops = [],
  sopProgress = {},
  comments = [],
  onStartTask,
  onCompleteTask,
  onBlockTask,
  onUnblockTask,
  onAddComment,
  onSopStepComplete,
  onSopSubStepComplete,
}: TaskDetailModalProps) {
  const [activeTab, setActiveTab] = useState<TabType>('details');
  const [newComment, setNewComment] = useState('');
  const [selectedSopId, setSelectedSopId] = useState<string | null>(null);
  const [blockReason, setBlockReason] = useState('');
  const [showBlockInput, setShowBlockInput] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  if (!isOpen || !task) return null;

  const statusInfo = statusConfig[task.status];
  const priorityInfo = priorityConfig[task.priority];
  const isOverdue = new Date(task.dueAt) < new Date() && task.status !== 'completed';

  const handleAction = async (action: () => Promise<void>) => {
    setIsSubmitting(true);
    try {
      await action();
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleAddComment = async () => {
    if (!newComment.trim() || !onAddComment) return;
    setIsSubmitting(true);
    try {
      await onAddComment(task.id, newComment);
      setNewComment('');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleBlock = async () => {
    if (!blockReason.trim() || !onBlockTask) return;
    setIsSubmitting(true);
    try {
      await onBlockTask(task.id, blockReason);
      setBlockReason('');
      setShowBlockInput(false);
    } finally {
      setIsSubmitting(false);
    }
  };

  const tabs = [
    { id: 'details' as const, label: 'Details', icon: <FileText className="w-4 h-4" /> },
    { id: 'sops' as const, label: `SOPs (${sops.length})`, icon: <FileText className="w-4 h-4" /> },
    { id: 'activity' as const, label: `Activity (${comments.length})`, icon: <MessageSquare className="w-4 h-4" /> },
  ];

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      {/* Backdrop */}
      <div 
        className="absolute inset-0 bg-black/60 backdrop-blur-sm"
        onClick={onClose}
      />
      
      {/* Modal */}
      <div className="relative task-detail-modal rounded-xl shadow-2xl w-full max-w-3xl max-h-[90vh] overflow-hidden flex flex-col">
        {/* Header */}
        <div className="flex items-start justify-between px-6 py-4 border-b border-[var(--border)]">
          <div className="flex-1 pr-4">
            <div className="flex items-center gap-3 mb-2">
              <span className={`${statusInfo.color}`}>{statusInfo.icon}</span>
              <span className={`text-xs font-medium px-2 py-0.5 rounded-full ${priorityInfo.bg} ${priorityInfo.color}`}>
                {priorityInfo.label}
              </span>
              {isOverdue && (
                <span className="flex items-center gap-1 text-xs font-medium text-rose-400">
                  <AlertTriangle className="w-3 h-3" />
                  Overdue
                </span>
              )}
            </div>
            <h2 className="text-xl font-semibold text-[var(--text-primary)]">{task.title}</h2>
          </div>
          <button
            onClick={onClose}
            className="text-[var(--text-muted)] hover:text-[var(--text-primary)] transition-colors p-1"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Tabs */}
        <div className="flex border-b border-[var(--border)] px-6">
          {tabs.map(tab => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={`flex items-center gap-2 px-4 py-3 text-sm font-medium border-b-2 transition-colors ${
                activeTab === tab.id
                  ? 'border-[var(--accent-cyan)] text-[var(--accent-cyan)]'
                  : 'border-transparent text-[var(--text-muted)] hover:text-[var(--text-primary)]'
              }`}
            >
              {tab.icon}
              {tab.label}
            </button>
          ))}
        </div>

        {/* Content */}
        <div className="flex-1 overflow-y-auto p-6">
          {activeTab === 'details' && (
            <DetailsTab task={task} isOverdue={isOverdue} />
          )}
          
          {activeTab === 'sops' && (
            <SopsTab 
              sops={sops}
              sopProgress={sopProgress}
              taskId={task.id}
              selectedSopId={selectedSopId}
              onSelectSop={setSelectedSopId}
              onStepComplete={onSopStepComplete}
              onSubStepComplete={onSopSubStepComplete}
              isTaskInProgress={task.status === 'in_progress'}
            />
          )}
          
          {activeTab === 'activity' && (
            <ActivityTab 
              comments={comments}
              newComment={newComment}
              onNewCommentChange={setNewComment}
              onAddComment={handleAddComment}
              isSubmitting={isSubmitting}
            />
          )}
        </div>

        {/* Footer Actions */}
        <div className="border-t border-[var(--border)] px-6 py-4 bg-[var(--bg-surface)]">
          {showBlockInput ? (
            <div className="flex gap-3">
              <input
                type="text"
                value={blockReason}
                onChange={(e) => setBlockReason(e.target.value)}
                placeholder="Enter reason for blocking..."
                className="flex-1 rounded-lg border border-[var(--border)] bg-[var(--bg-elevated)] text-[var(--text-primary)] text-sm px-3 py-2 focus:outline-none focus:border-[var(--accent-cyan)]"
              />
              <button
                onClick={handleBlock}
                disabled={!blockReason.trim() || isSubmitting}
                className="px-4 py-2 text-sm font-medium text-white bg-rose-500 rounded-lg hover:bg-rose-600 disabled:opacity-50 transition-colors"
              >
                Confirm Block
              </button>
              <button
                onClick={() => { setShowBlockInput(false); setBlockReason(''); }}
                className="px-4 py-2 text-sm font-medium text-[var(--text-muted)] hover:text-[var(--text-primary)] transition-colors"
              >
                Cancel
              </button>
            </div>
          ) : (
            <div className="flex items-center justify-between">
              <div className="text-sm text-[var(--text-muted)]">
                Status: <span className={`font-medium ${statusInfo.color}`}>{statusInfo.label}</span>
              </div>
              <div className="flex gap-3">
                {task.status === 'ready' && onStartTask && (
                  <button
                    onClick={() => handleAction(() => onStartTask(task.id))}
                    disabled={isSubmitting}
                    className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-white bg-[var(--accent-emerald)] rounded-lg hover:opacity-90 disabled:opacity-50 transition-all"
                  >
                    <PlayCircle className="w-4 h-4" />
                    Start Task
                  </button>
                )}
                {task.status === 'in_progress' && (
                  <>
                    {onBlockTask && (
                      <button
                        onClick={() => setShowBlockInput(true)}
                        disabled={isSubmitting}
                        className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-rose-400 border border-rose-400/30 rounded-lg hover:bg-rose-500/10 disabled:opacity-50 transition-all"
                      >
                        <AlertCircle className="w-4 h-4" />
                        Block
                      </button>
                    )}
                    {onCompleteTask && (
                      <button
                        onClick={() => handleAction(() => onCompleteTask(task.id))}
                        disabled={isSubmitting}
                        className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-white bg-[var(--accent-cyan)] rounded-lg hover:opacity-90 disabled:opacity-50 transition-all"
                      >
                        <CheckCircle2 className="w-4 h-4" />
                        Complete
                      </button>
                    )}
                  </>
                )}
                {task.status === 'blocked' && onUnblockTask && (
                  <button
                    onClick={() => handleAction(() => onUnblockTask(task.id))}
                    disabled={isSubmitting}
                    className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-white bg-amber-500 rounded-lg hover:bg-amber-600 disabled:opacity-50 transition-all"
                  >
                    <PlayCircle className="w-4 h-4" />
                    Unblock
                  </button>
                )}
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

function DetailsTab({ task, isOverdue }: { task: Task; isOverdue: boolean }) {
  return (
    <div className="space-y-6">
      {/* Description */}
      {task.description && (
        <div>
          <h3 className="text-sm font-medium text-[var(--text-muted)] mb-2">Description</h3>
          <p className="text-[var(--text-primary)] text-sm leading-relaxed">{task.description}</p>
        </div>
      )}

      {/* Metadata Grid */}
      <div className="grid grid-cols-2 gap-4">
        <div className="task-card p-4 rounded-lg">
          <div className="flex items-center gap-2 text-[var(--text-muted)] mb-1">
            <MapPin className="w-4 h-4" />
            <span className="text-xs font-medium uppercase">Location</span>
          </div>
          <p className="text-[var(--text-primary)] font-medium">{task.location}</p>
        </div>

        <div className="task-card p-4 rounded-lg">
          <div className="flex items-center gap-2 text-[var(--text-muted)] mb-1">
            <Calendar className="w-4 h-4" />
            <span className="text-xs font-medium uppercase">Due Date</span>
          </div>
          <p className={`font-medium ${isOverdue ? 'text-rose-400' : 'text-[var(--text-primary)]'}`}>
            {new Date(task.dueAt).toLocaleDateString(undefined, { 
              weekday: 'short', 
              month: 'short', 
              day: 'numeric',
              hour: '2-digit',
              minute: '2-digit'
            })}
            {isOverdue && ' (Overdue)'}
          </p>
        </div>

        <div className="task-card p-4 rounded-lg">
          <div className="flex items-center gap-2 text-[var(--text-muted)] mb-1">
            <User className="w-4 h-4" />
            <span className="text-xs font-medium uppercase">Assignee</span>
          </div>
          {task.assignee ? (
            <div className="flex items-center gap-2">
              {task.assignee.avatarUrl ? (
                <img src={task.assignee.avatarUrl} alt="" className="w-6 h-6 rounded-full" />
              ) : (
                <div className="w-6 h-6 rounded-full bg-[var(--accent-cyan)]/20 flex items-center justify-center">
                  <span className="text-xs text-[var(--accent-cyan)] font-medium">
                    {task.assignee.firstName[0]}
                  </span>
                </div>
              )}
              <span className="text-[var(--text-primary)] font-medium">
                {task.assignee.firstName} {task.assignee.lastName}
              </span>
            </div>
          ) : (
            <p className="text-[var(--text-subtle)] italic">Unassigned</p>
          )}
        </div>

        <div className="task-card p-4 rounded-lg">
          <div className="flex items-center gap-2 text-[var(--text-muted)] mb-1">
            <Flag className="w-4 h-4" />
            <span className="text-xs font-medium uppercase">Phase</span>
          </div>
          <p className="text-[var(--text-primary)] font-medium capitalize">
            {task.phase || 'Not specified'}
          </p>
        </div>
      </div>

      {/* Timeline */}
      {(task.startedAt || task.completedAt) && (
        <div>
          <h3 className="text-sm font-medium text-[var(--text-muted)] mb-3">Timeline</h3>
          <div className="space-y-2">
            {task.startedAt && (
              <div className="flex items-center gap-3 text-sm">
                <div className="w-2 h-2 rounded-full bg-amber-400" />
                <span className="text-[var(--text-muted)]">Started:</span>
                <span className="text-[var(--text-primary)]">
                  {new Date(task.startedAt).toLocaleString()}
                </span>
              </div>
            )}
            {task.completedAt && (
              <div className="flex items-center gap-3 text-sm">
                <div className="w-2 h-2 rounded-full bg-emerald-400" />
                <span className="text-[var(--text-muted)]">Completed:</span>
                <span className="text-[var(--text-primary)]">
                  {new Date(task.completedAt).toLocaleString()}
                </span>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

interface SopsTabProps {
  sops: StandardOperatingProcedure[];
  sopProgress: Record<string, SopProgress>;
  taskId: string;
  selectedSopId: string | null;
  onSelectSop: (id: string | null) => void;
  onStepComplete?: (sopId: string, stepId: string) => void;
  onSubStepComplete?: (sopId: string, stepId: string, subStepId: string) => void;
  isTaskInProgress: boolean;
}

function SopsTab({ 
  sops,
  sopProgress,
  taskId,
  selectedSopId,
  onSelectSop,
  onStepComplete,
  onSubStepComplete,
  isTaskInProgress,
}: SopsTabProps) {
  if (sops.length === 0) {
    return (
      <div className="text-center py-12">
        <FileText className="w-12 h-12 text-[var(--text-subtle)] mx-auto mb-3" />
        <p className="text-[var(--text-muted)]">No SOPs attached to this task</p>
      </div>
    );
  }

  const selectedSop = sops.find(s => s.id === selectedSopId);

  // If an SOP is selected, show the full SOP viewer
  if (selectedSop) {
    return (
      <div className="space-y-4">
        <button
          onClick={() => onSelectSop(null)}
          className="flex items-center gap-2 text-sm text-[var(--accent-cyan)] hover:underline"
        >
          ← Back to SOP list
        </button>
        <SopViewer
          sop={selectedSop}
          progress={sopProgress[selectedSop.id]}
          onStepComplete={onStepComplete ? (stepId) => onStepComplete(selectedSop.id, stepId) : undefined}
          onSubStepComplete={onSubStepComplete ? (stepId, subStepId) => onSubStepComplete(selectedSop.id, stepId, subStepId) : undefined}
          readOnly={!isTaskInProgress}
        />
      </div>
    );
  }

  // Show SOP list
  return (
    <div className="space-y-3">
      {sops.map(sop => {
        const progress = sopProgress[sop.id];
        const hasSteps = sop.steps && sop.steps.length > 0;
        const stepCount = sop.steps?.length || 0;
        const completedCount = progress?.completedStepIds?.length || 0;
        const progressPercent = hasSteps ? Math.round((completedCount / stepCount) * 100) : 0;

        return (
          <button
            key={sop.id}
            onClick={() => onSelectSop(sop.id)}
            className="w-full task-card rounded-lg p-4 text-left hover:bg-[var(--bg-tile-hover)] transition-colors"
          >
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-lg bg-[var(--accent-cyan)]/10 flex items-center justify-center flex-shrink-0">
                <FileText className="w-5 h-5 text-[var(--accent-cyan)]" />
              </div>
              <div className="flex-1 min-w-0">
                <h4 className="font-medium text-[var(--text-primary)]">{sop.title}</h4>
                <div className="flex items-center gap-3 mt-1">
                  <span className="text-xs text-[var(--text-muted)]">
                    v{sop.version} • {sop.category || 'General'}
                  </span>
                  {hasSteps && (
                    <>
                      <span className="text-xs text-[var(--text-subtle)]">•</span>
                      <span className="text-xs text-[var(--text-muted)]">
                        {stepCount} steps
                      </span>
                    </>
                  )}
                  {sop.estimatedTotalMinutes && (
                    <>
                      <span className="text-xs text-[var(--text-subtle)]">•</span>
                      <span className="text-xs text-[var(--text-muted)]">
                        ~{sop.estimatedTotalMinutes} min
                      </span>
                    </>
                  )}
                </div>
              </div>
              {hasSteps && (
                <div className="flex items-center gap-2">
                  <div className="w-16 h-1.5 bg-[var(--bg-tile)] rounded-full overflow-hidden">
                    <div 
                      className="h-full bg-[var(--accent-emerald)] transition-all duration-300"
                      style={{ width: `${progressPercent}%` }}
                    />
                  </div>
                  <span className="text-xs font-medium text-[var(--text-muted)] w-8">
                    {progressPercent}%
                  </span>
                </div>
              )}
            </div>
          </button>
        );
      })}
    </div>
  );
}

function ActivityTab({
  comments,
  newComment,
  onNewCommentChange,
  onAddComment,
  isSubmitting,
}: {
  comments: TaskComment[];
  newComment: string;
  onNewCommentChange: (value: string) => void;
  onAddComment: () => void;
  isSubmitting: boolean;
}) {
  return (
    <div className="flex flex-col h-full">
      {/* Comments List */}
      <div className="flex-1 space-y-4 mb-4">
        {comments.length === 0 ? (
          <div className="text-center py-12">
            <MessageSquare className="w-12 h-12 text-[var(--text-subtle)] mx-auto mb-3" />
            <p className="text-[var(--text-muted)]">No comments yet</p>
            <p className="text-sm text-[var(--text-subtle)]">Be the first to add a comment</p>
          </div>
        ) : (
          comments.map(comment => (
            <div key={comment.id} className="flex gap-3">
              {comment.userAvatar ? (
                <img src={comment.userAvatar} alt="" className="w-8 h-8 rounded-full" />
              ) : (
                <div className="w-8 h-8 rounded-full bg-[var(--accent-cyan)]/20 flex items-center justify-center flex-shrink-0">
                  <span className="text-xs text-[var(--accent-cyan)] font-medium">
                    {comment.userName[0]}
                  </span>
                </div>
              )}
              <div className="flex-1">
                <div className="flex items-baseline gap-2 mb-1">
                  <span className="font-medium text-[var(--text-primary)] text-sm">{comment.userName}</span>
                  <span className="text-xs text-[var(--text-subtle)]">
                    {new Date(comment.createdAt).toLocaleString()}
                  </span>
                </div>
                <p className="text-sm text-[var(--text-muted)] leading-relaxed">{comment.content}</p>
              </div>
            </div>
          ))
        )}
      </div>

      {/* Comment Input */}
      <div className="flex gap-3 pt-4 border-t border-[var(--border)]">
        <input
          type="text"
          value={newComment}
          onChange={(e) => onNewCommentChange(e.target.value)}
          placeholder="Add a comment..."
          className="flex-1 rounded-lg border border-[var(--border)] bg-[var(--bg-elevated)] text-[var(--text-primary)] text-sm px-4 py-2.5 focus:outline-none focus:border-[var(--accent-cyan)]"
          onKeyDown={(e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
              e.preventDefault();
              onAddComment();
            }
          }}
        />
        <button
          onClick={onAddComment}
          disabled={!newComment.trim() || isSubmitting}
          className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-white bg-[var(--accent-cyan)] rounded-lg hover:opacity-90 disabled:opacity-50 transition-all"
        >
          <Send className="w-4 h-4" />
          Send
        </button>
      </div>
    </div>
  );
}

