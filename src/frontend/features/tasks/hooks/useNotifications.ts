/**
 * useNotifications Hook
 * React hook for user notification management
 */

import { useState, useCallback, useEffect } from 'react';
import type { UserNotification } from '../types/notification.types';
import * as notificationService from '../services/notification.service';

export interface UseNotificationsOptions {
  autoFetch?: boolean;
  unreadOnly?: boolean;
  limit?: number;
  pollInterval?: number;
}

export interface UseNotificationsReturn {
  notifications: UserNotification[];
  unreadCount: number;
  isLoading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
  markAsRead: (notificationId: string) => Promise<void>;
  markAllAsRead: () => Promise<void>;
}

export function useNotifications(options: UseNotificationsOptions = {}): UseNotificationsReturn {
  const { autoFetch = true, unreadOnly, limit = 50, pollInterval } = options;
  const [notifications, setNotifications] = useState<UserNotification[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchCount = useCallback(async () => {
    try {
      const response = await notificationService.getUnreadCount();
      setUnreadCount(response.unreadCount);
    } catch (err) {
      console.error('Failed to fetch notification count:', err);
    }
  }, []);

  const refetch = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const [notifResponse] = await Promise.all([
        notificationService.getNotifications(unreadOnly, limit),
        fetchCount(),
      ]);
      setNotifications(notifResponse.notifications);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch notifications');
    } finally {
      setIsLoading(false);
    }
  }, [unreadOnly, limit, fetchCount]);

  useEffect(() => {
    if (autoFetch) {
      refetch();
    }
  }, [autoFetch, refetch]);

  // Poll for new notifications
  useEffect(() => {
    if (!pollInterval) return;

    const interval = setInterval(() => {
      fetchCount();
    }, pollInterval);

    return () => clearInterval(interval);
  }, [pollInterval, fetchCount]);

  const markAsRead = useCallback(async (notificationId: string) => {
    await notificationService.markAsRead(notificationId);
    setNotifications(prev =>
      prev.map(n => n.id === notificationId ? { ...n, isRead: true, readAt: new Date().toISOString() } : n)
    );
    setUnreadCount(prev => Math.max(0, prev - 1));
  }, []);

  const markAllAsRead = useCallback(async () => {
    await notificationService.markAllAsRead();
    setNotifications(prev =>
      prev.map(n => ({ ...n, isRead: true, readAt: new Date().toISOString() }))
    );
    setUnreadCount(0);
  }, []);

  return {
    notifications,
    unreadCount,
    isLoading,
    error,
    refetch,
    markAsRead,
    markAllAsRead,
  };
}

