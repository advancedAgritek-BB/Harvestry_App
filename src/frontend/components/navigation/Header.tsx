'use client';

import React, { useState } from 'react';
import { Command, ChevronDown, Bell, CheckCircle2, AlertTriangle, Zap, MessageSquare } from 'lucide-react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';

// Notification types and mock data
interface TaskNotification {
  id: string;
  type: 'task_assigned' | 'task_overdue' | 'task_comment' | 'blueprint_triggered' | 'task_completed';
  title: string;
  message: string;
  taskId?: string;
  timestamp: string;
  isRead: boolean;
}

const MOCK_NOTIFICATIONS: TaskNotification[] = [
  {
    id: 'notif-1',
    type: 'task_assigned',
    title: 'Task Assigned: IPM Inspection',
    message: "You've been assigned a new task",
    taskId: '1', // Maps to "Inspect Mother Room A for PM"
    timestamp: '2 min ago',
    isRead: false,
  },
  {
    id: 'notif-2',
    type: 'task_overdue',
    title: 'Task Overdue: pH Calibration',
    message: 'Task is now overdue by 2 hours',
    taskId: '3', // Maps to "Calibrate pH Sensors"
    timestamp: '2 hours ago',
    isRead: false,
  },
  {
    id: 'notif-3',
    type: 'task_comment',
    title: 'New Comment on: Defoliation',
    message: 'Sarah Chen commented on your task',
    taskId: '7', // Maps to "Week 3 Defoliation"
    timestamp: '3 hours ago',
    isRead: false,
  },
  {
    id: 'notif-4',
    type: 'blueprint_triggered',
    title: 'Blueprint Triggered',
    message: '3 new tasks created from "Week 1 Flower" blueprint',
    timestamp: 'Yesterday',
    isRead: true,
  },
];

const NOTIFICATION_STYLES = {
  task_assigned: {
    borderColor: 'border-emerald-500',
    bgColor: 'bg-emerald-500/5',
    icon: CheckCircle2,
    iconColor: 'text-emerald-400',
  },
  task_overdue: {
    borderColor: 'border-amber-500',
    bgColor: 'bg-amber-500/5',
    icon: AlertTriangle,
    iconColor: 'text-amber-400',
  },
  task_comment: {
    borderColor: 'border-sky-500',
    bgColor: 'bg-sky-500/5',
    icon: MessageSquare,
    iconColor: 'text-sky-400',
  },
  blueprint_triggered: {
    borderColor: 'border-violet-500',
    bgColor: 'bg-violet-500/5',
    icon: Zap,
    iconColor: 'text-violet-400',
  },
  task_completed: {
    borderColor: 'border-emerald-500',
    bgColor: 'bg-emerald-500/5',
    icon: CheckCircle2,
    iconColor: 'text-emerald-400',
  },
};

function NotificationIndicator() {
  const router = useRouter();
  const [notifications, setNotifications] = useState<TaskNotification[]>(MOCK_NOTIFICATIONS);
  const [isOpen, setIsOpen] = useState(false);

  const unreadCount = notifications.filter(n => !n.isRead).length;

  const handleNotificationClick = (notification: TaskNotification) => {
    // Mark as read
    setNotifications(prev =>
      prev.map(n => (n.id === notification.id ? { ...n, isRead: true } : n))
    );

    // Close dropdown
    setIsOpen(false);

    // Navigate to task if it has a taskId
    if (notification.taskId) {
      router.push(`/dashboard/tasks?taskId=${notification.taskId}`);
    } else {
      // For non-task notifications, just go to tasks page
      router.push('/dashboard/tasks');
    }
  };

  const handleMarkAllRead = () => {
    setNotifications(prev => prev.map(n => ({ ...n, isRead: true })));
  };

  return (
    <div className="relative">
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="relative p-2 text-[var(--text-muted)] hover:text-[var(--text-primary)] transition-colors rounded-lg hover:bg-[var(--bg-tile)]"
        aria-label="Notifications"
      >
        <Bell className="w-5 h-5" />
        {unreadCount > 0 && (
          <span className="absolute top-0.5 right-0.5 flex items-center justify-center min-w-[16px] h-4 text-[10px] font-bold text-white bg-rose-500 rounded-full px-1">
            {unreadCount}
          </span>
        )}
      </button>

      {isOpen && (
        <>
          <div className="fixed inset-0 z-40" onClick={() => setIsOpen(false)} />
          <div className="absolute right-0 mt-2 w-96 bg-[var(--bg-surface)] rounded-xl shadow-xl border border-[var(--border)] overflow-hidden z-50">
            {/* Header */}
            <div className="flex items-center justify-between px-4 py-3 bg-[var(--bg-tile)] border-b border-[var(--border)]">
              <span className="font-semibold text-sm text-[var(--text-primary)]">Notifications</span>
              {unreadCount > 0 && (
                <button
                  onClick={handleMarkAllRead}
                  className="text-xs text-[var(--accent-cyan)] hover:underline"
                >
                  Mark all read
                </button>
              )}
            </div>

            {/* Notifications List */}
            <div className="max-h-96 overflow-y-auto">
              {notifications.length === 0 ? (
                <div className="px-4 py-8 text-center text-[var(--text-muted)]">
                  <Bell className="w-8 h-8 mx-auto mb-2 opacity-50" />
                  <p className="text-sm">No notifications</p>
                </div>
              ) : (
                notifications.map(notification => {
                  const style = NOTIFICATION_STYLES[notification.type];
                  const IconComponent = style.icon;

                  return (
                    <button
                      key={notification.id}
                      onClick={() => handleNotificationClick(notification)}
                      className={`w-full text-left px-4 py-3 hover:bg-[var(--bg-tile-hover)] cursor-pointer border-l-2 transition-colors ${
                        notification.isRead
                          ? 'border-transparent'
                          : `${style.borderColor} ${style.bgColor}`
                      }`}
                    >
                      <div className="flex items-start gap-3">
                        <div className={`mt-0.5 ${style.iconColor}`}>
                          <IconComponent className="w-4 h-4" />
                        </div>
                        <div className="flex-1 min-w-0">
                          <p className={`text-sm font-medium ${notification.isRead ? 'text-[var(--text-muted)]' : 'text-[var(--text-primary)]'}`}>
                            {notification.title}
                          </p>
                          <p className="text-xs text-[var(--text-muted)] mt-0.5 truncate">
                            {notification.message}
                          </p>
                          <p className="text-xs text-[var(--text-subtle)] mt-1">
                            {notification.timestamp}
                          </p>
                        </div>
                        {!notification.isRead && (
                          <div className="w-2 h-2 rounded-full bg-[var(--accent-cyan)] mt-2" />
                        )}
                      </div>
                    </button>
                  );
                })
              )}
            </div>

            {/* Footer */}
            <Link
              href="/dashboard/tasks"
              className="block px-4 py-3 text-center text-sm font-medium text-[var(--accent-cyan)] hover:bg-[var(--bg-tile)] border-t border-[var(--border)] transition-colors"
              onClick={() => setIsOpen(false)}
            >
              View all tasks →
            </Link>
          </div>
        </>
      )}
    </div>
  );
}

export function Header() {
  return (
    <header className="h-[72px] px-6 flex items-center justify-between bg-[var(--bg-surface)]/80 backdrop-blur-xl border-b border-[var(--border)] sticky top-0 z-40">
      
      {/* Left: Brand Context */}
      <div className="flex items-center gap-3 min-w-[200px]">
        <div className="w-2 h-2 rounded-full bg-cyan-400 shadow-[0_0_8px_rgba(34,211,238,0.5)]" />
        <span className="text-2xl font-bold tracking-tight text-[var(--text-primary)]">Harvestry</span>
      </div>

      {/* Center: Global Search */}
      <div className="flex-1 max-w-2xl px-4">
        <div className="relative group">
          <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
            <Command className="h-4 w-4 text-[var(--text-muted)]/50 group-focus-within:text-cyan-400 transition-colors" />
          </div>
          <input
            type="text"
            className="w-full h-10 pl-11 pr-4 bg-[var(--bg-tile)] border border-[var(--border)] rounded-full text-sm text-[var(--text-primary)] placeholder:text-[var(--text-muted)]/40 focus:outline-none focus:ring-1 focus:ring-cyan-500/50 focus:bg-[var(--bg-elevated)] transition-all"
            placeholder="Quick Actions — search devices, recipes, setpoints, tasks..."
          />
          <div className="absolute inset-y-0 right-0 pr-3 flex items-center pointer-events-none">
             <span className="text-[10px] font-mono text-[var(--text-muted)]/40 border border-[var(--border)] rounded px-1.5 py-0.5">⌘K</span>
          </div>
        </div>
      </div>

      {/* Right: Controls & Context */}
      <div className="flex items-center gap-3 min-w-[200px] justify-end">
        
        {/* Site Selector */}
        <div className="hidden lg:flex items-center">
          <span className="text-xs text-[var(--text-muted)] mr-2">Site:</span>
          <button className="flex items-center gap-2 h-8 px-3 bg-[var(--bg-tile)] hover:bg-[var(--bg-tile-hover)] border border-[var(--border)] rounded-full text-xs font-medium text-[var(--text-primary)] transition-all group">
            <span>Evergreen</span>
            <ChevronDown className="w-3 h-3 text-[var(--text-muted)] group-hover:text-cyan-400" />
          </button>
        </div>

        {/* Room Selector */}
        <div className="hidden xl:flex items-center">
          <span className="text-xs text-[var(--text-muted)] mr-2">Room:</span>
          <button className="flex items-center gap-2 h-8 px-3 bg-[var(--bg-tile)] hover:bg-[var(--bg-tile-hover)] border border-[var(--border)] rounded-full text-xs font-medium text-[var(--text-primary)] transition-all group">
            <span>Flower • F1</span>
            <ChevronDown className="w-3 h-3 text-[var(--text-muted)] group-hover:text-cyan-400" />
          </button>
        </div>

        {/* Time Range */}
        <div className="hidden 2xl:flex items-center mr-4">
          <span className="text-xs text-[var(--text-muted)] mr-2">Range:</span>
          <button className="flex items-center gap-2 h-8 px-3 bg-[var(--bg-tile)] hover:bg-[var(--bg-tile-hover)] border border-[var(--border)] rounded-full text-xs font-medium text-[var(--text-primary)] transition-all group">
            <span>Last 24 h</span>
            <ChevronDown className="w-3 h-3 text-[var(--text-muted)] group-hover:text-cyan-400" />
          </button>
        </div>

        <div className="w-px h-6 bg-[var(--border)] mx-1" />

        {/* Notifications */}
        <NotificationIndicator />

        {/* User Profile */}
        <button className="flex items-center gap-3 pl-2 pr-1 py-1 rounded-full hover:bg-[var(--bg-tile)] transition-colors">
           <span className="text-sm font-medium text-[var(--text-primary)] hidden md:block">Brandon</span>
           <div className="w-8 h-8 rounded-full bg-gradient-to-b from-cyan-400 to-blue-600 ring-2 ring-[var(--bg-surface)] shadow-lg" />
        </button>

      </div>
    </header>
  );
}
