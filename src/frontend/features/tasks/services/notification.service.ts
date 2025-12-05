/**
 * Notification Service
 * Manages user notifications for task updates
 */

import type {
  UserNotification,
  NotificationCountResponse,
  NotificationListResponse,
} from '../types/notification.types';

const API_BASE = '/api/v1';

/**
 * Get notifications for the current user
 */
export async function getNotifications(
  unreadOnly?: boolean,
  limit = 50
): Promise<NotificationListResponse> {
  const params = new URLSearchParams({ limit: String(limit) });
  if (unreadOnly !== undefined) params.set('unreadOnly', String(unreadOnly));
  
  const response = await fetch(`${API_BASE}/notifications?${params}`);
  if (!response.ok) throw new Error('Failed to fetch notifications');
  
  const data = await response.json();
  return { notifications: data, total: data.length };
}

/**
 * Get unread notification count
 */
export async function getUnreadCount(): Promise<NotificationCountResponse> {
  const response = await fetch(`${API_BASE}/notifications/count`);
  if (!response.ok) throw new Error('Failed to fetch notification count');
  return response.json();
}

/**
 * Mark a notification as read
 */
export async function markAsRead(notificationId: string): Promise<void> {
  const response = await fetch(`${API_BASE}/notifications/${notificationId}/read`, {
    method: 'POST',
  });
  if (!response.ok) throw new Error('Failed to mark notification as read');
}

/**
 * Mark all notifications as read
 */
export async function markAllAsRead(): Promise<void> {
  const response = await fetch(`${API_BASE}/notifications/read-all`, {
    method: 'POST',
  });
  if (!response.ok) throw new Error('Failed to mark all notifications as read');
}

